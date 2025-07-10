using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Moq;
using NUnit.Framework;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Business.Testing
{
	public class ServiceManagerGovernorTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestSetServiceTaskNextRuntime()
		{
			// Arrange
			var timeNow = DateTime.UtcNow;
			const string taskCode = "BLA";

			var schedule = Factory.New<ServiceTaskSchedule>();
			schedule.S5_ScheduleType = taskCode;
			Factory.Save();

			// Act
			using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				serviceManagerGovernor.SetServiceTaskNextRuntime(taskCode, timeNow);
			}

			// Assert
			AssertNoExceptionThrown(() =>
			{
				serviceTaskScheduleStatusProviderMock.Verify(provider => provider.SetServiceTaskNextRuntime(taskCode, timeNow, null), Times.Once);
			});
		}

		public void TestSetServiceTaskIsActive()
		{
			// Arrange
			const string taskCode = "BLA";

			var schedule = Factory.New<ServiceTaskSchedule>();
			schedule.S5_ScheduleType = taskCode;
			schedule.S5_IsActive = true;
			Factory.Save();

			// Act
			using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				serviceManagerGovernor.SetServiceTaskIsActive(taskCode, false);
			}

			// Assert
			AssertEquals(expected: false, schedule.S5_IsActive);
		}

		public void TestUpdatesDbRecord()
		{
			// Arrange
			var timeNow = DateTime.UtcNow;
			var schedule = Factory.New<ServiceTaskSchedule>();
			schedule.S5_ScheduleType = "xxx";
			Factory.Save();

			// Act
			using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				serviceManagerGovernor.SetServiceTaskNextRuntime("xxx", timeNow.AddMinutes(5));
			}

			// Assert
			AssertEquals("Next run time should be updated", schedule.S5_NextScheduledPrintRunTimeUtc, timeNow.AddMinutes(5));
		}

		[ExpectNoExceptions]
		[TestDate(2025, 1, 1, 1, 1, 1)]
		public void TestSetStmServiceTaskNextRuntime()
		{
			// Arrange
			var timeNow = ZDateTimeOffset.UtcNow;
			const string taskCode = "BLA";

			var serviceTask = Factory.NewWithValidTestData<StmServiceTask>();
			serviceTask.SST_ServiceTaskCode = taskCode;
			serviceTask.SST_Configuration = "<ScheduleConfig><NextRunTimeCalculatorHours Period=\"1\" /></ScheduleConfig>";
			serviceTask.SST_NextRunTime = ZDateTime.MinSmallDateTimeValue.ToOffset();
			Factory.Save();

			// Act
			using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				serviceManagerGovernor.SetServiceTaskNextRuntime(taskCode, timeNow.ToDateTimeOffset());
			}

			// Assert
			CombineAssertions(() =>
			{
				serviceTask.Reload();
				serviceTaskScheduleStatusProviderMock.Verify(provider => provider.SetServiceTaskNextRuntime(taskCode, timeNow.ToDateTime(), null), Times.Once);
				AssertEquals(timeNow, serviceTask.SST_NextRunTime);
			});
		}

		public void TestSetStmServiceTaskIsActive()
		{
			// Arrange
			const string taskCode = "BLA";

			var serviceTask = Factory.NewWithValidTestData<StmServiceTask>();
			serviceTask.SST_ServiceTaskCode = taskCode;
			serviceTask.SST_Configuration = "<ScheduleConfig><NextRunTimeCalculatorHours Period=\"1\" /></ScheduleConfig>";
			serviceTask.SST_Active = true;
			Factory.Save();

			// Act
			using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				serviceManagerGovernor.SetServiceTaskIsActive(taskCode, false);
			}

			// Assert
			serviceTask.Reload();
			AssertEquals(expected: false, serviceTask.SST_Active);
		}

		protected override void SetUp()
		{
			base.SetUp();
			serviceTaskScheduleStatusProviderMock = new Mock<IServiceTaskScheduleStatusProvider>();
			serviceManagerGovernor = new ServiceManagerGovernor(serviceTaskScheduleStatusProviderMock.Object, Factory);
		}

		Mock<IServiceTaskScheduleStatusProvider> serviceTaskScheduleStatusProviderMock;
		ServiceManagerGovernor serviceManagerGovernor;
	}
}
