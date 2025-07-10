using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Service.Shared.Task.Dtos.Lifecycle;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Service.Test
{
	[TestClass]
	public class TaskLifecycleServiceTest : TestCaseWithFactory
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
			TestConfigsHelper.CreateSchematicTestConfig(Factory);
			taskLifecycleService = new TaskLifecycleService();
		}

		protected override void TearDown()
		{
			Globals.IsUserInteractive = IsUserInteractive;
			MasterFilesTestHelper.ClearIterationReasonsFromRegistry("ORG");
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

		#region Cancel

		public void Test_Cancel_NonExistingTask_ShouldReturnFalse()
		{
			var result = taskLifecycleService.TryCancel(Guid.NewGuid(), new CancelTaskRequest(), out _);
			Assert(!result);
		}

		[TestDateIncremental(seconds: 1)]
		public void Test_Cancel_Task_ShouldWork()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";

			var task = CreateWorkflowAndTask(staff.GS_Code);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			Factory.Save();

			var result = taskLifecycleService.TryCancel(task.PK.ToGuid(), new CancelTaskRequest(), out _);

			task.Reload();

			Assert("Should work", result);
			AssertEquals("New Task Status should be Cancelled", ProcessTaskStatusCodeList.Codes.Cancelled, task.P9_Status);
		}

		[TestDateIncremental(seconds: 1)]
		public void Test_Cancel_Task_ShouldFail_When_TaskIsAlreadyCancelled()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";

			var task = CreateWorkflowAndTask(staff.GS_Code);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			Factory.Save();

			var result = taskLifecycleService.TryCancel(task.PK.ToGuid(), new CancelTaskRequest(), out var businessResponse);

			task.Reload();

			Assert("Should not work", !result);
			AssertEquals("Task is already cancelled", TaskLifecycleService.BusinessMessages.TaskIsAlreadyCancelled.Text, businessResponse.Message.Text);
		}

		[TestDateIncremental(seconds: 1)]
		public void Test_Cancel_Task_ShouldFail_When_TaskIsClosed()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";

			var task = CreateWorkflowAndTask(staff.GS_Code);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();

			var result = taskLifecycleService.TryCancel(task.PK.ToGuid(), new CancelTaskRequest(), out var businessResponse);

			task.Reload();

			Assert("Should not work", !result);
			AssertEquals("Task is completed", TaskLifecycleService.BusinessMessages.ClosedTaskCantBeCancelled.Text, businessResponse.Message.Text);
		}

		#endregion

		#region Start

		public void Test_Start_Task()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";

			var task = CreateWorkflowAndTask(staff.GS_Code);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			Factory.Save();

			var result = taskLifecycleService.TryStart(task.PK.ToGuid(), new StartTaskRequest(), out _);

			task.Reload();

			Assert("Should work", result);
			AssertEquals("Task is started", ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
		}

		public void Test_Start_Task_ShouldFail_With_Suspended_Status()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";

			var task = CreateWorkflowAndTask(staff.GS_Code);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;

			Factory.Save();

			var result = taskLifecycleService.TryStart(task.PK.ToGuid(), new StartTaskRequest(), out var businessResponse);

			task.Reload();

			Assert("Should not work", !result);
			AssertEquals("Incorrect reason to not start", businessResponse.Message.Text, TaskLifecycleService.BusinessMessages.OnlyAssignedTasksCanBeStarted.Text);
		}

		public void Test_Start_Task_ShouldFail_With_Working_Status()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";

			var task = CreateWorkflowAndTask(staff.GS_Code);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			Factory.Save();

			var result = taskLifecycleService.TryStart(task.PK.ToGuid(), new StartTaskRequest(), out var businessResponse);

			task.Reload();

			Assert("Should not work", !result);
			AssertEquals("Incorrect reason to not start", businessResponse.Message.Text, TaskLifecycleService.BusinessMessages.OnlyAssignedTasksCanBeStarted.Text);
		}

		public void Test_Start_Task_ShouldFail_With_Closed_Status()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";

			var task = CreateWorkflowAndTask(staff.GS_Code);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();

			var result = taskLifecycleService.TryStart(task.PK.ToGuid(), new StartTaskRequest(), out var businessResponse);

			task.Reload();

			Assert("Should not work", !result);
			AssertEquals("Incorrect reason to not start", businessResponse.Message.Text, TaskLifecycleService.BusinessMessages.OnlyAssignedTasksCanBeStarted.Text);
		}

		public void Test_Start_Task_ShouldFail_With_Cancelled_Status()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";

			var task = CreateWorkflowAndTask(staff.GS_Code);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			Factory.Save();

			var result = taskLifecycleService.TryStart(task.PK.ToGuid(), new StartTaskRequest(), out var businessResponse);

			task.Reload();

			Assert("Should not work", !result);
			AssertEquals("Incorrect reason to not start", businessResponse.Message.Text, TaskLifecycleService.BusinessMessages.OnlyAssignedTasksCanBeStarted.Text);
		}

		public void Test_Start_ShouldFailNonExistingTask_ShouldReturnFalse()
		{
			var result = taskLifecycleService.TryStart(Guid.NewGuid(), new StartTaskRequest(), out _);
			Assert(!result);
		}

		#endregion

		#region Suspend

		public void Test_Suspend_Task()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";

			var task = CreateWorkflowAndTask(staff.GS_Code);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			Factory.Save();

			var result = taskLifecycleService.TrySuspend(task.PK.ToGuid(), new SuspendTaskRequest(), out _);

			task.Reload();

			Assert("Should work", result);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Suspended, task.P9_Status);
		}

		public void Test_Suspend_Task_ShouldFail_With_Assigned_Status()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";

			var task = CreateWorkflowAndTask(staff.GS_Code);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			Factory.Save();

			var result = taskLifecycleService.TrySuspend(task.PK.ToGuid(), new SuspendTaskRequest(), out var businessResponse);

			task.Reload();

			Assert("Should not work", !result);
			AssertEquals(businessResponse.Message.Text, TaskLifecycleService.BusinessMessages.OnlyWorkingTasksCanBeSuspended.Text);
		}

		public void Test_Suspend_Task_ShouldFail_With_Closed_Status()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";

			var task = CreateWorkflowAndTask(staff.GS_Code);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();

			var result = taskLifecycleService.TrySuspend(task.PK.ToGuid(), new SuspendTaskRequest(), out var businessResponse);

			task.Reload();

			Assert("Should not work", !result);
			AssertEquals(businessResponse.Message.Text, TaskLifecycleService.BusinessMessages.OnlyWorkingTasksCanBeSuspended.Text);
		}

		public void Test_Suspend_Task_ShouldFail_With_Cancelled_Status()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";

			var task = CreateWorkflowAndTask(staff.GS_Code);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			Factory.Save();

			var result = taskLifecycleService.TrySuspend(task.PK.ToGuid(), new SuspendTaskRequest(), out var businessResponse);

			task.Reload();

			Assert("Should not work", !result);
			AssertEquals(businessResponse.Message.Text, TaskLifecycleService.BusinessMessages.OnlyWorkingTasksCanBeSuspended.Text);
		}

		public void Test_Suspend_NonExistingTask_ShouldReturnFalse()
		{
			var result = taskLifecycleService.TrySuspend(Guid.NewGuid(), new SuspendTaskRequest(), out _);
			Assert(!result);
		}

		#endregion

		#region Resume

		public void Test_Resume_Task()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";

			var task = CreateWorkflowAndTask(staff.GS_Code);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;

			Factory.Save();

			var result = taskLifecycleService.TryResume(task.PK.ToGuid(), new ResumeTaskRequest(), out _);

			task.Reload();

			Assert("Should work", result);
			AssertEquals("Task is completed", ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
		}

		public void Test_Resume_Task_ShouldFail_With_Assigned_Status()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";

			var task = CreateWorkflowAndTask(staff.GS_Code);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			Factory.Save();

			var result = taskLifecycleService.TryResume(task.PK.ToGuid(), new ResumeTaskRequest(), out var businessResponse);

			task.Reload();

			Assert("Should not work", !result);
			AssertEquals("Incorrect reason to not start", businessResponse.Message.Text, TaskLifecycleService.BusinessMessages.OnlySuspendedTasksCanBeResumed.Text);
		}

		public void Test_Resume_Task_ShouldFail_With_Working_Status()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";

			var task = CreateWorkflowAndTask(staff.GS_Code);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			Factory.Save();

			var result = taskLifecycleService.TryResume(task.PK.ToGuid(), new ResumeTaskRequest(), out var businessResponse);

			task.Reload();

			Assert("Should not work", !result);
			AssertEquals("Incorrect reason to not start", businessResponse.Message.Text, TaskLifecycleService.BusinessMessages.OnlySuspendedTasksCanBeResumed.Text);
		}

		public void Test_Resume_Task_ShouldFail_With_Closed_Status()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";

			var task = CreateWorkflowAndTask(staff.GS_Code);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();

			var result = taskLifecycleService.TryResume(task.PK.ToGuid(), new ResumeTaskRequest(), out var businessResponse);

			task.Reload();

			Assert("Should not work", !result);
			AssertEquals("Incorrect reason to not start", businessResponse.Message.Text, TaskLifecycleService.BusinessMessages.OnlySuspendedTasksCanBeResumed.Text);
		}

		public void Test_Resume_Task_ShouldFail_With_Cancelled_Status()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";

			var task = CreateWorkflowAndTask(staff.GS_Code);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			Factory.Save();

			var result = taskLifecycleService.TryResume(task.PK.ToGuid(), new ResumeTaskRequest(), out var businessResponse);

			task.Reload();

			Assert("Should not work", !result);
			AssertEquals("Incorrect reason to not start", businessResponse.Message.Text, TaskLifecycleService.BusinessMessages.OnlySuspendedTasksCanBeResumed.Text);
		}

		public void Test_Resume_NonExistingTask_ShouldReturnFalse()
		{
			var result = taskLifecycleService.TryResume(Guid.NewGuid(), new ResumeTaskRequest(), out _);
			Assert(!result);
		}

		#endregion

		#region Reopen

		public void Test_ReopenAsWorking_Cancelled_Task()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";

			var task = CreateWorkflowAndTask(staff.GS_Code);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			Factory.Save();

			var result = taskLifecycleService.TryReopen(task.PK.ToGuid(), new ReopenTaskRequest { NewStatus = nameof(ProcessTaskStatusCodeList.Codes.Working) }, out _);

			task.Reload();

			Assert("Should work", result);
			AssertEquals("Task is completed", ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
		}

		public void Test_ReopenAsWorking_Closed_Task()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";

			var task = CreateWorkflowAndTask(staff.GS_Code);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();

			var result = taskLifecycleService.TryReopen(task.PK.ToGuid(), new ReopenTaskRequest { NewStatus = nameof(ProcessTaskStatusCodeList.Codes.Working) }, out _);

			task.Reload();

			Assert("Should work", result);
			AssertEquals("Task is completed", ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
		}

		public void Test_Reopen_With_Incorrect_Code()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";

			var task = CreateWorkflowAndTask(staff.GS_Code);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();

			var result = taskLifecycleService.TryReopen(task.PK.ToGuid(), new ReopenTaskRequest(), out var businessResponse);

			task.Reload();

			Assert("Should not work", !result);
			AssertEquals("Incorrect reason to not start", businessResponse.Message.Text, TaskLifecycleService.BusinessMessages.IncorrectReopenStatus.Text);
		}

		public void Test_ReopenAsWorking_Task_ShouldFail_With_Assigned_Status()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";

			var task = CreateWorkflowAndTask(staff.GS_Code);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			Factory.Save();

			var result = taskLifecycleService.TryReopen(task.PK.ToGuid(), new ReopenTaskRequest(), out var businessResponse);

			task.Reload();

			Assert("Should not work", !result);
			AssertEquals("Incorrect reason to not start", businessResponse.Message.Text, TaskLifecycleService.BusinessMessages.OnlyClosedOrCancelledTasksCanBeReopened.Text);
		}

		public void Test_ReopenAsWorking_Task_ShouldFail_With_Working_Status()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";

			var task = CreateWorkflowAndTask(staff.GS_Code);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			Factory.Save();

			var result = taskLifecycleService.TryReopen(task.PK.ToGuid(), new ReopenTaskRequest(), out var businessResponse);

			task.Reload();

			Assert("Should not work", !result);
			AssertEquals("Incorrect reason to not start", businessResponse.Message.Text, TaskLifecycleService.BusinessMessages.OnlyClosedOrCancelledTasksCanBeReopened.Text);
		}

		public void Test_ReopenAsWorking_Task_ShouldFail_With_Suspended_Status()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";

			var task = CreateWorkflowAndTask(staff.GS_Code);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;

			Factory.Save();

			var result = taskLifecycleService.TryReopen(task.PK.ToGuid(), new ReopenTaskRequest(), out var businessResponse);

			task.Reload();

			Assert("Should not work", !result);
			AssertEquals("Incorrect reason to not start", businessResponse.Message.Text, TaskLifecycleService.BusinessMessages.OnlyClosedOrCancelledTasksCanBeReopened.Text);
		}

		public void Test_ReopenAsWorking_NonExistingTask_ShouldReturnFalse()
		{
			var result = taskLifecycleService.TryReopen(Guid.NewGuid(), new ReopenTaskRequest(), out _);
			Assert(!result);
		}

		#endregion

		#region Assign

		public void Test_Assign_Task_ShouldChangeAssignStaffCode()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";

			var task = CreateWorkflowAndTask();
			Factory.Save();

			var result = taskLifecycleService.TryAssign(task.PK.ToGuid(), new AssignTaskRequest
			{
				StaffId = staff.PK.ToGuid(),
			}, out _);

			task.Reload();

			Assert("Should work", result);
			AssertEquals("Task is Assigned", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			AssertEquals("Task is Assigned to Staff", staff.GS_Code, task.P9_GS_NKAssignedStaffMember);
		}

		public void Test_Assign_Task_ShouldReassignTaskFromOneStaffToAnother()
		{
			var staffA = Factory.NewWithValidTestData<GlbStaff>();
			staffA.GS_Code = "STA";
			var staffB = Factory.NewWithValidTestData<GlbStaff>();
			staffB.GS_Code = "STB";

			var task = CreateWorkflowAndTask();
			task.P9_GS_NKAssignedStaffMember = staffA.GS_Code;
			Factory.Save();

			var result = taskLifecycleService.TryAssign(task.PK.ToGuid(), new AssignTaskRequest
			{
				StaffId = staffB.PK.ToGuid(),
			}, out _);

			task.Reload();

			Assert("Should work", result);
			AssertEquals("Task is Assigned", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			AssertEquals("Task is Assigned to Staff B", staffB.GS_Code, task.P9_GS_NKAssignedStaffMember);
		}

		public void Test_AssignTask_ShouldFail_Without_Staff()
		{
			var task = CreateWorkflowAndTask();
			Factory.Save();
			var result = taskLifecycleService.TryAssign(task.PK.ToGuid(), new AssignTaskRequest(), out var businessResponse);

			Assert("Should not work", !result);
			AssertEquals(businessResponse.Message.Text, WiseTech.Business.HumanResourcesManagement.BusinessMessages.StaffNotFound.Text);
		}

		public void Test_AssignTask_ShouldFail_WhenTryToAssignToTheCurrentUser()
		{
			var task = CreateWorkflowAndTask();
			Factory.Save();
			var result = taskLifecycleService.TryAssign(task.PK.ToGuid(), new AssignTaskRequest()
			{
				StaffId = GlbStaff.CurrentUser.PK.ToGuid(),
			}, out var businessResponse);

			Assert("Should not work", !result);
			AssertEquals(businessResponse.Message.Text, TaskLifecycleService.BusinessMessages.TaskCantBeAssignToTheCurrentUser.Text);
		}

		#endregion

		#region Claim

		public void Test_ClaimTask_ShouldSetCurrentStaff()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";

			var task = CreateWorkflowAndTask();
			Factory.Save();

			var result = false;

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				result = taskLifecycleService.TryClaim(task.PK.ToGuid(), new ClaimTaskRequest(), out var businessResponse);
			}

			task.Reload();

			Assert("Should work", result);
			AssertEquals("Task is Assigned to Staff", staff.GS_Code, task.P9_GS_NKAssignedStaffMember);
			AssertEquals("Task is started", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
		}

		public void Test_ClaimTask_ShouldFail_WhenTaskIsAlreadyAssignToTheCurrentUser()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";
			var task = CreateWorkflowAndTask();
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
				Factory.Save();

				var result = taskLifecycleService.TryClaim(task.PK.ToGuid(), new ClaimTaskRequest(), out var businessResponse);

				task.Reload();

				Assert("Should not work", !result);
				AssertEquals(businessResponse.Message.Text, TaskLifecycleService.BusinessMessages.TaskIsAlreadyClaimed.Text);
			}
		}

		#endregion

		#region ClaimAndStart

		public void Test_ClaimAndStart_Task_ShouldSetStaffAndTaskStatus()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";

			var task = CreateWorkflowAndTask();
			Factory.Save();

			var result = false;

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				result = taskLifecycleService.TryClaimAndStart(task.PK.ToGuid(), new ClaimAndStartTaskRequest(), out var businessResponse);
			}

			task.Reload();

			Assert("Should work", result);
			AssertEquals("Task is Assigned to Staff", staff.GS_Code, task.P9_GS_NKAssignedStaffMember);
			AssertEquals("Task is started", ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
		}
		#endregion
	}
}
