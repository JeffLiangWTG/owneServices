using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Business.Testing
{
	public class ServiceTaskAccessorTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGetServiceTaskSchedule()
		{
			using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				// Arrange
				const string taskCode = "BLA";

				var schedule = Factory.New<ServiceTaskSchedule>();
				schedule.S5_ScheduleType = taskCode;
				schedule.S5_IsActive = true;

				// Act
				var serviceTask = new ServiceTaskAccessor().GetServiceTask(schedule);

				// Assert
				Assert(serviceTask.S5_IsActive);
				AssertEquals(taskCode, serviceTask.S5_ScheduleType);
			}
		}

		[ExpectNoExceptions]
		public void TestGetStmServiceTask()
		{
			// Arrange
			var defaultScheduleMock = Mock.Of<IDefaultSchedule>(d =>
				d.RunEvery == "15minutes");

			var hostedServiceMock = Mock.Of<IHostedServiceAttribute>(a =>
				a.Description == "some description" &&
				a.Category == "some category" &&
				a.MutuallyExclusiveTaskGroup == MutuallyExclusiveServiceTaskGroups.NoGroup &&
				a.DefaultSchedule == defaultScheduleMock);

			var hostedServiceProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
			hostedServiceProviderMock
				.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
				.Returns(hostedServiceMock);

			using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ObjectFactory.Substitute(hostedServiceProviderMock.Object))
			{
				const string taskCode = "BLA";

				var stmTask = Factory.New<StmServiceTask>();
				stmTask.SST_ServiceTaskCode = taskCode;
				stmTask.SST_Active = true;

				// Act
				var serviceTask = new ServiceTaskAccessor().GetServiceTask(stmTask);

				// Assert
				Assert(serviceTask.S5_IsActive);
				AssertEquals(taskCode, serviceTask.S5_ScheduleType);
			}
		}
	}
}
