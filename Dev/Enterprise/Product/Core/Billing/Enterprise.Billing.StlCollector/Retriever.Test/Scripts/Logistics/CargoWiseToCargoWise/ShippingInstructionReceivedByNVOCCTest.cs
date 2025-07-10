using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(ShippingInstructionReceivedByNVOCC))]
	sealed class ShippingInstructionReceivedByNVOCCTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2022, 11);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals(2, transactions.Count());

			AssertNull("STU event is Amendent", transactions.FirstOrDefault(x => x.Reference4.StartsWith("RES002")));
			AssertNull("STU event is before DateTime range", transactions.FirstOrDefault(x => x.Reference4.StartsWith("RES003")));
			AssertNull("STU event is after DateTime range", transactions.FirstOrDefault(x => x.Reference4.StartsWith("RES004")));
			AssertNull("This is XX event", transactions.FirstOrDefault(x => x.Reference4.StartsWith("RES005")));

			var row0 = transactions.Single(x => x.Reference4.StartsWith("RES001"));
			var row1 = transactions.Single(x => x.Reference4.StartsWith("RES006"));

			CombineAssertions(() =>
			{
				AssertEquals("[Row-0] CompanyCode", "DAU", row0.GetCompanyCode());
				AssertEquals("[Row-0] BranchCode", "GB2", row0.GetBranchCode());
				AssertEquals("[Row-0] TransactionDateUtc", new DateTime(2022, 11, 2, 12, 01, 00), row0.ServiceOccuredUTC);
				AssertEquals("[Row-0] ItemCount", 1, row0.BillableCount);
				AssertEquals("[Row-0] TransactionReference01", "S00001001", row0.Reference1);
				AssertEquals("[Row-0] TransactionReference02", "BRC001", row0.Reference2);
				AssertEquals("[Row-0] TransactionReference03", "CompanyName1", row0.Reference3);
				AssertEquals("[Row-0] TransactionReference04", "RES001 - Original", row0.Reference4);

				AssertEquals("[Row-1] CompanyCode", "SZG", row1.GetCompanyCode());
				AssertEquals("[Row-1] BranchCode", "GB3", row1.GetBranchCode());
				AssertEquals("[Row-1] TransactionDateUtc", new DateTime(2022, 11, 22, 12, 02, 00), row1.ServiceOccuredUTC);
				AssertEquals("[Row-1] ItemCount", 1, row1.BillableCount);
				AssertEquals("[Row-1] TransactionReference01", "S00001006", row1.Reference1);
				AssertEquals("[Row-1] TransactionReference02", "BRC006", row1.Reference2);
				AssertNull("[Row-1] TransactionReference03", row1.Reference3);
				AssertEquals("[Row-1] TransactionReference04", "RES006 - Original", row1.Reference4);
			});
		}

		protected override void PrepareTestData()
		{
			var sqlText = $@"
DECLARE @JsPK01 UNIQUEIDENTIFIER = newid();
DECLARE @JsPK02 UNIQUEIDENTIFIER = newid();
DECLARE @JsPK03 UNIQUEIDENTIFIER = newid();
DECLARE @JsPK04 UNIQUEIDENTIFIER = newid();
DECLARE @JsPK05 UNIQUEIDENTIFIER = newid();
DECLARE @JsPK06 UNIQUEIDENTIFIER = newid();

DECLARE @jobDocAddr1PK UNIQUEIDENTIFIER = newid();
DECLARE @jobDocAddr2PK UNIQUEIDENTIFIER = newid();
DECLARE @jobDocAddr3PK UNIQUEIDENTIFIER = newid();
DECLARE @jobDocAddr4PK UNIQUEIDENTIFIER = newid();

DECLARE @GcPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @GcPk02 UNIQUEIDENTIFIER = NEWID();
DECLARE @GcPk03 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk00 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk02 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk03 UNIQUEIDENTIFIER = NEWID();

INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode) VALUES (@GcPk01, 'DAU', 'AU company', 'AUD', 'AU');
INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode) VALUES (@GcPk02, 'SHA', 'CN company1', 'CNY', 'CN');
INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode) VALUES (@GcPk03, 'SZG', 'CN company2', 'CNY', 'CN');
INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy, GB_RL_NKHomePort) VALUES (@GbPk00, 'GB0', @GcPk02, NULL, 'GBLON');
INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy, GB_RL_NKHomePort) VALUES (@GbPk01, 'GB1', @GcPk01, NULL, 'AUSYD');
INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy, GB_RL_NKHomePort) VALUES (@GbPk02, 'GB2', @GcPk01, NULL, 'BGSOF');
INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy, GB_RL_NKHomePort) VALUES (@GbPk03, 'GB3', @GcPk03, NULL, 'PYASU');
INSERT dbo.GlbBranchExtraPorts(GY_PK, GY_RL_NKAdditionalBranchRelatedPort, GY_GB) VALUES (NEWID(), 'AUSYD', @GbPk01);

INSERT dbo.StmALog (SL_PK, SL_Table, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_SE_NKEvent, SL_Reference, SL_GB_NKBranch, SL_IsCancelled) VALUES
	(newid(), 'JobShipment', getdate(), '2022-11-02 12:01:00', @JsPK01, 'STU', 'Original|RES=Electronic RES001|NEW=ESI|TYP=Shipment Status', 'GB1', 'N'), -- 01
	(newid(), 'JobShipment', getdate(), '2022-11-12 12:02:00', @JsPK02, 'STU', 'Amendent|RES=RES002|NEW=ESI|TYP=Shipment Status', 'GB3', 'N'),  -- Amendent
	(newid(), 'JobShipment', getdate(), '2022-10-01 12:03:00', @JsPK03, 'STU', 'Original|RES=RES002|NEW=ESI|TYP=Shipment Status', 'GB0', 'N'), -- Before date range
	(newid(), 'JobShipment', getdate(), '2022-12-01 12:04:00', @JsPK04, 'STU', 'Original|RES=RES003|NEW=ESI|TYP=Shipment Status', 'GB0', 'N'), -- After date range
	(newid(), 'JobShipment', getdate(), '2022-11-01 12:05:00', @JsPK05, 'XXX', 'Original|RES=RES004|NEW=ESI|TYP=Shipment Status', 'GB0', 'N'), -- SL_SE_NKEvent is not STU
	(newid(), 'JobShipment', getdate(), '2022-11-01 12:05:00', @JsPK05, 'STU', 'Original|RES=RES004|NEW=AAA|TYP=Shipment Status', 'GB0', 'N'), -- NEW is not ESI
	(newid(), 'JobShipment', getdate(), '2022-11-22 12:02:00', @JsPK06, 'STU', 'Original|RES=RES006|NEW=ESI|TYP=Shipment Status', 'GB3', 'N')  -- 06

INSERT dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_IsBooking, JS_IsShipping, JS_BookingReference, JS_RL_NKOrigin) VALUES
	(@JsPk01, 'S00001001', 0, 1, 'BRC001', 'BGSOF'),
	(@JsPk02, 'S00001002', 1, 0, 'BRC002', 'PGPOM'),
	(@JsPk03, 'S00001003', 0, 0, 'BRC003', 'NENIM'),
	(@JsPk04, 'S00001004', 1, 1, 'BRC004', 'PAPTY'),
	(@JsPk05, 'S00001005', 1, 1, 'BRC005', 'AUSYD'),
	(@JsPk06, 'S00001006', 1, 0, 'BRC006', 'PGPOM')

INSERT dbo.jobDocAddress(E2_PK, E2_ParentID, E2_ParentTableCode, E2_AddressType, E2_City, E2_State, E2_RN_NKCountryCode, E2_GeoLocation, E2_OA_Address,E2_AddressOverride,E2_ValidationStatus, E2_CompanyName, E2_SystemCreateTimeUtc,E2_SystemCreateUser,E2_SystemLastEditTimeUtc,E2_SystemLastEditUser) VALUES
	(@jobDocAddr1PK, @JsPK01, 'JS', 'LTS','ALEXANDRIA','NSW','AU',geography::Point(47.616, -122.360, 4326), NULL, 0, 'NRQ', '', GETUTCDATE(),'TST',GETUTCDATE(),'TST'),
	(@jobDocAddr2PK, @JsPK01, 'JS', 'BKD','DUTTON PARK','QLD','AU',geography::Point(47.626, -122.360, 4326), NULL, 1, 'NYV', 'CompanyName1', GETUTCDATE(),'TST',GETUTCDATE(),'TST'),
	(@jobDocAddr3PK, @JsPK02, 'JS', 'BKD','','','',geography::Point(47.636, -122.360, 4326), NULL, 0, 'NRQ','', GETUTCDATE(),'TST',GETUTCDATE(),'TST'),
	(@jobDocAddr4PK, @JsPK06, 'JS', 'BKD','','','',geography::Point(47.636, -122.360, 4326), NULL, 0, 'NRQ','', GETUTCDATE(),'TST',GETUTCDATE(),'TST')
";

			TestConnection.ExecuteNonQuery(sqlText);
		}
	}
}
