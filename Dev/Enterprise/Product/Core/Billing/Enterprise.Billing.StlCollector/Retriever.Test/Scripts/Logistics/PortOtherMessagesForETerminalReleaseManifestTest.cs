using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(PortOtherMessagesForETerminalReleaseManifest))]
	sealed class PortOtherMessagesForETerminalReleaseManifestTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2022, 11);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals(2, transactions.Count());

			AssertNull("ISN event is before DateTime range", transactions.FirstOrDefault(x => x.Reference2.StartsWith("CN003")));
			AssertNull("ISN event is after DateTime range", transactions.FirstOrDefault(x => x.Reference2.StartsWith("CN004")));
			AssertNull("This is XX event", transactions.FirstOrDefault(x => x.Reference2.StartsWith("CN005")));

			var row0 = transactions.Single(x => x.Reference2.StartsWith("CN001"));
			var row1 = transactions.Single(x => x.Reference2.StartsWith("CN002"));

			CombineAssertions(() =>
			{
				AssertEquals("[Row-0] CompanyCode", "DAU", row0.GetCompanyCode());
				AssertEquals("[Row-0] BranchCode", "GB1", row0.GetBranchCode());
				AssertEquals("[Row-0] TransactionDateUtc", new DateTime(2022, 11, 2, 12, 01, 00), row0.ServiceOccuredUTC);
				AssertEquals("[Row-0] ItemCount", 1, row0.BillableCount);
				AssertEquals("[Row-0] TransactionReference01", "JkUniqueConsignRef01", row0.Reference1);
				AssertEquals("[Row-0] TransactionReference02", "CN001 - eTerminal Release Manifest", row0.Reference2);
				AssertEquals("[Row-0] TransactionReference03", "ORG", row0.Reference3);
				AssertEquals("[Row-0] TransactionReference04", "JV02 - JVF02", row0.Reference4);

				AssertEquals("[Row-1] CompanyCode", "SZG", row1.GetCompanyCode());
				AssertEquals("[Row-1] BranchCode", "GB3", row1.GetBranchCode());
				AssertEquals("[Row-1] TransactionDateUtc", new DateTime(2022, 11, 12, 12, 02, 00), row1.ServiceOccuredUTC);
				AssertEquals("[Row-1] ItemCount", 1, row1.BillableCount);
				AssertEquals("[Row-1] TransactionReference01", "JkUniqueConsignRef02", row1.Reference1);
				AssertEquals("[Row-1] TransactionReference02", "CN002 - eTerminal Release Manifest", row1.Reference2);
				AssertNull("[Row-1] TransactionReference03", row1.Reference3);
				AssertEquals("[Row-1] TransactionReference04", "JV03 - JVF03", row1.Reference4);
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
	(newid(), 'JobDocumentData', getdate(), '2022-11-02 12:01:00', @JddPk01, 'ISN', '|MST=eTerminal Release Manifest|STA=ORG|LOC=CN001', 'GB0', 'N'), -- 01
	(newid(), 'JobDocumentData', getdate(), '2022-11-12 12:02:00', @JddPk02, 'ISN', '|MST=eTerminal Release Manifest|STA=AMD|LOC=CN002', 'GB0', 'N'),  -- 02
	(newid(), 'JobDocumentData', getdate(), '2022-10-01 12:03:00', @JddPk03, 'ISN', '|MST=eTerminal Release Manifest|STA=ORG|LOC=CN003', 'GB0', 'N'), -- Before date range
	(newid(), 'JobDocumentData', getdate(), '2022-12-01 12:04:00', @JddPk04, 'ISN', '|MST=eTerminal Release Manifest|STA=ORG|LOC=CN004', 'GB0', 'N'), -- After date range
	(newid(), 'JobDocumentData', getdate(), '2022-11-01 12:05:00', @JddPk05, 'XXX', '|MST=eTerminal Release Manifest|STA=ORG|LOC=CN005', 'GB0', 'N'), -- SL_SE_NKEvent is not ISN

	(newid(), 'JobDocumentData', getdate(), '2022-11-01 10:00:00', @JddPk01, 'MSN', 'MST=eTerminal Release Manifest', 'GB1', 'N'), -- 01 MSN
	(newid(), 'JobDocumentData', getdate(), '2022-11-10 10:00:00', @JddPk01, 'MSN', 'MST=eTerminal Release Manifest', 'GB2', 'N'), -- 01 MSN
	(newid(), 'JobDocumentData', getdate(), '2022-11-12 10:00:00', @JddPk02, 'MSN', 'MST=eTerminal Release Manifest', 'GB3', 'N'), -- 02 MSN
	(newid(), 'JobDocumentData', getdate(), '2022-10-10 10:00:00', @JddPk02, 'MSN', 'MST=eTerminal Release Manifest', 'GB1', 'N'),
	(newid(), 'JobDocumentData', getdate(), '2022-10-10 10:00:00', @JddPk03, 'MSN', 'MST=eTerminal Release Manifest', 'GB1', 'N'),
	(newid(), 'JobDocumentData', getdate(), '2022-10-10 10:00:00', @JddPk04, 'MSN', 'MST=eTerminal Release Manifest', 'GB2', 'N'),
	(newid(), 'JobDocumentData', getdate(), '2022-10-10 10:00:00', @JddPk05, 'MSN', 'MST=eTerminal Release Manifest', 'GB2', 'N'),

	(newid(), 'NotJobDocumentData', getdate(), '2022-11-12 12:02:00', @JddPk02, 'ISN', '|MST=eTerminal Release Manifest|STA=AMD|LOC=CN002', 'GB0', 'N'),  -- Not JobDocumentData
	(newid(), 'JobDocumentData', getdate(), '2022-11-12 12:02:00', @JddPk02, 'ISN', '|MST=Not eTerminal Release Manifest|STA=AMD|LOC=CN002', 'GB0', 'N');  -- Not eTerminal Release Manifest

INSERT dbo.JobDocumentData (JDD_PK, JDD_ParentID, JDD_ParentTableCode, JDD_SystemCreateTimeUtc, JDD_SystemLastEditTimeUtc, JDD_SystemCreateUser, JDD_SystemLastEditUser) VALUES
	(@JddPk01, @JkPk01, 'JK', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk02, @JkPk02, 'JK', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E');

INSERT dbo.JobConsol (JK_PK,JK_UniqueConsignRef,JK_TransportMode) VALUES
	(@JkPk01,N'JkUniqueConsignRef01','SEA'),
	(@JkPk02,N'JkUniqueConsignRef02','SEA');

INSERT INTO dbo.JobConsolTransport (JW_PK, JW_ParentGUID, JW_TransportMode, JW_LegOrder, JW_Vessel, JW_VoyageFlight) VALUES
	(NEWID(), @JkPk01, N'SEA', 2, N'JV01', N'JVF01'),
	(NEWID(), @JkPk01, N'SEA', 1, N'JV02', N'JVF02'),
	(NEWID(), @JkPk02, N'SEA', 1, N'JV03', N'JVF03'),
	(NEWID(), @JkPk02, N'AIR', 2, N'JV04', N'JVF04');

";

			TestConnection.ExecuteNonQuery(sqlText);
		}
	}
}
