using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Data;

namespace Enterprise.DbUpgrader.Transformations.Test.OceanCarrier;

static class DatabaseHelper
{
	public static void InsertIntoTable(DbConnection connection, string tableName, IEnumerable<(string column, object value)> values)
	{
		var sql = $@"
				INSERT INTO  dbo.{tableName}
				(
					{string.Join(",\n\t", values.Select(x => x.column))}
				)
				VALUES
				(
					{string.Join(",\n\t", values.Select(x => GetSqlValue(x.value)))}
				)";
		connection.ExecuteNonQuery(sql);
	}

	static string GetSqlValue(object value)
	{
		return value switch
		{
			null => "NULL",
			string s => $"'{s}'",
			bool b => b ? "1" : "0",
			decimal d => d.ToString(CultureInfo.InvariantCulture),
			DateTime d => $"'{d:s}'",
			DateTimeOffset d => $"'{d:O}'",
			int i => i.ToString(CultureInfo.InvariantCulture),
			Guid g => $"'{g:D}'",
			_ => throw new InvalidOperationException("Unknown data type: " + value.GetType().FullName)
		};
	}
}
