using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(NetherlandsPortStatus))]
	sealed class NetherlandsPortStatusTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => new RecurringRange(new DateTime(2024, 10, 25, 09, 00, 00), new DateTime(2024, 10, 25, 10, 00, 00));

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals(3, transactions.Count());

			var row1 = transactions.Single(x => x.Reference1.Equals("JkUniqueConsignRef05"));
			var row2 = transactions.Single(x => x.Reference1.Equals("JkUniqueConsignRef06"));
			var row3 = transactions.Single(x => x.Reference1.Equals("JkUniqueConsignRef16"));

			CombineAssertions(() =>
			{
				AssertEquals("[Row-1] CompanyCode", "SZG", row1.GetCompanyCode());
				AssertEquals("[Row-1] BranchCode", "GB3", row1.GetBranchCode());
				AssertEquals("[Row-1] TransactionDateUtc", new DateTime(2024, 10, 25, 09, 40, 00), row1.ServiceOccuredUTC);
				AssertEquals("[Row-1] TransactionReference01", "JkUniqueConsignRef05", row1.Reference1);
				AssertEquals("[Row-1] TransactionReference02", "STU - typ05", row1.Reference2);
				AssertEquals("[Row-1] TransactionReference03", "Export Notification", row1.Reference3);
				AssertEquals("[Row-1] TransactionReference04", "crf05", row1.Reference4);

				AssertEquals("[Row-2] CompanyCode", "DAU", row2.GetCompanyCode());
				AssertEquals("[Row-2] BranchCode", "GB0", row2.GetBranchCode());
				AssertEquals("[Row-2] TransactionDateUtc", new DateTime(2024, 10, 25, 09, 40, 00), row2.ServiceOccuredUTC);
				AssertEquals("[Row-2] TransactionReference01", "JkUniqueConsignRef06", row2.Reference1);
				AssertEquals("[Row-2] TransactionReference02", "STU - typ06", row2.Reference2);
				AssertEquals("[Row-2] TransactionReference03", "Export Notification", row2.Reference3);
				AssertEquals("[Row-2] TransactionReference04", "crf06", row2.Reference4);

				AssertEquals("[Row-3] CompanyCode", "SHA", row3.GetCompanyCode());
				AssertEquals("[Row-3] BranchCode", "GB2", row3.GetBranchCode());
				AssertEquals("[Row-3] TransactionDateUtc", new DateTime(2024, 10, 25, 09, 41, 00), row3.ServiceOccuredUTC);
				AssertEquals("[Row-3] TransactionReference01", "JkUniqueConsignRef16", row3.Reference1);
				AssertEquals("[Row-3] TransactionReference02", "STU - typ16", row3.Reference2);
				AssertEquals("[Row-3] TransactionReference03", "Export Notification", row3.Reference3);
				AssertEquals("[Row-3] TransactionReference04", "crf16", row3.Reference4);
			});
		}

		protected override void PrepareTestData()
		{
			var sqlText = $@"
DECLARE @JddPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @JddPk02 UNIQUEIDENTIFIER = NEWID();
DECLARE @JddPk03 UNIQUEIDENTIFIER = NEWID();
DECLARE @JddPk04 UNIQUEIDENTIFIER = NEWID();
DECLARE @JddPk05 UNIQUEIDENTIFIER = NEWID();
DECLARE @JddPk06 UNIQUEIDENTIFIER = NEWID();
DECLARE @JddPk07 UNIQUEIDENTIFIER = NEWID();
DECLARE @JddPk08 UNIQUEIDENTIFIER = NEWID();
DECLARE @JddPk09 UNIQUEIDENTIFIER = NEWID();
DECLARE @JddPk10 UNIQUEIDENTIFIER = NEWID();
DECLARE @JddPk11 UNIQUEIDENTIFIER = NEWID();
DECLARE @JddPk12 UNIQUEIDENTIFIER = NEWID();
DECLARE @JddPk13 UNIQUEIDENTIFIER = NEWID();
DECLARE @JddPk14 UNIQUEIDENTIFIER = NEWID();
DECLARE @JddPk15 UNIQUEIDENTIFIER = NEWID();
DECLARE @JddPk16 UNIQUEIDENTIFIER = NEWID();

DECLARE @JkPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @JkPk02 UNIQUEIDENTIFIER = NEWID();
DECLARE @JkPk03 UNIQUEIDENTIFIER = NEWID();
DECLARE @JkPk04 UNIQUEIDENTIFIER = NEWID();
DECLARE @JkPk05 UNIQUEIDENTIFIER = NEWID();
DECLARE @JkPk06 UNIQUEIDENTIFIER = NEWID();
DECLARE @JkPk07 UNIQUEIDENTIFIER = NEWID();
DECLARE @JkPk08 UNIQUEIDENTIFIER = NEWID();
DECLARE @JkPk09 UNIQUEIDENTIFIER = NEWID();
DECLARE @JkPk10 UNIQUEIDENTIFIER = NEWID();
DECLARE @JkPk11 UNIQUEIDENTIFIER = NEWID();
DECLARE @JkPk12 UNIQUEIDENTIFIER = NEWID();
DECLARE @JkPk13 UNIQUEIDENTIFIER = NEWID();
DECLARE @JkPk14 UNIQUEIDENTIFIER = NEWID();
DECLARE @JkPk15 UNIQUEIDENTIFIER = NEWID();
DECLARE @JkPk16 UNIQUEIDENTIFIER = NEWID();

DECLARE @JcPk01 UNIQUEIDENTIFIER = NEWID();

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
	(NEWID(), 'JobDocumentData', GETDATE(), '2024-10-25 09:01:00', @JddPk01, 'MAA', '|MST=Export Notification|STA=ORG|LOC=CN001|EQN=eqn01|CRF=crf01', 'GB0', 'N'), -- Not SUT
	(NEWID(), 'JobDocumentData', GETDATE(), '2024-10-25 09:30:00', @JddPk02, 'MRJ', '|MST=Import Notification|STA=AMD|LOC=CN002|EQN=eqn02|CRF=crf02', 'GB0', 'N'), -- Not SUT
	(NEWID(), 'JobDocumentData', GETDATE(), '2024-10-25 09:40:00', @JddPk03, 'MWA', '|STA=WTH|LOC=CN003|EQN=eqn03|CRF=crf03|MST=Export Notification', 'GB0', 'Y'), -- Not SUT
	(NEWID(), 'JobDocumentData', GETDATE(), '2024-10-25 09:40:00', @JddPk04, 'MAA', '|STA=ORG|LOC=CN004|EQN=eqn04|CRF=crf04|MST=Import Notification', 'GB0', 'N'), -- Not SUT
	(NEWID(), 'JobDocumentData', GETDATE(), '2024-10-25 09:40:00', @JddPk05, 'STU', '|STA=ORG|LOC=CN005|EQN=eqn05|CRF=crf05|DEP=Portbase|MST=Export Notification|TYP=typ05', 'GB2', 'N'),  -- 01
	(NEWID(), 'JobDocumentData', GETDATE(), '2024-10-25 09:40:00', @JddPk06, 'STU', '|STA=ORG|LOC=CN006|EQN=eqn06|CRF=crf06|DEP=Portbase|TYP=typ06|MST=Export Notification', 'GB0', 'N'),  -- 02

	(NEWID(), 'JobDocumentData', GETDATE(), '2024-10-25 09:40:00', @JddPk07, 'STU', '|STA=ORG|LOC=CN007|EQN=eqn07|CRF=crf07|DEP=Portbase|MST=Import Notification', 'GB0', 'N'),  -- STU Log but MST is Import Notification
	(NEWID(), 'JobDocumentData', GETDATE(), '2024-10-25 09:40:00', @JddPk07, 'STU', '|STA=ORG|LOC=CN007|EQN=eqn07|CRF=crf07|MST=Export Notification', 'GB0', 'N'),  -- STU Log but without DEP=Portbase
	(NEWID(), 'JobDocumentData', GETDATE(), '2024-10-25 09:30:00', @JddPk08, 'XXX', '|MST=Export Notification|STA=ORG|LOC=CN008|EQN=eqn08', 'GB0', 'N'), -- SL_SE_NKEvent is not MAA/MRJ/MWA/STU
	(NEWID(), 'JobDocumentData', GETDATE(), '2024-10-21 09:31:00', @JddPk09, 'MAA', '|MST=Export Notification (755)|STA=AMD|LOC=CN009|EQN=eqn09', 'GB0', 'N'),  -- Export Notification (755)
	(NEWID(), 'JobDocumentData', GETDATE(), '2024-10-21 09:31:00', @JddPk10, 'MAA', '|STA=AMD|LOC=CN010|EQN=eqn10|MST=Export Notification (755)', 'GB0', 'N'),  -- Export Notification (755)
	(NEWID(), 'NotJobDocumentData', GETDATE(), '2024-10-21 09:31:00', @JddPk11, 'MAA', '|STA=AMD|LOC=CN011|EQN=eqn11|MST=Export Notification (755)', 'GB0', 'N'),  -- NotJobDocumentData
	(NEWID(), 'JobDocumentData', GETDATE(), '2024-10-25 09:32:00', @JddPk12, 'MAA', '|MST=Export Notification (EBADEC)|STA=AMD|LOC=CN012|EQN=eqn12', 'GB0', 'N'),  -- Export Notification (EBADEC)
	(NEWID(), 'JobDocumentData', GETDATE(), '2024-10-25 08:59:00', @JddPk13, 'MAA', '|MST=Import Notification|STA=ORG|LOC=CN013|EQN=eqn13', 'GB0', 'N'), -- Before date range
	(NEWID(), 'JobDocumentData', GETDATE(), '2024-10-25 10:01:00', @JddPk14, 'MAA', '|MST=Export Notification|STA=ORG|LOC=CN014|EQN=eqn14', 'GB0', 'N'), -- After date range
	(NEWID(), 'JobDocumentData', GETDATE(), '2024-10-25 09:40:00', @JddPk15, 'STU', '|MST=Export Notification|DEP=Portbase|TYP=Reset to Original', 'GB0', 'N'), -- Reset to Original

	(NEWID(), 'JobContainer', GETDATE(), '2024-10-25 09:41:00', @JcPk01, 'STU', '|STA=ORG|LOC=CN016|EQN=eqn16|CRF=crf16|DEP=Portbase|TYP=typ16|MST=Export Notification', 'GB0', 'N'),  -- 03

	(NEWID(), 'JobDocumentData', GETDATE(), '2024-10-25 08:59:00', @JddPk01, 'MSN', 'MST=Export Notification', 'GB1', 'N'),
	(NEWID(), 'JobDocumentData', GETDATE(), '2024-10-25 09:00:00', @JddPk01, 'MSN', 'MST=Export Notification', 'GB0', 'N'),
	(NEWID(), 'JobDocumentData', GETDATE(), '2024-10-25 09:02:00', @JddPk01, 'MSN', 'MST=Export Notification', 'GB2', 'N'),
	(NEWID(), 'JobDocumentData', GETDATE(), '2024-10-25 08:00:00', @JddPk02, 'MSN', 'MST=Import Notification', 'GB1', 'N'),
	(NEWID(), 'JobDocumentData', GETDATE(), '2024-10-25 08:00:00', @JddPk03, 'MWR', 'MST=Export Notification', 'GB2', 'Y'),
	(NEWID(), 'JobDocumentData', GETDATE(), '2024-10-25 08:00:00', @JddPk04, 'MSN', 'MST=Import Notification', 'GB3', 'N'),
	(NEWID(), 'JobDocumentData', GETDATE(), '2024-10-25 07:00:00', @JddPk05, 'MSN', 'MST=Export Notification', 'GB0', 'N'),
	(NEWID(), 'JobDocumentData', GETDATE(), '2024-10-25 08:00:00', @JddPk05, 'MSN', 'MST=Export Notification', 'GB3', 'N'), -- 01 STU

	(NEWID(), 'JobDocumentData', GETDATE(), '2024-10-25 08:00:00', @JddPk16, 'MSN', 'MST=Export Notification', 'GB2', 'N'); -- 03 STU

INSERT dbo.JobDocumentData (JDD_PK, JDD_ParentID, JDD_ParentTableCode, JDD_Name, JDD_SystemCreateTimeUtc, JDD_SystemLastEditTimeUtc, JDD_SystemCreateUser, JDD_SystemLastEditUser) VALUES
	(@JddPk01, @JkPk01, 'JK', 'PortbaseExportNotification', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk02, @JkPk02, 'JK', 'PortbaseExportNotification', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk03, @JkPk03, 'JK', 'PortbaseExportNotification', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk04, @JkPk04, 'JK', 'PortbaseExportNotification', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk05, @JkPk05, 'JK', 'PortbaseExportNotification', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk06, @JkPk06, 'JK', 'PortbaseExportNotification', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk07, @JkPk07, 'JK', 'PortbaseExportNotification', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk08, @JkPk08, 'JK', 'PortbaseExportNotification', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk09, @JkPk09, 'JK', 'PortbaseExportNotification', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk10, @JkPk10, 'JK', 'PortbaseExportNotification', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk11, @JkPk11, 'JK', 'PortbaseExportNotification', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk12, @JkPk12, 'JK', 'PortbaseExportNotification', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk13, @JkPk13, 'JK', 'PortbaseExportNotification', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk14, @JkPk14, 'JK', 'PortbaseExportNotification', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk15, @JkPk15, 'JK', 'PortbaseExportNotification', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk16, @JkPk16, 'JK', 'PortbaseExportNotification', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E');

INSERT dbo.JobConsol (JK_PK,JK_UniqueConsignRef,JK_TransportMode,JK_BookingReference) VALUES
	(@JkPk01, N'JkUniqueConsignRef01', 'SEA', 'BookingReference01'),
	(@JkPk02, N'JkUniqueConsignRef02', 'SEA', 'BookingReference02'),
	(@JkPk03, N'JkUniqueConsignRef03', 'SEA', 'BookingReference03'),
	(@JkPk04, N'JkUniqueConsignRef04', 'SEA', 'BookingReference04'),
	(@JkPk05, N'JkUniqueConsignRef05', 'SEA', 'BookingReference05'),
	(@JkPk06, N'JkUniqueConsignRef06', 'SEA', 'BookingReference06'),
	(@JkPk07, N'JkUniqueConsignRef07', 'SEA', 'BookingReference07'),
	(@JkPk08, N'JkUniqueConsignRef08', 'SEA', 'BookingReference08'),
	(@JkPk09, N'JkUniqueConsignRef09', 'SEA', 'BookingReference09'),
	(@JkPk10, N'JkUniqueConsignRef10', 'SEA', 'BookingReference10'),
	(@JkPk11, N'JkUniqueConsignRef11', 'SEA', 'BookingReference11'),
	(@JkPk12, N'JkUniqueConsignRef12', 'SEA', 'BookingReference12'),
	(@JkPk13, N'JkUniqueConsignRef13', 'SEA', 'BookingReference13'),
	(@JkPk14, N'JkUniqueConsignRef14', 'SEA', 'BookingReference14'),
	(@JkPk15, N'JkUniqueConsignRef15', 'SEA', 'BookingReference14'),
	(@JkPk16, N'JkUniqueConsignRef16', 'SEA', 'BookingReference14');

INSERT dbo.JobContainer (JC_PK, JC_JK) VALUES
	(@JcPk01, @JkPk16);
";

			TestConnection.ExecuteNonQuery(sqlText);
		}
	}
}
