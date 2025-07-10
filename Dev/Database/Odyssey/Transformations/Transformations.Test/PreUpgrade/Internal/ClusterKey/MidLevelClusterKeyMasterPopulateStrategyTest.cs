using System;
using System.Collections.Generic;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Testing
{
	sealed class MidLevelClusterKeyMasterPopulateStrategyTest : TransactionedTestCase
	{
		public void TestPopulateClusterKeys()
		{
			PrepareTestData();
			AssertClusterKeyValue(new Guid[] { liqPk1, liqPk2, liqPk3, liqPk4, liqPk5 }, new int[] { 0, 0, 99, 0, 113 });

			var logger = new DummyClusterKeyStrategyLogger();
			var ckDefinition = new KeyDefinition(JobDeclarationSchema.PK, CusLiquidationSchema.B8_JE);
			var ckPopulator = new MidLevelClusterKeyMasterPopulateStrategy(logger, UpgradeMode.TestWithBatchSize2, ckDefinition);
			int newMax = ckPopulator.PopulateClusterKeys(500);

			CombineAssertions(() =>
			{
				AssertClusterKeyValue(new Guid[] { liqPk1, liqPk2, liqPk3, liqPk4, liqPk5 }, new int[] { 501, 502, 99, 503, 113 });
				AssertEquals($"New max cluster key", 503, newMax);
			});
		}

		void PrepareTestData()
		{
			var participatingClusterKeys = new string[]
			{
				JobDeclarationSchema.Constants.JE_ClusterKey,
				CusLiquidationSchema.Constants.B8_ClusterKey,
			};
			ClusterKeyTransformationHelper.DropClusterKeyIndexesAndConstraints(TestConnection, participatingClusterKeys);

			var (companyPk, branchPk) = DataHelpers.GetCompanyAndBranch();

			var script = $@"
				INSERT dbo.JobDeclaration (JE_PK, JE_DataModel, JE_GB, JE_GC, JE_ClusterKey, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser) VALUES
					('{decPk}', 'AU', '{branchPk}', '{companyPk}', 99, GetUtcDate(), '~BP', GetUtcDate(), '~BP');
				INSERT dbo.CusLiquidation (B8_PK, B8_JE, B8_GC, B8_ClusterKey, B8_SystemCreateTimeUtc, B8_SystemCreateUser, B8_SystemLastEditTimeUtc, B8_SystemLastEditUser) VALUES
					('{liqPk1}', NULL     , '{companyPk}', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					('{liqPk2}', NULL     , '{companyPk}', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					('{liqPk3}', '{decPk}', '{companyPk}', 99, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					('{liqPk4}', NULL     , '{companyPk}', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					('{liqPk5}', NULL     , '{companyPk}', 113, GetUtcDate(), '~BP', GetUtcDate(), '~BP');
			";

			TestConnection.ExecuteNonQuery(script);
		}

		void AssertClusterKeyValue(Guid[] pks, int[] expectedValues)
		{
			var sql = $"SELECT B8_ClusterKey FROM dbo.CusLiquidation WHERE B8_PK in ('{string.Join("','", pks)}')";
			var ckValues = new List<int>();
			TestConnection.ExecuteReader(sql, (r) => ckValues.Add(r.GetInt32(0)));
			AssertContainsExactElementsInAnyOrder("B8_ClusterKey values", expectedValues, ckValues);
		}

		readonly Guid decPk = Guid.NewGuid();
		readonly Guid liqPk1 = Guid.NewGuid();
		readonly Guid liqPk2 = Guid.NewGuid();
		readonly Guid liqPk3 = Guid.NewGuid();
		readonly Guid liqPk4 = Guid.NewGuid();
		readonly Guid liqPk5 = Guid.NewGuid();
	}
}
