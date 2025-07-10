using CargoWise.Schema;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Testing
{
	sealed class OnlineClusterKeyDoerWithBatchedUpdateTest : TransactionedTestCase
	{
		public void TestEachUpdateBatchStartsClusterKeySequenceFromCurrentMaxValuePlusOne()
		{
			PrepareTestData();

			CombineAssertions("[PRE-CONDITIONS]", () =>
			{
				AssertMaxClusterKeyValue(JobDeclarationSchema.JE_ClusterKey, expectedMax: 2);
				AssertClusterKeyCount(JobDeclarationSchema.JE_ClusterKey, clusterKeyValue: 0, expectedCount: 5);
				AssertClusterKeyCount(JobDeclarationSchema.JE_ClusterKey, clusterKeyValue: 1, expectedCount: 0);
				AssertClusterKeyCount(JobDeclarationSchema.JE_ClusterKey, clusterKeyValue: 2, expectedCount: 1);

				AssertMaxClusterKeyValue(JobComInvoiceHeaderSchema.JZ_ClusterKey, expectedMax: 1);
				AssertClusterKeyCount(JobComInvoiceHeaderSchema.JZ_ClusterKey, clusterKeyValue: 0, expectedCount: 0);
				AssertClusterKeyCount(JobComInvoiceHeaderSchema.JZ_ClusterKey, clusterKeyValue: 1, expectedCount: 1);
			});

			var clusterKeyDoer = new OnlineClusterKeyDoerWithSmallBatchSize();
			var keyDefinitions = new KeyDefinition[] { new KeyDefinition(JobDeclarationSchema.PK, JobComInvoiceHeaderSchema.JZ_JE, isMidLevelMaster: true) };
			clusterKeyDoer.Do(JobDeclarationSchema.PK, keyDefinitions);

			CombineAssertions(() =>
			{
				AssertMaxClusterKeyValue(JobDeclarationSchema.JE_ClusterKey, expectedMax: 7);
				AssertClusterKeyCount(JobDeclarationSchema.JE_ClusterKey, clusterKeyValue: 0, expectedCount: 0);
				AssertClusterKeyCount(JobDeclarationSchema.JE_ClusterKey, clusterKeyValue: 1, expectedCount: 0);
				AssertClusterKeyCount(JobDeclarationSchema.JE_ClusterKey, clusterKeyValue: 2, expectedCount: 1);
				AssertClusterKeyCount(JobDeclarationSchema.JE_ClusterKey, clusterKeyValue: 3, expectedCount: 1);
				AssertClusterKeyCount(JobDeclarationSchema.JE_ClusterKey, clusterKeyValue: 4, expectedCount: 1);
				AssertClusterKeyCount(JobDeclarationSchema.JE_ClusterKey, clusterKeyValue: 5, expectedCount: 1);
				AssertClusterKeyCount(JobDeclarationSchema.JE_ClusterKey, clusterKeyValue: 6, expectedCount: 1);
				AssertClusterKeyCount(JobDeclarationSchema.JE_ClusterKey, clusterKeyValue: 7, expectedCount: 1);

				AssertMaxClusterKeyValue(JobComInvoiceHeaderSchema.JZ_ClusterKey, expectedMax: 1);
				AssertClusterKeyCount(JobComInvoiceHeaderSchema.JZ_ClusterKey, clusterKeyValue: 0, expectedCount: 0);
				AssertClusterKeyCount(JobComInvoiceHeaderSchema.JZ_ClusterKey, clusterKeyValue: 1, expectedCount: 1);

				AssertEquals("FinalMaxClusterKeyValue", 7, clusterKeyDoer.FinalMaxClusterKeyValue);
			});
		}

		void AssertMaxClusterKeyValue(SchemaIntColumn clusterKey, int expectedMax)
		{
			AssertEquals(
				$"Max [{clusterKey.TableName}].[{clusterKey.Name}] value",
				expectedMax,
				TestConnection.ExecuteScalar<int>($"SELECT MAX({clusterKey.Name}) FROM {clusterKey.TableName}"));
		}

		void AssertClusterKeyCount(SchemaIntColumn clusterKey, int clusterKeyValue, int expectedCount)
		{
			AssertEquals(
				$"{clusterKey.Name} = {clusterKeyValue} count",
				expectedCount,
				TestConnection.ExecuteScalar<int>($"SELECT COUNT(*) FROM {clusterKey.TableName} WHERE {clusterKey.Name} = {clusterKeyValue}"));
		}

		void PrepareTestData()
		{
			var participatingClusterKeys = new string[] { JobDeclarationSchema.Constants.JE_ClusterKey };
			ClusterKeyTransformationHelper.DropClusterKeyIndexesAndConstraints(TestConnection, participatingClusterKeys);

			var script = $@"
				DECLARE @GbPk UNIQUEIDENTIFIER
				DECLARE @GcPk UNIQUEIDENTIFIER
				SELECT TOP 1 @GbPk = GB_PK, @GcPk = GB_GC FROM dbo.GlbBranch

				INSERT dbo.JobDeclaration (JE_PK, JE_DataModel, JE_GB, JE_GC, JE_ClusterKey, JE_DeclarationReference, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser) VALUES
					(newid(), 'AU', @GbPk, @GcPk, 0, 'Ref1', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), 'AU', @GbPk, @GcPk, 0, 'Ref2', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), 'AU', @GbPk, @GcPk, 0, 'Ref3', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), 'AU', @GbPk, @GcPk, 2, 'Ref4', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), 'AU', @GbPk, @GcPk, 0, 'Ref5', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), 'AU', @GbPk, @GcPk, 0, 'Ref6', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT dbo.JobComInvoiceHeader (JZ_PK, JZ_DataModel, JZ_GB, JZ_JE, JZ_ClusterKey, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser) VALUES
					(newid(), 'AU', @GbPk, NULL, 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP');
			";

			TestConnection.ExecuteNonQuery(script);
		}

		class OnlineClusterKeyDoerWithSmallBatchSize : ClusterKeyDoer.OnlineClusterKeyDoer
		{
			public OnlineClusterKeyDoerWithSmallBatchSize() : base(new DummyUpgradeManager()) { }
			protected override UpgradeMode UpgMode => UpgradeMode.TestWithBatchSize2;

			protected override void PerformFinalOfflineTransformationSteps(string numberFountainName, SchemaPKColumn topLevelTablePK, int maxClusterKey)
			{
				FinalMaxClusterKeyValue = maxClusterKey;
			}
			public int FinalMaxClusterKeyValue { get; private set; }
		}
	}
}
