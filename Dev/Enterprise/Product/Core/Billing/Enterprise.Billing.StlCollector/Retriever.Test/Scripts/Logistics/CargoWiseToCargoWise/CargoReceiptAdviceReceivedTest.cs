using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(CargoReceiptAdviceReceived))]
	sealed class CargoReceiptAdviceReceivedTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2023, 04);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals(2, transactions.Count());

			AssertNull("MRR event is before DateTime range", transactions.FirstOrDefault(x => x.Reference1 == "JS02"));
			AssertNull("MRR event is after DateTime range", transactions.FirstOrDefault(x => x.Reference1 == "JS03"));
			AssertNull("This is XXX event", transactions.FirstOrDefault(x => x.Reference1 == "JS04"));

			var row0 = transactions.Single(x => x.Reference1.StartsWith("JS01"));
			var row1 = transactions.Single(x => x.Reference1.StartsWith("JS05"));

			CombineAssertions(() =>
			{
				AssertEquals("[Row-0] CompanyCode", "DAU", row0.GetCompanyCode());
				AssertEquals("[Row-0] BranchCode", "GB1", row0.GetBranchCode());
				AssertEquals("[Row-0] TransactionDateUtc", new DateTime(2023, 04, 01, 12, 01, 00), row0.ServiceOccuredUTC);
				AssertEquals("[Row-0] ItemCount", 1, row0.BillableCount);
				AssertEquals("[Row-0] TransactionReference01", "JS01", row0.Reference1);
				AssertEquals("[Row-0] TransactionReference02", "REF01, REF05", row0.Reference2);
				AssertEquals("[Row-0] TransactionReference03", "OrgCusCode1", row0.Reference3);
				AssertEquals("[Row-0] TransactionReference04", "Cargo Receipt Advice - JinTianChiBaoLeMa:)", row0.Reference4);

				AssertEquals("[Row-1] CompanyCode", "SHA", row1.GetCompanyCode());
				AssertEquals("[Row-1] BranchCode", "GB2", row1.GetBranchCode());
				AssertEquals("[Row-1] TransactionDateUtc", new DateTime(2023, 04, 12, 12, 02, 00), row1.ServiceOccuredUTC);
				AssertEquals("[Row-1] ItemCount", 1, row1.BillableCount);
				AssertEquals("[Row-1] TransactionReference01", "JS05", row1.Reference1);
				AssertEquals("[Row-1] TransactionReference02", "REF06", row1.Reference2);
				AssertEquals("[Row-1] TransactionReference03", "OrgCusCode5", row1.Reference3);
				AssertEquals("[Row-1] TransactionReference04", "Cargo Receipt Advice", row1.Reference4);
			});
		}

		protected override void PrepareTestData()
		{
			var sqlText = @"DECLARE @JddPk01 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk02 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk03 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk04 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk05 UNIQUEIDENTIFIER = newid();

DECLARE @JsPk01 UNIQUEIDENTIFIER = newid();
DECLARE @JsPk02 UNIQUEIDENTIFIER = newid();
DECLARE @JsPk03 UNIQUEIDENTIFIER = newid();
DECLARE @JsPk04 UNIQUEIDENTIFIER = newid();
DECLARE @JsPk05 UNIQUEIDENTIFIER = newid();

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

INSERT dbo.OrgHeader (OH_PK, OH_Code,OH_FullName) VALUES
	(@OhPk01,'OH1','OrgHeader1'),
	(@OhPk02,'OH2','OrgHeader2'),
	(@OhPk03,'OH3','OrgHeader3'),
	(@OhPk04,'OH4','OrgHeader4'),
	(@OhPk05,'OH5','OrgHeader5');

INSERT dbo.OrgAddress (OA_PK, OA_OH, OA_Address1) VALUES
	(@OaPk01 ,@OhPk01, 'Address01'),
	(@OaPk02 ,@OhPk02, 'Address02'),
	(@OaPk03 ,@OhPk03, 'Address03'),
	(@OaPk04 ,@OhPk04, 'Address04'),
	(@OaPk05 ,@OhPk05, 'Address05');

INSERT dbo.OrgCusCode (OK_PK, OK_CustomsRegNo, OK_CodeType, OK_RN_NKCodeCountry, OK_OH) VALUES
	(newid(), 'OrgCusCode1', 'C1C', 'AU', @OhPk01),
	(newid(), 'OrgCusCode2', 'C1C', 'AU', @OhPk02),
	(newid(), 'OrgCusCode3', 'C1C', 'AU', @OhPk03),
	(newid(), 'OrgCusCode4', 'C1C', 'AU', @OhPk04),
	(newid(), 'OrgCusCode5', 'C1C', 'AU', @OhPk05);

INSERT dbo.StmALog (SL_PK, SL_Table, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_SE_NKEvent, SL_Reference, SL_GB_NKBranch, SL_IsCancelled) VALUES
	(newid(), 'JobShipment', getdate(), '2023-04-01 12:01:00', @JsPk01, 'MRR', 'from Carrier|MST=Cargo Receipt Advice|MSB=JinTianChiBaoLeMa:)', 'GB0', 'Y'), -- 00
	(newid(), 'JobShipment', getdate(), '2023-04-12 12:02:00', @JsPk05, 'MRR', 'from Carrier|MST=Cargo Receipt Advice', 'GB0', 'N'),  -- 01
	(newid(), 'JobShipment', getdate(), '2023-03-01 12:03:00', @JsPk02, 'MRR', 'from Carrier|MST=Cargo Receipt Advice', 'GB0', 'N'), -- Before date range
	(newid(), 'JobShipment', getdate(), '2023-05-01 12:04:00', @JsPk03, 'MRR', 'from Carrier|MST=Cargo Receipt Advice', 'GB0', 'N'), -- After date range
	(newid(), 'JobShipment', getdate(), '2023-04-01 12:05:00', @JsPk04, 'XXX', 'from Carrier|MST=Cargo Receipt Advice', 'GB0', 'N'), -- SL_SE_NKEvent is not MRR

	(newid(), 'JobDocumentData', getdate(), '2023-04-01 10:00:00', @JddPk01, 'MSN', 'MST=Booking Request', 'GB1', 'N'), -- 00 MSN
	(newid(), 'JobAlien', getdate(), '2023-04-01 10:11:01', @JddPk01, 'MSN', 'MST=Booking Request', 'GB2', 'N'),
	(newid(), 'JobDocumentData', getdate(), '2023-04-10 10:00:00', @JddPk05, 'MSN', 'MST=Booking Request', 'GB2', 'Y'), -- 01 MSN
	(newid(), 'JobDocumentData', getdate(), '2023-03-01 10:00:00', @JddPk01, 'MSN', 'MST=Booking Request', 'GB3', 'N'),
	(newid(), 'JobDocumentData', getdate(), '2023-05-01 10:00:00', @JddPk02, 'MSN', 'MST=Booking Request', 'GB1', 'N'),
	(newid(), 'JobDocumentData', getdate(), '2023-04-01 10:00:00', @JddPk03, 'MSN', 'MST=Booking Request', 'GB1', 'N'),
	(newid(), 'JobDocumentData', getdate(), '2023-04-10 10:00:00', @JddPk04, 'MSN', 'MST=Booking Request', 'GB2', 'N');

INSERT dbo.JobDocumentData (JDD_PK, JDD_ParentID, JDD_ParentTableCode, JDD_Name, JDD_SystemCreateTimeUtc, JDD_SystemLastEditTimeUtc, JDD_SystemCreateUser, JDD_SystemLastEditUser) VALUES
	(@JddPk01, @JsPk01, 'JS', 'BookingRequest', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(newid(), @JsPk01, 'JS', 'JddAlien', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk02, @JsPk02, 'JS', 'BookingRequest', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk03, @JsPk03, 'JS', 'BookingRequest', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk04, @JsPk04, 'JS', 'BookingRequest', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk05, @JsPk05, 'JS', 'BookingRequest', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E');

INSERT dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_OA_BookedShippingLineAddress) VALUES
	(@JsPk01, 'JS01', @OaPk01),
	(@JsPk02, 'JS02', @OaPk02),
	(@JsPk03, 'JS03', @OaPk03),
	(@JsPk04, 'JS04', @OaPk04),
	(@JsPk05, 'JS05', @OaPk05);

INSERT dbo.CusEntryNum (CE_PK, CE_ParentTable, CE_ParentID, CE_EntryType, CE_EntryNum, CE_IsValid, CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser) VALUES
	(NEWID(), 'JobShipment', @JsPk01, 'BKG', 'REF01', 1, '2023-01-01', 'E', '2023-01-01', 'E'),
	(NEWID(), 'JobShipment', @JsPk02, 'BKG', 'REF02', 1, '2023-01-01', 'E', '2023-01-01', 'E'),
	(NEWID(), 'JobShipment', @JsPk03, 'BKG', 'REF03', 1, '2023-01-01', 'E', '2023-01-01', 'E'),
	(NEWID(), 'JobShipment', @JsPk04, 'BKG', 'REF04', 1, '2023-01-01', 'E', '2023-01-01', 'E'),
	(NEWID(), 'JobShipment', @JsPk01, 'BKG', 'REF05', 1, '2023-01-01', 'E', '2023-01-01', 'E'),
	(NEWID(), 'JobShipment', @JsPk05, 'BKG', 'REF06', 1, '2023-01-01', 'E', '2023-01-01', 'E');";

			TestConnection.ExecuteNonQuery(sqlText);
		}
	}
}
