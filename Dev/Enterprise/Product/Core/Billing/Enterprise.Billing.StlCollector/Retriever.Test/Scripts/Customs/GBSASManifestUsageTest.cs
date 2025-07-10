using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(GBSASManifestUsage))]
	sealed class GBSASManifestUsageTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2022, 8);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("GB Safety and Security Usage Count", 1, transactions.Count());

			var transaction1 = transactions.First();
			AssertEquals("CompanyCode", "GBC", transaction1.GetCompanyCode());
			AssertEquals("BranchCode", "GBB", transaction1.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2022, 8, 1), transaction1.ServiceOccuredUTC);
			AssertEquals("TransactionReference01", "C0000ABC1", transaction1.Reference1);
			AssertEquals("TransactionReference02", "S&S", transaction1.Reference2);
			AssertEquals("TransactionGuidReference", amaPk01, transaction1.Reference5);
		}

		protected override void PrepareTestData()
		{
			string sqlText = $@"DECLARE @GcPkGB UNIQUEIDENTIFIER = newid();
								DECLARE @GcPkAU UNIQUEIDENTIFIER = newid();

								DECLARE @GbPkGB UNIQUEIDENTIFIER = newid();
								DECLARE @GbPkAU UNIQUEIDENTIFIER = newid();

								INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode) VALUES 
									(@GcPkGB, 'GBC', 'GB company', 'GB'),
									(@GcPkAU, 'AUC', 'AU company', 'AU');

								INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES 
									(@GbPkGB, 'GBB', @GcPkGB),
									(@GbPkAU, 'AUB', @GcPkAU);

								INSERT dbo.AsycudaManifestHeader (AMA_PK, AMA_JobReference, AMA_ManifestType, AMA_SystemCreateTimeUtc, AMA_SystemLastEditTimeUtc, AMA_SystemCreateUser, AMA_SystemLastEditUser, AMA_ClusterKey, AMA_RN_NKCountry, AMA_GB) VALUES
								('{amaPk01}', 'C0000ABC1', 'S&S', '2022-08-01', '2022-08-01', 'BOB', 'JOE', 1, 'GB', @GbPkGB),
								(newid(), 'C0000ABC2', 'XXX', '2022-08-01', '2022-08-01', 'BOB', 'JOE', 2, 'GB', @GbPkGB),
								(newid(), 'C0000ABC3', 'S&S', '2022-08-01', '2022-08-01', 'BOB', 'JOE', 3, 'AU', @GbPkAU),
								(newid(), 'C0000ABC4', 'S%S', '2022-09-01', '2022-09-01', 'BOB', 'JOE', 4, 'GB', @GbPkGB);";

			_ = TestConnection.ExecuteNonQuery(sqlText);
		}

		readonly string amaPk01 = "AC073529-7FB6-40C3-89B0-A6643D6D283C";
	}
}
