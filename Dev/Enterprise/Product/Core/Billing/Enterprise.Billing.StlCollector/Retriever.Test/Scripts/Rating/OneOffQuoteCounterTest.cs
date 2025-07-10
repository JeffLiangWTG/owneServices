using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Rating;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Rating
{
	[TestedType(typeof(OneOffQuoteCounter))]
	sealed class OneOffQuoteCounterTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			string sqlText = @"
			DECLARE @GcPk UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');

			INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES
					(NEWID(), 'SY1',  @GcPk),
					(NEWID(), 'TK1',  @GcPk);

			INSERT dbo.RatingHeader (TH_PK, TH_GC, TH_RateType, TH_QuoteNumber, TH_OneTimeQuote, TH_SystemCreateUser, TH_SystemCreateTimeUtc, TH_QuoteDate, TH_SystemLastEditTimeUtc, TH_SystemLastEditUser) VALUES
				(newid(), @GcPk, 'QTE', 'QBNE00001001', 1, 'US1', '2022-11-01 01:00:00', '2022-11-01', GetUtcDate(), '~BP'),
				(newid(), @GcPk, 'QTE', 'QBNE00001002', 1, 'US2', '2022-11-01 01:05:00', '2022-11-01', GetUtcDate(), '~BP'),
				(newid(), @GcPk, 'COS', 'QBNE00001003', 1, 'US3', '2022-11-01 01:10:00', '2022-11-01', GetUtcDate(), '~BP'),
				(newid(), @GcPk, 'QTE', 'QBNE00001004', 1, 'US4', '2022-11-01 01:15:00', '2022-11-01', GetUtcDate(), '~BP'),
				(newid(), @GcPk, 'QTE', 'QBNE00001005', 1, 'US5', '2022-11-18 02:15:00', '2022-11-01', GetUtcDate(), '~BP'),
				(newid(), @GcPk, 'QTE', 'QBNE00001006', 1, 'US6', '2022-11-01 01:20:00', '2022-11-01', GetUtcDate(), '~BP'),
				(newid(), @GcPk, 'QTE', 'QBNE00001007', 1, 'US7', '2022-11-01 01:25:00', '2022-11-01', GetUtcDate(), '~BP'),
				(newid(), @GcPk, 'QTE', 'QBNE00001008', 0, 'US8', '2022-11-01 01:25:00', '2022-11-01', GetUtcDate(), '~BP');
			";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 6, transactions.Count());
			var transaction1 = FindRowByRef1(transactions, "Header: QBNE00001001");
			AssertEquals(1, transaction1.BillableCount);
			var transaction2 = FindRowByRef1(transactions, "Header: QBNE00001002");
			AssertEquals(1, transaction2.BillableCount);
			var transaction3 = FindRowByRef1(transactions, "Header: QBNE00001004");
			AssertEquals(1, transaction3.BillableCount);
			var transaction4 = FindRowByRef1(transactions, "Header: QBNE00001005");
			AssertEquals(1, transaction4.BillableCount);
			var transaction5 = FindRowByRef1(transactions, "Header: QBNE00001006");
			AssertEquals(1, transaction5.BillableCount);
			var transaction6 = FindRowByRef1(transactions, "Header: QBNE00001007");
			AssertEquals(1, transaction6.BillableCount);
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2022, 11);

		public void TestIsSystemLevelFeature()
		{
			AssertEquals("IsSystemLevel", false, ScriptToTest.IsSystemLevel);
		}
	}
}
