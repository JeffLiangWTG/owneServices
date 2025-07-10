using System;
using System.Globalization;
using CargoWise.Common;

namespace CargoWise.Data.SqlServer
{
	public class DbSchemaTable
	{
		public DbSchemaTable(string databaseName, string schemaName, string tableName)
		{
			Argument.NotNullOrEmpty(databaseName, nameof(databaseName));
			Argument.NotNullOrEmpty(tableName, nameof(tableName));

			DatabaseName = databaseName;
			SchemaName = String.IsNullOrWhiteSpace(schemaName) ? Db.SqlDbOwnerSchema : schemaName;
			TableName = tableName;
		}

		public DbSchemaTable(string fullyQualifiedTableName)
		{
			Argument.NotNullOrEmpty(fullyQualifiedTableName, nameof(fullyQualifiedTableName));

			var fq = fullyQualifiedTableName.Split('.');

			if (fq.Length < 3)
			{
				throw new ArgumentException("Incorrect fully qualified table name");
			}

			var databaseName = Unbracketed(fq[0]);
			var schemaName = Unbracketed(fq[1]);
			var tableName = Unbracketed(fq[2]);

			if (String.IsNullOrWhiteSpace(databaseName) || String.IsNullOrWhiteSpace(tableName))
			{
				var message = String.Format(CultureInfo.InvariantCulture, "Incorrect fully qualified table name: \"{0}\"", fullyQualifiedTableName); // ArgumentException message
				throw new ArgumentException(message, nameof(fullyQualifiedTableName));
			}

			DatabaseName = databaseName;
			SchemaName = String.IsNullOrWhiteSpace(schemaName) ? Db.SqlDbOwnerSchema : schemaName;
			TableName = tableName;
		}

		public string DatabaseName { get; private set; }
		public string SchemaName { get; private set; }
		public string TableName { get; private set; }

		public override string ToString()
		{
			return String.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}", Bracketed(DatabaseName), Bracketed(SchemaName), Bracketed(TableName));
		}

		public override bool Equals(object obj)
		{
			var o = (DbSchemaTable)obj;
			return o != null
				&& o.ToString().Equals(ToString(), StringComparison.InvariantCultureIgnoreCase);
		}

		public override int GetHashCode()
		{
			return StringComparer.InvariantCultureIgnoreCase.GetHashCode(ToString());
		}

		string Bracketed(string s)
		{
			return "[" + s + "]";
		}

		string Unbracketed(string s)
		{
			Argument.NotNull(s, nameof(s)); // Suggested By ReviewBot 

			return s.Replace("[", "").Replace("]", "");
		}
	}
}
