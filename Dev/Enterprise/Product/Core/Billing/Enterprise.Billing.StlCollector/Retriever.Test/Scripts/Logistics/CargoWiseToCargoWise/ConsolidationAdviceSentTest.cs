using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(ConsolidationAdviceSent))]
	sealed class ConsolidationAdviceSentTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2023, 04);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals(6, transactions.Count());

			AssertNull("ISN event is before DateTime range", transactions.FirstOrDefault(x => x.Reference1 == "JS00000000002"));
			AssertNull("ISN event is after DateTime range", transactions.FirstOrDefault(x => x.Reference1 == "JS00000000003"));
			AssertNull("This is XXX event", transactions.FirstOrDefault(x => x.Reference1 == "JS00000000004"));

			var row0 = transactions.Single(x => x.Reference1.StartsWith("JS00000000001") && x.Reference4.EndsWith("ORG"));
			var row1 = transactions.Single(x => x.Reference1.StartsWith("JS00000000001") && x.Reference4.EndsWith("WTH"));
			var row2 = transactions.Single(x => x.Reference1.StartsWith("JS00000000005"));
			var row3 = transactions.Single(x => x.Reference1.StartsWith("JS00000000006"));
			var row4 = transactions.Single(x => x.Reference1.StartsWith("JS00000000007"));
			var row5 = transactions.Single(x => x.Reference1.StartsWith("JS00000000008"));

			CombineAssertions(() =>
			{
				AssertEquals("[Row-0] CompanyCode", "DAU", row0.GetCompanyCode());
				AssertEquals("[Row-0] BranchCode", "GB1", row0.GetBranchCode());
				AssertEquals("[Row-0] TransactionDateUtc", new DateTime(2023, 04, 03, 12, 01, 00), row0.ServiceOccuredUTC);
				AssertEquals("[Row-0] ItemCount", 1, row0.BillableCount);
				AssertEquals("[Row-0] TransactionReference01", "JS00000000001", row0.Reference1);
				AssertEquals("[Row-0] TransactionReference02", "BookingReference01", row0.Reference2);
				AssertEquals("[Row-0] TransactionReference03", "OA_CompanyName01", row0.Reference3);
				AssertEquals("[Row-0] TransactionReference04", "Consolidation Advice - ORG", row0.Reference4);

				AssertEquals("[Row-1] CompanyCode", "SHA", row1.GetCompanyCode());
				AssertEquals("[Row-1] BranchCode", "GB2", row1.GetBranchCode());
				AssertEquals("[Row-1] TransactionDateUtc", new DateTime(2023, 04, 12, 12, 02, 00), row1.ServiceOccuredUTC);
				AssertEquals("[Row-1] ItemCount", 1, row1.BillableCount);
				AssertEquals("[Row-1] TransactionReference01", "JS00000000001", row1.Reference1);
				AssertEquals("[Row-1] TransactionReference02", "BookingReference01", row1.Reference2);
				AssertEquals("[Row-1] TransactionReference03", "OA_CompanyName01", row1.Reference3);
				AssertEquals("[Row-1] TransactionReference04", "Consolidation Advice - WTH", row1.Reference4);

				AssertEquals("[Row-2] CompanyCode", "DAU", row2.GetCompanyCode());
				AssertEquals("[Row-2] BranchCode", "GB1", row2.GetBranchCode());
				AssertEquals("[Row-2] TransactionDateUtc", new DateTime(2023, 04, 12, 12, 02, 00), row2.ServiceOccuredUTC);
				AssertEquals("[Row-2] ItemCount", 1, row2.BillableCount);
				AssertEquals("[Row-2] TransactionReference01", "JS00000000005", row2.Reference1);
				AssertEquals("[Row-2] TransactionReference02", "BookingReference05", row2.Reference2);
				AssertEquals("[Row-2] TransactionReference03", "i1i1i1i", row2.Reference3);
				AssertEquals("[Row-2] TransactionReference04", "Consolidation Advice - WTH", row2.Reference4);

				AssertEquals("[Row-3] CompanyCode", "DAU", row3.GetCompanyCode());
				AssertEquals("[Row-3] BranchCode", "GB1", row3.GetBranchCode());
				AssertEquals("[Row-3] TransactionDateUtc", new DateTime(2023, 04, 12, 12, 02, 00), row3.ServiceOccuredUTC);
				AssertEquals("[Row-3] ItemCount", 1, row3.BillableCount);
				AssertEquals("[Row-3] TransactionReference01", "JS00000000006", row3.Reference1);
				AssertEquals("[Row-3] TransactionReference02", "BookingReference06", row3.Reference2);
				AssertEquals("[Row-3] TransactionReference03", "i2i2i2i2", row3.Reference3);
				AssertEquals("[Row-3] TransactionReference04", "Consolidation Advice - WTH", row3.Reference4);

				AssertEquals("[Row-4] CompanyCode", "DAU", row4.GetCompanyCode());
				AssertEquals("[Row-4] BranchCode", "GB1", row4.GetBranchCode());
				AssertEquals("[Row-4] TransactionDateUtc", new DateTime(2023, 04, 12, 12, 02, 00), row4.ServiceOccuredUTC);
				AssertEquals("[Row-4] ItemCount", 1, row4.BillableCount);
				AssertEquals("[Row-4] TransactionReference01", "JS00000000007", row4.Reference1);
				AssertEquals("[Row-4] TransactionReference02", "BookingReference07", row4.Reference2);
				AssertEquals("[Row-4] TransactionReference03", null, row4.Reference3);
				AssertEquals("[Row-4] TransactionReference04", "Consolidation Advice - WTH", row4.Reference4);

				AssertEquals("[Row-5] CompanyCode", "DAU", row5.GetCompanyCode());
				AssertEquals("[Row-5] BranchCode", "GB1", row5.GetBranchCode());
				AssertEquals("[Row-5] TransactionDateUtc", new DateTime(2023, 04, 12, 12, 02, 00), row5.ServiceOccuredUTC);
				AssertEquals("[Row-5] ItemCount", 1, row5.BillableCount);
				AssertEquals("[Row-5] TransactionReference01", "JS00000000008", row5.Reference1);
				AssertEquals("[Row-5] TransactionReference02", "BookingReference08", row5.Reference2);
				AssertEquals("[Row-5] TransactionReference03", "OH_FullName05", row5.Reference3);
				AssertEquals("[Row-5] TransactionReference04", "Consolidation Advice - WTH", row5.Reference4);
			});
		}

		protected override void PrepareTestData()
		{
			var sqlText = @"
DECLARE @JddPk01 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk02 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk03 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk04 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk05 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk06 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk07 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk08 UNIQUEIDENTIFIER = newid();

DECLARE @JsPk01 UNIQUEIDENTIFIER = newid();
DECLARE @JsPk02 UNIQUEIDENTIFIER = newid();
DECLARE @JsPk03 UNIQUEIDENTIFIER = newid();
DECLARE @JsPk04 UNIQUEIDENTIFIER = newid();
DECLARE @JsPk05 UNIQUEIDENTIFIER = newid();
DECLARE @JsPk06 UNIQUEIDENTIFIER = newid();
DECLARE @JsPk07 UNIQUEIDENTIFIER = newid();
DECLARE @JsPk08 UNIQUEIDENTIFIER = newid();

DECLARE @OhPk01 UNIQUEIDENTIFIER = newid();
DECLARE @OhPk02 UNIQUEIDENTIFIER = newid();
DECLARE @OhPk03 UNIQUEIDENTIFIER = newid();
DECLARE @OhPk04 UNIQUEIDENTIFIER = newid();
DECLARE @OhPk05 UNIQUEIDENTIFIER = newid();

DECLARE @OaPk01 UNIQUEIDENTIFIER = newid();
DECLARE @OaPk02 UNIQUEIDENTIFIER = newid();
DECLARE @OaPk03 UNIQUEIDENTIFIER = newid();
DECLARE @OaPk04 UNIQUEIDENTIFIER = newid();
DECLARE @OaPk05 UNIQUEIDENTIFIER = newid();

DECLARE @GcPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @GcPk02 UNIQUEIDENTIFIER = NEWID();
DECLARE @GcPk03 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk00 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk02 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk03 UNIQUEIDENTIFIER = NEWID();
DECLARE @GePk01 UNIQUEIDENTIFIER = NEWID();

INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode) VALUES (@GcPk01, 'DAU', 'AU company', 'AUD', 'AU');
INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode) VALUES (@GcPk02, 'SHA', 'CN company1', 'CNY', 'CN');
INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode) VALUES (@GcPk03, 'SZG', 'CN company2', 'CNY', 'CN');
INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy) VALUES (@GbPk00, 'GB0', @GcPk01, NULL);
INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy) VALUES (@GbPk01, 'GB1', @GcPk01, NULL);
INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy) VALUES (@GbPk02, 'GB2', @GcPk02, NULL);
INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy) VALUES (@GbPk03, 'GB3', @GcPk03, NULL);
INSERT dbo.GlbDepartment (GE_PK, GE_Code) VALUES (@GePk01, 'DEP');

DECLARE @MiscOrgHeaderPK UNIQUEIDENTIFIER = NULL;
SELECT @MiscOrgHeaderPK = OH_PK FROM dbo.OrgHeader WHERE OrgHeader.OH_Code = 'MISC'

IF @MiscOrgHeaderPK IS NULL
	BEGIN
		INSERT dbo.OrgHeader (OH_PK, OH_Code,OH_FullName) VALUES (newid(), 'MISC', 'MISC');
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
	(newid(), 'JobDocumentData', getdate(), '2023-04-03 12:01:00', @JddPk01, 'ISN', '|MST=Consolidation Advice|STA=ORG|DEP=CargoWise', 'GB0', 'Y'), -- 00
	(newid(), 'JobDocumentData', getdate(), '2023-04-12 12:02:00', @JddPk01, 'ISN', '|MST=Consolidation Advice|STA=WTH|DEP=CargoWise', 'GB0', 'N'),  -- 01
	(newid(), 'JobDocumentData', getdate(), '2023-03-01 12:03:00', @JddPk02, 'ISN', '|MST=Consolidation Advice|STA=ORG|DEP=CargoWise', 'GB0', 'N'), -- Before date range
	(newid(), 'JobDocumentData', getdate(), '2023-05-01 12:04:00', @JddPk03, 'ISN', '|MST=Consolidation Advice|STA=AMD|DEP=CargoWise', 'GB0', 'N'), -- After date range
	(newid(), 'JobDocumentData', getdate(), '2023-04-01 12:05:00', @JddPk04, 'XXX', '|MST=Consolidation Advice|STA=ORG|DEP=CargoWise', 'GB0', 'N'), -- SL_SE_NKEvent is not ISN

	(newid(), 'JobDocumentData', getdate(), '2023-04-12 12:02:00', @JddPk05, 'ISN', '|MST=Consolidation Advice|STA=WTH|DEP=CargoWise', 'GB0', 'N'),  -- 02
	(newid(), 'JobDocumentData', getdate(), '2023-04-12 12:02:00', @JddPk06, 'ISN', '|MST=Consolidation Advice|STA=WTH|DEP=CargoWise', 'GB0', 'N'),  -- 03
	(newid(), 'JobDocumentData', getdate(), '2023-04-12 12:02:00', @JddPk07, 'ISN', '|MST=Consolidation Advice|STA=WTH|DEP=CargoWise', 'GB0', 'N'),  -- 04
	(newid(), 'JobDocumentData', getdate(), '2023-04-12 12:02:00', @JddPk08, 'ISN', '|MST=Consolidation Advice|STA=WTH|DEP=CargoWise', 'GB0', 'N'),  -- 05

	(newid(), 'JobDocumentData', getdate(), '2023-04-03 10:00:00', @JddPk01, 'MSN', 'MST=Consolidation Advice', 'GB1', 'N'), -- 00 MSN
	(newid(), 'JobAlien', getdate(), '2023-04-01 10:00:01', @JddPk01, 'MSN', 'MST=Consolidation Advice', 'GB2', 'N'),
	(newid(), 'JobDocumentData', getdate(), '2023-04-10 10:00:00', @JddPk01, 'MSN', 'MST=Consolidation Advice', 'GB2', 'Y'), -- 01 MSN
	(newid(), 'JobDocumentData', getdate(), '2023-04-13 10:00:00', @JddPk01, 'MSN', 'MST=Consolidation Advice', 'GB3', 'N'),
	(newid(), 'JobDocumentData', getdate(), '2023-03-01 10:00:00', @JddPk02, 'MSN', 'MST=Consolidation Advice', 'GB1', 'N'),
	(newid(), 'JobDocumentData', getdate(), '2023-04-20 10:00:00', @JddPk03, 'MSN', 'MST=Consolidation Advice', 'GB1', 'N'),
	(newid(), 'JobDocumentData', getdate(), '2023-04-01 10:00:00', @JddPk04, 'MSN', 'MST=Consolidation Advice', 'GB2', 'N'),

	(newid(), 'JobDocumentData', getdate(), '2023-04-01 10:00:00', @JddPk05, 'MSN', 'MST=Consolidation Advice', 'GB1', 'N'), -- 02 MSN
	(newid(), 'JobDocumentData', getdate(), '2023-04-01 10:00:00', @JddPk06, 'MSN', 'MST=Consolidation Advice', 'GB1', 'N'), -- 03 MSN
	(newid(), 'JobDocumentData', getdate(), '2023-04-01 10:00:00', @JddPk07, 'MSN', 'MST=Consolidation Advice', 'GB1', 'N'), -- 04 MSN
	(newid(), 'JobDocumentData', getdate(), '2023-04-01 10:00:00', @JddPk08, 'MSN', 'MST=Consolidation Advice', 'GB1', 'N'); -- 05 MSN

INSERT dbo.JobDocumentData (JDD_PK, JDD_ParentID, JDD_ParentTableCode, JDD_SystemCreateTimeUtc, JDD_SystemLastEditTimeUtc, JDD_SystemCreateUser, JDD_SystemLastEditUser) VALUES
	(@JddPk01, @JsPk01, 'JS', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk02, @JsPk02, 'JS', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk03, @JsPk03, 'JS', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk04, @JsPk04, 'JS', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk05, @JsPk05, 'JS', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk06, @JsPk06, 'JS', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk07, @JsPk07, 'JS', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk08, @JsPk08, 'JS', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E');

INSERT dbo.JobShipment (JS_PK, JS_ShipmentType, JS_RL_NKOrigin, JS_UniqueConsignRef, JS_BookingReference) VALUES
	(@JsPk01, 'HVL', 'AUSYD', 'JS00000000001', 'BookingReference01'),
	(@JsPk02, 'STD', 'AUSYD', 'JS00000000002', 'BookingReference02'),
	(@JsPk03, 'STD', 'AUSYD', 'JS00000000003', 'BookingReference03'),
	(@JsPk04, 'STD', 'AUSYD', 'JS00000000004', 'BookingReference04'),
	(@JsPk05, 'STD', 'AUSYD', 'JS00000000005', 'BookingReference05'),
	(@JsPk06, 'STD', 'AUSYD', 'JS00000000006', 'BookingReference06'),
	(@JsPk07, 'STD', 'AUSYD', 'JS00000000007', 'BookingReference07'),
	(@JsPk08, 'STD', 'AUSYD', 'JS00000000008', 'BookingReference08');


INSERT dbo.JobDocAddress (E2_PK, E2_AddressOverride, E2_OA_Address, E2_ParentID, E2_AddressType, E2_ParentTableCode, E2_AddressSequence, E2_CompanyName, E2_ValidationStatus) VALUES
	(newid(), 0, @OaPk01, @JsPk01, 'BKD','JS', 0, '', 'NRQ'),
	(newid(), 1, @OaPk02, @JsPk01, 'BKD','JS', 1, 'i1i1i1i', 'NYV'),

	(newid(), 1, @OaPk02, @JsPk05, 'BKD','JS', 0, 'i1i1i1i', 'NYV'),

	(newid(), 0, @OaPk03, @JsPk06, 'BKD','JS', 0, 'i2i2i2i2', 'NRQ'),
	(newid(), 0, @OaPk04, @JsPk07, 'BKD','JS', 0, '', 'NRQ'),

	(newid(), 0, @OaPk05, @JsPk08, 'BKD','JS', 0, '', 'NRQ');";

			TestConnection.ExecuteNonQuery(sqlText);
		}
	}
}
