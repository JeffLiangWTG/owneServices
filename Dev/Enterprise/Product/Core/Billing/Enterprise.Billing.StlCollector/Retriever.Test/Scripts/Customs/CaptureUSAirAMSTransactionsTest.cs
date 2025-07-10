using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(CaptureUSAirAMSTransactions))]
	sealed class CaptureUSAirAMSTransactionsTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			var sqlText = @"
DECLARE @GcPkUS UNIQUEIDENTIFIER = newid();

DECLARE @GbPkUS UNIQUEIDENTIFIER = newid();

INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency)
VALUES
(@GcPkUS, 'USC', 'US company', 'US', 'USD')

INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC)
VALUES
(@GbPkUS, 'USB', @GcPkUS)

INSERT dbo.AsycudaManifestHeader (AMA_PK, AMA_JobReference, AMA_ManifestType, AMA_ApplicationCode, AMA_SystemCreateTimeUtc, AMA_SystemLastEditTimeUtc, AMA_SystemCreateUser, AMA_SystemLastEditUser, AMA_ClusterKey, AMA_RN_NKCountry, AMA_GB)
VALUES
(0x1, 'MAN00000010', 'IAM', 'NVC', '2023-09-19', '2023-09-20', 'BOB', 'JOE', 10, 'US', @GbPkUS),
(0x2, 'MAN00000020', 'MAN', 'NVC', '2023-09-19', '2023-09-20', 'BOB', 'JOE', 20, 'US', @GbPkUS),
(0x3, 'MAN00000030', 'IAM', 'NVC', '2023-09-19', '2023-09-20', 'BOB', 'JOE', 30, 'US', @GbPkUS),
(0x4, 'MAN00000040', 'IAM', 'NVC', '2023-09-19', '2023-09-20', 'BOB', 'JOE', 40, 'US', @GbPkUS),
(0x5, 'MAN00000050', 'IAM', 'NVC', '2023-09-20', '2023-09-21', 'BOB', 'JOE', 50, 'US', @GbPkUS)

INSERT dbo.AsycudaBill (ABL_PK, ABL_AMA, ABL_BolType, ABL_BillNumber, ABL_SystemCreateTimeUtc, ABL_SystemLastEditTimeUtc, ABL_SystemCreateUser, ABL_SystemLastEditUser, ABL_ClusterKey)
VALUES
(0x1, 0x1, 'BOL', 'BN001', '2023-09-20', '2023-09-20', 'BOB', 'JOE', 10),
(0x2, 0x1, 'STD', 'BN002', '2023-09-20', '2023-09-20', 'BOB', 'JOE', 20),
(0x3, 0x2, 'BOL', 'BN003', '2023-09-20', '2023-09-20', 'BOB', 'JOE', 30),
(0x4, 0x3, 'STD', 'BN004', '2023-09-20', '2023-09-20', 'BOB', 'JOE', 40),
(0x5, 0x4, 'BOL', 'BN005', '2023-09-20', '2023-09-20', 'BOB', 'JOE', 50),
(0x6, 0x5, 'BOL', 'BN006', '2023-09-21', '2023-09-21', 'BOB', 'JOE', 60)

INSERT dbo.StmALog (SL_PK, SL_Parent, SL_Table, SL_Reference, SL_IsCancelled, SL_SE_NKEvent, SL_PostedTimeUtc, SL_EventTime)
VALUES
(0x1, 0x4, 'AsycudaManifestHeader', 'TYP=HVL', 'N', 'TRF', '2023-09-20', '2023-09-20'),
(0x2, 0x5, 'Non-AsycudaManifestHeader', 'TYP=HVL', 'N', 'TRF', '2023-09-21', '2023-09-21'),
(0x3, 0x5, 'AsycudaManifestHeader', '', 'N', 'TRF', '2023-09-21', '2023-09-21'),
(0x4, 0x5, 'AsycudaManifestHeader', 'TYP=HVL', 'Y', 'TRF', '2023-09-21', '2023-09-21'),
(0x5, 0x5, 'AsycudaManifestHeader', 'TYP=HVL', 'N', 'NAN', '2023-09-21', '2023-09-21')";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 2, transactions.Count());

			var transaction1 = FindRowByRef1(transactions, "MAN00000010");
			AssertEquals("[T1] CompanyCode", "USC", transaction1.GetCompanyCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2023, 09, 19), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] TransactionReference02", "MAWB: BN001", transaction1.Reference2);
			AssertEquals("[T1] TransactionReference03", null, transaction1.Reference3);
			AssertEquals("[T1] TransactionReference04", null, transaction1.Reference4);
			AssertEquals("[T1] TransactionGuidReference", "00000001-0000-0000-0000-000000000000", transaction1.Reference5);
			AssertEquals("[T1] BranchCode", "USB", transaction1.GetBranchCode());
			AssertEquals("[T1] UserCode", "BOB", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);

			var transaction2 = FindRowByRef1(transactions, "MAN00000050");
			AssertEquals("[T2] CompanyCode", "USC", transaction2.GetCompanyCode());
			AssertEquals("[T2] TransactionDateUtc", new DateTime(2023, 09, 20), transaction2.ServiceOccuredUTC);
			AssertEquals("[T2] TransactionReference02", "MAWB: BN006", transaction2.Reference2);
			AssertEquals("[T2] TransactionReference03", null, transaction2.Reference3);
			AssertEquals("[T2] TransactionReference04", null, transaction2.Reference4);
			AssertEquals("[T2] TransactionGuidReference", "00000005-0000-0000-0000-000000000000", transaction2.Reference5);
			AssertEquals("[T2] BranchCode", "USB", transaction2.GetBranchCode());
			AssertEquals("[T2] UserCode", "BOB", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2023, 9);
	}
}
