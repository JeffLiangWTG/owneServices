using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Engine;
using Enterprise.Billing.Business;
using Enterprise.Billing.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ArchiveManager.Test.SystemDescriptors.RED
{
	public sealed class REDArchiveSystemIntegrationTest : ArchiveSystemIntegrationTest, IArchiveSystemDescriptorIntegrationTest
	{
		static readonly int OnOrBeforeMinimumValue = RegistryHelper.GetOnOrBeforeMinimumValue(ArchiveManagerConstants.Codes.RED);

		public REDArchiveSystemIntegrationTest()
		{
			TestConfig = new TestConfiguration()
			{
				ArchiveSystemCodeToTest = ArchiveManagerConstants.Codes.RED,
				ListOfStageNames = ["Purge Expired Rates"]
			};
		}

		public void TestGeneratedSummaryReport()
		{
			using (SystemDataRegistry.Instance.ExposeExpiredRatesArchiveSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var config = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, false, true);
				var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);

				TestHelpers.RunArchiveSystem(TestConfig.ArchiveSystemCodeToTest, config, archiveLogger, archiveSchedule);

				Assert("Logs shouldn't contain any errors", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));
				AssertEquals("Report for each stage should have been generated.", 1, TestHelpers.GetAllReportsFromSchedule(archiveSchedule).Count());
				ArchiveManagerAssertions.AssertPurgeSummaryReport(TestHelpers.GetArchiveReportFromSchedule(archiveSchedule, "PurgeExpiredRatesReport_"), TestConfig.ArchiveSystemCodeToTest, mainArchiveTableName: RateEntrySchema.Constants.TableName, archiveSystemHasMultipleArchiveStageDescriptors: false, archiveSystemHasDateParameterSelection: false);
			}
		}

		public void CreateData()
		{
			var archiveJobsOnOrBeforeThisDate = ZDateTime.UtcNow.AddYears(-OnOrBeforeMinimumValue - 1);
			var configuration = new ArchiveConfiguration(archiveJobsOnOrBeforeThisDate, 10, ZDateTime.UtcNow, false, false);

			var ratingHeader = Factory.NewWithValidTestData<RatingHeader>();
			var glbCompany = Factory.New<GlbCompany>();
			ratingHeader.TH_GC = glbCompany.PK;
			ratingHeader.TH_RateType = "GLB";
			ratingHeader.TH_QuoteDate = configuration.ArchiveJobsOnOrBeforeThisDate.Date.AddMonths(-3);

			var rateEntry = Factory.New<RateEntry>();
			rateEntry.TI_OriginLRC = "AUSYD";
			rateEntry.TI_Mode = Core.Constants.RateMode.SCN;
			rateEntry.TI_RateCategory = "AIR";
			rateEntry.TI_GC_Publisher = glbCompany.PK;
			rateEntry.TI_TH = ratingHeader.PK;
			rateEntry.TI_RateStartDate = configuration.ArchiveJobsOnOrBeforeThisDate.Date.AddMonths(-2);
			rateEntry.TI_RateEndDate = configuration.ArchiveJobsOnOrBeforeThisDate.Date.AddMonths(-1);

			var rateEntry2 = Factory.New<RateEntry>();
			rateEntry2.TI_OriginLRC = "AUSYD";
			rateEntry2.TI_Mode = Core.Constants.RateMode.SCN;
			rateEntry2.TI_RateCategory = "AIR";
			rateEntry2.TI_GC_Publisher = glbCompany.PK;
			rateEntry2.TI_TH = ratingHeader.PK;
			rateEntry2.TI_RateStartDate = configuration.ArchiveJobsOnOrBeforeThisDate.Date.AddMonths(-4);
			rateEntry2.TI_RateEndDate = configuration.ArchiveJobsOnOrBeforeThisDate.Date.AddMonths(-3);

			var rateEntry3 = Factory.New<RateEntry>();
			rateEntry3.TI_OriginLRC = "AUSYD";
			rateEntry3.TI_Mode = Core.Constants.RateMode.SCN;
			rateEntry3.TI_RateCategory = "AIR";
			rateEntry3.TI_GC_Publisher = glbCompany.PK;
			rateEntry3.TI_TH = ratingHeader.PK;
			rateEntry3.TI_RateStartDate = configuration.ArchiveJobsOnOrBeforeThisDate.Date.AddMonths(-6);
			rateEntry3.TI_RateEndDate = configuration.ArchiveJobsOnOrBeforeThisDate.Date.AddMonths(-5);

			Factory.Save();
		}

		[TestDate(2024, 05, 15)]
		public void TestLoggingOfAMUsageData()
		{
			using (SystemDataRegistry.Instance.ExposeExpiredRatesArchiveSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SystemDataRegistry.Instance.ExpiredRatesArchiveSystemBatchSizeControl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 60))
			{
				var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.RED);
				CreateData();
				archiveSchedule.IsArchiveRecordsOnOrBeforeDate = true;
				archiveSchedule.ShouldArchiveDeclaration = true;
				archiveSchedule.ArchiveRecordsOnOrBeforeDate = ZDateTime.UtcNow;
				archiveSchedule.MaxRunDurationInMinutes = 60;
				archiveSchedule.Run();

				var usageCollectorTestHelper = new UsageCollectorTestHelper(Factory);
				var messages = usageCollectorTestHelper.LoadUsageMessages(UsageFeatures.Codes.ArchiveManager);
				AssertEquals("There should be one and only one usage report", 1, messages.Length);

				var usageProperties = messages[0].UsageProperties;

				ArchiveManagerAssertions.AssertResultsForAMUsageCollector(
					usageProperties: usageProperties,
					archiveCode: TestConfig.ArchiveSystemCodeToTest,
					archiveSystemStageName: "Purge Expired Rates",
					includeCustomsJobs: true,
					batchSize: 60,
					totalRecordsDeleted: 3,
					jobHeadersProcessed: 0,
					mainRecordsLoaded: 3,
					totalMissingDocumentsGenerated: 0,
					documentsDeleted: 0);
			}
		}

		public void TestRunArchiveSystem_CorrectlyArchivesData()
		{
			using (SystemDataRegistry.Instance.ExposeExpiredRatesArchiveSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var archiveJobsOnOrBeforeThisDate = ZDateTime.UtcNow.AddYears(-OnOrBeforeMinimumValue - 1);
				var configuration = new ArchiveConfiguration(archiveJobsOnOrBeforeThisDate, 10, ZDateTime.UtcNow, false, false);
				var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.RED);

				var ratingHeader = Factory.NewWithValidTestData<RatingHeader>();
				var glbCompany = Factory.New<GlbCompany>();
				ratingHeader.TH_GC = glbCompany.PK;
				ratingHeader.TH_RateType = "GLB";
				ratingHeader.TH_QuoteDate = configuration.ArchiveJobsOnOrBeforeThisDate.Date.AddMonths(-3);

				var rateEntry = Factory.New<RateEntry>();
				rateEntry.TI_OriginLRC = "AUSYD";
				rateEntry.TI_Mode = Core.Constants.RateMode.SCN;
				rateEntry.TI_RateCategory = "AIR";
				rateEntry.TI_GC_Publisher = glbCompany.PK;
				rateEntry.TI_TH = ratingHeader.PK;
				rateEntry.TI_RateStartDate = configuration.ArchiveJobsOnOrBeforeThisDate.Date.AddMonths(-2);
				rateEntry.TI_RateEndDate = configuration.ArchiveJobsOnOrBeforeThisDate.Date.AddMonths(-1);

				Factory.Save();

				CombineAssertions("Precondition", () =>
				{
					AssertEquals("Expected Rating Header to exist.", 1, Factory.GetDatabaseCount(typeof(RatingHeader), new ZQuery(RatingHeaderSchema.PK, ratingHeader.PK)));
					AssertEquals("Expected Rate Entry to exist.", 1, Factory.GetDatabaseCount(typeof(RateEntry), new ZQuery(RateEntrySchema.PK, rateEntry.PK)));
				});

				TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.RED, configuration, archiveLogger, schedule);

				CombineAssertions("Expected objects are archived correctly", () =>
				{
					AssertEquals("Expected Rating Header to not be archived.", 1, Factory.GetDatabaseCount(typeof(RatingHeader), new ZQuery(RatingHeaderSchema.PK, ratingHeader.PK)));
					AssertEquals("Expected Rate Entry to be archived.", 0, Factory.GetDatabaseCount(typeof(RateEntry), new ZQuery(RateEntrySchema.PK, rateEntry.PK)));
				});

				Assert("Logs shouldn't contain any errors.", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));
				AssertLog(archiveLogger, rateEntry);
			}
		}

		void AssertLog(TestArchiveLogger logger, RateEntry rateEntry)
		{
			var index = 0;

			AssertEquals(17, logger.ListOfMessages.Count);

			AssertEquals($"Information|{ArchiveManagerConstants.Codes.RED}|Registry Settings:", logger.ListOfMessages[index++]);
			AssertEquals($"Information|{ArchiveManagerConstants.Codes.RED}|On or Before Minimum: 7", logger.ListOfMessages[index++]);
			AssertEquals($"Information|{ArchiveManagerConstants.Codes.RED}|Set Batch Size: 100", logger.ListOfMessages[index++]);

			AssertEquals($"Information|{ArchiveManagerConstants.Codes.RED}|Configuration Parameters:", logger.ListOfMessages[index++]);
			AssertContains($"Information|{ArchiveManagerConstants.Codes.RED}|Purging Records on or Before:", logger.ListOfMessages[index++]);
			AssertEquals($"Information|{ArchiveManagerConstants.Codes.RED}|Max Run Duration: 10 minutes", logger.ListOfMessages[index++]);
			AssertEquals($"Information|{ArchiveManagerConstants.Codes.RED}|Verbose Logging: No", logger.ListOfMessages[index++]);

			AssertEquals($"Information|{ArchiveManagerConstants.Codes.RED}|Executing: Purge Expired Rates", logger.ListOfMessages[index++]);

			AssertContains($"Information|{ArchiveManagerConstants.Codes.RED}|Loaded batch of 1 RateEntry", logger.ListOfMessages[index++]);
			AssertContains($"Information|{ArchiveManagerConstants.Codes.RED}|Deleting 1 records from RateEntry", logger.ListOfMessages[index++]);
			AssertContains($"Information|{ArchiveManagerConstants.Codes.RED}|Loaded batch of 0 RateEntry", logger.ListOfMessages[index++]);

			AssertContains($"Information|{ArchiveManagerConstants.Codes.RED}|Time taken to load 1 Archive Set(s) of RateEntry record(s) in 1 batch(es)", logger.ListOfMessages[index++]);
			AssertContains($"Information|{ArchiveManagerConstants.Codes.RED}|Time taken to purge 1 RateEntry record(s) and their related record(s)", logger.ListOfMessages[index++]);
			AssertContains($"Information|{ArchiveManagerConstants.Codes.RED}|RateEntrys Per Hour: ", logger.ListOfMessages[index++]);
			AssertContains($"Information|{ArchiveManagerConstants.Codes.RED}|Purged Data Dated Between Earliest Possible Date and", logger.ListOfMessages[index++]);
			AssertContains($"Information|{ArchiveManagerConstants.Codes.RED}|Generated Purge Report and stored on eDocs tab of the Purge Schedule", logger.ListOfMessages[index++]);
			AssertContains($"Information|{ArchiveManagerConstants.Codes.RED}|Completed purging records", logger.ListOfMessages[index++]);
		}
		public void TestArchiveLoggerContainsCorrectStageNames_InOrder()
		{
			using (SystemDataRegistry.Instance.ExposeExpiredRatesArchiveSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				IntegrationTestHelper.RunArchiveLoggerContainsCorrectStageNames_InOrder(TestConfig, Factory, archiveLogger);
			}
		}

		public void TestArchiveStagesSortByMainDateFilterColumn()
		{
			using (SystemDataRegistry.Instance.ExposeExpiredRatesArchiveSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var system = TestHelpers.GetArchiveSystem(TestConfig.ArchiveSystemCodeToTest);
				var archiveBeforeDate = ZDateTime.Now;
				var configWithoutCustoms = new ArchiveConfiguration(archiveBeforeDate, 10, ZDateTime.UtcNow, false, shouldIncludeDeclarations: false);
				var configWithCustoms = new ArchiveConfiguration(archiveBeforeDate, 10, ZDateTime.UtcNow, false, shouldIncludeDeclarations: true);

				ArchiveManagerAssertions.AssertAllStageDescriptorsForSystemSortProperlyByMainDateFilterColumn(system, configWithCustoms);
				ArchiveManagerAssertions.AssertAllStageDescriptorsForSystemSortProperlyByMainDateFilterColumn(system, configWithoutCustoms);
			}
		}

		public void TestRunArchiveSystem_ArchivesRecordsBeforeOnOrBeforeMin()
		{
			using (SystemDataRegistry.Instance.ExposeExpiredRatesArchiveSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var archiveJobsOnOrBeforeThisDate = ZDateTime.UtcNow.AddYears(-OnOrBeforeMinimumValue - 1);
				var configuration =
					new ArchiveConfiguration(archiveJobsOnOrBeforeThisDate, 10, ZDateTime.UtcNow, false, false);
				var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.RED);

				var ratingHeader = Factory.NewWithValidTestData<RatingHeader>();
				var glbCompany = Factory.New<GlbCompany>();
				ratingHeader.TH_GC = glbCompany.PK;
				ratingHeader.TH_RateType = "GLB";
				ratingHeader.TH_QuoteDate = ZDateTime.UtcNow.Date.AddDays(-10);

				var rateEntry = Factory.New<RateEntry>();
				rateEntry.TI_OriginLRC = "AUSYD";
				rateEntry.TI_Mode = Core.Constants.RateMode.SCN;
				rateEntry.TI_RateCategory = "AIR";
				rateEntry.TI_GC_Publisher = glbCompany.PK;
				rateEntry.TI_TH = ratingHeader.PK;
				rateEntry.TI_RateStartDate = ZDateTime.UtcNow.Date.AddMonths(-2);
				rateEntry.TI_RateEndDate = ZDateTime.UtcNow.Date.AddMonths(-1);

				Factory.Save();

				CombineAssertions("Precondition", () =>
				{
					AssertEquals("Expected Rating Header to exist.", 1,
						Factory.GetDatabaseCount(typeof(RatingHeader),
							new ZQuery(RatingHeaderSchema.PK, ratingHeader.PK)));
					AssertEquals("Expected Rate Entry to exist.", 1,
						Factory.GetDatabaseCount(typeof(RateEntry), new ZQuery(RateEntrySchema.PK, rateEntry.PK)));
				});

				TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.RED, configuration, archiveLogger, schedule);

				CombineAssertions("Expected objects are archived correctly", () =>
				{
					AssertEquals("Expected Rating Header to not be archived.", 1,
						Factory.GetDatabaseCount(typeof(RatingHeader),
							new ZQuery(RatingHeaderSchema.PK, ratingHeader.PK)));
					AssertEquals("Expected Rate Entry to be archived.", 1,
						Factory.GetDatabaseCount(typeof(RateEntry), new ZQuery(RateEntrySchema.PK, rateEntry.PK)));
				});
			}
		}
		public void TestRunArchiveSystem_ExcludesRecordsAfterOnOrBeforeMin()
		{
			using (SystemDataRegistry.Instance.ExposeExpiredRatesArchiveSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var archiveJobsOnOrBeforeThisDate = ZDateTime.UtcNow.AddYears(-OnOrBeforeMinimumValue - 1);
				var configuration = new ArchiveConfiguration(archiveJobsOnOrBeforeThisDate, 10, ZDateTime.UtcNow, false, false);
				var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.RED);

				var ratingHeader = Factory.NewWithValidTestData<RatingHeader>();
				var glbCompany = Factory.New<GlbCompany>();
				ratingHeader.TH_GC = glbCompany.PK;
				ratingHeader.TH_RateType = "GLB";
				ratingHeader.TH_QuoteDate = ZDateTime.UtcNow.Date.AddDays(-10);

				var excludedRateEntry = Factory.New<RateEntry>();
				excludedRateEntry.TI_OriginLRC = "AUSYD";
				excludedRateEntry.TI_Mode = Core.Constants.RateMode.SCN;
				excludedRateEntry.TI_RateCategory = RatingConstants.RateCategory.AIR;
				excludedRateEntry.TI_GC_Publisher = glbCompany.PK;
				excludedRateEntry.TI_TH = ratingHeader.PK;
				excludedRateEntry.TI_RateStartDate = ZDateTime.UtcNow.Date.AddDays(-9);
				excludedRateEntry.TI_RateEndDate = ZDateTime.UtcNow.Date.AddDays(-8);

				var archivedRateEntry = Factory.New<RateEntry>();
				archivedRateEntry.TI_OriginLRC = "AUSYD";
				archivedRateEntry.TI_Mode = Core.Constants.RateMode.ALL;
				archivedRateEntry.TI_RateCategory = RatingConstants.RateCategory.SOR;
				archivedRateEntry.TI_GC_Publisher = glbCompany.PK;
				archivedRateEntry.TI_TH = ratingHeader.PK;
				archivedRateEntry.TI_RateStartDate = configuration.ArchiveJobsOnOrBeforeThisDate.Date.AddMonths(-2);
				archivedRateEntry.TI_RateEndDate = configuration.ArchiveJobsOnOrBeforeThisDate.Date.AddMonths(-1);

				Factory.Save();

				CombineAssertions("Precondition", () =>
				{
					AssertEquals("Expected Rating Header to exist.", 1, Factory.GetDatabaseCount(typeof(RatingHeader), new ZQuery(RatingHeaderSchema.PK, ratingHeader.PK)));
					AssertEquals("Expected Rate Entry 1 to exist.", 1, Factory.GetDatabaseCount(typeof(RateEntry), new ZQuery(RateEntrySchema.PK, excludedRateEntry.PK)));
					AssertEquals("Expected Rate Entry 2 to exist.", 1, Factory.GetDatabaseCount(typeof(RateEntry), new ZQuery(RateEntrySchema.PK, archivedRateEntry.PK)));
				});

				TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.RED, configuration, archiveLogger, schedule);

				CombineAssertions("Expected objects are archived correctly", () =>
				{
					AssertEquals("Expected Rating Header to not be archived.", 1, Factory.GetDatabaseCount(typeof(RatingHeader), new ZQuery(RatingHeaderSchema.PK, ratingHeader.PK)));
					AssertEquals("Expected Rate Entry 1 to not be archived.", 1, Factory.GetDatabaseCount(typeof(RateEntry), new ZQuery(RateEntrySchema.PK, excludedRateEntry.PK)));
					AssertEquals("Expected Rate Entry 2 to be archived.", 0, Factory.GetDatabaseCount(typeof(RateEntry), new ZQuery(RateEntrySchema.PK, archivedRateEntry.PK)));
				});
			}
		}
	}
}
