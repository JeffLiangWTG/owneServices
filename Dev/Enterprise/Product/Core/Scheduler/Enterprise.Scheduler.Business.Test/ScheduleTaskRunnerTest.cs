using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.Database.ExtendedProperties;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Scheduler.Business.Testing
{
	[CargoWise.Data.Testing.UseSnapshotProtection(true)]
	sealed class ScheduleTaskRunnerTest : TestCaseWithFactory
	{
		#region StmScheduleTasks Tasks

		public void TestBusinessObjectFactoryOfStmReportRunShouldBelongToCurrentThread()
		{
			_ = Factory.NewWithValidTestData<DummyStmScheduleTask>();
			Factory.Save();
			var scheduleTaskRunner = new ScheduleTaskRunnerForTestFactoryOfStmReportRun();
			scheduleTaskRunner.Process(DummyBizoSchema.Constants.Prefix, true, Notifications, new CancellationToken());
			AssertNullOrEmpty(ErrorReporter.LastMessageReported);
		}

		[TestDate(2005, 1, 4)]
		public void TestRunStmScheduleTasks()
		{
			Assert("Precondition: must be a weekday", ZDateTime.Now.DayOfWeek != DayOfWeek.Saturday && ZDateTime.Now.DayOfWeek != DayOfWeek.Sunday);

			DummyStmScheduleTask scheduleTask1 = Factory.NewWithValidTestData<DummyStmScheduleTask>();
			DummyStmScheduleTask scheduleTask2 = Factory.NewWithValidTestData<DummyStmScheduleTask>();
			Factory.Save();
			ScheduleTaskRunner.Process(Notifications);
			scheduleTask1.Reload();
			scheduleTask2.Reload();

			AssertEquals("ScheduleTask1 run", "0=True $4=False $5=False $1=False $2=False $3=False $7=False $6=1 $8=0 $", scheduleTask1.S5_ScheduleDescription);
			AssertEquals("ScheduleTask2 run", "0=True $4=False $5=False $1=False $2=False $3=False $7=False $6=1 $8=0 $", scheduleTask2.S5_ScheduleDescription);
			AssertMultilineASCIIEquals("",
				@"2 scheduled tasks to run
Running 1 of 2 scheduled tasks: 
Task completed. . Time taken: 0 seconds
Running 2 of 2 scheduled tasks: 
Task completed. . Time taken: 0 seconds",
				Notifications.AsString.Trim());

			Notifications.Clear();
			ScheduleTaskRunner.Process(Notifications);
			AssertContains("0 scheduled tasks to run", Notifications.AsString);
		}

		[TestDate(2005, 1, 4)]
		public void TestStmScheduleTasksAreSortedByIsPrivateThenByNextScheduledPrintRunTime()
		{
			Assert("Precondition: must be a weekday", ZDateTime.Now.DayOfWeek != DayOfWeek.Saturday && ZDateTime.Now.DayOfWeek != DayOfWeek.Sunday);

			var scheduleTask3 = Factory.NewWithValidTestData<DummyStmScheduleTask>();
			scheduleTask3.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(-2);
			scheduleTask3.S5_ScheduleDescription = "Task 3";

			var scheduleTask1 = Factory.NewWithValidTestData<DummyStmScheduleTask>();
			scheduleTask1.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(-4);
			scheduleTask1.S5_ScheduleDescription = "Task 1";

			var scheduleTask4 = Factory.NewWithValidTestData<DummyStmScheduleTask>();
			scheduleTask4.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(-1);
			scheduleTask4.S5_ScheduleDescription = "Task 4";

			var scheduleTask2 = Factory.NewWithValidTestData<DummyStmScheduleTask>();
			scheduleTask2.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(-3);
			scheduleTask2.S5_ScheduleDescription = "Task 2";

			var scheduleTask5 = Factory.NewWithValidTestData<DummyStmScheduleTask>();
			scheduleTask5.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(-4);
			scheduleTask5.S5_IsPrivate = true;
			scheduleTask5.S5_ScheduleDescription = "Task 5";

			Factory.Save();
			ScheduleTaskRunner.Process(Notifications);
			scheduleTask1.Reload();
			scheduleTask2.Reload();
			scheduleTask3.Reload();
			scheduleTask4.Reload();
			scheduleTask5.Reload();

			CombineAssertions(() =>
			{
				AssertEquals("ScheduleTask1 run", "0=True $4=False $5=False $1=False $2=False $3=False $7=False $6=1 $8=0 $", scheduleTask1.S5_ScheduleDescription);
				AssertEquals("ScheduleTask2 run", "0=True $4=False $5=False $1=False $2=False $3=False $7=False $6=1 $8=0 $", scheduleTask2.S5_ScheduleDescription);
				AssertEquals("ScheduleTask3 run", "0=True $4=False $5=False $1=False $2=False $3=False $7=False $6=1 $8=0 $", scheduleTask3.S5_ScheduleDescription);
				AssertEquals("ScheduleTask4 run", "0=True $4=False $5=False $1=False $2=False $3=False $7=False $6=1 $8=0 $", scheduleTask4.S5_ScheduleDescription);
				// scheduleTask5 is deleted after run as it's private.

				AssertMultilineASCIIEquals("",
				@"5 scheduled tasks to run
Running 1 of 5 scheduled tasks: Description: Task 1
Task completed. Description: Task 1. Time taken: 0 seconds
Running 2 of 5 scheduled tasks: Description: Task 2
Task completed. Description: Task 2. Time taken: 0 seconds
Running 3 of 5 scheduled tasks: Description: Task 3
Task completed. Description: Task 3. Time taken: 0 seconds
Running 4 of 5 scheduled tasks: Description: Task 4
Task completed. Description: Task 4. Time taken: 0 seconds
Running 5 of 5 scheduled tasks: Description: Task 5
Task completed. Description: Task 5. Time taken: 0 seconds",
				Notifications.AsString.Trim());
			});
		}

		[TestDate(2005, 1, 4)]
		public void TestStmScheduleTasksAreSortedUsingPriorityAsAFinalTieBreaker()
		{
			Assert("Precondition: must be a weekday", ZDateTime.Now.DayOfWeek != DayOfWeek.Saturday && ZDateTime.Now.DayOfWeek != DayOfWeek.Sunday);

			var scheduleTask3 = Factory.NewWithValidTestData<DummyStmScheduleTask>();
			scheduleTask3.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(-3);
			scheduleTask3.SetPriority(100);

			var scheduleTask1 = Factory.NewWithValidTestData<DummyStmScheduleTask>();
			scheduleTask1.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(-4);
			scheduleTask1.SetPriority(1);

			var scheduleTask4 = Factory.NewWithValidTestData<DummyStmScheduleTask>();
			scheduleTask4.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(-3);
			scheduleTask4.SetPriority(10);

			var scheduleTask2 = Factory.NewWithValidTestData<DummyStmScheduleTask>();
			scheduleTask2.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(-3);
			scheduleTask2.SetPriority(1000);

			var scheduleTask5 = Factory.NewWithValidTestData<DummyStmScheduleTask>();
			scheduleTask5.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(-1);
			scheduleTask5.SetPriority(5000);

			var scheduleTask6 = Factory.NewWithValidTestData<DummyStmScheduleTask>();
			scheduleTask6.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(-4);
			scheduleTask6.S5_IsPrivate = true;
			scheduleTask6.SetPriority(10000);

			Factory.Save();
			ScheduleTaskRunner.Process(Notifications);

			CombineAssertions(() =>
			{
				AssertMultilineASCIIEquals("",
					$@"6 scheduled tasks to run
Running 1 of 6 scheduled tasks: Description: {scheduleTask1.S5_ScheduleDescription}
Task completed. Description: {scheduleTask1.S5_ScheduleDescription}. Time taken: 0 seconds
Running 2 of 6 scheduled tasks: Description: {scheduleTask2.S5_ScheduleDescription}
Task completed. Description: {scheduleTask2.S5_ScheduleDescription}. Time taken: 0 seconds
Running 3 of 6 scheduled tasks: Description: {scheduleTask3.S5_ScheduleDescription}
Task completed. Description: {scheduleTask3.S5_ScheduleDescription}. Time taken: 0 seconds
Running 4 of 6 scheduled tasks: Description: {scheduleTask4.S5_ScheduleDescription}
Task completed. Description: {scheduleTask4.S5_ScheduleDescription}. Time taken: 0 seconds
Running 5 of 6 scheduled tasks: Description: {scheduleTask5.S5_ScheduleDescription}
Task completed. Description: {scheduleTask5.S5_ScheduleDescription}. Time taken: 0 seconds
Running 6 of 6 scheduled tasks: Description: {scheduleTask6.S5_ScheduleDescription}
Task completed. Description: {scheduleTask6.S5_ScheduleDescription}. Time taken: 0 seconds",
					Notifications.AsString.Trim());
			});
		}

		[TestDate(2005, 1, 4)]
		public void TestRunStmScheduleTasksWithReports()
		{
			Assert("Precondition: must be a weekday", ZDateTime.Now.DayOfWeek != DayOfWeek.Saturday && ZDateTime.Now.DayOfWeek != DayOfWeek.Sunday);

			DummyStmScheduleTask scheduleTask1 = CreateDummyScheduleTask();
			DummyStmScheduleTask scheduleTask2 = CreateDummyScheduleTask();
			scheduleTask1.DontRun = true;
			scheduleTask1.MarkAsRunning = true;
			scheduleTask2.DontRun = true;
			scheduleTask2.S5_ScheduleDescription = "Who Framed Roger Rabbit";
			StmMenuItem item1 = Factory.LoadTop1<StmMenuItem>(new ZQuery());
			StmMenuItem item2 = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.PK, SQLComparisonOperator.NotEqual, item1.PK));
			AssertNotNull(item1);
			AssertNotNull(item2);
			scheduleTask1.S5_ParentID = item1.PK;
			scheduleTask1.S5_ParentTableCode = "SU";
			scheduleTask2.S5_ParentID = item2.PK;
			scheduleTask2.S5_ParentTableCode = "SU";

			Factory.Save();

			ScheduleTaskRunner.UseDummyTasks = true;
			ScheduleTaskRunner.DummyTasks.Add(scheduleTask1);
			ScheduleTaskRunner.DummyTasks.Add(scheduleTask2);
			ScheduleTaskRunner.Process(Notifications);

			AssertMultilineASCIIEquals("",
				@"2 scheduled tasks to run
Running 1 of 2 scheduled tasks: Report name: " + item1.SU_MenuName + @"
Task completed. Report name: " + item1.SU_MenuName + @". Time taken: 0 seconds
Running 2 of 2 scheduled tasks: Report name: " + item2.SU_MenuName + @", Description: Who Framed Roger Rabbit
Task completed. Report name: " + item2.SU_MenuName + @", Description: Who Framed Roger Rabbit. Time taken: 0 seconds",
				Notifications.AsString.Trim());
		}

		[TestDate(2005, 1, 4)]
		public void TestStmReportRunRetryLimit()
		{
			Assert("Precondition: must be a weekday", ZDateTime.Now.DayOfWeek != DayOfWeek.Saturday && ZDateTime.Now.DayOfWeek != DayOfWeek.Sunday);

			SystemDataRegistry.Instance.SRRMaximumRetryCount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 3);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "SRR";

			var scheduleTask1 = CreateDummyScheduleTask();
			var scheduleTask2 = CreateDummyScheduleTask();
			scheduleTask1.NotifyScheduleRunError = true;
			scheduleTask2.DontRun = true;
			scheduleTask2.S5_ScheduleDescription = "Who Framed Roger Rabbit";
			var item1 = Factory.LoadTop1<StmMenuItem>(new ZQuery());
			var item2 = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.PK, SQLComparisonOperator.NotEqual, item1.PK));
			AssertNotNull(item1);
			AssertNotNull(item2);
			scheduleTask1.S5_ParentID = item1.PK;
			scheduleTask1.S5_ParentTableCode = "SU";
			scheduleTask1.S5_GS_NKPrintUser = "SRR";
			scheduleTask2.S5_ParentID = item2.PK;
			scheduleTask2.S5_ParentTableCode = "SU";
			scheduleTask2.S5_GS_NKPrintUser = "SRR";

			Factory.Save();
			scheduleTask1.S5_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-3);
			scheduleTask1.S5_SystemLastEditTimeUtc = ZDateTime.UtcNow.AddDays(-2);

			scheduleTaskRunner = new ScheduleTaskRunnerForTest();
			scheduleTaskRunner.UseStmReportRun = true;
			ScheduleTaskRunner.UseDummyTasks = true;
			ScheduleTaskRunner.DummyTasks.Add(scheduleTask1);
			ScheduleTaskRunner.DummyTasks.Add(scheduleTask2);
			void SetUpAndProcess(bool setActive = false)
			{
				TestDateAttribute.AddMinutes(5);
				var anotherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var scheduleTask1InAnotherFactory = anotherFactory.Load<StmScheduleTask>(scheduleTask1.PK);
				var scheduleTask2InAnotherFactory = anotherFactory.Load<StmScheduleTask>(scheduleTask2.PK);
				scheduleTask1InAnotherFactory.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddDays(-1);
				scheduleTask2InAnotherFactory.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddDays(-1);
				if (setActive)
				{
					scheduleTask1InAnotherFactory.S5_IsActive = true;
					anotherFactory.Save();
					scheduleTask1.Reload();
				}
				anotherFactory.Save();
				ScheduleTaskRunner.Process(Notifications);
				AssertEquals(true, scheduleTask2.S5_IsActive);
				AssertEquals(false, scheduleTask1.HasChanges);
				scheduleTask1InAnotherFactory.Reload();
				AssertEquals(scheduleTask1.S5_IsActive, scheduleTask1InAnotherFactory.S5_IsActive);
			}
			AssertEquals(true, scheduleTask1.S5_IsActive);

			SetUpAndProcess();
			AssertEquals(true, scheduleTask1.S5_IsActive);

			SetUpAndProcess();
			AssertEquals(true, scheduleTask1.S5_IsActive);

			scheduleTask1.NotifyScheduleRunError = false;
			SetUpAndProcess();
			AssertEquals(true, scheduleTask1.S5_IsActive);

			scheduleTask1.NotifyScheduleRunError = true;
			SetUpAndProcess();
			AssertEquals(true, scheduleTask1.S5_IsActive);

			SetUpAndProcess();
			AssertEquals(true, scheduleTask1.S5_IsActive);

			SetUpAndProcess();
			AssertEquals(true, scheduleTask1.S5_IsActive);

			SetUpAndProcess();
			AssertEquals(false, scheduleTask1.S5_IsActive);
			AssertContains($"Task aborted and scheduled report {scheduleTask1.DescriptionForLog} is de-activated as it has failed more than 3 times, more than the failure threshold value set in the registry: System -> Reports -> Scheduled Report Failure Threshold. Please check the reasons for failure before enabling the scheduled report again.",
				Notifications.AsString);
			SetUpAndProcess();
			AssertEquals(false, scheduleTask1.S5_IsActive);

			SetUpAndProcess(true);
			AssertEquals(true, scheduleTask1.S5_IsActive);
		}

		[TestDate(2005, 1, 4)]
		public void TestSRRTimeout()
		{
			using (new DisposableAction(() => AsyncHelper.WaitAllActiveTasksForTest()))
			{
				Assert("Precondition: must be a weekday", ZDateTime.Now.DayOfWeek != DayOfWeek.Saturday && ZDateTime.Now.DayOfWeek != DayOfWeek.Sunday);

				SystemDataRegistry.Instance.SRRMaximumTimeElapsed.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);

				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_Code = "SRR";

				DummyStmScheduleTask scheduleTask1 = CreateDummyScheduleTask();
				scheduleTask1.SimulateTimeout = true;
				StmMenuItem item1 = Factory.LoadTop1<StmMenuItem>(new ZQuery());
				scheduleTask1.S5_ParentID = item1.PK;
				scheduleTask1.S5_ParentTableCode = "SU";
				scheduleTask1.S5_GS_NKPrintUser = "SRR";

				Factory.Save();
				scheduleTask1.S5_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-3);
				scheduleTask1.S5_SystemLastEditTimeUtc = ZDateTime.UtcNow.AddDays(-2);

				scheduleTaskRunner = new ScheduleTaskRunnerForTest();
				scheduleTaskRunner.UseStmReportRun = true;
				ScheduleTaskRunner.UseDummyTasks = true;
				ScheduleTaskRunner.DummyTasks.Add(scheduleTask1);
				ScheduleTaskRunner.Process(Notifications);

				var stmReportRun1 = Factory.LoadTop1<StmReportRun>(new ZQuery(StmReportRunSchema.RRI_S5_Schedule, scheduleTask1.PK));
				AssertEquals(item1.SU_MenuName, stmReportRun1.RRI_ReportName);
				AssertEquals(StmReportRunState.Error, stmReportRun1.RRI_Status);
				AssertEquals("0=False $4=False $5=False $1=False $2=False $3=False $7=True $6=0 $8=0 $", stmReportRun1.RRI_ReportDescription);
				AssertEquals(ZDateTime.UtcNow.AddDays(-1), stmReportRun1.RRI_StartTimeInQueueUtc);
				AssertEquals(ZDateTime.UtcNow, stmReportRun1.RRI_EndTimeInQueueUtc);
				AssertEquals(ZDateTime.UtcNow, stmReportRun1.RRI_StartTimeUtc);
				AssertEquals(ZDateTime.UtcNow, stmReportRun1.RRI_EndTimeUtc);
				AssertEquals("SRR", stmReportRun1.RRI_GS_NKPrintUser);
				AssertEquals(System.Environment.MachineName, stmReportRun1.RRI_RunningServer);
				AssertEquals(3, stmReportRun1.Notes.ClientVisibleNotes.Length);
				AssertContainsExactElementsInAnyOrder(new string[] { "Error", "Exception", "Information" },
					new string[]
					{
						stmReportRun1.Notes.ClientVisibleNotes[0].ST_Description,
						stmReportRun1.Notes.ClientVisibleNotes[1].ST_Description,
						stmReportRun1.Notes.ClientVisibleNotes[2].ST_Description
					});
				AssertContainsExactElementsInAnyOrder(
					new string[]
					{
						"System.InvalidOperationException: Task aborted; Report name: Combined Invoice and Certif. of Origin, Description: 0=False $4=False $5=False $1=False $2=False $3=False $7=True $6=0 $8=0 $ took longer than 1 seconds, more than the timeout value set in the registry: System -> Reports -> Scheduled Report Timeout",
						"Task aborted; Report name: Combined Invoice and Certif. of Origin, Description: 0=False $4=False $5=False $1=False $2=False $3=False $7=True $6=0 $8=0 $ took longer than 1 seconds, more than the timeout value set in the registry: System -> Reports -> Scheduled Report Timeout",
						"Scheduled task is canceled successfully. Report name: Combined Invoice and Certif. of Origin, Description: 0=False $4=False $5=False $1=False $2=False $3=False $7=True $6=0 $8=0 $"
					},
					new string[]
					{
						stmReportRun1.Notes.ClientVisibleNotes[0].ST_NoteDataAsText,
						stmReportRun1.Notes.ClientVisibleNotes[1].ST_NoteDataAsText,
						stmReportRun1.Notes.ClientVisibleNotes[2].ST_NoteDataAsText
					});
			}
		}

		[TestDate(2005, 1, 4)]
		public void TestStmReportRunPart1()
		{
			Assert("Precondition: must be a weekday", ZDateTime.Now.DayOfWeek != DayOfWeek.Saturday && ZDateTime.Now.DayOfWeek != DayOfWeek.Sunday);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "SRR";

			DummyStmScheduleTask scheduleTask1 = CreateDummyScheduleTask();
			DummyStmScheduleTask scheduleTask2 = CreateDummyScheduleTask();
			scheduleTask1.NotifyScheduleRunError = true;
			scheduleTask2.DontRun = true;
			scheduleTask2.S5_ScheduleDescription = "Who Framed Roger Rabbit";
			StmMenuItem item1 = Factory.LoadTop1<StmMenuItem>(new ZQuery());
			item1.SU_IsSystemDefined = false;
			StmMenuItem item2 = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.PK, SQLComparisonOperator.NotEqual, item1.PK));
			AssertNotNull(item1);
			AssertNotNull(item2);
			scheduleTask1.S5_ParentID = item1.PK;
			scheduleTask1.S5_ParentTableCode = "SU";
			scheduleTask1.S5_GS_NKPrintUser = "SRR";
			scheduleTask2.S5_ParentID = item2.PK;
			scheduleTask2.S5_ParentTableCode = "SU";
			scheduleTask2.S5_GS_NKPrintUser = "SRR";

			Factory.Save();
			scheduleTask1.S5_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-3);
			scheduleTask1.S5_SystemLastEditTimeUtc = ZDateTime.UtcNow.AddDays(-2);

			scheduleTaskRunner = new ScheduleTaskRunnerForTest();
			scheduleTaskRunner.UseStmReportRun = true;
			ScheduleTaskRunner.UseDummyTasks = true;
			ScheduleTaskRunner.DummyTasks.Add(scheduleTask1);
			ScheduleTaskRunner.DummyTasks.Add(scheduleTask2);
			ScheduleTaskRunner.Process(Notifications);

			/*Task completed. Report name: test, Description: 0=False $4=True $5=False $1=False $2=False $3=False $6=0 $8=0 $. Time taken: 0 seconds
Running 2 of 2 schedule tasks: Report name: Non-ABI3461 Entry/Immediate Delivery, Description: Who Framed Roger Rabbit
Task completed. Report name: Non-ABI3461 Entry/Immediate Delivery, Description: Who Framed Roger Rabbit. Time taken: 0 seconds

*/

			AssertMultilineASCIIEquals("",
				@"2 scheduled tasks to run
Running 1 of 2 scheduled tasks: Report name: " + item1.SU_MenuName + @", Description: 0=False $4=True $5=False $1=False $2=False $3=False $7=False $6=0 $8=0 $
An error occurred while processing the task
Error notification email could not be sent. Please make sure that the group specified in the Notification > Company Notification Group Registry setting has a member with an email address on their Staff profile, or the last editing user has an email address.
Task completed. Report name: " + item1.SU_MenuName + @", Description: 0=False $4=True $5=False $1=False $2=False $3=False $7=False $6=0 $8=0 $. Time taken: 0 seconds
Running 2 of 2 scheduled tasks: Report name: " + item2.SU_MenuName + @", Description: Who Framed Roger Rabbit
Task completed. Report name: " + item2.SU_MenuName + @", Description: Who Framed Roger Rabbit. Time taken: 0 seconds",
				Notifications.AsString.Trim());

			var stmReportRun1 = Factory.LoadTop1<StmReportRun>(new ZQuery(StmReportRunSchema.RRI_S5_Schedule, scheduleTask1.PK));
			AssertEquals(item1.SU_MenuName, stmReportRun1.RRI_ReportName);
			AssertEquals(false, stmReportRun1.RRI_IsSystemDefined);
			AssertNotEquals(null, stmReportRun1.RRI_SystemCreateTimeUtc);
			AssertLessThan(0, stmReportRun1.RRI_SystemCreateUser.Length);
			AssertEquals(StmReportRunState.Error, stmReportRun1.RRI_Status);
			AssertEquals("0=False $4=True $5=False $1=False $2=False $3=False $7=False $6=0 $8=0 $", stmReportRun1.RRI_ReportDescription);
			AssertEquals(ZDateTime.UtcNow.AddDays(-1), stmReportRun1.RRI_StartTimeInQueueUtc);
			AssertEquals(ZDateTime.UtcNow, stmReportRun1.RRI_EndTimeInQueueUtc);
			AssertEquals(ZDateTime.UtcNow, stmReportRun1.RRI_StartTimeUtc);
			AssertEquals(ZDateTime.UtcNow, stmReportRun1.RRI_EndTimeUtc);
			AssertEquals("SRR", stmReportRun1.RRI_GS_NKPrintUser);
			AssertEquals(System.Environment.MachineName, stmReportRun1.RRI_RunningServer);
			AssertEquals(2, stmReportRun1.Notes.ClientVisibleNotes.Length);
			AssertEquals("Error", stmReportRun1.Notes.ClientVisibleNotes[0].ST_Description);
			AssertEquals("Error", stmReportRun1.Notes.ClientVisibleNotes[1].ST_Description);
			AssertContainsExactElementsInAnyOrder(new string[] {
						"Error notification email could not be sent. Please make sure that the group specified in the Notification > Company Notification Group Registry setting has a member with an email address on their Staff profile, or the last editing user has an email address.",
						"An error occurred while processing the task" },
				new string[] { stmReportRun1.Notes.ClientVisibleNotes[0].ST_NoteDataAsText, stmReportRun1.Notes.ClientVisibleNotes[1].ST_NoteDataAsText });

			var stmReportRun2 = Factory.LoadTop1<StmReportRun>(new ZQuery(StmReportRunSchema.RRI_S5_Schedule, scheduleTask2.PK));
			AssertEquals(item2.SU_MenuName, stmReportRun2.RRI_ReportName);
			AssertEquals(StmReportRunState.Finished, stmReportRun2.RRI_Status);
			AssertEquals("Who Framed Roger Rabbit", stmReportRun2.RRI_ReportDescription);
			AssertEquals(ZDateTime.UtcNow, stmReportRun2.RRI_StartTimeInQueueUtc);
			AssertEquals(ZDateTime.UtcNow, stmReportRun2.RRI_EndTimeInQueueUtc);
			AssertEquals(ZDateTime.UtcNow, stmReportRun2.RRI_StartTimeUtc);
			AssertEquals(ZDateTime.UtcNow, stmReportRun2.RRI_EndTimeUtc);
			AssertEquals("SRR", stmReportRun2.RRI_GS_NKPrintUser);
			AssertEquals(System.Environment.MachineName, stmReportRun2.RRI_RunningServer);
			AssertEquals(1, stmReportRun2.Notes.ClientVisibleNotes.Length);
			AssertEquals("Dummy Note", stmReportRun2.Notes.ClientVisibleNotes[0].ST_Description);
			AssertNullOrEmpty("no cross thread exception when adding a note to StmReportRun", ErrorReporter.LastMessageReported);
		}

		[TestDate(2005, 1, 4)]
		[ExpectNoExceptions]
		public void TestRunStmScheduleTasksWithReportsWithNullMenuItem()
		{
			Assert("Precondition: must be a weekday", ZDateTime.Now.DayOfWeek != DayOfWeek.Saturday && ZDateTime.Now.DayOfWeek != DayOfWeek.Sunday);

			DummyStmScheduleTask scheduleTask1 = CreateDummyScheduleTask();
			scheduleTask1.DontRun = true;
			StmMenuItem item1 = Factory.NewWithValidTestData<StmMenuItem>();
			AssertNotNull(item1);
			scheduleTask1.S5_ParentID = item1.PK;
			scheduleTask1.S5_ParentTableCode = "SU";
			scheduleTask1.S5_ScheduleDescription = "test blow up";

			Factory.Save();

			item1.Delete();
			Factory.Save();

			ScheduleTaskRunner.UseDummyTasks = true;
			ScheduleTaskRunner.DummyTasks.Add(scheduleTask1);
			ScheduleTaskRunner.Process(Notifications);

			AssertMultilineASCIIEquals("", @"1 scheduled tasks to run
Running 1 of 1 scheduled tasks: Description: test blow up
Task completed. Description: test blow up. Time taken: 0 seconds", Notifications.AsString);
		}

		[ExpectNoExceptions]
		public void TestSqlLockLostDuringTaskRun()
		{
			var dummyTask = Factory.NewWithValidTestData<DummyTask>();
			dummyTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(-1);
			dummyTask.TimesToThrow = 3;

			ScheduleTaskRunner.UseDummyTasks = true;
			ScheduleTaskRunner.DummyTasks.Add(dummyTask);

			Factory.Save();
			ScheduleTaskRunner.Process(Notifications);
		}

		[TestDate(2005, 1, 4)]
		[ExpectNoExceptions]
		public void TestScheduleTaskProvidedCancellationTokenLogsCancellationInfo()
		{
			// Arrange
			var scheduleTask = Factory.NewWithValidTestData<EverlastingStmScheduleTask>();
			scheduleTask.S5_ScheduleDescription = "{98A015F8-6726-47C5-8FF9-A6E5107825AF}";

			Factory.Save();
			ScheduleTaskRunner.UseDummyTasks = true;
			ScheduleTaskRunner.DummyTasks.Add(scheduleTask);
			ScheduleTaskRunner.UseStmReportRun = true;

			var cancellationTokenSource = new CancellationTokenSource();

			var notificationsMock = new Mock<INotifications>();
			notificationsMock
				.Setup(note => note.Add(It.Is<INotification>(notification =>
					notification.Message.Contains(scheduleTask.S5_ScheduleDescription)
					&& notification.Message.IndexOf("received a cancellation request from Runner", StringComparison.OrdinalIgnoreCase) >= 0)))
				.Callback(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						ExtProperty.Database.Update(Db.Connection, $"{scheduleTask.S5_ScheduleDescription}_TimeToFinish", "yes");
					}
				});

			var task = Task.Run(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					while (!string.Equals(ExtProperty.Database.Select(Db.Connection, $"{scheduleTask.S5_ScheduleDescription}_Started"), "yes", StringComparison.OrdinalIgnoreCase))
					{
						cancellationTokenSource.Token.ThrowIfCancellationRequested();
						Task.Delay(TimeSpan.FromSeconds(1), cancellationTokenSource.Token);
					}
				}
				cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(5));
			}, cancellationTokenSource.Token);

			// Act
			ScheduleTaskRunner.Process(notificationsMock.Object, cancellationTokenSource.Token);

			// Assert
			notificationsMock
				.Verify(note => note.Add(It.Is<INotification>(notification =>
						notification.Message.Contains(scheduleTask.S5_ScheduleDescription)
						&& notification.Message.IndexOf("received a cancellation request from Runner", StringComparison.OrdinalIgnoreCase) >= 0)),
					Times.Once);
		}

		[TestDate(2005, 1, 4)]
		[ExpectNoExceptions]
		public void TestScheduleTaskProvidedCancellationTokenAllowsTaskToFinish()
		{
			// Arrange
			var scheduleTask = Factory.NewWithValidTestData<EverlastingStmScheduleTask>();
			scheduleTask.S5_ScheduleDescription = "{0424B604-C66E-435F-B0EF-3424CA726255}";

			Factory.Save();
			ScheduleTaskRunner.UseDummyTasks = true;
			ScheduleTaskRunner.DummyTasks.Add(scheduleTask);
			ScheduleTaskRunner.UseStmReportRun = true;

			var cancellationTokenSource = new CancellationTokenSource();

			var notificationsMock = new Mock<INotifications>();
			notificationsMock
				.Setup(note => note.Add(It.Is<INotification>(notification =>
						notification.Message.Contains(scheduleTask.S5_ScheduleDescription)
						&& notification.Message.IndexOf("received a cancellation request from Runner", StringComparison.OrdinalIgnoreCase) >= 0)))
				.Callback(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						ExtProperty.Database.Update(Db.Connection, $"{scheduleTask.S5_ScheduleDescription}_TimeToFinish", "yes");
					}
				});

			var task = Task.Run(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					while (!string.Equals(ExtProperty.Database.Select(Db.Connection, $"{scheduleTask.S5_ScheduleDescription}_Started"), "yes", StringComparison.OrdinalIgnoreCase))
					{
						cancellationTokenSource.Token.ThrowIfCancellationRequested();
						Task.Delay(TimeSpan.FromSeconds(1), cancellationTokenSource.Token);
					}
				}
				cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(5));
			}, cancellationTokenSource.Token);

			// Act
			ScheduleTaskRunner.Process(notificationsMock.Object, cancellationTokenSource.Token);

			// Assert
			try
			{
				task.Wait();
			}
			catch (AggregateException ae)
			{
				ae.Handle(ex => ex is TaskCanceledException);
			}
			notificationsMock
				.Verify(note => note.Add(It.Is<INotification>(notification =>
						notification.Message.Contains(scheduleTask.S5_ScheduleDescription)
						&& notification.Message.IndexOf("Task completed", StringComparison.InvariantCultureIgnoreCase) >= 0)),
					Times.Once);
		}

		class EverlastingStmScheduleTask : StmScheduleTask
		{
			public EverlastingStmScheduleTask(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override void RunCore(INotifications notifications, CancellationToken token)
			{
				base.RunCore(notifications, token);
				ExtProperty.Database.Update(Db.Connection, $"{S5_ScheduleDescription}_Started", "yes");
				var stopwatch = Stopwatch.StartNew();
				while (
					!string.Equals(ExtProperty.Database.Select(Db.Connection, $"{S5_ScheduleDescription}_TimeToFinish"), "yes", StringComparison.OrdinalIgnoreCase)
					&& stopwatch.Elapsed <= TimeSpan.FromMinutes(1))
				{
					Thread.Sleep(TimeSpan.FromSeconds(5));
				}

				Thread.Sleep(TimeSpan.FromSeconds(10));
			}
		}

		public void TestSqlLockRecoveredDuringTaskRun()
		{
			var dummyTask = Factory.NewWithValidTestData<DummyTask>();
			dummyTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(-1);
			dummyTask.TimesToThrow = 2;

			ScheduleTaskRunner.UseDummyTasks = true;
			ScheduleTaskRunner.DummyTasks.Add(dummyTask);

			Factory.Save();
			ScheduleTaskRunner.Process(Notifications);

			string result = Notifications.AsString.Replace("The connection to the database has been reset. Rerunning 1 of 1 scheduled tasks: \r\n", "");

			AssertMultilineASCIIEquals("", @"1 scheduled tasks to run
Running 1 of 1 scheduled tasks: 
Task completed. . Time taken: 0 seconds", result);
		}

		public void TestSqlLockFailDuringTaskRun()
		{
			var dummyTask = Factory.NewWithValidTestData<DummyTask>();
			dummyTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(-1);

			ScheduleTaskRunner.UseDummyTasks = true;
			ScheduleTaskRunner.DummyTasks.Add(dummyTask);

			Factory.Save();

			using (var anotherDbConnection = Db.NewExtraConnectionToMainDb())
			{
				SqlApplicationLock mutex;
				Assert(anotherDbConnection.TryGetLock("ScheduleTaskRunner:" + dummyTask.PK.ToString(), out mutex));
				using (mutex)
				{
					ScheduleTaskRunner.Process(Notifications);
				}
			}

			AssertMultilineASCIIEquals("", @"1 scheduled tasks to run
Task 1 of 1 is already being run.", Notifications.AsString);
		}

		internal class DummyTask : DummyStmScheduleTask
		{
			public DummyTask(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override void RunSafe(INotifications notifications, CancellationToken token)
			{
				if (TimesToThrow > 0)
				{
					TimesToThrow--;
					throw new SqlLockLostException("boom", new[] { "mutexKey" });
				}
			}

			public int TimesToThrow { get; set; }
		}

		[TestDate(2005, 1, 4)]
		public void TestRunStmScheduleTasksReportRunErrors()
		{
			SetUpPostMasterGroup();

			Assert("Precondition: must be a weekday", ZDateTime.Now.DayOfWeek != DayOfWeek.Saturday && ZDateTime.Now.DayOfWeek != DayOfWeek.Sunday);

			DummyStmScheduleTask scheduleTask1 = CreateDummyScheduleTask();
			StmMenuItem item1 = Factory.LoadTop1<StmMenuItem>(new ZQuery());
			AssertNotNull(item1);
			scheduleTask1.S5_ParentID = item1.PK;
			scheduleTask1.S5_ParentTableCode = "SU";
			scheduleTask1.NotifyScheduleRunError = true;
			Factory.Save();

			ScheduleTaskRunner.UseDummyTasks = true;
			ScheduleTaskRunner.DummyTasks.Add(scheduleTask1);
			ScheduleTaskRunner.Process(Notifications);

			AssertMultilineASCIIEquals("",
				@"1 scheduled tasks to run
Running 1 of 1 scheduled tasks: Report name: " + item1.SU_MenuName + @", Description: 0=False $4=True $5=False $1=False $2=False $3=False $7=False $6=0 $8=0 $
An error occurred while processing the task
Error notification email was sent to the following email address.
staff@group.com

Task completed. Report name: " + item1.SU_MenuName + @", Description: 0=False $4=True $5=False $1=False $2=False $3=False $7=False $6=0 $8=0 $. Time taken: 0 seconds
",
				Notifications.AsString.Trim());
		}

		[TestDate(2005, 1, 4)]
		public void TestRunStmScheduleTasksReportRunNonRecurrent()
		{
			GlbBranch branch1 = Factory.NewWithValidTestData<GlbBranch>();
			GlbDepartment department1 = Factory.NewWithValidTestData<GlbDepartment>();

			SetUpGlbStaffForStmScheduleTasksReport();
			using (EnvProxy.Instance.SetTemporaryUserContext("UserWithEmail", branch1.PK.ToGuid(), department1.PK.ToGuid()))
			{
				DummyStmScheduleTask scheduleTask1 = CreateDummyScheduleTask();
				StmMenuItem item2 = Factory.LoadTop1<StmMenuItem>(new ZQuery());
				item2.SU_IsSystemDefined = false;
				AssertNotNull(item2);
				scheduleTask1.S5_ParentID = item2.PK;
				scheduleTask1.S5_ParentTableCode = "SUC";
				scheduleTask1.S5_IsPrivate = false;
				scheduleTask1.ThrowExceptionInvalidOp = true;
				Factory.Save();

				AssertEquals(0, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);

				ScheduleTaskRunner.UseDummyTasks = true;
				ScheduleTaskRunner.DummyTasks.Add(scheduleTask1);
				AssertNoExceptionThrown(() => ScheduleTaskRunner.Process(Notifications));
				scheduleTask1.Reload();

				AssertEquals(false, scheduleTask1.S5_IsActive);
				AssertEquals(1, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		#region Test Send Notification Email

		public void TestRunStmScheduleTasksWithSendErrorNotificationEmailToDefaultStaffUser()
		{
			var newStaff = Factory.NewWithValidTestData<GlbStaff>();
			newStaff.GS_Code = "CSF";
			newStaff.GS_EmailAddress = "currentStaff@user.com";
			newStaff.Factory.Save();

			using (Env.SetTemporaryUserContext(newStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var scheduleTask = CreateDummyScheduleTaskForSendErrorNotification();
				ScheduleTaskRunner.UseDummyTasks = true;
				ScheduleTaskRunner.DummyTasks.Add(scheduleTask);
				ScheduleTaskRunner.Process(Notifications);

				var expectedNotificationUserMessage = @"Error notification email was sent to the following email address.
currentStaff@user.com";
				AssertContains(expectedNotificationUserMessage, Notifications.AsString);
				AssertErrorNotificationEmail("currentStaff@user.com");
			}
		}

		public void TestRunStmScheduleTasksWithSendErrorNotificationEmailToGroupRole()
		{
			SetUpNotificationGroup(true);

			var scheduleTask = CreateDummyScheduleTaskForSendErrorNotification();
			ScheduleTaskRunner.UseDummyTasks = true;
			ScheduleTaskRunner.DummyTasks.Add(scheduleTask);
			ScheduleTaskRunner.Process(Notifications);

			var expectedNotificationUserMessage = @"Error notification email was sent to the following email address.
groupStaff@user.com";
			AssertContains(expectedNotificationUserMessage, Notifications.AsString);
			AssertErrorNotificationEmail("groupStaff@user.com");
		}

		public void TestRunStmScheduleTasksWithSendErrorNotificationEmailToStaffRole()
		{
			var scheduleTask = CreateDummyScheduleTaskForSendErrorNotification();
			SetUpNotificationStaffRole("JTR", scheduleTask);

			ScheduleTaskRunner.UseDummyTasks = true;
			ScheduleTaskRunner.DummyTasks.Add(scheduleTask);
			ScheduleTaskRunner.Process(Notifications);

			var expectedNotificationUserMessage = @"Error notification email was sent to the following email address.
roleStaff@user.com";
			AssertContains(expectedNotificationUserMessage, Notifications.AsString);
			AssertErrorNotificationEmail("roleStaff@user.com");
		}

		public void TestRunStmScheduleTasksWithSendErrorNotificationEmailToDefaultStaffUserIfNotificationGroupHasNoStaffUser()
		{
			var newStaff = Factory.NewWithValidTestData<GlbStaff>();
			newStaff.GS_Code = "CSF";
			newStaff.GS_EmailAddress = "currentStaff@user.com";
			newStaff.Factory.Save();

			using (Env.SetTemporaryUserContext(newStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				SetUpNotificationGroup(false);

				var scheduleTask = CreateDummyScheduleTaskForSendErrorNotification();
				ScheduleTaskRunner.UseDummyTasks = true;
				ScheduleTaskRunner.DummyTasks.Add(scheduleTask);
				ScheduleTaskRunner.Process(Notifications);

				var expectedNotificationUserMessage = @"Error notification email was sent to the following email address.
currentStaff@user.com";
				AssertContains(expectedNotificationUserMessage, Notifications.AsString);
				AssertErrorNotificationEmail("currentStaff@user.com");
			}
		}

		public void TestRunStmScheduleTasksWithSendErrorNotificationEmailToGroupRoleIfNotificationStaffRoleHasNoStaffUser()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();

			var staff = group.Staff.AddNew();
			staff.GS_EmailAddress = "groupStaff@user.com";
			staff.GS_LoginName = "JerryTest";
			staff.GS_Code = "JYT";
			SystemDataRegistry.Instance.SRRErrorNotificationGroups.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			var scheduleTask = CreateDummyScheduleTaskForSendErrorNotification();
			SetUpNotificationStaffRole("SAL", scheduleTask);
			ScheduleTaskRunner.UseDummyTasks = true;
			ScheduleTaskRunner.DummyTasks.Add(scheduleTask);
			ScheduleTaskRunner.Process(Notifications);

			var expectedNotificationUserMessage = @"Error notification email was sent to the following email address.
groupStaff@user.com";
			AssertContains(expectedNotificationUserMessage, Notifications.AsString);
			AssertErrorNotificationEmail("groupStaff@user.com");
		}

		void SetUpNotificationGroup(bool shouldAddUserToGroup)
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();

			if (shouldAddUserToGroup)
			{
				var staff = group.Staff.AddNew();
				staff.GS_EmailAddress = "groupStaff@user.com";
				staff.GS_LoginName = "JerryTest";
				staff.GS_Code = "JYT";
			}
			Factory.Save();

			SystemDataRegistry.Instance.SRRErrorNotificationOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Enterprise.Core.Constants.ErrorNotificationOptions.Code.GRP);
			SystemDataRegistry.Instance.SRRErrorNotificationGroups.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
		}

		void SetUpNotificationStaffRole(string roleCode, DummyStmScheduleTask scheduleTask)
		{
			var staffRoles = new CodeDescriptionBoolDisallowNewCollection
			{
				new CodeDescriptionBoolDisallowNew { Bool = true, Code = "JTR" },
			};

			SystemDataRegistry.Instance.SRRErrorNotificationStaffRoles.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, staffRoles);
			SystemDataRegistry.Instance.SRRErrorNotificationOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Enterprise.Core.Constants.ErrorNotificationOptions.Code.ROL);

			var newStaff = Factory.NewWithValidTestData<GlbStaff>();
			newStaff.GS_Code = "RSF";
			newStaff.GS_EmailAddress = "roleStaff@user.com";

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var contact = orgHeader.Contacts.AddNew();

			var staffAssignments = orgHeader.StaffAssignments.AddNew();
			staffAssignments.O8_Role = roleCode;
			staffAssignments.O8_GS_NKPersonResponsible = newStaff.GS_Code;

			scheduleTask.Recipients.RemoveAndDeleteAll();
			var recipient = scheduleTask.Recipients.AddNew();
			recipient.S6_OH = orgHeader.PK;
			recipient.S6_OC = contact.PK;
			Factory.Save();
		}

		DummyStmScheduleTask CreateDummyScheduleTaskForSendErrorNotification()
		{
			var scheduleTask = CreateDummyScheduleTask();
			scheduleTask.S5_IsPrivate = true;
			scheduleTask.ThrowExceptionInvalidOp = true;
			Factory.Save();
			return scheduleTask;
		}

		void AssertErrorNotificationEmail(string exceptedEmailAddress)
		{
			AssertEquals("One error email should be created.", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			var createdEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Email.Recipients.Count", 1, createdEmail.Recipients.Count);
			AssertEquals("Email.Recipients[0]", exceptedEmailAddress, createdEmail.Recipients[0]);
			AssertContains("One-off report has failed to run: ", createdEmail.Subject);
			AssertContains("It has been deactivated. Ensure that the parameters used for the report are valid and do not create too much data, then activate it again.", createdEmail.Body);
		}

		#endregion

		public void TestIsFinished()
		{
			var scheduleTask = CreateDummyScheduleTask();
			ScheduleTaskRunner.UseDummyTasks = true;
			ScheduleTaskRunner.DummyTasks.Add(scheduleTask);
			ScheduleTaskRunner.UseStmReportRun = true;
			scheduleTask.S5_IsActive = false;
			scheduleTask.Factory.Save();
			ScheduleTaskRunner.UseStmReportRun = true;
			ScheduleTaskRunner.Process(Notifications);
			AssertMultilineASCIIEquals("",
				@"1 scheduled tasks to run
Task 1 of 1 is already being run.",
				Notifications.AsString);
		}

		void SetUpGlbStaffForStmScheduleTasksReport()
		{
			SetUpPostMasterGroup();

			Assert("Precondition: must be a weekday", ZDateTime.Now.DayOfWeek != DayOfWeek.Saturday && ZDateTime.Now.DayOfWeek != DayOfWeek.Sunday);

			GlbStaff userWithEmail = Factory.NewWithValidTestData<GlbStaff>();
			userWithEmail.GS_Code = "UWE";
			userWithEmail.GS_LoginName = "UserWithEmail";
			userWithEmail.GS_EmailAddress = "a@b.com";

			Factory.Save();
		}

		[TestDate(2005, 1, 4)]
		public void TestRunStmScheduleTasksReportRunExceptionsOnAdhocReports()
		{
			GlbBranch branch1 = Factory.NewWithValidTestData<GlbBranch>();
			GlbDepartment department1 = Factory.NewWithValidTestData<GlbDepartment>();

			SetUpGlbStaffForStmScheduleTasksReport();

			using (EnvProxy.Instance.SetTemporaryUserContext("UserWithEmail", branch1.PK.ToGuid(), department1.PK.ToGuid()))
			{
				Assert(!string.IsNullOrWhiteSpace(EnvProxy.Instance.CurrentUser.EmailAddress));

				DummyStmScheduleTask scheduleTask1 = CreateDummyScheduleTask();
				StmMenuItem item1 = Factory.LoadTop1<StmMenuItem>(new ZQuery());
				AssertNotNull(item1);
				item1.SU_IsSystemDefined = false;
				scheduleTask1.S5_ParentID = item1.PK;
				scheduleTask1.S5_ParentTableCode = "SU";
				scheduleTask1.S5_IsPrivate = true;
				scheduleTask1.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2005, 1, 3);
				scheduleTask1.ThrowExceptionInvalidOp = true;
				Factory.Save();

				AssertEquals(0, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);

				ScheduleTaskRunner.UseDummyTasks = true;
				ScheduleTaskRunner.DummyTasks.Add(scheduleTask1);
				AssertNoExceptionThrown(() => ScheduleTaskRunner.Process(Notifications));
				scheduleTask1.Reload();

				AssertEquals(false, scheduleTask1.S5_IsActive);
				AssertEquals(1, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		[TestDate(2005, 1, 4)]
		public void TestRunStmScheduleTasksReportRunWarnings()
		{
			SetUpPostMasterGroup();

			Assert("Precondition: must be a weekday", ZDateTime.Now.DayOfWeek != DayOfWeek.Saturday && ZDateTime.Now.DayOfWeek != DayOfWeek.Sunday);

			DummyStmScheduleTask scheduleTask1 = CreateDummyScheduleTask();
			StmMenuItem item1 = Factory.LoadTop1<StmMenuItem>(new ZQuery());
			AssertNotNull(item1);
			scheduleTask1.S5_ParentID = item1.PK;
			scheduleTask1.S5_ParentTableCode = "SU";
			scheduleTask1.NotifyScheduleRunWarning = true;
			Factory.Save();

			ScheduleTaskRunner.UseDummyTasks = true;
			ScheduleTaskRunner.DummyTasks.Add(scheduleTask1);
			ScheduleTaskRunner.Process(Notifications);

			Assert(Notifications.HasWarnings);
			Assert(!Notifications.HasErrors);
			AssertEquals("",
				@"1 scheduled tasks to run
Running 1 of 1 scheduled tasks: Report name: " + item1.SU_MenuName + @", Description: 0=False $4=False $5=True $1=False $2=False $3=False $7=False $6=0 $8=0 $
Here is a warning
Task completed. Report name: " + item1.SU_MenuName + @", Description: 0=False $4=False $5=True $1=False $2=False $3=False $7=False $6=0 $8=0 $. Time taken: 0 seconds",
				Notifications.AsString.Trim());
		}

		[TestDate(2018, 3, 19)]
		public void TestCancelRunningScheduleReport()
		{
			var dummyTask = Factory.NewWithValidTestData<DummyTaskRunningLongTime>();
			dummyTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(-1);
			dummyTask.S5_ScheduleType = "D";
			Factory.Save();

			ScheduleTaskRunner.UseDummyTasks = true;
			ScheduleTaskRunner.UseStmReportRun = true;
			ScheduleTaskRunner.DummyTasks.Add(dummyTask);
			ScheduleTaskRunner.Process(Notifications);

			AssertEquals("Before being cancelled, StmReportRun was Running", StmReportRunState.Running, dummyTask.InitialReportRun.RRI_Status);

			var lastStmReportRun = Factory.LoadTop1<StmReportRun>(new ZQuery(StmReportRunSchema.RRI_S5_Schedule, dummyTask.PK));
			AssertEquals("latest value in database: StmReportRun is Cancelled", StmReportRunState.Cancelled, lastStmReportRun.RRI_Status);

			var notes = lastStmReportRun.Notes.FindByVisibility(StmNoteVisibility.PUB).Select(x => x.ST_NoteDataAsText).ToArray();
			var expectedLogs = new[] { "Canceling scheduled task. ", "Scheduled task is canceled successfully. " };
			AssertContainsExactElementsInAnyOrder(expectedLogs, notes);

			AssertEquals("2018-03-19", dummyTask.S5_NextScheduledPrintRunTimeUtc.ToString("yyyy-MM-dd"));
			dummyTask.Reload();
			AssertEquals("2018-03-19", dummyTask.S5_NextScheduledPrintRunTimeUtc.ToString("yyyy-MM-dd"));
			AsyncHelper.WaitAllActiveTasksForTest();
		}

		public void TestRunStmScheduleTasksWithTaskNullAfterReloadWithNewFactory()
		{
			var scheduleTask = CreateDummyScheduleTask();
			scheduleTask.S5_ScheduleDescription = "Test Reloaded Schedule Task";
			scheduleTask.ThrowExceptionInvalidOp = true;
			Factory.Save();

			ScheduleTaskRunner.UseDummyTasks = true;
			ScheduleTaskRunner.DummyTasks.Add(scheduleTask);

			AssertNoExceptionThrown(() => ScheduleTaskRunner.Process(Notifications));
		}

		internal class DummyTaskRunningLongTime : DummyStmScheduleTask
		{
			public DummyTaskRunningLongTime(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override void RunSafe(INotifications notifications, CancellationToken token)
			{
				CancelRunning?.Invoke(this, EventArgs.Empty);
				Thread.Sleep(3000);
			}

			public StmReportRun InitialReportRun { get; set; }

			public event EventHandler CancelRunning;
		}

		internal class DummyTaskThrowsOperationCanceledException : DummyStmScheduleTask
		{
			public DummyTaskThrowsOperationCanceledException(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override void RunSafe(INotifications notifications, CancellationToken token)
			{
				base.RunSafe(notifications, token);
				throw new OperationCanceledException();
			}
		}

		public void TestThreadOwnershipIsRelinquishedOnOperationCanceledException()
		{
			var dummyTask = Factory.NewWithValidTestData<DummyTaskThrowsOperationCanceledException>();
			dummyTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(-1);
			dummyTask.S5_ScheduleType = "D";
			Factory.Save();

			ScheduleTaskRunner.UseDummyTasks = true;
			ScheduleTaskRunner.UseStmReportRun = true;
			ScheduleTaskRunner.DummyTasks.Add(dummyTask);
			ScheduleTaskRunner.Process(Notifications);

			AssertNull(ErrorReporter.LastExceptionReported);
		}

		#endregion

		#region Implementation

		void SetUpPostMasterGroup()
		{
			GlbGroup postMastersGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			GlbStaff staff = postMastersGroup.Staff.AddNew();
			staff.GS_EmailAddress = "staff@group.com";
			staff.GS_Code = "ZAC";
			Factory.Save();
		}

		DummyStmScheduleTask CreateDummyScheduleTask()
		{
			var dummyStmScheduleTask = Factory.NewWithValidTestData<DummyStmScheduleTask>();
			dummyStmScheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(-1);

			return dummyStmScheduleTask;
		}

		IDisposable runInPrimaryAppDomainForTestResetter;

		[Serializable]
		class ScheduleTaskRunnerForTest : ScheduleTaskRunner
		{
			public void Process(
				INotifications notifications,
				CancellationToken token
						= new CancellationToken()
					)
			{
				Process(DummyBizoSchema.Constants.Prefix, UseStmReportRun, notifications, token);
			}

			public List<StmScheduleTask> DummyTasks = new List<StmScheduleTask>();

			protected override StmScheduleTaskCollection LoadTasksToRun(BusinessObjectFactory factory, ZString parentTableCode)
			{
				if (!UseDummyTasks)
				{
					return base.LoadTasksToRun(factory, parentTableCode);
				}

				StmScheduleTaskCollection result = new StmScheduleTaskCollection(factory, DummyBizoSchema.Constants.Prefix);
				foreach (var task in DummyTasks)
				{
					result.Add(task);
				}
				return result;
			}

			protected override StmScheduleTask GetReloadedTaskFromNewFactory(Type type, ZGuid pk)
			{
				if (type == typeof(DummyReloadedStmScheduleTask))
				{
					return null;
				}
				return base.GetReloadedTaskFromNewFactory(type, pk);
			}

			void PopulateNonPersistentPropertiesBetweenDummyScheduleTasks(StmScheduleTask sourceTask, StmScheduleTask destinationTask)
			{
				var sourceDummyTask = sourceTask as DummyStmScheduleTask;
				var destinationDummyTask = destinationTask as DummyStmScheduleTask;

				var sourceDummyTask2 = sourceDummyTask as DummyTask;
				var destinationDummyTask2 = destinationDummyTask as DummyTask;
				if (sourceDummyTask != null && destinationDummyTask != null)
				{
					destinationDummyTask.DontRun = sourceDummyTask.DontRun;
					destinationDummyTask.MarkAsRunning = sourceDummyTask.MarkAsRunning;
					if (sourceDummyTask2 != null && destinationDummyTask2 != null)
					{
						destinationDummyTask2.TimesToThrow = sourceDummyTask2.TimesToThrow;
					}
				}

				var sourceDummyTask3 = sourceDummyTask as DummyTaskRunningLongTime;
				var destinationDummyTask3 = destinationDummyTask as DummyTaskRunningLongTime;
				if (sourceDummyTask3 != null && destinationDummyTask3 != null)
				{
					destinationDummyTask3.CancelRunning += (sender, args) =>
					{
						sourceDummyTask3.InitialReportRun = destinationDummyTask3.Factory.LoadTop1<StmReportRun>(new ZQuery(StmReportRunSchema.RRI_S5_Schedule, destinationDummyTask3.PK));
						sourceDummyTask3.StmReportRun.RRI_Status = StmReportRunState.Cancelled;
						//so the behaviour doesn't change in the case of the two factories being on the same thread - the whole point of InitialReportRun is the 'Initial' part, as in, before, unchanged
						destinationDummyTask3.Factory.RefreshEnabled = false;
						sourceDummyTask3.Factory.RefreshEnabled = false;
						sourceDummyTask3.Factory.Save();
					};
				}
			}

			protected override Action<StmScheduleTask, StmScheduleTask> betweenScheduleTasksTestAction => PopulateNonPersistentPropertiesBetweenDummyScheduleTasks;

			public bool UseStmReportRun { get; set; }

			public bool UseDummyTasks { get; set; }
		}

		[Serializable]
		class ScheduleTaskRunnerForTestFactoryOfStmReportRun : ScheduleTaskRunner
		{
			void TestAction(StmScheduleTask sourceTask, StmScheduleTask destinationTask)
			{
				var task = sourceTask.Factory.IsOwnedByCurrentThread ? sourceTask : destinationTask;
				task.StmReportRun.Factory.Load<GlbStaff>(Guid.NewGuid());
			}

			protected override Action<StmScheduleTask, StmScheduleTask> betweenScheduleTasksTestAction => TestAction;
		}

		ScheduleTaskRunnerForTest ScheduleTaskRunner => scheduleTaskRunner ?? (scheduleTaskRunner = new ScheduleTaskRunnerForTest());
		ScheduleTaskRunnerForTest scheduleTaskRunner;

		NotificationBuffer Notifications => notifications ?? (notifications = new NotificationBuffer());
		NotificationBuffer notifications;

		protected override void SetUp()
		{
			base.SetUp();
			runInPrimaryAppDomainForTestResetter = BackgroundAppDomainWorkerForTest.RunInPrimaryAppDomainForTest();
			PropertyChangeSubscriptionList.SubscribedProperties.Add(DummyBizoSchema.Z0_VarCharMax.Name);
			PropertyChangeSubscriptionList.SubscribedProperties.Add(DummyBizoSchema.Z0_Description.Name);
			ObjectFactory.Substitute<IStopwatch>(() => new TestDateStopwatch());
		}

		protected override void TearDown()
		{
			base.TearDown();
			runInPrimaryAppDomainForTestResetter.Dispose();
			// Clear the cross-thread exception as a result of mainConnectionUsageHolder
			if (ErrorReporter.TotalErrorCount == 1 && ErrorReporter.LastExceptionReported is CargoWise.Async.CrossThreadAccessException)
			{
				ErrorReporter.Clear();
			}
		}

		PropertyChangeSubscriptionListForTest PropertyChangeSubscriptionList
		{
			get { return PropertyChangeSubscriptionListForTest.GetInstance(Factory); }
		}

		#endregion
	}
}
