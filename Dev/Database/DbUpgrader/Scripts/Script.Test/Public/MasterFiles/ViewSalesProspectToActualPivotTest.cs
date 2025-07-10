using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.Testing
{
	[TestedType(typeof(ViewSalesProspectToActualPivot))]
	class ViewSalesProspectToActualPivotTest : DbCreateScriptTest
	{
		public void TestGeneralUsage()
		{
			var insertSql = @"
DECLARE @Org1 UNIQUEIDENTIFIER = '795E0F08-A561-4D6F-83BF-F66F677113E0'
DECLARE @Org2 UNIQUEIDENTIFIER = '968D3909-F41B-48B7-B17F-A4EC5A891A0A'


DECLARE @Au UNIQUEIDENTIFIER = (SELECT TOP 1 RN_PK FROM dbo.RefCountry WHERE RN_Code = 'AU')
DECLARE @Us UNIQUEIDENTIFIER = (SELECT TOP 1 RN_PK FROM dbo.RefCountry WHERE RN_Code = 'US')
DECLARE @Nsw UNIQUEIDENTIFIER = (SELECT TOP 1 RW_PK FROM dbo.RefCountryStates WHERE RW_RN_NKCountryCode = 'AU' AND RW_Code = 'NSW')
DECLARE @Vic UNIQUEIDENTIFIER = (SELECT TOP 1 RW_PK FROM dbo.RefCountryStates WHERE RW_RN_NKCountryCode = 'AU' AND RW_Code = 'VIC')
DECLARE @ShpProduct UNIQUEIDENTIFIER = (SELECT TOP 1 MP_PK From dbo.OrgSalesProduct WHERE MP_Code = 'SHP')
DECLARE @TrnProduct UNIQUEIDENTIFIER = (SELECT TOP 1 MP_PK From dbo.OrgSalesProduct WHERE MP_Code = 'TRN')
DECLARE @BrkProduct UNIQUEIDENTIFIER = (SELECT TOP 1 MP_PK From dbo.OrgSalesProduct WHERE MP_Code = 'BRK')

DECLARE @PrsShpAuUs UNIQUEIDENTIFIER = 'B1FAF719-63C4-40B3-8620-9955126ED66F'
DECLARE @PrsShpNswUs UNIQUEIDENTIFIER = 'AD4C78D0-1697-4465-B944-588DA981C72B'
DECLARE @PrsShpVicUs UNIQUEIDENTIFIER = '44248EAD-C9A6-4FC6-9FBD-62FC5CDC456F'
DECLARE @PrsShpUsUs UNIQUEIDENTIFIER = 'FC4D1A3D-DEEC-4CB5-86FF-580CA4939023'
DECLARE @PrsBrkAU UNIQUEIDENTIFIER = '488442A8-ABED-43AF-974A-91E4801DD3DD'

DECLARE @TraShpNswUsOrg1 UNIQUEIDENTIFIER = 'A4E6551E-D757-44E1-B2D0-5B0EBFA2D70E'
DECLARE @TraTrnVicUsOrg1 UNIQUEIDENTIFIER = 'B5E9BFB6-E9B1-4391-BF69-742E3F7FEFB2'
DECLARE @TraShpAuUsOrg1 UNIQUEIDENTIFIER = 'EE9D5151-D4BA-46AB-B441-A19ABFCB22CD'
DECLARE @TraShpNswUsOrg2 UNIQUEIDENTIFIER = 'FCD3CB6C-A01F-4C43-B760-CAAC24D2FF82'

DECLARE @TraBrkAuUsOrg1 UNIQUEIDENTIFIER = '0B206AAF-DFEA-4919-A6ED-D54B368E24AA'
DECLARE @TraBrkUsNswOrg1 UNIQUEIDENTIFIER = 'A9EA0E0E-425D-4E20-82A0-382AA88E5ADF'

INSERT INTO dbo.OrgHeader
	(OH_PK, OH_Code)
VALUES
	(@Org1, 'ORG1'),
	(@Org2, 'ORG2')


INSERT INTO dbo.OrgSales
	(OW_PK, OW_MP_Product, OW_OH_Primary, OW_OH_Supplier, OW_OH_Buyer, OW_OriginID, OW_OriginTableCode, OW_DestinationID, OW_DestinationTableCode, OW_IsTraded, OW_SystemCreateTimeUtc, OW_SystemCreateUser, OW_SystemLastEditTimeUtc, OW_SystemLastEditUser)
VALUES
	(@PrsShpAuUs, @ShpProduct, @Org1, NULL, NULL, @Au, 'RN', @Us, 'RN', 0, GetUtcDate(), 'E', GetUtcDate(), 'E'),
	(@PrsShpNswUs, @ShpProduct, NULL, @Org1, NULL, @Nsw, 'RW', @Us, 'RN', 0, GetUtcDate(), 'E', GetUtcDate(), 'E'),
	(@PrsShpVicUs, @ShpProduct, NULL, NULL, @Org1, @Vic, 'RW', @Us, 'RN', 0, GetUtcDate(), 'E', GetUtcDate(), 'E'),
	(@PrsShpUsUs, @ShpProduct, @Org1, @Org2, NULL, @Us, 'RN', @Us, 'RN', 0, GetUtcDate(), 'E', GetUtcDate(), 'E'),
	(@PrsBrkAU, @BrkProduct, @Org1, NULL, NULL, @Au, 'RN', NULL, '', 0, GetUtcDate(), 'E', GetUtcDate(), 'E'),

	(@TraShpNswUsOrg1, @ShpProduct, @Org1, NULL, NULL, @Nsw, 'RW', @Us, 'RN', 1, GetUtcDate(), 'E', GetUtcDate(), 'E'),
	(@TraShpAuUsOrg1, @ShpProduct, @Org1, NULL, NULL, @Au, 'RN', @Us, 'RN', 1, GetUtcDate(), 'E', GetUtcDate(), 'E'),
	(@TraShpNswUsOrg2, @ShpProduct, @Org2, NULL, NULL, @Nsw, 'RW', @Us, 'RN', 1, GetUtcDate(), 'E', GetUtcDate(), 'E'),

	(@TraTrnVicUsOrg1, @TrnProduct, @Org1, NULL, NULL, @Vic, 'RW', @Us, 'RN', 1, GetUtcDate(), 'E', GetUtcDate(), 'E'),

	(@TraBrkAuUsOrg1, @BrkProduct, @Org1, NULL, NULL, @Au, 'RN', @Us, 'RN', 1, GetUtcDate(), 'E', GetUtcDate(), 'E'),
	(@TraBrkUsNswOrg1, @BrkProduct, @Org1, NULL, NULL, @Us, 'RN', @Nsw, 'RW', 1, GetUtcDate(), 'E', GetUtcDate(), 'E')

";

			TestConnection.ExecuteNonQuery(insertSql);

			var org1Pk = Guid.Parse("795E0F08-A561-4D6F-83BF-F66F677113E0");

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					Guid.Parse("A4E6551E-D757-44E1-B2D0-5B0EBFA2D70E"),
					Guid.Parse("EE9D5151-D4BA-46AB-B441-A19ABFCB22CD")
				},
				GetAllActuals(org1Pk, Guid.Parse("B1FAF719-63C4-40B3-8620-9955126ED66F")));

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					Guid.Parse("A4E6551E-D757-44E1-B2D0-5B0EBFA2D70E")
				},
				GetAllActuals(org1Pk, Guid.Parse("AD4C78D0-1697-4465-B944-588DA981C72B")));

			AssertContainsExactElementsInAnyOrder(
				Array.Empty<Guid>(),
				GetAllActuals(org1Pk, Guid.Parse("44248EAD-C9A6-4FC6-9FBD-62FC5CDC456F")));

			AssertContainsExactElementsInAnyOrder(
				Array.Empty<Guid>(),
				GetAllActuals(org1Pk, Guid.Parse("FC4D1A3D-DEEC-4CB5-86FF-580CA4939023")));

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					Guid.Parse("0B206AAF-DFEA-4919-A6ED-D54B368E24AA"),
					Guid.Parse("A9EA0E0E-425D-4E20-82A0-382AA88E5ADF")
				},
				GetAllActuals(org1Pk, Guid.Parse("488442A8-ABED-43AF-974A-91E4801DD3DD")));
		}

		IEnumerable<Guid> GetAllActuals(Guid viewpointOrgPk, Guid prospectPk)
		{
			var selectSql = @"
SELECT VOW_OW_Actual
FROM dbo.ViewSalesProspectToActualPivot
WHERE
	VOW_OW_Prospect = @VOW_OW_Prospect
	AND @OrgPk IN (VOW_ProspectPrimary, VOW_ProspectBuyer, VOW_ProspectSupplier)
	AND @OrgPk IN (VOW_ActualPrimary, VOW_ActualBuyer, VOW_ActualSupplier)";

			using (var command = TestConnection.Command(selectSql))
			{
				command.AddParameter("@OrgPk", SqlDbType.UniqueIdentifier, viewpointOrgPk);
				command.AddParameter("@VOW_OW_Prospect", SqlDbType.UniqueIdentifier, prospectPk);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						yield return (Guid)reader["VOW_OW_Actual"];
					}
				}
			}
		}

		public void TestCorrectSalesProducts()
		{
			var selectSql = @"
SELECT * FROM dbo.OrgSalesProduct WHERE MP_IsSystemDefined = 1 ORDER BY MP_Code";

			var table = DataUtils.GetDataTableFromQuery(TestConnection, selectSql);

			AssertEquals("Sales Product Count", 5, table.Rows.Count);

			AssertSalesProduct(table, 0, new Guid("9D13E616-F2D3-4C60-BFEF-8862F4DC619C"), "BRK");
			AssertSalesProduct(table, 1, new Guid("4B815685-01C6-4CCF-90A5-565BBA0162AC"), "LGY");
			AssertSalesProduct(table, 2, new Guid("24FAB43B-7A5B-4B3E-A2FE-A6B5CFD2503F"), "SHP");
			AssertSalesProduct(table, 3, new Guid("B416C223-820D-4313-B9AB-6616D343255F"), "TRN");
			AssertSalesProduct(table, 4, new Guid("471C3735-3EE9-4DD2-BCA6-772A9553EDD3"), "WHS");
		}

		void AssertSalesProduct(DataTable table, int number, Guid pk, string code)
		{
			AssertEquals($"row {number} primary key", pk, (Guid)table.Rows[number]["MP_PK"]);
			AssertEquals($"row {number} code", code, (string)table.Rows[number]["MP_Code"]);
		}
	}
}

