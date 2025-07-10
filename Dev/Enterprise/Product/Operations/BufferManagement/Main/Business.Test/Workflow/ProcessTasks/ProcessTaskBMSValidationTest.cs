using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.BufferManagement.Business.Test
{
	class ProcessTaskBMSValidationTest : BMSTestCaseWithFactory
	{
		public void TestProcessHeaderValidation_ShouldNotBeRequiredWhenJobNotAssociatedWithABufferManagementSystem()
		{
			var processHeaderCount = Factory.GetDatabaseCount(typeof(ProcessHeader));

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var salesEnquiry = Factory.NewWithValidTestData<SalesEnquiry>();

			var orgTask = org.WorkflowItems.AddNew();
			var salesTask = salesEnquiry.WorkflowItems.AddNew();

			orgTask.Validation.ValidateAll();
			salesTask.Validation.ValidateAll();

			AssertNotEquals("Org task should get a default workflow specified", ZGuid.Empty, orgTask.P9_FH_ProcessHeader);
			AssertNoErrors(orgTask.P9_FH_ProcessHeaderInfo);

			AssertEquals("Should be no workflow on task for job not linked to a BMS", ZGuid.Empty, salesTask.P9_FH_ProcessHeader);
			AssertNoErrors("No errors when no workflow specified", salesTask.P9_FH_ProcessHeaderInfo);
		}

		public void TestProcessHeaderValidation_ShouldNotBeRequiredWhenJobAssociatedWithABufferManagementSystem_ButRelatedJobTypeIsDisabled()
		{
			var processHeaderCount = Factory.GetDatabaseCount(typeof(ProcessHeader));

			var system = BMSTestHelper.CreateSystemAndRelatedWorkflowType(Factory, "ORG", isActive: false);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgTask = org.WorkflowItems.AddNew();

			orgTask.Validation.ValidateAll();

			AssertEquals("Org task should not get a default workflow specified", ZGuid.Empty, orgTask.P9_FH_ProcessHeader);
			AssertNoErrors(orgTask.P9_FH_ProcessHeaderInfo);
		}

		public void TestProcessHeaderValidation_ShouldNotValidate_WhenJobAssociatedWithABufferManagementSystem_ButRelatedJobTypeIsDisabled_AndInvalidHeaderEntered()
		{
			var processHeaderCount = Factory.GetDatabaseCount(typeof(ProcessHeader));

			var system = BMSTestHelper.CreateSystemAndRelatedWorkflowType(Factory, "ORG", isActive: false);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgTask = org.WorkflowItems.AddNew();
			orgTask.P9_FH_ProcessHeader = ZGuid.BrettsGuid;

			orgTask.Validation.ValidateAll();

			AssertEquals(ZGuid.BrettsGuid, orgTask.P9_FH_ProcessHeader);
			AssertNoErrors(orgTask.P9_FH_ProcessHeaderInfo);
		}

		public void TestProcessHeaderValidation_ShowOnlyRequireProcessHeaderAndHasWarning_WhenBMSEnabled()
		{
			BMSTestHelper.CreateSystemAndRelatedWorkflowType(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode, true);
			var dummy = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(dummy, Factory);

			BMSRegistry.Instance.WorkflowManagementMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WorkflowManagementModes.Codes.BasicWorkflow);
			jobHeader.ProcessHeaders.AddNew();
			var firstTask = dummy.WorkflowItems.AddNew();

			firstTask.Validation.ValidateP9_FH_ProcessHeader();
			Assert(firstTask.P9_FH_ProcessHeader.IsEmpty);
			AssertNoError(firstTask.P9_FH_ProcessHeaderInfo, "Please enter a Workflow.");
			AssertNoWarning(firstTask.P9_FH_ProcessHeaderInfo, "Please enter a Workflow.");

			Factory.Save();

			firstTask.Validation.ValidateP9_FH_ProcessHeader();
			Assert(firstTask.P9_FH_ProcessHeader.IsEmpty);
			AssertNoError(firstTask.P9_FH_ProcessHeaderInfo, "Please enter a Workflow.");
			AssertNoWarning(firstTask.P9_FH_ProcessHeaderInfo, "Please enter a Workflow.");

			BMSRegistry.Instance.WorkflowManagementMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WorkflowManagementModes.Codes.EnhancedWorkflow);

			firstTask.Validation.ValidateP9_FH_ProcessHeader();
			Assert(firstTask.P9_FH_ProcessHeader.IsEmpty);
			AssertNoError(firstTask.P9_FH_ProcessHeaderInfo, "Please enter a Workflow.");
			AssertHasWarning(firstTask.P9_FH_ProcessHeaderInfo, "Please enter a Workflow.");

			var secondTask = dummy.WorkflowItems.AddNew();

			secondTask.Validation.ValidateP9_FH_ProcessHeader();
			Assert(secondTask.P9_FH_ProcessHeader.IsEmpty);
			AssertHasError(secondTask.P9_FH_ProcessHeaderInfo, "Please enter a Workflow.");
			AssertNoWarning(secondTask.P9_FH_ProcessHeaderInfo, "Please enter a Workflow.");

			Factory.Save();

			firstTask.Validation.ValidateP9_FH_ProcessHeader();
			Assert(firstTask.P9_FH_ProcessHeader.IsEmpty);
			AssertNoError(firstTask.P9_FH_ProcessHeaderInfo, "Please enter a Workflow.");
			AssertHasWarning(firstTask.P9_FH_ProcessHeaderInfo, "Please enter a Workflow.");

			secondTask.Validation.ValidateP9_FH_ProcessHeader();
			Assert(secondTask.P9_FH_ProcessHeader.IsEmpty);
			AssertNoError(secondTask.P9_FH_ProcessHeaderInfo, "Please enter a Workflow.");
			AssertHasWarning(secondTask.P9_FH_ProcessHeaderInfo, "Please enter a Workflow.");
		}

		public void TestProcessHeaderValidation_ShouldRequireProcessHeader_WhenTemplateHasWorkflows()
		{
			BMSTestHelper.CreateSystem(Factory, "WKI");

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "WKI";

			var task = template.WorkflowItems.Tasks.AddNew();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			AssertEquals("Precondition", ZGuid.Empty, task.P9_FH_ProcessHeader);
			AssertEquals("Precondition", 1, template.ProcessHeaders.Count);
			var header = template.ProcessHeaders[0];
			AssertNull("Precondition: should be a job header", header.ParentHeader);

			task.Validation.ValidateAll();
			AssertNoErrors(task.P9_FH_ProcessHeaderInfo);

			var workflow = template.ProcessHeaders.AddNew();
			task.P9_FH_ProcessHeader = ZGuid.Empty;
			task.Validation.ValidateAll();
			AssertHasError(task.P9_FH_ProcessHeaderInfo, "This workflow template includes at least one workflow. Please assign all tasks to a workflow from the template or remove all workflows from the template.");

			task.P9_FH_ProcessHeader = workflow.PK;
			task.Validation.ValidateAll();
			AssertNoErrors(task.P9_FH_ProcessHeaderInfo);
		}

		public void TestEstDurationWarning_ClosedTask()
		{
			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);
			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			AssertNoWarnings(task.P9_EstDurationInfo);

			task.P9_EstDuration = ZDateTime.Empty;
			AssertHasWarning(task.P9_EstDurationInfo, "Tasks on workflows that are in a Buffer component should have estimates entered. If they do not, transfer rules may not operate correctly.");

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertNoWarnings(task.P9_EstDurationInfo);
		}

		public void TestValidateAssignedStaff_WhenCapacityAssigned()
		{
			var system = Factory.New<BMSystem>();
			var buffer = system.Components.AddNew();
			buffer.FC_Type = BMComponentTypeList.Codes.Buffer;

			var workflow = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory).ProcessHeaders.AddNew();
			workflow.FH_FC_CurrentComponent = buffer.PK;

			var task = workflow.Parent.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = workflow.PK;

			task.Validation.ValidateAll();
			AssertEquals(ProcessTaskStatusCodeList.Codes.Open, task.P9_Status);
			AssertHasWarning(task.P9_GS_NKAssignedStaffMemberInfo, "Tasks on workflows that are in a Buffer component should be assigned. If they are not, transfer rules may not operate correctly.");

			task.P9_G4_RequiredCapability = Factory.New<GlbCapability>().PK;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			AssertNoWarning(task.P9_GS_NKAssignedStaffMemberInfo, "Tasks on workflows that are in a Buffer component should be assigned. If they are not, transfer rules may not operate correctly.");
		}
	}
}
