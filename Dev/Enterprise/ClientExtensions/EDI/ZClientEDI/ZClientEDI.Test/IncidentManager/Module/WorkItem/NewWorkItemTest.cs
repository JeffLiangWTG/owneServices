using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.Business.Test;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Module.Testing;

public class NewWorkItemTest : TestCaseWithFactory
{
	public void TestFormSave_ShouldNotChangeSelectedTask_TasksTab()
	{
		var workItem = Factory.NewWithValidTestData<NewWorkItem>();
		var task1 = workItem.WorkflowItems.Tasks.AddNew();
		var task2 = workItem.WorkflowItems.Tasks.AddNew();
		var task3 = workItem.WorkflowItems.Tasks.AddNew();
		task1.P9_Description = "Task 1";
		task2.P9_Description = "Task 2";
		task3.P9_Description = "Task 3";

		Factory.Save();

		var controller = ZControllerFactory.Create(ControllerIDs.WorkItem);

		void AssertTaskDoesntMove(string taskDescription, int taskPosition)
		{
			using (var form = (ZForm)controller.ShowEditForm(workItem))
			{
				Application.DoEvents();

				var workflowTabPage = form.FindSingle<ZWorkflowTabPage>();
				((TabControl)workflowTabPage.Parent).SelectTab(workflowTabPage);

				var workflowControl = (ZWorkflowUserControl)form.Controls.Find("ZWorkflowUserControl", true)[0];
				var taskGrid = workflowControl.FindAll<TaskWithDetailsAndFilterTab>().First().FilterControl.TasksGrid;

				taskGrid.ListManager.Position = taskPosition;
				var selectedTask = (ProcessTask)taskGrid.ListManager.GetCurrent();
				AssertEquals(taskDescription, selectedTask.P9_Description);

				selectedTask.P9_Status = "SUS";
				form.FireSaveButton();
				Application.DoEvents();
				AssertEquals("The selection should not have changed when the form was saved. SAD!", taskDescription, ((ProcessTask)taskGrid.ListManager.GetCurrent()).P9_Description);
			}
		}

		AssertTaskDoesntMove("Task 1", 0);
		AssertTaskDoesntMove("Task 2", 1);
		AssertTaskDoesntMove("Task 3", 2);
	}

	public void TestFormSave_ShouldNotChangeSelectedTask_ManagementTab()
	{
		BMSTestHelper.EnableBMSInRegistry();
		BMSTestHelper.CreateSystem(Factory, "WKI");

		var jobHeader = BMSTestHelper.CreateJobHeader<NewWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
		var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow");
		BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, description: "Task 1");
		BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, description: "Task 2");
		BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, description: "Task 3");

		Factory.Save();

		void AssertTaskDoesntMove(string taskDescription, int taskPosition)
		{
			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(workflow))
			{
				Application.DoEvents();

				var taskGrid = form.FindSingle<ZGrid>("TasksGrid");
				taskGrid.ListManager.Position = taskPosition;
				var selectedTask = (ProcessTask)taskGrid.ListManager.GetCurrent();
				AssertEquals(taskDescription, selectedTask.P9_Description);

				selectedTask.P9_Status = "SUS";
				form.FireSaveButton();
				Application.DoEvents();
				AssertEquals("The selection should not have changed when the form was saved. SAD!", taskDescription, ((ProcessTask)taskGrid.ListManager.GetCurrent()).P9_Description);
			}
		}

		AssertTaskDoesntMove("Task 1", 0);
		AssertTaskDoesntMove("Task 2", 1);
		AssertTaskDoesntMove("Task 3", 2);
	}

	public void TestSelectChildWorkflowAndTask_OnFormSave_ShouldKeepThePreviouslySelectedTaskSelected()
	{
		BMSTestHelper.EnableBMSInRegistry();
		var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "WKI");
		var jobHeader = BMSTestHelper.CreateJobHeader<NewWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
		var parentWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow", config.Bucket, releaseGroupPK: config.ReleaseGroup.PK);
		BMSTestHelper.CreateTask(parentWorkflow, GlbStaff.CurrentUser.GS_Code, description: "Parent Task", sequence: 1);

		var childWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow (Quality Iteration 1)", config.Bucket, releaseGroupPK: config.ReleaseGroup.PK);
		var taskInChildWorkflow1 = BMSTestHelper.CreateTask(childWorkflow, GlbStaff.CurrentUser.GS_Code, description: "Child Task 1", sequence: 2);
		var taskInChildWorkflow2 = BMSTestHelper.CreateTask(childWorkflow, GlbStaff.CurrentUser.GS_Code, description: "Child Task 2", sequence: 3);
		BMSTestHelper.CreateParentChildLink(parentWorkflow, childWorkflow);

		Factory.Save();

		AssertEquals(parentWorkflow, childWorkflow.WorkflowParent);

		using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(taskInChildWorkflow2))
		{
			Application.DoEvents();

			var taskGrid = form.FindSingle<ZGrid>("TasksGrid");
			var taskInGrid = (ProcessTask)taskGrid.ListManager.Current;
			AssertEquals(taskInChildWorkflow2.P9_Description, taskInGrid.P9_Description);

			taskInGrid.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			form.FireSaveButton();
			Application.DoEvents();

			AssertEquals("The list should have kept the second task in the child workflow selected.. SAD!", "Child Task 2", ((ProcessTask)taskGrid.ListManager.Current).P9_Description);
		}

		var loadedTask = new BusinessObjectFactory().Load<ProcessTask>(taskInChildWorkflow2.PK);
		AssertEquals("The task should have actually been saved. If it wasn't, then we aren't really testing the main issue that this test is meant to address. SAD!", ProcessTaskStatusCodeList.Codes.Working, loadedTask.P9_Status);
	}

	public static void SetupVersionForBuild(ReleaseBuild build, int major, int minor, int release, int patch)
	{
		build.HL_MajorVersion = major;
		build.HL_MinorVersion = minor;
		build.HL_Release = release;
		build.HL_Patch = patch;
	}
}

[TestedType(typeof(NewWorkItem))]
sealed class NewWorkItemRelatedItemSourceTest : EDIWorkItemRelatedItemSourceTestCase
{
	public new void TestRelatedItems_AttachAndDetach()
	{
		var source = GetNewSourceBusinessObject();
		var incidentSet = new HashSet<string>();
		incidentSet.Add("SupportIncident");
		incidentSet.Add("ProfessionalServicesQuote");

		foreach (var info in source.SupportedRelatedItemModules.Where(x => x.AllowAttach))
		{
			if (incidentSet.Contains(info.ModuleID.Name))
			{
				NewWorkItem workItem = (NewWorkItem)source;
				using (var module = ZFilterModule.GetZFilterModule(info.ModuleID))
				{
					var relatedItem = Factory.New(module.TypeOfTopLevelBusinessObject);
					relatedItem.FillWithValidTestData();
					IWorkTaskRelatedItemSource incident = (IWorkTaskRelatedItemSource)relatedItem;

					AssertNoExceptionThrown(() => incident.RelatedItems.Add(workItem));
					AssertNoExceptionThrown(Factory.Save);
					AssertContainsExactElementsInAnyOrder(new[] { ((IWorkTaskRelatedItem)workItem).Number }, incident.RelatedItems.Cast<IWorkTaskRelatedItem>().Select(x => x.Number));

					AssertNoExceptionThrown(() => incident.RelatedItems.Remove(workItem));
					AssertNoExceptionThrown(Factory.Save);
					AssertContainsExactElementsInAnyOrder(System.Array.Empty<string>(), incident.RelatedItems.Cast<IWorkTaskRelatedItem>().Select(x => x.Number));
				}
				continue;
			}

			using (var module = ZFilterModule.GetZFilterModule(info.ModuleID))
			{
				var relatedItem = Factory.New(module.TypeOfTopLevelBusinessObject);
				relatedItem.FillWithValidTestData();

				AssertNoExceptionThrown(() => source.RelatedItems.Add(relatedItem));
				AssertNoExceptionThrown(Factory.Save);
				AssertContainsExactElementsInAnyOrder(new[] { ((IWorkTaskRelatedItem)relatedItem).Number }, source.RelatedItems.Cast<IWorkTaskRelatedItem>().Select(x => x.Number));

				AssertNoExceptionThrown(() => source.RelatedItems.Remove(relatedItem));
				AssertNoExceptionThrown(Factory.Save);
				AssertContainsExactElementsInAnyOrder(System.Array.Empty<string>(), source.RelatedItems.Cast<IWorkTaskRelatedItem>().Select(x => x.Number));
			}
		}

		Assert(true); // If none of the modules support attaching
	}
}
