using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.WorkflowManager.ServiceTasks.Testing
{
	sealed class WorkflowFieldChangeTriggerListTest : TestCaseWithFactory
	{
		public void TestClear()
		{
			var list = new WorkflowFieldChangeTriggerList();
			list.Add(Factory.New<StmChangeLog>(), Factory.New<ProcessTask>());
			list.Add(Factory.New<StmChangeLog>(), Factory.New<ProcessTask>());
			AssertEquals("Precondition: 2 elements in list", 2, list.Count());

			list.Clear();
			AssertEquals("List created", 0, list.Count());
		}

		public void TestAdd()
		{
			StmChangeLog log1 = Factory.New<StmChangeLog>();
			StmChangeLog log2 = Factory.New<StmChangeLog>();

			ProcessTask task1 = Factory.New<ProcessTask>();
			ProcessTask task2 = Factory.New<ProcessTask>();
			ProcessTask task3 = Factory.New<ProcessTask>();
			ProcessTask task4 = Factory.New<ProcessTask>();
			ProcessTask task5 = Factory.New<ProcessTask>();
			ProcessTask task6 = Factory.New<ProcessTask>();

			var list = new WorkflowFieldChangeTriggerList();
			list.Add(null, task1);
			list.Add(null, task2);
			list.Add(log1, task3, task4);
			list.Add(log2, task5);
			list.Add(log1, task6);

			AssertEquals("Precondition: 3 elements in list", 3, list.Count());
			AssertContainsExactElementsInAnyOrder(new ProcessTask[] { task1, task2 }, list.First(element => element.Key == null));
			AssertContainsExactElementsInAnyOrder(new ProcessTask[] { task3, task4, task6 }, list.First(element => element.Key == log1));
			AssertContainsExactElementsInAnyOrder(new ProcessTask[] { task5 }, list.First(element => element.Key == log2));
		}
	}
}
