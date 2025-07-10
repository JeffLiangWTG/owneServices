using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Transformations
{
	static class ClusterKeyDbHelper
	{
		public static void CreateSupportingIndexIfNotExists(DbConnection connection, IUpgradeTaskWorkflowLogger logger, string tableName, string indexName, string columnName)
		{
			CreateSupportingIndexIfNotExists(connection, logger, tableName, indexName, columnName, includeColumn: null, filter: null);
		}

		public static void CreateSupportingIndexIfNotExists(DbConnection connection, IUpgradeTaskWorkflowLogger logger, string tableName, string indexName, string columnName, string includeColumn, string filter)
		{
			if (!DbObjectCreator.IndexExists(connection, tableName, indexName))
			{
				logger.ShowInfoMessage($"Creating index {indexName}");
				var includeClause = string.IsNullOrWhiteSpace(includeColumn) ? "" : $"INCLUDE ({includeColumn})";
				var filterClause = string.IsNullOrWhiteSpace(filter) ? "" : $"WHERE {filter}";
				var indexCreateSql = $"CREATE INDEX {indexName} ON [dbo].[{tableName}] ({columnName}) {includeClause} {filterClause} WITH (online=ON)";

				using (var cmd = connection.Command(indexCreateSql, DbCommand.Timeout.Infinite))
				{
					cmd.ExecuteNonQuery();
				}
			}
		}

		public static bool IsColumnClusteredIndexRoot(DbConnection connection, string tableName, string fkColumn)
		{
			string sql = $@"
				FROM sys.tables AS t
				INNER JOIN sys.indexes AS i ON i.object_id = t.object_id
				INNER JOIN sys.index_columns AS ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
				INNER JOIN sys.columns AS c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
				WHERE t.name = '{tableName}'
				AND c.name = '{fkColumn}'
				AND i.type = 1
				AND ic.key_ordinal = 1";
			return connection.Exists(sql);
		}

		public static int GetMaxClusterKeyValueSafe(DbConnection connection, string tableName, string clusterKeyColumn)
		{
			int result = (DbObjectCreator.ColumnExists(connection, tableName, clusterKeyColumn))
				? GetMaxClusterKeyValue(connection, tableName, clusterKeyColumn)
				: 0;

			return result;
		}

		public static int GetMaxClusterKeyValue(DbConnection connection, string tableName, string clusterKeyColumn)
		{
			var cmd = $"SELECT ISNULL(MAX([{clusterKeyColumn}]), 0) FROM [dbo].[{tableName}];";
			return connection.ExecuteScalar<int>(cmd);
		}

		public static long GetSqlBatchSize(UpgradeMode upgradeMode)
		{
			switch (upgradeMode)
			{
				case UpgradeMode.Offline:
					return long.MaxValue;
				case UpgradeMode.Online:
					return 10000;
#if DEBUG
				case UpgradeMode.TestWithBatchSize2:
					return 2;
#endif
				default:
					throw new ArgumentException($"Unexpected UpgradeMode [{upgradeMode}]");
			}
		}
	}

	enum UpgradeMode
	{
		Online,
		Offline,
#if DEBUG
		TestWithBatchSize2,
#endif
	}
}
