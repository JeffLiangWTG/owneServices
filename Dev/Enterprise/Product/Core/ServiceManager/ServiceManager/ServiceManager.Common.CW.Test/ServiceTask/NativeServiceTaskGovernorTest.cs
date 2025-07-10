using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ServiceManager.Business;
using Enterprise.ServiceManager.Shared;
using Enterprise.ZArchitecture.Schema;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.CW;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Shared.Abstractions;
using IDateTimeProvider = ServiceManager.Shared.Abstractions.IDateTimeProvider;

namespace ServiceManager.Common.CW1.Test.ServiceTask;

class NativeServiceTaskGovernorTest : TestCaseWithFactory
{
	public void TestSetActive()
	{
		// Arrange
		var dto = new NativeServiceTaskDTO("~TS", Guid.NewGuid(), false, DateTimeOffset.Now, DateTimeOffset.MinValue,
			Guid.NewGuid(), "BRN", "", "<ScheduleConfig><NextRunTimeCalculatorHours Period=\"1\" EndTime=\"06:00:00\" /></ScheduleConfig>");
		var governor = new NativeServiceTaskGovernor(dto, hostedServiceAttributeProviderMock.Object, statusProviderMock.Object, bindingsProviderMock.Object, dateTimeProviderMock.Object);

		// Act
		governor.SetActive(true);

		// Assert
		var result = governor.EntityMap["SST_Active"];
		AssertEquals("@IsActive", result.ParameterName);
		AssertEquals(SqlDbType.Bit, result.ParameterType);
		AssertEquals(true, result.ParameterValue);
	}

	public void TestSetBranchPk()
	{
		// Arrange
		var newCompany = Factory.New<GlbCompany>();
		newCompany.GC_Code = "~TC";
		newCompany.GC_RN_NKCountryCode = "BB";
		var newBranch = newCompany.Branches.AddNew();
		newBranch.GB_Code = "~TB";
		newBranch.GB_RN_NKCountryCode = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, newCompany.GC_RN_NKCountryCode)).RL_RN_NKCountryCode;
		Factory.Save();
		var branchPk = newBranch.PK.ToGuid();

		var governor = new NativeServiceTaskGovernor(newTaskDto, hostedServiceAttributeProviderMock.Object, statusProviderMock.Object, bindingsProviderMock.Object, dateTimeProviderMock.Object);

		// Act
		governor.SetBranchPk(branchPk);

		// Assert
		var result = governor.EntityMap["SST_GB_Branch"];
		AssertEquals("@BranchPk", result.ParameterName);
		AssertEquals(SqlDbType.UniqueIdentifier, result.ParameterType);
		AssertEquals(branchPk, result.ParameterValue);
	}

	public void TestSetBranchFromCode()
	{
		// Arrange
		var newCompany = Factory.New<GlbCompany>();
		newCompany.GC_Code = "~TC";
		newCompany.GC_RN_NKCountryCode = "BB";
		var newBranch = newCompany.Branches.AddNew();
		newBranch.GB_Code = "~TB";
		newBranch.GB_RN_NKCountryCode = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, newCompany.GC_RN_NKCountryCode)).RL_RN_NKCountryCode;
		Factory.Save();

		var governor = new NativeServiceTaskGovernor(newTaskDto, hostedServiceAttributeProviderMock.Object, statusProviderMock.Object, bindingsProviderMock.Object, dateTimeProviderMock.Object);

		// Act
		governor.SetBranchFromCode(newBranch.GB_Code);

		// Assert
		var result = governor.EntityMap["SST_GB_Branch"];
		AssertEquals("@BranchPk", result.ParameterName);
		AssertEquals(SqlDbType.UniqueIdentifier, result.ParameterType);
		AssertEquals(newBranch.PK, result.ParameterValue);
	}

	public void TestSetBranchFromCode_NonExistBranchCode()
	{
		// Arrange
		var governor = new NativeServiceTaskGovernor(newTaskDto, hostedServiceAttributeProviderMock.Object, statusProviderMock.Object, bindingsProviderMock.Object, dateTimeProviderMock.Object);

		// Act
		governor.SetBranchFromCode("XXX");

		// Assert
		Assert(!governor.EntityMap.ContainsKey("SST_GB_Branch"));
	}

	public void TestSetStartDate()
	{
		// Arrange
		var governor = new NativeServiceTaskGovernor(newTaskDto, hostedServiceAttributeProviderMock.Object, statusProviderMock.Object, bindingsProviderMock.Object, dateTimeProviderMock.Object);
		var startDate = new DateTimeOffset(2024, 11, 12, 0, 0, 0, TimeSpan.Zero);

		// Act
		governor.SetStartDate(startDate);

		// Assert
		var result = governor.EntityMap["SST_NextRunTime"];
		AssertEquals("@NextRunTime", result.ParameterName);
		AssertEquals(SqlDbType.DateTimeOffset, result.ParameterType);
		AssertEquals(startDate, result.ParameterValue);
	}

	public void TestSetNextRunTime()
	{
		// Arrange
		var governor = new NativeServiceTaskGovernor(newTaskDto, hostedServiceAttributeProviderMock.Object, statusProviderMock.Object, bindingsProviderMock.Object, dateTimeProviderMock.Object);
		var nextRunTime = new DateTimeOffset(2024, 11, 12, 0, 0, 0, TimeSpan.Zero);

		// Act
		governor.SetNextRunTime(nextRunTime);

		// Assert
		var result = governor.EntityMap["SST_NextRunTime"];
		AssertEquals("@NextRunTime", result.ParameterName);
		AssertEquals(SqlDbType.DateTimeOffset, result.ParameterType);
		AssertEquals(nextRunTime, result.ParameterValue);
	}

	public void TestSetLastRunTime()
	{
		// Arrange
		var governor = new NativeServiceTaskGovernor(newTaskDto, hostedServiceAttributeProviderMock.Object, statusProviderMock.Object, bindingsProviderMock.Object, dateTimeProviderMock.Object);
		var lastRunTime = new DateTimeOffset(2024, 11, 12, 0, 0, 0, TimeSpan.Zero);

		// Act
		governor.SetLastRunTime(lastRunTime);

		// Assert
		// Assert
		var result = governor.EntityMap["SST_LastRunTime"];
		AssertEquals("@LastRunTime", result.ParameterName);
		AssertEquals(SqlDbType.DateTimeOffset, result.ParameterType);
		AssertEquals(lastRunTime, result.ParameterValue);
	}

	public void TestSetSchedule()
	{
		// Arrange
		var governor = new NativeServiceTaskGovernor(newTaskDto, hostedServiceAttributeProviderMock.Object, statusProviderMock.Object, bindingsProviderMock.Object, dateTimeProviderMock.Object);

		// Act
		governor.SetSchedule(22, "H");

		// Assert
		var expectedValue = "<ScheduleConfig><NextRunTimeCalculatorHours Period=\"22\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount><ConfigString /></ScheduleConfig>";
		var result = governor.EntityMap["SST_Configuration"];
		AssertEquals("@Configuration", result.ParameterName);
		AssertEquals(SqlDbType.Xml, result.ParameterType);
		AssertEquals(expectedValue, result.ParameterValue);
		AssertNotNull(governor.GovernedTask);
		AssertEquals(string.Empty, governor.GovernedTask.ConfigString);
		AssertEquals(22, governor.GovernedTask.ScheduleFrequency);
		AssertNoExceptionThrown(() => governor.GovernedTask.CalculateNextRunTime(Mock.Of<ILogger>()));
	}

	public void TestSetWeeklySchedule()
	{
		// Arrange
		var governor = new NativeServiceTaskGovernor(newTaskDto, hostedServiceAttributeProviderMock.Object, statusProviderMock.Object, bindingsProviderMock.Object, dateTimeProviderMock.Object);
		var scheduleDaysOfWeek = new[] { DayOfWeek.Wednesday, DayOfWeek.Thursday };

		// Act
		governor.SetWeeklySchedule(3, scheduleDaysOfWeek);

		// Assert
		var expectedValue = "<ScheduleConfig><NextRunTimeCalculatorWeeks Period=\"3\" ScheduledRunTime=\"00:00:00\"><DaysOfOccurrence><DayOfWeek>Wednesday</DayOfWeek><DayOfWeek>Thursday</DayOfWeek></DaysOfOccurrence></NextRunTimeCalculatorWeeks><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount><ConfigString /></ScheduleConfig>";
		var result = governor.EntityMap["SST_Configuration"];
		AssertEquals("@Configuration", result.ParameterName);
		AssertEquals(SqlDbType.Xml, result.ParameterType);
		AssertEquals(expectedValue, result.ParameterValue);
		AssertNotNull(governor.GovernedTask);
		AssertEquals(string.Empty, governor.GovernedTask.ConfigString);
		AssertEquals(3, governor.GovernedTask.ScheduleFrequency);
		AssertNoExceptionThrown(() => governor.GovernedTask.CalculateNextRunTime(Mock.Of<ILogger>()));
	}

	public void TestSetMonthlySchedule()
	{
		// Arrange
		var governor = new NativeServiceTaskGovernor(newTaskDto, hostedServiceAttributeProviderMock.Object, statusProviderMock.Object, bindingsProviderMock.Object, dateTimeProviderMock.Object);

		// Act
		governor.SetMonthlySchedule(3, 15);

		// Assert
		var expectedValue = "<ScheduleConfig><NextRunTimeCalculatorMonthsByDate Period=\"3\" DayOfOccurrence=\"15\" ScheduledRunTime=\"00:00:00\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount><ConfigString /></ScheduleConfig>";
		var result = governor.EntityMap["SST_Configuration"];
		AssertEquals("@Configuration", result.ParameterName);
		AssertEquals(SqlDbType.Xml, result.ParameterType);
		AssertEquals(expectedValue, result.ParameterValue);
		AssertNotNull(governor.GovernedTask);
		AssertEquals(string.Empty, governor.GovernedTask.ConfigString);
		AssertEquals(15, governor.GovernedTask.ScheduleDayOfMonth);
		AssertEquals(3, governor.GovernedTask.ScheduleFrequency);
		AssertNoExceptionThrown(() => governor.GovernedTask.CalculateNextRunTime(Mock.Of<ILogger>()));
	}

	public void TestSetDailyStartTimeLocal_PositiveOffset()
	{
		// Arrange
		var newCompany = Factory.New<GlbCompany>();
		newCompany.GC_Code = "~TC";
		newCompany.GC_RN_NKCountryCode = "BB";
		var newBranch = newCompany.Branches.AddNew();
		newBranch.GB_Code = "~TB";
		newBranch.GB_RN_NKCountryCode = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, newCompany.GC_RN_NKCountryCode)).RL_RN_NKCountryCode;
		newBranch.GB_RL_NKHomePort = "SGSIN"; //UTC+8
		Factory.Save();

		var governor = new NativeServiceTaskGovernor(newTaskDto, hostedServiceAttributeProviderMock.Object, statusProviderMock.Object, bindingsProviderMock.Object, dateTimeProviderMock.Object);
		governor.SetSchedule(1, "H");
		governor.SetBranchPk(newBranch.PK.ToGuid());

		var startTime = TimeSpan.FromHours(17);
		var offSet = TimeSpan.FromSeconds(10);

		// Act
		governor.SetDailyStartTimeLocal(startTime, offSet);

		// Assert
		var expectedValue = "<ScheduleConfig><NextRunTimeCalculatorHours Period=\"1\" StartTime=\"09:00:10\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount><ConfigString /></ScheduleConfig>";
		var result = governor.EntityMap["SST_Configuration"];
		AssertEquals("@Configuration", result.ParameterName);
		AssertEquals(SqlDbType.Xml, result.ParameterType);
		AssertEquals(expectedValue, result.ParameterValue);
	}

	public void TestSetDailyStartTimeLocal_NegativeOffset()
	{
		// Arrange
		var newCompany = Factory.New<GlbCompany>();
		newCompany.GC_Code = "~TC";
		newCompany.GC_RN_NKCountryCode = "BB";
		var newBranch = newCompany.Branches.AddNew();
		newBranch.GB_Code = "~TB";
		newBranch.GB_RN_NKCountryCode = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, newCompany.GC_RN_NKCountryCode)).RL_RN_NKCountryCode;
		newBranch.GB_RL_NKHomePort = "ARBUE"; //UTC-3
		Factory.Save();

		var governor = new NativeServiceTaskGovernor(newTaskDto, hostedServiceAttributeProviderMock.Object, statusProviderMock.Object, bindingsProviderMock.Object, dateTimeProviderMock.Object);
		governor.SetSchedule(1, "H");
		governor.SetBranchPk(newBranch.PK.ToGuid());

		var startTime = TimeSpan.FromHours(17);
		var offSet = TimeSpan.FromSeconds(10);

		// Act
		governor.SetDailyStartTimeLocal(startTime, offSet);

		// Assert
		var expectedValue = "<ScheduleConfig><NextRunTimeCalculatorHours Period=\"1\" StartTime=\"20:00:10\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount><ConfigString /></ScheduleConfig>";
		var result = governor.EntityMap["SST_Configuration"];
		AssertEquals("@Configuration", result.ParameterName);
		AssertEquals(SqlDbType.Xml, result.ParameterType);
		AssertEquals(expectedValue, result.ParameterValue);
	}

	public void TestSetDailyEndTimeLocal_PositiveOffset()
	{
		// Arrange
		var newCompany = Factory.New<GlbCompany>();
		newCompany.GC_Code = "~TC";
		newCompany.GC_RN_NKCountryCode = "BB";
		var newBranch = newCompany.Branches.AddNew();
		newBranch.GB_Code = "~TB";
		newBranch.GB_RN_NKCountryCode = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, newCompany.GC_RN_NKCountryCode)).RL_RN_NKCountryCode;
		newBranch.GB_RL_NKHomePort = "SGSIN";
		Factory.Save();

		var governor = new NativeServiceTaskGovernor(newTaskDto, hostedServiceAttributeProviderMock.Object, statusProviderMock.Object, bindingsProviderMock.Object, dateTimeProviderMock.Object);
		governor.SetSchedule(1, "H");
		governor.SetBranchPk(newBranch.PK.ToGuid());

		var endTime = TimeSpan.FromHours(17);

		// Act
		governor.SetDailyEndTimeLocal(endTime);

		// Assert
		var expectedValue = "<ScheduleConfig><NextRunTimeCalculatorHours Period=\"1\" EndTime=\"09:00:00\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount><ConfigString /></ScheduleConfig>";
		var result = governor.EntityMap["SST_Configuration"];
		AssertEquals("@Configuration", result.ParameterName);
		AssertEquals(SqlDbType.Xml, result.ParameterType);
		AssertEquals(expectedValue, result.ParameterValue);
	}

	public void TestSetDailyEndTimeLocal_NegativeOffset()
	{
		// Arrange
		var newCompany = Factory.New<GlbCompany>();
		newCompany.GC_Code = "~TC";
		newCompany.GC_RN_NKCountryCode = "BB";
		var newBranch = newCompany.Branches.AddNew();
		newBranch.GB_Code = "~TB";
		newBranch.GB_RN_NKCountryCode = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, newCompany.GC_RN_NKCountryCode)).RL_RN_NKCountryCode;
		newBranch.GB_RL_NKHomePort = "ARBUE"; //UTC-3
		Factory.Save();

		var governor = new NativeServiceTaskGovernor(newTaskDto, hostedServiceAttributeProviderMock.Object, statusProviderMock.Object, bindingsProviderMock.Object, dateTimeProviderMock.Object);
		governor.SetSchedule(1, "H");
		governor.SetBranchPk(newBranch.PK.ToGuid());

		var endTime = TimeSpan.FromHours(17);

		// Act
		governor.SetDailyEndTimeLocal(endTime);

		// Assert
		var expectedValue = "<ScheduleConfig><NextRunTimeCalculatorHours Period=\"1\" EndTime=\"20:00:00\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount><ConfigString /></ScheduleConfig>";
		var result = governor.EntityMap["SST_Configuration"];
		AssertEquals("@Configuration", result.ParameterName);
		AssertEquals(SqlDbType.Xml, result.ParameterType);
		AssertEquals(expectedValue, result.ParameterValue);
	}

	public void TestSetDailyStartTimeUtc_CalculatorSeconds()
	{
		// Arrange
		var governor = new NativeServiceTaskGovernor(newTaskDto, hostedServiceAttributeProviderMock.Object, statusProviderMock.Object, bindingsProviderMock.Object, dateTimeProviderMock.Object);
		var startTime = TimeSpan.FromMinutes(360);
		var offSet = TimeSpan.FromMinutes(25);
		governor.SetSchedule(1, "S");

		// Act
		governor.SetDailyStartTimeUtc(startTime, offSet);

		// Assert
		var expectedValue = "<ScheduleConfig><NextRunTimeCalculatorSeconds Period=\"1\" StartTime=\"06:25:00\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount><ConfigString /></ScheduleConfig>";
		var result = governor.EntityMap["SST_Configuration"];
		AssertEquals("@Configuration", result.ParameterName);
		AssertEquals(SqlDbType.Xml, result.ParameterType);
		AssertEquals(expectedValue, result.ParameterValue);
	}

	public void TestSetDailyStartTimeUtc_CalculatorMinutes()
	{
		// Arrange
		var governor = new NativeServiceTaskGovernor(newTaskDto, hostedServiceAttributeProviderMock.Object, statusProviderMock.Object, bindingsProviderMock.Object, dateTimeProviderMock.Object);
		var startTime = TimeSpan.FromMinutes(360);
		var offSet = TimeSpan.FromMinutes(25);
		governor.SetSchedule(1, "T");

		// Act
		governor.SetDailyStartTimeUtc(startTime, offSet);

		// Assert
		var expectedValue = "<ScheduleConfig><NextRunTimeCalculatorMinutes Period=\"1\" StartTime=\"06:25:00\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount><ConfigString /></ScheduleConfig>";
		var result = governor.EntityMap["SST_Configuration"];
		AssertEquals("@Configuration", result.ParameterName);
		AssertEquals(SqlDbType.Xml, result.ParameterType);
		AssertEquals(expectedValue, result.ParameterValue);
	}

	public void TestSetDailyStartTimeUtc_CalculatorHours()
	{
		// Arrange
		var governor = new NativeServiceTaskGovernor(newTaskDto, hostedServiceAttributeProviderMock.Object, statusProviderMock.Object, bindingsProviderMock.Object, dateTimeProviderMock.Object);
		var startTime = TimeSpan.FromMinutes(360);
		var offSet = TimeSpan.FromSeconds(25);
		governor.SetSchedule(1, "H");

		// Act
		governor.SetDailyStartTimeUtc(startTime, offSet);

		// Assert
		var expectedValue = "<ScheduleConfig><NextRunTimeCalculatorHours Period=\"1\" StartTime=\"06:00:25\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount><ConfigString /></ScheduleConfig>";
		var result = governor.EntityMap["SST_Configuration"];
		AssertEquals("@Configuration", result.ParameterName);
		AssertEquals(SqlDbType.Xml, result.ParameterType);
		AssertEquals(expectedValue, result.ParameterValue);
	}

	public void TestSetDailyEndTimeUtc_CalculatorSeconds()
	{
		// Arrange
		var governor = new NativeServiceTaskGovernor(newTaskDto, hostedServiceAttributeProviderMock.Object, statusProviderMock.Object, bindingsProviderMock.Object, dateTimeProviderMock.Object);
		var endTime = TimeSpan.FromMinutes(360);
		governor.SetSchedule(1, "S");

		// Act
		governor.SetDailyEndTimeUtc(endTime);

		// Assert
		var expectedValue = "<ScheduleConfig><NextRunTimeCalculatorSeconds Period=\"1\" EndTime=\"06:00:00\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount><ConfigString /></ScheduleConfig>";
		var result = governor.EntityMap["SST_Configuration"];
		AssertEquals("@Configuration", result.ParameterName);
		AssertEquals(SqlDbType.Xml, result.ParameterType);
		AssertEquals(expectedValue, result.ParameterValue);
	}

	public void TestSetDailyEndTimeUtc_CalculatorMinutes()
	{
		// Arrange
		var governor = new NativeServiceTaskGovernor(newTaskDto, hostedServiceAttributeProviderMock.Object, statusProviderMock.Object, bindingsProviderMock.Object, dateTimeProviderMock.Object);
		var endTime = TimeSpan.FromMinutes(360);
		governor.SetSchedule(1, "T");

		// Act
		governor.SetDailyEndTimeUtc(endTime);

		// Assert
		var expectedValue = "<ScheduleConfig><NextRunTimeCalculatorMinutes Period=\"1\" EndTime=\"06:00:00\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount><ConfigString /></ScheduleConfig>";
		var result = governor.EntityMap["SST_Configuration"];
		AssertEquals("@Configuration", result.ParameterName);
		AssertEquals(SqlDbType.Xml, result.ParameterType);
		AssertEquals(expectedValue, result.ParameterValue);
	}

	public void TestSetDailyEndTimeUtc_CalculatorHours()
	{
		// Arrange
		var governor = new NativeServiceTaskGovernor(newTaskDto, hostedServiceAttributeProviderMock.Object, statusProviderMock.Object, bindingsProviderMock.Object, dateTimeProviderMock.Object);
		var endTime = TimeSpan.FromMinutes(360);
		governor.SetSchedule(1, "H");

		// Act
		governor.SetDailyEndTimeUtc(endTime);

		// Assert
		var expectedValue = "<ScheduleConfig><NextRunTimeCalculatorHours Period=\"1\" EndTime=\"06:00:00\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount><ConfigString /></ScheduleConfig>";
		var result = governor.EntityMap["SST_Configuration"];
		AssertEquals("@Configuration", result.ParameterName);
		AssertEquals(SqlDbType.Xml, result.ParameterType);
		AssertEquals(expectedValue, result.ParameterValue);
	}

	[TestDate(2025, 1, 1, 12, 30, 0)]
	public void TestGetCalculator()
	{
		// Arrange
		var governor = new NativeServiceTaskGovernor(newTaskDto, hostedServiceAttributeProviderMock.Object, statusProviderMock.Object, bindingsProviderMock.Object, dateTimeProviderMock.Object);

		// Act
		var startDate = ZDateTime.UtcNow.AddHours(2).ToNullableDateTimeOffset().Value;
		governor.SetNextRunTime(startDate);
		governor.SetSchedule(15, "T");
		var scheduledRunTime = TimeSpan.FromHours(11);
		governor.SetDailyStartTimeUtc(scheduledRunTime, TimeSpan.Zero);

		// Assert
		CombineAssertions(() =>
		{
			var minutesCalculator = governor.Calculator as NextRunTimeCalculatorMinutes;
			AssertNotNull(minutesCalculator);
			AssertEquals(15, minutesCalculator.Period);
			AssertEquals(scheduledRunTime, minutesCalculator.StartTime.Value);
		});
	}

	public void TestResetScheduleToDefault()
	{
		// Arrange
		var isMandatory = true;
		var hostedServiceConfigMock = Mock.Of<IHostedServiceAttribute>(o =>
			o.IsMandatory == isMandatory && o.DefaultSchedule.RunEvery == "1hour" &&
			o.Description == "dummy description");
		var dto = new NativeServiceTaskDTO("~TS", Guid.NewGuid(), false, DateTimeOffset.Now, DateTimeOffset.MinValue,
			Guid.NewGuid(), "BRN", string.Empty, "<ScheduleConfig><NextRunTimeCalculatorHours Period=\"1\" EndTime=\"06:00:00\" /></ScheduleConfig>");
		hostedServiceAttributeProviderMock
			.Setup(x => x.GetClientHostedServiceAttribute(It.IsAny<string>()))
			.Returns(hostedServiceConfigMock);

		var governor = new NativeServiceTaskGovernor(dto, hostedServiceAttributeProviderMock.Object, statusProviderMock.Object, bindingsProviderMock.Object, dateTimeProviderMock.Object);

		using var mock1 = ObjectFactory.Substitute(hostedServiceAttributeProviderMock.Object);

		// Act
		var result = governor.ResetScheduleToDefault(true, Mock.Of<ILogger>());

		// Assert
		Assert(result);
	}

	public void TestUpdateStatus()
	{
		// Arrange
		statusProviderMock
			.Setup(x => x.GetServiceTaskStatus(It.IsAny<string>()))
			.Returns(new TaskInstanceStatus()
			{
				StatusString = "status",
				PlaceInQueueString = "place in queue",
				ProcessIDsString = "process id",
				RegisteredOnHosts = "registered on hosts",
				RunningCount = 2,
				SecondsInQueueString = "seconds in queue",
				SecondsRunningString = "seconds running",
				NextRunTime = new DateTime(2020, 3, 8)
			});
		var governor = new NativeServiceTaskGovernor(newTaskDto, hostedServiceAttributeProviderMock.Object, statusProviderMock.Object, bindingsProviderMock.Object, dateTimeProviderMock.Object);
		var expected = new TaskInstanceStatus()
		{
			StatusString = "status",
			PlaceInQueueString = "place in queue",
			ProcessIDsString = "process id",
			RegisteredOnHosts = "registered on hosts",
			RunningCount = 2,
			SecondsInQueueString = "seconds in queue",
			SecondsRunningString = "seconds running",
			NextRunTime = new DateTime(2020, 3, 8)
		};

		// Act
		var actual = governor.UpdateStatus();

		// Assert
		AssertEquals(expected.StatusString, actual.StatusString);
	}

	public void TestOnErrorReported()
	{
		// Arrange
		var governor = new NativeServiceTaskGovernor(newTaskDto, hostedServiceAttributeProviderMock.Object, statusProviderMock.Object, bindingsProviderMock.Object, dateTimeProviderMock.Object);

		// Act
		// Assert
		AssertNoExceptionThrown(() =>
		{
			governor.OnErrorReported();
		});
	}

	public void TestSave_Insert()
	{
		// Arrange
		var governor = new NativeServiceTaskGovernor(newTaskDto, hostedServiceAttributeProviderMock.Object, statusProviderMock.Object, bindingsProviderMock.Object, dateTimeProviderMock.Object);

		governor.SetSchedule(1, "H");
		governor.SetNextRunTime(DateTimeOffset.UtcNow);

		// Act
		governor.Save();

		// Assert
		var result =
			Factory.LoadTop1<StmServiceTask>(new ZQuery(StmServiceTaskSchema.SST_ServiceTaskCode, serviceTaskCode));
		AssertNotNull(result);
		AssertEquals("~BP", result.SST_SystemCreateUser);
		AssertEquals("~BP", result.SST_SystemLastEditUser);
		AssertNotNull(result.SST_SystemCreateTimeUtc);
		AssertNotNull(result.SST_SystemLastEditTimeUtc);
	}

	public void TestSave_Insert_ReloadsGovernedTask()
	{
		// Arrange
		var governor = new NativeServiceTaskGovernor(newTaskDto, hostedServiceAttributeProviderMock.Object, statusProviderMock.Object, bindingsProviderMock.Object, dateTimeProviderMock.Object);

		governor.SetSchedule(1, "H");
		governor.SetNextRunTime(DateTimeOffset.UtcNow);

		var oldGovernedTask = governor.GovernedTask;

		// Act
		governor.Save();

		// Assert
		AssertEquals("New service task Pk should be Guid.Empty before it's saved to db", Guid.Empty, oldGovernedTask.Pk);
		AssertNotEquals("Pk should not be Guid.Empty after it's saved to db", Guid.Empty, governor.GovernedTask.Pk);
	}

	public void TestSave_Update()
	{
		// Arrange
		var defaultScheduleMock = Mock.Of<IDefaultSchedule>(d =>
			d.RunEvery == "15minutes");

		var hostedServiceMock = Mock.Of<IHostedServiceAttribute>(a =>
			a.Description == "some description" &&
			a.Category == "some category" &&
			a.ConfigControlTypeAssemblyName == "A" &&
			a.ConfigControlTypeName == "B" &&
			a.MutuallyExclusiveTaskGroup == MutuallyExclusiveServiceTaskGroups.NoGroup &&
			a.DefaultSchedule == defaultScheduleMock);

		hostedServiceAttributeProviderMock
			.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
			.Returns(hostedServiceMock);
		using var mock = ObjectFactory.Substitute(hostedServiceAttributeProviderMock.Object);
		var newCompany = Factory.New<GlbCompany>();
		newCompany.GC_Code = "~TC";
		newCompany.GC_RN_NKCountryCode = "BB";
		var newBranch = newCompany.Branches.AddNew();
		newBranch.GB_Code = "~TB";
		newBranch.GB_RN_NKCountryCode = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, newCompany.GC_RN_NKCountryCode)).RL_RN_NKCountryCode;
		var lastEditTime = new ZDateTime(2024, 11, 26, 0, 0, 0);
		var task = Factory.NewWithValidTestData<StmServiceTask>();
		task.SST_ServiceTaskCode = serviceTaskCode;
		task.SST_Active = false;
		task.SST_GB_Branch = newBranch.PK;
		task.SST_NextRunTime = new ZDateTimeOffset(new ZDateTime(2024, 11, 26, 0, 0, 0), TimeSpan.Zero);
		task.SST_Configuration = "<ScheduleConfig><NextRunTimeCalculatorHours Period=\"1\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount><ConfigString /></ScheduleConfig>";
		task.SST_SystemLastEditTimeUtc = lastEditTime;
		Factory.Save();

		var existingTaskDto = new NativeServiceTaskDTO(
			serviceTaskCode,
			task.PK.ToGuid(),
			task.SST_Active,
			task.SST_NextRunTime.ToDateTimeOffset(),
			null,
			task.SST_GB_Branch.ToGuid(),
			task.Branch.GB_Code,
			string.Empty,
			task.SST_Configuration);

		var governor = new NativeServiceTaskGovernor(existingTaskDto, hostedServiceAttributeProviderMock.Object, statusProviderMock.Object, bindingsProviderMock.Object, dateTimeProviderMock.Object);

		governor.SetActive(true);
		var newNextRunTime = new DateTimeOffset(2024, 11, 27, 0, 0, 0, TimeSpan.Zero);
		governor.SetNextRunTime(newNextRunTime);

		// Act
		governor.Save();

		// Assert
		TestConnection.ExecuteReader(
			"SELECT * FROM dbo.StmServiceTask WHERE SST_ServiceTaskCode = '~T1'",
			(reader) =>
			{
				AssertEquals(true, (bool)reader["SST_Active"]);
				AssertEquals(newNextRunTime, (DateTimeOffset)reader["SST_NextRunTime"]);
				AssertGreaterThan((DateTime)reader["SST_SystemLastEditTimeUtc"], lastEditTime.ToDateTime());
			}
		);
	}

	public void TestReload()
	{
		// Arrange
		var defaultScheduleMock = Mock.Of<IDefaultSchedule>(d =>
			d.RunEvery == "15minutes");

		var hostedServiceMock = Mock.Of<IHostedServiceAttribute>(a =>
			a.Description == "some description" &&
			a.Category == "some category" &&
			a.ConfigControlTypeAssemblyName == "A" &&
			a.ConfigControlTypeName == "B" &&
			a.MutuallyExclusiveTaskGroup == MutuallyExclusiveServiceTaskGroups.NoGroup &&
			a.DefaultSchedule == defaultScheduleMock);

		hostedServiceAttributeProviderMock
			.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
			.Returns(hostedServiceMock);
		using var mock = ObjectFactory.Substitute(hostedServiceAttributeProviderMock.Object);
		var newCompany = Factory.New<GlbCompany>();
		newCompany.GC_Code = "~TC";
		newCompany.GC_RN_NKCountryCode = "BB";
		var newBranch = newCompany.Branches.AddNew();
		newBranch.GB_Code = "~TB";
		newBranch.GB_RN_NKCountryCode = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, newCompany.GC_RN_NKCountryCode)).RL_RN_NKCountryCode;
		var lastEditTime = new ZDateTime(2024, 11, 26, 0, 0, 0);
		var task = Factory.NewWithValidTestData<StmServiceTask>();
		task.SST_ServiceTaskCode = serviceTaskCode;
		task.SST_Active = false;
		task.SST_GB_Branch = newBranch.PK;
		task.SST_NextRunTime = new ZDateTimeOffset(new ZDateTime(2024, 11, 26, 0, 0, 0), TimeSpan.Zero);
		task.SST_Configuration = "<ScheduleConfig><NextRunTimeCalculatorHours Period=\"1\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount><ConfigString /></ScheduleConfig>";
		task.SST_SystemLastEditTimeUtc = lastEditTime;
		Factory.Save();

		var existingTaskDto = new NativeServiceTaskDTO(
			serviceTaskCode,
			task.PK.ToGuid(),
			task.SST_Active,
			task.SST_NextRunTime.ToDateTimeOffset(),
			null,
			task.SST_GB_Branch.ToGuid(),
			task.Branch.GB_Code,
			string.Empty,
			task.SST_Configuration);

		var governor1 = new NativeServiceTaskGovernor(existingTaskDto, hostedServiceAttributeProviderMock.Object, statusProviderMock.Object, bindingsProviderMock.Object, dateTimeProviderMock.Object);
		var oldIsActive = governor1.GovernedTask.IsActive;

		var governor2 = new NativeServiceTaskGovernor(existingTaskDto, hostedServiceAttributeProviderMock.Object, statusProviderMock.Object, bindingsProviderMock.Object, dateTimeProviderMock.Object);

		governor2.SetActive(true);
		governor2.Save();

		// Act
		governor1.Reload();

		// Assert
		AssertEquals(false, oldIsActive);
		AssertEquals(true, governor1.GovernedTask.IsActive);
	}

	[TestDate(2025, 1, 1)]
	public void TestReactivatedMandatoryTaskCalculatesNextRunTime()
	{
		// Arrange
		var time = ZDateTimeOffset.Now.AddDays(-4);
		var newCompany = Factory.NewWithValidTestData<GlbCompany>();
		var newBranch = newCompany.Branches.AddNew();
		var task = Factory.NewWithValidTestData<StmServiceTask>();
		task.SST_ServiceTaskCode = serviceTaskCode;
		task.SST_Active = false;
		task.SST_GB_Branch = newBranch.PK;
		task.SST_NextRunTime = time;
		task.SST_Configuration = "<ScheduleConfig><NextRunTimeCalculatorHours Period=\"1\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount><ConfigString /></ScheduleConfig>";
		Factory.Save();

		var existingTaskDto = new NativeServiceTaskDTO(
			serviceTaskCode,
			task.PK.ToGuid(),
			task.SST_Active,
			task.SST_NextRunTime.ToDateTimeOffset(),
			null,
			task.SST_GB_Branch.ToGuid(),
			task.Branch.GB_Code,
			string.Empty,
			task.SST_Configuration);
		var governor = new NativeServiceTaskGovernor(existingTaskDto, hostedServiceAttributeProviderMock.Object, statusProviderMock.Object, bindingsProviderMock.Object, dateTimeProviderMock.Object);

		// Act
		governor.SetActive(true);
		governor.Save();
		governor.Reload();

		// Assert
		AssertNotEquals(time.ToDateTimeOffset(), governor.GovernedTask.NextRunTime);
	}

	protected override void SetUp()
	{
		base.SetUp();
		hostedServiceAttributeProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
		hostServiceAttributeMock = new Mock<IHostedServiceAttribute>();
		hostServiceAttributeMock.Setup(h => h.Code).Returns(serviceTaskCode);
		newTaskDto = new NativeServiceTaskDTO(hostServiceAttributeMock.Object, null);
		statusProviderMock = new Mock<IServiceTaskScheduleStatusProvider>();
		bindingsProviderMock = new Mock<IHostedServiceBusinessObjectBindingsProvider>();
		dateTimeProviderMock = new Mock<IDateTimeProvider>();
		dateTimeProviderMock.Setup(x => x.CurrentDateTimeUtc).Returns(DateTime.UtcNow);
	}

	NativeServiceTaskDTO newTaskDto;
	Mock<IHostedServiceAttribute> hostServiceAttributeMock;
	Mock<IClientHostedServiceAttributeProvider> hostedServiceAttributeProviderMock;
	Mock<IServiceTaskScheduleStatusProvider> statusProviderMock;
	Mock<IHostedServiceBusinessObjectBindingsProvider> bindingsProviderMock;
	Mock<IDateTimeProvider> dateTimeProviderMock;

	const string serviceTaskCode = "~T1";
}

