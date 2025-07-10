using System;
using System.Linq;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Testing
{
	sealed class OnlineClusterKeyWorkerPopulateStrategyTest : TransactionedTestCase
	{
		public void TestValidateUpgradeMode()
		{
			AssertExceptionThrown<ArgumentException>(
				"Attempt to instantiate OnlineClusterKeyWorkerPopulateStrategy in Offline upgrade mode",
				"UpgradeMode [Offline] is not valid for OnlineClusterKeyWorkerPopulateStrategy",
				() => new OnlineClusterKeyWorkerPopulateStrategy(null, ckDefinition, UpgradeMode.Offline)
			);

			AssertNotNull(
				"OnlineClusterKeyWorkerPopulateStrategy in Online upgrade mode",
				new OnlineClusterKeyWorkerPopulateStrategy(null, ckDefinition, UpgradeMode.Online)
			);

			AssertNotNull(
				"OnlineClusterKeyWorkerPopulateStrategy in TestWithBatchSize2 upgrade mode",
				new OnlineClusterKeyWorkerPopulateStrategy(null, ckDefinition, UpgradeMode.TestWithBatchSize2)
			);
		}

		public void TestUpdateCommandContainsOptimizeForOption()
		{
			var ckPopulator = new OnlineClusterKeyWorkerPopulateStrategy(new DummyUpgradeManager(), ckDefinition, UpgradeMode.Online);

			using (TestConnection.TrackExecutedCommands())
			{
				ckPopulator.PopulateClusterKeys();
				var lastExecutedCommand = TestConnection.ExecutedCommands.LastOrDefault();
				AssertContains("Update command contains expected option", "OPTION (OPTIMIZE FOR (@BatchSize = 1))", lastExecutedCommand);
			}
		}

		public void TestPopulateClusterKeys()
		{
			PrepareTestData();
			AssertChildClusterKey(inv1000, populated: 1, nonPopulated: 3);
			AssertChildClusterKey(inv2000, populated: 1, nonPopulated: 2);
			AssertChildClusterKeyValueCount(invZero.Pk, ckValue: 0, expectedCount: 2);

			var logger = new DummyClusterKeyStrategyLogger();
			var ckPopulator = new OnlineClusterKeyWorkerPopulateStrategy(logger, ckDefinition, UpgradeMode.TestWithBatchSize2);
			ckPopulator.PopulateClusterKeys();

			var expectedLogs = new string[]
			{
				"Creating index IX_JZ_PK_NonZero_JZ_ClusterKey",
				"Creating index IX_JI_JZ_Zero_JI_ClusterKey",
				$"\t: 2 rows updated in JobComInvoiceLine (last updated FK: {inv1000.Pk})",
				$"\t: 2 rows updated in JobComInvoiceLine (last updated FK: {inv2000.Pk})",
				$"\t: 1 rows updated in JobComInvoiceLine (last updated FK: {inv2000.Pk})",
			};
			AssertSequencesEqual("Logs", expectedLogs, logger.Logs);

			AssertChildClusterKey(inv1000, populated: 4, nonPopulated: 0);
			AssertChildClusterKey(inv2000, populated: 3, nonPopulated: 0);
			AssertChildClusterKeyValueCount(invZero.Pk, ckValue: 0, expectedCount: 2);

			// Calling againg when there are no more children cluster keys to be populated => no logs
			logger.Logs.Clear();
			ckPopulator.PopulateClusterKeys();
			AssertSequencesEqual("Logs", Array.Empty<string>(), logger.Logs);
		}

		void PrepareTestData()
		{
			var participatingClusterKeys = new string[]
			{
				ckDefinition.ParentClusterKey,
				ckDefinition.ChildClusterKey,
			};
			ClusterKeyTransformationHelper.DropClusterKeyIndexesAndConstraints(TestConnection, participatingClusterKeys);

			var script = $@"
				DECLARE @anyBranchPK uniqueidentifier = (SELECT TOP 1 GB_PK FROM dbo.GlbBranch);
				INSERT dbo.JobComInvoiceHeader (JZ_PK, JZ_DataModel, JZ_ClusterKey, JZ_GB, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser) VALUES
					('{inv1000.Pk}', 'AU', {inv1000.Ck}, @anyBranchPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					('{inv2000.Pk}', 'AU', {inv2000.Ck}, @anyBranchPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					('{invZero.Pk}', 'AU', {invZero.Ck}, @anyBranchPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP');
				INSERT dbo.JobComInvoiceLine (JI_PK, JI_DataModel, JI_JZ, JI_ClusterKey, JI_SystemCreateTimeUtc, JI_SystemCreateUser, JI_SystemLastEditTimeUtc, JI_SystemLastEditUser) VALUES
					(newid(), 'AU', '{inv1000.Pk}', {inv1000.Ck}, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), 'AU', '{inv1000.Pk}', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), 'AU', '{inv1000.Pk}', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), 'AU', '{inv1000.Pk}', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), 'AU', '{inv2000.Pk}', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), 'AU', '{inv2000.Pk}', {inv2000.Ck}, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), 'AU', '{inv2000.Pk}', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), 'AU', '{invZero.Pk}', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), 'AU', '{invZero.Pk}', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP');
			";
			TestConnection.ExecuteNonQuery(script);
		}

		void AssertChildClusterKey((Guid Pk, int Ck) parent, int populated, int nonPopulated)
		{
			ClusterKeyWorkerPopulateStrategyTestHelper.AssertChildClusterKey(TestConnection, ckDefinition.ChildColumn, parent, populated, nonPopulated);
		}

		void AssertChildClusterKeyValueCount(Guid parentFk, int ckValue, int expectedCount)
		{
			ClusterKeyWorkerPopulateStrategyTestHelper.AssertChildClusterKeyValueCount(TestConnection, ckDefinition.ChildColumn, parentFk, ckValue, expectedCount);
		}

		readonly KeyDefinition ckDefinition = new KeyDefinition(JobComInvoiceHeaderSchema.PK, JobComInvoiceLineSchema.JI_JZ);

		readonly (Guid Pk, int Ck) inv1000 = (new Guid("10000000-0000-0000-0000-000000001000"), 1000);
		readonly (Guid Pk, int Ck) inv2000 = (new Guid("20000000-0000-0000-0000-000000002000"), 2000);
		readonly (Guid Pk, int Ck) invZero = (new Guid("30000000-0000-0000-0000-000000000000"), 0);
	}
}
