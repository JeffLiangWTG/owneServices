using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Engine;
using Enterprise.Billing.Business;
using Enterprise.Billing.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ArchiveManager.Test.SystemDescriptors.PAL
{
	public class PALArchiveSystemIntegrationTest : ArchiveSystemIntegrationTest, IArchiveSystemDescriptorIntegrationTest
	{
		public PALArchiveSystemIntegrationTest()
		{
			TestConfig = new TestConfiguration()
			{
				ArchiveSystemCodeToTest = ArchiveManagerConstants.Codes.PAL,
				ListOfStageNames = ["Purge Activity Logs"]
			};
		}

		public void TestGeneratedSummaryReport()
		{
			using (SystemDataRegistry.Instance.ExposeActivityLogsArchiveSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var config = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, false, true);
				var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);

				TestHelpers.RunArchiveSystem(TestConfig.ArchiveSystemCodeToTest, config, archiveLogger, archiveSchedule);

				Assert("Logs shouldn't contain any errors", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));
				AssertEquals("Report for each stage should have been generated.", 1, TestHelpers.GetAllReportsFromSchedule(archiveSchedule).Count());
			}
		}

		[TestDate(1971, 07, 18)]
		public void TestLoggingOfAMUsageData()
		{
			using (SystemDataRegistry.Instance.ExposeActivityLogsArchiveSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				TestConfig.AllJobHeadersNeedToBeClosed = true;
				TestConfig.NeedJobHeaderConsol = true;
				TestConfig.NeedShipmentDeclaration = true;
				TestConfig.IsPeriodClosed = true;
				TestConfig.IsHotChequeCancelled = true;
				TestConfig.IsHotChequeLinkedToAH = true;

				var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PAL);
				archiveSchedule.IsArchiveRecordsOnOrBeforeDate = true;
				archiveSchedule.ArchiveRecordsOnOrBeforeDate = ZDateTime.UtcNow;

				var activityLog1 = Factory.NewWithValidTestData<StmActivityLog>();
				activityLog1.S7_OpenDateTimeUtc = archiveSchedule.ArchiveRecordsOnOrBeforeDate.Date.AddMonths(-1);
				activityLog1.S7_CloseDateTimeUtc = archiveSchedule.ArchiveRecordsOnOrBeforeDate.Date.AddMonths(-1);

				archiveSchedule.ShouldArchiveDeclaration = true;
				archiveSchedule.MaxRunDurationInMinutes = 60;
				archiveSchedule.Run();

				var usageCollectorTestHelper = new UsageCollectorTestHelper(Factory);
				var messages = usageCollectorTestHelper.LoadUsageMessages(UsageFeatures.Codes.ArchiveManager);
				AssertEquals("There should be one and only one usage report", 1, messages.Length);

				var usageProperties = messages[0].UsageProperties;

				ArchiveManagerAssertions.AssertResultsForAMUsageCollector(
					usageProperties: usageProperties,
					archiveCode: TestConfig.ArchiveSystemCodeToTest,
					archiveSystemStageName: "Purge Activity Logs",
					includeCustomsJobs: true,
					batchSize: 50,
					totalRecordsDeleted: 1,
					jobHeadersProcessed: 0,
					mainRecordsLoaded: 1,
					totalMissingDocumentsGenerated: 0,
					documentsDeleted: 0
					);
			}
		}

		public void TestRunArchiveSystem_CorrectlyArchivesData()
		{
			using (SystemDataRegistry.Instance.ExposeActivityLogsArchiveSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var archiveJobsOnOrBeforeThisDate = ZDateTime.BrettsBirthday;
				var configuration = new ArchiveConfiguration(archiveJobsOnOrBeforeThisDate, 10, ZDateTime.UtcNow, false, false);
				var logger = new TestArchiveLogger();
				var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PAL);

				var activityLog1 = Factory.NewWithValidTestData<StmActivityLog>();
				activityLog1.S7_OpenDateTimeUtc = configuration.ArchiveJobsOnOrBeforeThisDate.Date.AddMonths(1);
				activityLog1.S7_CloseDateTimeUtc = configuration.ArchiveJobsOnOrBeforeThisDate.Date.AddMonths(1);

				var activityLog2 = Factory.NewWithValidTestData<StmActivityLog>();
				activityLog2.S7_OpenDateTimeUtc = configuration.ArchiveJobsOnOrBeforeThisDate.Date.AddMonths(-1);
				activityLog2.S7_CloseDateTimeUtc = configuration.ArchiveJobsOnOrBeforeThisDate.Date.AddMonths(-1);

				var activityLog3 = Factory.NewWithValidTestData<StmActivityLog>();
				activityLog3.S7_OpenDateTimeUtc = configuration.ArchiveJobsOnOrBeforeThisDate.Date;
				activityLog3.S7_CloseDateTimeUtc = configuration.ArchiveJobsOnOrBeforeThisDate.Date;

				Factory.Save();

				CombineAssertions("Precondition", () =>
				{
					AssertEquals("Expected Activity Log to exist.", 1, Factory.GetDatabaseCount(typeof(StmActivityLog), new ZQuery(StmActivityLogSchema.PK, activityLog1.PK)));
					AssertEquals("Expected Activity Log to exist.", 1, Factory.GetDatabaseCount(typeof(StmActivityLog), new ZQuery(StmActivityLogSchema.PK, activityLog2.PK)));
					AssertEquals("Expected Activity Log to exist.", 1, Factory.GetDatabaseCount(typeof(StmActivityLog), new ZQuery(StmActivityLogSchema.PK, activityLog3.PK)));
				});

				TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.PAL, configuration, logger, schedule);

				CombineAssertions("Expected objects are archived correctly", () =>
				{
					AssertEquals("Expected Activity Log to exist.", 1, Factory.GetDatabaseCount(typeof(StmActivityLog), new ZQuery(StmActivityLogSchema.PK, activityLog1.PK)));
					AssertEquals("Activity Log with Open Date Time lower than On or Before Date should be purged", 0, Factory.GetDatabaseCount(typeof(StmActivityLog), new ZQuery(StmActivityLogSchema.PK, activityLog2.PK)));
					AssertEquals("Activity Log with Open Date Time equal to On or Before Date should be purged", 0, Factory.GetDatabaseCount(typeof(StmActivityLog), new ZQuery(StmActivityLogSchema.PK, activityLog3.PK)));
				});

				Assert("Logs shouldn't contain any errors.", !logger.ListOfMessages.Exists(log => log.Contains("Error")));
				AssertLog(logger);
			}
		}

		void AssertLog(TestArchiveLogger logger)
		{
			var index = 0;

			AssertEquals(15, logger.ListOfMessages.Count);

			AssertEquals($"Information|{ArchiveManagerConstants.Codes.PAL}|Registry Settings:", logger.ListOfMessages[index++]);

			AssertEquals($"Information|{ArchiveManagerConstants.Codes.PAL}|Configuration Parameters:", logger.ListOfMessages[index++]);
			AssertContains($"Information|{ArchiveManagerConstants.Codes.PAL}|Purging Records on or Before:", logger.ListOfMessages[index++]);
			AssertEquals($"Information|{ArchiveManagerConstants.Codes.PAL}|Max Run Duration: 10 minutes", logger.ListOfMessages[index++]);
			AssertEquals($"Information|{ArchiveManagerConstants.Codes.PAL}|Verbose Logging: No", logger.ListOfMessages[index++]);

			AssertEquals($"Information|{ArchiveManagerConstants.Codes.PAL}|Executing: Purge Activity Logs", logger.ListOfMessages[index++]);

			AssertContains($"Information|{ArchiveManagerConstants.Codes.PAL}|Loaded batch of 2 StmActivityLog", logger.ListOfMessages[index++]);
			AssertContains($"Information|{ArchiveManagerConstants.Codes.PAL}|Deleting 2 records from StmActivityLog Time taken", logger.ListOfMessages[index++]);
			AssertContains($"Information|{ArchiveManagerConstants.Codes.PAL}|Loaded batch of 0 StmActivityLog", logger.ListOfMessages[index++]);

			AssertContains($"Information|{ArchiveManagerConstants.Codes.PAL}|Time taken to load 2 Archive Set(s) of StmActivityLog record(s) in 1 batch(es)", logger.ListOfMessages[index++]);
			AssertContains($"Information|{ArchiveManagerConstants.Codes.PAL}|Time taken to purge 2 StmActivityLog record(s) and their related record(s)", logger.ListOfMessages[index++]);
			AssertContains($"Information|{ArchiveManagerConstants.Codes.PAL}|StmActivityLogs Per Hour: ", logger.ListOfMessages[index++]);
			AssertContains($"Information|{ArchiveManagerConstants.Codes.PAL}|Purged Data Dated Between Earliest Possible Date and 18-Sep-71", logger.ListOfMessages[index++]);
			AssertContains($"Information|{ArchiveManagerConstants.Codes.PAL}|Generated Purge Report and stored on eDocs tab of the Purge Schedule", logger.ListOfMessages[index++]);
			AssertContains($"Information|{ArchiveManagerConstants.Codes.PAL}|Completed purging records", logger.ListOfMessages[index++]);
		}

		public void TestArchiveLoggerContainsCorrectStageNames_InOrder()
		{
			using (SystemDataRegistry.Instance.ExposeActivityLogsArchiveSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				IntegrationTestHelper.RunArchiveLoggerContainsCorrectStageNames_InOrder(TestConfig, Factory, archiveLogger);
			}
		}

		public void TestArchiveStagesSortByMainDateFilterColumn()
			{
				using (SystemDataRegistry.Instance.ExposeActivityLogsArchiveSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					IntegrationTestHelper.RunArchiveStagesSortByMainDateFilterColumn(TestConfig);
				}
			}
	}
}
