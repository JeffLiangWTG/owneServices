using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(CaptureNZOCRManifestsBills))]
	sealed class CaptureNZOCRManifestsBillsTests : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @GcPkAU UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkNZ UNIQUEIDENTIFIER = newid();

				DECLARE @GbPkAU UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkNZ UNIQUEIDENTIFIER = newid();

				DECLARE @AblPk1 UNIQUEIDENTIFIER = newid();
				DECLARE @AblPk2 UNIQUEIDENTIFIER = newid();
				DECLARE @AblPk3 UNIQUEIDENTIFIER = newid();
				DECLARE @AblPk4 UNIQUEIDENTIFIER = newid();
				DECLARE @AblPk5 UNIQUEIDENTIFIER = newid();
				DECLARE @AblPk6 UNIQUEIDENTIFIER = newid();
				DECLARE @AblPk7 UNIQUEIDENTIFIER = newid();
				DECLARE @AblPk8 UNIQUEIDENTIFIER = newid();

				INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES 
					(@GcPkAU, 'AUC', 'AU company', 'AU', 'AUD'),
					(@GcPkNZ, 'NZC', 'NZ company', 'NZ', 'NZD');

				INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES 
					(@GbPkAU, 'AUB', @GcPkAU),
					(@GbPkNZ, 'NZB', @GcPkNZ);

				INSERT dbo.AsycudaManifestHeader (AMA_PK, AMA_JobReference, AMA_ManifestType, AMA_ApplicationCode, AMA_SystemCreateTimeUtc, AMA_SystemLastEditTimeUtc, AMA_SystemCreateUser, AMA_SystemLastEditUser, AMA_ClusterKey, AMA_RN_NKCountry, AMA_GB) VALUES
				(0x1, 'C0000ABC1', 'IAM', 'OUT', '2022-08-29', '2022-08-29', 'BOB', 'JOE', 21, 'AU', @GbPkAU),
				(0x2, 'C0000ABC2', 'OCR', 'OUT', '2022-08-29', '2022-08-29', 'BOB', 'JOE', 22, 'AU', @GbPkAU),
				(0x3, 'C0000ABC3', 'IAM', 'OUT', '2022-08-29', '2022-08-29', 'BOB', 'JOE', 23, 'NZ', @GbPkNZ),
				(0x4, 'C0000ABC4', 'OCR', 'OUT', '2022-08-29', '2022-08-29', 'BOB', 'JOE', 24, 'NZ', @GbPkNZ),
				(0x5, 'C0000ABC5', 'IAM', 'OUT', '2022-08-29', '2022-08-29', 'BOB', 'JOE', 25, 'NZ', @GbPkNZ),
				(0x6, 'C0000ABC6', 'OCR', 'OUT', '2022-10-29', '2022-10-29', 'BOB', 'JOE', 26, 'NZ', @GbPkNZ);

				INSERT dbo.AsycudaBill (ABL_PK, ABL_AMA, ABL_BolType, ABL_BillNumber, ABL_SystemCreateTimeUtc, ABL_SystemLastEditTimeUtc, ABL_SystemCreateUser, ABL_SystemLastEditUser, ABL_ClusterKey) VALUES
				(@AblPk1, 0x1, 'BOL', 'BN001', '2022-08-29', '2022-08-29', 'BOB', 'JOE', 31),
				(@AblPk2, 0x1, 'STD', 'BN002', '2022-08-29', '2022-08-29', 'BOB', 'JOE', 32),
				(@AblPk3, 0x2, 'BOL', 'BN003', '2022-08-29', '2022-08-29', 'BOB', 'JOE', 33),
				(@AblPk4, 0x2, 'STD', 'BN004', '2022-08-29', '2022-08-29', 'BOB', 'JOE', 34),
				(@AblPk5, 0x3, 'BOL', 'BN005', '2022-08-29', '2022-08-29', 'BOB', 'JOE', 35),
				(@AblPk6, 0x4, 'BOL', 'BN006', '2022-08-29', '2022-08-29', 'BOB', 'JOE', 36),
				(@AblPk7, 0x5, 'STD', 'BN007', '2022-08-29', '2022-08-29', 'BOB', 'JOE', 37),
				(@AblPk8, 0x6, 'BOL', 'BN008', '2022-10-29', '2022-10-29', 'BOB', 'JOE', 38)";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 1, transactions.Count());

			var transaction1 = transactions.First();
			AssertEquals("[T1] CompanyCode", "NZC", transaction1.GetCompanyCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2022, 8, 29), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] TransactionReference01", "Manifest: C0000ABC4", transaction1.Reference1);
			AssertEquals("[T1] TransactionReference02", "MAWB: BN006", transaction1.Reference2);
			AssertEquals("[T1] TransactionReference03", null, transaction1.Reference3);
			AssertEquals("[T1] TransactionReference04", null, transaction1.Reference4);
			AssertEquals("[T1] TransactionGuidReference", "00000004-0000-0000-0000-000000000000", transaction1.Reference5);
			AssertEquals("[T1] BranchCode", "NZB", transaction1.GetBranchCode());
			AssertEquals("[T1] UserCode", "BOB", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2022, 8);
	}
}
