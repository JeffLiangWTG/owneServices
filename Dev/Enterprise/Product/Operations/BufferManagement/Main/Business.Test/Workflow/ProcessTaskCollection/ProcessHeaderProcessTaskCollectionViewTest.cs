using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(ProcessHeaderProcessTaskCollectionView))]
	class ProcessHeaderProcessTaskCollectionViewTest : BusinessObjectCollectionViewTestCase<ProcessHeaderProcessTaskCollectionView>
	{
		public void TestTypeForGridView()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "Org");
			var workflow = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders.AddNew();

			var taskCollection = (ProcessHeaderProcessTaskCollectionView)workflow.TaskCollection;
			AssertEquals(((IUseParentGridContext)taskCollection).ParentType, BusinessObjectCollection.GetElementTypeFromCollectionType(workflow.Parent.WorkflowItems.GetType()));
		}

		public void TestJobHeaderFiltersTasksFromOtherProcessHeaders()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "Org");
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders.AddNew();
			var otherWorkflow = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders.AddNew();

			var task1 = BMSTestHelper.CreateTask(workflow, string.Empty, 30);
			var task2 = BMSTestHelper.CreateTask(workflow, string.Empty, 30);

			AssertEquals(2, jobHeader.TaskCollection.Count);

			task2.P9_FH_ProcessHeader = otherWorkflow.PK;
			task2.P9_ParentID = otherWorkflow.FH_ParentId;

			AssertEquals(1, jobHeader.TaskCollection.Count);
		}

		public void TestJobHeaderAllTasksBelongToHeader()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "Org");
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var task1 = BMSTestHelper.CreateTask(workflow1, string.Empty, 30);
			var task2 = BMSTestHelper.CreateTask(workflow2, string.Empty, 30);

			var unassignedTask = jobHeader.Parent.WorkflowItems.AddNew();

			AssertEquals(3, jobHeader.TaskCollection.Count);
		}

		public void TestIgnoresCompletionStatements()
		{
			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var categorisedTaskType = categorisedTaskTypes.AddNew();
			categorisedTaskType.Code = "ORG";
			var taskType = categorisedTaskType.TaskTypes.AddNew();
			taskType.Code = "COM";
			taskType.IsCompletionStatementTaskType = true;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, categorisedTaskTypes);

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders.AddNew();
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "BEN", "Ben Kenobi");
			var task1 = BMSTestHelper.CreateTask(workflow, "BEN", 7);

			AssertEquals(1, workflow.TaskCollection.Count);
			AssertEquals(1, jobHeader.TaskCollection.Count);

			var task2 = BMSTestHelper.CreateTask(workflow, "BEN", 7);

			AssertEquals(2, workflow.TaskCollection.Count);
			AssertEquals(2, jobHeader.TaskCollection.Count);

			task2.P9_Type = taskType.Code;

			Assert(task2.IsCompletionStatement);
			AssertEquals(1, workflow.TaskCollection.Count);
			AssertEquals(1, jobHeader.TaskCollection.Count);
		}

		public void TestAddToCollectionWithDeletedHeader()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var collection = new ProcessHeaderProcessTaskCollectionView(jobHeader, true);
			jobHeader.Delete();

			var element = GetNewElementToAddToTheCollection();
			collection.Add(element);
			Assert(!collection.Contains(element));
		}

		#region implementation

		protected override ProcessHeaderProcessTaskCollectionView GetCollectionToTest()
		{
			var workflow = ProcessJobHeader.GetForParent(Factory.New<OrgHeader>(), Factory).ProcessHeaders.AddNew();
			return new ProcessHeaderProcessTaskCollectionView(workflow);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			ProcessTask result = Factory.New<ProcessTask>();
			result.P9_Description = "Match";
			return result;
		}

		#endregion
	}
}
