using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(WorkflowManagementViewModel))]
	class WorkflowManagementViewModelTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new WorkflowManagementViewModel(ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory));
		}

		public void TestAllProcessHeaders_ShouldIncludeJobAndWorkflows()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var viewModel = new WorkflowManagementViewModel(jobHeader);

			AssertEquals(3, viewModel.AllProcessHeaders.Count);
			AssertEquals(jobHeader, viewModel.AllProcessHeaders[0]);
			AssertEquals(workflow1, viewModel.AllProcessHeaders[1]);
			AssertEquals(workflow2, viewModel.AllProcessHeaders[2]);

			var workflow3 = viewModel.AllProcessHeaders.AddNew();
			AssertEquals(workflow1.FH_ParentId, workflow3.FH_ParentId);
			AssertEquals(workflow1.FH_ParentTableCode, workflow3.FH_ParentTableCode);
			AssertEquals(workflow1.FH_FH_ParentHeader, workflow3.FH_FH_ParentHeader);
		}

		public void TestAllProcessHeaders_ShouldOrderBySequence()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();

			workflow3.MakePrerequisiteOf(workflow2);
			workflow2.MakePrerequisiteOf(workflow1);

			Factory.Save();

			var viewModel = new WorkflowManagementViewModel(new BusinessObjectFactory().Load<ProcessJobHeader>(jobHeader.PK));

			AssertEquals(jobHeader.PK, viewModel.AllProcessHeaders[0].PK);
			AssertEquals(workflow3.PK, viewModel.AllProcessHeaders[1].PK);
			AssertEquals(workflow2.PK, viewModel.AllProcessHeaders[2].PK);
			AssertEquals(workflow1.PK, viewModel.AllProcessHeaders[3].PK);
		}

		public void TestDeleteJobHeader_ShouldNotAllow()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var viewModel = new WorkflowManagementViewModel(jobHeader);

			AssertEquals(3, viewModel.AllProcessHeaders.Count);
			WorkflowManagementViewModel.TryDelete_ForTest(jobHeader);

			AssertEquals("The Job-level workflow cannot be deleted.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(3, viewModel.AllProcessHeaders.Count);
		}

		public void TestDeleteWorkflowWithNoTasks_ShouldAllow()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var viewModel = new WorkflowManagementViewModel(jobHeader);

			AssertEquals(3, viewModel.AllProcessHeaders.Count);
			WorkflowManagementViewModel.TryDelete_ForTest(workflow1);

			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(2, viewModel.AllProcessHeaders.Count);
		}

		public void TestDeleteWorkflowWithNoWorkflow()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var viewModel = new WorkflowManagementViewModel(jobHeader);

			AssertNoExceptionThrown(() => WorkflowManagementViewModel.TryDelete_ForTest(new[] { jobHeader.ProcessHeaders.AddNew(), null }));
		}

		public void TestDeleteWorkflowWithTasks_ShouldPromptUser_TasksDeleted()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			workflow1.MakePrerequisiteOf(workflow2);

			var viewModel = new WorkflowManagementViewModel(jobHeader);

			var task = jobHeader.Parent.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = workflow1.PK;

			var completionStatement = workflow1.CompletionStatementTasksIncludingChildWorkflowTasks.AddNew();

			AssertEquals(3, viewModel.AllProcessHeaders.Count);
			WorkflowManagementViewModel.TryDelete_ForTest(workflow1);

			AssertEquals(2, viewModel.AllProcessHeaders.Count);

			AssertEquals(true, workflow1.IsDeleted);
			AssertEquals(true, task.IsDeleted);
			AssertEquals(true, completionStatement.IsDeleted);
		}

		public void TestDeleteWorkflow_ApprovedShapeExists_SelectingApprovedAndNonApprovedWorkflows()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3");

			var shape1_approved = Factory.New<IBMNCNShape>();
			shape1_approved.BNS_RelatedEntityID = workflow1.PK;
			shape1_approved.Approve(GlbStaff.CurrentUser.GS_Code);
			var shape2_notApproved = Factory.New<IBMNCNShape>();
			shape2_notApproved.BNS_RelatedEntityID = workflow2.PK;
			var shape3_approved = Factory.New<IBMNCNShape>();
			shape3_approved.BNS_RelatedEntityID = workflow3.PK;
			shape3_approved.Approve(GlbStaff.CurrentUser.GS_Code);

			var viewModel = new WorkflowManagementViewModel(jobHeader);
			WorkflowManagementViewModel.TryDelete_ForTest(workflow1, workflow2, workflow3);

			AssertMultilineASCIIEquals("", @"The following workflows are part of an approved project plan and cannot be deleted:
	workflow1
	workflow3", UnitTestUserNotification.Instance.LastMessage.Text);

			AssertEquals(false, workflow1.IsDeleted);
			AssertEquals(false, workflow2.IsDeleted);
			AssertEquals(false, workflow3.IsDeleted);
			AssertEquals(false, ((BusinessObject)shape1_approved).IsDeleted);
			AssertEquals(false, ((BusinessObject)shape2_notApproved).IsDeleted);
			AssertEquals(false, ((BusinessObject)shape3_approved).IsDeleted);
		}

		public void TestDeleteWorkflow_ApprovedShapeExists_SelectingJustApprovedWorkflow()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3");

			var shape1_approved = Factory.New<IBMNCNShape>();
			shape1_approved.BNS_RelatedEntityID = workflow1.PK;
			shape1_approved.Approve(GlbStaff.CurrentUser.GS_Code);
			var shape2_notApproved = Factory.New<IBMNCNShape>();
			shape2_notApproved.BNS_RelatedEntityID = workflow2.PK;

			var viewModel = new WorkflowManagementViewModel(jobHeader);
			WorkflowManagementViewModel.TryDelete_ForTest(workflow1);

			AssertMultilineASCIIEquals("", @"The following workflows are part of an approved project plan and cannot be deleted:
	workflow1", UnitTestUserNotification.Instance.LastMessage.Text);

			AssertEquals(false, workflow1.IsDeleted);
			AssertEquals(false, workflow2.IsDeleted);
			AssertEquals(false, workflow3.IsDeleted);
			AssertEquals(false, ((BusinessObject)shape1_approved).IsDeleted);
			AssertEquals(false, ((BusinessObject)shape2_notApproved).IsDeleted);
		}

		public void TestDeleteWorkflow_ApprovedShapeExists_WithTasks()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var task = BMSTestHelper.CreateTask(workflow);

			var shape_approved = Factory.New<IBMNCNShape>();
			shape_approved.BNS_RelatedEntityID = workflow.PK;
			shape_approved.Approve(GlbStaff.CurrentUser.GS_Code);

			var viewModel = new WorkflowManagementViewModel(jobHeader);
			WorkflowManagementViewModel.TryDelete_ForTest(workflow);

			AssertMultilineASCIIEquals("", @"The following workflows are part of an approved project plan and cannot be deleted:
	workflow1", UnitTestUserNotification.Instance.LastMessage.Text);

			AssertEquals(false, workflow.IsDeleted);
			AssertEquals(false, ((BusinessObject)shape_approved).IsDeleted);
		}

		public void TestDeleteWorkflow_ShouldPromptToDeleteDescendantWorkflows()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3");
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow4");

			var childWorkflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "childWorkflow1");
			var childWorkflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "childWorkflow2");
			var grandchildWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "grandchildWorkflow");

			childWorkflow1.GetOrCreateLinkToParent(workflow1);
			childWorkflow2.GetOrCreateLinkToParent(workflow2);
			grandchildWorkflow.GetOrCreateLinkToParent(childWorkflow2);

			AssertEquals(7, jobHeader.ProcessHeaders.Count);

			var viewModel = new WorkflowManagementViewModel(jobHeader);
			WorkflowManagementViewModel.TryDelete_ForTest(workflow1, workflow2, workflow3);

			AssertMultilineASCIIEquals("", @"The following workflows have descendant workflows. Would you like to delete the descendants also?
workflow1:
	• childWorkflow1
workflow2:
	• childWorkflow2
	• grandchildWorkflow", UnitTestUserNotification.Instance.LastMessage.Text);

			AssertEquals(1, jobHeader.ProcessHeaders.Count);
			AssertEquals(workflow4, jobHeader.ProcessHeaders[0]);
		}

		public void TestDeleteWorkflow_WithTaskThatHasFormFlowType()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var orgPK = helper.CreateClient("Org1");
			var warehouse = helper.CreateWarehouse("WHS", "A");
			var product = (OrgSupplierPart)helper.CreateProduct(orgPK, "Pro01");
			var receive = helper.CreateWhsReceiveWithInventory(orgPK, warehouse.PK, "R1", product.PK, 10m);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			workflow.FH_ParentTableCode = "WD";
			workflow.FH_ParentId = receive.PK;
			Factory.Save();

			var task = BMSTestHelper.CreateTask(workflow);
			task.P9_ParentID = receive.PK;
			task.P9_ParentTableCode = "WD";
			task.P9_FormFlowType = "WUL";
			Factory.Save();

			var viewModel = new WorkflowManagementViewModel(jobHeader);
			WorkflowManagementViewModel.TryDelete_ForTest(workflow);

			AssertMultilineASCIIEquals("", @"The following workflow(s) have a system maintained task linked to jobs and cannot be deleted:
	workflow1", UnitTestUserNotification.Instance.LastMessage.Text);

			AssertEquals(false, workflow.IsDeleted);
			AssertEquals(false, task.IsDeleted);
		}
	}
}
