using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	public class RestrictionsTestingEnvironment : Assertion, IDisposable
	{
		public RestrictionsTestingEnvironment(IWorkflowCapabilityAssignerTestCase testCase, bool bothResourcesOwnCapability = true)
		{
			this.testCase = testCase;
			factory = testCase.Factory;
			helper = new WorkflowCapabilityAssignerTestHelper(factory);
			SetupRestrictionsTestingEnvironment(bothResourcesOwnCapability);
		}

		public void Dispose()
		{
			BMSTestHelper.ClearTaskAssignmentRestrictions();
		}

		readonly IWorkflowCapabilityAssignerTestCase testCase;
		readonly BusinessObjectFactory factory;
		readonly WorkflowCapabilityAssignerTestHelper helper;

		public BMComponent Buffer;
		public BMComponent Bucket;

		public OrgHeader Job;
		public ProcessJobHeader JobHeader;

		public ProcessHeader CurrentWorkflow;
		public ProcessHeader PreReqWorkflow;
		public ProcessHeader PostReqWorkflow;
		public ProcessHeader ChildWorkflow;

		public ProcessHeader WorkflowForDIFTask;

		public ProcessTask TaskCapability;  // current workflow, assigned to capability
		public ProcessTask TaskCapability_Reloaded;

		public ProcessTask TaskWFL1; // current workflow, not assigned to capabilities
		public ProcessTask TaskWFL1_Reloaded;
		public ProcessTask TaskWFL2; // current workflow, not assigned to capabilities
		public ProcessTask TaskWFL2_Reloaded;
		public ProcessTask TaskPRE;  // pre-requisite workflow, not assigned to capabilities
		public ProcessTask TaskPRE_Reloaded;
		public ProcessTask TaskPRE2;  // pre-requisite workflow, not assigned to capabilities
		public ProcessTask TaskPRE2_Reloaded;
		public ProcessTask TaskPST;  // post-requisite workflow, not assigned to capabilities
		public ProcessTask TaskPST_Reloaded;
		public ProcessTask TaskCLD;  // child workflow, not assigned to capabilities
		public ProcessTask TaskCLD_Reloaded;

		public ProcessTask TaskUndefinedType;  // current workflow but P9_Type is not part of registry restriction
		public ProcessTask TaskWithUndefinedType_Reloaded;

		public ProcessTask TaskCapability_DIF;  // different workflows (depending on the scopeForDIFTasks parameter), assigned to capability, DIF restriction with Notification Type = ERR or WRN (depending on the errNotification parameter)
		public ProcessTask TaskCapability_DIF_Reloaded;

		public GlbCapability Capability;
		public GlbStaff Resource1;
		public GlbStaff Resource2;

		public void ReloadTasksWithNewFactory()
		{
			var newFactory = new BusinessObjectFactory();
			TaskCapability_Reloaded = newFactory.Load<ProcessTask>(TaskCapability.PK);
			TaskWFL1_Reloaded = newFactory.Load<ProcessTask>(TaskWFL1.PK);
			TaskWFL2_Reloaded = newFactory.Load<ProcessTask>(TaskWFL2.PK);
			TaskPRE_Reloaded = newFactory.Load<ProcessTask>(TaskPRE.PK);
			TaskPRE2_Reloaded = newFactory.Load<ProcessTask>(TaskPRE2.PK);
			TaskPST_Reloaded = newFactory.Load<ProcessTask>(TaskPST.PK);
			TaskCLD_Reloaded = newFactory.Load<ProcessTask>(TaskCLD.PK);
			TaskWithUndefinedType_Reloaded = newFactory.Load<ProcessTask>(TaskUndefinedType.PK);

			TaskCapability_DIF_Reloaded = TaskCapability_DIF != null ? newFactory.Load<ProcessTask>(TaskCapability_DIF.PK) : null;
		}

		void SetupRegistry(string scopeForRestrictions)
		{
			var categorisedWorkflowTaskTypeCollection = new CategorisedWorkflowTaskTypesCollection();

			var categorisedWorkflowTaskType = categorisedWorkflowTaskTypeCollection.AddNew();
			categorisedWorkflowTaskType.Code = "ORG";

			var workflowTaskType1 = categorisedWorkflowTaskType.TaskTypes.AddNew();
			var workflowTaskType2 = categorisedWorkflowTaskType.TaskTypes.AddNew();
			var workflowTaskType3 = categorisedWorkflowTaskType.TaskTypes.AddNew();
			var workflowTaskType4 = categorisedWorkflowTaskType.TaskTypes.AddNew();
			var workflowTaskType5 = categorisedWorkflowTaskType.TaskTypes.AddNew();
			var workflowTaskType6 = categorisedWorkflowTaskType.TaskTypes.AddNew();

			workflowTaskType1.Code = "INV";
			workflowTaskType2.Code = "CDU";
			workflowTaskType3.Code = "CDF";
			workflowTaskType4.Code = "CBC";
			workflowTaskType5.Code = "CBF";
			workflowTaskType6.Code = "CNT";

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categorisedWorkflowTaskTypeCollection);

			var collection = new TaskTypeRestrictionsCollection();

			//SAME type restriction - INV, CDU, CDF
			var restriction = collection.AddNew();
			restriction.Active = true;
			restriction.WorkflowType = "ORG";
			restriction.TaskType = "INV";
			restriction.NotificationType = NotificationTypeList.Codes.Error;
			restriction.Scope = scopeForRestrictions;
			restriction.RestrictionType = RestrictionTypeList.Codes.SameResource;

			var samTaskType1 = restriction.TaskTypesCollection.AddNew();
			var samTaskType2 = restriction.TaskTypesCollection.AddNew();

			samTaskType1.Code = "CDU";
			samTaskType2.Code = "CDF";

			//DIF type restriction - CBC
			restriction = collection.AddNew();
			restriction.Active = true;
			restriction.WorkflowType = "ORG";
			restriction.TaskType = "CBC";
			restriction.NotificationType = NotificationTypeList.Codes.Error;
			restriction.Scope = scopeForRestrictions;
			restriction.RestrictionType = RestrictionTypeList.Codes.DifferentResource;

			var difTaskType1 = restriction.TaskTypesCollection.AddNew();
			var difTaskType2 = restriction.TaskTypesCollection.AddNew();
			var difTaskType3 = restriction.TaskTypesCollection.AddNew();

			difTaskType1.Code = "INV";
			difTaskType2.Code = "CDU";
			difTaskType3.Code = "CDF";

			//DIF type restriction - CBF
			restriction = collection.AddNew();
			restriction.Active = true;
			restriction.WorkflowType = "ORG";
			restriction.TaskType = "CBF";
			restriction.NotificationType = NotificationTypeList.Codes.Warning; //to test what happens when it's warning, not error
			restriction.Scope = scopeForRestrictions;
			restriction.RestrictionType = RestrictionTypeList.Codes.DifferentResource;

			difTaskType1 = restriction.TaskTypesCollection.AddNew();
			difTaskType2 = restriction.TaskTypesCollection.AddNew();
			difTaskType3 = restriction.TaskTypesCollection.AddNew();

			difTaskType1.Code = "INV";
			difTaskType2.Code = "CDU";
			difTaskType3.Code = "CDF";

			//DIF type restriction - CNT
			restriction = collection.AddNew();
			restriction.Active = true;
			restriction.WorkflowType = "ORG";
			restriction.TaskType = "CNT";
			restriction.NotificationType = NotificationTypeList.Codes.Error;
			restriction.Scope = scopeForRestrictions;
			restriction.RestrictionType = RestrictionTypeList.Codes.DifferentResource;

			difTaskType1 = restriction.TaskTypesCollection.AddNew();

			difTaskType1.Code = "CDF";

			WorkflowDataRegistry.Instance.TaskAssignmentRestrictions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
		}

		void SetupRestrictionsTestingEnvironment(bool bothResourcesOwnCapability = true)
		{
			Buffer = BMSTestHelper.CreateBuffer(testCase.System, "Buffer");
			Buffer.FC_AutoAssignTasksAge = new ZInt(60).GetDateTimeFromMinutes();
			Bucket = BMSTestHelper.CreateBucket(testCase.System, "Bucket");

			Capability = factory.NewWithValidTestData<GlbCapability>();
			Capability.G4_Code = "COD";
			Capability.G4_AllowTaskAutoAssignment = true;

			Resource1 = helper.CreateResourceWithHomeBranchDeptSet("Frodo", Capability);
			if (bothResourcesOwnCapability)
			{
				Resource2 = helper.CreateResourceWithHomeBranchDeptSet("Gandalf", Capability);
			}
			else
			{
				Resource2 = helper.CreateResourceWithHomeBranchDeptSet("Gandalf");
			}

			Job = factory.NewWithValidTestData<OrgHeader>();
			Job.OH_Code = "MYORG";
			JobHeader = ProcessJobHeader.GetForParent(Job, factory);

			CurrentWorkflow = JobHeader.ProcessHeaders.AddNew();
			CurrentWorkflow.Name = "WFL";
			PreReqWorkflow = JobHeader.ProcessHeaders.AddNew();
			PreReqWorkflow = JobHeader.ProcessHeaders.AddNew();
			PreReqWorkflow.Name = "PRE";
			PostReqWorkflow = JobHeader.ProcessHeaders.AddNew();
			PostReqWorkflow.Name = "PST";
			ChildWorkflow = JobHeader.ProcessHeaders.AddNew();
			ChildWorkflow.Name = "CLD";

			PreReqWorkflow.GetOrCreateDependencyLink(CurrentWorkflow);
			CurrentWorkflow.GetOrCreateDependencyLink(PostReqWorkflow);
			ChildWorkflow.GetOrCreateLinkToParent(CurrentWorkflow);

			CurrentWorkflow.FH_AllowTaskAutoAssignment = true;
			PreReqWorkflow.FH_AllowTaskAutoAssignment = true;
			PostReqWorkflow.FH_AllowTaskAutoAssignment = true;
			ChildWorkflow.FH_AllowTaskAutoAssignment = true;

			CurrentWorkflow.FH_ReleaseDateTime = ZDateTime.Today.AddDays(-1);
			PreReqWorkflow.FH_ReleaseDateTime = ZDateTime.Today.AddDays(-1);
			PostReqWorkflow.FH_ReleaseDateTime = ZDateTime.Today.AddDays(-1);
			ChildWorkflow.FH_ReleaseDateTime = ZDateTime.Today.AddDays(-1);
		}

		public void SetupRegistryAndTasksForRestrictionsTesting(ZString scopeForRestrictions, string scopeForDIFTask = null, string notificationType = NotificationTypeList.Codes.Error, bool difInvolvedThroughSam = false)
		{
			SetupRegistry(scopeForRestrictions);

			TestCaseHelper.ClearTable(ProcessTasksSchema.Constants.TableName);
			TaskCapability = helper.CreateTask(CurrentWorkflow, Capability, null, 60);
			TaskCapability.P9_Type = "INV";

			TaskWFL1 = helper.CreateTask(CurrentWorkflow, null, null, 60);
			TaskWFL1.P9_Type = "CDU";

			TaskWFL2 = helper.CreateTask(CurrentWorkflow, null, null, 60);
			TaskWFL2.P9_Type = "CDF";

			TaskPRE = helper.CreateTask(PreReqWorkflow, null, null, 60);
			TaskPRE.P9_Type = "CDU";

			TaskPRE2 = helper.CreateTask(PreReqWorkflow, null, null, 60);
			TaskPRE2.P9_Type = "CDF";

			TaskPST = helper.CreateTask(PostReqWorkflow, null, null, 60);
			TaskPST.P9_Type = "CDU";

			TaskCLD = helper.CreateTask(ChildWorkflow, null, null, 60);
			TaskCLD.P9_Type = "CDF";

			TaskUndefinedType = helper.CreateTask(CurrentWorkflow, null, null, 60);
			TaskUndefinedType.P9_Type = "UDF";

			WorkflowForDIFTask = GetWorkflowForScope(scopeForDIFTask);
			TaskCapability_DIF = null;
			if (WorkflowForDIFTask != null)
			{
				TaskCapability_DIF = helper.CreateTask(WorkflowForDIFTask, Capability, null, 15);
				TaskCapability_DIF.P9_Type = difInvolvedThroughSam ? "CNT" : notificationType == NotificationTypeList.Codes.Error ? "CBC" : "CBF";
				TaskCapability_DIF.P9_Description = "DIF";
			}
		}

		ProcessHeader GetWorkflowForScope(ZString scope)
		{
			switch (scope)
			{
				case "WFL":
					return CurrentWorkflow;
				case "PRE":
					return PreReqWorkflow;
				case "PST":
					return PostReqWorkflow;
				case "CLD":
					return ChildWorkflow;
				default:
					return null;
			}
		}

		public void CheckDIFRestrictionsRespected(ZString scope, string notificationType)
		{
			CombineAssertions("DIF task is in the main workflow", () =>
			{
				CheckResourcesNotAutoAssigned_WhenOpenAssignedRestrictingTaskExists(scopeForRestrictions: scope, scopeForDIFTask: ScopeList.Codes.Workflow, notificationType: notificationType);
				CheckResourcesNotAutoAssigned_WhenClosedRestrictingTaskExists(scopeForRestrictions: scope, scopeForDIFTask: ScopeList.Codes.Workflow, notificationType: notificationType);
				CheckResourcesAutoAssignedToOneTaskOnly_WhenTwoOpenUnassignedTasksUnderRestrictionsAreInTheSameWorkflow(scopeForRestrictions: scope, notificationType: notificationType);
			});

			if (scope != ScopeList.Codes.Workflow)
			{
				CombineAssertions("DIF task is in the scope of the task with the same capability", () =>
				{
					CheckResourcesNotAutoAssigned_WhenOpenAssignedRestrictingTaskExists(scopeForRestrictions: scope, scopeForDIFTask: scope, notificationType: notificationType);
					CheckResourceAutoAssignedToOneTaskOnly_WhenTwoOpenUnassignedTasksUnderRestrictionsAreInDifferentWorkflows(scopeForRestrictions: scope, scopeForDIFTask: scope, notificationType: notificationType);
				});
			}

			CombineAssertions("DIF task is outside the scope of the task with the same capability", () =>
			{
				if (scope != ScopeList.Codes.Prerequisites)
				{
					CheckResourcesAutoAssigned_WhenOpenTasksAreInDifferentScopes(scopeForRestrictions: scope, scopeForDIFTask: ScopeList.Codes.Prerequisites, notificationType: notificationType);
				}
				if (scope != ScopeList.Codes.Postrequisites)
				{
					CheckResourcesAutoAssigned_WhenOpenTasksAreInDifferentScopes(scopeForRestrictions: scope, scopeForDIFTask: ScopeList.Codes.Postrequisites, notificationType: notificationType);
				}
				if (scope != ScopeList.Codes.Child)
				{
					CheckResourcesAutoAssigned_WhenOpenTasksAreInDifferentScopes(scopeForRestrictions: scope, scopeForDIFTask: ScopeList.Codes.Child, notificationType: notificationType);
				}
			});

			CombineAssertions("No tasks under DIF restrictions", () =>
			{
				CheckResourcesAutoAssigned_WhenThereIsNoOpenRestrictingTask(scope, notificationType);
			});
		}

		public void CheckDIFRestrictionsRespected_ScopeJob(string notificationType)
		{
			CheckResourcesAutoAssigned_WhenThereIsNoOpenRestrictingTask(ScopeList.Codes.Job, notificationType);
			CheckResourcesNotAutoAssigned_WhenOpenAssignedRestrictingTaskExists(scopeForRestrictions: ScopeList.Codes.Job, scopeForDIFTask: ScopeList.Codes.Workflow, notificationType: notificationType);
			CheckResourcesNotAutoAssigned_WhenOpenAssignedRestrictingTaskExists(scopeForRestrictions: ScopeList.Codes.Job, scopeForDIFTask: ScopeList.Codes.Prerequisites, notificationType: notificationType);
			CheckResourcesNotAutoAssigned_WhenOpenAssignedRestrictingTaskExists(scopeForRestrictions: ScopeList.Codes.Job, scopeForDIFTask: ScopeList.Codes.Postrequisites, notificationType: notificationType);
			CheckResourcesNotAutoAssigned_WhenOpenAssignedRestrictingTaskExists(scopeForRestrictions: ScopeList.Codes.Job, scopeForDIFTask: ScopeList.Codes.Child, notificationType: notificationType);
		}

		public void CheckResourcesAutoAssigned_WhenThereIsNoOpenRestrictingTask(ZString scope, string notificationType)
		{
			SetupRegistryAndTasksForRestrictionsTesting(scopeForRestrictions: scope);
			factory.Save();

			RunAssignment();

			ReloadTasksWithNewFactory();
			var errorMessage = $@"Should have auto-assigned to a resource in capability when there are no tasks under DIF restrictions (scope for restrictions = {scope}, notification type = {notificationType})";
			Assert(errorMessage, !TaskCapability_Reloaded.P9_GS_NKAssignedStaffMember.IsEmpty);

			CheckResourcesAutoAssigned_WhenThereIsCancelledRestrictingTask(scope, ScopeList.Codes.Workflow, notificationType);
			CheckResourcesAutoAssigned_WhenThereIsCancelledRestrictingTask(scope, ScopeList.Codes.Prerequisites, notificationType);
			CheckResourcesAutoAssigned_WhenThereIsCancelledRestrictingTask(scope, ScopeList.Codes.Postrequisites, notificationType);
			CheckResourcesAutoAssigned_WhenThereIsCancelledRestrictingTask(scope, ScopeList.Codes.Child, notificationType);
		}

		public void CheckResourcesAutoAssigned_WhenThereIsCancelledRestrictingTask(ZString scopeForRestrictions, ZString scopeForDIFTask, string notificationType)
		{
			SetupRegistryAndTasksForRestrictionsTesting(scopeForRestrictions, scopeForDIFTask, notificationType);
			TaskCapability_DIF.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			factory.Save();

			RunAssignment();

			ReloadTasksWithNewFactory();
			var errorMessage = $@"Should have auto-assigned to a resource in capability when tasks under DIF restrictions are cancelled
								(scope for restrictions = {scopeForRestrictions}, workflow for open task = {ScopeList.Codes.Workflow}, workflow for closed task = {scopeForDIFTask}, notification type = {notificationType})";
			Assert(errorMessage, !TaskCapability_Reloaded.P9_GS_NKAssignedStaffMember.IsEmpty);
		}

		public void CheckResourcesNotAutoAssigned_WhenOpenAssignedRestrictingTaskExists(ZString scopeForRestrictions, string scopeForDIFTask, string notificationType)
		{
			Assert("Prerequisite", scopeForRestrictions == ScopeList.Codes.Job || scopeForDIFTask == ScopeList.Codes.Workflow || scopeForRestrictions == scopeForDIFTask);
			SetupRegistryAndTasksForRestrictionsTesting(scopeForRestrictions, scopeForDIFTask, notificationType);
			TaskCapability_DIF.P9_GS_NKAssignedStaffMember = Resource1.GS_Code;
			factory.Save();
			ReloadTasksWithNewFactory();
			Assert("Prerequisite", TaskCapability_Reloaded.RequiresResourceWithCapability);
			Assert("Prerequisite", !TaskCapability_DIF_Reloaded.RequiresResourceWithCapability);

			RunAssignment();

			ReloadTasksWithNewFactory();
			var errorMessage = $@"Should have not auto-assigned to a resource due to DIF restrictions
								when an unassigned capability task and an assigned task are under the restrictions
								(scope for restrictions = {scopeForRestrictions}, workflow for task 1 = {ScopeList.Codes.Workflow}, workflow for task 2 = {scopeForDIFTask}, notification type = {notificationType})";
			Assert(errorMessage, TaskCapability_Reloaded.P9_GS_NKAssignedStaffMember.IsEmpty);
		}
		public void CheckResourcesNotAutoAssigned_WhenClosedRestrictingTaskExists(ZString scopeForRestrictions, string scopeForDIFTask, string notificationType)
		{
			Assert("Prerequisite", scopeForRestrictions == ScopeList.Codes.Job || scopeForDIFTask == ScopeList.Codes.Workflow || scopeForRestrictions == scopeForDIFTask);
			SetupRegistryAndTasksForRestrictionsTesting(scopeForRestrictions, scopeForDIFTask, notificationType);
			TaskCapability_DIF.P9_GS_NKAssignedStaffMember = Resource1.GS_Code;
			TaskCapability_DIF.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			factory.Save();
			ReloadTasksWithNewFactory();
			Assert("Prerequisite", TaskCapability_Reloaded.RequiresResourceWithCapability);
			Assert("Prerequisite", !TaskCapability_DIF_Reloaded.RequiresResourceWithCapability);

			RunAssignment();

			ReloadTasksWithNewFactory();
			var errorMessage = $@"Should have not auto-assigned to a resource due to DIF restrictions on a closed task
								(scope for restrictions = {scopeForRestrictions}, workflow for task 1 = {ScopeList.Codes.Workflow}, workflow for task 2 = {scopeForDIFTask}, notification type = {notificationType})";
			Assert(errorMessage, TaskCapability_Reloaded.P9_GS_NKAssignedStaffMember.IsEmpty);
		}

		public void CheckResourceAutoAssignedToOneTaskOnly_WhenTwoOpenUnassignedTasksUnderRestrictionsAreInDifferentWorkflows(ZString scopeForRestrictions, string scopeForDIFTask, string notificationType)
		{
			Assert("Prerequisite", scopeForRestrictions == ScopeList.Codes.Job || scopeForRestrictions == scopeForDIFTask);
			AssertNotEquals("Prerequisite", ScopeList.Codes.Workflow, scopeForDIFTask);
			SetupRegistryAndTasksForRestrictionsTesting(scopeForRestrictions, scopeForDIFTask, notificationType);
			factory.Save();
			ReloadTasksWithNewFactory();
			Assert("Prerequisite", TaskCapability_Reloaded.RequiresResourceWithCapability);
			Assert("Prerequisite", TaskCapability_DIF_Reloaded.RequiresResourceWithCapability);

			RunAssignment();

			ReloadTasksWithNewFactory();
			var errorMessage = $@"Should have auto-assigned one task only due to DIF restrictions when there are two unassigned capability tasks under the restrictions in different workflows
								(scope for restrictions = {scopeForRestrictions}, workflow for task 1 = {ScopeList.Codes.Workflow}, workflow for task 2 = {scopeForDIFTask}, notification type = {notificationType})";
			Assert(errorMessage,
				TaskCapability_Reloaded.P9_GS_NKAssignedStaffMember.IsEmpty && !TaskCapability_DIF_Reloaded.P9_GS_NKAssignedStaffMember.IsEmpty ||
				!TaskCapability_Reloaded.P9_GS_NKAssignedStaffMember.IsEmpty && TaskCapability_DIF_Reloaded.P9_GS_NKAssignedStaffMember.IsEmpty);
		}

		public void CheckResourcesAutoAssignedToOneTaskOnly_WhenTwoOpenUnassignedTasksUnderRestrictionsAreInTheSameWorkflow(ZString scopeForRestrictions, string notificationType)
		{
			SetupRegistryAndTasksForRestrictionsTesting(scopeForRestrictions, ScopeList.Codes.Workflow, notificationType);
			factory.Save();
			ReloadTasksWithNewFactory();
			Assert("Prerequisite", TaskCapability_Reloaded.RequiresResourceWithCapability);
			Assert("Prerequisite", TaskCapability_DIF_Reloaded.RequiresResourceWithCapability);

			RunAssignment();

			ReloadTasksWithNewFactory();
			var errorMessage = $@"Should have auto-assigned one task only due to DIF restrictions when there are two unassigned capability tasks under the restrictions in the same workflow
								(scope for restrictions = {scopeForRestrictions}, workflow = {ScopeList.Codes.Workflow}, notification type = {notificationType})";
			Assert(errorMessage,
				TaskCapability_Reloaded.P9_GS_NKAssignedStaffMember.IsEmpty && !TaskCapability_DIF_Reloaded.P9_GS_NKAssignedStaffMember.IsEmpty ||
				!TaskCapability_Reloaded.P9_GS_NKAssignedStaffMember.IsEmpty && TaskCapability_DIF_Reloaded.P9_GS_NKAssignedStaffMember.IsEmpty);
		}

		public void CheckResourcesAutoAssigned_WhenOpenTasksAreInDifferentScopes(ZString scopeForRestrictions, string scopeForDIFTask, string notificationType)
		{
			SetupRegistryAndTasksForRestrictionsTesting(scopeForRestrictions, scopeForDIFTask, notificationType);
			factory.Save();
			ReloadTasksWithNewFactory();
			Assert("Prerequisite", TaskCapability_Reloaded.RequiresResourceWithCapability);
			Assert("Prerequisite", TaskCapability_DIF_Reloaded.RequiresResourceWithCapability);

			RunAssignment();

			ReloadTasksWithNewFactory();
			var errorMessage = $@"Should have auto-assigned to the resource in capability when the tasks under DIF restrictions are in different scopes
								(scope for restrictions = {scopeForRestrictions}, workflow for task 1 = {ScopeList.Codes.Workflow}, workflow for task 2 = {scopeForDIFTask}, notification type = {notificationType})";
			Assert(errorMessage, !TaskCapability_Reloaded.P9_GS_NKAssignedStaffMember.IsEmpty);
		}

		public void RunAssignment(Action<ProcessHeader[]> preassignmentAction = null)
		{
			var workflows = new ProcessHeader[] {
				CurrentWorkflow,
				PreReqWorkflow,
				PostReqWorkflow,
				ChildWorkflow,
				WorkflowForDIFTask
			};

			Logger = testCase.RunAutoAssignmentAndGetLog(Buffer, workflows.Where(w => w != null).Select(w => w.PK), preassignmentAction);
		}

		public ILogger Logger;
	}
}
