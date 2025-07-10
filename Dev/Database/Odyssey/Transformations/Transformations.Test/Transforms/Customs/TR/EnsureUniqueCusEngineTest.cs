using System.Collections.Generic;
using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.TR;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.TR
{
	[TestedType(typeof(EnsureUniqueCusEngine))]
	sealed class EnsureUniqueCusEngineTest : DataTransformationTestCase
	{
		public void TestOfflinePreUpgrade()
		{
			PrepareTestData();

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);
			AssertTransformationResults();
			transform.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);
			AssertTransformationResults();
		}

		protected override void AssertTransformationResults()
		{
			var values = new List<(string, string, string)>();
			var sqlText = @"SELECT CEG_DataModel, CVH_VehicleIdentificationNumber, CEG_EngineModel
FROM dbo.CusEngine
INNER JOIN dbo.CusVehicle ON CEG_ParentID = CVH_PK
ORDER BY CEG_DataModel, CVH_VehicleIdentificationNumber, CEG_EngineModel";

			using (var cmd = Db.Connection.Command(sqlText))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					values.Add((reader.GetString(0), reader.GetString(1), reader.GetString(2)));
				}
			}

			AssertContainsExactElementsInExactOrder(new[] {
				("TR", "FirstVehicle", "LatestCreatedEngine"),
				("TR", "SecondVehicle", "LatestCreatedEngine"),
				("US", "FirstVehicle", "EarliestCreatedEngine"),
				("US", "FirstVehicle", "LastInsertedEngine"),
				("US", "FirstVehicle", "LatestCreatedEngine"),
				("US", "SecondVehicle", "LastInsertedEngine"),
				("US", "SecondVehicle", "LatestCreatedEngine")
			}, values);
		}

		protected override void PrepareTestData()
		{
			int clusterKey = 0;
			foreach (var countryCode in new[] { "US", "TR" })
			{
				clusterKey++;
				var sqlText = $@"
DECLARE @CountryCode VARCHAR(2) = '{countryCode}';
DECLARE @ClusterKey INT = {clusterKey};
DECLARE @CompanyPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.GlbCompany(GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_SystemCreateTimeUtc, GC_SystemCreateUser, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser) VALUES (@CompanyPK, 'D' + @CountryCode, 'AU company', @CountryCode, 'XCD', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
DECLARE @BranchPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.GlbBranch(GB_PK, GB_GC, GB_Code, GB_SystemCreateTimeUtc, GB_SystemCreateUser, GB_SystemLastEditTimeUtc, GB_SystemLastEditUser) VALUES(@BranchPK, @CompanyPK, 'B' + @CountryCode, GetUtcDate(), '~BP', GetUtcDate(), '~BP');
DECLARE @DecPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.JobDeclaration(JE_PK, JE_GB, JE_GC, JE_ClusterKey, JE_DataModel, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser) VALUES (@DecPK, @BranchPK, @CompanyPK, @ClusterKey, @CountryCode, GetUtcDate(), '~BP', GetUtcDate(), '~BP');
DECLARE @InvoicePK UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.JobComInvoiceHeader(JZ_PK, JZ_JE, JZ_ClusterKey, JZ_DataModel, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser) VALUES (@InvoicePK, @DecPK, @ClusterKey, @CountryCode, GetUtcDate(), '~BP', GetUtcDate(), '~BP');
DECLARE @InvoiceLinePK UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_JZ, JI_ClusterKey, JI_LineNo, JI_DataModel, JI_AddInfo, JI_SystemCreateTimeUtc, JI_SystemCreateUser, JI_SystemLastEditTimeUtc, JI_SystemLastEditUser) VALUES (@InvoiceLinePK, @InvoicePK, @ClusterKey, 1, @CountryCode, '', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
DECLARE @Vehicle1PK UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.CusVehicle(CVH_PK, CVH_ParentID, CVH_ClusterKey, CVH_ParentTableCode, CVH_DataModel, CVH_VehicleIdentificationNumber, CVH_SystemCreateTimeUtc, CVH_SystemCreateUser, CVH_SystemLastEditTimeUtc, CVH_SystemLastEditUser) VALUES (@Vehicle1PK, @InvoiceLinePK, @ClusterKey, 'JI', @CountryCode, 'FirstVehicle', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
INSERT INTO dbo.CusEngine(CEG_PK, CEG_ParentID, CEG_ClusterKey, CEG_ParentTableCode, CEG_DataModel, CEG_EngineModel, CEG_SystemCreateTimeUtc, CEG_SystemCreateUser, CEG_SystemLastEditTimeUtc, CEG_SystemLastEditUser) VALUES (NEWID(), @Vehicle1PK, @ClusterKey, 'CVH', @CountryCode, 'EarliestCreatedEngine', DateAdd(day, -1, GetUtcDate()), '~BP', GetUtcDate(), '~BP');
INSERT INTO dbo.CusEngine(CEG_PK, CEG_ParentID, CEG_ClusterKey, CEG_ParentTableCode, CEG_DataModel, CEG_EngineModel, CEG_SystemCreateTimeUtc, CEG_SystemCreateUser, CEG_SystemLastEditTimeUtc, CEG_SystemLastEditUser) VALUES (NEWID(), @Vehicle1PK, @ClusterKey, 'CVH', @CountryCode, 'LatestCreatedEngine', DateAdd(day, 1, GetUtcDate()), '~BP', GetUtcDate(), '~BP');
INSERT INTO dbo.CusEngine(CEG_PK, CEG_ParentID, CEG_ClusterKey, CEG_ParentTableCode, CEG_DataModel, CEG_EngineModel, CEG_SystemCreateTimeUtc, CEG_SystemCreateUser, CEG_SystemLastEditTimeUtc, CEG_SystemLastEditUser) VALUES (NEWID(), @Vehicle1PK, @ClusterKey, 'CVH', @CountryCode, 'LastInsertedEngine', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

DECLARE @Vehicle2PK UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.CusVehicle(CVH_PK, CVH_ParentID, CVH_ClusterKey, CVH_ParentTableCode, CVH_DataModel, CVH_VehicleIdentificationNumber, CVH_SystemCreateTimeUtc, CVH_SystemCreateUser, CVH_SystemLastEditTimeUtc, CVH_SystemLastEditUser) VALUES (@Vehicle2PK, @InvoiceLinePK, @ClusterKey, 'JI', @CountryCode, 'SecondVehicle', DateAdd(day, -1, GetUtcDate()), '~BP', GetUtcDate(), '~BP');
INSERT INTO dbo.CusEngine(CEG_PK, CEG_ParentID, CEG_ClusterKey, CEG_ParentTableCode, CEG_DataModel, CEG_EngineModel, CEG_SystemCreateTimeUtc, CEG_SystemCreateUser, CEG_SystemLastEditTimeUtc, CEG_SystemLastEditUser) VALUES (NEWID(), @Vehicle2PK, @ClusterKey, 'CVH', @CountryCode, 'LatestCreatedEngine', DateAdd(day, 1, GetUtcDate()), '~BP', GetUtcDate(), '~BP');
INSERT INTO dbo.CusEngine(CEG_PK, CEG_ParentID, CEG_ClusterKey, CEG_ParentTableCode, CEG_DataModel, CEG_EngineModel, CEG_SystemCreateTimeUtc, CEG_SystemCreateUser, CEG_SystemLastEditTimeUtc, CEG_SystemLastEditUser) VALUES (NEWID(), @Vehicle2PK, @ClusterKey, 'CVH', @CountryCode, 'LastInsertedEngine', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
";
				Db.Connection.ExecuteNonQuery(sqlText);
			}
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			var manager = new UpgradeManagerForTestWithOutputBuffer();
			var transform = new EnsureUniqueCusEngine();
			transform.Initialise(null, manager);
			return transform;
		}

		protected override void SetUp()
		{
			base.SetUp();
			AddColumnIfNotExists();
			DBTransformationTestHelper.DropIndexIfExists(CusEngineSchema.Constants.TableName, CusEngineSchema.Constants.Indexes.NR_UX__CEG_ParentID);
		}

		static void AddColumnIfNotExists()
		{
			if (!DbObjectCreator.ColumnExists(Db.Connection, CusEngineSchema.Constants.TableName, CusEngineSchema.Constants.CEG_DataModel))
			{
				Db.Connection.ExecuteNonQuery(@"ALTER TABLE dbo.CusEngine
					ADD CEG_DataModel VARCHAR(3) NOT NULL DEFAULT ''");
			}
		}
	}
}
