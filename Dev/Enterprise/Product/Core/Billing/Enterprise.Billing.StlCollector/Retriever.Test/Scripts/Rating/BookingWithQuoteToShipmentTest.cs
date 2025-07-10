using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Rating;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Rating
{
	[TestedType(typeof(BookingWithQuoteToShipment))]
	sealed class BookingWithQuoteToShipmentTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			string sqlText = @"
			DECLARE @GcPk UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');

			INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES
					(NEWID(), 'SY1',  @GcPk),
					(NEWID(), 'TK1',  @GcPk);

			DECLARE @ThPk01 UNIQUEIDENTIFIER = newid();
			DECLARE @ThPk02 UNIQUEIDENTIFIER = newid();
			DECLARE @ThPk03 UNIQUEIDENTIFIER = newid();
			DECLARE @ThPk04 UNIQUEIDENTIFIER = newid();
			DECLARE @ThPk05 UNIQUEIDENTIFIER = newid();
			DECLARE @ThPk06 UNIQUEIDENTIFIER = newid();

			DECLARE @JkPk01 UNIQUEIDENTIFIER = newid();
			DECLARE @JkPk02 UNIQUEIDENTIFIER = newid();

			DECLARE @JsPk01 UNIQUEIDENTIFIER = newid();
			DECLARE @JsPk02 UNIQUEIDENTIFIER = newid();
			DECLARE @JsPk03 UNIQUEIDENTIFIER = newid();
			DECLARE @JsPk04 UNIQUEIDENTIFIER = newid();
			DECLARE @JsPk05 UNIQUEIDENTIFIER = newid();
			DECLARE @JsPk06 UNIQUEIDENTIFIER = newid();

			INSERT dbo.RatingHeader (TH_PK, TH_GC, TH_RateType, TH_QuoteNumber, TH_OneTimeQuote, TH_SystemCreateUser, TH_SystemCreateTimeUtc, TH_QuoteDate, TH_IsCancelled, TH_SystemLastEditTimeUtc, TH_SystemLastEditUser) VALUES
				(@ThPk01, @GcPk, 'QTE', 'QBNE00001001', 0, 'US1', '2022-10-01 01:00:00', '2022-10-06', 0, GetUtcDate(), '~BP'),
				(@ThPk02, @GcPk, 'QTE', 'QBNE00001002', 0, 'US2', '2022-10-01 01:05:00', '2022-10-05', 0, GetUtcDate(), '~BP'),
				(@ThPk03, @GcPk, 'COS', 'QBNE00001003', 0, 'US3', '2022-10-01 01:10:00', '2022-10-04', 0, GetUtcDate(), '~BP'),
				(@ThPk04, @GcPk, 'QTE', 'QBNE00001004', 0, 'US4', '2022-10-01 01:15:00', '2022-10-03', 0, GetUtcDate(), '~BP'),
				(@ThPk05, @GcPk, 'QTE', 'QBNE00001005', 0, 'US5', GetUtcDate(), '2022-10-02', 0, GetUtcDate(), '~BP'),
				(@ThPk06, @GcPk, 'QTE', 'QBNE00001006', 0, 'US6', '2022-10-01 01:20:00', '2022-10-07', 1, GetUtcDate(), '~BP'),
				(newid(), @GcPk, 'QTE', 'QBNE00001008', 0, 'US7', '2022-10-01 01:25:00', '2022-10-07', 0, GetUtcDate(), '~BP'),
				(newid(), @GcPk, 'QTE', 'QBNE00001009', 1, 'US8', '2022-10-01 01:25:00', '2022-10-09', 0, GetUtcDate(), '~BP');

			INSERT dbo.JobConsol(JK_PK, JK_UniqueConsignRef, JK_SystemCreateTimeUtc) VALUES(@JkPk01, 'con1', '2022-11-21 11:01');
			INSERT dbo.JobConsol(JK_PK, JK_UniqueConsignRef, JK_SystemCreateTimeUtc) VALUES(@JkPk02, 'con2', '2022-11-21 11:01');

			INSERT dbo.JobShipment (JS_PK, JS_ShipmentType, JS_RL_NKOrigin, JS_IsForwardRegistered, JS_UniqueConsignRef, JS_IsBooking, JS_IsCancelled, JS_TH_OneTimeQuote, JS_SystemCreateTimeUtc) VALUES
				(@JsPk01, 'HVL', 'AUSYD', 1, 'Consign1', 1, 0, @ThPk01, '2022-11-02 01:25:00'),
				(@JsPk02, 'STD', 'AUSYD', 1, 'Consign2', 1, 0, @ThPk02, '2022-11-03 01:25:00'),
				(@JsPk03, 'HVL', 'USAAA', 0, 'Consign3', 1, 0, @ThPk03, '2022-11-04 01:25:00'),
				(@JsPk04, 'HVL', 'AUSYD', 1, 'Consign4', 0, 0, @ThPk04, '2022-11-05 01:25:00'),
				(@JsPk05, 'STD', 'AUSYD', 1, 'Consign5', 1, 0, @ThPk05, '2022-11-06 01:25:00'),
				(@JsPk06, 'HVL', 'AUSYD', 1, 'Consign6', 1, 0, @ThPk06, '2022-10-05 01:25:00'),
				(newid(), 'HVL', 'AUSYD', 1, 'Consign7', 1, 0, null, '2022-11-04 01:25:00');

			INSERT dbo.JobConShipLink(JN_PK, JN_JK, JN_JS) VALUES
				(newid(), @JkPk01, @JsPk01),
				(newid(), @JkPk02, @JsPk02);
			";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 1, transactions.Count());
			var transaction1 = FindRowByRef1(transactions, "Consign5");
			AssertEquals(1, transaction1.BillableCount);
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2022, 11);

		public void TestIsSystemLevelFeature()
		{
			AssertEquals("IsSystemLevel", false, ScriptToTest.IsSystemLevel);
		}
	}
}
