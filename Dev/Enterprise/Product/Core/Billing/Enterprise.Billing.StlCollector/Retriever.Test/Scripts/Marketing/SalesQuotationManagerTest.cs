using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Marketing;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Marketing
{
	[TestedType(typeof(SalesQuotationManager))]
	sealed class SalesQuotationManagerTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @GcPk UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @ThPk02 UNIQUEIDENTIFIER = newid();
				DECLARE @ThPk05 UNIQUEIDENTIFIER = newid();

				INSERT dbo.RatingHeader (TH_PK, TH_GC, TH_RateType, TH_QuoteNumber, TH_OneTimeQuote, TH_SystemCreateUser, TH_SystemCreateTimeUtc, TH_QuoteDate, TH_IsCancelled, TH_SystemLastEditTimeUtc, TH_SystemLastEditUser) VALUES
					(newid(), @GcPk, 'QTE', 'QM01', 0, 'US1', '2016-02-01 01:00:00', '2016-02-01', 0, GetUtcDate(), '~BP'),
					(@ThPk02, @GcPk, 'QTE', 'QM02', 0, 'US2', '2016-02-01 01:05:00', '2016-02-01', 0, GetUtcDate(), '~BP'),
					(newid(), @GcPk, 'COS', ''    , 0, 'US3', '2016-02-01 01:10:00', '2016-02-01', 0, GetUtcDate(), '~BP'),
					(newid(), @GcPk, 'QTE', 'QM04', 0, 'US4', '2016-02-01 01:15:00', '2016-02-01', 0, GetUtcDate(), '~BP'),
					(@ThPk05, @GcPk, 'QTE', 'QM05', 0, 'US5', GetUtcDate(), '2016-02-01', 0, GetUtcDate(), '~BP'),
					(newid(), @GcPk, 'QTE', 'QM06', 0, 'US6', '2016-02-01 01:20:00', '2016-02-01', 1, GetUtcDate(), '~BP'),
					(newid(), @GcPk, 'QTE', 'QM07', 0, 'US7', '2016-02-01 01:25:00', '2016-02-01', 0, GetUtcDate(), '~BP'),
					(newid(), @GcPk, 'QTE', 'QM08', 1, 'US8', '2016-02-01 01:25:00', '2016-02-01', 0, GetUtcDate(), '~BP');";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 4, transactions.Count());

			var transaction1 = FindRowByRef1(transactions, "Header: QM01");
			AssertEquals("[T1] CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("[T1] BranchCode", null, transaction1.GetBranchCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2016, 2, 1, 1, 0, 0), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] UserCode", "US1", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);

			var transaction2 = FindRowByRef1(transactions, "Header: QM02");
			AssertEquals("[T2] CompanyCode", "DEM", transaction2.GetCompanyCode());
			AssertEquals("[T2] BranchCode", null, transaction2.GetBranchCode());
			AssertEquals("[T2] TransactionDateUtc", new DateTime(2016, 2, 1, 1, 5, 0), transaction2.ServiceOccuredUTC);
			AssertEquals("[T2] UserCode", "US2", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);

			var transaction3 = FindRowByRef1(transactions, "Header: QM04");
			AssertEquals("[T3] CompanyCode", "DEM", transaction3.GetCompanyCode());
			AssertEquals("[T3] BranchCode", null, transaction3.GetBranchCode());
			AssertEquals("[T3] TransactionDateUtc", new DateTime(2016, 2, 1, 1, 15, 0), transaction3.ServiceOccuredUTC);
			AssertEquals("[T3] UserCode", "US4", transaction3.ClientStaffCode);
			AssertEquals("[T3] ItemCount", 1, transaction3.BillableCount);

			var transaction4 = FindRowByRef1(transactions, "Header: QM07");
			AssertEquals("[T4] CompanyCode", "DEM", transaction4.GetCompanyCode());
			AssertEquals("[T4] BranchCode", null, transaction4.GetBranchCode());
			AssertEquals("[T4] TransactionDateUtc", new DateTime(2016, 2, 1, 1, 25, 0), transaction4.ServiceOccuredUTC);
			AssertEquals("[T4] UserCode", "US7", transaction4.ClientStaffCode);
			AssertEquals("[T4] ItemCount", 1, transaction4.BillableCount);
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2016, 2);

		public void TestIsSystemLevelFeature()
		{
			AssertEquals("IsSystemLevel", false, ScriptToTest.IsSystemLevel);
		}
	}
}
