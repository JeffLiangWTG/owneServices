using System.Linq;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.ServiceManager.Business.Testing
{
	[TestedType(typeof(SchedulerServiceTasksLoader))]
	class SchedulerServiceTasksLoaderTest : TestCaseWithFactory
	{
		public void TestLoadWithNoTasks()
		{
			// Arrange

			// Act
			var collectionGovernor = new SchedulerServiceTasksLoader(Factory).Load();

			// Assert
			AssertEquals(0, collectionGovernor.GovernedTasks.Count());
		}

		public void TestLoadTasks()
		{
			// Arrange
			var schedule = Factory.New<ServiceTaskSchedule>();
			schedule.S5_ScheduleType = "XYZ";
			var settings = new HostedServiceAttribute();
			settings.IsMandatory = false;
			schedule.SetStaticServiceAttributesDebugOnly(settings);

			var schedule2 = Factory.New<ServiceTaskSchedule>();
			schedule2.S5_ScheduleType = "~TS";
			var settings2 = new HostedServiceAttribute();
			settings2.IsMandatory = false;
			schedule.SetStaticServiceAttributesDebugOnly(settings2);

			Factory.Save();

			// Act
			var collectionGovernor = new SchedulerServiceTasksLoader(Factory).Load();

			// Assert
			AssertContainsExactElementsInAnyOrder(new[] { "XYZ", "~TS" }, collectionGovernor.GovernedTasks.Select(t => t.Code));
		}
	}
}
