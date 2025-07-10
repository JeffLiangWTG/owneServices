using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Scheduler.Business.Testing
{
	sealed class StmScheduleTaskRecurrenceValidationTest : BusinessObjectLookupsTestCase
	{
		public void TestValidateAll()
		{
			Recurrence.StartDateLocal = ZDateTime.Now;
			using (Recurrence.SuspendValidationTesting())
			{
				Recurrence.DayNameInfo.AddError("x");
				Recurrence.EveryMonthNumberDayAsStringInfo.AddError("x");
				Recurrence.MonthNumberAsStringInfo.AddError("x");
				Recurrence.StartDateLocalForUserInfo.AddError("x");
				Recurrence.WeekCountAsStringInfo.AddError("x");

				Recurrence.MondayInfo.AddError("x");
				Recurrence.TuesdayInfo.AddError("x");
				Recurrence.WednesdayInfo.AddError("x");
				Recurrence.ThursdayInfo.AddError("x");
				Recurrence.FridayInfo.AddError("x");
				Recurrence.SaturdayInfo.AddError("x");
				Recurrence.SundayInfo.AddError("x");
			}
			Recurrence.RunPreSaveValidation();
			AssertNoNotifications(Recurrence);
		}

		public void TestValidateDayName()
		{
			TestValidateNumber(Recurrence.DayNameInfo, Recurrence.Lookups.WeekDays);
		}

		public void TestValidateEveryMonthNumberDayAsString()
		{
			AssertNoErrors("Precondition: EveryMonthNumberDayAsString should not have errors.", Recurrence.EveryMonthNumberDayAsStringInfo);
			Assert("Precondition: Lookups.Months[0].Code should not be empty.", Recurrence.Lookups.Months[0].Code.Length > 0);
			TestValidateNumber(Recurrence.EveryMonthNumberDayAsStringInfo, Recurrence.YearlyRangeInfo, Recurrence.YearlyWeekDayInfo, Recurrence.Lookups.Months);
		}

		public void TestValidateMonthNumberAsString()
		{
			AssertNoErrors("Precondition: MonthNumberAsString should not have errors.", Recurrence.MonthNumberAsStringInfo);
			Assert("Precondition: Lookups.Months[0].Code should not be empty.", Recurrence.Lookups.Months[0].Code.Length > 0);
			TestValidateNumber(Recurrence.MonthNumberAsStringInfo, Recurrence.YearlyRangeInfo, Recurrence.YearlyWeekDayInfo, Recurrence.Lookups.Months);
		}

		public void TestValidateStartDate()
		{
			AssertNoNotifications("Precondition: StartDate should not have notifications.", Recurrence.StartDateLocalForUserInfo);

			Recurrence.AccountingRange = true;
			Recurrence.StartDateLocalForUser = ZDateTime.Empty;
			AssertHasError(Recurrence.StartDateLocalForUserInfo, "Please enter a value.");
			AssertNoWarnings(Recurrence.StartDateLocalForUserInfo);

			Recurrence.StartDateLocalForUser = ZDateTime.Now;
			AssertNoErrors(Recurrence.StartDateLocalForUserInfo);
			AssertHasWarning(Recurrence.StartDateLocalForUserInfo, "An Accounting Period does not exist for the entered Start Date.");

			Recurrence.DailyRange = true;
			Recurrence.Validation.ValidateStartDateLocalForUser();
			AssertNoNotifications(Recurrence.StartDateLocalForUserInfo);

			AccountingPeriodTestHelper helper = new AccountingPeriodTestHelper(Factory);
			helper.PostPeriodsForEntireYear(ZDateTime.Today.Year - 1);
			helper.PostPeriodsForEntireYear(ZDateTime.Today.Year);
			helper.PostPeriodsForEntireYear(ZDateTime.Today.Year + 1);
			Factory.Save();

			Recurrence.AccountingRange = true;
			Recurrence.Validation.ValidateStartDateLocalForUser();
			AssertNoNotifications(Recurrence.StartDateLocalForUserInfo);
		}

		public void TestValidateWeekCountAsString()
		{
			TestValidateNumber(Recurrence.WeekCountAsStringInfo, Recurrence.Lookups.WeekCounts);
		}

		public void TestValidateWeeklyRange()
		{
			ValidateDay(Recurrence.MondayInfo);
			ValidateDay(Recurrence.TuesdayInfo);
			ValidateDay(Recurrence.WednesdayInfo);
			ValidateDay(Recurrence.ThursdayInfo);
			ValidateDay(Recurrence.FridayInfo);
			ValidateDay(Recurrence.SaturdayInfo);
			ValidateDay(Recurrence.SundayInfo);
		}

		public void ValidateDay(ZPropertyInfo dayInfo)
		{
			AssertNoNotifications("Precondition: Monday should not have notifications.", dayInfo);
			Recurrence.DailyRange = true;
			Recurrence.DayList = "NNNNNNN";
			AssertNoErrors(dayInfo);

			Recurrence.WeeklyRange = true;
			Recurrence.DayList = "NNNNNNN";
			//Recurrence.RunPreSaveValidation();
			//Recurrence.Validation.ValidateWeekDays();
			AssertHasError(dayInfo, "Please select at least one day.");

			Recurrence.DayList = "NNNYNNN";
			//Recurrence.Validation.ValidateWeekDays();
			AssertNoErrors(dayInfo);
		}

		//public void TestValidateMonday()
		//{
		//	AssertNoNotifications("Precondition: Monday should not have notifications.", Recurrence.MondayInfo);
		//	Recurrence.DailyRange = true;
		//	Recurrence.DayList = "NNNNNNN";
		//	AssertNoErrors(Recurrence.MondayInfo);

		//	Recurrence.WeeklyRange = true;
		//	Recurrence.DayList = "NNNNNNN";
		//	Recurrence.RunPreSaveValidation();
		//	AssertHasError(Recurrence.MondayInfo, "Please select at least one day.");

		//	Recurrence.DayList = "NNNYNNN";
		//	Recurrence.RunPreSaveValidation();
		//	AssertNoErrors(Recurrence.MondayInfo);
		//}

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

		void TestValidateNumber(ZPropertyInfo propertyInfo, CodeDescriptionPairList validList)
		{
			AssertNoErrors("Precondition: " + propertyInfo.Name + " should not have errors.", propertyInfo);
			Assert("Precondition: validList[0].Code should not be empty.", validList[0].Code.Length > 0);

			TestValidateNumber(propertyInfo, recurrence.MonthlyRangeInfo, recurrence.MonthlyWeekDayInfo, validList);
			TestValidateNumber(propertyInfo, recurrence.AccountingRangeInfo, recurrence.AccountingWeekDayInfo, validList);
			TestValidateNumber(propertyInfo, recurrence.YearlyRangeInfo, recurrence.YearlyWeekDayInfo, validList);
		}

		void TestValidateNumber(ZPropertyInfo validatedPropertyInfo, ZPropertyInfo rangePropertyInfo, ZPropertyInfo weekDayPropertyInfo, CodeDescriptionPairList validList)
		{
			ZString validValue = validList[0].Code;
			ZString invalidValue = "!";

			rangePropertyInfo.Value = ZBool.True;
			weekDayPropertyInfo.Value = ZBool.True;

			validatedPropertyInfo.Value = ZString.Empty;
			AssertHasError(validatedPropertyInfo, "Please enter a value.");

			rangePropertyInfo.Value = ZBool.True;
			weekDayPropertyInfo.Value = ZBool.True;

			validatedPropertyInfo.Value = invalidValue;
			AssertHasError(validatedPropertyInfo, "Enter a valid selection.");

			validatedPropertyInfo.Value = validValue;
			AssertNoErrors(validatedPropertyInfo);

			Recurrence.TaskPeriod = "";
			validatedPropertyInfo.Value = invalidValue;
			AssertNoErrors(validatedPropertyInfo);
		}

		StmScheduleTaskRecurrence recurrence;

		#endregion
	}
}
