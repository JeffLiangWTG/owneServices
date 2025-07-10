using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Scheduler.Business.Testing
{
	sealed class StmScheduleTaskValidationTest : BusinessObjectLookupsTestCase
	{
		public void TestCheckS5_DayList()
		{
			AssertNoErrors("Precondition: DayList should not have errors.", scheduleTask.S5_DayListInfo);

			recurrence.DailyRange = true;
			recurrence.DayList = "NNNNNNN";
			AssertNoErrors(scheduleTask.S5_DayListInfo);

			recurrence.WeeklyRange = true;
			recurrence.DayList = "NNNNNNN";
			AssertHasError(scheduleTask.S5_DayListInfo, "Please select at least one day.");

			recurrence.DayList = "NNNYNNN";
			AssertNoErrors(scheduleTask.S5_DayListInfo);
		}

		public void TestCheckS5_TaskPeriodCount()
		{
			AssertNoErrors("Precondition: TaskPeriodCount should not have errors.", scheduleTask.S5_TaskPeriodCountInfo);

			recurrence.DailyRange = true;
			recurrence.DailyDay = true;
			recurrence.TaskPeriodCount = 0;
			AssertHasError(scheduleTask.S5_TaskPeriodCountInfo, "Please enter a value.");

			recurrence.TaskPeriodCount = 1;
			AssertNoErrors(scheduleTask.S5_TaskPeriodCountInfo);

			recurrence.DailyDay = false;
			recurrence.TaskPeriodCount = 0;
			AssertNoErrors(scheduleTask.S5_TaskPeriodCountInfo);

			recurrence.WeeklyRange = true;
			recurrence.TaskPeriodCount = 0;
			AssertHasError(scheduleTask.S5_TaskPeriodCountInfo, "Please enter a value.");

			recurrence.TaskPeriodCount = 1;
			AssertNoErrors(scheduleTask.S5_TaskPeriodCountInfo);

			recurrence.MonthlyRange = true;
			recurrence.MonthlyDay = true;
			recurrence.TaskPeriodCount = 0;
			AssertHasError(scheduleTask.S5_TaskPeriodCountInfo, "Please enter a value.");

			recurrence.TaskPeriodCount = 1;
			AssertNoErrors(scheduleTask.S5_TaskPeriodCountInfo);

			recurrence.MonthlyDay = false;
			recurrence.TaskPeriodCount = 0;
			AssertNoErrors(scheduleTask.S5_TaskPeriodCountInfo);

			recurrence.AccountingRange = true;
			recurrence.AccountingDay = true;
			recurrence.TaskPeriodCount = 0;
			AssertHasError(scheduleTask.S5_TaskPeriodCountInfo, "Please enter a value.");

			recurrence.TaskPeriodCount = 1;
			AssertNoErrors(scheduleTask.S5_TaskPeriodCountInfo);

			recurrence.AccountingDay = false;
			recurrence.TaskPeriodCount = 0;
			AssertNoErrors(scheduleTask.S5_TaskPeriodCountInfo);
		}

		public void TestCheckS5_DailyStartTimeIsValidZDateTimeRange()
		{
			AssertNoErrors("[PRE-CONDITION] DailyStartTime should not have errors.", scheduleTask.S5_DailyStartTimeInfo);

			scheduleTask.S5_DailyStartTime = ZDateTime.MinSmallDateTimeValue;
			AssertNoErrors("DailyStartTime should not have errors.", scheduleTask.S5_DailyStartTimeInfo);

			scheduleTask.S5_DailyStartTime = new ZDateTime(1990, 5, 12, 2, 4, 6);
			AssertNoErrors("DailyStartTime should not have errors.", scheduleTask.S5_DailyStartTimeInfo);
		}

		public void TestCheckS5_DailyEndTimeIsValidZDateTimeRange()
		{
			AssertNoErrors("[PRE-CONDITION] DailyEndTime should not have errors.", scheduleTask.S5_DailyEndTimeInfo);

			scheduleTask.S5_DailyEndTime = ZDateTime.MinSmallDateTimeValue;
			AssertNoErrors("DailyEndTime should not have errors.", scheduleTask.S5_DailyEndTimeInfo);

			scheduleTask.S5_DailyEndTime = new ZDateTime(1990, 5, 12, 2, 4, 6);
			AssertNoErrors("DailyEndTime should not have errors.", scheduleTask.S5_DailyEndTimeInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();
			recurrence = new StmScheduleTaskRecurrence(scheduleTask);
		}
		StmScheduleTask scheduleTask;
		StmScheduleTaskRecurrence recurrence;
	}
}
