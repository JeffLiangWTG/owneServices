using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Database.Shared;

namespace CargoWise.Data.SqlServer.Metadata.ComputedColumn
{
	public static class ComputedColumnLoader
	{
		public static ComputedColumnInfo LoadTop1(DbConnection connection, string schemaName, string tableName, string columnName)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(schemaName, nameof(schemaName));
			Argument.NotNullOrEmpty(tableName, nameof(tableName));
			Argument.NotNullOrEmpty(columnName, nameof(columnName));

			return LoadInfo(connection, true, schemaName, tableName, columnName)
				.FirstOrDefault();
		}

		public static ComputedColumnInfo ToDatabaseFormat(DbConnection connection, ComputedColumnInfo columnInfo)
		{
			ComputedColumnInfo info = null;
			var tempTableName = $"#{columnInfo.TableName}";
			connection.ExecuteNonQuery($"SELECT TOP 0 * INTO [{tempTableName}] FROM [{columnInfo.SchemaName}].[{columnInfo.TableName}]");
			var tempInfo = ComputedColumnInfo.Builder.Copy(columnInfo, columnInfo.SchemaName, tempTableName).GetInfo();
			connection.ExecuteNonQuery(tempInfo.DropDefinition);
			connection.ExecuteNonQuery(tempInfo.AddDefinition);
			using (((ICurrentDbControl)connection).UseDatabase("tempdb"))
			{
				var dbInfo = LoadTop1(connection, columnInfo.SchemaName, tempTableName, columnInfo.ColumnName);
				if (dbInfo != null)
				{
					info = ComputedColumnInfo.Builder.Copy(dbInfo, columnInfo.SchemaName, columnInfo.TableName).GetInfo();
				}
			}
			connection.ExecuteNonQuery($"DROP TABLE [{tempTableName}]");
			return info;
		}

		static List<ComputedColumnInfo> LoadInfo(DbConnection connection, bool useTop1, string schemaName, string tableName, string computedColumnName)
		{
			var result = new List<ComputedColumnInfo>();
			var top_1 = useTop1 ? "TOP (1)" : ""; // Direct SQL query

			var sql = FormattableString.Invariant($@"-- MetaData.ComputedColumnInfo.LoadInfo
SELECT {top_1}
	sch_name         = OBJECT_SCHEMA_NAME(Col.object_id),
	obj_name         = OBJECT_NAME(Col.object_id),
	col_name         = Col.name,
	col_expression   = Col.definition,
	col_is_persisted = Col.is_persisted
FROM
	sys.objects Obj
	JOIN sys.computed_columns AS Col ON Col.object_id = Obj.object_id
WHERE 1=1
	AND Obj.is_ms_shipped = 0
	AND Obj.type in ('U', 'V') -- user tables and views
	AND Obj.object_id = OBJECT_ID(@fullObjectName)
	AND (@col_name is NULL or Col.name = @col_name)
ORDER BY
	sch_name, obj_name, col_name
");

			var prev_sch_name = "";
			var prev_tab_name = "";
			var prev_col_name = "";
			ComputedColumnInfo.Builder builder = null;

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@fullObjectName", SqlDbType.NVarChar, 300, schemaName + "." + tableName);
				cmd.AddParameter("@col_name", SqlDbType.NVarChar, 128, computedColumnName);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var sch_name = (string)reader["sch_name"];
						var tab_name = (string)reader["obj_name"];
						var col_name = (string)reader["col_name"];

						if (prev_sch_name != sch_name || prev_tab_name != tab_name || prev_col_name != col_name)
						{
							if (builder != null)
							{
								result.Add(builder.GetInfo());
							}

							var col_is_persisted = Convert.ToBoolean(reader["col_is_persisted"], CultureInfo.InvariantCulture);
							var col_expression = (string)reader["col_expression"];

							builder = ComputedColumnInfo.Builder.New(sch_name, tab_name)
								.Name(col_name)
								.ComputedExpression(col_expression);

							if (col_is_persisted)
							{
								builder = builder.Persisted();
							}

							prev_sch_name = sch_name;
							prev_tab_name = tab_name;
							prev_col_name = col_name;
						}
					}

					if (builder != null)
					{
						result.Add(builder.GetInfo());
					}
				}
			}

			return result;
		}
	}
}
