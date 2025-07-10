using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.PAVE.Common.DTO;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Service.Shared.Task.Dtos.Lifecycle;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Service.Test
{
	[TestClass]
	public class TaskLifecycleService_Complete_Test : TestCaseWithFactory
	{
		#region Setup and Helpers

		TaskLifecycleService taskLifecycleService;
		bool IsUserInteractive;

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();
			IsUserInteractive = Globals.IsUserInteractive;
			Globals.IsUserInteractive = false;
			TestConfigsHelper.CreateSchematicTestConfig(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			taskLifecycleService = new TaskLifecycleService();
		}

		protected override void TearDown()
		{
			Globals.IsUserInteractive = IsUserInteractive;
			MasterFilesTestHelper.ClearIterationReasonsFromRegistry(WorkflowDescriptors.DummyWorkflowDescriptorCode);
			base.TearDown();
		}

		ProcessTask CreateWorkflowAndTask(string staffCode = "")
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow");
			var task = BMSTestHelper.CreateTask(workflow, description: "Task", staffCode: staffCode);

			return task;
		}

		#endregion

		public void Test_TryCheckBeforeComplete_WithActualDuration_NotRequired()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";

			var task = CreateWorkflowAndTask(staff.GS_Code);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			task.P9_ActualDuration = TimeSpan.FromMinutes(60);

			Factory.Save();

			var result = taskLifecycleService.TryCheckBeforeComplete(task.PK.ToGuid(), out var taskCompletionCheckResponse, out _);

			Assert("Should work", result);
			Assert(!taskCompletionCheckResponse.IsActualDurationRequired);
			AssertEquals(taskCompletionCheckResponse.ActualDurationInMinutes, 60);
			Assert(!taskCompletionCheckResponse.IsContainmentBarrierAnswerRequired);
			AssertEquals(taskCompletionCheckResponse.ContainmentBarrierDetails, null);
		}

		public void Test_TryCheckBeforeComplete_WithActualDuration_Required()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";

			var task = CreateWorkflowAndTask(staff.GS_Code);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			task.P9_ActualDuration = TimeSpan.FromMinutes(60);

			Factory.Save();

			var registryItem = new WorkflowTaskTypesRegistryItem("ProcessManagerTaskTypes",
				RawDataRegistry.Categories.WorkflowManager,
				null,
				null,
				RegistryStorageFlags.System | RegistryStorageFlags.Company);

			var collection = registryItem.Value;
			var workflowTaskTypes = collection.GetTaskTypesFromWorkflowCode(task.WorkflowType);
			var taskType = workflowTaskTypes.OfType<WorkflowTaskType>().SingleOrDefault(w => w.Code == task.P9_Type) ?? workflowTaskTypes.AddNew();
			taskType.Code = task.P9_Type;
			taskType.IsRequireActualDuration = true;

			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var result = taskLifecycleService.TryCheckBeforeComplete(task.PK.ToGuid(), out var taskCompletionCheckResponse, out var businessResponse);

			Assert("Should work", result);
			Assert(taskCompletionCheckResponse.IsActualDurationRequired);
			AssertEquals(taskCompletionCheckResponse.ActualDurationInMinutes, 60);
			Assert(!taskCompletionCheckResponse.IsContainmentBarrierAnswerRequired);
			AssertEquals(taskCompletionCheckResponse.ContainmentBarrierDetails, null);
		}

		public void Test_TryComplete_WithActualDuration_Required_ShouldFail()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";

			var task = CreateWorkflowAndTask(staff.GS_Code);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			task.P9_ActualDuration = TimeSpan.FromMinutes(60);

			Factory.Save();

			var registryItem = new WorkflowTaskTypesRegistryItem("ProcessManagerTaskTypes",
				RawDataRegistry.Categories.WorkflowManager,
				null,
				null,
				RegistryStorageFlags.System | RegistryStorageFlags.Company);

			var collection = registryItem.Value;
			var workflowTaskTypes = collection.GetTaskTypesFromWorkflowCode(task.WorkflowType);
			var taskType = workflowTaskTypes.OfType<WorkflowTaskType>().SingleOrDefault(w => w.Code == task.P9_Type) ?? workflowTaskTypes.AddNew();
			taskType.Code = task.P9_Type;
			taskType.IsRequireActualDuration = true;

			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var result = taskLifecycleService.TryComplete(task.PK.ToGuid(), new CompleteTaskRequest() { ActualDurationInMinutes = null, ContainmentBarrierAnswer = null }, out var businessResponse);

			Assert("Should not work", !result);
			AssertEquals(TaskService.BusinessMessages.ActualDurationIsMandatoryForThisTask.Text, businessResponse.Message.Text);
		}

		public void Test_TryCheckBeforeComplete_WithContainmentBarrier_Required()
		{
			MasterFilesTestHelper.AddIterationReasonToRegistry(WorkflowDescriptors.DummyWorkflowDescriptorCode, "01", "Oh NO");
			BMSTestHelper.AddTaskTypesToRegistry(WorkflowDescriptors.DummyWorkflowDescriptorCode, "COD", "QCB");
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("QCB", WorkflowDescriptors.DummyWorkflowDescriptorCode);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";

			var task = CreateWorkflowAndTask(staff.GS_Code);
			task.P9_Type = "QCB";

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			task.P9_ActualDuration = TimeSpan.FromMinutes(60);

			Factory.Save();

			var result = taskLifecycleService.TryCheckBeforeComplete(task.PK.ToGuid(), out var taskCompletionCheckResponse, out _);

			Assert("Should work", result);

			Assert(taskCompletionCheckResponse.IsContainmentBarrierAnswerRequired);
			AssertNotNull(taskCompletionCheckResponse.ContainmentBarrierDetails);
		}

		[TestDateIncremental(seconds: 1)]
		public void Test_TryCheckBeforeComplete_ShouldReturnAnswerRequired_WhenIterationIsRequired()
		{
			MasterFilesTestHelper.AddIterationReasonToRegistry(WorkflowDescriptors.DummyWorkflowDescriptorCode, "01", "Oh NO");
			BMSTestHelper.AddTaskTypesToRegistry(WorkflowDescriptors.DummyWorkflowDescriptorCode, "CDU", "CDF");
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("QCB", WorkflowDescriptors.DummyWorkflowDescriptorCode);

			var staffUnderReview = BMSTestHelper.CreateStaff(Factory, "STU", "StaffUnderReview");
			var staffReviewer = BMSTestHelper.CreateStaff(Factory, "STR", "StaffReviewer");

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow");

			var cduTask = BMSTestHelper.CreateTask(workflow, staffUnderReview.GS_Code, taskType: "CDU", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, description: "Coding Tests", sequence: 1);
			var cdfTask = BMSTestHelper.CreateTask(workflow, staffUnderReview.GS_Code, taskType: "CDF", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, description: "Coding functionality", sequence: 2);
			var qcbTask = BMSTestHelper.CreateTask(workflow, staffReviewer.GS_Code, taskType: "QCB", taskStatus: ProcessTaskStatusCodeList.Codes.Working, description: "Review", sequence: 3);

			Factory.Save();

			var dateTimeBeforeCallTryChangeStatus = ZDateTime.Now;
			var result = taskLifecycleService.TryCheckBeforeComplete(qcbTask.PK.ToGuid(), out var taskCompletionCheckResponse, out var businessResponse);

			var logs = qcbTask.GetLogs().Find(log => log.SL_EventTime > dateTimeBeforeCallTryChangeStatus);

			CombineAssertions(() =>
			{
				Assert("Should work", result);
				Assert(taskCompletionCheckResponse.IsContainmentBarrierAnswerRequired);
				AssertNotNull(taskCompletionCheckResponse.ContainmentBarrierDetails);
				AssertEquals("Should not have any Log", 0, logs.Count());
				AssertEquals("Task Status should be Open", ProcessTaskStatusCodeList.Codes.Working, qcbTask.P9_Status);

				AssertNotNull(taskCompletionCheckResponse.ContainmentBarrierDetails.Reasons);
				AssertNotNull(taskCompletionCheckResponse.ContainmentBarrierDetails.ResourcesUnderReview);
				AssertContainsExactElementsInAnyOrder("containmentBarrierRequired should have the registry iterations reasons", [new KeyValuePair<string, string>("01", "Oh NO")], taskCompletionCheckResponse.ContainmentBarrierDetails.Reasons.ToList());
				AssertContainsExactElementsInAnyOrder("containmentBarrierRequired should have the ResourcesUnderReview", [staffUnderReview.PK.ToGuid()], taskCompletionCheckResponse.ContainmentBarrierDetails.ResourcesUnderReview.Select(staff => staff.PK));

				AssertNotNull(taskCompletionCheckResponse.ContainmentBarrierDetails.Tasks);

				var containmentBarrierTasks = taskCompletionCheckResponse.ContainmentBarrierDetails.Tasks != null ? taskCompletionCheckResponse.ContainmentBarrierDetails.Tasks.ToArray() : [];

				TaskServiceTest.AssertContainmentBarrierTask(cduTask, containmentBarrierTasks[0]);
				TaskServiceTest.AssertContainmentBarrierTask(cdfTask, containmentBarrierTasks[1]);
				TaskServiceTest.AssertContainmentBarrierTask(qcbTask, containmentBarrierTasks[2]);
			});
		}

		[TestDateIncremental(seconds: 1)]
		public void Test_TryCheckBeforeComplete_ContainmentBarrierTaskShouldAppearLast_WhenAContainmentBarrierTaskAndOtherTasksHaveTheSameSequence()
		{
			MasterFilesTestHelper.AddIterationReasonToRegistry(WorkflowDescriptors.DummyWorkflowDescriptorCode, "01", "Oh NO");
			BMSTestHelper.AddTaskTypesToRegistry(WorkflowDescriptors.DummyWorkflowDescriptorCode, "COD", "QCB");
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("QCB", WorkflowDescriptors.DummyWorkflowDescriptorCode);

			var staffUnderReview = BMSTestHelper.CreateStaff(Factory, "STU", "StaffUnderReview");
			var staffReviewer = BMSTestHelper.CreateStaff(Factory, "STR", "StaffReviewer");

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow");

			BMSTestHelper.CreateTask(workflow, staffUnderReview.GS_Code, taskType: "CDU", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, description: "Coding Tests", sequence: 2);
			BMSTestHelper.CreateTask(workflow, staffUnderReview.GS_Code, taskType: "CDF", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, description: "Coding functionality", sequence: 2);
			BMSTestHelper.CreateTask(workflow, staffUnderReview.GS_Code, taskType: "CDF", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, description: "Coding functionality 1", sequence: 2);
			var reviewQcbTask = BMSTestHelper.CreateTask(workflow, staffReviewer.GS_Code, taskType: "QCB", taskStatus: ProcessTaskStatusCodeList.Codes.Working, description: "Review", sequence: 2);

			Factory.Save();

			var result = taskLifecycleService.TryCheckBeforeComplete(reviewQcbTask.PK.ToGuid(), out var taskCompletionCheckResponse, out var businessResponse);

			reviewQcbTask.Reload();

			var containmentBarrierTasks = taskCompletionCheckResponse.ContainmentBarrierDetails.Tasks != null ? taskCompletionCheckResponse.ContainmentBarrierDetails.Tasks.ToArray() : [];

			CombineAssertions("The review task should always be the last task in the set", () =>
			{
				AssertEquals(4, containmentBarrierTasks.Length);
				TaskServiceTest.AssertContainmentBarrierTask(reviewQcbTask, containmentBarrierTasks.Last());
			});
		}

		public void TestTryComplete_ShouldFailWith_AnswerRequired_When_TaskIsContainmentBarrier()
		{
			MasterFilesTestHelper.AddIterationReasonToRegistry(WorkflowDescriptors.DummyWorkflowDescriptorCode, "01", "Oh NO");
			BMSTestHelper.AddTaskTypesToRegistry(WorkflowDescriptors.DummyWorkflowDescriptorCode, "COD", "QCB");
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("QCB", WorkflowDescriptors.DummyWorkflowDescriptorCode);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow");
			var task = BMSTestHelper.CreateTask(workflow, description: "Task", taskType: "QCB", staffCode: staff.GS_Code);

			Factory.Save();

			var result = taskLifecycleService.TryComplete(task.PK.ToGuid(), new CompleteTaskRequest(), out var businessResponse);

			Assert("Should not return Success", !result);
			AssertEquals(TaskLifecycleService.BusinessMessages.ContainmentBarrierAnswerRequiredToCompleteTask.Text, businessResponse.Message.Text);
		}

		public void Test_TryComplete_ShouldFailWhen_Task_HasNoStaff()
		{
			var task = CreateWorkflowAndTask();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task.P9_GS_NKAssignedStaffMember = null;

			Factory.Save();

			var result = taskLifecycleService.TryComplete(task.PK.ToGuid(), new CompleteTaskRequest(), out var businessResponse);

			task.Reload();

			Assert("Should not work", !result);
			AssertEquals(businessResponse.Message.Text, TaskLifecycleService.BusinessMessages.TaskNeedStaffToBeCompleted.Text);
		}

		public void Test_TryComplete_ShouldFailWhen_StatusIsNot_WorkingOrSuspendedOrAssigned()
		{
			var task = CreateWorkflowAndTask();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task.P9_GS_NKAssignedStaffMember = null;

			Factory.Save();

			var result = taskLifecycleService.TryComplete(task.PK.ToGuid(), new CompleteTaskRequest(), out var businessResponse);

			task.Reload();

			Assert("Should not work", !result);
			AssertEquals(businessResponse.Message.Text, TaskLifecycleService.BusinessMessages.TaskNeedStaffToBeCompleted.Text);
		}

		[TestDateIncremental(seconds: 1)]
		public void Test_TryComplete_ShouldWork_WhenCorrectAnswerIsProvided()
		{
			MasterFilesTestHelper.AddIterationReasonToRegistry(WorkflowDescriptors.DummyWorkflowDescriptorCode, "AA", "Cause I Want!");
			BMSTestHelper.AddTaskTypesToRegistry(WorkflowDescriptors.DummyWorkflowDescriptorCode, "CDF");
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("QCB", WorkflowDescriptors.DummyWorkflowDescriptorCode);

			var staffUnderReview = BMSTestHelper.CreateStaff(Factory, "STU", "StaffUnderReview");
			var staffReviewer = BMSTestHelper.CreateStaff(Factory, "STR", "StaffReviewer");

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "WF");
			var cdfTask = BMSTestHelper.CreateTask(workflow, staffUnderReview.GS_Code, taskType: "CDF", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, description: "Coding functionality", sequence: 2);
			var qcbTask = BMSTestHelper.CreateTask(workflow, staffReviewer.GS_Code, taskType: "QCB", taskStatus: ProcessTaskStatusCodeList.Codes.Working, description: "Review", sequence: 2);

			Factory.Save();

			var answer = new ContainmentBarrierRequestDTO()
			{
				ReasonCode = "aa",
				ResourceCodeUnderReview = staffUnderReview.GS_Code,
				Response = ContainmentBarrierResponse.IterationRequired,
				SelectedTaskPKs = [cdfTask.PK.ToGuid(), qcbTask.PK.ToGuid()],
			};

			var dateTimeBeforeCallTryChangeStatus = ZDateTime.Now;

			var result = taskLifecycleService.TryComplete(qcbTask.PK.ToGuid(), new CompleteTaskRequest() { ActualDurationInMinutes = 60, ContainmentBarrierAnswer = answer }, out var businessResponse);

			var logs = qcbTask.GetLogs().Find(log => log.SL_EventTime > dateTimeBeforeCallTryChangeStatus);

			qcbTask.Reload();

			workflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);

			CombineAssertions(() =>
			{
				Assert("Should work", result);
				AssertNull("Should return no validationErrors", businessResponse);
				AssertEquals("Should only have one StatusChange Log", 1, logs.Count(log => log.SL_SE_NKEvent == Events.StatusChange.Code));
				AssertEquals("Review Task Status should be Closed", ProcessTaskStatusCodeList.Codes.Closed, qcbTask.P9_Status);

				var workflowChildLink = workflow.ChildLinks.FirstOrDefault();
				AssertNotNull("Workflow should have an child link", workflowChildLink);
				var iterationWorkflow = workflowChildLink.HeaderFrom;
				AssertContains("Child workflow should be the Iteration one", "WF (Quality Iteration 1)", iterationWorkflow.Description);
				AssertEquals("Iteration workflow should have two new tasks", 2, iterationWorkflow.Tasks.Count(task => task.P9_Status == ProcessTaskStatusCodeList.Codes.Assigned));
			});
		}
	}
}
