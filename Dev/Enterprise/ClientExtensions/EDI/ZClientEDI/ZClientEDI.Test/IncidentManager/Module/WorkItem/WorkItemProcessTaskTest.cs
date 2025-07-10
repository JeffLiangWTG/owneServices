using System;
using System.Linq;
using System.Net;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.Business.Test;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;
using WTG.DevTools.ServiceClient.Assess;
using WTG.DevTools.ServiceClient.Common;
using ZClientEDI.Business.Test;

namespace Enterprise.Client.EDI.IncidentManager.Module.Testing;

public class WorkItemProcessTaskTest : TestCaseWithFactory
{
	public void TestWorkingAndClosingOnReviewTask_NoSkillRequirement()
	{
		WorkItemProcessTaskTestHelper.SetupReviewTasksRegistry("CBC");
		var staff = Factory.NewWithValidTestData<GlbStaff>();
		staff.GS_Code = "IDT";

		var workItem = Factory.NewWithValidTestData<NewWorkItem>();
		var task = workItem.WorkflowItems.AddNew();
		task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
		task.P9_Type = "CBC";
		Factory.Save();

		task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
		AssertEquals(false, task.HasAssignedStaffNotCompletedAnyAssessRequirements());

		ZFormModaliser.LastFormShownDialogForTest = null;
		task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
		AssertEquals("Should not show TaskSkillsForm", null, ZFormModaliser.LastFormShownDialogForTest);
		AssertEquals("Should stay working if skill requirements are met", ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);

		ZFormModaliser.LastFormShownDialogForTest = null;
		task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
		AssertEquals("Should not show TaskSkillsForm", null, ZFormModaliser.LastFormShownDialogForTest);
		AssertEquals("Should stay working if skill requirements are met", ProcessTaskStatusCodeList.Codes.Closed, task.P9_Status);
	}

	[GuiTest]
	public void TestWorkingOnAndClosingReviewTask_WithAssessRequirement()
	{
		var aspectPK1 = Guid.NewGuid();
		var aspectPK2 = Guid.NewGuid();

		WorkItemProcessTaskTestHelper.SetupReviewTasksRegistry("CBC");
		var staff = Factory.NewWithValidTestData<GlbStaff>();
		staff.GS_Code = "IDT";
		Factory.Save();

		var workItem = Factory.NewWithValidTestData<NewWorkItem>();
		var task = workItem.WorkflowItems.AddNew();
		task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
		task.P9_Type = "CBC";

		WorkItemProcessTaskTestHelper.AddRequiredAspects(task, aspectPK1, aspectPK2);
		using (AssessTestSetupHelper.SetupForEnsureEnrolled(staff, aspectPK1, aspectPK2))
		{
			Factory.Save();
		}

		using (AssessTestSetupHelper.SetupHasCompletedLearningUnits(staff, (aspectPK1, false), (aspectPK2, false)))
		{
			TrySetStatusAndExpectOutcome(task, ProcessTaskStatusCodeList.Codes.Working, shouldShowTaskSkillsForm: true);
			TrySetStatusAndExpectOutcome(task, ProcessTaskStatusCodeList.Codes.Closed, shouldShowTaskSkillsForm: true);
		}

		using (AssessTestSetupHelper.SetupHasCompletedLearningUnits(staff, (aspectPK1, true), (aspectPK2, false)))
		{
			TrySetStatusAndExpectOutcome(task, ProcessTaskStatusCodeList.Codes.Working, shouldShowTaskSkillsForm: true);
			TrySetStatusAndExpectOutcome(task, ProcessTaskStatusCodeList.Codes.Closed, shouldShowTaskSkillsForm: true);
		}

		using (AssessTestSetupHelper.SetupHasCompletedLearningUnits(staff, (aspectPK1, true), (aspectPK2, true)))
		{
			TrySetStatusAndExpectOutcome(task, ProcessTaskStatusCodeList.Codes.Working, shouldShowTaskSkillsForm: false);
			TrySetStatusAndExpectOutcome(task, ProcessTaskStatusCodeList.Codes.Closed, shouldShowTaskSkillsForm: false);
		}
	}

	[GuiTest]
	public void TestWorkingOnReviewTask_WithAssessRequirement_WhenWtaServiceOffline()
	{
		var aspectPK = Guid.NewGuid();

		WorkItemProcessTaskTestHelper.SetupReviewTasksRegistry("CBC");
		var staff = Factory.NewWithValidTestData<GlbStaff>();
		staff.GS_Code = "IDT";
		Factory.Save();

		var workItem = Factory.NewWithValidTestData<NewWorkItem>();
		var task = workItem.WorkflowItems.AddNew();
		task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
		task.P9_Type = "CBC";

		WorkItemProcessTaskTestHelper.AddRequiredAspects(task, aspectPK);
		using (AssessTestSetupHelper.SetupForEnsureEnrolled(staff, aspectPK))
		{
			Factory.Save();
		}

		using (AssessTestSetupHelper.SetupHasCompletedLearningUnits(staff, (aspectPK, ServiceResponse<bool>.Failure(HttpStatusCode.InternalServerError, "Error"))))
		{
			TrySetStatusAndExpectOutcome(task, ProcessTaskStatusCodeList.Codes.Working, shouldShowTaskSkillsForm: false);
		}

		task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

		using (AssessTestSetupHelper.SetupHasCompletedLearningUnits(staff, (aspectPK, true)))
		{
			TrySetStatusAndExpectOutcome(task, ProcessTaskStatusCodeList.Codes.Working, shouldShowTaskSkillsForm: false);
		}

		task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

		using (AssessTestSetupHelper.SetupHasCompletedLearningUnits(staff, (aspectPK, false)))
		{
			TrySetStatusAndExpectOutcome(task, ProcessTaskStatusCodeList.Codes.Working, shouldShowTaskSkillsForm: true);
		}
	}

	[GuiTest]
	public void TestWorkingAndClosingOnReviewTask_StaffLackOfRequiredSkills()
	{
		var aspectPK1 = Guid.NewGuid();
		var aspectPK2 = Guid.NewGuid();
		var aspectPK3 = Guid.NewGuid();

		WorkItemProcessTaskTestHelper.SetupReviewTasksRegistry("CBC");
		var staff = Factory.NewWithValidTestData<GlbStaff>();
		staff.GS_Code = "IDT";

		var workItem = Factory.NewWithValidTestData<NewWorkItem>();
		var task = workItem.WorkflowItems.Tasks.AddNew();
		task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
		task.P9_Type = "CBC";

		WorkItemProcessTaskTestHelper.AddRequiredAspects(task, aspectPK1, aspectPK2, aspectPK3);
		Factory.Save();

		using (AssessTestSetupHelper.SetupHasCompletedLearningUnits(staff, (aspectPK1, false), (aspectPK2, false), (aspectPK3, false)))
		{
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertEquals("Should show TaskSkillsForm", typeof(TaskSkillsForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			AssertEquals("Should rollback to Assigned if not meeting skill requirement", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("Should show TaskSkillsForm", typeof(TaskSkillsForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			AssertEquals("Should rollback to Assigned if not meeting skill requirement", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
		}
	}

	[GuiTest]
	public void TestWorkingAndClosingOnReviewTask_StaffHasNotCompletedCourse_WhenLearningTaskCreated()
	{
		var aspectPK = Guid.NewGuid();
		const string learningTaskType = "LRN";

		WorkItemProcessTaskTestHelper.SetupReviewTasksRegistry("CBC");
		WorkItemProcessTaskTestHelper.SetupLearningTaskRegistry(learningTaskType);

		var staff = Factory.NewWithValidTestData<GlbStaff>();
		var workItem = Factory.NewWithValidTestData<NewWorkItem>();
		var task = workItem.WorkflowItems.Tasks.AddNew();
		task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
		task.P9_Description = "Even more zing and pep";
		task.P9_Type = "CBC";
		WorkItemProcessTaskTestHelper.AddRequiredAspects(task, aspectPK);

		Factory.Save();

		ZFormModaliser.ShowDialogsInTest = true;
		ZFormModaliser.SetDelegateToCallOnFormShown(form =>
		{
			var taskSkillsForm = (TaskSkillsForm)form;
			taskSkillsForm.CreateSkillLearningTaskFunction.Invoke(task);
		});

		using (AssessTestSetupHelper.SetupHasCompletedLearningUnits(staff, (aspectPK, false)))
		{
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertEquals("Should show TaskSkillsForm", typeof(TaskSkillsForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		}

		var learningTask = workItem.WorkflowItems.Tasks.Cast<ProcessTask>().Single(t => t.P9_Type == learningTaskType);
		AssertEquals("Learning: Even more zing and pep", learningTask.P9_Description);
	}

	[GuiTest]
	public void TestWorkingAndClosingOnNonReviewTask()
	{
		var aspectPK1 = Guid.NewGuid();
		var aspectPK2 = Guid.NewGuid();
		var aspectPK3 = Guid.NewGuid();

		WorkItemProcessTaskTestHelper.SetupReviewTasksRegistry("CBC", "CBR");
		var staff = Factory.NewWithValidTestData<GlbStaff>();
		staff.GS_Code = "IDT";

		var workItem = Factory.NewWithValidTestData<NewWorkItem>();
		var task = workItem.WorkflowItems.AddNew();
		task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
		task.P9_Type = "CDF";
		WorkItemProcessTaskTestHelper.AddRequiredAspects(task, aspectPK1, aspectPK2, aspectPK3);
		Factory.Save();

		using (AssessTestSetupHelper.SetupHasCompletedLearningUnits(staff, (aspectPK1, false), (aspectPK2, false), (aspectPK3, false)))
		{
			// CBC - require skill check
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task.P9_Type = "CBC";
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertEquals("Should show TaskSkillsForm", typeof(TaskSkillsForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			AssertEquals("Should rollback to Assigned if not meeting skill requirement", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("Should show TaskSkillsForm", typeof(TaskSkillsForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			AssertEquals("Should rollback to Assigned if not meeting skill requirement", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);

			// CBR - require skill check
			ZFormModaliser.LastFormShownDialogForTest = null;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task.P9_Type = "CBR";
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertEquals("Should show TaskSkillsForm", typeof(TaskSkillsForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			AssertEquals("Should rollback to Assigned if not meeting skill requirement", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("Should show TaskSkillsForm", typeof(TaskSkillsForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			AssertEquals("Should rollback to Assigned if not meeting skill requirement", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);

			// CBX - non review task - not require skill check
			ZFormModaliser.LastFormShownDialogForTest = null;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task.P9_Type = "CBX";
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertEquals("Should not show TaskSkillsForm", null, ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals("Should stay working if skill requirements are met", ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("Should not show TaskSkillsForm", null, ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals("Should stay working if skill requirements are met", ProcessTaskStatusCodeList.Codes.Closed, task.P9_Status);
		}
	}

	[GuiTest]
	public void TestWorkingAndClosingOnReviewTask_StaffHasCompletedRequiredSkills()
	{
		var aspectPK1 = Guid.NewGuid();
		var aspectPK2 = Guid.NewGuid();
		var aspectPK3 = Guid.NewGuid();

		WorkItemProcessTaskTestHelper.SetupReviewTasksRegistry("CBC");
		var staff = Factory.NewWithValidTestData<GlbStaff>();
		staff.GS_Code = "IDT";

		var workItem = Factory.NewWithValidTestData<NewWorkItem>();
		var task = workItem.WorkflowItems.AddNew();
		task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
		task.P9_Type = "CBC";

		WorkItemProcessTaskTestHelper.AddRequiredAspects(task, aspectPK1, aspectPK2, aspectPK3);
		Factory.Save();

		using (AssessTestSetupHelper.SetupHasCompletedLearningUnits(staff, (aspectPK1, false), (aspectPK2, false), (aspectPK3, false)))
		{
			var serviceClientMock = Mock.Get(ObjectFactory.Get<IAssessServiceClient>());

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertEquals("Should show TaskSkillsForm", typeof(TaskSkillsForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			AssertEquals("Should rollback to Assigned if not meeting skill requirement", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);

			// Acquire skill1
			serviceClientMock.SetupHasCompletedLearningUnit(staff, aspectPK1, true);
			ZFormModaliser.LastFormShownDialogForTest = null;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertEquals("Should show TaskSkillsForm", typeof(TaskSkillsForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			AssertEquals("Should rollback to Assigned if not meeting skill requirement", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("Should show TaskSkillsForm", typeof(TaskSkillsForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			AssertEquals("Should rollback to Assigned if not meeting skill requirement", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);

			// Acquire skill2
			serviceClientMock.SetupHasCompletedLearningUnit(staff, aspectPK2, true);
			ZFormModaliser.LastFormShownDialogForTest = null;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertEquals("Should show TaskSkillsForm", typeof(TaskSkillsForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			AssertEquals("Should rollback to Assigned if not meeting skill requirement", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("Should show TaskSkillsForm", typeof(TaskSkillsForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			AssertEquals("Should rollback to Assigned if not meeting skill requirement", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);

			// All skill acquired - this time should not expect the TaskSkillsForm
			serviceClientMock.SetupHasCompletedLearningUnit(staff, aspectPK3, true);
			ZFormModaliser.LastFormShownDialogForTest = null;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertEquals("Should not show TaskSkillsForm", null, ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals("Should stay working if skill requirements are met", ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("Should not show TaskSkillsForm", null, ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals("Should stay working if skill requirements are met", ProcessTaskStatusCodeList.Codes.Closed, task.P9_Status);
		}
	}

	[GuiTest]
	public void TestWorkingAndClosingOnReviewTask_StaffHasCompletedRequiredSkills_StaffNotAssigned()
	{
		var aspectPK1 = Guid.NewGuid();
		var aspectPK2 = Guid.NewGuid();
		var aspectPK3 = Guid.NewGuid();

		WorkItemProcessTaskTestHelper.SetupReviewTasksRegistry("CBC");
		var staff = Factory.NewWithValidTestData<GlbStaff>();
		staff.GS_Code = "A.G";

		var capability = Factory.NewWithValidTestData<GlbCapability>();
		var pivot = staff.CapabilityPivots.AddNew();
		pivot.G5_G4_Capability = capability.PK;

		Factory.Save();

		var workItem = Factory.NewWithValidTestData<NewWorkItem>();
		var task = workItem.WorkflowItems.AddNew();
		task.P9_Type = "CBC";
		task.P9_G4_RequiredCapability = capability.PK;

		WorkItemProcessTaskTestHelper.AddRequiredAspects(task, aspectPK1, aspectPK2, aspectPK3);
		Factory.Save();

		using (AssessTestSetupHelper.SetupHasCompletedLearningUnits(staff, (aspectPK1, false), (aspectPK2, false), (aspectPK3, false)))
		using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
		{
			var serviceClientMock = Mock.Get(ObjectFactory.Get<IAssessServiceClient>());

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertEquals("Should show TaskSkillsForm", typeof(TaskSkillsForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			AssertEquals("Should rollback to Assigned if not meeting skill requirement", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);

			// Acquire skill1
			serviceClientMock.SetupHasCompletedLearningUnit(staff, aspectPK1, true);
			ZFormModaliser.LastFormShownDialogForTest = null;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertEquals("Should show TaskSkillsForm", typeof(TaskSkillsForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			AssertEquals("Should rollback to Assigned if not meeting skill requirement", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("Should show TaskSkillsForm", typeof(TaskSkillsForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			AssertEquals("Should rollback to Assigned if not meeting skill requirement", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);

			task.P9_GS_NKAssignedStaffMember = "";
			// Acquire skill2
			serviceClientMock.SetupHasCompletedLearningUnit(staff, aspectPK2, true);
			ZFormModaliser.LastFormShownDialogForTest = null;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertEquals("Should show TaskSkillsForm", typeof(TaskSkillsForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			AssertEquals("Should rollback to Assigned if not meeting skill requirement", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("Should show TaskSkillsForm", typeof(TaskSkillsForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			AssertEquals("Should rollback to Assigned if not meeting skill requirement", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);

			task.P9_GS_NKAssignedStaffMember = "";
			// All skill acquired - this time should not expect the TaskSkillsForm
			serviceClientMock.SetupHasCompletedLearningUnit(staff, aspectPK3, true);
			ZFormModaliser.LastFormShownDialogForTest = null;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertEquals("Should not show TaskSkillsForm", null, ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals("Should stay working if skill requirements are met", ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("Should not show TaskSkillsForm", null, ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals("Should stay working if skill requirements are met", ProcessTaskStatusCodeList.Codes.Closed, task.P9_Status);
		}
	}

	public static void TrySetStatusAndExpectOutcome(ProcessTask task, string newStatus, bool shouldShowTaskSkillsForm)
	{
		ZFormModaliser.LastFormShownDialogForTest = null;
		var reloadedTask = task.Factory.CreateNewFactory().Load<ProcessTask>(task.PK); // Refreshes the cached API client
		reloadedTask.P9_Status = newStatus;
		if (shouldShowTaskSkillsForm)
		{
			Assertion.AssertType<TaskSkillsForm>("Should have shown TaskSkillsForm", ZFormModaliser.LastFormShownDialogForTest);
			Assertion.AssertEquals("Should rollback to Assigned if ASSESS requirements are not met", ProcessTaskStatusCodeList.Codes.Assigned, reloadedTask.P9_Status);
		}
		else
		{
			Assertion.AssertEquals("Should allow status to be changed when ASSESS requirements are met", newStatus, reloadedTask.P9_Status);
		}
	}
}
