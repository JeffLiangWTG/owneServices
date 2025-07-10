using System.Linq;
using Moq;
using NUnit.Framework;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host.Testing.Core.RunnableTask
{
	class AllTasksCollectionTest
	{
		[Test]
		public void TestAdd()
		{
			Assert.DoesNotThrow(() => allTasks.Add(Mock.Of<IRunnableServiceTask>()));
			Assert.That(allTasks.Count, Is.EqualTo(1));
		}

		[Test]
		public void TestClear()
		{
			allTasks.Add(Mock.Of<IRunnableServiceTask>());
			allTasks.Add(Mock.Of<IRunnableServiceTask>());
			allTasks.Add(Mock.Of<IRunnableServiceTask>());

			allTasks.Clear();

			Assert.That(allTasks, Is.Empty);
		}

		[Test]
		public void TestGet()
		{
			allTasks.Add(Mock.Of<IRunnableServiceTask>(t => t.Code == "AAA"));
			allTasks.Add(Mock.Of<IRunnableServiceTask>(t => t.Code == "BBB"));
			allTasks.Add(Mock.Of<IRunnableServiceTask>(t => t.Code == "CCC"));

			var result = allTasks.TryGetByCode("BBB", out var task);

			Assert.That(result, Is.True);
			Assert.That(task.Code, Is.EqualTo("BBB"));
		}

		[Test]
		public void TestGetAll()
		{
			var task1 = Mock.Of<IRunnableServiceTask>(t => t.Code == "AAA");
			var task2 = Mock.Of<IRunnableServiceTask>(t => t.Code == "BBB");
			var task3 = Mock.Of<IRunnableServiceTask>(t => t.Code == "CCC");

			allTasks.Add(task1);
			allTasks.Add(task2);
			allTasks.Add(task3);

			var results = allTasks.GetAll();

			Assert.That(results.Count(), Is.EqualTo(3));
			Assert.That(results, Is.EquivalentTo(new[] { task1, task2, task3 }));
		}

		[SetUp]
		public void SetUp()
		{
			allTasks = new AllTasksCollection();
		}

		AllTasksCollection allTasks;
	}
}
