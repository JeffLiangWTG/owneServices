using System.Linq;
using Enterprise.DbUpgrader.Shared;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Testing
{
	sealed class ClusterKeyDbHelperTest : TransactionedTestCase
	{
		public void TestCreateSupportingIndexIfNotExists()
		{
			var logger = new DummyClusterKeyStrategyLogger();
			var tableName = DummyBizoSchema.Constants.TableName;
			var columnName = DummyBizoSchema.Constants.Z0_Number;
			var indexName = $"IX_TestCreateIndex__{columnName}";

			AssertEquals("[PRE-CONDITION] Index exists?", false, DbObjectCreator.IndexExists(TestConnection, tableName, indexName));

			ClusterKeyDbHelper.CreateSupportingIndexIfNotExists(TestConnection, logger, tableName, indexName, columnName);
			AssertEquals("Log", $"Creating index {indexName}", logger.Logs.SingleOrDefault());
			AssertIndexExists(tableName, indexName, columnName, includeColumn: null, filterDefinition: null);

			logger.Logs.Clear();
			ClusterKeyDbHelper.CreateSupportingIndexIfNotExists(TestConnection, logger, tableName, indexName, columnName);
			AssertEquals("Log Count (when index already exists)", 0, logger.Logs.Count);
			AssertIndexExists(tableName, indexName, columnName, includeColumn: null, filterDefinition: null);
		}

		public void TestCreateSupportingIndexIfNotExists_WithFilter()
		{
			var logger = new DummyClusterKeyStrategyLogger();
			var tableName = DummyBizoSchema.Constants.TableName;
			var columnName = DummyBizoSchema.Constants.Z0_Number;
			var indexName = $"IX_TestCreateIndex__{columnName}";
			var filter = $"([{columnName}]=(0))";

			AssertEquals("[PRE-CONDITION] Index exists?", false, DbObjectCreator.IndexExists(TestConnection, tableName, indexName));

			ClusterKeyDbHelper.CreateSupportingIndexIfNotExists(TestConnection, logger, tableName, indexName, columnName, includeColumn: null, filter);
			AssertEquals("Log", $"Creating index {indexName}", logger.Logs.SingleOrDefault());
			AssertIndexExists(tableName, indexName, columnName, includeColumn: null, filter);

			logger.Logs.Clear();
			ClusterKeyDbHelper.CreateSupportingIndexIfNotExists(TestConnection, logger, tableName, indexName, columnName);
			AssertEquals("Log Count (when index already exists)", 0, logger.Logs.Count);
			AssertIndexExists(tableName, indexName, columnName, includeColumn: null, filter);
		}

		public void TestCreateSupportingIndexIfNotExists_WithIncludeColumnAndFilter()
		{
			var logger = new DummyClusterKeyStrategyLogger();
			var tableName = DummyBizoSchema.Constants.TableName;
			var keyColumnName = DummyBizoSchema.Constants.Z0_Number;
			var includeColumnName = DummyBizoSchema.Constants.Z0_AnotherNumber;
			var indexName = $"IX_TestCreateIndex__{keyColumnName}__Include__{includeColumnName}";
			var filter = $"([{includeColumnName}]=(0))";

			AssertEquals("[PRE-CONDITION] Index exists?", false, DbObjectCreator.IndexExists(TestConnection, tableName, indexName));

			ClusterKeyDbHelper.CreateSupportingIndexIfNotExists(TestConnection, logger, tableName, indexName, keyColumnName, includeColumnName, filter);
			AssertEquals("Log", $"Creating index {indexName}", logger.Logs.SingleOrDefault());
			AssertIndexExists(tableName, indexName, keyColumnName, includeColumnName, filter);

			logger.Logs.Clear();
			ClusterKeyDbHelper.CreateSupportingIndexIfNotExists(TestConnection, logger, tableName, indexName, keyColumnName);
			AssertEquals("Log Count (when index already exists)", 0, logger.Logs.Count);
			AssertIndexExists(tableName, indexName, keyColumnName, includeColumnName, filter);
		}

		void AssertIndexExists(string tableName, string indexName, string columnName, string includeColumn, string filterDefinition)
		{
			var filterDefinitionPredicate = (filterDefinition == null) ? "IS NULL" : $"= '{filterDefinition}'";
			var includedColumnPredicate = (includeColumn == null) ? "ic2.index_column_id IS NULL" : $"c2.name = '{includeColumn}'";

			var sql = $@"
				FROM
					sys.tables t
					INNER JOIN sys.indexes i
						ON i.object_id = t.object_id 
					INNER JOIN sys.index_columns ic1
						ON ic1.object_id = i.object_id AND ic1.index_id = i.index_id AND ic1.index_column_id = 1 AND ic1.is_included_column = 0
					INNER JOIN sys.columns c1
						ON c1.object_id = ic1.object_id AND c1.column_id = ic1.column_id
					LEFT JOIN sys.index_columns ic2
						ON ic2.object_id = i.object_id AND ic2.index_id = i.index_id AND ic2.index_column_id = 2 AND ic2.is_included_column = 1
					LEFT JOIN sys.columns c2
						ON c2.object_id = ic2.object_id AND c2.column_id = ic2.column_id
					LEFT JOIN sys.index_columns ic3
						ON ic3.object_id = i.object_id AND ic3.index_id = i.index_id AND ic3.index_column_id = 3
				WHERE
					t.name = '{tableName}'
					AND i.name = '{indexName}'
					AND i.filter_definition {filterDefinitionPredicate}
					AND c1.name = '{columnName}'
					AND {includedColumnPredicate}
					AND ic3.index_column_id IS NULL";

			Assert($"Index [{tableName}].[{indexName}] should exist but it doesn't.", TestConnection.Exists(sql));
		}

		public void TestIsColumnClusteredIndexRoot()
		{
			const string tableName = "TestIsFkColumnClusteredIndexRoot-Table";
			const string columnName = "Col1";

			TestConnection.ExecuteNonQuery($"CREATE TABLE [{tableName}] ([{columnName}] INT, Col2 BIT)");
			AssertEquals("Table has no index => IsColumnClusteredIndexRoot?", false, ClusterKeyDbHelper.IsColumnClusteredIndexRoot(TestConnection, tableName, columnName));

			TestConnection.ExecuteNonQuery($"CREATE NONCLUSTERED INDEX [IX1] ON [{tableName}] ([{columnName}])");
			AssertEquals("Table has no clustered index => IsColumnClusteredIndexRoot?", false, ClusterKeyDbHelper.IsColumnClusteredIndexRoot(TestConnection, tableName, columnName));

			TestConnection.ExecuteNonQuery($"CREATE CLUSTERED INDEX [IX2] ON [{tableName}] ([Col2])");
			AssertEquals("Column is not part of the clustered index => IsColumnClusteredIndexRoot?", false, ClusterKeyDbHelper.IsColumnClusteredIndexRoot(TestConnection, tableName, columnName));

			TestConnection.ExecuteNonQuery($"CREATE CLUSTERED INDEX [IX2] ON [{tableName}] ([Col2], [{columnName}]) WITH (DROP_EXISTING = ON)");
			AssertEquals("Column is not the root key of the clustered index => IsColumnClusteredIndexRoot?", false, ClusterKeyDbHelper.IsColumnClusteredIndexRoot(TestConnection, tableName, columnName));

			TestConnection.ExecuteNonQuery($"CREATE CLUSTERED INDEX [IX2] ON [{tableName}] ([{columnName}]) WITH (DROP_EXISTING = ON)");
			AssertEquals("Column is the only one in the clustered index => IsColumnClusteredIndexRoot?", true, ClusterKeyDbHelper.IsColumnClusteredIndexRoot(TestConnection, tableName, columnName));

			TestConnection.ExecuteNonQuery($"CREATE CLUSTERED INDEX [IX2] ON [{tableName}] ([{columnName}], [Col2]) WITH (DROP_EXISTING = ON)");
			AssertEquals("Column is the root key in the clustered index => IsColumnClusteredIndexRoot?", true, ClusterKeyDbHelper.IsColumnClusteredIndexRoot(TestConnection, tableName, columnName));
		}

		public void TestGetMaxClusterKeyValueSafe()
		{
			AssertEquals("Max value when table is empty", 0, ClusterKeyDbHelper.GetMaxClusterKeyValueSafe(TestConnection, JobComInvoiceHeaderSchema.Constants.TableName, JobComInvoiceHeaderSchema.Constants.JZ_ClusterKey));

			TestConnection.ExecuteNonQuery(@"
DECLARE @anyBranchPK uniqueidentifier = (SELECT TOP 1 GB_PK FROM dbo.GlbBranch);
INSERT dbo.JobComInvoiceHeader (JZ_PK, JZ_DataModel, JZ_ClusterKey, JZ_GB, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser) VALUES
(newid(), 'AU', 99, @anyBranchPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
(newid(), 'AU', 137, @anyBranchPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			AssertEquals("Max value after adding rows", 137, ClusterKeyDbHelper.GetMaxClusterKeyValueSafe(TestConnection, JobComInvoiceHeaderSchema.Constants.TableName, JobComInvoiceHeaderSchema.Constants.JZ_ClusterKey));

			AssertEquals("Max value when table does not exist", 0, ClusterKeyDbHelper.GetMaxClusterKeyValueSafe(TestConnection, "Non-Existing-Table", "Non-Existing-Column"));
			AssertEquals("Max value when column does not exist", 0, ClusterKeyDbHelper.GetMaxClusterKeyValueSafe(TestConnection, JobComInvoiceHeaderSchema.Constants.TableName, "Non-Existing-Column"));
		}

		public void TestGetMaxClusterKeyValue()
		{
			AssertEquals("Max value when table is empty", 0, ClusterKeyDbHelper.GetMaxClusterKeyValue(TestConnection, JobComInvoiceHeaderSchema.Constants.TableName, JobComInvoiceHeaderSchema.Constants.JZ_ClusterKey));

			TestConnection.ExecuteNonQuery(@"
DECLARE @anyBranchPK uniqueidentifier = (SELECT TOP 1 GB_PK FROM dbo.GlbBranch);
INSERT dbo.JobComInvoiceHeader (JZ_PK, JZ_DataModel, JZ_ClusterKey, JZ_GB, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser) VALUES
(newid(), 'AU', 121, @anyBranchPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
(newid(), 'AU', 163, @anyBranchPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
(newid(), 'AU', 17, @anyBranchPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			AssertEquals("Max value after adding rows", 163, ClusterKeyDbHelper.GetMaxClusterKeyValue(TestConnection, JobComInvoiceHeaderSchema.Constants.TableName, JobComInvoiceHeaderSchema.Constants.JZ_ClusterKey));

			AssertExceptionThrown<SqlException>("When table does not exist", () => ClusterKeyDbHelper.GetMaxClusterKeyValue(TestConnection, "Non-Existing-Table", "Non-Existing-Column"));
			AssertExceptionThrown<SqlException>("When column does not exist", () => ClusterKeyDbHelper.GetMaxClusterKeyValue(TestConnection, JobComInvoiceHeaderSchema.Constants.TableName, "Non-Existing-Column"));
		}

		public void TestGetSqlBatchTopClause()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Online", 10000, ClusterKeyDbHelper.GetSqlBatchSize(UpgradeMode.Online));
				AssertEquals("Offline", long.MaxValue, ClusterKeyDbHelper.GetSqlBatchSize(UpgradeMode.Offline));
				AssertEquals("TestWithBatchSize2", 2, ClusterKeyDbHelper.GetSqlBatchSize(UpgradeMode.TestWithBatchSize2));
			});
		}
	}
}
