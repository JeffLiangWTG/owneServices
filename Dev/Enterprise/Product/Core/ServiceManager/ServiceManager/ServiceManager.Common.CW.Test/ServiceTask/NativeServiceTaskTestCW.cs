using System;
//using System.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Moq;
using ServiceManager.Common.CW;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Shared.Abstractions;

namespace ServiceManager.Common.CW1.Test.ServiceTask;

class NativeServiceTaskTestCW : TestCaseWithFactory
{
	public void TestGetSchedulePeriodDuration_NudgeableTaskBelowMinimum()
	{
		// Arrange
		using var registrySetup = SystemDataRegistry.Instance.ServiceTaskBusinessObjectBindingEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

		hostedServiceAttributeMock = new Mock<IHostedServiceAttribute>();
		hostedServiceAttributeMock.SetupGet(a => a.Code).Returns("111");
		hostedServiceAttributeMock.SetupGet(a => a.DefaultSchedule).Returns(Mock.Of<IDefaultSchedule>(d => d.RunEvery == "5minutes"));

		hostedServiceAttributeProviderMock
			.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
			.Returns(hostedServiceAttributeMock.Object);

		hostedServiceBusinessObjectBindingsProviderMock
			.SetupGet(p => p.BusinessObjectBindings)
			.Returns(new[] {
				new HostedServiceBusinessObjectBindingAttribute("111", "DummyBizo", Array.Empty<string>(), null),
				new HostedServiceBusinessObjectBindingAttribute("222", "DummyBizo", Array.Empty<string>(), null),
				new HostedServiceBusinessObjectBindingAttribute("333", "DummyBizo", Array.Empty<string>(), null),
				new HostedServiceBusinessObjectBindingAttribute("111", "DummyBizo2", Array.Empty<string>(), null) });

		// Act
		var dto = new NativeServiceTaskDTO("111", taskPk, true, DateTimeOffset.Now, DateTimeOffset.MinValue, branchPk, "BRN", "", "<ScheduleConfig><NextRunTimeCalculatorMinutes Period=\"5\" /></ScheduleConfig>");
		var task = new NativeServiceTask(dto, hostedServiceAttributeProviderMock.Object,
			hostedServiceBusinessObjectBindingsProviderMock.Object, scheduleStatusProviderMock.Object, dateTimeProviderMock.Object);

		// Assert
		AssertEquals(TimeSpan.FromMinutes(15), task.SchedulePeriodDuration);
	}

	public void TestGetSchedulePeriodDuration_NudgeableTaskAboveMinimum()
	{
		// Arrange
		using var registrySetup = SystemDataRegistry.Instance.ServiceTaskBusinessObjectBindingEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

		hostedServiceAttributeMock = new Mock<IHostedServiceAttribute>();
		hostedServiceAttributeMock.SetupGet(a => a.Code).Returns("111");
		hostedServiceAttributeMock.SetupGet(a => a.DefaultSchedule).Returns(Mock.Of<IDefaultSchedule>(d => d.RunEvery == "35minutes"));

		hostedServiceAttributeProviderMock
			.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
			.Returns(hostedServiceAttributeMock.Object);

		hostedServiceBusinessObjectBindingsProviderMock
			.SetupGet(p => p.BusinessObjectBindings)
			.Returns(new[] {
				new HostedServiceBusinessObjectBindingAttribute("111", "DummyBizo", Array.Empty<string>(), null),
				new HostedServiceBusinessObjectBindingAttribute("222", "DummyBizo", Array.Empty<string>(), null),
				new HostedServiceBusinessObjectBindingAttribute("333", "DummyBizo", Array.Empty<string>(), null),
				new HostedServiceBusinessObjectBindingAttribute("111", "DummyBizo2", Array.Empty<string>(), null) });

		// Act
		var dto = new NativeServiceTaskDTO("111", taskPk, true, DateTimeOffset.Now, DateTimeOffset.MinValue, branchPk, "BRN", "", "<ScheduleConfig><NextRunTimeCalculatorMinutes Period=\"5\" /></ScheduleConfig>");
		var task = new NativeServiceTask(dto, hostedServiceAttributeProviderMock.Object,
			hostedServiceBusinessObjectBindingsProviderMock.Object, scheduleStatusProviderMock.Object, dateTimeProviderMock.Object);

		// Assert
		AssertEquals(TimeSpan.FromMinutes(35), task.SchedulePeriodDuration);
	}

	protected override void SetUp()
	{
		base.SetUp();
		hostedServiceAttributeProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
		hostedServiceBusinessObjectBindingsProviderMock = new Mock<IHostedServiceBusinessObjectBindingsProvider>();
		scheduleStatusProviderMock = new Mock<IServiceTaskScheduleStatusProvider>();
		dateTimeProviderMock = new Mock<IDateTimeProvider>();
		dateTimeProviderMock.Setup(x => x.CurrentDateTimeUtc).Returns(DateTime.UtcNow);
		hostedServiceAttributeMock = new Mock<IHostedServiceAttribute>();
	}

	readonly Guid taskPk = Guid.NewGuid();
	readonly Guid branchPk = Guid.NewGuid();
	Mock<IClientHostedServiceAttributeProvider> hostedServiceAttributeProviderMock;
	Mock<IHostedServiceBusinessObjectBindingsProvider> hostedServiceBusinessObjectBindingsProviderMock;
	Mock<IServiceTaskScheduleStatusProvider> scheduleStatusProviderMock;
	Mock<IDateTimeProvider> dateTimeProviderMock;
	Mock<IHostedServiceAttribute> hostedServiceAttributeMock;
}
