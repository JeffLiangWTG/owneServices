using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	public class RestrictionsAndCapabilitiesTestingEnvironment : IDisposable
	{
		public RestrictionsAndCapabilitiesTestingEnvironment(IWorkflowCapabilityAssignerTestCase testCase, bool bothResourcesOwnCapability = true)
		{
			this.testCase = testCase;
			factory = testCase.Factory;
			helper = new WorkflowCapabilityAssignerTestHelper(factory);
			SetupRestrictionsTestingEnvironment();
			SetupRegistryAndTasksForRestrictionsTesting();
		}

		public void Dispose()
		{
			BMSTestHelper.ClearTaskAssignmentRestrictions();
		}

		readonly IWorkflowCapabilityAssignerTestCase testCase;
		readonly BusinessObjectFactory factory;
		readonly WorkflowCapabilityAssignerTestHelper helper;

		public BMComponent Buffer;

		public ProcessHeader Workflow;

		public GlbCapability Capability1;
		public GlbCapability Capability2;

		public GlbStaff ResourceWithCapability1;
		public GlbStaff ResourceWithCapability2;

		public ProcessTask TaskCapability1;
		public ProcessTask TaskCapability2;

		public ProcessTask TaskCapability1_Reloaded;
		public ProcessTask TaskCapability2_Reloaded;

		public void ReloadTasksWithNewFactory()
		{
			var newFactory = new BusinessObjectFactory();
			TaskCapability1_Reloaded = newFactory.Load<ProcessTask>(TaskCapability1.PK);
			TaskCapability2_Reloaded = newFactory.Load<ProcessTask>(TaskCapability2.PK);
		}

		void SetupRestrictionsTestingEnvironment()
		{
			Buffer = BMSTestHelper.CreateBuffer(testCase.System, "Buffer");
			Buffer.FC_AutoAssignTasksAge = new ZInt(60).GetDateTimeFromMinutes();

			Capability1 = testCase.Factory.NewWithValidTestData<GlbCapability>();
			Capability1.G4_Code = "AUT";
			Capability1.G4_AllowTaskAutoAssignment = true;

			Capability2 = testCase.Factory.NewWithValidTestData<GlbCapability>();
			Capability2.G4_Code = "OTH";
			Capability2.G4_AllowTaskAutoAssignment = true;

			ResourceWithCapability1 = helper.CreateResourceWithHomeBranchDeptSet("Frodo", Capability1);
			ResourceWithCapability1.GS_Code = "FRO";
			ResourceWithCapability2 = helper.CreateResourceWithHomeBranchDeptSet("Balrog", Capability2);
			ResourceWithCapability2.GS_Code = "BAL";

			var job = testCase.Factory.NewWithValidTestData<OrgHeader>();
			job.OH_Code = "MYORG";
			var jobHeader = ProcessJobHeader.GetForParent(job, testCase.Factory);

			Workflow = jobHeader.ProcessHeaders.AddNew();
			Workflow.Name = "WFL";
			Workflow.FH_FC_CurrentComponent = Buffer.PK;
			Workflow.FH_AllowTaskAutoAssignment = true;
			Workflow.FH_ReleaseDateTime = ZDateTime.Today.AddDays(-1);
		}

		void SetupRegistry()
		{
			var categorisedWorkflowTaskTypeCollection = new CategorisedWorkflowTaskTypesCollection();

			var categorisedWorkflowTaskType = categorisedWorkflowTaskTypeCollection.AddNew();
			categorisedWorkflowTaskType.Code = "ORG";

			var workflowTaskType1 = categorisedWorkflowTaskType.TaskTypes.AddNew();
			var workflowTaskType2 = categorisedWorkflowTaskType.TaskTypes.AddNew();
			var workflowTaskType3 = categorisedWorkflowTaskType.TaskTypes.AddNew();

			workflowTaskType1.Code = "INV";
			workflowTaskType2.Code = "CDU";
			workflowTaskType3.Code = "CDF";

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categorisedWorkflowTaskTypeCollection);

			var collection = new TaskTypeRestrictionsCollection();

			//SAME type restriction - INV, CDU, CDF
			var restriction = collection.AddNew();
			restriction.Active = true;
			restriction.WorkflowType = "ORG";
			restriction.TaskType = "INV";
			restriction.NotificationType = NotificationTypeList.Codes.Error;
			restriction.Scope = ScopeList.Codes.Workflow;
			restriction.RestrictionType = RestrictionTypeList.Codes.SameResource;

			var samTaskType1 = restriction.TaskTypesCollection.AddNew();
			var samTaskType2 = restriction.TaskTypesCollection.AddNew();

			samTaskType1.Code = "CDU";
			samTaskType2.Code = "CDF";

			WorkflowDataRegistry.Instance.TaskAssignmentRestrictions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
		}

		public void SetupRegistryAndTasksForRestrictionsTesting()
		{
			SetupRegistry();

			TestCaseHelper.ClearTable(ProcessTasksSchema.Constants.TableName);

			TaskCapability1 = helper.CreateTask(Workflow, Capability1, null, 60);
			TaskCapability1.P9_Type = "INV";

			TaskCapability2 = helper.CreateTask(Workflow, Capability2, null, 60);
			TaskCapability2.P9_Type = "CDU";
		}

		public void RunAssignment(Action<ProcessHeader[]> preassignmentAction = null)
		{
			testCase.RunAutoAssignmentAndGetLog(Buffer, Workflow.PK, preassignmentAction);
		}
	}
}
