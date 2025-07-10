using Enterprise.Scheduler.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Scheduler.Business.Testing
{
	[TestedType(typeof(DateSchedule))]
	sealed class DateScheduleLookupsTest : ScheduleLookupsTestCase<DateSchedule>
	{
		public void TestWeekDays()
		{
			AssertEquals("WeekDays.GetType()", typeof(WeekDayList), Schedule.Lookups.WeekDays.GetType());
		}
	}
}
