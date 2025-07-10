using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.PeriodManagement
{
	[TestedType(typeof(NewYearPeriodSettings))]
	public class NewYearPeriodSettingsTest : NonPersistentBusinessObjectTestCase
	{
		public void TestStartAndEndDateDefaultValues()
		{
			NewYearPeriodSettings periodSettings = (NewYearPeriodSettings)GetNewBusinessObject();
			AssertEquals("StartDate is empty by default", true, periodSettings.StartDate.IsEmpty);
			AssertEquals("EndDate is empty by default", true, periodSettings.EndDate.IsEmpty);
		}

		[TestDate(2023, 12, 12)]
		public void TestIgnoreStartDateValidation()
		{
			var periodSettings = (NewYearPeriodSettings)GetNewBusinessObject();
			periodSettings.EndDate = new ZDateTime(2000, 6, 30);
			periodSettings.StartDate = new ZDateTime(1997, 12, 1);
			AssertHasError(periodSettings.StartDateInfo, "Start date must be equal to or later than 1/Jan/2022");
			periodSettings = new NewYearPeriodSettings(true);
			periodSettings.StartDate = new ZDateTime(1997, 11, 1);
			AssertNoError(periodSettings.StartDateInfo, "Start date must be equal to or later than 1/Jan/2022");
		}

		[TestDate(1999, 7, 2)]
		public void TestValidateStartDate()
		{
			var periodSettings = (NewYearPeriodSettings)GetNewBusinessObject();
			periodSettings.PeriodFormat = Core.Constants.ACPeriodFormat.FourWeeks;
			periodSettings.EndDate = new ZDateTime(2000, 6, 30);
			periodSettings.StartDate = new ZDateTime(1997, 12, 1);
			AssertHasError(periodSettings.StartDateInfo, "Start date must be equal to or later than 1/Jan/1998");
			periodSettings.StartDate = new ZDateTime(1999, 7, 3);
			AssertHasError(periodSettings.StartDateInfo, "Start date must be less than or equal to today");
			periodSettings.StartDate = periodSettings.EndDate.AddDays(-(periodSettings.MaximumNumberOfDaysInFinancialYear + 1));
			AssertHasError(periodSettings.StartDateInfo, "The maximum number of days in a financial year is " + periodSettings.MaximumNumberOfDaysInFinancialYear + " days");
			periodSettings.StartDate = periodSettings.EndDate.AddDays(-periodSettings.MaximumNumberOfDaysInFinancialYear);
			AssertEquals("Valid start date", false, periodSettings.StartDateInfo.HasErrors());
			periodSettings.StartDate = new ZDateTime(1999, 7, 2);
			AssertEquals("Valid start date", false, periodSettings.StartDateInfo.HasErrors());
			AssertHasWarning(periodSettings.StartDateInfo, "Financial year normally starts on 1st day of a month");
		}

		[TestDate(1999, 7, 2)]
		public void TestValidateEndDate()
		{
			NewYearPeriodSettings periodSettings = (NewYearPeriodSettings)GetNewBusinessObject();
			periodSettings.StartDate = new ZDateTime(2000, 7, 1);
			periodSettings.PeriodFormat = Core.Constants.ACPeriodFormat.FourWeeks;
			periodSettings.EndDate = new ZDateTime(2000, 7, 1);
			AssertHasError(periodSettings.EndDateInfo, "End date must be later than start date");
			periodSettings.EndDate = periodSettings.StartDate.AddDays(periodSettings.MaximumNumberOfDaysInFinancialYear + 1);
			AssertHasError(periodSettings.EndDateInfo, "The maximum number of days in a financial year is " + periodSettings.MaximumNumberOfDaysInFinancialYear + " days");
			periodSettings.EndDate = new ZDateTime(2001, 6, 29);
			AssertHasWarning(periodSettings.EndDateInfo, "Financial year normally ends on the last day of a month");
			periodSettings.EndDate = new ZDateTime(2001, 6, 30);
			AssertEquals("Valid end date", false, periodSettings.EndDateInfo.HasErrors());
			periodSettings.EndDate = new ZDateTime(2001, 12, 31);
			AssertEquals("Valid end date", false, periodSettings.EndDateInfo.HasWarnings());
		}

		[TestDate(1999, 7, 2)]
		public void TestCALmonthTypeStartDate()
		{
			NewYearPeriodSettings periodSettings = (NewYearPeriodSettings)GetNewBusinessObject();
			periodSettings.PeriodFormat = Core.Constants.ACPeriodFormat.Month;
			periodSettings.StartDate = new ZDateTime(1999, 7, 2);
			AssertEquals("Invalid start date", true, periodSettings.StartDateInfo.HasErrors());
			periodSettings.StartDate = new ZDateTime(1999, 7, 1);
			AssertEquals("Valid start date", false, periodSettings.StartDateInfo.HasErrors());
		}

		[TestDate(1999, 7, 2)]
		public void TestMinimumNumberOfDaysInFinancialYear()
		{
			NewYearPeriodSettings periodSettings = (NewYearPeriodSettings)GetNewBusinessObject();
			periodSettings.PeriodFormat = Core.Constants.ACPeriodFormat.FourWeeks;
			periodSettings.StartDate = new ZDateTime(1999, 7, 1);
			periodSettings.EndDate = periodSettings.StartDate.AddDays(periodSettings.MinimumNumberOfDaysInFinancialYear - 1);
			AssertEquals("Invalid end date", true, periodSettings.EndDateInfo.HasErrors());
			periodSettings.EndDate = periodSettings.StartDate.AddDays(periodSettings.MinimumNumberOfDaysInFinancialYear);
			AssertEquals("Valid end date", false, periodSettings.EndDateInfo.HasErrors());
		}

		[TestDate(1999, 7, 2)]
		public void TestMaximumNumberOfDaysInFinancialYear()
		{
			NewYearPeriodSettings periodSettings = (NewYearPeriodSettings)GetNewBusinessObject();
			periodSettings.PeriodFormat = Core.Constants.ACPeriodFormat.FourWeeks;
			periodSettings.StartDate = new ZDateTime(1999, 7, 1);
			periodSettings.EndDate = periodSettings.StartDate.AddDays(periodSettings.MaximumNumberOfDaysInFinancialYear + 1);
			AssertEquals("Invalid end date", true, periodSettings.EndDateInfo.HasErrors());
			periodSettings.EndDate = periodSettings.StartDate.AddDays(periodSettings.MaximumNumberOfDaysInFinancialYear);
			AssertEquals("Valid end date", false, periodSettings.EndDateInfo.HasErrors());
		}

		public void TestDayOfWeek()
		{
			NewYearPeriodSettings periodSettings = (NewYearPeriodSettings)GetNewBusinessObject();
			periodSettings.WeekDay = DayOfWeekCodeList.Codes.Sunday;
			AssertEquals(0, periodSettings.DayOfWeek);
			periodSettings.WeekDay = DayOfWeekCodeList.Codes.Monday;
			AssertEquals(1, periodSettings.DayOfWeek);
			periodSettings.WeekDay = DayOfWeekCodeList.Codes.Tuesday;
			AssertEquals(2, periodSettings.DayOfWeek);
			periodSettings.WeekDay = DayOfWeekCodeList.Codes.Wednesday;
			AssertEquals(3, periodSettings.DayOfWeek);
			periodSettings.WeekDay = DayOfWeekCodeList.Codes.Thursday;
			AssertEquals(4, periodSettings.DayOfWeek);
			periodSettings.WeekDay = DayOfWeekCodeList.Codes.Friday;
			AssertEquals(5, periodSettings.DayOfWeek);
			periodSettings.WeekDay = DayOfWeekCodeList.Codes.Saturday;
			AssertEquals(6, periodSettings.DayOfWeek);
		}

		public void TestRunPreSaveValidation()
		{
			NewYearPeriodSettings periodSettings = (NewYearPeriodSettings)GetNewBusinessObject();
			AssertEquals("Startdate has error", true, periodSettings.StartDateInfo.HasErrors());
			AssertEquals("Enddate has no errors yet", false, periodSettings.EndDateInfo.HasErrors());
			AssertEquals("PeriodFormat has no errors yet", false, periodSettings.PeriodFormatInfo.HasErrors());
			AssertEquals("WeekDay has no errors yet", false, periodSettings.WeekDayInfo.HasErrors());
			AssertEquals("AccountingYearBasedType has no errors yet", false, periodSettings.AccountingYearBasedTypeInfo.HasErrors());
			periodSettings.RunPreSaveValidation();
			AssertEquals("Startdate is validated", true, periodSettings.StartDateInfo.HasErrors());
			AssertEquals("Enddate is validated", true, periodSettings.EndDateInfo.HasErrors());
			AssertEquals("PeriodFormat is validated", false, periodSettings.PeriodFormatInfo.HasErrors());
			AssertEquals("WeekDay is validated", false, periodSettings.WeekDayInfo.HasErrors());
			AssertEquals("AccountingYearBasedType is validated", false, periodSettings.AccountingYearBasedTypeInfo.HasErrors());
		}

		[TestDate(1999, 7, 2)]
		public void TestCALEndDate()
		{
			NewYearPeriodSettings periodSettings = (NewYearPeriodSettings)GetNewBusinessObject();
			periodSettings.PeriodFormat = Core.Constants.ACPeriodFormat.Month;
			AssertEquals("Should be Read Only", true, periodSettings.EndDateInfo.ReadOnly);
			periodSettings.StartDate = new ZDateTime(1999, 7, 1);
			AssertEquals("End date should be 30/6/2000", new ZDateTime(2000, 6, 30), periodSettings.EndDate.Date);
			periodSettings.PeriodFormat = Core.Constants.ACPeriodFormat.FourWeeks;
			AssertEquals("Should NOT be Read Only", false, periodSettings.EndDateInfo.ReadOnly);
			periodSettings.PeriodFormat = Core.Constants.ACPeriodFormat.FourFourFive;
			AssertEquals("Should NOT be Read Only", false, periodSettings.EndDateInfo.ReadOnly);
			periodSettings.PeriodFormat = Core.Constants.ACPeriodFormat.Weeks;
			AssertEquals("Should NOT be Read Only", false, periodSettings.EndDateInfo.ReadOnly);
		}

		[TestDate(2023, 1, 1)]
		public void TestAccountingYearBasedTypeValidation()
		{
			NewYearPeriodSettings periodSettings = (NewYearPeriodSettings)GetNewBusinessObject();
			AssertEquals(3, periodSettings.AccountingYearBasedTypeInfo.MaxLength);

			periodSettings.AccountingYearBasedType = "CCC";
			periodSettings.RunPreSaveValidation();
			AssertHasError(periodSettings.AccountingYearBasedTypeInfo, "Enter a valid selection.");

			periodSettings.StartDate = new ZDateTime(2023, 1, 1);
			periodSettings.EndDate = new ZDateTime(2023, 12, 30);
			periodSettings.AccountingYearBasedType = NewYearPeriodSettings.AccountingYearBaseTypes.StartDateCalendarYear;
			periodSettings.RunPreSaveValidation();
			AssertHasError(periodSettings.AccountingYearBasedTypeInfo, "The Accounting Year Based On option should remain as EDY if the Start Date and End Date fall into the same year.");
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new NewYearPeriodSettings();
		}
	}
}
