using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.CW;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Shared.Abstractions;
using IDateTimeProvider = ServiceManager.Shared.Abstractions.IDateTimeProvider;

namespace ServiceManager.Common.CW1.Test.ServiceTask;

[TestedType(typeof(NativeServiceTaskCollectionGovernor))]
class NativeServiceTaskCollectionGovernorTest : TestCaseWithFactory
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
		dateTimeProviderMock.Setup(p => p.CurrentDateTimeUtc).Returns(DateTime.UtcNow);
	}

	public void TestSetActiveNoFilter()
	{
		// Arrange
		using var mock = ObjectFactory.Substitute(attributeProviderMock.Object);
		var tasks = new StmServiceTask[5];
		for (int i = 0; i < tasks.Length; i++)
		{
			tasks[i] = Factory.NewWithValidTestData<StmServiceTask>();
			tasks[i].SST_Active = false;
			tasks[i].SST_Configuration =
				"<ScheduleConfig><NextRunTimeCalculatorHours Period=\"1\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount><ConfigString /></ScheduleConfig>";
		}
		Factory.Save();

		using var adapter = new NativeServiceTaskTransactionAdapter(
			attributeProviderMock.Object,
			statusProviderMock.Object,
			bindingsProviderMock.Object,
			dateTimeProviderMock.Object);
		var collectionGovernor = adapter.GetCollectionGovernorForAllTasks();

		// Act
		collectionGovernor.SetActive(true, Array.Empty<Guid>());

		// Assert
		AssertEquals(
			5,
			collectionGovernor
				.GovernedTasks
				.Count(t =>
				{
					var governor = t.AssignedGovernor as NativeServiceTaskGovernor;
					return governor.EntityMap.ContainsKey("SST_Active") && (bool)governor.EntityMap["SST_Active"].ParameterValue;
				})
		);
	}

	public void TestSetActiveWithFilter()
	{
		// Arrange
		using var mock = ObjectFactory.Substitute(attributeProviderMock.Object);
		var tasks = new StmServiceTask[5];
		for (int i = 0; i < tasks.Length; i++)
		{
			tasks[i] = Factory.NewWithValidTestData<StmServiceTask>();
			tasks[i].SST_Active = false;
			tasks[i].SST_Configuration =
				"<ScheduleConfig><NextRunTimeCalculatorHours Period=\"1\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount><ConfigString /></ScheduleConfig>";
		}
		Factory.Save();

		var filterTaskIndices = new[] { 0, 4 };
		var filterTaskPks = new List<Guid>();

		Array.ForEach(filterTaskIndices, index =>
		{
			filterTaskPks.Add(tasks[index].PK.ToGuid());
		});

		using var adapter = new NativeServiceTaskTransactionAdapter(
			attributeProviderMock.Object,
			statusProviderMock.Object,
			bindingsProviderMock.Object,
			dateTimeProviderMock.Object);
		var collectionGovernor = adapter.GetCollectionGovernorForAllTasks();

		// Act
		collectionGovernor.SetActive(true, filterTaskPks);

		// Assert
		AssertEquals(
			2,
			collectionGovernor
				.GovernedTasks
				.Count(t =>
				{
					var governor = t.AssignedGovernor as NativeServiceTaskGovernor;
					return governor.EntityMap.ContainsKey("SST_Active") && (bool)governor.EntityMap["SST_Active"].ParameterValue;
				})
		);
	}

	public void TestSetBranchPkNoFilter()
	{
		// Arrange
		using var mock = ObjectFactory.Substitute(attributeProviderMock.Object);
		var newCompany = Factory.New<GlbCompany>();
		newCompany.GC_Code = "~TC";
		newCompany.GC_RN_NKCountryCode = "BB";
		var newBranch = newCompany.Branches.AddNew();
		newBranch.GB_Code = "~TB";
		newBranch.GB_RN_NKCountryCode = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, newCompany.GC_RN_NKCountryCode)).RL_RN_NKCountryCode;
		Factory.Save();
		var branchPk = newBranch.PK.ToGuid();

		var tasks = new StmServiceTask[5];
		for (int i = 0; i < tasks.Length; i++)
		{
			tasks[i] = Factory.NewWithValidTestData<StmServiceTask>();
			tasks[i].SST_Configuration =
				"<ScheduleConfig><NextRunTimeCalculatorHours Period=\"1\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount><ConfigString /></ScheduleConfig>";
		}
		Factory.Save();

		using var adapter = new NativeServiceTaskTransactionAdapter(
			attributeProviderMock.Object,
			statusProviderMock.Object,
			bindingsProviderMock.Object,
			dateTimeProviderMock.Object);
		var collectionGovernor = adapter.GetCollectionGovernorForAllTasks();

		// Act
		collectionGovernor.SetBranchPk(branchPk, Array.Empty<Guid>());

		// Assert
		AssertEquals(
			5,
			collectionGovernor
				.GovernedTasks
				.Count(t =>
				{
					var governor = t.AssignedGovernor as NativeServiceTaskGovernor;
					return governor.EntityMap.ContainsKey("SST_GB_Branch") && (Guid)governor.EntityMap["SST_GB_Branch"].ParameterValue == branchPk;
				})
		);
	}

	public void TestSetBranchPkWithFilter()
	{
		// Arrange
		using var mock = ObjectFactory.Substitute(attributeProviderMock.Object);
		var newCompany = Factory.New<GlbCompany>();
		newCompany.GC_Code = "~TC";
		newCompany.GC_RN_NKCountryCode = "BB";
		var newBranch = newCompany.Branches.AddNew();
		newBranch.GB_Code = "~TB";
		newBranch.GB_RN_NKCountryCode = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, newCompany.GC_RN_NKCountryCode)).RL_RN_NKCountryCode;
		Factory.Save();
		var branchPk = newBranch.PK.ToGuid();

		var tasks = new StmServiceTask[5];
		for (int i = 0; i < tasks.Length; i++)
		{
			tasks[i] = Factory.NewWithValidTestData<StmServiceTask>();
			tasks[i].SST_Configuration =
				"<ScheduleConfig><NextRunTimeCalculatorHours Period=\"1\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount><ConfigString /></ScheduleConfig>";
		}
		Factory.Save();

		var filterTaskIndices = new[] { 1, 2, 0 };
		var filterTaskPks = new List<Guid>();

		Array.ForEach(filterTaskIndices, index =>
		{
			filterTaskPks.Add(tasks[index].PK.ToGuid());
		});

		using var adapter = new NativeServiceTaskTransactionAdapter(
			attributeProviderMock.Object,
			statusProviderMock.Object,
			bindingsProviderMock.Object,
			dateTimeProviderMock.Object);
		var collectionGovernor = adapter.GetCollectionGovernorForAllTasks();

		// Act
		collectionGovernor.SetBranchPk(branchPk, filterTaskPks);

		// Assert
		AssertEquals(
			3,
			collectionGovernor
				.GovernedTasks
				.Count(t =>
				{
					var governor = t.AssignedGovernor as NativeServiceTaskGovernor;
					return governor.EntityMap.ContainsKey("SST_GB_Branch") && (Guid)governor.EntityMap["SST_GB_Branch"].ParameterValue == branchPk;
				})
		);
	}

	public void TestReload()
	{
		// Arrange
		using var mock = ObjectFactory.Substitute(attributeProviderMock.Object);
		var tasks = new StmServiceTask[5];
		for (int i = 0; i < tasks.Length; i++)
		{
			tasks[i] = Factory.NewWithValidTestData<StmServiceTask>();
			tasks[i].SST_Active = false;
			tasks[i].SST_Configuration =
				"<ScheduleConfig><NextRunTimeCalculatorHours Period=\"1\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount><ConfigString /></ScheduleConfig>";
		}
		Factory.Save();

		using var adapter = new NativeServiceTaskTransactionAdapter(
			attributeProviderMock.Object,
			statusProviderMock.Object,
			bindingsProviderMock.Object,
			dateTimeProviderMock.Object);
		var collectionGovernor = adapter.GetCollectionGovernorForAllTasks();

		var updatedTaskIndices = new[] { 0, 4 };

		// Act
		Array.ForEach(updatedTaskIndices, index =>
		{
			Db.Connection.ExecuteNonQuery(@"
UPDATE dbo.StmServiceTask
SET SST_Active = 1, SST_SystemLastEditTimeUtc = GetUtcDate(), SST_SystemLastEditUser = '~BP'
WHERE SST_PK = @pk
",
				cmd =>
				{
					cmd.AddParameter("@pk", System.Data.SqlDbType.UniqueIdentifier, tasks[index].PK.ToGuid());
				});
		});

		// Act
		collectionGovernor.Reload();

		// Assert
		AssertEquals(2, collectionGovernor.GovernedTasks.Count(t => t.IsActive));
	}

	public void TestReloadEmptyCollection_DoesNotThrow()
	{
		// Arrange
		using var mock = ObjectFactory.Substitute(attributeProviderMock.Object);

		using var adapter = new NativeServiceTaskTransactionAdapter(
			attributeProviderMock.Object,
			statusProviderMock.Object,
			bindingsProviderMock.Object,
			dateTimeProviderMock.Object);
		var collectionGovernor = adapter.GetCollectionGovernorForAllTasks();

		// Act
		// Assert
		AssertNoExceptionThrown(collectionGovernor.Reload);
		AssertEquals(0, collectionGovernor.GovernedTasks.Count());
	}

	Mock<IClientHostedServiceAttributeProvider> attributeProviderMock;
	Mock<IServiceTaskScheduleStatusProvider> statusProviderMock;
	Mock<IHostedServiceBusinessObjectBindingsProvider> bindingsProviderMock;
	Mock<IDateTimeProvider> dateTimeProviderMock;
}
