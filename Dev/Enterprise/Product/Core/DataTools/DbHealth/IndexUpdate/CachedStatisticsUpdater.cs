using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using WTG.Statistics;

namespace Enterprise.DbHealth.IndexUpdate
{
	public class CachedStatisticsUpdater
	{
		public CachedStatisticsUpdater(ILogger logger)
		{
			this.logger = logger;
		}

		readonly ILogger logger;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public void Run(DbConnection connection)
		{
			logger?.Log(LogType.Information, "Retrieving tables from dbo schema for statistics calculation");
			var tableList = new List<(string schema, string tableName)>();
			const string commandText = @"
SELECT s.Name SchemaName, o.Name TableName
FROM sys.objects o
	JOIN sys.schemas s
		ON s.schema_id = o.schema_id
	LEFT JOIN sys.indexes i
		ON o.object_id = i.object_id AND i.type = 1
WHERE 1=1
	AND Is_Ms_Shipped = 0
	AND (o.type = 'U' OR (o.Type = 'V' AND i.Object_id IS NOT NULL))
	AND s.Name NOT IN ('SYS', 'CDC')
ORDER BY
	s.Name, o.Name
";

			using (var cmd = connection.Command(commandText))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					tableList.Add((reader["SchemaName"].ToString(), reader["TableName"].ToString()));
				}
			}

			IDbConnectionInternals internals = connection;
			var retriever = new HistogramsRetriever();
			retriever.SaveAllHistogramsToCache((System.Data.Common.DbConnection)internals.InternalDbConnection, (System.Data.Common.DbTransaction)internals.InternalDbTransaction, tableList, new StatisticsCacheManager());
		}
	}
}
