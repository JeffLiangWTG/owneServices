using System;
using CargoWise.Types;
using Enterprise.Scheduler.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Scheduler.Business.Testing
{
	[TestedType(typeof(DateSchedule))]
	sealed class DateScheduleTest : ScheduleTestCase<DateSchedule>
	{
		public void TestToAndFromStorageValueForWeeklyThisWeek()
		{
			var dateSchedule = new DateSchedule();
			dateSchedule.ByWeek = ZBool.True;
			dateSchedule.PeriodScope = PeriodScopeList.Codes.This;
			dateSchedule.DayName = WeekDayList.Codes.Monday;

			var storageValue = dateSchedule.ToStorageValue();

			DateSchedule parsedStorageValue;
			Assert(DateSchedule.TryParse(storageValue, out parsedStorageValue));

			CombineAssertions(delegate
			{
				AssertEquals("parsedStorageValue.ByWeek", ZBool.True, parsedStorageValue.ByWeek);
				AssertEquals("parsedStorageValue.PeriodScope", PeriodScopeList.Codes.This, parsedStorageValue.PeriodScope);
				AssertEquals("parsedStorageValue.DayName", WeekDayList.Codes.Monday, parsedStorageValue.DayName);
			});
		}

		public void TestToAndFromStorageValueForWeeklyPreviousWeek()
		{
			var dateSchedule = new DateSchedule();
			dateSchedule.ByWeek = ZBool.True;
			dateSchedule.PeriodScope = PeriodScopeList.Codes.Previous;
			dateSchedule.PeriodCount = 2;
			dateSchedule.DayName = WeekDayList.Codes.Wednesday;

			var storageValue = dateSchedule.ToStorageValue();

			DateSchedule parsedStorageValue;
			Assert(DateSchedule.TryParse(storageValue, out parsedStorageValue));

			CombineAssertions(delegate
			{
				AssertEquals("parsedStorageValue.ByWeek", ZBool.True, parsedStorageValue.ByWeek);
				AssertEquals("parsedStorageValue.PeriodScope", PeriodScopeList.Codes.Previous, parsedStorageValue.PeriodScope);
				AssertEquals("parsedStorageValue.PeriodCount", new ZByte(2), parsedStorageValue.PeriodCount);
				AssertEquals("parsedStorageValue.DayName", WeekDayList.Codes.Wednesday, parsedStorageValue.DayName);
			});
		}

		public void TestToAndFromStorageValueForDay()
		{
			var dateSchedule = new DateSchedule();
			dateSchedule.ByDay = ZBool.True;
			dateSchedule.PeriodScope = PeriodScopeList.Codes.Next;
			dateSchedule.PeriodCount = 5;

			var storageValue = dateSchedule.ToStorageValue();

			DateSchedule parsedStorageValue;
			Assert(DateSchedule.TryParse(storageValue, out parsedStorageValue));

			AssertEquals("parsedStorageValue.ByDay", ZBool.True, parsedStorageValue.ByDay);
			AssertEquals("parsedStorageValue.PeriodScope", PeriodScopeList.Codes.Next, parsedStorageValue.PeriodScope);
			AssertEquals("parsedStorageValue.PeriodNumber", new ZByte(5), parsedStorageValue.PeriodCount);
		}

		public void TestToAndFromStorageValueForHourAndMinute()
		{
			var dateSchedule = new DateSchedule();
			dateSchedule.ByHourAndMinute = ZBool.True;
			dateSchedule.PeriodScope = PeriodScopeList.Codes.Next;
			dateSchedule.Hour = 999;
			dateSchedule.MinuteOfHour = 59;

			var storageValue = dateSchedule.ToStorageValue();

			DateSchedule parsedStorageValue;
			Assert(DateSchedule.TryParse(storageValue, out parsedStorageValue));

			AssertEquals("parsedStorageValue.ByHourAndMinute", ZBool.True, parsedStorageValue.ByHourAndMinute);
			AssertEquals("parsedStorageValue.PeriodScope", PeriodScopeList.Codes.Next, parsedStorageValue.PeriodScope);
			AssertEquals("parsedStorageValue.HourOfDay", (ZShort)999, parsedStorageValue.Hour);
			AssertEquals("parsedStorageValue.MinuteOfHour", (ZShort)59, parsedStorageValue.MinuteOfHour);
		}

		[TestDate(2008, 8, 8)]
		public void TestInvalidDateScheduleDescription()
		{
			var dateSchedule = new DateSchedule();
			dateSchedule.ScheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			dateSchedule.PeriodScope = ZString.Empty;

			AssertEquals("dateSchedule.Description", "(no date selected)", dateSchedule.Description);
		}

		[TestDate(2008, 8, 8)]
		public void TestDayPeriodInDifferentScopes()
		{
			var dateSchedule = new DateSchedule();
			dateSchedule.Period = ScheduleRecurrenceType.Daily;

			dateSchedule.ScheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();

			dateSchedule.PeriodScope = PeriodScopeList.Codes.This;
			dateSchedule.PeriodCount = 1;
			AssertEquals("dateSchedule.GetScheduleDate()", new ZDateTime(2008, 8, 8), dateSchedule.GetScheduleDate());

			dateSchedule.PeriodScope = PeriodScopeList.Codes.Next;
			dateSchedule.PeriodCount = 1;
			AssertEquals("dateSchedule.GetScheduleDate()", new ZDateTime(2008, 8, 9), dateSchedule.GetScheduleDate());

			dateSchedule.PeriodScope = PeriodScopeList.Codes.Previous;
			dateSchedule.PeriodCount = 1;
			AssertEquals("dateSchedule.GetScheduleDate()", new ZDateTime(2008, 8, 7), dateSchedule.GetScheduleDate());
		}

		[TestDate(2008, 8, 8, 0, 0, 0)]
		public void TestHourMinutePeriodInDifferentScopes()
		{
			var dateSchedule = new DateSchedule();
			dateSchedule.Period = ScheduleRecurrenceType.HourAndMinute;
			dateSchedule.Hour = 20;
			dateSchedule.MinuteOfHour = 30;

			dateSchedule.ScheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();

			dateSchedule.PeriodScope = PeriodScopeList.Codes.Next;
			AssertEquals("dateSchedule.GetScheduleDate()", new ZDateTime(2008, 8, 8, 20, 30, 0), dateSchedule.GetScheduleDate());
			AssertEquals("dateSchedule.PeriodCount", 1, dateSchedule.PeriodCount);

			dateSchedule.PeriodScope = PeriodScopeList.Codes.Previous;
			AssertEquals("dateSchedule.GetScheduleDate()", new ZDateTime(2008, 8, 7, 3, 30, 0), dateSchedule.GetScheduleDate());
		}

		[TestDate(2008, 8, 8)]
		public void TestDayPeriodDescriptionInDifferentScopes()
		{
			var dateSchedule = new DateSchedule();
			dateSchedule.Period = ScheduleRecurrenceType.Daily;
			dateSchedule.ScheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();

			CombineAssertions(() =>
			{
				dateSchedule.PeriodScope = PeriodScopeList.Codes.This;
				dateSchedule.PeriodCount = 1;
				AssertEquals("dateSchedule.Description",
@"The day when the report is run. If the Report ran today (08-Aug-08), the date used would be 08-Aug-08.", dateSchedule.Description);

				dateSchedule.PeriodScope = PeriodScopeList.Codes.Next;
				dateSchedule.PeriodCount = 1;
				AssertEquals("dateSchedule.Description",
@"1 day after when the report is run. If the Report ran today (08-Aug-08), the date used would be 09-Aug-08.", dateSchedule.Description);

				dateSchedule.PeriodScope = PeriodScopeList.Codes.Previous;
				dateSchedule.PeriodCount = 1;
				AssertEquals("dateSchedule.Description",
@"1 day prior to when the report is run. If the Report ran today (08-Aug-08), the date used would be 07-Aug-08.", dateSchedule.Description);

				dateSchedule.PeriodScope = PeriodScopeList.Codes.Next;
				dateSchedule.PeriodCount = 6;
				AssertEquals("dateSchedule.Description",
@"6 days after when the report is run. If the Report ran today (08-Aug-08), the date used would be 14-Aug-08.", dateSchedule.Description);

				dateSchedule.PeriodScope = PeriodScopeList.Codes.Previous;
				dateSchedule.PeriodCount = 8;
				AssertEquals("dateSchedule.Description",
@"8 days prior to when the report is run. If the Report ran today (08-Aug-08), the date used would be 31-Jul-08.", dateSchedule.Description);

				dateSchedule.PeriodScope = PeriodScopeList.Codes.This;
				AssertEquals("dateSchedule.Description",
@"The day when the report is run. If the Report ran today (08-Aug-08), the date used would be 08-Aug-08.", dateSchedule.Description);
			});
		}

		[TestDate(2008, 8, 8, 0, 0, 0)]
		public void TestHourMinutePeriodDescriptionInDifferentScopes()
		{
			var dateSchedule = new DateSchedule();
			dateSchedule.Period = ScheduleRecurrenceType.HourAndMinute;
			dateSchedule.ScheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			dateSchedule.ScheduleTask.Recurrence.RecurringStartTimeLocal = new ZDateTime(1900, 1, 1, 2, 0, 0);

			CombineAssertions(() =>
			{
				dateSchedule.PeriodScope = PeriodScopeList.Codes.Next;
				dateSchedule.Hour = 20;
				dateSchedule.MinuteOfHour = 0;
				AssertEquals("dateSchedule.Description",
@"20 hours after the report's Scheduled Run Time, on the day of its Next Run Time. If the Report ran now (08-Aug-08 00:00), the date/time used would be 08-Aug-08 22:00.", dateSchedule.Description);

				dateSchedule.PeriodScope = PeriodScopeList.Codes.Previous;
				AssertEquals("dateSchedule.Description",
@"20 hours prior to the report's Scheduled Run Time, on the day of its Next Run Time. If the Report ran now (08-Aug-08 00:00), the date/time used would be 07-Aug-08 06:00.", dateSchedule.Description);

				dateSchedule.PeriodScope = PeriodScopeList.Codes.Next;
				dateSchedule.Hour = 20;
				dateSchedule.MinuteOfHour = 30;
				AssertEquals("dateSchedule.Description",
@"20 hours and 30 minutes after the report's Scheduled Run Time, on the day of its Next Run Time. If the Report ran now (08-Aug-08 00:00), the date/time used would be 08-Aug-08 22:30.", dateSchedule.Description);

				dateSchedule.PeriodScope = PeriodScopeList.Codes.Previous;
				AssertEquals("dateSchedule.Description",
@"20 hours and 30 minutes prior to the report's Scheduled Run Time, on the day of its Next Run Time. If the Report ran now (08-Aug-08 00:00), the date/time used would be 07-Aug-08 05:30.", dateSchedule.Description);

				dateSchedule.PeriodScope = PeriodScopeList.Codes.Next;
				dateSchedule.Hour = 1;
				dateSchedule.MinuteOfHour = 0;
				AssertEquals("dateSchedule.Description",
@"1 hour after the report's Scheduled Run Time, on the day of its Next Run Time. If the Report ran now (08-Aug-08 00:00), the date/time used would be 08-Aug-08 03:00.", dateSchedule.Description);

				dateSchedule.PeriodScope = PeriodScopeList.Codes.Previous;
				AssertEquals("dateSchedule.Description",
@"1 hour prior to the report's Scheduled Run Time, on the day of its Next Run Time. If the Report ran now (08-Aug-08 00:00), the date/time used would be 08-Aug-08 01:00.", dateSchedule.Description);

				dateSchedule.PeriodScope = PeriodScopeList.Codes.Next;
				dateSchedule.Hour = 1;
				dateSchedule.MinuteOfHour = 1;
				AssertEquals("dateSchedule.Description",
@"1 hour and 1 minute after the report's Scheduled Run Time, on the day of its Next Run Time. If the Report ran now (08-Aug-08 00:00), the date/time used would be 08-Aug-08 03:01.", dateSchedule.Description);

				dateSchedule.PeriodScope = PeriodScopeList.Codes.Previous;
				AssertEquals("dateSchedule.Description",
@"1 hour and 1 minute prior to the report's Scheduled Run Time, on the day of its Next Run Time. If the Report ran now (08-Aug-08 00:00), the date/time used would be 08-Aug-08 00:59.", dateSchedule.Description);

				dateSchedule.PeriodScope = PeriodScopeList.Codes.Next;
				dateSchedule.Hour = 1;
				dateSchedule.MinuteOfHour = 30;
				AssertEquals("dateSchedule.Description",
@"1 hour and 30 minutes after the report's Scheduled Run Time, on the day of its Next Run Time. If the Report ran now (08-Aug-08 00:00), the date/time used would be 08-Aug-08 03:30.", dateSchedule.Description);

				dateSchedule.PeriodScope = PeriodScopeList.Codes.Previous;
				AssertEquals("dateSchedule.Description",
@"1 hour and 30 minutes prior to the report's Scheduled Run Time, on the day of its Next Run Time. If the Report ran now (08-Aug-08 00:00), the date/time used would be 08-Aug-08 00:30.", dateSchedule.Description);

				dateSchedule.Hour = 0;
				dateSchedule.MinuteOfHour = 0;
				AssertEquals("dateSchedule.Description",
@"The Scheduled Run Time of the report, on the day of its Next Run Time. If the Report ran now (08-Aug-08 00:00), the date/time used would be 08-Aug-08 02:00.", dateSchedule.Description);

				dateSchedule.PeriodScope = PeriodScopeList.Codes.Next;
				AssertEquals("dateSchedule.Description",
@"The Scheduled Run Time of the report, on the day of its Next Run Time. If the Report ran now (08-Aug-08 00:00), the date/time used would be 08-Aug-08 02:00.", dateSchedule.Description);
			});
		}

		public void TestHourMinutePeriodDescriptionWhenBaseTimeIsEmpty()
		{
			var dateSchedule = new DateSchedule();
			dateSchedule.Period = ScheduleRecurrenceType.HourAndMinute;
			dateSchedule.ScheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			dateSchedule.ScheduleTask.S5_StartDate = ZDateTime.Empty;

			dateSchedule.PeriodScope = PeriodScopeList.Codes.Next;
			dateSchedule.Hour = 1;
			dateSchedule.MinuteOfHour = 0;
			AssertEquals("Failed to calculate due to no date being specified for \"Next Run Time\", \"Start Date\" and \"Scheduled Run Time\".", dateSchedule.Description);
		}

		[TestDate(2008, 8, 8, 2, 30, 0)]
		public void TestHourMinuteGetScheduleDate()
		{
			var dateSchedule = new DateSchedule();
			dateSchedule.Period = ScheduleRecurrenceType.HourAndMinute;
			dateSchedule.PeriodScope = PeriodScopeList.Codes.Next;
			dateSchedule.ScheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			dateSchedule.Period = ScheduleRecurrenceType.HourAndMinute;
			dateSchedule.ScheduleTask.Recurrence.RecurringStartTimeLocal = new ZDateTime(1910, 1, 1, 10, 0, 0);
			dateSchedule.ScheduleTask.CalcNextRunTimeLocal = new ZDateTime(2008, 8, 7, 23, 59, 0);
			dateSchedule.Hour = 1;
			dateSchedule.MinuteOfHour = 30;

			AssertEquals("Precondition", new ZDateTime(2008, 8, 7, 10, 0, 0), dateSchedule.BaseDateTimeForTesting);
			AssertEquals("dateSchedule.GetScheduleDate() should use the scheduled time on the next run time's day", new ZDateTime(2008, 8, 7, 11, 30, 0), dateSchedule.GetScheduleDate());

			dateSchedule.PeriodScope = PeriodScopeList.Codes.Previous;

			AssertEquals("dateSchedule.GetScheduleDate() should use the scheduled time on the next run time's day", new ZDateTime(2008, 8, 7, 8, 30, 0), dateSchedule.GetScheduleDate());
		}

		public void TestHourMinuteGetScheduleDateWhenBaseTimeIsEmpty()
		{
			var dateSchedule = new DateSchedule();
			dateSchedule.Period = ScheduleRecurrenceType.HourAndMinute;
			dateSchedule.PeriodScope = PeriodScopeList.Codes.Next;
			dateSchedule.ScheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			dateSchedule.Hour = 1;
			dateSchedule.MinuteOfHour = 30;

			dateSchedule.ScheduleTask.S5_StartDate = ZDateTime.Empty;

			AssertNoExceptionThrown(() => _ = dateSchedule.BaseDateTimeForTesting);
			AssertEquals("Empty DateTime occurs when basetime empty", ZDateTime.Empty, dateSchedule.BaseDateTimeForTesting);
			AssertEquals("dateSchedule.GetScheduleDate() should return DateTime.Empty for empty start time", ZDateTime.Empty, dateSchedule.GetScheduleDate());
		}

		public void TestSettingHourAndMinuteEdgeCases()
		{
			var dateSchedule = new DateSchedule();
			dateSchedule.Period = ScheduleRecurrenceType.HourAndMinute;

			CombineAssertions("Initial values", () =>
			{
				AssertEquals("Initial hour value", (short)0, dateSchedule.Hour);
				AssertEquals("Initial minute value", (short)0, dateSchedule.Hour);
			});

			dateSchedule.MinuteOfHour = 60;
			AssertEquals("Minute value snaps back to 59 after being set to 60 or above", (short)59, dateSchedule.MinuteOfHour);

			dateSchedule.MinuteOfHour = -1;
			AssertEquals("Minute value snaps back to 0 after being set to negative values", (short)0, dateSchedule.MinuteOfHour);

			dateSchedule.Hour = -1;
			AssertEquals("Hour value snaps back to 0 after being set to negative values", (short)0, dateSchedule.Hour);
		}

		public override void TestIsValid()
		{
			Schedule.ByWeek = true;
			base.TestIsValid();
		}

		public void TestDefaultPeriod()
		{
			AssertEquals("Period", ScheduleRecurrenceType.Daily, Schedule.Period);
			AssertEquals("PeriodScope", PeriodScopeList.Codes.This, Schedule.PeriodScope);
			AssertEquals("DayNumber", (ZShort)1, Schedule.DayNumber);
		}

		public void TestDayNumber()
		{
			Schedule.LastDay = false;

			Schedule.ByMonth = true;
			Schedule.DayNumber = 15;
			AssertEquals("DayNumber", (ZShort)15, Schedule.DayNumber);
			Schedule.DayNumber = 32;
			AssertEquals("DayNumber", (ZShort)31, Schedule.DayNumber);

			Schedule.ByYear = true;
			Schedule.DayNumber = 100;
			AssertEquals("DayNumber", (ZShort)100, Schedule.DayNumber);
			Schedule.DayNumber = 367;
			AssertEquals("DayNumber", (ZShort)366, Schedule.DayNumber);

			Schedule.DayNumber = 0;
			AssertEquals("DayNumber", (ZShort)1, Schedule.DayNumber);

			Schedule.LastDay = true;
			Schedule.DayNumber = 0;
			AssertEquals("DayNumber", (ZShort)0, Schedule.DayNumber);

			Schedule.ByDay = true;
			AssertEquals("DayNumber", (ZShort)0, Schedule.DayNumber);
		}

		public void TestDayNumberInfoReadOnly()
		{
			Schedule.ByWeek = true;
			Schedule.LastDay = true;
			AssertEquals("DayNumberInfo.ReadOnly", true, Schedule.DayNumberInfo.ReadOnly);
			Schedule.LastDay = false;
			AssertEquals("DayNumberInfo.ReadOnly", false, Schedule.DayNumberInfo.ReadOnly);
		}

		public void TestDayName()
		{
			Schedule.ByWeek = true;
			AssertEquals("DayName", WeekDayList.Codes.Sunday, Schedule.DayName);

			Schedule.DayName = WeekDayList.Codes.Tuesday;
			AssertEquals("DayName", WeekDayList.Codes.Tuesday, Schedule.DayName);

			Schedule.DayName = "THU";
			AssertEquals("DayName", "THU", Schedule.DayName);

			Schedule.DayName = "a";
			AssertEquals("DayName", "a", Schedule.DayName);
			AssertHasErrors(Schedule.DayNameInfo);
		}

		public override void TestDescription()
		{
			Schedule.ByWeek = true;
			Schedule.DayName = WeekDayList.Codes.Sunday;
			Schedule.PeriodScope = PeriodScopeList.Codes.This;
			AssertEquals("Description", "The Sunday of the week of when the report is run.", Schedule.Description);

			Schedule.DayName = WeekDayList.Codes.Monday;
			Schedule.PeriodScope = PeriodScopeList.Codes.Previous;
			Schedule.PeriodCount = 1;
			AssertEquals("Description", "The Monday of the week prior to when the report is run.", Schedule.Description);

			Schedule.PeriodCount = 2;
			AssertEquals("Description", "The Monday of 2 weeks prior to when the report is run.", Schedule.Description);

			Schedule.PeriodScope = PeriodScopeList.Codes.Next;
			AssertEquals("Description", "The Monday of 2 weeks after when the report is run.", Schedule.Description);

			Schedule.ByMonth = true;
			Schedule.DayNumber = 2;
			AssertEquals("Description", "The 2nd day of 2 months after when the report is run.", Schedule.Description);

			Schedule.LastDay = true;
			AssertEquals("Description", "The last day of 2 months after when the report is run.", Schedule.Description);

			Schedule.ByWeek = true;
			AssertEquals("Description", "The Monday of 2 weeks after when the report is run.", Schedule.Description);

			Schedule.ByYear = true;
			AssertEquals("Description", "The last day of 2 years after when the report is run.", Schedule.Description);

			Schedule.LastDay = false;
			Schedule.DayNumber = 10;
			Schedule.PeriodScope = PeriodScopeList.Codes.This;
			AssertEquals("Description", "The 10th day of the year of when the report is run.", Schedule.Description);

			Schedule.ByDay = true;
			AssertEquals("Description", "The day when the report is run.", Schedule.Description);

			Schedule.ByHourAndMinute = true;
		}

		public void TestLastDay()
		{
			Schedule.LastDay = false;
			AssertEquals("Schedule.LastDay", false, Schedule.LastDay);
			Schedule.LastDay = true;
			AssertEquals("Schedule.LastDay", true, Schedule.LastDay);
		}

		public void TestPeriods()
		{
			Schedule.ByMonth = true;
			AssertEquals("Month", true, Schedule.ByMonth);
			AssertEquals("Week", false, Schedule.ByWeek);
			AssertEquals("Year", false, Schedule.ByYear);
			AssertEquals("Today", false, Schedule.ByDay);

			Schedule.ByWeek = true;
			AssertEquals("Month", false, Schedule.ByMonth);
			AssertEquals("Week", true, Schedule.ByWeek);
			AssertEquals("Year", false, Schedule.ByYear);
			AssertEquals("Today", false, Schedule.ByDay);

			Schedule.ByYear = true;
			AssertEquals("Month", false, Schedule.ByMonth);
			AssertEquals("Week", false, Schedule.ByWeek);
			AssertEquals("Year", true, Schedule.ByYear);
			AssertEquals("Today", false, Schedule.ByDay);

			Schedule.ByDay = true;
			AssertEquals("Month", false, Schedule.ByMonth);
			AssertEquals("Week", false, Schedule.ByWeek);
			AssertEquals("Year", false, Schedule.ByYear);
			AssertEquals("Today", true, Schedule.ByDay);
		}

		public void TestWeek()
		{
			Schedule.ByMonth = true;
			AssertEquals("Schedule.Week", false, Schedule.ByWeek);
			Schedule.ByWeek = true;
			AssertEquals("Schedule.Week", true, Schedule.ByWeek);
			Schedule.ByYear = true;
			AssertEquals("Schedule.Week", false, Schedule.ByWeek);
		}

		[TestUtcOffset(14, 0, 0)]
		public void TestGetScheduleDate()
		{
			Schedule.ByWeek = true;
			Schedule.DayName = WeekDayList.Codes.Monday;
			Schedule.PeriodScope = PeriodScopeList.Codes.This;
			AssertEquals("IsValid", true, Schedule.IsValid);
			AssertEquals("GetScheduleDate()", ZDateTime.Empty, Schedule.GetScheduleDate());

			ReportScheduleTask scheduleTask = Factory.New<ReportScheduleTask>();
			Schedule.ScheduleTask = scheduleTask;
			scheduleTask.CalcStartDateLocal = new ZDateTime(2006, 5, 1);

			Schedule.DayName = "x";
			AssertEquals("HasErrors", true, Schedule.HasErrors);
			AssertEquals("GetScheduleDate()", ZDateTime.Empty, Schedule.GetScheduleDate());

			Schedule.DayName = WeekDayList.Codes.Monday;
			AssertEquals("HasErrors", false, Schedule.HasErrors);
			AssertEquals("GetScheduleDate()", new ZDateTime(2006, 5, 1), Schedule.GetScheduleDate());

			Schedule.ByMonth = true;
			Schedule.DayNumber = 2;
			AssertEquals("GetScheduleDate()", new ZDateTime(2006, 5, 2), Schedule.GetScheduleDate());

			Schedule.ByYear = true;
			AssertEquals("GetScheduleDate()", new ZDateTime(2006, 1, 2), Schedule.GetScheduleDate());

			Schedule.PeriodScope = PeriodScopeList.Codes.Previous;
			Schedule.PeriodCount = 1;

			Schedule.ByWeek = true;
			AssertEquals("GetScheduleDate()", new ZDateTime(2006, 4, 24), Schedule.GetScheduleDate());

			Schedule.ByMonth = true;
			AssertEquals("GetScheduleDate()", new ZDateTime(2006, 4, 2), Schedule.GetScheduleDate());

			Schedule.ByYear = true;
			AssertEquals("GetScheduleDate()", new ZDateTime(2005, 1, 2), Schedule.GetScheduleDate());

			Schedule.PeriodScope = PeriodScopeList.Codes.Next;
			Schedule.PeriodCount = 2;
			Schedule.DayNumber = 5;
			scheduleTask.CalcNextRunTimeLocal = new ZDateTime(2006, 7, 1);

			Schedule.ByWeek = true;
			Schedule.DayName = WeekDayList.Codes.Thursday;
			AssertEquals("GetScheduleDate()", new ZDateTime(2006, 7, 13), Schedule.GetScheduleDate());

			Schedule.ByMonth = true;
			AssertEquals("GetScheduleDate()", new ZDateTime(2006, 9, 5), Schedule.GetScheduleDate());

			Schedule.ByYear = true;
			AssertEquals("GetScheduleDate()", new ZDateTime(2008, 1, 5), Schedule.GetScheduleDate());

			Schedule.ByDay = true;
			Schedule.PeriodScope = PeriodScopeList.Codes.This;
			AssertEquals("GetScheduleDate()", ZDateTime.Today, Schedule.GetScheduleDate());
		}

		public override void TestClear()
		{
			base.TestClear();

			Schedule.ByMonth = true;
			Schedule.LastDay = true;
			Schedule.DayNumber = 3;
			Schedule.DayName = "x";
			Schedule.Hour = 12;
			Schedule.MinuteOfHour = 30;

			Schedule.Clear();
			AssertEquals("Period", ScheduleRecurrenceType.Daily, Schedule.Period);
			AssertEquals("DayNumber", (ZShort)0, Schedule.DayNumber);
			AssertEquals("DayName", "", Schedule.DayName);
			AssertEquals("LastDay", false, Schedule.LastDay);
			AssertEquals("Hour", (short)0, Schedule.Hour);
			AssertEquals("MinuteOfHour", (short)0, Schedule.MinuteOfHour);
		}

		[ExpectExceptionMessage(typeof(InvalidOperationException), "StorageValueDay cannot be obtained because the Period is invalid.")]
		public void TestToStorageValueThrowsIfPeriodIsEmpty()
		{
			Schedule.PeriodScope = PeriodScopeList.Codes.This;
			Schedule.Period = ZString.Empty;
			Schedule.ToStorageValue();
		}

		[TestDate(2006, 12, 12)]
		public override void TestToStorageValue()
		{
			Schedule.ByWeek = true;
			Schedule.DayName = WeekDayList.Codes.Sunday;
			Schedule.PeriodScope = PeriodScopeList.Codes.This;
			AssertEquals("ToStorageValue()", new DateTime(1910, 1, 1, 0, 1, 0), Schedule.ToStorageValue());

			Schedule.DayName = WeekDayList.Codes.Saturday;
			Schedule.PeriodScope = PeriodScopeList.Codes.Previous;
			Schedule.PeriodCount = 2;
			AssertEquals("ToStorageValue()", new DateTime(1909, 11, 1, 0, 7, 0), Schedule.ToStorageValue());

			Schedule.DayName = WeekDayList.Codes.Thursday;
			Schedule.PeriodScope = PeriodScopeList.Codes.Next;
			Schedule.PeriodCount = 3;
			AssertEquals("ToStorageValue()", new DateTime(1910, 4, 1, 0, 5, 0), Schedule.ToStorageValue());

			Schedule.ByMonth = true;
			Schedule.DayNumber = 5;
			AssertEquals("ToStorageValue()", new DateTime(1910, 4, 2, 0, 5, 0), Schedule.ToStorageValue());

			Schedule.LastDay = true;
			AssertEquals("ToStorageValue()", new DateTime(1910, 4, 2, 23, 0, 0), Schedule.ToStorageValue());

			Schedule.ByYear = true;
			AssertEquals("ToStorageValue()", new DateTime(1910, 4, 3, 23, 0, 0), Schedule.ToStorageValue());

			Schedule.ByDay = true;
			Schedule.PeriodScope = PeriodScopeList.Codes.This;
			AssertEquals("ToStorageValue()", new DateTime(1910, 1, 4, 0, 0, 0), Schedule.ToStorageValue());

			Schedule.ByHourAndMinute = true;
			Schedule.PeriodScope = PeriodScopeList.Codes.Previous;
			Schedule.PeriodCount = 1;
			Schedule.Hour = 10;
			Schedule.MinuteOfHour = 30;
			AssertEquals("ToStorageValue()", new DateTime(1909, 12, 5, 0, 0, 30, 10), Schedule.ToStorageValue());

			Schedule.ByHourAndMinute = true;
			Schedule.PeriodScope = PeriodScopeList.Codes.Next;
			Schedule.PeriodCount = 1;
			AssertEquals("ToStorageValue()", new DateTime(1910, 2, 5, 0, 0, 30, 10), Schedule.ToStorageValue());
		}

		public void TestTryParse()
		{
			DateSchedule schedule;

			AssertEquals("TryParse()", true, DateSchedule.TryParse(new DateTime(1910, 1, 1, 0, 1, 0), out schedule));
			AssertParsedValue(schedule, 1, ZBool.False, 0, ScheduleRecurrenceType.Weekly);

			AssertEquals("TryParse()", true, DateSchedule.TryParse(new DateTime(1909, 11, 1, 0, 7, 0), out schedule));
			AssertParsedValue(schedule, 7, ZBool.False, -2, ScheduleRecurrenceType.Weekly);

			AssertEquals("TryParse()", true, DateSchedule.TryParse(new DateTime(1910, 4, 1, 0, 5, 0), out schedule));
			AssertParsedValue(schedule, 5, ZBool.False, 3, ScheduleRecurrenceType.Weekly);

			AssertEquals("TryParse()", true, DateSchedule.TryParse(new DateTime(1910, 1, 2, 0, 1, 0), out schedule));
			AssertParsedValue(schedule, 1, ZBool.False, 0, ScheduleRecurrenceType.Monthly);

			AssertEquals("TryParse()", true, DateSchedule.TryParse(new DateTime(1909, 12, 2, 0, 3, 0), out schedule));
			AssertParsedValue(schedule, 3, ZBool.False, -1, ScheduleRecurrenceType.Monthly);

			AssertEquals("TryParse()", true, DateSchedule.TryParse(new DateTime(1910, 6, 2, 23, 0, 0), out schedule));
			AssertParsedValue(schedule, 1, ZBool.True, 5, ScheduleRecurrenceType.Monthly);

			AssertEquals("TryParse()", true, DateSchedule.TryParse(new DateTime(1910, 1, 3, 0, 1, 0), out schedule));
			AssertParsedValue(schedule, 1, ZBool.False, 0, ScheduleRecurrenceType.Yearly);

			AssertEquals("TryParse()", true, DateSchedule.TryParse(new DateTime(1901, 9, 3, 0, 3, 0), out schedule));
			AssertParsedValue(schedule, 3, ZBool.False, -100, ScheduleRecurrenceType.Yearly);

			AssertEquals("TryParse()", true, DateSchedule.TryParse(new DateTime(1918, 5, 3, 23, 0, 0), out schedule));
			AssertParsedValue(schedule, 1, ZBool.True, 100, ScheduleRecurrenceType.Yearly);

			AssertEquals("TryParse()", true, DateSchedule.TryParse(new DateTime(1915, 1, 3, 6, 6, 0), out schedule));
			AssertParsedValue(schedule, 366, ZBool.False, 60, ScheduleRecurrenceType.Yearly);

			AssertEquals("TryParse()", true, DateSchedule.TryParse(new DateTime(1910, 2, 5, 0, 0, 0), out schedule));
			AssertParsedValue(schedule, 1, ZBool.False, 1, ScheduleRecurrenceType.HourAndMinute);

			AssertEquals("TryParse()", false, DateSchedule.TryParse(new DateTime(1915, 1, 6, 6, 6, 0), out schedule));
			AssertNull("schedule", schedule);

			AssertEquals("TryParse()", false, DateSchedule.TryParse(DateTime.MinValue, out schedule));
			AssertNull("schedule", schedule);
		}

		void AssertParsedValue(DateSchedule schedule, int dayNumber, ZBool lastDay, int periodNumber, string period)
		{
			AssertEquals("DayNumber", dayNumber, schedule.DayNumber);
			AssertEquals("LastDay", lastDay, schedule.LastDay);

			string periodScope;
			if (periodNumber < 0)
			{
				periodScope = PeriodScopeList.Codes.Previous;
			}
			else if (periodNumber == 0)
			{
				periodScope = PeriodScopeList.Codes.This;
			}
			else
			{
				periodScope = PeriodScopeList.Codes.Next;
			}
			AssertEquals("PeriodScope", periodScope, schedule.PeriodScope);

			if (periodNumber < 0)
			{
				periodNumber *= -1;
			}
			AssertEquals("PeriodNumber", periodNumber, schedule.PeriodCount);

			AssertEquals("Period", period, schedule.Period);
		}

		public void TestFillData()
		{
			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.CalcNextRunTimeLocal = new ZDateTime(2006, 4, 1, 2, 45, 0);
			var schedule = new DateSchedule(scheduleTask);

			var scheduleData = new DateScheduleData()
			{
				PeriodScope = PeriodScopeList.Codes.Next,
				RecurrenceType = ScheduleRecurrenceType.Monthly,
				DayNumber = 4,
				IsLastDay = true,
				PeriodCount = 3,
				DayNameAsDayNumber = 2,
				Hour = 2,
				MinuteOfHour = 3,
			};

			schedule.FillData(scheduleData);
			AssertEquals("Period", schedule.Period, scheduleData.RecurrenceType);
			AssertEquals("PeriodCount", schedule.PeriodCount , scheduleData.PeriodCount);
			AssertEquals("PeriodScope",schedule.PeriodScope , scheduleData.PeriodScope);
			AssertEquals("LastDay",schedule.LastDay , scheduleData.IsLastDay);
			AssertEquals("DayNumber",schedule.DayNumber , scheduleData.DayNumber);
			AssertEquals("Hour",schedule.Hour , scheduleData.Hour);
			AssertEquals("MinuteOfHour",schedule.MinuteOfHour , scheduleData.MinuteOfHour);
		}

		public void TestExtractData()
		{
			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.CalcNextRunTimeLocal = new ZDateTime(2006, 4, 1, 2, 45, 0);
			var schedule = new DateSchedule(scheduleTask);

			var scheduleData = new DateScheduleData()
			{
				PeriodScope = PeriodScopeList.Codes.Next,
				RecurrenceType = ScheduleRecurrenceType.Monthly,
				DayNumber = 4,
				IsLastDay = true,
				PeriodCount = 3,
				DayNameAsDayNumber = 2,
				Hour = 2,
				MinuteOfHour = 3,
			};

			schedule.FillData(scheduleData);

			var expectedData = schedule.ExtractData();
			AssertEquals("RecurrenceType",expectedData.RecurrenceType, schedule.Period);
			AssertEquals("PeriodScope",expectedData.PeriodScope, schedule.PeriodScope);
			AssertEquals("DayNameAsDayNumber", expectedData.DayNameAsDayNumber, scheduleData.DayNameAsDayNumber);
			AssertEquals("DayNumber", expectedData.DayNumber, schedule.DayNumber);
			AssertEquals("PeriodCount",expectedData.PeriodCount, schedule.PeriodCount);
			AssertEquals("IsLastDay",expectedData.IsLastDay, schedule.LastDay);
			AssertEquals("Hour",expectedData.Hour, schedule.Hour);
			AssertEquals("MinuteOfHour",expectedData.MinuteOfHour, schedule.MinuteOfHour);
			AssertEquals("CalculatedResult", expectedData.CalculatedResult, schedule.Description);
			AssertEquals("StorageValue",expectedData.StorageValue, schedule.ToStorageValue());
			AssertEquals("ScheduleDate", expectedData.ScheduleDate, schedule.GetScheduleDate().ToDateTime());

			scheduleData = new DateScheduleData();
			schedule.FillData(scheduleData);
			expectedData = schedule.ExtractData();
			Assert("invalid schedule", !schedule.IsValid);
			AssertNull(expectedData);
		}
	}
}
