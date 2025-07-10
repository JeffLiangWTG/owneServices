using System;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Testing
{
	sealed class ClusterKeyDoerForMidLevelClusterKeyMasterTest : ClusterKeyDoerWhereDataIsInsertedWhileRunningTest
	{
		public void TestAttachedInvoicesAreNotGivenNewClusterValues()
		{
			var (companyPk, branchPk) = DataHelpers.GetCompanyAndBranch();
			DropIndexesAndConstraints();

			var script = $@"
				INSERT dbo.JobDeclaration (JE_PK, JE_DataModel, JE_GB, JE_GC, JE_ClusterKey, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser) VALUES ('{declarationPk}', 'AU', '{branchPk}', '{companyPk}', 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP');
				INSERT dbo.JobComInvoiceHeader (JZ_PK, JZ_DataModel, JZ_GB, JZ_JE, JZ_ClusterKey, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser) VALUES ('{detachedInvoicePk}', 'AU', '{branchPk}', NULL, 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP');";
			TestConnection.ExecuteNonQuery(script);

			var concurrentUserInsertSimulationScript = $"INSERT dbo.JobComInvoiceHeader (JZ_PK, JZ_DataModel, JZ_JE, JZ_ClusterKey, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser) VALUES ('{attachedInvoicePk}', 'AU', '{declarationPk}', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP');";
			var clusterKeyDoer = new OnlineClusterKeyDoerForOnlineTransformationSimulation(concurrentUserInsertSimulationScript, TestConnection);

			var keyDefinitions = new KeyDefinition[] { new KeyDefinition(JobDeclarationSchema.PK, JobComInvoiceHeaderSchema.JZ_JE, isMidLevelMaster: true) };
			clusterKeyDoer.Do(JobDeclarationSchema.PK, keyDefinitions);

			CombineAssertions(() =>
			{
				AssertEquals("Detached Invoice Cluster Key", 2, (int)TestConnection.ExecuteScalar($"SELECT JZ_ClusterKey FROM dbo.JobComInvoiceHeader WHERE JZ_PK = '{detachedInvoicePk}'"));
				AssertEquals("Attached Invoice Cluster Key", 1, (int)TestConnection.ExecuteScalar($"SELECT JZ_ClusterKey FROM dbo.JobComInvoiceHeader WHERE JZ_PK = '{attachedInvoicePk}'"));
			});
		}

		void DropIndexesAndConstraints()
		{
			var participatingClusterKeys = new string[]
			{
				JobComInvoiceHeaderSchema.Constants.JZ_ClusterKey,
				JobDeclarationSchema.Constants.JE_ClusterKey,
			};

			ClusterKeyTransformationHelper.DropClusterKeyIndexesAndConstraints(TestConnection, participatingClusterKeys);
		}

		readonly Guid declarationPk = Guid.NewGuid();
		readonly Guid attachedInvoicePk = Guid.NewGuid();
		readonly Guid detachedInvoicePk = Guid.NewGuid();
	}
}
