using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(CompletionStatementProcessTaskCollectionView))]
	public class CompletionStatementProcessTaskCollectionViewTest : BusinessObjectCollectionViewTestCase<CompletionStatementProcessTaskCollectionView>
	{
		public void TestNewCompletionStatement_ShouldHaveCompletionStatementTaskType()
		{
			RemoveCOMTaskTypeRegistryEntries();
			AssertNoPreExistingCOMTaskTypes();

			var completionStatement = GetNewTaskToAddToTheCollection();

			AssertEquals("Our new task should have the 'COM' Task type", "COM", completionStatement.P9_Type);
		}

		public void TestNewCompletionStatement_ShouldNotHaveInvalidTaskType()
		{
			RemoveCOMTaskTypeRegistryEntries();
			AssertNoPreExistingCOMTaskTypes();

			var completionStatement = GetNewTaskToAddToTheCollection();

			AssertNoErrors("We should have a valid task type immediately after adding a new task to the collection", completionStatement.P9_TypeInfo);

			completionStatement.Validation.ValidateP9_Type();

			AssertNoErrors("We should have a valid task type", completionStatement.P9_TypeInfo);
		}

		public void TestNewCompletionStatement_WhenOnBufferedWorkflow_ShouldNotComplainAboutEstimates()
		{
			BMSTestHelper.EnableBMSInRegistry();

			var system = Factory.New<BMSystem>();
			var buffer = system.Components.AddNew();
			buffer.FC_Type = BMComponentTypeList.Codes.Buffer;

			var workflowProvider = Factory.New<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(workflowProvider, Factory);
			var workflow = jobHeader.ProcessHeaders.AddNew();
			workflow.FH_FC_CurrentComponent = buffer.PK;

			var completionStatement = GetNewTaskToAddToTheCollectionOfAWorkflow(workflow);
			completionStatement.P9_Description = "oWo";

			CombineAssertions("We have a fresh completion statement, it should have no validation errors or warnings yet", () =>
			{
				AssertNoWarnings(completionStatement);
				AssertNoErrors(completionStatement);
			});
		}

		#region Helper Methods

		public List<CategorisedWorkflowTaskTypes> GetRegistryTaskTypes()
		{
			return WorkflowDataRegistry.Instance.TaskTypes.Value.ToList<CategorisedWorkflowTaskTypes>();
		}

		public void AssertNoPreExistingCOMTaskTypes()
		{
			var existingTaskTypes = GetRegistryTaskTypes();
			var comTaskTypes = existingTaskTypes.Where(type => type.Code == "COM");

			AssertEquals("We should have no pre-existing COM task types, as this is not guaranteed in production code", 0, comTaskTypes.Count());
		}

		public void RemoveCOMTaskTypeRegistryEntries()
		{
			var existingTaskTypes = GetRegistryTaskTypes();
			var antiCOMTaskTypes = existingTaskTypes.Where(type => type.Code != "COM");

			var newTaskTypeCollection = new CategorisedWorkflowTaskTypesCollection();

			foreach (var type in antiCOMTaskTypes)
			{
				newTaskTypeCollection.Add(type);
			}

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newTaskTypeCollection);
		}

		#endregion

		#region Implementation

		protected override CompletionStatementProcessTaskCollectionView GetCollectionToTest()
		{
			return (CompletionStatementProcessTaskCollectionView)Workflow.CompletionStatementTasksIncludingChildWorkflowTasks;
		}

		public ProcessTask GetNewTaskToAddToTheCollection(ProcessHeader processHeader = null)
		{
			return (ProcessTask)GetNewElementToAddToTheCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var task = Workflow.JobHeader.CompletionStatementTasksIncludingChildWorkflowTasks.AddNew();
			AssertEquals("This operation should have set the 'COM' task type", "COM", task.P9_Type);

			return task;
		}

		public ProcessTask GetNewTaskToAddToTheCollectionOfAWorkflow(ProcessHeader processHeader = null)
		{
			var task = processHeader.CompletionStatementTasksIncludingChildWorkflowTasks.AddNew();
			AssertEquals("This operation should have set the 'COM' task type", "COM", task.P9_Type);

			return task;
		}

		protected override void SetUp()
		{
			base.SetUp();

			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var categorisedTaskType = categorisedTaskTypes.AddNew();
			categorisedTaskType.Code = "ORG";
			var taskType = categorisedTaskType.TaskTypes.AddNew();
			taskType.Code = "COM";
			taskType.IsCompletionStatementTaskType = true;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categorisedTaskTypes);
		}

		protected override void TearDown()
		{
			base.TearDown();
			workflow = null;
		}

		ProcessHeader Workflow
		{
			get => workflow ?? (workflow = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders.AddNew());
		}

		ProcessHeader workflow;

		#endregion
	}
}
