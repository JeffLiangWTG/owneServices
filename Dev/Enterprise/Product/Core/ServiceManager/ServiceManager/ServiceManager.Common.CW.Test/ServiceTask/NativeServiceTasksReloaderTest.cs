using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.ServiceManager.Business;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.CW;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Shared.Abstractions;
using IDateTimeProvider = ServiceManager.Shared.Abstractions.IDateTimeProvider;

namespace ServiceManager.Common.CW1.Test.ServiceTask;

[TestedType(typeof(NativeServiceTasksReloader))]
class NativeServiceTasksReloaderTest : TestCaseWithFactory
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

	public void TestReloadTasks()
	{
		// Arrange
		using var mock = ObjectFactory.Substitute(attributeProviderMock.Object);
		var tasks = new StmServiceTask[5];
		for (int i = 0; i < tasks.Length; i++)
		{
			tasks[i] = Factory.NewWithValidTestData<StmServiceTask>();
			tasks[i].SST_Active = false;
			tasks[i].SST_Configuration = "<ScheduleConfig><NextRunTimeCalculatorHours Period=\"1\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount><ConfigString /></ScheduleConfig>";
		}
		Factory.Save();

		var tasksLoader = new NativeServiceTasksLoader(
			attributeProviderMock.Object,
			statusProviderMock.Object,
			bindingsProviderMock.Object,
			dateTimeProviderMock.Object);

		var initialCollection = tasksLoader.Load();
		AssertEquals(5, initialCollection.GovernedTasks.Count());

		var updatedTime = DateTime.UtcNow.AddMinutes(2);
		var updatedTaskIndices = new[] { 0, 4 };

		// Act
		Array.ForEach(updatedTaskIndices, index =>
		{
			Db.Connection.ExecuteNonQuery(@"
UPDATE dbo.StmServiceTask
SET SST_Active = 1, SST_SystemLastEditTimeUtc = @updatedTime, SST_SystemLastEditUser = '~BP'
WHERE SST_PK = @pk
",
				cmd =>
				{
					cmd.AddParameter("@updatedTime", System.Data.SqlDbType.SmallDateTime, updatedTime);
					cmd.AddParameter("@pk", System.Data.SqlDbType.UniqueIdentifier, tasks[index].PK.ToGuid());
				});
		});

		var updatedCollection = new NativeServiceTasksReloader(
				attributeProviderMock.Object,
				statusProviderMock.Object,
				bindingsProviderMock.Object,
				dateTimeProviderMock.Object)
			.Reload(new DateTimeOffset(updatedTime, TimeSpan.Zero));

		// Assert
		CombineAssertions(() =>
		{
			AssertEquals(updatedTaskIndices.Length, updatedCollection.GovernedTasks.Count());
			Array.ForEach(updatedCollection.GovernedTasks.ToArray(), task =>
			{
				Assert(task.IsActive);
			});
		});
	}

	Mock<IClientHostedServiceAttributeProvider> attributeProviderMock;
	Mock<IServiceTaskScheduleStatusProvider> statusProviderMock;
	Mock<IHostedServiceBusinessObjectBindingsProvider> bindingsProviderMock;
	Mock<IDateTimeProvider> dateTimeProviderMock;
}

