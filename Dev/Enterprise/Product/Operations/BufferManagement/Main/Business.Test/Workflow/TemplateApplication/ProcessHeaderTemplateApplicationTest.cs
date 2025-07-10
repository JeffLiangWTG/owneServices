using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	class ProcessHeaderTemplateApplicationTest : TestCaseWithFactory
	{
		public void TestDeletedHeaderDoesNotReapply()
		{
			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.CreateSystem(Factory, "DUM");
			Factory.Save();
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "DUM");
			var workflow1 = BMSTestHelper.CreateWorkflow(template, "Alpha");
			var workflow2 = BMSTestHelper.CreateWorkflow(template, "Beta");

			var task1 = BMSTestHelper.CreateTask(template, workflow1, description: "See the light");
			var task2 = BMSTestHelper.CreateTask(template, workflow2, description: "It burns eyes");

			Factory.Save();
			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();

			var dummyJobHeader = ProcessJobHeader.GetForParent(dummy, Factory, false);
			AssertEquals("Should have both headers created", 2, dummyJobHeader.ProcessHeaders.Count);

			var dummyWorkflow = dummyJobHeader.ProcessHeaders.Single(p => p.FH_CompletionStatement == "Alpha");
			dummyWorkflow.Delete();

			dummy.ApplyWorkflowTemplates();
			AssertEquals("Don't re-add the deleted header.", 1, dummyJobHeader.ProcessHeaders.Count);
		}

		public void TestReapplyProcessHeadersOptionToDuplicateProcessHeaders()
		{
			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.CreateSystem(Factory, "DUM");
			Factory.Save();
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "DUM");
			var workflow1Name = "Alpha";
			var workflow2Name = "Beta";
			var workflow1 = BMSTestHelper.CreateWorkflow(template, workflow1Name);
			var workflow2 = BMSTestHelper.CreateWorkflow(template, workflow2Name);
			var task1Name = "Task1";
			var task2Name = "Task2";
			var task1 = BMSTestHelper.CreateTask(template, workflow1, description: task1Name);
			var task2 = BMSTestHelper.CreateTask(template, workflow2, description: task2Name);
			var trigger1 = template.WorkflowItems.Triggers.AddNew();
			trigger1.P9_Description = "Trigger";
			trigger1.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
			var milestone1 = template.WorkflowItems.Milestones.AddNew();
			milestone1.P9_Description = "Milestone";
			milestone1.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;

			Factory.Save();
			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();

			var dummyJobHeader = ProcessJobHeader.GetForParent(dummy, Factory, false);
			AssertEquals("Should have both headers created", 2, dummyJobHeader.ProcessHeaders.Count);
			AssertEquals("Milestone should be added from tempalte", 1, dummy.WorkflowItems.Milestones.Count);
			AssertEquals("Trigger should be added from tempalte", 1, dummy.WorkflowItems.Triggers.Count);
			Factory.Save();

			var parameters = TemplateApplicationParameters.ReapplyTemaplate();
			dummy.ApplyWorkflowTemplates(parameters);

			dummyJobHeader = ProcessJobHeader.GetForParent(dummy, Factory, false);
			AssertEquals("Should have both headers added again", 4, dummyJobHeader.ProcessHeaders.Count);
			AssertEquals("Should have both tasks applied again", 4, dummy.WorkflowItems.Tasks.Count);
			AssertEquals("Only tasks and workflows should be reapplied (for this iteration of this feature anyway)", 1, dummy.WorkflowItems.Milestones.Count);
			AssertEquals("Only tasks and workflows should be reapplied (for this iteration of this feature anyway)", 1, dummy.WorkflowItems.Triggers.Count);

			void AssertProcessHeaderHasCorrectTaskAndSequence(string processHeaderCompletionDescription, string taskDescription, int taskSequence)
			{
				var processHeader = dummyJobHeader.ProcessHeaders.Single(x => x.FH_CompletionStatement == processHeaderCompletionDescription);
				CombineAssertions($"'{processHeaderCompletionDescription}' workflow should have 1 task with description '{taskDescription}' and sequence '{taskSequence}'", () =>
				{
					AssertNotNull("Workflow exists", processHeader);
					AssertEquals("Workflow has one task", 1, processHeader.Tasks.Count());
					var task = processHeader.Tasks.Single();
					AssertEquals("Task Description", taskDescription, task.P9_Description);
					AssertEquals("Task Sequence", taskSequence, task.P9_Sequence);
				});
			}

			AssertProcessHeaderHasCorrectTaskAndSequence(workflow1Name, task1Name, 1);
			AssertProcessHeaderHasCorrectTaskAndSequence(workflow2Name, task2Name, 2);
			AssertProcessHeaderHasCorrectTaskAndSequence(workflow1Name + " (1)", task1Name, 103);
			AssertProcessHeaderHasCorrectTaskAndSequence(workflow2Name + " (1)", task2Name, 104);
		}

		[TestDate(2000, 1, 1)]
		public void TestReapplyProcessHeadersOptionToDuplicateProcessHeaders_Dependancies()
		{
			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.CreateSystem(Factory, "DUM");
			Factory.Save();
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "DUM");
			var workflow1 = BMSTestHelper.CreateWorkflow(template, "Workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(template, "Workflow2");
			var task1 = BMSTestHelper.CreateTask(template, workflow1, description: "Task1");
			var task2 = BMSTestHelper.CreateTask(template, workflow2, description: "Task2");
			BMSTestHelper.CreateDependencyLink(template, workflow1, workflow2);

			Factory.Save();
			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();

			var dummyJobHeader = ProcessJobHeader.GetForParent(dummy, Factory, false);
			Factory.Save();

			TestDateAttribute.AddMilliseconds(1);//dependancies when reapplying templates are added to the newest matching workflow
			var parameters = TemplateApplicationParameters.ReapplyTemaplate();
			dummy.ApplyWorkflowTemplates(parameters);
			AssertEquals("Should have both headers created twice", 4, dummyJobHeader.ProcessHeaders.Count);
			dummyJobHeader = ProcessJobHeader.GetForParent(dummy, Factory, false);

			var header1 = dummyJobHeader.ProcessHeaders.First(x => x.FH_CompletionStatement == "Workflow1");
			var header2 = dummyJobHeader.ProcessHeaders.First(x => x.FH_CompletionStatement == "Workflow2");
			var header12 = dummyJobHeader.ProcessHeaders.First(x => x.FH_CompletionStatement == "Workflow1 (1)");
			var header22 = dummyJobHeader.ProcessHeaders.First(x => x.FH_CompletionStatement == "Workflow2 (1)");

			AssertEquals(1, header1.LinksFromMeToOthers.Count());
			AssertNotNull("This dependancy is added during first application - Workflow1 -> Workflow2", header1.LinksFromMeToOthers.FirstOrDefault(x => x.FP_FH_HeaderTo == header2.PK));
			AssertEquals(1, header12.LinksFromMeToOthers.Count());
			AssertNotNull("This dependancy is added the template reapply - Workflow1 (1) -> Workflow2 (1)", header12.LinksFromMeToOthers.FirstOrDefault(x => x.FP_FH_HeaderTo == header22.PK));
		}

		public void TestPartiaTemplateMatchOnOriginalWorkflowNameFromTemplate()
		{
			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.CreateSystem(Factory, "DUM");
			Factory.Save();
			var partialTemplate = BMSTestHelper.CreateWorkflowTemplate(Factory, "DUM");
			partialTemplate.P0_IsPartialTemplate = true;
			var partialTempalteWorkflow = BMSTestHelper.CreateWorkflow(partialTemplate, "CODING");
			BMSTestHelper.CreateTask(partialTemplate, partialTempalteWorkflow, description: "Partial Template Task");

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "DUM");
			var workflow = BMSTestHelper.CreateWorkflow(template, "CODING");
			var task = BMSTestHelper.CreateTask(template, workflow, description: "Main Tempalte Task");
			var trigger = template.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Apply partial template";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce;
			action.PQ_P0_WorkflowTemplate = partialTemplate.PK;
			Factory.Save();

			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();

			var dummyJobHeader = ProcessJobHeader.GetForParent(dummy, Factory, false);
			AssertEquals("Header is created from template", 1, dummyJobHeader.ProcessHeaders.Count);
			AssertEquals("Task is created from template", 1, dummy.WorkflowItems.Tasks.Count);

			Factory.Save();

			var jobWorkflow = dummyJobHeader.ProcessHeaders.Single();
			var lastWorkflow = jobWorkflow.CloneWorkflow();
			lastWorkflow = lastWorkflow.CloneWorkflow();
			AssertEquals("Precondition: This requires all workflows came from a template workflow with the same name as partial template workflow", jobWorkflow.FH_ParentTemplateId, lastWorkflow.FH_ParentTemplateId);
			AssertEquals("Precondition: The latest workflow is the third 'CODING' workflow but the name is now different", "CODING (2)", lastWorkflow.FH_CompletionStatement);

			dummy.Logs.AddNew(Events.CustomisableEvent00);
			Factory.Save();

			AssertEquals("1 task per workflow", 3, dummy.WorkflowItems.Tasks.Count);
			MasterFilesTestHelper.RunLogWalker();

			dummy.WorkflowItems.Reload(true);
			AssertEquals("Extra Task is created from partial template", 4, dummy.WorkflowItems.Tasks.Count);
			AssertEquals(2, lastWorkflow.TaskCollection.Count);
		}

		public void TestReapplyProcessHeadersTwiceWithoutSavingErrorReports()
		{
			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.CreateSystem(Factory, "DUM");
			Factory.Save();

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "DUM");
			var workflow = BMSTestHelper.CreateWorkflow(template, "CODING");
			BMSTestHelper.CreateTask(template, workflow, description: "Main Tempalte Task");
			Factory.Save();

			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();

			var dummyJobHeader = ProcessJobHeader.GetForParent(dummy, Factory, false);
			AssertEquals("Header is created from template", 1, dummyJobHeader.ProcessHeaders.Count);
			AssertEquals("Task is created from template", 1, dummy.WorkflowItems.Tasks.Count);

			var parameters = TemplateApplicationParameters.ReapplyTemaplate();
			dummy.ApplyWorkflowTemplates(parameters);
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();

			Factory.Save();

			dummy.ApplyWorkflowTemplates(parameters);
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}
	}
}
