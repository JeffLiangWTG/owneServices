using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(ForwardingPortMessagingInbound))]
	sealed class ForwardingPortMessagingInboundTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2022, 10);
			}
		}
		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals(15, transactions.Count());

			var row0 = transactions.Single(x => x.Reference2.StartsWith("Export Notification (EBADEC)"));
			var row1 = transactions.Single(x => x.Reference2.StartsWith("AHSNC"));
			var row2 = transactions.Single(x => x.Reference2.StartsWith("NSHAC"));
			var row3 = transactions.Single(x => x.Reference2.StartsWith("SHACN"));
			var row4 = transactions.Single(x => x.Reference2.StartsWith("AUSYD"));
			var row5 = transactions.Single(x => x.Reference2.StartsWith("IDNKW"));
			var row6 = transactions.Single(x => x.Reference2.StartsWith("VBNMM"));
			var row7 = transactions.Single(x => x.Reference2.StartsWith("IIIII"));
			var row8 = transactions.Single(x => x.Reference2.StartsWith("A0001"));
			var row9 = transactions.Single(x => x.Reference2.StartsWith("A0002"));
			var row10 = transactions.Single(x => x.Reference2.StartsWith("A0003"));
			var row11 = transactions.Single(x => x.Reference2.StartsWith("A0004"));
			var row12 = transactions.Single(x => x.Reference2.StartsWith("A0005"));
			var row13 = transactions.Single(x => x.Reference2.StartsWith("A0006"));
			var row14 = transactions.Single(x => x.Reference2.StartsWith("A0007"));

			CombineAssertions(() =>
			{
				AssertEquals("[Row-0] CompanyCode", "DAU", row0.GetCompanyCode());
				AssertEquals("[Row-0] BranchCode", "GB1", row0.GetBranchCode());
				AssertEquals("[Row-0] TransactionDateUtc", new DateTime(2022, 10, 1, 12, 10, 00), row0.ServiceOccuredUTC);
				AssertEquals("[Row-0] ItemCount", 1, row0.BillableCount);
				AssertEquals("[Row-0] TransactionReference01", "JK01", row0.Reference1);
				AssertEquals("[Row-0] TransactionReference02", "Export Notification (EBADEC)", row0.Reference2);
				AssertEquals("[Row-0] TransactionReference03", "MAA", row0.Reference3);
				AssertEquals("[Row-0] TransactionReference04", "SQL", row0.Reference4);

				AssertEquals("[Row-1] CompanyCode", "SHA", row1.GetCompanyCode());
				AssertEquals("[Row-1] BranchCode", "GB2", row1.GetBranchCode());
				AssertEquals("[Row-1] TransactionDateUtc", new DateTime(2022, 10, 11, 12, 11, 00), row1.ServiceOccuredUTC);
				AssertEquals("[Row-1] ItemCount", 1, row1.BillableCount);
				AssertEquals("[Row-1] TransactionReference01", "JK01", row1.Reference1);
				AssertEquals("[Row-1] TransactionReference02", "AHSNC - Export Notification (EBADEC)", row1.Reference2);
				AssertEquals("[Row-1] TransactionReference03", "MPP", row1.Reference3);
				AssertEquals("[Row-1] TransactionReference04", "IS", row1.Reference4);

				AssertEquals("[Row-2] CompanyCode", "SZG", row2.GetCompanyCode());
				AssertEquals("[Row-2] BranchCode", "GB3", row2.GetBranchCode());
				AssertEquals("[Row-2] TransactionDateUtc", new DateTime(2022, 10, 12, 12, 12, 00), row2.ServiceOccuredUTC);
				AssertEquals("[Row-2] ItemCount", 1, row2.BillableCount);
				AssertEquals("[Row-2] TransactionReference01", "JK01", row2.Reference1);
				AssertEquals("[Row-2] TransactionReference02", "NSHAC - Export Notification (EBADEC)", row2.Reference2);
				AssertEquals("[Row-2] TransactionReference03", "MWA", row2.Reference3);
				AssertEquals("[Row-2] TransactionReference04", "TOO", row2.Reference4);

				AssertEquals("[Row-3] CompanyCode", "SHA", row3.GetCompanyCode());
				AssertEquals("[Row-3] BranchCode", "GB2", row3.GetBranchCode());
				AssertEquals("[Row-3] TransactionDateUtc", new DateTime(2022, 10, 13, 12, 13, 00), row3.ServiceOccuredUTC);
				AssertEquals("[Row-3] ItemCount", 1, row3.BillableCount);
				AssertEquals("[Row-3] TransactionReference01", "JK01", row3.Reference1);
				AssertEquals("[Row-3] TransactionReference02", "SHACN - Export Notification (EBADEC)", row3.Reference2);
				AssertEquals("[Row-3] TransactionReference03", "MRJ", row3.Reference3);
				AssertEquals("[Row-3] TransactionReference04", "HARD", row3.Reference4);

				AssertEquals("[Row-4] CompanyCode", "DAU", row4.GetCompanyCode());
				AssertEquals("[Row-4] BranchCode", "GB1", row4.GetBranchCode());
				AssertEquals("[Row-4] TransactionDateUtc", new DateTime(2022, 10, 1, 12, 17, 00), row4.ServiceOccuredUTC);
				AssertEquals("[Row-4] ItemCount", 1, row4.BillableCount);
				AssertEquals("[Row-4] TransactionReference01", "JK05", row4.Reference1);
				AssertEquals("[Row-4] TransactionReference02", "AUSYD - Dangerous Goods Notification - Export", row4.Reference2);
				AssertEquals("[Row-4] TransactionReference03", "MAA", row4.Reference3);
				AssertEquals("[Row-4] TransactionReference04", "HOW", row4.Reference4);

				AssertEquals("[Row-5] CompanyCode", "SHA", row5.GetCompanyCode());
				AssertEquals("[Row-5] BranchCode", "GB2", row5.GetBranchCode());
				AssertEquals("[Row-5] TransactionDateUtc", new DateTime(2022, 10, 1, 12, 18, 00), row5.ServiceOccuredUTC);
				AssertEquals("[Row-5] ItemCount", 1, row5.BillableCount);
				AssertEquals("[Row-5] TransactionReference01", "JK06", row5.Reference1);
				AssertEquals("[Row-5] TransactionReference02", "IDNKW - Dangerous Goods Notification - Import", row5.Reference2);
				AssertEquals("[Row-5] TransactionReference03", "MPP", row5.Reference3);
				AssertEquals("[Row-5] TransactionReference04", "TO", row5.Reference4);

				AssertEquals("[Row-6] CompanyCode", "SHA", row6.GetCompanyCode());
				AssertEquals("[Row-6] BranchCode", "GB2", row6.GetBranchCode());
				AssertEquals("[Row-6] TransactionDateUtc", new DateTime(2022, 10, 1, 12, 19, 00), row6.ServiceOccuredUTC);
				AssertEquals("[Row-6] ItemCount", 1, row6.BillableCount);
				AssertEquals("[Row-6] TransactionReference01", "JK06", row6.Reference1);
				AssertEquals("[Row-6] TransactionReference02", "VBNMM - Dangerous Goods Notification - Import", row6.Reference2);
				AssertEquals("[Row-6] TransactionReference03", "MWA", row6.Reference3);
				AssertEquals("[Row-6] TransactionReference04", "OPTIMIZE", row6.Reference4);

				AssertEquals("[Row-7] CompanyCode", "SZG", row7.GetCompanyCode());
				AssertEquals("[Row-7] BranchCode", "GB3", row7.GetBranchCode());
				AssertEquals("[Row-7] TransactionDateUtc", new DateTime(2022, 10, 1, 12, 20, 00), row7.ServiceOccuredUTC);
				AssertEquals("[Row-7] ItemCount", 1, row7.BillableCount);
				AssertEquals("[Row-7] TransactionReference01", "JK07", row7.Reference1);
				AssertEquals("[Row-7] TransactionReference02", "IIIII - Dangerous Goods Notification - Export", row7.Reference2);
				AssertEquals("[Row-7] TransactionReference03", "MRJ", row7.Reference3);
				AssertEquals("[Row-7] TransactionReference04", "IT", row7.Reference4);

				AssertEquals("[Row-8] CompanyCode", "DAU", row8.GetCompanyCode());
				AssertEquals("[Row-8] BranchCode", "GB1", row8.GetBranchCode());
				AssertEquals("[Row-8] TransactionDateUtc", new DateTime(2022, 10, 1, 12, 21, 00), row8.ServiceOccuredUTC);
				AssertEquals("[Row-8] ItemCount", 1, row8.BillableCount);
				AssertEquals("[Row-8] TransactionReference01", "JK_C01", row8.Reference1);
				AssertEquals("[Row-8] TransactionReference02", "A0001 - Certified Pickup - Accept/Decline", row8.Reference2);
				AssertEquals("[Row-8] TransactionReference03", "MRJ", row8.Reference3);
				AssertEquals("[Row-8] TransactionReference04", "NEQQEN - A", row8.Reference4);

				AssertEquals("[Row-9] CompanyCode", "SHA", row9.GetCompanyCode());
				AssertEquals("[Row-9] BranchCode", "GB2", row9.GetBranchCode());
				AssertEquals("[Row-9] TransactionDateUtc", new DateTime(2022, 10, 15, 12, 22, 00), row9.ServiceOccuredUTC);
				AssertEquals("[Row-9] ItemCount", 1, row9.BillableCount);
				AssertEquals("[Row-9] TransactionReference01", "JK_C01", row9.Reference1);
				AssertEquals("[Row-9] TransactionReference02", "A0002 - Certified Pickup - Accept/Decline", row9.Reference2);
				AssertEquals("[Row-9] TransactionReference03", "MPP", row9.Reference3);
				AssertEquals("[Row-9] TransactionReference04", "NEQNEQ - C", row9.Reference4);

				AssertEquals("[Row-10] CompanyCode", "DAU", row10.GetCompanyCode());
				AssertEquals("[Row-10] BranchCode", "GB1", row10.GetBranchCode());
				AssertEquals("[Row-10] TransactionDateUtc", new DateTime(2022, 10, 1, 12, 23, 00), row10.ServiceOccuredUTC);
				AssertEquals("[Row-10] ItemCount", 1, row10.BillableCount);
				AssertEquals("[Row-10] TransactionReference01", "JK_C01", row10.Reference1);
				AssertEquals("[Row-10] TransactionReference02", "A0003 - Certified Pickup - Accept/Decline", row10.Reference2);
				AssertEquals("[Row-10] TransactionReference03", "MAA", row10.Reference3);
				AssertEquals("[Row-10] TransactionReference04", "NEQENQ - E", row10.Reference4);

				AssertEquals("[Row-11] CompanyCode", "SZG", row11.GetCompanyCode());
				AssertEquals("[Row-11] BranchCode", "GB3", row11.GetBranchCode());
				AssertEquals("[Row-11] TransactionDateUtc", new DateTime(2022, 10, 1, 12, 24, 00), row11.ServiceOccuredUTC);
				AssertEquals("[Row-11] ItemCount", 1, row11.BillableCount);
				AssertEquals("[Row-11] TransactionReference01", "JK_C01", row11.Reference1);
				AssertEquals("[Row-11] TransactionReference02", "A0004 - Certified Pickup - Revoke", row11.Reference2);
				AssertEquals("[Row-11] TransactionReference03", "MWA", row11.Reference3);
				AssertEquals("[Row-11] TransactionReference04", "EHUB - IT", row11.Reference4);

				AssertEquals("[Row-12] CompanyCode", "DAU", row12.GetCompanyCode());
				AssertEquals("[Row-12] BranchCode", "GB1", row12.GetBranchCode());
				AssertEquals("[Row-12] TransactionDateUtc", new DateTime(2022, 10, 1, 12, 25, 00), row12.ServiceOccuredUTC);
				AssertEquals("[Row-12] ItemCount", 1, row12.BillableCount);
				AssertEquals("[Row-12] TransactionReference01", "JK_C01", row12.Reference1);
				AssertEquals("[Row-12] TransactionReference02", "A0005 - Certified Pickup - Transfer", row12.Reference2);
				AssertEquals("[Row-12] TransactionReference03", "MAA", row12.Reference3);
				AssertEquals("[Row-12] TransactionReference04", "XHUB - XD", row12.Reference4);

				AssertEquals("[Row-13] CompanyCode", "SHA", row13.GetCompanyCode());
				AssertEquals("[Row-13] BranchCode", "GB2", row13.GetBranchCode());
				AssertEquals("[Row-13] TransactionDateUtc", new DateTime(2022, 10, 1, 12, 26, 00), row13.ServiceOccuredUTC);
				AssertEquals("[Row-13] ItemCount", 1, row13.BillableCount);
				AssertEquals("[Row-13] TransactionReference01", "JK_C02", row13.Reference1);
				AssertEquals("[Row-13] TransactionReference02", "A0006 - Certified Pickup - Revoke", row13.Reference2);
				AssertEquals("[Row-13] TransactionReference03", "MWA", row13.Reference3);
				AssertEquals("[Row-13] TransactionReference04", "EQN07 - B", row13.Reference4);

				AssertEquals("[Row-14] CompanyCode", "SZG", row14.GetCompanyCode());
				AssertEquals("[Row-14] BranchCode", "GB3", row14.GetBranchCode());
				AssertEquals("[Row-14] TransactionDateUtc", new DateTime(2022, 10, 1, 12, 27, 00), row14.ServiceOccuredUTC);
				AssertEquals("[Row-14] ItemCount", 1, row14.BillableCount);
				AssertEquals("[Row-14] TransactionReference01", "JK_C03", row14.Reference1);
				AssertEquals("[Row-14] TransactionReference02", "A0007 - Certified Pickup - Transfer", row14.Reference2);
				AssertEquals("[Row-14] TransactionReference03", "MPP", row14.Reference3);
				AssertEquals("[Row-14] TransactionReference04", "EQN08 - C", row14.Reference4);
			});
		}

		protected override void PrepareTestData()
		{
			string sqlText = $@"
DECLARE @JddPk01 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk02 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk03 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk04 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk05 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk06 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk07 UNIQUEIDENTIFIER = newid();

DECLARE @JcPk01 UNIQUEIDENTIFIER = newid();
DECLARE @JcPk02 UNIQUEIDENTIFIER = newid();
DECLARE @JcPk03 UNIQUEIDENTIFIER = newid();
DECLARE @JcPk04 UNIQUEIDENTIFIER = newid();
DECLARE @JcPk05 UNIQUEIDENTIFIER = newid();
DECLARE @JcPk06 UNIQUEIDENTIFIER = newid();
DECLARE @JcPk07 UNIQUEIDENTIFIER = newid();

DECLARE @JkPk01 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk02 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk03 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk04 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk05 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk06 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk07 UNIQUEIDENTIFIER = newid();

DECLARE @Jk_cPk01 UNIQUEIDENTIFIER = newid();
DECLARE @Jk_cPk02 UNIQUEIDENTIFIER = newid();
DECLARE @Jk_cPk03 UNIQUEIDENTIFIER = newid();
DECLARE @Jk_cPk04 UNIQUEIDENTIFIER = newid();
DECLARE @Jk_cPk05 UNIQUEIDENTIFIER = newid();
DECLARE @Jk_cPk06 UNIQUEIDENTIFIER = newid();
DECLARE @Jk_cPk07 UNIQUEIDENTIFIER = newid();

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

INSERT dbo.StmALog (SL_PK, SL_Table, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_SE_NKEvent, SL_Reference, SL_GB_NKBranch, SL_IsCancelled) VALUES
	(newid(), 'JobDocumentData', getdate(), '2022-10-01 12:10:00', @JddPk01, 'MAA', '|MST=Export Notification (EBADEC)|RFN=SQL|EQN=AUTOMAN', 'GB0', 'N'), -- 00
	(newid(), 'JobDocumentData', getdate(), '2022-10-11 12:11:00', @JddPk01, 'MPP', '|MST=Export Notification (EBADEC)|RFN=IS|LOC=AHSNC|EQN=WUKONG', 'GB0', 'Y'),  -- 01
	(newid(), 'JobDocumentData', getdate(), '2022-10-12 12:12:00', @JddPk01, 'MWA', '|MST=Export Notification (EBADEC)|RFN=TOO|LOC=NSHAC|EQN=WUKONG', 'GB0', 'N'),  -- 02
	(newid(), 'JobDocumentData', getdate(), '2022-10-13 12:13:00', @JddPk01, 'MRJ', '|MST=Export Notification (EBADEC)|RFN=HARD|LOC=SHACN|EQN=WUKONG', 'GB0', 'N'),  -- 03
	(newid(), 'JobDocumentData', getdate(), '2022-09-01 12:14:00', @JddPk02, 'MAA', '|MST=Export Notification (EBADEC)|RFN=DO|LOC=CNSHA|EQN=AUTOMAN', 'GB0', 'N'), -- Before date range
	(newid(), 'JobDocumentData', getdate(), '2022-11-01 12:15:00', @JddPk03, 'MPP', '|MST=Export Notification (EBADEC)|RFN=NOT|LOC=CNSHA|EQN=HULUWA', 'GB0', 'N'), -- After date range
	(newid(), 'JobDocumentData', getdate(), '2022-10-01 12:16:00', @JddPk04, 'ISN', '|MST=Export Notification (EBADEC)|RFN=KNOW|LOC=CNSHA|EQN=AUTOMAN', 'GB0', 'N'), -- SL_SE_NKEvent is ISN

	(newid(), 'JobDocumentData', getdate(), '2022-09-02 10:00:00', @JddPk01, 'MSN', 'MST=Export Notification (EBADEC)', 'GB1', 'Y'), -- 00(MAA) -> MSN
	(newid(), 'JobAlien', getdate(), '2022-09-02 10:00:01', @JddPk01, 'MSN', 'MST=Export Notification (EBADEC)', 'GB2', 'N'),
	(newid(), 'JobDocumentData', getdate(), '2022-10-03 10:00:00', @JddPk01, 'MSN', 'MST=Export Notification (EBADEC)', 'GB2', 'N'), -- 01(MPP), 03(MRJ) -> MSN
	(newid(), 'JobDocumentData', getdate(), '2022-10-03 11:00:00', @JddPk01, 'MWR', 'MST=Export Notification (EBADEC)', 'GB3', 'Y'), -- 02(MWA) -> MWR

	(newid(), 'JobDocumentData', getdate(), '2022-09-01 10:00:00', @JddPk02, 'MSN', 'MST=Export Notification (EBADEC)', 'GB1', 'N'),
	(newid(), 'JobDocumentData', getdate(), '2022-09-01 10:00:00', @JddPk03, 'MSN', 'MST=Export Notification (EBADEC)', 'GB1', 'N'),
	(newid(), 'JobDocumentData', getdate(), '2022-09-01 10:00:00', @JddPk04, 'MSN', 'MST=Export Notification (EBADEC)', 'GB2', 'N'),

	(newid(), 'JobDocumentData', getdate(), '2022-10-01 12:17:00', @JddPk05, 'MAA', 'MST=Dangerous Goods Notification - Export|RFN=HOW|LOC=AUSYD|EQN=NEQ==', 'GB0', 'N'), -- 04
	(newid(), 'JobDocumentData', getdate(), '2022-10-01 12:18:00', @JddPk06, 'MPP', 'MST=Dangerous Goods Notification - Import|RFN=TO|LOC=IDNKW', 'GB0', 'N'), -- 05
	(newid(), 'JobDocumentData', getdate(), '2022-10-01 12:19:00', @JddPk06, 'MWA', 'MST=Dangerous Goods Notification - Import|RFN=OPTIMIZE|LOC=VBNMM', 'GB0', 'N'), -- 06
	(newid(), 'JobDocumentData', getdate(), '2022-10-01 12:20:00', @JddPk07, 'MRJ', 'MST=Dangerous Goods Notification - Export|RFN=IT|LOC=IIIII|EQN=NEQ!=', 'GB0', 'N'), -- 07

	(newid(), 'JobDocumentData', getdate(), '2022-09-15 10:00:00', @JddPk05, 'MSN', 'MST=Dangerous Goods Notification - Export', 'GB1', 'N'), -- 04(MAA) -> MSN
	(newid(), 'JobDocumentData', getdate(), '2022-09-15 10:11:00', @JddPk06, 'MSN', 'MST=Dangerous Goods Notification - Import', 'GB2', 'N'), -- 05(MPP) -> MSN
	(newid(), 'JobDocumentData', getdate(), '2022-09-15 10:11:00', @JddPk06, 'MWR', 'MST=Dangerous Goods Notification - Import', 'GB2', 'N'), -- 06(MWA) -> MWR
	(newid(), 'JobDocumentData', getdate(), '2022-09-15 10:00:00', @JddPk07, 'MSN', 'MST=Dangerous Goods Notification - Export', 'GB3', 'N'), -- 07(MRJ) -> MSN

	(newid(), 'JobContainer', getdate(), '2022-10-01 12:21:00', @JcPk01, 'MRJ', 'MST=Certified Pickup - Accept/Decline|RFN=A|LOC=A0001|EQN=NEQQEN', 'GB0', 'N'), -- 08
	(newid(), 'JobContainer', getdate(), '2022-10-15 12:22:00', @JcPk01, 'MPP', 'MST=Certified Pickup - Accept/Decline|RFN=C|LOC=A0002|EQN=NEQNEQ', 'GB0', 'N'), -- 09
	(newid(), 'JobContainer', getdate(), '2022-10-01 12:23:00', @JcPk01, 'MAA', 'MST=Certified Pickup - Accept/Decline|RFN=E|LOC=A0003|EQN=NEQENQ', 'GB0', 'N'), -- 10
	(newid(), 'JobContainer', getdate(), '2022-10-01 12:24:00', @JcPk01, 'MWA', 'MST=Certified Pickup - Revoke|RFN=IT|LOC=A0004|EQN=EHUB', 'GB0', 'N'), -- 11
	(newid(), 'JobContainer', getdate(), '2022-10-01 12:25:00', @JcPk01, 'MAA', 'MST=Certified Pickup - Transfer|RFN=XD|LOC=A0005|EQN=XHUB', 'GB0', 'N'), -- 12

	(newid(), 'JobConsol', getdate(), '2022-09-15 10:00:00', @Jk_cPk01, 'MSN', 'MST=Certified Pickup - Accept/Decline', 'GB1', 'N'), -- 08(MRJ), 10(MAA) -> MSN
	(newid(), 'JobConsol', getdate(), '2022-10-10 10:00:00', @Jk_cPk01, 'MSN', 'MST=Certified Pickup - Accept/Decline', 'GB2', 'N'), -- 09(MPP) -> MSN
	(newid(), 'JobConsol', getdate(), '2022-09-15 10:00:00', @Jk_cPk01, 'MWR', 'MST=Certified Pickup - Revoke', 'GB3', 'N'), -- 11(MWA) -> MWR
	(newid(), 'JobConsol', getdate(), '2022-09-15 10:00:00', @Jk_cPk01, 'MSN', 'MST=Certified Pickup - Transfer', 'GB1', 'N'), -- 12(MAA) -> MSN
	(newid(), 'JobConsol', getdate(), '2022-10-10 10:00:00', @Jk_cPk01, 'MSN', 'MST=Certified Pickup - Transfer', 'GB2', 'N'),

	(newid(), 'JobContainer', getdate(), '2022-10-01 12:26:00', @JcPk02, 'MWA', 'MST=Certified Pickup - Revoker|RFN=B|LOC=A0006|EQN=EQN07', 'GB0', 'N'), -- 13
	(newid(), 'JobContainer', getdate(), '2022-10-01 12:27:00', @JcPk03, 'MPP', 'MST=Certified Pickup - Transfer|RFN=C|LOC=A0007|EQN=EQN08', 'GB0', 'N'), -- 14

	(newid(), 'JobContainer', getdate(), '2022-09-01 12:28:00', @JcPk03, 'MPP', 'MST=Certified Pickup - Transfer|RFN=D|LOC=A0008|EQN=EQN09', 'GB0', 'N'), -- Before
	(newid(), 'JobContainer', getdate(), '2022-11-01 12:29:00', @JcPk03, 'MPP', 'MST=Certified Pickup - Transfer|RFN=E|LOC=A0009|EQN=EQN10', 'GB0', 'N'), -- After

	(newid(), 'JobContainer', getdate(), '2022-10-01 12:30:00', @JcPk04, 'XXX', 'MST=Certified Pickup - Transfer|RFN=F|LOC=A0010|EQN=EQN11', 'GB0', 'N'), -- SL_SE_NKEvent is XXX

	(newid(), 'JobConsol', getdate(), '2022-09-15 10:00:00', @Jk_cPk02, 'MWR', 'MST=Certified Pickup - Revoker', 'GB2', 'N'), -- 13(MWA) -> MWR
	(newid(), 'JobAlien', getdate(), '2022-09-15 10:00:01', @Jk_cPk02, 'MWR', 'MST=Certified Pickup - Revoker', 'GB3', 'N'),
	(newid(), 'JobConsol', getdate(), '2022-09-05 10:00:00', @Jk_cPk03, 'MSN', 'MST=Certified Pickup - Transfer', 'GB3', 'N'), -- 14(MPP) -> MSN

	(newid(), 'JobConsol', getdate(), '2022-09-01 10:00:00', @Jk_cPk04, 'MSN', 'MST=Certified Pickup - Transfer', 'GB3', 'N'),

	(newid(), 'JobContainer', getdate(), '2022-10-01 12:31:00', @JcPk05, 'ATH', 'TYP=Container Release|RFN=G|LOC=B0001|EQN=EQN01|STA=sssss', 'GB0', 'N'), -- 15
	(newid(), 'JobContainer', getdate(), '2022-10-01 12:32:00', @JcPk05, 'ATW', 'TYP=Container Release|RFN=H|LOC=B0002|EQN=EQN02|STA=aaaaa', 'GB0', 'N'), -- 16
	(newid(), 'JobContainer', getdate(), '2022-09-01 12:33:00', @JcPk05, 'ATH', 'TYP=Container Release|RFN=I|LOC=B0003|EQN=EQN03|STA=bbbbb', 'GB0', 'N'), -- Before
	(newid(), 'JobContainer', getdate(), '2022-11-01 12:34:00', @JcPk05, 'ATW', 'TYP=Container Release|RFN=J|LOC=B0004|EQN=EQN04|STA=ccccc', 'GB0', 'N'), -- After

	(newid(), 'JobContainer', getdate(), '2022-10-01 12:35:00', @JcPk06, 'ATH', 'TYP=Container Release|RFN=K|LOC=B0005|EQN=EQN05|STA=eeeee', 'GB2', 'N'), -- 17 -- No MSN
	(newid(), 'JobContainer', getdate(), '2022-10-22 12:36:00', @JcPk07, 'ATW', 'TYP=Container Release|RFN=~AD|LOC=B0006|EQN=EQN06|STA=fffff', 'GB3', 'N'), -- 18 -- No MSN

	(newid(), 'JobAlien', getdate(), '2022-10-15 08:59:59', @Jk_cPk05, 'MSN', 'MST=Certified Pickup - Accept/Decline', 'GB2', 'N'),
	(newid(), 'JobConsol', getdate(), '2022-10-15 09:00:00', @Jk_cPk05, 'MSN', 'MST=Certified Pickup - Accept/Decline', 'GB1', 'N'), -- 15(ATH), 16(ATW) -> MSN
	(newid(), 'JobConsol', getdate(), '2022-10-15 10:00:00', @Jk_cPk05, 'MSN', 'MST=Certified Pickup - Accept/Decline', 'GB2', 'N'),
	(newid(), 'JobConsol', getdate(), '2022-10-15 11:00:00', @Jk_cPk05, 'MSN', 'MST=Certified Pickup - Accept/Decline', 'GB3', 'N');

INSERT dbo.JobDocumentData (JDD_PK, JDD_ParentID, JDD_ParentTableCode, JDD_SystemCreateTimeUtc, JDD_SystemLastEditTimeUtc, JDD_SystemCreateUser, JDD_SystemLastEditUser) VALUES
	(@JddPk01, @JkPk01, 'JK', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk02, @JkPk02, 'JK', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk03, @JkPk03, 'JK', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk04, @JkPk04, 'JK', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk05, @JkPk05, 'JK', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk06, @JkPk06, 'JK', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk07, @JkPk07, 'JK', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),

	(newid(), @JkPk07, 'JC', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E');

INSERT dbo.JobConsol (JK_PK, JK_UniqueConsignRef) VALUES
	(@JkPk01, 'JK01'),
	(@JkPk02, 'JK02'),
	(@JkPk03, 'JK03'),
	(@JkPk04, 'JK04'),
	(@JkPk05, 'JK05'),
	(@JkPk06, 'JK06'),
	(@JkPk07, 'JK07'),

	(@Jk_cPk01, 'JK_C01'),
	(@Jk_cPk02, 'JK_C02'),
	(@Jk_cPk03, 'JK_C03'),
	(@Jk_cPk04, 'JK_C04'),
	(@Jk_cPk05, 'JK_C05'),
	(@Jk_cPk06, 'JK_C06'),
	(@Jk_cPk07, 'JK_C07');

INSERT dbo.JobContainer (JC_PK, JC_JK) VALUES
	(@JcPk01, @Jk_cPk01),
	(@JcPk02, @Jk_cPk02),
	(@JcPk03, @Jk_cPk03),
	(@JcPk04, @Jk_cPk04),
	(@JcPk05, @Jk_cPk05),
	(@JcPk06, @Jk_cPk06),
	(@JcPk07, @Jk_cPk07);";

			TestConnection.ExecuteNonQuery(sqlText);
		}
	}
}
