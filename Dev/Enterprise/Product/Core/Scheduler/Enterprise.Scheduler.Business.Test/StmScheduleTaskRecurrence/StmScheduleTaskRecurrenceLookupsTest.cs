using CargoWise.EntityFramework.Testing;

namespace Enterprise.Scheduler.Business.Testing
{
	sealed class StmScheduleTaskRecurrenceLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestMonths()
		{
			AssertEquals("Count", 12, Recurrence.Lookups.Months.Count);
			AssertEquals("GetDescriptionFromCode(\"1\")", "January", Recurrence.Lookups.Months.GetDescriptionFromCode("1"));
			AssertEquals("GetDescriptionFromCode(\"12\")", "December", Recurrence.Lookups.Months.GetDescriptionFromCode("12"));
		}

		public void TestWeekCounts()
		{
			AssertEquals("Count", 4, Recurrence.Lookups.WeekCounts.Count);
			AssertEquals("GetDescriptionFromCode(\"1\")", "first", Recurrence.Lookups.WeekCounts.GetDescriptionFromCode("1"));
			AssertEquals("GetDescriptionFromCode(\"4\")", "fourth", Recurrence.Lookups.WeekCounts.GetDescriptionFromCode("4"));
		}

		public void TestWeekDays()
		{
			AssertEquals("WeekDays.GetType()", typeof(WeekDayList), Recurrence.Lookups.WeekDays.GetType());
		}

		#region Implementation

		StmScheduleTaskRecurrence Recurrence
		{
			get
			{
				if (recurrence == null)
				{
					StmScheduleTask scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();
					recurrence = new StmScheduleTaskRecurrence(scheduleTask);
				}
				return recurrence;
			}
		}
		StmScheduleTaskRecurrence recurrence;

		#endregion
	}
}
