using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Billing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Billing.StlCollector.Retriever.Testing
{
	sealed class StlRetrieverMonthlyTest : TestCaseWithFactory
	{
		public void TestLogs()
		{
			using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTestingWithALPVersionBasedOffDateNow()))
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Production, isInternalSystem: false))
			using (RawDataRegistry.Instance.ObtainDynamicSTLCollectorDefinitionsFromTheCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var cts = new CancellationTokenSource())
			{
				var logger = new StlRetrieverLoggerForTest();
				var retriever = new StlRetrieverMonthly(logger);

				var twoMonthsAgo = ZDateTime.UtcNow.AddMonths(-2);
				var monthRangeCalculator = AusydMonthRange.New(twoMonthsAgo.Year, twoMonthsAgo.Month);

				retriever.CollectAndSend(cts.Token, monthRangeCalculator);
				Assert(!logger.Logs.Contains("Invalid date range selected"));

				var stlScripts = monthRangeCalculator.SelectApplicableScripts(new ScriptLoader().Load(Factory));
				Assert(logger.Logs.Contains("Collecting and sending STL data."));

				foreach (var stlScript in stlScripts)
				{
					Assert(logger.Logs.Contains("Collecting - " + stlScript.Script.Code + " - " + stlScript.Script.Feature));
				}

				var mandatoryScripts = retriever.Collector.Scripts.Where(s => s.Script.IsMandatoryForMilestones);
				Assert(logger.Logs.Contains($"STL Milestone Created - {SqlFormatInfo.ToSqlDateString(monthRangeCalculator.StlMilestoneTimestamp.Value.Date)} (MONTH) MandatoryItemsCount({mandatoryScripts.Count()}) Version({ReleaseInfo.Instance.VersionNumber})"));
			}
		}

		public void TestCollectAndSend()
		{
			using var cts = new CancellationTokenSource();
			var retriever = new StlRetrieverMonthlyForTest();

			var validDate = ZDateTime.UtcNow.Date.AddDays(-2);
			var monthRange = AusydMonthRange.New(validDate.Year, validDate.Month);

			retriever.CollectAndSend(cts.Token, monthRange);
			AssertBillingTransactions(
				monthRange,
				detailTransactionExpected: true,
				monthlyAllowHistoricalTransactionExpected: true,
				monthlyCurrentOnlyTransactionExpected: true,
				stlMilestoneTransactionExpected: true,
				retriever.Collector.Scripts
			);
		}

		public void TestCollectAndSendIsNotImpactedByWatermarks()
		{
			using var cts = new CancellationTokenSource();
			var retriever = new StlRetrieverMonthlyForTest();

			var validDate = ZDateTime.UtcNow.Date.AddDays(-2);
			var monthRange = AusydMonthRange.New(validDate.Year, validDate.Month);

			SystemDataRegistry.Instance.StlCollectorHighWaterMark.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, monthRange.EndDateTimeExclusive);
			retriever.CollectAndSend(cts.Token, monthRange);
			AssertBillingTransactions(
				monthRange,
				detailTransactionExpected: true,
				monthlyAllowHistoricalTransactionExpected: true,
				monthlyCurrentOnlyTransactionExpected: true,
				stlMilestoneTransactionExpected: true,
				retriever.Collector.Scripts
			);
		}

		public void TestCollectDayAndSend()
		{
			using var cts = new CancellationTokenSource();
			var retriever = new StlRetrieverMonthlyForTest();

			var validDate = ZDateTime.UtcNow.Date.AddDays(-2);
			var monthRange = AusydMonthRange.NewByStartEnd(validDate.AddDays(-2).ToDateTime(), validDate.ToDateTime());

			retriever.CollectAndSend(cts.Token, monthRange);
			AssertBillingTransactions(
				monthRange,
				detailTransactionExpected: true,
				monthlyAllowHistoricalTransactionExpected: true,
				monthlyCurrentOnlyTransactionExpected: true,
				stlMilestoneTransactionExpected: true,
				retriever.Collector.Scripts
			);
		}

		public void TestCollectAndSend_ItemCodeTRN()
		{
			using var cts = new CancellationTokenSource();
			var retriever = new StlRetrieverMonthlyForTest();
			var validDate = ZDateTime.UtcNow.Date.AddDays(-2).AddMonths(-1);
			var monthRange = AusydMonthRange.New(validDate.Year, validDate.Month);

			retriever.CollectAndSend(cts.Token, monthRange, code: "TRN");
			AssertBillingTransactions(
				monthRange,
				detailTransactionExpected: true,
				monthlyAllowHistoricalTransactionExpected: false,
				monthlyCurrentOnlyTransactionExpected: false,
				stlMilestoneTransactionExpected: false,
				retriever.Collector.Scripts
			);
		}

		public void TestCollectAndSend_ItemCodeMON()
		{
			using var cts = new CancellationTokenSource();
			var retriever = new StlRetrieverMonthlyForTest();
			var validDate = ZDateTime.UtcNow.Date.AddDays(-2).AddMonths(-5);
			var monthRange = AusydMonthRange.New(validDate.Year, validDate.Month);

			retriever.CollectAndSend(cts.Token, monthRange, code: "MON");
			AssertBillingTransactions(
				monthRange,
				detailTransactionExpected: false,
				monthlyAllowHistoricalTransactionExpected: true,
				monthlyCurrentOnlyTransactionExpected: false,
				stlMilestoneTransactionExpected: false,
				retriever.Collector.Scripts
			);
		}

		public void TestCollectAndSend_NonexistingItemCode()
		{
			using var cts = new CancellationTokenSource();
			var retriever = new StlRetrieverMonthlyForTest();
			var validDate = ZDateTime.UtcNow.Date.AddDays(-2).AddMonths(-9);
			var monthRange = AusydMonthRange.New(validDate.Year, validDate.Month);

			AssertExceptionThrown(
				typeof(BillingException),
				"No logger available to log exception: No script found for ~!@.",
				() => retriever.CollectAndSend(cts.Token, monthRange, code: "~!@")
			);

			AssertBillingTransactions(
				monthRange,
				detailTransactionExpected: false,
				monthlyAllowHistoricalTransactionExpected: false,
				monthlyCurrentOnlyTransactionExpected: false,
				stlMilestoneTransactionExpected: false,
				retriever.Collector.Scripts
			);
		}

		void AssertBillingTransactions(AusydMonthRange collectionRange, bool detailTransactionExpected, bool monthlyAllowHistoricalTransactionExpected, bool monthlyCurrentOnlyTransactionExpected, bool stlMilestoneTransactionExpected, IEnumerable<IStlScriptWithConfig> scripts)
		{
			var billingTransactions = BillingTestHelper.GetBillingTransactionsFromDatabase(TestConnection);

			int expectedTotalTransactionCount = 0;

			if (detailTransactionExpected)
			{
				int detailTransactionCount = BillingTestHelper.AssertDetailRangeTransactions(billingTransactions, collectionRange);
				AssertEquals("Transactional Item - transaction count", 1, detailTransactionCount);
				expectedTotalTransactionCount++;
			}

			if (monthlyAllowHistoricalTransactionExpected)
			{
				int monthlyAllowHistoricalTransactionCount = BillingTestHelper.AssertMonthlyAllowHistoricalRangeTransactions(billingTransactions, collectionRange);
				AssertEquals("Monthly Allow Historical Item - transaction count", 1, monthlyAllowHistoricalTransactionCount);
				expectedTotalTransactionCount++;
			}

			if (monthlyCurrentOnlyTransactionExpected)
			{
				int monthlyCurrentOnlyTransactionCount = BillingTestHelper.AssertMonthlyCurrentOnlyRangeTransactions(billingTransactions, collectionRange);
				AssertEquals("Monthly Current Only Item - transaction count", 1, monthlyCurrentOnlyTransactionCount);
				expectedTotalTransactionCount++;
			}

			if (stlMilestoneTransactionExpected)
			{
				int dailyTransactionCount = BillingTestHelper.AssertDailyTransactions(billingTransactions, collectionRange);
				AssertEquals("Daily - transaction count", 1, dailyTransactionCount);
				expectedTotalTransactionCount++;
				int stlMilestoneTransactionCount = BillingTestHelper.AssertStlMilestoneTransactions(billingTransactions, collectionRange, scripts);
				AssertEquals("STL Milestone transaction count", 1, stlMilestoneTransactionCount);
				expectedTotalTransactionCount++;
			}

			AssertEquals("Total transaction count", expectedTotalTransactionCount, billingTransactions.Count());
		}

		#region Implementation

		class StlRetrieverMonthlyForTest : StlRetrieverMonthly
		{
			public StlRetrieverMonthlyForTest()
				: base(null, new BillingDataCollectorForTest())
			{
			}
		}

		class StlRetrieverLoggerForTest : IUserAttendedStlRetrieverLogger
		{
			void IUserAttendedStlRetrieverLogger.InitProgress(string count)
			{ }

			void IUserAttendedStlRetrieverLogger.TaskProgress(string message)
			{
				Logs.Add(message);
			}

			void IUserAttendedStlRetrieverLogger.TaskInfo(string message)
			{
				Logs.Add(message);
			}

			void IUserAttendedStlRetrieverLogger.TaskFailed(string message)
			{
				Logs.Add(message);
			}

			void ILogger.Log(LogType type, string message)
			{
				Logs.Add(message);
			}

			void ILogger.Log(LogType type, string message, Exception ex)
			{
				Logs.Add(message);
			}

			public List<string> Logs = new List<string>();
		}

		#endregion
	}
}
