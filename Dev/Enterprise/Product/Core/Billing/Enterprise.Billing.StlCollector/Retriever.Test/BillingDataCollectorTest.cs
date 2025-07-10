using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Billing.StlCollector.Retriever.Scripts;
using Enterprise.Integration;
using Enterprise.Integration.Billing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Testing
{
	sealed class BillingDataCollectorTest : TransactionedTestCase
	{
		public void TestThrowsExceptionIfLoggerIsNull()
		{
#if NETFRAMEWORK
			AssertExceptionThrown(
				"Attempt to instantiate BillingDataCollector with a null logger",
				typeof(ArgumentNullException),
				"Value cannot be null.\r\nParameter name: logger",
				() => new BillingDataCollector(null)
			);
#elif NET
			AssertExceptionThrown(
				"Attempt to instantiate BillingDataCollector with a null logger",
				typeof(ArgumentNullException),
				"Value cannot be null. (Parameter 'logger')",
				() => new BillingDataCollector(null)
			);
#endif
		}

		[TestDate(2015, 8, 31, 18, 0, 0)]
		public void TestCollectData()
		{
			TestDateAttribute.UseUNLOCO = true;
			BillingTestHelper.StlCollectorHighWaterMarkRegistry = new DateTime(2015, 8, 31, 13, 0, 0);
			var collector = new BillingDataCollectorForTest();
			var collectionRange = new RecurringRange(new DateTime(2015, 8, 31, 13, 0, 0), new DateTime(2015, 8, 31, 14, 0, 0));
			collector.CollectAndSendData(CancellationToken.None, collectionRange);
			AssertLogCount(13, collector);
			int logIterator = 0;
			AssertLogEntry(collector.Logs, "Collecting - TRN - Transactional Feature", ref logIterator);
			AssertLogEntry(collector.Logs, "- 1 transaction(s) sent", ref logIterator);
			AssertLogEntry(collector.Logs, "Collecting - MON - Monthly Allow Historical Data Feature", ref logIterator);
			AssertLogEntry(collector.Logs, "- 1 transaction(s) sent", ref logIterator);
			AssertLogEntry(collector.Logs, "Collecting - MCO - Monthly Current Data Only Feature", ref logIterator);
			AssertLogEntry(collector.Logs, "- 1 transaction(s) sent", ref logIterator);
			AssertLogEntry(collector.Logs, "Collecting - DAY - Daily Collection Feature", ref logIterator);
			AssertLogEntry(collector.Logs, "- 1 transaction(s) sent", ref logIterator);
			var mandatoryScripts = collector.Scripts.Where(s => s.Script.IsMandatoryForMilestones);
			AssertLogEntry(collector.Logs, $"STL Milestone Created - 2015-08-31 (DAY) MandatoryItemsCount({mandatoryScripts.Count()}) Version({ReleaseInfo.Instance.VersionNumber})", ref logIterator);
			AssertLogEntry(collector.Logs, "Collecting - SPT - Snapshot Time Based", ref logIterator);
			AssertLogEntry(collector.Logs, "- 1 transaction(s) sent", ref logIterator);
			AssertLogEntry(collector.Logs, "Collecting - SPC - Snapshot Live Configuration", ref logIterator);
			AssertLogEntry(collector.Logs, "- 1 transaction(s) sent", ref logIterator);
		}

		[TestDate(2015, 8, 31, 18, 0, 0)]
		public void TestMilestoneGeneratedWhenWatermarksOutOfSync()
		{
			TestDateAttribute.UseUNLOCO = true;
			BillingTestHelper.StlCollectorHighWaterMarkRegistry = new DateTime(2015, 8, 31, 12, 0, 0);
			var collector = new BillingDataCollectorForTest();
			var trnScript = collector.Scripts.First(swc => swc.Script.Code == "TRN");
			trnScript.HighWaterMarkSettings.HighWaterMark = new DateTime(2015, 8, 31, 13, 0, 0);
			collector.Factory.Save();

			collector.CollectAndSendData(CancellationToken.None, new RecurringRange(new DateTime(2015, 8, 31, 12, 0, 0), new DateTime(2015, 8, 31, 14, 0, 0)));
			var billingTransactions = BillingTestHelper.GetBillingTransactionsFromDatabase(TestConnection);
			collector.CollectAndSendData(CancellationToken.None, new RecurringRange(new DateTime(2015, 8, 31, 13, 0, 0), new DateTime(2015, 8, 31, 15, 0, 0)));
			billingTransactions.Append(BillingTestHelper.GetBillingTransactionsFromDatabase(TestConnection).ToArray());
			var stlMilestoneTransactionCount = BillingTestHelper.AssertStlMilestoneTransactions(billingTransactions, new RecurringRange(new DateTime(2015, 8, 31, 12, 0, 0), new DateTime(2015, 8, 31, 15, 0, 0)), collector.Scripts);
			AssertEquals("STL Milestone transaction count", 1, stlMilestoneTransactionCount);
		}

		[ExpectNoExceptions]
		[TestDate(2015, 8, 31, 18, 0, 0)]
		public void TestCollectDataWithUserAttendedLogger()
		{
			TestDateAttribute.UseUNLOCO = true;
			BillingTestHelper.StlCollectorHighWaterMarkRegistry = new DateTime(2015, 8, 31, 13, 0, 0);
			var mockLogger = new Mock<IUserAttendedStlRetrieverLogger>();
			var collector = new BillingDataCollectorWithUserAttendedLoggerForTest(mockLogger.Object);
			var mandatoryScripts = collector.Scripts.Where(s => s.Script.IsMandatoryForMilestones);
			mockLogger.Setup(m => m.TaskProgress("Collecting - TRN - Transactional Feature"));
			mockLogger.Setup(m => m.TaskProgress("Collecting - MON - Monthly Allow Historical Data Feature"));
			mockLogger.Setup(m => m.TaskProgress("Collecting - MCO - Monthly Current Data Only Feature"));
			mockLogger.Setup(m => m.TaskProgress("Collecting - DAY - Daily Collection Feature"));
			mockLogger.Setup(m => m.TaskProgress("Collecting - SPT - Snapshot Time Based"));
			mockLogger.Setup(m => m.TaskProgress("Collecting - SPC - Snapshot Live Configuration"));
			mockLogger.Setup(m => m.TaskInfo("- 1 transaction(s) sent"));
			mockLogger.Setup(m => m.Log(LogType.Information, $"STL Milestone Created - 2015-08-31 (DAY) MandatoryItemsCount({mandatoryScripts.Count()}) Version({ReleaseInfo.Instance.VersionNumber})"));
			collector.CollectAndSendData(CancellationToken.None, new RecurringRange(new DateTime(2015, 8, 31, 13, 0, 0), new DateTime(2015, 8, 31, 14, 0, 0)));
			mockLogger.Verify(m => m.TaskInfo("- 1 transaction(s) sent"), Times.Exactly(6));
			mockLogger.VerifyAll();
		}

		[TestDate(2016, 7, 11, 18, 0, 0)]
		public void TestCollectData_NoDateBoundary()
		{
			TestDateAttribute.UseUNLOCO = true;
			BillingTestHelper.StlCollectorHighWaterMarkRegistry = new DateTime(2016, 7, 11, 10, 20, 5);
			var collectionRange = new RecurringRange(new DateTime(2016, 7, 11, 10, 20, 5), new DateTime(2016, 7, 11, 10, 45, 10));
			CollectAndAssertBillingTransactions(collectionRange);
		}

		[TestDate(2015, 12, 31, 18, 0, 0)]
		public void TestCollectData_OverAusydMonthBoundary()
		{
			TestDateAttribute.UseUNLOCO = true;
			BillingTestHelper.StlCollectorHighWaterMarkRegistry = new DateTime(2015, 12, 31, 12, 0, 0);
			var collectionRange = new RecurringRange(new DateTime(2015, 12, 31, 12, 0, 0), new DateTime(2015, 12, 31, 13, 0, 0));
			CollectAndAssertBillingTransactions(collectionRange);
		}

		public void TestCollectData_OverAusydMonthBoundaryAndCurrent()
		{
			var utcPlusOneMonth = EnvProxy.Instance.Time.CurrentUtcDateTime.AddMonths(1);
			var utcEnd = new DateTime(utcPlusOneMonth.Year, utcPlusOneMonth.Month, 1);
			var utcStart = utcEnd.AddDays(-1);
			var collectionRange = new RecurringRange(utcStart, utcEnd);
			BillingTestHelper.StlCollectorHighWaterMarkRegistry = utcStart;
			CollectAndAssertBillingTransactions(collectionRange);
		}

		[TestDate(2016, 2, 10, 18, 0, 0)]
		public void TestCollectData_OverAusydDateBoundary()
		{
			TestDateAttribute.UseUNLOCO = true;
			BillingTestHelper.StlCollectorHighWaterMarkRegistry = new DateTime(2016, 2, 10, 12, 0, 0);
			var collectionRange = new RecurringRange(new DateTime(2016, 2, 10, 12, 0, 0), new DateTime(2016, 2, 10, 13, 0, 0));
			CollectAndAssertBillingTransactions(collectionRange);
		}

		public void TestCollectData_InvalidRange()
		{
			DateTime utcNow = DateTime.UtcNow;
			var invalidRange = new RecurringRange(utcNow, utcNow.AddHours(26));

			AssertExceptionThrown(
				"Collecting STL data with an invalid range",
				typeof(BillingException),
				string.Format(CultureInfo.InvariantCulture, "Attempt to collect data for an invalid date range: {0}.", invalidRange.ToString()),
				() => new BillingDataCollectorForTest().CollectAndSendData(CancellationToken.None, invalidRange)
			);
		}

		[TestDate(2016, 2, 10, 18, 0, 0)]
		public void TestCollectData_MandatoryItemsAreGivenHigherSubmissionPriority()
		{
			TestDateAttribute.UseUNLOCO = true;
			var startDate = new DateTime(2016, 2, 10, 12, 0, 0);
			BillingTestHelper.StlCollectorHighWaterMarkRegistry = startDate;
			var collectionRange = new RecurringRange(startDate, new DateTime(2016, 2, 10, 13, 0, 0));
			var collector = new BillingDataCollectorForTest(new ScriptLoader(new ScriptFactoryForTest(new FeatureForValidationTest("SIN", "") { IsMandatoryForMilestones = false }, new TestTransactionalStlItem())));
			collector.CollectAndSendData(CancellationToken.None, collectionRange);

			var logEntries = collector.Logs.ToArray();
			AssertEquals("Wrong count of log entries", collectionRange.StlMilestoneTimestamp.HasValue ? 5 : 4, logEntries.Length);
			AssertEquals("Log Entry (0)", "Collecting - ~BB - FeatureForValidationTest", logEntries[0].Trim());
			AssertEquals("Log Entry (1)", "- 1 transaction(s) sent", logEntries[1].Trim());
			AssertEquals("Log Entry (2)", "Collecting - TRN - Transactional Feature", logEntries[2].Trim());
			AssertEquals("Log Entry (3)", "- 1 transaction(s) sent", logEntries[3].Trim());
			if (collectionRange.StlMilestoneTimestamp.HasValue)
			{
				var mandatoryScripts = collector.Scripts.Where(s => s.Script.IsMandatoryForMilestones);
				AssertEquals("Log Entry (4)", $"STL Milestone Created - {SqlFormatInfo.ToSqlDateString(startDate)} (DAY) MandatoryItemsCount({mandatoryScripts.Count()}) Version({ReleaseInfo.Instance.VersionNumber})", logEntries[4].Trim());
			}

			var factory = new BusinessObjectFactory();
			var transactionalUsages = factory.Load(BillingManager.StmUsageDataType, new ZQuery(StmUsageDataSchema.SUD_Code, "TRN"));
			AssertEquals("Wrong number of TRN usages", 1, transactionalUsages.Length);
			var transactionalUsage = transactionalUsages[0];
			AssertEquals("Wrong priority for TRN usage", new ZByte(1), transactionalUsage[StmUsageDataSchema.Constants.SUD_SubmissionPriority]);
			var validationUsages = factory.Load(BillingManager.StmUsageDataType, new ZQuery(StmUsageDataSchema.SUD_Code, "~BB"));
			AssertEquals("Wrong number of ~BB usages", 1, validationUsages.Length);
			var validationUsage = validationUsages[0];
			AssertEquals("Wrong priority for ~BB usage", new ZByte(2), validationUsage[StmUsageDataSchema.Constants.SUD_SubmissionPriority]);
			if (collectionRange.StlMilestoneTimestamp.HasValue)
			{
				var milestones = factory.Load(BillingManager.StmUsageDataType, new ZQuery(StmUsageDataSchema.SUD_Code, "STL"));
				AssertEquals("Wrong number of milestones", 1, validationUsages.Length);
				var milestone = milestones[0];
				AssertEquals("Wrong priority for milestone", new ZByte(1), milestone[StmUsageDataSchema.Constants.SUD_SubmissionPriority]);
			}
		}

		[TestDate(2015, 8, 31, 17, 0, 0)]
		public void TestCollectData_NonMandatoryMonthlyCollectorsOnlyRunMonthly()
		{
			BillingTestHelper.StlCollectorHighWaterMarkRegistry = new DateTime(2015, 8, 10, 12, 0, 0);
			TestDateAttribute.UseUNLOCO = true;
			var collector = new BillingDataCollectorForTest(new ScriptLoader(new ScriptFactoryForTest(new TestMonthlyAllowHistoricalStlItem(isMandatoryForMilestones: false), new TestMonthlyCurrentStlItem(isMandatoryForMilestones: false))));
			collector.CollectAndSendData(CancellationToken.None, new RecurringRange(new DateTime(2015, 8, 10, 12, 0, 0), new DateTime(2015, 8, 10, 14, 0, 0)));

			var logEntries = collector.Logs.ToArray();
			AssertEquals("Wrong count of log entries", 0, logEntries.Length);

			var collectionRange = new RecurringRange(new DateTime(2015, 8, 31, 13, 0, 0), new DateTime(2015, 8, 31, 14, 0, 0));
			collector.CollectAndSendData(CancellationToken.None, collectionRange);

			AssertLogCount(4, collector);
			Assert("Should contain MON script", collector.Logs.First().Contains("Collecting - MON - Monthly Allow Historical Data Feature"));
			Assert("Should contain MON transaction count", collector.Logs.Skip(1).First().Contains("- 1 transaction(s) sent"));
			Assert("Should contain MON script", collector.Logs.Skip(2).First().Contains("Collecting - MCO - Monthly Current Data Only Feature"));
			Assert("Should contain MON transaction count", collector.Logs.Skip(3).First().Contains("- 1 transaction(s) sent"));
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestCollectData_DelayReportExceptionExceedThreshold()
		{
			var utcStart = EnvProxy.Instance.Time.CurrentUtcDateTime;
			var utcEnd = utcStart.AddHours(1);
			BillingTestHelper.StlCollectorHighWaterMarkRegistry = utcStart;
			TestDateAttribute.UseUNLOCO = true;
			var collector = new BillingDataCollectorForTest(new ScriptLoader(new ScriptFactoryForTest(new FeatureWithQueryError())));
			collector.ExceptionThreshold = TimeSpan.FromHours(3);
			var collectionRange1 = new RecurringRange(utcStart, utcEnd);
			var script = collector.Scripts.First(swc => swc.Script.Code == "~QE");
			script.HighWaterMarkSettings.HighWaterMark = utcStart.AddHours(-2);
			collector.CollectAndSendData(CancellationToken.None, collectionRange1);
			AssertLogCount(2, collector);
			AssertEquals("Collecting - ~QE - FeatureWithQueryError", collector.Logs.First());
			Assert("Should contain INV Exception Without Throw", collector.Logs.Skip(1).First().Contains("Data collection failed for item [~QE]"));
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2015, 8, 31, 17, 0, 0)]
		public void TestCollectData_InvalidBillingTransaction()
		{
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Production))
			{
				BillingTestHelper.StlCollectorHighWaterMarkRegistry = new DateTime(2015, 8, 10, 12, 0, 0);
				TestDateAttribute.UseUNLOCO = true;
				var collector = new BillingDataCollectorForTest(new ScriptLoader(new ScriptFactoryForTest(new TestInvalidBillingTransactionStlItem(isMandatoryForMilestones: false))));
				collector.SkipValidation = false;
				var collectionRange = new RecurringRange(new DateTime(2015, 8, 31, 13, 0, 0), new DateTime(2015, 8, 31, 14, 0, 0));
				collector.CollectAndSendData(CancellationToken.None, collectionRange);
				AssertEquals("Error was reported as developer exception", 1, ExceptionReporterTestListener.Instance.Count);
				AssertType<BillingException>(ExceptionReporterTestListener.Instance[0]);
				ExceptionReporterTestListener.Instance.Clear();

				AssertLogCount(2, collector);
				Assert("Should contain INV Exception", collector.Logs.First().Contains("Collecting - INV - Invalid Billing Transaction"));

				var expectedErrorMessage =
					"Error collecting STL data: Data collection failed for item [INV].\r\n"
					+ "Transaction validation failed.\r\n"
					+ "The field ServiceOccuredUTC is required and must be no more than 5 years in the past and no more than one month in the future.\r\n";

				AssertEquals("Query error", expectedErrorMessage, collector.Logs.Skip(1).First());
			}
		}

		public void TestCollectData_ShouldNotReport_KnownException()
		{
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Production))
			{
				var collector = new BillingDataCollectorForTest(new ScriptLoader(new ScriptFactoryForTest(new TestKnownExceptionStlItem())));
				var utcNow = DateTime.UtcNow;

				collector.CollectAndSendData(CancellationToken.None, new RecurringRange(utcNow.AddHours(-2), utcNow.AddHours(-1)));
				AssertEquals("Billing Transaction count", 0, Convert.ToInt32(TestConnection.ExecuteScalar("SELECT count(*) FROM dbo.StmUsageData")));
				AssertEquals("Error was reported as developer exception", 1, ExceptionReporterTestListener.Instance.Count);
				AssertType<BillingException>(ExceptionReporterTestListener.Instance[0]);

				var expectedErrorMessage =
					"Data collection failed for item [~KE].\r\n"
					+ "Cannot continue the execution because the session is in the kill state.";

				AssertEquals("Query error", expectedErrorMessage, ExceptionReporterTestListener.Instance[0].Message);
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestCollectData_BillingTransactionWithControlChars()
		{
			var collector = new BillingDataCollectorForTest(new ScriptLoader(new ScriptFactoryForTest(new TestBillingTransactionWithControlChars(isMandatoryForMilestones: false))));
			collector.SkipValidation = false;
			var startDate = DateTime.UtcNow;
			var collectionRange = new RecurringRange(startDate, startDate.AddDays(1));
			collector.CollectAndSendData(CancellationToken.None, collectionRange);
			AssertLogCount(2, collector);
			Assert("Should contain CAR script", collector.Logs.First().Contains("Collecting - CAR - Billing Transaction with control characters in data"));
			Assert("Should contain CAR transaction count", collector.Logs.Skip(1).First().Contains("- 1 transaction(s) sent"));
			var billingTransaction = BillingTestHelper.GetBillingTransactionsFromDatabase(TestConnection).Single();
			AssertEquals("Ref With Nulls", billingTransaction.Reference1);
			AssertEquals("Ref With Bells", billingTransaction.Reference2);
			AssertEquals("Ref With Backspaces", billingTransaction.Reference3);
			AssertEquals("Ref With VerticalTabs", billingTransaction.Reference4);
			AssertEquals("Ref With Escapes", billingTransaction.Reference5);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2015, 8, 31, 17, 0, 0)]
		public void TestCollectData_DynamicMonthlyCollectorsOnlyRunMonthly()
		{
			BillingTestHelper.StlCollectorHighWaterMarkRegistry = new DateTime(2015, 8, 10, 12, 0, 0);
			TestDateAttribute.UseUNLOCO = true;
			var collector = new BillingDataCollectorForTest(new ScriptLoader(new ScriptFactoryForTest(new TestMonthlyAllowHistoricalStlItem(collectorType: StlCollectorType.Dynamic), new TestMonthlyCurrentStlItem(collectorType: StlCollectorType.Dynamic))));
			collector.CollectAndSendData(CancellationToken.None, new RecurringRange(new DateTime(2015, 8, 10, 12, 0, 0), new DateTime(2015, 8, 10, 14, 0, 0)));

			var logEntries = collector.Logs.ToArray();
			AssertEquals("Wrong count of log entries", 0, logEntries.Length);

			var startDate = new DateTime(2015, 8, 31, 13, 0, 0);
			var collectionRange = new RecurringRange(startDate, new DateTime(2015, 8, 31, 14, 0, 0));
			collector.CollectAndSendData(CancellationToken.None, collectionRange);

			AssertLogCount(5, collector);
			Assert("Should contain MON script", collector.Logs.First().Contains("Collecting - MON - Monthly Allow Historical Data Feature"));
			Assert("Should contain MON transaction count", collector.Logs.Skip(1).First().Contains("- 1 transaction(s) sent"));
			Assert("Should contain MON script", collector.Logs.Skip(2).First().Contains("Collecting - MCO - Monthly Current Data Only Feature"));
			Assert("Should contain MON transaction count", collector.Logs.Skip(3).First().Contains("- 1 transaction(s) sent"));
			Assert("Should contain milestone", collector.Logs.Skip(4).First().Contains("STL Milestone Created - " + SqlFormatInfo.ToSqlDateString(startDate) + " (DAY)"));
		}

		void CollectAndAssertBillingTransactions(RecurringRange collectionRange)
		{
			var collector = new BillingDataCollectorForTest();
			collector.CollectAndSendData(CancellationToken.None, collectionRange);

			AssertLogEntries(collector.Logs, collectionRange, collector.Scripts);
			AssertBillingTransactions(collectionRange, collector.Scripts);
			BillingTestHelper.AssertUsageTransactions(BillingTestHelper.GetUsageTransactionsFromDatabase(TestConnection), expectedTimebasedCount: 1, expectedLiveConfigCount: 1);
		}

		void AssertLogEntries(IEnumerable<string> logEntries, IDateTimeRange collectionRange, IEnumerable<IStlScriptWithConfig> scripts)
		{
			var logIterator = 0;
			var mandatoryLogCount = 0;

			if (collectionRange.IsValid)
			{
				AssertLogEntry(logEntries, "Collecting - TRN - Transactional Feature", ref logIterator);
				AssertLogEntry(logEntries, "- 1 transaction(s) sent", ref logIterator);

				if (collectionRange.StlMilestoneTimestamp.HasValue)
				{
					var mandatoryScripts = scripts.Where(s => s.Script.IsMandatoryForMilestones);
					mandatoryLogCount = mandatoryScripts.Count();

					if (collectionRange.MonthlyRangeStartInclusive.HasValue)
					{
						AssertLogEntry(logEntries, "Collecting - MON - Monthly Allow Historical Data Feature", ref logIterator);
						AssertLogEntry(logEntries, "- 1 transaction(s) sent", ref logIterator);

						if (collectionRange.IsCurrentMonthlyCollection())
						{
							AssertLogEntry(logEntries, "Collecting - MCO - Monthly Current Data Only Feature", ref logIterator);
							AssertLogEntry(logEntries, "- 1 transaction(s) sent", ref logIterator);
						}
						else
						{
							AssertLogEntry(logEntries, "Collecting - MCO - Monthly Current Data Only Feature", ref logIterator);
						}
					}

					AssertLogEntry(logEntries, "Collecting - DAY - Daily Collection Feature", ref logIterator);
					AssertLogEntry(logEntries, "- 1 transaction(s) sent", ref logIterator);
				}

				if (collectionRange.StlMilestoneTimestamp.HasValue)
				{
					var expectedStlMilestoneLog = $"STL Milestone Created - {SqlFormatInfo.ToSqlDateString(collectionRange.StlMilestoneTimestamp.Value)} (DAY) MandatoryItemsCount({mandatoryLogCount}) Version({ReleaseInfo.Instance.VersionNumber})";
					AssertLogEntry(logEntries, expectedStlMilestoneLog, ref logIterator);
				}

				AssertLogEntry(logEntries, "Collecting - SPT - Snapshot Time Based", ref logIterator);
				AssertLogEntry(logEntries, "- 1 transaction(s) sent", ref logIterator);
				AssertLogEntry(logEntries, "Collecting - SPC - Snapshot Live Configuration", ref logIterator);
				AssertLogEntry(logEntries, "- 1 transaction(s) sent", ref logIterator);
			}
			AssertEquals("Log count", logIterator, logEntries.Count());
		}

		void AssertLogEntry(IEnumerable<string> logEntries, string expectedValue, ref int logIterator)
		{
			AssertEquals($"Log Entry ({logIterator})", expectedValue, logEntries.Skip(logIterator).First().Trim());
			logIterator++;
		}

		void AssertBillingTransactions(IDateTimeRange collectionRange, IEnumerable<IStlScriptWithConfig> scripts)
		{
			var billingTransactions = BillingTestHelper.GetBillingTransactionsFromDatabase(TestConnection);

			if (collectionRange.IsValid)
			{
				AssertEquals("Total transaction count >= 1", true, billingTransactions.Any());
				int detailTransactionCount = BillingTestHelper.AssertDetailRangeTransactions(billingTransactions, collectionRange);
				AssertEquals("Transactional Item - transaction count", 1, detailTransactionCount);

				if (collectionRange.StlMilestoneTimestamp.HasValue)
				{
					AssertEquals("Total transaction count >= 3", true, billingTransactions.Count() >= 3);
					int stlMilestoneTransactionCount = BillingTestHelper.AssertStlMilestoneTransactions(billingTransactions, collectionRange, scripts);
					AssertEquals("STL Milestone transaction count", 1, stlMilestoneTransactionCount);
					int dailyTransactionCount = BillingTestHelper.AssertDailyTransactions(billingTransactions, collectionRange);
					AssertEquals("Daily - transaction count", 1, dailyTransactionCount);

					if (collectionRange.MonthlyRangeStartInclusive.HasValue)
					{
						AssertEquals("Total transaction count >= 4", true, billingTransactions.Count() >= 4);
						int monthlyAllowHistoricalTransactionCount = BillingTestHelper.AssertMonthlyAllowHistoricalRangeTransactions(billingTransactions, collectionRange);
						AssertEquals("Monthly Allow Historical Item - transaction count", 1, monthlyAllowHistoricalTransactionCount);

						if (collectionRange.IsCurrentMonthlyCollection())
						{
							AssertEquals("Total transaction count", 5, billingTransactions.Count());
							int monthlyCurrentOnlyTransactionCount = BillingTestHelper.AssertMonthlyCurrentOnlyRangeTransactions(billingTransactions, collectionRange);
							AssertEquals("Monthly Current Only Item - transaction count", 1, monthlyCurrentOnlyTransactionCount);
						}
					}
				}
			}
			else
			{
				AssertEquals("Total transaction count", 0, billingTransactions.Count());
			}
		}

		public void TestCollectData_WithBlankBranchInformation()
		{
			CollectAndAssertBillingTransaction(
				originalCompany: "SIN",
				originalBranch: "",
				expectedTransacionCompany: "SIN",
				expectedTransacionBranch: null);
		}

		public void TestCollectData_WithDemoCompany()
		{
			CollectAndAssertBillingTransaction(
				originalCompany: "DEM",
				originalBranch: "ANY",
				expectedTransacionCompany: GlbCompany.CurrentCompany.GC_Code,
				expectedTransacionBranch: "ANY");
		}

		public void TestCollectData_WithCollectionQueryError()
		{
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Production))
			{
				var collector = new BillingDataCollectorForTest(new ScriptLoader(new ScriptFactoryForTest(new FeatureWithQueryError())));
				var utcNow = DateTime.UtcNow;

				collector.CollectAndSendData(CancellationToken.None, new RecurringRange(utcNow.AddHours(-2), utcNow.AddHours(-1)));
				// No billing transactions inserted to StmUsageData.
				AssertEquals("Billing Transaction count", 0, Convert.ToInt32(TestConnection.ExecuteScalar("SELECT count(*) FROM dbo.StmUsageData")));
				AssertEquals("Error was reported as developer exception", 1, ExceptionReporterTestListener.Instance.Count);
				AssertType<BillingException>(ExceptionReporterTestListener.Instance[0]);

				var expectedErrorMessage =
					"Data collection failed for item [~QE].\r\n"
					+ "Incorrect syntax near 'SELECT'.";

				AssertEquals("Query error", expectedErrorMessage, ExceptionReporterTestListener.Instance[0].Message);
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		[TestDate(2015, 12, 31, 18, 0, 0)]
		public void TestFailureOnOneItemWillNotStopOthersFromProgressing()
		{
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Production))
			{
				TestDateAttribute.UseUNLOCO = true;
				BillingTestHelper.StlCollectorHighWaterMarkRegistry = new DateTime(2015, 12, 31, 12, 0, 0);
				var collectionRange = new RecurringRange(new DateTime(2015, 12, 31, 12, 0, 0), new DateTime(2015, 12, 31, 13, 0, 0));
				var collector = new BillingDataCollectorForTest(new ScriptLoader(new ScriptFactoryForTest(new FeatureWithQueryError(), new FeatureForValidationTest("SIN", ""))));
				collector.CollectAndSendData(CancellationToken.None, collectionRange);
				AssertEquals("Error was reported as developer exception", 1, ExceptionReporterTestListener.Instance.Count);
				AssertType<BillingException>(ExceptionReporterTestListener.Instance[0]);
				ExceptionReporterTestListener.Instance.Clear();
				AssertTransactionLogsOneSuccessOneFailure(collector, expectMilestone: false);

				var billingTransactions = BillingTestHelper.GetBillingTransactionsFromDatabase(TestConnection);
				AssertEquals("Total transaction count", 1, billingTransactions.Count());
			}
		}

		[TestDate(2015, 12, 31, 18, 0, 0)]
		public void TestFailureOnMandatoryItemBlocksMilestones()
		{
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Production))
			{
				TestDateAttribute.UseUNLOCO = true;
				var startTime = new DateTime(2015, 12, 31, 12, 0, 0);
				SystemDataRegistry.Instance.GetStlCollectorHighWaterMark("~QE", "Feature With Query Error").SetValue(Guid.Empty, Guid.Empty, Guid.Empty, startTime);
				BillingTestHelper.StlCollectorHighWaterMarkRegistry = startTime;
				var collectionRange = new RecurringRange(startTime, new DateTime(2015, 12, 31, 13, 0, 0));
				var collector = new BillingDataCollectorForTest(new ScriptLoader(new ScriptFactoryForTest(new FeatureWithQueryError() { IsMandatoryForMilestones = true }, new FeatureForValidationTest("SIN", "") { IsMandatoryForMilestones = true })));
				collector.CollectAndSendData(CancellationToken.None, collectionRange);
				AssertEquals("Error was reported as developer exception", 1, ExceptionReporterTestListener.Instance.Count);
				AssertType<BillingException>(ExceptionReporterTestListener.Instance[0]);
				ExceptionReporterTestListener.Instance.Clear();
				AssertTransactionLogsOneSuccessOneFailure(collector, expectMilestone: false);

				var billingTransactions = BillingTestHelper.GetBillingTransactionsFromDatabase(TestConnection);
				AssertEquals("Total transaction count", 1, billingTransactions.Count());
			}
		}

		[TestDate(2015, 12, 31, 18, 0, 0)]
		public void TestFailureOnNonMandatoryItemDoesNotBlockMilestones()
		{
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Production))
			{
				TestDateAttribute.UseUNLOCO = true;
				var startTime = new DateTime(2015, 12, 31, 12, 0, 0);
				SystemDataRegistry.Instance.GetStlCollectorHighWaterMark("~QE", "Feature With Query Error").SetValue(Guid.Empty, Guid.Empty, Guid.Empty, startTime);
				BillingTestHelper.StlCollectorHighWaterMarkRegistry = startTime;
				var collectionRange = new RecurringRange(startTime, new DateTime(2015, 12, 31, 13, 0, 0));
				var collector = new BillingDataCollectorForTest(new ScriptLoader(new ScriptFactoryForTest(new FeatureWithQueryError(), new FeatureForValidationTest("SIN", "") { IsMandatoryForMilestones = true })));
				collector.CollectAndSendData(CancellationToken.None, collectionRange);
				AssertEquals("Error was reported as developer exception", 1, ExceptionReporterTestListener.Instance.Count);
				AssertType<BillingException>(ExceptionReporterTestListener.Instance[0]);
				ExceptionReporterTestListener.Instance.Clear();
				AssertTransactionLogsOneSuccessOneFailure(collector, expectMilestone: true);

				var billingTransactions = BillingTestHelper.GetBillingTransactionsFromDatabase(TestConnection);
				AssertEquals("Total transaction count", 2, billingTransactions.Count());
				int stlMilestoneTransactionCount = BillingTestHelper.AssertStlMilestoneTransactions(billingTransactions, collectionRange, collector.Scripts);
				AssertEquals("STL Milestone transaction count", 1, stlMilestoneTransactionCount);
			}
		}

		[TestDate(2015, 12, 31, 18, 0, 0)]
		public void TestFailureOnNewCollectorDoesNotBlockMilestones()
		{
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Production))
			{
				TestDateAttribute.UseUNLOCO = true;
				BillingTestHelper.StlCollectorHighWaterMarkRegistry = new DateTime(2015, 12, 31, 11, 0, 0);
				var collectionRange1 = new RecurringRange(new DateTime(2015, 12, 31, 11, 0, 0), new DateTime(2015, 12, 31, 12, 0, 0));
				var collector1 = new BillingDataCollectorForTest(new ScriptLoader(new ScriptFactoryForTest(new FeatureForValidationTest("SIN", "") { IsMandatoryForMilestones = true })));
				collector1.CollectAndSendData(CancellationToken.None, collectionRange1);
				AssertLogCount(2, collector1);
				AssertEquals("Collecting - ~BB - FeatureForValidationTest", collector1.Logs.First());
				AssertEquals("- 1 transaction(s) sent", collector1.Logs.Skip(1).First());

				var collectionRange2 = new RecurringRange(new DateTime(2015, 12, 31, 12, 0, 0), new DateTime(2015, 12, 31, 13, 0, 0));
				var collector2 = new BillingDataCollectorForTest(new ScriptLoader(new ScriptFactoryForTest(new FeatureWithQueryError() { IsMandatoryForMilestones = true }, new FeatureForValidationTest("SIN", "") { IsMandatoryForMilestones = true })));
				collector2.CollectAndSendData(CancellationToken.None, collectionRange2);
				AssertEquals("Error was reported as developer exception", 1, ExceptionReporterTestListener.Instance.Count);
				AssertType<BillingException>(ExceptionReporterTestListener.Instance[0]);
				ExceptionReporterTestListener.Instance.Clear();
				AssertTransactionLogsOneSuccessOneFailure(collector2, expectMilestone: true);

				var billingTransactions = BillingTestHelper.GetBillingTransactionsFromDatabase(TestConnection);
				AssertEquals("Total transaction count", 3, billingTransactions.Count());
				int stlMilestoneTransactionCount = BillingTestHelper.AssertStlMilestoneTransactions(billingTransactions, collectionRange2, collector2.Scripts);
				AssertEquals("STL Milestone transaction count", 1, stlMilestoneTransactionCount);
			}
		}

		[TestDate(2015, 12, 31, 18, 0, 0)]
		public void TestFailureOnCollectorThatHasAlreadyBeenGatheredDoesNotBlockMilestone()
		{
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Production))
			{
				TestDateAttribute.UseUNLOCO = true;
				SystemDataRegistry.Instance.GetStlCollectorHighWaterMark("~BB", "FeatureForValidationTest").SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2015, 12, 31, 12, 0, 0));
				SystemDataRegistry.Instance.GetStlCollectorHighWaterMark("~QE", "Feature With Query Error").SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2015, 12, 31, 13, 0, 0));

				var collectionRange2 = new RecurringRange(new DateTime(2015, 12, 31, 12, 0, 0), new DateTime(2015, 12, 31, 14, 0, 0));
				var collector2 = new BillingDataCollectorForTest(new ScriptLoader(new ScriptFactoryForTest(new FeatureWithQueryError() { IsMandatoryForMilestones = true }, new FeatureForValidationTest("SIN", "") { IsMandatoryForMilestones = true })));
				collector2.CollectAndSendData(CancellationToken.None, collectionRange2);
				AssertEquals("Error was reported as developer exception", 1, ExceptionReporterTestListener.Instance.Count);
				AssertType<BillingException>(ExceptionReporterTestListener.Instance[0]);
				ExceptionReporterTestListener.Instance.Clear();
				AssertTransactionLogsOneSuccessOneFailure(collector2, expectMilestone: true);

				var billingTransactions = BillingTestHelper.GetBillingTransactionsFromDatabase(TestConnection);
				AssertEquals("Total transaction count", 2, billingTransactions.Count());
				int stlMilestoneTransactionCount = BillingTestHelper.AssertStlMilestoneTransactions(billingTransactions, collectionRange2, collector2.Scripts);
				AssertEquals("STL Milestone transaction count", 1, stlMilestoneTransactionCount);
			}
		}

		[TestDate(2015, 12, 31, 18, 0, 0)]
		public void TestMilestonesNotReportedIfNoMandatoryItemsWereCollected()
		{
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Production))
			{
				TestDateAttribute.UseUNLOCO = true;
				BillingTestHelper.StlCollectorHighWaterMarkRegistry = new DateTime(2015, 12, 31, 12, 0, 0);
				var collectionRange = new RecurringRange(new DateTime(2015, 12, 31, 12, 0, 0), new DateTime(2015, 12, 31, 13, 0, 0));
				SystemDataRegistry.Instance.GetStlCollectorHighWaterMark("~BB", "Bla").SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collectionRange.EndDateTimeExclusive);
				var collector = new BillingDataCollectorForTest(new ScriptLoader(new ScriptFactoryForTest(new FeatureWithQueryError() { IsMandatoryForMilestones = false }, new FeatureForValidationTest("SIN", "") { IsMandatoryForMilestones = true })));
				collector.CollectAndSendData(CancellationToken.None, collectionRange);
				AssertEquals("Error was reported as developer exception", 1, ExceptionReporterTestListener.Instance.Count);
				AssertType<BillingException>(ExceptionReporterTestListener.Instance[0]);
				ExceptionReporterTestListener.Instance.Clear();

				AssertLogCount(2, collector);
				AssertEquals("Collecting - ~QE - FeatureWithQueryError", collector.Logs.First());
				AssertEquals("Error collecting STL data: Data collection failed for item [~QE].\r\nIncorrect syntax near 'SELECT'.", collector.Logs.Skip(1).First());
				var billingTransactions = BillingTestHelper.GetBillingTransactionsFromDatabase(TestConnection);
				AssertEquals("Total transaction count", 0, billingTransactions.Count());
				AssertEquals("STL Milestone transaction count", 0, billingTransactions.Count(t => t.PriceItemCode == "STL"));
			}
		}

		[TestDate(2015, 8, 31, 18, 0, 0)]
		public void TestCollectUsageTransactions()
		{
			TestDateAttribute.UseUNLOCO = true;
			BillingTestHelper.StlCollectorHighWaterMarkRegistry = new DateTime(2015, 8, 31, 13, 0, 0);
			var expectedTransaction = new UsageTransaction() { ServiceOccuredUTC = new DateTime(2015, 8, 31, 13, 0, 0), UsageCount = 5, AdditionalRefs = "Additional" };
			var collector = new BillingDataCollectorForTest(new ScriptLoader(new ScriptFactoryForTest(new UsageTranactionsTest(new UsageTransaction[1] { expectedTransaction }))));
			var collectionRange = new RecurringRange(new DateTime(2015, 8, 31, 13, 0, 0), new DateTime(2015, 8, 31, 14, 0, 0));
			collector.CollectAndSendData(CancellationToken.None, collectionRange);
			AssertLogCount(2, collector);
			Assert("Should contain TRN script", collector.Logs.First().Contains("Collecting - UTT - Usage Transaction Test"));
			Assert("Should contain TRN transaction count", collector.Logs.Skip(1).First().Contains("- 1 transaction(s) sent"));
			var usageTransactions = BillingTestHelper.GetUsageTransactionsFromDatabase(TestConnection);
			AssertEquals("Total transaction count", 1, usageTransactions.Count());
			AssertEquals("Transaction Matches", expectedTransaction, usageTransactions.Single());
		}

		void AssertTransactionLogsOneSuccessOneFailure(BillingDataCollectorForTest collector, bool expectMilestone)
		{
			AssertLogCount(expectMilestone ? 5 : 4, collector);
			var logIterator = 0;
			AssertLogEntry(collector.Logs, "Collecting - ~QE - FeatureWithQueryError", ref logIterator);
			AssertLogEntry(collector.Logs, "Error collecting STL data: Data collection failed for item [~QE].\r\nIncorrect syntax near 'SELECT'.", ref logIterator);
			AssertLogEntry(collector.Logs, "Collecting - ~BB - FeatureForValidationTest", ref logIterator);
			AssertLogEntry(collector.Logs, "- 1 transaction(s) sent", ref logIterator);
			if (expectMilestone)
			{
				var mandatoryScripts = collector.Scripts.Where(s => s.Script.IsMandatoryForMilestones);
				AssertLogEntry(collector.Logs, $"STL Milestone Created - 2015-12-31 (DAY) MandatoryItemsCount({mandatoryScripts.Count()}) Version({ReleaseInfo.Instance.VersionNumber})", ref logIterator);
			}
		}

		void AssertLogCount(int expected, BillingDataCollectorForTest collector)
		{
			AssertEquals($"Log line count incorrect, see all logs below: \r\n{string.Join("\r\n", collector.Logs.ToArray())}", expected, collector.Logs.Count());
		}

		void CollectAndAssertBillingTransaction(string originalCompany, string originalBranch, string expectedTransacionCompany, string expectedTransacionBranch)
		{
			DateTime baseUtc = DateTime.UtcNow.Date.AddHours(-3);
			BillingTestHelper.StlCollectorHighWaterMarkRegistry = baseUtc;

			var collector = new BillingDataCollectorForTest(new ScriptLoader(new ScriptFactoryForTest(new FeatureForValidationTest(originalCompany, originalBranch) { IsMandatoryForMilestones = true })));
			collector.CollectAndSendData(CancellationToken.None, new RecurringRange(baseUtc, baseUtc.AddHours(1)));

			AssertLogCount(2, collector);
			Assert("Should contain DAY script", collector.Logs.First().Equals("Collecting - ~BB - FeatureForValidationTest"));
			Assert("Should contain DAY script", collector.Logs.Skip(1).First().Equals("- 1 transaction(s) sent"));

			var transaction = BillingManager.DecryptTransaction(TestConnection.ExecuteScalar("SELECT SUD_Data FROM dbo.StmUsageData").ToString(), BillingManager.CurrentSchemaVersion);

			AssertEquals("Category", "STL", transaction.Category);
			AssertEquals("ReportingSource", BillingManager.ReportingSource, transaction.ReportingSource);
			AssertEquals("ClientID", "EDI" + expectedTransacionCompany + "DAT", transaction.ClientID);
			AssertEquals("ClientNumber", "J." + originalCompany, transaction.ClientNumber);
			AssertEquals("Branch", expectedTransacionBranch, transaction.Branch);
			AssertEquals("ClientStaffCode", "", transaction.ClientStaffCode);
			AssertEquals("Reference1", "TRANSACTIONAL_ForValidationTest", transaction.Reference1);
			AssertNull("Reference2", transaction.Reference2);
			AssertNull("Reference3", transaction.Reference3);
			AssertNull("Reference4", transaction.Reference4);
			AssertEquals("Reference5", "CA91BF57-5678-4872-923A-89885864BCD3", transaction.Reference5);
			AssertEquals("BillableCount", 99, transaction.BillableCount);

			AssertEquals(
				"ServiceOccuredUTC [" + SqlFormatInfo.ToSqlDateTimeString(transaction.ServiceOccuredUTC) + "] >= Base UTC [" + SqlFormatInfo.ToSqlDateTimeString(baseUtc) + "]?",
				true, transaction.ServiceOccuredUTC >= baseUtc);
		}

		#region Implementation

		public class FeatureWithQueryError : MockScript
		{
			public override string Code => "~QE";
			public override string Feature => "FeatureWithQueryError";
			public override StlDataGrain StlGrain => StlDataGrain.Transactional;
			public override string ScriptText => "SELECT";
			public override int TimeoutSecs => 30;

			public override IEnumerable<ZSqlParameter> GetInputParameters(IDateTimeRange dateTimeRange)
			{
				yield break;
			}

			public override IEnumerable<IStlTransaction> Run(IDateTimeRange dateTimeRange) => ScriptRunner.Run(this, dateTimeRange, new BillingTransactionFactory());
		}

		class FeatureForValidationTest : MockScript
		{
			public FeatureForValidationTest(string companyCode, string branchCode)
			{
				this.companyCode = companyCode;
				this.branchCode = branchCode;
			}

			public override string Code => "~BB";
			public override string Feature => "FeatureForValidationTest";
			public override StlDataGrain StlGrain => StlDataGrain.Transactional;

			public override string ScriptText
			{
				get
				{
					return string.Format(CultureInfo.InvariantCulture, @"
						SELECT
							CompanyCode = '{0}',
							BranchCode = '{1}',
							TransactionDateUtc = GETUTCDATE(),
							UserCode = '',
							TransactionReference01 = 'TRANSACTIONAL_ForValidationTest',
							TransactionReference02 = '',
							TransactionReference03 = '',
							TransactionReference04 = '',
							TransactionGuidReference = 'CA91BF57-5678-4872-923A-89885864BCD3',
							ItemCount = 99,
							AdditionalRefs = 0x",
						companyCode,
						branchCode
					);
				}
			}

			public override int TimeoutSecs => 30;
			public override StlDateType DateType { get => StlDateType.DateTime; }
			public override StlCollectorType CollectorType => StlCollectorType.Dynamic;

			public override IEnumerable<ZSqlParameter> GetInputParameters(IDateTimeRange dateTimeRange)
			{
				yield break;
			}

			public override IEnumerable<IStlTransaction> Run(IDateTimeRange dateTimeRange) => ScriptRunner.Run(this, dateTimeRange, new BillingTransactionFactory());

			readonly string companyCode;
			readonly string branchCode;
		}

		class UsageTranactionsTest : IStlItem
		{
			public UsageTranactionsTest(UsageTransaction[] usageTransactions)
			{
				this.usageTransactions = usageTransactions;
			}

			readonly UsageTransaction[] usageTransactions;

			public string Code => "UTT";
			public string Role => "Usage Transaction Test";
			public string Module => "Usage Transaction Test";
			public string Function => "Usage Transaction Test";
			public string Feature => "Usage Transaction Test";
			public StlDataGrain StlGrain => StlDataGrain.Transactional;
			public bool IsSystemLevel => false;
			public bool IsMandatoryForMilestones => false;
			public bool IsActive => true;
			public Exception CollectionException { get; set; }
			public bool CollectionOccurred { get; set; }
			public StlDateType DateType => StlDateType.DateTime;
			public StlCollectorType CollectorType => StlCollectorType.Custom;
			public DateTime CollectionStartDateUtc => DateTime.MinValue;

			public IEnumerable<ZSqlParameter> GetInputParameters(IDateTimeRange dateTimeRange)
			{
				yield break;
			}

			public IEnumerable<IStlTransaction> Run(IDateTimeRange dateTimeRange)
			{
				foreach (var usageTransaction in usageTransactions)
				{
					yield return usageTransaction;
				}
			}
		}

		#endregion
	}
}
