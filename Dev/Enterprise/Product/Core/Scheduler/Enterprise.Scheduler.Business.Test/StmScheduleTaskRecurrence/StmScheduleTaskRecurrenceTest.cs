using System;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Scheduler.Business
{
	[TestedType(typeof(StmScheduleTaskRecurrence))]
	sealed class StmScheduleTaskRecurrenceTest : CargoWise.EntityFramework.Testing.NonPersistentBusinessObjectTestCase
	{
		[TestDate(2020, 7, 20)]
		public void TestMonthlyRecurrence()
		{
			Recurrence.scheduleTask.S5_StartDate = new ZDateTime(2020, 7, 20);
			Recurrence.TaskPeriod = "M";
			Recurrence.TaskPeriodCount = 1;
			Recurrence.DayOfMonth = 15;

			AssertEquals(new ZDateTime(2020, 8, 15), Recurrence.StartDateLocal);
		}

		[TestDate(2019, 7, 20)]
		public void TestYearlyRecurrence()
		{
			Recurrence.scheduleTask.S5_StartDate = new ZDateTime(2019, 7, 20);
			Recurrence.TaskPeriod = "Y";
			Recurrence.MonthNumber = 2;
			Recurrence.DayOfMonth = 30;

			AssertEquals(new ZDateTime(2020, 2, 29), Recurrence.StartDateLocal);
		}

		[TestDate(2023, 06, 15)]
		public void TestYearlyMonthNumberChange()
		{
			void TestCase(byte month, int expectedYear)
			{
				// Arrange
				Recurrence.scheduleTask.S5_StartDate = new ZDateTime(2023, 1, 1);
				Recurrence.TaskPeriod = ScheduleRecurrenceType.Yearly;
				Recurrence.MonthNumber = 6;
				Recurrence.DayOfMonth = 15;

				// Act
				Recurrence.MonthNumber = month;

				// Assert
				AssertEquals(new ZDateTime(expectedYear, month, 15), Recurrence.StartDateLocal);
			}

			TestCase(7, 2023 /* same year */);
			TestCase(5, 2024 /* next year */);
		}

		[TestDate(2023, 06, 30)]
		public void TestChangeToYearly()
		{
			// Arrange
			Recurrence.scheduleTask.S5_StartDate = new ZDateTime(2023, 1, 1);
			Recurrence.DayOfMonth = 15;
			Recurrence.MonthNumber = 7;

			// Act
			Recurrence.TaskPeriod = ScheduleRecurrenceType.Yearly;

			// Assert
			AssertEquals(new ZDateTime(2023, 7, 15), Recurrence.StartDateLocal);
		}

		[TestUtcOffset(10, 0, 0)]
		public void TestProxiedProperties()
		{
			ZDateTime date1 = new ZDateTime(2000, 1, 2);
			ZDateTime date2 = new ZDateTime(2001, 3, 4);
			ZDateTime date3 = new ZDateTime(2002, 5, 6);

			ScheduleTask.S5_DayList = "x";
			ScheduleTask.S5_DayNumber = 1;
			ScheduleTask.S5_EndAfterCount = 2;
			ScheduleTask.S5_EndDate = date1;
			ScheduleTask.S5_MonthNumber = 3;
			ScheduleTask.S5_NextScheduledPrintRunTimeUtc = date2;
			ScheduleTask.S5_StartDate = date3;
			ScheduleTask.S5_TaskPeriod = "y";
			ScheduleTask.S5_TaskPeriodCount = 4;
			ScheduleTask.S5_WeekDaysOnly = true;
			ScheduleTask.S5_WeekDayOccurrenceNumber = 5;

			AssertEquals("Recurrence.DayList", "x", Recurrence.DayList);
			AssertEquals("Recurrence.DayNumber", (ZByte)1, Recurrence.DayNumber);
			AssertEquals("Recurrence.EndAfterCount", 2, Recurrence.EndAfterCount);
			AssertEquals("Recurrence.EndDate", new ZDateTime(2000, 1, 2, 10, 0, 0), Recurrence.EndDateLocal);
			AssertEquals("Recurrence.MonthNumber", (ZByte)3, Recurrence.MonthNumber);
			AssertEquals("Recurrence.NextScheduledPrintRunTime", new ZDateTime(2001, 3, 4, 10, 0, 0), Recurrence.NextScheduledPrintRunTimeLocal);
			AssertEquals("Recurrence.TaskPeriod", "y", Recurrence.TaskPeriod);
			AssertEquals("Recurrence.TaskPeriodCount", (ZByte)4, Recurrence.TaskPeriodCount);
			AssertEquals("Recurrence.WeekDaysOnly", true, Recurrence.WeekDaysOnly);
			AssertEquals("Recurrence.WeekDayOccurrenceNumber", (ZByte)5, Recurrence.WeekDayOccurrenceNumber);
			AssertEquals("Recurrence.StartDate (Date)", new ZDateTime(2002, 5, 6, 10, 0, 0).Date, Recurrence.StartDateLocal.Date);
			AssertEquals("Recurrence.StartDate (Time)", new ZDateTime(2002, 5, 6, 10, 0, 0).TimeOfDay, Recurrence.StartDateLocal.TimeOfDay);

			Recurrence.DayList = "a";
			Recurrence.DayNumber = 2;
			Recurrence.EndAfterCount = 3;
			Recurrence.EndDateLocal = date2;
			Recurrence.MonthNumber = 4;
			Recurrence.NextScheduledPrintRunTimeLocal = date3;
			Recurrence.StartDateLocal = date1;
			Recurrence.TaskPeriod = "b";
			Recurrence.TaskPeriodCount = 5;
			Recurrence.WeekDaysOnly = false;
			Recurrence.WeekDayOccurrenceNumber = 6;

			AssertEquals("ScheduleTask.S5_DayList", "a", ScheduleTask.S5_DayList);
			AssertEquals("ScheduleTask.S5_DayNumber", (ZByte)2, ScheduleTask.S5_DayNumber);
			AssertEquals("ScheduleTask.S5_EndAfterCount", 3, ScheduleTask.S5_EndAfterCount);
			AssertEquals("ScheduleTask.S5_EndDate", new ZDateTime(2001, 3, 3, 14, 0, 0), ScheduleTask.S5_EndDate);
			AssertEquals("ScheduleTask.S5_MonthNumber", (ZByte)4, ScheduleTask.S5_MonthNumber);
			AssertEquals("ScheduleTask.S5_NextScheduledPrintRunTimeUtc", new ZDateTime(2002, 5, 5, 14, 0, 0), ScheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals("ScheduleTask.S5_TaskPeriod", "b", ScheduleTask.S5_TaskPeriod);
			AssertEquals("ScheduleTask.S5_TaskPeriodCount", (ZByte)5, ScheduleTask.S5_TaskPeriodCount);
			AssertEquals("ScheduleTask.S5_WeekDaysOnly", false, ScheduleTask.S5_WeekDaysOnly);
			AssertEquals("ScheduleTask.S5_WeekDayOccurrenceNumber", (ZByte)6, ScheduleTask.S5_WeekDayOccurrenceNumber);
			AssertEquals("ScheduleTask.S5_StartDate (Date)", new ZDateTime(2000, 1, 1, 14, 0, 0).Date, ScheduleTask.S5_StartDate.Date);
			AssertEquals("ScheduleTask.S5_StartDate (Time)", new ZDateTime(2000, 1, 1, 14, 0, 0).TimeOfDay, ScheduleTask.S5_StartDate.TimeOfDay);

			AssertEquals("Recurrence.DayList", "a", Recurrence.DayList);
			AssertEquals("Recurrence.DayNumber", (ZByte)2, Recurrence.DayNumber);
			AssertEquals("Recurrence.EndAfterCount", 3, Recurrence.EndAfterCount);
			AssertEquals("Recurrence.EndDate", date2, Recurrence.EndDateLocal);
			AssertEquals("Recurrence.MonthNumber", (ZByte)4, Recurrence.MonthNumber);
			AssertEquals("Recurrence.NextScheduledPrintRunTime", date3, Recurrence.NextScheduledPrintRunTimeLocal);
			AssertEquals("Recurrence.TaskPeriod", "b", Recurrence.TaskPeriod);
			AssertEquals("Recurrence.TaskPeriodCount", (ZByte)5, Recurrence.TaskPeriodCount);
			AssertEquals("Recurrence.WeekDaysOnly", false, Recurrence.WeekDaysOnly);
			AssertEquals("Recurrence.WeekDayOccurrenceNumber", (ZByte)6, Recurrence.WeekDayOccurrenceNumber);
			AssertEquals("Recurrence.StartDate (Date)", date1.Date, Recurrence.StartDateLocal.Date);
			AssertEquals("Recurrence.StartDate (Time)", date1.TimeOfDay, Recurrence.StartDateLocal.TimeOfDay);
		}

		public void TestProxiedPropertiesLocalTime()
		{
			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch branch = company.Branches.AddNew();
			branch.GB_RL_NKHomePort = "USFDW";
			ScheduleTask.S5_GB = branch.PK;

			ZDateTime date1 = new ZDateTime(2001, 1, 1, 10, 2, 30);
			ZDateTime date2 = new ZDateTime(2001, 1, 1, 7, 7, 7);

			ScheduleTask.CalcDailyStartTimeLocal = date1;
			AssertEquals("Recurrence.RecurringStartTimeLocal", date1.TimeOfDay, Recurrence.RecurringStartTimeLocal.TimeOfDay);

			Recurrence.RecurringStartTimeLocal = date2;
			AssertEquals("ScheduleTask.CalcDailyStartTimeLocal", date2.TimeOfDay, ScheduleTask.CalcDailyStartTimeLocal.TimeOfDay);
			AssertEquals("Recurrence.RecurringStartTimeLocal", date2.TimeOfDay, Recurrence.RecurringStartTimeLocal.TimeOfDay);
		}

		public void TestEndAfter()
		{
			AssertEquals("EndAfter", false, Recurrence.EndAfter);
			AssertEquals("EndAfterCount", 0, Recurrence.EndAfterCount);

			Recurrence.EndAfter = true;
			AssertEquals("EndAfter", true, Recurrence.EndAfter);
			AssertEquals("EndAfterCount", 1, Recurrence.EndAfterCount);

			Recurrence.EndAfter = false;
			AssertEquals("EndAfter", false, Recurrence.EndAfter);
			AssertEquals("EndAfterCount", 0, Recurrence.EndAfterCount);
		}

		public void TestEndBy()
		{
			ZDateTime now = ZDateTime.Now;
			Recurrence.StartDateLocal = now;
			Recurrence.EndDateLocal = now.AddDays(1);
			Recurrence.EndAfter = false;
			AssertEquals("EndBy", true, Recurrence.EndBy);

			Recurrence.EndBy = false;
			AssertEquals("EndBy", false, Recurrence.EndBy);
			AssertEquals("EndDate", ZDateTime.Empty, Recurrence.EndDateLocal);

			Recurrence.EndBy = true;
			AssertEquals("EndBy", true, Recurrence.EndBy);
			AssertEquals("EndDate", Recurrence.StartDateLocal, Recurrence.EndDateLocal);
		}

		public void TestNoEndDate()
		{
			Recurrence.EndAfterCount = 1;
			Recurrence.EndDateLocal = ZDateTime.Now;
			AssertEquals("NoEndDate", false, Recurrence.NoEndDate);

			Recurrence.NoEndDate = true;
			AssertEquals("NoEndDate", true, Recurrence.NoEndDate);
			AssertEquals("EndAfterCount", 0, Recurrence.EndAfterCount);
			AssertEquals("EndDate", ZDateTime.Empty, Recurrence.EndDateLocal);
		}

		public void TestRecurrenceRanges()
		{
			Recurrence.TaskPeriod = "";
			AssertRecurrenceRanges(false, false, false, false, false, false, false, false);

			Recurrence.AccountingRange = true;
			AssertEquals("TaskPeriod", ScheduleRecurrenceType.AccountingPeriod, Recurrence.TaskPeriod);
			AssertRecurrenceRanges(true, false, false, false, false, false, false, false);

			Recurrence.DailyRange = true;
			AssertEquals("TaskPeriod", ScheduleRecurrenceType.Daily, Recurrence.TaskPeriod);
			AssertRecurrenceRanges(false, true, false, false, false, false, false, false);

			Recurrence.MonthlyRange = true;
			AssertEquals("TaskPeriod", ScheduleRecurrenceType.Monthly, Recurrence.TaskPeriod);
			AssertRecurrenceRanges(false, false, true, false, false, false, false, false);

			Recurrence.WeeklyRange = true;
			AssertEquals("TaskPeriod", ScheduleRecurrenceType.Weekly, Recurrence.TaskPeriod);
			AssertRecurrenceRanges(false, false, false, true, false, false, false, false);

			Recurrence.YearlyRange = true;
			AssertEquals("TaskPeriod", ScheduleRecurrenceType.Yearly, Recurrence.TaskPeriod);
			AssertRecurrenceRanges(false, false, false, false, true, false, false, false);

			Recurrence.HourlyRange = true;
			AssertEquals("TaskPeriod", ScheduleRecurrenceType.Hourly, Recurrence.TaskPeriod);
			AssertRecurrenceRanges(false, false, false, false, false, true, false, false);

			Recurrence.MinuteRange = true;
			AssertEquals("TaskPeriod", ScheduleRecurrenceType.Minute, Recurrence.TaskPeriod);
			AssertRecurrenceRanges(false, false, false, false, false, false, true, false);

			Recurrence.SecondRange = true;
			AssertEquals("TaskPeriod", ScheduleRecurrenceType.Second, Recurrence.TaskPeriod);
			AssertRecurrenceRanges(false, false, false, false, false, false, false, true);
		}

		public void TestWeekDays()
		{
			AssertEquals("DayList", "NNNNNNN", Recurrence.DayList);
			AssertWeekDays(false, false, false, false, false, false, false);

			Recurrence.Sunday = true;
			AssertEquals("DayList", "YNNNNNN", Recurrence.DayList);
			AssertWeekDays(true, false, false, false, false, false, false);

			Recurrence.Monday = true;
			AssertEquals("DayList", "YYNNNNN", Recurrence.DayList);
			AssertWeekDays(true, true, false, false, false, false, false);

			Recurrence.Tuesday = true;
			AssertEquals("DayList", "YYYNNNN", Recurrence.DayList);
			AssertWeekDays(true, true, true, false, false, false, false);

			Recurrence.Wednesday = true;
			AssertEquals("DayList", "YYYYNNN", Recurrence.DayList);
			AssertWeekDays(true, true, true, true, false, false, false);

			Recurrence.Thursday = true;
			AssertEquals("DayList", "YYYYYNN", Recurrence.DayList);
			AssertWeekDays(true, true, true, true, true, false, false);

			Recurrence.Friday = true;
			AssertEquals("DayList", "YYYYYYN", Recurrence.DayList);
			AssertWeekDays(true, true, true, true, true, true, false);

			Recurrence.Saturday = true;
			AssertEquals("DayList", "YYYYYYY", Recurrence.DayList);
			AssertWeekDays(true, true, true, true, true, true, true);

			Recurrence.Sunday = false;
			AssertEquals("DayList", "NYYYYYY", Recurrence.DayList);
			AssertWeekDays(false, true, true, true, true, true, true);

			Recurrence.Monday = false;
			AssertEquals("DayList", "NNYYYYY", Recurrence.DayList);
			AssertWeekDays(false, false, true, true, true, true, true);

			Recurrence.Tuesday = false;
			AssertEquals("DayList", "NNNYYYY", Recurrence.DayList);
			AssertWeekDays(false, false, false, true, true, true, true);

			Recurrence.Wednesday = false;
			AssertEquals("DayList", "NNNNYYY", Recurrence.DayList);
			AssertWeekDays(false, false, false, false, true, true, true);

			Recurrence.Thursday = false;
			AssertEquals("DayList", "NNNNNYY", Recurrence.DayList);
			AssertWeekDays(false, false, false, false, false, true, true);

			Recurrence.Friday = false;
			AssertEquals("DayList", "NNNNNNY", Recurrence.DayList);
			AssertWeekDays(false, false, false, false, false, false, true);

			Recurrence.Saturday = false;
			AssertEquals("DayList", "NNNNNNN", Recurrence.DayList);
			AssertWeekDays(false, false, false, false, false, false, false);
		}

		public void TestEveryMonthNumberDayAsString()
		{
			Recurrence.MonthNumber = 0;
			AssertEquals("EveryMonthNumberDayAsString", "", Recurrence.EveryMonthNumberDayAsString);

			Recurrence.MonthNumber = 2;
			AssertEquals("EveryMonthNumberDayAsString", "2", Recurrence.EveryMonthNumberDayAsString);

			Recurrence.EveryMonthNumberDayAsString = "3";
			AssertEquals("EveryMonthNumberDayAsString", "3", Recurrence.EveryMonthNumberDayAsString);
			AssertEquals("MonthNumber", (ZByte)3, Recurrence.MonthNumber);

			Recurrence.EveryMonthNumberDayAsString = "!";
			AssertEquals("EveryMonthNumberDayAsString", "!", Recurrence.EveryMonthNumberDayAsString);
			AssertEquals("MonthNumber", (ZByte)0, Recurrence.MonthNumber);

			Recurrence.YearlyRange = true;
			Recurrence.YearlyWeekDay = true;
			Recurrence.EveryMonthNumberDayAsString = "99";
			AssertEquals("EveryMonthNumberDayAsString", "99", Recurrence.EveryMonthNumberDayAsString);
			AssertEquals("MonthNumber", (ZByte)0, Recurrence.MonthNumber);
		}

		public void TestMonthNumberAsString()
		{
			Recurrence.MonthNumber = 0;
			AssertEquals("MonthNumberAsString", "", Recurrence.MonthNumberAsString);

			Recurrence.MonthNumber = 1;
			AssertEquals("MonthNumberAsString", "1", Recurrence.MonthNumberAsString);

			Recurrence.MonthNumberAsString = "2";
			AssertEquals("MonthNumberAsString", "2", Recurrence.MonthNumberAsString);
			AssertEquals("MonthNumber", (ZByte)2, Recurrence.MonthNumber);

			Recurrence.MonthNumberAsString = "!";
			AssertEquals("MonthNumberAsString", "!", Recurrence.MonthNumberAsString);
			AssertEquals("MonthNumber", (ZByte)0, Recurrence.MonthNumber);

			Recurrence.YearlyRange = true;
			Recurrence.YearlyWeekDay = true;
			Recurrence.MonthNumberAsString = "99";
			AssertEquals("MonthNumberAsString", "99", Recurrence.MonthNumberAsString);
			AssertEquals("MonthNumber", (ZByte)0, Recurrence.MonthNumber);
		}

		public void TestOptionsInRanges()
		{
			Recurrence.WeekDayOccurrenceNumber = 0;
			Recurrence.TaskPeriod = ScheduleRecurrenceType.AccountingPeriod;
			AssertOptionsInRanges(true, false, false);
			Recurrence.TaskPeriod = ScheduleRecurrenceType.Monthly;
			AssertOptionsInRanges(false, true, false);
			Recurrence.TaskPeriod = ScheduleRecurrenceType.Yearly;
			AssertOptionsInRanges(false, false, true);

			Recurrence.WeekDayOccurrenceNumber = 1;
			Recurrence.TaskPeriod = ScheduleRecurrenceType.AccountingPeriod;
			AssertOptionsInRanges(false, false, false);
			Recurrence.TaskPeriod = ScheduleRecurrenceType.Monthly;
			AssertOptionsInRanges(false, false, false);
			Recurrence.TaskPeriod = ScheduleRecurrenceType.Yearly;
			AssertOptionsInRanges(false, false, false);

			Recurrence.AccountingDay = true;
			AssertEquals("WeekDayOccurrenceNumber", (ZByte)0, Recurrence.WeekDayOccurrenceNumber);
			Recurrence.AccountingDay = false;
			AssertEquals("WeekDayOccurrenceNumber", (ZByte)1, Recurrence.WeekDayOccurrenceNumber);
			Recurrence.AccountingWeekDay = false;
			AssertEquals("WeekDayOccurrenceNumber", (ZByte)0, Recurrence.WeekDayOccurrenceNumber);
			Recurrence.AccountingWeekDay = true;
			AssertEquals("WeekDayOccurrenceNumber", (ZByte)1, Recurrence.WeekDayOccurrenceNumber);

			Recurrence.MonthlyDay = true;
			AssertEquals("WeekDayOccurrenceNumber", (ZByte)0, Recurrence.WeekDayOccurrenceNumber);
			Recurrence.MonthlyDay = false;
			AssertEquals("WeekDayOccurrenceNumber", (ZByte)1, Recurrence.WeekDayOccurrenceNumber);
			Recurrence.MonthlyWeekDay = false;
			AssertEquals("WeekDayOccurrenceNumber", (ZByte)0, Recurrence.WeekDayOccurrenceNumber);
			Recurrence.MonthlyWeekDay = true;
			AssertEquals("WeekDayOccurrenceNumber", (ZByte)1, Recurrence.WeekDayOccurrenceNumber);

			Recurrence.YearlyEvery = true;
			AssertEquals("WeekDayOccurrenceNumber", (ZByte)0, Recurrence.WeekDayOccurrenceNumber);
			Recurrence.YearlyEvery = false;
			AssertEquals("WeekDayOccurrenceNumber", (ZByte)1, Recurrence.WeekDayOccurrenceNumber);
			Recurrence.YearlyWeekDay = false;
			AssertEquals("WeekDayOccurrenceNumber", (ZByte)0, Recurrence.WeekDayOccurrenceNumber);
			Recurrence.YearlyWeekDay = true;
			AssertEquals("WeekDayOccurrenceNumber", (ZByte)1, Recurrence.WeekDayOccurrenceNumber);
		}

		public void TestWeekCountAsString()
		{
			Recurrence.WeekDayOccurrenceNumber = 1;
			AssertEquals("WeekCountAsString", "1", Recurrence.WeekCountAsString);

			Recurrence.WeekCountAsString = "4";
			AssertEquals("WeekCountAsString", "4", Recurrence.WeekCountAsString);
			AssertEquals("WeekDayOccurrenceNumber", (ZByte)4, Recurrence.WeekDayOccurrenceNumber);

			Recurrence.WeekCountAsString = "!";
			AssertEquals("WeekCountAsString", "!", Recurrence.WeekCountAsString);
			AssertEquals("WeekDayOccurrenceNumber", (ZByte)0, Recurrence.WeekDayOccurrenceNumber);

			Recurrence.YearlyRange = true;
			Recurrence.YearlyWeekDay = true;
			Recurrence.WeekCountAsString = "9";
			AssertEquals("WeekCountAsString", "9", Recurrence.WeekCountAsString);
			AssertEquals("WeekDayOccurrenceNumber", (ZByte)0, Recurrence.WeekDayOccurrenceNumber);
		}

		public void TestDayName()
		{
			Recurrence.DayNumber = 2;
			AssertEquals("DayName", "MON", Recurrence.DayName);

			Recurrence.DayNumber = 99;
			AssertEquals("DayName", "", Recurrence.DayName);

			Recurrence.DayName = WeekDayList.Codes.Sunday;
			AssertEquals("DayName", WeekDayList.Codes.Sunday, Recurrence.DayName);
			AssertEquals("DayNumber", (ZByte)1, Recurrence.DayNumber);

			Recurrence.DayName = "!";
			AssertEquals("DayName", "!", Recurrence.DayName);
			AssertEquals("DayNumber", (ZByte)0, Recurrence.DayNumber);

			Recurrence.YearlyRange = true;
			Recurrence.YearlyWeekDay = true;
			Recurrence.DayName = "9";
			AssertEquals("DayName", "9", Recurrence.DayName);
			AssertEquals("DayNumber", (ZByte)0, Recurrence.DayNumber);
		}

		public void TestDayNameWhenChangingMonthlyFromLastDay()
		{
			Recurrence.DayName = "2";
			Recurrence.TaskPeriod = ScheduleRecurrenceType.Monthly;
			Recurrence.WeekDayOccurrenceNumber = 0;
			Recurrence.MonthLastDay = true;

			AssertEquals("DayName", "2", Recurrence.DayName);
			AssertEquals("IsLastDay", true, Recurrence.IsLastDay);

			Recurrence.MonthlyDay = false;
			CombineAssertions(() =>
			{
				AssertEquals("IsLastDay", false, Recurrence.IsLastDay);
				AssertEquals("WeekCountAsString", "1", Recurrence.WeekCountAsString);
				AssertEquals("WeekDayOccurrenceNumber", (ZByte)1, Recurrence.WeekDayOccurrenceNumber);
				AssertEquals("MonthlyWeekDay", true, Recurrence.MonthlyWeekDay);
				AssertEquals("DayName", ZString.Empty, Recurrence.DayName);
				AssertEquals("DayNumber", (ZByte)0, Recurrence.DayNumber);
			});
		}

		public void TestDayNameWhenChangingAccountingPeriodFromLastDay()
		{
			Recurrence.DayName = "2";
			Recurrence.TaskPeriod = ScheduleRecurrenceType.AccountingPeriod;
			Recurrence.WeekDayOccurrenceNumber = 0;
			Recurrence.AccountingLastDay = true;

			AssertEquals("DayName", "2", Recurrence.DayName);
			AssertEquals("IsLastDay", true, Recurrence.IsLastDay);

			Recurrence.AccountingDay = false;
			CombineAssertions(() =>
			{
				AssertEquals("IsLastDay", false, Recurrence.IsLastDay);
				AssertEquals("WeekCountAsString", "1", Recurrence.WeekCountAsString);
				AssertEquals("WeekDayOccurrenceNumber", (ZByte)1, Recurrence.WeekDayOccurrenceNumber);
				AssertEquals("AccountingWeekDay", true, Recurrence.AccountingWeekDay);
				AssertEquals("DayName", ZString.Empty, Recurrence.DayName);
				AssertEquals("DayNumber", (ZByte)0, Recurrence.DayNumber);
			});
		}

		public void TestDayNameWhenChangingYearlyFromLastDay()
		{
			Recurrence.DayName = "2";
			Recurrence.TaskPeriod = ScheduleRecurrenceType.Monthly;
			Recurrence.WeekDayOccurrenceNumber = 0;
			Recurrence.MonthLastDay = true;

			AssertEquals("DayName", "2", Recurrence.DayName);
			AssertEquals("IsLastDay", true, Recurrence.IsLastDay);

			Recurrence.TaskPeriod = ScheduleRecurrenceType.Monthly;
			Recurrence.YearlyEvery = false;
			CombineAssertions(() =>
			{
				AssertEquals("IsLastDay", false, Recurrence.IsLastDay);
				AssertEquals("WeekCountAsString", "1", Recurrence.WeekCountAsString);
				AssertEquals("WeekDayOccurrenceNumber", (ZByte)1, Recurrence.WeekDayOccurrenceNumber);
				AssertEquals("YearlyWeekDay", true, Recurrence.YearlyWeekDay);
				AssertEquals("DayName", ZString.Empty, Recurrence.DayName);
				AssertEquals("DayNumber", (ZByte)0, Recurrence.DayNumber);
			});
		}

		public void TestLastDayOptions()
		{
			Recurrence.TaskPeriod = ScheduleRecurrenceType.AccountingPeriod;

			Recurrence.AccountingLastDay = false;
			AssertEquals("AccountingLastDay", false, Recurrence.AccountingLastDay);
			AssertEquals("DayNumber", (ZByte)0, Recurrence.DayNumber);

			Recurrence.AccountingLastDay = true;
			AssertEquals("AccountingLastDay", true, Recurrence.AccountingLastDay);
			AssertEquals("DayNumber", (ZByte)99, Recurrence.DayNumber);

			Recurrence.TaskPeriod = ScheduleRecurrenceType.Monthly;

			Recurrence.MonthLastDay = false;
			AssertEquals("MonthLastDay", false, Recurrence.MonthLastDay);
			AssertEquals("DayNumber", (ZByte)0, Recurrence.DayNumber);

			Recurrence.MonthLastDay = true;
			AssertEquals("MonthLastDay", true, Recurrence.MonthLastDay);
			AssertEquals("DayNumber", (ZByte)99, Recurrence.DayNumber);
		}

		public void TestDayOfAccountingPeriod()
		{
			ZDateTime startDate = new ZDate(2006, 6, 13);
			Recurrence.StartDateLocal = startDate;
			AssertEquals("DayOfAccountingPeriod", 0, Recurrence.DayOfAccountingPeriod);

			Recurrence.DayOfAccountingPeriod = 10;
			AssertEquals("DayOfAccountingPeriod", 0, Recurrence.DayOfAccountingPeriod);
			AssertEquals("StartDate", startDate, Recurrence.StartDateLocal);

			CreateAccPeriodTestData();
			//Factory.Save(); ?
			AssertEquals("DayOfAccountingPeriod", 74, Recurrence.DayOfAccountingPeriod);

			Recurrence.DayOfAccountingPeriod = 10;
			AssertEquals("DayOfAccountingPeriod", 10, Recurrence.DayOfAccountingPeriod);
			AssertEquals("StartDate", new ZDateTime(2006, 7, 10), Recurrence.StartDateLocal);

			Recurrence.StartDateLocal = new ZDateTime(2007, 10, 1);
			Recurrence.DayOfAccountingPeriod = -2;
			AssertEquals("DayOfAccountingPeriod", 1, Recurrence.DayOfAccountingPeriod);
			AssertEquals("StartDate", new ZDateTime(2007, 10, 1), Recurrence.StartDateLocal);
		}

		[TestDate(2007, 1, 1, 0, 0, 0)]
		public void TestDayOfMonth()
		{
			Recurrence.StartDateLocal = ZDateTime.Empty;
			AssertEquals("DayOfMonth", 0, Recurrence.DayOfMonth);

			Recurrence.StartDateLocal = new ZDateTime(2006, 12, 15);
			AssertEquals("DayOfMonth", 15, Recurrence.DayOfMonth);

			Recurrence.DayOfMonth = 16;
			AssertEquals("DayOfMonth", 16, Recurrence.DayOfMonth);
			AssertEquals("StartDate", new ZDateTime(2006, 12, 16), Recurrence.StartDateLocal);

			Recurrence.DayOfMonth = -1;
			AssertEquals("DayOfMonth", 1, Recurrence.DayOfMonth);
			AssertEquals("StartDate", new ZDateTime(2007, 1, 1), Recurrence.StartDateLocal);

			Recurrence.DayOfMonth = 32;
			AssertEquals("DayOfMonth", 31, Recurrence.DayOfMonth);
			AssertEquals("StartDate", new ZDateTime(2007, 1, 31), Recurrence.StartDateLocal);
		}

		[TestDate(2016, 4, 30, 4, 0, 0)]
		public void TestDayOfMonthUpgradesNextRunTime()
		{
			Recurrence.MonthlyRange = true;
			Recurrence.scheduleTask.S5_IsPrivate = true;
			Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2016, 2, 10);
			Recurrence.scheduleTask.S5_DailyStartTime = new ZDateTime(1, 1, 1, 17, 30, 0);
			Recurrence.WeekDayOccurrenceNumber = 0;
			Recurrence.TaskPeriodCount = 2;
			Recurrence.DayNumber = 99;

			Recurrence.DayOfMonth = 10;
			AssertEquals("S5_NextScheduledPrintRunTimeUtc updated by DayOfMonth", new ZDateTime(2016, 5, 10, 17, 30, 0), Recurrence.scheduleTask.S5_NextScheduledPrintRunTimeUtc);

			Recurrence.DayOfMonth = 19;
			AssertEquals("S5_NextScheduledPrintRunTimeUtc updated by DayOfMonth", new ZDateTime(2016, 5, 19, 17, 30, 0), Recurrence.scheduleTask.S5_NextScheduledPrintRunTimeUtc);

			Recurrence.DayOfMonth = 14;
			AssertEquals("S5_NextScheduledPrintRunTimeUtc updated by DayOfMonth", new ZDateTime(2016, 5, 14, 17, 30, 0), Recurrence.scheduleTask.S5_NextScheduledPrintRunTimeUtc);

			Recurrence.DayOfMonth = 10;
			AssertEquals("S5_NextScheduledPrintRunTimeUtc updated by DayOfMonth", new ZDateTime(2016, 5, 10, 17, 30, 0), Recurrence.scheduleTask.S5_NextScheduledPrintRunTimeUtc);
		}

		public void TestDailyDay()
		{
			Recurrence.WeekDaysOnly = true;
			AssertEquals("DailyDay", false, Recurrence.DailyDay);
			Recurrence.WeekDaysOnly = false;
			AssertEquals("DailyDay", true, Recurrence.DailyDay);
		}

		/// <summary>
		/// It cannot assume Current Branch and DB are in the same Time Zone.
		/// It MUST recalculate Local Time.
		/// </summary>
		public void TestLocalRunTimeGMTOffset()
		{
			Recurrence.StartDateLocal = new ZDateTime(1980, 2, 17, 0, 0, 0);
			Recurrence.scheduleTask.S5_DailyStartTime = new ZDateTime(1, 1, 1, 15, 45, 0);

			DateTime utcTime = new DateTime(1980, 2, 17, 15, 45, 0);
			ZDateTime localTime = Env.Time.GetLocalTimeFromUtc(utcTime);
			TimeSpan localToUtcOffsetSpan = localTime.ToDateTime().Subtract(utcTime);
			ZDecimal localToUtcOffset = localToUtcOffsetSpan.TotalHours;

			string expected = String.Format("UTC{0:+#.#;-#.#;#}", localToUtcOffset);
			AssertEquals(expected, Recurrence.LocalRunTimeUtcOffset);
		}

		public void TestEndAfterCountReadOnly()
		{
			Recurrence.EndAfter = true;
			AssertEquals("EndAfterCountInfo.ReadOnly", false, Recurrence.EndAfterCountInfo.ReadOnly);
			Recurrence.EndAfter = false;
			AssertEquals("EndAfterCountInfo.ReadOnly", true, Recurrence.EndAfterCountInfo.ReadOnly);
		}

		public void TestEndDateReadOnly()
		{
			Recurrence.StartDateLocal = ZDateTime.Now;
			Recurrence.EndBy = true;
			AssertEquals("EndDateInfo.ReadOnly", false, Recurrence.EndDateLocalForUserInfo.ReadOnly);
			Recurrence.EndBy = false;
			AssertEquals("EndDateInfo.ReadOnly", true, Recurrence.EndDateLocalForUserInfo.ReadOnly);
		}

		public void TestTaskPeriodCountReadOnly()
		{
			TestIsRangeAndNotDayReadOnly(Recurrence.TaskPeriodCountInfo);
		}

		public void TestEveryMonthNumberDayAsStringReadOnly()
		{
			TestIsRangeAndNotWeekDayReadOnly(Recurrence.EveryMonthNumberDayAsStringInfo, false);
		}

		public void TestMonthNumberAsStringReadOnly()
		{
			TestIsRangeAndNotWeekDayReadOnly(Recurrence.MonthNumberAsStringInfo, true);
		}

		public void TestWeekCountAsStringReadOnly()
		{
			TestIsRangeAndNotWeekDayReadOnly(Recurrence.WeekCountAsStringInfo, true);
		}

		public void TestDayNameReadOnly()
		{
			TestIsRangeAndNotWeekDayReadOnly(Recurrence.DayNameInfo, true);
		}

		public void TestAccountingLastDayReadOnly()
		{
			TestIsRangeAndNotDayReadOnly(Recurrence.AccountingLastDayInfo);
		}

		public void TestMonthLastDayReadOnly()
		{
			TestIsRangeAndNotWeekDayReadOnly(Recurrence.MonthLastDayInfo, false);
		}

		public void TestDayOfAccountingPeriodReadOnly()
		{
			Recurrence.AccountingRange = true;
			Recurrence.DayNumber = 99;
			AssertEquals("DayOfAccountingPeriodInfo.ReadOnly", true, Recurrence.DayOfAccountingPeriodInfo.ReadOnly);

			Recurrence.DayNumber = 98;
			Recurrence.AccountingWeekDay = false;
			AssertEquals("DayOfAccountingPeriodInfo.ReadOnly", false, Recurrence.DayOfAccountingPeriodInfo.ReadOnly);

			Recurrence.AccountingWeekDay = true;
			AssertEquals("DayOfAccountingPeriodInfo.ReadOnly", true, Recurrence.DayOfAccountingPeriodInfo.ReadOnly);
		}

		public void TestDayOfMonthReadOnly()
		{
			Recurrence.MonthlyRange = true;
			Recurrence.DayNumber = 99;
			AssertEquals("DayOfMonthInfo.ReadOnly", true, Recurrence.DayOfMonthInfo.ReadOnly);

			Recurrence.DayNumber = 98;
			Recurrence.MonthlyDay = true;
			AssertEquals("DayOfMonthInfo.ReadOnly", false, Recurrence.DayOfMonthInfo.ReadOnly);

			Recurrence.MonthlyDay = false;
			AssertEquals("DayOfMonthInfo.ReadOnly", true, Recurrence.DayOfMonthInfo.ReadOnly);

			Recurrence.YearlyRange = true;
			Recurrence.YearlyEvery = true;
			AssertEquals("DayOfMonthInfo.ReadOnly", true, Recurrence.DayOfMonthInfo.ReadOnly);

			Recurrence.MonthNumber = 1;
			AssertEquals("DayOfMonthInfo.ReadOnly", false, Recurrence.DayOfMonthInfo.ReadOnly);

			Recurrence.YearlyEvery = false;
			AssertEquals("DayOfMonthInfo.ReadOnly", true, Recurrence.DayOfMonthInfo.ReadOnly);
		}

		public void TestCalculateDate()
		{
			AssertEquals("CalculateUtcDateFromLocal(1, DayOfWeek.Monday, 1, 2000)", Recurrence.GetUtcDate(new DateTime(2000, 1, 3)), Recurrence.CalculateUtcDateFromLocal(1, DayOfWeek.Monday, 1, 2000));
			AssertEquals("CalculateUtcDateFromLocal(2, DayOfWeek.Tuesday, 2, 2001)", Recurrence.GetUtcDate(new DateTime(2001, 2, 13)), Recurrence.CalculateUtcDateFromLocal(2, DayOfWeek.Tuesday, 2, 2001));
		}

		public void TestCalculateDateForAccountingPeriod()
		{
			ZDateTime baseDateTime = new ZDateTime(2006, 10, 5);
			AssertEquals("CalculateUtcDateForAccountingPeriodFromLocal", baseDateTime, Recurrence.CalculateUtcDateForAccountingPeriodFromLocal(1, DayOfWeek.Monday, 200604, baseDateTime));
			CreateAccPeriodTestData();
			AssertEquals("CalculateUtcDateForAccountingPeriodFromLocal", Recurrence.GetUtcDate(new DateTime(2006, 4, 3)), Recurrence.CalculateUtcDateForAccountingPeriodFromLocal(1, DayOfWeek.Monday, 200604, baseDateTime));
			AssertEquals("CalculateUtcDateForAccountingPeriodFromLocal", baseDateTime, Recurrence.CalculateUtcDateForAccountingPeriodFromLocal(1, DayOfWeek.Monday, 0, baseDateTime));
		}

		[TestUtcOffset(0, 0, 0)]
		public void TestCalculateNewAccountingPeriodStartDate()
		{
			ZDateTime startDate = new ZDateTime(2006, 10, 2);
			CreateAccPeriodTestData();
			Recurrence.AccountingRange = true;

			Recurrence.StartDateLocal = startDate;
			Recurrence.WeekDayOccurrenceNumber = 0;
			Recurrence.DayNumber = 99;
			AssertEquals("CalculateNewStartDate()", new ZDateTime(2006, 12, 31), Recurrence.CalculateNewStartDate());

			Recurrence.DayNumber = 4;
			AssertEquals("CalculateNewStartDate()", startDate, Recurrence.CalculateNewStartDate());

			Recurrence.WeekDayOccurrenceNumber = 1;
			AssertEquals("CalculateNewStartDate()", new ZDateTime(2006, 10, 4), Recurrence.CalculateNewStartDate());

			Recurrence.StartDateLocal = new ZDateTime(2007, 2, 10);
			AssertEquals("CalculateNewStartDate()", new ZDateTime(2007, 4, 4), Recurrence.CalculateNewStartDate());

			startDate = new ZDateTime(2008, 1, 1);
			Recurrence.StartDateLocal = startDate;
			Recurrence.DayNumber = 99;
			Recurrence.WeekDayOccurrenceNumber = 0;
			AssertEquals("CalculateNewStartDate()", startDate, Recurrence.CalculateNewStartDate());
		}

		[TestUtcOffset(0, 0, 0)]
		public void TestCalculateNewWeeklyStartDate()
		{
			Recurrence.WeeklyRange = true;
			Recurrence.StartDateLocal = new ZDateTime(2006, 1, 6);
			Recurrence.DayList = "NNNYYNN";
			AssertEquals("CalculateNewStartDate()", new ZDateTime(2006, 1, 11), Recurrence.CalculateNewStartDate());
		}

		[TestUtcOffset(-4, 0, 0)]
		public void TestCalculateNewWeeklyStartDateByLocalTime()
		{
			Recurrence.WeeklyRange = true;
			Recurrence.scheduleTask.S5_StartDate = new ZDateTime(2010, 9, 7, 1, 0, 0); // 2010-09-06 21:00 Local Monday (UTC Tuesday)
			Recurrence.DayList = "NNNYYNN";
			AssertEquals("CalculateNewStartDate()", new ZDateTime(2010, 9, 9, 1, 0, 0), Recurrence.CalculateNewStartDate()); // 2010-09-08 21:00 Local Wednesday (UTC Thursday)
		}

		[TestUtcOffset(0, 0, 0)]
		public void TestCalculateNewDailyStartDate()
		{
			ZDateTime startDate = new ZDateTime(2006, 9, 23);
			Recurrence.DailyRange = true;
			Recurrence.StartDateLocal = startDate;

			Recurrence.WeekDaysOnly = false;
			AssertEquals("CalculateNewStartDate()", startDate, Recurrence.CalculateNewStartDate());

			Recurrence.WeekDaysOnly = true;
			AssertEquals("CalculateNewStartDate()", new ZDateTime(2006, 9, 25), Recurrence.CalculateNewStartDate());
		}

		[TestUtcOffset(-4, 0, 0)]
		public void TestCalculateNewDailyStartDateByLocalTime()
		{
			Recurrence.DailyRange = true;
			Recurrence.WeekDaysOnly = true;

			Recurrence.scheduleTask.S5_StartDate = new ZDateTime(2010, 9, 11, 1, 0, 0); // 2010-09-10 21:00 Local Friday (UTC Satturday)
			AssertEquals("Local Friday (UTC Satturday)", new ZDateTime(2010, 9, 11, 1, 0, 0), Recurrence.CalculateNewStartDate());

			Recurrence.scheduleTask.S5_StartDate = new ZDateTime(2010, 9, 12, 1, 0, 0); // 2010-09-11 21:00 Local Satturday (UTC Sunday)
			AssertEquals("Local Monday (UTC Tuesday)", new ZDateTime(2010, 9, 14, 1, 0, 0), Recurrence.CalculateNewStartDate());
		}

		[TestUtcOffset(0, 0, 0)]
		public void TestCalculateNewMonthlyStartDate()
		{
			ZDateTime startDate = new ZDateTime(2006, 5, 6);
			Recurrence.MonthlyRange = true;

			Recurrence.WeekDayOccurrenceNumber = 0;
			Recurrence.DayNumber = 99;
			Recurrence.StartDateLocal = startDate;
			AssertEquals("CalculateNewStartDate()", new ZDateTime(2006, 5, 31), Recurrence.CalculateNewStartDate());

			Recurrence.DayNumber = 5;
			AssertEquals("CalculateNewStartDate()", startDate, Recurrence.CalculateNewStartDate());

			Recurrence.WeekDayOccurrenceNumber = 2;
			AssertEquals("CalculateNewStartDate()", new ZDateTime(2006, 5, 11), Recurrence.CalculateNewStartDate());

			Recurrence.StartDateLocal = new ZDateTime(2006, 5, 31);
			AssertEquals("CalculateNewStartDate()", new ZDateTime(2006, 6, 8), Recurrence.CalculateNewStartDate());

			Recurrence.StartDateLocal = new ZDateTime(2013, 03, 01);
			Recurrence.WeekDayOccurrenceNumber = 1;
			Recurrence.DayNumber = 2;
			AssertEquals(new ZDateTime(2013, 03, 04), Recurrence.CalculateNewStartDate());

			Recurrence.StartDateLocal = new ZDateTime(2013, 03, 04);
			AssertEquals("Should not go to next month if date is same", new ZDateTime(2013, 03, 04), Recurrence.CalculateNewStartDate());
		}

		[TestUtcOffset(-4, 0, 0)]
		public void TestCalculateNewMonthlyStartDateByLocalTime()
		{
			ZDateTime startDate = new ZDateTime(2010, 9, 6, 1, 0, 0); // 2010-09-05 21:00 Local Time
			Recurrence.MonthlyRange = true;

			Recurrence.WeekDayOccurrenceNumber = 0;
			Recurrence.DayNumber = 99;
			Recurrence.scheduleTask.S5_StartDate = startDate;
			AssertEquals("CalculateNewStartDate()", new ZDateTime(2010, 10, 1, 1, 0, 0), Recurrence.CalculateNewStartDate()); // 2010-09-30 21:00 Local Time

			Recurrence.DayNumber = 5;
			AssertEquals("CalculateNewStartDate()", startDate, Recurrence.CalculateNewStartDate());

			Recurrence.WeekDayOccurrenceNumber = 3;
			AssertEquals("CalculateNewStartDate()", new ZDateTime(2010, 9, 17, 1, 0, 0), Recurrence.CalculateNewStartDate()); // 2010-09-16 21:00 Local Time

			Recurrence.scheduleTask.S5_StartDate = new ZDateTime(2010, 9, 30, 1, 0, 0);
			AssertEquals("CalculateNewStartDate()", new ZDateTime(2010, 10, 22, 1, 0, 0), Recurrence.CalculateNewStartDate()); // 2010-10-21 21:00 Local Time
		}

		[TestUtcOffset(0, 0, 0)]
		public void TestCalculateNewYearlyStartDate()
		{
			Recurrence.YearlyRange = true;
			var startDate = new ZDateTime(2006, 11, 25);
			Recurrence.StartDateLocal = startDate;

			Recurrence.WeekDayOccurrenceNumber = 0;
			Recurrence.MonthNumber = 2;
			AssertEquals("CalculateNewStartDate()", new ZDateTime(2007, 2, 25), Recurrence.CalculateNewStartDate());

			Recurrence.StartDateLocal = startDate;
			Recurrence.MonthNumber = 12;
			AssertEquals("CalculateNewStartDate()", new ZDateTime(2006, 12, 25), Recurrence.CalculateNewStartDate());

			Recurrence.StartDateLocal = startDate;
			Recurrence.WeekDayOccurrenceNumber = 3;
			Recurrence.DayNumber = 2;
			AssertEquals("CalculateNewStartDate()", new ZDateTime(2006, 12, 18), Recurrence.CalculateNewStartDate());

			Recurrence.StartDateLocal = new ZDateTime(2006, 12, 31);
			AssertEquals("CalculateNewStartDate()", new ZDateTime(2007, 12, 17), Recurrence.CalculateNewStartDate());

			Recurrence.StartDateLocal = new ZDateTime(2007, 12, 17);
			AssertEquals("Should not go to next year if date is same", new ZDateTime(2007, 12, 17), Recurrence.CalculateNewStartDate());
		}

		[TestUtcOffset(0, 0, 0)]
		public void TestCalculateNewHourlyStartDate()
		{
			ZDateTime startDate = new ZDateTime(2007, 6, 16);
			Recurrence.TaskPeriod = ScheduleRecurrenceType.Hourly;
			Recurrence.StartDateLocal = startDate;

			Recurrence.WeekDaysOnly = false;
			AssertEquals("CalculateNewStartDate()", startDate, Recurrence.CalculateNewStartDate());

			Recurrence.WeekDaysOnly = true;
			AssertEquals("CalculateNewStartDate()", new ZDateTime(2007, 6, 18), Recurrence.CalculateNewStartDate());
		}

		[TestUtcOffset(0, 0, 0)]
		public void TestCalculateNewMinutePeriodStartDate()
		{
			ZDateTime startDate = new ZDateTime(2007, 6, 16);
			Recurrence.TaskPeriod = ScheduleRecurrenceType.Minute;
			Recurrence.StartDateLocal = startDate;

			Recurrence.WeekDaysOnly = false;
			AssertEquals("CalculateNewStartDate()", startDate, Recurrence.CalculateNewStartDate());

			Recurrence.WeekDaysOnly = true;
			AssertEquals("CalculateNewStartDate()", new ZDateTime(2007, 6, 18), Recurrence.CalculateNewStartDate());
		}

		[TestUtcOffset(0, 0, 0)]
		public void TestCalculateNewSecondPeriodStartDate()
		{
			ZDateTime startDate = new ZDateTime(2007, 6, 16);
			Recurrence.TaskPeriod = ScheduleRecurrenceType.Second;
			Recurrence.StartDateLocal = startDate;

			Recurrence.WeekDaysOnly = false;
			AssertEquals("CalculateNewStartDate()", startDate, Recurrence.CalculateNewStartDate());

			Recurrence.WeekDaysOnly = true;
			AssertEquals("CalculateNewStartDate()", new ZDateTime(2007, 6, 18), Recurrence.CalculateNewStartDate());
		}

		[ExpectExceptionMessage(typeof(InvalidOperationException), "CalculateNewStartDate cannot be called when TaskPeriod is invalid.")]
		public void TestCalculateNewStartDateWhenTaskPeriodIsInvalid()
		{
			Recurrence.TaskPeriod = "";
			Recurrence.CalculateNewStartDate();
		}

		[TestUtcOffset(0, 0, 0)]
		public void TestCalculateNextAccountingPeriodScheduleDate()
		{
			ZDateTime nextScheduledPrintRunTime = new ZDateTime(2006, 9, 3, 17, 30, 0);
			Recurrence.AccountingRange = true;
			Recurrence.NextScheduledPrintRunTimeLocal = nextScheduledPrintRunTime;
			Recurrence.scheduleTask.S5_DailyStartTime = new ZDateTime(1, 1, 1, 17, 30, 0);

			Recurrence.WeekDayOccurrenceNumber = 0;
			Recurrence.TaskPeriodCount = 2;
			AssertEquals("CalculateNextScheduleDate()", nextScheduledPrintRunTime, Recurrence.CalculateNextScheduleDate());

			CreateAccPeriodTestData();
			Recurrence.NextScheduledPrintRunTimeLocal = nextScheduledPrintRunTime;
			AssertEquals("CalculateNextScheduleDate()", new ZDateTime(2007, 3, 6, 17, 30, 0), Recurrence.CalculateNextScheduleDate());

			Recurrence.DayNumber = 99;
			AssertEquals("CalculateNextScheduleDate()", new ZDateTime(2007, 3, 31, 17, 30, 0), Recurrence.CalculateNextScheduleDate());

			Recurrence.WeekDayOccurrenceNumber = 1;
			Recurrence.DayNumber = 6;
			AssertEquals("CalculateNextScheduleDate()", new ZDateTime(2006, 10, 6, 17, 30, 0), Recurrence.CalculateNextScheduleDate());
		}

		[TestUtcOffset(0, 0, 0)]
		public void TestCalculateNextWeeklyScheduleDate()
		{
			Recurrence.WeeklyRange = true;
			Recurrence.DayList = "NNNYYYY";
			Recurrence.TaskPeriodCount = 3;
			Recurrence.scheduleTask.S5_DailyStartTime = new ZDateTime(1, 1, 1, 17, 30, 0);

			Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2006, 9, 23);
			AssertEquals("CalculateNextScheduleDate()", new ZDateTime(2006, 10, 11, 17, 30, 0), Recurrence.CalculateNextScheduleDate());

			Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2006, 9, 24);
			AssertEquals("CalculateNextScheduleDate()", new ZDateTime(2006, 10, 11, 17, 30, 0), Recurrence.CalculateNextScheduleDate());
		}

		[TestUtcOffset(-4, 0, 0)]
		public void TestCalculateNextWeeklyScheduleDateByLocalTime()
		{
			Recurrence.WeeklyRange = true;
			Recurrence.DayList = "NNNYYYY";
			Recurrence.TaskPeriodCount = 3;
			Recurrence.scheduleTask.S5_DailyStartTime = new ZDateTime(1, 1, 1, 21, 0, 0); // 21:00 Local time

			Recurrence.scheduleTask.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2010, 9, 19, 1, 0, 0); // 2010-09-18 21:00 Local Saturday (UTC Sunday)
			AssertEquals("CalculateNextScheduleDate()", new ZDateTime(2010, 10, 7, 1, 0, 0), Recurrence.CalculateNextScheduleDate()); // 2010-09-06 21:00 Local Wednesday (UTC Thursday)

			Recurrence.scheduleTask.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2010, 9, 20, 1, 0, 0); // 2010-09-19 21:00 Local Sunday (UTC Monday)
			AssertEquals("CalculateNextScheduleDate()", new ZDateTime(2010, 10, 7, 1, 0, 0), Recurrence.CalculateNextScheduleDate()); // 2010-09-06 21:00 Local Wednesday (UTC Thursday)
		}

		public void TestCalculateNextWeeklyScheduleDateThrowsExceptionWhenNoSelectedDay()
		{
			var task = Factory.NewWithValidTestData<StmScheduleTask>();
			task.S5_ScheduleType = "JNG";
			var recurrence = new StmScheduleTaskRecurrence(task);
			recurrence.WeeklyRange = true;
			recurrence.DayList = "NNNNNNN";

			AssertExceptionThrown(typeof(InvalidOperationException), "Cannot calculate weekly schedule when no days are selected", () => recurrence.CalculateNextScheduleDate());
		}

		[TestUtcOffset(0, 0, 0)]
		public void TestCalculateNextDailyScheduleDate()
		{
			Recurrence.DailyRange = true;
			Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2006, 9, 22);
			Recurrence.scheduleTask.S5_DailyStartTime = new ZDateTime(1, 1, 1, 17, 30, 0);

			Recurrence.WeekDaysOnly = true;
			AssertEquals("CalculateNextScheduleDate()", new ZDateTime(2006, 9, 25, 17, 30, 0), Recurrence.CalculateNextScheduleDate());

			Recurrence.WeekDaysOnly = false;
			Recurrence.TaskPeriodCount = 1;
			AssertEquals("CalculateNextScheduleDate()", new ZDateTime(2006, 9, 23, 17, 30, 0), Recurrence.CalculateNextScheduleDate());
		}

		[TestUtcOffset(-4, 0, 0)]
		public void TestCalculateNextDailyScheduleDateByLocalTime()
		{
			Recurrence.DailyRange = true;
			Recurrence.WeekDaysOnly = true;
			Recurrence.scheduleTask.S5_DailyStartTime = new ZDateTime(1, 1, 1, 21, 0, 0);

			Recurrence.scheduleTask.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2010, 9, 10, 1, 0, 0); // 2010-09-09 21:00 Local Thursady (UTC Friday)
			AssertEquals("Local Friday (UTC Satturday)", new ZDateTime(2010, 9, 11, 1, 0, 0), Recurrence.CalculateNextScheduleDate());

			Recurrence.scheduleTask.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2010, 9, 11, 1, 0, 0); // 2010-09-10 21:00 Local Friday (UTC Satturday)
			AssertEquals("Local Monday (UTC Tuesady)", new ZDateTime(2010, 9, 14, 1, 0, 0), Recurrence.CalculateNextScheduleDate());
		}

		[TestUtcOffset(0, 0, 0)]
		public void TestCalculateNextMonthlyScheduleDate()
		{
			Recurrence.MonthlyRange = true;
			Recurrence.DayOfMonth = 10;
			Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2006, 2, 10);
			Recurrence.scheduleTask.S5_DailyStartTime = new ZDateTime(1, 1, 1, 17, 30, 0);

			Recurrence.WeekDayOccurrenceNumber = 0;
			Recurrence.TaskPeriodCount = 2;
			Recurrence.DayNumber = 99;
			AssertEquals("CalculateNextScheduleDate()", new ZDateTime(2006, 4, 30, 17, 30, 0), Recurrence.CalculateNextScheduleDate());

			Recurrence.DayNumber = 3;
			AssertEquals("CalculateNextScheduleDate()", new ZDateTime(2006, 4, 10, 17, 30, 0), Recurrence.CalculateNextScheduleDate());

			Recurrence.WeekDayOccurrenceNumber = 3;
			AssertEquals("CalculateNextScheduleDate()", new ZDateTime(2006, 3, 21, 17, 30, 0), Recurrence.CalculateNextScheduleDate());
		}

		[TestUtcOffset(0, 0, 0)]
		public void TestCalculateNextWeeklyScheduleDate_InNoUtcOffset_WithEmptyDailyStartTime()
		{
			Recurrence.WeeklyRange = true;
			Recurrence.DayList = "NNYNYNN"; //Tuesday and Thursday

			Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2016, 3, 6, 1, 0, 0); //a sunday. randomly increasing hours to test this scenario is handled correctly
			Recurrence.scheduleTask.S5_DailyStartTime = ZDateTime.Empty;
			Recurrence.scheduleTask.S5_TaskPeriodCount = 1;

			AssertEquals(new ZDateTime(2016, 3, 8), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate()));
			AssertEquals(new ZDateTime(2016, 3, 10), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate()));
			AssertEquals(new ZDateTime(2016, 3, 15), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate()));
			AssertEquals(new ZDateTime(2016, 3, 17), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate()));
		}

		public void TestNextScheduledPrintRunTimeLocal_WithUtcOffsetOverride()
		{
			var task = Factory.New<Testing.StmScheduleTaskTest.StmScheduleTaskWithUtcOffset>();
			task.SetUtcOffsetOverrideForTesting(new TimeSpan(6, 0, 0));

			task.Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2002, 2, 2, 10, 0, 0);
			AssertEquals(new ZDateTime(2002, 2, 2, 4, 0, 0), task.S5_NextScheduledPrintRunTimeUtc);

			task.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2002, 2, 2, 16, 0, 0);
			AssertEquals(new ZDateTime(2002, 2, 2, 22, 0, 0), task.Recurrence.NextScheduledPrintRunTimeLocal);
		}

		[TestUtcOffset(8, 0, 0)]
		public void TestCalculateNextWeeklyScheduleDate_InPositiveUtcOffset_WithEmptyDailyStartTime()
		{
			Recurrence.WeeklyRange = true;
			Recurrence.DayList = "NNYNYNN"; //Tuesday and Thursday

			Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2016, 3, 6, 2, 0, 0); //a sunday. randomly increasing hours to test this scenario is handled correctly
			Recurrence.scheduleTask.S5_DailyStartTime = ZDateTime.Empty;
			Recurrence.scheduleTask.S5_TaskPeriodCount = 1;

			//adding 8 hours to Utc to get local
			AssertEquals(new ZDateTime(2016, 3, 8), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate().AddHours(8)));
			AssertEquals(new ZDateTime(2016, 3, 10), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate().AddHours(8)));
			AssertEquals(new ZDateTime(2016, 3, 15), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate().AddHours(8)));
			AssertEquals(new ZDateTime(2016, 3, 17), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate().AddHours(8)));
		}

		[TestUtcOffset(-8, 0, 0)]
		public void TestCalculateNextWeeklyScheduleDate_InNegativeUtcOffset_WithEmptyDailyStartTime()
		{
			Recurrence.WeeklyRange = true;
			Recurrence.DayList = "NNYNYNN"; //Tuesday and Thursday

			Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2016, 3, 6, 3, 0, 0); //a sunday. randomly increasing hours to test this scenario is handled correctly
			Recurrence.scheduleTask.S5_DailyStartTime = ZDateTime.Empty;
			Recurrence.scheduleTask.S5_TaskPeriodCount = 1;

			//adding -8 hours to Utc to get local
			AssertEquals(new ZDateTime(2016, 3, 8), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate().AddHours(-8)));
			AssertEquals(new ZDateTime(2016, 3, 10), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate().AddHours(-8)));
			AssertEquals(new ZDateTime(2016, 3, 15), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate().AddHours(-8)));
			AssertEquals(new ZDateTime(2016, 3, 17), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate().AddHours(-8)));
		}

		[TestUtcOffset(0, 0, 0)]
		public void TestCalculateNextWeeklyScheduleDate_InNoUtcOffset_WithValidDailyStartTime()
		{
			Recurrence.WeeklyRange = true;
			Recurrence.DayList = "NNYNYNN"; //Tuesday and Thursday

			Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2016, 3, 6, 4, 0, 0); //a sunday. randomly increasing hours to test this scenario is handled correctly
			Recurrence.scheduleTask.S5_DailyStartTime = ZDateTime.MinSmallDateTimeValue.Date.AddHours(12);
			Recurrence.scheduleTask.S5_TaskPeriodCount = 1;

			AssertEquals(new ZDateTime(2016, 3, 8, 12, 0, 0), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate()));
			AssertEquals(new ZDateTime(2016, 3, 10, 12, 0, 0), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate()));
			AssertEquals(new ZDateTime(2016, 3, 15, 12, 0, 0), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate()));
			AssertEquals(new ZDateTime(2016, 3, 17, 12, 0, 0), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate()));
		}

		[TestUtcOffset(8, 0, 0)]
		public void TestCalculateNextWeeklyScheduleDate_InPositiveUtcOffset_WithValidDailyStartTime()
		{
			Recurrence.WeeklyRange = true;
			Recurrence.DayList = "NNYNYNN"; //Tuesday and Thursday

			Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2016, 3, 6, 5, 0, 0); //a sunday. randomly increasing hours to test this scenario is handled correctly
			Recurrence.scheduleTask.S5_DailyStartTime = ZDateTime.MinSmallDateTimeValue.Date.AddHours(20); //12:00 UTC is 20:00 local
			Recurrence.scheduleTask.S5_TaskPeriodCount = 1;

			//adding 8 hours to Utc to get local
			AssertEquals(new ZDateTime(2016, 3, 8, 20, 0, 0), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate().AddHours(8)));
			AssertEquals(new ZDateTime(2016, 3, 10, 20, 0, 0), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate().AddHours(8)));
			AssertEquals(new ZDateTime(2016, 3, 15, 20, 0, 0), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate().AddHours(8)));
			AssertEquals(new ZDateTime(2016, 3, 17, 20, 0, 0), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate().AddHours(8)));
		}

		[TestUtcOffset(-8, 0, 0)]
		public void TestCalculateNextWeeklyScheduleDate_InNegativeUtcOffset_WithValidDailyStartTime()
		{
			Recurrence.WeeklyRange = true;
			Recurrence.DayList = "NNYNYNN"; //Tuesday and Thursday

			Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2016, 3, 6, 6, 0, 0); //a sunday. randomly increasing hours to test this scenario is handled correctly
			Recurrence.scheduleTask.S5_DailyStartTime = ZDateTime.MinSmallDateTimeValue.Date.AddHours(4); //12:00 UTC is 4:00 local
			Recurrence.scheduleTask.S5_TaskPeriodCount = 1;

			//adding -8 hours to Utc to get local
			AssertEquals(new ZDateTime(2016, 3, 8, 4, 0, 0), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate().AddHours(-8)));
			AssertEquals(new ZDateTime(2016, 3, 10, 4, 0, 0), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate().AddHours(-8)));
			AssertEquals(new ZDateTime(2016, 3, 15, 4, 0, 0), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate().AddHours(-8)));
			AssertEquals(new ZDateTime(2016, 3, 17, 4, 0, 0), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate().AddHours(-8)));
		}

		[TestUtcOffset(0, 0, 0)]
		public void TestCalculateNextDailyScheduleDate_WeekdaysOnly_InNoUtcOffset_WithEmptyDailyStartTime()
		{
			Recurrence.DailyRange = true;
			Recurrence.WeekDaysOnly = true;

			Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2016, 3, 6, 7, 0, 0); //a sunday. randomly increasing hours to test this scenario is handled correctly
			Recurrence.scheduleTask.S5_DailyStartTime = ZDateTime.Empty;
			Recurrence.scheduleTask.S5_TaskPeriodCount = 1;

			AssertEquals(new ZDateTime(2016, 3, 7), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate()));
			AssertEquals(new ZDateTime(2016, 3, 8), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate()));
			AssertEquals(new ZDateTime(2016, 3, 9), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate()));
			AssertEquals(new ZDateTime(2016, 3, 10), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate()));
			AssertEquals(new ZDateTime(2016, 3, 11), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate()));
			AssertEquals(new ZDateTime(2016, 3, 14), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate()));
		}

		[TestUtcOffset(8, 0, 0)]
		public void TestCalculateNextDailyScheduleDate_WeekdaysOnly_InPositiveUtcOffset_WithEmptyDailyStartTime()
		{
			Recurrence.DailyRange = true;
			Recurrence.WeekDaysOnly = true;

			Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2016, 3, 6, 8, 0, 0); //a sunday. randomly increasing hours to test this scenario is handled correctly
			Recurrence.scheduleTask.S5_DailyStartTime = ZDateTime.Empty;
			Recurrence.scheduleTask.S5_TaskPeriodCount = 1;

			//adding 8 hours to Utc to get local
			AssertEquals(new ZDateTime(2016, 3, 7), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate().AddHours(8)));
			AssertEquals(new ZDateTime(2016, 3, 8), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate().AddHours(8)));
			AssertEquals(new ZDateTime(2016, 3, 9), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate().AddHours(8)));
			AssertEquals(new ZDateTime(2016, 3, 10), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate().AddHours(8)));
			AssertEquals(new ZDateTime(2016, 3, 11), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate().AddHours(8)));
			AssertEquals(new ZDateTime(2016, 3, 14), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate().AddHours(8)));
		}

		[TestUtcOffset(-8, 0, 0)]
		public void TestCalculateNextDailyScheduleDate_WeekdaysOnly_InNegativeUtcOffset_WithEmptyDailyStartTime()
		{
			Recurrence.DailyRange = true;
			Recurrence.WeekDaysOnly = true;

			Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2016, 3, 6, 9, 0, 0); //a sunday. randomly increasing hours to test this scenario is handled correctly
			Recurrence.scheduleTask.S5_DailyStartTime = ZDateTime.Empty;
			Recurrence.scheduleTask.S5_TaskPeriodCount = 1;

			//adding -8 hours to Utc to get local
			AssertEquals(new ZDateTime(2016, 3, 7), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate().AddHours(-8)));
			AssertEquals(new ZDateTime(2016, 3, 8), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate().AddHours(-8)));
			AssertEquals(new ZDateTime(2016, 3, 9), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate().AddHours(-8)));
			AssertEquals(new ZDateTime(2016, 3, 10), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate().AddHours(-8)));
			AssertEquals(new ZDateTime(2016, 3, 11), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate().AddHours(-8)));
			AssertEquals(new ZDateTime(2016, 3, 14), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate().AddHours(-8)));
		}

		[TestUtcOffset(0, 0, 0)]
		public void TestCalculateNextDailyScheduleDate_WeekdaysOnly_InNoUtcOffset_WithValidDailyStartTime()
		{
			Recurrence.DailyRange = true;
			Recurrence.WeekDaysOnly = true;

			Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2016, 3, 6, 10, 0, 0); //a sunday. randomly increasing hours to test this scenario is handled correctly
			Recurrence.scheduleTask.S5_DailyStartTime = ZDateTime.MinSmallDateTimeValue.Date.AddHours(12);
			Recurrence.scheduleTask.S5_TaskPeriodCount = 1;

			AssertEquals(new ZDateTime(2016, 3, 7, 12, 0, 0), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate()));
			AssertEquals(new ZDateTime(2016, 3, 8, 12, 0, 0), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate()));
			AssertEquals(new ZDateTime(2016, 3, 9, 12, 0, 0), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate()));
			AssertEquals(new ZDateTime(2016, 3, 10, 12, 0, 0), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate()));
			AssertEquals(new ZDateTime(2016, 3, 11, 12, 0, 0), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate()));
			AssertEquals(new ZDateTime(2016, 3, 14, 12, 0, 0), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate()));
		}

		[TestUtcOffset(8, 0, 0)]
		public void TestCalculateNextDailyScheduleDate_WeekdaysOnly_InPositiveUtcOffset_WithValidDailyStartTime()
		{
			Recurrence.DailyRange = true;
			Recurrence.WeekDaysOnly = true;

			Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2016, 3, 6, 11, 0, 0); //a sunday. randomly increasing hours to test this scenario is handled correctly
			Recurrence.scheduleTask.S5_DailyStartTime = ZDateTime.MinSmallDateTimeValue.Date.AddHours(20); //12:00 UTC is 20:00 local
			Recurrence.scheduleTask.S5_TaskPeriodCount = 1;

			//adding 8 hours to Utc to get local
			AssertEquals(new ZDateTime(2016, 3, 7, 20, 0, 0), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate().AddHours(8)));
			AssertEquals(new ZDateTime(2016, 3, 8, 20, 0, 0), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate()).AddHours(8));
			AssertEquals(new ZDateTime(2016, 3, 9, 20, 0, 0), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate().AddHours(8)));
			AssertEquals(new ZDateTime(2016, 3, 10, 20, 0, 0), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate().AddHours(8)));
			AssertEquals(new ZDateTime(2016, 3, 11, 20, 0, 0), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate().AddHours(8)));
			AssertEquals(new ZDateTime(2016, 3, 14, 20, 0, 0), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate().AddHours(8)));
		}

		[TestUtcOffset(-8, 0, 0)]
		public void TestCalculateNextDailyScheduleDate_WeekdaysOnly_InNegativeUtcOffset_WithValidDailyStartTime()
		{
			Recurrence.DailyRange = true;
			Recurrence.WeekDaysOnly = true;

			Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2016, 3, 6, 12, 0, 0); //a sunday. randomly increasing hours to test this scenario is handled correctly
			Recurrence.scheduleTask.S5_DailyStartTime = ZDateTime.MinSmallDateTimeValue.Date.AddHours(4); //12:00 UTC is 4:00 local
			Recurrence.scheduleTask.S5_TaskPeriodCount = 1;

			//adding -8 hours to Utc to get local
			AssertEquals(new ZDateTime(2016, 3, 7, 4, 0, 0), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate().AddHours(-8)));
			AssertEquals(new ZDateTime(2016, 3, 8, 4, 0, 0), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate()).AddHours(-8));
			AssertEquals(new ZDateTime(2016, 3, 9, 4, 0, 0), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate().AddHours(-8)));
			AssertEquals(new ZDateTime(2016, 3, 10, 4, 0, 0), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate().AddHours(-8)));
			AssertEquals(new ZDateTime(2016, 3, 11, 4, 0, 0), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate().AddHours(-8)));
			AssertEquals(new ZDateTime(2016, 3, 14, 4, 0, 0), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate().AddHours(-8)));
		}

		[TestUtcOffset(0, 0, 0)]
		public void TestCalculateNextMonthlyScheduleDateWithDayOfMonth()
		{
			Recurrence.MonthlyRange = true;
			Recurrence.DayOfMonth = 2;
			Recurrence.WeekDayOccurrenceNumber = 0;
			Recurrence.TaskPeriodCount = 1;

			Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2010, 11, 2);
			Recurrence.scheduleTask.S5_DailyStartTime = new ZDateTime(1, 1, 1, 9, 30, 0);

			AssertEquals(new ZDateTime(2010, 12, 2, 9, 30, 0), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate()));
			AssertEquals(new ZDateTime(2011, 1, 2, 9, 30, 0), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate()));

			Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2010, 11, 4);

			AssertEquals(new ZDateTime(2010, 12, 2, 9, 30, 0), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate()));
			AssertEquals(new ZDateTime(2011, 1, 2, 9, 30, 0), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate()));

			Recurrence.StartDateLocal = new ZDateTime(2011, 1, 1);
			Recurrence.DayOfMonth = 31;
			Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2011, 1, 31);
			AssertEquals(new ZDateTime(2011, 2, 28, 9, 30, 0), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate()));
			AssertEquals(new ZDateTime(2011, 3, 31, 9, 30, 0), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate()));
			AssertEquals(new ZDateTime(2011, 4, 30, 9, 30, 0), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate()));
			AssertEquals(new ZDateTime(2011, 5, 31, 9, 30, 0), (Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate()));
		}

		[TestUtcOffset(-4, 0, 0)]
		public void TestCalculateNextMonthlyScheduleDateByLocalTime()
		{
			Recurrence.MonthlyRange = true;
			Recurrence.DayOfMonth = 10;
			Recurrence.scheduleTask.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2010, 9, 11, 1, 0, 0); // 2010-09-10 21:00 Local Time
			Recurrence.scheduleTask.S5_DailyStartTime = new ZDateTime(1, 1, 1, 21, 0, 0);

			Recurrence.WeekDayOccurrenceNumber = 0;
			Recurrence.TaskPeriodCount = 2;
			Recurrence.DayNumber = 99;
			AssertEquals("CalculateNextScheduleDate()", new ZDateTime(2010, 12, 1, 1, 0, 0), Recurrence.CalculateNextScheduleDate()); // 2010-11-30 21:00 Local Time

			Recurrence.DayNumber = 3;
			AssertEquals("CalculateNextScheduleDate()", new ZDateTime(2010, 11, 11, 1, 0, 0), Recurrence.CalculateNextScheduleDate()); // 2010-11-10 21:00 Local Time

			Recurrence.WeekDayOccurrenceNumber = 3;
			AssertEquals("CalculateNextScheduleDate()", new ZDateTime(2010, 10, 20, 1, 0, 0), Recurrence.CalculateNextScheduleDate()); // 2010-09-19 21:00 Local Time
		}

		[TestDate(2013, 6, 1, 11, 20, 00)]
		[TestUtcOffset(10, 0, 0)]
		public void TestCalculateNextMonthlyScheduleDateOn1stDay()
		{
			Recurrence.MonthlyRange = true;
			Recurrence.DayOfMonth = 1;
			Recurrence.TaskPeriodCount = 1;
			Recurrence.WeekDayOccurrenceNumber = 0;
			Recurrence.scheduleTask.S5_StartDate = new ZDateTime(2013, 5, 31, 14, 00, 00);
			Recurrence.scheduleTask.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2013, 05, 31, 14, 00, 00); // 2013-06-01 00:00 local time
			Recurrence.scheduleTask.S5_DailyStartTime = ZDateTime.Empty;

			AssertEquals(new ZDateTime(2013, 6, 30, 14, 0, 0), Recurrence.CalculateNextScheduleDate()); // 2013-07-01 00:00 local time
		}

		[TestDate(2013, 6, 1, 11, 20, 00)]
		[TestUtcOffset(10, 0, 0)]
		public void TestCalculateNextScheduleDate_Monthly_WeekDayOccurence()
		{
			Recurrence.MonthlyRange = true;
			Recurrence.DayOfMonth = 1;
			Recurrence.TaskPeriodCount = 1;
			Recurrence.DayNumber = 1;
			Recurrence.WeekDayOccurrenceNumber = 1;
			Recurrence.scheduleTask.S5_StartDate = new ZDateTime(2013, 5, 31, 14, 00, 00);
			Recurrence.scheduleTask.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2013, 05, 31, 14, 00, 00); // 2013-06-01 00:00 local time
			Recurrence.scheduleTask.S5_DailyStartTime = ZDateTime.Empty;

			AssertEquals(new ZDateTime(2013, 7, 6, 14, 0, 0), Recurrence.CalculateNextScheduleDate()); // 2013-07-07 00:00 local time
		}

		[TestDate(2013, 6, 1, 11, 20, 00)]
		[TestUtcOffset(10, 0, 0)]
		public void TestCalculateNextScheduleDate_Monthly_LastDayOccurence()
		{
			Recurrence.MonthlyRange = true;
			Recurrence.DayOfMonth = 1;
			Recurrence.TaskPeriodCount = 1;
			Recurrence.DayNumber = 99;
			Recurrence.scheduleTask.S5_StartDate = new ZDateTime(2013, 5, 31, 14, 00, 00);
			Recurrence.scheduleTask.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2013, 05, 31, 14, 00, 00); // 2013-06-01 00:00 local time
			Recurrence.scheduleTask.S5_DailyStartTime = ZDateTime.Empty;

			AssertEquals(new ZDateTime(2013, 7, 30, 14, 0, 0), Recurrence.CalculateNextScheduleDate()); // 2013-07-31 00:00 local time
		}

		[TestUtcOffset(0, 0, 0)]
		public void TestCalculateNextYearlyScheduleDate()
		{
			Recurrence.YearlyRange = true;
			Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2006, 5, 10);
			Recurrence.scheduleTask.S5_DailyStartTime = new ZDateTime(1, 1, 1, 17, 30, 0);

			Recurrence.MonthNumber = 3;
			Recurrence.DayOfMonth = 10;
			Recurrence.WeekDayOccurrenceNumber = 0;
			AssertEquals("CalculateNextScheduleDate()", new ZDateTime(2007, 3, 10, 17, 30, 0), Recurrence.CalculateNextScheduleDate());

			Recurrence.WeekDayOccurrenceNumber = 2;
			Recurrence.DayNumber = 6;
			AssertEquals("CalculateNextScheduleDate()", new ZDateTime(2007, 3, 9, 17, 30, 0), Recurrence.CalculateNextScheduleDate());
		}

		[TestUtcOffset(0, 0, 0)]
		public void TestCalculateNextHourlyScheduleDate()
		{
			Recurrence.TaskPeriod = ScheduleRecurrenceType.Hourly;
			Recurrence.StartDateLocal = new ZDateTime(2007, 6, 15, 0, 0, 0);
			Recurrence.scheduleTask.S5_DailyStartTime = new ZDateTime(1900, 1, 1, 13, 0, 0);
			Recurrence.scheduleTask.S5_DailyEndTime = new ZDateTime(1900, 1, 1, 18, 0, 0);
			// Set next run time to Saturday
			Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2007, 6, 16, 14, 20, 0);

			Recurrence.WeekDaysOnly = true;
			Recurrence.TaskPeriodCount = 3;
			AssertEquals("CalculateNextScheduleDate()", new ZDateTime(2007, 6, 16, 17, 20, 0), Recurrence.CalculateNextScheduleDate());

			Recurrence.WeekDaysOnly = false;
			Recurrence.TaskPeriodCount = 2;
			AssertEquals("CalculateNextScheduleDate()", new ZDateTime(2007, 6, 16, 16, 20, 0), Recurrence.CalculateNextScheduleDate());

			Recurrence.WeekDaysOnly = false;
			Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2007, 6, 16, 17, 0, 0);
			AssertEquals("CalculateNextScheduleDate()", new ZDateTime(2007, 6, 17, 13, 0, 0), Recurrence.CalculateNextScheduleDate());

			Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2007, 6, 16, 2, 0, 0);
			AssertEquals("CalculateNextScheduleDate()", new ZDateTime(2007, 6, 16, 13, 0, 0), Recurrence.CalculateNextScheduleDate());

			Recurrence.scheduleTask.S5_DailyStartTime = new ZDateTime(1900, 1, 1, 21, 0, 0);
			Recurrence.scheduleTask.S5_DailyEndTime = new ZDateTime(1900, 1, 1, 8, 0, 0);
			Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2007, 6, 16, 2, 0, 0);
			AssertEquals("CalculateNextScheduleDate()", new ZDateTime(2007, 6, 16, 4, 0, 0), Recurrence.CalculateNextScheduleDate());

			Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2007, 6, 16, 9, 0, 0);
			AssertEquals("CalculateNextScheduleDate()", new ZDateTime(2007, 6, 16, 21, 0, 0), Recurrence.CalculateNextScheduleDate());

			Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2007, 6, 16, 22, 0, 0);
			AssertEquals("CalculateNextScheduleDate()", new ZDateTime(2007, 6, 17, 0, 0, 0), Recurrence.CalculateNextScheduleDate());
		}

		[TestUtcOffset(8, 0, 0)]
		public void TestCalculateNextScheduleDateWithDayLimitsAndDST()
		{
			Recurrence.TaskPeriod = ScheduleRecurrenceType.Hourly;
			Recurrence.TaskPeriodCount = 5;
			Recurrence.WeekDaysOnly = false;

			Recurrence.scheduleTask.CalcDailyStartTimeLocal = new ZDateTime(1900, 1, 1, 0, 30, 00);
			Recurrence.scheduleTask.CalcDailyEndTimeLocal = new ZDateTime(1900, 1, 1, 15, 30, 00);
			Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2011, 12, 13, 18, 00, 00);

			AssertEquals(new ZDateTime(2011, 12, 14, 00, 30, 00), Recurrence.GetLocalDate(Recurrence.CalculateNextScheduleDate()));

			TestUtcOffsetAttribute.Time = new TimeSpan(7, 0, 0);

			Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2011, 12, 13, 18, 00, 00);

			AssertEquals(new ZDateTime(1900, 1, 1, 0, 30, 00), Recurrence.scheduleTask.CalcDailyStartTimeLocal);
			AssertEquals(new ZDateTime(1900, 1, 1, 15, 30, 00), Recurrence.scheduleTask.CalcDailyEndTimeLocal);
			AssertEquals(new ZDateTime(2011, 12, 14, 00, 30, 00), Recurrence.GetLocalDate(Recurrence.CalculateNextScheduleDate()));
		}

		[TestDate(2012, 1, 1)]
		public void TestCalculateNextScheduleDateWithDayLimitsAndDifferentZones()
		{
			GlbBranch tokyoBranch = Factory.New<GlbBranch>();
			tokyoBranch.GB_RL_NKHomePort = "JPNRT";
			tokyoBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			tokyoBranch.GB_Code = "**T";

			GlbBranch sydneyBranch = Factory.New<GlbBranch>();
			sydneyBranch.GB_RL_NKHomePort = "AUSYD";
			sydneyBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			sydneyBranch.GB_Code = "**S";

			Factory.Save();

			using (new TemporaryUserContext { BranchPK = tokyoBranch.PK.ToGuid() }.Set())
			{
				Recurrence.TaskPeriod = ScheduleRecurrenceType.Hourly;
				Recurrence.TaskPeriodCount = 5;
				Recurrence.WeekDaysOnly = false;

				Recurrence.scheduleTask.S5_GB = sydneyBranch.PK;
				Recurrence.scheduleTask.CalcDailyStartTimeLocal = new ZDateTime(1900, 1, 1, 00, 30, 00);
				Recurrence.scheduleTask.CalcDailyEndTimeLocal = new ZDateTime(1900, 1, 1, 16, 00, 00);
				Recurrence.scheduleTask.CalcNextRunTimeLocal = new ZDateTime(2032, 12, 13, 18, 00, 00); // Future year so unit test won't outdate

				ZDateTime nextRunTime;
				using (new TemporaryUserContext { BranchPK = sydneyBranch.PK.ToGuid() }.Set())
				{
					nextRunTime = Recurrence.CalculateNextScheduleDate();
				}

				AssertEquals(new DateTime(2032, 12, 14, 00, 30, 00), Env.Time.GetLocalTimeFromUtc(nextRunTime.ToDateTime()));
			}
		}

		[TestUtcOffset(0, 0, 0)]
		public void TestCalculateNextMinutePeriodScheduleDate()
		{
			Recurrence.TaskPeriod = ScheduleRecurrenceType.Minute;
			Recurrence.StartDateLocal = new ZDateTime(2007, 6, 15, 0, 0, 0);
			Recurrence.scheduleTask.S5_DailyStartTime = new ZDateTime(1900, 1, 1, 16, 30, 0);
			Recurrence.scheduleTask.S5_DailyEndTime = new ZDateTime(1900, 1, 1, 19, 30, 0);
			Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2007, 6, 16, 18, 30, 0);

			Recurrence.WeekDaysOnly = true;
			Recurrence.TaskPeriodCount = 20;
			AssertEquals("CalculateNextScheduleDate()", new ZDateTime(2007, 6, 16, 18, 50, 0), Recurrence.CalculateNextScheduleDate());

			Recurrence.WeekDaysOnly = false;
			Recurrence.TaskPeriodCount = 45;
			AssertEquals("CalculateNextScheduleDate()", new ZDateTime(2007, 6, 16, 19, 15, 0), Recurrence.CalculateNextScheduleDate());

			Recurrence.WeekDaysOnly = false;
			Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2007, 6, 16, 19, 30, 0);
			Recurrence.TaskPeriodCount = 30;
			AssertEquals("CalculateNextScheduleDate()", new ZDateTime(2007, 6, 17, 16, 30, 0), Recurrence.CalculateNextScheduleDate());

			Recurrence.WeekDaysOnly = true;
			Recurrence.TaskPeriodCount = 20;
			Recurrence.scheduleTask.S5_DailyStartTime = ZDateTime.Empty;
			Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2007, 6, 16, 18, 30, 0);
			AssertEquals("CalculateNextScheduleDate()", new ZDateTime(2007, 6, 16, 18, 50, 0), Recurrence.CalculateNextScheduleDate());
		}

		[TestUtcOffset(10, 0, 0)]
		public void TestCalculateNextMinutePeriodScheduleDate2()
		{
			Recurrence.TaskPeriod = ScheduleRecurrenceType.Minute;
			Recurrence.StartDateLocal = new ZDateTime(2010, 10, 12, 0, 0, 0);
			Recurrence.scheduleTask.S5_DailyStartTime = new ZDateTime(1900, 1, 1, 5, 0, 0); // UTC 5am - 10 GMT
			Recurrence.scheduleTask.S5_DailyEndTime = new ZDateTime(1900, 1, 1, 23, 59, 0); // UTC 23:59pm - 10 GMT
			Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2010, 10, 12, 14, 0, 0); // current local

			Recurrence.WeekDaysOnly = false;
			Recurrence.TaskPeriodCount = 20;
			var date = Env.Time.GetLocalTimeFromUtc(Recurrence.CalculateNextScheduleDate().ToDateTime());
			AssertEquals("CalculateNextScheduleDate()", new DateTime(2010, 10, 12, 14, 20, 0), date);
		}

		[TestUtcOffset(0, 0, 0)]
		public void TestCalculateNextSecondPeriodScheduleDate()
		{
			Recurrence.TaskPeriod = ScheduleRecurrenceType.Second;
			Recurrence.StartDateLocal = new ZDateTime(2007, 6, 15, 0, 0, 0);
			Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2007, 6, 16, 18, 30, 0);
			Recurrence.WeekDaysOnly = false;
			Recurrence.TaskPeriodCount = 45;
			AssertEquals("CalculateNextScheduleDate()", new ZDateTime(2007, 6, 16, 18, 30, 45), Recurrence.CalculateNextScheduleDate());
		}

		[ExpectExceptionMessage(typeof(InvalidOperationException), "CalculateNextScheduleDate cannot be called when TaskPeriod is invalid.")]
		public void TestCalculateNextScheduleDateWhenTaskPeriodIsInvalid()
		{
			Recurrence.TaskPeriod = "";
			Recurrence.CalculateNextScheduleDate();
		}

		[TestDate(2015, 7, 6)]
		public void TestCalculateNextScheduleDate_S5_NextScheduledPrintRunTimeInValid()
		{
			var taskCount = 5;
			Recurrence.TaskPeriodCount = taskCount;

			Recurrence.TaskPeriod = ScheduleRecurrenceType.Hourly;
			var dateTime = new ZDateTime(2015, 6, 9, 10, 0, 0);
			Recurrence.scheduleTask.S5_NextScheduledPrintRunTimeUtc = dateTime;
			Assert("S5_NextScheduledPrintRunTimeUtc should be valid", scheduleTask.S5_NextScheduledPrintRunTimeUtc.IsValid);
			AssertEquals("CalculateNextScheduleDate() should match the expected result", dateTime.AddHours(taskCount), Recurrence.CalculateNextScheduleDate());

			Recurrence.scheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Invalid;
			Assert("S5_NextScheduledPrintRunTimeUtc should be invalid", !scheduleTask.S5_NextScheduledPrintRunTimeUtc.IsValid);
			AssertEquals("CalculateNextScheduleDate() should match the expected result", ZDateTime.Now.AddHours(taskCount).ToShortDateString(), Recurrence.CalculateNextScheduleDate().ToShortDateString());

			Recurrence.TaskPeriod = ScheduleRecurrenceType.Minute;
			Recurrence.scheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Empty;
			Assert("S5_NextScheduledPrintRunTimeUtc should be invalid", !scheduleTask.S5_NextScheduledPrintRunTimeUtc.IsValid);
			AssertEquals("CalculateNextScheduleDate() should match the expected result", ZDateTime.Now.AddHours(taskCount).ToShortDateString(), Recurrence.CalculateNextScheduleDate().ToShortDateString());
		}

		[TestDate(2024, 1, 15, 0, 0, 0)]
		public void TestCalculateNewMonthlyLocalDate()
		{
			Recurrence.TaskPeriodCount = 1;
			Recurrence.MonthNumber = 0;
			Recurrence.DayOfMonth = 17;
			Recurrence.TaskPeriod = ScheduleRecurrenceType.Monthly;
			AssertEquals("CalculateNextScheduleDate()", new ZDateTime(2024, 1, 17, 0, 0, 0),
				Recurrence.CalculateNextScheduleDate(new ZDateTime(2024, 1, 15, 0, 0, 0), true));
			Recurrence.DayOfMonth = 14;
			AssertEquals("CalculateNextScheduleDate()", new ZDateTime(2024, 2, 14, 0, 0, 0),
				Recurrence.CalculateNextScheduleDate(new ZDateTime(2024, 1, 15, 0, 0, 0), true));
			Recurrence.DayOfMonth = 17;
			AssertEquals("CalculateNextScheduleDate()", new ZDateTime(2024, 2, 17, 0, 0, 0),
				Recurrence.CalculateNextScheduleDate(new ZDateTime(2024, 2, 20, 0, 0, 0), true));
		}

		[TestDate(2016, 2, 3, 0, 0, 0)]
		public void TestCalculateNextScheduleDate_WhenMonthNumberIsInvalid()
		{
			Recurrence.TaskPeriodCount = 1;
			Recurrence.MonthNumber = 0;

			// Issue WI00096854: ArgumentOutOfRange exception when getting yearly schedule w/o setting MonthNumber property.
			Recurrence.TaskPeriod = ScheduleRecurrenceType.Yearly;
			AssertEquals("CalculateNextScheduleDate()", new ZDateTime(2017, 2, 3, 0, 0, 0), Recurrence.CalculateNextScheduleDate());
			AssertEquals("CalculateNextScheduleDate()", "StmScheduleTaskRecurrence.GetValidMonthNumber", ErrorReporter.LastKeyReported);
			AssertEquals("CalculateNextScheduleDate()", "Month number '0' is invalid, falling back to current month '2'; Task Period = 'Y', Start Date = '03-Feb-16 00:00:00'", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			Recurrence.TaskPeriod = ScheduleRecurrenceType.Monthly;
			AssertEquals("CalculateNextScheduleDate()", new ZDateTime(2016, 3, 3, 0, 0, 0), Recurrence.CalculateNextScheduleDate());

			Recurrence.TaskPeriod = ScheduleRecurrenceType.Weekly;
			Recurrence.DayList = "NNNYNNN";
			AssertEquals("CalculateNextScheduleDate()", new ZDateTime(2016, 2, 10, 0, 0, 0), Recurrence.CalculateNextScheduleDate());

			Recurrence.TaskPeriod = ScheduleRecurrenceType.Daily;
			AssertEquals("CalculateNextScheduleDate()", new ZDateTime(2016, 2, 4, 0, 0, 0), Recurrence.CalculateNextScheduleDate());

			Recurrence.TaskPeriod = ScheduleRecurrenceType.Hourly;
			AssertEquals("CalculateNextScheduleDate()", new ZDateTime(2016, 2, 3, 1, 0, 0), Recurrence.CalculateNextScheduleDate());

			Recurrence.TaskPeriod = ScheduleRecurrenceType.Minute;
			AssertEquals("CalculateNextScheduleDate()", new ZDateTime(2016, 2, 3, 0, 1, 0), Recurrence.CalculateNextScheduleDate());

			Recurrence.TaskPeriod = ScheduleRecurrenceType.Second;
			AssertEquals("CalculateNextScheduleDate()", new ZDateTime(2016, 2, 3, 0, 0, 1), Recurrence.CalculateNextScheduleDate());
		}

		[TestUtcOffset(0, 0, 0)]
		public void TestCalculateNewStartDate_WhenMonthNumberIsInvalid()
		{
			Recurrence.TaskPeriodCount = 1;
			Recurrence.StartDateLocal = new ZDateTime(2016, 2, 3, 0, 0, 0);
			Recurrence.MonthNumber = 0;

			Recurrence.TaskPeriod = ScheduleRecurrenceType.Yearly;
			AssertEquals("CalculateNewStartDate()", new ZDateTime(2016, 2, 3, 0, 0, 0), Recurrence.CalculateNewStartDate());
			AssertEquals("CalculateNewStartDate()", "StmScheduleTaskRecurrence.GetValidMonthNumber", ErrorReporter.LastKeyReported);
			AssertEquals("CalculateNewStartDate()", "Month number '0' is invalid, falling back to current month '2'; Task Period = 'Y', Start Date = '03-Feb-16 00:00:00'", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			Recurrence.TaskPeriod = ScheduleRecurrenceType.Monthly;
			AssertEquals("CalculateNewStartDate()", new ZDateTime(2016, 2, 3, 0, 0, 0), Recurrence.CalculateNewStartDate());

			Recurrence.TaskPeriod = ScheduleRecurrenceType.Weekly;
			Recurrence.DayList = "NNNYNNN";
			AssertEquals("CalculateNewStartDate()", new ZDateTime(2016, 2, 3, 0, 0, 0), Recurrence.CalculateNewStartDate());

			Recurrence.TaskPeriod = ScheduleRecurrenceType.Daily;
			AssertEquals("CalculateNewStartDate()", new ZDateTime(2016, 2, 3, 0, 0, 0), Recurrence.CalculateNewStartDate());

			Recurrence.TaskPeriod = ScheduleRecurrenceType.Hourly;
			AssertEquals("CalculateNewStartDate()", new ZDateTime(2016, 2, 3, 0, 0, 0), Recurrence.CalculateNewStartDate());

			Recurrence.TaskPeriod = ScheduleRecurrenceType.Minute;
			AssertEquals("CalculateNewStartDate()", new ZDateTime(2016, 2, 3, 0, 0, 0), Recurrence.CalculateNewStartDate());

			Recurrence.TaskPeriod = ScheduleRecurrenceType.Second;
			AssertEquals("CalculateNewStartDate()", new ZDateTime(2016, 2, 3, 0, 0, 0), Recurrence.CalculateNewStartDate());
		}

		[TestUtcOffset(0, 0, 0)]
		public void TestHourlyScheduleUnaffectedByWeekDaysOnly()
		{
			Recurrence.TaskPeriodCount = 1;
			Recurrence.StartDateLocal = new ZDateTime(2000, 1, 1, 1, 0, 0);
			Recurrence.NextScheduledPrintRunTimeLocal = new ZDateTime(2000, 1, 1, 1, 0, 0);
			Recurrence.MonthNumber = 0;
			Recurrence.TaskPeriod = ScheduleRecurrenceType.Hourly;
			Recurrence.WeekDaysOnly = true;

			TestDateAttribute.Date = new DateTime(2000, 1, 1, 1, 0, 0);
			for (int i = 0; i < 200; ++i)
			{
				Recurrence.NextScheduledPrintRunTimeLocal = Recurrence.CalculateNextScheduleDate();
				TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
				AssertEquals(TestDateAttribute.Date, Recurrence.NextScheduledPrintRunTimeLocal.ToDateTime());
			}
		}

		#region Implementation

		StmScheduleTask scheduleTask;
		StmScheduleTaskRecurrence recurrence;

		StmScheduleTask ScheduleTask
		{
			get
			{
				if (scheduleTask == null)
				{
					scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();
				}
				return scheduleTask;
			}
		}

		StmScheduleTaskRecurrence Recurrence
		{
			get
			{
				if (recurrence == null)
				{
					recurrence = (StmScheduleTaskRecurrence)GetNewBusinessObject();
				}
				return recurrence;
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new StmScheduleTaskRecurrence(ScheduleTask);
		}

		void AssertRecurrenceRanges(bool accounting, bool daily, bool monthly, bool weekly, bool yearly, bool hourly, bool minute, bool second)
		{
			AssertEquals("AccountingRange", accounting, Recurrence.AccountingRange);
			AssertEquals("DailyRange", daily, Recurrence.DailyRange);
			AssertEquals("MonthlyRange", monthly, Recurrence.MonthlyRange);
			AssertEquals("WeeklyRange", weekly, Recurrence.WeeklyRange);
			AssertEquals("YearlyRange", yearly, Recurrence.YearlyRange);
			AssertEquals("HourlyRange", hourly, Recurrence.HourlyRange);
			AssertEquals("MinuteRange", minute, Recurrence.MinuteRange);
			AssertEquals("SecondRange", second, Recurrence.SecondRange);
		}

		void AssertWeekDays(bool sunday, bool monday, bool tuesday, bool wednesday, bool thursday, bool friday, bool saturday)
		{
			AssertEquals("Sunday", sunday, Recurrence.Sunday);
			AssertEquals("Monday", monday, Recurrence.Monday);
			AssertEquals("Tuesday", tuesday, Recurrence.Tuesday);
			AssertEquals("Wednesday", wednesday, Recurrence.Wednesday);
			AssertEquals("Thursday", thursday, Recurrence.Thursday);
			AssertEquals("Friday", friday, Recurrence.Friday);
			AssertEquals("Saturday", saturday, Recurrence.Saturday);
		}

		void AssertOptionsInRanges(bool accounting, bool monthly, bool yearly)
		{
			AssertEquals("AccountingDay", accounting, Recurrence.AccountingDay);
			AssertEquals("AccountingWeekDay", !accounting, Recurrence.AccountingWeekDay);
			AssertEquals("MonthlyDay", monthly, Recurrence.MonthlyDay);
			AssertEquals("MonthlyWeekDay", !monthly, Recurrence.MonthlyWeekDay);
			AssertEquals("YearlyEvery", yearly, Recurrence.YearlyEvery);
			AssertEquals("YearlyWeekDay", !yearly, Recurrence.YearlyWeekDay);
		}

		void TestIsRangeAndNotDayReadOnly(ZPropertyInfo propertyInfo)
		{
			string message = propertyInfo.Name + ".ReadOnly";
			PropertyInfo info = typeof(StmScheduleTaskRecurrence).GetProperty(propertyInfo.Name + "Info");

			Recurrence.DailyRange = true;
			Recurrence.DailyDay = false;
			AssertEquals(message, true, IsReadOnly(info));

			Recurrence.MonthlyRange = true;
			AssertEquals(message, false, IsReadOnly(info));

			Recurrence.DailyRange = true;
			Recurrence.DailyDay = true;
			AssertEquals(message, false, IsReadOnly(info));

			Recurrence.MonthlyRange = true;
			Recurrence.MonthlyDay = false;
			AssertEquals(message, true, IsReadOnly(info));

			Recurrence.DailyRange = true;
			AssertEquals(message, false, IsReadOnly(info));

			Recurrence.MonthlyRange = true;
			Recurrence.MonthlyDay = true;
			AssertEquals(message, false, IsReadOnly(info));

			Recurrence.AccountingRange = true;
			Recurrence.AccountingDay = false;
			AssertEquals(message, true, IsReadOnly(info));

			Recurrence.DailyRange = true;
			AssertEquals(message, false, IsReadOnly(info));

			Recurrence.AccountingRange = true;
			Recurrence.AccountingDay = true;
			AssertEquals(message, false, IsReadOnly(info));
		}

		void TestIsRangeAndNotWeekDayReadOnly(ZPropertyInfo propertyInfo, bool readOnlyWhenMatch)
		{
			string message = propertyInfo.Name + ".ReadOnly";
			PropertyInfo info = typeof(StmScheduleTaskRecurrence).GetProperty(propertyInfo.Name + "Info");

			Recurrence.YearlyRange = true;
			Recurrence.YearlyWeekDay = false;
			AssertEquals(message, readOnlyWhenMatch, IsReadOnly(info));

			Recurrence.TaskPeriod = "";
			AssertEquals(message, !readOnlyWhenMatch, IsReadOnly(info));

			Recurrence.YearlyRange = true;
			Recurrence.YearlyWeekDay = true;
			AssertEquals(message, !readOnlyWhenMatch, IsReadOnly(info));

			Recurrence.MonthlyRange = true;
			Recurrence.MonthlyWeekDay = false;
			AssertEquals(message, readOnlyWhenMatch, IsReadOnly(info));

			Recurrence.TaskPeriod = "";
			AssertEquals(message, !readOnlyWhenMatch, IsReadOnly(info));

			Recurrence.MonthlyRange = true;
			Recurrence.MonthlyWeekDay = true;
			AssertEquals(message, !readOnlyWhenMatch, IsReadOnly(info));

			Recurrence.AccountingRange = true;
			Recurrence.AccountingWeekDay = false;
			AssertEquals(message, readOnlyWhenMatch, IsReadOnly(info));

			Recurrence.TaskPeriod = "";
			AssertEquals(message, !readOnlyWhenMatch, IsReadOnly(info));

			Recurrence.AccountingRange = true;
			Recurrence.AccountingWeekDay = true;
			AssertEquals(message, !readOnlyWhenMatch, IsReadOnly(info));
		}

		void CreateAccPeriodTestData()
		{
			ZGuid companyPK = GlbCompany.CurrentCompany.PK;
			AccPeriodManagement accPeriod;
			AccPeriodManagementCollection collection = new AccPeriodManagementCollection(Factory);

			accPeriod = collection.AddNew();
			accPeriod.AM_GC_Company = companyPK;
			accPeriod.AM_Period = 200601;
			accPeriod.AM_Year = 2006;
			accPeriod.AM_StartDate = new ZDateTime(2006, 1, 1);
			accPeriod.AM_EndDate = new ZDateTime(2006, 3, 31);

			accPeriod = collection.AddNew();
			accPeriod.AM_GC_Company = companyPK;
			accPeriod.AM_Period = 200604;
			accPeriod.AM_Year = 2006;
			accPeriod.AM_StartDate = new ZDateTime(2006, 4, 1);
			accPeriod.AM_EndDate = new ZDateTime(2006, 6, 30);

			accPeriod = collection.AddNew();
			accPeriod.AM_GC_Company = companyPK;
			accPeriod.AM_Period = 200607;
			accPeriod.AM_Year = 2006;
			accPeriod.AM_StartDate = new ZDateTime(2006, 7, 1);
			accPeriod.AM_EndDate = new ZDateTime(2006, 9, 30);

			accPeriod = collection.AddNew();
			accPeriod.AM_GC_Company = companyPK;
			accPeriod.AM_Period = 200610;
			accPeriod.AM_Year = 2006;
			accPeriod.AM_StartDate = new ZDateTime(2006, 10, 1);
			accPeriod.AM_EndDate = new ZDateTime(2006, 12, 31);

			accPeriod = collection.AddNew();
			accPeriod.AM_GC_Company = companyPK;
			accPeriod.AM_Period = 200701;
			accPeriod.AM_Year = 2007;
			accPeriod.AM_StartDate = new ZDateTime(2007, 1, 1);
			accPeriod.AM_EndDate = new ZDateTime(2007, 3, 31);

			accPeriod = collection.AddNew();
			accPeriod.AM_GC_Company = companyPK;
			accPeriod.AM_Period = 200704;
			accPeriod.AM_Year = 2007;
			accPeriod.AM_StartDate = new ZDateTime(2007, 4, 1);
			accPeriod.AM_EndDate = new ZDateTime(2007, 6, 30);

			accPeriod = collection.AddNew();
			accPeriod.AM_GC_Company = companyPK;
			accPeriod.AM_Period = 200707;
			accPeriod.AM_Year = 2007;
			accPeriod.AM_StartDate = new ZDateTime(2007, 7, 1);
			accPeriod.AM_EndDate = new ZDateTime(2007, 9, 30);

			accPeriod = collection.AddNew();
			accPeriod.AM_GC_Company = companyPK;
			accPeriod.AM_Period = 200710;
			accPeriod.AM_Year = 2007;
			accPeriod.AM_StartDate = new ZDateTime(2007, 10, 1);
			accPeriod.AM_EndDate = new ZDateTime(2007, 12, 31);
		}

		bool IsReadOnly(PropertyInfo propertyInfo)
		{
			return ((ZPropertyInfo)propertyInfo.GetValue(Recurrence, null)).ReadOnly;
		}

		#endregion
	}
}
