using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Rating;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Rating
{
	[TestedType(typeof(RatingHeaders))]
	sealed class RatingHeadersTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			string sqlText = $@"
			DECLARE @GcPk UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
			DECLARE @GbPk UNIQUEIDENTIFIER = (SELECT GB_PK FROM dbo.GlbBranch WHERE GB_Code = 'DEM');
			DECLARE @GePk UNIQUEIDENTIFIER = (SELECT TOP(1) GE_PK FROM dbo.GlbDepartment);

			INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES
					(NEWID(), 'SY1',  @GcPk),
					(NEWID(), 'TK1',  @GcPk);

			INSERT dbo.RatingHeader (TH_PK, TH_GC, TH_RateType, TH_QuoteNumber, TH_OneTimeQuote, TH_SystemCreateUser, TH_SystemCreateTimeUtc, TH_QuoteDate, TH_IsCancelled, TH_SystemLastEditTimeUtc, TH_SystemLastEditUser,TH_Accepted) VALUES
				(newid(), @GcPk, 'QTE', 'QBNE00001001', 0, 'US1', '2022-11-11 01:00:00', '2022-11-01', 0, GetUtcDate(), '~BP','2022-11-18 01:00:00'),
				(newid(), @GcPk, 'QTE', 'QBNE00001002', 1, 'US2', '2022-11-11 01:05:00', '2022-11-01', 0, GetUtcDate(), '~BP','2022-11-19 03:30:00'),
				(newid(), @GcPk, 'QTE', 'QBNE00001004', 1, 'US4', '2022-11-11 01:15:00', '2022-11-01', 0, GetUtcDate(), '~BP','2022-11-21 02:00:00'),
				(newid(), @GcPk, 'QTE', 'QBNE00001005', 0, 'US5', '2022-11-01 01:16:00', '2022-11-01', 1, GetUtcDate(), '~BP','2022-11-22 05:00:00'),
				(newid(), @GcPk, 'QTE', 'QBNE00001006', 0, 'US6', '2022-11-02 01:20:00', '2022-11-01', 1, GetUtcDate(), '~BP','2022-11-23 06:00:00'),
				(newid(), null, 'COS', 'QBNE00001007', 1, 'US7', '2022-11-02 01:20:00', '2022-11-01', 0, GetUtcDate(), '~BP', null),
				(newid(), null, 'QTE', 'QBNE00001008', 1, 'US8', '2022-11-03 01:26:00', '2022-11-01', 0, GetUtcDate(), '~BP', null),
				(newid(), null, 'QTE', 'QBNE00001009', 1, 'US9', '2022-11-03 01:25:00', '2022-11-01', 0, GetUtcDate(), '~BP', null),
				(newid(), null, 'QTE', 'QBNE00001010', 1, 'US2', '2022-10-04 01:25:00', '2022-11-06', 0, GetUtcDate(), '~BP', null),
				(newid(), null, 'QTE', 'QBNE00001011', 1, 'US3', '2022-12-04 01:25:00', '2022-11-04', 0, GetUtcDate(), '~BP', null),
				(newid(), null, 'QTE', 'QBNE00001012', 1, 'US4', '2022-09-04 01:25:00', '2022-11-05', 0, GetUtcDate(), '~BP', null);";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 3, transactions.Count());

			AssertRow(transactions, "QTE", "True", 3);
			AssertRow(transactions, "QTE", "False", 2);
			AssertRow(transactions, "COS", "False", 1);
		}

		void AssertRow(IEnumerable<IStlTransaction> transactions, string expectedRateType, string accepted, int expectedCount)
		{
			var transaction = transactions.Single(t => t.Reference1 == expectedRateType && t.Reference2 == accepted);
			AssertEquals("Item Count", expectedCount, transaction.BillableCount);
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2022, 11);

		public void TestIsSystemLevelFeature()
		{
			AssertEquals("IsSystemLevel", false, ScriptToTest.IsSystemLevel);
		}
	}
}
