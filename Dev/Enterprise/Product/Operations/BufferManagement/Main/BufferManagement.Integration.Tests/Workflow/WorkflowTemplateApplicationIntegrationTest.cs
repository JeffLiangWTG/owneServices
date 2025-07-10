using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.GUI;
using Enterprise.ProcessManagement.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Integration.Tests
{
	class WorkflowTemplateApplicationIntegrationTest : BMSTestCaseWithFactory
	{
		public void TestTasksAndMilestonesLoaderDoesNotApplyTemplatesForProcessHeaderEvents()
		{
			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.CreateSystem(Factory, "WKI");
			Factory.Save();
			var job = Factory.New<WorkItem>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow = (ProcessHeader)job.Workflows.AddNew();
			workflow.FH_CompletionStatement = "Noodly";
			workflow.Logs.AddNew(new EventValue(Events.WorkflowTransferredBetweenSystemComponents, deferFiringWorkflow: true));
			var milestone = job.WorkflowItems.Milestones.AddNew(); // Matching milestone is important to proc HasChanges on the Work Item.
			milestone.P9_Description = "I am mil";
			milestone.TriggerConditions.TriggerEventCode = Events.WorkflowTransferredBetweenSystemComponentsCode;
			Factory.Save();

			var templateFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var template1 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, "WKI", name: "Template A");
			var trigger = template1.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "I am trig";
			templateFactory.Save();

			BMSTestHelper.RunLogWalker();

			job.WorkflowItems.Reload(true);
			AssertEquals("Template application should not have occurred in the LWK service task", 0, job.WorkflowItems.Triggers.Count);
		}

		[GuiTest]
		[RequiresSTA]
		public void TestConcurrencyUseOfJobWithoutTemplateApplied_ShouldNotCreateDuplicateJobLevelWorkflow()
		{
			var job = Factory.New<WorkItem>();
			job.WKI_Summary = "Edge case";
			Factory.Save();

			var workflows = Factory.Load<ProcessHeader>(new ZQuery(ProcessHeaderSchema.FH_ParentId, job.PK));
			AssertContainsExactElementsInAnyOrder(@"Buffer Management isn't enabled and there's no template so no workflows should have been created.
This is to recreate a state that we believe customers are facing where their job starts in the database without a template applied and then gets applied later by users.",
				Array.Empty<string>(), workflows.Select(x => x.FH_CompletionStatement));

			var templateFactory = new BusinessObjectFactory { RefreshEnabled = false };
			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.CreateSystem(templateFactory, "WKI");

			var template1 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, "WKI", name: "Template A");
			var template1Workflow = BMSTestHelper.CreateWorkflow(template1, "Workflow 1");
			template1Workflow.JobHeader.FH_CompletionStatement = "Job 1";
			BMSTestHelper.CreateTask(template1, template1Workflow);

			var template2 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, "WKI", name: "Template B", subType1: "INQ");
			var template2Workflow = BMSTestHelper.CreateWorkflow(template2, "Workflow 2");
			template2Workflow.JobHeader.FH_CompletionStatement = "Job 2";
			BMSTestHelper.CreateTask(template2, template2Workflow);

			templateFactory.Save();

			var factory1 = new BusinessObjectFactory { RefreshEnabled = false };
			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };

			var job1 = factory1.Load<WorkItem>(job.PK);
			var job2 = factory2.Load<WorkItem>(job.PK);

			using (var form1 = new WorkItemForm(job1))
			using (var form2 = new WorkItemForm(job2))
			{
				form1.Show();
				Application.DoEvents();

				form2.Show();

				WaitUntilTabsAreBoundToDataSourcesOnIdle(); // Will trigger ZWorkflowTabPage.WorkflowItems_ListChanged_OnIdle, which ends up trying to load the JLW which doesn't exist yet, but the query gets cached so later it won't know that one has been created.

				AssertEquals("You are currently modifying WI00000001 - Edge case in another form. You may be able to save modifications from only one form.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();

				var form1Job = form1.BusinessEntity;
				var form2Job = form2.BusinessEntity;

				AssertNotEquals("We need the same job loaded in different factories for this concurrency problem to be reproduced.", form1Job.Factory._Instance, form2Job.Factory._Instance);

				((IWorkItem)form1Job).WKI_Summary = "Changed";
				Application.DoEvents();
				AssertContainsExactElementsInAnyOrder("There should be no validation errors blocking save.", Array.Empty<string>(), ((BusinessObject)form1Job).GetErrors().Select(x => x.Message));
				AssertEquals("If there are no changes then there will be nothing to save.", true, form1Job.HasChanges);

				((IBusinessObjectFactoryInternals)factory2).DisableQueryCacheReset = true; // Otherwise it will clear factory2's cache, which means this defect won't be reproduced.

				AssertEquals("The form should have saved.", ContinueWithSave.Yes, form1.FireSaveButton());
				Application.DoEvents();

				((IBusinessObjectFactoryInternals)factory2).DisableQueryCacheReset = false;

				var workflows1 = factory1.Load<ProcessHeader>(new ZQuery(ProcessHeaderSchema.FH_ParentId, job.PK));
				AssertContainsExactElementsInAnyOrder("The first template should have been applied", new[] { "Job 1", "Workflow 1" }, workflows1.Select(x => x.FH_CompletionStatement));

				((IWorkItem)form2Job).WKI_Summary = "Changed";
				Application.DoEvents();
				AssertContainsExactElementsInAnyOrder("There should be no validation errors blocking save.", Array.Empty<string>(), ((BusinessObject)form2Job).GetErrors().Select(x => x.Message));
				AssertEquals("If there are no changes then there will be nothing to save.", true, form2Job.HasChanges);

				var didForm2Save = form2.FireSaveButton();
				Application.DoEvents();
				AssertContains("ReportInformationMessage", "The following objects have changes and will be merged:", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				var didForm2SaveAgain = form2.FireSaveButton();
				AssertNull("No message about duplicate job-level workflows should be shown.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("The form should have saved.", ContinueWithSave.Yes, didForm2SaveAgain);

				var workflows2 = factory2.Load<ProcessHeader>(new ZQuery(ProcessHeaderSchema.FH_ParentId, job.PK));
				AssertContainsExactElementsInAnyOrder("The first template should remain and the second one deleted because a template was already applied on save.", new[] { "Job 1", "Workflow 1" }, workflows2.Select(x => x.FH_CompletionStatement));
			}
		}

		void WaitUntilTabsAreBoundToDataSourcesOnIdle()
		{
			Application.DoEvents();

			var timeElapsed = 0;
			const int timeIncrement = 100;
			const int timeout = 5000;

			while (timeElapsed < timeout && UserIdleWorker.QueuedWorkItemCount > 0)
			{
				Thread.Sleep(timeIncrement); // to trigger OnIdle
				timeElapsed += timeIncrement;
			}
		}
	}
}
