using System.Collections.Generic;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.ES;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.ES
{
	[TestedType(typeof(PopulateCVH_BrandNameAndCVH_ModelName))]
	class PopulateCVH_BrandNameAndCVH_ModelNameTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new PopulateCVH_BrandNameAndCVH_ModelName();

		public void TestUserDescription()
		{
			AssertEquals("Change the field JobComInvoiceLine.JI_BrandName to CusVehicle.CVH_BrandName and JobComInvoiceLine.JI_Model to CusVehicle.CVH_ModelName.", GetNewTestTransformationInstance().UserDescription);
		}

		protected override void PrepareTestData()
		{
			var clusterKey1 = 1;
			var clusterKey2 = 2;
			var countryCodeES = "ES";
			var clusterKey3 = 3;
			var countryCodeNonES = "DE";
			_ = TestConnection.ExecuteNonQuery($@"
DECLARE @CountryCode VARCHAR(2) = '{countryCodeES}'
DECLARE @ClusterKey1 INT = {clusterKey1}
DECLARE @ClusterKey2 INT = {clusterKey2}
DECLARE @CompanyPK UNIQUEIDENTIFIER = NEWID()
INSERT INTO dbo.GlbCompany(GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_SystemCreateTimeUtc, GC_SystemCreateUser, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser) VALUES (@CompanyPK, 'D' + @CountryCode, 'AU company', @CountryCode, 'XCD', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
DECLARE @BranchPK UNIQUEIDENTIFIER = NEWID()
INSERT INTO dbo.GlbBranch(GB_PK, GB_GC, GB_Code, GB_SystemCreateTimeUtc, GB_SystemCreateUser, GB_SystemLastEditTimeUtc, GB_SystemLastEditUser) VALUES(@BranchPK, @CompanyPK, 'B' + @CountryCode, GetUtcDate(), '~BP', GetUtcDate(), '~BP')

DECLARE @CountryCodeNonES VARCHAR(2) = '{countryCodeNonES}'
DECLARE @ClusterKey3 INT = {clusterKey3}
DECLARE @CompanyNonESPK UNIQUEIDENTIFIER = NEWID()
INSERT INTO dbo.GlbCompany(GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_SystemCreateTimeUtc, GC_SystemCreateUser, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser) VALUES (@CompanyNonESPK, 'D' + @CountryCodeNonES, 'AU company', @CountryCodeNonES, 'XCD', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
DECLARE @BranchNonESPK UNIQUEIDENTIFIER = NEWID()
INSERT INTO dbo.GlbBranch(GB_PK, GB_GC, GB_Code, GB_SystemCreateTimeUtc, GB_SystemCreateUser, GB_SystemLastEditTimeUtc, GB_SystemLastEditUser) VALUES(@BranchNonESPK, @CompanyNonESPK, 'B' + @CountryCodeNonES, GetUtcDate(), '~BP', GetUtcDate(), '~BP')

DECLARE @Dec1PK UNIQUEIDENTIFIER = NEWID()
INSERT INTO dbo.JobDeclaration(JE_PK, JE_GB, JE_GC, JE_ClusterKey, JE_DataModel, JE_DeclarationReference, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser) VALUES (@Dec1PK, @BranchPK, @CompanyPK, @ClusterKey1, @CountryCode, 'REF1', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
DECLARE @Invoice11PK UNIQUEIDENTIFIER = NEWID()
INSERT INTO dbo.JobComInvoiceHeader(JZ_PK, JZ_JE, JZ_ClusterKey, JZ_DataModel, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser) VALUES (@Invoice11PK, @Dec1PK, @ClusterKey1, @CountryCode, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
DECLARE @InvoiceLine11PK UNIQUEIDENTIFIER = NEWID()
INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_JZ, JI_ClusterKey, JI_LineNo, JI_DataModel, JI_BrandName, JI_Model, JI_AddInfo, JI_SystemCreateTimeUtc, JI_SystemCreateUser, JI_SystemLastEditTimeUtc, JI_SystemLastEditUser) VALUES (@InvoiceLine11PK, @Invoice11PK, @ClusterKey1, 1, @CountryCode, 'Brand1', 'Model1', '', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
INSERT INTO dbo.CusVehicle(CVH_PK, CVH_ParentID, CVH_ParentTableCode, CVH_ClusterKey, CVH_DataModel, CVH_SystemCreateTimeUtc, CVH_SystemCreateUser, CVH_SystemLastEditTimeUtc, CVH_SystemLastEditUser) VALUES (NEWID(), @InvoiceLine11PK, 'JI', @clusterKey1, 'ES', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
DECLARE @InvoiceLine12PK UNIQUEIDENTIFIER = NEWID()
INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_JZ, JI_ClusterKey, JI_LineNo, JI_DataModel, JI_BrandName, JI_AddInfo, JI_SystemCreateTimeUtc, JI_SystemCreateUser, JI_SystemLastEditTimeUtc, JI_SystemLastEditUser) VALUES (@InvoiceLine12PK, @Invoice11PK, @ClusterKey1, 1, @CountryCode, 'Brand2', '', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
INSERT INTO dbo.CusVehicle(CVH_PK, CVH_ParentID, CVH_ParentTableCode, CVH_ClusterKey, CVH_DataModel, CVH_SystemCreateTimeUtc, CVH_SystemCreateUser, CVH_SystemLastEditTimeUtc, CVH_SystemLastEditUser) VALUES (NEWID(), @InvoiceLine12PK, 'JI', @ClusterKey1, 'ES', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

DECLARE @Dec2PK UNIQUEIDENTIFIER = NEWID()
INSERT INTO dbo.JobDeclaration(JE_PK, JE_GB, JE_GC, JE_ClusterKey, JE_DataModel, JE_DeclarationReference, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser) VALUES (@Dec2PK, @BranchPK, @CompanyPK, @ClusterKey2, @CountryCode, 'REF2', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
DECLARE @Invoice21PK UNIQUEIDENTIFIER = NEWID()
INSERT INTO dbo.JobComInvoiceHeader(JZ_PK, JZ_JE, JZ_ClusterKey, JZ_DataModel, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser) VALUES (@Invoice21PK, @Dec2PK, @ClusterKey2, @CountryCode, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
DECLARE @InvoiceLine21PK UNIQUEIDENTIFIER = NEWID()
INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_JZ, JI_ClusterKey, JI_LineNo, JI_DataModel, JI_BrandName, JI_Model, JI_AddInfo, JI_SystemCreateTimeUtc, JI_SystemCreateUser, JI_SystemLastEditTimeUtc, JI_SystemLastEditUser) VALUES (@InvoiceLine21PK, @Invoice21PK, @ClusterKey2, 1, @CountryCode, 'Brand3', 'Model3', '', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
INSERT INTO dbo.CusVehicle(CVH_PK, CVH_ParentID, CVH_ParentTableCode, CVH_ClusterKey, CVH_DataModel, CVH_SystemCreateTimeUtc, CVH_SystemCreateUser, CVH_SystemLastEditTimeUtc, CVH_SystemLastEditUser) VALUES (NEWID(), @InvoiceLine21PK, 'JI', @ClusterKey2, 'ES', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

DECLARE @Dec2NonESPK UNIQUEIDENTIFIER = NEWID()
INSERT INTO dbo.JobDeclaration(JE_PK, JE_GB, JE_GC, JE_ClusterKey, JE_DataModel, JE_DeclarationReference, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser) VALUES (@Dec2NonESPK, @BranchNonESPK, @CompanyNonESPK, @ClusterKey3, @CountryCodeNonES, 'REF2', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
DECLARE @Invoice21NonESPK UNIQUEIDENTIFIER = NEWID()
INSERT INTO dbo.JobComInvoiceHeader(JZ_PK, JZ_JE, JZ_ClusterKey, JZ_DataModel, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser) VALUES (@Invoice21NonESPK, @Dec2NonESPK, @ClusterKey3, @CountryCodeNonES, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
DECLARE @InvoiceLine21NonESPK UNIQUEIDENTIFIER = NEWID()
INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_JZ, JI_ClusterKey, JI_LineNo, JI_DataModel, JI_BrandName, JI_Model, JI_AddInfo, JI_SystemCreateTimeUtc, JI_SystemCreateUser, JI_SystemLastEditTimeUtc, JI_SystemLastEditUser) VALUES (@InvoiceLine21NonESPK, @Invoice21NonESPK, @ClusterKey3, 1, @CountryCodeNonES, 'Brand3', 'Model3', '', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
INSERT INTO dbo.CusVehicle(CVH_PK, CVH_ParentID, CVH_ParentTableCode, CVH_ClusterKey, CVH_DataModel, CVH_SystemCreateTimeUtc, CVH_SystemCreateUser, CVH_SystemLastEditTimeUtc, CVH_SystemLastEditUser) VALUES (NEWID(), @InvoiceLine21NonESPK, 'JI', @ClusterKey3, 'ES', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
");
		}

		protected override void AssertTransformationResults()
		{
			var valuesLineES = new List<(string, string)>();
			var valuesVehicleES = new List<(string, string)>();

			var valuesLineDE = new List<(string, string)>();
			var valuesVehicleDE = new List<(string, string)>();

			var expectedValuesLineDE = new List<(string, string)>();
			var expectedValuesVehicleDE = new List<(string, string)>();

			var sqlTextES = @"SELECT Li.JI_BrandName, Li.JI_Model, ISNULL(Ve.CVH_BrandName, ''), ISNULL(Ve.CVH_ModelName, ''), Ve.CVH_VehicleIdentificationNumber 
FROM dbo.JobComInvoiceLine Li
INNER JOIN dbo.CusVehicle Ve
ON Li.JI_PK = Ve.CVH_ParentID and Ve.CVH_ParentTableCode = 'JI' and Li.JI_DataModel = 'ES' and(Li.JI_BrandName != '' or Li.JI_Model != '')";

			var sqlTextDE = @"SELECT Li.JI_BrandName, Li.JI_Model, ISNULL(Ve.CVH_BrandName, ''), ISNULL(Ve.CVH_ModelName, ''), Ve.CVH_VehicleIdentificationNumber 
FROM dbo.JobComInvoiceLine Li
INNER JOIN dbo.CusVehicle Ve
ON Li.JI_PK = Ve.CVH_ParentID and Ve.CVH_ParentTableCode = 'JI' and Li.JI_DataModel = 'DE' and(Li.JI_BrandName != '' or Li.JI_Model != '')";

			using (var cmdES = TestConnection.Command(sqlTextES))
			using (var readerES = cmdES.ExecuteReader())
			{
				while (readerES.Read())
				{
					valuesLineES.Add((readerES.GetString(0), readerES.GetString(1)));
					valuesVehicleES.Add((readerES.GetString(2), readerES.GetString(3)));
				}
			}
			using (var cmdDE = TestConnection.Command(sqlTextDE))
			using (var readerDE = cmdDE.ExecuteReader())
			{
				while (readerDE.Read())
				{
					valuesLineDE.Add((readerDE.GetString(0), readerDE.GetString(1)));
					valuesVehicleDE.Add((readerDE.GetString(2), readerDE.GetString(3)));
				}
			}
			CombineAssertions(() =>
			{
				AssertEquals("Total Invoice Lines ES", 3, valuesLineES.Count);
				AssertEquals("Total Vehicles ES", 3, valuesVehicleES.Count);
				AssertContainsExactElementsInExactOrder(valuesLineES, valuesVehicleES);

				expectedValuesLineDE.Add(("Brand3", "Model3"));
				expectedValuesVehicleDE.Add((string.Empty, string.Empty));
				AssertEquals("Total Invoice Lines DE", 1, valuesLineDE.Count);
				AssertEquals("Total Vehicles DE", 1, valuesVehicleDE.Count);
				AssertContainsExactElementsInExactOrder(expectedValuesLineDE, valuesLineDE);
				AssertContainsExactElementsInExactOrder(expectedValuesVehicleDE, valuesVehicleDE);
			});
		}
	}
}
