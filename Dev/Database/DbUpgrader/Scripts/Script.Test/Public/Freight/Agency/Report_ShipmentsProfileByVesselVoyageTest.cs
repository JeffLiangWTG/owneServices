using System;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Agency;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Agency.Testing
{
	[TestedType(typeof(Report_ShipmentsProfileByVesselVoyage))]
	internal class Report_ShipmentsProfileByVesselVoyageTest : DbCreateScriptTest
	{
		public void TestNoDuplicatedLocalClientsInReport_ShipmentsProfileByVesselVoyage()
		{
			#region Prepare Test Data

			var sqlCmd = @"

DECLARE @CurrentCompany AS UNIQUEIDENTIFIER = 'D381CB3B-281E-4BA4-B7B8-A0B045FDA68D'
INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES (@CurrentCompany, 'DAN', 'AU company', 'AU', 'AUD')

DECLARE @BranchPk AS UNIQUEIDENTIFIER = 'F865DC82-7DE7-451F-A6E0-971C92474CC4'
INSERT INTO dbo.GlbBranch (GB_PK, GB_GC) VALUES (@BranchPk, @CurrentCompany)

DECLARE @DepartmentPk AS UNIQUEIDENTIFIER = 'C4C7E683-DBE2-4358-9FB8-92183112B8C0'
INSERT INTO dbo.GlbDepartment (GE_PK) VALUES (@DepartmentPk)

DECLARE @OrgHeaderPK1 AS UNIQUEIDENTIFIER = 'F032F76A-8778-422D-A689-8F2292759413'
INSERT INTO dbo.OrgHeader(OH_PK, OH_Code) VALUES (@OrgHeaderPK1, 'BENMEATEE')

DECLARE @OrgHeaderPK2 AS UNIQUEIDENTIFIER = 'ee38868f-faba-3a86-44f8-2c5cb546914e'
INSERT INTO dbo.OrgHeader(OH_PK, OH_Code) VALUES (@OrgHeaderPK2, 'BENMEATER')

DECLARE @ShipmentPk1 AS UNIQUEIDENTIFIER = '9A1E098C-037A-4A62-989E-72E14F9C2618'
INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_IsShipping, JS_IsCancelled, JS_RL_NKOrigin, JS_RL_NKDestination, 
	JS_ShipmentStatus, JS_PackingMode, JS_ActualWeight, JS_UnitOfWeight, JS_ActualVolume, JS_UnitOfVolume, JS_OH_DeliveryAgent)
	VALUES (@ShipmentPk1, 'S00001999', 1, 0, 'AUSYD', 'NZAKL', 'CNF', 'ALL', 10, 'KG', 10, 'KG', @OrgHeaderPK1)

DECLARE @ShipmentPk2 AS UNIQUEIDENTIFIER = '2bb8d0f5-6351-b8bd-4b60-a64b47b3fa07'
INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_IsShipping, JS_IsCancelled, JS_RL_NKOrigin, JS_RL_NKDestination, 
	JS_ShipmentStatus, JS_PackingMode, JS_ActualWeight, JS_UnitOfWeight, JS_ActualVolume, JS_UnitOfVolume, JS_OH_DeliveryAgent)
	VALUES (@ShipmentPk2, 'S00001088', 1, 0, 'AUSYD', 'NZAKL', 'CNF', 'ALL', 10, 'KG', 10, 'KG', @OrgHeaderPK2)

DECLARE @OrgAddressPK1 AS UNIQUEIDENTIFIER = '0692A39A-C9F0-4D13-AF93-AF079D0F820F'
INSERT INTO dbo.OrgAddress(OA_PK, OA_OH, OA_Address1) VALUES (@OrgAddressPK1, @OrgHeaderPK1, 'Consignor')

DECLARE @OrgAddressPK2 AS UNIQUEIDENTIFIER = '13c5634b-ef92-0792-4d6c-d1daa3c991b5'
INSERT INTO dbo.OrgAddress(OA_PK, OA_OH, OA_Address1) VALUES (@OrgAddressPK2, @OrgHeaderPK2, 'Consignee')

DECLARE @JobDocAddressPK1 AS UNIQUEIDENTIFIER = 'EBB2172C-944F-4944-9882-8E53B882BD69'
INSERT INTO dbo.JobDocAddress(E2_PK, E2_ParentID, E2_ParentTableCode, E2_AddressType, E2_OA_Address, E2_AddressSequence)
	VALUES (@JobDocAddressPK1, @ShipmentPk1, 'JS', 'BKD', @OrgAddressPK1, 0)

DECLARE @JobDocAddressPK2 AS UNIQUEIDENTIFIER = '95C80B5B-852C-4EA4-A3ED-394C84F92FF6'
INSERT INTO dbo.JobDocAddress(E2_PK, E2_ParentID, E2_ParentTableCode, E2_AddressType, E2_OA_Address, E2_AddressSequence)
	VALUES (@JobDocAddressPK2, @ShipmentPk2, 'JS', 'BKD', @OrgAddressPK2, 0)

DECLARE @ConsolPK AS UNIQUEIDENTIFIER = '9A1E098C-037A-4A62-989E-72E14F9C2618'
INSERT INTO dbo.JobConsol (JK_PK, JK_UniqueConsignRef, JK_RL_NKLoadPort) VALUES(@ConsolPK, 'C00123456', 'AUSYD')

DECLARE @JobConShipLinkPK AS UNIQUEIDENTIFIER = '781A7446-95DC-4393-98E9-D3DFC1392183'
INSERT INTO dbo.JobConShipLink(JN_PK, JN_JK, JN_JS) VALUES(@JobConShipLinkPK, @ConsolPK, @ShipmentPk1)

DECLARE @VoyagePK1 AS UNIQUEIDENTIFIER = 'E9D1CF6C-E015-4EEB-9B61-8225B8527123'
INSERT INTO dbo.JobVoyage(JV_PK) VALUES (@VoyagePK1)

DECLARE @VoyageOriginPK1 AS UNIQUEIDENTIFIER = 'AC7351D1-EF43-4F78-ACF7-A3C5ECECC2B5'
INSERT INTO dbo.JobVoyOrigin(JA_PK, JA_RL_NKPortOfLoading, JA_JV, JA_E_DEP) VALUES(@VoyageOriginPK1, 'AUSYD', @VoyagePK1, '06/08/2020 23:59:29')

DECLARE @VoyageDestinationPK1 AS UNIQUEIDENTIFIER = 'F12BB249-CFDB-42C6-9197-29A93E176107'
INSERT INTO dbo.JobVoyDestination(JB_PK, JB_RL_NKPortOfDischarge, JB_JV, JB_E_ARV) VALUES(@VoyageDestinationPK1, 'NZAKL', @VoyagePK1, '06/10/2020 23:59:29')

DECLARE @SailingPK1 AS UNIQUEIDENTIFIER = '26518219-E910-4A25-BCCF-8D9750A7BDB9'
INSERT INTO dbo.JobSailing(JX_PK, JX_JA, JX_JB) VALUES (@SailingPK1, @VoyageOriginPK1, @VoyageDestinationPK1)
UPDATE dbo.JobShipment
SET
	JS_JX = @SailingPK1,
	JS_SystemLastEditTimeUtc = GETUTCDATE(),
	JS_SystemLastEditUser = '~BP'
WHERE
	JS_PK = @ShipmentPk1

DECLARE @TransportPK1 AS UNIQUEIDENTIFIER = 'AF857762-28DA-401B-BBB6-408B2162DB18'
INSERT INTO dbo.JobConsolTransport (JW_PK, JW_JX, JW_ParentGUID, JW_ParentType) VALUES (@TransportPK1, @SailingPK1, @ConsolPK, 'CON')

DECLARE @JobHeaderPK1 AS UNIQUEIDENTIFIER = '3AB47702-EDF6-4E19-89AF-BF42A2753380'
INSERT INTO dbo.JobHeader (JH_PK, JH_JobNum, JH_GB, JH_GE, JH_GC, JH_OA_LocalChargesAddr, JH_ParentTableCode, JH_ParentID, JH_GS_NKRepSales, JH_Status)
	VALUES (@JobHeaderPK1, 'S00001005',  @BranchPk, @DepartmentPk, @CurrentCompany, @OrgAddressPK1, 'JS', @ShipmentPk1, 'ADL', 'CLS')

DECLARE @TradeLane1 AS UNIQUEIDENTIFIER = 'A433C594-8C39-46D2-B601-30877A9F69FB'
INSERT INTO dbo.JobTradeLane(EJ_PK, EJ_OH_RelatedOrg) VALUES (@TradeLane1, @OrgHeaderPK1)

DECLARE @JobTradeLaneVoyagePK1 AS UNIQUEIDENTIFIER = '598C0C9D-2979-401F-845A-4A11C58D6A6F'
INSERT INTO dbo.JobTradeLaneVoyage(NB_PK, NB_JV, NB_EJ, NB_OH) VALUES (@JobTradeLaneVoyagePK1, @VoyagePK1, @TradeLane1, @OrgHeaderPK1)

DECLARE @CommisionableChargeCodePk AS UNIQUEIDENTIFIER = '6717DBA0-BDF4-41A2-BA3A-A44246D71955'
DECLARE @UncommisionableChargeCodePk AS UNIQUEIDENTIFIER = 'E01CB27E-D33A-4CC7-B8BB-82D8A52434D8'
INSERT INTO dbo.AccChargeCode (AC_PK, AC_Code, AC_ChargeGroup, AC_IsCommissionable) VALUES (@CommisionableChargeCodePk, 'ISCOM', 'FRT', 1)
INSERT INTO dbo.AccChargeCode (AC_PK, AC_Code, AC_ChargeGroup, AC_IsCommissionable) VALUES (@UncommisionableChargeCodePk, 'NOTCOM', 'FRT', 0)
";

			TestConnection.ExecuteNonQuery(sqlCmd);

			#endregion

			var queryToCheck = " select * from dbo.ctfn_JobShipmentOrg('BKD') ";

			using (var command = TestConnection.Command(queryToCheck))
			{
				using (var result = DataUtils.GetDataTableFromCommand(command))
				{
					AssertEquals("The result should be 2 rows", 2, result.Rows.Count);

					AssertEquals("JS_PK", "9A1E098C-037A-4A62-989E-72E14F9C2618".ToUpperInvariant(),
						result.Rows[0]["JS_PK"].ToString().ToUpperInvariant());
					AssertEquals("OH_PK", "F032F76A-8778-422D-A689-8F2292759413".ToUpperInvariant(),
						result.Rows[0]["OH_PK"].ToString().ToUpperInvariant());

					AssertEquals("JS_PK", "2bb8d0f5-6351-b8bd-4b60-a64b47b3fa07".ToUpperInvariant(),
						result.Rows[1]["JS_PK"].ToString().ToUpperInvariant());
					AssertEquals("OH_PK", "EE38868F-FABA-3A86-44F8-2C5CB546914E".ToUpperInvariant(),
						result.Rows[1]["OH_PK"].ToString().ToUpperInvariant());
				}
			}

			var query = @"
SELECT * FROM Report_ShipmentsProfileByVesselVoyage
(
	@VesselVoyage, 0, @Principal, 'AUSYD', 'NZAKL', 'AUSYD', 'NZAKL',
	@ClientBookingParty, 0, 'ALL', 'ALL', @CurrentCompany, 'ACT', '20F,20H,20R,40F,40H,40R',
	'Load Port', '06/06/2020 23:59:29', '06/08/2020 23:59:29', '06/09/2020 23:59:29', '06/10/2020 23:59:29',
	@TradeLane, @ChargeCode, 0, ''
)";
			using (var command = TestConnection.Command(query))
			{
				command.AddTableValuedParameter("@ClientBookingParty", "dbo.TVP_uniqueidentifier",
					new Guid[] { new Guid("F032F76A-8778-422D-A689-8F2292759413") });
				command.AddTableValuedParameter("@VesselVoyage", "dbo.TVP_uniqueidentifier",
					new Guid[] { new Guid("E9D1CF6C-E015-4EEB-9B61-8225B8527123") });
				command.AddTableValuedParameter("@ChargeCode", "dbo.TVP_uniqueidentifier",
					new Guid[] { new Guid("6717DBA0-BDF4-41A2-BA3A-A44246D71955") });

				command.AddParameter("@Principal", SqlDbType.UniqueIdentifier, Guid.Parse("F032F76A-8778-422D-A689-8F2292759413"));
				command.AddParameter("@CurrentCompany", SqlDbType.UniqueIdentifier, Guid.Parse("D381CB3B-281E-4BA4-B7B8-A0B045FDA68D"));
				command.AddParameter("@TradeLane", SqlDbType.UniqueIdentifier, Guid.Parse("A433C594-8C39-46D2-B601-30877A9F69FB"));

				using (var result = DataUtils.GetDataTableFromCommand(command))
				{
					AssertEquals("The result should be 1 rows", 1, result.Rows.Count);

					AssertEquals("ShipmentPK", "9A1E098C-037A-4A62-989E-72E14F9C2618".ToUpperInvariant(), result.Rows[0]["ShipmentPK"].ToString().ToUpperInvariant());
					AssertEquals("ShipmentNumber", "S00001999", result.Rows[0]["ShipmentNumber"]);
				}
			}
		}

		public void TestQueryToCheckInEDW()
		{
			#region Prepare Test Data

			var sqlCmd = $@"
DECLARE @OrgHeaderPK1 AS UNIQUEIDENTIFIER = 'F032F76A-8778-422D-A689-8F2292759413'
INSERT INTO {Db.EdwDatabaseName}.Organization.BAS__Organization(OrganizationID, OrganizationKey, Code) VALUES (@OrgHeaderPK1, 1, 'BENMEATEE')

DECLARE @OrgHeaderPK2 AS UNIQUEIDENTIFIER = 'ee38868f-faba-3a86-44f8-2c5cb546914e'
INSERT INTO {Db.EdwDatabaseName}.Organization.BAS__Organization(OrganizationID, OrganizationKey, Code) VALUES (@OrgHeaderPK2, 2, 'BENMEATER')

DECLARE @OrgAddressPK1 AS UNIQUEIDENTIFIER = '0692A39A-C9F0-4D13-AF93-AF079D0F820F'
INSERT INTO {Db.EdwDatabaseName}.Organization.BAS__OrganizationAddress(OrganizationAddressID, OrganizationAddressKey, OrganizationKey, Address1) VALUES (@OrgAddressPK1, 11, 1, 'Consignor')

DECLARE @OrgAddressPK2 AS UNIQUEIDENTIFIER = '13c5634b-ef92-0792-4d6c-d1daa3c991b5'
INSERT INTO {Db.EdwDatabaseName}.Organization.BAS__OrganizationAddress(OrganizationAddressID, OrganizationAddressKey, OrganizationKey, Address1) VALUES (@OrgAddressPK2, 22, 2, 'Consignee')

DECLARE @JobDocAddressPK1 AS UNIQUEIDENTIFIER = 'EBB2172C-944F-4944-9882-8E53B882BD69'
INSERT INTO {Db.EdwDatabaseName}.InternationalLogistics.BAS__DocAddress(DocAddressID, DocAddressKey, ParentID, ParentTableCode, AddressType, OrganizationAddressKey, AddressSequence, AddressOverride)
	VALUES (@JobDocAddressPK1, 111, '2C69258B-C804-487A-8432-2EF98C4F4DE9', 'JS', 'BKD', 11, 0, 0)

DECLARE @JobDocAddressPK2 AS UNIQUEIDENTIFIER = '95C80B5B-852C-4EA4-A3ED-394C84F92FF6'
INSERT INTO {Db.EdwDatabaseName}.InternationalLogistics.BAS__DocAddress(DocAddressID, DocAddressKey, ParentID, ParentTableCode, AddressType, OrganizationAddressKey, AddressSequence, AddressOverride)
	VALUES (@JobDocAddressPK2, 222, '623AD091-E2CE-433C-BFF6-4277B07F1305', 'JS', 'BKD', 22, 0, 0)
";

			Db.Connection.ExecuteNonQuery(sqlCmd);
			#endregion

			var queryToCheck = string.Format(CultureInfo.InvariantCulture, $@" select * from {Db.EdwDatabaseName}.dbo.ctfn_JobShipmentOrg('BKD') ");
			using (var command = Db.Connection.Command(queryToCheck))
			{
				using (var result = DataUtils.GetDataTableFromCommand(command))
				{
					AssertEquals("The result should be 2 rows", 2, result.Rows.Count);

					AssertEquals("JS_PK", "2C69258B-C804-487A-8432-2EF98C4F4DE9".ToUpperInvariant(),
						result.Rows[0]["JS_PK"].ToString().ToUpperInvariant());
					AssertEquals("OH_PK", "F032F76A-8778-422D-A689-8F2292759413".ToUpperInvariant(),
						result.Rows[0]["OH_PK"].ToString().ToUpperInvariant());

					AssertEquals("JS_PK", "623AD091-E2CE-433C-BFF6-4277B07F1305".ToUpperInvariant(),
						result.Rows[1]["JS_PK"].ToString().ToUpperInvariant());
					AssertEquals("OH_PK", "EE38868F-FABA-3A86-44F8-2C5CB546914E".ToUpperInvariant(),
						result.Rows[1]["OH_PK"].ToString().ToUpperInvariant());
				}
			}
		}
	}
}

