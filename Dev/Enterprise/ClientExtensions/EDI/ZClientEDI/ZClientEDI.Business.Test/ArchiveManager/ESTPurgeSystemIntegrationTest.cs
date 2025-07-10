using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Engine;
using Enterprise.ArchiveManager.Test;
using Enterprise.ArchiveManager.Test.SystemDescriptors;
using Enterprise.Billing.Business;
using Enterprise.Billing.Business.Testing;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.ApplicationLogging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace ZClientEDI.Business.Test.ArchiveManager
{
	public class ESTPurgeSystemIntegrationTest : ArchiveSystemIntegrationTest, IArchiveSystemDescriptorIntegrationTest
	{
		public ESTPurgeSystemIntegrationTest()
		{
			TestConfig = new TestConfiguration()
			{
				ArchiveSystemCodeToTest = ArchiveManagerConstants.Codes.EST,
				ListOfStageNames = ["Purge Application Active Logger"]
			};
		}

		protected override void SetUp()
		{
			TestCaseHelper.RunClientDbCreateScripts();
			TestCaseHelper.ClearTable(ApplicationActiveLoggerSchema.Constants.TableName);

			base.SetUp();
		}

		[TestDate(2025, 01, 01)]
		public void TestGeneratedSummaryReport()
		{
			SetupArchiveableObjects(out var activeLogger2022, out var activeLogger2024);
			AssertEquals("Precondition - both records exist", 2, Factory.GetDatabaseCount(typeof(ApplicationActiveLogger)));

			var config = new ArchiveConfiguration(new ZDateTime(2023, 1, 1), 1, ZDateTime.UtcNow, false, true);
			var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);

			using (EDIDataRegistry.Instance.PurgeEdiProdSpecificTablesBatchSizeControl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 50))
			using (EDIDataRegistry.Instance.PurgeEdiProdSpecificTablesOnOrBeforeMinimum.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			{
				TestHelpers.RunArchiveSystem(TestConfig.ArchiveSystemCodeToTest, config, archiveLogger, archiveSchedule);
			}

			CombineAssertions(() =>
			{
				AssertEquals("Report for each stage should have been generated.", 1, TestHelpers.GetAllReportsFromSchedule(archiveSchedule).Count());
				AssertEquals("2022 should have been deleted", 0, Factory.GetDatabaseCount(typeof(ApplicationActiveLogger), new(ApplicationActiveLoggerSchema.PK, activeLogger2022.PK)));
				AssertEquals("2024 should not have been deleted", 1, Factory.GetDatabaseCount(typeof(ApplicationActiveLogger), new(ApplicationActiveLoggerSchema.PK, activeLogger2024.PK)));
			});

			ArchiveManagerAssertions.AssertPurgeSummaryReport(
				archiveReport: TestHelpers.GetAllReportsFromSchedule(archiveSchedule).Single(),
				purgeCode: TestConfig.ArchiveSystemCodeToTest,
				mainArchiveTableName: ApplicationActiveLoggerSchema.Constants.TableName,
				archiveSystemHasMultipleArchiveStageDescriptors: false);
		}

		[TestDate(2025, 01, 01)]
		public void TestLoggingOfAMUsageData()
		{
			SetupArchiveableObjects(out _, out _);
			AssertEquals("Precondition - both records exist", 2, Factory.GetDatabaseCount(typeof(ApplicationActiveLogger)));

			using (EDIDataRegistry.Instance.PurgeEdiProdSpecificTablesBatchSizeControl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 60))
			using (EDIDataRegistry.Instance.PurgeEdiProdSpecificTablesOnOrBeforeMinimum.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			{
				var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.EST);
				archiveSchedule.IsArchiveRecordsOnOrBeforeDate = true;
				archiveSchedule.UseOnOrBeforeDateWhenWatermarkReset = true;
				archiveSchedule.ArchiveRecordsOnOrBeforeDate = ZDateTime.UtcNow;
				archiveSchedule.Run();
			}

			AssertEquals("Both records should have been purged", 0, Factory.GetDatabaseCount(typeof(ApplicationActiveLogger)));

			var usageCollectorTestHelper = new UsageCollectorTestHelper(Factory);
			var messages = usageCollectorTestHelper.LoadUsageMessages(UsageFeatures.Codes.ArchiveManager);
			var usageProperties = messages.Single().UsageProperties;

			ArchiveManagerAssertions.AssertResultsForAMUsageCollector(
				usageProperties: usageProperties,
				archiveCode: TestConfig.ArchiveSystemCodeToTest,
				archiveSystemStageName: "Purge Application Active Logger",
				includeCustomsJobs: false,
				batchSize: 60,
				totalRecordsDeleted: 2,
				jobHeadersProcessed: 0,
				mainRecordsLoaded: 2,
				totalMissingDocumentsGenerated: 0,
				documentsDeleted: 0);
		}

		[TestDate(2025, 01, 01)]
		public void TestDoesNotPurgeIneligibleRecords()
		{
			SetupArchiveableObjects(out var activeLogger2022, out var activeLogger2024);
			AssertEquals("Precondition - both records exist", 2, Factory.GetDatabaseCount(typeof(ApplicationActiveLogger)));

			var config = new ArchiveConfiguration(new ZDateTime(2023, 1, 1), 1, ZDateTime.UtcNow, false, true);
			var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);

			using (EDIDataRegistry.Instance.PurgeEdiProdSpecificTablesBatchSizeControl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 50))
			using (EDIDataRegistry.Instance.PurgeEdiProdSpecificTablesOnOrBeforeMinimum.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			{
				TestHelpers.RunArchiveSystem(TestConfig.ArchiveSystemCodeToTest, config, archiveLogger, archiveSchedule);
			}

			CombineAssertions(() =>
			{
				AssertEquals("2022 should have been deleted", 0, Factory.GetDatabaseCount(typeof(ApplicationActiveLogger), new(ApplicationActiveLoggerSchema.PK, activeLogger2022.PK)));
				AssertEquals("2024 should not have been deleted", 1, Factory.GetDatabaseCount(typeof(ApplicationActiveLogger), new(ApplicationActiveLoggerSchema.PK, activeLogger2024.PK)));
			});
		}

		public void TestArchiveLoggerContainsCorrectStageNames_InOrder()
			=> IntegrationTestHelper.RunArchiveLoggerContainsCorrectStageNames_InOrder(TestConfig, Factory, archiveLogger);

		public void TestArchiveStagesSortByMainDateFilterColumn()
			=> IntegrationTestHelper.RunArchiveStagesSortByMainDateFilterColumn(TestConfig);

		void SetupArchiveableObjects(out ApplicationActiveLogger activeLogger2022, out ApplicationActiveLogger activeLogger2024)
		{
			var logger = Factory.NewWithValidTestData<ApplicationLogger>();
			activeLogger2022 = Factory.New<ApplicationActiveLogger>();
			activeLogger2024 = Factory.New<ApplicationActiveLogger>();

			activeLogger2022.AAL_ALG_ApplicationLogger = logger.PK;
			activeLogger2022.AAL_Environment = Guid.NewGuid().ToString();
			activeLogger2024.AAL_ALG_ApplicationLogger = logger.PK;
			activeLogger2024.AAL_Environment = Guid.NewGuid().ToString();
			logger.ALG_Product = "CargoWise";
			Factory.Save();

			_ = Db.Connection.ExecuteNonQuery($"UPDATE dbo.ApplicationActiveLogger SET AAL_SystemLastEditTimeUtc = '2022-01-01' WHERE AAL_PK = {activeLogger2022.PK.ToSqlGuid()}");
			_ = Db.Connection.ExecuteNonQuery($"UPDATE dbo.ApplicationActiveLogger SET AAL_SystemLastEditTimeUtc = '2024-01-01' WHERE AAL_PK = {activeLogger2024.PK.ToSqlGuid()}");

			activeLogger2022.Reload();
			activeLogger2024.Reload();

			AssertEquals(nameof(activeLogger2022), new ZDateTime(2022, 1, 1), activeLogger2022.AAL_SystemLastEditTimeUtc);
			AssertEquals(nameof(activeLogger2024), new ZDateTime(2024, 1, 1), activeLogger2024.AAL_SystemLastEditTimeUtc);
		}
	}
}
