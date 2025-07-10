using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Data.Testing;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business.Schedule;
using Enterprise.Scheduler.Business;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ArchiveManager.Service.Test
{
	[TestedType(typeof(ArchiveManagerServiceTask))]
	public class ArchiveServiceTest : ServiceTaskTestCase<ArchiveManagerServiceTask>
	{
		public void TestWhenNoScheduledArchiveTasksFound()
		{
			var testTask = new ArchiveManagerServiceTask();
			testTask.ServiceLogger = serviceLogger;

			_ = InitialiseTaskSchedule(testTask);
			RunTaskSchedule(testTask);

			int index = 0;
			AssertEquals("Information|0 scheduled tasks to run", serviceLogger[index++]);
		}

		[TestDate(2009, 1, 14)]
		[UseSnapshotProtection]
		public void TestWhenScheduledTasksFound()
		{
			TestConnection.CommitTransaction();

			var dummySimpleTask = Factory.New<ArchiveScheduleTask>();
			dummySimpleTask.IsArchiveRecordsOnOrBeforeDate = false;
			dummySimpleTask.IsArchiveRecordsOnOrBeforeRelativeDate = true;
			dummySimpleTask.ArchiveRecordsOnOrBeforeNumber = 2;
			dummySimpleTask.ArchiveRecordsOnOrBeforeType = "W";

			dummySimpleTask.S5_ScheduleType = "DMS";
			dummySimpleTask.S5_IsActive = true;
			dummySimpleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Today.AddHours(-1);

			var dummyComplexTask = Factory.New<ArchiveScheduleTask>();
			dummyComplexTask.IsArchiveRecordsOnOrBeforeDate = false;
			dummyComplexTask.IsArchiveRecordsOnOrBeforeRelativeDate = true;
			dummyComplexTask.ArchiveRecordsOnOrBeforeNumber = 1;
			dummyComplexTask.ArchiveRecordsOnOrBeforeType = "Y";

			dummyComplexTask.S5_ScheduleType = "DMC";
			dummyComplexTask.S5_IsActive = true;
			dummyComplexTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Today.AddDays(-1);

			var dummyComplexTask2 = Factory.New<ArchiveScheduleTask>();
			dummyComplexTask2.IsArchiveRecordsOnOrBeforeDate = false;
			dummyComplexTask2.IsArchiveRecordsOnOrBeforeRelativeDate = true;
			dummyComplexTask2.ArchiveRecordsOnOrBeforeNumber = 1;
			dummyComplexTask2.ArchiveRecordsOnOrBeforeType = "Y";

			dummyComplexTask2.S5_ScheduleType = "DMC";
			dummyComplexTask2.S5_IsActive = true;
			dummyComplexTask2.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Today.AddDays(1);

			Factory.Save();

			var testTask = new ArchiveManagerServiceTask();
			testTask.ServiceLogger = serviceLogger;

			_ = InitialiseTaskSchedule(testTask);
			RunTaskSchedule(testTask);

			var actual = serviceLogger.ToString();
			AssertContains("Should find 2 Schaduled tasks to run", "Information|2 scheduled tasks to run", actual);

			TestConnection.BeginTransaction();
		}

		[TestDate(2024, 1, 14)]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestRunTask_WhenArchiveSchedulesFound_ThenNudgeACL()
		{
			var dummySimpleTask = Factory.NewWithValidTestData<ArchiveScheduleTask>();
			dummySimpleTask.S5_IsActive = true;
			dummySimpleTask.S5_ScheduleType = "DMS";
			dummySimpleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Today.AddHours(-1);
			dummySimpleTask.IsArchiveRecordsOnOrBeforeDate = false;
			dummySimpleTask.IsArchiveRecordsOnOrBeforeRelativeDate = true;
			dummySimpleTask.ArchiveRecordsOnOrBeforeNumber = 2;
			dummySimpleTask.ArchiveRecordsOnOrBeforeType = "W";

			var dummyComplexTask = Factory.New<ArchiveScheduleTask>();
			dummyComplexTask.S5_IsActive = true;
			dummyComplexTask.S5_ScheduleType = "DMC";
			dummyComplexTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Today.AddDays(-1);
			dummyComplexTask.IsArchiveRecordsOnOrBeforeDate = false;
			dummyComplexTask.IsArchiveRecordsOnOrBeforeRelativeDate = true;
			dummyComplexTask.ArchiveRecordsOnOrBeforeNumber = 1;
			dummyComplexTask.ArchiveRecordsOnOrBeforeType = "Y";

			Factory.Save();

			var archiveManagerServiceTask = new ArchiveManagerServiceTask();
			var testLogger = InitialiseTaskSchedule(archiveManagerServiceTask);

			var mockServiceTaskNudger = new Mock<IServiceTaskNudger>();

			using (ObjectFactory.Substitute(mockServiceTaskNudger.Object))
			{
				RunTaskSchedule(archiveManagerServiceTask);

				AssertContains("When archive schedules have run, then ACL service task should be nudged.", "Nudging archive manager cleanup.", testLogger.ToString());
				mockServiceTaskNudger.Verify(x => x.NudgeServiceTask("ACL", null), Times.Once());
			}
		}

		[TestDate(2024, 1, 14)]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestRunTask_WhenNoArchiveSchedulesFound_ThenDoNotNudgeACL()
		{
			var archiveManagerServiceTask = new ArchiveManagerServiceTask();
			var testLogger = InitialiseTaskSchedule(archiveManagerServiceTask);

			var mockServiceTaskNudger = new Mock<IServiceTaskNudger>();

			using (ObjectFactory.Substitute(mockServiceTaskNudger.Object))
			{
				RunTaskSchedule(archiveManagerServiceTask);

				AssertNotContains("When there are no archive schedules run, then should not nudge ACL service task", "Nudging archive manager cleanup.", testLogger.ToString());
				mockServiceTaskNudger.Verify(x => x.NudgeServiceTask("ACL", null), Times.Never);
			}
		}

		// No bizo-based nudging: no queue table.
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();

		public void TestHasNoQueueProvider()
		{
			var queueProvider = GetHostedServiceQueueProviderInstanceByServiceTaskCode("ARC");
			AssertNull("No queue table for this service task.", queueProvider);
		}

		TestServiceLogger serviceLogger;

		protected override void SetUpCore()
		{
			base.SetUpCore();

			TestCaseHelper.ClearTable(StmScheduleTaskRecipient.Schema.TableName);
			TestCaseHelper.ClearTable(ArchiveScheduleTask.Schema.TableName);
			serviceLogger = new TestServiceLogger();
		}
	}
}
