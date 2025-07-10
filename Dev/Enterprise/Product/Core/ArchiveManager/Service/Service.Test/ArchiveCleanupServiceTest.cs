using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ArchiveManager.Engine;
using Enterprise.ArchiveManager.Engine.Test.TestDoubles;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ArchiveManager.Service.Test
{
	[TestedType(typeof(ArchiveCleanupServiceTask))]
	public class ArchiveCleanupServiceTest : ServiceTaskTestCase<ArchiveCleanupServiceTask>
	{
		[UseSnapshotProtection]
		public void TestCleanupLoadingRecords()
		{
			var sql = "UPDATE dbo.ArchiveMainItemQueue SET AIM_SystemLastEditTimeUtc = DATEADD(day, -10, GETUTCDATE()), AIM_SystemLastEditUser = '~BP', AIM_IsLoading = 1";
			TestCleanupRecordsHelper(sql);
		}

		[UseSnapshotProtection]
		public void TestCleanupLoadedButNotSkippedRecords()
		{
			var sql = "UPDATE dbo.ArchiveMainItemQueue SET AIM_SystemLastEditTimeUtc = DATEADD(day, -10, GETUTCDATE()), AIM_SystemLastEditUser = '~BP', AIM_IsLoading = 0, AIM_IsLoaded = 1";
			TestCleanupRecordsHelper(sql);
		}

		[UseSnapshotProtection]
		public void TestCleanupLoadedAndSkippedRecords()
		{
			var sql = "UPDATE dbo.ArchiveMainItemQueue SET AIM_SystemLastEditTimeUtc = DATEADD(day, -10, GETUTCDATE()), AIM_SystemLastEditUser = '~BP', AIM_IsLoading = 0, AIM_IsLoaded = 1, AIM_IsSkipped = 1";
			TestCleanupRecordsHelper(sql);
		}

		[UseSnapshotProtection]
		public void TestCleanupUntouchedRecordsOlderThan7Days()
		{
			var sql = "UPDATE dbo.ArchiveMainItemQueue SET AIM_SystemLastEditTimeUtc = DATEADD(day, -10, GETUTCDATE()), AIM_SystemLastEditUser = '~BP', AIM_IsLoading = 0, AIM_IsLoaded = 0, AIM_IsSkipped = 0";
			TestCleanupRecordsHelper(sql);
		}

		[UseSnapshotProtection]
		public void TestUntouchedRecordsYoungerThan7DaysShouldNotBeCleanedUp()
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.NewWithValidTestData<DummyBusinessObject>();
			dummyBizo.Z0_Date = ZDateTime.Now.AddYears(-10);
			dummyBizo.Z0_Code = "D1";

			var systemDescriptor = new DummySimpleArchiveSystemDescriptor();
			var config = new ArchiveConfiguration(ZDateTime.Now.AddYears(-7), 1, ZDateTime.Now, isVerboseLog: false, shouldIncludeDeclarations: false);
			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
			var archiveStage = new ArchiveStage(stageDescriptor, systemDescriptor);
			archiveStage.BeginRun(config, schedule, logger);

			factory.Save();

			var result = archiveStage.GetNextArchiveSet(null, schedule, archiveStage);

			AssertEquals("Prerequisite: 1 item is loaded", 1, result.Count());
			AssertEquals("Prerequisite: There is one record in the queue", 1, ArchiveTableHelper.GetNumberOfItemsInMainArchiveQueue());

			var sql = "UPDATE dbo.ArchiveMainItemQueue SET AIM_SystemLastEditTimeUtc = DATEADD(DAY, -6, GETUTCDATE()), AIM_SystemLastEditUser = '~BP', AIM_IsLoading = 0, AIM_IsLoaded = 0, AIM_IsSkipped = 0";
			_ = Db.Connection.ExecuteNonQuery(sql);

			var testTask = new ArchiveCleanupServiceTask();
			testTask.ServiceLogger = serviceLogger;

			testTask.RunTask();

			AssertEquals("The item in ArchiveMainItemQueue is not deleted", 1, ArchiveTableHelper.GetNumberOfItemsInMainArchiveQueue());
		}

		public void TestCleanupRecordsHelper(string setStatusOfQueueRowsSql)
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.NewWithValidTestData<DummyBusinessObject>();
			dummyBizo.Z0_Date = ZDateTime.Now.AddYears(-10);
			dummyBizo.Z0_Code = "D1";

			var systemDescriptor = new DummySimpleArchiveSystemDescriptor();
			var config = new ArchiveConfiguration(ZDateTime.Now.AddYears(-7), 1, ZDateTime.Now, isVerboseLog: false, shouldIncludeDeclarations: false);
			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
			var archiveStage = new ArchiveStage(stageDescriptor, systemDescriptor);
			archiveStage.BeginRun(config, schedule, logger);

			factory.Save();

			var result = archiveStage.GetNextArchiveSet(null, schedule, archiveStage);

			AssertEquals("Prerequisite: 1 item is loaded", 1, result.Count());
			AssertEquals("Prerequisite: There is one record in the queue", 1, ArchiveTableHelper.GetNumberOfItemsInMainArchiveQueue());

			_ = Db.Connection.ExecuteNonQuery(setStatusOfQueueRowsSql);

			var testTask = new ArchiveCleanupServiceTask();
			testTask.ServiceLogger = serviceLogger;

			testTask.RunTask();

			AssertEquals("No records remain in ArchiveMainItemQueue",
				0, ArchiveTableHelper.GetNumberOfItemsInMainArchiveQueue());

			CombineAssertions("Logs should be correct", () =>
			{
				AssertContains("Information|Started Archive Manager Cleanup", serviceLogger.ToString());
				AssertContains("Information|Deleted 1 rows from ArchiveMainItemQueue.", serviceLogger.ToString());
				AssertContains("Information|There are no more rows to delete.", serviceLogger.ToString());
				AssertContains("Information|Completed successfully.", serviceLogger.ToString());
			});
		}

		[UseSnapshotProtection]
		public void TestDoNotCleanupRecordsThatAreTooNew()
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.NewWithValidTestData<DummyBusinessObject>();
			dummyBizo.Z0_Date = ZDateTime.Now.AddYears(-10);
			dummyBizo.Z0_Code = "D1";

			var systemDescriptor = new DummySimpleArchiveSystemDescriptor();
			var config = new ArchiveConfiguration(ZDateTime.Now.AddYears(-7), 1, ZDateTime.Now, isVerboseLog: false, shouldIncludeDeclarations: false);
			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
			var archiveStage = new ArchiveStage(stageDescriptor, systemDescriptor);
			archiveStage.BeginRun(config, schedule, logger);

			factory.Save();

			_ = archiveStage.GetNextArchiveSet(null, schedule, archiveStage);

			var testTask = new ArchiveCleanupServiceTask();
			testTask.ServiceLogger = serviceLogger;

			testTask.RunTask();

			AssertEquals("New record isn't cleaned up", 1, ArchiveTableHelper.GetNumberOfItemsInMainArchiveQueue());

			CombineAssertions("Logs should be correct", () =>
			{
				AssertContains("Information|Started Archive Manager Cleanup", serviceLogger.ToString());
				AssertContains("Information|There are no rows to delete from ArchiveMainItemQueue.", serviceLogger.ToString());
				AssertContains("Information|Completed successfully.", serviceLogger.ToString());
			});
		}

		[UseSnapshotProtection]
		public void TestCleanupOldRecordsWithError()
		{
			var testTask = new ArchiveCleanupServiceTaskThatThrowsExceptionForTest();
			testTask.ServiceLogger = serviceLogger;

			testTask.RunTask();

			Assert("No exception should be thrown, the error should be caught and the run should stop",
				serviceLogger.ToString().Contains("will re-run later."));
			AssertContains("Error is reported", "A non-fatal sql exception", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();

		TestServiceLogger serviceLogger;

		protected override void SetUpCore()
		{
			base.SetUpCore();

			serviceLogger = new TestServiceLogger();
		}

		class ArchiveCleanupServiceTaskThatThrowsExceptionForTest : ArchiveCleanupServiceTask
		{
			protected override int CleanupBatchOfRecords()
			{
				throw SqlExceptionBuilder.CreateSqlException(1222, "Lock request time out period exceeded.");
			}
		}
	}
}
