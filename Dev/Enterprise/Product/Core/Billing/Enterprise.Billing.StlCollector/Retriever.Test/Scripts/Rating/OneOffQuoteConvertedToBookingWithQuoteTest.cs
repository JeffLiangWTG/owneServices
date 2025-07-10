using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Rating;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Rating
{
	[TestedType(typeof(OneOffQuoteConvertedToBookingWithQuote))]
	sealed class OneOffQuoteConvertedToBookingWithQuoteTest : RefStlScriptWithDefaultsTest
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
			DECLARE @ThPk07 UNIQUEIDENTIFIER = newid();

			DECLARE @GcPkAU UNIQUEIDENTIFIER = newid();
			DECLARE @GbPkAU UNIQUEIDENTIFIER = newid();

			INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode) VALUES 
				(@GcPkAU, 'AUC', 'AU company', 'AU');

			INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES 
				(@GbPkAU, 'AUB', @GcPkAU);

			INSERT dbo.RatingHeader (TH_PK, TH_GC, TH_RateType, TH_QuoteNumber, TH_OneTimeQuote, TH_SystemCreateUser, TH_SystemCreateTimeUtc, TH_QuoteDate, TH_IsCancelled, TH_SystemLastEditTimeUtc, TH_SystemLastEditUser) VALUES
				(@ThPk01, @GcPk, 'QTE', 'QBNE00001001', 1, 'US1', '2022-11-01 01:00:00', '2022-11-01', 0, GetUtcDate(), '~BP'),
				(@ThPk02, @GcPk, 'QTE', 'QBNE00001002', 1, 'US2', '2022-11-01 01:05:00', '2022-11-01', 0, GetUtcDate(), '~BP'),
				(@ThPk03, @GcPk, 'COS', 'QBNE00001003', 1, 'US3', '2022-11-01 01:10:00', '2022-11-01', 0, GetUtcDate(), '~BP'),
				(@ThPk04, @GcPk, 'QTE', 'QBNE00001004', 1, 'US4', '2022-11-01 01:15:00', '2022-11-01', 0, GetUtcDate(), '~BP'),
				(@ThPk05, @GcPk, 'QTE', 'QBNE00001005', 1, 'US5', '2022-10-01 01:15:00', '2022-11-01', 0, GetUtcDate(), '~BP'),
				(@ThPk06, @GcPk, 'QTE', 'QBNE00001006', 1, 'US6', '2022-10-01 01:20:00', '2022-11-01', 0, GetUtcDate(), '~BP'),
				(@ThPk07, @GcPkAU, 'QTE', 'QBNE00001007', 1, 'US9', '2022-10-01 01:20:00', '2022-11-01', 0, GetUtcDate(), '~BP'),
				(newid(), @GcPk, 'QTE', 'QBNE00001008', 1, 'US7', '2022-11-01 01:25:00', '2022-11-01', 0, GetUtcDate(), '~BP'),
				(newid(), @GcPk, 'QTE', 'QBNE00001009', 1, 'US8', '2022-11-01 01:25:00', '2022-11-01', 0, GetUtcDate(), '~BP');

			INSERT dbo.JobShipment (JS_PK, JS_ShipmentType, JS_RL_NKOrigin, JS_IsForwardRegistered, JS_UniqueConsignRef, JS_IsBooking, JS_IsCancelled, JS_TH_OneTimeQuote, JS_SystemCreateTimeUtc) VALUES
				(newid(), 'HVL', 'AUSYD', 1, 'Consign1', 1, 0,@ThPk01,'2022-10-01 01:15:00'),
				(newid(), 'STD', 'AUSYD', 1, 'Consign2', 1, 0,@ThPk02,'2022-10-01 01:15:00'),
				(newid(), 'HVL', 'USAAA', 1, 'Consign3', 1, 0,@ThPk03,'2022-11-01 01:15:00'),
				(newid(), 'HVL', 'AUSYD', 1, 'Consign4', 0, 0,@ThPk04,'2022-11-01 01:15:00'),
				(newid(), 'STD', 'AUSYD', 1, 'Consign5', 1, 0,@ThPk05,'2022-11-01 01:15:00'),
				(newid(), 'STD', 'AUSYD', 0, 'Consign6', 1, 0,@ThPk06,'2022-11-01 01:15:00'),
				(newid(), 'HVL', 'AUMEL', 0, 'Consign7', 1, 0,@ThPk07,'2022-11-01 01:15:00');";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 2, transactions.Count());

			AssertRow(transactions, "DEM", 2);
			AssertRow(transactions, "AUC", 1);
		}

		void AssertRow(IEnumerable<IStlTransaction> transactions, string company, int expectedCount)
		{
			var transaction = transactions.Single(t => t.GetCompanyCode() == company);
			AssertEquals("Item Count", expectedCount, transaction.BillableCount);
		}

		protected override sealed IDateTimeRange TestDateTimeRange
		{
			get
			{
				var startDate = new DateTime(2022, 11, 1);
				return new RecurringRange(startDate, startDate.AddDays(1));
			}
		}

		public void TestIsSystemLevelFeature()
		{
			AssertEquals("IsSystemLevel", false, ScriptToTest.IsSystemLevel);
		}
	}
}
