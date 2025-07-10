using System;
using System.Collections.Generic;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Testing
{
	sealed class ClusterKeyDoerWithStmNumberCacheTest : TransactionedTestCase
	{
		public void TestClusterKeyDoerWhenStmNumberCacheOverlap()
		{
			PrepareAsycudaTestData();
			PrepareStmNumberCacheTestData();

			var clusterKeyDoer = ClusterKeyDoer.New(new DummyUpgradeManager(), false);
			clusterKeyDoer.Do(AsycudaManifestHeaderSchema.PK, definitions, "AsycudaManifestHeaderClusterKey");

			CombineAssertions(() =>
			{
				AssertEquals("Number Fountain updated", 4, (long)TestConnection.ExecuteScalar("SELECT SN_Value FROM dbo.StmNums WHERE SN_Name = 'AsycudaManifestHeaderClusterKey'"));
				var numberFountainId = (int)TestConnection.ExecuteScalar("SELECT SN_Id FROM dbo.StmNums WHERE SN_Name = 'AsycudaManifestHeaderClusterKey'");
				AssertEquals("StmNumberCache 1 Updated", true, (bool)TestConnection.ExecuteScalar($"SELECT SG_IsUsed FROM dbo.StmNumberCache WHERE SG_Value = 1 AND SG_SN = {numberFountainId}"));
				AssertEquals("StmNumberCache 2 Updated", true, (bool)TestConnection.ExecuteScalar($"SELECT SG_IsUsed FROM dbo.StmNumberCache WHERE SG_Value = 2 AND SG_SN = {numberFountainId}"));
			});

			void PrepareStmNumberCacheTestData()
			{
				var script = @"
					DECLARE @numberFountainName VARCHAR(256) = 'AsycudaManifestHeaderClusterKey';
					IF NOT EXISTS (SELECT NULL FROM dbo.StmNums WHERE SN_Name = @numberFountainName)
					BEGIN
						INSERT INTO dbo.StmNums(SN_Name, SN_Value, SN_MinimumValue, SN_MaximumValue, SN_CanRollover, SN_Sequence, SN_SystemCreateTimeUtc)
						VALUES (@numberFountainName, 1, 1, 2147483647, 0, 0, GETUTCDATE())
					END

					DECLARE @numberFountainId INT = (SELECT SN_Id FROM dbo.StmNums WHERE SN_Name = @numberFountainName);
					DELETE FROM dbo.StmNumberCache WHERE SG_SN = @numberFountainId;
					UPDATE dbo.StmNums SET SN_VALUE = 3 WHERE SN_ID = @numberFountainId;
					INSERT INTO dbo.StmNumberCache (SG_VALUE, SG_SN, SG_IsUsed) VALUES (1, @numberFountainId, 0);
					INSERT INTO dbo.StmNumberCache (SG_VALUE, SG_SN, SG_IsUsed) VALUES (2, @numberFountainId, 0);";

				TestConnection.ExecuteNonQuery(script);
			}
		}

		public void TestClusterKeyDoerWhenNoStmNumberCacheOverlap()
		{
			PrepareAsycudaTestData();
			PrepareStmNumberCacheTestData();

			var clusterKeyDoer = ClusterKeyDoer.New(new DummyUpgradeManager(), false);
			clusterKeyDoer.Do(AsycudaManifestHeaderSchema.PK, definitions, "AsycudaManifestHeaderClusterKey");

			CombineAssertions(() =>
			{
				AssertEquals("Number Fountain already higher so unchanged", 8, (long)TestConnection.ExecuteScalar("SELECT SN_Value FROM dbo.StmNums WHERE SN_Name = 'AsycudaManifestHeaderClusterKey'"));
				var numberFountainId = (int)TestConnection.ExecuteScalar("SELECT SN_Id FROM dbo.StmNums WHERE SN_Name = 'AsycudaManifestHeaderClusterKey'");
				AssertEquals("StmNumberCache 1 Unchanged", true, (bool)TestConnection.ExecuteScalar($"SELECT SG_IsUsed FROM dbo.StmNumberCache WHERE SG_Value = 1 AND SG_SN = {numberFountainId}"));
				AssertEquals("StmNumberCache 2 Unchanged", true, (bool)TestConnection.ExecuteScalar($"SELECT SG_IsUsed FROM dbo.StmNumberCache WHERE SG_Value = 2 AND SG_SN = {numberFountainId}"));
				AssertEquals("StmNumberCache 3 Unchanged", true, (bool)TestConnection.ExecuteScalar($"SELECT SG_IsUsed FROM dbo.StmNumberCache WHERE SG_Value = 3 AND SG_SN = {numberFountainId}"));
				AssertEquals("StmNumberCache 4 Unchanged", true, (bool)TestConnection.ExecuteScalar($"SELECT SG_IsUsed FROM dbo.StmNumberCache WHERE SG_Value = 4 AND SG_SN = {numberFountainId}"));
				AssertEquals("StmNumberCache 5 Unchanged", true, (bool)TestConnection.ExecuteScalar($"SELECT SG_IsUsed FROM dbo.StmNumberCache WHERE SG_Value = 5 AND SG_SN = {numberFountainId}"));
				AssertEquals("StmNumberCache 6 Unchanged", false, (bool)TestConnection.ExecuteScalar($"SELECT SG_IsUsed FROM dbo.StmNumberCache WHERE SG_Value = 6 AND SG_SN = {numberFountainId}"));
				AssertEquals("StmNumberCache 7 Unchanged", false, (bool)TestConnection.ExecuteScalar($"SELECT SG_IsUsed FROM dbo.StmNumberCache WHERE SG_Value = 7 AND SG_SN = {numberFountainId}"));
			});

			void PrepareStmNumberCacheTestData()
			{
				var script = @"
					DECLARE @numberFountainName VARCHAR(256) = 'AsycudaManifestHeaderClusterKey';
					IF NOT EXISTS (SELECT NULL FROM dbo.StmNums WHERE SN_Name = @numberFountainName)
					BEGIN
						INSERT INTO dbo.StmNums(SN_Name, SN_Value, SN_MinimumValue, SN_MaximumValue, SN_CanRollover, SN_Sequence, SN_SystemCreateTimeUtc)
						VALUES (@numberFountainName, 1, 1, 2147483647, 0, 0, GETUTCDATE())
					END

					DECLARE @numberFountainId INT = (SELECT SN_Id FROM dbo.StmNums WHERE SN_Name = @numberFountainName);
					DELETE FROM dbo.StmNumberCache WHERE SG_SN = @numberFountainId;
					UPDATE dbo.StmNums SET SN_VALUE = 8 WHERE SN_ID = @numberFountainId;
					INSERT INTO dbo.StmNumberCache (SG_VALUE, SG_SN, SG_IsUsed) VALUES (1, @numberFountainId, 1);
					INSERT INTO dbo.StmNumberCache (SG_VALUE, SG_SN, SG_IsUsed) VALUES (2, @numberFountainId, 1);
					INSERT INTO dbo.StmNumberCache (SG_VALUE, SG_SN, SG_IsUsed) VALUES (3, @numberFountainId, 1);
					INSERT INTO dbo.StmNumberCache (SG_VALUE, SG_SN, SG_IsUsed) VALUES (4, @numberFountainId, 1);
					INSERT INTO dbo.StmNumberCache (SG_VALUE, SG_SN, SG_IsUsed) VALUES (5, @numberFountainId, 1);
					INSERT INTO dbo.StmNumberCache (SG_VALUE, SG_SN, SG_IsUsed) VALUES (6, @numberFountainId, 0);
					INSERT INTO dbo.StmNumberCache (SG_VALUE, SG_SN, SG_IsUsed) VALUES (7, @numberFountainId, 0);";

				TestConnection.ExecuteNonQuery(script);
			}
		}

		public void TestClusterKeyDoerWhenNoStmNumberCache()
		{
			PrepareAsycudaTestData();
			TestConnection.ExecuteNonQuery("DELETE FROM dbo.StmNums WHERE SN_Name = 'AsycudaManifestHeaderClusterKey'");

			var clusterKeyDoer = ClusterKeyDoer.New(new DummyUpgradeManager(), false);
			clusterKeyDoer.Do(AsycudaManifestHeaderSchema.PK, definitions, "AsycudaManifestHeaderClusterKey");

			CombineAssertions(() =>
			{
				AssertEquals("StmNum Created", 4, (long)TestConnection.ExecuteScalar("SELECT SN_Value FROM dbo.StmNums WHERE SN_Name = 'AsycudaManifestHeaderClusterKey'"));
				AssertEquals("No StmNumberCache", 0, (int)TestConnection.ExecuteScalar("SELECT COUNT(1) FROM dbo.StmNumberCache WHERE SG_SN IN (SELECT SN_Id FROM dbo.StmNums WHERE SN_Name = 'AsycudaManifestHeaderClusterKey')"));
			});
		}

		void PrepareAsycudaTestData()
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

DELETE FROM dbo.AsycudaManifestHeader;

INSERT INTO dbo.AsycudaManifestHeader (AMA_PK, AMA_JobReference, AMA_SystemCreateTimeUtc, AMA_SystemLastEditTimeUtc, AMA_SystemCreateUser, AMA_SystemLastEditUser, AMA_RN_NKCountry, AMA_GB) VALUES('{manifestHeaderPK1}', 'M01', '2019-02-12', '2019-02-12', 'AA', 'AA', 'ZA', '{DataHelpers.GetFirstKey(GlbBranchSchema.Instance)}');
INSERT INTO dbo.AsycudaManifestHeader (AMA_PK, AMA_JobReference, AMA_SystemCreateTimeUtc, AMA_SystemLastEditTimeUtc, AMA_SystemCreateUser, AMA_SystemLastEditUser, AMA_RN_NKCountry, AMA_GB) VALUES('{manifestHeaderPK2}', 'M11', '2019-02-13', '2019-02-13', 'AA', 'AA', 'SG', '{DataHelpers.GetFirstKey(GlbBranchSchema.Instance)}');
INSERT INTO dbo.AsycudaManifestHeader (AMA_PK, AMA_JobReference, AMA_SystemCreateTimeUtc, AMA_SystemLastEditTimeUtc, AMA_SystemCreateUser, AMA_SystemLastEditUser, AMA_RN_NKCountry, AMA_GB) VALUES('{manifestHeaderPK3}', 'M21', '2019-02-14', '2019-02-14', 'AA', 'AA', 'FJ', '{DataHelpers.GetFirstKey(GlbBranchSchema.Instance)}');
";

			TestConnection.ExecuteNonQuery(script);
		}

		readonly List<KeyDefinition> definitions = new List<KeyDefinition>()
			{
				new KeyDefinition(AsycudaManifestHeaderSchema.PK, AsycudaContainerSchema.ACN_AMA_Manifest),
				new KeyDefinition(AsycudaManifestHeaderSchema.PK, AsycudaBillSchema.ABL_AMA),
				new KeyDefinition(AsycudaBillSchema.PK, AsycudaPackSchema.APA_ABL_Bill),
				new KeyDefinition(AsycudaPackSchema.PK, AsycudaPackedItemSchema.API_ABL_Bill),
				new KeyDefinition(AsycudaPackSchema.PK, AsycudaContainerBillOrPackageLinkSchema.APC_APA_Pack),
			};

		readonly Guid manifestHeaderPK1 = Guid.NewGuid();
		readonly Guid manifestHeaderPK2 = Guid.NewGuid();
		readonly Guid manifestHeaderPK3 = Guid.NewGuid();
	}
}
