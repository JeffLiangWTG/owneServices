using System.Linq;
using CargoWise.Common;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	class TaskOrderSorterTest : BMSTestCaseWithFactory
	{
		[TestDate(2015, 1, 21, 9, 0, 0)]
		public void TestSort()
		{
			var system = CreateSystem("ORG");

			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);
			var sectionViewModel = BMSTestHelper.CreateViewModel(section);

			var job1 = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader1 = ProcessJobHeader.GetForParent(job1, Factory);
			var workflow1_1 = CreateWorkflow(jobHeader1, "workflow1_1");
			var workflow1_2 = CreateWorkflow(jobHeader1, "workflow1_2");
			var task1_1 = CreateTask(workflow1_1, string.Empty, 60, description: "task1_1", taskStatus: ProcessTaskStatusCodeList.Codes.Suspended, sequence: 1);
			var task1_2 = CreateTask(workflow1_2, string.Empty, 60, description: "task1_2", sequence: 3);

			var job2 = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader2 = ProcessJobHeader.GetForParent(job2, Factory);
			var workflow2_1 = CreateWorkflow(jobHeader2, "workflow2_1");
			var workflow2_2 = CreateWorkflow(jobHeader2, "workflow2_2");
			var task2_1 = CreateTask(workflow2_1, string.Empty, 60, description: "task2_1", taskStatus: ProcessTaskStatusCodeList.Codes.Working, sequence: 8);
			var task2_2 = CreateTask(workflow2_2, string.Empty, 60, description: "task2_2", sequence: 4);

			var job3 = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader3 = ProcessJobHeader.GetForParent(job3, Factory);
			var workflow3_1 = CreateWorkflow(jobHeader3, "workflow3_1");
			var workflow3_2 = CreateWorkflow(jobHeader3, "workflow3_2");
			var task3_1 = CreateTask(workflow3_1, string.Empty, 60, description: "task3_1", sequence: 5);
			var task3_2 = CreateTask(workflow3_2, string.Empty, 60, description: "task3_2", sequence: 10);

			var priorityTags = BMSTestHelper.CreateTagDefinition(Factory, "PRI", "Priority Tags");
			var platinum = BMSTestHelper.CreateTagMagnitude(priorityTags, "PLT", nudge: 100);
			var mrShearer = BMSTestHelper.CreateTagMagnitude(priorityTags, "BAS", nudge: 1000);

			workflow3_2.AddTag(mrShearer);
			workflow3_1.AddTag(platinum);
			Factory.Save();

			var tasks = TaskOrderSorter.SortByBuckettedTasks(new[] { task3_2, task3_1, task2_2, task2_1, task1_2, task1_1 }, t => new NaiveTaskOrderable(t, sectionViewModel), CardSortType.BufferWorkSequence).ToArray();

			AssertEquals(tasks[0].P9_Description, task2_1, tasks[0]);
			AssertEquals(tasks[1].P9_Description, task1_1, tasks[1]);
			AssertEquals(tasks[2].P9_Description, task3_2, tasks[2]);
			AssertEquals(tasks[3].P9_Description, task3_1, tasks[3]);
			AssertEquals(tasks[4].P9_Description, task1_2, tasks[4]);
			AssertEquals(tasks[5].P9_Description, task2_2, tasks[5]);

			tasks = TaskOrderSorter.SortByBuckettedTasks(new[] { task3_2, task3_1, task2_2, task2_1, task1_2, task1_1 }, t => new NaiveTaskOrderable(t, sectionViewModel), CardSortType.ReleaseSequence).ToArray();

			AssertEquals(tasks[0].P9_Description, task2_1, tasks[0]);
			AssertEquals(tasks[1].P9_Description, task1_1, tasks[1]);
			AssertEquals(tasks[2].P9_Description, task3_2, tasks[2]);
			AssertEquals(tasks[3].P9_Description, task3_1, tasks[3]);
			AssertEquals(tasks[4].P9_Description, task2_2, tasks[4]);
			AssertEquals(tasks[5].P9_Description, task1_2, tasks[5]);
		}

		[TestDate(2015, 1, 21, 9, 0, 0)]
		public void TestSort_TaskID()
		{
			var system = CreateSystem("ORG");

			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);
			var sectionViewModel = BMSTestHelper.CreateViewModel(section);

			var job1 = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader1 = ProcessJobHeader.GetForParent(job1, Factory);
			var workflow1 = CreateWorkflow(jobHeader1, "workflow1");

			var tasks = Enumerable.Range(0, 4)
				.Select(index => CreateTask(workflow1, staffCode: string.Empty, lowEstMinutes: 60, description: $"task-{index}", sequence: 1))
				.ToArray();

			Factory.Save();

			var unsortedTasks = new[] { tasks[2], tasks[0], tasks[3], tasks[1] };

			foreach (var sortType in new[] { CardSortType.BufferWorkSequence, CardSortType.ReleaseSequence, CardSortType.LastTransferTime })
			{
				AssertSortByBuckettedTasks_OrderByTaskID(unsortedTasks, sectionViewModel, sortType);
			}
		}

		public void TestNullReferenceException()
		{
			var system = CreateSystem("ORG");

			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);
			var sectionViewModel = BMSTestHelper.CreateViewModel(section);

			var job1 = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader1 = ProcessJobHeader.GetForParent(job1, Factory);
			var workflow1 = CreateWorkflow(jobHeader1, "workflow1");

			var tasks = Enumerable.Range(0, 4)
				.Select(index => CreateTask(workflow1, staffCode: string.Empty, lowEstMinutes: 60, description: $"task-{index}", sequence: 1))
				.ToArray();

			Factory.Save();

			var unsortedTasks = new[] { tasks[2], tasks[0], tasks[3], tasks[1] };

			AssertNoExceptionThrown(() =>
			{
				TaskOrderSorter.SortByBuckettedTasks(
unsortedTasks,
t => null,
CardSortType.ReleaseSequence
).ToArray();
			});
		}

		static void AssertSortByBuckettedTasks_OrderByTaskID(ProcessTask[] unsortedTasks, BMBoardSectionViewModel sectionViewModel, CardSortType sortType)
		{
			CombineAssertions(
				$"GIVEN unsorted tasks-with same P9_Sequence",
				() =>
				{
					AssertArrayEqualsByElements("Precondition - sort", new[] { "task-2", "task-0", "task-3", "task-1" }, unsortedTasks.Select(t => t.P9_Description.ToString()).ToArray());
					unsortedTasks.ForEach(t => AssertEquals("Precondition - sequence", 1, t.P9_Sequence));
				});

			var sortedTasks = TaskOrderSorter.SortByBuckettedTasks(
				unsortedTasks,
				t => new NaiveTaskOrderable(t, sectionViewModel),
				sortType
			).ToArray();

			var expectedSortedTasks = unsortedTasks.OrderBy(t => t.P9_TaskID).ToArray();

			CombineAssertions($"WHEN sorting {sortType} THEN should sorted by P9_TaskID", () =>
			{
				Enumerable.Range(0, expectedSortedTasks.Length)
					.ToList()
					.ForEach(index =>
						AssertEquals($"index: {index}",
							expectedSortedTasks[index].P9_Description,
							sortedTasks[index].P9_Description));
			});
		}
	}
}
