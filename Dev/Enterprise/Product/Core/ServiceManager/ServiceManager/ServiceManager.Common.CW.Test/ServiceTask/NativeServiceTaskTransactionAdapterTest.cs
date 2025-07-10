using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ServiceManager.Business;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.CW;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Shared.Abstractions;
using IDateTimeProvider = ServiceManager.Shared.Abstractions.IDateTimeProvider;

namespace ServiceManager.Common.CW1.Test.ServiceTask;

[TestedType(typeof(NativeServiceTaskTransactionAdapter))]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1052:Static holder types should be Static or NotInheritable", Justification = "Test Class")]
public class NativeServiceTaskTransactionAdapterTest : TestCase
{
	public void TestWrongConstructorParams()
	{
		var attributeProviderMock = Mock.Of<IClientHostedServiceAttributeProvider>();
		var statusProviderMock = Mock.Of<IServiceTaskScheduleStatusProvider>();
		var bindingsProviderMock = Mock.Of<IHostedServiceBusinessObjectBindingsProvider>();
		var dateTimeProviderMock = Mock.Of<IDateTimeProvider>();

		CombineAssertions(() =>
		{
			var result = AssertExceptionThrown<ArgumentNullException>(() => new NativeServiceTaskTransactionAdapter(null, statusProviderMock, bindingsProviderMock, dateTimeProviderMock));
			AssertEquals("attributeProvider", result.ParamName);

			result = AssertExceptionThrown<ArgumentNullException>(() => new NativeServiceTaskTransactionAdapter(attributeProviderMock, null, bindingsProviderMock, dateTimeProviderMock));
			AssertEquals("statusProvider", result.ParamName);

			result = AssertExceptionThrown<ArgumentNullException>(() => new NativeServiceTaskTransactionAdapter(attributeProviderMock, statusProviderMock, null, dateTimeProviderMock));
			AssertEquals("bindingsProvider", result.ParamName);

			result = AssertExceptionThrown<ArgumentNullException>(() => new NativeServiceTaskTransactionAdapter(attributeProviderMock, statusProviderMock, bindingsProviderMock, null));
			AssertEquals("dateTimeProvider", result.ParamName);
		});
	}

	class GetTaskGovernorTest : TestCaseWithFactory
	{
		public void TestSuccessfulGet()
		{
			// Arrange
			using var mock = ObjectFactory.Substitute(attributeProviderMock);
			var task = Factory.NewWithValidTestData<StmServiceTask>();
			task.SST_ServiceTaskCode = ServiceTaskCode;
			task.SST_Active = true;
			task.SST_NextRunTime = new ZDateTimeOffset(new ZDateTime(2024, 11, 26, 0, 0, 0), TimeSpan.Zero);
			task.SST_Configuration = "<ScheduleConfig><NextRunTimeCalculatorHours Period=\"1\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount><ConfigString /></ScheduleConfig>";
			Factory.Save();

			IServiceTaskGovernor governor = null;

			// Act
			AssertNoExceptionThrown(() =>
			{
				using var adapter = new NativeServiceTaskTransactionAdapter(attributeProviderMock, statusProviderMock, bindingsProviderMock, dateTimeProviderMock);
				governor = adapter.GetServiceTaskGovernor(task.PK.ToGuid());
			});

			// Assert
			AssertNotNull(governor);
		}

		public void TestInvalidTask()
		{
			// Act
			using var adapter = new NativeServiceTaskTransactionAdapter(attributeProviderMock, statusProviderMock, bindingsProviderMock, dateTimeProviderMock);
			var governor = adapter.GetServiceTaskGovernor(Guid.Empty);

			// Assert
			AssertNull(governor);
		}

		public void TestCreateTaskCanRunInAnyBranch()
		{
			// Arrange
			var taskDescription = "XYZ Description";
			var canRunInAnyBranch = true;
			hostedServiceAttributeMock = Mock.Of<IHostedServiceAttribute>(o =>
				o.Code == ServiceTaskCode &&
				o.DefaultSchedule.RunEvery == "1hour" &&
				o.Description == taskDescription &&
				o.CanRunInAnyBranch == canRunInAnyBranch);

			attributeProviderMock = Mock.Of<IClientHostedServiceAttributeProvider>(p =>
				p.GetClientHostedServiceAttribute(It.IsAny<string>()) == hostedServiceAttributeMock);

			using var adapter = new NativeServiceTaskTransactionAdapter(attributeProviderMock, statusProviderMock, bindingsProviderMock, dateTimeProviderMock);

			// Act
			var governor = adapter.GetNewServiceTaskGovernor(hostedServiceAttributeMock);

			// Assert
			AssertEquals(ServiceTaskCode, governor.GovernedTask.Code);
			AssertEquals(taskDescription, governor.GovernedTask.Description);
			AssertEquals(ZGuid.Empty, governor.GovernedTask.BranchPk);
		}

		public void TestCreateTaskNoDefaultBranch()
		{
			// Arrange
			using var adapter = new NativeServiceTaskTransactionAdapter(attributeProviderMock, statusProviderMock, bindingsProviderMock, dateTimeProviderMock);

			// Act
			var governor = adapter.GetNewServiceTaskGovernor(hostedServiceAttributeMock);

			// Assert
			AssertEquals(ServiceTaskCode, governor.GovernedTask.Code);
			AssertNotNull(governor.GovernedTask.BranchPk);
			AssertType<Guid>(governor.GovernedTask.BranchPk);
		}

		public void TestGetGovernorByCode()
		{
			// Arrange
			using var mock = ObjectFactory.Substitute(attributeProviderMock);
			var task = Factory.NewWithValidTestData<StmServiceTask>();
			task.SST_ServiceTaskCode = ServiceTaskCode;
			task.SST_Active = true;
			task.SST_NextRunTime = new ZDateTimeOffset(new ZDateTime(2024, 11, 26, 0, 0, 0), TimeSpan.Zero);
			task.SST_Configuration = "<ScheduleConfig><NextRunTimeCalculatorHours Period=\"1\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount><ConfigString /></ScheduleConfig>";
			Factory.Save();

			IServiceTaskGovernor governor = null;

			// Act
			AssertNoExceptionThrown(() =>
			{
				using var adapter = new NativeServiceTaskTransactionAdapter(attributeProviderMock, statusProviderMock, bindingsProviderMock, dateTimeProviderMock);
				governor = adapter.GetServiceTaskGovernor(ServiceTaskCode);
			});

			// Assert
			AssertEquals(task.PK.ToGuid(), governor.GovernedTask.Pk);
		}

		protected override void SetUp()
		{
			base.SetUp();
			defaultScheduleMock = Mock.Of<IDefaultSchedule>(d =>
				d.RunEvery == "15minutes");

			hostedServiceAttributeMock = Mock.Of<IHostedServiceAttribute>(a =>
				a.Code == ServiceTaskCode &&
				a.Description == "some description" &&
				a.Category == "some category" &&
				a.ConfigControlTypeAssemblyName == "A" &&
				a.ConfigControlTypeName == "B" &&
				a.MutuallyExclusiveTaskGroup == MutuallyExclusiveServiceTaskGroups.NoGroup &&
				a.DefaultSchedule == defaultScheduleMock);

			attributeProviderMock = Mock.Of<IClientHostedServiceAttributeProvider>(p =>
				p.GetClientHostedServiceAttribute(It.IsAny<string>()) == hostedServiceAttributeMock);

			statusProviderMock = Mock.Of<IServiceTaskScheduleStatusProvider>();
			bindingsProviderMock = Mock.Of<IHostedServiceBusinessObjectBindingsProvider>();
			dateTimeProviderMock = Mock.Of<IDateTimeProvider>();
		}

		IClientHostedServiceAttributeProvider attributeProviderMock;
		IServiceTaskScheduleStatusProvider statusProviderMock;
		IHostedServiceBusinessObjectBindingsProvider bindingsProviderMock;
		IDateTimeProvider dateTimeProviderMock;
		IDefaultSchedule defaultScheduleMock;
		IHostedServiceAttribute hostedServiceAttributeMock;
	}

	class CommitTest : TestCaseWithFactory
	{
		public void TestSuccessfulCommitNewRecord()
		{
			// Arrange
			var taskDescription = "XYZ Description";
			var canRunInAnyBranch = true;
			var commitEventFired = false;
			var hostedServiceConfigMock = Mock.Of<IHostedServiceAttribute>(o =>
				o.Code == ServiceTaskCode &&
				o.DefaultSchedule.RunEvery == "1hour" &&
				o.Description == taskDescription &&
				o.CanRunInAnyBranch == canRunInAnyBranch);
			attributeProviderMock
				.Setup(x => x.GetClientHostedServiceAttribute(It.IsAny<string>()))
				.Returns(hostedServiceConfigMock);

			using var adapter = new NativeServiceTaskTransactionAdapter(attributeProviderMock.Object, statusProviderMock.Object, bindingsProviderMock.Object, dateTimeProviderMock.Object);
			adapter.Committing += delegate
			{ commitEventFired = true; };

			var governor = adapter.GetNewServiceTaskGovernor(hostedServiceConfigMock);

			governor.SetSchedule(1, "H");
			governor.SetNextRunTime(DateTimeOffset.UtcNow);

			// Act
			adapter.Commit();

			// Assert
			TestConnection.ExecuteReader(
				"SELECT * FROM dbo.StmServiceTask WHERE SST_ServiceTaskCode = 'XYZ'",
				(reader) =>
				{
					AssertEquals(ServiceTaskCode, reader["SST_ServiceTaskCode"]);
				}
			);
			Assert(commitEventFired);
		}

		public void TestSuccessfulCommitExistingRecord()
		{
			// Arrange
			using var mock = ObjectFactory.Substitute(attributeProviderMock.Object);
			var commitEventFired = false;
			var task = Factory.NewWithValidTestData<StmServiceTask>();
			task.SST_ServiceTaskCode = ServiceTaskCode;
			task.SST_Active = false;
			task.SST_NextRunTime = new ZDateTimeOffset(new ZDateTime(2024, 11, 26, 0, 0, 0), TimeSpan.Zero);
			task.SST_Configuration = "<ScheduleConfig><NextRunTimeCalculatorHours Period=\"1\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount><ConfigString /></ScheduleConfig>";
			Factory.Save();
			using var adapter = new NativeServiceTaskTransactionAdapter(attributeProviderMock.Object, statusProviderMock.Object, bindingsProviderMock.Object, dateTimeProviderMock.Object);
			adapter.Committing += delegate
			{  commitEventFired = true; };

			var governor = adapter.GetServiceTaskGovernor(task.PK.ToGuid());

			governor.SetActive(true);

			// Act
			adapter.Commit();

			// Assert
			TestConnection.ExecuteReader(
				"SELECT * FROM dbo.StmServiceTask WHERE SST_ServiceTaskCode = 'XYZ'",
				(reader) =>
				{
					AssertEquals(true, reader["SST_Active"]);
				}
			);
			Assert(commitEventFired);
		}

		public void TestCommitWithMultipleGovernors()
		{
			// Arrange
			var newTaskDescription = "New Task Description";
			var canRunInAnyBranch = true;
			var hostedServiceConfigMock = Mock.Of<IHostedServiceAttribute>(o =>
				o.Code == "~T1" &&
				o.DefaultSchedule.RunEvery == "1hour" &&
				o.Description == newTaskDescription &&
				o.CanRunInAnyBranch == canRunInAnyBranch);
			attributeProviderMock
				.Setup(x => x.GetClientHostedServiceAttribute(It.IsAny<string>()))
				.Returns(hostedServiceConfigMock);
			using var mock = ObjectFactory.Substitute(attributeProviderMock.Object);

			var existingTask = Factory.NewWithValidTestData<StmServiceTask>();
			existingTask.SST_ServiceTaskCode = "~T2";
			existingTask.SST_Active = false;
			existingTask.SST_NextRunTime = new ZDateTimeOffset(new ZDateTime(2024, 11, 26, 0, 0, 0), TimeSpan.Zero);
			existingTask.SST_Configuration = "<ScheduleConfig><NextRunTimeCalculatorHours Period=\"1\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount><ConfigString /></ScheduleConfig>";
			Factory.Save();

			using var adapter = new NativeServiceTaskTransactionAdapter(attributeProviderMock.Object, statusProviderMock.Object, bindingsProviderMock.Object, dateTimeProviderMock.Object);

			var time = new DateTimeOffset(new DateTime(2025, 1, 1));
			var governor1 = adapter.GetNewServiceTaskGovernor(hostedServiceConfigMock);
			governor1.SetSchedule(1, "H");
			governor1.SetNextRunTime(time);

			var governor2 = adapter.GetServiceTaskGovernor(existingTask.PK.ToGuid());
			governor2.SetNextRunTime(time);

			// Act
			adapter.Commit();

			// Assert
			var tasks = new List<(string code, DateTimeOffset nextRunTime)>();
			TestConnection.ExecuteReader(
				"SELECT SST_ServiceTaskCode, SST_NextRunTime FROM dbo.StmServiceTask WHERE SST_ServiceTaskCode IN ('~T1', '~T2') ORDER BY SST_ServiceTaskCode",
				(reader) => tasks.Add(((string)reader["SST_ServiceTaskCode"], (DateTimeOffset)reader["SST_NextRunTime"]))
			);
			AssertEquals(2, tasks.Count);
			AssertEquals("~T1", tasks[0].code);
			AssertEquals(time, tasks[0].nextRunTime);
			AssertEquals("~T2", tasks[1].code);
			AssertEquals(time, tasks[1].nextRunTime);
		}

		protected override void SetUp()
		{
			base.SetUp();

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
			dateTimeProviderMock.Setup(p => p.CurrentDateTimeUtc).Returns(DateTime.UtcNow);
		}

		Mock<IClientHostedServiceAttributeProvider> attributeProviderMock;
		Mock<IServiceTaskScheduleStatusProvider> statusProviderMock;
		Mock<IHostedServiceBusinessObjectBindingsProvider> bindingsProviderMock;
		Mock<IDateTimeProvider> dateTimeProviderMock;
	}

	const string ServiceTaskCode = "XYZ";
}

