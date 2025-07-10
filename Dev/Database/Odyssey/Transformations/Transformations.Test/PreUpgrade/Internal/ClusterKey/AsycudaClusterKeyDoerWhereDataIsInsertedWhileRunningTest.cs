using System;
using System.Collections.Generic;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Testing
{
	sealed class AsycudaClusterKeyDoerWhereDataIsInsertedWhileRunningTest : ClusterKeyDoerWhereDataIsInsertedWhileRunningTest
	{
		public void TestClusterKeyDoerWhereDataIsInsertedWhileRunning()
		{
			PrepareTestData();

			var definitions = new List<KeyDefinition>()
			{
				new KeyDefinition(AsycudaManifestHeaderSchema.PK, AsycudaContainerSchema.ACN_AMA_Manifest),
				new KeyDefinition(AsycudaManifestHeaderSchema.PK, AsycudaBillSchema.ABL_AMA),
				new KeyDefinition(AsycudaBillSchema.PK, AsycudaPackSchema.APA_ABL_Bill),
				new KeyDefinition(AsycudaPackSchema.PK, AsycudaPackedItemSchema.API_ABL_Bill),
				new KeyDefinition(AsycudaPackSchema.PK, AsycudaContainerBillOrPackageLinkSchema.APC_APA_Pack),
			};

			var concurrentUserInsertSimulationScript = $@"
				INSERT INTO dbo.AsycudaManifestHeader (AMA_PK, AMA_JobReference, AMA_SystemCreateTimeUtc, AMA_SystemLastEditTimeUtc, AMA_SystemCreateUser, AMA_SystemLastEditUser, AMA_RN_NKCountry, AMA_GB) VALUES('{manifestHeaderPK2}', 'M11', '2019-02-13', '2019-02-13', 'BB', 'BB', 'SG', '{DataHelpers.GetFirstKey(GlbBranchSchema.Instance)}');
				INSERT INTO dbo.AsycudaBill (ABL_PK, ABL_AMA, ABL_SystemCreateTimeUtc, ABL_SystemLastEditTimeUtc, ABL_SystemCreateUser, ABL_SystemLastEditUser) VALUES('{billPK2}', '{manifestHeaderPK2}', '2019-02-13', '2019-02-13', 'BB', 'BB');
				INSERT INTO dbo.AsycudaPack (APA_PK, APA_ABL_Bill, APA_SystemCreateTimeUtc, APA_SystemLastEditTimeUtc, APA_SystemCreateUser, APA_SystemLastEditUser) VALUES('{packPK2}', '{billPK2}', GetUtcDate(), GetUtcDate(), '~BP', '~BP');
				INSERT INTO dbo.AsycudaPackedItem (API_PK, API_ABL_Bill, API_SystemCreateTimeUtc, API_SystemLastEditTimeUtc, API_SystemCreateUser, API_SystemLastEditUser) VALUES('{packedItemPK2}', '{billPK2}', GetUtcDate(), GetUtcDate(), '~BP', '~BP')";

			var clusterKeyDoer = new OnlineClusterKeyDoerForOnlineTransformationSimulation(concurrentUserInsertSimulationScript, TestConnection);
			clusterKeyDoer.Do(AsycudaManifestHeaderSchema.PK, definitions, "AsycudaManifestHeaderClusterKey");

			CombineAssertions(() =>
			{
				AssertEquals("Header left until second run", 0, (int)TestConnection.ExecuteScalar($"SELECT AMA_ClusterKey FROM dbo.AsycudaManifestHeader WHERE AMA_PK = '{manifestHeaderPK2}'"));
				AssertEquals("Bill left until second run", 0, (int)TestConnection.ExecuteScalar($"SELECT ABL_ClusterKey FROM dbo.AsycudaBill WHERE ABL_PK = '{billPK2}'"));
				AssertEquals("Pack left until second run", 0, (int)TestConnection.ExecuteScalar($"SELECT APA_ClusterKey FROM dbo.AsycudaPack WHERE APA_PK = '{packPK2}'"));
				AssertEquals("Packed Item left until second run", 0, (int)TestConnection.ExecuteScalar($"SELECT API_ClusterKey FROM dbo.AsycudaPackedItem WHERE API_PK = '{packedItemPK2}'"));
			});
		}

		void PrepareTestData()
		{
			var script = $@"
DROP INDEX NR_UC__AMA_ClusterKey ON AsycudaManifestHeader;
ALTER TABLE dbo.AsycudaManifestHeader DROP CONSTRAINT Constraint_AMA_ClusterKey;
DROP INDEX NR_RC__ABL_ClusterKey ON AsycudaBill;
ALTER TABLE dbo.AsycudaBill DROP CONSTRAINT Constraint_ABL_ClusterKey;
DROP INDEX NR_RC__ACN_ClusterKey ON AsycudaContainer;
ALTER TABLE dbo.AsycudaContainer DROP CONSTRAINT Constraint_ACN_ClusterKey;
DROP INDEX NR_RC__APA_ClusterKey_APA_ABL_Bill ON AsycudaPack;
ALTER TABLE dbo.AsycudaPack DROP CONSTRAINT Constraint_APA_ClusterKey;
DROP INDEX NR_RC__APC_ClusterKey_APC_APA_Pack ON AsycudaContainerBillOrPackageLink;
ALTER TABLE dbo.AsycudaContainerBillOrPackageLink DROP CONSTRAINT Constraint_APC_ClusterKey;
DROP INDEX NR_RC__API_ClusterKey ON AsycudaPackedItem;
ALTER TABLE dbo.AsycudaPackedItem DROP CONSTRAINT Constraint_API_ClusterKey;
DROP INDEX NR_RC__ATH_ClusterKey ON AsycudaArrivalHeader;
ALTER TABLE dbo.AsycudaArrivalHeader DROP CONSTRAINT Constraint_ATH_ClusterKey;
DROP INDEX NR_RC__ATL_ClusterKey ON AsycudaArrivalLine;
ALTER TABLE dbo.AsycudaArrivalLine DROP CONSTRAINT Constraint_ATL_ClusterKey;

INSERT INTO dbo.AsycudaManifestHeader (AMA_PK, AMA_JobReference, AMA_SystemCreateTimeUtc, AMA_SystemLastEditTimeUtc, AMA_SystemCreateUser, AMA_SystemLastEditUser, AMA_RN_NKCountry, AMA_GB) VALUES('{manifestHeaderPK1}', 'M01', '2019-02-12', '2019-02-12', 'AA', 'AA', 'ZA', '{DataHelpers.GetFirstKey(GlbBranchSchema.Instance)}');
INSERT INTO dbo.AsycudaBill (ABL_PK, ABL_AMA, ABL_SystemCreateTimeUtc, ABL_SystemLastEditTimeUtc, ABL_SystemCreateUser, ABL_SystemLastEditUser) VALUES('{billPK1}', '{manifestHeaderPK1}', '2019-02-12', '2019-02-12', 'AA', 'AA');
INSERT INTO dbo.AsycudaPack (APA_PK, APA_ABL_Bill, APA_SystemCreateTimeUtc, APA_SystemCreateUser, APA_SystemLastEditTimeUtc, APA_SystemLastEditUser) VALUES('{packPK1}', '{billPK1}', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
INSERT INTO dbo.AsycudaPackedItem (API_PK, API_ABL_Bill, API_SystemCreateTimeUtc, API_SystemCreateUser, API_SystemLastEditTimeUtc, API_SystemLastEditUser) VALUES(NEWID(), '{billPK1}', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
";

			TestConnection.ExecuteNonQuery(script);
		}

		readonly Guid manifestHeaderPK1 = Guid.NewGuid();
		readonly Guid billPK1 = Guid.NewGuid();
		readonly Guid packPK1 = Guid.NewGuid();

		readonly Guid manifestHeaderPK2 = Guid.NewGuid();
		readonly Guid billPK2 = Guid.NewGuid();
		readonly Guid packPK2 = Guid.NewGuid();
		readonly Guid packedItemPK2 = Guid.NewGuid();
	}
}
