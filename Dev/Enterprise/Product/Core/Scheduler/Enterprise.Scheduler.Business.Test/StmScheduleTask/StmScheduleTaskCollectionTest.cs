using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Scheduler.Business.Testing
{
	[TestedType(typeof(StmScheduleTaskCollection))]
	sealed class StmScheduleTaskCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCreateAdditionalFilter()
		{
			StmScheduleTask task1 = Factory.NewWithValidTestData<StmScheduleTask>();
			task1.S5_ParentTableCode = StmServiceHostSchema.Constants.Prefix;

			StmScheduleTask task2 = Factory.NewWithValidTestData<StmScheduleTask>();
			task2.S5_ParentTableCode = "##";

			StmScheduleTask task3 = Factory.NewWithValidTestData<StmScheduleTask>();
			task3.S5_ParentTableCode = Constants.ArchiveManager.ParentTableCode;

			StmScheduleTaskCollection tasks = new StmScheduleTaskCollection(Factory, "##");
			tasks.Load();
			Assert(!tasks.Contains(task1));
			Assert(tasks.Contains(task2));
			Assert(!tasks.Contains(task3));
		}

		public void TestReLoad()
		{
			var task1 = CreateDummyStmScheduleTask(new ZDateTime(2006, 1, 1));
			var task2 = CreateDummyStmScheduleTask(new ZDateTime(2006, 2, 2));
			var task3 = CreateDummyStmScheduleTask(new ZDateTime(2006, 3, 3));

			var task4 = CreateDummyStmScheduleTask(new ZDateTime(2006, 3, 3));
			task4.IsCancelled = true;

			var task5 = CreateDummyStmScheduleTask(new ZDateTime(2006, 2, 2), false);

			var task6 = CreateDummyStmScheduleTask(new ZDateTime(2006, 2, 2));
			task6.S5_IsPrivate = true;

			var tasks = new StmScheduleTaskCollection(Factory, DummyBizoSchema.Constants.Prefix);
			tasks.LoadTasksToRun(new DateTime(2006, 5, 5));
			AssertEquals("Count", 4, tasks.Count);
			AssertEquals("Contains(task1)", true, tasks.Contains(task1));
			AssertEquals("Contains(task2)", true, tasks.Contains(task2));
			AssertEquals("Contains(task3)", true, tasks.Contains(task3));
			AssertEquals("Contains(task6)", true, tasks.Contains(task6));

			tasks.LoadTasksToRun(new DateTime(2006, 3, 2));
			AssertEquals("Count", 3, tasks.Count);
			AssertEquals("Contains(task1)", true, tasks.Contains(task1));
			AssertEquals("Contains(task2)", true, tasks.Contains(task2));
			AssertEquals("Contains(task6)", true, tasks.Contains(task6));

			tasks.LoadTasksToRun(new DateTime(2006, 2, 1));
			AssertEquals("Count", 1, tasks.Count);
			AssertEquals("Contains(task1)", true, tasks.Contains(task1));

			tasks.LoadTasksToRun(new DateTime(2005, 12, 30));
			AssertEquals(0, tasks.Count);
		}

		DummyStmScheduleTask CreateDummyStmScheduleTask(ZDateTime nextSchedulePrintRunTime, bool isActive = true)
		{
			var task = Factory.New<DummyStmScheduleTask>();
			task.S5_ParentTableCode = DummyBizoSchema.Constants.Prefix;
			task.S5_NextScheduledPrintRunTimeUtc = nextSchedulePrintRunTime;
			task.S5_IsActive = isActive;

			if (task.S5_ParentTableCode != "SH")
			{
				task.S5_ParentID = Guid.NewGuid();
			}

			return task;
		}

		public void TestGetCountOfPendingTasks()
		{
			var scheduledDate = new ZDateTime(2021, 9, 22);
			CreateDummyStmScheduleTask(scheduledDate, true);
			CreateDummyStmScheduleTask(scheduledDate, true);
			CreateDummyStmScheduleTask(scheduledDate, true);

			Factory.Save();

			var tasks = new StmScheduleTaskCollection(Factory, DummyBizoSchema.Constants.Prefix);
			var pendingTasksQueue = tasks.GetPendingTasksQueue(scheduledDate);

			AssertEquals(3, pendingTasksQueue.QueueSize);
			NUnit.Framework.Assert.That((int)pendingTasksQueue.MaximumItemAge.TotalSeconds, NUnit.Framework.Is.EqualTo((int)(DateTime.UtcNow - scheduledDate).TotalSeconds).Within(60));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new StmScheduleTaskCollection(Factory, DummyBizoSchema.Constants.Prefix);
		}
	}
}
