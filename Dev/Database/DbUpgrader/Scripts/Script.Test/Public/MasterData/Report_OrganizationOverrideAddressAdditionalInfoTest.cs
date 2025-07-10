using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterData;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.MasterData
{
	[TestedType(typeof(Report_OrganizationOverrideAddressAdditionalInfo))]
	class Report_OrganizationOverrideAddressAdditionalInfoTest : DbCreateScriptTest
	{
		public void TestGetReportData()
		{
			var orgHeaderPK = Guid.NewGuid();
			var orgAddressPK = Guid.NewGuid();
			var shipment1PK = Guid.NewGuid();
			var testDataSql = $@"
INSERT INTO dbo.OrgHeader
(OH_PK, OH_Code, OH_FullName, OH_IsConsignor, OH_OverrideAdditionalAddressInformation)
VALUES
('{orgHeaderPK}', 'QANAIR_WW', 'QANTAS AIRWAYS', 1, 1)

INSERT INTO dbo.OrgAddress
(OA_PK, OA_Address1, OA_State, OA_PostCode, OA_OH, OA_RL_NKRelatedPortCode, OA_RN_NKCountryCode, OA_City, OA_AdditionalAddressInformation)
VALUES
('{orgAddressPK}', 'CNR QANTAS DRIVE & LINK ROAD', 'NSW', '2000', '{orgHeaderPK}', 'AUSYD', 'AU', 'SYDNEY', 'GEORGE STREET BUILDING')

INSERT INTO dbo.JobShipment
(JS_PK, JS_UniqueConsignRef)
VALUES
('{shipment1PK}', 'S00001010')

INSERT INTO dbo.JobDocAddress
(E2_PK, E2_OA_Address, E2_AddressOverride, E2_ParentID, E2_ParentTableCode, E2_AddressType, E2_AdditionalAddressInformation, E2_SystemLastEditTimeUtc, E2_SystemLastEditUser)
VALUES
('{Guid.NewGuid()}', '{orgAddressPK}', 0, '{shipment1PK}', 'JS', 'CRD', 'OTHER BUILDING', '2020-12-17 15:30', 'E')

INSERT INTO dbo.JobDocAddress
(E2_PK, E2_OA_Address, E2_AddressOverride, E2_ParentID, E2_ParentTableCode, E2_AddressType, E2_AdditionalAddressInformation, E2_SystemLastEditTimeUtc, E2_SystemLastEditUser)
VALUES
('{Guid.NewGuid()}', '{orgAddressPK}', 0, '{shipment1PK}', 'JS', 'CRG', '', '2020-12-17 15:30', 'E')

INSERT INTO dbo.JobDocAddress
(E2_PK, E2_OA_Address, E2_AddressOverride, E2_ValidationStatus, E2_ParentID, E2_ParentTableCode, E2_AddressType, E2_AdditionalAddressInformation, E2_SystemLastEditTimeUtc, E2_SystemLastEditUser)
VALUES
('{Guid.NewGuid()}', '{orgAddressPK}', 1, 'NYV', '{shipment1PK}', 'JS', 'ABC', 'CITY 17', '2020-12-17 15:30', 'E')

INSERT INTO dbo.JobDocAddress
(E2_PK, E2_OA_Address, E2_AddressOverride, E2_ParentID, E2_ParentTableCode, E2_AddressType, E2_AdditionalAddressInformation, E2_SystemLastEditTimeUtc, E2_SystemLastEditUser)
VALUES
('{Guid.NewGuid()}', '{orgAddressPK}', 0, '{shipment1PK}', 'JS', 'EFG', 'New Vegas', '2020-12-16', 'E')

INSERT INTO dbo.JobDocAddress
(E2_PK, E2_OA_Address, E2_AddressOverride, E2_ParentID, E2_ParentTableCode, E2_AddressType, E2_AdditionalAddressInformation, E2_SystemLastEditTimeUtc, E2_SystemLastEditUser)
VALUES
('{Guid.NewGuid()}', '{orgAddressPK}', 0, '{shipment1PK}', 'JS', 'HIJ', 'Inner Sanctum', '2020-12-17 15:31', 'E')";

			TestConnection.ExecuteNonQuery(testDataSql);

			var reportSql = "select * from Report_OrganizationOverrideAddressAdditionalInfo('2020-12-17 15:30', '2020-12-17 15:30')";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, reportSql);

			CombineAssertions(() =>
			{
				AssertEquals("Result should return 1 row", 1, result.Rows.Count);
				AssertEquals("QANAIR_WW", result.Rows[0]["OH_Code"]);
				AssertEquals("QANTAS AIRWAYS", result.Rows[0]["OH_FullName"]);
				AssertEquals("OTHER BUILDING", result.Rows[0]["E2_AdditionalAddressInformation"]);
				AssertEquals(new DateTime(2020, 12, 17, 15, 30, 0), result.Rows[0]["E2_SystemLastEditTimeUtc"]);
				AssertEquals("E", result.Rows[0]["E2_SystemLastEditUser"]);
			});

			reportSql = "select * from Report_OrganizationOverrideAddressAdditionalInfo('2020-12-17 00:00', '2020-12-17 23:59')";
			result = DataUtils.GetDataTableFromQuery(TestConnection, reportSql);
			AssertEquals("Result should return 2 rows", 2, result.Rows.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "OTHER BUILDING", "Inner Sanctum" }, new[] { result.Rows[0]["E2_AdditionalAddressInformation"], result.Rows[1]["E2_AdditionalAddressInformation"] });

			reportSql = "select * from Report_OrganizationOverrideAddressAdditionalInfo('2020-12-16 00:00', '2020-12-17 23:59')";
			result = DataUtils.GetDataTableFromQuery(TestConnection, reportSql);
			AssertEquals("Result should return 3 rows", 3, result.Rows.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "OTHER BUILDING", "Inner Sanctum", "New Vegas" }, new[] { result.Rows[0]["E2_AdditionalAddressInformation"], result.Rows[1]["E2_AdditionalAddressInformation"], result.Rows[2]["E2_AdditionalAddressInformation"] });
		}
	}
}
