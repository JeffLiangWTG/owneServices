using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.ServiceManager.Business;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.CW;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Shared.Abstractions;
using IDateTimeProvider = ServiceManager.Shared.Abstractions.IDateTimeProvider;

namespace ServiceManager.Common.CW1.Test.ServiceTask;

[TestedType(typeof(NativeServiceTaskLoader))]
class NativeServiceTaskLoaderTest : TestCaseWithFactory
{
	protected override void SetUp()
	{
		var defaultScheduleMock = Mock.Of<IDefaultSchedule>(d =>
			d.RunEvery == "15minutes");
		var hostedServiceMock = Mock.Of<IHostedServiceAttribute>(a =>
			a.Description == "some description" &&
			a.Category == "some category" &&
			a.ConfigControlTypeAssemblyName == "A" &&
			a.ConfigControlTypeName == "B" &&
			a.MutuallyExclusiveTaskGroup == MutuallyExclusiveServiceTaskGroups.NoGroup &&
			a.DefaultSchedule == defaultScheduleMock);
		attributeProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
		attributeProviderMock
			.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
			.Returns(hostedServiceMock);
		statusProviderMock = new Mock<IServiceTaskScheduleStatusProvider>();
		bindingsProviderMock = new Mock<IHostedServiceBusinessObjectBindingsProvider>();
		dateTimeProviderMock = new Mock<IDateTimeProvider>();
	}

	public void TestLoadEmptyTask()
	{
		// Arrange

		// Act
		var task = new NativeServiceTaskLoader(attributeProviderMock.Object, statusProviderMock.Object,
			bindingsProviderMock.Object, dateTimeProviderMock.Object).Load("XYZ");

		// Assert
		AssertNull(task);
	}

	public void TestLoadValidTask()
	{
		// Arrange
		using var mock = ObjectFactory.Substitute(attributeProviderMock.Object);
		var schedule = Factory.NewWithValidTestData<StmServiceTask>();
		schedule.SST_ServiceTaskCode = "XYZ";
		schedule.SST_Configuration = "<ScheduleConfig><NextRunTimeCalculatorHours Period=\"1\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount><ConfigString /></ScheduleConfig>";
		schedule.Factory.Save();

		// Act
		var task = new NativeServiceTaskLoader(attributeProviderMock.Object, statusProviderMock.Object,
			bindingsProviderMock.Object, dateTimeProviderMock.Object).Load("XYZ");

		// Assert
		AssertNotNull(task);
		AssertEquals("XYZ", task.Code);
	}

	public void TestLoadAll()
	{
		// Arrange
		using var mock = ObjectFactory.Substitute(attributeProviderMock.Object);
		var schedule1 = Factory.NewWithValidTestData<StmServiceTask>();
		schedule1.SST_ServiceTaskCode = "TS1";
		schedule1.SST_Configuration = "<ScheduleConfig><NextRunTimeCalculatorHours Period=\"1\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount><ConfigString /></ScheduleConfig>";
		schedule1.Factory.Save();

		var schedule2 = Factory.NewWithValidTestData<StmServiceTask>();
		schedule2.SST_ServiceTaskCode = "TS2";
		schedule2.SST_Configuration = "<ScheduleConfig><NextRunTimeCalculatorHours Period=\"1\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount><ConfigString /></ScheduleConfig>";
		schedule2.Factory.Save();

		// Act
		var tasks = new NativeServiceTaskLoader(attributeProviderMock.Object, statusProviderMock.Object,
			bindingsProviderMock.Object, dateTimeProviderMock.Object).LoadAll();

		// Assert
		AssertEquals(2, tasks.Count());
		AssertContainsExactElementsInAnyOrder(new[] { "TS1", "TS2" }, tasks.Select(t => t.Code));
	}

	Mock<IClientHostedServiceAttributeProvider> attributeProviderMock;
	Mock<IServiceTaskScheduleStatusProvider> statusProviderMock;
	Mock<IHostedServiceBusinessObjectBindingsProvider> bindingsProviderMock;
	Mock<IDateTimeProvider> dateTimeProviderMock;
}
