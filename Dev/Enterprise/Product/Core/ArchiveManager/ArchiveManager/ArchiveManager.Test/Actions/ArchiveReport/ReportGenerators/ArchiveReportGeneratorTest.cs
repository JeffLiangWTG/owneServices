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
	public class ArchiveReportGeneratorTest : TestCaseWithDocumentFactory
	{
		public void TestReportTypeString()
			=> AssertEquals("Archive", new ArchiveReportGenerator().ReportTypeString);

		[TestDate(2022, 02, 22, 10, 30, 0)]
		public void TestPrepareReportDataSource()
		{
			var archiveReportGenerator = new ArchiveReportGenerator();
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

			archiveReportGenerator.PrepareReportDataSource(dummyStageDescriptor, dummySystemDescriptor, schedule, logger, config);

			var topLevelDataSource = archiveReportGenerator.TopLevelDataSource;

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

				AssertEquals("Batch size should be set to registry default", 50, topLevelDataSource.BatchSize);
				AssertEquals("On or before minimum should be set to registry default", 7, topLevelDataSource.OnOrBeforeMinimum);

				AssertEquals("Should not allow date parameter selection", "N/A", topLevelDataSource.DateParameter);
				AssertEquals("On or before date should be the test date", "22/02/2022", topLevelDataSource.OnOrBeforeDate);
				AssertEquals(1, topLevelDataSource.MaxRunDuration);

				AssertEquals("Archived", topLevelDataSource.PastTenseVerb);
				AssertEquals("Earliest Possible Date", topLevelDataSource.StageStartDate);
				AssertEquals("22-Feb-22", topLevelDataSource.StageEndDate);
			});
		}

		[UseSnapshotProtection]
		[TestDate(2022, 02, 22, 10, 30, 0)]
		public void TestPrepareReportDataSourceForOPS()
		{
			var archiveReportGenerator = new ArchiveReportGenerator();
			var stageDescriptor = new OPSArchiveStageDescriptor();
			var systemDescriptor = new OPSArchiveSystemDescriptor();
			var schedule = Factory.New<ArchiveScheduleTask>();
			schedule.S5_ScheduleType = systemDescriptor.Code;
			var logger = new TestArchiveLogger();
			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, false, shouldIncludeDeclarations: false);
			config.SetIsFilteringByJobOpenDate(true);
			var stage = new ArchiveStage(stageDescriptor, systemDescriptor);
			stageDescriptor.Setup(stage, schedule, config);
			stageDescriptor.ArchiveStageStopWatch.Stop();
			_ = stageDescriptor.ProcessingInfoPerTable.TryAdd(DummyBizoSchema.Constants.TableName, new TableProcessingInfo(1));

			archiveReportGenerator.PrepareReportDataSource(stageDescriptor, systemDescriptor, schedule, logger, config);

			var topLevelDataSource = archiveReportGenerator.TopLevelDataSource;

			CombineAssertions("Report Data Source should be prepared correctly", () =>
			{
				AssertEquals("Operational Jobs Archive System", topLevelDataSource.ReportName);

				AssertEquals("Operational Jobs Archive", topLevelDataSource.StageName);
				AssertEquals(1, topLevelDataSource.StageCount);
				AssertEquals("22-Feb-22 10:30:00", topLevelDataSource.StageStartTime);
				AssertEquals("22-Feb-22 10:30:00", topLevelDataSource.StageEndTime);
				AssertEquals("00:00:00:00", topLevelDataSource.StageDuration);

				AssertEquals(JobHeaderSchema.Constants.TableName, topLevelDataSource.MainArchiveTableName);
				AssertEquals(0, topLevelDataSource.MainArchiveTablePerHour);

				AssertEquals("Batch size should be set to registry default", 50, topLevelDataSource.BatchSize);
				AssertEquals("On or before minimum should be set to registry default", 7, topLevelDataSource.OnOrBeforeMinimum);

				AssertEquals("Filtering By Job Open Date should be true", "Job Open Date", topLevelDataSource.DateParameter);
				AssertEquals("On or before date should be the test date", "22/02/2022", topLevelDataSource.OnOrBeforeDate);
				AssertEquals(1, topLevelDataSource.MaxRunDuration);

				AssertEquals("Archived", topLevelDataSource.PastTenseVerb);
				AssertEquals("Earliest Possible Date", topLevelDataSource.StageStartDate);
				AssertEquals("22-Feb-22", topLevelDataSource.StageEndDate);
			});
		}

		[TestDate(2022, 02, 22)]
		public void TestGenerateReport()
		{
			var archiveReportGenerator = new ArchiveReportGenerator();
			var dummyStageDescriptor = new DummyArchiveStage();
			var dummySystemDescriptor = new DummyArchiveSystem();
			var schedule = Factory.New<ArchiveScheduleTask>();
			schedule.S5_ScheduleType = dummySystemDescriptor.Code;
			var logger = new TestArchiveLogger();
			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, false, shouldIncludeDeclarations: false);
			var stage = new ArchiveStage(dummyStageDescriptor, dummySystemDescriptor);
			dummyStageDescriptor.Setup(stage, schedule, config);
			dummyStageDescriptor.ArchiveStageStopWatch.Stop();
			_ = dummyStageDescriptor.ProcessingInfoPerTable.TryAdd(DummyBizoSchema.Constants.TableName, new TableProcessingInfo(10));
			_ = dummyStageDescriptor.ProcessingInfoPerTable.TryAdd(DummyDependentBizoSchema.Constants.TableName, new TableProcessingInfo(5));

			archiveReportGenerator.GenerateReport(dummyStageDescriptor, dummySystemDescriptor, schedule, logger, config);

			AssertLog(logger);
			AssertArchiveReport(schedule);
		}

		[UseSnapshotProtection]
		[TestDate(2022, 02, 22)]
		public void TestGenerateReportForOPS()
		{
			var archiveReportGenerator = new ArchiveReportGenerator();
			var stageDescriptor = new OPSArchiveStageDescriptor();
			var systemDescriptor = new OPSArchiveSystemDescriptor();
			var schedule = Factory.New<ArchiveScheduleTask>();
			schedule.S5_ScheduleType = systemDescriptor.Code;
			var logger = new TestArchiveLogger();
			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, false, shouldIncludeDeclarations: false);
			config.SetIsFilteringByJobOpenDate(true);
			var stage = new ArchiveStage(stageDescriptor, systemDescriptor);
			stageDescriptor.Setup(stage, schedule, config);
			stageDescriptor.ArchiveStageStopWatch.Stop();
			_ = stageDescriptor.ProcessingInfoPerTable.TryAdd(DummyBizoSchema.Constants.TableName, new TableProcessingInfo(10));
			_ = stageDescriptor.ProcessingInfoPerTable.TryAdd(DummyDependentBizoSchema.Constants.TableName, new TableProcessingInfo(5));

			archiveReportGenerator.GenerateReport(stageDescriptor, systemDescriptor, schedule, logger, config);

			AssertLogForOPS(logger);
			AssertArchiveReportForOPS(schedule);
		}

		void AssertLog(TestArchiveLogger logger)
		{
			CombineAssertions("Logs did not contain the correct messages", () =>
			{
				AssertEquals(3, logger.ListOfMessages.Count);

				AssertEquals("Log 1: ", "Information|DMY|DummyBizos Per Hour: 36000", logger.ListOfMessages[0]);
				AssertEquals("Log 2: ", $"Information|DMY|Archived Data Dated Between Earliest Possible Date and 22-Feb-22{System.Environment.NewLine}" +
					$"DummyBizo: 10{System.Environment.NewLine}" +
					$"DummyDependentBizo: 5{System.Environment.NewLine}",
					logger.ListOfMessages[1]);
				AssertEquals("Log 3: ", "Information|DMY|Generated Archive Report and stored on eDocs tab of the Archive Schedule", logger.ListOfMessages[2]);
			});
		}

		void AssertLogForOPS(TestArchiveLogger logger)
		{
			CombineAssertions("OPS logs did not contain the correct messages", () =>
			{
				AssertEquals(3, logger.ListOfMessages.Count);

				AssertEquals("Log 1: ", "Information|OPS|JobShipments Per Hour: 0, JobHeaders Per Hour: 0", logger.ListOfMessages[0]);
				AssertEquals("Log 2: ", $"Information|OPS|Archived Data Dated Between Earliest Possible Date and 22-Feb-22{System.Environment.NewLine}" +
					$"DummyBizo: 10{System.Environment.NewLine}" +
					$"DummyDependentBizo: 5{System.Environment.NewLine}",
					logger.ListOfMessages[1]);
				AssertEquals("Log 3: ", "Information|OPS|Generated Archive Report and stored on eDocs tab of the Archive Schedule", logger.ListOfMessages[2]);
			});
		}

		void AssertArchiveReport(ArchiveScheduleTask schedule)
		{
			var archiveScheduleDocuments = TestHelpers.GetAllReportsFromSchedule(schedule);

			AssertEquals("There should be 1 report attached to the archive schedule.", 1, archiveScheduleDocuments.Count());

			var archiveReport = archiveScheduleDocuments.First();

			AssertEquals("DummyArchiveSystemReport_202202220000", archiveReport.FileNameOnly);

			using var stream = archiveReport.GetImageDataReader();
			using var excel = new ExcelInterface();
			excel.LoadExcelFile(stream);

			var worksheet = excel.WorkSheets[0];
			var rowCount = 0;
			var inputDataColumn = 2;

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

				AssertEquals("DummyBizos Per Hour", worksheet[++rowCount, 1].ToString());
				AssertEquals("Value for 'DummyBizos Per Hour'", "36000", worksheet[rowCount, inputDataColumn].ToString());
				AssertEquals(string.Empty, worksheet[++rowCount, 1].ToString());

				AssertEquals("Registry Settings:", worksheet[++rowCount, 1].ToString());
				AssertEquals("Batch Size", worksheet[++rowCount, 1].ToString());
				AssertEquals("Value for 'Batch Size'", "50", worksheet[rowCount, inputDataColumn + 1].ToString());
				AssertEquals("On Or Before Minimum", worksheet[++rowCount, 1].ToString());
				AssertEquals("Value for 'On Or Before Minimum'", "7", worksheet[rowCount, inputDataColumn + 1].ToString());
				AssertEquals(string.Empty, worksheet[++rowCount, 1].ToString());

				AssertEquals("Archive Schedule Parameters:", worksheet[++rowCount, 1].ToString());
				AssertEquals("On Or Before Date", worksheet[++rowCount, 1].ToString());
				AssertEquals("Value for 'On Or Before Date'", "22/02/2022", worksheet[rowCount, inputDataColumn + 1].ToString());
				AssertEquals("Max. Run Duration (Minutes)", worksheet[++rowCount, 1].ToString());
				AssertEquals("Value for 'Max. Run Duration (Minutes)'", "1", worksheet[rowCount, inputDataColumn + 1].ToString());
				AssertEquals(string.Empty, worksheet[++rowCount, 1].ToString());

				AssertEquals("Archived Data Dated Between Earliest Possible Date And 22-Feb-22", worksheet[++rowCount, 1].ToString());
				AssertEquals("Table Name", worksheet[++rowCount, 1].ToString());
				AssertEquals("Deleted Record Count", worksheet[rowCount, 2].ToString());
				AssertEquals("Record Count", worksheet[rowCount, 4].ToString());
				AssertEquals("Approximate Space Used (MB)", worksheet[rowCount, 6].ToString());

				AssertEquals("Summary report should include archived data: DummyBizo", DummyBizoSchema.Constants.TableName, worksheet[++rowCount, 1].ToString());
				AssertEquals("Summary report should include archived data: Deleted Record Count", "10", worksheet[rowCount, 2].ToString());
				AssertEquals("Summary report should include archived data: Record Count", "0", worksheet[rowCount, 4].ToString());
				Assert("Summary report should include archived data: Approximate Space Used (MB)", !worksheet[rowCount, 6].ToString().IsNullOrEmpty());

				AssertEquals("Summary report should include archived data: DummyDependentBizo", DummyDependentBizoSchema.Constants.TableName, worksheet[++rowCount, 1].ToString());
				AssertEquals("Summary report should include archived data: Deleted Record Count", "5", worksheet[rowCount, 2].ToString());
				AssertEquals("Summary report should include archived data: Record Count", "0", worksheet[rowCount, 4].ToString());
				Assert("Summary report should include archived data: Approximate Space Used (MB)", !worksheet[rowCount, 6].ToString().IsNullOrEmpty());
			});
		}

		void AssertArchiveReportForOPS(ArchiveScheduleTask schedule)
		{
			var archiveScheduleDocuments = TestHelpers.GetAllReportsFromSchedule(schedule);

			AssertEquals("There should be 1 report attached to the archive schedule.", 1, archiveScheduleDocuments.Count());

			var archiveReport = archiveScheduleDocuments.First();

			AssertEquals("OperationalJobsArchiveSystemReport_202202220000", archiveReport.FileNameOnly);

			using var stream = archiveReport.GetImageDataReader();
			using var excel = new ExcelInterface();
			excel.LoadExcelFile(stream);

			var worksheet = excel.WorkSheets[0];
			var rowCount = 0;
			var inputDataColumn = 2;

			CombineAssertions(() =>
			{
				AssertEquals(Core.Constants.FileFormats.XLSX, excel.GetExtensionForExcelFromFile());

				AssertEquals("Operational Jobs Archive System Summary Report", worksheet[rowCount, 1].ToString());

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

				AssertEquals("JobShipments Per Hour", worksheet[++rowCount, 1].ToString());
				AssertEquals("Value for 'JobShipments Per Hour'", "0", worksheet[rowCount, inputDataColumn].ToString());

				AssertEquals("JobHeaders Per Hour", worksheet[++rowCount, 1].ToString());
				AssertEquals("Value for 'JobHeaders Per Hour'", "0", worksheet[rowCount, inputDataColumn].ToString());
				AssertEquals(string.Empty, worksheet[++rowCount, 1].ToString());

				AssertEquals("Registry Settings:", worksheet[++rowCount, 1].ToString());
				AssertEquals("Batch Size", worksheet[++rowCount, 1].ToString());
				AssertEquals("Value for 'Batch Size'", "50", worksheet[rowCount, inputDataColumn + 1].ToString());
				AssertEquals("On Or Before Minimum", worksheet[++rowCount, 1].ToString());
				AssertEquals("Value for 'On Or Before Minimum'", "7", worksheet[rowCount, inputDataColumn + 1].ToString());
				AssertEquals(string.Empty, worksheet[++rowCount, 1].ToString());

				AssertEquals("Archive Schedule Parameters:", worksheet[++rowCount, 1].ToString());
				AssertEquals("Date Parameter", worksheet[++rowCount, 1].ToString());
				AssertEquals("Value for 'Date Parameter'", "Job Open Date", worksheet[rowCount, inputDataColumn + 1].ToString());
				AssertEquals("On Or Before Date", worksheet[++rowCount, 1].ToString());
				AssertEquals("Value for 'On Or Before Date'", "22/02/2022", worksheet[rowCount, inputDataColumn + 1].ToString());
				AssertEquals("Max. Run Duration (Minutes)", worksheet[++rowCount, 1].ToString());
				AssertEquals("Value for 'Max. Run Duration (Minutes)'", "1", worksheet[rowCount, inputDataColumn + 1].ToString());
				AssertEquals(string.Empty, worksheet[++rowCount, 1].ToString());

				AssertEquals("Archived Data Dated Between Earliest Possible Date And 22-Feb-22", worksheet[++rowCount, 1].ToString());
				AssertEquals("Table Name", worksheet[++rowCount, 1].ToString());
				AssertEquals("Deleted Record Count", worksheet[rowCount, 2].ToString());
				AssertEquals("Record Count", worksheet[rowCount, 4].ToString());
				AssertEquals("Approximate Space Used (MB)", worksheet[rowCount, 6].ToString());

				AssertEquals("Summary report should include archived data: DummyBizo", DummyBizoSchema.Constants.TableName, worksheet[++rowCount, 1].ToString());
				AssertEquals("Summary report should include archived data: Deleted Record Count", "10", worksheet[rowCount, 2].ToString());
				AssertEquals("Summary report should include archived data: Record Count", "0", worksheet[rowCount, 4].ToString());
				Assert("Summary report should include archived data: Approximate Space Used (MB)", !worksheet[rowCount, 6].ToString().IsNullOrEmpty());

				AssertEquals("Summary report should include archived data: DummyDependentBizo", DummyDependentBizoSchema.Constants.TableName, worksheet[++rowCount, 1].ToString());
				AssertEquals("Summary report should include archived data: Deleted Record Count", "5", worksheet[rowCount, 2].ToString());
				AssertEquals("Summary report should include archived data: Record Count", "0", worksheet[rowCount, 4].ToString());
				Assert("Summary report should include archived data: Approximate Space Used (MB)", !worksheet[rowCount, 6].ToString().IsNullOrEmpty());
			});
		}
	}
}
