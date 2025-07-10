#if DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Data;

namespace Enterprise.DbUpgrader.Shared
{
	public static class TestDbObjectHelper
	{
		public static IEnumerable<string> GetInFiltersFromConstraint(string columnName, string constraintName = null)
		{
			if (constraintName == null)
			{
				constraintName = $"Constraint_{columnName}";
			}

			// "WL_MaxWeightUnit in ('DT', 'G')" in Schema_Main.sql becomes "[WL_MaxWeightUnit]='DT' OR [WL_MaxWeightUnit]='G'" in the database
			var regex = new Regex($@"\[{columnName}\]='([A-Za-z0-9\-]+)'");
			var constraintText = (string)Db.Connection.ExecuteScalar($"SELECT OBJECT_DEFINITION(OBJECT_ID(N'{constraintName}', N'C'))");
			var codesOnConstraint = regex.Matches(constraintText).Cast<Match>().Select(m => m.Groups[1].Value);

			return codesOnConstraint;
		}

		public static bool IsColumnExistCaseSensitive(DbConnection connection, string dbName, string schemaName, string tableName, string columnName)
		{
			var sqlText = string.Format(@"
				IF EXISTS(
					SELECT NULL
					FROM
						[{0}].sys.schemas sch 
						INNER JOIN [{0}].sys.tables tab ON sch.schema_id = tab.schema_id
						INNER JOIN [{0}].sys.columns col ON tab.object_id = col.object_id
					WHERE
						sch.name = '{1}'
						AND tab.name = '{2}'
						AND col.name = '{3}' COLLATE SQL_Latin1_General_CP1_CS_AS
				) SELECT 1;
				ELSE SELECT 0;",
				dbName, schemaName, tableName, columnName);

			return Convert.ToBoolean(connection.ExecuteScalar(sqlText));
		}
	}
}

#endif
