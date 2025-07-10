using System.Collections.ObjectModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Main.Navigation.ViewModels;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace CargoWise.Main.Navigation.Test;

sealed class MyTasksViewModelTest : TestCaseWithFactory
{
	public void TestProperties()
	{
		// Arrange
		var uut = new MyTasksViewModel();

		AssertEquals(nameof(uut.NoMyTasksText), "No tasks.", uut.NoMyTasksText);
		AssertEquals(nameof(uut.IdHeaderText), "ID #", uut.IdHeaderText);
		AssertEquals(nameof(uut.NameHeaderText), "Name", uut.NameHeaderText);
		AssertEquals(nameof(uut.TaskDescriptionHeaderText), "Task Description", uut.TaskDescriptionHeaderText);
		AssertEquals(nameof(uut.StatusHeaderText), "Status", uut.StatusHeaderText);
		AssertEquals(nameof(uut.DisplayName), "Current Tasks", uut.DisplayName);
	}

	public void TestMyTasks_WhenNull()
	{
		// Arrange
		var uut = new MyTasksViewModel();

		// Assert
		AssertNull(uut.MyTasks);
	}

	public void TestMyTasks_WhenRefresh_WhenEmptyTasks()
	{
		// Arrange
		var uut = new MyTasksViewModel();

		// Act
		uut.Refresh();

		// Assert
		AssertNotNull(uut.MyTasks);
		AssertEquals(0, uut.MyTasks.Count);
	}

	public void TestMyTasks_ParentIDandSummarySet()
	{
		var dummy = Factory.New<DummyWithWorkflow>();
		var task = dummy.WorkflowItems.Tasks.AddNew();
		var taskItem = new MyTasksItemViewModel(task);

		AssertNotNullOrEmpty(taskItem.ParentId);
		AssertNotNullOrEmpty(taskItem.ParentSummary);

		Assert(taskItem.AreBothParentIDandSummarySet);
	}

	public void TestMyTasks_ParentIDandSummaryNotSet()
	{
		var task = Factory.New<ProcessTask>();
		var taskItem = new MyTasksItemViewModel(task);

		AssertNullOrEmpty(taskItem.ParentId);
		AssertNullOrEmpty(taskItem.ParentSummary);

		Assert(!taskItem.AreBothParentIDandSummarySet);
	}

	public void TestMyTasksViewModelOpenTask()
	{
		var tasksModel = new MyTasksViewModel();

		var dummy = Factory.New<DummyWithWorkflow>();
		var task = dummy.WorkflowItems.Tasks.AddNew();
		var tasks = new ObservableCollection<IMyTasksItemViewModel>();
		var taskItem = new MyTasksItemViewModel(task);
		tasks.Add(taskItem);
		tasksModel.MyTasks = tasks;

		AssertNoExceptionThrown(() => tasks[0].LinkAction.Execute(null));
	}

	public void TestMyTasksViewModelOpenTaskNotFound()
	{
		var tasksModel = new MyTasksViewModel();

		var dummy = Factory.New<DummyWithWorkflow>();
		var pTask = dummy.WorkflowItems.Tasks.AddNew();
		var task = new MyTasksItemViewModelNull(pTask);
		var tasks = new ObservableCollection<IMyTasksItemViewModel>();
		tasks.Add(task);
		tasksModel.MyTasks = tasks;

		AssertNoExceptionThrown(() => tasks[0].LinkAction.Execute(null));
		Assert(task.IsTaskNotFound);
	}

	class MyTasksItemViewModelNull : MyTasksItemViewModel
	{
		public MyTasksItemViewModelNull(ProcessTask taskItem) : base(taskItem)
		{
		}

		protected override ProcessTask GetProcessTask(ZGuid pk)
		{
			return null;
		}
	}
}
