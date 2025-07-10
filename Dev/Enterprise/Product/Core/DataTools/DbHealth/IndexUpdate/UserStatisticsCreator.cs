using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Environment;
using Enterprise.Integration;

namespace Enterprise.DbHealth.IndexUpdate
{
	public class UserStatisticsCreator
	{
		public UserStatisticsCreator(ILogger logger)
		{
			this.logger = logger;
		}

		readonly ILogger logger;

		public void Run(DbConnection connection, string mainDbName)
		{
			if (Env.Registry.ISU_DisableAutoStatisticsDuringUpgrade)
			{
				return;
			}

			logger.Log(LogType.Information, "Creating statistics for each column that do not have a dedicated statistics");

			using (((ICurrentDbControl)connection).UseDatabase(mainDbName))
			{
				var columnDetailList = GetAllColumnsWithoutStatistics(connection);

				foreach (var columnDetail in columnDetailList.OrderBy(row => row.schema).ThenBy(row => row.table).ThenBy(row => row.column))
				{
					var statisticsName = "WTG_" + columnDetail.column;
					var sqlText = $@"CREATE STATISTICS {statisticsName.QuoteName()} ON {columnDetail.schema.QuoteName()}.{columnDetail.table.QuoteName()} ({columnDetail.column.QuoteName()});";

					logger.Log(LogType.Information, $"(+) {sqlText}");
					connection.ExecuteNonQuery(sqlText);
				}
			}
		}

		List<(string schema, string table, string column)> GetAllColumnsWithoutStatistics(DbConnection connection)
		{
			var columnDetailList = new List<(string schema, string table, string column)>();
			string sql = $@"
				SELECT
					SchemaName = sch.name,
					TableName  = obj.name,
					ColumnName = col.name
				FROM
					sys.schemas      AS sch
					JOIN sys.objects AS obj	ON obj.schema_id = sch.schema_id
					JOIN sys.columns AS col ON col.object_id = obj.object_id
				WHERE 1=1
					AND sch.name NOT IN ('{string.Join("','", Db.SqlReservedSchemas)}')
					AND obj.is_ms_shipped = 0
					AND obj.type = 'U'
					AND col.max_length <> -1
					AND col.is_computed = 0
					AND col.column_id NOT IN
					(
						SELECT
							scol.column_id
						FROM
							sys.stats              AS sts
							JOIN sys.stats_columns AS scol ON scol.object_id = sts.object_id AND scol.stats_id = sts.stats_id
						WHERE 1=1
							AND sts.object_id = obj.object_id
							AND sts.has_filter = 0
							AND 
							(
								scol.column_id IN 
								(
									SELECT
										icol.column_id 
									FROM
										sys.indexes            AS ind
										JOIN sys.index_columns AS icol ON icol.object_id = ind.object_id AND icol.index_id = ind.index_id
									WHERE 1=1
										AND ind.object_id = sts.object_id
										AND ind.index_id = sts.stats_id
										AND ind.has_filter = 0
										AND icol.key_ordinal = 1
								)
								OR
								(
									(
										sts.user_created = 1
										OR sts.auto_created = 1
									)
									AND scol.stats_column_id = 1
								)
							)
					);
				";

			connection.ExecuteReader(sql, reader =>
			{
				columnDetailList.Add((reader["SchemaName"].ToString(), reader["TableName"].ToString(), reader["ColumnName"].ToString()));
			});

			return columnDetailList;
		}
	}
}
