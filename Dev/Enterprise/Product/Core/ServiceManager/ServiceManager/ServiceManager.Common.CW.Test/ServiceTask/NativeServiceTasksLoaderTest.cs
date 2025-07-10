using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ServiceManager.Business;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.CW;
using ServiceManager.Shared.Abstractions;
using IDateTimeProvider = ServiceManager.Shared.Abstractions.IDateTimeProvider;

namespace ServiceManager.Common.CW1.Test.ServiceTask;

[TestedType(typeof(NativeServiceTasksLoader))]
class NativeServiceTasksLoaderTest : TestCaseWithFactory
{
	protected override void SetUp()
	{
		attributeProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
		statusProviderMock = new Mock<IServiceTaskScheduleStatusProvider>();
		bindingsProviderMock = new Mock<IHostedServiceBusinessObjectBindingsProvider>();
		dateTimeProviderMock = new Mock<IDateTimeProvider>();
	}

	public void TestWrongConstructorParams()
	{
		CombineAssertions(() =>
		{
			var result = AssertExceptionThrown<ArgumentNullException>(() => new NativeServiceTasksLoader(null, statusProviderMock.Object, bindingsProviderMock.Object, dateTimeProviderMock.Object));
			AssertEquals("attributeProvider", result.ParamName);

			result = AssertExceptionThrown<ArgumentNullException>(() => new NativeServiceTasksLoader(attributeProviderMock.Object, null, bindingsProviderMock.Object, dateTimeProviderMock.Object));
			AssertEquals("statusProvider", result.ParamName);

			result = AssertExceptionThrown<ArgumentNullException>(() => new NativeServiceTasksLoader(attributeProviderMock.Object, statusProviderMock.Object, null, dateTimeProviderMock.Object));
			AssertEquals("bindingsProvider", result.ParamName);

			result = AssertExceptionThrown<ArgumentNullException>(() => new NativeServiceTasksLoader(attributeProviderMock.Object, statusProviderMock.Object, bindingsProviderMock.Object, null));
			AssertEquals("dateTimeProvider", result.ParamName);
		});
	}

	public void TestLoadWithNoTasks()
	{
		// Arrange

		// Act
		var collectionGovernor = new NativeServiceTasksLoader(
				attributeProviderMock.Object,
				statusProviderMock.Object,
				bindingsProviderMock.Object,
				dateTimeProviderMock.Object)
			.Load();

		// Assert
		AssertEquals(0, collectionGovernor.GovernedTasks.Count());
	}

	public void TestLoadTasks()
	{
		// Arrange
		var schedule1 = Factory.NewWithValidTestData<StmServiceTask>();
		schedule1.SST_ServiceTaskCode = "TS1";
		schedule1.SST_Configuration =
			"<ScheduleConfig><NextRunTimeCalculatorHours Period=\"1\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount><ConfigString /></ScheduleConfig>";
		schedule1.Factory.Save();

		var schedule2 = Factory.NewWithValidTestData<StmServiceTask>();
		schedule2.SST_ServiceTaskCode = "TS2";
		schedule2.SST_Configuration =
			"<ScheduleConfig><NextRunTimeCalculatorHours Period=\"1\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount><ConfigString /></ScheduleConfig>";
		schedule2.Factory.Save();

		Factory.Save();

		// Act
		var collectionGovernor = new NativeServiceTasksLoader(
				attributeProviderMock.Object,
				statusProviderMock.Object,
				bindingsProviderMock.Object,
				dateTimeProviderMock.Object)
			.Load();

		// Assert
		AssertContainsExactElementsInAnyOrder(new[] { "TS1", "TS2" },
			collectionGovernor.GovernedTasks.Select(t => t.Code));
	}

	Mock<IClientHostedServiceAttributeProvider> attributeProviderMock;
	Mock<IServiceTaskScheduleStatusProvider> statusProviderMock;
	Mock<IHostedServiceBusinessObjectBindingsProvider> bindingsProviderMock;
	Mock<IDateTimeProvider> dateTimeProviderMock;
}
