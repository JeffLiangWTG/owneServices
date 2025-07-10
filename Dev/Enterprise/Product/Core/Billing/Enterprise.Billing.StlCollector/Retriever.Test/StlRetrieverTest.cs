using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Billing.Integration;
using Enterprise.Billing.StlCollector.Retriever.Scripts;
using Enterprise.Billing.StlCollector.Retriever.Testing.Scripts;
using Enterprise.Customs.Universal;
using Enterprise.Integration;
using Enterprise.Integration.Billing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using TimeZoneInfo = System.TimeZoneInfo;

namespace Enterprise.Billing.StlCollector.Retriever.Testing
{
	public class StlRetrieverTest : TestCaseWithFactory
	{
		[TestDate(2014, 8, 15, 0, 59, 59)]
		public void TestCollectAndSend_DateRangeStartAndEndInTheSameMonth()
		{
			DateTime startDate = TestDateAttribute.Date.Date.AddDays(-5);
			DateTime expectedEndDateExclusive = TestDateAttribute.Date.AddHours(-2).AddSeconds(1);
			AssertCollectAndSend(startDate, expectedEndDateExclusive);
		}

		[TestDate(2014, 7, 2, 23, 59, 59)]
		public void TestCollectAndSend_DateRangeStartAndEndInDifferentMonths()
		{
			DateTime startDate = TestDateAttribute.Date.Date.AddDays(-4);
			DateTime expectedEndDateExclusive = TestDateAttribute.Date.AddHours(-2).AddSeconds(1);
			AssertCollectAndSend(startDate, expectedEndDateExclusive);
		}

		[TestDate(2014, 7, 15, 23, 59, 59)]
		public void TestCollectAndSend_DateRangeStartAndEndInDifferentMonthsEndMidMonth()
		{
			DateTime startDate = TestDateAttribute.Date.Date.AddDays(-19);
			DateTime expectedEndDateExclusive = TestDateAttribute.Date.AddHours(-2).AddSeconds(1);
			AssertCollectAndSend(startDate, expectedEndDateExclusive, expectedMilestoneOffset: 5);
		}

		[TestDate(2014, 8, 15, 0, 59, 59)]
		public void TestCollectAndSend_MandatoryItemsFirst()
		{
			DateTime startDate = TestDateAttribute.Date.Date.AddDays(-5);
			DateTime expectedEndDateExclusive = TestDateAttribute.Date.AddHours(-2).AddSeconds(1);
			BillingTestHelper.StlCollectorHighWaterMarkRegistry = startDate;
			var settings = new BusinessObjectFactory().Load<StmData>(new ZQuery(StmDataSchema.SD_Name, SQLComparisonOperator.Equal, SystemDataRegistry.StlCollectorHighWaterMarkPrefix));
			AssertEquals("Legacy Watermark should have set", 1, settings.Length);

			var stlRetriever = new StlRetrieverForTest(new ScriptLoader(new ScriptFactoryForTest(new[] { new TestTransactionalNonMandatoryStlItem(), new TestTransactionalStlItem() })));
			stlRetriever.CollectAndSend();

			var logEntryArray = stlRetriever.LogEntries.ToArray();
			AssertEquals("Log count > 1?", true, logEntryArray.Length > 1);

			int logIndex = 1;
			AssertCollectionLogEntriesMandatoryItemsFirst(startDate, expectedEndDateExclusive, "TRN", "Transactional Feature", StlDataGrain.Transactional, collectingMandatoryItems: true, logEntryArray, ref logIndex, stlRetriever.Scripts);
			AssertCollectionLogEntriesMandatoryItemsFirst(startDate, expectedEndDateExclusive, "TNM", "Transactional Non-Mandatory Feature", StlDataGrain.Transactional, collectingMandatoryItems: false, logEntryArray, ref logIndex, stlRetriever.Scripts);
			AssertEquals("Number of Log Entries", logIndex, logEntryArray.Length);

			var billingTransactions = BillingTestHelper.GetBillingTransactionsFromDatabase(TestConnection);
			var mandatoryRangeItemTransactions = billingTransactions.Where(t => t.PriceItemCode == "TRN").OrderBy(t => t.ServiceOccuredUTC);
			var nonMandatoryRangeItemTransactions = billingTransactions.Where(t => t.PriceItemCode == "TNM").OrderBy(t => t.ServiceOccuredUTC);
			var stlMilestoneTransactions = billingTransactions.Where(t => t.PriceItemCode == "STL").OrderBy(t => t.ServiceOccuredUTC);

			AssertEquals("Any TRN transactions found?", true, mandatoryRangeItemTransactions.Any());
			AssertEquals("Any TNM transactions found?", true, nonMandatoryRangeItemTransactions.Any());
			AssertEquals("STL Milestone dummy transactions found?", true, stlMilestoneTransactions.Any());
			AssertEquals("All transaction count", billingTransactions.Count(), mandatoryRangeItemTransactions.Count() + nonMandatoryRangeItemTransactions.Count() + stlMilestoneTransactions.Count());
			AssertDailyTransactions(startDate, expectedEndDateExclusive, mandatoryRangeItemTransactions);
			AssertDailyTransactions(startDate, expectedEndDateExclusive, nonMandatoryRangeItemTransactions);
			AssertMilestoneTransactions(startDate, stlMilestoneTransactions, stlRetriever.Scripts);

			settings = new BusinessObjectFactory().Load<StmData>(new ZQuery(StmDataSchema.SD_Name, SQLComparisonOperator.Equal, SystemDataRegistry.StlCollectorHighWaterMarkPrefix));
			AssertEquals("Legacy Watermark should have been cleared", 0, settings.Length);
		}

		[TestDate(2014, 8, 15, 12, 0, 0)]
		public void TestCollectAndSend_AdditionalRefsPopulated()
		{
			DateTime startDate = TestDateAttribute.Date.Date.AddDays(-1);
			DateTime expectedEndDateExclusive = TestDateAttribute.Date.AddHours(-1);
			BillingTestHelper.StlCollectorHighWaterMarkRegistry = startDate;
			var settings = new BusinessObjectFactory().Load<StmData>(new ZQuery(StmDataSchema.SD_Name, SQLComparisonOperator.Equal, SystemDataRegistry.StlCollectorHighWaterMarkPrefix));
			AssertEquals("Legacy Watermark should have set", 1, settings.Length);

			var dummyEntry1 = Factory.New<DummyBusinessObject>();
			dummyEntry1.Z0_Date = startDate.AddHours(5);
			var binaryData1 = "BinaryData1";
			dummyEntry1.Z0_VarBinaryMax = new UTF8Encoding(false).GetBytes(binaryData1);
			var dummyEntry2 = Factory.New<DummyBusinessObject>();
			dummyEntry2.Z0_Date = startDate.AddHours(5).AddDays(1);
			var binaryData2 = "dddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd";
			dummyEntry2.Z0_VarBinaryMax = new UTF8Encoding(false).GetBytes(binaryData2);
			Factory.Save();

			var stlRetriever = new StlRetrieverForTest(new ScriptLoader(new ScriptFactoryForTest(new[] { new RefStlScriptRetriever(new StlScriptTests.DummyScript()) })));
			stlRetriever.CollectAndSend();

			var logEntryArray = stlRetriever.LogEntries.ToArray();
			AssertEquals("Log count > 1?", true, logEntryArray.Length > 1);

			int logIndex = 1;
			AssertCollectionLogEntriesMandatoryItemsFirst(startDate, expectedEndDateExclusive, "DUM", "Dummy", StlDataGrain.Transactional, collectingMandatoryItems: true, logEntryArray, ref logIndex, stlRetriever.Scripts);
			AssertEquals("Number of Log Entries", logIndex, logEntryArray.Length);

			var billingTransactions = BillingTestHelper.GetBillingTransactionsFromDatabase(TestConnection);
			var mandatoryRangeItemTransactions = billingTransactions.Where(t => t.PriceItemCode == "DUM").OrderBy(t => t.ServiceOccuredUTC);
			var stlMilestoneTransactions = billingTransactions.Where(t => t.PriceItemCode == "STL").OrderBy(t => t.ServiceOccuredUTC);

			AssertEquals("Any DUM transactions found?", true, mandatoryRangeItemTransactions.Any());
			AssertEquals("STL Milestone dummy transactions found?", true, stlMilestoneTransactions.Any());
			AssertEquals("All transaction count", billingTransactions.Count(), mandatoryRangeItemTransactions.Count() + stlMilestoneTransactions.Count());
			AssertMilestoneTransactions(startDate, stlMilestoneTransactions, stlRetriever.Scripts);
			AssertEquals(binaryData1, mandatoryRangeItemTransactions.First().AdditionalRefs);
			AssertEquals(binaryData2, mandatoryRangeItemTransactions.Skip(1).First().AdditionalRefs);

			settings = new BusinessObjectFactory().Load<StmData>(new ZQuery(StmDataSchema.SD_Name, SQLComparisonOperator.Equal, SystemDataRegistry.StlCollectorHighWaterMarkPrefix));
			AssertEquals("Legacy Watermark should have been cleared", 0, settings.Length);
		}

		[TestDate(2014, 8, 15, 12, 0, 0)]
		public void TestCollectAndSend_Static_DateTimeOffset()
		{
			AssertCollectAndSend_DateTimeOffset(new RefStlScriptRetriever(new StlScriptTests.DummyDateTimeOffsetScript()));
		}

		void AssertCollectAndSend_DateTimeOffset(IStlScript script)
		{
			DateTime startDate = TestDateAttribute.Date.Date.AddDays(-1);
			DateTime expectedEndDateExclusive = TestDateAttribute.Date.AddHours(-1);
			BillingTestHelper.StlCollectorHighWaterMarkRegistry = startDate;
			var settings = new BusinessObjectFactory().Load<StmData>(new ZQuery(StmDataSchema.SD_Name, SQLComparisonOperator.Equal, SystemDataRegistry.StlCollectorHighWaterMarkPrefix));
			AssertEquals("Legacy Watermark should have set", 1, settings.Length);

			var dummyEntry1 = Factory.New<DummyBusinessObject>();
			dummyEntry1.Z0_DateTimeOffset = startDate.AddHours(5);
			var dummyEntry2 = Factory.New<DummyBusinessObject>();
			dummyEntry2.Z0_DateTimeOffset = startDate.AddHours(5).AddDays(1);
			Factory.Save();

			var stlRetriever = new StlRetrieverForTest(new ScriptLoader(new ScriptFactoryForTest(new[] { script })));
			stlRetriever.CollectAndSend();

			var logEntryArray = stlRetriever.LogEntries.ToArray();
			AssertEquals("Log count > 1?", true, logEntryArray.Length > 1);

			int logIndex = 1;
			AssertCollectionLogEntriesMandatoryItemsFirst(startDate, expectedEndDateExclusive, script.Code, script.Feature, script.StlGrain, collectingMandatoryItems: true, logEntryArray, ref logIndex, stlRetriever.Scripts);
			AssertEquals("Number of Log Entries", logIndex, logEntryArray.Length);

			var billingTransactions = BillingTestHelper.GetBillingTransactionsFromDatabase(TestConnection);
			var mandatoryRangeItemTransactions = billingTransactions.Where(t => t.PriceItemCode == script.Code).OrderBy(t => t.ServiceOccuredUTC);
			var stlMilestoneTransactions = billingTransactions.Where(t => t.PriceItemCode == "STL").OrderBy(t => t.ServiceOccuredUTC);

			var expectedTransactions = (script.StlGrain == StlDataGrain.Daily) ? 1 : 2;
			AssertEquals("Transactions found?", expectedTransactions, mandatoryRangeItemTransactions.Count());
			AssertEquals("STL Milestone dummy transactions found?", true, stlMilestoneTransactions.Any());
			AssertEquals("All transaction count", billingTransactions.Count(), mandatoryRangeItemTransactions.Count() + stlMilestoneTransactions.Count());
			AssertMilestoneTransactions(startDate, stlMilestoneTransactions, stlRetriever.Scripts);
			AssertEquals(startDate.AddHours(5), mandatoryRangeItemTransactions.First().ServiceOccuredUTC);
			if (expectedTransactions > 1)
			{
				AssertEquals(startDate.AddHours(5).AddDays(1), mandatoryRangeItemTransactions.Skip(1).First().ServiceOccuredUTC);
			}
			settings = new BusinessObjectFactory().Load<StmData>(new ZQuery(StmDataSchema.SD_Name, SQLComparisonOperator.Equal, SystemDataRegistry.StlCollectorHighWaterMarkPrefix));
			AssertEquals("Legacy Watermark should have been cleared", 0, settings.Length);
		}

		[TestDate(2014, 7, 1, 1, 59, 59)]
		public void TestCollectAndSend_DailyGranuality_EndOfMonth()
		{
			DateTime startDate = TestDateAttribute.Date.Date.AddHours(-11);
			DateTime expectedEndDateExclusive = TestDateAttribute.Date.AddHours(-2).AddSeconds(1);
			BillingTestHelper.StlCollectorHighWaterMarkRegistry = startDate;
			var settings = new BusinessObjectFactory().Load<StmData>(new ZQuery(StmDataSchema.SD_Name, SQLComparisonOperator.Equal, SystemDataRegistry.StlCollectorHighWaterMarkPrefix));
			AssertEquals("Legacy Watermark should have set", 1, settings.Length);

			var dummyEntry1 = Factory.New<DummyBusinessObject>();
			dummyEntry1.Z0_Date = startDate.AddDays(-1);
			var binaryData1 = "BinaryData1";
			dummyEntry1.Z0_VarBinaryMax = new UTF8Encoding(false).GetBytes(binaryData1);
			var dummyEntry2 = Factory.New<DummyBusinessObject>();
			dummyEntry2.Z0_Date = startDate.AddHours(-7);
			var binaryData2 = "BinaryData2";
			dummyEntry2.Z0_VarBinaryMax = new UTF8Encoding(false).GetBytes(binaryData2);
			Factory.Save();

			var dummyScript = new StlScriptTests.DummyScript();
			dummyScript.StlItemGrainOverride = StlDataGrain.Daily;
			var stlRetriever = new StlRetrieverForTest(new ScriptLoader(new ScriptFactoryForTest(new[] { new RefStlScriptRetriever(dummyScript) })));
			stlRetriever.CollectAndSend();

			var logEntryArray = stlRetriever.LogEntries.ToArray();
			AssertEquals("Log count > 1?", true, logEntryArray.Length > 1);

			int logIndex = 1;
			AssertCollectionLogEntriesMandatoryItemsFirst(startDate, expectedEndDateExclusive, "DUM", "Dummy", StlDataGrain.Daily, collectingMandatoryItems: true, logEntryArray, ref logIndex, stlRetriever.Scripts);
			AssertEquals("Number of Log Entries", logIndex, logEntryArray.Length);

			var billingTransactions = BillingTestHelper.GetBillingTransactionsFromDatabase(TestConnection);
			var mandatoryRangeItemTransactions = billingTransactions.Where(t => t.PriceItemCode == "DUM").OrderBy(t => t.ServiceOccuredUTC);
			var stlMilestoneTransactions = billingTransactions.Where(t => t.PriceItemCode == "STL").OrderBy(t => t.ServiceOccuredUTC);

			AssertEquals("STL Milestone dummy transactions found?", true, stlMilestoneTransactions.Any());
			AssertEquals("All transaction count", billingTransactions.Count(), mandatoryRangeItemTransactions.Count() + stlMilestoneTransactions.Count());
			AssertMilestoneTransactions(startDate, stlMilestoneTransactions, stlRetriever.Scripts);
			AssertEquals("Transactions count", 1, mandatoryRangeItemTransactions.Count());
			AssertEquals(binaryData2, mandatoryRangeItemTransactions.First().AdditionalRefs);

			settings = new BusinessObjectFactory().Load<StmData>(new ZQuery(StmDataSchema.SD_Name, SQLComparisonOperator.Equal, SystemDataRegistry.StlCollectorHighWaterMarkPrefix));
			AssertEquals("Legacy Watermark should have been cleared", 0, settings.Length);
		}

		[TestDate(2014, 6, 10)]
		public void TestCollectAndSend_InvalidDateRange()
		{
			AssertCollectAndSend(
				startDate: TestDateAttribute.Date.Date.AddDays(2),
				expectedEndDateExclusive: TestDateAttribute.Date.AddHours(-1),
				legacyWaterMarkShouldHaveCleared: false
			);
		}

		[TestDate(2014, 5, 10, 1, 0, 0)]
		public void TestCollectAndSend_OneHourClearOfMidnight()
		{
			AssertCollectAndSend(
				startDate: TestDateAttribute.Date.Date.AddDays(-1),
				expectedEndDateExclusive: TestDateAttribute.Date.Date
			);
		}

		[ExpectNoExceptions]
		[TestDate(2014, 5, 10, 1, 0, 0)]
		public void TestSqlTimeoutProductionCausesIssueReport()
		{
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Production))
			{
				var sqlTimeoutException = SqlExceptionBuilder.CreateSqlException(-2, "Execution timeout expired");
				var stlRetriever = new StlRetrieverForTest(new BillingDataCollectorThatThrowsExceptionForTestFactory(sqlTimeoutException));
				stlRetriever.CollectAndSend();
				AssertEquals("Errors were reported as developer exception", 6, ExceptionReporterTestListener.Instance.Count);
				AssertType<BillingException>(ExceptionReporterTestListener.Instance[0]);
				AssertEquals("Wrong exception key", "Error collecting STL data: Data collection failed for item [MON].\r\nExecution timeout expired", ExceptionReporterTestListener.Instance.GetExceptionKey(0));
				AssertType<BillingException>(ExceptionReporterTestListener.Instance[1]);
				AssertEquals("Wrong exception key", "Error collecting STL data: Data collection failed for item [MCO].\r\nExecution timeout expired", ExceptionReporterTestListener.Instance.GetExceptionKey(1));
				AssertType<BillingException>(ExceptionReporterTestListener.Instance[2]);
				AssertEquals("Wrong exception key", "Error collecting STL data: Data collection failed for item [TRN].\r\nExecution timeout expired", ExceptionReporterTestListener.Instance.GetExceptionKey(2));
				AssertType<BillingException>(ExceptionReporterTestListener.Instance[3]);
				AssertEquals("Wrong exception key", "Error collecting STL data: Data collection failed for item [DAY].\r\nExecution timeout expired", ExceptionReporterTestListener.Instance.GetExceptionKey(3));
				AssertType<BillingException>(ExceptionReporterTestListener.Instance[4]);
				AssertEquals("Wrong exception key", "Error collecting STL data: Data collection failed for item [SPT].\r\nExecution timeout expired", ExceptionReporterTestListener.Instance.GetExceptionKey(4));
				AssertType<BillingException>(ExceptionReporterTestListener.Instance[5]);
				AssertEquals("Wrong exception key", "Error collecting STL data: Data collection failed for item [SPC].\r\nExecution timeout expired", ExceptionReporterTestListener.Instance.GetExceptionKey(5));
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		[ExpectNoExceptions]
		[TestDate(2014, 5, 10, 1, 0, 0)]
		public void TestSqlTimeoutNonProductionDoesCauseIssueReport()
		{
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Test))
			{
				var sqlTimeoutException = SqlExceptionBuilder.CreateSqlException(-2, "Execution timeout expired");
				var stlRetriever = new StlRetrieverForTest(new BillingDataCollectorThatThrowsExceptionForTestFactory(sqlTimeoutException));
				stlRetriever.CollectAndSend();
				AssertEquals("Errors were reported as developer exception", 6, ExceptionReporterTestListener.Instance.Count);
				AssertType<BillingException>(ExceptionReporterTestListener.Instance[0]);
				AssertEquals("Wrong exception key", "Error collecting STL data: Data collection failed for item [MON].\r\nExecution timeout expired", ExceptionReporterTestListener.Instance.GetExceptionKey(0));
				AssertType<BillingException>(ExceptionReporterTestListener.Instance[1]);
				AssertEquals("Wrong exception key", "Error collecting STL data: Data collection failed for item [MCO].\r\nExecution timeout expired", ExceptionReporterTestListener.Instance.GetExceptionKey(1));
				AssertType<BillingException>(ExceptionReporterTestListener.Instance[2]);
				AssertEquals("Wrong exception key", "Error collecting STL data: Data collection failed for item [TRN].\r\nExecution timeout expired", ExceptionReporterTestListener.Instance.GetExceptionKey(2));
				AssertType<BillingException>(ExceptionReporterTestListener.Instance[3]);
				AssertEquals("Wrong exception key", "Error collecting STL data: Data collection failed for item [DAY].\r\nExecution timeout expired", ExceptionReporterTestListener.Instance.GetExceptionKey(3));
				AssertType<BillingException>(ExceptionReporterTestListener.Instance[4]);
				AssertEquals("Wrong exception key", "Error collecting STL data: Data collection failed for item [SPT].\r\nExecution timeout expired", ExceptionReporterTestListener.Instance.GetExceptionKey(4));
				AssertType<BillingException>(ExceptionReporterTestListener.Instance[5]);
				AssertEquals("Wrong exception key", "Error collecting STL data: Data collection failed for item [SPC].\r\nExecution timeout expired", ExceptionReporterTestListener.Instance.GetExceptionKey(5));
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		[ExpectNoExceptions]
		[TestDate(2014, 5, 10, 1, 0, 0)]
		public void TestRefDataExceptionProductionDoesNotCauseIssueReport()
		{
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Production))
			{
				var zBlobException = new ZBlobReadException("Error reading blob field STL_FromClause", new InvalidOperationException("Bang"));
				var refDataException = new RefDataException("Temporary exception caused by failover operation, will self-recover in next try", zBlobException, false);
				var stlRetriever = new StlRetrieverForTest(new BillingDataCollectorThatThrowsExceptionForTestFactory(refDataException));
				stlRetriever.CollectAndSend();
				AssertEquals("Error was not reported as developer exception", 0, ExceptionReporterTestListener.Instance.Count);
			}
		}

		[ExpectNoExceptions]
		[TestDate(2014, 5, 10, 1, 0, 0)]
		public void TestRefDataExceptionNonProductionDoesNotCauseIssueReport()
		{
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Test))
			{
				var zBlobException = new ZBlobReadException("Error reading blob field STL_FromClause", new InvalidOperationException("Bang"));
				var refDataException = new RefDataException("Temporary exception caused by failover operation, will self-recover in next try", zBlobException, false);
				var stlRetriever = new StlRetrieverForTest(new BillingDataCollectorThatThrowsExceptionForTestFactory(refDataException));
				stlRetriever.CollectAndSend();
				AssertEquals("Error was not reported as developer exception", 0, ExceptionReporterTestListener.Instance.Count);
			}
		}

		[ExpectNoExceptions]
		[TestDate(2014, 5, 10, 1, 0, 0)]
		public void TestInvalidOperationInternalTestSystemDoesNotCauseIssueReport()
		{
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Test, isInternalSystem: true))
			{
				var stlRetriever = new StlRetrieverForTest(new BillingDataCollectorThatThrowsExceptionForTestFactory(new InvalidOperationException("Bang")));
				stlRetriever.CollectAndSend();
				AssertEquals("Error was not reported as developer exception", 0, ExceptionReporterTestListener.Instance.Count);
			}
		}

		[ExpectNoExceptions]
		[TestDate(2014, 5, 10, 1, 0, 0)]
		public void TestInvalidOperationInternalProductionSystemDoesNotCauseIssueReport()
		{
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Production, isInternalSystem: true))
			{
				var stlRetriever = new StlRetrieverForTest(new BillingDataCollectorThatThrowsExceptionForTestFactory(new InvalidOperationException("Bang")));
				stlRetriever.CollectAndSend();
				AssertEquals("Error was not reported as developer exception", 0, ExceptionReporterTestListener.Instance.Count);
			}
		}

		[TestDate(2014, 7, 4, 23, 59, 59)]
		public void TestCollectAndSend_SkipsFailingOrAlreadyWatermarkedItems()
		{
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Production))
			{
				var startDate = TestDateAttribute.Date.Date.AddDays(-4);
				var expectedEndDateExclusive = TestDateAttribute.Date.AddHours(-2).AddSeconds(1);
				var failedItem = new ScriptWithConfig(new BillingDataCollectorTest.FeatureWithQueryError() { IsMandatoryForMilestones = false }, new StlItemRegistrySettingsStub() { HighWaterMark = startDate });
				var monthlyHistItem = new ScriptWithConfig(new TestMonthlyAllowHistoricalStlItem(), new StlItemRegistrySettingsStub() { HighWaterMark = startDate });
				var monthlyCurrItem = new ScriptWithConfig(new TestMonthlyCurrentStlItem(), new StlItemRegistrySettingsStub() { HighWaterMark = startDate.AddDays(1) });
				var tranItem = new ScriptWithConfig(new TestTransactionalStlItem(), new StlItemRegistrySettingsStub() { HighWaterMark = startDate.AddDays(2) });
				var stlRetriever = new StlRetrieverForTest(new LoggerForTest(), new BillingDataCollectorForTestFactory(), new ScriptLoaderForTest(new IStlScriptWithConfig[] { failedItem, monthlyHistItem, monthlyCurrItem, tranItem }));
				stlRetriever.CollectAndSend();
				AssertEquals("Error was reported as developer exception", 1, ExceptionReporterTestListener.Instance.Count);
				AssertType<BillingException>(ExceptionReporterTestListener.Instance[0]);
				ExceptionReporterTestListener.Instance.Clear();

				var billingTransactions = BillingTestHelper.GetBillingTransactionsFromDatabase(TestConnection);
				var detailRangeItemTransactions = billingTransactions.Where(t => t.PriceItemCode == "TRN").OrderBy(t => t.ServiceOccuredUTC);
				var monthlyAllowHistoricalItemTransactions = billingTransactions.Where(t => t.PriceItemCode == "MON").OrderBy(t => t.ServiceOccuredUTC);
				var monthlyCurrentOnlyItemTransactions = billingTransactions.Where(t => t.PriceItemCode == "MCO").OrderBy(t => t.ServiceOccuredUTC);
				var stlMilestoneTransactions = billingTransactions.Where(t => t.PriceItemCode == "STL").OrderBy(t => t.ServiceOccuredUTC);

				AssertEquals("Range transaction Count", 3, detailRangeItemTransactions.Count());
				AssertEquals("Monthly Allow Historical transactions count", 1, monthlyAllowHistoricalItemTransactions.Count());
				AssertEquals("Monthly Current Only transactions Count", 0, monthlyCurrentOnlyItemTransactions.Count());
				AssertEquals("STL Milestone dummy transactions Count", 4, stlMilestoneTransactions.Count());
				AssertEquals("All transaction count",
					billingTransactions.Count(),
					detailRangeItemTransactions.Count() + monthlyAllowHistoricalItemTransactions.Count() + monthlyCurrentOnlyItemTransactions.Count() + stlMilestoneTransactions.Count());
				AssertEquals("Monthly Historical New Watermark", expectedEndDateExclusive, monthlyHistItem.HighWaterMarkSettings.HighWaterMark);
				AssertEquals("Monthly Current New Watermark", expectedEndDateExclusive, monthlyCurrItem.HighWaterMarkSettings.HighWaterMark);
				AssertEquals("Monthly Historical New Watermark", expectedEndDateExclusive, tranItem.HighWaterMarkSettings.HighWaterMark);
				AssertEquals("Query Error Watermark", startDate, failedItem.HighWaterMarkSettings.HighWaterMark);
			}
		}

		[TestDate(2020, 10, 25, 11, 30, 00)]
		public void TestCollectAndSend_FullCycleDBHits()
		{
			using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTestingWithALPVersionBasedOffDateNow()))
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Production, isInternalSystem: false))
			using (RawDataRegistry.Instance.ObtainDynamicSTLCollectorDefinitionsFromTheCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var scriptLoader = new ScriptLoader();
				var retriever = new StlRetrieverForTest(new LoggerForTest(), new BillingDataCollectorForTestFactory(), scriptLoader);
				var scripts = scriptLoader.Load(Factory);

				AssertEquals("Wrong value for legacy watermark before test", DateTime.MinValue, SystemDataRegistry.Instance.StlCollectorHighWaterMark.Value);
				foreach (var script in retriever.Scripts.Select(t => t.Script))
				{
					AssertEquals("Wrong value for watermark before test", DateTime.MinValue, SystemDataRegistry.Instance.GetStlCollectorHighWaterMark(script.Code, script.Feature).Value);
				}

				var countBefore = Db.Connection.ExecutedCommandCount;
				retriever.CollectAndSend(CancellationToken.None);
				var countAfter = Db.Connection.ExecutedCommandCount;

				AssertEquals("Wrong value for legacy watermark after test", DateTime.MinValue, SystemDataRegistry.Instance.StlCollectorHighWaterMark.Value);
				foreach (var script in retriever.Scripts.Select(t => t.Script))
				{
					var expectedWatermark = script.CollectionStartDateUtc != DateTime.MinValue ? DateTime.MinValue : new DateTime(2020, 10, 25, 10, 0, 0);
					AssertEquals("Wrong value for watermark after test", expectedWatermark, SystemDataRegistry.Instance.GetStlCollectorHighWaterMark(script.Code, script.Feature).Value);
				}
				var expectedCount = countBefore + scripts.Count() * 122;
				AssertLessThan(countAfter, expectedCount);
			}
		}

		[TestDate(2014, 7, 4, 23, 59, 59)]
		public void TestCollectAndSendWithCollectionStartTimeConfiguredAndNoWatermark()
		{
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Production))
			{
				var startDate = TestDateAttribute.Date.Date.AddDays(-4);
				var expectedEndDateExclusive = TestDateAttribute.Date.AddHours(-2).AddSeconds(1);
				var tranItem = new ScriptWithConfig(new TestTransactionalStlItem() { CollectionStartDateUtcOverride = startDate.AddDays(2) }, new StlItemRegistrySettingsStub());
				var monthlyHistItem = new ScriptWithConfig(new TestMonthlyAllowHistoricalStlItem(), new StlItemRegistrySettingsStub() { HighWaterMark = startDate });
				var stlRetriever = new StlRetrieverForTest(new LoggerForTest(), new BillingDataCollectorForTestFactory(), new ScriptLoaderForTest(new IStlScriptWithConfig[] { tranItem, monthlyHistItem }));
				stlRetriever.CollectAndSend();

				var utcNow = EnvProxy.Instance.Time.CurrentUtcDateTime;
				var billingTransactions = BillingTestHelper.GetBillingTransactionsFromDatabase(TestConnection);
				var detailRangeItemTransactions = billingTransactions.Where(t => t.PriceItemCode == "TRN").OrderBy(t => t.ServiceOccuredUTC);
				var stlMilestoneTransactions = billingTransactions.Where(t => t.PriceItemCode == "STL").OrderBy(t => t.ServiceOccuredUTC);
				var monthlyAllowHistoricalItemTransactions = billingTransactions.Where(t => t.PriceItemCode == "MON").OrderBy(t => t.ServiceOccuredUTC);

				AssertEquals("Range transaction Count", 3, detailRangeItemTransactions.Count());
				AssertEquals("STL Milestone dummy transactions Count", 5, stlMilestoneTransactions.Count());
				AssertEquals("Monthly Allow Historical transactions count", 1, monthlyAllowHistoricalItemTransactions.Count());
				AssertEquals("All transaction count", billingTransactions.Count(), detailRangeItemTransactions.Count() + stlMilestoneTransactions.Count() + monthlyAllowHistoricalItemTransactions.Count());
				AssertEquals("New Watermark", expectedEndDateExclusive, tranItem.HighWaterMarkSettings.HighWaterMark);
			}
		}

		[TestDate(2014, 7, 4, 23, 59, 59)]
		public void TestCollectAndSendWithCollectionStartTimeConfiguredEarlierThanWatermark()
		{
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Production))
			{
				var startDate = TestDateAttribute.Date.Date.AddDays(-4);
				var expectedEndDateExclusive = TestDateAttribute.Date.AddHours(-2).AddSeconds(1);
				var tranItem = new ScriptWithConfig(new TestTransactionalStlItem() { CollectionStartDateUtcOverride = startDate.AddDays(2) }, new StlItemRegistrySettingsStub() { HighWaterMark = startDate.AddDays(3) });
				var stlRetriever = new StlRetrieverForTest(new LoggerForTest(), new BillingDataCollectorForTestFactory(), new ScriptLoaderForTest(new IStlScriptWithConfig[] { tranItem }));
				stlRetriever.CollectAndSend();

				var billingTransactions = BillingTestHelper.GetBillingTransactionsFromDatabase(TestConnection);
				var detailRangeItemTransactions = billingTransactions.Where(t => t.PriceItemCode == "TRN").OrderBy(t => t.ServiceOccuredUTC);
				var stlMilestoneTransactions = billingTransactions.Where(t => t.PriceItemCode == "STL").OrderBy(t => t.ServiceOccuredUTC);

				AssertEquals("Range transaction Count", 2, detailRangeItemTransactions.Count());
				AssertEquals("STL Milestone dummy transactions Count", 2, stlMilestoneTransactions.Count());
				AssertEquals("All transaction count", billingTransactions.Count(), detailRangeItemTransactions.Count() + stlMilestoneTransactions.Count());
				AssertEquals("New Watermark", expectedEndDateExclusive, tranItem.HighWaterMarkSettings.HighWaterMark);
			}
		}

		[TestDate(2014, 7, 4, 23, 59, 59)]
		public void TestCollectAndSendWithCollectionStartTimeConfiguredOlderThan3MonthsCustomerSystem()
		{
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Production))
			{
				var expectedEndDateExclusive = TestDateAttribute.Date.AddHours(-2).AddSeconds(1);
				var tranItem = new ScriptWithConfig(new TestTransactionalStlItem() { CollectionStartDateUtcOverride = TestDateAttribute.Date.Date.AddMonths(-5) }, new StlItemRegistrySettingsStub());
				var stlRetriever = new StlRetrieverForTest(new LoggerForTest(), new BillingDataCollectorForTestFactory(), new ScriptLoaderForTest(new IStlScriptWithConfig[] { tranItem }));
				stlRetriever.CollectAndSend();

				var billingTransactions = BillingTestHelper.GetBillingTransactionsFromDatabase(TestConnection);
				var detailRangeItemTransactions = billingTransactions.Where(t => t.PriceItemCode == "TRN").OrderBy(t => t.ServiceOccuredUTC);
				var stlMilestoneTransactions = billingTransactions.Where(t => t.PriceItemCode == "STL").OrderBy(t => t.ServiceOccuredUTC);

				AssertEquals("Range transaction Count", 151, detailRangeItemTransactions.Count());
				AssertEquals("STL Milestone dummy transactions Count", 151, stlMilestoneTransactions.Count());
				AssertEquals("All transaction count", billingTransactions.Count(), detailRangeItemTransactions.Count() + stlMilestoneTransactions.Count());
				AssertEquals("New Watermark", expectedEndDateExclusive, tranItem.HighWaterMarkSettings.HighWaterMark);
			}
		}

		[TestDate(2014, 7, 4, 23, 59, 59)]
		public void TestCollectAndSendWithCollectionStartTimeConfiguredOlderThan3MonthsSnapshot()
		{
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Production))
			{
				var snapShotItem = new ScriptWithConfig(new TestSnapshotTimeBased(), new StlItemRegistrySettingsStub());
				var stlRetriever = new StlRetrieverForTest(new LoggerForTest(), new BillingDataCollectorForTestFactory(), new ScriptLoaderForTest(new IStlScriptWithConfig[] { snapShotItem }));
				stlRetriever.CollectAndSend();

				var usageTransactions = BillingTestHelper.GetUsageTransactionsFromDatabase(TestConnection);
				AssertEquals("Range transaction Count", 308, usageTransactions.Count());
				AssertEquals("New Watermark", DateTime.MaxValue, snapShotItem.HighWaterMarkSettings.HighWaterMark);
			}
		}

		[TestDate(2014, 7, 4, 23, 59, 59)]
		public void TestCollectAndSendWithCollectionStartTimeConfiguredOlderThan3MonthsInternalSystem()
		{
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Production, isInternalSystem: true))
			{
				var expectedEndDateExclusive = TestDateAttribute.Date.AddHours(-2).AddSeconds(1);
				var tranItem = new ScriptWithConfig(new TestTransactionalStlItem() { CollectionStartDateUtcOverride = TestDateAttribute.Date.Date.AddMonths(-5) }, new StlItemRegistrySettingsStub());
				var stlRetriever = new StlRetrieverForTest(new LoggerForTest(), new BillingDataCollectorForTestFactory(), new ScriptLoaderForTest(new IStlScriptWithConfig[] { tranItem }));
				stlRetriever.CollectAndSend();

				var utcNow = EnvProxy.Instance.Time.CurrentUtcDateTime;
				var billingTransactions = BillingTestHelper.GetBillingTransactionsFromDatabase(TestConnection);
				var detailRangeItemTransactions = billingTransactions.Where(t => t.PriceItemCode == "TRN").OrderBy(t => t.ServiceOccuredUTC);
				var stlMilestoneTransactions = billingTransactions.Where(t => t.PriceItemCode == "STL").OrderBy(t => t.ServiceOccuredUTC);

				AssertEquals("Range transaction Count", 7, detailRangeItemTransactions.Count());
				AssertEquals("STL Milestone dummy transactions Count", 7, stlMilestoneTransactions.Count());
				AssertEquals("All transaction count", billingTransactions.Count(), detailRangeItemTransactions.Count() + stlMilestoneTransactions.Count());
				AssertEquals("New Watermark", expectedEndDateExclusive, tranItem.HighWaterMarkSettings.HighWaterMark);
			}
		}

		[TestDate(2014, 7, 4, 23, 59, 59)]
		public void TestCollectAndSendWithNoCollectionStartTimeOrWatermarkConfigured()
		{
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Production))
			{
				var expectedEndDateExclusive = TestDateAttribute.Date.AddHours(-2).AddSeconds(1);
				var tranItem = new ScriptWithConfig(new TestTransactionalStlItem(), new StlItemRegistrySettingsStub());
				var stlRetriever = new StlRetrieverForTest(new LoggerForTest(), new BillingDataCollectorForTestFactory(), new ScriptLoaderForTest(new IStlScriptWithConfig[] { tranItem }));
				stlRetriever.CollectAndSend();

				var utcNow = EnvProxy.Instance.Time.CurrentUtcDateTime;
				var billingTransactions = BillingTestHelper.GetBillingTransactionsFromDatabase(TestConnection);
				var detailRangeItemTransactions = billingTransactions.Where(t => t.PriceItemCode == "TRN").OrderBy(t => t.ServiceOccuredUTC);
				var stlMilestoneTransactions = billingTransactions.Where(t => t.PriceItemCode == "STL").OrderBy(t => t.ServiceOccuredUTC);

				AssertEquals("Range transaction Count", 7, detailRangeItemTransactions.Count());
				AssertEquals("STL Milestone dummy transactions Count", 7, stlMilestoneTransactions.Count());
				AssertEquals("All transaction count", billingTransactions.Count(), detailRangeItemTransactions.Count() + stlMilestoneTransactions.Count());
				AssertEquals("New Watermark", expectedEndDateExclusive, tranItem.HighWaterMarkSettings.HighWaterMark);
			}
		}

		void AssertCollectAndSend(DateTime startDate, DateTime expectedEndDateExclusive, bool legacyWaterMarkShouldHaveCleared = true, int expectedMilestoneOffset = 1)
		{
			BillingTestHelper.StlCollectorHighWaterMarkRegistry = startDate;
			var settings = new BusinessObjectFactory().Load<StmData>(new ZQuery(StmDataSchema.SD_Name, SQLComparisonOperator.Equal, SystemDataRegistry.StlCollectorHighWaterMarkPrefix));
			AssertEquals("Legacy Watermark should have set", 1, settings.Length);

			var stlRetriever = new StlRetrieverForTest(includingSnapshots: false);
			stlRetriever.CollectAndSend();

			AssertLogEntries(stlRetriever.LogEntries, startDate, expectedEndDateExclusive, stlRetriever.Scripts);
			AssertBillingTransactions(startDate, expectedEndDateExclusive, stlRetriever.Scripts, codes: string.Empty, expectedMilestoneOffset);
			settings = new BusinessObjectFactory().Load<StmData>(new ZQuery(StmDataSchema.SD_Name, SQLComparisonOperator.Equal, SystemDataRegistry.StlCollectorHighWaterMarkPrefix));
			AssertEquals("Legacy Watermark not in expected state", legacyWaterMarkShouldHaveCleared ? 0 : 1, settings.Length);
		}

		void AssertLogEntries(IEnumerable<string> logEntries, DateTime expectedStartDate, DateTime expectedEndDate, IEnumerable<IStlScriptWithConfig> scripts, int expectedCollectionCount = -1, string codes = "", bool expectWatermarkReset = false, DateTime resetFromDate = new DateTime())
		{
			var logEntryArray = logEntries.ToArray();

			if (expectedStartDate < expectedEndDate)
			{
				int logIndex = 0;
				AssertEquals("Log count > 1?", true, logEntryArray.Length > 1);

				if (expectWatermarkReset)
				{
					foreach (var script in scripts)
					{
						var code = script.Script.Code;
						if ((string.IsNullOrEmpty(codes) || codes.Contains(code)) && expectedStartDate < resetFromDate)
						{
							Assert($"{code} watermark has reset?", logEntries.Contains(string.Format("Resetting {0} Watermark from {1} to {2}", code, SqlFormatInfo.ToSqlDateTimeString(resetFromDate), SqlFormatInfo.ToSqlDateTimeString(expectedStartDate))));
							logIndex++;
						}
					}
				}
				else
				{
					Assert("Reset message",
						string.Equals(string.Format("Last watermark reset up-to-date, skipping reset"), logEntryArray[logIndex]) ||
						string.Equals(string.Format("No watermark reset values found, skipping reset"), logEntryArray[logIndex]));
					logIndex++;
				}

				AssertEquals($"Log Entry ({logIndex})", string.Format("Date Range: >= {0}, < {1}, Mandatory Items", SqlFormatInfo.ToSqlDateTimeString(expectedStartDate), SqlFormatInfo.ToSqlDateTimeString(expectedEndDate)), logEntryArray[logIndex]);
				logIndex++;

				int collectionCount = 0;
				var rangeStart = expectedStartDate;
				var ausydExpectedDateEnd = TimeZoneInfo.ConvertTimeFromUtc(expectedEndDate, billingTimeZoneInfo);
				bool firstCollectionOccured = false;

				while (rangeStart < expectedEndDate)
				{
					var rangeEnd = BaseDateTimeRange.GetMinDateTimeValue(rangeStart.AddDays(1), expectedEndDate);
					AssertEquals("Collect and Send (" + logIndex + ")", "Collect and send: [" + SqlFormatInfo.ToSqlDateTimeString(rangeStart) + ", " + SqlFormatInfo.ToSqlDateTimeString(rangeEnd) + ")", logEntryArray[logIndex]);
					++logIndex;

					if (rangeEnd >= expectedEndDate.AddDays(-7))
					{
						if (expectedCollectionCount != -1 && collectionCount >= expectedCollectionCount)
						{
							break;
						}
						AssertCollectingLogEntry(logEntryArray, ref logIndex, ref collectionCount, ref firstCollectionOccured, "TRN", "Transactional Feature", codes: codes);
					}

					if (rangeStart.Month != rangeStart.AddDays(1).Month)
					{
						if (expectedCollectionCount != -1 && collectionCount >= expectedCollectionCount)
						{
							break;
						}
						AssertCollectingLogEntry(logEntryArray, ref logIndex, ref collectionCount, ref firstCollectionOccured, "MON", "Monthly Allow Historical Data Feature", codes: codes);

						if (expectedCollectionCount != -1 && collectionCount >= expectedCollectionCount)
						{
							break;
						}
						AssertCollectingLogEntry(logEntryArray, ref logIndex, ref collectionCount, ref firstCollectionOccured, "MCO", "Monthly Current Data Only Feature", codes: codes);
					}

					if (rangeStart.AddDays(1) <= expectedEndDate || (rangeEnd == expectedEndDate && expectedEndDate.Date != ausydExpectedDateEnd.Date))
					{
						if (rangeStart >= ausydExpectedDateEnd.Date.AddDays(-7))
						{
							if (expectedCollectionCount != -1 && collectionCount >= expectedCollectionCount)
							{
								break;
							}
							AssertCollectingLogEntry(logEntryArray, ref logIndex, ref collectionCount, ref firstCollectionOccured, "DAY", "Daily Collection Feature", codes: codes);
						}
					}

					if (scripts.Any(s => s.Script.Code == "SPT"))
					{
						if (expectedCollectionCount != -1 && collectionCount >= expectedCollectionCount)
						{
							break;
						}
						AssertCollectingLogEntry(logEntryArray, ref logIndex, ref collectionCount, ref firstCollectionOccured, "SPT", "Snapshot Time Based", expectSendTransactions: true, codes: codes);
					}
					if (scripts.Any(s => s.Script.Code == "SPC"))
					{
						if (rangeStart == expectedStartDate)
						{
							if (expectedCollectionCount != -1 && collectionCount >= expectedCollectionCount)
							{
								break;
							}
							AssertCollectingLogEntry(logEntryArray, ref logIndex, ref collectionCount, ref firstCollectionOccured, "SPC", "Snapshot Live Configuration", expectSendTransactions: true, codes: codes);
						}
					}

					if (firstCollectionOccured && rangeStart.AddDays(1) <= expectedEndDate || (rangeEnd == expectedEndDate && expectedEndDate.Date != ausydExpectedDateEnd.Date))
					{
						var mandatoryScripts = scripts.Where(s => s.Script.IsMandatoryForMilestones);
						AssertEquals("Milestone (" + logIndex + ")", $"STL Milestone Created - {SqlFormatInfo.ToSqlDateString(rangeStart)} (DAY) MandatoryItemsCount({mandatoryScripts.Count()}) Version({ReleaseInfo.Instance.VersionNumber})", logEntryArray[logIndex]);
						++logIndex;
					}
					rangeStart = rangeEnd;
				}

				AssertEquals("Number of Log Entries", logIndex, logEntryArray.Length);
			}
			else
			{
				AssertEquals("Number of Log Entries", 2, logEntryArray.Length);
				AssertEquals("Log Entry (1)", string.Format("Date Range: >= {0}, < {1} (no STL collection), Mandatory Items", SqlFormatInfo.ToSqlDateTimeString(expectedStartDate), SqlFormatInfo.ToSqlDateTimeString(expectedEndDate)), logEntryArray[1]);
			}
		}

		void AssertCollectingLogEntry(string[] logEntryArray, ref int logIndex, ref int collectionCount, ref bool firstCollectionOccured, string expectedCode, string expectedFeature, bool expectSendTransactions = true, string codes = "")
		{
			if (!string.IsNullOrEmpty(codes) && !codes.Contains(expectedCode))
			{
				return;
			}
			firstCollectionOccured = true;
			AssertEquals("Collecting " + expectedCode + " (" + logIndex + ")", "Collecting - " + expectedCode + " - " + expectedFeature, logEntryArray[logIndex]);
			++logIndex;
			if (expectSendTransactions)
			{
				AssertEquals("Sent (" + logIndex + ")", "- 1 transaction(s) sent", logEntryArray[logIndex]);
				++logIndex;
			}
			++collectionCount;
		}

		void AssertCollectionLogEntriesMandatoryItemsFirst(DateTime startDate, DateTime endDate, string code, string feature, StlDataGrain dataGranularity, bool collectingMandatoryItems, string[] logEntryArray, ref int logIndex, IEnumerable<IStlScriptWithConfig> scripts)
		{
			AssertEquals("Date Range Entry (" + logIndex + ")", string.Format("Date Range: >= {0}, < {1}, {2} Items", SqlFormatInfo.ToSqlDateTimeString(startDate), SqlFormatInfo.ToSqlDateTimeString(endDate), collectingMandatoryItems ? "Mandatory" : "Non-Mandatory"), logEntryArray[logIndex]);
			++logIndex;

			var rangeStart = startDate;
			int collectionCount = 0;
			var firstCollectionOccured = false;
			while (rangeStart < endDate)
			{
				DateTime rangeEnd = BaseDateTimeRange.GetMinDateTimeValue(rangeStart.AddDays(1), endDate);
				AssertEquals("Collect and Send (" + logIndex + ")", "Collect and send: [" + SqlFormatInfo.ToSqlDateTimeString(rangeStart) + ", " + SqlFormatInfo.ToSqlDateTimeString(rangeEnd) + ")", logEntryArray[logIndex]);
				++logIndex;
				if (dataGranularity != StlDataGrain.Daily)
				{
					AssertCollectingLogEntry(logEntryArray, ref logIndex, ref collectionCount, ref firstCollectionOccured, code, feature);
				}

				if (rangeStart.Day != endDate.Day || (rangeEnd == endDate && endDate.Day < TimeZoneInfo.ConvertTimeFromUtc(endDate, billingTimeZoneInfo).Day))
				{
					if (dataGranularity == StlDataGrain.Daily)
					{
						AssertCollectingLogEntry(logEntryArray, ref logIndex, ref collectionCount, ref firstCollectionOccured, code, feature);
					}
					if (collectingMandatoryItems && firstCollectionOccured)
					{
						var mandatoryScripts = scripts.Where(s => s.Script.IsMandatoryForMilestones);
						AssertEquals($"Milestone ({logIndex})", $"STL Milestone Created - {SqlFormatInfo.ToSqlDateString(rangeStart)} (DAY) MandatoryItemsCount({mandatoryScripts.Count()}) Version({ReleaseInfo.Instance.VersionNumber})", logEntryArray[logIndex]);
						++logIndex;
					}
				}

				rangeStart = rangeEnd;
			}
		}

		void AssertBillingTransactions(DateTime startDateTime, DateTime expectedEndDateTimeExclusive, IEnumerable<IStlScriptWithConfig> scripts, string codes = "", int expectedMilestoneOffset = 1)
		{
			bool anyTransactionExpected = (startDateTime < expectedEndDateTimeExclusive) && (string.IsNullOrEmpty(codes) || codes.Contains("TRN"));
			bool monthlyAllowHistoricalTransactionExpected = (startDateTime.Year * 100 + startDateTime.Month < expectedEndDateTimeExclusive.Year * 100 + expectedEndDateTimeExclusive.Month) && (string.IsNullOrEmpty(codes) || codes.Contains("MON"));
			DateTime utcNow = EnvProxy.Instance.Time.CurrentUtcDateTime;
			bool monthlyCurrentOnlyTransactionExpected = monthlyAllowHistoricalTransactionExpected && utcNow > startDateTime.AddDays(-1) && utcNow < expectedEndDateTimeExclusive.AddDays(21) && (string.IsNullOrEmpty(codes) || codes.Contains("MCO"));
			bool dailyTransactionExpected = (startDateTime.Date < expectedEndDateTimeExclusive.Date) && (string.IsNullOrEmpty(codes) || codes.Contains("DAY"));
			bool stlMilestoneTransactionExpected = (startDateTime.Date < expectedEndDateTimeExclusive.Date);

			var billingTransactions = BillingTestHelper.GetBillingTransactionsFromDatabase(TestConnection);
			var detailRangeItemTransactions = billingTransactions.Where(t => t.PriceItemCode == "TRN").OrderBy(t => t.ServiceOccuredUTC);
			var monthlyAllowHistoricalItemTransactions = billingTransactions.Where(t => t.PriceItemCode == "MON").OrderBy(t => t.ServiceOccuredUTC);
			var monthlyCurrentOnlyItemTransactions = billingTransactions.Where(t => t.PriceItemCode == "MCO").OrderBy(t => t.ServiceOccuredUTC);
			var dailyItemTransactions = billingTransactions.Where(t => t.PriceItemCode == "DAY").OrderBy(t => t.ServiceOccuredUTC);
			var stlMilestoneTransactions = billingTransactions.Where(t => t.PriceItemCode == "STL").OrderBy(t => t.ServiceOccuredUTC);

			AssertEquals("Any range transactions found?", anyTransactionExpected, detailRangeItemTransactions.Any());
			AssertEquals("Monthly Allow Historical range transactions found?", monthlyAllowHistoricalTransactionExpected, monthlyAllowHistoricalItemTransactions.Any());
			AssertEquals("Monthly Current Only range transactions found?", monthlyCurrentOnlyTransactionExpected, monthlyCurrentOnlyItemTransactions.Any());
			AssertEquals("Daily transactions found?", dailyTransactionExpected, dailyItemTransactions.Any());
			AssertEquals("STL Milestone dummy transactions found?", stlMilestoneTransactionExpected, stlMilestoneTransactions.Any());
			AssertEquals("All transaction count",
				billingTransactions.Count(),
				detailRangeItemTransactions.Count() + monthlyAllowHistoricalItemTransactions.Count() + monthlyCurrentOnlyItemTransactions.Count() + stlMilestoneTransactions.Count() + dailyItemTransactions.Count()
			);

			AssertDailyTransactions(startDateTime, expectedEndDateTimeExclusive, detailRangeItemTransactions);
			AssertMilestoneTransactions(startDateTime, stlMilestoneTransactions, scripts, expectedMilestoneOffset);

			//
			// Monthly Allow Historical transactions
			foreach (var monthlyAllowHistoricalTransaction in monthlyAllowHistoricalItemTransactions)
			{
				var endDate = TimeZoneInfo.ConvertTimeToUtc(new DateTime(startDateTime.Year, startDateTime.Month, 1), billingTimeZoneInfo);
				AssertTransactionFields(monthlyAllowHistoricalTransaction);

				AssertEquals("ServiceOccuredUTC", endDate, monthlyAllowHistoricalTransaction.ServiceOccuredUTC);

				AssertEquals("Reference1", "MONTHLYHISTORICAL#1", monthlyAllowHistoricalTransaction.Reference1);
				AssertEquals("Reference2", "MONTHLYHISTORICAL#2", monthlyAllowHistoricalTransaction.Reference2);
				AssertEquals("Reference3", "MONTHLYHISTORICAL#3", monthlyAllowHistoricalTransaction.Reference3);
				AssertNull("Reference4", monthlyAllowHistoricalTransaction.Reference4);
				AssertEquals("Reference5", "C93C8E21-5E1B-4265-9616-82D0383E99EA", monthlyAllowHistoricalTransaction.Reference5);
				AssertEquals("BillableCount", 20, monthlyAllowHistoricalTransaction.BillableCount);
			}

			//
			// Monthly Current Only transactions
			foreach (var monthlyCurrentOnlyTransaction in monthlyCurrentOnlyItemTransactions)
			{
				var endDate = TimeZoneInfo.ConvertTimeToUtc(new DateTime(startDateTime.Year, startDateTime.Month, 1), billingTimeZoneInfo);
				AssertTransactionFields(monthlyCurrentOnlyTransaction);

				AssertEquals("ServiceOccuredUTC", endDate, monthlyCurrentOnlyTransaction.ServiceOccuredUTC);

				AssertEquals("Reference1", "MONTHLYCURRENT#1", monthlyCurrentOnlyTransaction.Reference1);
				AssertEquals("Reference2", "MONTHLYCURRENT#2", monthlyCurrentOnlyTransaction.Reference2);
				AssertEquals("Reference3", "MONTHLYCURRENT#3", monthlyCurrentOnlyTransaction.Reference3);
				AssertEquals("Reference4", "MONTHLYCURRENT#4", monthlyCurrentOnlyTransaction.Reference4);
				AssertEquals("Reference5", "171D86DF-065D-4232-8F9D-246FF3612EF5", monthlyCurrentOnlyTransaction.Reference5);
				AssertEquals("BillableCount", 25, monthlyCurrentOnlyTransaction.BillableCount);
			}
		}

		void AssertDailyTransactions(DateTime startDateTime, DateTime expectedEndDateTimeExclusive, IOrderedEnumerable<BillingTransaction> dailyTransactions)
		{
			var sevenDaysBeforeEndTime = expectedEndDateTimeExclusive.AddDays(-7);
			var earliestDailyTransaction = startDateTime;
			var secondDailyTransactionTime = startDateTime.AddDays(1);

			if (sevenDaysBeforeEndTime > startDateTime)
			{
				earliestDailyTransaction = sevenDaysBeforeEndTime;
				secondDailyTransactionTime = startDateTime;
				while (secondDailyTransactionTime < earliestDailyTransaction)
				{
					secondDailyTransactionTime = secondDailyTransactionTime.AddDays(1);
				}
			}

			var dayCount = 0;
			foreach (var transaction in dailyTransactions)
			{
				AssertTransactionFields(transaction);

				AssertEquals("ServiceOccuredUTC", (dayCount == 0) ? earliestDailyTransaction : secondDailyTransactionTime.AddDays(dayCount - 1), transaction.ServiceOccuredUTC);

				AssertEquals("Reference1", "DAILY#1", transaction.Reference1);
				AssertEquals("Reference2", "DAILY#2", transaction.Reference2);
				AssertNull("Reference3", transaction.Reference3);
				AssertNull("Reference4", transaction.Reference4);
				AssertEquals("Reference5", "448AC110-9B18-497F-8F6E-CA80AC82E098", transaction.Reference5);
				AssertEquals("BillableCount", 1, transaction.BillableCount);

				dayCount++;
			}
		}

		void AssertMilestoneTransactions(DateTime startDateTime, IOrderedEnumerable<BillingTransaction> stlMilestoneTransactions, IEnumerable<IStlScriptWithConfig> scripts, int expectedMilestoneOffset = 1)
		{
			foreach (var milestoneTransaction in stlMilestoneTransactions)
			{
				var endDateTimeUtc = DateTime.SpecifyKind(startDateTime.Date.AddDays(expectedMilestoneOffset), DateTimeKind.Utc);
				var endDateTimeAuSyd = TimeZoneInfo.ConvertTimeFromUtc(endDateTimeUtc, billingTimeZoneInfo);
				var endDateTimeChangeUtc = TimeZoneInfo.ConvertTimeToUtc(endDateTimeAuSyd.Date);
				AssertCommonTransactionFields(milestoneTransaction);

				var mandatoryScripts = scripts.Where(s => s.Script.IsMandatoryForMilestones);
				AssertEquals("ServiceOccuredUTC", endDateTimeChangeUtc.AddTicks(-1), milestoneTransaction.ServiceOccuredUTC);
				AssertEquals("Reference1", SqlFormatInfo.ToSqlDateString(endDateTimeChangeUtc.AddTicks(-1)), milestoneTransaction.Reference1);
				AssertEquals("Reference2", "DAY", milestoneTransaction.Reference2);
				AssertNull("Reference3", milestoneTransaction.Reference3);
				AssertNull("Reference4", milestoneTransaction.Reference4);
				AssertNull("Reference5", milestoneTransaction.Reference5);
				AssertEquals("BillableCount", 1, milestoneTransaction.BillableCount);
				AssertEquals("ClientNumber", "J", milestoneTransaction.ClientNumber);
				AssertNull("Branch", milestoneTransaction.Branch);
				AssertNull("ClientStaffCode", milestoneTransaction.ClientStaffCode);
				AssertEquals("AdditionalRefs", BillingTestHelper.GetExpectedAdditionalRefsForStlMilestone(mandatoryScripts), milestoneTransaction.AdditionalRefs);

				expectedMilestoneOffset++;
			}
		}

		void AssertCommonTransactionFields(BillingTransaction transaction)
		{
			AssertEquals("ReportingSource", BillingManager.ReportingSource, transaction.ReportingSource);
			AssertEquals("Category", "STL", transaction.Category);
			AssertEquals("ClientID", "EDIEDIDAT", transaction.ClientID);
		}

		void AssertTransactionFields(BillingTransaction transaction)
		{
			AssertCommonTransactionFields(transaction);

			AssertEquals("ClientNumber", "J.DEM", transaction.ClientNumber);
			AssertEquals("Branch", "~BR", transaction.Branch);
			AssertEquals("ClientStaffCode", "USR", transaction.ClientStaffCode);
		}

		[TestDate(2015, 6, 24)]
		public void TestLoadLastEndDateExclusive()
		{
			// Set invalid registry value
			SetTestHighWaterMarkRegistryValue("INVALID-DATE-VALUE");
			var stlRetriever = new StlRetrieverForTest(includingSnapshots: false);
			AssertEquals("When registry value is invalid",
				TestDateAttribute.Date.AddHours(-1).AddMonths(-3),
				stlRetriever.LoadLastEndDateTimeExclusive_Exposed());

			// Set good registry value
			DateTime testDate = new DateTime(2015, 4, 2);
			SetTestHighWaterMarkRegistryValue(SqlFormatInfo.ToSqlDateTimeString(testDate));
			stlRetriever = new StlRetrieverForTest(includingSnapshots: false);
			AssertEquals("When registry value is a valid DateTime",
				testDate,
				stlRetriever.LoadLastEndDateTimeExclusive_Exposed());

			// No registry value (no row in StmData)
			SetTestHighWaterMarkRegistryValue(testValue: null);
			stlRetriever = new StlRetrieverForTest(includingSnapshots: false);
			AssertEquals("When  there is no registry value (no row in StmData)",
				TestDateAttribute.Date.AddHours(-1).AddMonths(-3),
				stlRetriever.LoadLastEndDateTimeExclusive_Exposed());
		}

		[TestDate(2015, 6, 24)]
		public void TestCollectAndSendRecalculatesDateRangeEveryTime()
		{
			DateTime startDateTime = TestDateAttribute.Date.AddDays(-3);
			DateTime expectedEndDateTimeExclusive = TestDateAttribute.Date.AddHours(-1);

			var stlRetriever = new StlRetrieverForTest(includingSnapshots: false);
			AssertEquals("[PRE-CONDITION] No watermarks, collect default range", TestDateAttribute.Date.AddHours(-1).AddMonths(-3), stlRetriever.LoadLastEndDateTimeExclusive_Exposed());

			BillingTestHelper.StlCollectorHighWaterMarkRegistry = startDateTime;
			stlRetriever = new StlRetrieverForTest(includingSnapshots: false);
			AssertEquals("StlCollectorHighWaterMark", startDateTime, stlRetriever.LoadLastEndDateTimeExclusive_Exposed());

			stlRetriever.ClearLogAndTransactions(TestConnection);
			stlRetriever.CollectAndSend();
			AssertLogEntries(stlRetriever.LogEntries, startDateTime, expectedEndDateTimeExclusive, stlRetriever.Scripts);
			AssertBillingTransactions(startDateTime, expectedEndDateTimeExclusive, stlRetriever.Scripts);

			stlRetriever = new StlRetrieverForTest(includingSnapshots: false);
			AssertEquals("StlLastEndDateExclusive", expectedEndDateTimeExclusive, stlRetriever.LoadLastEndDateTimeExclusive_Exposed());

			stlRetriever.ClearLogAndTransactions(TestConnection);
			stlRetriever.CollectAndSend();
			AssertLogEntries(stlRetriever.LogEntries, expectedEndDateTimeExclusive, expectedEndDateTimeExclusive, stlRetriever.Scripts);
			AssertBillingTransactions(expectedEndDateTimeExclusive, expectedEndDateTimeExclusive, stlRetriever.Scripts);

			stlRetriever = new StlRetrieverForTest(includingSnapshots: false);
			AssertEquals("StlLastEndDateExclusive", expectedEndDateTimeExclusive, stlRetriever.LoadLastEndDateTimeExclusive_Exposed());
		}

		[TestDate(2015, 7, 2)]
		public void TestCollectAndSendCancelledMidWayThrough()
		{
			var startDateTime = TestDateAttribute.Date.AddDays(-3);
			var expectedEndDateTimeExclusive = TestDateAttribute.Date.AddHours(-1);
			BillingTestHelper.StlCollectorHighWaterMarkRegistry = startDateTime;
			SystemDataRegistry.Instance.GetStlCollectorHighWaterMark("DAY", "Daily Collection Feature").SetValue(Guid.Empty, Guid.Empty, Guid.Empty, startDateTime);
			var scriptFactory = new ScriptFactoryForTest(includingSnapshots: false);
			var stlRetriever = new StlRetrieverForTest(new ScriptLoader(scriptFactory));
			var cancelTokenSource = new CancellationTokenSource();

			AssertEquals("StlCollectorHighWaterMark", startDateTime, stlRetriever.LoadLastEndDateTimeExclusive_Exposed());

			var collectionCount = 0;
			scriptFactory.CollectionOccurredEvent += (s) =>
			{
				++collectionCount;
				if (collectionCount > 1)
				{
					cancelTokenSource.Cancel();
				}
			};
			stlRetriever.ClearLogAndTransactions(TestConnection);
			AssertExceptionThrown<OperationCanceledException>(() => stlRetriever.CollectAndSend(cancelTokenSource.Token));
			AssertLogEntries(stlRetriever.LogEntries, startDateTime, expectedEndDateTimeExclusive, stlRetriever.Scripts, expectedCollectionCount: 2);

			var scriptLoader = new ScriptLoader(scriptFactory);
			var scripts = scriptLoader.Load(Factory);
			var trnScript = scripts.Single(swc => swc.Script.Code == "TRN");
			AssertEquals("StlCollectorHighWaterMark(TRN)", startDateTime.AddDays(1), trnScript.HighWaterMarkSettings.HighWaterMark);
			var monScript = scripts.Single(swc => swc.Script.Code == "MON");
			AssertEquals("StlCollectorHighWaterMark(MON)", startDateTime.AddDays(1), monScript.HighWaterMarkSettings.HighWaterMark);
			var mcoScript = scripts.Single(swc => swc.Script.Code == "MCO");
			AssertEquals("StlCollectorHighWaterMark(MCO)", startDateTime.AddDays(1), mcoScript.HighWaterMarkSettings.HighWaterMark);
			var dayScript = scripts.Single(swc => swc.Script.Code == "DAY");
			AssertEquals("StlCollectorHighWaterMark(DAY)", startDateTime.AddDays(1), dayScript.HighWaterMarkSettings.HighWaterMark);
		}

		void CreateDefaultWatermarkResetVariables(DateTime resetDate, DateTime now, string codes = "")
		{
			SystemDataRegistry.Instance.LastWatermarkResetTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, now);
			var configType = Factory.New<RefSysConfigType>();
			configType.ZRT_ConfigCode = WatermarkReset.STLWatermarkResetInfoCode;
			configType.ZRT_Description = "Description";
			configType.ZRT_LongDescription = "Deeeeeesssssccccrrriiiippppttttiiiooooonnnnn";
			var config = Factory.New<RefSysConfig>();
			config.ZRC_ZRT_NKConfigCode = WatermarkReset.STLWatermarkResetInfoCode;
			config.ZRC_StringValue = $"{SqlFormatInfo.ToSqlDateTimeString(resetDate)}|{SqlFormatInfo.ToSqlDateTimeString(now)}|{codes}";
			config.ZRC_StartDate = now;
			Factory.Save();
		}

		[TestDate(2024, 6, 26, 11, 0, 0)]
		public void TestRequestWatermarkReset()
		{
			CreateDefaultWatermarkResetVariables(new DateTime(2024, 3, 5), new DateTime(2024, 4, 2), "ABC,LOK,IJH,ENM");
			WatermarkReset.RequestWatermarkReset(Factory,new DateTime(2024, 06, 18, 12, 0, 0), TestDateAttribute.Date, "CBA,NUY,EWD,FGD");

			var result = Factory.Load<RefSysConfig>(new ZQuery(RefSysConfigSchema.ZRC_ZRT_NKConfigCode, SQLComparisonOperator.Equal, WatermarkReset.STLWatermarkResetInfoCode)).FirstOrDefault();
			AssertEquals("2024-06-18 12:00:00.000|2024-06-26 11:00:00.000|CBA,NUY,EWD,FGD", result.ZRC_StringValue);
		}

		[TestDate(2024, 6, 1, 12, 0, 0)]
		public void TestResetWatermarks()
		{
			CreateDefaultWatermarkResetVariables(new DateTime(2024, 3, 5), new DateTime(2024, 4, 2), "");
			var startDate = TestDateAttribute.Date.Date.AddDays(-7);
			var expectedEndDateExclusive = TestDateAttribute.Date.AddHours(-1);

			BillingTestHelper.StlCollectorHighWaterMarkRegistry = startDate;
			var settings = new BusinessObjectFactory().Load<StmData>(new ZQuery(StmDataSchema.SD_Name, SQLComparisonOperator.Equal, SystemDataRegistry.StlCollectorHighWaterMarkPrefix));
			AssertEquals("Legacy Watermark should have set", 1, settings.Length);

			var stlRetriever = new StlRetrieverForTest(includingSnapshots: false);
			stlRetriever.CollectAndSend();

			AssertLogEntries(stlRetriever.LogEntries, startDate, expectedEndDateExclusive, stlRetriever.Scripts);
			AssertBillingTransactions(startDate, expectedEndDateExclusive, stlRetriever.Scripts);

			stlRetriever.ClearLogAndTransactions(TestConnection);
			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(4);
			var resetStartDate = new DateTime(2024, 05, 27, 0, 0, 0);
			WatermarkReset.RequestWatermarkReset(Factory, resetStartDate, TestDateAttribute.Date);
			startDate = expectedEndDateExclusive;
			expectedEndDateExclusive = TestDateAttribute.Date.AddHours(-1);
			BillingTestHelper.StlCollectorHighWaterMarkRegistry = startDate;

			settings = new BusinessObjectFactory().Load<StmData>(new ZQuery(StmDataSchema.SD_Name, SQLComparisonOperator.Equal, SystemDataRegistry.StlCollectorHighWaterMarkPrefix));
			AssertEquals("Legacy Watermark should have set", 1, settings.Length);

			var stlRetriever2 = new StlRetrieverForTest(includingSnapshots: false);
			stlRetriever2.CollectAndSend();

			AssertLogEntries(stlRetriever2.LogEntries, resetStartDate, expectedEndDateExclusive, stlRetriever2.Scripts, expectWatermarkReset: true, resetFromDate: startDate);
			AssertBillingTransactions(resetStartDate, expectedEndDateExclusive, stlRetriever2.Scripts, codes: string.Empty, expectedMilestoneOffset: 3);
			AssertEquals("Has correct daterange", $"Date Range: >= {SqlFormatInfo.ToSqlDateTimeString(resetStartDate)}, < {SqlFormatInfo.ToSqlDateTimeString(expectedEndDateExclusive)}, Mandatory Items", stlRetriever2.LogEntries[4]);
			Assert("Has Collected previous data", stlRetriever2.LogEntries.Contains($"Collect and send: [{SqlFormatInfo.ToSqlDateTimeString(resetStartDate)}, {SqlFormatInfo.ToSqlDateTimeString(resetStartDate.AddDays(1))})"));
		}

		[TestDate(2024, 6, 1, 12, 0, 0)]
		public void TestDontResetEarlierWatermarks()
		{
			CreateDefaultWatermarkResetVariables(new DateTime(2024, 3, 5), new DateTime(2024, 4, 2), "");
			var startDate = TestDateAttribute.Date.Date.AddDays(-7);
			var expectedEndDateExclusive = TestDateAttribute.Date.AddHours(-1);

			BillingTestHelper.StlCollectorHighWaterMarkRegistry = startDate;
			var settings = new BusinessObjectFactory().Load<StmData>(new ZQuery(StmDataSchema.SD_Name, SQLComparisonOperator.Equal, SystemDataRegistry.StlCollectorHighWaterMarkPrefix));
			AssertEquals("Legacy Watermark should have set", 1, settings.Length);

			var stlRetriever = new StlRetrieverForTest(includingSnapshots: false);
			stlRetriever.CollectAndSend();

			AssertLogEntries(stlRetriever.LogEntries, startDate, expectedEndDateExclusive, stlRetriever.Scripts);
			AssertBillingTransactions(startDate, expectedEndDateExclusive, stlRetriever.Scripts);

			stlRetriever.ClearLogAndTransactions(TestConnection);
			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(4);

			var resetStartDate = new DateTime(2024, 06, 03, 0, 0, 0);
			WatermarkReset.RequestWatermarkReset(Factory, resetStartDate, TestDateAttribute.Date);
			startDate = expectedEndDateExclusive;
			expectedEndDateExclusive = TestDateAttribute.Date.AddHours(-1);
			BillingTestHelper.StlCollectorHighWaterMarkRegistry = startDate;

			settings = new BusinessObjectFactory().Load<StmData>(new ZQuery(StmDataSchema.SD_Name, SQLComparisonOperator.Equal, SystemDataRegistry.StlCollectorHighWaterMarkPrefix));
			AssertEquals("Legacy Watermark should have set", 1, settings.Length);

			var stlRetriever2 = new StlRetrieverForTest(includingSnapshots: false);
			stlRetriever2.CollectAndSend();

			AssertLogEntries(stlRetriever2.LogEntries, startDate, expectedEndDateExclusive, stlRetriever2.Scripts, expectWatermarkReset: true, resetFromDate: startDate);
			AssertBillingTransactions(startDate, expectedEndDateExclusive, stlRetriever2.Scripts);
			AssertEquals("Has correct daterange", $"Date Range: >= {SqlFormatInfo.ToSqlDateTimeString(startDate)}, < {SqlFormatInfo.ToSqlDateTimeString(expectedEndDateExclusive)}, Mandatory Items", stlRetriever2.LogEntries[0]);
			Assert("Has not skipped data", stlRetriever2.LogEntries.Contains($"Collect and send: [{SqlFormatInfo.ToSqlDateTimeString(startDate)}, {SqlFormatInfo.ToSqlDateTimeString(startDate.AddDays(1))})"));
		}

		[TestDate(2024, 6, 19, 12, 0, 0)]
		public void TestDontResetWatermarklessCollectors()
		{
			CreateDefaultWatermarkResetVariables(new DateTime(2024, 3, 5), new DateTime(2024, 4, 2), "");
			var startDate = TestDateAttribute.Date.Date.AddDays(-7);
			var expectedEndDateExclusive = TestDateAttribute.Date.AddHours(-1);

			BillingTestHelper.StlCollectorHighWaterMarkRegistry = startDate;
			var settings = new BusinessObjectFactory().Load<StmData>(new ZQuery(StmDataSchema.SD_Name, SQLComparisonOperator.Equal, SystemDataRegistry.StlCollectorHighWaterMarkPrefix));
			AssertEquals("Legacy Watermark should have set", 1, settings.Length);

			var stlRetriever = new StlRetrieverForTest(includingSnapshots: false);
			stlRetriever.CollectAndSend();

			AssertLogEntries(stlRetriever.LogEntries, startDate, expectedEndDateExclusive, stlRetriever.Scripts);
			AssertBillingTransactions(startDate, expectedEndDateExclusive, stlRetriever.Scripts);

			stlRetriever.ClearLogAndTransactions(TestConnection);
			var resetStartDate = new DateTime(2024, 06, 14, 0, 0, 0);
			WatermarkReset.RequestWatermarkReset(Factory, resetStartDate, TestDateAttribute.Date);
			startDate = expectedEndDateExclusive;
			expectedEndDateExclusive = TestDateAttribute.Date.AddHours(-1);
			BillingTestHelper.StlCollectorHighWaterMarkRegistry = startDate;

			settings = new BusinessObjectFactory().Load<StmData>(new ZQuery(StmDataSchema.SD_Name, SQLComparisonOperator.Equal, SystemDataRegistry.StlCollectorHighWaterMarkPrefix));
			AssertEquals("Legacy Watermark should have set", 1, settings.Length);

			var stlRetriever2 = new StlRetrieverForTest(includingSnapshots: false);
			stlRetriever2.Scripts.FirstOrDefault(s => s.Script.Code == "MON").HighWaterMarkSettings.ClearHighWaterMark();

			stlRetriever2.CollectAndSend();

			AssertLogEntries(stlRetriever2.LogEntries, resetStartDate, expectedEndDateExclusive, stlRetriever2.Scripts, codes: "TRN,MCO,DAY", expectWatermarkReset: true, resetFromDate: startDate);
			AssertBillingTransactions(resetStartDate, expectedEndDateExclusive, stlRetriever2.Scripts);
			AssertEquals("Has correct daterange", $"Date Range: >= {SqlFormatInfo.ToSqlDateTimeString(resetStartDate)}, < {SqlFormatInfo.ToSqlDateTimeString(expectedEndDateExclusive)}, Mandatory Items", stlRetriever2.LogEntries[3]);
			Assert("Has not affected watermarkless collector", !stlRetriever2.LogEntries.Contains("Collecting - MON - Daily Collection Feature"));
		}

		[TestDate(2024, 6, 1, 12, 0, 0)]
		public void TestOnlyResetSpecifiedWatermarks()
		{
			CreateDefaultWatermarkResetVariables(new DateTime(2024, 3, 5), new DateTime(2024, 4, 2), "");
			var startDate = TestDateAttribute.Date.Date.AddDays(-7);
			var expectedEndDateExclusive = TestDateAttribute.Date.AddHours(-1);

			BillingTestHelper.StlCollectorHighWaterMarkRegistry = startDate;
			var settings = new BusinessObjectFactory().Load<StmData>(new ZQuery(StmDataSchema.SD_Name, SQLComparisonOperator.Equal, SystemDataRegistry.StlCollectorHighWaterMarkPrefix));
			AssertEquals("Legacy Watermark should have set", 1, settings.Length);

			var stlRetriever = new StlRetrieverForTest(includingSnapshots: false);
			stlRetriever.CollectAndSend();

			AssertLogEntries(stlRetriever.LogEntries, startDate, expectedEndDateExclusive, stlRetriever.Scripts);
			AssertBillingTransactions(startDate, expectedEndDateExclusive, stlRetriever.Scripts);

			stlRetriever.ClearLogAndTransactions(TestConnection);

			var resetStartDate = new DateTime(2024, 05, 27, 0, 0, 0);
			var resetCodes = "TRN,MON";
			WatermarkReset.RequestWatermarkReset(Factory, resetStartDate, TestDateAttribute.Date, resetCodes);
			startDate = expectedEndDateExclusive;
			expectedEndDateExclusive = TestDateAttribute.Date.AddHours(-1);
			BillingTestHelper.StlCollectorHighWaterMarkRegistry = startDate;

			settings = new BusinessObjectFactory().Load<StmData>(new ZQuery(StmDataSchema.SD_Name, SQLComparisonOperator.Equal, SystemDataRegistry.StlCollectorHighWaterMarkPrefix));
			AssertEquals("Legacy Watermark should have set", 1, settings.Length);

			var stlRetriever2 = new StlRetrieverForTest(includingSnapshots: false);
			stlRetriever2.CollectAndSend();

			AssertLogEntries(stlRetriever2.LogEntries, resetStartDate, expectedEndDateExclusive, stlRetriever2.Scripts, codes: resetCodes, expectWatermarkReset: true, resetFromDate: startDate);
			AssertBillingTransactions(resetStartDate, expectedEndDateExclusive, stlRetriever2.Scripts, codes: resetCodes);
			AssertEquals("Has correct daterange", $"Date Range: >= {SqlFormatInfo.ToSqlDateTimeString(resetStartDate)}, < {SqlFormatInfo.ToSqlDateTimeString(expectedEndDateExclusive)}, Mandatory Items", stlRetriever2.LogEntries[2]);
			Assert("Has not collected previous data of wrong collectors", !stlRetriever2.LogEntries.Contains("Collecting - MCO - Monthly Current Data Only Feature"));
			Assert("Has not collected previous data of wrong collectors", !stlRetriever2.LogEntries.Contains("Collecting - DAY - Daily Collection Feature"));
		}

		[TestDate(2024, 6, 1, 12, 0, 0)]
		public void TestWatermarkResetDoesntAffectFutureCollections()
		{
			CreateDefaultWatermarkResetVariables(new DateTime(2024, 3, 5), new DateTime(2024, 4, 2), "");
			var startDate = TestDateAttribute.Date.Date.AddDays(-7);
			var expectedEndDateExclusive = TestDateAttribute.Date.AddHours(-1);

			BillingTestHelper.StlCollectorHighWaterMarkRegistry = startDate;
			var settings = new BusinessObjectFactory().Load<StmData>(new ZQuery(StmDataSchema.SD_Name, SQLComparisonOperator.Equal, SystemDataRegistry.StlCollectorHighWaterMarkPrefix));
			AssertEquals("Legacy Watermark should have set", 1, settings.Length);

			var stlRetriever = new StlRetrieverForTest(includingSnapshots: false);
			stlRetriever.CollectAndSend();
			AssertLogEntries(stlRetriever.LogEntries, startDate, expectedEndDateExclusive, stlRetriever.Scripts);
			AssertBillingTransactions(startDate, expectedEndDateExclusive, stlRetriever.Scripts);
			stlRetriever.ClearLogAndTransactions(TestConnection);

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(4);
			var resetStartDate = new DateTime(2024, 05, 27, 0, 0, 0);
			WatermarkReset.RequestWatermarkReset(Factory, resetStartDate, TestDateAttribute.Date);

			startDate = expectedEndDateExclusive;
			expectedEndDateExclusive = TestDateAttribute.Date.AddHours(-1);
			BillingTestHelper.StlCollectorHighWaterMarkRegistry = startDate;

			settings = new BusinessObjectFactory().Load<StmData>(new ZQuery(StmDataSchema.SD_Name, SQLComparisonOperator.Equal, SystemDataRegistry.StlCollectorHighWaterMarkPrefix));
			AssertEquals("Legacy Watermark should have set", 1, settings.Length);

			var stlRetriever2 = new StlRetrieverForTest(includingSnapshots: false);
			stlRetriever2.CollectAndSend();
			AssertLogEntries(stlRetriever2.LogEntries, resetStartDate, expectedEndDateExclusive, stlRetriever2.Scripts, expectWatermarkReset: true, resetFromDate: startDate);
			AssertBillingTransactions(resetStartDate, expectedEndDateExclusive, stlRetriever2.Scripts, codes: string.Empty, expectedMilestoneOffset: 3);
			stlRetriever2.ClearLogAndTransactions(TestConnection);

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(4);
			startDate = expectedEndDateExclusive;
			expectedEndDateExclusive = TestDateAttribute.Date.AddHours(-1);
			BillingTestHelper.StlCollectorHighWaterMarkRegistry = startDate;

			settings = new BusinessObjectFactory().Load<StmData>(new ZQuery(StmDataSchema.SD_Name, SQLComparisonOperator.Equal, SystemDataRegistry.StlCollectorHighWaterMarkPrefix));
			AssertEquals("Legacy Watermark should have set", 1, settings.Length);

			var stlRetriever3 = new StlRetrieverForTest(includingSnapshots: false);
			stlRetriever3.CollectAndSend();
			AssertLogEntries(stlRetriever3.LogEntries, startDate, expectedEndDateExclusive, stlRetriever3.Scripts);
			AssertBillingTransactions(startDate, expectedEndDateExclusive, stlRetriever3.Scripts);

			AssertEquals("Has correct daterange", $"Date Range: >= {SqlFormatInfo.ToSqlDateTimeString(startDate)}, < {SqlFormatInfo.ToSqlDateTimeString(expectedEndDateExclusive)}, Mandatory Items", stlRetriever3.LogEntries[1]);
			Assert("Has not reset collector", !stlRetriever3.LogEntries.Contains($"Collect and send: [{SqlFormatInfo.ToSqlDateTimeString(resetStartDate)}, {SqlFormatInfo.ToSqlDateTimeString(resetStartDate.AddDays(1))})"));
		}

		[TestDate(2024, 6, 1, 12, 0, 0)]
		public void TestResetWatermarkInvalidSysRefConfig()
		{
			var startDate = TestDateAttribute.Date.Date.AddDays(-7);
			var expectedEndDateExclusive = TestDateAttribute.Date.AddHours(-1);

			BillingTestHelper.StlCollectorHighWaterMarkRegistry = startDate;
			var settings = new BusinessObjectFactory().Load<StmData>(new ZQuery(StmDataSchema.SD_Name, SQLComparisonOperator.Equal, SystemDataRegistry.StlCollectorHighWaterMarkPrefix));
			AssertEquals("Legacy Watermark should have set", 1, settings.Length);

			CreateDefaultWatermarkResetVariables(new DateTime(2024, 3, 5), new DateTime(2024, 4, 2), "");
			var query = new ZQuery(RefSysConfigSchema.ZRC_ZRT_NKConfigCode, SQLComparisonOperator.Equal, WatermarkReset.STLWatermarkResetInfoCode);
			var config = Factory.Load<RefSysConfig>(query).FirstOrDefault();
			config.ZRC_StringValue = "Mistakes were made";
			Factory.Save();

			var stlRetriever = new StlRetrieverForTest(includingSnapshots: false);
			AssertNoExceptionThrown(() => stlRetriever.CollectAndSend());
			AssertEquals("", $"Invalid watermark reset config format, value was: '{config.ZRC_StringValue}'. Skipping Reset", stlRetriever.LogEntries[0]);

			stlRetriever.ClearLogAndTransactions(TestConnection);
			config.ZRC_StringValue = "ABC,123,STL|2024-07-08 09:55:00.000|2024-07-08 09:55:00.000";
			Factory.Save();

			AssertNoExceptionThrown(() => stlRetriever.CollectAndSend());
			AssertEquals("", $"Invalid watermark reset config format, value was: '{config.ZRC_StringValue}'. Skipping Reset", stlRetriever.LogEntries[0]);

			stlRetriever.ClearLogAndTransactions(TestConnection);
			config.ZRC_StringValue = "2024-07-08 09:55:00.000|5th June 2024|";
			Factory.Save();

			AssertNoExceptionThrown(() => stlRetriever.CollectAndSend());
			AssertEquals("", $"Invalid watermark reset config format, value was: '{config.ZRC_StringValue}'. Skipping Reset", stlRetriever.LogEntries[0]);

			stlRetriever.ClearLogAndTransactions(TestConnection);
			config.ZRC_StringValue = "||";
			Factory.Save();

			AssertNoExceptionThrown(() => stlRetriever.CollectAndSend());
			AssertEquals("", $"Invalid watermark reset config format, value was: '{config.ZRC_StringValue}'. Skipping Reset", stlRetriever.LogEntries[0]);
		}

		void SetTestHighWaterMarkRegistryValue(string testValue)
		{
			var sqlBuilder = new StringBuilder();
			sqlBuilder.AppendFormat("DELETE dbo.StmData WHERE SD_Name = '{0}';", SystemDataRegistry.Instance.StlCollectorHighWaterMark.Name);

			if (testValue != null)
			{
				sqlBuilder.AppendLine().AppendFormat(
					"INSERT dbo.StmData (SD_PK, SD_Name, SD_BinaryValue) VALUES (newid(), '{0}', convert(varbinary(max), N'{1}'));",
					SystemDataRegistry.Instance.StlCollectorHighWaterMark.Name,
					testValue);
			}

			TestConnection.ExecuteNonQuery(sqlBuilder.ToString());
		}

		#region Supporting Classes

		class BillingDataCollectorThatThrowsExceptionForTestFactory : IBillingDataCollectorFactory
		{
			readonly Exception exceptionToThrow;

			public BillingDataCollectorThatThrowsExceptionForTestFactory(Exception exceptionToThrow)
			{
				this.exceptionToThrow = exceptionToThrow;
			}

			public BillingDataCollector Create(ILogger logger, BusinessObjectFactory bizoFactory, IEnumerable<IStlScriptWithConfig> stlScripts) => new BillingDataCollectorThatThrowsExceptionForTest(exceptionToThrow, logger, bizoFactory, stlScripts);

			class BillingDataCollectorThatThrowsExceptionForTest : BillingDataCollectorForTest
			{
				readonly Exception exceptionToThrow;

				public BillingDataCollectorThatThrowsExceptionForTest(Exception exceptionToThrow, ILogger logger, BusinessObjectFactory bizoFactory, IEnumerable<IStlScriptWithConfig> stlScripts)
					: base(logger, bizoFactory, stlScripts)
				{
					this.exceptionToThrow = exceptionToThrow;
				}

				protected override IEnumerable<IStlTransaction> RetrieveFeatureData(IStlItem stlScript, IDateTimeRange dateTimeRange)
				{
					throw exceptionToThrow;
				}
			}
		}

		readonly TimeZoneInfo billingTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time");

		#endregion
	}
}
