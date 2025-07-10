using System.Linq;
using CargoWise.Common;
using CargoWise.Data.Testing;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Business.Actions.ArchiveReport;
using Enterprise.ArchiveManager.Business.Schedule;
using Enterprise.ArchiveManager.Engine;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentScanning.Business.Test;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ArchiveManager.Test.Actions.ArchiveReport.ReportGenerators
{
	public class PurgeReportGeneratorTest : TestCaseWithDocumentFactory
	{
		public void TestReportTypeString()
			=> AssertEquals("Purge", new PurgeReportGenerator().ReportTypeString);

		[TestDate(2022, 02, 22, 10, 30, 0)]
		public void TestPrepareReportDataSource()
		{
			var purgeReportGenerator = new PurgeReportGenerator();
			var dummyStageDescriptor = new DummyArchiveStage();
			var dummySystemDescriptor = new DummyArchiveSystem();
			var schedule = Factory.New<ArchiveScheduleTask>();
			schedule.S5_ScheduleType = dummySystemDescriptor.Code;
			var logger = new TestArchiveLogger();
			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, false, shouldIncludeDeclarations: false);
			var stage = new ArchiveStage(dummyStageDescriptor, dummySystemDescriptor);
			dummyStageDescriptor.Setup(stage, schedule, config);
			dummyStageDescriptor.ArchiveStageStopWatch.Stop();
			_ = dummyStageDescriptor.ProcessingInfoPerTable.TryAdd(DummyBizoSchema.Constants.TableName, new TableProcessingInfo(1));

			purgeReportGenerator.PrepareReportDataSource(dummyStageDescriptor, dummySystemDescriptor, schedule, logger, config);

			var topLevelDataSource = purgeReportGenerator.TopLevelDataSource;

			CombineAssertions("Report Data Source should be prepared correctly", () =>
			{
				AssertEquals("Dummy Archive System", topLevelDataSource.ReportName);

				AssertEquals("Dummy Stage", topLevelDataSource.StageName);
				AssertEquals(1, topLevelDataSource.StageCount);
				AssertEquals("22-Feb-22 10:30:00", topLevelDataSource.StageStartTime);
				AssertEquals("22-Feb-22 10:30:00", topLevelDataSource.StageEndTime);
				AssertEquals("00:00:00:00", topLevelDataSource.StageDuration);

				AssertEquals(DummyBizoSchema.Constants.TableName, topLevelDataSource.MainArchiveTableName);
				AssertEquals(3600, topLevelDataSource.MainArchiveTablePerHour);
				AssertEquals(0, topLevelDataSource.TotaleDocs);
				AssertEquals(0, topLevelDataSource.StorageDocsPerHour);

				AssertEquals("Batch size should be set to registry default", 50, topLevelDataSource.BatchSize);
				AssertEquals("On or before minimum should be set to registry default", 7, topLevelDataSource.OnOrBeforeMinimum);

				AssertEquals("Should not allow date parameter selection", "N/A", topLevelDataSource.DateParameter);
				AssertEquals("On or before should be test date", "22/02/2022", topLevelDataSource.OnOrBeforeDate);
				AssertEquals(1, topLevelDataSource.MaxRunDuration);

				AssertEquals("Archived", topLevelDataSource.PastTenseVerb);
				AssertEquals("Earliest Possible Date", topLevelDataSource.StageStartDate);
				AssertEquals("22-Feb-22", topLevelDataSource.StageEndDate);
			});
		}

		[UseSnapshotProtection]
		[TestDate(2022, 02, 22, 10, 30, 0)]
		public void TestPrepareReportDataSourceForPDR()
		{
			var purgeReportGenerator = new PurgeReportGenerator();
			var stageDescriptor = new PDRArchiveStageDescriptor();
			var systemDescriptor = new PDRPurgeSystemDescriptor();
			var schedule = Factory.New<ArchiveScheduleTask>();
			schedule.S5_ScheduleType = systemDescriptor.Code;
			var logger = new TestArchiveLogger();
			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, false, shouldIncludeDeclarations: false);
			config.SetIsFilteringByJobOpenDate(true);
			var stage = new ArchiveStage(stageDescriptor, systemDescriptor);
			stageDescriptor.Setup(stage, schedule, config);
			stageDescriptor.ArchiveStageStopWatch.Stop();
			_ = stageDescriptor.ProcessingInfoPerTable.TryAdd(DummyBizoSchema.Constants.TableName, new TableProcessingInfo(1));

			purgeReportGenerator.PrepareReportDataSource(stageDescriptor, systemDescriptor, schedule, logger, config);

			var topLevelDataSource = purgeReportGenerator.TopLevelDataSource;

			CombineAssertions("Report Data Source Should be prepared correctly", () =>
			{
				AssertEquals("Purge Documents and Records", topLevelDataSource.ReportName);

				AssertEquals("Purge Documents and Records", topLevelDataSource.StageName);
				AssertEquals(1, topLevelDataSource.StageCount);
				AssertEquals("22-Feb-22 10:30:00", topLevelDataSource.StageStartTime);
				AssertEquals("22-Feb-22 10:30:00", topLevelDataSource.StageEndTime);
				AssertEquals("00:00:00:00", topLevelDataSource.StageDuration);

				AssertEquals(JobHeaderSchema.Constants.TableName, topLevelDataSource.MainArchiveTableName);
				AssertEquals(0, topLevelDataSource.MainArchiveTablePerHour);
				AssertEquals(0, topLevelDataSource.TotaleDocs);
				AssertEquals(0, topLevelDataSource.StorageDocsPerHour);

				AssertEquals(50, topLevelDataSource.BatchSize);
				AssertEquals(7, topLevelDataSource.OnOrBeforeMinimum);

				AssertEquals("Filtering by Job Open Date should be true", "Job Open Date", topLevelDataSource.DateParameter);
				AssertEquals("On or before data should be the test date", "22/02/2022", topLevelDataSource.OnOrBeforeDate);
				AssertEquals(1, topLevelDataSource.MaxRunDuration);

				AssertEquals("Purged", topLevelDataSource.PastTenseVerb);
				AssertEquals("Earliest Possible Date", topLevelDataSource.StageStartDate);
				AssertEquals("22-Feb-22", topLevelDataSource.StageEndDate);
			});
		}

		[TestDate(2022, 02, 22)]
		public void TestGenerateReport()
		{
			var purgeReportGenerator = new PurgeReportGenerator();
			var dummyStageDescriptor = new DummyArchiveStage();
			var dummySystemDescriptor = new DummyArchiveSystem();
			var schedule = Factory.New<ArchiveScheduleTask>();
			schedule.S5_ScheduleType = dummySystemDescriptor.Code;
			var logger = new TestArchiveLogger();
			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, false, shouldIncludeDeclarations: false);
			var stage = new ArchiveStage(dummyStageDescriptor, dummySystemDescriptor);
			dummyStageDescriptor.Setup(stage, schedule, config);
			dummyStageDescriptor.ArchiveStageStopWatch.Stop();
			_ = dummyStageDescriptor.ProcessingInfoPerTable.TryAdd(DummyBizoSchema.Constants.TableName, new TableProcessingInfo(1));

			purgeReportGenerator.GenerateReport(dummyStageDescriptor, dummySystemDescriptor, schedule, logger, config);

			AssertLog(logger);
			AssertPurgeReport(schedule);
		}

		[UseSnapshotProtection]
		[TestDate(2022, 02, 22)]
		public void TestGenerateReportForPDR()
		{
			var purgeReportGenerator = new PurgeReportGenerator();
			var stageDescriptor = new PDRArchiveStageDescriptor();
			var systemDescriptor = new PDRPurgeSystemDescriptor();
			var schedule = Factory.New<ArchiveScheduleTask>();
			schedule.S5_ScheduleType = systemDescriptor.Code;
			var logger = new TestArchiveLogger();
			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, false, shouldIncludeDeclarations: false);
			config.SetIsFilteringByJobOpenDate(true);
			var stage = new ArchiveStage(stageDescriptor, systemDescriptor);
			stageDescriptor.Setup(stage, schedule, config);
			stageDescriptor.ArchiveStageStopWatch.Stop();
			_ = stageDescriptor.ProcessingInfoPerTable.TryAdd(DummyBizoSchema.Constants.TableName, new TableProcessingInfo(1));

			purgeReportGenerator.GenerateReport(stageDescriptor, systemDescriptor, schedule, logger, config);

			AssertLogForPDR(logger);
			AssertPurgeReportForPDR(schedule);
		}

		void AssertLog(TestArchiveLogger logger)
		{
			CombineAssertions("Logs did not contain the correct messages", () =>
			{
				AssertEquals(3, logger.ListOfMessages.Count);

				AssertEquals("Log 1: ", "Information|DMY|DummyBizos Per Hour: 3600, StorageDocs Per Hour: 0", logger.ListOfMessages[0]);
				AssertEquals("Log 2: ", $"Information|DMY|Archived Data Dated Between Earliest Possible Date and 22-Feb-22{System.Environment.NewLine}" +
					$"DummyBizo: 1{System.Environment.NewLine}",
					logger.ListOfMessages[1]);
				AssertEquals("Log 3: ", "Information|DMY|Generated Purge Report and stored on eDocs tab of the Purge Schedule", logger.ListOfMessages[2]);
			});
		}

		void AssertLogForPDR(TestArchiveLogger logger)
		{
			CombineAssertions("Logs did not contain the correct messages", () =>
			{
				AssertEquals(3, logger.ListOfMessages.Count);

				AssertEquals("Log 1: ", "Information|PDR|JobHeaders Per Hour: 0, StorageDocs Per Hour: 0", logger.ListOfMessages[0]);
				AssertEquals("Log 2: ", $"Information|PDR|Purged Data Dated Between Earliest Possible Date and 22-Feb-22{System.Environment.NewLine}" +
					$"DummyBizo: 1{System.Environment.NewLine}",
					logger.ListOfMessages[1]);
				AssertEquals("Log 3: ", "Information|PDR|Generated Purge Report and stored on eDocs tab of the Purge Schedule", logger.ListOfMessages[2]);
			});
		}

		void AssertPurgeReport(ArchiveScheduleTask schedule)
		{
			var archiveScheduleDocuments = TestHelpers.GetAllReportsFromSchedule(schedule);

			AssertEquals("There should be 1 report attached to the purge schedule.", 1, archiveScheduleDocuments.Count());

			var purgeReport = archiveScheduleDocuments.First();

			AssertEquals("DummyArchiveSystemReport_202202220000", purgeReport.FileNameOnly);

			using var stream = purgeReport.GetImageDataReader();
			using var excel = new ExcelInterface();
			excel.LoadExcelFile(stream);

			var worksheet = excel.WorkSheets[0];
			var rowCount = 0;
			var inputDataColumn = 4;

			CombineAssertions(() =>
			{
				AssertEquals(Core.Constants.FileFormats.XLSX, excel.GetExtensionForExcelFromFile());

				AssertEquals("Dummy Archive System Summary Report", worksheet[rowCount, 1].ToString());

				AssertEquals("Database Server", worksheet[++rowCount, 1].ToString());
				AssertNotNullOrEmpty("Cell should contain the current Database Server.", worksheet[rowCount, inputDataColumn].ToString());

				AssertEquals("Database Name", worksheet[++rowCount, 1].ToString());
				AssertNotNullOrEmpty("Cell should contain the current Database Name.", worksheet[rowCount, inputDataColumn].ToString());

				AssertEquals("Start Time", worksheet[++rowCount, 1].ToString());
				AssertEquals("Value for 'Start Time'", "22-Feb-22 00:00:00", worksheet[rowCount, inputDataColumn].ToString());

				AssertEquals("End Time", worksheet[++rowCount, 1].ToString());
				AssertEquals("Value for 'End Time'", "22-Feb-22 00:00:00", worksheet[rowCount, inputDataColumn].ToString());

				AssertEquals("Duration", worksheet[++rowCount, 1].ToString());
				AssertEquals("Value for 'Duration'", "00:00:00:00", worksheet[rowCount, inputDataColumn].ToString());

				AssertEquals("Total eDocs Purged", worksheet[++rowCount, 1].ToString());
				AssertEquals("Value for 'Total eDocs Purged'", "0", worksheet[rowCount, 4].ToString());

				AssertEquals("eDocs Purged Per Hour", worksheet[++rowCount, 1].ToString());
				AssertEquals("Value for 'eDocsPurgedPerHour'", "0", worksheet[rowCount, 4].ToString());

				AssertEquals("DummyBizos Per Hour", worksheet[++rowCount, 1].ToString());
				AssertEquals("Value for 'DummyBizos Per Hour'", "3600", worksheet[rowCount, 4].ToString());
				AssertEquals(string.Empty, worksheet[++rowCount, 1].ToString());

				AssertEquals("Registry Settings:", worksheet[++rowCount, 1].ToString());
				AssertEquals("Batch Size", worksheet[++rowCount, 1].ToString());
				AssertEquals("Value for 'Batch Size'", "50", worksheet[rowCount, 4].ToString());
				AssertEquals("On Or Before Minimum", worksheet[++rowCount, 1].ToString());
				AssertEquals("Value for 'On Or Before Minimum'", "7", worksheet[rowCount, 4].ToString());
				AssertEquals(string.Empty, worksheet[++rowCount, 1].ToString());

				AssertEquals("Archive Schedule Parameters:", worksheet[++rowCount, 1].ToString());
				AssertEquals("On Or Before Date", worksheet[++rowCount, 1].ToString());
				AssertEquals("Value for 'On Or Before Date'", "22/02/2022", worksheet[rowCount, 4].ToString());
				AssertEquals("Max. Run Duration (Minutes)", worksheet[++rowCount, 1].ToString());
				AssertEquals("Value for 'Max. Run Duration (Minutes)'", "1", worksheet[rowCount, 4].ToString());
				AssertEquals(string.Empty, worksheet[++rowCount, 1].ToString());

				AssertEquals("Archived Data Dated Between Earliest Possible Date And 22-Feb-22", worksheet[++rowCount, 1].ToString());
				AssertEquals("Table Name", worksheet[++rowCount, 1].ToString());
				AssertEquals("Deleted Record Count", worksheet[rowCount, 5].ToString());
				AssertEquals("Record Count", worksheet[rowCount, 7].ToString());
				AssertEquals("Approximate Space Used (MB)", worksheet[rowCount, 9].ToString());

				AssertEquals("Summary report should include purged data: Table Name", DummyBizoSchema.Constants.TableName, worksheet[++rowCount, 1].ToString());
				AssertEquals("Summary report should include purged data: Deleted Record Count", "1", worksheet[rowCount, 5].ToString());
				AssertEquals("Summary report should include purged data: Record Count", "0", worksheet[rowCount, 7].ToString());
				Assert("Summary report should include purged data: Approximate Space Used (MB)", !worksheet[rowCount, 9].ToString().IsNullOrEmpty());
			});
		}

		void AssertPurgeReportForPDR(ArchiveScheduleTask schedule)
		{
			var archiveScheduleDocuments = TestHelpers.GetAllReportsFromSchedule(schedule);

			AssertEquals("There should be 1 report attached to the purge schedule.", 1, archiveScheduleDocuments.Count());

			var purgeReport = archiveScheduleDocuments.First();

			AssertEquals("PurgeDocumentsandRecordsReport_202202220000", purgeReport.FileNameOnly);

			using var stream = purgeReport.GetImageDataReader();
			using var excel = new ExcelInterface();
			excel.LoadExcelFile(stream);

			var worksheet = excel.WorkSheets[0];
			var rowCount = 0;
			var inputDataColumn = 4;

			CombineAssertions(() =>
			{
				AssertEquals(Core.Constants.FileFormats.XLSX, excel.GetExtensionForExcelFromFile());

				AssertEquals("Purge Documents and Records Summary Report", worksheet[rowCount, 1].ToString());

				AssertEquals("Database Server", worksheet[++rowCount, 1].ToString());
				AssertNotNullOrEmpty("Cell should contain the current Database Server.", worksheet[rowCount, inputDataColumn].ToString());

				AssertEquals("Database Name", worksheet[++rowCount, 1].ToString());
				AssertNotNullOrEmpty("Cell should contain the current Database Name.", worksheet[rowCount, inputDataColumn].ToString());

				AssertEquals("Start Time", worksheet[++rowCount, 1].ToString());
				AssertEquals("Value for 'Start Time'", "22-Feb-22 00:00:00", worksheet[rowCount, inputDataColumn].ToString());

				AssertEquals("End Time", worksheet[++rowCount, 1].ToString());
				AssertEquals("Value for 'End Time'", "22-Feb-22 00:00:00", worksheet[rowCount, inputDataColumn].ToString());

				AssertEquals("Duration", worksheet[++rowCount, 1].ToString());
				AssertEquals("Value for 'Duration'", "00:00:00:00", worksheet[rowCount, inputDataColumn].ToString());

				AssertEquals("Total eDocs Purged", worksheet[++rowCount, 1].ToString());
				AssertEquals("Value for 'Total eDocs Purged'", "0", worksheet[rowCount, 4].ToString());

				AssertEquals("eDocs Purged Per Hour", worksheet[++rowCount, 1].ToString());
				AssertEquals("Value for 'eDocsPurgedPerHour'", "0", worksheet[rowCount, 4].ToString());

				AssertEquals("JobHeaders Per Hour", worksheet[++rowCount, 1].ToString());
				AssertEquals("Value for 'JobHeaders Per Hour'", "0", worksheet[rowCount, 4].ToString());
				AssertEquals(string.Empty, worksheet[++rowCount, 1].ToString());

				AssertEquals("Registry Settings:", worksheet[++rowCount, 1].ToString());
				AssertEquals("Batch Size", worksheet[++rowCount, 1].ToString());
				AssertEquals("Value for 'Batch Size'", "50", worksheet[rowCount, 4].ToString());
				AssertEquals("On Or Before Minimum", worksheet[++rowCount, 1].ToString());
				AssertEquals("Value for 'On Or Before Minimum'", "7", worksheet[rowCount, 4].ToString());
				AssertEquals(string.Empty, worksheet[++rowCount, 1].ToString());

				AssertEquals("Archive Schedule Parameters:", worksheet[++rowCount, 1].ToString());
				AssertEquals("Date Parameter", worksheet[++rowCount, 1].ToString());
				AssertEquals("Value for 'Date Parameter'", "Job Open Date", worksheet[rowCount, 4].ToString());
				AssertEquals("On Or Before Date", worksheet[++rowCount, 1].ToString());
				AssertEquals("Value for 'On Or Before Date'", "22/02/2022", worksheet[rowCount, 4].ToString());
				AssertEquals("Max. Run Duration (Minutes)", worksheet[++rowCount, 1].ToString());
				AssertEquals("Value for 'Max. Run Duration (Minutes)'", "1", worksheet[rowCount, 4].ToString());
				AssertEquals(string.Empty, worksheet[++rowCount, 1].ToString());

				AssertEquals("Purged Data Dated Between Earliest Possible Date And 22-Feb-22", worksheet[++rowCount, 1].ToString());
				AssertEquals("Table Name", worksheet[++rowCount, 1].ToString());
				AssertEquals("Deleted Record Count", worksheet[rowCount, 5].ToString());
				AssertEquals("Record Count", worksheet[rowCount, 7].ToString());
				AssertEquals("Approximate Space Used (MB)", worksheet[rowCount, 9].ToString());

				AssertEquals("Summary report should include purged data: Table Name", DummyBizoSchema.Constants.TableName, worksheet[++rowCount, 1].ToString());
				AssertEquals("Summary report should include purged data: Deleted Record Count", "1", worksheet[rowCount, 5].ToString());
				AssertEquals("Summary report should include purged data: Record Count", "0", worksheet[rowCount, 7].ToString());
				Assert("Summary report should include purged data: Approximate Space Used (MB)", !worksheet[rowCount, 9].ToString().IsNullOrEmpty());
			});
		}
	}
}
