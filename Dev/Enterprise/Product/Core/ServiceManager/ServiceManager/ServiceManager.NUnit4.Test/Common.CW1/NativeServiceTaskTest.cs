using System;
using System.Collections.Generic;
using CargoWise.Application;
using Enterprise.ServiceManager.Shared;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.CW;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Shared.Abstractions;
using IDateTimeProvider = ServiceManager.Shared.Abstractions.IDateTimeProvider;

namespace ServiceManager.NUnit4.Test.Common.CW1;

class NativeServiceTaskTest
{
	public NativeServiceTaskTest()
	{
		hostedServiceAttributeProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
		hostedServiceBusinessObjectBindingsProviderMock = new Mock<IHostedServiceBusinessObjectBindingsProvider>();
		scheduleStatusProviderMock = new Mock<IServiceTaskScheduleStatusProvider>();
		dateTimeProviderMock = new Mock<IDateTimeProvider>();
		dateTimeProviderMock.Setup(x => x.CurrentDateTimeUtc).Returns(DateTime.UtcNow);
		hostServiceAttributeMock = new Mock<IHostedServiceAttribute>();
	}

	[Test]
	public void TestGetPk()
	{
		// Arrange
		var dto = new NativeServiceTaskDTO("~TS", taskPk, true, DateTimeOffset.Now, DateTimeOffset.MinValue, branchPk,
			"BRN", "", "<ScheduleConfig></ScheduleConfig>");
		var task = new NativeServiceTask(dto, hostedServiceAttributeProviderMock.Object,
			hostedServiceBusinessObjectBindingsProviderMock.Object, scheduleStatusProviderMock.Object, dateTimeProviderMock.Object);

		// Act & Assert
		Assert.That(task.Pk, Is.EqualTo(taskPk));
	}

	[Test]
	public void TestGetIsActive()
	{
		// Arrange
		var dto = new NativeServiceTaskDTO("~TS", taskPk, true, DateTimeOffset.Now, DateTimeOffset.MinValue, branchPk,
			"BRN", "", "<ScheduleConfig></ScheduleConfig>");
		var task = new NativeServiceTask(dto, hostedServiceAttributeProviderMock.Object,
			hostedServiceBusinessObjectBindingsProviderMock.Object, scheduleStatusProviderMock.Object, dateTimeProviderMock.Object);

		// Act & Assert
		Assert.That(task.IsActive, Is.EqualTo(true));
	}

	[Test]
	public void TestGetNextRunTime()
	{
		// Arrange
		var dto = new NativeServiceTaskDTO( "~TS", taskPk, true, DateTimeOffset.MinValue, DateTimeOffset.MinValue,
			branchPk, "BRN", "", "<ScheduleConfig></ScheduleConfig>");
		var task = new NativeServiceTask(dto, hostedServiceAttributeProviderMock.Object,
			hostedServiceBusinessObjectBindingsProviderMock.Object, scheduleStatusProviderMock.Object, dateTimeProviderMock.Object);

		// Act & Assert
		Assert.That(task.NextRunTime, Is.EqualTo(DateTimeOffset.MinValue));
	}

	static readonly IEnumerable<(TimeSpan expected, string xml)> DailyStartTimeCases = new[]
	{
		(TimeSpan.FromHours(9), "<ScheduleConfig><NextRunTimeCalculatorSeconds Period=\"30\" StartTime=\"09:00:00\" EndTime=\"17:00:00\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount></ScheduleConfig>"),
		(TimeSpan.FromHours(9), "<ScheduleConfig><NextRunTimeCalculatorMinutes Period=\"10\" StartTime=\"09:00:00\" EndTime=\"17:00:00\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount></ScheduleConfig>"),
		(TimeSpan.FromHours(9), "<ScheduleConfig><NextRunTimeCalculatorHours Period=\"1\" StartTime=\"09:00:00\" EndTime=\"17:00:00\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount></ScheduleConfig>"),
	};

	[TestCaseSource(nameof(DailyStartTimeCases))]
	public void TestGetDailyStartTime((TimeSpan expected, string xml) tuple)
	{
		// Arrange
		var dto = new NativeServiceTaskDTO("~TS", taskPk, true, DateTimeOffset.Now, DateTimeOffset.MinValue, branchPk, "BRN", "", tuple.xml);
		var task = new NativeServiceTask(dto, hostedServiceAttributeProviderMock.Object,
			hostedServiceBusinessObjectBindingsProviderMock.Object, scheduleStatusProviderMock.Object, dateTimeProviderMock.Object);

		// Act & Assert
		Assert.That(task.DailyStartTime, Is.EqualTo(tuple.expected));
	}

	[Test]
	public void TestGetDailyStartTime_NoStartTimeAttribute()
	{
		// Arrange
		var xml = "<ScheduleConfig><NextRunTimeCalculatorHours Period=\"1\"/><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount></ScheduleConfig>";
		var dto = new NativeServiceTaskDTO("~TS", taskPk, true, DateTimeOffset.Now, DateTimeOffset.MinValue, branchPk, "BRN", "", xml);
		var task = new NativeServiceTask(dto, hostedServiceAttributeProviderMock.Object,
			hostedServiceBusinessObjectBindingsProviderMock.Object, scheduleStatusProviderMock.Object, dateTimeProviderMock.Object);

		// Act & Assert
		Assert.That(task.DailyStartTime, Is.EqualTo(TimeSpan.Zero));
	}

	static readonly IEnumerable<(TimeSpan expected, string xml)> DailyEndTimeCases = new[]
	{
		(TimeSpan.FromHours(17),
			"<ScheduleConfig><NextRunTimeCalculatorSeconds Period=\"30\" StartTime=\"09:00:00\" EndTime=\"17:00:00\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount></ScheduleConfig>"),
		(TimeSpan.FromHours(17),
			"<ScheduleConfig><NextRunTimeCalculatorMinutes Period=\"10\" StartTime=\"09:00:00\" EndTime=\"17:00:00\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount></ScheduleConfig>"),
		(TimeSpan.FromHours(17),
			"<ScheduleConfig><NextRunTimeCalculatorHours Period=\"1\" StartTime=\"09:00:00\" EndTime=\"17:00:00\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount></ScheduleConfig>"),
	};

	[TestCaseSource(nameof(DailyEndTimeCases))]
	public void TestGetDailyEndTime((TimeSpan expected, string xml) tuple)
	{
		// Arrange
		var dto = new NativeServiceTaskDTO("~TS", taskPk, true, DateTimeOffset.Now, DateTimeOffset.MinValue, branchPk, "BRN", "", tuple.xml);
		var task = new NativeServiceTask(dto, hostedServiceAttributeProviderMock.Object,
			hostedServiceBusinessObjectBindingsProviderMock.Object, scheduleStatusProviderMock.Object, dateTimeProviderMock.Object);

		// Act & Assert
		Assert.That(task.DailyEndTime, Is.EqualTo(tuple.expected));
	}

	[Test]
	public void TestGetDailyEndTime_NoEndTimeAttribute()
	{
		// Arrange
		var xml = "<ScheduleConfig><NextRunTimeCalculatorHours Period=\"1\"/><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount></ScheduleConfig>";
		var dto = new NativeServiceTaskDTO("~TS", taskPk, true, DateTimeOffset.Now, DateTimeOffset.MinValue, branchPk, "BRN", "", xml);
		var task = new NativeServiceTask(dto, hostedServiceAttributeProviderMock.Object,
			hostedServiceBusinessObjectBindingsProviderMock.Object, scheduleStatusProviderMock.Object, dateTimeProviderMock.Object);

		// Act & Assert
		Assert.That(task.DailyEndTime, Is.EqualTo(TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59)).Add(TimeSpan.FromSeconds(59))));
	}

	[Test]
	public void TestGetCode()
	{
		// Arrange
		var dto = new NativeServiceTaskDTO("~TS", taskPk, true, DateTimeOffset.Now, DateTimeOffset.MinValue, branchPk, "BRN", "", "<ScheduleConfig></ScheduleConfig>");
		var task = new NativeServiceTask(dto, hostedServiceAttributeProviderMock.Object,
			hostedServiceBusinessObjectBindingsProviderMock.Object, scheduleStatusProviderMock.Object, dateTimeProviderMock.Object);

		// Act & Assert
		Assert.That(task.Code, Is.EqualTo("~TS"));
	}

	[Test]
	public void TestGetDescription()
	{
		// Arrange
		var hostedServiceConfigMock = Mock.Of<IHostedServiceAttribute>(o => o.Description == "Dummy Description");
		var dto = new NativeServiceTaskDTO("~TS", taskPk, true, DateTimeOffset.Now, DateTimeOffset.MinValue, branchPk, "BRN", "", "<ScheduleConfig></ScheduleConfig>");
		hostedServiceAttributeProviderMock
			.Setup(x => x.GetClientHostedServiceAttribute(It.IsAny<string>()))
			.Returns(hostedServiceConfigMock);
		var task = new NativeServiceTask(dto, hostedServiceAttributeProviderMock.Object,
			hostedServiceBusinessObjectBindingsProviderMock.Object, scheduleStatusProviderMock.Object, dateTimeProviderMock.Object);

		// Act & Assert
		Assert.That(task.Description, Is.EqualTo("Dummy Description"));
	}

	[Test]
	public void TestGetMultilingualDescription()
	{
		// Arrange
		var hostedServiceConfigMock = Mock.Of<IHostedServiceAttribute>(o => o.Description == "Dummy Description");
		var dto = new NativeServiceTaskDTO("~TS", taskPk, true, DateTimeOffset.Now, DateTimeOffset.MinValue, branchPk, "BRN", "", "<ScheduleConfig></ScheduleConfig>");
		hostedServiceAttributeProviderMock
			.Setup(x => x.GetClientHostedServiceAttribute(It.IsAny<string>()))
			.Returns(hostedServiceConfigMock);
		var task = new NativeServiceTask(dto, hostedServiceAttributeProviderMock.Object,
			hostedServiceBusinessObjectBindingsProviderMock.Object, scheduleStatusProviderMock.Object, dateTimeProviderMock.Object);

		// Act & Assert
		Assert.That(task.MultilingualDescription, Is.EqualTo("Dummy Description"));
	}

	[Test]
	public void TestGetBranchPk()
	{
		// Arrange
		var dto = new NativeServiceTaskDTO("~TS", taskPk, true, DateTimeOffset.Now, DateTimeOffset.MinValue, branchPk, "BRN", "", "<ScheduleConfig></ScheduleConfig>");
		var task = new NativeServiceTask(dto, hostedServiceAttributeProviderMock.Object,
			hostedServiceBusinessObjectBindingsProviderMock.Object, scheduleStatusProviderMock.Object, dateTimeProviderMock.Object);

		// Act & Assert
		Assert.That(task.BranchPk, Is.EqualTo(branchPk));
	}

	[Test]
	public void TestGetBranchName()
	{
		// Arrange
		var dto = new NativeServiceTaskDTO("~TS", taskPk, true, DateTimeOffset.Now, DateTimeOffset.MinValue, branchPk, "BRN", "", "<ScheduleConfig></ScheduleConfig>");
		var task = new NativeServiceTask(dto, hostedServiceAttributeProviderMock.Object,
			hostedServiceBusinessObjectBindingsProviderMock.Object, scheduleStatusProviderMock.Object, dateTimeProviderMock.Object);

		// Act & Assert
		Assert.That(task.BranchName, Is.EqualTo("BRN"));
	}

	[Test]
	public void TestGetBranchErrorMessage()
	{
		// Arrange
		var dto = new NativeServiceTaskDTO("~TS", taskPk, true, DateTimeOffset.Now, DateTimeOffset.MinValue, branchPk, "BRN", "dummy branch error msg", "<ScheduleConfig></ScheduleConfig>");
		var task = new NativeServiceTask(dto, hostedServiceAttributeProviderMock.Object,
			hostedServiceBusinessObjectBindingsProviderMock.Object, scheduleStatusProviderMock.Object, dateTimeProviderMock.Object);

		// Act & Assert
		Assert.That(task.BranchErrorMessage, Is.EqualTo("dummy branch error msg"));
	}

	[Test]
	public void TestGetConfigString()
	{
		// Arrange
		var dto = new NativeServiceTaskDTO("~TS", taskPk, true, DateTimeOffset.Now, DateTimeOffset.MinValue, branchPk, "BRN", "", "<ScheduleConfig><ConfigString>TestConfigString</ConfigString></ScheduleConfig>");
		var task = new NativeServiceTask(dto, hostedServiceAttributeProviderMock.Object,
			hostedServiceBusinessObjectBindingsProviderMock.Object, scheduleStatusProviderMock.Object, dateTimeProviderMock.Object);

		// Act & Assert
		Assert.That(task.ConfigString, Is.EqualTo("TestConfigString"));
	}

	[Test]
	public void TestConfigStringIsEmptyWhenXmlNotContainsConfigString()
	{
		// Arrange
		var dto = new NativeServiceTaskDTO("~TS", taskPk, true, DateTimeOffset.Now, DateTimeOffset.MinValue, branchPk, "BRN", "", "<ScheduleConfig></ScheduleConfig>");
		var task = new NativeServiceTask(dto, hostedServiceAttributeProviderMock.Object,
			hostedServiceBusinessObjectBindingsProviderMock.Object, scheduleStatusProviderMock.Object, dateTimeProviderMock.Object);

		// Act & Assert
		Assert.That(task.ConfigString, Is.EqualTo(string.Empty));
	}

	[Test]
	public void TestGetSettingsXml()
	{
		// Arrange
		var dto = new NativeServiceTaskDTO("~TS", taskPk, true, DateTimeOffset.Now, DateTimeOffset.MinValue, branchPk, "BRN", "", "<ScheduleConfig></ScheduleConfig>");
		var task = new NativeServiceTask(dto, hostedServiceAttributeProviderMock.Object,
			hostedServiceBusinessObjectBindingsProviderMock.Object, scheduleStatusProviderMock.Object, dateTimeProviderMock.Object);

		// Act & Assert
		Assert.That(task.SettingsXml, Is.EqualTo("<ScheduleConfig></ScheduleConfig>"));
	}

	[Test]
	public void TestGetSecondaryProcessesMaxCount()
	{
		// Arrange
		var hostedServiceConfigMock = Mock.Of<IHostedServiceAttribute>(o =>
			o.Description == "Dummy Description" && o.Category == "TST" && o.DefaultSchedule.RunEvery == "1minute");
		hostedServiceAttributeProviderMock
			.Setup(x => x.GetClientHostedServiceAttribute(It.IsAny<string>()))
			.Returns(hostedServiceConfigMock);
		scheduleStatusProviderMock
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
		var dto = new NativeServiceTaskDTO("~TS", taskPk, true, DateTimeOffset.Now, DateTimeOffset.MinValue, branchPk, "BRN", "", "<ScheduleConfig><SecondaryProcessesMaxCount>5</SecondaryProcessesMaxCount></ScheduleConfig>");
		var task = new NativeServiceTask(dto, hostedServiceAttributeProviderMock.Object,
			hostedServiceBusinessObjectBindingsProviderMock.Object, scheduleStatusProviderMock.Object, dateTimeProviderMock.Object);

		// Act & Assert
		Assert.That(task.SecondaryProcessesMaxCount, Is.EqualTo(5));
	}

	[Test]
	public void TestGetSecondaryProcessesMaxCountIsZeroWhenXmlNotContainsIt()
	{
		// Arrange
		var hostedServiceConfigMock = Mock.Of<IHostedServiceAttribute>(o =>
			o.Description == "Dummy Description" && o.Category == "TST" && o.DefaultSchedule.RunEvery == "1minute");
		hostedServiceAttributeProviderMock
			.Setup(x => x.GetClientHostedServiceAttribute(It.IsAny<string>()))
			.Returns(hostedServiceConfigMock);
		scheduleStatusProviderMock
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
		var dto = new NativeServiceTaskDTO("~TS", taskPk, true, DateTimeOffset.Now, DateTimeOffset.MinValue, branchPk, "BRN", "", "<ScheduleConfig></ScheduleConfig>");
		var task = new NativeServiceTask(dto, hostedServiceAttributeProviderMock.Object,
			hostedServiceBusinessObjectBindingsProviderMock.Object, scheduleStatusProviderMock.Object, dateTimeProviderMock.Object);

		// Act & Assert
		Assert.That(task.SecondaryProcessesMaxCount, Is.EqualTo(0));
	}

	[Test]
	public void TestGetScheduleDaysOfWeek()
	{
		// Arrange
		var xml = "<ScheduleConfig><NextRunTimeCalculatorWeeks Period=\"1\" ScheduledRunTime=\"00:00:00\"><DaysOfOccurrence><DayOfWeek>Sunday</DayOfWeek><DayOfWeek>Monday</DayOfWeek></DaysOfOccurrence></NextRunTimeCalculatorWeeks></ScheduleConfig>";
		var dto = new NativeServiceTaskDTO("~TS", taskPk, true, DateTimeOffset.Now, DateTimeOffset.MinValue, branchPk, "BRN", "", xml);
		var task = new NativeServiceTask(dto, hostedServiceAttributeProviderMock.Object,
			hostedServiceBusinessObjectBindingsProviderMock.Object, scheduleStatusProviderMock.Object, dateTimeProviderMock.Object);

		// Act & Assert
		Assert.That(task.ScheduleDaysOfWeek, Is.EquivalentTo(new DayOfWeek[] { DayOfWeek.Sunday, DayOfWeek.Monday }));
	}

	[Test]
	public void TestGetScheduleDaysOfWeek_NotCalculatorWeeks()
	{
		// Arrange
		var xml = "<ScheduleConfig><NextRunTimeCalculatorHours Period=\"1\"/></ScheduleConfig>";
		var dto = new NativeServiceTaskDTO("~TS", taskPk, true, DateTimeOffset.Now, DateTimeOffset.MinValue, branchPk, "BRN", "", xml);
		var task = new NativeServiceTask(dto, hostedServiceAttributeProviderMock.Object,
			hostedServiceBusinessObjectBindingsProviderMock.Object, scheduleStatusProviderMock.Object, dateTimeProviderMock.Object);

		// Act & Assert
		Assert.That(task.ScheduleDaysOfWeek, Is.EquivalentTo(Array.Empty<DayOfWeek>()));
	}

	[Test]
	public void TestGetScheduleDayOfMonth_CalculatorMonthsByDate()
	{
		// Arrange
		var xml = "<ScheduleConfig><NextRunTimeCalculatorMonthsByDate Period=\"1\" DayOfOccurrence=\"20\" ScheduledRunTime=\"00:00:00\" /></ScheduleConfig>";
		var dto = new NativeServiceTaskDTO("~TS", taskPk, true, DateTimeOffset.Now, DateTimeOffset.MinValue, branchPk, "BRN", "", xml);
		var task = new NativeServiceTask(dto, hostedServiceAttributeProviderMock.Object,
			hostedServiceBusinessObjectBindingsProviderMock.Object, scheduleStatusProviderMock.Object, dateTimeProviderMock.Object);

		// Act & Assert
		Assert.That(task.ScheduleDayOfMonth, Is.EqualTo(20));
	}

	[Test]
	public void TestGetScheduleDayOfMonth_NotCalculatorMonthsByDate()
	{
		// Arrange
		var xml = "<ScheduleConfig><NextRunTimeCalculatorDays Period=\"2\" /></ScheduleConfig>";
		var dto = new NativeServiceTaskDTO("~TS", taskPk, true, DateTimeOffset.Now, DateTimeOffset.MinValue, branchPk, "BRN", "", xml);
		var task = new NativeServiceTask(dto, hostedServiceAttributeProviderMock.Object,
			hostedServiceBusinessObjectBindingsProviderMock.Object, scheduleStatusProviderMock.Object, dateTimeProviderMock.Object);

		// Act & Assert
		Assert.That(task.ScheduleDayOfMonth, Is.EqualTo(1));
	}

	static readonly IEnumerable<(int expected, string xml)> ScheduleDayNumberCases = new[]
	{
		(0, "<ScheduleConfig><NextRunTimeCalculatorHours Period=\"2\" /></ScheduleConfig>"),
		(0, "<ScheduleConfig><NextRunTimeCalculatorMonthsByDate Period=\"3\" DayOfOccurrence=\"2\" ScheduledRunTime=\"00:00:00\" /></ScheduleConfig>"),
		(3, "<ScheduleConfig><NextRunTimeCalculatorMonthsByDayOfWeek WeekOfTheMonth=\"2\" DayOfTheWeek=\"Tuesday\" ScheduledRunTime=\"00:00:00\" /></ScheduleConfig>"),
		(0, "<ScheduleConfig><NextRunTimeCalculatorMonthsByLastDay Period=\"24\" ScheduledRunTime=\"00:00:00\" /></ScheduleConfig>"),
		(0, "<ScheduleConfig><NextRunTimeCalculatorYearsByDate Month=\"7\" Day=\"1\" ScheduledRunTime=\"00:00:00\" /></ScheduleConfig>"),
		(6, "<ScheduleConfig><NextRunTimeCalculatorYearsByDayOfMonth WeekOfTheMonth=\"2\" DayOfTheWeek=\"Friday\" MonthOfTheYear=\"7\" ScheduledRunTime=\"00:00:00\" /></ScheduleConfig>"),
		(0, "<ScheduleConfig></ScheduleConfig>"),
	};

	[TestCaseSource(nameof(ScheduleDayNumberCases))]
	public void TestGetScheduleDayNumber((int expected, string xml) tuple)
	{
		// Arrange
		var dto = new NativeServiceTaskDTO("~TS", taskPk, true, DateTimeOffset.Now, DateTimeOffset.MinValue, branchPk, "BRN", "", tuple.xml);
		var task = new NativeServiceTask(dto, hostedServiceAttributeProviderMock.Object,
			hostedServiceBusinessObjectBindingsProviderMock.Object, scheduleStatusProviderMock.Object, dateTimeProviderMock.Object);

		// Act & Assert
		Assert.That(task.ScheduleDayNumber, Is.EqualTo(tuple.expected));
	}

	static readonly IEnumerable<(int expected, string xml)> ScheduleMonthNumberCases = new[]
	{
		(1, "<ScheduleConfig><NextRunTimeCalculatorHours Period=\"2\" /></ScheduleConfig>"),
		(1, "<ScheduleConfig><NextRunTimeCalculatorMonthsByDate Period=\"3\" DayOfOccurrence=\"2\" ScheduledRunTime=\"00:00:00\" /></ScheduleConfig>"),
		(1, "<ScheduleConfig><NextRunTimeCalculatorMonthsByDayOfWeek WeekOfTheMonth=\"2\" DayOfTheWeek=\"Tuesday\" ScheduledRunTime=\"00:00:00\" /></ScheduleConfig>"),
		(1, "<ScheduleConfig><NextRunTimeCalculatorMonthsByLastDay Period=\"24\" ScheduledRunTime=\"00:00:00\" /></ScheduleConfig>"),
		(7, "<ScheduleConfig><NextRunTimeCalculatorYearsByDate Month=\"7\" Day=\"1\" ScheduledRunTime=\"00:00:00\" /></ScheduleConfig>"),
		(7, "<ScheduleConfig><NextRunTimeCalculatorYearsByDayOfMonth WeekOfTheMonth=\"2\" DayOfTheWeek=\"Friday\" MonthOfTheYear=\"7\" ScheduledRunTime=\"00:00:00\" /></ScheduleConfig>"),
		(1, "<ScheduleConfig></ScheduleConfig>"),
	};

	[TestCaseSource(nameof(ScheduleMonthNumberCases))]
	public void TestGetScheduleMonthNumber((int expected, string xml) tuple)
	{
		// Arrange
		var dto = new NativeServiceTaskDTO("~TS", taskPk, true, DateTimeOffset.Now, DateTimeOffset.MinValue, branchPk, "BRN", "", tuple.xml);
		var task = new NativeServiceTask(dto, hostedServiceAttributeProviderMock.Object,
			hostedServiceBusinessObjectBindingsProviderMock.Object, scheduleStatusProviderMock.Object, dateTimeProviderMock.Object);

		// Act & Assert
		Assert.That(task.ScheduleMonth, Is.EqualTo(tuple.expected));
	}

	[Test]
	public void TestGetScheduleStartDate()
	{
		// Arrange
		var nextRunTime = new DateTimeOffset(2024, 11, 15, 0, 0, 0, TimeSpan.Zero);
		var dto = new NativeServiceTaskDTO("~TS", taskPk, true, nextRunTime, DateTimeOffset.MinValue, branchPk, "BRN", "", "<ScheduleConfig></ScheduleConfig>");
		var task = new NativeServiceTask(dto, hostedServiceAttributeProviderMock.Object,
			hostedServiceBusinessObjectBindingsProviderMock.Object, scheduleStatusProviderMock.Object, dateTimeProviderMock.Object);

		// Act & Assert
		Assert.That(task.ScheduleStartDate, Is.EqualTo(nextRunTime));
	}

	static readonly IEnumerable<(string expecxted, string xml)> ScheduleOccurrenceCases = new[]
	{
		("S", "<ScheduleConfig><NextRunTimeCalculatorSeconds Period=\"30\" /></ScheduleConfig>"),
		("T", "<ScheduleConfig><NextRunTimeCalculatorMinutes Period=\"30\" /></ScheduleConfig>"),
		("H", "<ScheduleConfig><NextRunTimeCalculatorHours Period=\"2\" /></ScheduleConfig>"),
		("D", "<ScheduleConfig><NextRunTimeCalculatorDays Period=\"2\" /></ScheduleConfig>"),
		("D", "<ScheduleConfig><NextRunTimeCalculatorWorkingDays ScheduledRunTime=\"00:00:00\" /></ScheduleConfig>"),
		("W", "<ScheduleConfig><NextRunTimeCalculatorWeeks Period=\"1\" ScheduledRunTime=\"00:00:00\"><DaysOfOccurrence><DayOfWeek>Monday</DayOfWeek></DaysOfOccurrence></NextRunTimeCalculatorWeeks></ScheduleConfig>"),
		("M", "<ScheduleConfig><NextRunTimeCalculatorMonthsByDate Period=\"2\" DayOfOccurrence=\"2\" ScheduledRunTime=\"00:00:00\" /></ScheduleConfig>"),
		("M", "<ScheduleConfig><NextRunTimeCalculatorMonthsByDayOfWeek WeekOfTheMonth=\"2\" DayOfTheWeek=\"Tuesday\" ScheduledRunTime=\"00:00:00\" /></ScheduleConfig>"),
		("M", "<ScheduleConfig><NextRunTimeCalculatorMonthsByLastDay Period=\"1\" ScheduledRunTime=\"00:00:00\" /></ScheduleConfig>"),
		("Y", "<ScheduleConfig><NextRunTimeCalculatorYearsByDate Month=\"7\" Day=\"1\" ScheduledRunTime=\"00:00:00\" /></ScheduleConfig>"),
		("Y", "<ScheduleConfig><NextRunTimeCalculatorYearsByDayOfMonth WeekOfTheMonth=\"2\" DayOfTheWeek=\"Friday\" MonthOfTheYear=\"7\" ScheduledRunTime=\"00:00:00\" /></ScheduleConfig>"),
		(string.Empty, "<ScheduleConfig></ScheduleConfig>"),
	};

	[TestCaseSource(nameof(ScheduleOccurrenceCases))]
	public void TestGetScheduleOccurrence((string expecxted, string xml) tuple)
	{
		// Arrange
		var dto = new NativeServiceTaskDTO("~TS", taskPk, true, DateTimeOffset.Now, DateTimeOffset.MinValue, branchPk, "BRN", "", tuple.xml);
		var task = new NativeServiceTask(dto, hostedServiceAttributeProviderMock.Object,
			hostedServiceBusinessObjectBindingsProviderMock.Object, scheduleStatusProviderMock.Object, dateTimeProviderMock.Object);

		// Act & Assert
		Assert.That(task.ScheduleOccurrence, Is.EqualTo(tuple.expecxted));
	}

	static readonly IEnumerable<(int expected, string xml)> ScheduleFrequencyCases = new[]
	{
		(30, "<ScheduleConfig><NextRunTimeCalculatorSeconds Period=\"30\" /></ScheduleConfig>"),
		(45, "<ScheduleConfig><NextRunTimeCalculatorMinutes Period=\"45\" /></ScheduleConfig>"),
		(5, "<ScheduleConfig><NextRunTimeCalculatorDays Period=\"5\" /></ScheduleConfig>"),
		(1, "<ScheduleConfig><NextRunTimeCalculatorWorkingDays ScheduledRunTime=\"00:00:00\" /></ScheduleConfig>"),
		(1, "<ScheduleConfig><NextRunTimeCalculatorWeeks Period=\"1\" ScheduledRunTime=\"00:00:00\"><DaysOfOccurrence><DayOfWeek>Monday</DayOfWeek></DaysOfOccurrence></NextRunTimeCalculatorWeeks></ScheduleConfig>"),
		(3, "<ScheduleConfig><NextRunTimeCalculatorMonthsByDate Period=\"3\" DayOfOccurrence=\"2\" ScheduledRunTime=\"00:00:00\" /></ScheduleConfig>"),
		(1, "<ScheduleConfig><NextRunTimeCalculatorMonthsByDayOfWeek WeekOfTheMonth=\"2\" DayOfTheWeek=\"Tuesday\" ScheduledRunTime=\"00:00:00\" /></ScheduleConfig>"),
		(24, "<ScheduleConfig><NextRunTimeCalculatorMonthsByLastDay Period=\"24\" ScheduledRunTime=\"00:00:00\" /></ScheduleConfig>"),
		(1, "<ScheduleConfig><NextRunTimeCalculatorYearsByDate Month=\"7\" Day=\"1\" ScheduledRunTime=\"00:00:00\" /></ScheduleConfig>"),
		(1, "<ScheduleConfig><NextRunTimeCalculatorYearsByDayOfMonth WeekOfTheMonth=\"2\" DayOfTheWeek=\"Friday\" MonthOfTheYear=\"7\" ScheduledRunTime=\"00:00:00\" /></ScheduleConfig>"),
		(1, "<ScheduleConfig></ScheduleConfig>"),
	};

	[TestCaseSource(nameof(ScheduleFrequencyCases))]
	public void TestGetScheduleFrequency((int expected, string xml) tuple)
	{
		// Arrange
		var dto = new NativeServiceTaskDTO("~TS", taskPk, true, DateTimeOffset.Now, DateTimeOffset.MinValue, branchPk, "BRN", "", tuple.xml);
		var task = new NativeServiceTask(dto, hostedServiceAttributeProviderMock.Object,
			hostedServiceBusinessObjectBindingsProviderMock.Object, scheduleStatusProviderMock.Object, dateTimeProviderMock.Object);

		// Act & Assert
		Assert.That(task.ScheduleFrequency, Is.EqualTo(tuple.expected));
	}

	[Test]
	public void TestGetWeekDaysOnly_CalculatorWorkingDays()
	{
		// Arrange
		var xml = "<ScheduleConfig><NextRunTimeCalculatorWorkingDays ScheduledRunTime=\"03:00:00\" /></ScheduleConfig>";
		var dto = new NativeServiceTaskDTO("~TS", taskPk, true, DateTimeOffset.Now, DateTimeOffset.MinValue, branchPk, "BRN", "", xml);
		var task = new NativeServiceTask(dto, hostedServiceAttributeProviderMock.Object,
			hostedServiceBusinessObjectBindingsProviderMock.Object, scheduleStatusProviderMock.Object, dateTimeProviderMock.Object);

		// Act & Assert
		Assert.That(task.WeekDaysOnly, Is.EqualTo(true));
	}

	[Test]
	public void TestGetWeekDaysOnly_NotCalculatorWorkingDays()
	{
		// Arrange
		var xml = "<ScheduleConfig><NextRunTimeCalculatorMonthsByDate Period=\"1\" DayOfOccurrence=\"20\" ScheduledRunTime=\"00:00:00\" /></ScheduleConfig>";
		var dto = new NativeServiceTaskDTO("~TS", taskPk, true, DateTimeOffset.Now, DateTimeOffset.MinValue, branchPk, "BRN", "", xml);
		var task = new NativeServiceTask(dto, hostedServiceAttributeProviderMock.Object,
			hostedServiceBusinessObjectBindingsProviderMock.Object, scheduleStatusProviderMock.Object, dateTimeProviderMock.Object);

		// Act & Assert
		Assert.That(task.WeekDaysOnly, Is.EqualTo(false));
	}

	[Test]
	public void TestGetCategory()
	{
		// Arrange
		const string category = "CAT";
		var hostedServiceConfigMock = Mock.Of<IHostedServiceAttribute>(o => o.Category == category);
		hostedServiceAttributeProviderMock
			.Setup(x => x.GetClientHostedServiceAttribute(It.IsAny<string>()))
			.Returns(hostedServiceConfigMock);

		var dto = new NativeServiceTaskDTO("~TS", taskPk, true, DateTimeOffset.Now, DateTimeOffset.MinValue, branchPk, "BRN", "", "<ScheduleConfig></ScheduleConfig>");
		var task = new NativeServiceTask(dto, hostedServiceAttributeProviderMock.Object,
			hostedServiceBusinessObjectBindingsProviderMock.Object, scheduleStatusProviderMock.Object, dateTimeProviderMock.Object);

		// Act & Assert
		Assert.That(task.Category, Is.EqualTo(category));
	}

	[Test]
	public void TestGetSchedulePeriodDuration_NonNudgeableTask()
	{
		// Arrange
		var hostedServiceAttributeMock = Mock.Of<IHostedServiceAttribute>(a =>
			a.Code == "111" &&
			a.DefaultSchedule == Mock.Of<IDefaultSchedule>(d => d.RunEvery == "5minutes"));

		hostedServiceAttributeProviderMock
			.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
			.Returns(hostedServiceAttributeMock);

		// Act
		var dto = new NativeServiceTaskDTO("111", taskPk, true, DateTimeOffset.Now, DateTimeOffset.MinValue, branchPk, "BRN", "", "<ScheduleConfig><NextRunTimeCalculatorMinutes Period=\"5\" /></ScheduleConfig>");
		var task = new NativeServiceTask(dto, hostedServiceAttributeProviderMock.Object,
			hostedServiceBusinessObjectBindingsProviderMock.Object, scheduleStatusProviderMock.Object, dateTimeProviderMock.Object);

		// Assert
		Assert.That(task.SchedulePeriodDuration, Is.EqualTo(TimeSpan.FromMinutes(5)));
	}

	[Test]
	public void TestGetOverdueDuration()
	{
		// Arrange
		var hostedServiceAttributeMock = Mock.Of<IHostedServiceAttribute>(a =>
			a.Code == "111" &&
			a.DefaultSchedule == Mock.Of<IDefaultSchedule>(d => d.RunEvery == "5minutes" && d.DoNotRunTillNextDueTimeIfOverdue == "30minutes"));

		hostedServiceAttributeProviderMock
			.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
			.Returns(hostedServiceAttributeMock);

		// Act
		var dto = new NativeServiceTaskDTO("111", taskPk, true, DateTimeOffset.Now, DateTimeOffset.MinValue, branchPk, "BRN", "", "<ScheduleConfig><NextRunTimeCalculatorMinutes Period=\"5\" /></ScheduleConfig>");
		var task = new NativeServiceTask(dto, hostedServiceAttributeProviderMock.Object,
			hostedServiceBusinessObjectBindingsProviderMock.Object, scheduleStatusProviderMock.Object, dateTimeProviderMock.Object);

		// Assert
		Assert.That(task.OverdueDuration, Is.EqualTo(TimeSpan.FromMinutes(30)));
	}

	[Test]
	public void TestCalculateNextRunTime()
	{
		// Arrange
		var utcNowOverride = new DateTime(2025, 2, 12, 0, 0, 0, DateTimeKind.Utc);
		dateTimeProviderMock
			.SetupGet(x => x.CurrentDateTimeUtc)
			.Returns(utcNowOverride);
		var nextRunTime = utcNowOverride.AddMinutes(-1);
		var dto = new NativeServiceTaskDTO("~TS", taskPk, true, nextRunTime, DateTimeOffset.MinValue, branchPk, "BRN", "", "<ScheduleConfig><NextRunTimeCalculatorDays Period=\"5\" ScheduledRunTime=\"00:00:00\"/></ScheduleConfig>");
		var task = new NativeServiceTask(dto, hostedServiceAttributeProviderMock.Object,
			hostedServiceBusinessObjectBindingsProviderMock.Object, scheduleStatusProviderMock.Object, dateTimeProviderMock.Object);
		var loggerMock = new Mock<ILogger>();
		var expected = new NextRunTimeCalculatorDays() { Period = 5, ScheduledRunTime = TimeSpan.Zero }
			.CalculateNextRunTime(nextRunTime);
		// Act
		var actual = task.CalculateNextRunTime(loggerMock.Object);

		// Assert
		Assert.That(actual, Is.EqualTo(expected));
		loggerMock.Verify(
			x => x.Log(
				LogLevel.Debug,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((state, _) => state.ToString() == "Next runtime was out of the expected range, so it was recalculated based off 'now'"),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception?, string>>()
			),
			Times.Never
		);
	}

	[Test]
	public void TestCalculateNextRunTime_NextRunTimeInFuture()
	{
		// Arrange
		var utcNowOverride = new DateTime(2025, 2, 12, 0, 0, 0, DateTimeKind.Utc);
		dateTimeProviderMock
			.SetupGet(x => x.CurrentDateTimeUtc)
			.Returns(utcNowOverride);
		var nextRunTime = utcNowOverride.AddMinutes(1);
		var dto = new NativeServiceTaskDTO("~TS", taskPk, true, nextRunTime, DateTimeOffset.MinValue, branchPk, "BRN", "", "<ScheduleConfig><NextRunTimeCalculatorDays Period=\"5\" ScheduledRunTime=\"00:00:00\"/></ScheduleConfig>");
		var task = new NativeServiceTask(dto, hostedServiceAttributeProviderMock.Object,
			hostedServiceBusinessObjectBindingsProviderMock.Object, scheduleStatusProviderMock.Object, dateTimeProviderMock.Object);
		var loggerMock = new Mock<ILogger>();
		var expected = new NextRunTimeCalculatorDays() { Period = 5, ScheduledRunTime = TimeSpan.Zero }
			.CalculateNextRunTime(utcNowOverride);
		// Act
		var actual = task.CalculateNextRunTime(loggerMock.Object);

		// Assert
		Assert.That(actual, Is.EqualTo(expected));
		loggerMock.Verify(
			x => x.Log(
				LogLevel.Debug,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((state, _) => state.ToString() == "Next runtime was out of the expected range, so it was recalculated based off 'now'"),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception?, string>>()
			),
			Times.Once
		);
		loggerMock.VerifyNoOtherCalls();
	}

	[Test]
	public void TestCalculateNextRunTime_NextRunTimeExpired()
	{
		// Arrange
		var utcNowOverride = new DateTime(2025, 2, 12, 0, 0, 0, DateTimeKind.Utc);
		dateTimeProviderMock
			.SetupGet(x => x.CurrentDateTimeUtc)
			.Returns(utcNowOverride);
		var nextRunTime = utcNowOverride.AddDays(-5);
		var dto = new NativeServiceTaskDTO("~TS", taskPk, true, nextRunTime, DateTimeOffset.MinValue, branchPk, "BRN", "", "<ScheduleConfig><NextRunTimeCalculatorDays Period=\"5\" ScheduledRunTime=\"00:00:00\"/></ScheduleConfig>");
		var task = new NativeServiceTask(dto, hostedServiceAttributeProviderMock.Object,
			hostedServiceBusinessObjectBindingsProviderMock.Object, scheduleStatusProviderMock.Object, dateTimeProviderMock.Object);
		var loggerMock = new Mock<ILogger>();
		var expected = new NextRunTimeCalculatorDays() { Period = 5, ScheduledRunTime = TimeSpan.Zero }
			.CalculateNextRunTime(utcNowOverride);
		// Act
		var actual = task.CalculateNextRunTime(loggerMock.Object);

		// Assert
		Assert.That(actual, Is.EqualTo(expected));
		loggerMock.Verify(
			x => x.Log(
				LogLevel.Debug,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((state, _) => state.ToString() == "Next runtime was out of the expected range, so it was recalculated based off 'now'"),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception?, string>>()
			),
			Times.Once
		);
		loggerMock.VerifyNoOtherCalls();
	}

	[Test]
	public void TestInitializationWithOnlyCodeInDto()
	{
		// Arrange
		hostServiceAttributeMock.Setup(h => h.Code).Returns("~TS");
		var dto = new NativeServiceTaskDTO(hostServiceAttributeMock.Object, null);

		// Act & Assert
		Assert.DoesNotThrow(() =>
		{
			var task = new NativeServiceTask(dto, hostedServiceAttributeProviderMock.Object,
				hostedServiceBusinessObjectBindingsProviderMock.Object, scheduleStatusProviderMock.Object, dateTimeProviderMock.Object);
		});
	}

	[Test]
	public void TestAssignedGovernor()
	{
		// Arrange
		hostServiceAttributeMock.Setup(h => h.Code).Returns("~TS");
		var dto = new NativeServiceTaskDTO(hostServiceAttributeMock.Object, null);
		var task = new NativeServiceTask(dto, hostedServiceAttributeProviderMock.Object,
			hostedServiceBusinessObjectBindingsProviderMock.Object, scheduleStatusProviderMock.Object, dateTimeProviderMock.Object);
		using var mock1 = ObjectFactory.Substitute(hostedServiceAttributeProviderMock.Object);
		using var mock2 = ObjectFactory.Substitute(scheduleStatusProviderMock.Object);

		// Act
		var governor = task.AssignedGovernor;

		// Assert
		Assert.That(governor, Is.Not.Null);
	}

	[Test]
	public void TestAssignedGovernorIsCached()
	{
		// Arrange
		var dto = new NativeServiceTaskDTO("~TS", Guid.NewGuid(), false, DateTimeOffset.Now, DateTimeOffset.MinValue,
			null, "BRN", "", "<ScheduleConfig><NextRunTimeCalculatorHours Period=\"1\" EndTime=\"06:00:00\" /></ScheduleConfig>");
		var task = new NativeServiceTask(dto, hostedServiceAttributeProviderMock.Object,
			hostedServiceBusinessObjectBindingsProviderMock.Object, scheduleStatusProviderMock.Object, dateTimeProviderMock.Object);
		using var mock1 = ObjectFactory.Substitute(hostedServiceAttributeProviderMock.Object);
		using var mock2 = ObjectFactory.Substitute(scheduleStatusProviderMock.Object);

		// Act
		var governor1 = task.AssignedGovernor;
		governor1.SetActive(true);
		var governor2 = task.AssignedGovernor;

		// Assert
		Assert.That(governor1, Is.EqualTo(governor2));
	}

	readonly Guid taskPk = Guid.NewGuid();
	readonly Guid branchPk = Guid.NewGuid();
	readonly Mock<IClientHostedServiceAttributeProvider> hostedServiceAttributeProviderMock;
	readonly Mock<IHostedServiceBusinessObjectBindingsProvider> hostedServiceBusinessObjectBindingsProviderMock;
	readonly Mock<IServiceTaskScheduleStatusProvider> scheduleStatusProviderMock;
	readonly Mock<IDateTimeProvider> dateTimeProviderMock;
	readonly Mock<IHostedServiceAttribute> hostServiceAttributeMock;
}
