using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Scheduler.Business.Testing
{
	[TestedType(typeof(DateScheduleValidation))]
	sealed class DateScheduleValidationTest : ScheduleValidationTestCase<DateSchedule>
	{
		public void TestValidateDayName()
		{
			AssertNoErrors("Precondition: DayName should not have errors.", Schedule.DayNameInfo);
			Assert("Precondition: Lookups.WeekDays[0].Code should not be empty.", Schedule.Lookups.WeekDays[0].Code.Length > 0);

			Schedule.ByWeek = true;
			Schedule.DayName = ZString.Empty;
			AssertHasError(Schedule.DayNameInfo, "Please enter a value.");

			Schedule.DayName = "!";
			AssertHasError(Schedule.DayNameInfo, "Enter a valid selection.");

			Schedule.DayName = Schedule.Lookups.WeekDays[0].Code;
			AssertNoErrors(Schedule.DayNameInfo);

			Schedule.DayName = "";
			AssertHasError(Schedule.DayNameInfo, "Please enter a value.");
		}

		public void TestValidateHourAndMinuteScope()
		{
			AssertNoErrors("Precondition: Hour should not have errors.", Schedule.HourInfo);
			AssertNoErrors("Precondition: Minute should not have errors.", Schedule.MinuteOfHourInfo);

			CombineAssertions("Should contain previous and next in that order", () =>
			{
				AssertEquals("Previous", Schedule.Lookups.HourMinutePeriodScopes[0].Code);
				AssertEquals("Next", Schedule.Lookups.HourMinutePeriodScopes[1].Code);
			});

			Schedule.ByHourAndMinute = true;
			Schedule.PeriodScope = "Previous";
			AssertNoErrors(Schedule.PeriodScopeInfo);

			Schedule.PeriodScope = "Next";
			AssertNoErrors(Schedule.PeriodScopeInfo);

			Schedule.PeriodScope = "This";
			AssertHasError(Schedule.PeriodScopeInfo, "Enter a valid selection.");
		}

		public override void TestValidatePeriodCount()
		{
			Schedule.ByWeek = true;
			base.TestValidatePeriodCount();

			Schedule.ByHourAndMinute = true;
			base.TestValidatePeriodCount();
		}

		public override void TestValidatePeriodScope()
		{
			Schedule.ByWeek = true;
			base.TestValidatePeriodScope();

			Schedule.ByHourAndMinute = true;
			base.TestValidatePeriodScope();
		}

		protected override void PrepareScheduleForTestValidateAll()
		{
			base.PrepareScheduleForTestValidateAll();
			Schedule.DayNumber = 5;
			Schedule.DayNameInfo.AddError("x");
		}
	}
}
