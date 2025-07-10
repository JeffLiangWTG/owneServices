using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DbUpgrader.Schema.OnlineUpgrade;
using Enterprise.DbUpgrader.Schema.OnlineUpgrade.Testing;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.Shared
{
	[TestedType(typeof(PopulateCEG_DataModel))]
	sealed class PopulateCEG_DataModelTest : DataTransformationTestCase
	{
		public void TestOnlinePreUpgrade()
		{
			TransformationRunTwiceTestContextSetupAndDispose();
			PrepareTestData();

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			AssertTransformationResults();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			AssertTransformationResults();

			using var cmd = Db.Connection.Command("SELECT COUNT(*) FROM dbo.CusEngine WHERE CEG_ParentID = @DeletedVehiclePK");
			cmd.AddParameter("@DeletedVehiclePK", SqlDbType.UniqueIdentifier, DeletedVehiclePK);
			cmd.ExecuteNonQuery();
			AssertEquals("Deleted Vehicle", 1, cmd.ExecuteScalar());

			using var cmd2 = Db.Connection.Command("SELECT COUNT(*) FROM dbo.CusEngine WHERE CEG_ParentID = @DeletedInvoiceLinePK");
			cmd2.AddParameter("@DeletedInvoiceLinePK", SqlDbType.UniqueIdentifier, DeletedInvoiceLinePK);
			cmd2.ExecuteNonQuery();
			AssertEquals("Deleted Invoice Line", 1, cmd2.ExecuteScalar());

			using var cmd3 = Db.Connection.Command("SELECT COUNT(*) FROM dbo.CusEngine WHERE CEG_ParentTableCode = 'OP'");
			cmd3.ExecuteNonQuery();
			AssertEquals("CusEngine with CEG_ParentTableCode = 'OP'", 1, cmd3.ExecuteScalar());
		}

		public void TestOffLinePostUpgrade()
		{
			DBTransformationTestHelper.DropConstraintIfExists(CusEngineSchema.Constants.TableName, "Constraint_CEG_DataModel");
			DBTransformationTestHelper.DropConstraintIfExists(CusEngineSchema.Constants.TableName, "Constraint_CEG_ParentTableCode");
			AddColumnIfNotExists();
			PrepareTestData();

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			AssertTransformationResults();
			transform.Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			AssertTransformationResults();

			using var cmd = Db.Connection.Command("SELECT COUNT(*) FROM dbo.CusEngine WHERE CEG_ParentID = @DeletedVehiclePK");
			cmd.AddParameter("@DeletedVehiclePK", SqlDbType.UniqueIdentifier, DeletedVehiclePK);
			cmd.ExecuteNonQuery();
			AssertEquals("Deleted Vehicle", 0, cmd.ExecuteScalar());

			using var cmd2 = Db.Connection.Command("SELECT COUNT(*) FROM dbo.CusEngine WHERE CEG_ParentID = @DeletedInvoiceLinePK");
			cmd2.AddParameter("@DeletedInvoiceLinePK", SqlDbType.UniqueIdentifier, DeletedInvoiceLinePK);
			cmd2.ExecuteNonQuery();
			AssertEquals("Deleted Invoice Line", 0, cmd2.ExecuteScalar());

			using var cmd3 = Db.Connection.Command("SELECT COUNT(*) FROM dbo.CusEngine WHERE CEG_ParentTableCode = 'OP'");
			cmd3.ExecuteNonQuery();
			AssertEquals("CusEngine with CEG_ParentTableCode = 'OP'", 0, cmd3.ExecuteScalar());
		}

		protected override void AssertTransformationResults()
		{
			var values = new List<(string, string, string)>();
			var sqlText = @"SELECT DataModel, CEG_ParentTableCode, CEG_DataModel
FROM dbo.CusEngine INNER JOIN (
	SELECT PK = CVH_PK, DataModel = CVH_DataModel FROM dbo.CusVehicle
	UNION ALL
	SELECT PK = JI_PK, DataModel = JI_DataModel FROM dbo.JobComInvoiceLine
) A ON CEG_ParentID = PK
ORDER BY DataModel, CEG_ParentTableCode";

			using (var cmd = Db.Connection.Command(sqlText))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					values.Add((reader.GetString(0), reader.GetString(1), reader.GetString(2)));
				}
			}

			AssertContainsExactElementsInExactOrder(new[] {
				("AI", "CVH", "AI"),
				("AI", "JI", "AI"),
				("ASY", "CVH", "ASY"),
				("ASY", "CVH", "ASY"),
				("ASY", "CVH", "ASY"),
				("ASY", "CVH", "ASY"),
				("BW", "JI", "BW"),
				("CH", "CVH", "CH"),
				("CH", "JI", "CH"),
				("CN", "CVH", "CN"),
				("CN", "JI", "CN"),
				("LS", "JI", "LS"),
				("NA", "JI", "NA"),
				("PL", "CVH", "PL"),
				("PL", "JI", "PL"),
				("SZ", "JI", "SZ"),
				("TR", "CVH", "TR"),
				("TR", "JI", "TR"),
				("US", "CVH", "US"),
				("US", "JI", "US"),
			}, values);
		}

		Guid DeletedVehiclePK = Guid.NewGuid();
		Guid DeletedInvoiceLinePK = Guid.NewGuid();

		protected override void PrepareTestData()
		{
			int clusterKey = 0;
			foreach (var countryCode in new[] { "AI", "BW", "CH", "CN", "PL", "TR", "US", "NA", "LS", "SZ" })
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
DECLARE @CVH_DataModel VARCHAR(3) = CASE WHEN @CountryCode IN ('NA', 'LS', 'BW', 'SZ') THEN 'ASY' ELSE @CountryCode END;
DECLARE @VehiclePK UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.CusVehicle(CVH_PK, CVH_ParentID, CVH_ClusterKey, CVH_ParentTableCode, CVH_DataModel, CVH_SystemCreateTimeUtc, CVH_SystemCreateUser, CVH_SystemLastEditTimeUtc, CVH_SystemLastEditUser) VALUES (@VehiclePK, @InvoiceLinePK, @ClusterKey, 'JI', @CVH_DataModel, GetUtcDate(), '~BP', GetUtcDate(), '~BP');
INSERT INTO dbo.CusEngine(CEG_PK, CEG_ParentID, CEG_ClusterKey, CEG_ParentTableCode, CEG_SystemCreateTimeUtc, CEG_SystemCreateUser, CEG_SystemLastEditTimeUtc, CEG_SystemLastEditUser) VALUES (NEWID(), @VehiclePK, @ClusterKey, 'CVH', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
INSERT INTO dbo.CusEngine(CEG_PK, CEG_ParentID, CEG_ClusterKey, CEG_ParentTableCode, CEG_SystemCreateTimeUtc, CEG_SystemCreateUser, CEG_SystemLastEditTimeUtc, CEG_SystemLastEditUser) VALUES (NEWID(), @InvoiceLinePK, @ClusterKey, 'JI', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
";
				Db.Connection.ExecuteNonQuery(sqlText);
			}

			using var cmd = Db.Connection.Command("INSERT INTO dbo.CusEngine(CEG_PK, CEG_ParentID, CEG_ClusterKey, CEG_ParentTableCode, CEG_SystemCreateTimeUtc, CEG_SystemCreateUser, CEG_SystemLastEditTimeUtc, CEG_SystemLastEditUser) VALUES (NEWID(), @DeletedVehiclePK, 111, 'CVH', GetUtcDate(), '~BP', GetUtcDate(), '~BP');");
			cmd.AddParameter("@DeletedVehiclePK", SqlDbType.UniqueIdentifier, DeletedVehiclePK);
			cmd.ExecuteNonQuery();

			using var cmd2 = Db.Connection.Command("INSERT INTO dbo.CusEngine(CEG_PK, CEG_ParentID, CEG_ClusterKey, CEG_ParentTableCode, CEG_SystemCreateTimeUtc, CEG_SystemCreateUser, CEG_SystemLastEditTimeUtc, CEG_SystemLastEditUser) VALUES (NEWID(), @DeletedInvoiceLinePK, 222, 'JI', GetUtcDate(), '~BP', GetUtcDate(), '~BP');");
			cmd2.AddParameter("@DeletedInvoiceLinePK", SqlDbType.UniqueIdentifier, DeletedInvoiceLinePK);
			cmd2.ExecuteNonQuery();

			using var cmd3 = Db.Connection.Command("INSERT INTO dbo.CusEngine(CEG_PK, CEG_ParentID, CEG_ClusterKey, CEG_ParentTableCode, CEG_SystemCreateTimeUtc, CEG_SystemCreateUser, CEG_SystemLastEditTimeUtc, CEG_SystemLastEditUser) VALUES (NEWID(), NEWID(), 333, 'OP', GetUtcDate(), '~BP', GetUtcDate(), '~BP');");
			cmd3.ExecuteNonQuery();
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			var manager = new UpgradeManagerForTestWithOutputBuffer();
			var transform = new PopulateCEG_DataModel();
			transform.Initialise(null, manager);
			return transform;
		}

		protected override IDisposable TransformationRunTwiceTestContextSetupAndDispose()
		{
			DBTransformationTestHelper.DropConstraintIfExists(CusEngineSchema.Constants.TableName, "Constraint_CEG_DataModel");
			DBTransformationTestHelper.DropConstraintIfExists(CusEngineSchema.Constants.TableName, "Constraint_CEG_ParentTableCode");
			DBTransformationTestHelper.DropDefaultConstraintFor(CusEngineSchema.Constants.TableName, CusEngineSchema.Constants.CEG_DataModel);
			DBTransformationTestHelper.DropIndexIfExists(CusEngineSchema.Constants.TableName, CusEngineSchema.Constants.Indexes.NR_UX__CEG_ParentID);
			DBTransformationTestHelper.DropColumnIfExists(CusEngineSchema.Constants.TableName, CusEngineSchema.Constants.CEG_DataModel);
			return DisposableAction.NoAction;
		}

		static void AddColumnIfNotExists()
		{
			if (!DbObjectCreator.ColumnExists(Db.Connection, CusEngineSchema.Constants.TableName, CusEngineSchema.Constants.CEG_DataModel))
			{
				Db.Connection.ExecuteNonQuery(@"ALTER TABLE dbo.CusEngine
ADD CEG_DataModel VarChar(3) NOT NULL
DEFAULT ''");
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			templateDbCreator = new TablePreSynchroniserTestTemplateDbCreator(UpgUtils.GetTemplateDbName(), CusEngineSchema.Constants.TableName, new[]
			{
				$"{CusEngineSchema.Constants.CEG_DataModel} varchar(3) NOT NULL DEFAULT ''",
				$"{CusEngineSchema.Constants.CEG_SystemLastEditTimeUtc} smalldatetime NOT NULL DEFAULT GETUTCDATE()",
				$"{CusEngineSchema.Constants.CEG_SystemLastEditUser} varchar(3) NOT NULL DEFAULT '~BP'"
			 });
			templateDbCreator.CreateDropExisting();
			TablePreSynchroniser.CreatePreAddDb_ForTest();
		}

		protected override void OnAfterBaseTestCaseRunBare()
		{
			// Drop can only happen once transaction rolled back.
			TablePreSynchroniser.DropPreAddDb_ForTest();
			templateDbCreator.Drop();
			base.OnAfterBaseTestCaseRunBare();
		}

		IAuxiliaryDbCreator templateDbCreator;
	}
}
