using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.VisualBoards.Business.Test;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	class FactorylessWorkflowCardContentLoaderTest : CardBusinessObjectLoaderTest
	{
		public void TestTaskDeleted_FallbackToAlternativeTask()
		{
			var alternativeTask = CreateTask(Workflow, Task.P9_GS_NKAssignedStaffMember, 20);
			Task.Delete();
			Factory.Save();

			var result = CardBusinessObjectLoader.Load(CardContent, ViewModel);

			Assert(result.LoadSucceeded);
			AssertEquals(alternativeTask.PK, result.Task.PK);
		}

		public override void TestFailure_ReassignTask()
		{
			var newWorkflow = CreateWorkflow(JobHeader, "OtherWorkflow");
			Task.P9_FH_ProcessHeader = newWorkflow.PK;
			Factory.Save();

			var result = CardBusinessObjectLoader.Load(CardContent, ViewModel);
			AssertLoadFailed(ExpectedDeleteTaskMessage, result);
		}

		public void TestReassignTask_FallbackToAlternativeTask()
		{
			var newWorkflow = CreateWorkflow(JobHeader, "OtherWorkflow");
			Task.P9_FH_ProcessHeader = newWorkflow.PK;
			var alternativeTask = CreateTask(Workflow, Task.P9_GS_NKAssignedStaffMember, 20);

			Factory.Save();

			var result = CardBusinessObjectLoader.Load(CardContent, ViewModel);
			Assert(result.LoadSucceeded);
			AssertEquals(alternativeTask.PK, result.Task.PK);
		}

		protected override ZString ExpectedDeleteTaskMessage
		{
			get { return "This workflow no longer has tasks. Workflows without tasks cannot be displayed."; }
		}

		protected override ICardContent GetCardContent()
		{
			return new FactorylessCardContent(Workflow, Task, ViewModel, new CustomisedControlDataCache(), new TagDefinitionCache(Factory), new PopulateWorkflowCardStrategy());
		}
	}

	class FactorylessTaskCardContentLoaderTest : CardBusinessObjectLoaderTest
	{
		protected override ICardContent GetCardContent()
		{
			return new FactorylessCardContent(Workflow, Task, ViewModel, new CustomisedControlDataCache(), new TagDefinitionCache(Factory), new PopulateTaskCardStrategy());
		}
	}

	class TaskCardContentLoaderTest : CardBusinessObjectLoaderTest
	{
		protected override ICardContent GetCardContent()
		{
			return new TaskCardContent(Task, ViewModel);
		}
	}

	abstract class CardBusinessObjectLoaderTest : BMSTestCaseWithFactory
	{
		public void TestSuccess()
		{
			var result = CardBusinessObjectLoader.Load(CardContent, ViewModel);

			Assert(result.LoadSucceeded);
			AssertNotEquals(Factory, result.Task.Factory);
			AssertEquals(Workflow.PK, result.Workflow.PK);
			AssertEquals(Task.PK, result.Task.PK);
		}

		[TestDate(2021, 9, 20, 4, 30, 00)]
		public void TestSuccessWithDifferentTaskContext()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "BR1";
			branch.GB_RL_NKHomePort = "CNSHA"; // China/Shanghai - difference with UTC +8:00
			ViewModel.Section.Board.MB_GB_AgingBranch = branch.PK;

			var department = Factory.NewWithValidTestData<GlbDepartment>();
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, department.PK);
			ViewModel.Section.Board.MB_GE_AgingDepartment = department.PK;

			Factory.Save();

			TestDateAttribute.AddHours(30);
			var result = CardBusinessObjectLoader.Load(CardContent, ViewModel);

			Assert(result.LoadSucceeded);
			AssertNotEquals(Factory, result.Task.Factory);
			AssertEquals(Workflow.PK, result.Workflow.PK);
			AssertEquals(Task.PK, result.Task.PK);
			AssertEquals(new TimeSpan(12, 30, 0), result.Task.WorkingTimeSinceBecomingStartable);
		}

		public void TestFailure_DeleteTask()
		{
			Task.Delete();
			Factory.Save();

			var result = CardBusinessObjectLoader.Load(CardContent, ViewModel);
			AssertLoadFailed(ExpectedDeleteTaskMessage, result);
		}

		protected virtual ZString ExpectedDeleteTaskMessage
		{
			get { return "This card's task has been deleted and will now be removed."; }
		}

		public void TestFailure_DeleteTaskAndWorkflow()
		{
			Workflow.Delete(); // Task is implicitly deleted
			Factory.Save();

			var result = CardBusinessObjectLoader.Load(CardContent, ViewModel);
			AssertLoadFailed("This card's workflow has been deleted and will now be removed.", result);
		}

		public virtual void TestFailure_ReassignTask()
		{
			var newWorkflow = CreateWorkflow(JobHeader, "OtherWorkflow");
			Task.P9_FH_ProcessHeader = newWorkflow.PK;
			Factory.Save();

			var result = CardBusinessObjectLoader.Load(CardContent, ViewModel);
			AssertLoadFailed("This card's task has been assigned to a different workflow and will now be removed.", result);
		}

		public void TestFailure_ReassignTaskAndDeleteWorkflow()
		{
			var newWorkflow = CreateWorkflow(JobHeader, "OtherWorkflow");
			Task.P9_FH_ProcessHeader = newWorkflow.PK;

			Workflow.Delete();
			Factory.Save();

			var result = CardBusinessObjectLoader.Load(CardContent, ViewModel);
			AssertLoadFailed("This card's workflow has been deleted and will now be removed.", result);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			var staff = CreateStaffInCurrentBranchDept("ABA", "ABBA is a good band");

			var system = CreateSystem("ORG");
			var component = CreateBuffer(system);
			var section = CreateBoardSection(component);

			JobHeader = CreateJobHeader<OrgHeader>(false);
			Workflow = CreateWorkflow(JobHeader, "Workflow");
			Task = CreateTask(Workflow, staff.GS_Code, 60);

			Factory.Save();

			ViewModel = VisualBoardsTestHelper.CreateViewModel(section);

			CardContent = GetCardContent();
		}

		protected abstract ICardContent GetCardContent();

		protected ProcessHeader Workflow { get; set; }
		protected ProcessJobHeader JobHeader { get; set; }
		protected ProcessTask Task { get; set; }

		protected ICardContent CardContent { get; set; }

		protected BMBoardSectionViewModel ViewModel { get; set; }

		protected static void AssertLoadFailed(ZString failureMessage, CardLoadResult result)
		{
			Assert(!result.LoadSucceeded);
			AssertEquals(failureMessage, result.FailureMessage);
		}

		#endregion
	}
}
