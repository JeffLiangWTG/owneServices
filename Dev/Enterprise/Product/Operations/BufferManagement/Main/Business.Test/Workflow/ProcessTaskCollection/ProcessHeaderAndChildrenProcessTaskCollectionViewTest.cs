using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(ProcessHeaderAndChildrenProcessTaskCollectionView))]
	class ProcessHeaderAndChildrenProcessTaskCollectionViewTest : BusinessObjectCollectionViewTestCase<ProcessHeaderAndChildrenProcessTaskCollectionView>
	{
		protected override ProcessHeaderAndChildrenProcessTaskCollectionView GetCollectionToTest()
		{
			var workflow = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders.AddNew();
			var collection = workflow.TaskCollectionIncludingChildWorkflowTasks;

			AssertType<ProcessHeaderAndChildrenProcessTaskCollectionView>(collection);

			return (ProcessHeaderAndChildrenProcessTaskCollectionView)collection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<ProcessTask>();
		}

		public void TestAddTasksInDeletedWorkflowToNewCollection_ShouldNotThrowException()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var deletedWorkflow = BMSTestHelper.CreateWorkflow(jobHeader1, "deletedWorkflow", autoAssignTasks: true);

			deletedWorkflow.Delete();

			var task = BMSTestHelper.CreateTask(deletedWorkflow, description: "task");
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow", autoAssignTasks: true);
			var collection = new ProcessHeaderAndChildrenProcessTaskCollectionView_ForTest(workflow, false, deletedWorkflow);

			AssertNoExceptionThrown("Checking if a task is part of the collection when the task's workflow has been deleted shouldn't throw exceptions.", () => collection.Add(task));
			AssertEquals("Checking if a task is part of the collection when the task's workflow has been deleted should return false.", 0, collection.Count);
		}

		class ProcessHeaderAndChildrenProcessTaskCollectionView_ForTest : ProcessHeaderAndChildrenProcessTaskCollectionView
		{
			readonly ProcessHeader workflow2;

			public ProcessHeaderAndChildrenProcessTaskCollectionView_ForTest(ProcessHeader workflow1, bool allowCompletionStatements, ProcessHeader workflow2)
				: base(workflow1, allowCompletionStatements)
			{
				this.workflow2 = workflow2;
			}

			protected override ProcessHeader GetProcessHeader(ProcessTask task)
			{
				return task.P9_FH_ProcessHeader == workflow2?.PK ? workflow2 : base.GetProcessHeader(task);
			}
		}
	}
}
