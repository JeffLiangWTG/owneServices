using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Schema;

namespace Enterprise.DataTransfer.Native.DB.Sql
{
	public class FindByKeyStatement : SqlStatement
	{
		public FindByKeyStatement(string table, IColumnDef columnsDef, string key, object value)
			: base(table, null, new Dictionary<string, SqlParameter>())
		{
			this.key = key;
			this.value = value;
			this.columnDefinition = columnsDef;
		}
		readonly string key;
		readonly object value;
		readonly IColumnDef columnDefinition;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public DbCommand GetCommand(DbConnection connection)
		{
			var sql = Generate();
			var command = connection.Command(sql);
			AddParameters(command, keyToParameters);
			return command;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		protected override string CreateFromClause()
		{
			var schema = ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchema(tableName);
			var schemaName = schema?.SqlSchemaName ?? "dbo";
			return " FROM " + schemaName + "." + tableName;
		}

		protected override string CreateWhereClause()
		{
			var paremeter = AddSqlParameter(value.ToString().Trim(), columnDefinition);
			return " WHERE " + key + " = " + paremeter.Name;
		}
	}
}
