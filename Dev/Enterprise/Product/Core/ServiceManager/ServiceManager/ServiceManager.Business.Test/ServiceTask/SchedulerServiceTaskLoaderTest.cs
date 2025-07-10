using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Business.Testing
{
	[TestedType(typeof(SchedulerServiceTaskLoader))]
	class SchedulerServiceTaskLoaderTest
	{
		class LoadServiceTaskTest : TestCaseWithFactory
		{
			public void TestLoadEmptyTask()
			{
				// Arrange

				// Act
				IServiceTask task = new SchedulerServiceTaskLoader(new Lazy<BusinessObjectFactory>(() => Factory)).Load("XYZ");

				// Assert
				AssertNull(task);
			}

			public void TestLoadValidTask()
			{
				// Arrange
				var schedule = Factory.New<ServiceTaskSchedule>();
				schedule.S5_ScheduleType = "XYZ";
				var settings = new HostedServiceAttribute();
				settings.IsMandatory = false;
				schedule.SetStaticServiceAttributesDebugOnly(settings);
				schedule.Factory.Save();

				// Act
				IServiceTask task = new SchedulerServiceTaskLoader(new Lazy<BusinessObjectFactory>(() => Factory)).Load("XYZ");

				// Assert
				AssertNotNull(task);
				AssertEquals("XYZ", task.Code);
			}
		}
	}
}
