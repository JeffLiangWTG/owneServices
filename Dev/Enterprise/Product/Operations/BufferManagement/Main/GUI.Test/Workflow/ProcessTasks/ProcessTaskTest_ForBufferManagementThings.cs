using System;
using System.Linq;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	class ProcessTaskTest_ForBufferManagementThings : BMSTestCaseWithFactory
	{
		#region Startable Task Filter

		[GuiTest]
		public void TestCurrentTaskFilter_ShouldShowAllCurrentTasksInOpenWorkflows()
		{
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("UDF", "INQ");

			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var jobWorkflow = BMSTestHelper.CreateJobHeader(enquiry, addDefaultProcessHeaderIfNone: false);
			var openWorkflow1 = BMSTestHelper.CreateWorkflow(jobWorkflow, "Open workflow 1");
			var openWorkflow2 = BMSTestHelper.CreateWorkflow(jobWorkflow, "Open workflow 2");
			var blockedWorkflow = BMSTestHelper.CreateWorkflow(jobWorkflow, "Blocked workflow");
			var closedWorkflow = BMSTestHelper.CreateWorkflow(jobWorkflow, "Closed workflow");

			openWorkflow2.GetOrCreateDependencyLink(blockedWorkflow);

			var task1 = BMSTestHelper.CreateTask(openWorkflow1, sequence: 9, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, description: "Open workflow 1, first task");
			var task2 = BMSTestHelper.CreateTask(openWorkflow1, sequence: 10, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, description: "Open workflow 1, second task");
			var task3 = BMSTestHelper.CreateTask(openWorkflow1, sequence: 11, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, description: "Open workflow 1, third task");
			BMSTestHelper.CreateTask(openWorkflow2, sequence: 12, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, description: "Open workflow 2, first task");
			BMSTestHelper.CreateTask(openWorkflow2, sequence: 13, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, description: "Open workflow 2, second task");
			var taskToChangeSequenceLater = BMSTestHelper.CreateTask(blockedWorkflow, sequence: 5, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, description: "Blocked workflow, first task");
			BMSTestHelper.CreateTask(blockedWorkflow, sequence: 6, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, description: "Blocked workflow, second task");
			BMSTestHelper.CreateTask(closedWorkflow, sequence: 1, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, description: "Closed workflow, first task");
			BMSTestHelper.CreateTask(closedWorkflow, sequence: 2, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, description: "Closed workflow, second task");

			Factory.Save();

			AssertEquals(WorkflowStatusList.Codes.Open, openWorkflow1.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Open, openWorkflow2.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Blocked, blockedWorkflow.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Closed, closedWorkflow.FH_Status);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.ProcessTasks))
			{
				var filterBizo = module.FilterBusinessObject;
				var strip = filterBizo.FilterStrips.AddNew("Current Task Only");
				var filter = (ModuleFlagsFilter)strip.CurrentModuleFilter;
				filter.Property0 = true;

				strip = filterBizo.FilterStrips.AddNew("Parent Job");
				var parentJobFilter = (ModuleGuidModuleSpecifiedFilter)strip.CurrentModuleFilter;
				parentJobFilter.SelectedModule = ModuleIDs.SalesEnquiry.Name;
				parentJobFilter.Property = enquiry.PK;

				var query = filterBizo.Filter;
				var result = Factory.Load<ProcessTask>(query);

				AssertContainsExactElementsInAnyOrder(query.LiteralTextSqlFormatted, new[] { "Open workflow 1, first task", "Open workflow 2, first task" }, result.Select(x => x.P9_Description));

				taskToChangeSequenceLater.P9_Sequence = 9; // Same as an actual current task in a different workflow.
				Factory.Save();

				result = Factory.Load<ProcessTask>(query);
				AssertContainsExactElementsInAnyOrder(query.LiteralTextSqlFormatted, new[] { "Open workflow 1, first task", "Open workflow 2, first task" }, result.Select(x => x.P9_Description));

				task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				var iterationWorkflow = CreateQualityIteration(task1, task2);
				var iterationTask = iterationWorkflow.Tasks.Single(x => x.P9_Description == "Open workflow 1, first task");
				iterationTask.P9_Description = "Task in QI";
				Factory.Save();

				result = Factory.Load<ProcessTask>(query);
				AssertContainsExactElementsInAnyOrder("The QI task should be current and not tasks in the parent workflow except the task the comes before the task that created the QI, and yet...", new[] { "Task in QI", "Open workflow 1, first task", "Open workflow 2, first task" }, result.Select(x => x.P9_Description));

				task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				Factory.Save();

				result = Factory.Load<ProcessTask>(query);
				AssertContainsExactElementsInAnyOrder("The remaining task in workflow 1 should still not be current because the open QI comes before it in sequence, and yet...", new[] { "Task in QI", "Open workflow 2, first task" }, result.Select(x => x.P9_Description));

				iterationTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				iterationWorkflow.Tasks.Single(x => x != iterationTask).P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				Factory.Save();

				result = Factory.Load<ProcessTask>(query);
				AssertContainsExactElementsInAnyOrder("The remaining task in workflow 1 should be current because the QI, which comes before it in sequence, is now closed, and yet...", new[] { "Open workflow 1, third task", "Open workflow 2, first task" }, result.Select(x => x.P9_Description));
			}
		}

		[GuiTest]
		public void TestCurrentTaskFilter_WhenIgnoringIterations_ShouldShowAllCurrentTasksInOpenWorkflows()
		{
			BMSRegistry.Instance.IgnoreIterationsWhenCalculatingStartability.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("UDF", "INQ");

			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var jobWorkflow = BMSTestHelper.CreateJobHeader(enquiry, addDefaultProcessHeaderIfNone: false);
			var openWorkflow1 = BMSTestHelper.CreateWorkflow(jobWorkflow, "Open workflow 1");
			var openWorkflow2 = BMSTestHelper.CreateWorkflow(jobWorkflow, "Open workflow 2");
			var blockedWorkflow = BMSTestHelper.CreateWorkflow(jobWorkflow, "Blocked workflow");
			var closedWorkflow = BMSTestHelper.CreateWorkflow(jobWorkflow, "Closed workflow");

			openWorkflow2.GetOrCreateDependencyLink(blockedWorkflow);

			var task1 = BMSTestHelper.CreateTask(openWorkflow1, sequence: 9, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, description: "Open workflow 1, first task");
			var task2 = BMSTestHelper.CreateTask(openWorkflow1, sequence: 10, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, description: "Open workflow 1, second task");
			var task3 = BMSTestHelper.CreateTask(openWorkflow1, sequence: 11, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, description: "Open workflow 1, third task");
			BMSTestHelper.CreateTask(openWorkflow2, sequence: 12, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, description: "Open workflow 2, first task");
			BMSTestHelper.CreateTask(openWorkflow2, sequence: 13, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, description: "Open workflow 2, second task");
			var taskToChangeSequenceLater = BMSTestHelper.CreateTask(blockedWorkflow, sequence: 5, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, description: "Blocked workflow, first task");
			BMSTestHelper.CreateTask(blockedWorkflow, sequence: 6, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, description: "Blocked workflow, second task");
			BMSTestHelper.CreateTask(closedWorkflow, sequence: 1, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, description: "Closed workflow, first task");
			BMSTestHelper.CreateTask(closedWorkflow, sequence: 2, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, description: "Closed workflow, second task");

			Factory.Save();

			AssertEquals(WorkflowStatusList.Codes.Open, openWorkflow1.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Open, openWorkflow2.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Blocked, blockedWorkflow.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Closed, closedWorkflow.FH_Status);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.ProcessTasks))
			{
				var filterBizo = module.FilterBusinessObject;
				var strip = filterBizo.FilterStrips.AddNew("Current Task Only");
				var filter = (ModuleFlagsFilter)strip.CurrentModuleFilter;
				filter.Property0 = true;

				strip = filterBizo.FilterStrips.AddNew("Parent Job");
				var parentJobFilter = (ModuleGuidModuleSpecifiedFilter)strip.CurrentModuleFilter;
				parentJobFilter.SelectedModule = ModuleIDs.SalesEnquiry.Name;
				parentJobFilter.Property = enquiry.PK;

				var query = filterBizo.Filter;
				var result = Factory.Load<ProcessTask>(query);

				AssertContainsExactElementsInAnyOrder(query.LiteralTextSqlFormatted, new[] { "Open workflow 1, first task", "Open workflow 2, first task" }, result.Select(x => x.P9_Description));

				taskToChangeSequenceLater.P9_Sequence = 9; // Same as an actual current task in a different workflow.
				Factory.Save();

				result = Factory.Load<ProcessTask>(query);
				AssertContainsExactElementsInAnyOrder(query.LiteralTextSqlFormatted, new[] { "Open workflow 1, first task", "Open workflow 2, first task" }, result.Select(x => x.P9_Description));

				task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				var iterationWorkflow = CreateQualityIteration(task1, task2);
				var iterationTask = iterationWorkflow.Tasks.Single(x => x.P9_Description == "Open workflow 1, first task");
				iterationTask.P9_Description = "Task in QI";
				Factory.Save();

				result = Factory.Load<ProcessTask>(query);
				AssertContainsExactElementsInAnyOrder(new[] { "Task in QI", "Open workflow 1, first task", "Open workflow 2, first task" }, result.Select(x => x.P9_Description));

				task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				Factory.Save();

				result = Factory.Load<ProcessTask>(query);
				AssertContainsExactElementsInAnyOrder(new[] { "Task in QI", "Open workflow 2, first task", "Open workflow 1, third task" }, result.Select(x => x.P9_Description));

				iterationTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				iterationWorkflow.Tasks.Single(x => x != iterationTask).P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				Factory.Save();

				result = Factory.Load<ProcessTask>(query);
				AssertContainsExactElementsInAnyOrder(new[] { "Open workflow 1, third task", "Open workflow 2, first task" }, result.Select(x => x.P9_Description));
			}
		}

		[GuiTest] // Because the Containment Barrier form will be shown
		public void TestStartableTaskOnlyFilter_WhenQualityIterationExistsWithoutWorkflow_AndAllIterationTasksAreComplete_ShouldMatchLaterTasks()
		{
			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.CreateSystem(Factory, "DUM");
			MasterFilesTestHelper.AddTaskTypesToRegistry("DUM", "INV", "CBC");
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("CBC", "DUM");

			Factory.Save();

			var workflow = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "Workflow");
			var task1 = BMSTestHelper.CreateTask(workflow, description: "Task 1", taskType: "INV", sequence: 1);
			var task2 = BMSTestHelper.CreateTask(workflow, description: "Task 2", taskType: "CBC", sequence: 2);
			var task3 = BMSTestHelper.CreateTask(workflow, description: "Task 3", taskType: "INV", sequence: 3);

			Factory.Save();

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			BMSTestHelper.CreateQualityIteration(task1, task2, shouldCreateWorkflowForIteration: false);

			var iterationTask1 = workflow.Tasks.Single(x => x.P9_Description == "Task 1" && x.PK != task1.PK);
			var iterationTask2 = workflow.Tasks.Single(x => x.P9_Description == "Task 2" && x.PK != task2.PK);

			AssertEquals(3, iterationTask1.P9_Sequence);
			AssertEquals(4, iterationTask2.P9_Sequence);
			AssertEquals("Precondition: the new iteration tasks should not be closed yet.", ProcessTaskStatusCodeList.Codes.Assigned, iterationTask1.P9_Status);
			AssertEquals("Precondition: the new iteration tasks should not be closed yet.", ProcessTaskStatusCodeList.Codes.Assigned, iterationTask2.P9_Status);
			AssertEquals("Precondition: the remaining task should still not be closed.", ProcessTaskStatusCodeList.Codes.Assigned, task3.P9_Status);
			AssertEquals("Precondition: the remaining task should have had its sequence number bumped.", 5, task3.P9_Sequence);

			// Let's make it easier to tell the difference between these tasks.
			iterationTask1.P9_Description += " (iteration)";
			iterationTask2.P9_Description += " (iteration)";

			Factory.Save();

			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessTasks))
			{
				var filterBizo = module.FilterBusinessObject;
				var filter = filterBizo.AddFilterStrip<ModuleFlagsFilter>("Current Task Only");
				filter.Property0 = true;

				var query = filterBizo.Filter;
				var results = Factory.Load<ProcessTask>(query);
				AssertContainsExactElementsInAnyOrder("The iteration tasks are still open, so the additional non-iteration task shouldn't be startable.",
					new[] { "Task 1 (iteration)" }, results.Select(x => x.P9_Description));

				iterationTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				iterationTask2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				Factory.Save();

				results = Factory.Load<ProcessTask>(query);
				AssertContainsExactElementsInAnyOrder("The iteration tasks are now, so the additional non-iteration task should be startable. This should happen even if the iteration didn't create a new workflow.",
					new[] { "Task 3" }, results.Select(x => x.P9_Description));
			}
		}

		#endregion
	}
}
