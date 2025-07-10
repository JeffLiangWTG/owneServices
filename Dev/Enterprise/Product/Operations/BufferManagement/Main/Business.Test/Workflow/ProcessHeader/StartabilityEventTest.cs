using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	public class StartabilityEventTest : TestCaseWithFactory
	{
		public void TestStartabilityEvent_GivenPaveIsDisabled_WhenSaving_ThenShouldNotLogStartabilityEvent()
		{
			BMSTestHelper.DisableBMSInRegistry();
			AssertEquals("GIVEN BufferManagement is disabled", false, BMSRegistryProvider.IsBufferManagementEnabled);

			var task1 = BMSTestHelper.CreateTask(workflow, sequence: 1, description: "task1");
			Factory.Save();

			AssertArrayEqualsByElements(System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task1));
		}

		#region Template

		[TestDate(2017, 1, 2)]
		[TestDateIncremental(0, 0, 0, 1)]
		public void TestGivenTemplate_WhenSaving_ShouldNotLogStartability()
		{
			BMSTestHelper.MakeCompletionStatementTaskType("ORG", "COM");
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");

			var templateWorkflow1 = (ProcessHeader)template.ProcessHeaders.AddNew();
			templateWorkflow1.FH_CompletionStatement = "templateWorkflow1";

			var templateTask1 = template.WorkflowItems.Tasks.AddNew();
			templateTask1.P9_FH_ProcessHeader = templateWorkflow1.PK;
			templateTask1.P9_Description = "templateTask1";
			var templateTask2 = template.WorkflowItems.Tasks.AddNew();
			templateTask2.P9_FH_ProcessHeader = templateWorkflow1.PK;
			templateTask2.P9_Description = "templateTask2";

			Factory.Save();

			AssertArrayEqualsByElements(System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(templateTask1));
			AssertArrayEqualsByElements(System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(templateTask2));
		}

		#endregion

		#region ProcessTasks in same workflow

		[TestDate(2017, 1, 2)]
		[TestDateIncremental(0, 0, 0, 1)]
		public void TestGivenStartableTask_WhenClosedAndReopen_ThenStartabilityEventShouldBeRaisedTwice()
		{
			var workflow1 = BMSTestHelper.CreateWorkflow(Factory, completionStatement: "workflow1");
			var task1 = BMSTestHelper.CreateTask(workflow1, description: "task1");
			Factory.Save();
			AssertArrayEqualsByElements(new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task1));

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			AssertArrayEqualsByElements(new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task1));

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			Factory.Save();
			AssertArrayEqualsByElements(new[] { "|SRT=Y", "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task1));
		}

		[TestDate(2017, 1, 2)]
		[TestDateIncremental(0, 0, 0, 1)]
		public void TestStatusAndSequenceUpdate_SRTEvent()
		{
			var workflow1 = BMSTestHelper.CreateWorkflow(Factory, completionStatement: "workflow");

			foreach (var testCase in taskStatusAndSequenceSRTEventTestCases)
			{
				var currentTask = BMSTestHelper.CreateTask(workflow1, description: "currentTask", taskStatus: testCase.GivenTask.Status, sequence: testCase.GivenTask.Sequence);
				var otherTask = BMSTestHelper.CreateTask(workflow1, description: "otherTask", taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, sequence: testCase.GivenTask.Sequence == lowerTaskSequence ? higherTaskSequence : lowerTaskSequence);

				Factory.Save();

				currentTask.P9_Status = testCase.WhenUpdateTaskTo.Status;
				currentTask.P9_Sequence = testCase.WhenUpdateTaskTo.Sequence == lowerTaskSequence
					? updatedLowerTaskSequence
					: updatedHigherTaskSequence;

				Factory.Save();

				var assertMessage = $@"{testCase.Message}: GIVEN task with status='{testCase.GivenTask.Status}' and sequence='{testCase.GivenTask.Sequence}'
GIVEN other-task with status='{otherTask.P9_Status}' and sequence='{otherTask.P9_Sequence}'
WHEN updating task to status='{currentTask.P9_Status}' and sequence='{currentTask.P9_Sequence}'
THEN task should log '{testCase.ThenTaskLog}' and other-task should log '{testCase.ThenOtherTaskLog}'";

				CombineAssertions(assertMessage, () =>
				{
					AssertTaskLog("currentTask", currentTask, testCase.ThenTaskLog);
					AssertTaskLog("otherTask", otherTask, testCase.ThenOtherTaskLog);
				});

				currentTask.Delete();
				otherTask.Delete();
			}
		}

		void AssertTaskLog(string message, ProcessTask task, ExpectedTaskLog expectedTaskLog)
		{
			switch (expectedTaskLog)
			{
				case ExpectedTaskLog.Startable:
					AssertEquals(message, "|SRT=Y", task.Logs?.MostRecentLogByEventTime(Events.StartabilityChanged)?.SL_Reference);
					break;
				case ExpectedTaskLog.NonStartable:
					AssertEquals(message, "|SRT=N", task.Logs?.MostRecentLogByEventTime(Events.StartabilityChanged)?.SL_Reference);
					break;
			}
		}

		[TestDate(2021, 1, 2)]
		[TestDateIncremental(0, 0, 0, 1)]
		public void TestGivenStartableTask_ShouldNotDuplicateStartabilityEvent()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, completionStatement: "workflow");
			var staff = BMSTestHelper.CreateStaff(Factory, "STF");
			var preTask = BMSTestHelper.CreateTask(workflow, staff.GS_Code, description: "PRE ", sequence: 5);
			var startableTask = BMSTestHelper.CreateTask(workflow, staff.GS_Code, description: "STARTABLE", sequence: 10);

			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false, NameForDebugging = "New Factory2" };

			var preTaskFromNewFactory = newFactory.Load<ProcessTask>(preTask.PK);
			var startableTasksFromNewFactory = newFactory.Load<ProcessTask>(startableTask.PK);

			var lastLog = startableTasksFromNewFactory.Logs.MostRecentLogByEventTime(Events.StartabilityChanged);
			AssertNull("Task is not startable yet, log should be null", lastLog);

			Db.Connection.ExecuteNonQuery($@"INSERT INTO [dbo].[StmALog]
(SL_PK, SL_Table, SL_Parent, SL_IsEstimate, SL_IsCancelled, SL_Reference, SL_PostedTimeUtc, SL_EventTime, SL_GS_NKUser, SL_SE_NKEvent, SL_GB_NKBranch, SL_GE_NKDepartment, SL_FireWorkflow)
VALUES (newid(),'ProcessTasks','{startableTask.PK.ToGuid()}','N','N','|SRT=Y|I WAS HERE BEFORE',GETUTCDATE(),GETDATE(),'E','SRT','{Env.CurrentBranch.Code}','{Env.CurrentDepartment.Code}',0)");

			preTaskFromNewFactory.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			newFactory.Save();

			var filter = new ZQuery(StmALogSchema.SL_Parent, startableTasksFromNewFactory.PK);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.StartabilityChanged.Code);
			filter.AddToFilter(StmALogSchema.SL_IsCancelled, "N");
			filter.FetchOnlyFromLocalCache = false;

			var logs = Factory.Load<StmALog>(filter);

			AssertEquals("Should have only have 1 log", 1, logs.Length);
			AssertContains("Log should be the one that was there before", "|SRT=Y|I WAS HERE BEFORE", logs.First().SL_Reference);
		}

		#region Updating ProcessTask sequence

		[TestDate(2017, 1, 2)]
		[TestDateIncremental(0, 0, 0, 1)]
		public void TestLowerTask_UpdateToSameSequence()
		{
			AssertLowerTask_UpdateSequence(updatedSequence: 2);
			AssertArrayEqualsByElements("THEN task1 should not changed", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task1));
		}

		[TestDate(2017, 1, 2)]
		[TestDateIncremental(0, 0, 0, 1)]
		public void TestLowerTask_UpdateToHigherSequence()
		{
			AssertLowerTask_UpdateSequence(updatedSequence: 100);
			AssertArrayEqualsByElements("THEN task1 should be not startable", new[] { "|SRT=Y", "|SRT=N" }, GetSRTLogReferenceOrderByEventTime(task1));
		}

		void AssertLowerTask_UpdateSequence(int updatedSequence)
		{
			var task2 = BMSTestHelper.CreateTask(workflow, description: "task2", sequence: 2);

			Factory.Save();

			Assert("GIVEN task1 has lower sequence than task2", task1.P9_Sequence < task2.P9_Sequence);
			AssertArrayEqualsByElements("GIVEN task1 is startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task1));
			AssertArrayEqualsByElements("GIVEN task2 is not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task2));

			task1.P9_Sequence = updatedSequence;
			Factory.Save();
			Assert("WHEN updating task1 to higher/equal sequence", task1.P9_Sequence >= task2.P9_Sequence);

			AssertArrayEqualsByElements("THEN task2 should be startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task2));
		}

		[TestDate(2017, 1, 2)]
		[TestDateIncremental(seconds: 1)]
		public void TestHigherTask_UpdatedToLowerSequence()
		{
			foreach (var openStatus in ProcessTasks.GetCurrentTaskStatuses())
			{
				var currentWorkflow = BMSTestHelper.CreateWorkflowAndTask(Factory, completionStatement: "currentWorkflow", sequence: 1, description: "currentTask1");
				var currentTask1 = currentWorkflow.Tasks.First();
				var currentTask2 = BMSTestHelper.CreateTask(currentWorkflow, sequence: 2, description: "currentTask2", taskStatus: openStatus);

				Factory.Save();

				Assert("GIVEN currentTask1 sequence is lower than currentTask2", currentTask1.P9_Sequence < currentTask2.P9_Sequence);
				AssertArrayEqualsByElements("GIVEN currentTask1 is startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(currentTask1));
				AssertArrayEqualsByElements("GIVEN currentTask2 is not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(currentTask2));

				currentTask2.P9_Sequence = 0;
				Factory.Save();
				Assert("GIVEN higher currentTask2 is updated with lower-sequence", currentTask2.P9_Sequence < currentTask1.P9_Sequence);

				AssertArrayEqualsByElements("THEN currentTask1 should not startable", new[] { "|SRT=Y", "|SRT=N" }, GetSRTLogReferenceOrderByEventTime(currentTask1));
				AssertArrayEqualsByElements("THEN currentTask2 should be startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(currentTask2));
			}
		}

		public void TestHigherTasks_BecomeLower()
		{
			var task2 = BMSTestHelper.CreateTask(workflow, sequence: 1, description: "task2");
			var task3 = BMSTestHelper.CreateTask(workflow, sequence: 3, description: "task3");
			var task4 = BMSTestHelper.CreateTask(workflow, sequence: 4, description: "task4");

			Factory.Save();

			AssertArrayEqualsByElements("GIVEN task1 is startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task1));
			AssertArrayEqualsByElements("GIVEN task2 is startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task2));
			AssertArrayEqualsByElements("GIVEN task3 is not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task3));
			AssertArrayEqualsByElements("GIVEN task4 is not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task4));

			task3.P9_Sequence = 0;
			task4.P9_Sequence = 0;
			Factory.Save();

			Assert("WHEN task3 becomes lowest task", task3.P9_Sequence < task1.P9_Sequence && task3.P9_Sequence < task2.P9_Sequence);
			Assert("WHEN task4 becomes lowest task", task3.P9_Sequence < task1.P9_Sequence && task3.P9_Sequence < task2.P9_Sequence);

			AssertArrayEqualsByElements("THEN task1 should not startable", new[] { "|SRT=Y", "|SRT=N" }, GetSRTLogReferenceOrderByEventTime(task1));
			AssertArrayEqualsByElements("THEN task2 should not startable", new[] { "|SRT=Y", "|SRT=N" }, GetSRTLogReferenceOrderByEventTime(task2));
			AssertArrayEqualsByElements("THEN task3 should startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task3));
			AssertArrayEqualsByElements("THEN task4 should startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task4));
		}

		#endregion

		#region Updating ProcessTask status

		[TestDate(2017, 1, 2)]
		[TestDateIncremental(seconds: 1)]
		public void TestClosedLowerTask_BecomesStartable()
		{
			foreach (var oldClosedStatus in ProcessTasks.IsClosedCodes.Where(s => !s.IsEmpty))
			{
				foreach (var newOpenStatus in ProcessTasks.GetCurrentTaskStatuses())
				{
					var currentWorkflow = BMSTestHelper.CreateWorkflowAndTask(Factory, completionStatement: "currentWorkflow", sequence: 1, description: "currentTask1", taskStatus: oldClosedStatus);
					var currentTask1 = currentWorkflow.Tasks.First();
					var currentTask2 = BMSTestHelper.CreateTask(currentWorkflow, description: "currentTask2", sequence: 2);

					Factory.Save();
					Assert("GIVEN lower task1 is closed", currentTask1.P9_Status.In(ProcessTasks.IsClosedCodes));
					AssertArrayEqualsByElements("GIVEN task2 is startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(currentTask2));

					currentTask1.P9_Status = newOpenStatus;
					Factory.Save();
					Assert("WHEN closed task1 is reopen", currentTask1.P9_Status.ToString().In(ProcessTasks.GetOpenTaskStatuses()));

					AssertArrayEqualsByElements("THEN task1 should be startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(currentTask1));
					AssertArrayEqualsByElements("THEN task2 should not startable", new[] { "|SRT=Y", "|SRT=N" }, GetSRTLogReferenceOrderByEventTime(currentTask2));
				}
			}
		}

		[TestDate(2017, 1, 2)]
		[TestDateIncremental(seconds: 1)]
		public void TestLowerTasksAreClosed()
		{
			var task2 = BMSTestHelper.CreateTask(workflow, sequence: 2, description: "task2");
			var task3 = BMSTestHelper.CreateTask(workflow, sequence: 3, description: "task3");

			Factory.Save();

			AssertArrayEqualsByElements("GIVEN task1 is startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task1));
			AssertArrayEqualsByElements("GIVEN task2 is not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task2));
			AssertArrayEqualsByElements("GIVEN task3 is not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task3));

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			AssertEquals("WHEN task1 is closed", ProcessTaskStatusCodeList.Codes.Closed, task1.P9_Status);
			AssertEquals("WHEN task2 is closed", ProcessTaskStatusCodeList.Codes.Closed, task2.P9_Status);

			AssertArrayEqualsByElements("THEN task1 SRT log should be unchanged", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task1));
			AssertArrayEqualsByElements("THEN task2 SRT log should be unchanged", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task2));
			AssertArrayEqualsByElements("THEN task3 should startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task3));
		}

		[TestDate(2017, 1, 2)]
		[TestDateIncremental(seconds: 1)]
		public void TestHigherTasksBecomeStartableOrNonStartable()
		{
			var task2 = BMSTestHelper.CreateTask(workflow, sequence: 10, description: "task2");
			var task3 = BMSTestHelper.CreateTask(workflow, sequence: 10, description: "task3");

			Factory.Save();

			AssertArrayEqualsByElements("GIVEN task1 is startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task1));
			AssertArrayEqualsByElements("GIVEN task2 is not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task2));
			AssertArrayEqualsByElements("GIVEN task3 is not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task3));

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			AssertEquals("WHEN lower task1 is closed", ProcessTaskStatusCodeList.Codes.Closed, task1.P9_Status);

			AssertArrayEqualsByElements("THEN task1 SRT log should unchanged", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task1));
			AssertArrayEqualsByElements("THEN task2 should startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task2));
			AssertArrayEqualsByElements("THEN task3 should startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task3));

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			Factory.Save();
			AssertEquals("WHEN lower task1 is re-opened", ProcessTaskStatusCodeList.Codes.Assigned, task1.P9_Status);

			AssertArrayEqualsByElements("THEN task1 SRT log should unchanged", new[] { "|SRT=Y", "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task1));
			AssertArrayEqualsByElements("THEN task2 should not startable", new[] { "|SRT=Y", "|SRT=N" }, GetSRTLogReferenceOrderByEventTime(task2));
			AssertArrayEqualsByElements("THEN task3 should not startable", new[] { "|SRT=Y", "|SRT=N" }, GetSRTLogReferenceOrderByEventTime(task3));
		}

		#endregion

		[TestDate(2017, 1, 2)]
		[TestDateIncremental(seconds: 1)]
		public void TestNewLowerSequenceTask()
		{
			Factory.Save();

			AssertArrayEqualsByElements("GIVEN task1 is startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task1));

			var task0 = BMSTestHelper.CreateTask(workflow, sequence: 0, description: "task0");
			Factory.Save();

			Assert("GIVEN new lower-sequence-task", task0.P9_Sequence < task1.P9_Sequence);

			AssertArrayEqualsByElements("THEN task0 should be startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task0));
			AssertArrayEqualsByElements("THEN task1 should not startable", new[] { "|SRT=Y", "|SRT=N" }, GetSRTLogReferenceOrderByEventTime(task1));
		}

		[TestDate(2017, 1, 2)]
		[TestDateIncremental(seconds: 1)]
		public void TestNotLogNonStartable_IfPreviouslyHasNoStartableLog()
		{
			var task2 = BMSTestHelper.CreateTask(workflow, sequence: 1, description: "task2", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);

			Factory.Save();

			AssertEquals("GIVEN task2 is closed", ProcessTaskStatusCodeList.Codes.Closed, task2.P9_Status);
			AssertArrayEqualsByElements("GIVEN task1 is startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task1));
			AssertArrayEqualsByElements("GIVEN task2 is not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task2));

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			Factory.Save();
			AssertEquals("WHEN task2 is reopen", ProcessTaskStatusCodeList.Codes.Open, task2.P9_Status);
			AssertArrayEqualsByElements("THEN task1 should startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task1));
			AssertArrayEqualsByElements("THEN task2 should not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task2));

			task2.P9_Sequence = 1000;
			Factory.Save();
			Assert("WHEN task2 is updated to higher sequence", task1.P9_Sequence < task2.P9_Sequence);
			AssertArrayEqualsByElements("THEN task1 should startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task1));
			AssertArrayEqualsByElements("THEN task2 should not log |SRT=N because previously has no |SRT=Y", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task2));

			var task3 = BMSTestHelper.CreateTask(workflow, sequence: 100, description: "task3");
			Factory.Save();
			AssertArrayEqualsByElements("THEN task1 should startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task1));
			AssertArrayEqualsByElements("THEN task2 should not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task2));
			AssertArrayEqualsByElements("THEN task3 should not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task3));

			task1.P9_Sequence = 0;
			Factory.Save();
			AssertEquals("WHEN task1 set to lowest sequence", 0, task1.P9_Sequence);

			AssertArrayEqualsByElements("THEN task1 should startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task1));
			AssertArrayEqualsByElements("THEN task2 should not log |SRT=N because previously has no |SRT=Y", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task2));
			AssertArrayEqualsByElements("THEN task3 should not log |SRT=N because previously has no |SRT=Y", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task3));
		}

		[TestDate(2017, 1, 2)]
		[TestDateIncremental(seconds: 1)]
		public void TestMultipleTasksUpdate_ShouldNotCauseCascadeEffect()
		{
			var task2 = BMSTestHelper.CreateTask(workflow, sequence: 2, description: "task2");
			var task3 = BMSTestHelper.CreateTask(workflow, sequence: 3, description: "task3");
			var task4 = BMSTestHelper.CreateTask(workflow, sequence: 4, description: "task4");
			var workflow2 = BMSTestHelper.CreateWorkflow(task1.ProcessHeader.JobHeader as ProcessJobHeader, "workflow2");

			Factory.Save();

			AssertArrayEqualsByElements("GIVEN task1 is startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task1));
			AssertArrayEqualsByElements("GIVEN task2 is not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task2));
			AssertArrayEqualsByElements("GIVEN task3 is not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task3));
			AssertArrayEqualsByElements("GIVEN task4 is not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task4));

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			workflow2.FH_CompletionStatement = "workflow2 updated"; // simulate other workflow updating, should not have impact i.e. cascading effect
			Factory.Save();

			AssertEquals("WHEN task1 is closed", ProcessTaskStatusCodeList.Codes.Closed, task1.P9_Status);
			AssertEquals("WHEN task2 is closed", ProcessTaskStatusCodeList.Codes.Closed, task2.P9_Status);
			AssertEquals("WHEN workflow is updated", "workflow2 updated", workflow2.FH_CompletionStatement);

			AssertArrayEqualsByElements("THEN task1 SRT log should unchanged", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task1));
			AssertArrayEqualsByElements("THEN task2 SRT log should unchanged", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task2));
			AssertArrayEqualsByElements("THEN task3 should startable (only 1 SRT=Y log i.e. no cascading effect)", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task3));
			AssertArrayEqualsByElements("THEN task4 SRT log should unchanged (no cascading effect)", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task4));
		}

		#region Startability status for task modification

		[TestDate(2017, 1, 2)]
		[TestDateIncremental(seconds: 1)]
		public void TestLowerTaskClose()
		{
			AssertLowerTaskClosed(lowerTaskStatus: ProcessTaskStatusCodeList.Codes.Closed);
		}

		[TestDate(2017, 1, 2)]
		[TestDateIncremental(seconds: 1)]
		public void TestLowerTaskCancelled()
		{
			AssertLowerTaskClosed(lowerTaskStatus: ProcessTaskStatusCodeList.Codes.Cancelled);
		}

		[TestDate(2017, 1, 2)]
		[TestDateIncremental(seconds: 1)]
		public void TestLowerTaskDeleted()
		{
			AssertLowerTaskClosed(lowerTaskStatus: "delete");
		}

		void AssertLowerTaskClosed(string lowerTaskStatus)
		{
			var currentWorkflow = BMSTestHelper.CreateWorkflowAndTask(Factory, completionStatement: "currentWorkflow", description: "currentTask1", sequence: 1);
			var currentTask1 = currentWorkflow.Tasks.First();
			var currentTask2 = BMSTestHelper.CreateTask(currentWorkflow, sequence: 2, description: "currentTask2");

			Factory.Save();

			Assert("GIVEN currentTask1 has lower sequence than currentTask2", currentTask1.P9_Sequence < currentTask2.P9_Sequence);
			AssertArrayEqualsByElements("GIVEN currentTask1 is startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(currentTask1));
			AssertArrayEqualsByElements("GIVEN currentTask2 is not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(currentTask2));

			if (lowerTaskStatus == "delete")
			{
				currentTask1.Delete();
				AssertEquals("WHEN lower currentTask1 is deleted", true, currentTask1.IsDeleted);
			}
			else
			{
				currentTask1.P9_Status = lowerTaskStatus;
				AssertEquals("WHEN lower currentTask1 is closed", false, currentTask1.IsOpen);
			}

			Factory.Save();

			AssertArrayEqualsByElements("THEN currentTask2 should startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(currentTask2));
		}

		[TestDate(2017, 1, 2)]
		[TestDateIncremental(seconds: 1)]
		public void TestFormat()
		{
			var task2 = BMSTestHelper.CreateTask(workflow, sequence: 2, description: "task2");
			Factory.Save();

			AssertEquals("GIVEN task1 has no staff", string.Empty, task1.P9_GS_NKAssignedStaffMember);
			AssertArrayEqualsByElements("GIVEN task1 is startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task1));

			task2.P9_Sequence = 0;
			Factory.Save();
			Assert("WHEN task2 is updated to lower sequence than task1", task1.P9_Sequence > task2.P9_Sequence);
			AssertArrayEqualsByElements("THEN task1 should not startable", new[] { "|SRT=Y", "|SRT=N" }, GetSRTLogReferenceOrderByEventTime(task1));
			AssertArrayEqualsByElements("THEN task2 should startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task2));

			task1.P9_GS_NKAssignedStaffMember = "BAS";
			task2.P9_Sequence = 2;
			Factory.Save();

			AssertEquals("WHEN task1 is assigned to BAS", "BAS", task1.P9_GS_NKAssignedStaffMember);
			Assert("WHEN task2 is updated to higher sequence than task1", task1.P9_Sequence < task2.P9_Sequence);

			AssertArrayEqualsByElements("THEN task1 should startable", new[] { "|SRT=Y", "|SRT=N", "|SRT=Y|ASN=BAS" }, GetSRTLogReferenceOrderByEventTime(task1));
			AssertArrayEqualsByElements("THEN task2 should not startable", new[] { "|SRT=Y", "|SRT=N" }, GetSRTLogReferenceOrderByEventTime(task2));

			task2.P9_Sequence = 0;
			Factory.Save();
			Assert("WHEN task2 is updated to lower sequence than task1", task1.P9_Sequence > task2.P9_Sequence);

			AssertArrayEqualsByElements("THEN task1 should not startable", new[] { "|SRT=Y", "|SRT=N", "|SRT=Y|ASN=BAS", "|SRT=N|ASN=BAS" }, GetSRTLogReferenceOrderByEventTime(task1));
			AssertArrayEqualsByElements("THEN task2 should startable", new[] { "|SRT=Y", "|SRT=N", "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task2));
		}

		[TestDate(2017, 1, 2)]
		[TestDateIncremental(seconds: 1)]
		public void TestDescription_OnStatusChange()
		{
			var task2 = BMSTestHelper.CreateTask(workflow, sequence: 2, description: "task2");

			Factory.Save();

			AssertEquals("GIVEN startable task THEN description should be 'Task became startable'", "Task became startable", task1.Logs.MostRecentLogByEventTime(Events.StartabilityChanged).DisplayEventReference);

			task2.P9_Sequence = 0;
			Factory.Save();
			AssertEquals("GIVEN non-startable task THEN description should be 'Task became no longer startable'", "Task became no longer startable", task1.Logs.MostRecentLogByEventTime(Events.StartabilityChanged).DisplayEventReference);
		}

		#region Startability status for milestone, trigger, exception

		[TestDate(2017, 1, 2)]
		[TestDateIncremental(seconds: 1)]
		public void TestMilestone()
		{
			AssertMilestone_Trigger_Exception(Core.Constants.Workflow.MilestoneType);
		}

		[TestDate(2017, 1, 2)]
		[TestDateIncremental(seconds: 1)]
		public void TestTrigger()
		{
			AssertMilestone_Trigger_Exception(Core.Constants.Workflow.WorkflowTriggerType);
		}

		[TestDate(2017, 1, 2)]
		[TestDateIncremental(seconds: 1)]
		public void TestException()
		{
			AssertMilestone_Trigger_Exception(Core.Constants.Workflow.ExceptionType);
		}

		void AssertMilestone_Trigger_Exception(string type)
		{
			task1.P9_Type = type;
			var task2 = BMSTestHelper.CreateTask(workflow, sequence: 2, description: "task2");
			task2.P9_Type = type;

			Factory.Save();

			Assert("GIVEN task1 is milestone/trigger/exception", task1.P9_Type.ToString().In(Core.Constants.Workflow.MilestoneType, Core.Constants.Workflow.WorkflowTriggerType, Core.Constants.Workflow.ExceptionType));
			Assert("GIVEN task2 is milestone/trigger/exception", task2.P9_Type.ToString().In(Core.Constants.Workflow.MilestoneType, Core.Constants.Workflow.WorkflowTriggerType, Core.Constants.Workflow.ExceptionType));
			AssertArrayEqualsByElements("GIVEN task1 has no SRT log", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task1));
			AssertArrayEqualsByElements("GIVEN task1 has no SRT log", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task2));

			task2.P9_Sequence = 0;
			Factory.Save();

			Assert("WHEN task2 sequence is updated to lower than task1", task1.P9_Sequence > task2.P9_Sequence);

			AssertArrayEqualsByElements("THEN task1 should not raise SRT log", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task1));
			AssertArrayEqualsByElements("THEN task2 should not raise SRT log", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task2));
		}

		#endregion

		#endregion

		#endregion

		#region Startability status for Component in bucket and buffer

		[TestDate(2017, 1, 2)]
		[TestDateIncremental(milliseconds: 10)]
		public void TestBucket()
		{
			var config = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory);

			workflow.FH_FC_CurrentComponent = config.Bucket.PK;
			var task2 = BMSTestHelper.CreateTask(workflow, sequence: 2, description: "task2");

			Factory.Save();

			AssertEquals("GIVEN workflow in bucket", config.Bucket.PK, workflow.CurrentComponent.PK);
			AssertEquals("GIVEN task1 has no staff", string.Empty, task1.P9_GS_NKAssignedStaffMember);
			Assert("GIVEN task1 has lower sequence than Task2", task2.P9_Sequence > task1.P9_Sequence);
			AssertArrayEqualsByElements("GIVEN task1 is startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task1));
			AssertArrayEqualsByElements("GIVEN task2 is not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task2));

			task2.P9_Sequence = 0;
			Factory.Save();
			Assert("WHEN modifying task2 sequence to lower than task1", task2.P9_Sequence < task1.P9_Sequence);
			AssertArrayEqualsByElements("THEN task1 becomes not-startable", new[] { "|SRT=Y", "|SRT=N" }, GetSRTLogReferenceOrderByEventTime(task1));
			AssertArrayEqualsByElements("THEN task2 becomes startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task2));

			task1.P9_GS_NKAssignedStaffMember = "BAS";
			task2.P9_Sequence = 2;
			Factory.Save();
			Assert("WHEN modifying task2 sequence to higher than task1", task2.P9_Sequence > task1.P9_Sequence);
			AssertEquals("WHEN assign task1 to BAS", "BAS", task1.P9_GS_NKAssignedStaffMember);
			AssertArrayEqualsByElements("THEN task1 becomes startable", new[] { "|SRT=Y", "|SRT=N", "|SRT=Y|ASN=BAS" }, GetSRTLogReferenceOrderByEventTime(task1));
			AssertArrayEqualsByElements("THEN task2 becomes not startable", new[] { "|SRT=Y", "|SRT=N" }, GetSRTLogReferenceOrderByEventTime(task2));

			task2.P9_Sequence = 0;
			Factory.Save();
			Assert("WHEN modifying task2 sequence to lower than task1", task2.P9_Sequence < task1.P9_Sequence);
			AssertArrayEqualsByElements("THEN task1 becomes not-startable", new[] { "|SRT=Y", "|SRT=N", "|SRT=Y|ASN=BAS", "|SRT=N|ASN=BAS" }, GetSRTLogReferenceOrderByEventTime(task1));
			AssertArrayEqualsByElements("THEN task2 becomes startable", new[] { "|SRT=Y", "|SRT=N", "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task2));
		}

		[TestDate(2017, 1, 2)]
		[TestDateIncremental(milliseconds: 10)]
		public void TestBuffer()
		{
			var config = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory);

			workflow.FH_FC_CurrentComponent = config.Buffer.PK;
			var task2 = BMSTestHelper.CreateTask(workflow, sequence: 2, description: "task2");

			Factory.Save();

			AssertEquals("GIVEN workflow in buffer", config.Buffer.PK, workflow.CurrentComponent.PK);
			AssertEquals("GIVEN task1 has no staff", string.Empty, task1.P9_GS_NKAssignedStaffMember);
			Assert("GIVEN task1 has lower sequence than task2", task2.P9_Sequence > task1.P9_Sequence);
			AssertArrayEqualsByElements("GIVEN task1 is startable", new[] { "|SRT=Y|PEN=0.00|ZON=3" }, GetSRTLogReferenceOrderByEventTime(task1));
			AssertArrayEqualsByElements("GIVEN task2 is not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task2));

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(3);
			task2.P9_Sequence = 0;
			Factory.Save();
			Assert("WHEN modifying task2 sequence to lower than task1", task2.P9_Sequence < task1.P9_Sequence);
			AssertArrayEqualsByElements("THEN task1 becomes non-startable", new[] { "|SRT=Y|PEN=0.00|ZON=3", "|SRT=N|PEN=0.25|ZON=3" }, GetSRTLogReferenceOrderByEventTime(task1));
			AssertArrayEqualsByElements("THEN task2 becomes startable", new[] { "|SRT=Y|PEN=0.25|ZON=3" }, GetSRTLogReferenceOrderByEventTime(task2));

			task1.P9_GS_NKAssignedStaffMember = "BAS";
			task2.P9_Sequence = 2;
			Factory.Save();
			Assert("WHEN modifying task2 sequence to higher than task1", task2.P9_Sequence > task1.P9_Sequence);
			AssertEquals("WHEN assign task1 to BAS", "BAS", task1.P9_GS_NKAssignedStaffMember);
			AssertArrayEqualsByElements("THEN task1 becomes startable", new[] { "|SRT=Y|PEN=0.00|ZON=3", "|SRT=N|PEN=0.25|ZON=3", "|SRT=Y|ASN=BAS|PEN=0.25|ZON=3" }, GetSRTLogReferenceOrderByEventTime(task1));
			AssertArrayEqualsByElements("THEN task2 becomes not startable", new[] { "|SRT=Y|PEN=0.25|ZON=3", "|SRT=N|PEN=0.25|ZON=3" }, GetSRTLogReferenceOrderByEventTime(task2));

			task2.P9_Sequence = 0;
			Factory.Save();
			Assert("WHEN modifying task2 sequence to lower than task1", task2.P9_Sequence < task1.P9_Sequence);
			AssertArrayEqualsByElements("THEN task1 becomes non-startable", new[] { "|SRT=Y|PEN=0.00|ZON=3", "|SRT=N|PEN=0.25|ZON=3", "|SRT=Y|ASN=BAS|PEN=0.25|ZON=3", "|SRT=N|ASN=BAS|PEN=0.25|ZON=3" }, GetSRTLogReferenceOrderByEventTime(task1));
			AssertArrayEqualsByElements("THEN task2 becomes startable", new[] { "|SRT=Y|PEN=0.25|ZON=3", "|SRT=N|PEN=0.25|ZON=3", "|SRT=Y|PEN=0.25|ZON=3" }, GetSRTLogReferenceOrderByEventTime(task2));
		}

		#endregion

		#region Startability status for Workflow

		[TestDate(2017, 1, 2)]
		[TestDateIncremental(seconds: 1)]
		public void TestWorkflowStatus_OPN_to_BLK_SameJob()
		{
			var task2 = BMSTestHelper.CreateTask(workflow, sequence: 2, description: "task2");
			Factory.Save();

			AssertEquals("GIVEN workflow is Open", WorkflowStatusList.Codes.Open, workflow.FH_Status);
			AssertArrayEqualsByElements("GIVEN task1 is startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task1));
			AssertNull("GIVEN task2 is not startable", task2.Logs?.MostRecentLogByEventTime(Events.StartabilityChanged));

			var prereqWorkflow = BMSTestHelper.CreateWorkflow(workflow.JobHeader, "prereqWorkflow");
			var prereqTask = BMSTestHelper.CreateTask(prereqWorkflow, description: "prereqTask");
			prereqWorkflow.GetOrCreateDependencyLink(workflow);
			Factory.Save();
			Assert("WHEN prereqWorkflow is added", prereqWorkflow.IsPrerequisiteOf(workflow));

			AssertEquals("THEN prereqWorkflow is open", WorkflowStatusList.Codes.Open, prereqWorkflow.FH_Status);
			AssertEquals("THEN workflow should be blocked", WorkflowStatusList.Codes.Blocked, workflow.FH_Status);
			AssertArrayEqualsByElements("THEN task1 should not startable", new[] { "|SRT=Y", "|SRT=N" }, GetSRTLogReferenceOrderByEventTime(task1));
			AssertArrayEqualsByElements("THEN task2 should not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task2));
		}

		[TestDate(2017, 1, 2)]
		[TestDateIncremental(seconds: 1)]
		public void TestWorkflowStatus_BLK_to_OPN_SameJob()
		{
			var jobHeader = workflow.JobHeader;
			var task2 = BMSTestHelper.CreateTask(workflow, sequence: 2, description: "task2");
			var prereqWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "prereqWorkflow");
			var prereqTask1 = BMSTestHelper.CreateTask(prereqWorkflow, description: "prereqTask1");

			prereqWorkflow.GetOrCreateDependencyLink(workflow);

			Factory.Save();

			Assert("GIVEN relationship: prereqWorkflow -> workflow", prereqWorkflow.IsPrerequisiteOf(workflow));
			AssertEquals("GIVEN prereqWorkflow is open", WorkflowStatusList.Codes.Open, prereqWorkflow.FH_Status);
			AssertEquals("GIVEN workflow status is blocked", WorkflowStatusList.Codes.Blocked, workflow.FH_Status);
			AssertArrayEqualsByElements("GIVEN prereqTask1 is startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(prereqTask1));
			AssertArrayEqualsByElements("GIVEN task1 is not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task1));
			AssertArrayEqualsByElements("GIVEN task2 is not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task2));

			prereqTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			AssertEquals("WHEN prereqTask is closed", WorkflowStatusList.Codes.Closed, prereqTask1.P9_Status);

			AssertEquals("THEN prereqWorkflow should be closed", WorkflowStatusList.Codes.Closed, prereqWorkflow.FH_Status);
			AssertEquals("THEN workflow should be open", WorkflowStatusList.Codes.Open, workflow.FH_Status);
			AssertArrayEqualsByElements("THEN prereqTask1 should not has new SRT log", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(prereqTask1));
			AssertArrayEqualsByElements("THEN task1 should be startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task1));
			AssertArrayEqualsByElements("THEN task2 should not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task2));
		}

		[TestDate(2017, 1, 2)]
		[TestDateIncremental(seconds: 1)]
		public void TestWorkflowStatus_BLK_to_OPN_DifferentJobs()
		{
			var task2 = BMSTestHelper.CreateTask(workflow, sequence: 2, description: "task2");
			var prereqWorkflow = BMSTestHelper.CreateWorkflowAndTask(Factory, completionStatement: "prereqWorkflow", description: "prereqTask1");
			var prereqTask1 = prereqWorkflow.Tasks.First();

			prereqWorkflow.GetOrCreateDependencyLink(workflow);

			Factory.Save();
			Assert("GIVEN relationship: prereqWorkflow -> workflow", prereqWorkflow.IsPrerequisiteOf(workflow));
			AssertEquals("GIVEN prereqWorkflow is open", WorkflowStatusList.Codes.Open, prereqWorkflow.FH_Status);
			AssertEquals("GIVEN workflow is blocked", WorkflowStatusList.Codes.Blocked, workflow.FH_Status);
			AssertArrayEqualsByElements("GIVEN prereqTask1 is startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(prereqTask1));
			AssertArrayEqualsByElements("GIVEN task1 is not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task1));
			AssertArrayEqualsByElements("GIVEN task2 is not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task2));

			prereqTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			AssertEquals("WHEN prereqWorkflow task is closed", WorkflowStatusList.Codes.Closed, prereqTask1.P9_Status);

			AssertEquals("THEN prereqWorkflow should be closed", WorkflowStatusList.Codes.Closed, prereqWorkflow.FH_Status);
			AssertEquals("THEN workflow should be open", WorkflowStatusList.Codes.Open, workflow.FH_Status);
			AssertArrayEqualsByElements("THEN prereqTask1 should has no new SRT log", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(prereqTask1));
			AssertArrayEqualsByElements("THEN task1 should be startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task1));
			AssertArrayEqualsByElements("THEN task2 should not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task2));
		}

		[TestDate(2017, 1, 2)]
		[TestDateIncremental(seconds: 1)]
		public void TestWorkflowStatus_BLK_to_OPN_DifferentJobs_WithPrerequisites()
		{
			var prereqWorkflow1 = BMSTestHelper.CreateWorkflowAndTask(Factory, completionStatement: "prereqWorkflow1", description: "prereqTask1");
			var prereqTask1 = prereqWorkflow1.Tasks.First();
			prereqWorkflow1.GetOrCreateDependencyLink(workflow);

			var prereqWorkflow2 = BMSTestHelper.CreateWorkflowAndTask(Factory, completionStatement: "prereqWorkflow2", description: "prereqTask2");
			var prereqTask2 = prereqWorkflow2.Tasks.First();
			prereqWorkflow2.GetOrCreateDependencyLink(prereqWorkflow1);

			Factory.Save();

			Assert("GIVEN relationship: prereqWorkflow2 -> prereqWorkflow1", prereqWorkflow2.IsPrerequisiteOf(prereqWorkflow1));
			Assert("GIVEN relationship: prereqWorkflow1 -> workflow1", prereqWorkflow1.IsPrerequisiteOf(workflow));
			AssertEquals("GIVEN prereqWorkflow2 is open", WorkflowStatusList.Codes.Open, prereqWorkflow2.FH_Status);
			AssertEquals("GIVEN prereqWorkflow1 is blocked", WorkflowStatusList.Codes.Blocked, prereqWorkflow1.FH_Status);
			AssertEquals("GIVEN workflow1 is blocked", WorkflowStatusList.Codes.Blocked, workflow.FH_Status);
			AssertArrayEqualsByElements("GIVEN prereqTask2 is startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(prereqTask2));
			AssertArrayEqualsByElements("GIVEN prereqTask1 is not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(prereqTask1));
			AssertArrayEqualsByElements("GIVEN task1 is not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task1));

			prereqTask2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			prereqTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			AssertEquals("WHEN prereqTask2 is closed", WorkflowStatusList.Codes.Closed, prereqTask2.P9_Status);
			AssertEquals("WHEN prereqTask1 is closed", WorkflowStatusList.Codes.Closed, prereqTask1.P9_Status);

			AssertEquals("THEN prereqWorkflow2 should be closed", WorkflowStatusList.Codes.Closed, prereqWorkflow2.FH_Status);
			AssertEquals("THEN prereqWorkflow1 should be closed", WorkflowStatusList.Codes.Closed, prereqWorkflow1.FH_Status);
			AssertEquals("THEN workflow1 should be open", WorkflowStatusList.Codes.Open, workflow.FH_Status);
			AssertArrayEqualsByElements("THEN prereqTask2 should not have new SRT log", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(prereqTask2));
			AssertArrayEqualsByElements("THEN prereqTask1 should not startable ", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(prereqTask1));
			AssertArrayEqualsByElements("GIVEN task1 should be startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task1));
		}

		[TestDate(2017, 1, 2)]
		[TestDateIncremental(seconds: 1)]
		public void TestWorkflowStatus_BLK_to_OPN_DifferentJobs_WithPostrequisites()
		{
			var postreqWorkflow1 = BMSTestHelper.CreateWorkflowAndTask(Factory, completionStatement: "postreqWorkflow1", description: "postreqTask1");
			var postreqTask1 = postreqWorkflow1.Tasks.First();
			workflow.GetOrCreateDependencyLink(postreqWorkflow1);

			var postreqWorkflow2 = BMSTestHelper.CreateWorkflowAndTask(Factory, completionStatement: "postreqWorkflow2", description: "postreqTask2");
			var postreqTask2 = postreqWorkflow2.Tasks.First();
			workflow.GetOrCreateDependencyLink(postreqWorkflow2);

			Factory.Save();

			Assert("GIVEN relationship: workflow -> postreqWorkflow1", workflow.IsPrerequisiteOf(postreqWorkflow1));
			Assert("GIVEN relationship: workflow -> postreqWorkflow2", workflow.IsPrerequisiteOf(postreqWorkflow2));
			AssertEquals("GIVEN workflow is open", WorkflowStatusList.Codes.Open, workflow.FH_Status);
			AssertEquals("GIVEN postreqWorkflow1 is blocked", WorkflowStatusList.Codes.Blocked, postreqWorkflow1.FH_Status);
			AssertEquals("GIVEN postreqWorkflow2 is blocked", WorkflowStatusList.Codes.Blocked, postreqWorkflow1.FH_Status);
			AssertArrayEqualsByElements("GIVEN task1 is startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task1));
			AssertArrayEqualsByElements("GIVEN postreqTask1 is not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(postreqTask1));
			AssertArrayEqualsByElements("GIVEN postreqTask2 is not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(postreqTask2));

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			AssertEquals("WHEN task1 is closed", WorkflowStatusList.Codes.Closed, task1.P9_Status);

			AssertEquals("THEN workflow should be closed", WorkflowStatusList.Codes.Closed, workflow.FH_Status);
			AssertEquals("THEN postreqWorkflow1 should be open", WorkflowStatusList.Codes.Open, postreqWorkflow1.FH_Status);
			AssertEquals("THEN postreqWorkflow2 should be open", WorkflowStatusList.Codes.Open, postreqWorkflow2.FH_Status);
			AssertArrayEqualsByElements("THEN task1 should not have new SRT log", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task1));
			AssertArrayEqualsByElements("THEN postreqTask1 should be startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(postreqTask1));
			AssertArrayEqualsByElements("THEN postreqTask2 should be startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(postreqTask2));
		}

		[TestDate(2017, 1, 2)]
		[TestDateIncremental(seconds: 1)]
		public void TestReversePrerequisite()
		{
			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(Factory, completionStatement: "workflow2", description: "task2");
			var task2 = workflow2.Tasks.First();
			var link = workflow.GetOrCreateDependencyLink(workflow2);

			Factory.Save();

			Assert("GIVEN relationship: workflow -> workflow2", workflow.IsPrerequisiteOf(workflow2));
			AssertEquals("GIVEN workflow is open", WorkflowStatusList.Codes.Open, workflow.FH_Status);
			AssertEquals("GIVEN workflow2 is blocked", WorkflowStatusList.Codes.Blocked, workflow2.FH_Status);
			AssertArrayEqualsByElements("GIVEN task1 is startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task1));
			AssertArrayEqualsByElements("GIVEN task1 is not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task2));

			link.Delete();
			workflow2.GetOrCreateDependencyLink(workflow);
			Factory.Save();
			Assert("WHEN relationship is reversed: workflow2 -> workflow", workflow2.IsPrerequisiteOf(workflow));

			AssertEquals("THEN workflow is blocked", WorkflowStatusList.Codes.Blocked, workflow.FH_Status);
			AssertEquals("THEN workflow2 is open", WorkflowStatusList.Codes.Open, workflow2.FH_Status);
			AssertArrayEqualsByElements("THEN task1 should not startable", new[] { "|SRT=Y", "|SRT=N" }, GetSRTLogReferenceOrderByEventTime(task1));
			AssertArrayEqualsByElements("GIVEN task2 should be startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task2));
		}

		[TestDate(2017, 1, 2)]
		[TestDateIncremental(seconds: 1)]
		public void TestReopenWorkflowShouldLogStartability()
		{
			var prereqWorkflow = BMSTestHelper.CreateWorkflowAndTask(Factory, completionStatement: "prereqWorkflow", description: "prereqTask", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var prereqTask = prereqWorkflow.Tasks.First();
			prereqWorkflow.GetOrCreateDependencyLink(workflow);
			Factory.Save();
			Assert("GIVEN relationship: prereqWorkflow -> workflow", prereqWorkflow.IsPrerequisiteOf(workflow));
			AssertEquals("GIVEN prereqWorkflow is closed", WorkflowStatusList.Codes.Closed, prereqWorkflow.FH_Status);
			AssertEquals("GIVEN workflow is open", WorkflowStatusList.Codes.Open, workflow.FH_Status);
			AssertArrayEqualsByElements("GIVEN prereqTask is not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(prereqTask));
			AssertArrayEqualsByElements("GIVEN task1 is startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task1));

			prereqTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			Factory.Save();
			AssertEquals("WHEN prereqWorkflow is reopen", WorkflowStatusList.Codes.Open, prereqWorkflow.FH_Status);

			AssertEquals("THEN workflow is blocked", WorkflowStatusList.Codes.Blocked, workflow.FH_Status);
			AssertArrayEqualsByElements("THEN prereqTask should be startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(prereqTask));
			AssertArrayEqualsByElements("GIVEN task1 should not startable", new[] { "|SRT=Y", "|SRT=N" }, GetSRTLogReferenceOrderByEventTime(task1));
		}

		[TestDate(2017, 1, 2)]
		[TestDateIncremental(seconds: 1)]
		public void TestAddWorkflows()
		{
			Factory.Save();
			AssertEquals("GIVEN workflow is open", WorkflowStatusList.Codes.Open, workflow.FH_Status);
			AssertArrayEqualsByElements("GIVEN task1 is startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task1));

			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(Factory, completionStatement: "workflow2", description: "task2");
			var task2 = workflow2.Tasks.First();
			Factory.Save();
			CombineAssertions("WHEN adding workflow2 without relation", () =>
			{
				AssertEquals("THEN workflow is open", WorkflowStatusList.Codes.Open, workflow.FH_Status);
				AssertArrayEqualsByElements("GIVEN task1 has no new SRT log", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task1));
				AssertEquals("THEN workflow2 is open", WorkflowStatusList.Codes.Open, workflow2.FH_Status);
				AssertArrayEqualsByElements("GIVEN task2 is startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task2));
			});

			var postreqWorkflow = BMSTestHelper.CreateWorkflowAndTask(Factory, completionStatement: "postreqWorkflow", description: "postreqTask");
			var postreqTask = postreqWorkflow.Tasks.First();
			workflow.GetOrCreateDependencyLink(postreqWorkflow);
			Factory.Save();
			Assert("WHEN adding relationship: workflow -> postreqWorkflow", postreqWorkflow.IsPostrequisiteOf(workflow));

			AssertEquals("THEN workflow is open", WorkflowStatusList.Codes.Open, workflow.FH_Status);
			AssertArrayEqualsByElements("THEN task1 has no new SRT log", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task1));
			AssertEquals("THEN workflow2 is open", WorkflowStatusList.Codes.Open, workflow2.FH_Status);
			AssertArrayEqualsByElements("THEN task2 has no new SRT log", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task2));
			AssertEquals("THEN postreqWorkflow is blocked", WorkflowStatusList.Codes.Blocked, postreqWorkflow.FH_Status);
			AssertArrayEqualsByElements("THEN postreqTask is not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(postreqTask));

			var prereqWorkflow = BMSTestHelper.CreateWorkflowAndTask(Factory, completionStatement: "prereqWorkflow", description: "prereqTask");
			var prereqTask = prereqWorkflow.Tasks.First();
			prereqWorkflow.GetOrCreateDependencyLink(workflow);

			Factory.Save();
			Assert("WHEN adding prereqWorkflow", prereqWorkflow.IsPrerequisiteOf(workflow));

			AssertEquals("THEN workflow is blocked", WorkflowStatusList.Codes.Blocked, workflow.FH_Status);
			AssertArrayEqualsByElements("THEN task1 is not startable", new[] { "|SRT=Y", "|SRT=N" }, GetSRTLogReferenceOrderByEventTime(task1));
			AssertEquals("THEN workflow2 is open", WorkflowStatusList.Codes.Open, workflow2.FH_Status);
			AssertArrayEqualsByElements("THEN task2 has no new SRT log", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task2));
			AssertEquals("THEN postreqWorkflow is blocked", WorkflowStatusList.Codes.Blocked, postreqWorkflow.FH_Status);
			AssertArrayEqualsByElements("THEN postreqTask is not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(postreqTask));
			AssertEquals("THEN prereqWorkflow is open", WorkflowStatusList.Codes.Open, prereqWorkflow.FH_Status);
			AssertArrayEqualsByElements("THEN prereqTask is startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(prereqTask));
		}

		[TestDate(2017, 1, 2)]
		[TestDateIncremental(seconds: 1)]
		public void TestDeleteWorkflows()
		{
			var prereqWorkflow = BMSTestHelper.CreateWorkflowAndTask(Factory, completionStatement: "prereqWorkflow", description: "prereqTask");
			var prereqTask = prereqWorkflow.Tasks.First();
			prereqWorkflow.GetOrCreateDependencyLink(workflow);

			var postreqWorkflow = BMSTestHelper.CreateWorkflowAndTask(Factory, completionStatement: "postreqWorkflow", description: "postreqTask");
			var postreqTask = postreqWorkflow.Tasks.First();
			workflow.GetOrCreateDependencyLink(postreqWorkflow);

			Factory.Save();

			Assert("GIVEN relationship: prereqWorkflow -> workflow", prereqWorkflow.IsPrerequisiteOf(workflow));
			Assert("GIVEN relationship: workflow -> postreqWorkflow", workflow.IsPrerequisiteOf(postreqWorkflow));
			AssertEquals("GIVEN prereqWorkflow is open", WorkflowStatusList.Codes.Open, prereqWorkflow.FH_Status);
			AssertEquals("GIVEN workflow is blocked", WorkflowStatusList.Codes.Blocked, workflow.FH_Status);
			AssertEquals("GIVEN postrqWorkflow is blocked", WorkflowStatusList.Codes.Blocked, workflow.FH_Status);
			AssertArrayEqualsByElements("GIVEN prereqTask is startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(prereqTask));
			AssertArrayEqualsByElements("GIVEN task1 is not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task1));
			AssertArrayEqualsByElements("GIVEN postreqTask is not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(postreqTask));

			postreqWorkflow.Delete();
			Factory.Save();
			CombineAssertions("WHEN deleting postreqWorkflow THEN nothing change to other workflows", () =>
			{
				AssertEquals("WHEN deleting postreqWorkflow", true, postreqWorkflow.IsDeleted);
				AssertEquals("THEN prereqWorkflow should be open", WorkflowStatusList.Codes.Open, prereqWorkflow.FH_Status);
				AssertEquals("THEN workflow should be blocked", WorkflowStatusList.Codes.Blocked, workflow.FH_Status);
				AssertArrayEqualsByElements("THEN prereqTask SRT log is unchanged", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(prereqTask));
				AssertArrayEqualsByElements("THEN task1 SRT log is unchanged", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task1));
				AssertArrayEqualsByElements("THEN postreqTask SRT log is unchanged", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(postreqTask));
			});

			prereqWorkflow.Delete();
			Factory.Save();
			CombineAssertions("WHEN deleting prereqWorkflow THEN workflow should be open", () =>
			{
				AssertEquals("WHEN deleting prereqWorkflow", true, prereqWorkflow.IsDeleted);
				AssertEquals("THEN workflow should be open", WorkflowStatusList.Codes.Open, workflow.FH_Status);
				AssertArrayEqualsByElements("THEN task1 should be startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task1));
				AssertArrayEqualsByElements("THEN prereqTask SRT log is unchanged", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(prereqTask));
				AssertArrayEqualsByElements("THEN postreqTask SRT log is unchanged", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(postreqTask));
			});
		}

		[TestDate(2017, 1, 2)]
		[TestDateIncremental(seconds: 1)]
		public void TestAddTasks()
		{
			task1.P9_Sequence = 10;
			var prereqWorkflow = BMSTestHelper.CreateWorkflowAndTask(Factory, completionStatement: "prereqWorkflow", description: "prereqTask", sequence: 10);
			var prereqTask = prereqWorkflow.Tasks.First();
			prereqWorkflow.GetOrCreateDependencyLink(workflow);

			Factory.Save();

			Assert("GIVEN relationship: prereqWorkflow -> workflow", prereqWorkflow.IsPrerequisiteOf(workflow));
			AssertArrayEqualsByElements("GIVEN prereqTask is startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(prereqTask));
			AssertArrayEqualsByElements("GIVEN task1 is not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task1));

			var task2 = BMSTestHelper.CreateTask(workflow, description: "task2", sequence: 100);
			Factory.Save();
			CombineAssertions("WHEN adding new higher sequence task on workflow", () =>
			{
				AssertArrayEqualsByElements("THEN prereqTask SRT log should be unchanged", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(prereqTask));
				AssertArrayEqualsByElements("THEN task1 should not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task1));
				AssertArrayEqualsByElements("THEN task2 should not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task2));
			});

			var task3 = BMSTestHelper.CreateTask(workflow, description: "task3", sequence: 0);
			Factory.Save();
			CombineAssertions("WHEN adding lower sequence task on workflow", () =>
			{
				AssertArrayEqualsByElements("THEN prereqTask SRT log should be unchanged", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(prereqTask));
				AssertArrayEqualsByElements("THEN task1 should not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task1));
				AssertArrayEqualsByElements("THEN task2 should not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task2));
				AssertArrayEqualsByElements("THEN task3 should not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task3));
			});

			var prereqTask2 = BMSTestHelper.CreateTask(prereqWorkflow, description: "prereqTask2", sequence: 100);
			Factory.Save();
			CombineAssertions("WHEN adding new higher sequence task on prereqWorkflow", () =>
			{
				AssertArrayEqualsByElements("THEN prereqTask SRT log should be unchanged", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(prereqTask));
				AssertArrayEqualsByElements("THEN task1 should not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task1));
				AssertArrayEqualsByElements("THEN task2 should not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task2));
				AssertArrayEqualsByElements("THEN task3 should not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task3));
				AssertArrayEqualsByElements("THEN prereqTask2 should not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(prereqTask2));
			});

			var prereqTask3 = BMSTestHelper.CreateTask(prereqWorkflow, description: "prereqTask3", sequence: 0);
			Factory.Save();
			CombineAssertions("WHEN adding lower sequence task on prereqWorkflow", () =>
			{
				AssertArrayEqualsByElements("THEN prereqTask SRT log should not startable", new[] { "|SRT=Y", "|SRT=N" }, GetSRTLogReferenceOrderByEventTime(prereqTask));
				AssertArrayEqualsByElements("THEN task1 should not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task1));
				AssertArrayEqualsByElements("THEN task2 should not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task2));
				AssertArrayEqualsByElements("THEN task3 should not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task3));
				AssertArrayEqualsByElements("THEN prereqTask2 should not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(prereqTask2));
				AssertArrayEqualsByElements("THEN prereqTask3 should startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(prereqTask3));
			});
		}

		[TestDate(2017, 1, 2)]
		[TestDateIncremental(seconds: 1)]
		public void TestAddAndDeleteLink()
		{
			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(Factory, completionStatement: "workflow2", description: "task2");
			var task2 = workflow2.Tasks.First();

			Factory.Save();

			AssertEquals("GIVEN workflow is open", WorkflowStatusList.Codes.Open, workflow.FH_Status);
			AssertEquals("GIVEN workflow2 is open", WorkflowStatusList.Codes.Open, workflow2.FH_Status);
			AssertArrayEqualsByElements("GIVEN task1 is startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task1));
			AssertArrayEqualsByElements("GIVEN task2 is startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task2));

			var link = workflow.GetOrCreateDependencyLink(workflow2);
			Factory.Save();
			Assert("WHEN create link: workflow -> workflow2", workflow.IsPrerequisiteOf(workflow2));

			AssertEquals("THEN workflow is open", WorkflowStatusList.Codes.Open, workflow.FH_Status);
			AssertEquals("THEN workflow2 is blocked", WorkflowStatusList.Codes.Blocked, workflow2.FH_Status);
			AssertArrayEqualsByElements("THEN task1 SRT log should be unchanged", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task1));
			AssertArrayEqualsByElements("THEN task2 should not startable", new[] { "|SRT=Y", "|SRT=N" }, GetSRTLogReferenceOrderByEventTime(task2));

			link.Delete();
			Factory.Save();
			AssertEquals("WHEN link is deleted", false, workflow.IsPrerequisiteOf(workflow2));

			AssertEquals("THEN workflow1 is open", WorkflowStatusList.Codes.Open, workflow.FH_Status);
			AssertEquals("THEN workflow2 is open", WorkflowStatusList.Codes.Open, workflow.FH_Status);
			AssertArrayEqualsByElements("THEN task1 SRT log should be unchanged", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task1));
			AssertArrayEqualsByElements("THEN task2 should be startable", new[] { "|SRT=Y", "|SRT=N", "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(task2));
		}

		[TestDate(2017, 1, 2)]
		[TestDateIncremental(seconds: 1)]
		public void TestParentChild()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var currentWorkflow = BMSTestHelper.CreateWorkflowAndTask(jobHeader, completionStatement: "currentWorkflow", description: "currentTask");
			var currentTask1 = currentWorkflow.Tasks.First();
			var prereqWorkflow = BMSTestHelper.CreateWorkflowAndTask(jobHeader, completionStatement: "prereqWorkflow", description: "prereqTask");
			var prereqTask = prereqWorkflow.Tasks.First();
			var parentWorkflow = BMSTestHelper.CreateWorkflowAndTask(jobHeader, completionStatement: "parentWorkflow", description: "parentTask");
			var parentTask = parentWorkflow.Tasks.First();
			var childWorkflow = BMSTestHelper.CreateWorkflowAndTask(jobHeader, completionStatement: "childWorkflow", description: "childTask");
			var childTask = childWorkflow.Tasks.First();

			prereqWorkflow.GetOrCreateDependencyLink(currentWorkflow);
			currentWorkflow.GetOrCreateLinkToParent(parentWorkflow);
			childWorkflow.GetOrCreateLinkToParent(currentWorkflow);

			Factory.Save();

			Assert("GIVEN prereqWorkflow -> currentWorkflow", prereqWorkflow.IsPrerequisiteOf(currentWorkflow));
			Assert("GIVEN currentWorkflow -> parentWorkflow", parentWorkflow.IsParentOf(currentWorkflow));
			Assert("GIVEN childWorkflow -> currentWorkflow", currentWorkflow.IsParentOf(childWorkflow));
			AssertArrayEqualsByElements("GIVEN prereqTask is startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(prereqTask));
			AssertArrayEqualsByElements("GIVEN currentTask1 is not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(currentTask1));
			AssertArrayEqualsByElements("GIVEN parentTask is startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(parentTask));
			AssertArrayEqualsByElements("GIVEN childTask is not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(childTask));

			prereqTask.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			Factory.Save();
			AssertEquals("WHEN prereqWorkflow is closed", WorkflowStatusList.Codes.Closed, prereqWorkflow.FH_Status);

			AssertArrayEqualsByElements("THEN prereqTask should be unchanged", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(prereqTask));
			AssertArrayEqualsByElements("THEN currentTask1 should be unchanged", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(currentTask1));
			AssertArrayEqualsByElements("THEN parentTask should be unchanged", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(parentTask));
			AssertArrayEqualsByElements("THEN childTask should be startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(childTask));
		}

		[TestDate(2017, 1, 2)]
		[TestDateIncremental(seconds: 1)]
		public void TestParentChild_DifferentJob()
		{
			var currentWorkflow = BMSTestHelper.CreateWorkflowAndTask(Factory, completionStatement: "currentWorkflow", description: "currentTask");
			var currentTask1 = currentWorkflow.Tasks.First();
			var prereqWorkflow = BMSTestHelper.CreateWorkflowAndTask(Factory, completionStatement: "prereqWorkflow", description: "prereqTask");
			var prereqTask = prereqWorkflow.Tasks.First();
			var parentWorkflow = BMSTestHelper.CreateWorkflowAndTask(Factory, completionStatement: "parentWorkflow", description: "parentTask");
			var parentTask = parentWorkflow.Tasks.First();
			var childWorkflow = BMSTestHelper.CreateWorkflowAndTask(Factory, completionStatement: "childWorkflow", description: "childTask");
			var childTask = childWorkflow.Tasks.First();

			prereqWorkflow.GetOrCreateDependencyLink(currentWorkflow);
			currentWorkflow.GetOrCreateLinkToParent(parentWorkflow);
			childWorkflow.GetOrCreateLinkToParent(currentWorkflow);

			Factory.Save();

			Assert("GIVEN prereqWorkflow -> currentWorkflow", prereqWorkflow.IsPrerequisiteOf(currentWorkflow));
			Assert("GIVEN currentWorkflow -> parentWorkflow", parentWorkflow.IsParentOf(currentWorkflow));
			Assert("GIVEN childWorkflow -> currentWorkflow", currentWorkflow.IsParentOf(childWorkflow));

			AssertEquals("GIVEN prereqWorkflow is startable", true, ((IProposedNetworkEntity)prereqWorkflow).IsStartable);
			AssertArrayEqualsByElements("GIVEN prereqTask is startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(prereqTask));

			AssertEquals("GIVEN currentWorkflow is not startable", false, ((IProposedNetworkEntity)currentWorkflow).IsStartable);
			AssertArrayEqualsByElements("GIVEN currentTask1 is not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(currentTask1));

			AssertEquals("GIVEN parentWorkflow is startable", true, ((IProposedNetworkEntity)parentWorkflow).IsStartable);
			AssertArrayEqualsByElements("GIVEN parentTask is startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(parentTask));

			AssertEquals("GIVEN childWorkflow is startable", false, ((IProposedNetworkEntity)childWorkflow).IsStartable);

			prereqTask.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			Factory.Save();
			AssertEquals("WHEN prereqWorkflow is closed", WorkflowStatusList.Codes.Closed, prereqWorkflow.FH_Status);

			AssertArrayEqualsByElements("THEN prereqTask should be unchanged", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(prereqTask));
			AssertArrayEqualsByElements("THEN currentTask1 should be unchanged", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(currentTask1));
			AssertArrayEqualsByElements("THEN parentTask should be startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(parentTask));
			AssertArrayEqualsByElements("THEN childTask should be startable", new[] { "|SRT=Y" }, GetSRTLogReferenceOrderByEventTime(childTask));
		}

		[TestDate(2017, 1, 2)]
		[TestDateIncremental(seconds: 1)]
		public void TestFetchHint()
		{
			var config = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory);

			workflow.FH_FC_CurrentComponent = config.Buffer.PK;
			var task2 = BMSTestHelper.CreateTask(workflow, sequence: 2, description: "task2");

			Factory.Save();

			AssertEquals("GIVEN workflow in buffer", config.Buffer.PK, workflow.CurrentComponent.PK);
			AssertEquals("GIVEN task1 has no staff", string.Empty, task1.P9_GS_NKAssignedStaffMember);
			Assert("GIVEN task1 has lower sequence than task2", task2.P9_Sequence > task1.P9_Sequence);
			AssertArrayEqualsByElements("GIVEN task1 is startable", new[] { "|SRT=Y|PEN=0.00|ZON=3" }, GetSRTLogReferenceOrderByEventTime(task1));
			AssertArrayEqualsByElements("GIVEN task2 is not startable", System.Array.Empty<string>(), GetSRTLogReferenceOrderByEventTime(task2));

			Factory.ResetDatabaseLoadCount();

			var preConditionDbHits = new Dictionary<string, int>
			{
				{ StmALogSchema.Constants.TableName, 0 },
			};

			AssertDbHits(preConditionDbHits, Factory);

			AssertEquals("GIVEN 0 fetchHint on StmALog", 0, Factory.ActiveFetchHintsForTable(StmALogSchema.Constants.TableName));

			workflow.FH_CompletionStatement = "updating workflow";

			AssertEquals("GIVEN workflow status has not changed", false, workflow.FH_StatusInfo.HasChanges);
			AssertEquals("GIVEN workflow completion-statement is changed to trigger FetchForFactorySaveCore", true, workflow.FH_CompletionStatementInfo.HasChanges);

			Factory.Save();

			CombineAssertions("WHEN saving THEN no StmAlog fetchHint / dbHit from ProcessHeaderLogger.AddStartabilityEvent because workflow status has not changed", () =>
			{
				AssertEquals("active fetch hints", 0, Factory.ActiveFetchHintsForTable(StmALogSchema.Constants.TableName));

				var dbHits = new Dictionary<string, int>
				{
					{ ProcessTasksSchema.Constants.TableName, 3 },
					{ ProcessHeaderLinkSchema.Constants.TableName, 1 },
				};

				AssertDbHits(dbHits, Factory);
			});
		}

		#endregion

		#region Implementation

		internal static string[] GetSRTLogReferenceOrderByEventTime(ProcessTask processTask)
		{
			return processTask.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.StartabilityChanged.Code))
				.OrderBy(log => log.SL_EventTime)
				.Select(log => log.SL_Reference.ToString())
				.ToArray();
		}

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartment.PK);

			workflow = BMSTestHelper.CreateWorkflowAndTask(Factory, completionStatement: "workflow", sequence: 1, description: "task1");
			task1 = workflow.Tasks.First();
		}

		ProcessHeader workflow;
		ProcessTask task1;

		static readonly int lowerTaskSequence = 10;
		static readonly int higherTaskSequence = 11;

		readonly int updatedLowerTaskSequence;
		readonly int updatedHigherTaskSequence = 100;

		static readonly StartabilityTaskInfoForTest openLowerTask = new StartabilityTaskInfoForTest() { Status = ProcessTaskStatusCodeList.Codes.Assigned, Sequence = lowerTaskSequence };
		static readonly StartabilityTaskInfoForTest openHigherTask = new StartabilityTaskInfoForTest() { Status = ProcessTaskStatusCodeList.Codes.Assigned, Sequence = higherTaskSequence };
		static readonly StartabilityTaskInfoForTest closedLowerTask = new StartabilityTaskInfoForTest() { Status = ProcessTaskStatusCodeList.Codes.Closed, Sequence = lowerTaskSequence };
		static readonly StartabilityTaskInfoForTest closedHigherTask = new StartabilityTaskInfoForTest() { Status = ProcessTaskStatusCodeList.Codes.Closed, Sequence = higherTaskSequence };

		enum ExpectedTaskLog { None, Startable, NonStartable }

		readonly StartabilityTasksTestCase[] taskStatusAndSequenceSRTEventTestCases = new StartabilityTasksTestCase[]
		{
			new StartabilityTasksTestCase(givenTask: openHigherTask, whenUpdateTaskTo: openHigherTask, thenTaskLog: ExpectedTaskLog.None, thenOtherTaskLog: ExpectedTaskLog.None, message: "open-higher-task is updated to higher-sequence"),
			new StartabilityTasksTestCase(openHigherTask, openLowerTask, ExpectedTaskLog.Startable, ExpectedTaskLog.NonStartable, "open-higher-task is updated to lower-sequence"),
			new StartabilityTasksTestCase(openHigherTask, closedHigherTask, ExpectedTaskLog.None, ExpectedTaskLog.None, "open-higher-task is closed to higher sequence"),
			new StartabilityTasksTestCase(openHigherTask, closedLowerTask, ExpectedTaskLog.None, ExpectedTaskLog.None, "open-higher-task is closed to lower sequence"),

			new StartabilityTasksTestCase(openLowerTask, openHigherTask, ExpectedTaskLog.NonStartable, ExpectedTaskLog.Startable, "open-lower-task is updated to higher-sequence"),
			new StartabilityTasksTestCase(openLowerTask, openLowerTask, ExpectedTaskLog.None, ExpectedTaskLog.None, "open-lower-task is updated to lower-sequence"),
			new StartabilityTasksTestCase(openLowerTask, closedHigherTask, ExpectedTaskLog.None, ExpectedTaskLog.Startable, "open-lower-task is closed to higher-sequence"),
			new StartabilityTasksTestCase(openLowerTask, closedLowerTask, ExpectedTaskLog.None, ExpectedTaskLog.Startable, "open-lower-task is closed to lower-sequence"),

			new StartabilityTasksTestCase(closedHigherTask, openHigherTask, ExpectedTaskLog.None, ExpectedTaskLog.None, "closed-higher-task is re-opened"),
			new StartabilityTasksTestCase(closedHigherTask, openLowerTask, ExpectedTaskLog.Startable, ExpectedTaskLog.NonStartable, "closed-higher-task is re-opened to lower-sequence"),
			new StartabilityTasksTestCase(closedHigherTask, closedHigherTask, ExpectedTaskLog.None, ExpectedTaskLog.None, "closed-higher-task is closed to higher-sequence"),
			new StartabilityTasksTestCase(closedHigherTask, closedLowerTask, ExpectedTaskLog.None, ExpectedTaskLog.None, "closed-higher-task is closed to lower-sequence"),

			new StartabilityTasksTestCase(closedLowerTask, openHigherTask, ExpectedTaskLog.None, ExpectedTaskLog.None, "lower-closed-task is re-opened to higher-sequence"),
			new StartabilityTasksTestCase(closedLowerTask, openLowerTask, ExpectedTaskLog.Startable, ExpectedTaskLog.NonStartable, "lower-closed-task is re-opened"),
			new StartabilityTasksTestCase(closedLowerTask, closedHigherTask, ExpectedTaskLog.None, ExpectedTaskLog.None, "lower-closed-task is closed to higher-sequence"),
			new StartabilityTasksTestCase(closedLowerTask, closedLowerTask, ExpectedTaskLog.None, ExpectedTaskLog.None, "lower-closed-task is closed to lower-sequence"),
		};

		class StartabilityTaskInfoForTest
		{
			public string Status { get; set; }
			public int Sequence { get; set; }
		}

		class StartabilityTasksTestCase
		{
			public StartabilityTaskInfoForTest GivenTask { get; set; }
			public StartabilityTaskInfoForTest WhenUpdateTaskTo { get; set; }

			public ExpectedTaskLog ThenTaskLog { get; set; }
			public ExpectedTaskLog ThenOtherTaskLog { get; set; }

			public string Message { get; set; }

			public StartabilityTasksTestCase(StartabilityTaskInfoForTest givenTask, StartabilityTaskInfoForTest whenUpdateTaskTo, ExpectedTaskLog thenTaskLog, ExpectedTaskLog thenOtherTaskLog, string message)
			{
				GivenTask = givenTask;
				WhenUpdateTaskTo = whenUpdateTaskTo;
				ThenTaskLog = thenTaskLog;
				ThenOtherTaskLog = thenOtherTaskLog;
				Message = message;
			}
		}

		#endregion
	}
}
