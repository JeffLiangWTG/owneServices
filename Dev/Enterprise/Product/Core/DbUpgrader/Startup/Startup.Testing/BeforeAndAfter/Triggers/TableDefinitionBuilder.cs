using System.Collections.Generic;
using System.Text;

namespace Enterprise.DbUpgrader.Startup.Testing
{
	class TableDefinitionBuilder
	{
		readonly string schema;
		readonly string tableName;
		readonly string tableCode;

		public TableDefinitionBuilder(string schema, string tableName, string tableCode)
		{
			this.schema = schema;
			this.tableName = tableName;
			this.tableCode = tableCode;
			columns = new List<string>();
		}

		readonly List<string> columns;
		public TableDefinitionBuilder AddPk()
		{
			columns.Add($"[{tableCode}_PK] UNIQUEIDENTIFIER NOT NULL");
			return this;
		}

		public TableDefinitionBuilder AddSystemCreateTimeUtc()
		{
			columns.Add($"[{tableCode}_SystemCreateTimeUtc] DATETIME NOT NULL");
			return this;
		}

		public TableDefinitionBuilder AddSystemCreateUser()
		{
			columns.Add($"[{tableCode}_SystemCreateUser] VARCHAR(3) NOT NULL");
			return this;
		}

		public TableDefinitionBuilder AddSystemLastEditTimeUtc()
		{
			columns.Add($"[{tableCode}_SystemLastEditTimeUtc] DATETIME NOT NULL");
			return this;
		}

		public TableDefinitionBuilder AddSystemLastEditUser()
		{
			columns.Add($"[{tableCode}_SystemLastEditUser] VARCHAR(3) NOT NULL");
			return this;
		}

		public TableDefinitionBuilder AddData()
		{
			columns.Add($"[{tableCode}_Data] INT NOT NULL DEFAULT 1");
			return this;
		}

		public TableDefinitionBuilder AddAutoVersion()
		{
			columns.Add($"[{tableCode}_AutoVersion] SMALLINT NOT NULL DEFAULT 0");
			return this;
		}

		public string Build()
		{
			var sb = new StringBuilder();
			sb.AppendFormat(@"CREATE TABLE [{0}].[{1}]
(
", schema, tableName);

			for (var index = 0; index < columns.Count; ++index)
			{
				sb.Append("    ");

				if (index != 0)
				{
					sb.Append(", ");
				}

				sb.AppendLine(columns[index]);
			}

			sb.AppendLine(");");
			return sb.ToString();
		}
	}
}
