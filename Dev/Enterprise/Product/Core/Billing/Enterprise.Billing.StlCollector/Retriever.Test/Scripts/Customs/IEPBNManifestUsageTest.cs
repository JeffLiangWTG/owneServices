using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(IEPBNManifestUsage))]
	sealed class IEPBNManifestUsageTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2024, 8);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("IE PBN Manifests for Billing Count", 4, transactions.Count());

			var transaction1 = transactions.FirstOrDefault(t => t.Reference1 == "MAN00001000");
			AssertNotNull(transaction1);
			AssertTransaction(transaction1, "IEC", "IEB", new DateTime(2024, 8, 1), pk1);

			var transaction2 = transactions.FirstOrDefault(t => t.Reference1 == "MAN00001002");
			AssertNotNull(transaction2);
			AssertTransaction(transaction2, "IEC", "IEB", new DateTime(2024, 8, 22), pk2);

			var transaction3 = transactions.FirstOrDefault(t => t.Reference1 == "MAN00001005");
			AssertNotNull(transaction3);
			AssertTransaction(transaction3, "IEC", "IEB", new DateTime(2024, 8, 31), pk3);

			var transaction4 = transactions.FirstOrDefault(t => t.Reference1 == "MAN00001006");
			AssertNotNull(transaction4);
			AssertTransaction(transaction4, "IEC", "IEB", new DateTime(2024, 8, 1), pk4);
		}

		void AssertTransaction(IStlTransaction transaction, string expectedCompanyCode, string expectedBranchCode, DateTime expectedDate, string expectedRef5)
		{
			CombineAssertions($"Transaction Ref 1: {transaction.Reference1}", () =>
			{
				AssertEquals("CompanyCode", expectedCompanyCode, transaction.GetCompanyCode());
				AssertEquals("BranchCode", expectedBranchCode, transaction.GetBranchCode());
				AssertEquals("TransactionDateUtc", expectedDate, transaction.ServiceOccuredUTC);
				AssertEquals("TransactionGuidReference", expectedRef5, transaction.Reference5);
			});
		}

		protected override void PrepareTestData()
		{
			var sqlText = $@"
				DECLARE @GcPkIe UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkGB UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkAU UNIQUEIDENTIFIER = newid();
				
				DECLARE @GbPkIe UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkGB UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkAU UNIQUEIDENTIFIER = newid();

				INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode) VALUES 
					(@GcPkIe, 'IEC', 'IE company', 'IE'),
					(@GcPkGB, 'GBC', 'GB company', 'GB'),
					(@GcPkAU, 'AUC', 'AU company', 'AU');

				INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES 
					(@GbPkIe, 'IEB', @GcPkIe),
					(@GbPkGB, 'GBB', @GcPkGB),
					(@GbPkAU, 'AUB', @GcPkAU);

				INSERT dbo.AsycudaManifestHeader (AMA_PK, AMA_JobReference, AMA_ManifestType, AMA_SystemCreateTimeUtc, AMA_SystemLastEditTimeUtc, AMA_SystemCreateUser, AMA_SystemLastEditUser, AMA_ClusterKey, AMA_RN_NKCountry, AMA_GB, AMA_ISActive) VALUES
				('{pk1}', 'MAN00001000', 'PBN', '2024-08-01', '2024-08-01', 'BOB', 'JOE', 1, 'IE', @GbPkIe, 1), -- valid for billing
				(newid(), 'MAN00001001', 'GVM', '2024-08-01', '2024-08-01', 'BOB', 'JOE', 2, 'GB', @GbPkGB, 1), -- wrong manifest type (GVMS)
				('{pk2}', 'MAN00001002', 'PBN', '2024-08-22', '2024-08-22', 'BOB', 'JOE', 3, 'IE', @GbPkIe, 1), -- valid for billing
				(newid(), 'MAN00001003', 'XXX', '2024-08-01', '2024-08-01', 'BOB', 'JOE', 4, 'IE', @GbPkIe, 1), -- wrong manifest type
				(newid(), 'MAN00001004', 'PBN', '2024-08-01', '2024-08-01', 'BOB', 'JOE', 5, 'AU', @GbPkIe, 1), -- wrong country
				('{pk3}', 'MAN00001005', 'PBN', '2024-08-31', '2024-08-31', 'BOB', 'JOE', 6, 'IE', @GbPkIe, 1), -- valid for billing
				('{pk4}', 'MAN00001006', 'PBN', '2024-08-01', '2024-08-01', 'BOB', 'JOE', 7, 'IE', @GbPkIe, 0), -- not active
				(newid(), 'MAN00001007', 'PBN', '2024-09-01', '2024-09-01', 'BOB', 'JOE', 8, 'IE', @GbPkIe, 1); -- wrong month";

			_ = TestConnection.ExecuteNonQuery(sqlText);
		}

		readonly string pk1 = "C3185AC1-2DE3-41CF-B928-7E892E64FF65";
		readonly string pk2 = "7560BC07-93D8-480A-A8C8-E599E9AAF8CF";
		readonly string pk3 = "3617A148-151D-433C-80A9-84BBB50D8123";
		readonly string pk4 = "CE61841D-74E3-4517-8BE1-4AB55C6BA2B8";
	}
}
