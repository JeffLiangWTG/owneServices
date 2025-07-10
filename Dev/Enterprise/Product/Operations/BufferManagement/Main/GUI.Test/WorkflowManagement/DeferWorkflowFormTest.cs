using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(DeferWorkflowForm))]
	class DeferWorkflowFormTest : ZFormBasherTest
	{
		[ExpectNoExceptions]
		public void TestDeferFormHandlesConcurrencyIssues()
		{
			Factory.RefreshEnabled = false;
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var buffer = BMSTestHelper.CreateBuffer(system);
			BMSTestHelper.LinkComponents(bucket, buffer);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_FC_CurrentComponent = buffer.PK;
			workflow1.FH_CompletionStatement = "workflow1";

			Factory.Save();

			var viewModel = new DeferWorkflowViewModel(workflow1, new[] { buffer.PK });
			using (var form = new DeferWorkflowForm(viewModel))
			{
				form.Show();
				viewModel.DoNotStartBeforeDate = ZDateTime.Today.AddDays(2);

				var anotherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var loadedWorkflow = anotherFactory.Load<ProcessHeader>(workflow1.PK);
				loadedWorkflow.FH_FC_CurrentComponent = bucket.PK;
				loadedWorkflow.DoNotStartBeforeDateLocal = ZDateTime.Today.AddDays(12);
				anotherFactory.Save();

				form.DeferButton.PerformClick();
			}
		}

		public void TestShowDeferForm_ShouldTakeNoActionOnPrerequisitesByDefault()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var buffer = BMSTestHelper.CreateBuffer(system);
			BMSTestHelper.LinkComponents(bucket, buffer);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", buffer);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2", buffer);
			workflow1.MakePrerequisiteOf(workflow2);

			Factory.Save();

			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(form =>
			{
				((DeferWorkflowForm)form).DataSource.DoNotStartBeforeDate = ZDateTime.Now.AddDays(1);
				((DeferWorkflowForm)form).DataSource.DeferralReason = WorkflowDeferralReasonsList.Codes.PrioritiesChanged;
			});

			var result = DeferWorkflowForm.ShowDeferForm(workflow1, new[] { buffer.PK });

			AssertEquals(DialogResult.OK, result.DialogResult);
			AssertEquals(1, result.WorkflowsDeferred.Count);
			VisualBoardsTestCase.AssertSamePK(workflow1, result.WorkflowsDeferred.Single());
		}

		public void TestCancel()
		{
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Jimminy Jillikers");
			Factory.Save();

			var loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			using (var form = new DeferWorkflowForm(new DeferWorkflowViewModel(loadedWorkflow, null)))
			{
				form.Show();

				loadedWorkflow.FH_DateAcceptability = DateAcceptabilityList.Codes.ExtendedStartExtendedFinish;

				form.CancelDeferButton.PerformClick();
				AssertEquals(ZString.Empty, workflow.FH_DateAcceptability);
			}
		}

		[TestDate(2013, 8, 23)]
		public void TestDefer()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var bucket1 = system.Components.AddNew();
			bucket1.FC_Name = "bucket1";
			var bucket2 = system.Components.AddNew();
			bucket2.FC_Name = "bucket2";

			var link = bucket1.FromMeToOthersLinks.AddNew();
			link.FL_FC_ComponentTo = bucket2.PK;

			var workflow = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory).ProcessHeaders.AddNew();
			workflow.FH_FC_CurrentComponent = bucket2.PK;
			workflow.FH_CompletionStatement = "workflow1";

			workflow.Validation.ValidateAll();
			AssertEquals(false, workflow.HasErrors);

			Factory.Save();

			var loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			var viewModel = new DeferWorkflowViewModel(loadedWorkflow, null);
			using (var form = new DeferWorkflowForm(viewModel))
			{
				form.Show();

				viewModel.DoNotStartBeforeDate = ZDateTime.Today.AddDays(1);
				viewModel.DeferralReason = WorkflowDeferralReasonsList.Codes.PrioritiesChanged;

				form.DeferButton.PerformClick();
				AssertEquals(bucket1.PK, workflow.FH_FC_CurrentComponent);
				BMSTestCaseWithFactory.AssertComponentChangedEventRaised(loadedWorkflow, ComponentChangeMode.Defer, bucket2.PK, bucket1.PK, deferReason: "PRI", status: "CLS");
			}
		}

		[TestDate(2013, 8, 23)]
		public void TestDefer_WithErrors()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var bucket1 = system.Components.AddNew();
			bucket1.FC_Name = "bucket1";
			var bucket2 = system.Components.AddNew();
			bucket2.FC_Name = "bucket2";

			var link = bucket1.FromMeToOthersLinks.AddNew();
			link.FL_FC_ComponentTo = bucket2.PK;

			var workflow = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory).ProcessHeaders.AddNew();
			workflow.FH_FC_CurrentComponent = bucket2.PK;
			workflow.FH_CompletionStatement = "workflow1";

			workflow.Validation.ValidateAll();
			AssertEquals(false, workflow.HasErrors);

			Factory.Save();

			var loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			var viewModel = new DeferWorkflowViewModel(loadedWorkflow, new[] { bucket2.PK });
			using (var form = new DeferWorkflowForm(viewModel))
			{
				form.Show();

				viewModel.DoNotStartBeforeDate = ZDateTime.Today.AddDays(-1);
				AssertEquals(true, viewModel.HasErrors);

				form.DeferButton.PerformClick();
				AssertEquals(bucket2.PK, workflow.FH_FC_CurrentComponent);
			}
		}

		public void TestShowDeferForm()
		{
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Jimminy Jillikers");
			Factory.Save();

			DeferWorkflowForm.ShowDeferForm(workflow, null);

			var lastForm = ZFormModaliser.LastFormShownDialogForTest;
			AssertType<DeferWorkflowForm>(lastForm);
		}

		public void TestShowDeferForm_NotInDatabase()
		{
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Jimminy Jillikers");

			DeferWorkflowForm.ShowDeferForm(workflow, null);

			AssertNull(ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals("Workflow must be saved first.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestShowDeferForm_Deleted()
		{
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Jimminy Jillikers");
			Factory.Save();
			workflow.Delete();
			Factory.Save();

			DeferWorkflowForm.ShowDeferForm(workflow, null);

			AssertNull(ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals("Workflow has been deleted.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestStatusBar_ShouldShowAllText()
		{
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Jimminy Jillikers");
			Factory.Save();

			var loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			var viewModel = new DeferWorkflowViewModel(loadedWorkflow, null);

			using (var form = new DeferWorkflowForm(viewModel))
			{
				form.Show();
				var dropEdit = form.Controls.Find("DoNotStartBeforeDateEdit", true).Single();
				dropEdit.Focus();

				var attribute = (ResourceStringDataAttribute)Attribute.GetCustomAttribute(viewModel.GetType().GetProperty("DoNotStartBeforeDate"), typeof(ResourceStringDataAttribute));
				var panel = form.MainStatusBar.Panels[0];

				AssertEquals("All of the text should fit on the status bar, and yet...", attribute.FullDescription, panel.Text);

				using (var graphics = Graphics.FromHwnd(form.Handle))
				{
					var desiredTextWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX((int)graphics.MeasureString(panel.Text, form.MainStatusBar.Font).Width);
					var panelWidth = panel.Width;

					AssertEquals("The text should be able to fit in the status bar, and yet...", true, desiredTextWidth < panelWidth);
				}
			}
		}

		public void TestDeferJob_WithMultipleWorkflows_ShouldCustomiseControls()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var bucket1 = BMSTestHelper.CreateBucket(system, "Bucket 1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "Bucket 2");
			var bucket3 = BMSTestHelper.CreateBucket(system, "Bucket 3");

			var link1 = bucket1.FromMeToOthersLinks.AddNew();
			link1.FL_FC_ComponentTo = bucket2.PK;
			var link2 = bucket2.FromMeToOthersLinks.AddNew();
			link2.FL_FC_ComponentTo = bucket3.PK;

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "Workflow 1", bucket2);
			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "Workflow 2", bucket2);
			var workflow3 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "Workflow 3", bucket3);
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader, "Closed workflow", bucket2);
			var childWorkflow = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "ChildWorkflow", bucket2);
			childWorkflow.GetOrCreateLinkToParent(workflow1);

			Factory.Save();

			var viewModel = new DeferWorkflowViewModel(jobHeader, new[] { bucket2.PK });
			using (var form = new DeferWorkflowForm(viewModel))
			{
				form.Show();
				Application.DoEvents();

				var doNotStartBeforeHintLabel = (Label)form.Controls.Find("DoNotStartBeforeHintLabel", true).Single();
				AssertEquals("Please nominate the date and time this job should be deferred until:", doNotStartBeforeHintLabel.Text);

				var workflowHintLabel = form.Controls.Find("WorkflowHintLabel", true).Single();
				AssertEquals(false, workflowHintLabel.Visible);

				var workflowLabel = form.Controls.Find("WorkflowLabel", true).Single();
				AssertEquals(false, workflowLabel.Visible);

				var workflowsPanel = form.Controls.Find("WorkflowsPanel", true).Single();
				AssertEquals(true, workflowsPanel.Visible);

				var gridHintLabel = (Label)form.Controls.Find("GridHintLabel", true).Single();
				AssertEquals("Please indicate which workflows will be deferred:", gridHintLabel.Text);

				var workflowsGrid = form.FindAll<ZGrid>().Single();
				AssertEquals(3, workflowsGrid.List.Count);

				var workflows = workflowsGrid.ListManager.List.Cast<WorkflowToDeferBusinessObject>().ToArray();
				AssertContainsExactElementsInAnyOrder(new[] { "Workflow 1", "Workflow 2", "Workflow 3" }, workflows.Select(x => x.ProcessHeader.FH_CompletionStatement));
				AssertContainsExactElementsInAnyOrder(new[] { "Workflow 1", "Workflow 2" }, workflows.Where(x => x.ActionToBeTaken == WorkflowDeferalActionList.Codes.Defer).Select(x => x.ProcessHeader.FH_CompletionStatement));
				AssertContainsExactElementsInAnyOrder(new[] { "Workflow 3" }, workflows.Where(x => x.ActionToBeTaken == WorkflowDeferalActionList.Codes.None).Select(x => x.ProcessHeader.FH_CompletionStatement));

				var dependencyRemovalHintLabel = form.Controls.Find("DependencyRemovalHintLabel", true).Single();
				AssertEquals(false, dependencyRemovalHintLabel.Visible);

				using (var defaultForm = new DeferWorkflowForm())
				{
					var defaultGrid = defaultForm.FindAll<ZGrid>().Single();
					AssertEquals(defaultGrid.Height + dependencyRemovalHintLabel.Height, workflowsGrid.Height);

					var jobHintLabel = form.Controls.Find("JobHintLabel", true).Single();
					var defaultJobHintLabel = defaultForm.Controls.Find("JobHintLabel", true).Single();
					AssertEquals(true, jobHintLabel.Top > defaultJobHintLabel.Top);
				}
			}
		}

		public void TestDeferJob_WithSingleWorkflow_ShouldCustomiseOnlyOneLabel()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var bucket1 = BMSTestHelper.CreateBucket(system, "Bucket 1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "Bucket 2");

			var link1 = bucket1.FromMeToOthersLinks.AddNew();
			link1.FL_FC_ComponentTo = bucket2.PK;

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 1", bucket2);
			var childWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "ChildWorkflow", bucket2);
			childWorkflow.GetOrCreateLinkToParent(workflow1);

			Factory.Save();

			var viewModel = new DeferWorkflowViewModel(jobHeader, new[] { bucket2.PK });
			using (var form = new DeferWorkflowForm(viewModel))
			{
				form.Show();
				Application.DoEvents();

				var doNotStartBeforeHintLabel = (Label)form.Controls.Find("DoNotStartBeforeHintLabel", true).Single();
				AssertEquals("Please nominate the date and time this job should be deferred until:", doNotStartBeforeHintLabel.Text);

				var workflowHintLabel = form.Controls.Find("WorkflowHintLabel", true).Single();
				AssertEquals(true, workflowHintLabel.Visible);

				var workflowLabel = form.Controls.Find("WorkflowLabel", true).Single();
				AssertEquals(true, workflowLabel.Visible);

				var workflowsPanel = form.Controls.Find("WorkflowsPanel", true).Single();
				AssertEquals(false, workflowsPanel.Visible);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var workflow = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory).ProcessHeaders.AddNew();
			Factory.Save();

			return new DeferWorkflowForm(new DeferWorkflowViewModel(workflow, null));
		}
	}
}
