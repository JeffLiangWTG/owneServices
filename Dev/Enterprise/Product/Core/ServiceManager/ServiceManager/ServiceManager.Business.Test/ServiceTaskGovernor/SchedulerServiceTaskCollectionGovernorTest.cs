using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Business.Testing
{
	[TestedType(typeof(SchedulerServiceTaskCollectionGovernor))]
	public class SchedulerServiceTaskCollectionGovernorTest : TestCaseWithFactory
	{
		public void TestSetActiveNoFilter()
		{
			// Arrange
			var taskSchedules = new ServiceTaskSchedule[5];
			for (int i = 0; i < taskSchedules.Length; i++)
			{
				taskSchedules[i] = Factory.NewWithValidTestData<ServiceTaskSchedule>();
				taskSchedules[i].S5_IsActive = false;
			}
			Factory.Save();

			using var adapter = new SchedulerServiceTaskTransactionAdapter();
			var collectionGovernor = adapter.GetCollectionGovernorForAllTasks();

			// Act
			collectionGovernor.SetActive(true, Array.Empty<Guid>());

			// Assert
			AssertEquals(5, collectionGovernor.GovernedTasks.Count(t => t.IsActive));
		}

		public void TestSetActiveWithFilter()
		{
			// Arrange
			var taskSchedules = new ServiceTaskSchedule[5];
			for (int i = 0; i < taskSchedules.Length; i++)
			{
				taskSchedules[i] = Factory.NewWithValidTestData<ServiceTaskSchedule>();
				taskSchedules[i].S5_IsActive = false;
			}
			Factory.Save();

			var filterTaskIndices = new[] { 0, 4 };
			var filterTaskPks = new List<Guid>();

			Array.ForEach(filterTaskIndices, index =>
			{
				filterTaskPks.Add(taskSchedules[index].PK.ToGuid());
			});

			using var adapter = new SchedulerServiceTaskTransactionAdapter();
			var collectionGovernor = adapter.GetCollectionGovernorForAllTasks();

			// Act
			collectionGovernor.SetActive(true, filterTaskPks);

			// Assert
			AssertEquals(2, collectionGovernor.GovernedTasks.Count(t => t.IsActive));
		}

		public void TestSetBranchPkNoFilter()
		{
			// Arrange
			var taskSchedules = new ServiceTaskSchedule[5];
			for (int i = 0; i < taskSchedules.Length; i++)
			{
				taskSchedules[i] = Factory.NewWithValidTestData<ServiceTaskSchedule>();
			}
			Factory.Save();

			var dummyBranchPk = new Guid();

			using var adapter = new SchedulerServiceTaskTransactionAdapter();
			var collectionGovernor = adapter.GetCollectionGovernorForAllTasks();

			// Act
			collectionGovernor.SetBranchPk(dummyBranchPk, Array.Empty<Guid>());

			// Assert
			AssertEquals(5, collectionGovernor.GovernedTasks.Count(t => t.BranchPk == dummyBranchPk));
		}

		public void TestSetBranchPkWithFilter()
		{
			// Arrange
			var taskSchedules = new ServiceTaskSchedule[5];
			for (int i = 0; i < taskSchedules.Length; i++)
			{
				taskSchedules[i] = Factory.NewWithValidTestData<ServiceTaskSchedule>();
			}
			Factory.Save();

			var dummyBranchPK = new Guid();

			var filterTaskIndices = new[] { 1, 2, 0 };
			var filterTaskPks = new List<Guid>();

			Array.ForEach(filterTaskIndices, index =>
			{
				filterTaskPks.Add(taskSchedules[index].PK.ToGuid());
			});

			using var adapter = new SchedulerServiceTaskTransactionAdapter();
			var collectionGovernor = adapter.GetCollectionGovernorForAllTasks();

			// Act
			collectionGovernor.SetBranchPk(dummyBranchPK, filterTaskPks);

			// Assert
			AssertEquals(3, collectionGovernor.GovernedTasks.Count(t => t.BranchPk == dummyBranchPK));
		}

		public void TestReload()
		{
			// Arrange
			var taskSchedules = new ServiceTaskSchedule[5];
			for (int i = 0; i < taskSchedules.Length; i++)
			{
				taskSchedules[i] = Factory.NewWithValidTestData<ServiceTaskSchedule>();
				taskSchedules[i].S5_IsActive = false;
			}
			Factory.Save();

			using var transactionAdapter = new SchedulerServiceTaskTransactionAdapter();
			var collectionGovernor = transactionAdapter.GetCollectionGovernorForAllTasks();

			var updatedTaskIndices = new[] { 0, 4 };

			// Act
			Array.ForEach(updatedTaskIndices, index =>
			{
				Db.Connection.ExecuteNonQuery(@"
UPDATE dbo.StmScheduleTask
SET S5_IsActive = 1
WHERE S5_PK = @pk
",
					cmd =>
					{
						cmd.AddParameter("@pk", System.Data.SqlDbType.UniqueIdentifier, taskSchedules[index].PK.ToGuid());
					});
			});

			// Act
			collectionGovernor.Reload();

			// Assert
			AssertEquals(2, collectionGovernor.GovernedTasks.Count(t => t.IsActive));
		}
	}
}
