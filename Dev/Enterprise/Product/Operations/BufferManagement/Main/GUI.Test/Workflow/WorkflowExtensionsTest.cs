using System.Linq;
using System.Windows.Forms;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.BufferManagement.GUI.Test
{
	class WorkflowExtensionsTest : BMSTestCaseWithFactory
	{
		#region MoveToComponent

		public void TestMoveToComponent_SameComponent_ShouldShowErrorByDefault()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow", buffer);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			workflow.MoveToComponent(buffer);

			AssertEquals("The destination component cannot be the same as the source component.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);

			UnitTestUserNotification.Instance.ClearMessages();
			workflow.MoveToComponent(buffer, showConfirmationAndErrors: false);

			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
		}

		public void TestMoveToComponent_TemplateWorkflows_ShouldNotBeMovable()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var bucket = BMSTestHelper.CreateBucket(system);
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "WKI");
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI");

			var templateWorkflow1 = BMSTestHelper.CreateWorkflow(template, "templateWorkflow1");

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			templateWorkflow1.MoveToComponent(bucket);

			AssertMultilineASCIIEquals("", @"The following workflow was not able to be moved because Template workflows cannot be moved into a Buffer Management System Component:

templateWorkflow1", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
			AssertEquals(true, templateWorkflow1.FH_FC_CurrentComponent.IsEmpty);
		}

		public void TestMoveJobHeader_SameComponent_ShouldOnlyShowErrorOnce()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow", buffer);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow", buffer);

			jobHeader.MoveJobToComponent(new[] { buffer.PK }, buffer);

			AssertEquals(1, UnitTestUserNotification.Instance.PreviousMessages.Count(x => !x.WasNone));
			AssertEquals("The destination component cannot be the same as the source component.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestMoveJobHeader_WithSomeEligibleWorkflowsButNotAll_ShouldMoveOnlyEligibleWorkflows()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer1 = BMSTestHelper.CreateBuffer(system, "Buffer 1");
			var buffer2 = BMSTestHelper.CreateBuffer(system, "Buffer 2");
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 1", buffer1);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 2", buffer1);
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 3", buffer2);
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 4", buffer2);

			Factory.Save();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			var movedWorkflows = jobHeader.MoveJobToComponent(new[] { buffer1.PK, buffer2.PK }, buffer2);

			AssertEquals(1, UnitTestUserNotification.Instance.PreviousMessages.Count(x => !x.WasNone));
			AssertEquals("Are you sure to move 2 workflows in job [Organization (XVBQP68SIYXQ) - Job Organization (XVBQP68SIYXQ) is complete.] to component [Buffer 2]? Note that based on transfer rules, your workflows may move back to their original components.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(2, movedWorkflows.Item1);
			AssertContainsExactElementsInAnyOrder(new[] { "Workflow 1", "Workflow 2" }, movedWorkflows.Item2.Select(x => x.FH_CompletionStatement));
			AssertEquals(buffer2.PK, workflow1.FH_FC_CurrentComponent);
			AssertEquals(buffer2.PK, workflow2.FH_FC_CurrentComponent);
			AssertEquals(false, workflow3.HasChanges);
			AssertEquals(false, workflow4.HasChanges);
		}

		public void TestMoveJobHeader_WithOneEligibleWorkflowButNotAll_ShouldMoveOnlyEligibleWorkflow()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer1 = BMSTestHelper.CreateBuffer(system, "Buffer 1");
			var buffer2 = BMSTestHelper.CreateBuffer(system, "Buffer 2");
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 1", buffer1);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 2", buffer2);
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 3", buffer2);

			Factory.Save();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			var movedWorkflows = jobHeader.MoveJobToComponent(new[] { buffer1.PK, buffer2.PK }, buffer2);

			AssertEquals(1, UnitTestUserNotification.Instance.PreviousMessages.Count(x => !x.WasNone));
			AssertEquals("Are you sure to move workflow Organization (XVBQP68SIYXQ) - Workflow 1 from component [Buffer 1] to component [Buffer 2]? Note that based on transfer rules, your workflows may move back to their original components.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(1, movedWorkflows.Item1);
			AssertContainsExactElementsInAnyOrder(new[] { "Workflow 1" }, movedWorkflows.Item2.Select(x => x.FH_CompletionStatement));
			AssertEquals(buffer2.PK, workflow1.FH_FC_CurrentComponent);
			AssertEquals(false, workflow2.HasChanges);
			AssertEquals(false, workflow3.HasChanges);
		}

		public void TestGroupMoveToComponent_ToSameComponent_WithMultipleWorkflows_ShouldNotShowErrorMessage()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow", buffer);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow", buffer);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			IProcessHeaderExtensions.GroupMoveToComponent(new[] { workflow1, workflow2 }, buffer);

			AssertEquals(1, UnitTestUserNotification.Instance.PreviousMessages.Count(x => !x.WasNone));
			AssertEquals("Are you sure to move 2 workflows to component [buffer]? Note that based on transfer rules, your workflows may move back to their original components.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestGroupMoveToComponent_ToSameComponent_WithSingleWorkflow_ShouldShowErrorMessage()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow", buffer);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			IProcessHeaderExtensions.GroupMoveToComponent(new[] { workflow1 }, buffer);

			AssertEquals(1, UnitTestUserNotification.Instance.PreviousMessages.Count(x => !x.WasNone));
			AssertEquals("The destination component cannot be the same as the source component.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestGroupMoveToComponent_WithMultipleWorkflows_WithJobDescription_ShouldIncludeJobDescriptionInConfirmationMessage()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow", buffer);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow", buffer);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			IProcessHeaderExtensions.GroupMoveToComponent(new[] { workflow1, workflow2 }, buffer, "Shalala");

			AssertEquals(1, UnitTestUserNotification.Instance.PreviousMessages.Count(x => !x.WasNone));
			AssertEquals("Are you sure to move 2 workflows in job [Shalala] to component [buffer]? Note that based on transfer rules, your workflows may move back to their original components.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestGroupMoveToComponent_WithSingleWorkflow_WithJobDescription_ShouldNotIncludeJobDescriptionInConfirmationMessage()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system);
			var bucket = BMSTestHelper.CreateBucket(system);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow", buffer);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			IProcessHeaderExtensions.GroupMoveToComponent(new[] { workflow1 }, bucket, "Shalala");

			AssertEquals(1, UnitTestUserNotification.Instance.PreviousMessages.Count(x => !x.WasNone));
			AssertEquals("Are you sure to move workflow Organization (XVBQP68SIYXQ) - workflow from component [buffer] to component [bucket]? Note that based on transfer rules, your workflows may move back to their original components.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestGroupMoveToComponent_TemplateWorkflows_ShouldNotBeMovable()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system);
			var bucket = BMSTestHelper.CreateBucket(system);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "WKI");
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI");

			ProcessHeader normalWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow", buffer);
			var templateWorkflow1 = BMSTestHelper.CreateWorkflow(template, "templateWorkflow1");
			var templateWorkflow2 = BMSTestHelper.CreateWorkflow(template, "templateWorkflow2");

			//First try to move the 2 template workflows
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			IProcessHeaderExtensions.GroupMoveToComponent(new[] { templateWorkflow1, templateWorkflow2 }, bucket, "Shalala");

			AssertMultilineASCIIEquals("", @"The following workflows were not able to be moved because Template workflows cannot be moved into a Buffer Management System Component:

templateWorkflow1
templateWorkflow2", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
			AssertEquals(true, templateWorkflow1.FH_FC_CurrentComponent.IsEmpty);
			AssertEquals(true, templateWorkflow2.FH_FC_CurrentComponent.IsEmpty);

			//Now try to move all 3 workflows at once
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			IProcessHeaderExtensions.GroupMoveToComponent(new[] { normalWorkflow, templateWorkflow1, templateWorkflow2 }, bucket, "Shalala");
			AssertMultilineASCIIEquals("", @"The following workflows were not able to be moved because Template workflows cannot be moved into a Buffer Management System Component:

templateWorkflow1
templateWorkflow2", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
			AssertEquals(bucket.PK, normalWorkflow.FH_FC_CurrentComponent);
			AssertEquals(true, templateWorkflow1.FH_FC_CurrentComponent.IsEmpty);
			AssertEquals(true, templateWorkflow2.FH_FC_CurrentComponent.IsEmpty);
		}

		#endregion
	}
}
