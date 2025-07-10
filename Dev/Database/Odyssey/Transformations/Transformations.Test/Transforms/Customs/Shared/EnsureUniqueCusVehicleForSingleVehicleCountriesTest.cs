using System.Collections.Generic;
using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.Shared
{
	[TestedType(typeof(EnsureUniqueCusVehicleForSingleVehicleCountries))]
	sealed class EnsureUniqueCusVehicleForSingleVehicleCountriesTest : DataTransformationTestCase
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
			var values = new List<(string, string)>();
			var sqlText = @"SELECT DataModel, CVH_VehicleIdentificationNumber 
FROM dbo.CusVehicle
INNER JOIN (
	SELECT PK = JI_PK, DataModel = JI_DataModel FROM dbo.JobComInvoiceLine
) A ON PK = CVH_ParentID
ORDER BY DataModel, CVH_VehicleIdentificationNumber";

			using (var cmd = Db.Connection.Command(sqlText))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					values.Add((reader.GetString(0), reader.GetString(1)));
				}
			}

			AssertContainsExactElementsInExactOrder(new[] {
				("BW", "SortedByPk"),
				("CH", "EarliestCreated"),
				("CH", "FirstInserted"),
				("CH", "LastInserted"),
				("CH", "LatestCreated"),
				("CH", "SortedByPk"),
				("ES", "FirstInserted"),
				("LS", "SortedByPk"),
				("NA", "SortedByPk"),
				("PL", "LatestCreated"),
				("SZ", "SortedByPk"),
				("US", "EarliestCreated"),
				("US", "FirstInserted"),
				("US", "LastInserted"),
				("US", "LatestCreated"),
				("US", "SortedByPk"),
			}, values);
		}

		protected override void PrepareTestData()
		{
			int clusterKey = 0;
			foreach (var countryCode in new[] { "US", "CH", "BW", "LS", "NA", "SZ", "ES", "PL" })
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
INSERT INTO dbo.CusVehicle(CVH_PK, CVH_ParentID, CVH_ClusterKey, CVH_ParentTableCode, CVH_DataModel, CVH_VehicleIdentificationNumber, CVH_SystemCreateTimeUtc, CVH_SystemCreateUser, CVH_SystemLastEditTimeUtc, CVH_SystemLastEditUser) VALUES (NEWID(), @InvoiceLinePK, @ClusterKey, 'JI', @CountryCode, 'FirstInserted', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
INSERT INTO dbo.CusVehicle(CVH_PK, CVH_ParentID, CVH_ClusterKey, CVH_ParentTableCode, CVH_DataModel, CVH_VehicleIdentificationNumber, CVH_SystemCreateTimeUtc, CVH_SystemCreateUser, CVH_SystemLastEditTimeUtc, CVH_SystemLastEditUser) VALUES (NEWID(), @InvoiceLinePK, @ClusterKey, 'JI', @CountryCode, 'EarliestCreated', DateAdd(day, -1, GetUtcDate()), '~BP', GetUtcDate(), '~BP');
INSERT INTO dbo.CusVehicle(CVH_PK, CVH_ParentID, CVH_ClusterKey, CVH_ParentTableCode, CVH_DataModel, CVH_VehicleIdentificationNumber, CVH_SystemCreateTimeUtc, CVH_SystemCreateUser, CVH_SystemLastEditTimeUtc, CVH_SystemLastEditUser) VALUES (NEWID(), @InvoiceLinePK, @ClusterKey, 'JI', @CountryCode, 'LatestCreated', DateAdd(day, 1, GetUtcDate()), '~BP', GetUtcDate(), '~BP');
INSERT INTO dbo.CusVehicle(CVH_PK, CVH_ParentID, CVH_ClusterKey, CVH_ParentTableCode, CVH_DataModel, CVH_VehicleIdentificationNumber, CVH_SystemCreateTimeUtc, CVH_SystemCreateUser, CVH_SystemLastEditTimeUtc, CVH_SystemLastEditUser) VALUES ('00000000-0000-0000-0000-00000000000' + CAST(@ClusterKey AS VARCHAR(1)), @InvoiceLinePK, @ClusterKey, 'JI', @CountryCode, 'SortedByPk', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
INSERT INTO dbo.CusVehicle(CVH_PK, CVH_ParentID, CVH_ClusterKey, CVH_ParentTableCode, CVH_DataModel, CVH_VehicleIdentificationNumber, CVH_SystemCreateTimeUtc, CVH_SystemCreateUser, CVH_SystemLastEditTimeUtc, CVH_SystemLastEditUser) VALUES (NEWID(), @InvoiceLinePK, @ClusterKey, 'JI', @CountryCode, 'LastInserted', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
";
				Db.Connection.ExecuteNonQuery(sqlText);
			}
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			var manager = new UpgradeManagerForTestWithOutputBuffer();
			var transform = new EnsureUniqueCusVehicleForSingleVehicleCountries();
			transform.Initialise(null, manager);
			return transform;
		}

		protected override void SetUp()
		{
			base.SetUp();
			AddColumnIfNotExists();
			DBTransformationTestHelper.DropIndexIfExists(CusVehicleSchema.Constants.TableName, CusVehicleSchema.Constants.Indexes.NR_UX__CVH_ParentID);
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
	}
}
