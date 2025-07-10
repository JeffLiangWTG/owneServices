using System;
using System.Collections.Generic;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Testing
{
	sealed class TopLevelClusterKeyMasterPopulateStrategyTest : TransactionedTestCase
	{
		public void TestPopulateClusterKeys()
		{
			PrepareTestData();
			AssertClusterKeyValue(new Guid[] { decPk1, decPk2, decPk3, decPk4 }, new int[] { 0, 31, 0, 0 });

			var logger = new DummyClusterKeyStrategyLogger();
			var ckPopulator = new TopLevelClusterKeyMasterPopulateStrategy(logger, UpgradeMode.TestWithBatchSize2, JobDeclarationSchema.PK);
			int newMax = ckPopulator.PopulateClusterKeys(55);

			CombineAssertions(() =>
			{
				AssertClusterKeyValue(new Guid[] { decPk1, decPk2, decPk3, decPk4 }, new int[] { 56, 31, 57, 58 });
				AssertEquals($"New max cluster key", 58, newMax);
			});
		}

		void PrepareTestData()
		{
			var participatingClusterKeys = new string[] { JobDeclarationSchema.Constants.JE_ClusterKey };
			ClusterKeyTransformationHelper.DropClusterKeyIndexesAndConstraints(TestConnection, participatingClusterKeys);

			var (companyPk, branchPk) = DataHelpers.GetCompanyAndBranch();

			var script = $@"
				INSERT dbo.JobDeclaration (JE_PK, JE_DataModel, JE_GB, JE_GC, JE_ClusterKey, JE_DeclarationReference, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser) VALUES
					('{decPk1}', 'AU', '{branchPk}', '{companyPk}', 0 , 'Ref1', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					('{decPk2}', 'AU', '{branchPk}', '{companyPk}', 31, 'Ref2', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					('{decPk3}', 'AU', '{branchPk}', '{companyPk}', 0 , 'Ref3', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					('{decPk4}', 'AU', '{branchPk}', '{companyPk}', 0 , 'Ref4', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
			";

			TestConnection.ExecuteNonQuery(script);
		}

		void AssertClusterKeyValue(Guid[] pks, int[] expectedValues)
		{
			var sql = $"SELECT JE_ClusterKey FROM dbo.JobDeclaration WHERE JE_PK in ('{string.Join("','", pks)}')";
			var ckValues = new List<int>();
			TestConnection.ExecuteReader(sql, (r) => ckValues.Add(r.GetInt32(0)));
			AssertContainsExactElementsInAnyOrder("JE_ClusterKey values", expectedValues, ckValues);
		}

		readonly Guid decPk1 = Guid.NewGuid();
		readonly Guid decPk2 = Guid.NewGuid();
		readonly Guid decPk3 = Guid.NewGuid();
		readonly Guid decPk4 = Guid.NewGuid();
	}
}
