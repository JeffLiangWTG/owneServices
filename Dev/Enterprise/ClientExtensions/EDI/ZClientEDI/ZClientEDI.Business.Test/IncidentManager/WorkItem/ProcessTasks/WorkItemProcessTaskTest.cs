using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.Service;
using Enterprise.BufferManagement.Service.Shared.Task.Dtos.Lifecycle;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Client.EDI.TfsRest;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WTG.DevTools.Definitions;
using WTG.DevTools.ServiceClient.Assess;
using WTG.DevTools.ServiceClient.Common;
using ZClientEDI.Business.Test;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public static class EDITaskTypes_ForTest
	{
		public const string TaskCodingOfFunctionality = "CDF";
	}

	[TestedType(typeof(WorkItemProcessTask))]
	public class WorkItemProcessTaskTest : ProcessManagement.Business.Test.WorkItemProcessTaskTest
	{
		[ExpectNoExceptions]
		public void TestClaimCapabilityTaskRequiringAssessCourse()
		{
			var aspectPK = Guid.NewGuid();

			WorkItemProcessTaskTestHelper.SetupReviewTasksRegistry("CBC");
			var staff = MasterFilesTestHelper.CreateStaff(Factory, "DEA", "Davey", "dave@something.com");
			var capability = MasterFilesTestHelper.CreateCapability(Factory, "ABC", "abc", staff);

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var task = workItem.WorkflowItems.AddNew();
			task.P9_G4_RequiredCapability = capability.PK;
			task.P9_Type = "CBC";
			WorkItemProcessTaskTestHelper.AddRequiredAspects(task, aspectPK);
			Factory.Save();

			using (AssessTestSetupHelper.SetupForEnsureEnrolled(staff, aspectPK))
			{
				var serviceClientMock = Mock.Get(ObjectFactory.Get<IAssessServiceClient>());
				task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
				Factory.Save();
				serviceClientMock.Verify(c => c.EnsureEnrolledAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<EnrolmentParameters>(), It.IsAny<ServiceRequestOptions>()), Times.Once());
			}
		}

		public void TestClaimCapabilityTaskRequiringAssessCourse_WhenTaskNotSavedSuccessfully()
		{
			var aspectPK = Guid.NewGuid();

			WorkItemProcessTaskTestHelper.SetupReviewTasksRegistry("CBC");
			var staff = MasterFilesTestHelper.CreateStaff(Factory, "DEA", "Davey", "dave@something.com");
			var capability = MasterFilesTestHelper.CreateCapability(Factory, "ABC", "abc", staff);

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var task = workItem.WorkflowItems.AddNew();
			task.P9_G4_RequiredCapability = capability.PK;
			task.P9_Type = "CBC";
			WorkItemProcessTaskTestHelper.AddRequiredAspects(task, aspectPK);
			Factory.Save();

			using (AssessTestSetupHelper.SetupForEnsureEnrolled(staff, aspectPK))
			{
				var serviceClientMock = Mock.Get(ObjectFactory.Get<IAssessServiceClient>());
				task.P9_OC = ZGuid.NewZGuid(); // Invalid FK to prevent successful save
				task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
				AssertExceptionThrown<ZSaveException>(Factory.Save);
				serviceClientMock.Verify(c => c.EnsureEnrolledAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<EnrolmentParameters>(), It.IsAny<ServiceRequestOptions>()), Times.Never());
			}
		}

		public void TestTryStart_WhenTaskRequiresAssessCourse()
		{
			var aspectPK = Guid.NewGuid();

			WorkItemProcessTaskTestHelper.SetupReviewTasksRegistry("CBC");
			WorkItemProcessTaskTestHelper.SetupLearningTaskRegistry("CBC");
			var staff = MasterFilesTestHelper.CreateStaff(Factory, "DEA", "Davey", "dave@something.com");
			var capability = MasterFilesTestHelper.CreateCapability(Factory, "ABC", "abc", staff);

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var task = workItem.WorkflowItems.AddNew();
			task.P9_G4_RequiredCapability = capability.PK;
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task.P9_Type = "CBC";
			task.P9_Description = "Test";
			WorkItemProcessTaskTestHelper.AddRequiredAspects(task, aspectPK);
			Factory.Save();

			using (AssessTestSetupHelper.SetupHasCompletedLearningUnits(staff, (aspectPK, false)))
			{	
				var taskLifecycleService = new TaskLifecycleService();
				var result = taskLifecycleService.TryStart(task.PK.ToGuid(), new StartTaskRequest(), out var businessResponse);

				Assert(!result);
				AssertEquals("You do not have the required ASSESS competencies to work on this task.", businessResponse.Message.Text, TaskLifecycleService.BusinessMessages.StaffLacksRequiredAssessCompetency.Text);
			}
		}

		public void TestCreateSkillLearningTask()
		{
			var aspectPK1 = Guid.NewGuid();
			var aspectPK2 = Guid.NewGuid();
			var aspectPK3 = Guid.NewGuid();

			WorkItemProcessTaskTestHelper.SetupReviewTasksRegistry("CBC");
			var learningTaskType = "ASS";
			WorkItemProcessTaskTestHelper.SetupLearningTaskRegistry(learningTaskType);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "IDT";

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var task = workItem.WorkflowItems.AddNew();
			task.P9_Description = "Barking";
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task.P9_Type = "CBC";

			WorkItemProcessTaskTestHelper.AddRequiredAspects(task, aspectPK1, aspectPK2, aspectPK3);
			Factory.Save();

			var learningTask = WorkItemProcessTask.CreateSkillLearningTask(task);

			AssertEquals(task.P9_GS_NKAssignedStaffMember, learningTask.P9_GS_NKAssignedStaffMember);
			AssertEquals(task.P9_G4_RequiredCapability, learningTask.P9_G4_RequiredCapability);
			AssertEquals(learningTaskType, learningTask.P9_Type);
			AssertEquals("Learning: Barking", learningTask.P9_Description);
			AssertEquals(task.P9_Sequence + 1, learningTask.P9_Sequence);
			AssertEquals(new ZInt(10).GetDateTimeFromMinutes(), learningTask.P9_EstDuration);

			// wont add another if already exists
			var learningTask2 = WorkItemProcessTask.CreateSkillLearningTask(task);
			AssertEquals(learningTask.PK, learningTask2.PK);
		}

		public void TestDeleteCallsRefresh()
		{
			var workItem = Factory.New<NewWorkItem>();
			var task1 = workItem.WorkflowItems.AddNew();
			var task2 = workItem.WorkflowItems.AddNew();
			task1.P9_Status = "CLS";
			task2.Delete();

			AssertEquals("CLS", workItem.WKI_Status);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			NewWorkItem workItem = Factory.New<NewWorkItem>();
			return workItem.WorkflowItems.AddNew();
		}

		public void TestClone_SetsSH0ToSHV()
		{
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var task = workItem.WorkflowItems.AddNew();
			task.P9_Type = "SH0";
			task.P9_Description = "Description";

			var collection = new CategorisedWorkflowTaskTypesCollection();
			var parent = collection.AddNew();
			parent.Code = task.WorkflowType;

			var shvTaskType = parent.TaskTypes.AddNew();
			shvTaskType.Code = "SHV";
			shvTaskType.Description = (NoResString)"Shelf Test";

			using (WorkflowDataRegistry.Instance.TaskTypes.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection))
			{
				var newTask = (ProcessTask)task.Clone();

				AssertEquals("SH0 tasks should be set to SHV", "SHV", newTask.P9_Type.ToString());
				AssertEquals("Description should be defaulted", "Shelf Test", newTask.P9_Description);
			}
		}

		public void TestClone_SetsCH0ToCHK()
		{
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var task = workItem.WorkflowItems.AddNew();
			task.P9_Type = "CH0";
			task.P9_Description = "Description";

			var collection = new CategorisedWorkflowTaskTypesCollection();
			var parent = collection.AddNew();
			parent.Code = task.WorkflowType;

			var chkTaskType = parent.TaskTypes.AddNew();
			chkTaskType.Code = "CHK";
			chkTaskType.Description = (NoResString)"Checkin";

			using (WorkflowDataRegistry.Instance.TaskTypes.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection))
			{
				var newTask = (ProcessTask)task.Clone();

				AssertEquals("CH0 tasks should be set to CHK", "CHK", newTask.P9_Type.ToString());
				AssertEquals("Description should be defaulted", "Checkin", newTask.P9_Description);
			}
		}

		public void TestClone_SetsUA0ToUAT()
		{
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var task = workItem.WorkflowItems.AddNew();
			task.P9_Type = "UA0";
			task.P9_Description = "Description";

			var collection = new CategorisedWorkflowTaskTypesCollection();
			var parent = collection.AddNew();
			parent.Code = task.WorkflowType;

			var chkTaskType = parent.TaskTypes.AddNew();
			chkTaskType.Code = "UAT";
			chkTaskType.Description = (NoResString)"UAT Shelf";

			using (WorkflowDataRegistry.Instance.TaskTypes.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection))
			{
				var newTask = (ProcessTask)task.Clone();

				AssertEquals("UA0 tasks should be set to UAT", "UAT", newTask.P9_Type.ToString());
				AssertEquals("Description should be defaulted", "UAT Shelf", newTask.P9_Description);
			}
		}

		public void TestClone_SetAS0toASP()
		{
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var task = workItem.WorkflowItems.AddNew();
			task.P9_Type = "AS0";
			task.P9_Description = "Description";

			var collection = new CategorisedWorkflowTaskTypesCollection();
			var parent = collection.AddNew();
			parent.Code = task.WorkflowType;

			var chkTaskType = parent.TaskTypes.AddNew();
			chkTaskType.Code = "ASP";
			chkTaskType.Description = (NoResString)"Aspect Only Shelf";

			using (WorkflowDataRegistry.Instance.TaskTypes.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection))
			{
				var newTask = (ProcessTask)task.Clone();

				AssertEquals("AS0 tasks should be set to ASP", "ASP", newTask.P9_Type.ToString());
				AssertEquals("Description should be defaulted", "Aspect Only Shelf", newTask.P9_Description);
			}
		}
		public void TestClone_SetsJP0ToJPR()
		{
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var task = workItem.WorkflowItems.AddNew();
			task.P9_Type = "JP0";
			task.P9_Description = "Description";

			var collection = new CategorisedWorkflowTaskTypesCollection();
			var parent = collection.AddNew();
			parent.Code = task.WorkflowType;

			var shvTaskType = parent.TaskTypes.AddNew();
			shvTaskType.Code = "JPR";
			shvTaskType.Description = (NoResString)"Default Description For JPR Task";

			using (WorkflowDataRegistry.Instance.TaskTypes.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection))
			{
				var newTask = (ProcessTask)task.Clone();

				AssertEquals("JP0 tasks should be set to JPR", "JPR", newTask.P9_Type.ToString());
				AssertEquals("Description should be defaulted", "Default Description For JPR Task", newTask.P9_Description);
			}
		}

		public void TestP9_Status_UpdateParentStatus()
		{
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var task = workItem.WorkflowItems.AddNew();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			Factory.Save();
			AssertEquals(ProcessTaskStatusCodeList.Codes.Open, workItem.WKI_Status);

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var taskLoad2 = factory2.Load<ProcessManagement.Business.WorkItemProcessTask>(task.PK);
			taskLoad2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			factory2.Save();

			var factory3 = new BusinessObjectFactory() { RefreshEnabled = false };
			var workItemLoad3 = factory3.Load<NewWorkItem>(workItem.PK);
			AssertEquals("parent status is updated even if parent is not explicitly loaded", ProcessTaskStatusCodeList.Codes.Closed, workItemLoad3.WKI_Status);
		}

		public void TestValidateAssignedStaffMember_ShouldOnlyBeCalledIfHasChanges() // This was the cause of a massive memory leak in BMS service task
		{
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.RelatedItems.Add(workItem);

			var task = workItem.WorkflowItems.AddNew();

			var field = typeof(ProcessManagement.Business.WorkItem).GetField("relatedItems", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			AssertNotNull(field);
			AssertNull(field.GetValue(workItem));

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			newFactory.SuspendValidation();

			var loadedWorkItem = newFactory.Load<NewWorkItem>(workItem.PK);
			var loadedTask = loadedWorkItem.WorkflowItems[0];
			AssertNotNull(loadedTask);

			newFactory.Save();

			AssertNull(field.GetValue(loadedWorkItem));
		}

		public void TestUpdateAssignedStaff()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "bug.killer";
			staff.GS_Code = "BK";

			Factory.Save();

			WorkItemProcessTask task = Factory.New<WorkItemProcessTask>();

			AssertEquals("Precondition: Assigned Staff is Empty", ZString.Empty, task.P9_GS_NKAssignedStaffMember);
			task.UpdateAssignedStaff("CORP\\bug.killer");
			AssertEquals("Assigned Staff should updated when shelf owner has been changed", "BK", task.P9_GS_NKAssignedStaffMember);
		}

		public void TestIsCheckInTask()
		{
			WorkItemProcessTask task = Factory.New<WorkItemProcessTask>();
			foreach (var ring in ReleaseRings.List())
			{
				task.P9_Type = ring.CheckinTask;
				AssertEquals("IsCheckInTask", true, task.IsCheckInTask);
			}
			task.P9_Type = "COD";
			AssertEquals("IsCheckInTask", false, task.IsCheckInTask);
			task.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			task.P9_GS_NKAssignedStaffMember = "DAT";
			AssertEquals("IsCheckInTask should be false when its assigned to DAT", false, task.IsCheckInTask);
			task.P9_Type = WorkItemProcessTask.ShelfsetTestTask;
			AssertEquals("IsCheckInTask", false, task.IsCheckInTask);
		}

		public void TestIsShelfsetTask()
		{
			WorkItemProcessTask task = Factory.New<WorkItemProcessTask>();
			task.P9_GS_NKAssignedStaffMember = "ABC";
			foreach (var ring in ReleaseRings.List())
			{
				task.P9_Type = ring.CheckinTask;
				AssertEquals("IsShelfsetTask", true, task.IsShelfsetTask);
			}
			task.P9_Type = "COD";
			AssertEquals("IsShelfsetTask", false, task.IsShelfsetTask);
			task.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			task.P9_GS_NKAssignedStaffMember = "DAT";
			AssertEquals("IsShelfsetTask should be false when its assigned to DAT", false, task.IsShelfsetTask);
			task.P9_Type = WorkItemProcessTask.ShelfsetTestTask;
			AssertEquals("IsShelfsetTask", true, task.IsShelfsetTask);
		}

		public void TestIsShelfsetTaskEvenWhenAssignedToDAT()
		{
			WorkItemProcessTask task = Factory.New<WorkItemProcessTask>();
			foreach (var ring in ReleaseRings.List())
			{
				task.P9_Type = ring.CheckinTask;
				AssertEquals("IsShelfsetTaskEvenWhenAssignedToDAT", true, task.IsShelfsetTaskEvenWhenAssignedToDAT);
			}
			task.P9_Type = "COD";
			AssertEquals("IsShelfsetTaskEvenWhenAssignedToDAT", false, task.IsShelfsetTaskEvenWhenAssignedToDAT);
			task.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			task.P9_GS_NKAssignedStaffMember = "DAT";
			AssertEquals("IsShelfsetTaskEvenWhenAssignedToDAT should be true even when it's assigned to DAT", true, task.IsShelfsetTaskEvenWhenAssignedToDAT);
			task.P9_Type = WorkItemProcessTask.ShelfsetTestTask;
			AssertEquals("IsShelfsetTaskEvenWhenAssignedToDAT", true, task.IsShelfsetTaskEvenWhenAssignedToDAT);
		}

		public void TestHasShelfToAdd()
		{
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			GlbStaff newStaff = Factory.NewWithValidTestData<GlbStaff>();
			WorkItemProcessTask task = AddNewTask(workItem, newStaff.GS_Code, EDITaskTypes.TaskCheckin, ProcessTaskStatusCodeList.Codes.Assigned);
			Assert("HasShelfToAdd", !task.HasShelfToAdd);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Assert("HasShelfToAdd", !task.HasShelfToAdd);
			task.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			Assert("HasShelfToAdd", task.HasShelfToAdd);
			Factory.Save();
			Assert("HasShelfToAdd", !task.HasShelfToAdd);

			task.P9_Type = WorkItemProcessTask.ShelfsetTestTask;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			Assert("HasShelfToAdd", task.HasShelfToAdd);

			task.P9_GS_NKAssignedStaffMember = string.Empty;
			Assert("HasShelfToAdd", !task.HasShelfToAdd);
		}

		public void TestChangeUnrelatedProperties_ShouldNotReshelve()
		{
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var jobHeader = ProcessJobHeader.GetForParent(workItem, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "Workflow 1";
			workflow2.FH_CompletionStatement = "Workflow 2";
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "LKS";
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var task = AddNewTask(workItem, staff1.GS_Code, EDITaskTypes.TaskActiveShelfTest, ProcessTaskStatusCodeList.Codes.Assigned);
			task.P9_FH_ProcessHeader = workflow1.PK;
			Factory.Save();
			var currentShelfPk = GetShelfPk(task);

			task.P9_CardNote = "Grizzly bear underwear";
			task.P9_EstimatedHandoverTime = ZDateTimeOffset.Now.AddHours(1);
			task.P9_EstDuration = new ZDateTime(2016, 1, 1, 1, 1, 1);
			task.P9_EstimatedTimeToComplete = new ZDateTime(2016, 1, 1, 1, 1, 1);
			task.P9_EstimateVariationFactor = 4;
			task.P9_FH_ProcessHeader = workflow2.PK;
			task.P9_G4_RequiredCapability = capability.PK;
			task.P9_Sequence = 30;
			Factory.Save();
			AssertEquals("The task should not create a new shelf because only non-shelf-related details were changed, and yet...", currentShelfPk, GetShelfPk(task));

			task.P9_Type = EDITaskTypes.TaskActiveCheckin;
			Factory.Save();

			var newShelfPk = GetShelfPk(task);
			AssertNotEquals("The task should create a new shelf because a shelf-related detail (Task type) was changed, and yet...", currentShelfPk, newShelfPk);

			currentShelfPk = newShelfPk;
			task.P9_Description = "WORKITEMNUMBER";
			Factory.Save();

			newShelfPk = GetShelfPk(task);
			AssertNotEquals("The task should create a new shelf because a shelf-related detail (Description) was changed, and yet...", currentShelfPk, newShelfPk);

			currentShelfPk = newShelfPk;
			task.P9_GS_NKAssignedStaffMember = staff2.GS_Code;
			Factory.Save();

			newShelfPk = GetShelfPk(task);
			AssertNotEquals("The task should create a new shelf because a shelf-related detail (Staff) was changed, and yet...", currentShelfPk, newShelfPk);

			currentShelfPk = newShelfPk;
			task.P9_NotesAsString = "Gluten Frake";
			Factory.Save();

			newShelfPk = GetShelfPk(task);
			AssertNotEquals("The task should create a new shelf because a shelf-related detail (Notes) was changed, and yet...", currentShelfPk, newShelfPk);

			currentShelfPk = newShelfPk;
			task.P9_Notes = ORtfTextUtil.TextToRtfBytes(task.P9_NotesAsString, (int)ORtfTextUtil.DefaultFontSize + 1);
			Factory.Save();
			newShelfPk = GetShelfPk(task);
			AssertEquals("The task should not create a new shelf because although the Notes RTF changed, the text didnt't change", currentShelfPk, newShelfPk);
		}

		static Guid GetShelfPk(WorkItemProcessTask task)
		{
			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				using (var command = connection.Command($"SELECT TOP 1 UH_PK FROM UserTestHeader WHERE UH_P9 = '{task.PK}' ORDER BY UH_DateRecordAdded DESC"))
				{
					return (Guid)command.ExecuteScalar();
				}
			}
		}

		public void TestIsOpen()
		{
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			WorkItemProcessTask task = workItem.WorkflowItems.AddNew();

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("IsOpen", false, task.IsOpen);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertEquals("IsOpen", false, task.IsOpen);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			AssertEquals("IsOpen", true, task.IsOpen);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			AssertEquals("IsOpen", true, task.IsOpen);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			AssertEquals("IsOpen", true, task.IsOpen);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertEquals("IsOpen", true, task.IsOpen);
		}

		//TODO uncommment when form is in accessible DLL
		//[GuiTest]
		//public new void TestDelete()
		//{
		//	var staff = Factory.New<GlbStaff>();
		//	staff.GS_Code = "TST";
		//	staff.GS_LoginName = "test";
		//	staff.GS_FullName = "Test Test";

		//	tfsContextForTest.AddShelves(TfsRestContext.DomainNames[0] + "\\" + staff.GS_LoginName, "another shelf for test");

		//	ShelvesetToolsTest.AddCheckInTaskTypes();
		//	NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();

		//	workItem.WKI_Summary = "xxx";

		//	WorkItemProcessTask task = (WorkItemProcessTask)workItem.WorkflowItems.AddNew();

		//	task.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
		//	task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
		//	task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
		//	task.P9_Description = "another shelf for test";

		//	using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
		//	{
		//		var crikeyDataAccess = CrikeyDataAccessFactory.GetInstance(connection);

		//		EDIShelvesetInfo shelfInfo = new EDIShelvesetInfo("", "", "", task);
		//		AssertEquals("Shelf for this ProcessTask should NOT be scheduled yet", false, crikeyDataAccess.IsShelfScheduled(shelfInfo));

		//		using (NewWorkItemForm form = new NewWorkItemForm(workItem))
		//		{
		//			form.FireSaveButton();
		//			AssertNoErrors(workItem);

		//			AssertEquals("Shelf for this ProcessTask should be scheduled already", true, crikeyDataAccess.IsShelfScheduled(shelfInfo));
		//			task.Delete();
		//			AssertEquals("Shelf for this ProcessTask should be still be scheduled", true, crikeyDataAccess.IsShelfScheduled(shelfInfo));

		//			form.FireSaveButton();
		//			AssertEquals("Shelf for this ProcessTask should be removed from schedule", false, crikeyDataAccess.IsShelfScheduled(shelfInfo));
		//		}
		//	}
		//}

		public void TestDescriptionFieldType()
		{
			WorkItemProcessTask task = Factory.New<WorkItemProcessTask>();
			AssertEquals("task.DescriptionFieldType", nameof(FieldType.Text), task.DescriptionFieldType);

			task.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			task.P9_GS_NKAssignedStaffMember = "ABC";
			AssertEquals("task.DescriptionFieldType", nameof(FieldType.TextCodeFindBox), task.DescriptionFieldType);

			task.P9_Type = WorkItemProcessTask.UATBuildShelfTask;
			AssertEquals("task.DescriptionFieldType", nameof(FieldType.TextCodeFindBox), task.DescriptionFieldType);
		}

		public void TestP9_Type()
		{
			WorkItemProcessTask task = Factory.New<WorkItemProcessTask>();
			task.P9_Type = "COD";

			AssertEquals("P9_Description", task.TypeDescription, task.P9_Description);

			task.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			AssertEquals("P9_Description", ZString.Empty, task.P9_Description);
		}

		public void TestSuspendedTasksForAssignedUser()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "SCW";

			NewWorkItem workItem1 = Factory.NewWithValidTestData<NewWorkItem>();
			workItem1.WKI_Summary = "Testing";

			WorkItemProcessTask task1 = workItem1.WorkflowItems.AddNew();
			task1.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			task1.P9_Description = "A shelf for test";
			task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			WorkItemProcessTask task2 = workItem1.WorkflowItems.AddNew();
			task1.P9_Type = EDITaskTypes_ForTest.TaskCodingOfFunctionality;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			task1.P9_Description = "Coding";
			task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			NewWorkItem workItem2 = Factory.NewWithValidTestData<NewWorkItem>();
			workItem2.WKI_Summary = "Testing Again";

			WorkItemProcessTask task3 = workItem2.WorkflowItems.AddNew();
			task3.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.GPR).CheckinTask;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			task3.P9_Description = "Another shelf for test";
			task3.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			WorkItemProcessTask task4 = workItem2.WorkflowItems.AddNew();
			task4.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.STD).CheckinTask;
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			task4.P9_Description = "Still a shelf for test";
			task4.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			AssertEquals("Shoulde only contains one task", 1, task4.SuspendedTasksForAssignedUser.Count);
			AssertEquals("Shoulde be the coding task", "Coding", task4.SuspendedTasksForAssignedUser[0].P9_Description);
		}

		public void TestIsShelfCheckedIn()
		{
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			GlbStaff newStaff = Factory.NewWithValidTestData<GlbStaff>();
			WorkItemProcessTask manualCheckIn = AddNewTask(workItem, newStaff.GS_Code, EDITaskTypes.TaskCheckin, ProcessTaskStatusCodeList.Codes.Closed);

			List<WorkItemProcessTask> checkinTasks = new List<WorkItemProcessTask>();
			foreach (var checkinType in ReleaseRingsLookup.CheckInTaskTypes)
			{
				checkinTasks.Add(AddNewTask(workItem, newStaff.GS_Code, checkinType, ProcessTaskStatusCodeList.Codes.Closed));
			}
			WorkItemProcessTask anotherAlpCheckIn = AddNewTask(workItem, newStaff.GS_Code, ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask, ProcessTaskStatusCodeList.Codes.Assigned);
			Factory.Save();

			Assert(!manualCheckIn.IsShelfCheckedIn);
			foreach (var task in checkinTasks)
			{
				Assert(task.IsShelfCheckedIn);
			}
			Assert(!anotherAlpCheckIn.IsShelfCheckedIn);
		}

		WorkItemProcessTask AddNewTask(NewWorkItem workItem, string staffCode, string type, string status)
		{
			WorkItemProcessTask result = workItem.WorkflowItems.AddNew();
			result.P9_GS_NKAssignedStaffMember = staffCode;
			result.P9_Type = type;
			result.P9_Status = status;
			return result;
		}

		public void TestDefectCount()
		{
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			WorkItemProcessTask task = workItem.WorkflowItems.AddNew();
			task.DefectCount = 1;
			AssertEquals(1, task.DefectCount);

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var taskReloaded = factory2.Load<WorkItemProcessTask>(task.PK);
			AssertEquals(1, taskReloaded.DefectCount);
		}

		public void TestIsClosedOrCancelled()
		{
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			WorkItemProcessTask task = workItem.WorkflowItems.AddNew();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(true, task.IsClosedOrCancelled);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertEquals(true, task.IsClosedOrCancelled);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			AssertEquals(false, task.IsClosedOrCancelled);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			AssertEquals(false, task.IsClosedOrCancelled);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			AssertEquals(false, task.IsClosedOrCancelled);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertEquals(false, task.IsClosedOrCancelled);
		}

		public void TestIsTaskMandatoryForShelfQueue()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("FOO", "fooey");
			EDIDataRegistry.Instance.TasksMandatoryInShelfCreation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			WorkItemProcessTask task = Factory.New<WorkItemProcessTask>();
			task.P9_Type = "FOO";
			AssertEquals(true, task.IsTaskMandatoryForShelfQueue);

			task.P9_Type = "BAR";
			AssertEquals(false, task.IsTaskMandatoryForShelfQueue);
		}

		public void TestIsReviewTask()
		{
			var reviewTasks = new CodeDescriptionBoolCollection();
			reviewTasks.Add("RVW", (NoResString)"review");
			reviewTasks.Add("FRV", (NoResString)"functional review");
			EDIDataRegistry.Instance.ReviewTasks.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, reviewTasks);

			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			WorkItemProcessTask task = workItem.WorkflowItems.AddNew();

			task.P9_Type = "RVW";
			AssertEquals(true, task.IsReviewTask);
			task.P9_Type = "FRV";
			AssertEquals(true, task.IsReviewTask);
			task.P9_Type = "ZZZ";
			AssertEquals(false, task.IsReviewTask);
			task.P9_Type = EDITaskTypes.TaskCheckin;
			AssertEquals(false, task.IsReviewTask);
		}

		public void TestTaskPatchType()
		{
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var task = workItem.WorkflowItems.AddNew();

			task.P9_Type = "INV";
			AssertEquals("Tasks not for the release rings should not be patch tasks.", TaskPatchType.None, task.TaskPatchType);

			task.P9_Type = "CH0";
			AssertEquals("Alpha checkin tasks should not be patch tasks, as the checkin tasks for the non-ALP release rings work differently than the ALP ones.", TaskPatchType.None, task.TaskPatchType);

			task.P9_Type = "CH1";
			task.Iteration = ZString.Empty;
			AssertEquals("Auto patch tasks should be checkin tasks without Iteration.", TaskPatchType.Auto, task.TaskPatchType);

			task.P9_Type = "CH2";
			task.Iteration = "1";
			AssertEquals("Manual patch tasks should be checkin tasks with Iteration.", TaskPatchType.Manual, task.TaskPatchType);
		}

		public void TestLogShelfReviewWarningTime()
		{
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();

			WorkItemProcessTask shelfTask1 = workItem.WorkflowItems.AddNew();
			shelfTask1.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			shelfTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			shelfTask1.MinutesElapsedForShelfCreationTimeLog = 20;
			Factory.Save();
			AssertEquals(true, shelfTask1.Logs.HasLogWith(StmALogSchema.SL_Reference, "Shelf Review Warning:20"));

			shelfTask1.MinutesElapsedForShelfCreationTimeLog = 10;
			Factory.Save();
			AssertEquals(false, shelfTask1.Logs.HasLogWith(new ZQuery(StmALogSchema.SL_Reference, "Shelf Review Warning:10")));

			WorkItemProcessTask shelfTask2 = workItem.WorkflowItems.AddNew();
			shelfTask2.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			shelfTask2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			shelfTask2.MinutesElapsedForShelfCreationTimeLog = 40;
			Factory.Save();
			AssertEquals(true, shelfTask2.Logs.HasLogWith(StmALogSchema.SL_Reference, "Shelf Review Warning:40"));

			NewWorkItem workItem2 = Factory.NewWithValidTestData<NewWorkItem>();
			WorkItemProcessTask shelfTask = workItem2.WorkflowItems.AddNew();
			shelfTask.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			shelfTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			shelfTask.MinutesElapsedForShelfCreationTimeLog = 60;
			Factory.Save();
			AssertEquals(true, shelfTask.Logs.HasLogWith(StmALogSchema.SL_Reference, "Shelf Review Warning:60"));
		}

		public void TestIsShelfsetTestTask()
		{
			WorkItemProcessTask task = Factory.New<WorkItemProcessTask>();
			task.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			AssertEquals(false, task.IsShelfsetTestTask);
			task.P9_Type = "SH0";
			AssertEquals(true, task.IsShelfsetTestTask);
			task.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.GPR).CheckinTask;
			AssertEquals(false, task.IsShelfsetTestTask);
		}

		public void TestIsShelfUpdateSuspended()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_LoginName = "edward.onwodi";
			staff.GS_FullName = "Edward Onwodi";

			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				var crikeyDataAccess = CrikeyDataAccessFactory.GetInstance(connection);

				var workItem = Factory.New<NewWorkItem>();
				var shelfTestTask = workItem.WorkflowItems.AddNew();
				using (shelfTestTask.BeginShelfUpdateSuspender())
				{
					shelfTestTask.P9_GS_NKAssignedStaffMember = "TST";
					shelfTestTask.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
					shelfTestTask.P9_NotesAsString = "http://example.com/pullrequest/123";
					Factory.Save();

					var submission = ShelvesetTools.LoadShelfByProcessTask(shelfTestTask);
					AssertNull("Pull Request should not have been submitted", submission);
				}

				var workItem2 = Factory.New<NewWorkItem>();
				var shelfTestTask2 = workItem2.WorkflowItems.AddNew();
				shelfTestTask2.P9_GS_NKAssignedStaffMember = "TST";
				shelfTestTask2.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
				shelfTestTask2.P9_NotesAsString = "http://example.com/pullrequest/123";
				Factory.Save();

				var submission2 = ShelvesetTools.LoadShelfByProcessTask(shelfTestTask2);
				AssertNotNull("Pull Request should have been submitted", submission2);
				AssertEquals("Pull Request should have been scheduled", true, crikeyDataAccess.IsShelfScheduled(submission2));
			}
		}

		public void TestCancelSh0AndSetCh0()
		{
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			WorkItemProcessTask taskSh0 = workItem.WorkflowItems.AddNew();
			WorkItemProcessTask taskCh0 = workItem.WorkflowItems.AddNew();

			taskSh0.P9_Type = "SH0";
			taskSh0.P9_Status = "ASN";
			taskSh0.P9_Description = "WI00140537";
			taskSh0.P9_GS_NKAssignedStaffMember = "ABC";
			taskCh0.P9_Type = "CHK";
			taskCh0.P9_Status = "ASN";
			taskCh0.P9_Description = "WI00140537";
			taskCh0.P9_GS_NKAssignedStaffMember = "ABC";
			Factory.Save();

			taskSh0.P9_Status = "CAN";
			taskCh0.P9_Type = "CH0";
			Factory.Save();

			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				var crikeyDataAccess = CrikeyDataAccessFactory.GetInstance(connection);
				EDIShelvesetInfo shelfSh0 = ShelvesetTools.LoadShelfByProcessTask(taskSh0);
				EDIShelvesetInfo shelfCh0 = ShelvesetTools.LoadShelfByProcessTask(taskCh0);

				AssertNotNull("SH0 task in UserTesTHeader", shelfSh0);
				AssertNotNull("CH0 task in UserTestHeader", shelfCh0);
				AssertEquals("Shelf for SH0 task is scheduled", false, crikeyDataAccess.IsShelfScheduled(shelfSh0));
				AssertEquals("Shelf for CH0 task is scheduled", true, crikeyDataAccess.IsShelfScheduled(shelfCh0));
			}
		}

		public void TestCancelDatSubmissionWhenTaskTypeChanged()
		{
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var taskSh0 = workItem.WorkflowItems.AddNew();
			taskSh0.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			taskSh0.P9_Type = "SH0";
			taskSh0.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			taskSh0.P9_NotesAsString = "http://example.com/pullrequest/123";
			Factory.Save();

			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				var crikeyDataAccess = CrikeyDataAccessFactory.GetInstance(connection);
				var shelfSh0 = ShelvesetTools.LoadShelfByProcessTask(taskSh0);

				AssertNotNull("SH0 task in Dat", shelfSh0);
				AssertEquals(string.Empty, shelfSh0.Name);
				AssertEquals("Shelf WI00140537 is scheduled", true, crikeyDataAccess.IsShelfScheduled(shelfSh0));
				AssertEquals("Shelf is Queued", ShelfStatuses.QueuedForBranchDetection, shelfSh0.Status);
				AssertShelfComments(shelfSh0, "http://example.com/pullrequest/123");

				taskSh0.P9_Type = "UDF";
				Factory.Save();
				shelfSh0 = ShelvesetTools.LoadShelfByProcessTask(taskSh0);

				AssertNotNull("SH0 task in Dat", shelfSh0);
				AssertEquals("Shelf WI00140537 is scheduled", false, crikeyDataAccess.IsShelfScheduled(shelfSh0));
				AssertEquals("Shelf is Cancelled", ShelfStatuses.Cancelled, shelfSh0.Status);
			}
		}

		public void TestCancelDatSubmissionWhenTaskTypeChangedToAnotherSubmission()
		{
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var taskSh0 = workItem.WorkflowItems.AddNew();
			taskSh0.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			taskSh0.P9_Type = "SH0";
			taskSh0.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			taskSh0.P9_NotesAsString = "http://example.com/pullrequest/123";
			Factory.Save();

			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				var crikeyDataAccess = CrikeyDataAccessFactory.GetInstance(connection);
				var shelfSh0 = ShelvesetTools.LoadShelfByProcessTask(taskSh0);

				AssertNotNull("SH0 task in Dat", shelfSh0);
				AssertEquals(string.Empty, shelfSh0.Name);
				AssertEquals("Shelf WI00140537 is scheduled", true, crikeyDataAccess.IsShelfScheduled(shelfSh0));
				AssertEquals("Shelf is Queued", ShelfStatuses.QueuedForBranchDetection, shelfSh0.Status);
				AssertShelfComments(shelfSh0, "http://example.com/pullrequest/123");

				taskSh0.P9_Type = "UA0";
				Factory.Save();
				var allShelvesFromThisTask = LoadShelvesByProcessTaskAndShelfName(taskSh0);

				var updatedSh0 = allShelvesFromThisTask.SingleOrDefault(s => s.UserHeaderPK == shelfSh0.UserHeaderPK);
				AssertNotNull("updated Sh0 task in Dat", updatedSh0);
				var uatBuild = allShelvesFromThisTask.Except(updatedSh0).SingleOrDefault();
				AssertNotNull("uatBuild task in Dat", uatBuild);

				AssertEquals("Shelf is Cancelled", ShelfStatuses.Cancelled, updatedSh0.Status);
				AssertEquals("Shelf is Queued", ShelfStatuses.QueuedForBranchDetection, uatBuild.Status);
				AssertEquals("Shelf WI00140537 is scheduled", true, crikeyDataAccess.IsShelfScheduled(shelfSh0));
			}
		}

		public List<EDIShelvesetInfo> LoadShelvesByProcessTaskAndShelfName(WorkItemProcessTask processTask, string shelfName = null)
		{
			var results = new List<EDIShelvesetInfo>();
			var processTaskPK = processTask.PK.ToGuid();
			var sql = @"
	SELECT U1_Name, UH_PK, UH_ShelfName, UH_Type, UH_P9, UH_Status
	FROM UserTestHeader JOIN [User] ON UH_U1 = [User].U1_PK
	WHERE UH_P9 = @ProcessTaskPK
	";
			if (!string.IsNullOrEmpty(shelfName))
			{
				sql += " AND UH_ShelfName = @UH_ShelfName";
			}
			sql += " ORDER BY UH_DateRecordAdded DESC";
			using (var conn = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			using (var cmd = conn.Command(sql))
			{
				cmd.AddParameter("@ProcessTaskPK", SqlDbType.UniqueIdentifier, processTaskPK);
				if (!string.IsNullOrEmpty(shelfName))
				{
					cmd.AddParameter("@UH_ShelfName", SqlDbType.VarChar, shelfName);
				}

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var userName = (string)reader["U1_Name"];
						var userHeaderPK = (Guid)reader["UH_PK"];
						var type = (string)reader["UH_Type"];
						var status = reader["UH_Status"];
						var shelfStatus = (status == null || status == DBNull.Value) ? "" : (string)status;

						results.Add(new EDIShelvesetInfo(userName, shelfName, type, processTask)
						{
							Status = shelfStatus,
							UserHeaderPK = userHeaderPK,
						});
					}
				}
			}

			return results;
		}

		public void TestCancelDatSubmissionWhenTaskStatusChanged()
		{
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var taskSh0 = workItem.WorkflowItems.AddNew();
			taskSh0.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			taskSh0.P9_Type = "SH0";
			taskSh0.P9_Status = "ASN";
			taskSh0.P9_NotesAsString = "http://example.com/pullrequest/123";
			Factory.Save();

			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				var crikeyDataAccess = CrikeyDataAccessFactory.GetInstance(connection);
				var shelfSh0 = ShelvesetTools.LoadShelfByProcessTask(taskSh0);

				AssertNotNull("SH0 task in Dat", shelfSh0);
				AssertEquals(string.Empty, shelfSh0.Name);
				AssertShelfComments(shelfSh0, "http://example.com/pullrequest/123");
				AssertEquals("PR is scheduled", true, crikeyDataAccess.IsShelfScheduled(shelfSh0));

				taskSh0.P9_Status = "SUS";
				Factory.Save();
				shelfSh0 = ShelvesetTools.LoadShelfByProcessTask(taskSh0);

				AssertNotNull("SH0 task in Dat", shelfSh0);
				AssertEquals(string.Empty, shelfSh0.Name);
				AssertShelfComments(shelfSh0, "http://example.com/pullrequest/123");
				AssertEquals("PR is scheduled", false, crikeyDataAccess.IsShelfScheduled(shelfSh0));
			}
		}

		public void TestCancelDatSubmissionWhenTaskTypeDescriptionAndStatusChanged()
		{
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var taskSh0 = workItem.WorkflowItems.AddNew();
			taskSh0.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			taskSh0.P9_Type = "SH0";
			taskSh0.P9_Status = "ASN";
			taskSh0.P9_NotesAsString = "http://example.com/pullrequest/123";
			Factory.Save();

			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				var crikeyDataAccess = CrikeyDataAccessFactory.GetInstance(connection);
				var shelfSh0 = ShelvesetTools.LoadShelfByProcessTask(taskSh0);

				AssertNotNull("SH0 task in Dat", shelfSh0);
				AssertEquals("Shelf WI00140537 is scheduled", true, crikeyDataAccess.IsShelfScheduled(shelfSh0));
				AssertEquals(string.Empty, shelfSh0.Name);
				AssertShelfComments(shelfSh0, "http://example.com/pullrequest/123");

				taskSh0.P9_Type = "CHK";
				taskSh0.P9_Status = "SUS";
				taskSh0.P9_Description = "hmm";
				Factory.Save();
				shelfSh0 = ShelvesetTools.LoadShelfByProcessTask(taskSh0);

				AssertNotNull("SH0 task in Dat", shelfSh0);
				AssertEquals("Shelf WI00140537 is scheduled", false, crikeyDataAccess.IsShelfScheduled(shelfSh0));
			}
		}

		public void TestCancelDatSubmissionWhenChangeUser()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = "lee+ucc@wtg.com";
			staff.GS_LoginName = "lee.coady";
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var taskSh0 = workItem.WorkflowItems.AddNew();
			taskSh0.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			taskSh0.P9_Type = "SH0";
			taskSh0.P9_Status = "ASN";
			taskSh0.P9_NotesAsString = "http://example.com/pullrequest/123";
			Factory.Save();

			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				var crikeyDataAccess = CrikeyDataAccessFactory.GetInstance(connection);
				var shelfSh0 = ShelvesetTools.LoadShelfByProcessTask(taskSh0);

				AssertNotNull("SH0 task in Dat", shelfSh0);
				AssertEquals("Shelf WI00140537 is scheduled", true, crikeyDataAccess.IsShelfScheduled(shelfSh0));
				AssertEquals(string.Empty, shelfSh0.Name);
				AssertShelfComments(shelfSh0, "http://example.com/pullrequest/123");
				AssertEquals("Original Shelf Owner", TfsRestContext.DomainName + "\\" + GlbStaff.CurrentUser.GS_LoginName, shelfSh0.Owner);

				taskSh0.P9_GS_NKAssignedStaffMember = staff.GS_Code;
				Factory.Save();
				var allShelvesFromThisTask = LoadShelvesByProcessTaskAndShelfName(taskSh0);

				var updatedSh0 = allShelvesFromThisTask.SingleOrDefault(s => s.UserHeaderPK == shelfSh0.UserHeaderPK);
				AssertNotNull("updated Sh0 task in Dat", updatedSh0);
				var newStaffSh0 = allShelvesFromThisTask.Except(updatedSh0).SingleOrDefault();
				AssertNotNull("uatBuild task in Dat", newStaffSh0);

				AssertEquals("Old Shelf is Cancelled", ShelfStatuses.Cancelled, updatedSh0.Status);
				AssertEquals("Old Shelf Owner", TfsRestContext.DomainName + "\\" + GlbStaff.CurrentUser.GS_LoginName, updatedSh0.Owner);
				AssertEquals("New Shelf is Queued", ShelfStatuses.QueuedForBranchDetection, newStaffSh0.Status);
				AssertEquals("New Shelf Owner", TfsRestContext.DomainName + "\\lee.coady", newStaffSh0.Owner);
				AssertEquals("Shelf WI00140537 is scheduled", true, crikeyDataAccess.IsShelfScheduled(shelfSh0));
			}
		}

		public void TestCancelDatSubmissionGitPullRequestChanged()
		{
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var taskSh0 = workItem.WorkflowItems.AddNew();
			taskSh0.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			taskSh0.P9_Type = "SH0";
			taskSh0.P9_Status = "ASN";
			taskSh0.P9_NotesAsString = "http://tfs.wtg.zone:8080/tfs/cargowise/_git/borderwise/pullrequest/123?_a=overview";
			Factory.Save();

			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				var crikeyDataAccess = CrikeyDataAccessFactory.GetInstance(connection);
				var shelfSh0 = ShelvesetTools.LoadShelfByProcessTask(taskSh0);

				AssertNotNull("SH0 task in Dat", shelfSh0);
				AssertEquals("PR is scheduled", true, crikeyDataAccess.IsShelfScheduled(shelfSh0));
				AssertEquals("No shelf name for a pull request", string.Empty, shelfSh0.Name);
				AssertShelfComments(shelfSh0, "http://tfs.wtg.zone:8080/tfs/cargowise/_git/borderwise/pullrequest/123?_a=overview");

				taskSh0.P9_NotesAsString = "http://tfs.wtg.zone:8080/tfs/cargowise/_git/borderwise/pullrequest/456?_a=overview";
				Factory.Save();
				var allShelvesFromThisTask = LoadShelvesByProcessTaskAndShelfName(taskSh0);

				var updatedSh0 = allShelvesFromThisTask.SingleOrDefault(s => s.UserHeaderPK == shelfSh0.UserHeaderPK);
				AssertNotNull("updated Sh0 task in Dat", updatedSh0);
				var newShelfSh0 = allShelvesFromThisTask.Except(updatedSh0).SingleOrDefault();
				AssertNotNull("uatBuild task in Dat", newShelfSh0);

				AssertEquals("Old PR is Cancelled", ShelfStatuses.Cancelled, updatedSh0.Status);
				AssertEquals("New PR is Queued", ShelfStatuses.QueuedForBranchDetection, newShelfSh0.Status);
				AssertEquals("PR is scheduled", true, crikeyDataAccess.IsShelfScheduled(shelfSh0));

				shelfSh0 = ShelvesetTools.LoadShelfByProcessTask(taskSh0);
				AssertEquals(string.Empty, shelfSh0.Name);
				AssertShelfComments(shelfSh0, "http://tfs.wtg.zone:8080/tfs/cargowise/_git/borderwise/pullrequest/456?_a=overview");
			}
		}

		public void TestUATBuildTaskSubmitsShelf()
		{
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var task = workItem.WorkflowItems.AddNew();
			task.P9_Type = "UA0";
			task.P9_Status = "ASN";
			task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task.P9_Notes = ZBlob.FromAscii(ORtfTextUtil.TextToRtf("http://example.com/pullrequest/123"));
			Factory.Save();

			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				var crikeyDataAccess = CrikeyDataAccessFactory.GetInstance(connection);
				var shelfInfo = ShelvesetTools.LoadShelfByProcessTask(task);

				AssertNotNull(shelfInfo);
				AssertEquals("UAB", shelfInfo.ActionType);
				AssertEquals("Shelf", string.Empty, shelfInfo.Name);
				AssertShelfComments(shelfInfo, "http://example.com/pullrequest/123");
				AssertEquals("Shelf for UA0 is scheduled", true, crikeyDataAccess.IsShelfScheduled(shelfInfo));
			}
		}

		public void TestAspectOnlyBuildTaskSubmitsShelf()
		{
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var task = workItem.WorkflowItems.AddNew();
			task.P9_Type = "AS0";
			task.P9_Status = "ASN";
			task.P9_Description = "Shelf";
			task.P9_NotesAsString = "http://example.com/pullrequest/123";
			task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			Factory.Save();

			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				var crikeyDataAccess = CrikeyDataAccessFactory.GetInstance(connection);
				var shelfInfo = ShelvesetTools.LoadShelfByProcessTask(task);

				AssertNotNull(shelfInfo);
				AssertEquals("ASB", shelfInfo.ActionType);
				AssertEquals(string.Empty, shelfInfo.Name);
				AssertShelfComments(shelfInfo, "http://example.com/pullrequest/123");
				AssertEquals("Shelf for AS0 is scheduled", true, crikeyDataAccess.IsShelfScheduled(shelfInfo));
			}
		}

		public void TestCrikeyFailureToSubmitShelfShouldSetToSUSStatus()
		{
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			WorkItemProcessTask task = workItem.WorkflowItems.AddNew();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			task.P9_Description = "WI00198934";
			task.P9_GS_NKAssignedStaffMember = "ABC";

			DisableCrikeyScheduleTask();

			AssertExceptionThrown<System.Data.SqlClient.SqlException>("Should fail to submit shelf", () => Factory.Save());

			AssertShelfAndTaskStatusForTask(task, null, ProcessTaskStatusCodeList.Codes.Suspended);
		}

		public void TestCrikeyFailureToSubmitShelfWhenTaskWasDeleted()
		{
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var task = workItem.WorkflowItems.AddNew();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task.P9_Description = "WI00198934";
			task.P9_GS_NKAssignedStaffMember = "ABC";
			task.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			Factory.Save();

			task.Delete();
			DisableCrikeyRemoveScheduleTask();

			AssertExceptionThrown<System.Data.SqlClient.SqlException>("Should fail to submit shelf", Factory.Save);
			AssertShelfStatus(task, "QBD");
		}

		public void TestCrikeyFailureToCancelShelfShouldSetToASNStatus()
		{
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			WorkItemProcessTask task = workItem.WorkflowItems.AddNew();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			task.P9_Description = "WI00198934";
			task.P9_GS_NKAssignedStaffMember = "ABC";
			Factory.Save();

			AssertShelfAndTaskStatusForTask(task, "QBD", ProcessTaskStatusCodeList.Codes.Assigned);

			DisableCrikeyRemoveScheduleTask();

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			AssertExceptionThrown<System.Data.SqlClient.SqlException>("Should fail to cancel shelf", () => Factory.Save());

			AssertShelfAndTaskStatusForTask(task, "QBD", ProcessTaskStatusCodeList.Codes.Assigned);
		}

		public void TestCrikeyFailureWhithMultipleTasksShelved()
		{
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			WorkItemProcessTask taskALP = workItem.WorkflowItems.AddNew();
			taskALP.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			taskALP.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			taskALP.P9_Description = "WI00198934";
			taskALP.P9_GS_NKAssignedStaffMember = "ABC";

			WorkItemProcessTask taskDPR = workItem.WorkflowItems.AddNew();
			taskDPR.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			taskDPR.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.DPR).CheckinTask;
			taskDPR.P9_Description = "WI00198934 DPR";
			taskDPR.P9_GS_NKAssignedStaffMember = "ABC";

			WorkItemProcessTask taskSTD = workItem.WorkflowItems.AddNew();
			taskSTD.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			taskSTD.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.STD).CheckinTask;
			taskSTD.P9_Description = "WI00198934 STD";
			taskSTD.P9_GS_NKAssignedStaffMember = "ABC";

			DisableCrikeyScheduleTask();

			AssertExceptionThrown<System.Data.SqlClient.SqlException>(() => Factory.Save());

			AssertShelfAndTaskStatusForTask(taskALP, null, ProcessTaskStatusCodeList.Codes.Suspended);
			AssertShelfAndTaskStatusForTask(taskDPR, null, ProcessTaskStatusCodeList.Codes.Suspended);
			AssertShelfAndTaskStatusForTask(taskSTD, null, ProcessTaskStatusCodeList.Codes.Suspended);
		}

		public void TestCrikeyFailureOnMultipleUpdatesWithOnlyOneFailShouldRollbackTransaction()
		{
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			WorkItemProcessTask taskALP = workItem.WorkflowItems.AddNew();
			taskALP.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			taskALP.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			taskALP.P9_Description = "first shelf";
			taskALP.P9_GS_NKAssignedStaffMember = "ABC";
			WorkItemProcessTask taskDPR = workItem.WorkflowItems.AddNew();
			taskDPR.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			taskDPR.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.DPR).CheckinTask;
			taskDPR.P9_Description = "second shelf";
			taskDPR.P9_GS_NKAssignedStaffMember = "ABC";
			Factory.Save();

			AssertShelfAndTaskStatusForTask(taskALP, "QBD", ProcessTaskStatusCodeList.Codes.Assigned);
			AssertShelfAndTaskStatusForTask(taskDPR, null, ProcessTaskStatusCodeList.Codes.Suspended);

			taskALP.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			taskDPR.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			DisableCrikeyScheduleTask();

			AssertExceptionThrown<System.Data.SqlClient.SqlException>("Should fail to submit shelf", () => Factory.Save());

			AssertShelfAndTaskStatusForTask(taskALP, "QBD", ProcessTaskStatusCodeList.Codes.Assigned);
			AssertShelfAndTaskStatusForTask(taskDPR, null, ProcessTaskStatusCodeList.Codes.Suspended);
		}

		public void TestCrikeyFailureShouldNotCauseSaveErrorAfterTheFirst()
		{
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			WorkItemProcessTask taskALP = workItem.WorkflowItems.AddNew();
			taskALP.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			taskALP.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			taskALP.P9_Description = "first shelf";
			taskALP.P9_GS_NKAssignedStaffMember = "ABC";
			WorkItemProcessTask taskDPR = workItem.WorkflowItems.AddNew();
			taskDPR.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			taskDPR.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.DPR).CheckinTask;
			taskDPR.P9_Description = "second shelf";
			taskDPR.P9_GS_NKAssignedStaffMember = "ABC";
			Factory.Save();

			AssertShelfAndTaskStatusForTask(taskALP, "QBD", ProcessTaskStatusCodeList.Codes.Assigned);
			AssertShelfAndTaskStatusForTask(taskDPR, null, ProcessTaskStatusCodeList.Codes.Suspended);

			taskALP.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			taskDPR.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			DisableCrikeyScheduleTask();
			DisableCrikeyRemoveScheduleTask();

			AssertExceptionThrown<System.Data.SqlClient.SqlException>("Should fail to submit/cancel shelf", () => Factory.Save());

			AssertShelfAndTaskStatusForTask(taskALP, "QBD", ProcessTaskStatusCodeList.Codes.Assigned);
			AssertShelfAndTaskStatusForTask(taskDPR, null, ProcessTaskStatusCodeList.Codes.Suspended);

			Assert("SQL failed so changing back to ASN will leave pending changes", taskALP.P9_StatusInfo.HasChanges);
			Assert("SQL failed so changing back to SUS will leave pending changes", taskDPR.P9_StatusInfo.HasChanges);
			//lets just commit these changes made, this shouldn't try to resubmit/cancel, the task status should reflect the crikey status
			AssertNoExceptionThrown("No exception when saving to previous values.", () => Factory.Save());

			AssertShelfAndTaskStatusForTask(taskALP, "QBD", ProcessTaskStatusCodeList.Codes.Assigned);
			AssertShelfAndTaskStatusForTask(taskDPR, null, ProcessTaskStatusCodeList.Codes.Suspended);
			Assert(!taskALP.P9_StatusInfo.HasChanges);
			Assert(!taskDPR.P9_StatusInfo.HasChanges);
		}

		public void TestCrikeyFailureShouldSubmitOnRetryIfProblemFixed()
		{
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			WorkItemProcessTask taskALP = workItem.WorkflowItems.AddNew();
			taskALP.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			taskALP.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			taskALP.P9_Description = "first shelf";
			taskALP.P9_GS_NKAssignedStaffMember = "ABC";
			WorkItemProcessTask taskDPR = workItem.WorkflowItems.AddNew();
			taskDPR.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			taskDPR.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.DPR).CheckinTask;
			taskDPR.P9_Description = "second shelf";
			taskDPR.P9_GS_NKAssignedStaffMember = "ABC";
			Factory.Save();

			AssertShelfAndTaskStatusForTask(taskALP, "QBD", ProcessTaskStatusCodeList.Codes.Assigned);
			AssertShelfAndTaskStatusForTask(taskDPR, null, ProcessTaskStatusCodeList.Codes.Suspended);

			taskALP.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			taskDPR.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			DisableCrikeyScheduleTask();
			DisableCrikeyRemoveScheduleTask();

			AssertExceptionThrown<System.Data.SqlClient.SqlException>("Should fail to submit/cancel shelf", () => Factory.Save());

			AssertShelfAndTaskStatusForTask(taskALP, "QBD", ProcessTaskStatusCodeList.Codes.Assigned);
			AssertShelfAndTaskStatusForTask(taskDPR, null, ProcessTaskStatusCodeList.Codes.Suspended);

			Assert("SQL failed so changing back to ASN will leave pending changes", taskALP.P9_StatusInfo.HasChanges);
			Assert("SQL failed so changing back to SUS will leave pending changes", taskDPR.P9_StatusInfo.HasChanges);

			EnableCrikeyScheduleTask();
			EnableCrikeyRemoveScheduleTask();
			//retrying without saving the status
			taskALP.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			taskDPR.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			AssertNoExceptionThrown("No exception on second saving attempt.", () => Factory.Save());

			AssertShelfAndTaskStatusForTask(taskALP, "CAN", ProcessTaskStatusCodeList.Codes.Suspended);
			AssertShelfAndTaskStatusForTask(taskDPR, "QBD", ProcessTaskStatusCodeList.Codes.Assigned);
			Assert("Save should succeed on second attempt", !taskALP.P9_StatusInfo.HasChanges);
			Assert("Save should succeed on second attempt", !taskDPR.P9_StatusInfo.HasChanges);
		}

		public void TestCrikeyFailureShouldSubmitOnRetryIfProblemFixed_WithIntermediateSave()
		{
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			WorkItemProcessTask taskALP = workItem.WorkflowItems.AddNew();
			taskALP.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			taskALP.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			taskALP.P9_Description = "first shelf";
			taskALP.P9_GS_NKAssignedStaffMember = "ABC";
			WorkItemProcessTask taskDPR = workItem.WorkflowItems.AddNew();
			taskDPR.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			taskDPR.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.DPR).CheckinTask;
			taskDPR.P9_Description = "second shelf";
			taskDPR.P9_GS_NKAssignedStaffMember = "ABC";
			Factory.Save();

			AssertShelfAndTaskStatusForTask(taskALP, "QBD", ProcessTaskStatusCodeList.Codes.Assigned);
			AssertShelfAndTaskStatusForTask(taskDPR, null, ProcessTaskStatusCodeList.Codes.Suspended);

			taskALP.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			taskDPR.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			DisableCrikeyScheduleTask();
			DisableCrikeyRemoveScheduleTask();

			AssertExceptionThrown<System.Data.SqlClient.SqlException>("Should fail to submit/cancel shelf", () => Factory.Save());

			AssertShelfAndTaskStatusForTask(taskALP, "QBD", ProcessTaskStatusCodeList.Codes.Assigned);
			AssertShelfAndTaskStatusForTask(taskDPR, null, ProcessTaskStatusCodeList.Codes.Suspended);

			Assert("SQL failed so changing back to ASN will leave pending changes", taskALP.P9_StatusInfo.HasChanges);
			Assert("SQL failed so changing back to SUS will leave pending changes", taskDPR.P9_StatusInfo.HasChanges);

			AssertNoExceptionThrown("No exception when saving to previous values.", () => Factory.Save());

			AssertShelfAndTaskStatusForTask(taskALP, "QBD", ProcessTaskStatusCodeList.Codes.Assigned);
			AssertShelfAndTaskStatusForTask(taskDPR, null, ProcessTaskStatusCodeList.Codes.Suspended);
			Assert(!taskALP.P9_StatusInfo.HasChanges);
			Assert(!taskDPR.P9_StatusInfo.HasChanges);

			EnableCrikeyScheduleTask();
			EnableCrikeyRemoveScheduleTask();
			//retrying after saving the status between tries
			taskALP.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			taskDPR.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			AssertNoExceptionThrown("No exception on second saving attempt.", () => Factory.Save());

			AssertShelfAndTaskStatusForTask(taskALP, "CAN", ProcessTaskStatusCodeList.Codes.Suspended);
			AssertShelfAndTaskStatusForTask(taskDPR, "QBD", ProcessTaskStatusCodeList.Codes.Assigned);
			Assert("Save should succeed on last save attempt", !taskALP.P9_StatusInfo.HasChanges);
			Assert("Save should succeed on last save attempt", !taskDPR.P9_StatusInfo.HasChanges);
		}

		public void TestCrikeyFailureWithMultipleFailingAttempts()
		{
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			WorkItemProcessTask taskALP = workItem.WorkflowItems.AddNew();
			taskALP.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			taskALP.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			taskALP.P9_Description = "first shelf";
			taskALP.P9_GS_NKAssignedStaffMember = "ABC";
			WorkItemProcessTask taskDPR = workItem.WorkflowItems.AddNew();
			taskDPR.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			taskDPR.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.DPR).CheckinTask;
			taskDPR.P9_Description = "second shelf";
			taskDPR.P9_GS_NKAssignedStaffMember = "ABC";
			Factory.Save();

			AssertShelfAndTaskStatusForTask(taskALP, "QBD", ProcessTaskStatusCodeList.Codes.Assigned);
			AssertShelfAndTaskStatusForTask(taskDPR, null, ProcessTaskStatusCodeList.Codes.Suspended);

			DisableCrikeyScheduleTask();
			DisableCrikeyRemoveScheduleTask();
			//attempt 1
			taskALP.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			taskDPR.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			AssertExceptionThrown<System.Data.SqlClient.SqlException>("Should fail to submit/cancel shelf", () => Factory.Save());

			AssertShelfAndTaskStatusForTask(taskALP, "QBD", ProcessTaskStatusCodeList.Codes.Assigned);
			AssertShelfAndTaskStatusForTask(taskDPR, null, ProcessTaskStatusCodeList.Codes.Suspended);

			Assert("SQL failed so changing back to ASN will leave pending changes", taskALP.P9_StatusInfo.HasChanges);
			Assert("SQL failed so changing back to SUS will leave pending changes", taskDPR.P9_StatusInfo.HasChanges);

			AssertNoExceptionThrown("No exception when saving to previous values.", () => Factory.Save());

			AssertShelfAndTaskStatusForTask(taskALP, "QBD", ProcessTaskStatusCodeList.Codes.Assigned);
			AssertShelfAndTaskStatusForTask(taskDPR, null, ProcessTaskStatusCodeList.Codes.Suspended);
			Assert(!taskALP.P9_StatusInfo.HasChanges);
			Assert(!taskDPR.P9_StatusInfo.HasChanges);
			//attempt 2 after saving changes between attempts
			taskALP.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			taskDPR.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			AssertExceptionThrown<System.Data.SqlClient.SqlException>("Should fail to submit/cancel shelf", () => Factory.Save());

			AssertShelfAndTaskStatusForTask(taskALP, "QBD", ProcessTaskStatusCodeList.Codes.Assigned);
			AssertShelfAndTaskStatusForTask(taskDPR, null, ProcessTaskStatusCodeList.Codes.Suspended);

			Assert("SQL failed so changing back to ASN will leave pending changes", taskALP.P9_StatusInfo.HasChanges);
			Assert("SQL failed so changing back to SUS will leave pending changes", taskDPR.P9_StatusInfo.HasChanges);

			AssertShelfAndTaskStatusForTask(taskALP, "QBD", ProcessTaskStatusCodeList.Codes.Assigned);
			AssertShelfAndTaskStatusForTask(taskDPR, null, ProcessTaskStatusCodeList.Codes.Suspended);
			//attempt 3 without saving
			taskALP.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			taskDPR.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			AssertExceptionThrown<System.Data.SqlClient.SqlException>("Should fail to submit/cancel shelf", () => Factory.Save());

			AssertShelfAndTaskStatusForTask(taskALP, "QBD", ProcessTaskStatusCodeList.Codes.Assigned);
			AssertShelfAndTaskStatusForTask(taskDPR, null, ProcessTaskStatusCodeList.Codes.Suspended);

			Assert("SQL failed so changing back to ASN will leave pending changes", taskALP.P9_StatusInfo.HasChanges);
			Assert("SQL failed so changing back to SUS will leave pending changes", taskDPR.P9_StatusInfo.HasChanges);

			//attempt 4 after problem is fixed
			EnableCrikeyScheduleTask();
			EnableCrikeyRemoveScheduleTask();
			taskALP.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			taskDPR.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			AssertNoExceptionThrown("No exception on saving and successfus submit/cancel of shelf.", () => Factory.Save());

			AssertShelfAndTaskStatusForTask(taskALP, "CAN", ProcessTaskStatusCodeList.Codes.Suspended);
			AssertShelfAndTaskStatusForTask(taskDPR, "QBD", ProcessTaskStatusCodeList.Codes.Assigned);
			Assert("Save should succeed leaving no pending changes", !taskALP.P9_StatusInfo.HasChanges);
			Assert("Save should succeed leaving no pending changes", !taskDPR.P9_StatusInfo.HasChanges);
		}

		public void TestEnsureRequiredAspectReviewExists_ExceptionNotThrownForSkill()
		{
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var taskCh0 = workItem.WorkflowItems.AddNew();

			AssertNoExceptionThrown(delegate
			{ taskCh0.EnsureRequiredAspectReviewExists(null, "", ""); });
		}

		public void TestEnsureRequiredAspectReviewExists_TaskNotAddedForSkill()
		{
			var aspectName = Guid.NewGuid().ToString("N").Substring(0, 16);
			var initialTaskNote = Guid.NewGuid().ToString();
			var additionalTaskNote = Guid.NewGuid().ToString();
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.Capabilities.Add(capability);

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var jobHeader = ProcessJobHeader.GetForParent(workItem, Factory, addDefaultProcessHeaderIfNone: true);
			var taskSh0 = workItem.WorkflowItems.AddNew();
			taskSh0.P9_Type = WorkItemProcessTask.ShelfsetTestTask;
			taskSh0.P9_Description = "shelf test";
			taskSh0.P9_Sequence = 100;
			taskSh0.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			taskSh0.P9_FH_ProcessHeader = jobHeader.PK;
			taskSh0.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			var taskReview = workItem.WorkflowItems.AddNew();
			taskReview.P9_Type = WorkItemProcessTask.AspectReviewTaskType;
			taskReview.P9_Description = $"Aspect Review - {aspectName} (with CB)";
			taskReview.P9_Sequence = 105;
			taskReview.P9_G4_RequiredCapability = capability.PK;
			taskReview.AppendTextToTaskNote(initialTaskNote);
			taskReview.P9_FH_ProcessHeader = jobHeader.PK;
			taskReview.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			var taskCh0 = workItem.WorkflowItems.AddNew();
			taskCh0.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			taskCh0.P9_Description = "check-in";
			taskCh0.P9_Sequence = 110;
			taskCh0.P9_FH_ProcessHeader = jobHeader.PK;
			taskCh0.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			taskCh0.EnsureRequiredAspectReviewExists(capability.G4_Code, aspectName, additionalTaskNote);

			AssertEquals("Should not have added a task", 3, workItem.WorkflowItems.Count);
		}

		public void TestEnsureRequiredAspectReviewExists_MatchingReviewItemFound_UpdatesTaskNoteInReviewItem()
		{
			var aspectName = Guid.NewGuid().ToString("N").Substring(0, 16);
			var initialTaskNote = Guid.NewGuid().ToString();
			var additionalTaskNote = Guid.NewGuid().ToString();
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.Capabilities.Add(capability);

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var jobHeader = ProcessJobHeader.GetForParent(workItem, Factory, addDefaultProcessHeaderIfNone: true);
			var taskSh0 = workItem.WorkflowItems.AddNew();
			taskSh0.P9_Type = WorkItemProcessTask.ShelfsetTestTask;
			taskSh0.P9_Description = "shelf test";
			taskSh0.P9_Sequence = 100;
			taskSh0.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			taskSh0.P9_FH_ProcessHeader = jobHeader.PK;
			taskSh0.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			var taskReview = workItem.WorkflowItems.AddNew();
			taskReview.P9_Type = WorkItemProcessTask.AspectReviewTaskType;
			taskReview.P9_Description = $"Aspect Review - {aspectName} (with CB)";
			taskReview.P9_Sequence = 105;
			taskReview.P9_G4_RequiredCapability = capability.PK;
			taskReview.AppendTextToTaskNote(initialTaskNote);
			taskReview.P9_FH_ProcessHeader = jobHeader.PK;
			taskReview.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			var taskCh0 = workItem.WorkflowItems.AddNew();
			taskCh0.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			taskCh0.P9_Description = "check-in";
			taskCh0.P9_Sequence = 110;
			taskCh0.P9_FH_ProcessHeader = jobHeader.PK;
			taskCh0.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			Factory.Save();

			taskCh0.EnsureRequiredAspectReviewExists(capability.G4_Code, aspectName, additionalTaskNote);

			AssertContains("Review task notes should have initial content", initialTaskNote, taskReview.P9_NotesAsString);
			AssertContains("Review task notes should be updated", additionalTaskNote, taskReview.P9_NotesAsString);
			Assert("SH0 task should not change", !taskSh0.HasChanges);
			Assert("CH0 task should not change", !taskCh0.HasChanges);
			AssertEquals("No additional tasks", 3, workItem.WorkflowItems.Count);
		}

		public void TestEnsureRequiredAspectReviewExists_NoReviewItemMatchingCapability_CreatesNewReviewItemAssignedToAspectCapability()
		{
			var aspectName = Guid.NewGuid().ToString("N").Substring(0, 16);
			var initialTaskNote = Guid.NewGuid().ToString();
			var additionalTaskNote = Guid.NewGuid().ToString();
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var jobHeader = ProcessJobHeader.GetForParent(workItem, Factory, addDefaultProcessHeaderIfNone: true);
			var taskSh0 = workItem.WorkflowItems.AddNew();
			taskSh0.P9_Type = WorkItemProcessTask.ShelfsetTestTask;
			taskSh0.P9_Description = "shelf test";
			taskSh0.P9_Sequence = 100;
			taskSh0.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			taskSh0.P9_FH_ProcessHeader = jobHeader.PK;
			taskSh0.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			var taskReview = workItem.WorkflowItems.AddNew();
			taskReview.P9_Type = WorkItemProcessTask.AspectReviewTaskType;
			taskReview.P9_Description = $"Aspect Review - {aspectName} (with CB)";
			taskReview.P9_Sequence = 105;
			taskReview.AppendTextToTaskNote(initialTaskNote);
			taskReview.P9_FH_ProcessHeader = jobHeader.PK;
			taskReview.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			var taskCh0 = workItem.WorkflowItems.AddNew();
			taskCh0.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			taskCh0.P9_Description = "check-in";
			taskCh0.P9_Sequence = 110;
			taskCh0.P9_FH_ProcessHeader = jobHeader.PK;
			taskCh0.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			Factory.Save();

			taskCh0.EnsureRequiredAspectReviewExists(capability.G4_Code, aspectName, additionalTaskNote);

			Assert("SH0 task should not change", !taskSh0.HasChanges);
			Assert("CBS task should not change", !taskReview.HasChanges);
			Assert("CH0 task should not change", !taskCh0.HasChanges);
			AssertEquals("A new task should be created", 4, workItem.WorkflowItems.Count);

			var newTask = workItem.WorkflowItems.Where(t => t.P9_Type == "CBS" && t.PK != taskReview.PK).FirstOrDefault();
			AssertContains("New review should have expected notes", additionalTaskNote, newTask.P9_NotesAsString);
			AssertEquals("New review should be assigned to capability", capability, newTask.RequiredCapability);
		}

		public void TestEnsureRequiredAspectReviewExists_NoReviewItems_CreatesNewReviewItemAssignedToAspectCapability()
		{
			var aspectName = Guid.NewGuid().ToString("N").Substring(0, 16);
			var additionalTaskNote = Guid.NewGuid().ToString();
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var jobHeader = ProcessJobHeader.GetForParent(workItem, Factory, addDefaultProcessHeaderIfNone: true);
			var taskSh0 = workItem.WorkflowItems.AddNew();
			taskSh0.P9_Type = WorkItemProcessTask.ShelfsetTestTask;
			taskSh0.P9_Description = "shelf test";
			taskSh0.P9_Sequence = 100;
			taskSh0.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			taskSh0.P9_FH_ProcessHeader = jobHeader.PK;
			taskSh0.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			var taskCh0 = workItem.WorkflowItems.AddNew();
			taskCh0.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			taskCh0.P9_Description = "check-in";
			taskCh0.P9_Sequence = 110;
			taskCh0.P9_FH_ProcessHeader = jobHeader.PK;
			taskCh0.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			Factory.Save();

			taskCh0.EnsureRequiredAspectReviewExists(capability.G4_Code, aspectName, additionalTaskNote);

			Assert("SH0 task should not change", !taskSh0.HasChanges);
			Assert("CH0 task should not change", !taskCh0.HasChanges);
			AssertEquals("A new task should be created", 3, workItem.WorkflowItems.Count);

			var newTask = workItem.WorkflowItems.Where(t => t.P9_Type == "CBS").FirstOrDefault();
			AssertContains("New review should have expected notes", additionalTaskNote, newTask.P9_NotesAsString);
			AssertEquals("New review should be assigned to capability", capability, newTask.RequiredCapability);
		}

		public void TestEnsureRequiredAspectReviewExists_MultipleCallsShouldNotDuplicateTaskNotes()
		{
			var aspectName = Guid.NewGuid().ToString("N").Substring(0, 16);
			var additionalTaskNote = Guid.NewGuid().ToString();
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var jobHeader = ProcessJobHeader.GetForParent(workItem, Factory, addDefaultProcessHeaderIfNone: true);

			var taskCh0 = workItem.WorkflowItems.AddNew();
			taskCh0.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			taskCh0.P9_Description = "check-in";
			taskCh0.P9_Sequence = 110;
			taskCh0.P9_FH_ProcessHeader = jobHeader.PK;
			taskCh0.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			Factory.Save();

			taskCh0.EnsureRequiredAspectReviewExists(capability.G4_Code, aspectName, additionalTaskNote);
			taskCh0.EnsureRequiredAspectReviewExists(capability.G4_Code, aspectName, additionalTaskNote);
			taskCh0.EnsureRequiredAspectReviewExists(capability.G4_Code, aspectName, additionalTaskNote);

			Assert("CH0 task should not change", !taskCh0.HasChanges);
			AssertEquals("A new task should be created", 2, workItem.WorkflowItems.Count);

			var newTask = workItem.WorkflowItems.Where(t => t.P9_Type == "CBS").FirstOrDefault();
			AssertContains("New review should have expected notes", additionalTaskNote, newTask.P9_NotesAsString);
			AssertEquals("New review should be assigned to capability", capability, newTask.RequiredCapability);
			AssertEquals("Only one instance of added task notes should exist", 1, Regex.Matches(newTask.P9_NotesAsString, additionalTaskNote).Count);
		}

		public void TestEnsureRequiredAspectReviewExists_MatchingReviewCancelled_CreatesNewReviewItemAssignedToAspectCapability()
		{
			var aspectName = Guid.NewGuid().ToString("N").Substring(0, 16);
			var initialTaskNote = Guid.NewGuid().ToString();
			var additionalTaskNote = Guid.NewGuid().ToString();
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.Capabilities.Add(capability);

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var jobHeader = ProcessJobHeader.GetForParent(workItem, Factory, addDefaultProcessHeaderIfNone: true);
			var taskSh0 = workItem.WorkflowItems.AddNew();
			taskSh0.P9_Type = WorkItemProcessTask.ShelfsetTestTask;
			taskSh0.P9_Description = "shelf test";
			taskSh0.P9_Sequence = 100;
			taskSh0.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			taskSh0.P9_FH_ProcessHeader = jobHeader.PK;
			taskSh0.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			var taskReview = workItem.WorkflowItems.AddNew();
			taskReview.P9_Type = WorkItemProcessTask.AspectReviewTaskType;
			taskReview.P9_Description = $"Aspect Review - {aspectName} (with CB)";
			taskReview.P9_Sequence = 105;
			taskReview.P9_G4_RequiredCapability = capability.PK;
			taskReview.AppendTextToTaskNote(initialTaskNote);
			taskReview.P9_FH_ProcessHeader = jobHeader.PK;
			taskReview.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			var taskCh0 = workItem.WorkflowItems.AddNew();
			taskCh0.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			taskCh0.P9_Description = "check-in";
			taskCh0.P9_Sequence = 110;
			taskCh0.P9_FH_ProcessHeader = jobHeader.PK;
			taskCh0.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			Factory.Save();

			taskCh0.EnsureRequiredAspectReviewExists(capability.G4_Code, aspectName, additionalTaskNote);

			AssertContains("Old Review task notes should have initial content", initialTaskNote, taskReview.P9_NotesAsString);
			AssertNotContains("Old Review task notes should not be updated", additionalTaskNote, taskReview.P9_NotesAsString);
			Assert("SH0 task should not change", !taskSh0.HasChanges);
			Assert("CH0 task should not change", !taskCh0.HasChanges);

			var newTask = workItem.WorkflowItems.Where(t => t.P9_Type == "CBS" && t.PK != taskReview.PK).FirstOrDefault();
			AssertContains("New review should have expected notes", additionalTaskNote, newTask.P9_NotesAsString);
			AssertEquals("New review should be assigned to capability", capability, newTask.RequiredCapability);

			AssertEquals("No additional tasks", 4, workItem.WorkflowItems.Count);
		}

		public void TestEnsureRequiredAspectReviewExists_NoOtherTasks_CreatesNewReviewItemAssignedToAspectCapability()
		{
			var aspectName = Guid.NewGuid().ToString("N").Substring(0, 16);
			var initialTaskNote = Guid.NewGuid().ToString();
			var additionalTaskNote = Guid.NewGuid().ToString();
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.Capabilities.Add(capability);

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var jobHeader = ProcessJobHeader.GetForParent(workItem, Factory, addDefaultProcessHeaderIfNone: true);
			var taskCh0 = workItem.WorkflowItems.AddNew();
			taskCh0.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			taskCh0.P9_Description = "check-in";
			taskCh0.P9_Sequence = 110;
			taskCh0.P9_FH_ProcessHeader = jobHeader.PK;
			taskCh0.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			Factory.Save();

			taskCh0.EnsureRequiredAspectReviewExists(capability.G4_Code, aspectName, additionalTaskNote);

			Assert("CH0 task should not change", !taskCh0.HasChanges);
			var newTask = workItem.WorkflowItems.Where(t => t.P9_Type == "CBS").FirstOrDefault();
			AssertContains("New review should have expected notes", additionalTaskNote, newTask.P9_NotesAsString);
			AssertEquals("New review should be assigned to capability", capability, newTask.RequiredCapability);

			AssertEquals("No additional tasks", 2, workItem.WorkflowItems.Count);
		}

		public void TestEnsureRequiredAspectReviewExists_ShelfCapabilityNotFound_CreatesTasksToFixCapabilityMismatchButNoNewReview()
		{
			var datAdmins = Factory.NewWithValidTestData<GlbCapability>();
			datAdmins.G4_Code = "DAT";
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var capabilityCode = capability.G4_Code;
			capability.Delete();

			var aspect = new AspectReview(Guid.NewGuid(), capabilityCode, Guid.NewGuid().ToString("N").Substring(0, 16));
			var initialTaskNote = Guid.NewGuid().ToString();
			var aspectReference = Guid.NewGuid().ToString();
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var taskDescription = "Fix aspect capability assignment in DAT";
			var capabilityNote = $"The aspect capability '{capabilityCode}' could not be found.";
			var investigateNote = "Please determine the correct capability for this aspect review," +
									" and communicate with DAT admins to get it fixed in DAT.";
			var assistNote = $"Please assist {staff.GS_Code} to fix DAT's aspect review capability assignment.";

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var taskSh0 = workItem.WorkflowItems.AddNew();
			taskSh0.P9_Type = WorkItemProcessTask.ShelfsetTestTask;
			taskSh0.P9_Description = "shelf test";
			taskSh0.P9_Sequence = 100;
			taskSh0.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			taskSh0.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			var taskCh0 = workItem.WorkflowItems.AddNew();
			taskCh0.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			taskCh0.P9_Description = "check-in";
			taskCh0.P9_Sequence = 110;
			taskCh0.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			Factory.Save();

			taskCh0.EnsureRequiredAspectReviewExists(capabilityCode, aspect.AspectName, aspectReference);

			Assert("SH0 task should not change", !taskSh0.HasChanges);
			Assert("CH0 task should not change", !taskCh0.HasChanges);
			AssertEquals("New tasks should be created", 4, workItem.WorkflowItems.Count);

			var taskInv = AssertNewTaskCreated("INV");
			AssertEquals("INV task should be assigned to shelf staff", staff, taskInv.AssignedStaffMember);
			AssertContains($"INV task should contain investigation note", investigateNote, taskInv.P9_NotesAsString);

			var taskAst = AssertNewTaskCreated("AST");
			AssertNull("AST task should not have staff assigned", taskAst.AssignedStaffMember);
			AssertEquals("AST task should be assigned to DAT admins", datAdmins.PK, taskAst.P9_G4_RequiredCapability);
			AssertContains($"AST task should contain assist note", assistNote, taskAst.P9_NotesAsString);

			ProcessTask AssertNewTaskCreated(string taskType)
			{
				var task = workItem.WorkflowItems.Where(t => t.P9_Type == taskType).FirstOrDefault();

				AssertNotNull($"{taskType} task should be created", task);
				AssertEquals($"{taskType} task should have expected description", taskDescription, task.P9_Description);
				AssertEquals($"{taskType} task should have same sequence as checkin task", taskCh0.P9_Sequence, task.P9_Sequence);
				AssertEquals($"{taskType} task should have same prcess header as checkin task", taskCh0.P9_FH_ProcessHeader, task.P9_FH_ProcessHeader);
				AssertContains($"{taskType} task should contain capability missing note", capabilityNote, task.P9_NotesAsString);
				AssertContains($"{taskType} task should contain aspect review reference", aspectReference, task.P9_NotesAsString);
				return task;
			}
		}

		public void TestGetTopWorkflow_ShouldIgnoreJobHeaders()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			var jobHeader = helper.CreateJobHeader<NewWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var parentWorkflow = helper.CreateWorkflow(jobHeader, "Parent");
			var childWorkflow = helper.CreateWorkflow(jobHeader, "Child");

			childWorkflow.GetOrCreateLinkToParent(parentWorkflow);
			childWorkflow.GetOrCreateLinkToParent(jobHeader);

			var task = (WorkItemProcessTask)helper.CreateTask(childWorkflow);
			var topWorkflow = task.GetTopWorkflow();
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			AssertEquals(topWorkflow, parentWorkflow);
		}

		public void TestGetAllTasksInWorkflowInOrder_ShouldIgnoreDuplicateLinks()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("QCB", "WKI");

			var jobHeader = helper.CreateJobHeader<NewWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var iterationWorkflow = helper.CreateWorkflow(jobHeader, "Iteration");
			var iterationTask = (WorkItemProcessTask)helper.CreateTask(iterationWorkflow);
			var parentWorkflow = helper.CreateWorkflow(jobHeader, "Parent");
			var parentTask = (WorkItemProcessTask)helper.CreateTask(parentWorkflow, taskType: "QCB");

			var pivot1 = (IProcessTaskIterationLink)parentTask.IterationLinks.AddNew();
			pivot1.P9I_P9_IterationTask = iterationTask.PK;
			pivot1.P9I_FH_IterationWorkflow = iterationWorkflow.PK;
			pivot1.P9I_IterationReason = "RS1";
			var pivot2 = (IProcessTaskIterationLink)parentTask.IterationLinks.AddNew();
			pivot2.P9I_P9_IterationTask = iterationTask.PK;
			pivot2.P9I_FH_IterationWorkflow = iterationWorkflow.PK;
			pivot2.P9I_IterationReason = "RS2";
			Factory.Save();

			var tasks = parentTask.GetAllTasksInWorkflowInOrder();
			AssertEquals("There should be only 2 tasks returned", 2, tasks.Count());
		}

		public void TestSH0SubmitsPullRequest()
		{
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var task = workItem.WorkflowItems.AddNew();
			task.P9_Type = "SH0";
			task.P9_Status = "ASN";
			task.P9_Description = "Shelf Test";
			task.P9_NotesAsString = "http://tfs.wtg.zone:8080/tfs/cargowise/_git/borderwise/pullrequest/1278?_a=overview";
			task.P9_GS_NKAssignedStaffMember = "ABC";
			Factory.Save();

			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				var crikeyDataAccess = CrikeyDataAccessFactory.GetInstance(connection);
				var shelfInfo = ShelvesetTools.LoadShelfByProcessTask(task);

				AssertNotNull(shelfInfo);
				AssertEquals("SHV", shelfInfo.ActionType);
				AssertNullOrEmpty(shelfInfo.Name);
				AssertShelfComments(shelfInfo, "http://tfs.wtg.zone:8080/tfs/cargowise/_git/borderwise/pullrequest/1278?_a=overview");
				AssertEquals(true, crikeyDataAccess.IsShelfScheduled(shelfInfo));
			}
		}

		public void TestCH0SubmitsPullRequest()
		{
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var task = workItem.WorkflowItems.AddNew();
			task.P9_Type = "CH0";
			task.P9_Status = "ASN";
			task.P9_Description = "Check-In";
			task.P9_NotesAsString = "http://tfs.wtg.zone:8080/tfs/cargowise/_git/borderwise/pullrequest/1278?_a=overview";
			task.P9_GS_NKAssignedStaffMember = "ABC";
			Factory.Save();

			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				var crikeyDataAccess = CrikeyDataAccessFactory.GetInstance(connection);
				var shelfInfo = ShelvesetTools.LoadShelfByProcessTask(task);

				AssertNotNull(shelfInfo);
				AssertEquals("SCH", shelfInfo.ActionType);
				AssertNullOrEmpty(shelfInfo.Name);
				AssertShelfComments(shelfInfo, "http://tfs.wtg.zone:8080/tfs/cargowise/_git/borderwise/pullrequest/1278?_a=overview");
				AssertEquals(true, crikeyDataAccess.IsShelfScheduled(shelfInfo));
			}
		}

		public void TestSubmitTwoPullRequests()
		{
			var workItem1 = Factory.NewWithValidTestData<NewWorkItem>();
			var task1 = workItem1.WorkflowItems.AddNew();
			task1.P9_Type = "CH0";
			task1.P9_Status = "ASN";
			task1.P9_Description = "Check-In";
			task1.P9_NotesAsString = "http://tfs.wtg.zone:8080/tfs/cargowise/_git/borderwise/pullrequest/1278?_a=overview";
			task1.P9_GS_NKAssignedStaffMember = "ABC";
			Factory.Save();

			var workItem2 = Factory.NewWithValidTestData<NewWorkItem>();
			var task2 = workItem2.WorkflowItems.AddNew();
			task2.P9_Type = "CH0";
			task2.P9_Status = "ASN";
			task2.P9_Description = "Check-In";
			task2.P9_NotesAsString = "http://tfs.wtg.zone:8080/tfs/cargowise/_git/borderwise/pullrequest/1279?_a=overview";
			task2.P9_GS_NKAssignedStaffMember = "ABC";
			Factory.Save();

			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				var crikeyDataAccess = CrikeyDataAccessFactory.GetInstance(connection);

				var shelfInfo1 = ShelvesetTools.LoadShelfByProcessTask(task1);
				AssertNotNull(shelfInfo1);
				AssertEquals("SCH", shelfInfo1.ActionType);
				AssertNullOrEmpty(shelfInfo1.Name);
				AssertShelfComments(shelfInfo1, "http://tfs.wtg.zone:8080/tfs/cargowise/_git/borderwise/pullrequest/1278?_a=overview");
				AssertEquals(true, crikeyDataAccess.IsShelfScheduled(shelfInfo1));

				var shelfInfo2 = ShelvesetTools.LoadShelfByProcessTask(task2);
				AssertNotNull(shelfInfo2);

				AssertEquals("SCH", shelfInfo2.ActionType);
				AssertNullOrEmpty(shelfInfo2.Name);
				AssertShelfComments(shelfInfo2, "http://tfs.wtg.zone:8080/tfs/cargowise/_git/borderwise/pullrequest/1279?_a=overview");
				AssertEquals(true, crikeyDataAccess.IsShelfScheduled(shelfInfo2));
			}
		}

		public void TestSH0SubmitsShelfAndPullRequest()
		{
			TestSubmitsShelfAndPullRequest("SH0", "SHV");
		}

		public void TestCH0SubmitsShelfAndPullRequest()
		{
			TestSubmitsShelfAndPullRequest("CH0", "SCH");
		}

		public void TestUA0SubmitsShelfAndPullRequest()
		{
			TestSubmitsShelfAndPullRequest("UA0", "UAB");
		}

		public void TestAS0SubmitsShelfAndPullRequest()
		{
			TestSubmitsShelfAndPullRequest("AS0", "ASB");
		}

		public void TestJP0SubmitsShelfAndPullRequest()
		{
			TestSubmitsShelfAndPullRequest("JP0", "SHV");
		}

		void TestSubmitsShelfAndPullRequest(string p9type, string expectedActionType)
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "FOO";
			staff.GS_LoginName = "foo.bar";
			staff.GS_FullName = "Foo Bar";

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var task = workItem.WorkflowItems.AddNew();
			task.P9_Type = p9type;
			task.P9_Status = "ASN";
			task.P9_GS_NKAssignedStaffMember = "FOO";
			task.P9_Description = "My Shelf";
			task.P9_NotesAsString = "http://tfs.wtg.zone:8080/tfs/cargowise/_git/borderwise/pullrequest/1278?_a=overview";
			Factory.Save();

			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				var crikeyDataAccess = CrikeyDataAccessFactory.GetInstance(connection);
				var shelfInfo = ShelvesetTools.LoadShelfByProcessTask(task);

				AssertNotNull(shelfInfo);
				AssertEquals(expectedActionType, shelfInfo.ActionType);
				AssertEquals("My Shelf", string.Empty, shelfInfo.Name);
				AssertShelfComments(shelfInfo, "http://tfs.wtg.zone:8080/tfs/cargowise/_git/borderwise/pullrequest/1278?_a=overview");
				AssertEquals(true, crikeyDataAccess.IsShelfScheduled(shelfInfo));
			}
		}

		public void TestWorkItemNumberPropagatedToUserTestHeader()
		{
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var task = workItem.WorkflowItems.AddNew();
			task.P9_Type = "CH0";
			task.P9_Status = "ASN";
			task.P9_Description = "Check-In";
			task.P9_NotesAsString = "http://tfs.wtg.zone:8080/tfs/cargowise/_git/borderwise/pullrequest/1278?_a=overview";
			task.P9_GS_NKAssignedStaffMember = "ABC";
			Factory.Save();

			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			using (var cmd = connection.Command("select top 1 UH_WorkItemNumber from UserTestHeader"))
			{
				AssertEquals(workItem.WKI_WorkItemNumber, cmd.ExecuteScalar());
			}
		}

		#region TestConvertRtfNotesPullRequestUrl

		public void TestConvertRtfNotesPullRequestUrl_SinglePRInEdgeFormat()
		{
			AssertEDIShelvesetInfoComments(@"{\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang3081{\fonttbl{\f0\fnil\fcharset0 Microsoft Sans Serif;}}
{\colortbl ;\red0\green0\blue255;}
{\*\generator Riched20 10.0.19041}\viewkind4\uc1
\pard {\f0\fs20{\field{\*\fldinst{HYPERLINK ""https://devops.wisetechglobal.com/wtg/CargoWise/_git/Blazor.Client/pullrequest/123456""}}{\fldrslt{\ul\cf1 Pull request 123456: WI00000001 - Test Pull Reqeust - Repos (wisetechglobal.com)}}}}\f0\fs20\lang3081\par
}

", @"https://devops.wisetechglobal.com/wtg/CargoWise/_git/Blazor.Client/pullrequest/123456");
		}

		public void TestConvertRtfNotesPullRequestUrl_SinglePRInNormalFormat()
		{
			AssertEDIShelvesetInfoComments(@"{\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang3081{\fonttbl{\f0\fnil\fcharset0 Microsoft Sans Serif;}}
{\colortbl ;\red0\green0\blue255;}
{\*\generator Riched20 10.0.19041}\viewkind4\uc1
\pard {\f0\fs20{\field{\*\fldinst{HYPERLINK https://devops.wisetechglobal.com/wtg/CargoWise/_git/Blazor.Client/pullrequest/123456 }}{\fldrslt{https://devops.wisetechglobal.com/wtg/CargoWise/_git/Blazor.Client/pullrequest/123456\ul0\cf0}}}}\f0\fs20\lang3081\par
\par
}

", @"https://devops.wisetechglobal.com/wtg/CargoWise/_git/Blazor.Client/pullrequest/123456
"
			);
		}

		public void TestConvertRtfNotesPullRequestUrl_2PRsInEdgeFormat()
		{
			AssertEDIShelvesetInfoComments(@"{\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang3081{\fonttbl{\f0\fnil\fcharset0 Microsoft Sans Serif;}}
{\*\generator Riched20 10.0.19041}\viewkind4\uc1 \pard\f0{\fs20{\field{\*\fldinst{HYPERLINK ""https://github.com/WiseTechGlobal/DevTools/pull/123/files"" \\t ""_blank"" }}{\fldrslt{WI00000001 - Test Pull Reqeust \u183? Pull Request #123 \u183? WiseTechGlobal/DevTools (github.com)\ulnone\cf0}}}}\par{\fs20{\field{\*\fldinst{HYPERLINK ""https://devops.wisetechglobal.com/wtg/CargoWise/_git/Blazor.Client/pullrequest/456?_a=files"" \\t ""_blank"" }}{\fldrslt{Pull request 198086: WI00687361 - Security - Authenticode sign all binaries in client app - Repos (wisetechglobal.com)\ulnone\cf0}}}}\fs20\par
}

", @"https://github.com/WiseTechGlobal/DevTools/pull/123/files
https://devops.wisetechglobal.com/wtg/CargoWise/_git/Blazor.Client/pullrequest/456?_a=files");
		}

		public void TestConvertRtfNotesPullRequestUrl_SinglePRInNormalFormatAndTestRigs()
		{
			AssertEDIShelvesetInfoComments(@"{\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang3081{\fonttbl{\f0\fnil Microsoft Sans Serif;}{\f1\fnil\fcharset0 Microsoft Sans Serif;}}
{\colortbl ;\red0\green0\blue255;}
{\*\generator Riched20 10.0.19041}\viewkind4\uc1
\pard {\f0\fs20\lang1033{\field{\*\fldinst{HYPERLINK https://devops.wisetechglobal.com/wtg/CargoWise/_git/Blazor.Client/pullrequest/123456?_a=files }}{\fldrslt{https://devops.wisetechglobal.com/wtg/CargoWise/_git/Blazor.Client/pullrequest/123456?_a=files\ul0\cf0}}}}\f0\fs20\par
\par
\f1\fs20\lang3081 TestRigRestoreFromBackup: {{\field{\*\fldinst{HYPERLINK ""\\\\\\\\uat-backups.wtg.zone\\\\SQL_Backup\\\\Winzor\\\\HybridMode_latest.bak""}}{\fldrslt{\\\\uat-backups.wtg.zone\\SQL_Backup\\Winzor\\HybridMode_latest.bak\ul0\cf0}}}}\f1\fs20\par
TestRigDeployWinzor: true\par
\f0\fs20\lang1033\par
}

", @"https://devops.wisetechglobal.com/wtg/CargoWise/_git/Blazor.Client/pullrequest/123456?_a=files

TestRigRestoreFromBackup: \\uat-backups.wtg.zone\SQL_Backup\Winzor\HybridMode_latest.bak
TestRigDeployWinzor: true
");
		}

		public void TestConvertRtfNotesPullRequestUrl_SinglePRInNormalFormatAndTestRigs_DifferentRTFContent()
		{
			AssertEDIShelvesetInfoComments(@"{\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang3081{\fonttbl{\f0\fnil Microsoft Sans Serif;}{\f1\fnil\fcharset0 Microsoft Sans Serif;}}
{\colortbl ;\red0\green0\blue255;}
{\*\generator Riched20 10.0.19041}\viewkind4\uc1
\pard {\f0\fs20\lang1033{\field{\*\fldinst{HYPERLINK https://devops.wisetechglobal.com/wtg/CargoWise/_git/Blazor.Client/pullrequest/123456?_a=files }}{\fldrslt{https://devops.wisetechglobal.com/wtg/CargoWise/_git/Blazor.Client/pullrequest/123456?_a=files\ul0\cf0}}}}\f0\fs20\lang3081\par
\f1 TestRigRestoreFromBackup: {{\field{\*\fldinst{HYPERLINK ""\\\\\\\\uat-backups.wtg.zone\\\\SQL_Backup\\\\Winzor\\\\HybridMode_latest.bak""}}{\fldrslt{\\\\uat-backups.wtg.zone\\SQL_Backup\\Winzor\\HybridMode_latest.bak\ul0\cf0}}}}\f1\fs20\par
TestRigDeployWinzor: true\par
\f0\par
}

", @"https://devops.wisetechglobal.com/wtg/CargoWise/_git/Blazor.Client/pullrequest/123456?_a=files
TestRigRestoreFromBackup: \\uat-backups.wtg.zone\SQL_Backup\Winzor\HybridMode_latest.bak
TestRigDeployWinzor: true
");
		}

		public void TestConvertRtfNotesPullRequestUrl_2PRsInEdgeFormatAndTestRigs()
		{
			AssertEDIShelvesetInfoComments(@"{\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang3081{\fonttbl{\f0\fnil\fcharset0 Microsoft Sans Serif;}}
{\colortbl ;\red0\green0\blue255;}
{\*\generator Riched20 10.0.19041}\viewkind4\uc1
\pard {\f0\fs20{\field{\*\fldinst{HYPERLINK ""https://github.com/WiseTechGlobal/DevTools/pull/123/files""}}{\fldrslt{\ul\cf1 WI00000001 - Test Pull Reqeust \u183? Pull Request #123 \'b7 WiseTechGlobal/DevTools (github.com)}}}}\f0\fs24\lang3081\par
{\fs20{\field{\*\fldinst{HYPERLINK ""https://devops.wisetechglobal.com/wtg/CargoWise/_git/Blazor.Client/pullrequest/123456?_a=files""}}{\fldrslt{\ul\cf1 Pull request 123456: WI00000001 - Test Pull Reqeust - Repos (wisetechglobal.com)}}}}\f0\fs20\par
TestRigRestoreFromBackup: {{\field{\*\fldinst{HYPERLINK ""\\\\\\\\uat-backups.wtg.zone\\\\SQL_Backup\\\\Winzor\\\\HybridMode_latest.bak""}}{\fldrslt{\\\\uat-backups.wtg.zone\\SQL_Backup\\Winzor\\HybridMode_latest.bak\ul0\cf0}}}}\f0\fs20\par
TestRigDeployWinzor: true\par
\par
}

", @"https://github.com/WiseTechGlobal/DevTools/pull/123/files
https://devops.wisetechglobal.com/wtg/CargoWise/_git/Blazor.Client/pullrequest/123456?_a=files
TestRigRestoreFromBackup: \\uat-backups.wtg.zone\SQL_Backup\Winzor\HybridMode_latest.bak
TestRigDeployWinzor: true
");
		}

		public void TestConvertRtfNotesPullRequestUrl_3PRsInEdgeAndNormalFormatWithDifferentStylesAndTestRigs()
		{
			AssertEDIShelvesetInfoComments(@"{\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang3081{\fonttbl{\f0\fnil\fcharset0 Microsoft Sans Serif;}}
{\colortbl ;\red0\green0\blue255;}
{\*\generator Riched20 10.0.19041}\viewkind4\uc1
\pard {\f0\fs20{\field{\*\fldinst{HYPERLINK ""https://github.com/WiseTechGlobal/DevTools/pull/123/commits""}}{\fldrslt{\ul\cf1 WI00000001 - Test Pull Reqeust \u183? \'b7 Pull Request #123 \'b7 WiseTechGlobal/DevTools (github.com)}}}}\f0\fs24\lang3081\par
{\fs20{\field{\*\fldinst{HYPERLINK https://github.com/WiseTechGlobal/DevTools/pull/456/files }}{\fldrslt{https://github.com/WiseTechGlobal/DevTools/pull/456/files\ul0\cf0}}}}\f0\fs24\par
{{\field{\*\fldinst{HYPERLINK https://github.com/WiseTechGlobal/DevTools/pull/789 }}{\fldrslt{https://github.com/WiseTechGlobal/DevTools/pull/789\ul0\cf0}}}}\f0\fs24\par
\par
\fs20 TestRigRestoreFromBackup: {{\field{\*\fldinst{HYPERLINK ""\\\\\\\\uat-backups.wtg.zone\\\\SQL_Backup\\\\Winzor\\\\HybridMode_latest.bak""}}{\fldrslt{\\\\uat-backups.wtg.zone\\SQL_Backup\\Winzor\\HybridMode_latest.bak\ul0\cf0}}}}\f0\fs20\line TestRigDeployWinzor: true\par
}

", @"https://github.com/WiseTechGlobal/DevTools/pull/123/commits
https://github.com/WiseTechGlobal/DevTools/pull/456/files
https://github.com/WiseTechGlobal/DevTools/pull/789

TestRigRestoreFromBackup: \\uat-backups.wtg.zone\SQL_Backup\Winzor\HybridMode_latest.bak" + "\nTestRigDeployWinzor: true");
		}

		void AssertEDIShelvesetInfoComments(string notes, string expectedComments)
		{
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var task = workItem.WorkflowItems.AddNew();
			task.P9_Type = "SH0";
			task.P9_Status = "ASN";
			task.P9_Description = "Shelf Test";
			task.P9_NotesAsString = notes;
			task.P9_GS_NKAssignedStaffMember = "ABC";
			Factory.Save();

			var shelfInfo = ShelvesetTools.LoadShelfByProcessTask(task);

			AssertShelfComments(shelfInfo, expectedComments);
		}

		#endregion

		void AssertShelfAndTaskStatusForTask(WorkItemProcessTask task, string shelfStatus, string taskStatus)
		{
			AssertShelfStatus(task, shelfStatus);
			AssertEquals(taskStatus, task.P9_Status);
		}

		void AssertShelfStatus(WorkItemProcessTask task, string shelfStatus)
		{
			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				var crikeyDataAccess = CrikeyDataAccessFactory.GetInstance(connection);
				var scheduledShelf = crikeyDataAccess.LoadShelfByProcessTask(task);

				AssertEquals(shelfStatus, scheduledShelf?.Status);
			}
		}

		void AssertShelfComments(EDIShelvesetInfo shelf, string expectedComments)
		{
			var assertMessage = "Shelf task notes did not match expected";
			if (shelf.RelatedProcessTask.Parent == null)
			{
				AssertEquals(assertMessage, expectedComments, shelf.Comments);
			}
			else
			{
				AssertEquals(assertMessage, @$"TestRigOrigin: {shelf.RelatedProcessTask.Parent.WKI_WorkItemNumber}
{expectedComments}", shelf.Comments);
			}
		}

		void DisableCrikeyScheduleTask()
		{
			RenameStoredProcedure("ScheduleTask", "DisabledScheduleTask");
		}

		void DisableCrikeyRemoveScheduleTask()
		{
			RenameStoredProcedure("XT_RemoveScheduledTask", "DisabledXT_RemoveScheduledTask");
		}

		void EnableCrikeyScheduleTask()
		{
			RenameStoredProcedure("DisabledScheduleTask", "ScheduleTask");
		}

		void EnableCrikeyRemoveScheduleTask()
		{
			RenameStoredProcedure("Disabledxt_RemoveScheduledTask", "XT_RemoveScheduledTask");
		}

		void RenameStoredProcedure(string oldName, string newName)
		{
			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				using (var command = connection.Command($@"EXEC sp_rename 'dbo.{oldName}', '{newName}'"))
				{
					command.ExecuteNonQuery();
				}
			}
		}
	}
}
