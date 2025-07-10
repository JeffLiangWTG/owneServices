using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(PortMessagingDataRecord))]
	sealed class PortMessagingDataRecordTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2022, 11);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals(2, transactions.Count());

			AssertNull("ISN event is before DateTime range", transactions.FirstOrDefault(x => x.Reference1 == "JK02"));
			AssertNull("ISN event is after DateTime range", transactions.FirstOrDefault(x => x.Reference1 == "JK03"));
			AssertNull("This is XX event", transactions.FirstOrDefault(x => x.Reference1 == "JK04"));

			var row0 = transactions.Single(x => x.Reference2.StartsWith("Advanced Logistics Port Order"));
			var row1 = transactions.Single(x => x.Reference2.StartsWith("CNSHA"));

			CombineAssertions(() =>
			{
				AssertEquals("[Row-0] CompanyCode", "DAU", row0.GetCompanyCode());
				AssertEquals("[Row-0] BranchCode", "GB1", row0.GetBranchCode());
				AssertEquals("[Row-0] TransactionDateUtc", new DateTime(2022, 11, 2, 12, 01, 00), row0.ServiceOccuredUTC);
				AssertEquals("[Row-0] ItemCount", 1, row0.BillableCount);
				AssertEquals("[Row-0] TransactionReference01", "JK01", row0.Reference1);
				AssertEquals("[Row-0] TransactionReference02", "Advanced Logistics Port Order", row0.Reference2);
				AssertEquals("[Row-0] TransactionReference03", "Original", row0.Reference3);
				AssertEquals("[Row-0] TransactionReference04", "AUTOMAN", row0.Reference4);

				AssertEquals("[Row-1] CompanyCode", "SHA", row1.GetCompanyCode());
				AssertEquals("[Row-1] BranchCode", "GB2", row1.GetBranchCode());
				AssertEquals("[Row-1] TransactionDateUtc", new DateTime(2022, 11, 12, 12, 02, 00), row1.ServiceOccuredUTC);
				AssertEquals("[Row-1] ItemCount", 1, row1.BillableCount);
				AssertEquals("[Row-1] TransactionReference01", "JK01", row1.Reference1);
				AssertEquals("[Row-1] TransactionReference02", "CNSHA - Advanced Logistics Port Order", row1.Reference2);
				AssertEquals("[Row-1] TransactionReference03", "Withdrawal", row1.Reference3);
				AssertEquals("[Row-1] TransactionReference04", "WUKONG", row1.Reference4);
			});
		}

		protected override void PrepareTestData()
		{
			string sqlText = $@"
DECLARE @JddPk01 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk02 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk03 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk04 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk10 UNIQUEIDENTIFIER = newid();

DECLARE @JkPk01 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk02 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk03 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk04 UNIQUEIDENTIFIER = newid();

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
	(newid(), 'JobDocumentData', getdate(), '2022-11-02 12:01:00', @JddPk01, 'ISN', '|MST=Advanced Logistics Port Order|STA=ORG|EQN=AUTOMAN', 'GB0', 'Y'), -- 00
	(newid(), 'JobDocumentData', getdate(), '2022-11-12 12:02:00', @JddPk01, 'ISN', '|MST=Advanced Logistics Port Order|STA=WTH|LOC=CNSHA|EQN=WUKONG', 'GB0', 'N'),  -- 01
	(newid(), 'JobDocumentData', getdate(), '2022-10-01 12:03:00', @JddPk02, 'ISN', '|MST=Advanced Logistics Port Order|STA=ORG|LOC=CNSHA|EQN=AUTOMAN', 'GB0', 'N'), -- Before date range
	(newid(), 'JobDocumentData', getdate(), '2022-12-01 12:04:00', @JddPk03, 'ISN', '|MST=Advanced Logistics Port Order|STA=AMD|LOC=CNSHA|EQN=HULUWA', 'GB0', 'N'), -- After date range
	(newid(), 'JobDocumentData', getdate(), '2022-11-01 12:05:00', @JddPk04, 'XXX', '|MST=Advanced Logistics Port Order|STA=ORG|LOC=CNSHA|EQN=AUTOMAN', 'GB0', 'N'), -- SL_SE_NKEvent is not ISN

	(newid(), 'JobDocumentData', getdate(), '2022-11-01 10:00:00', @JddPk01, 'MSN', 'MST=Advanced Logistics Port Order', 'GB1', 'N'), -- 00 MSN
	(newid(), 'JobAlien', getdate(), '2022-11-01 10:00:01', @JddPk01, 'MSN', 'MST=Advanced Logistics Port Order', 'GB2', 'N'),
	(newid(), 'JobDocumentData', getdate(), '2022-11-10 10:00:00', @JddPk01, 'MWR', 'MST=Advanced Logistics Port Order', 'GB2', 'Y'), -- 01 MWR
	(newid(), 'JobDocumentData', getdate(), '2022-11-13 10:00:00', @JddPk01, 'MSN', 'MST=Advanced Logistics Port Order', 'GB3', 'N'),
	(newid(), 'JobDocumentData', getdate(), '2022-10-10 10:00:00', @JddPk02, 'MSN', 'MST=Advanced Logistics Port Order', 'GB1', 'N'),
	(newid(), 'JobDocumentData', getdate(), '2022-10-10 10:00:00', @JddPk03, 'MSN', 'MST=Advanced Logistics Port Order', 'GB1', 'N'),
	(newid(), 'JobDocumentData', getdate(), '2022-10-10 10:00:00', @JddPk04, 'MSN', 'MST=Advanced Logistics Port Order', 'GB2', 'N');

INSERT dbo.JobDocumentData (JDD_PK, JDD_ParentID, JDD_ParentTableCode, JDD_SystemCreateTimeUtc, JDD_SystemLastEditTimeUtc, JDD_SystemCreateUser, JDD_SystemLastEditUser) VALUES
	(@JddPk01, @JkPk01, 'JK', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk02, @JkPk02, 'JK', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk03, @JkPk03, 'JK', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk04, @JkPk04, 'JK', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E');

INSERT dbo.JobConsol (JK_PK, JK_UniqueConsignRef) VALUES
	(@JkPk01, 'JK01'),
	(@JkPk02, 'JK02'),
	(@JkPk03, 'JK03'),
	(@JkPk04, 'JK04');";

			TestConnection.ExecuteNonQuery(sqlText);
		}
	}
}
