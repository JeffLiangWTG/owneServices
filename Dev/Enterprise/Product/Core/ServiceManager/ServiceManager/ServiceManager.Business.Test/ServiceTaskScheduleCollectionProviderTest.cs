using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Scheduler.Business;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Business.Testing
{
	[TestedType(typeof(ServiceTaskScheduleCollectionProvider))]
	[UseSnapshotProtection]
	class ServiceTaskScheduleCollectionProviderTest : TestCaseWithFactory
	{
		public void TestScheduledServiceTasksProvider_Tasks_Getter()
		{
			const int numberOfTasks = 5;
			_ = CreateTaskSchedules(numberOfTasks);
			Factory.Save();

			var serviceTasksProvider = new ServiceTaskScheduleCollectionProvider();
			var serviceTaskScheduleCollection = serviceTasksProvider.Load(new BusinessObjectFactory());

			AssertEquals(numberOfTasks, serviceTaskScheduleCollection.Tasks.Count);
		}

		public void TestLoadUpdatedFromTime()
		{
			// Arrange
			const int numberOfTasks = 5;
			var taskSchedules = CreateTaskSchedules(numberOfTasks);
			var serviceTasksProvider = new ServiceTaskScheduleCollectionProvider();
			Factory.Save();

			var initialCollection = serviceTasksProvider.Load(new BusinessObjectFactory());

			// Act
			var updatedTime = DateTime.UtcNow.AddMinutes(2);
			var updatedDescription = updatedTime.ToString("o");
			var updatedTaskIndices = new[] { 0, 4 };
			Array.ForEach(updatedTaskIndices, index =>
			{
				taskSchedules[index].S5_IsActive = !taskSchedules[index].S5_IsActive;
				taskSchedules[index].S5_ScheduleDescription = updatedDescription;
			});

			Factory.Save();

			Array.ForEach(updatedTaskIndices, index =>
			{
				Db.Connection.ExecuteNonQuery(@"
UPDATE dbo.StmScheduleTask
SET S5_SystemLastEditTimeUtc = @updatedTime
WHERE S5_PK = @pk
",
				cmd =>
				{
					cmd.AddParameter("@updatedTime", System.Data.SqlDbType.SmallDateTime, updatedTime);
					cmd.AddParameter("@pk", System.Data.SqlDbType.UniqueIdentifier, taskSchedules[index].PK.ToGuid());
				});
			});

			var updatedCollection = serviceTasksProvider.LoadUpdatedFromTime(new BusinessObjectFactory(), updatedTime);

			// Assert
			CombineAssertions(() =>
			{
				AssertEquals(updatedTaskIndices.Length, updatedCollection.Tasks.Count);
				Array.ForEach(updatedCollection.Tasks.Cast<AutoStmScheduleTask>().ToArray(), task =>
				{
					AssertEquals(updatedDescription, task.S5_ScheduleDescription);
				});
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			Db.Connection.ExecuteNonQuery("DELETE FROM dbo.StmScheduleTask");
		}

		ServiceTaskSchedule[] CreateTaskSchedules(int numberOfTasks)
		{
			var tasks = new ServiceTaskSchedule[numberOfTasks];
			for (var i = 0; i < numberOfTasks; i++)
			{
				tasks[i] = Factory.NewWithValidTestData<ServiceTaskSchedule>();
			}

			return tasks;
		}
	}
}
