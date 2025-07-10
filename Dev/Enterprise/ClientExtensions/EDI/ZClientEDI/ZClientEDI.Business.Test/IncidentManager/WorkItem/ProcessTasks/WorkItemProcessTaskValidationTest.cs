using System;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Core;
using WTG.DevTools.Definitions;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	class WorkItemProcessTaskValidationTest : ProcessTaskValidationTest
	{
		public void TestP9_GS_NKAssignedStaffMember()
		{
			WorkItemProcessTask task = ProcessTask as WorkItemProcessTask;

			AssertNoErrors(task.P9_GS_NKAssignedStaffMemberInfo);
			task.P9_GS_NKAssignedStaffMember = "XYZ";
			task.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			task.Validation.ValidateAll();
			AssertHasErrors(task.P9_GS_NKAssignedStaffMemberInfo);
		}

		public void TestNoP9_GS_NKAssignedStaffMember()
		{
			WorkItemProcessTask task = ProcessTask as WorkItemProcessTask;

			AssertNoErrors(task.P9_TypeInfo);
			task.P9_GS_NKAssignedStaffMember = ZString.Empty;
			task.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			task.Validation.ValidateAll();
			AssertHasError(task.P9_TypeInfo, "You cannot have a checkin task assigned to no-one as this will cause build errors with DAT.");
		}

		public void TestP9_GS_NKAssignedStaffMember_DAT()
		{
			var newStaff = Factory.NewWithValidTestData<GlbStaff>();
			var task = ProcessTask as WorkItemProcessTask;
			task.P9_Type = "CH1";
			task.P9_GS_NKAssignedStaffMember = newStaff.GS_Code;
			Factory.Save();

			task.P9_GS_NKAssignedStaffMember = "DAT";
			AssertHasWarning("should have warning if STAFF -> DAT", task.P9_GS_NKAssignedStaffMemberInfo, "Please assign a staff member for the task, so that notification emails can be received.");
		}

		public void TestP9_Notes()
		{
			var task = (WorkItemProcessTask)ProcessTask;

			AssertNoErrors(task.P9_NotesInfo);
			AssertNoErrors(task.P9_GS_NKAssignedStaffMemberInfo);

			task.P9_NotesAsString = "";
			task.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			task.Validation.ValidateAll();

			AssertHasErrors(task.P9_NotesInfo);
			AssertHasErrors(task.P9_GS_NKAssignedStaffMemberInfo);
		}

		public void TestP9_NotesNoPullRequest_ChangingTaskType()
		{
			WorkItemProcessTask task = Factory.NewWithValidTestData<WorkItemProcessTask>();
			task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			task.P9_NotesAsString = "";
			task.RunPreSaveValidation();
			Factory.Save();
			((ILightValidationInternals)task).IsValid = true;
			AssertHasError(task.P9_NotesInfo, "This DAT submission contains no Pull Requests. Please add a Pull Request URL, or change the task type or status.");

			task.P9_Type = EDITaskTypes.TaskCheckin;
			task.RunPreSaveValidation();
			AssertNoErrors(task.P9_NotesInfo);

			task.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			task.RunPreSaveValidation();
			AssertHasError(task.P9_NotesInfo, "This DAT submission contains no Pull Requests. Please add a Pull Request URL, or change the task type or status.");

			task.P9_Type = WorkItemProcessTask.ShelfsetTestTask;
			task.RunPreSaveValidation();
			AssertHasError(task.P9_NotesInfo, "This DAT submission contains no Pull Requests. Please add a Pull Request URL, or change the task type or status.");

			task.P9_Type = WorkItemProcessTask.UATBuildShelfTask;
			task.RunPreSaveValidation();
			AssertHasError(task.P9_NotesInfo, "This DAT submission contains no Pull Requests. Please add a Pull Request URL, or change the task type or status.");

			task.P9_Type = WorkItemProcessTask.AspectOnlyBuildShelfTask;
			task.RunPreSaveValidation();
			AssertHasError(task.P9_NotesInfo, "This DAT submission contains no Pull Requests. Please add a Pull Request URL, or change the task type or status.");
		}

		public void TestP9_NotesNoValidPullRequestLink_For_RTF()
		{
			const string rtfTextForTesting = @"{\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang3081{\fonttbl{\f0\fnil\fcharset0 Microsoft Sans Serif;}}\r\n\viewkind4\uc1\pard\widctlpar\sa200\sl276\slmult1 {\f0\fs24\lang11274{\field{\*\fldinst{HYPERLINK ""https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev/pullrequest""}}{\fldrslt{\ul\cf1\ul https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev/pullrequest}}}}\f0\fs24\lang11274\par\par}";
			WorkItemProcessTask task = Factory.NewWithValidTestData<WorkItemProcessTask>();
			task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task.P9_Type = WorkItemProcessTask.ShelfsetTestTask;
			task.P9_NotesAsString = rtfTextForTesting;
			task.RunPreSaveValidation();
			AssertHasError(task.P9_NotesInfo, "This DAT submission contains no Pull Requests. Please add a Pull Request URL, or change the task type or status.");
		}

		public void TestP9_Description_PullRequest()
		{
			var task = Factory.NewWithValidTestData<WorkItemProcessTask>();
			task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			task.P9_Description = "Shelf Test";
			task.P9_NotesAsString = "http://tfs.wtg.zone:8080/tfs/cargowise/_git/borderwise/pullrequest/1278?_a=overview";
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task.RunPreSaveValidation();
			Factory.Save();
			((ILightValidationInternals)task).IsValid = true;
			AssertNoErrors(task.P9_DescriptionInfo);
		}

		public void TestCheckIsReusedCheckInTask()
		{
			ShelvesetToolsTest.AddCheckInTaskTypes();
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			WorkItemProcessTask checkInTask = workItem.WorkflowItems.AddNew();
			GlbStaff newStaff = Factory.NewWithValidTestData<GlbStaff>();
			checkInTask.P9_GS_NKAssignedStaffMember = newStaff.GS_Code;
			checkInTask.P9_Description = "WI00005135";
			checkInTask.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			checkInTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();
			checkInTask.Validation.ValidateAll();
			AssertNoErrors("Precondition: no errors", checkInTask);

			checkInTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			checkInTask.Validation.ValidateAll();
			AssertHasRowError(checkInTask, "This task has previously been checked in. Please do not reuse.");

			checkInTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			checkInTask.P9_GS_NKAssignedStaffMember = "C";
			checkInTask.Validation.ValidateAll();
			AssertHasRowError(checkInTask, "This task has previously been checked in. Please do not reuse.");

			checkInTask.P9_GS_NKAssignedStaffMember = newStaff.GS_Code;
			checkInTask.P9_Description = "Reusing shelf";
			checkInTask.Validation.ValidateAll();
			AssertHasRowError(checkInTask, "This task has previously been checked in. Please do not reuse.");

			checkInTask.P9_Description = "WI00005135";
			checkInTask.P9_Type = EDITaskTypes.TaskCheckin;
			checkInTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			checkInTask.Validation.ValidateAll();
			AssertHasRowError(checkInTask, "This task has previously been checked in. Please do not reuse.");

			checkInTask.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			checkInTask.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			checkInTask.Validation.ValidateAll();
			AssertNoRowError(checkInTask, "This task has previously been checked in. Please do not reuse.");

			checkInTask.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			checkInTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			checkInTask.Validation.ValidateAll();
			AssertNoErrors(checkInTask);
		}

		public void TestCheckIsReusedShelfsetTestTask()
		{
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			WorkItemProcessTask shelvesetTestTask = workItem.WorkflowItems.AddNew();
			GlbStaff newStaff = Factory.NewWithValidTestData<GlbStaff>();
			shelvesetTestTask.P9_GS_NKAssignedStaffMember = newStaff.GS_Code;
			shelvesetTestTask.P9_Description = "WI00125760";
			shelvesetTestTask.P9_Type = WorkItemProcessTask.ShelfsetTestTask;
			shelvesetTestTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();

			shelvesetTestTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			shelvesetTestTask.Validation.ValidateAll();
			AssertHasRowError(shelvesetTestTask, "This task has previously been shelf tested. Please do not reuse.");

			shelvesetTestTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			shelvesetTestTask.P9_GS_NKAssignedStaffMember = "C";
			shelvesetTestTask.Validation.ValidateAll();
			AssertHasRowError(shelvesetTestTask, "This task has previously been shelf tested. Please do not reuse.");

			shelvesetTestTask.P9_GS_NKAssignedStaffMember = newStaff.GS_Code;
			shelvesetTestTask.P9_Description = "Reusing shelf";
			shelvesetTestTask.Validation.ValidateAll();
			AssertHasRowError(shelvesetTestTask, "This task has previously been shelf tested. Please do not reuse.");

			shelvesetTestTask.P9_Description = "New Description";
			shelvesetTestTask.P9_Type = WorkItemProcessTask.ShelfsetTestTask;
			shelvesetTestTask.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			shelvesetTestTask.Validation.ValidateAll();
			AssertHasRowError(shelvesetTestTask, "This task has previously been shelf tested. Please do not reuse.");

			shelvesetTestTask.P9_Description = "WI00125760";
			shelvesetTestTask.P9_Type = WorkItemProcessTask.ShelfsetTestTask;
			shelvesetTestTask.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			shelvesetTestTask.Validation.ValidateAll();
			AssertNoRowError(shelvesetTestTask, "This task has previously been shelf tested. Please do not reuse.");

			shelvesetTestTask.P9_Description = "WI00125760";
			shelvesetTestTask.P9_Type = WorkItemProcessTask.ShelfsetTestTask;
			shelvesetTestTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			shelvesetTestTask.Validation.ValidateAll();
			Assert("SH0 task should have no row errors", !shelvesetTestTask.HasRowErrors);
		}

		public void TestCheckIsShelfsetTestTask_BecauseWeCanNotChangeItWeIgnoreOtherValidation()
		{
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			WorkItemProcessTask shelvesetTestTask = workItem.WorkflowItems.AddNew();
			var newStaff = Factory.NewWithValidTestData<GlbStaff>();
			var newGroup = Factory.NewWithValidTestData<GlbGroup>();
			shelvesetTestTask.P9_GS_NKAssignedStaffMember = newStaff.GS_Code;
			shelvesetTestTask.P9_Description = "WI00125760";
			shelvesetTestTask.P9_Type = WorkItemProcessTask.ShelfsetTestTask;
			shelvesetTestTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			WorkItemProcessTask invalidShelfSetTask = workItem.WorkflowItems.AddNew();
			invalidShelfSetTask.P9_GS_NKAssignedStaffMember = "";
			invalidShelfSetTask.P9_Description = "";
			invalidShelfSetTask.P9_Type = WorkItemProcessTask.ShelfsetTestTask;
			invalidShelfSetTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();
			shelvesetTestTask.P9_GG_AssignedGroup = newGroup.PK;
			shelvesetTestTask.Validation.ValidateAll();
			AssertNoErrors(shelvesetTestTask);

			invalidShelfSetTask.Validation.ValidateAll();
			AssertNoErrors(invalidShelfSetTask);

			shelvesetTestTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			shelvesetTestTask.Validation.ValidateAll();
			AssertHasRowError(shelvesetTestTask, "This task has previously been shelf tested. Please do not reuse.");
		}

		public void TestJanitorReusedShelfsetTestTask()
		{
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			WorkItemProcessTask shelvesetTestTask = workItem.WorkflowItems.AddNew();
			GlbStaff newStaff = Factory.NewWithValidTestData<GlbStaff>();
			shelvesetTestTask.P9_GS_NKAssignedStaffMember = newStaff.GS_Code;
			shelvesetTestTask.P9_Description = "WI00125760";
			shelvesetTestTask.P9_Type = WorkItemProcessTask.ExperimentalPullRequestTask;
			shelvesetTestTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();

			shelvesetTestTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			shelvesetTestTask.Validation.ValidateAll();
			AssertHasRowError(shelvesetTestTask, "This task has previously been shelf tested. Please do not reuse.");

			shelvesetTestTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			shelvesetTestTask.P9_GS_NKAssignedStaffMember = "C";
			shelvesetTestTask.Validation.ValidateAll();
			AssertHasRowError(shelvesetTestTask, "This task has previously been shelf tested. Please do not reuse.");

			shelvesetTestTask.P9_GS_NKAssignedStaffMember = newStaff.GS_Code;
			shelvesetTestTask.P9_Description = "Reusing shelf";
			shelvesetTestTask.Validation.ValidateAll();
			AssertHasRowError(shelvesetTestTask, "This task has previously been shelf tested. Please do not reuse.");

			shelvesetTestTask.P9_Description = "New Description";
			shelvesetTestTask.P9_Type = WorkItemProcessTask.ExperimentalPullRequestTask;
			shelvesetTestTask.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			shelvesetTestTask.Validation.ValidateAll();
			AssertHasRowError(shelvesetTestTask, "This task has previously been shelf tested. Please do not reuse.");

			shelvesetTestTask.P9_Description = "WI00125760";
			shelvesetTestTask.P9_Type = WorkItemProcessTask.ExperimentalPullRequestTask;
			shelvesetTestTask.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			shelvesetTestTask.Validation.ValidateAll();
			AssertNoRowError(shelvesetTestTask, "This task has previously been shelf tested. Please do not reuse.");

			shelvesetTestTask.P9_Description = "WI00125760";
			shelvesetTestTask.P9_Type = WorkItemProcessTask.ExperimentalPullRequestTask;
			shelvesetTestTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			shelvesetTestTask.Validation.ValidateAll();
			Assert("SH0 task should have no row errors", !shelvesetTestTask.HasRowErrors);
		}

		public void TestCanReuseFailShelfsetTestTask()
		{
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			WorkItemProcessTask shelvesetTestTask = workItem.WorkflowItems.AddNew();
			GlbStaff newStaff = Factory.NewWithValidTestData<GlbStaff>();
			shelvesetTestTask.P9_GS_NKAssignedStaffMember = newStaff.GS_Code;
			shelvesetTestTask.P9_Description = "WI00125760";
			shelvesetTestTask.P9_Type = WorkItemProcessTask.ShelfsetTestTask;
			shelvesetTestTask.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;

			Factory.Save();

			shelvesetTestTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			shelvesetTestTask.Validation.ValidateAll();
			AssertNoRowError(shelvesetTestTask, "This task has previously been shelf tested. Please do not reuse.");
		}

		public void TestDefectCount()
		{
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			WorkItemProcessTask task = workItem.WorkflowItems.AddNew();

			task.DefectCount = 0;
			AssertNoErrors(task.DefectCountInfo);

			task.DefectCount = 1000;
			AssertHasErrors(task.DefectCountInfo);

			task.DefectCount = 999;
			AssertNoErrors(task.DefectCountInfo);
		}

		public void TestP9_DescriptionNonExistingShelf_ChangingTaskTypeAndStatus()
		{
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();

			WorkItemProcessTask shelfTask = workItem.WorkflowItems.AddNew();
			shelfTask.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			shelfTask.P9_NotesAsString = "http://example.com/pullrequest/123";
			shelfTask.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			shelfTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			shelfTask.Validation.ValidateP9_Notes();
			AssertNoError(shelfTask.P9_NotesInfo, "This DAT submission contains no Pull Requests. Please add a Pull Request URL, or change the task type or status.");

			shelfTask.P9_NotesAsString = "Your PR is in another castle.";
			shelfTask.Validation.ValidateP9_Notes();
			AssertHasError(shelfTask.P9_NotesInfo, "This DAT submission contains no Pull Requests. Please add a Pull Request URL, or change the task type or status.");

			shelfTask.P9_Type = EDITaskTypes.TaskCheckin;
			shelfTask.Validation.ValidateP9_Notes();
			AssertNoError(shelfTask.P9_NotesInfo, "This DAT submission contains no Pull Requests. Please add a Pull Request URL, or change the task type or status.");

			shelfTask.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			shelfTask.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			shelfTask.Validation.ValidateP9_Notes();
			AssertNoError(shelfTask.P9_NotesInfo, "This DAT submission contains no Pull Requests. Please add a Pull Request URL, or change the task type or status.");

			shelfTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			shelfTask.Validation.ValidateP9_Notes();
			AssertHasError(shelfTask.P9_NotesInfo, "This DAT submission contains no Pull Requests. Please add a Pull Request URL, or change the task type or status.");
		}

		public void TestDatSubmissionAssignedToNoOne()
		{
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var shelfTask = workItem.WorkflowItems.AddNew();
			shelfTask.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			shelfTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			shelfTask.P9_NotesAsString = "http://example.com/pullrequest/123";
			AssertHasError(shelfTask.P9_GS_NKAssignedStaffMemberInfo, "Please choose a valid user or change the task type.");
		}

		public void TestCheckinWithMissingReviewTaskWhenRegistryItemDisabled()
		{
			var context = new CheckinTaskReviewValidationTestContext(Factory);

			var codingWorkflow = context.CreateWorkflow("Coding");
			var codingTask = context.CreateCodingTask(codingWorkflow);
			var reviewTask = context.CreateReviewTask(codingWorkflow);
			var checkinTask = context.CreateCheckinTask(codingWorkflow);

			checkinTask.P9_Type = "CH0";
			AssertHasError(checkinTask.P9_TypeInfo, WorkItemProcessTaskValidation.IncompleteReviewTaskMessage);

			EDIDataRegistry.Instance.RequireCompletedCodeReviewForCheckin.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			checkinTask.Validation.ValidateAll();
			AssertNoErrors(checkinTask);
		}

		public void TestCheckinWithCancelledReviewTask()
		{
			var context = new CheckinTaskReviewValidationTestContext(Factory);

			var codingWorkflow = context.CreateWorkflow("Coding");
			var codingTask = context.CreateCodingTask(codingWorkflow);
			var reviewTask = context.CreateReviewTask(codingWorkflow, status: ProcessTaskStatusCodeList.Codes.Cancelled);
			var checkinTask = context.CreateCheckinTask(codingWorkflow);

			checkinTask.Validation.ValidateAll();
			AssertNoErrors(checkinTask);

			checkinTask.P9_Type = "CH0";
			AssertHasError(checkinTask.P9_TypeInfo, WorkItemProcessTaskValidation.NoCompletedReviewTaskMessage);

			reviewTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			checkinTask.Validation.ValidateAll();
			AssertNoErrors(checkinTask);
		}

		public void TestCheckinWithCancelledAndClosedReviewTask()
		{
			var context = new CheckinTaskReviewValidationTestContext(Factory);

			var codingWorkflow = context.CreateWorkflow("Coding");
			var codingTask = context.CreateCodingTask(codingWorkflow);
			var reviewTask1 = context.CreateReviewTask(codingWorkflow, status: ProcessTaskStatusCodeList.Codes.Cancelled);
			var reviewTask2 = context.CreateReviewTask(codingWorkflow, status: ProcessTaskStatusCodeList.Codes.Assigned);
			var checkinTask = context.CreateCheckinTask(codingWorkflow);

			checkinTask.Validation.ValidateAll();
			AssertNoErrors(checkinTask);

			checkinTask.P9_Type = "CH0";
			AssertHasError(checkinTask.P9_TypeInfo, WorkItemProcessTaskValidation.IncompleteReviewTaskMessage);

			reviewTask2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			checkinTask.Validation.ValidateAll();
			AssertNoErrors(checkinTask);
		}

		public void TestCheckinWithReviewTask()
		{
			var context = new CheckinTaskReviewValidationTestContext(Factory);

			var codingWorkflow = context.CreateWorkflow("Coding");
			var codingTask = context.CreateCodingTask(codingWorkflow);
			var reviewTask = context.CreateReviewTask(codingWorkflow);
			var checkinTask = context.CreateCheckinTask(codingWorkflow);

			checkinTask.Validation.ValidateAll();
			AssertNoErrors(checkinTask);

			checkinTask.P9_Type = "CH0";
			AssertHasError(checkinTask.P9_TypeInfo, WorkItemProcessTaskValidation.IncompleteReviewTaskMessage);

			reviewTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			checkinTask.Validation.ValidateAll();
			AssertNoErrors(checkinTask);
		}

		public void TestCheckinWithReviewTaskWithHigherSequence()
		{
			var context = new CheckinTaskReviewValidationTestContext(Factory);

			var codingWorkflow = context.CreateWorkflow("Coding");
			var codingTask = context.CreateCodingTask(codingWorkflow);
			var reviewTask = context.CreateReviewTask(codingWorkflow, sequence: 4);
			var checkinTask = context.CreateCheckinTask(codingWorkflow, sequence: 3);

			checkinTask.Validation.ValidateAll();
			AssertNoErrors(checkinTask);

			checkinTask.P9_Type = "CH0";
			AssertHasError(checkinTask.P9_TypeInfo, WorkItemProcessTaskValidation.NoCompletedReviewTaskMessage);

			reviewTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			checkinTask.Validation.ValidateAll();
			AssertHasError(checkinTask.P9_TypeInfo, WorkItemProcessTaskValidation.NoCompletedReviewTaskMessage);

			reviewTask.P9_Sequence = 2;
			checkinTask.Validation.ValidateAll();
			AssertNoErrors(checkinTask);
		}

		public void TestCheckinWithMultipleClosedReviewTasks()
		{
			MasterFilesTestHelper.CreateStaff(Factory, "AD", "Adam");
			var context = new CheckinTaskReviewValidationTestContext(Factory);

			var codingWorkflow = context.CreateWorkflow("Coding");
			var codingTask = context.CreateCodingTask(codingWorkflow);
			var reviewTask1 = context.CreateReviewTask(codingWorkflow, sequence: 2, staffCode: CheckinTaskReviewValidationTestContext.ReviewerStaffCode);
			var reviewTask2 = context.CreateReviewTask(codingWorkflow, sequence: 3, staffCode: "AD");
			var checkinTask = context.CreateCheckinTask(codingWorkflow, sequence: 4);

			Factory.Save();

			checkinTask.Validation.ValidateAll();
			AssertNoErrors(checkinTask);

			checkinTask.P9_Type = "CH0";
			AssertHasError(checkinTask.P9_TypeInfo, WorkItemProcessTaskValidation.IncompleteReviewTaskMessage);

			reviewTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			checkinTask.Validation.ValidateAll();
			AssertHasError(checkinTask.P9_TypeInfo, WorkItemProcessTaskValidation.IncompleteReviewTaskMessage);

			reviewTask2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			checkinTask.Validation.ValidateAll();
			AssertNoErrors(checkinTask);
		}

		public void TestCheckinWithReviewTaskWhenCheckinSuspended()
		{
			var context = new CheckinTaskReviewValidationTestContext(Factory);

			var codingWorkflow = context.CreateWorkflow("Coding");
			var codingTask = context.CreateCodingTask(codingWorkflow);
			var reviewTask = context.CreateReviewTask(codingWorkflow);
			var checkinTask = context.CreateCheckinTask(codingWorkflow);

			checkinTask.Validation.ValidateAll();
			AssertNoErrors(checkinTask);

			checkinTask.P9_Type = "CH0";
			AssertHasError(checkinTask.P9_TypeInfo, WorkItemProcessTaskValidation.IncompleteReviewTaskMessage);

			checkinTask.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			checkinTask.Validation.ValidateAll();
			AssertNoErrors(checkinTask);
		}

		public void TestCheckinWithReviewTaskWhenCheckinAlreadySaved()
		{
			var context = new CheckinTaskReviewValidationTestContext(Factory);

			var codingWorkflow = context.CreateWorkflow("Coding");
			var codingTask = context.CreateCodingTask(codingWorkflow);
			var reviewTask = context.CreateReviewTask(codingWorkflow);
			var checkinTask = context.CreateCheckinTask(codingWorkflow);

			checkinTask.P9_Type = "CH0";
			AssertHasError(checkinTask.P9_TypeInfo, WorkItemProcessTaskValidation.IncompleteReviewTaskMessage);

			Factory.Save();

			checkinTask.Validation.ValidateAll();
			AssertNoErrors("Validation should not apply on saved tasks since existing tasks may not have setup compliant with new validation", checkinTask);
		}

		public void TestCheckinWithMissingReviewTask()
		{
			var context = new CheckinTaskReviewValidationTestContext(Factory);

			var codingWorkflow = context.CreateWorkflow("Coding");
			var codingTask = context.CreateCodingTask(codingWorkflow);
			var checkinTask = context.CreateCheckinTask(codingWorkflow);

			checkinTask.Validation.ValidateAll();
			AssertNoErrors(checkinTask);

			checkinTask.P9_Type = "CH0";
			AssertHasError(checkinTask.P9_TypeInfo, WorkItemProcessTaskValidation.NoCompletedReviewTaskMessage);
		}

		public void TestCheckinWithReviewTaskInPrerequisiteWorkflow()
		{
			var context = new CheckinTaskReviewValidationTestContext(Factory);

			var codingWorkflow = context.CreateWorkflow("Coding");
			var publishWorkflow = context.CreateWorkflow("Publish");
			codingWorkflow.GetOrCreateDependencyLink(publishWorkflow);

			var codingTask = context.CreateCodingTask(codingWorkflow);
			var reviewTask = context.CreateReviewTask(codingWorkflow);
			var checkinTask = context.CreateCheckinTask(publishWorkflow);

			checkinTask.Validation.ValidateAll();
			AssertNoErrors(checkinTask);

			checkinTask.P9_Type = "CH0";
			AssertHasError(checkinTask.P9_TypeInfo, WorkItemProcessTaskValidation.IncompleteReviewTaskMessage);

			reviewTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			checkinTask.Validation.ValidateAll();
			AssertNoErrors(checkinTask);
		}

		public void TestCheckinWithReviewTaskInPrerequisiteOfPrerequisiteWorkflow()
		{
			var context = new CheckinTaskReviewValidationTestContext(Factory);

			var codingWorkflow = context.CreateWorkflow("Coding");
			var interimWorkflow = context.CreateWorkflow("Interim");
			var publishWorkflow = context.CreateWorkflow("Publish");
			codingWorkflow.GetOrCreateDependencyLink(interimWorkflow);
			interimWorkflow.GetOrCreateDependencyLink(publishWorkflow);

			var codingTask = context.CreateCodingTask(codingWorkflow);
			var reviewTask = context.CreateReviewTask(codingWorkflow);
			var checkinTask = context.CreateCheckinTask(publishWorkflow);

			checkinTask.Validation.ValidateAll();
			AssertNoErrors(checkinTask);

			checkinTask.P9_Type = "CH0";
			AssertHasError(checkinTask.P9_TypeInfo, WorkItemProcessTaskValidation.IncompleteReviewTaskMessage);

			reviewTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			checkinTask.Validation.ValidateAll();
			AssertNoErrors(checkinTask);
		}

		public void TestCheckinWithReviewTaskInUnrelatedWorkflow()
		{
			var context = new CheckinTaskReviewValidationTestContext(Factory);

			var codingWorkflow = context.CreateWorkflow("Coding");
			var publishWorkflow = context.CreateWorkflow("Publish");
			var codingTask = context.CreateCodingTask(codingWorkflow);
			var reviewTask = context.CreateReviewTask(codingWorkflow);
			var checkinTask = context.CreateCheckinTask(publishWorkflow);

			checkinTask.Validation.ValidateAll();
			AssertNoErrors(checkinTask);

			checkinTask.P9_Type = "CH0";
			AssertHasError(checkinTask.P9_TypeInfo, WorkItemProcessTaskValidation.NoCompletedReviewTaskMessage);

			reviewTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			checkinTask.Validation.ValidateAll();
			AssertHasError(checkinTask.P9_TypeInfo, WorkItemProcessTaskValidation.NoCompletedReviewTaskMessage);

			codingWorkflow.GetOrCreateDependencyLink(publishWorkflow);
			checkinTask.Validation.ValidateAll();
			AssertNoErrors(checkinTask);
		}

		public void TestCheckinWithReviewTaskInPrerequisiteWorkflowOutsideWorkItem()
		{
			var context = new CheckinTaskReviewValidationTestContext(Factory);

			var workflow1 = context.CreateWorkflow("Coding WI1");
			var jobHeader2 = context.BMTestHelper.CreateJobHeader<NewWorkItem>(Factory);
			var workflow2 = context.BMTestHelper.CreateWorkflow(jobHeader2, "Coding WI2");

			var codingTask = context.CreateCodingTask(workflow1);
			var reviewTask = context.CreateReviewTask(workflow1);
			var checkinTask = context.CreateCheckinTask(workflow2);

			checkinTask.Validation.ValidateAll();
			AssertNoErrors(checkinTask);

			checkinTask.P9_Type = "CH0";
			AssertHasError(checkinTask.P9_TypeInfo, WorkItemProcessTaskValidation.NoCompletedReviewTaskMessage);

			reviewTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			checkinTask.Validation.ValidateAll();
			AssertHasError(checkinTask.P9_TypeInfo, WorkItemProcessTaskValidation.NoCompletedReviewTaskMessage);
		}

		public void TestP9_Type_ValidationForCheckIn()
		{
			ProcessManagement.Business.Test.WorkItemTest.SetupRegistryForDefectTesting();
			EDIDataRegistry.Instance.RequireCompletedCodeReviewForCheckin.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			workItem.WKI_ActivitySubtype = NewWorkItemLookups.WorkItemTypeConstants.EnhanceClientSpecific;
			var task1 = workItem.WorkflowItems.AddNew();
			task1.P9_Type = "COD";
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			var task2 = workItem.WorkflowItems.AddNew();
			task2.P9_Type = "CHK";
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			AssertNoErrors("This is not a defect fix WI, therefore we should not fill 'Defect Introduced in Work Item' field", task2.P9_TypeInfo);

			task2.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			AssertNoErrors("This is not a defect fix WI, therefore we should not fill 'Defect Introduced in Work Item' field", task2.P9_TypeInfo);
		}

		public void TestNonModernizationCheckinWithIncompletePrecedingTasks()
		{
			var context = new CheckinTaskForPrecedingTasksValidationTestContext(Factory);
			var codingWorkflow = context.CreateWorkflow("Coding");
			var codingTask = context.CreateCodingTask(codingWorkflow, "CDF", 1);
			var codingUnitTestTask = context.CreateCodingTask(codingWorkflow, "CDU", 2);
			var reviewTask = context.CreateReviewTask(codingWorkflow, 3);
			var checkinTask = context.CreateCheckinTask(codingWorkflow, 4);

			context.WorkItem.WKI_WorkItemType = "SOM";
			context.WorkItem.WKI_WorkItemArea = "ELS";

			codingTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			reviewTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			checkinTask.P9_Type = "CH0";
			AssertNoErrors(checkinTask);

			context.WorkItem.WKI_WorkItemType = WorkItemProcessTaskValidation.EnterpriseCode;
			context.WorkItem.WKI_WorkItemArea = "INT";
			checkinTask.Validation.ValidateAll();
			AssertNoErrors(checkinTask);

			context.WorkItem.WKI_WorkItemType = "SOM";
			context.WorkItem.WKI_WorkItemArea = WorkItemProcessTaskValidation.ModernizationCode;
			checkinTask.Validation.ValidateAll();
			AssertNoErrors(checkinTask);
		}

		public void TestModernizationCheckinWithIncompletePrecedingTasks()
		{
			var context = new CheckinTaskForPrecedingTasksValidationTestContext(Factory);
			var codingWorkflow = context.CreateWorkflow("Coding");
			var codingTask = context.CreateCodingTask(codingWorkflow, "CDF", 1);
			var codingUnitTestTask = context.CreateCodingTask(codingWorkflow, "CDU", 2);
			var reviewTask = context.CreateReviewTask(codingWorkflow, 3);
			var reviewTask2 = context.CreateReviewTask(codingWorkflow, 4);
			var checkinTask = context.CreateCheckinTask(codingWorkflow, sequence: 5);

			codingTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			codingUnitTestTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			reviewTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			reviewTask2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			checkinTask.P9_Type = "CH0";
			AssertHasError(checkinTask.P9_TypeInfo, WorkItemProcessTaskValidation.IncompletePrecedingTaskMessage);

			codingUnitTestTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			reviewTask2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			checkinTask.Validation.ValidateAll();
			AssertHasError(checkinTask.P9_TypeInfo, WorkItemProcessTaskValidation.IncompletePrecedingTaskMessage);

			codingTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			codingUnitTestTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			checkinTask.Validation.ValidateAll();
			AssertHasError(checkinTask.P9_TypeInfo, WorkItemProcessTaskValidation.IncompletePrecedingTaskMessage);
		}

		public void TestModernizationCheckinWithIncompleteButFollowingTasks()
		{
			var context = new CheckinTaskForPrecedingTasksValidationTestContext(Factory);
			var codingWorkflow = context.CreateWorkflow("Coding");
			var codingTask = context.CreateCodingTask(codingWorkflow, "CDF", 1);
			var codingUnitTestTask = context.CreateCodingTask(codingWorkflow, "CDU", 2);
			var reviewTask = context.CreateReviewTask(codingWorkflow, 3);
			var reviewTask2 = context.CreateReviewTask(codingWorkflow, 4);
			var checkinTask = context.CreateCheckinTask(codingWorkflow, 5);

			codingTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			codingTask.P9_Sequence = checkinTask.P9_Sequence + 1;
			codingUnitTestTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			reviewTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			reviewTask2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			checkinTask.P9_Type = "CH0";
			AssertNoErrors(checkinTask);

			codingTask.P9_Sequence = checkinTask.P9_Sequence;
			reviewTask2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			reviewTask2.P9_Sequence = checkinTask.P9_Sequence + 2;
			checkinTask.Validation.ValidateAll();
			AssertNoErrors(checkinTask);

			codingTask.P9_Sequence = checkinTask.P9_Sequence;
			reviewTask2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			reviewTask2.P9_Sequence = checkinTask.P9_Sequence;
			checkinTask.Validation.ValidateAll();
			AssertNoErrors(checkinTask);
		}

		public void TestModernizationCheckinWithIncompletePrecedingTasksInAnotherWorkflow()
		{
			var context = new CheckinTaskForPrecedingTasksValidationTestContext(Factory);
			var codingWorkflow = context.CreateWorkflow("Coding");
			var codingTask = context.CreateCodingTask(codingWorkflow, "CDF", 1);
			var codingUnitTestTask = context.CreateCodingTask(codingWorkflow, "CDU", 2);
			var reviewTask = context.CreateReviewTask(codingWorkflow, 3);
			var checkinTask = context.CreateCheckinTask(codingWorkflow, 4);

			var anotherCodingWorkflow = context.CreateWorkflow("Coding2");
			var anotherCodingTask = context.CreateCodingTask(anotherCodingWorkflow, "CDF", 1);
			var anotherCheckinTask = context.CreateCheckinTask(anotherCodingWorkflow, 2);

			codingTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			codingUnitTestTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			reviewTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			anotherCodingTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			checkinTask.P9_Type = "CH0";
			AssertNoErrors(checkinTask);
		}

		public void TestModernizationCheckinWithIncompletePrecedingTasksInPrerequisiteWorkflow()
		{
			var context = new CheckinTaskForPrecedingTasksValidationTestContext(Factory);
			var codingWorkflow = context.CreateWorkflow("Coding");
			var publishWorkflow = context.CreateWorkflow("Publish");
			codingWorkflow.GetOrCreateDependencyLink(publishWorkflow);

			var codingTask = context.CreateCodingTask(codingWorkflow, "CDF", 1);
			var reviewTask = context.CreateReviewTask(codingWorkflow, 2);
			codingTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			reviewTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			var checkinTask = context.CreateCheckinTask(publishWorkflow);

			checkinTask.Validation.ValidateAll();
			AssertNoErrors(checkinTask);

			checkinTask.P9_Type = "CH0";
			AssertNoErrors(checkinTask);
		}

		public void TestModernizationCheckinWithIncompletePrecedingTasksInIterationWorkflow()
		{
			var context = new CheckinTaskForPrecedingTasksValidationTestContext(Factory);
			var codingWorkflow = context.CreateWorkflow("Coding");
			var iterationWorkflow = context.CreateWorkflow("Iteration");
			iterationWorkflow.GetOrCreateLinkToParent(codingWorkflow);

			var parentCodingTask = context.CreateCodingTask(codingWorkflow, "CDF", 1);
			var parentReviewTask = context.CreateReviewTask(codingWorkflow, 2);
			var codingTask = context.CreateCodingTask(iterationWorkflow, "CDF", 3);
			var reviewTask = context.CreateReviewTask(iterationWorkflow, 4);
			var checkinTask = context.CreateCheckinTask(codingWorkflow, 5);

			parentCodingTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			parentReviewTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			codingTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			reviewTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			var pivot = (IProcessTaskIterationLink)parentReviewTask.IterationLinks.AddNew();
			pivot.P9I_P9_IterationTask = codingTask.PK;
			pivot.P9I_FH_IterationWorkflow = iterationWorkflow.PK;
			pivot.P9I_IterationReason = "RS";

			checkinTask.Validation.ValidateAll();
			AssertNoErrors(checkinTask);

			checkinTask.P9_Type = "CH0";
			AssertHasError(checkinTask.P9_TypeInfo, WorkItemProcessTaskValidation.IncompletePrecedingTaskMessage);
		}

		public void TestP9_Type_ValidationForCheckInDefects()
		{
			SetupTaskTypeAndStatusValidation_ForDefectWorkItems_AssertCorrectValidationWarningsAndErrors("CH0", ProcessTaskStatusCodeList.Codes.Assigned);
		}

		public void TestP9_Type_ValidationForCheckInDefects_ForNonDATCheckins()
		{
			SetupTaskTypeAndStatusValidation_ForDefectWorkItems_AssertCorrectValidationWarningsAndErrors("CHK", ProcessTaskStatusCodeList.Codes.Closed);
		}

		void SetupTaskTypeAndStatusValidation_ForDefectWorkItems_AssertCorrectValidationWarningsAndErrors(string checkinTaskType, string relevantCheckinTaskStatus)
		{
			ProcessManagement.Business.Test.WorkItemTest.SetupRegistryForDefectTesting("COD");

			var workItem1 = Factory.NewWithValidTestData<NewWorkItem>();
			workItem1.WKI_ActivitySubtype = NewWorkItemLookups.WorkItemTypeConstants.AmnestyFix;

			var identifyDefectCauseTask = workItem1.WorkflowItems.AddNew();
			identifyDefectCauseTask.P9_Type = "DDD"; // This is the one set in SetupRegistryForDefectTesting.
			identifyDefectCauseTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			var checkinTask = workItem1.WorkflowItems.AddNew();

			checkinTask.P9_Type = checkinTaskType;
			checkinTask.P9_Status = relevantCheckinTaskStatus;

			AssertHasError("Task type is considered a queued or completed checkin, and 'Defect Introduced in Work Item' is not entered for defect fix WI even though 'Identify Defect Cause' task is closed.", checkinTask.P9_TypeInfo, WorkItemProcessTaskValidation.DefectIntroducedWorkItemMustBeEntered);
			AssertNoError("Should be no error for field 'Defect Introduced in Task' due to precedence of warnings", checkinTask.P9_TypeInfo, WorkItemProcessTaskValidation.DefectIntroducedTaskMustBeEntered);
			AssertNoError("Should be no error for field 'First CB That Missed Defect' due to precedence of warnings", checkinTask.P9_TypeInfo, WorkItemProcessTaskValidation.FirstCBThatMissedDefectMustBeEntered);

			identifyDefectCauseTask.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			checkinTask.Validation.ValidateAll();

			AssertHasError("Expect validation error to persist even if 'Identify Defect Cause' task is cancelled.", checkinTask.P9_TypeInfo, WorkItemProcessTaskValidation.DefectIntroducedWorkItemMustBeEntered);
			AssertNoError("Should be no error for field 'Defect Introduced in Task' due to precedence of warnings", checkinTask.P9_TypeInfo, WorkItemProcessTaskValidation.DefectIntroducedTaskMustBeEntered);
			AssertNoError("Should be no error for field 'First CB That Missed Defect' due to precedence of warnings", checkinTask.P9_TypeInfo, WorkItemProcessTaskValidation.FirstCBThatMissedDefectMustBeEntered);

			identifyDefectCauseTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			checkinTask.Validation.ValidateAll();

			AssertHasError("Task type is considered a queued or completed checkin, and 'Defect Introduced in Work Item' is not entered for defect fix WI even though 'Identify Defect Cause' task is closed.", checkinTask.P9_TypeInfo, WorkItemProcessTaskValidation.DefectIntroducedWorkItemMustBeEntered);
			AssertNoError("Should be no error for field 'Defect Introduced in Task' due to precedence of errors", checkinTask.P9_TypeInfo, WorkItemProcessTaskValidation.DefectIntroducedTaskMustBeEntered);
			AssertNoError("Should be no error for field 'First CB That Missed Defect' due to precedence of errors", checkinTask.P9_TypeInfo, WorkItemProcessTaskValidation.FirstCBThatMissedDefectMustBeEntered);

			checkinTask.P9_Type = "COD";

			AssertNoError("No validation error 'Identify Defect Cause' now since the task is no longer considered to be a 'checkin'.", checkinTask.P9_TypeInfo, WorkItemProcessTaskValidation.DefectIntroducedWorkItemMustBeEntered);
			AssertNoError("No validation error 'Defect Introduced in Task' now since the task is no longer considered to be a 'checkin'.", checkinTask.P9_TypeInfo, WorkItemProcessTaskValidation.DefectIntroducedTaskMustBeEntered);
			AssertNoError("No validation error 'First CB That Missed Defect' now since the task is no longer considered to be a 'checkin'.", checkinTask.P9_TypeInfo, WorkItemProcessTaskValidation.FirstCBThatMissedDefectMustBeEntered);

			checkinTask.P9_Type = checkinTaskType;
			checkinTask.P9_Status = relevantCheckinTaskStatus;

			AssertHasError("Now that we have changed the task to be a checkin type again, the validation error should return.", checkinTask.P9_TypeInfo, WorkItemProcessTaskValidation.DefectIntroducedWorkItemMustBeEntered);
			AssertNoError("Should be no error for field 'Defect Introduced in Task' due to precedence of errors", checkinTask.P9_TypeInfo, WorkItemProcessTaskValidation.DefectIntroducedTaskMustBeEntered);
			AssertNoError("Should be no error for field 'First CB That Missed Defect' due to precedence of errors", checkinTask.P9_TypeInfo, WorkItemProcessTaskValidation.FirstCBThatMissedDefectMustBeEntered);

			identifyDefectCauseTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			var workItem2 = Factory.NewWithValidTestData<NewWorkItem>();
			var taskInWorkItem2 = workItem2.WorkflowItems.AddNew();
			ProcessManagement.Business.ProcessManagementRegistry.Instance.DefectIntroducedTaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { "CH0" });
			taskInWorkItem2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			taskInWorkItem2.P9_Type = "CH0";

			workItem1.DefectCausedByWorkItemPK = workItem2.PK;

			checkinTask.Validation.ValidateP9_Type();
			AssertNoError("No validation error now since the defect cause was recorded.", checkinTask.P9_TypeInfo, WorkItemProcessTaskValidation.DefectIntroducedWorkItemMustBeEntered);
			AssertHasError("Should be error for field 'Defect Introduced in Task' as the defect introduced in task is not recorded/invalid", checkinTask.P9_TypeInfo, WorkItemProcessTaskValidation.DefectIntroducedTaskMustBeEntered);
			AssertNoError("Should be no warning for field 'First CB That Missed Defect' due to precedence of errors", checkinTask.P9_TypeInfo, WorkItemProcessTaskValidation.FirstCBThatMissedDefectMustBeEntered);

			workItem1.WKI_P9_DefectCausedByTask = taskInWorkItem2.PK;

			checkinTask.Validation.ValidateP9_Type();
			AssertNoError("No validation error now since the defect cause was recorded.", checkinTask.P9_TypeInfo, WorkItemProcessTaskValidation.DefectIntroducedWorkItemMustBeEntered);
			AssertNoError("No validation error now since the defect introduced task was recorded.", checkinTask.P9_TypeInfo, WorkItemProcessTaskValidation.DefectIntroducedTaskMustBeEntered);
			AssertHasError("Should be error for field 'First CB That Missed Defect' as this is not recorded/invalid", checkinTask.P9_TypeInfo, WorkItemProcessTaskValidation.FirstCBThatMissedDefectMustBeEntered);

			workItem1.WKI_P9_DefectFirstMissedInTask = taskInWorkItem2.PK;

			checkinTask.Validation.ValidateP9_Type();
			AssertNoError("No validation error now since the defect cause was recorded.", checkinTask.P9_TypeInfo, WorkItemProcessTaskValidation.DefectIntroducedWorkItemMustBeEntered);
			AssertNoError("No validation error now since the defect introduced task was recorded.", checkinTask.P9_TypeInfo, WorkItemProcessTaskValidation.DefectIntroducedTaskMustBeEntered);
			AssertNoError("No validation error now since the first CB that missed defect was recorded.", checkinTask.P9_TypeInfo, WorkItemProcessTaskValidation.FirstCBThatMissedDefectMustBeEntered);
		}

		public void TestP9_Type_ValidationForTasksForShelfCreation()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("TTT", "task for shelf creation");
			EDIDataRegistry.Instance.TasksMandatoryInShelfCreation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			var reviewTasksList = new CodeDescriptionBoolCollection();
			reviewTasksList.Add("RVW");
			reviewTasksList.Add("FRV");
			EDIDataRegistry.Instance.ReviewTasks.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, reviewTasksList);

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var codingTask = workItem.WorkflowItems.AddNew();
			codingTask.P9_Type = "COD";
			codingTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			var reviewTask = workItem.WorkflowItems.AddNew();
			reviewTask.P9_Type = "RVW";
			reviewTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Assert("Precondition: this is a review task", reviewTask.IsReviewTask);

			var shelfCreationTask = workItem.WorkflowItems.AddNew();
			shelfCreationTask.P9_Type = "TTT";
			shelfCreationTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Assert("Precondition: this is a shelf creation task", shelfCreationTask.IsTaskMandatoryForShelfQueue);

			reviewTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertNoWarning("Should be no warning because this is not shelf creation task.",
				reviewTask.P9_StatusInfo, WorkItemProcessTaskValidation.DefectIntroducedWorkItemMustBeEntered);

			shelfCreationTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertNoWarning("Should be no warning because this is not a defect fixing work item.",
				shelfCreationTask.P9_StatusInfo, WorkItemProcessTaskValidation.DefectIntroducedWorkItemMustBeEntered);

			ProcessManagement.Business.ProcessManagementRegistry.Instance.DefectWorkItemTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { "DEF" });
			workItem.WKI_ActivitySubtype = "DEF";
			reviewTask.Validation.ValidateP9_Status();
			AssertNoWarning("Warning should not appears because this is not a shelf creation task.",
				reviewTask.P9_StatusInfo, WorkItemProcessTaskValidation.DefectIntroducedWorkItemMustBeEntered);

			shelfCreationTask.Validation.ValidateP9_Status();
			AssertHasWarning(@"Warning should appears because this a defect fixing work item & shelf creation task 
and field 'Defect Introduced in Work Item' is empty.",
				shelfCreationTask.P9_StatusInfo, WorkItemProcessTaskValidation.DefectIntroducedWorkItemMustBeEntered);
			AssertNoWarning("Should be no warning for field 'Defect Introduced in Task' due to precedence of warnings", shelfCreationTask.P9_StatusInfo, WorkItemProcessTaskValidation.DefectIntroducedTaskMustBeEntered);
			AssertNoWarning("Should be no warning for field 'First CB That Missed Defect' due to precedence of warnings", shelfCreationTask.P9_StatusInfo, WorkItemProcessTaskValidation.FirstCBThatMissedDefectMustBeEntered);

			codingTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertNoWarning("Should be no warning (Defect Introduced Work Item) for coding task - only Shelf creation tasks are validated.",
				codingTask.P9_StatusInfo, WorkItemProcessTaskValidation.DefectIntroducedWorkItemMustBeEntered);
			AssertNoWarning("Should be no warning (Defect Introduced Task) for coding task - only Shelf creation tasks are validated.",
				codingTask.P9_StatusInfo, WorkItemProcessTaskValidation.DefectIntroducedTaskMustBeEntered);
			AssertNoWarning("Should be no warning (First CB That Missed Defect) for coding task - only Shelf creation tasks are validated.",
				codingTask.P9_StatusInfo, WorkItemProcessTaskValidation.FirstCBThatMissedDefectMustBeEntered);

			var workitem2 = Factory.NewWithValidTestData<NewWorkItem>();
			var causedWorkItemTask = workitem2.WorkflowItems.AddNew();
			ProcessManagement.Business.ProcessManagementRegistry.Instance.DefectIntroducedTaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { "CH0" });
			causedWorkItemTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			causedWorkItemTask.P9_Type = "CH0";
			workItem.DefectCausedByWorkItemPK = workitem2.PK;

			shelfCreationTask.Validation.ValidateP9_Status();
			AssertNoWarning("Should be no warning because field 'Defect Introduced in Work Item' is set.",
				shelfCreationTask.P9_StatusInfo, WorkItemProcessTaskValidation.DefectIntroducedWorkItemMustBeEntered);
			AssertHasWarning("Should be warning for field 'Defect Introduced in Task' as it is not set", shelfCreationTask.P9_StatusInfo, WorkItemProcessTaskValidation.DefectIntroducedTaskMustBeEntered);
			AssertNoWarning("Should be no warning for field 'First CB That Missed Defect' due to precedence of warnings", shelfCreationTask.P9_StatusInfo, WorkItemProcessTaskValidation.FirstCBThatMissedDefectMustBeEntered);

			workItem.WKI_P9_DefectCausedByTask = causedWorkItemTask.PK;
			shelfCreationTask.Validation.ValidateP9_Status();
			AssertNoWarning("Should be no warning because field 'Defect Introduced in Work Item' is set.",
				shelfCreationTask.P9_StatusInfo, WorkItemProcessTaskValidation.DefectIntroducedWorkItemMustBeEntered);
			AssertNoWarning("Should be warning for field 'Defect Introduced in Task' not set", shelfCreationTask.P9_StatusInfo, WorkItemProcessTaskValidation.DefectIntroducedTaskMustBeEntered);
			AssertHasWarning("Should be warning for field 'First CB That Missed Defect' as it is not set", shelfCreationTask.P9_StatusInfo, WorkItemProcessTaskValidation.FirstCBThatMissedDefectMustBeEntered);
		}

		public void TestDefectErrorMessageDependsOnRegistryValues()
		{
			ProcessManagement.Business.Test.WorkItemTest.SetupRegistryForDefectTesting("COD");
			ProcessManagement.Business.ProcessManagementRegistry.Instance.DefectIntroducedInWorkItemLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "DIWI 123");
			ProcessManagement.Business.ProcessManagementRegistry.Instance.DefectIntroducedInTaskLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "DIT 456");
			ProcessManagement.Business.ProcessManagementRegistry.Instance.FirstCBThatMissedDefectLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "FCBTMD 789");

			var expectedDefectIntroducedInWorkItemErrorMsg = "There is no nominated cause for this defect. A DIWI 123 value must be entered before this change can be queued for check-in.";
			var expectedDefectIntroducedInTaskErrorMsg = "There is no nominated task for this defect. A DIT 456 value must be entered before this change can be queued for check-in.";
			var expectedFirstCBThatMissedDefectErrorMsg = "There is no nominated containment barrier for this defect. A FCBTMD 789 value must be entered before this change can be queued for check-in.";

			var workItem1 = Factory.NewWithValidTestData<NewWorkItem>();
			workItem1.WKI_ActivitySubtype = NewWorkItemLookups.WorkItemTypeConstants.AmnestyFix;

			var identifyDefectCauseTask = workItem1.WorkflowItems.AddNew();
			identifyDefectCauseTask.P9_Type = "DDD"; // This is the one set in SetupRegistryForDefectTesting.
			identifyDefectCauseTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			var checkinTask = workItem1.WorkflowItems.AddNew();

			checkinTask.P9_Type = "CH0";
			checkinTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			checkinTask.Validation.ValidateAll();

			AssertHasError("Task type is considered a queued or completed checkin, and 'Defect Introduced in Work Item' is not entered for defect fix WI even though 'Identify Defect Cause' task is closed.", checkinTask.P9_TypeInfo, expectedDefectIntroducedInWorkItemErrorMsg);
			AssertNoError("Should be no error for field 'Defect Introduced in Task' due to precedence of errors", checkinTask.P9_TypeInfo, expectedDefectIntroducedInTaskErrorMsg);
			AssertNoError("Should be no error for field 'First CB That Missed Defect' due to precedence of errors", checkinTask.P9_TypeInfo, expectedFirstCBThatMissedDefectErrorMsg);

			identifyDefectCauseTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			var workItem2 = Factory.NewWithValidTestData<NewWorkItem>();
			var taskInWorkItem2 = workItem2.WorkflowItems.AddNew();
			ProcessManagement.Business.ProcessManagementRegistry.Instance.DefectIntroducedTaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { "CH0" });
			taskInWorkItem2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			taskInWorkItem2.P9_Type = "CH0";

			workItem1.DefectCausedByWorkItemPK = workItem2.PK;

			checkinTask.Validation.ValidateP9_Type();
			AssertNoError("No validation error now since the defect cause was recorded.", checkinTask.P9_TypeInfo, expectedDefectIntroducedInWorkItemErrorMsg);
			AssertHasError("Should be error for field 'Defect Introduced in Task' as the defect introduced in task is not recorded/invalid", checkinTask.P9_TypeInfo, expectedDefectIntroducedInTaskErrorMsg);
			AssertNoError("Should be no warning for field 'First CB That Missed Defect' due to precedence of errors", checkinTask.P9_TypeInfo, expectedFirstCBThatMissedDefectErrorMsg);

			workItem1.WKI_P9_DefectCausedByTask = taskInWorkItem2.PK;

			checkinTask.Validation.ValidateP9_Type();
			AssertNoError("No validation error now since the defect cause was recorded.", checkinTask.P9_TypeInfo, expectedDefectIntroducedInWorkItemErrorMsg);
			AssertNoError("No validation error now since the defect introduced task was recorded.", checkinTask.P9_TypeInfo, expectedDefectIntroducedInTaskErrorMsg);
			AssertHasError("Should be error for field 'First CB That Missed Defect' as this is not recorded/invalid", checkinTask.P9_TypeInfo, expectedFirstCBThatMissedDefectErrorMsg);

			workItem1.WKI_P9_DefectFirstMissedInTask = taskInWorkItem2.PK;

			checkinTask.Validation.ValidateP9_Type();
			AssertNoError("No validation error now since the defect cause was recorded.", checkinTask.P9_TypeInfo, expectedDefectIntroducedInWorkItemErrorMsg);
			AssertNoError("No validation error now since the defect introduced task was recorded.", checkinTask.P9_TypeInfo, expectedDefectIntroducedInTaskErrorMsg);
			AssertNoError("No validation error now since the first CB that missed defect was recorded.", checkinTask.P9_TypeInfo, expectedFirstCBThatMissedDefectErrorMsg);
		}

		public void TestActiveShelfTasksShouldNotBeSetToWorking()
		{
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var codingTask = CreateAssignedTask(workItem, "COD", GlbStaff.CurrentUser.GS_Code);
			var shelfTask = CreateAssignedTask(workItem, EDITaskTypes.TaskActiveShelfTest, GlbStaff.CurrentUser.GS_Code);
			var checkinTask = CreateAssignedTask(workItem, EDITaskTypes.TaskActiveCheckin, GlbStaff.CurrentUser.GS_Code);
			var patchingTask = CreateAssignedTask(workItem, ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask, GlbStaff.CurrentUser.GS_Code);
			var patchingDATTask = CreateAssignedTask(workItem, ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask, "DAT");
			var aspectTask = CreateAssignedTask(workItem, EDITaskTypes.TaskActiveAspectOnlyBuild, GlbStaff.CurrentUser.GS_Code);
			var uatTask = CreateAssignedTask(workItem, EDITaskTypes.TaskActiveUATBuild, GlbStaff.CurrentUser.GS_Code);

			AssertNoError("Assigned status should not generate an error", codingTask.P9_StatusInfo, WorkItemProcessTaskValidation.ShelfTaskCannotBeSetToWorking);
			AssertNoError("Assigned status should not generate an error", shelfTask.P9_StatusInfo, WorkItemProcessTaskValidation.ShelfTaskCannotBeSetToWorking);
			AssertNoError("Assigned status should not generate an error", checkinTask.P9_StatusInfo, WorkItemProcessTaskValidation.ShelfTaskCannotBeSetToWorking);
			AssertNoError("Assigned status should not generate an error", patchingTask.P9_StatusInfo, WorkItemProcessTaskValidation.ShelfTaskCannotBeSetToWorking);

			codingTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertNoError("Coding Task OK", codingTask.P9_StatusInfo, WorkItemProcessTaskValidation.ShelfTaskCannotBeSetToWorking);
			codingTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertNoError("Coding Task OK", codingTask.P9_StatusInfo, WorkItemProcessTaskValidation.ShelfTaskCannotBeSetToWorking);

			AssertSettingStatusGivesErrorWhenSetToWorking(shelfTask);
			AssertSettingStatusGivesErrorWhenSetToWorking(checkinTask);
			AssertSettingStatusGivesErrorWhenSetToWorking(patchingTask);
			AssertSettingStatusGivesErrorWhenSetToWorking(patchingDATTask);
			AssertSettingStatusGivesErrorWhenSetToWorking(aspectTask);
			AssertSettingStatusGivesErrorWhenSetToWorking(uatTask);
		}

		static void AssertSettingStatusGivesErrorWhenSetToWorking(WorkItemProcessTask task)
		{
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			AssertNoError("task is not set to working so should not generate an error", task.P9_StatusInfo, WorkItemProcessTaskValidation.ShelfTaskCannotBeSetToWorking);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertHasError("An active shelf task set to Working is not a valid combination", task.P9_StatusInfo, WorkItemProcessTaskValidation.ShelfTaskCannotBeSetToWorking);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertNoError("task is not set to working so should not generate an error", task.P9_StatusInfo, WorkItemProcessTaskValidation.ShelfTaskCannotBeSetToWorking);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			AssertNoError("task is not set to working so should not generate an error", task.P9_StatusInfo, WorkItemProcessTaskValidation.ShelfTaskCannotBeSetToWorking);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertNoError("task is not set to working so should not generate an error", task.P9_StatusInfo, WorkItemProcessTaskValidation.ShelfTaskCannotBeSetToWorking);
		}

		public void TestWorkingTaskShouldNotBeSetToActiveShelfType()
		{
			EnsureShelfTaskTypesExist();
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();

			var shelfTask = CreateAssignedTask(workItem, EDITaskTypes.TaskShelfTest, GlbStaff.CurrentUser.GS_Code);
			var checkinTask = CreateAssignedTask(workItem, EDITaskTypes.TaskCheckin, GlbStaff.CurrentUser.GS_Code);
			var aspectTask = CreateAssignedTask(workItem, EDITaskTypes.TaskAspectOnlyBuild, GlbStaff.CurrentUser.GS_Code);
			var uatTask = CreateAssignedTask(workItem, EDITaskTypes.TaskUATBuild, GlbStaff.CurrentUser.GS_Code);

			AssertActiveShelfGivesErrorWhenWorkingStatus(shelfTask, EDITaskTypes.TaskShelfTest, EDITaskTypes.TaskActiveShelfTest);
			AssertActiveShelfGivesErrorWhenWorkingStatus(checkinTask, EDITaskTypes.TaskCheckin, EDITaskTypes.TaskActiveCheckin);
			AssertActiveShelfGivesErrorWhenWorkingStatus(aspectTask, EDITaskTypes.TaskAspectOnlyBuild, EDITaskTypes.TaskActiveAspectOnlyBuild);
			AssertActiveShelfGivesErrorWhenWorkingStatus(uatTask, EDITaskTypes.TaskUATBuild, EDITaskTypes.TaskActiveUATBuild);
		}

		static void AssertActiveShelfGivesErrorWhenWorkingStatus(WorkItemProcessTask task, string shelfType, string activeShelfType)
		{
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertNoError("Working status should not generate an error as task is not an active shelf task", task.P9_StatusInfo, WorkItemProcessTaskValidation.ShelfTaskCannotBeSetToWorking);
			AssertNoError("Task type should not have an error", task.P9_TypeInfo, WorkItemProcessTaskValidation.ShelfTaskCannotBeSetToWorking);
			task.P9_Type = activeShelfType;
			AssertHasError($"An active shelf task ({activeShelfType}) and status = WRK should give a validation error", task.P9_TypeInfo, WorkItemProcessTaskValidation.ShelfTaskCannotBeSetToWorking);
			task.P9_Type = shelfType;
			AssertNoError("task type is not an active shelf task, so should not generate an error", task.P9_TypeInfo, WorkItemProcessTaskValidation.ShelfTaskCannotBeSetToWorking);
		}

		#region Implementation

		WorkItemProcessTask CreateAssignedTask(NewWorkItem workItem, string type, string assignedStaff)
		{
			var task = workItem.WorkflowItems.AddNew();
			task.P9_Type = type;
			task.P9_GS_NKAssignedStaffMember = assignedStaff;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task.P9_Description = "ShelfName";
			return task;
		}

		void EnsureShelfTaskTypesExist()
		{
			var categorisedTaskTypes = WorkflowDataRegistry.Instance.TaskTypes.Value;

			var taskTypeCategory = categorisedTaskTypes.Cast<CategorisedWorkflowTaskTypes>().SingleOrDefault(c => c.Code == "WKI") ?? categorisedTaskTypes.AddNew();
			taskTypeCategory.Code = "WKI";

			foreach (var taskType in typeof(EDITaskTypes).GetFields(BindingFlags.Static | BindingFlags.Public))
			{
				var taskTypeCode = taskType.GetValue(null).ToString();
				var workflowTaskType = taskTypeCategory.TaskTypes.Cast<WorkflowTaskType>().SingleOrDefault(x => x.Code == taskTypeCode) ?? taskTypeCategory.TaskTypes.AddNew();
				workflowTaskType.Code = taskTypeCode;
			}

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categorisedTaskTypes);
		}

		protected override ProcessTask NewProcessTask()
		{
			return Factory.New<WorkItemProcessTask>();
		}

		#endregion
	}
}
