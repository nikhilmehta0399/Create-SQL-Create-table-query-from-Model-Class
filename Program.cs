using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;

namespace TableCreation
{
    class Program
    {
        static void Main(string[] args)
        {
            List<TableClass> tables = new List<TableClass>();
            string dirRootPath = Directory.GetCurrentDirectory();
            string filepath = string.Empty;
            
            Assembly a = Assembly.LoadFile("");// assembly path i.e. .dll file

            Type[] types = a.GetTypes();

            foreach (Type t in types)//Types in the assembly
            {
                TableClass tc = new TableClass(t);
                tables.Add(tc);
            }

            
            foreach (TableClass table in tables)
            {

                filepath = Path.Combine(dirRootPath, table.ClassName.ToString() + ".sql"); // create sql file for each class


                StreamWriter log;
                if (!System.IO.File.Exists(filepath))
                {
                    log = new StreamWriter(filepath);
                }
                else
                {
                    log = System.IO.File.AppendText(filepath);
                }
                log.WriteLine(table.CreateTableScript()); // create table query generation 
                // Close the stream:
                log.Close();
            }

            // Total Hacked way to find FK relationships! Too lazy to fix right now
            //foreach (TableClass table in tables)
            //{
            //    foreach (KeyValuePair<String, Type> field in table.Fields)
            //    {
            //        foreach (TableClass t2 in tables)
            //        {
            //            if (field.Value.Name == t2.ClassName)
            //            {
            //                // We have a FK Relationship!
            //                Console.WriteLine("GO");
            //                Console.WriteLine("ALTER TABLE " + table.ClassName + " WITH NOCHECK");
            //                Console.WriteLine("ADD CONSTRAINT FK_" + field.Key + " FOREIGN KEY (" + field.Key + ") REFERENCES " + t2.ClassName + "(ID)");
            //                Console.WriteLine("GO");

            //            }
            //        }
            //    }
            //}
        }
    }

    public class TableClass
    {
        private List<KeyValuePair<String, Type>> _fieldInfo = new List<KeyValuePair<String, Type>>();
        private string _className = String.Empty;

        private Dictionary<Type, String> dataMapper
        {
            get
            {
                // mapping c# datatypes to sql
                Dictionary<Type, String> dataMapper = new Dictionary<Type, string>();
                dataMapper.Add(typeof(int), "INT");
                dataMapper.Add(typeof(string), "NVARCHAR(500)");
                dataMapper.Add(typeof(bool), "BIT");
                dataMapper.Add(typeof(DateTime), "DATETIME");
                dataMapper.Add(typeof(float), "FLOAT");
                dataMapper.Add(typeof(decimal), "DECIMAL(18,0)");
                dataMapper.Add(typeof(Guid), "UNIQUEIDENTIFIER");

                return dataMapper;
            }
        }

        public List<KeyValuePair<String, Type>> Fields
        {
            get { return this._fieldInfo; }
            set { this._fieldInfo = value; }
        }

        public string ClassName
        {
            get { return this._className; }
            set { this._className = value; }
        }

        public TableClass(Type t)
        {
            this._className = t.Name;

            foreach (PropertyInfo p in t.GetProperties())
            {
                KeyValuePair<String, Type> field = new KeyValuePair<String, Type>(p.Name, p.PropertyType);

                this.Fields.Add(field);
            }
        }

        public string CreateTableScript()
        {
            System.Text.StringBuilder script = new StringBuilder();

            script.AppendLine("CREATE TABLE " + this.ClassName);
            script.AppendLine("(");
            for (int i = 0; i < this.Fields.Count; i++)
            {
                KeyValuePair<String, Type> field = this.Fields[i];

                if (dataMapper.ContainsKey(field.Value))
                {
                    script.Append(field.Key + " " + dataMapper[field.Value]);
                }
                else
                {
                    // Complex Type? 
                    script.Append( field.Key + " BIGINT");
                }

                if (i != this.Fields.Count - 1)
                {
                    script.Append(",");
                }

                script.Append(Environment.NewLine);
            }

            script.AppendLine(")");

            return script.ToString();
        }

    }
}
