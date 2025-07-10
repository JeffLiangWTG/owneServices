using System;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Testing
{
	sealed class SimpleClusterKeyPopulateStrategyTest : TransactionedTestCase
	{
		public void TestPopulateClusterKeys()
		{
			PrepareTestDataAndAssertPopulateClusterKeys(
				"Creating index IX_JE_PK_NonZero_JE_ClusterKey",
				"Creating index IX_J3_JE_Zero_J3_ClusterKey",
				"\t: 3 rows updated in JobDecRefs"
			);
		}

		void PrepareTestDataAndAssertPopulateClusterKeys(params string[] expectedLogs)
		{
			PrepareTestData();
			AssertChildClusterKey(dec1000, populated: 1, nonPopulated: 3);
			AssertChildClusterKeyValueCount(decZero.Pk, ckValue: 0, expectedCount: 1);

			var logger = new DummyClusterKeyStrategyLogger();
			var ckPopulator = new SimpleClusterKeyPopulateStrategy(logger, ckDefinition);
			ckPopulator.PopulateClusterKeys();
			AssertSequencesEqual("Logs", expectedLogs, logger.Logs);
			AssertChildClusterKey(dec1000, populated: 4, nonPopulated: 0);
			AssertChildClusterKeyValueCount(decZero.Pk, ckValue: 0, expectedCount: 1);

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

			var (companyPk, branchPk) = DataHelpers.GetCompanyAndBranch();

			var script = $@"
				INSERT dbo.JobDeclaration (JE_PK, JE_DataModel, JE_GB, JE_GC, JE_ClusterKey, JE_DeclarationReference, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser) VALUES
					('{dec1000.Pk}', 'AU', '{branchPk}', '{companyPk}', {dec1000.Ck}, 'Ref1000', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					('{decZero.Pk}', 'AU', '{branchPk}', '{companyPk}', {decZero.Ck}, 'Ref0000', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
				INSERT dbo.JobDecRefs (J3_PK, J3_JE, J3_ClusterKey, J3_SystemCreateTimeUtc, J3_SystemCreateUser, J3_SystemLastEditTimeUtc, J3_SystemLastEditUser) VALUES
					(newid(), '{dec1000.Pk}', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), '{dec1000.Pk}', {dec1000.Ck}, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), '{dec1000.Pk}', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), '{dec1000.Pk}', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), '{decZero.Pk}', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP');
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

		readonly KeyDefinition ckDefinition = new KeyDefinition(JobDeclarationSchema.PK, JobDecRefsSchema.J3_JE);

		readonly (Guid Pk, int Ck) dec1000 = (new Guid("10000000-0000-0000-0000-000000001000"), 1000);
		readonly (Guid Pk, int Ck) decZero = (new Guid("20000000-0000-0000-0000-000000000000"), 0);
	}
}
