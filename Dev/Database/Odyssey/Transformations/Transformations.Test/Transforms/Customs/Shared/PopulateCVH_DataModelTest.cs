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
	[TestedType(typeof(PopulateCVH_DataModel))]
	sealed class PopulateCVH_DataModelTest : DataTransformationTestCase
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
			using var cmd = Db.Connection.Command("SELECT COUNT(*) FROM dbo.CusVehicle WHERE CVH_ParentID = @DeletedInvoiceLinePK");
			cmd.AddParameter("@DeletedInvoiceLinePK", SqlDbType.UniqueIdentifier, DeletedInvoiceLinePK);
			cmd.ExecuteNonQuery();
			AssertEquals("Deleted Data", 1, cmd.ExecuteScalar());
		}

		public void TestOffLinePostUpgrade()
		{
			DBTransformationTestHelper.DropConstraintIfExists(CusVehicleSchema.Constants.TableName, "Constraint_CVH_DataModel");
			AddColumnIfNotExists();
			PrepareTestData();

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			AssertTransformationResults();
			transform.Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			AssertTransformationResults();
			using var cmd = Db.Connection.Command("SELECT COUNT(*) FROM dbo.CusVehicle WHERE CVH_ParentID = @DeletedInvoiceLinePK");
			cmd.AddParameter("@DeletedInvoiceLinePK", SqlDbType.UniqueIdentifier, DeletedInvoiceLinePK);
			cmd.ExecuteNonQuery();
			AssertEquals("Deleted Data", 0, cmd.ExecuteScalar());
		}

		protected override void AssertTransformationResults()
		{
			var values = new List<(string, string)>();
			var sqlText = @"SELECT DataModel, CVH_DataModel 
FROM dbo.CusVehicle
INNER JOIN (
	SELECT PK = JI_PK, DataModel = JI_DataModel FROM dbo.JobComInvoiceLine
) A ON PK = CVH_ParentID
ORDER BY DataModel";

			using (var cmd = Db.Connection.Command(sqlText))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					values.Add((reader.GetString(0), reader.GetString(1)));
				}
			}

			AssertContainsExactElementsInExactOrder(new[] {
				("AI", "AI"),
				("BW", "ASY"),
				("CH", "CH"),
				("CN", "CN"),
				("LS", "ASY"),
				("NA", "ASY"),
				("SZ", "ASY"),
				("US", "US"),
			}, values);
		}

		Guid DeletedInvoiceLinePK = Guid.NewGuid();

		protected override void PrepareTestData()
		{
			int clusterKey = 0;
			foreach (var countryCode in new[] { "AI", "CN", "US", "CH", "BW", "LS", "NA", "SZ" })
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
INSERT INTO dbo.CusVehicle(CVH_PK, CVH_ParentID, CVH_ClusterKey, CVH_ParentTableCode, CVH_SystemCreateTimeUtc, CVH_SystemCreateUser, CVH_SystemLastEditTimeUtc, CVH_SystemLastEditUser) VALUES (NEWID(), @InvoiceLinePK, @ClusterKey, 'JI', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
";
				Db.Connection.ExecuteNonQuery(sqlText);
			}
			using var cmd = Db.Connection.Command("INSERT INTO dbo.CusVehicle(CVH_PK, CVH_ParentID, CVH_ClusterKey, CVH_ParentTableCode, CVH_SystemCreateTimeUtc, CVH_SystemCreateUser, CVH_SystemLastEditTimeUtc, CVH_SystemLastEditUser) VALUES (NEWID(), @DeletedInvoiceLinePK, 111, 'JI', GetUtcDate(), '~BP', GetUtcDate(), '~BP');");
			cmd.AddParameter("@DeletedInvoiceLinePK", SqlDbType.UniqueIdentifier, DeletedInvoiceLinePK);
			cmd.ExecuteNonQuery();
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			var manager = new UpgradeManagerForTestWithOutputBuffer();
			var transform = new PopulateCVH_DataModel();
			transform.Initialise(null, manager);
			return transform;
		}

		protected override IDisposable TransformationRunTwiceTestContextSetupAndDispose()
		{
			DBTransformationTestHelper.DropConstraintIfExists(CusVehicleSchema.Constants.TableName, "Constraint_CVH_DataModel");
			DBTransformationTestHelper.DropDefaultConstraintFor(CusVehicleSchema.Constants.TableName, CusVehicleSchema.Constants.CVH_DataModel);
			DBTransformationTestHelper.DropIndexIfExists(CusVehicleSchema.Constants.TableName, CusVehicleSchema.Constants.Indexes.NR_UX__CVH_ParentID);
			DBTransformationTestHelper.DropColumnIfExists(CusVehicleSchema.Constants.TableName, CusVehicleSchema.Constants.CVH_DataModel);
			return DisposableAction.NoAction;
		}

		static void AddColumnIfNotExists()
		{
			if (!DbObjectCreator.ColumnExists(Db.Connection, CusVehicleSchema.Constants.TableName, CusVehicleSchema.Constants.CVH_DataModel))
			{
				Db.Connection.ExecuteNonQuery(@"ALTER TABLE dbo.CusVehicle
ADD CVH_DataModel VarChar(3) NOT NULL
DEFAULT ''");
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			templateDbCreator = new TablePreSynchroniserTestTemplateDbCreator(UpgUtils.GetTemplateDbName(), CusVehicleSchema.Constants.TableName, new[]
			{
				$"{CusVehicleSchema.Constants.CVH_DataModel} varchar(3) NOT NULL DEFAULT ''",
				$"{CusVehicleSchema.Constants.CVH_SystemLastEditTimeUtc} smalldatetime NOT NULL DEFAULT GETUTCDATE()",
				$"{CusVehicleSchema.Constants.CVH_SystemLastEditUser} varchar(3) NOT NULL DEFAULT '~BP'"
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
