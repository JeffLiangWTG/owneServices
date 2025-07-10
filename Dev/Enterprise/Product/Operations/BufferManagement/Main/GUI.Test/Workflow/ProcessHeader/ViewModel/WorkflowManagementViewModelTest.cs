using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(WorkflowManagementViewModel))]
	class WorkflowManagementViewModelTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new WorkflowManagementViewModel(ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory));
		}

		public void TestDeleteWorkflowWithTasks_ShouldPromptUser_TasksNotDeleted()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			workflow1.MakePrerequisiteOf(workflow2);
			workflow1.FH_CompletionStatement = "Recur";

			var viewModel = new WorkflowManagementViewModel(jobHeader);

			var task = jobHeader.Parent.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = workflow1.PK;

			var completionStatement = workflow1.CompletionStatementTasksIncludingChildWorkflowTasks.AddNew();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);

			AssertEquals(3, viewModel.AllProcessHeaders.Count);
			WorkflowManagementViewModel.TryDelete_ForTest(workflow1);

			AssertEquals(string.Format("Delete workflow 'Recur' and all associated tasks?", workflow1.FH_CompletionStatement), UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(3, viewModel.AllProcessHeaders.Count);

			AssertEquals(false, workflow1.IsDeleted);
			AssertEquals(false, task.IsDeleted);
			AssertEquals(false, completionStatement.IsDeleted);
		}

		public void TestDeleteWorkflow_ShouldPromptToDeleteDescendantWorkflows_AnsweringNo()
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

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			var viewModel = new WorkflowManagementViewModel(jobHeader);
			WorkflowManagementViewModel.TryDelete_ForTest(workflow1, workflow2, workflow3);

			AssertMultilineASCIIEquals("", @"The following workflows have descendant workflows. Would you like to delete the descendants also?
workflow1:
	• childWorkflow1
workflow2:
	• childWorkflow2
	• grandchildWorkflow", UnitTestUserNotification.Instance.LastMessage.Text);

			AssertEquals(4, jobHeader.ProcessHeaders.Count);
			AssertContainsExactElementsInAnyOrder(new[] { workflow4, childWorkflow1, childWorkflow2, grandchildWorkflow }, jobHeader.ProcessHeaders);
		}
	}
}
