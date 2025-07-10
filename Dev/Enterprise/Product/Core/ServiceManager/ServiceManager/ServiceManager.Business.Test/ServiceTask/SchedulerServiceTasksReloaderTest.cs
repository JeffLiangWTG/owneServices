using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Business.Testing
{
	[TestedType(typeof(SchedulerServiceTasksReloader))]
	class SchedulerServiceTasksReloaderTest : TestCaseWithFactory
	{
		public void TestReloadTasks()
		{
			// Arrange
			var taskSchedules = new ServiceTaskSchedule[5];
			for (int i = 0; i < taskSchedules.Length; i++)
			{
				taskSchedules[i] = Factory.NewWithValidTestData<ServiceTaskSchedule>();
			}
			Factory.Save();

			var tasksLoader = new SchedulerServiceTasksLoader(new BusinessObjectFactory());

			var initialCollection = tasksLoader.Load();
			AssertEquals(5, initialCollection.GovernedTasks.Count());

			var updatedTime = DateTime.UtcNow.AddMinutes(2);
			var updatedDescription = updatedTime.ToString("o");
			var updatedTaskIndices = new[] { 0, 4 };

			Array.ForEach(updatedTaskIndices, index =>
			{
				taskSchedules[index].S5_ScheduleDescription = updatedDescription;
			});

			Factory.Save();

			// Act
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

			var updatedCollection = new SchedulerServiceTasksReloader().Reload(new DateTimeOffset(updatedTime, TimeSpan.Zero));

			// Assert
			CombineAssertions(() =>
			{
				AssertEquals(updatedTaskIndices.Length, updatedCollection.GovernedTasks.Count());
				Array.ForEach(updatedCollection.GovernedTasks.ToArray(), task =>
				{
					AssertEquals(updatedDescription, task.Description);
				});
			});
		}
	}
}
