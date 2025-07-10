using System;
using System.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Scheduler.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	class GenericDateTypeHelperTest : TestCaseWithFactory
	{
		public void TestIsRangeExeedingMaxTimeSpan_MaxYears()
		{
			AssertEquals(false, GenericDateTypeHelper.IsRangeExceedingMaxTimeSpan(new ZDateTime(2024, 7, 1, 0, 0, 0, DateTimeKind.Local), new ZDateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Local), 2, 0));
			AssertEquals(true, GenericDateTypeHelper.IsRangeExceedingMaxTimeSpan(new ZDateTime(2024, 7, 1, 0, 0, 0, DateTimeKind.Local), new ZDateTime(2026, 7, 2, 0, 0, 0, DateTimeKind.Local), 2, 0));

			AssertEquals(false, GenericDateTypeHelper.IsRangeExceedingMaxTimeSpan(new ZDateTimeOffset(2024, 7, 1, 0, 0, 0, TimeSpan.FromHours(8)), new ZDateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.FromHours(8)), 2, 0));
			AssertEquals(true, GenericDateTypeHelper.IsRangeExceedingMaxTimeSpan(new ZDateTimeOffset(2024, 7, 1, 0, 0, 0, TimeSpan.FromHours(8)), new ZDateTimeOffset(2026, 7, 2, 0, 0, 0, TimeSpan.FromHours(8)), 2, 0));
		}

		public void TestIsRangeExeedingMaxTimeSpan_MaxMonths()
		{
			AssertEquals(false, GenericDateTypeHelper.IsRangeExceedingMaxTimeSpan(new ZDateTime(2024, 7, 1, 0, 0, 0, DateTimeKind.Local), new ZDateTime(2024, 12, 1, 0, 0, 0, DateTimeKind.Local), 0, 5));
			AssertEquals(true, GenericDateTypeHelper.IsRangeExceedingMaxTimeSpan(new ZDateTime(2024, 7, 1, 0, 0, 0, DateTimeKind.Local), new ZDateTime(2024, 12, 2, 0, 0, 0, DateTimeKind.Local), 0, 5));

			AssertEquals(false, GenericDateTypeHelper.IsRangeExceedingMaxTimeSpan(new ZDateTimeOffset(2024, 7, 1, 0, 0, 0, TimeSpan.FromHours(8)), new ZDateTimeOffset(2024, 12, 1, 0, 0, 0, TimeSpan.FromHours(8)), 0, 5));
			AssertEquals(true, GenericDateTypeHelper.IsRangeExceedingMaxTimeSpan(new ZDateTimeOffset(2024, 7, 1, 0, 0, 0, TimeSpan.FromHours(8)), new ZDateTimeOffset(2024, 12, 2, 0, 0, 0, TimeSpan.FromHours(8)), 0, 5));
		}

		public void TestIsRangeExeedingMaxTimeSpan_DifferentOffsets()
		{
			AssertEquals(false, GenericDateTypeHelper.IsRangeExceedingMaxTimeSpan(new ZDateTimeOffset(2024, 7, 1, 0, 0, 0, TimeSpan.FromHours(8)), new ZDateTimeOffset(2026, 7, 1, 2, 0, 0, TimeSpan.FromHours(10)), 2, 0));
			AssertEquals(true, GenericDateTypeHelper.IsRangeExceedingMaxTimeSpan(new ZDateTimeOffset(2024, 7, 1, 0, 0, 0, TimeSpan.FromHours(8)), new ZDateTimeOffset(2026, 7, 2, 0, 0, 0, TimeSpan.FromHours(10)), 2, 0));

			AssertEquals(false, GenericDateTypeHelper.IsRangeExceedingMaxTimeSpan(new ZDateTimeOffset(2024, 7, 1, 0, 0, 0, TimeSpan.FromHours(8)), new ZDateTimeOffset(2024, 12, 1, 2, 0, 0, TimeSpan.FromHours(10)), 0, 5));
			AssertEquals(true, GenericDateTypeHelper.IsRangeExceedingMaxTimeSpan(new ZDateTimeOffset(2024, 7, 1, 0, 0, 0, TimeSpan.FromHours(8)), new ZDateTimeOffset(2024, 12, 2, 2, 0, 0, TimeSpan.FromHours(10)), 0, 5));
		}

		public void TestAddMinutesForDate()
		{
			AssertEquals(new DateTime(2024, 7, 1, 0, 1, 0, DateTimeKind.Local), GenericDateTypeHelper.AddMinutesForDate(new DateTime(2024, 7, 1, 0, 0, 0, DateTimeKind.Local), 1));
			AssertEquals(new DateTimeOffset(2024, 7, 1, 0, 1, 0, TimeSpan.FromHours(8)), GenericDateTypeHelper.AddMinutesForDate(new DateTimeOffset(2024, 7, 1, 0, 0, 0, TimeSpan.FromHours(8)), 1));

			AssertEquals(new DateTime(2024, 6, 30, 23, 59, 0, DateTimeKind.Local), GenericDateTypeHelper.AddMinutesForDate(new DateTime(2024, 7, 1, 0, 0, 0, DateTimeKind.Local), -1));
			AssertEquals(new DateTimeOffset(2024, 6, 30, 23, 59, 0, TimeSpan.FromHours(8)), GenericDateTypeHelper.AddMinutesForDate(new DateTimeOffset(2024, 7, 1, 0, 0, 0, TimeSpan.FromHours(8)), -1));
		}

		public void TestAddDaysForDate()
		{
			AssertEquals(new DateTime(2024, 7, 2, 0, 0, 0, DateTimeKind.Local), GenericDateTypeHelper.AddDaysForDate(new DateTime(2024, 7, 1, 0, 0, 0, DateTimeKind.Local), 1));
			AssertEquals(new DateTimeOffset(2024, 7, 2, 0, 0, 0, TimeSpan.FromHours(8)), GenericDateTypeHelper.AddDaysForDate(new DateTimeOffset(2024, 7, 1, 0, 0, 0, TimeSpan.FromHours(8)), 1));

			AssertEquals(new DateTime(2024, 6, 30, 0, 0, 0, DateTimeKind.Local), GenericDateTypeHelper.AddDaysForDate(new DateTime(2024, 7, 1, 0, 0, 0, DateTimeKind.Local), -1));
			AssertEquals(new DateTimeOffset(2024, 6, 30, 0, 0, 0, TimeSpan.FromHours(8)), GenericDateTypeHelper.AddDaysForDate(new DateTimeOffset(2024, 7, 1, 0, 0, 0, TimeSpan.FromHours(8)), -1));
		}

		[TestDate(2024, 7, 16, 20, 30, 30)]
		[TestUtcOffset(8, 0, 0)]
		public void TestGetDateFromSchedule()
		{
			var dateSchedule = new DateSchedule();
			dateSchedule.ByWeek = ZBool.True;
			dateSchedule.PeriodScope = PeriodScopeList.Codes.This;
			dateSchedule.DayName = WeekDayList.Codes.Monday;
			dateSchedule.ScheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			AssertEquals(new ZDateTime(2024, 7, 15, 0, 0, 0, DateTimeKind.Local), GenericDateTypeHelper.GetDateFromSchedule<ZDateTime>(dateSchedule));
			AssertEquals(new ZDateTimeOffset(2024, 7, 15, 0, 0, 0, TimeSpan.FromHours(8)), GenericDateTypeHelper.GetDateFromSchedule<ZDateTimeOffset>(dateSchedule));
		}

		public void TestGetSqlDbType()
		{
			AssertEquals(SqlDbType.DateTime, GenericDateTypeHelper.GetSqlDbType<DateTime>());
			AssertEquals(SqlDbType.DateTimeOffset, GenericDateTypeHelper.GetSqlDbType<DateTimeOffset>());
		}

		public void TestParseDate()
		{
			AssertEquals(new ZDateTime(2024, 7, 15, 12, 30, 30, DateTimeKind.Local), GenericDateTypeHelper.ParseBaseDate<DateTime>("2024-07-15T12:30:30", System.Globalization.CultureInfo.CurrentCulture));
			AssertEquals(new ZDateTimeOffset(2024, 7, 15, 12, 30, 30, TimeSpan.FromHours(8)), GenericDateTypeHelper.ParseBaseDate<DateTimeOffset>("2024-07-15T12:30:30.0000000+08:00", System.Globalization.CultureInfo.CurrentCulture));
		}

		public void TestToZDateTimeOrZDateTimeOffset()
		{
			AssertEquals(new ZDateTime(2024, 7, 15, 12, 30, 30, DateTimeKind.Local), GenericDateTypeHelper.ToZDateTimeOrZDateTimeOffset(new DateTime(2024, 7, 15, 12, 30, 30, DateTimeKind.Local)));
			AssertEquals(new ZDateTimeOffset(2024, 7, 15, 12, 30, 30, TimeSpan.FromHours(8)), GenericDateTypeHelper.ToZDateTimeOrZDateTimeOffset(new DateTimeOffset(2024, 7, 15, 12, 30, 30, TimeSpan.FromHours(8))));
		}

		public void TestToDateTime()
		{
			AssertEquals(new ZDateTime(2024, 7, 15, 12, 30, 30, DateTimeKind.Local), GenericDateTypeHelper.ToDateTime(new DateTime(2024, 7, 15, 12, 30, 30, DateTimeKind.Local)));
			AssertEquals(new ZDateTime(2024, 7, 15, 12, 30, 30, DateTimeKind.Local), GenericDateTypeHelper.ToDateTime(new DateTimeOffset(2024, 7, 15, 12, 30, 30, TimeSpan.FromHours(8))));
		}

		public void TestToStringWithDateFormatFromDate()
		{
			AssertEquals("15-Jul-24 12:30", GenericDateTypeHelper.ToStringWithDateFormatFromDate(new DateTime(2024, 7, 15, 12, 30, 30, DateTimeKind.Local), "dd-MMM-yy HH:mm"));
			AssertEquals("15-Jul-24", GenericDateTypeHelper.ToStringWithDateFormatFromDate(new DateTime(2024, 7, 15, 12, 30, 30, DateTimeKind.Local), "dd-MMM-yy"));
			AssertEquals("15-Jul-24 12:30 +08:00", GenericDateTypeHelper.ToStringWithDateFormatFromDate(new DateTimeOffset(2024, 7, 15, 12, 30, 30, TimeSpan.FromHours(8)), "dd-MMM-yy HH:mm zzz"));
		}
	}
}
