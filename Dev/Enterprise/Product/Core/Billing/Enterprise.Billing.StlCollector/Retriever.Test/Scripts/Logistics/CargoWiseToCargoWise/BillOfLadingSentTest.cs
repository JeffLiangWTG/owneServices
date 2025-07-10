using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(BillOfLadingSent))]
	sealed class BillOfLadingSentTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2023, 04);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals(6, transactions.Count());

			AssertNull("ISN event is before DateTime range", transactions.FirstOrDefault(x => x.Reference1 == "JS00000000002"));
			AssertNull("ISN event is after DateTime range", transactions.FirstOrDefault(x => x.Reference1 == "JS00000000003"));
			AssertNull("This is XXX event", transactions.FirstOrDefault(x => x.Reference1 == "JS00000000004"));

			var row0 = transactions.Single(x => x.Reference1.StartsWith("JS00000000001") && x.Reference4.EndsWith("00"));
			var row1 = transactions.Single(x => x.Reference1.StartsWith("JS00000000001") && x.Reference4.EndsWith("01"));
			var row2 = transactions.Single(x => x.Reference1.StartsWith("JS00000000005"));
			var row3 = transactions.Single(x => x.Reference1.StartsWith("JS00000000006"));
			var row4 = transactions.Single(x => x.Reference1.StartsWith("JS00000000007"));
			var row5 = transactions.Single(x => x.Reference1.StartsWith("JS00000000008"));

			CombineAssertions(() =>
			{
				AssertEquals("[Row-0] CompanyCode", "DAU", row0.GetCompanyCode());
				AssertEquals("[Row-0] BranchCode", "GB0", row0.GetBranchCode());
				AssertEquals("[Row-0] TransactionDateUtc", new DateTime(2023, 04, 02, 12, 01, 00), row0.ServiceOccuredUTC);
				AssertEquals("[Row-0] ItemCount", 1, row0.BillableCount);
				AssertEquals("[Row-0] TransactionReference01", "JS00000000001", row0.Reference1);
				AssertEquals("[Row-0] TransactionReference02", "BookingReference01", row0.Reference2);
				AssertEquals("[Row-0] TransactionReference03", "OA_CompanyName01", row0.Reference3);
				AssertEquals("[Row-0] TransactionReference04", "00", row0.Reference4);

				AssertEquals("[Row-1] CompanyCode", "DAU", row1.GetCompanyCode());
				AssertEquals("[Row-1] BranchCode", "GB0", row1.GetBranchCode());
				AssertEquals("[Row-1] TransactionDateUtc", new DateTime(2023, 04, 12, 12, 02, 00), row1.ServiceOccuredUTC);
				AssertEquals("[Row-1] ItemCount", 1, row1.BillableCount);
				AssertEquals("[Row-1] TransactionReference01", "JS00000000001", row1.Reference1);
				AssertEquals("[Row-1] TransactionReference02", "BookingReference01", row1.Reference2);
				AssertEquals("[Row-1] TransactionReference03", "OA_CompanyName01", row1.Reference3);
				AssertEquals("[Row-1] TransactionReference04", "01", row1.Reference4);

				AssertEquals("[Row-2] CompanyCode", "DK1", row2.GetCompanyCode());
				AssertEquals("[Row-2] BranchCode", "GB1", row2.GetBranchCode());
				AssertEquals("[Row-2] TransactionDateUtc", new DateTime(2023, 04, 12, 12, 02, 00), row2.ServiceOccuredUTC);
				AssertEquals("[Row-2] ItemCount", 1, row2.BillableCount);
				AssertEquals("[Row-2] TransactionReference01", "JS00000000005", row2.Reference1);
				AssertEquals("[Row-2] TransactionReference02", "BookingReference05", row2.Reference2);
				AssertEquals("[Row-2] TransactionReference03", "i1i1i1i", row2.Reference3);
				AssertEquals("[Row-2] TransactionReference04", "05", row2.Reference4);

				AssertEquals("[Row-3] CompanyCode", "DK1", row3.GetCompanyCode());
				AssertEquals("[Row-3] BranchCode", "GB3", row3.GetBranchCode());
				AssertEquals("[Row-3] TransactionDateUtc", new DateTime(2023, 04, 12, 12, 02, 00), row3.ServiceOccuredUTC);
				AssertEquals("[Row-3] ItemCount", 1, row3.BillableCount);
				AssertEquals("[Row-3] TransactionReference01", "JS00000000006", row3.Reference1);
				AssertEquals("[Row-3] TransactionReference02", "BookingReference06", row3.Reference2);
				AssertEquals("[Row-3] TransactionReference03", "i2i2i2i2", row3.Reference3);
				AssertEquals("[Row-3] TransactionReference04", "06", row3.Reference4);

				AssertEquals("[Row-4] CompanyCode", "DK1", row4.GetCompanyCode());
				AssertEquals("[Row-4] BranchCode", "GB4", row4.GetBranchCode());
				AssertEquals("[Row-4] TransactionDateUtc", new DateTime(2023, 04, 12, 12, 02, 00), row4.ServiceOccuredUTC);
				AssertEquals("[Row-4] ItemCount", 1, row4.BillableCount);
				AssertEquals("[Row-4] TransactionReference01", "JS00000000007", row4.Reference1);
				AssertEquals("[Row-4] TransactionReference02", "BookingReference07", row4.Reference2);
				AssertEquals("[Row-4] TransactionReference03", null, row4.Reference3);
				AssertEquals("[Row-4] TransactionReference04", "07", row4.Reference4);

				AssertEquals("[Row-5] CompanyCode", "DK1", row5.GetCompanyCode());
				AssertEquals("[Row-5] BranchCode", "GB2", row5.GetBranchCode());
				AssertEquals("[Row-5] TransactionDateUtc", new DateTime(2023, 04, 12, 12, 02, 00), row5.ServiceOccuredUTC);
				AssertEquals("[Row-5] ItemCount", 1, row5.BillableCount);
				AssertEquals("[Row-5] TransactionReference01", "JS00000000008", row5.Reference1);
				AssertEquals("[Row-5] TransactionReference02", "BookingReference08", row5.Reference2);
				AssertEquals("[Row-5] TransactionReference03", "OH_FullName05", row5.Reference3);
				AssertEquals("[Row-5] TransactionReference04", null, row5.Reference4);
			});
		}

		protected override void PrepareTestData()
		{
			var sqlText = @"
DECLARE @JsPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @JsPk02 UNIQUEIDENTIFIER = NEWID();
DECLARE @JsPk03 UNIQUEIDENTIFIER = NEWID();
DECLARE @JsPk04 UNIQUEIDENTIFIER = NEWID();
DECLARE @JsPk05 UNIQUEIDENTIFIER = NEWID();
DECLARE @JsPk06 UNIQUEIDENTIFIER = NEWID();
DECLARE @JsPk07 UNIQUEIDENTIFIER = NEWID();
DECLARE @JsPk08 UNIQUEIDENTIFIER = NEWID();

DECLARE @OhPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @OhPk02 UNIQUEIDENTIFIER = NEWID();
DECLARE @OhPk03 UNIQUEIDENTIFIER = NEWID();
DECLARE @OhPk04 UNIQUEIDENTIFIER = NEWID();
DECLARE @OhPk05 UNIQUEIDENTIFIER = NEWID();

DECLARE @OaPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @OaPk02 UNIQUEIDENTIFIER = NEWID();
DECLARE @OaPk03 UNIQUEIDENTIFIER = NEWID();
DECLARE @OaPk04 UNIQUEIDENTIFIER = NEWID();
DECLARE @OaPk05 UNIQUEIDENTIFIER = NEWID();

DECLARE @GcPk0 UNIQUEIDENTIFIER = NEWID();
DECLARE @GcPk1 UNIQUEIDENTIFIER = NEWID();
DECLARE @GcPk2 UNIQUEIDENTIFIER = NEWID();
DECLARE @GcPk3 UNIQUEIDENTIFIER = NEWID();
DECLARE @GcPk4 UNIQUEIDENTIFIER = NEWID();

DECLARE @GbPk00 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk11 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk12 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk13 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk14 UNIQUEIDENTIFIER = NEWID();

INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode) VALUES
	(@GcPk0, 'DAU', 'AU company', 'AUD', 'AU'),
	(@GcPk1, 'DK1', 'DK company', 'EUR', 'DK');

INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy, GB_RL_NKHomePort) VALUES
	(@GbPk00, 'GB0', @GcPk0, NULL, 'AUSYD'),

	(@GbPk11, 'GB1', @GcPk1, NULL, 'DKAAB'),
	(@GbPk12, 'GB2', @GcPk1, NULL, 'CNSZG'),
	(@GbPk13, 'GB3', @GcPk1, NULL, 'NZAKL'),
	(@GbPk14, 'GB4', @GcPk1, NULL, 'DEACM');

INSERT dbo.GlbBranchExtraPorts(GY_PK, GY_RL_NKAdditionalBranchRelatedPort, GY_GB) VALUES
	(NEWID(), 'DKAAB', @GbPk12),
	(NEWID(), 'CNSHA', @GbPk14);

DECLARE @MiscOrgHeaderPK UNIQUEIDENTIFIER = NULL;
SELECT @MiscOrgHeaderPK = OH_PK FROM dbo.OrgHeader WHERE OrgHeader.OH_Code = 'MISC'

IF @MiscOrgHeaderPK IS NULL
	BEGIN
		INSERT dbo.OrgHeader (OH_PK, OH_Code,OH_FullName) VALUES (NEWID(), 'MISC', 'MISC');
		SELECT @MiscOrgHeaderPK = OH_PK FROM dbo.OrgHeader WHERE OrgHeader.OH_Code = 'MISC';
	END

INSERT dbo.OrgHeader (OH_PK, OH_Code,OH_FullName) VALUES
	(@OhPk01, 'OH1', 'OH_FullName01'),
	(@OhPk02, 'OH2', 'OH_FullName02'),
	(@OhPk03, 'OH3', 'OH_FullName03'),
	(@OhPk04, 'OH4', 'OH_FullName04'),
	(@OhPk05, 'OH5', 'OH_FullName05');

INSERT dbo.OrgAddress (OA_PK, OA_OH, OA_Address1, OA_CompanyNameOverride, OA_Code) VALUES
	(@OaPk01 , @OhPk01, 'Address01', 'OA_CompanyName01', 'OA_Code01'),
	(@OaPk02 , @OhPk02, 'Address02', 'OA_CompanyName02', 'OA_Code02'),
	(@OaPk03 , @MiscOrgHeaderPK, 'Address03', 'OA_CompanyName03', 'OA_Code03'),
	(@OaPk04 , @MiscOrgHeaderPK, 'Address04', 'OA_CompanyName04', 'OA_Code04'),
	(@OaPk05 , @OhPk05, 'Address05', '', 'OA_Code05');

INSERT dbo.StmALog (SL_PK, SL_Table, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_SE_NKEvent, SL_Reference, SL_GB_NKBranch, SL_IsCancelled) VALUES
	(NEWID(), 'JobShipment', getdate(), '2023-04-02 12:01:00', @JsPk01, 'STU', '|NEW=CNF|TYP=Shipment Status|RES=00', 'GB0', 'Y'), -- 00
	(NEWID(), 'JobShipment', getdate(), '2023-04-12 12:02:00', @JsPk01, 'STU', '|NEW=CNF|TYP=Shipment Status|RES=01', 'GB0', 'N'),  -- 01
	(NEWID(), 'JobShipment', getdate(), '2023-03-01 12:03:00', @JsPk02, 'STU', '|NEW=CNF|TYP=Shipment Status|RES=02', 'GB0', 'N'), -- Before date range
	(NEWID(), 'JobShipment', getdate(), '2023-05-01 12:04:00', @JsPk03, 'STU', '|NEW=CNF|TYP=Shipment Status|RES=03', 'GB0', 'N'), -- After date range
	(NEWID(), 'JobShipment', getdate(), '2023-04-01 12:05:00', @JsPk04, 'XXX', '|NEW=CNF|TYP=Shipment Status|RES=04', 'GB0', 'N'), -- SL_SE_NKEvent is not STU

	(NEWID(), 'JobShipment', getdate(), '2023-04-12 12:02:00', @JsPk05, 'STU', '|NEW=CNF|TYP=Shipment Status|RES=05', 'GB2', 'N'),  -- 02
	(NEWID(), 'JobShipment', getdate(), '2023-04-12 12:02:00', @JsPk06, 'STU', '|NEW=CNF|TYP=Shipment Status|RES=06', 'GB3', 'N'),  -- 03
	(NEWID(), 'JobShipment', getdate(), '2023-04-12 12:02:00', @JsPk07, 'STU', '|NEW=CNF|TYP=Shipment Status|RES=07', 'GB4', 'N'),  -- 04
	(NEWID(), 'JobShipment', getdate(), '2023-04-12 12:02:00', @JsPk08, 'STU', '|NEW=CNF|TYP=Shipment Status', 'GB2', 'N');  -- 05


INSERT dbo.JobShipment (JS_PK, JS_ShipmentType, JS_RL_NKOrigin, JS_UniqueConsignRef, JS_BookingReference, JS_IsBooking) VALUES
	(@JsPk01, 'HVL', 'AUMEL', 'JS00000000001', 'BookingReference01', 0),
	(@JsPk02, 'STD', 'AUMEL', 'JS00000000002', 'BookingReference02', 0),
	(@JsPk03, 'STD', 'AUMEL', 'JS00000000003', 'BookingReference03', 0),
	(@JsPk04, 'STD', 'AUMEL', 'JS00000000004', 'BookingReference04', 0),
	(@JsPk05, 'STD', 'DKAAB', 'JS00000000005', 'BookingReference05', 0),
	(@JsPk06, 'STD', 'NZAKL', 'JS00000000006', 'BookingReference06', 0),
	(@JsPk07, 'STD', 'CNSHA', 'JS00000000007', 'BookingReference07', 0),
	(@JsPk08, 'STD', 'CNSZG', 'JS00000000008', 'BookingReference08', 0);


INSERT dbo.JobDocAddress (E2_PK, E2_AddressOverride, E2_OA_Address, E2_ParentID, E2_AddressType, E2_ParentTableCode, E2_AddressSequence, E2_CompanyName, E2_ValidationStatus) VALUES
	(NEWID(), 0, @OaPk01, @JsPk01, 'BKD','JS', 0, '', 'NRQ'),
	(NEWID(), 1, @OaPk02, @JsPk01, 'BKD','JS', 1, 'i1i1i1i', 'NYV'),

	(NEWID(), 1, @OaPk02, @JsPk05, 'BKD','JS', 0, 'i1i1i1i', 'NYV'),

	(NEWID(), 0, @OaPk03, @JsPk06, 'BKD','JS', 0, 'i2i2i2i2', 'NRQ'),
	(NEWID(), 0, @OaPk04, @JsPk07, 'BKD','JS', 0, '', 'NRQ'),

	(NEWID(), 0, @OaPk05, @JsPk08, 'BKD','JS', 0, '', 'NRQ');";

			TestConnection.ExecuteNonQuery(sqlText);
		}
	}
}
