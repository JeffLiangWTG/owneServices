using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(CaptureLATAMBillingCount))]
	sealed class CaptureBillingCountFromInsertRefSTLScriptTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2022, 8);

		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @GcPkUY UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkAU UNIQUEIDENTIFIER = newid();

				DECLARE @GbPkUY UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkAU UNIQUEIDENTIFIER = newid();

				DECLARE @AmaPk1 UNIQUEIDENTIFIER = newid();
				DECLARE @AmaPk2 UNIQUEIDENTIFIER = newid();
				DECLARE @AmaPk3 UNIQUEIDENTIFIER = newid();
				DECLARE @AmaPk4 UNIQUEIDENTIFIER = newid();
				DECLARE @AmaPk5 UNIQUEIDENTIFIER = newid();
				DECLARE @AmaPk6 UNIQUEIDENTIFIER = newid();

				INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode) VALUES 
					(@GcPkUY, 'UYC', 'UY company', 'UY'),
					(@GcPkAU, 'AUC', 'AU company', 'AU');

				INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES 
					(@GbPkUY, 'UYB', @GcPkUY),
					(@GbPkAU, 'AUB', @GcPkAU);

				INSERT dbo.AsycudaManifestHeader (AMA_PK, AMA_JobReference, AMA_ManifestType, AMA_SystemCreateTimeUtc, AMA_SystemLastEditTimeUtc, AMA_SystemCreateUser, AMA_SystemLastEditUser, AMA_ClusterKey, AMA_RN_NKCountry, AMA_GB) VALUES
				(@AmaPk1, 'C0000ABC1', 'MAN', '2022-08-01', '2022-08-01', 'BOB', 'JOE', 1, 'UY', @GbPkUY),
				(@AmaPk2, 'C0000ABC2', 'MAN', '2022-08-01', '2022-08-01', 'BOB', 'JOE', 2, 'UY', @GbPkUY),
				(@AmaPk3, 'C0000ABC3', 'MAN', '2022-08-01', '2022-08-01', 'BOB', 'JOE', 3, 'UY', @GbPkUY),
				(@AmaPk4, 'C0000ABC4', 'IAM', '2022-08-01', '2022-08-01', 'BOB', 'JOE', 4, 'UY', @GbPkUY),
				(@AmaPk5, 'C0000ABC5', 'MAN', '2022-08-01', '2022-08-01', 'BOB', 'JOE', 5, 'AU', @GbPkAU),
				(@AmaPk6, 'C0000ABC6', 'MAN', '2022-09-01', '2022-09-01', 'BOB', 'JOE', 6, 'UY', @GbPkUY);

				INSERT dbo.AsycudaBill (ABL_PK, ABL_AMA, ABL_BolType, ABL_BillNumber, ABL_SystemCreateTimeUtc, ABL_SystemLastEditTimeUtc, ABL_SystemCreateUser, ABL_SystemLastEditUser, ABL_ClusterKey) VALUES
				(0x1, @AmaPk1, 'BOL', 'BN001', '2022-08-01', '2022-08-01', 'BOB', 'JOE', 1),
				(0x2, @AmaPk1, 'STD', 'BN002', '2022-08-01', '2022-08-01', 'BOB', 'JOE', 2),
				(0x3, @AmaPk2, 'STD', 'BN003', '2022-08-01', '2022-08-01', 'BOB', 'JOE', 3),
				(0x4, @AmaPk3, 'CLD', 'BN004', '2022-08-01', '2022-08-01', 'BOB', 'JOE', 4),
				(0x5, @AmaPk3, 'BOL', 'BN005', '2022-08-01', '2022-08-01', 'BOB', 'JOE', 5),
				(0x6, @AmaPk4, 'STD', 'BN006', '2022-08-01', '2022-08-01', 'BOB', 'JOE', 6),
				(0x7, @AmaPk4, 'BOL', 'BN007', '2022-08-01', '2022-08-01', 'BOB', 'JOE', 7),
				(0x8, @AmaPk5, 'STD', 'BN008', '2022-08-01', '2022-08-01', 'BOB', 'JOE', 8),
				(0x9, @AmaPk5, 'BOL', 'BN009', '2022-08-01', '2022-08-01', 'BOB', 'JOE', 9),
				(0x10, @AmaPk6, 'BOL', 'BN010', '2022-09-01', '2022-09-01', 'BOB', 'JOE', 10),
				(0x11, @AmaPk6, 'STD', 'BN011', '2022-09-01', '2022-09-01', 'BOB', 'JOE', 11);";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of LATAM Billing Count", 1, transactions.Count());

			var transaction1 = transactions.First();
			AssertEquals("CompanyCode", "UYC", transaction1.GetCompanyCode());
			AssertEquals("BranchCode", "UYB", transaction1.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2022, 8, 1), transaction1.ServiceOccuredUTC);
			AssertEquals("TransactionReference01", "C0000ABC1", transaction1.Reference1);
			AssertEquals("TransactionReference02", "MAWB: BN001", transaction1.Reference2);
			AssertEquals("TransactionReference03", "HAWB: BN002", transaction1.Reference3);
			AssertEquals("TransactionReference04", "UY", transaction1.Reference4);
			AssertEquals("TransactionGuidReference", "00000002-0000-0000-0000-000000000000", transaction1.Reference5);
			AssertEquals("UserCode", "BOB", transaction1.ClientStaffCode);
		}

		public void TestFilterCountries()
		{
			AssertContains("am.AMA_RN_NKCountry IN ('UY', 'MX', 'CL', 'AR', 'CO', 'BR', 'CR', 'PA', 'PY', 'PE', 'DO', 'EC')", ScriptToTest.ScriptText);
		}
	}
}
