using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(ForwardingPortMessagingOutbound))]
	sealed class ForwardingPortMessagingOutboundTest : RefStlScriptWithDefaultsTest
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
			AssertEquals(10, transactions.Count());

			AssertNull("ISN event is before DateTime range", transactions.FirstOrDefault(x => x.Reference1 == "JK02"));
			AssertNull("ISN event is after DateTime range", transactions.FirstOrDefault(x => x.Reference1 == "JK03"));
			AssertNull("This is XX event", transactions.FirstOrDefault(x => x.Reference1 == "JK04"));

			var row0 = transactions.Single(x => x.Reference2.StartsWith("Export Notification (EBADEC)"));
			var row1 = transactions.Single(x => x.Reference2.StartsWith("CNSHA"));
			var row2 = transactions.Single(x => x.Reference2.StartsWith("AUSYD"));
			var row3 = transactions.Single(x => x.Reference2.StartsWith("IDNKW"));
			var row4 = transactions.Single(x => x.Reference2.StartsWith("ABCDE"));
			var row5 = transactions.Single(x => x.Reference2.StartsWith("DEFGA"));
			var row6 = transactions.Single(x => x.Reference2.StartsWith("MMMMM"));
			var row7 = transactions.Single(x => x.Reference2.StartsWith("ZZZZZ"));
			var row8 = transactions.Single(x => x.Reference2.StartsWith("QQQQQ"));
			var row9 = transactions.Single(x => x.Reference2.StartsWith("SSSSS"));

			CombineAssertions(() =>
			{
				AssertEquals("[Row-0] CompanyCode", "DAU", row0.GetCompanyCode());
				AssertEquals("[Row-0] BranchCode", "GB1", row0.GetBranchCode());
				AssertEquals("[Row-0] TransactionDateUtc", new DateTime(2022, 10, 2, 12, 01, 00), row0.ServiceOccuredUTC);
				AssertEquals("[Row-0] ItemCount", 1, row0.BillableCount);
				AssertEquals("[Row-0] TransactionReference01", "JK01", row0.Reference1);
				AssertEquals("[Row-0] TransactionReference02", "Export Notification (EBADEC)", row0.Reference2);
				AssertEquals("[Row-0] TransactionReference03", "Original", row0.Reference3);
				AssertEquals("[Row-0] TransactionReference04", "AUTOMAN", row0.Reference4);

				AssertEquals("[Row-1] CompanyCode", "SHA", row1.GetCompanyCode());
				AssertEquals("[Row-1] BranchCode", "GB2", row1.GetBranchCode());
				AssertEquals("[Row-1] TransactionDateUtc", new DateTime(2022, 10, 12, 12, 02, 00), row1.ServiceOccuredUTC);
				AssertEquals("[Row-1] ItemCount", 1, row1.BillableCount);
				AssertEquals("[Row-1] TransactionReference01", "JK01", row1.Reference1);
				AssertEquals("[Row-1] TransactionReference02", "CNSHA - Export Notification (EBADEC)", row1.Reference2);
				AssertEquals("[Row-1] TransactionReference03", "Withdrawal", row1.Reference3);
				AssertEquals("[Row-1] TransactionReference04", "WUKONG", row1.Reference4);

				AssertEquals("[Row-2] CompanyCode", "DAU", row2.GetCompanyCode());
				AssertEquals("[Row-2] BranchCode", "GB1", row2.GetBranchCode());
				AssertEquals("[Row-2] TransactionDateUtc", new DateTime(2022, 10, 1, 12, 06, 00), row2.ServiceOccuredUTC);
				AssertEquals("[Row-2] ItemCount", 1, row2.BillableCount);
				AssertEquals("[Row-2] TransactionReference01", "JK05", row2.Reference1);
				AssertEquals("[Row-2] TransactionReference02", "AUSYD - Dangerous Goods Notification - Export", row2.Reference2);
				AssertEquals("[Row-2] TransactionReference03", "Original", row2.Reference3);
				AssertEquals("[Row-2] TransactionReference04", "BAJIE", row2.Reference4);

				AssertEquals("[Row-3] CompanyCode", "SHA", row3.GetCompanyCode());
				AssertEquals("[Row-3] BranchCode", "GB2", row3.GetBranchCode());
				AssertEquals("[Row-3] TransactionDateUtc", new DateTime(2022, 10, 1, 12, 07, 00), row3.ServiceOccuredUTC);
				AssertEquals("[Row-3] ItemCount", 1, row3.BillableCount);
				AssertEquals("[Row-3] TransactionReference01", "JK06", row3.Reference1);
				AssertEquals("[Row-3] TransactionReference02", "IDNKW - Dangerous Goods Notification - Import", row3.Reference2);
				AssertEquals("[Row-3] TransactionReference03", "Amendment", row3.Reference3);
				AssertNull("[Row-3] TransactionReference04", row3.Reference4);

				AssertEquals("[Row-4] CompanyCode", "SHA", row4.GetCompanyCode());
				AssertEquals("[Row-4] BranchCode", "GB2", row4.GetBranchCode());
				AssertEquals("[Row-4] TransactionDateUtc", new DateTime(2022, 10, 1, 12, 08, 00), row4.ServiceOccuredUTC);
				AssertEquals("[Row-4] ItemCount", 1, row4.BillableCount);
				AssertEquals("[Row-4] TransactionReference01", "JK07", row4.Reference1);
				AssertEquals("[Row-4] TransactionReference02", "ABCDE - Certified Pickup - Accept/Decline", row4.Reference2);
				AssertEquals("[Row-4] TransactionReference03", "Original", row4.Reference3);
				AssertEquals("[Row-4] TransactionReference04", "GAOLAOZHUANG", row4.Reference4);

				AssertEquals("[Row-5] CompanyCode", "SHA", row5.GetCompanyCode());
				AssertEquals("[Row-5] BranchCode", "GB2", row5.GetBranchCode());
				AssertEquals("[Row-5] TransactionDateUtc", new DateTime(2022, 10, 1, 12, 09, 00), row5.ServiceOccuredUTC);
				AssertEquals("[Row-5] ItemCount", 1, row5.BillableCount);
				AssertEquals("[Row-5] TransactionReference01", "JK07", row5.Reference1);
				AssertEquals("[Row-5] TransactionReference02", "DEFGA - Certified Pickup - Transfer", row5.Reference2);
				AssertEquals("[Row-5] TransactionReference03", "Amendment", row5.Reference3);
				AssertEquals("[Row-5] TransactionReference04", "SHUILINADONG", row5.Reference4);

				AssertEquals("[Row-6] CompanyCode", "SZG", row6.GetCompanyCode());
				AssertEquals("[Row-6] BranchCode", "GB3", row6.GetBranchCode());
				AssertEquals("[Row-6] TransactionDateUtc", new DateTime(2022, 10, 1, 12, 10, 00), row6.ServiceOccuredUTC);
				AssertEquals("[Row-6] ItemCount", 1, row6.BillableCount);
				AssertEquals("[Row-6] TransactionReference01", "JK07", row6.Reference1);
				AssertEquals("[Row-6] TransactionReference02", "MMMMM - Certified Pickup - Revoke", row6.Reference2);
				AssertEquals("[Row-6] TransactionReference03", "Withdrawal", row6.Reference3);
				AssertEquals("[Row-6] TransactionReference04", "DONGTUDATANG", row6.Reference4);

				AssertEquals("[Row-7] CompanyCode", "SHA", row7.GetCompanyCode());
				AssertEquals("[Row-7] BranchCode", "GB2", row7.GetBranchCode());
				AssertEquals("[Row-7] TransactionDateUtc", new DateTime(2022, 10, 1, 12, 11, 00), row7.ServiceOccuredUTC);
				AssertEquals("[Row-7] ItemCount", 1, row7.BillableCount);
				AssertEquals("[Row-7] TransactionReference01", "JK08", row7.Reference1);
				AssertEquals("[Row-7] TransactionReference02", "ZZZZZ - Certified Pickup - Accept/Decline", row7.Reference2);
				AssertEquals("[Row-7] TransactionReference03", "Amendment", row7.Reference3);
				AssertEquals("[Row-7] TransactionReference04", "LANZHOU", row7.Reference4);

				AssertEquals("[Row-8] CompanyCode", "SZG", row8.GetCompanyCode());
				AssertEquals("[Row-8] BranchCode", "GB3", row8.GetBranchCode());
				AssertEquals("[Row-8] TransactionDateUtc", new DateTime(2022, 10, 1, 12, 12, 00), row8.ServiceOccuredUTC);
				AssertEquals("[Row-8] ItemCount", 1, row8.BillableCount);
				AssertEquals("[Row-8] TransactionReference01", "JK09", row8.Reference1);
				AssertEquals("[Row-8] TransactionReference02", "QQQQQ - Certified Pickup - Transfer", row8.Reference2);
				AssertEquals("[Row-8] TransactionReference03", "Original", row8.Reference3);
				AssertEquals("[Row-8] TransactionReference04", "XINJIANG", row8.Reference4);

				AssertEquals("[Row-9] CompanyCode", "DAU", row9.GetCompanyCode());
				AssertEquals("[Row-9] BranchCode", "GB1", row9.GetBranchCode());
				AssertEquals("[Row-9] TransactionDateUtc", new DateTime(2022, 10, 1, 12, 13, 00), row9.ServiceOccuredUTC);
				AssertEquals("[Row-9] ItemCount", 1, row9.BillableCount);
				AssertEquals("[Row-9] TransactionReference01", "JK10", row9.Reference1);
				AssertEquals("[Row-9] TransactionReference02", "SSSSS - Certified Pickup - Revoke", row9.Reference2);
				AssertEquals("[Row-9] TransactionReference03", "Withdrawal", row9.Reference3);
				AssertEquals("[Row-9] TransactionReference04", "HUHEHAOTE", row9.Reference4);
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
DECLARE @JddPk08 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk09 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk10 UNIQUEIDENTIFIER = newid();

DECLARE @JkPk01 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk02 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk03 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk04 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk05 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk06 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk07 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk08 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk09 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk10 UNIQUEIDENTIFIER = newid();

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
	(newid(), 'JobDocumentData', getdate(), '2022-10-02 12:01:00', @JddPk01, 'ISN', '|MST=Export Notification (EBADEC)|STA=ORG|EQN=AUTOMAN', 'GB0', 'Y'), -- 00
	(newid(), 'JobDocumentData', getdate(), '2022-10-12 12:02:00', @JddPk01, 'ISN', '|MST=Export Notification (EBADEC)|STA=WTH|LOC=CNSHA|EQN=WUKONG', 'GB0', 'N'),  -- 01
	(newid(), 'JobDocumentData', getdate(), '2022-09-01 12:03:00', @JddPk02, 'ISN', '|MST=Export Notification (EBADEC)|STA=ORG|LOC=CNSHA|EQN=AUTOMAN', 'GB0', 'N'), -- Before date range
	(newid(), 'JobDocumentData', getdate(), '2022-11-01 12:04:00', @JddPk03, 'ISN', '|MST=Export Notification (EBADEC)|STA=AMD|LOC=CNSHA|EQN=HULUWA', 'GB0', 'N'), -- After date range
	(newid(), 'JobDocumentData', getdate(), '2022-10-01 12:05:00', @JddPk04, 'XXX', '|MST=Export Notification (EBADEC)|STA=ORG|LOC=CNSHA|EQN=AUTOMAN', 'GB0', 'N'), -- SL_SE_NKEvent is not ISN

	(newid(), 'JobDocumentData', getdate(), '2022-10-01 10:00:00', @JddPk01, 'MSN', 'MST=Export Notification (EBADEC)', 'GB1', 'N'), -- 00 MSN
	(newid(), 'JobAlien', getdate(), '2022-10-01 10:00:01', @JddPk01, 'MSN', 'MST=Export Notification (EBADEC)', 'GB2', 'N'),
	(newid(), 'JobDocumentData', getdate(), '2022-10-10 10:00:00', @JddPk01, 'MWR', 'MST=Export Notification (EBADEC)', 'GB2', 'Y'), -- 01 MWR
	(newid(), 'JobDocumentData', getdate(), '2022-10-13 10:00:00', @JddPk01, 'MSN', 'MST=Export Notification (EBADEC)', 'GB3', 'N'),
	(newid(), 'JobDocumentData', getdate(), '2022-09-10 10:00:00', @JddPk02, 'MSN', 'MST=Export Notification (EBADEC)', 'GB1', 'N'),
	(newid(), 'JobDocumentData', getdate(), '2022-09-10 10:00:00', @JddPk03, 'MSN', 'MST=Export Notification (EBADEC)', 'GB1', 'N'),
	(newid(), 'JobDocumentData', getdate(), '2022-09-10 10:00:00', @JddPk04, 'MSN', 'MST=Export Notification (EBADEC)', 'GB2', 'N'),

	(newid(), 'JobDocumentData', getdate(), '2022-10-01 12:06:00', @JddPk05, 'ISN', 'MST=Dangerous Goods Notification - Export|STA=ORG|LOC=AUSYD|EQN=BAJIE', 'GB0', 'N'), -- 02
	(newid(), 'JobDocumentData', getdate(), '2022-10-01 12:07:00', @JddPk06, 'ISN', 'MST=Dangerous Goods Notification - Import|STA=AMD|LOC=IDNKW', 'GB0', 'N'), -- 03

	(newid(), 'JobDocumentData', getdate(), '2022-09-11 10:00:00', @JddPk05, 'MSN', 'MST=Dangerous Goods Notification - Export', 'GB1', 'N'), -- 02 MSN
	(newid(), 'JobDocumentData', getdate(), '2022-09-11 10:00:00', @JddPk06, 'MWR', 'MST=Dangerous Goods Notification - Import', 'GB2', 'N'), -- 03 MWR

	(newid(), 'JobDocumentData', getdate(), '2022-10-01 12:08:00', @JddPk07, 'ISN', 'MST=Certified Pickup - Accept/Decline|STA=ORG|LOC=ABCDE|EQN=GAOLAOZHUANG', 'GB0', 'N'), -- 04
	(newid(), 'JobDocumentData', getdate(), '2022-10-01 12:09:00', @JddPk07, 'ISN', 'MST=Certified Pickup - Transfer|STA=AMD|LOC=DEFGA|EQN=SHUILINADONG', 'GB0', 'N'), -- 05
	(newid(), 'JobDocumentData', getdate(), '2022-10-01 12:10:00', @JddPk07, 'ISN', 'MST=Certified Pickup - Revoke|STA=WTH|LOC=MMMMM|EQN=DONGTUDATANG', 'GB0', 'N'), -- 06

	(newid(), 'JobDocumentData', getdate(), '2022-10-01 10:00:00', @JddPk07, 'MSN', 'MST=Certified Pickup - Accept/Decline', 'GB1', 'N'),
	(newid(), 'JobDocumentData', getdate(), '2022-10-01 11:00:00', @JddPk07, 'MSN', 'MST=Certified Pickup - Accept/Decline', 'GB2', 'N'), -- 04 MSN
	(newid(), 'JobDocumentData', getdate(), '2022-11-01 11:00:00', @JddPk07, 'MSN', 'MST=Certified Pickup - Accept/Decline', 'GB3', 'N'),
	(newid(), 'JobDocumentData', getdate(), '2022-09-02 10:00:00', @JddPk07, 'MSN', 'MST=Certified Pickup - Transfer', 'GB2', 'N'), -- 05 MSN
	(newid(), 'JobDocumentData', getdate(), '2022-09-02 10:00:00', @JddPk07, 'MWR', 'MST=Certified Pickup - Revoke', 'GB3', 'N'), -- 06 MWR

	(newid(), 'JobDocumentData', getdate(), '2022-10-01 12:11:00', @JddPk08, 'ISN', 'MST=Certified Pickup - Accept/Decline|STA=AMD|LOC=ZZZZZ|EQN=LANZHOU', 'GB0', 'N'), -- 07
	(newid(), 'JobDocumentData', getdate(), '2022-10-01 12:12:00', @JddPk09, 'ISN', 'MST=Certified Pickup - Transfer|STA=ORG|LOC=QQQQQ|EQN=XINJIANG', 'GB0', 'N'), --08
	(newid(), 'JobDocumentData', getdate(), '2022-10-01 12:13:00', @JddPk10, 'ISN', 'MST=Certified Pickup - Revoke|STA=WTH|LOC=SSSSS|EQN=HUHEHAOTE', 'GB0', 'N'), -- 09

	(newid(), 'JobDocumentData', getdate(), '2022-09-01 10:14:00', @JddPk08, 'MSN', 'MST=Certified Pickup - Accept/Decline', 'GB1', 'N'),
	(newid(), 'JobDocumentData', getdate(), '2022-10-01 10:14:00', @JddPk08, 'MSN', 'MST=Certified Pickup - Accept/Decline', 'GB2', 'N'), -- 07 MSN
	(newid(), 'JobDocumentData', getdate(), '2022-09-02 10:15:00', @JddPk09, 'MSN', 'MST=Certified Pickup - Transfer', 'GB3', 'N'), -- 08 MSN
	(newid(), 'JobDocumentData', getdate(), '2022-09-02 10:16:00', @JddPk10, 'MWR', 'MST=Certified Pickup - Revoke', 'GB1', 'N'); -- 09 MWR

INSERT dbo.JobDocumentData (JDD_PK, JDD_ParentID, JDD_ParentTableCode, JDD_SystemCreateTimeUtc, JDD_SystemLastEditTimeUtc, JDD_SystemCreateUser, JDD_SystemLastEditUser) VALUES
	(@JddPk01, @JkPk01, 'JK', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk02, @JkPk02, 'JK', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk03, @JkPk03, 'JK', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk04, @JkPk04, 'JK', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk05, @JkPk05, 'JK', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk06, @JkPk06, 'JK', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk07, @JkPk07, 'JK', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk08, @JkPk08, 'JK', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk09, @JkPk09, 'JK', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk10, @JkPk10, 'JK', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),

	(newid(), @JkPk10, 'JC', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E');

INSERT dbo.JobConsol (JK_PK, JK_UniqueConsignRef) VALUES
	(@JkPk01, 'JK01'),
	(@JkPk02, 'JK02'),
	(@JkPk03, 'JK03'),
	(@JkPk04, 'JK04'),
	(@JkPk05, 'JK05'),
	(@JkPk06, 'JK06'),
	(@JkPk07, 'JK07'),
	(@JkPk08, 'JK08'),
	(@JkPk09, 'JK09'),
	(@JkPk10, 'JK10');";

			TestConnection.ExecuteNonQuery(sqlText);
		}
	}
}
