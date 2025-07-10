using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Scheduler.Business.Testing
{
	sealed class ScheduleCalculatorTest : TestCaseWithFactory
	{
		public void TestGetCorrectedDate()
		{
			AssertEquals("GetCorrectedDate(2000, 1, -1)", new ZDateTime(2000, 1, 1), Calculator.GetCorrectedDate(2000, 1, -1));
			AssertEquals("GetCorrectedDate(2000, 1, 2)", new ZDateTime(2000, 1, 2), Calculator.GetCorrectedDate(2000, 1, 2));
			AssertEquals("GetCorrectedDate(2000, 1, 99)", new ZDateTime(2000, 1, 31), Calculator.GetCorrectedDate(2000, 1, 99));
		}

		[TestDate(2023, 1, 1)]
		public void TestGetCorrectedDateIfInputInvalidValue()
		{
			AssertEquals(new ZDateTime(2023, 1, 1), Calculator.GetCorrectedDate(1, 1, 1));
			AssertEquals(new ZDateTime(2023, 1, 1), Calculator.GetCorrectedDate(1, 1, 0));

			TestDateAttribute.Date = new DateTime(2023, 4, 1);

			AssertEquals(new ZDateTime(2024, 1, 1), Calculator.GetCorrectedDate(1, 1, 1));
			AssertEquals(new ZDateTime(2024, 1, 1), Calculator.GetCorrectedDate(1, 1, 0));
		}

		public void TestConvertToDayOfWeek()
		{
			try
			{
				Calculator.ConvertToDayOfWeek(0);
				Fail("ConvertToDayOfWeek should throw if day is less than 1.");
			}
			catch (ArgumentOutOfRangeException ex)
			{
#if NETFRAMEWORK
				AssertEquals("ex.Message", "Day cannot be less than one or more than seven.\r\nParameter name: day", ex.Message);
#elif NET
				AssertEquals("ex.Message", "Day cannot be less than one or more than seven. (Parameter 'day')", ex.Message);
#endif
			}

			AssertEquals("ConvertToDayOfWeek(1)", DayOfWeek.Sunday, Calculator.ConvertToDayOfWeek(1));
			AssertEquals("ConvertToDayOfWeek(2)", DayOfWeek.Monday, Calculator.ConvertToDayOfWeek(2));
			AssertEquals("ConvertToDayOfWeek(3)", DayOfWeek.Tuesday, Calculator.ConvertToDayOfWeek(3));
			AssertEquals("ConvertToDayOfWeek(4)", DayOfWeek.Wednesday, Calculator.ConvertToDayOfWeek(4));
			AssertEquals("ConvertToDayOfWeek(5)", DayOfWeek.Thursday, Calculator.ConvertToDayOfWeek(5));
			AssertEquals("ConvertToDayOfWeek(6)", DayOfWeek.Friday, Calculator.ConvertToDayOfWeek(6));
			AssertEquals("ConvertToDayOfWeek(7)", DayOfWeek.Saturday, Calculator.ConvertToDayOfWeek(7));

			try
			{
				Calculator.ConvertToDayOfWeek(8);
				Fail("ConvertToDayOfWeek should throw if day is more than 7.");
			}
			catch (ArgumentOutOfRangeException ex)
			{
#if NETFRAMEWORK
				AssertEquals("ex.Message", "Day cannot be less than one or more than seven.\r\nParameter name: day", ex.Message);
#elif NET
				AssertEquals("ex.Message", "Day cannot be less than one or more than seven. (Parameter 'day')", ex.Message);
#endif
			}
		}

		public void TestCalculateDayOfWeek()
		{
			ZDateTime baseDateTime = new ZDateTime(2005, 5, 9);
			AssertEquals("CalculateDayOfWeek(baseDateTime, -3, 1)", new ZDateTime(2005, 4, 17), Calculator.CalculateDayOfWeek(baseDateTime, -3, 1));
			AssertEquals("CalculateDayOfWeek(baseDateTime, -2, 2)", new ZDateTime(2005, 4, 25), Calculator.CalculateDayOfWeek(baseDateTime, -2, 2));
			AssertEquals("CalculateDayOfWeek(baseDateTime, -1, 3)", new ZDateTime(2005, 5, 3), Calculator.CalculateDayOfWeek(baseDateTime, -1, 3));
			AssertEquals("CalculateDayOfWeek(baseDateTime, 0, 4)", new ZDateTime(2005, 5, 11), Calculator.CalculateDayOfWeek(baseDateTime, 0, 4));
			AssertEquals("CalculateDayOfWeek(baseDateTime, 1, 5)", new ZDateTime(2005, 5, 19), Calculator.CalculateDayOfWeek(baseDateTime, 1, 5));
			AssertEquals("CalculateDayOfWeek(baseDateTime, 2, 6)", new ZDateTime(2005, 5, 27), Calculator.CalculateDayOfWeek(baseDateTime, 2, 6));
			AssertEquals("CalculateDayOfWeek(baseDateTime, 3, 7)", new ZDateTime(2005, 6, 4), Calculator.CalculateDayOfWeek(baseDateTime, 3, 7));
		}

		public void TestCalculateDayOfMonth()
		{
			ZDateTime baseDateTime = new ZDateTime(2006, 9, 25);
			AssertEquals("CalculateDayOfMonth(baseDateTime, -3, 1)", new ZDateTime(2006, 6, 1), Calculator.CalculateDayOfMonth(baseDateTime, -3, 1));
			AssertEquals("CalculateDayOfMonth(baseDateTime, -2, 5)", new ZDateTime(2006, 7, 5), Calculator.CalculateDayOfMonth(baseDateTime, -2, 5));
			AssertEquals("CalculateDayOfMonth(baseDateTime, -1, 10)", new ZDateTime(2006, 8, 10), Calculator.CalculateDayOfMonth(baseDateTime, -1, 10));
			AssertEquals("CalculateDayOfMonth(baseDateTime, 0, 15)", new ZDateTime(2006, 9, 15), Calculator.CalculateDayOfMonth(baseDateTime, 0, 15));
			AssertEquals("CalculateDayOfMonth(baseDateTime, 1, 30)", new ZDateTime(2006, 10, 30), Calculator.CalculateDayOfMonth(baseDateTime, 1, 30));
			AssertEquals("CalculateDayOfMonth(baseDateTime, 2, 31)", new ZDateTime(2006, 11, 30), Calculator.CalculateDayOfMonth(baseDateTime, 2, 31));
			AssertEquals("CalculateDayOfMonth(baseDateTime, 3, 0)", new ZDateTime(2006, 12, 31), Calculator.CalculateDayOfMonth(baseDateTime, 3, 0));
		}

		public void TestCalculateDayOfAccountingPeriod()
		{
			ZDateTime baseDateTime = new ZDateTime(2006, 6, 15);
			AssertEquals("CalculateDayOfAccountingPeriod(baseDateTime, 0, 5)", baseDateTime, Calculator.CalculateDayOfAccountingPeriod(baseDateTime, 0, 5));

			CreateAccPeriodTestData();
			Factory.Save();
			AssertEquals("CalculateDayOfAccountingPeriod(baseDateTime, -2, 1)", new ZDateTime(2006, 1, 1), Calculator.CalculateDayOfAccountingPeriod(baseDateTime, -2, 1));
			AssertEquals("CalculateDayOfAccountingPeriod(baseDateTime, -1, 2)", new ZDateTime(2006, 1, 2), Calculator.CalculateDayOfAccountingPeriod(baseDateTime, -1, 2));
			AssertEquals("CalculateDayOfAccountingPeriod(baseDateTime, 0, 5)", new ZDateTime(2006, 4, 5), Calculator.CalculateDayOfAccountingPeriod(baseDateTime, 0, 5));
			AssertEquals("CalculateDayOfAccountingPeriod(baseDateTime, 1, 0)", new ZDateTime(2006, 9, 30), Calculator.CalculateDayOfAccountingPeriod(baseDateTime, 1, 0));
			AssertEquals("CalculateDayOfAccountingPeriod(baseDateTime, 2, 200)", new ZDateTime(2006, 12, 31), Calculator.CalculateDayOfAccountingPeriod(baseDateTime, 2, 200));
			AssertEquals("CalculateDayOfAccountingPeriod(baseDateTime, 20, 10)", new ZDateTime(2007, 10, 10), Calculator.CalculateDayOfAccountingPeriod(baseDateTime, 20, 10));
		}

		public void TestCalculateDayOfYear()
		{
			ZDateTime baseDateTime = new ZDateTime(2006, 9, 25);
			AssertEquals("CalculateDayOfYear(baseDateTime, -2, 1)", new ZDateTime(2004, 1, 1), Calculator.CalculateDayOfYear(baseDateTime, -2, 1));
			AssertEquals("CalculateDayOfYear(baseDateTime, -1, 100)", new ZDateTime(2005, 4, 10), Calculator.CalculateDayOfYear(baseDateTime, -1, 100));
			AssertEquals("CalculateDayOfYear(baseDateTime, 0, 0)", new ZDateTime(2006, 12, 31), Calculator.CalculateDayOfYear(baseDateTime, 0, 0));
			AssertEquals("CalculateDayOfYear(baseDateTime, 1, 365)", new ZDateTime(2007, 12, 31), Calculator.CalculateDayOfYear(baseDateTime, 1, 365));
			AssertEquals("CalculateDayOfYear(baseDateTime, 2, 365)", new ZDateTime(2008, 12, 30), Calculator.CalculateDayOfYear(baseDateTime, 2, 365));
			AssertEquals("CalculateDayOfYear(baseDateTime, 3, 366)", new ZDateTime(2009, 12, 31), Calculator.CalculateDayOfYear(baseDateTime, 3, 366));
		}

		public void TestCalculateDayOfYear_NoException()
		{
			ZDateTime baseDateTime = new ZDateTime(2006, 9, 25);
			AssertNoExceptionThrown(() => Calculator.CalculateDayOfYear(new ZDateTime(2004, 1, 1), -3000, 1));
			AssertNoExceptionThrown(() => Calculator.CalculateDayOfYear(new ZDateTime(2004, 1, 1), 9000, 1));
			AssertNoExceptionThrown(() => Calculator.CalculateDayOfYear(new ZDateTime(2004, 1, 1), -3000, 365));
			AssertNoExceptionThrown(() => Calculator.CalculateDayOfYear(new ZDateTime(2004, 1, 1), 9000, 365));
		}

		public void TestCalculateAccountingPeriod()
		{
			ZDateTime baseDateTime = new ZDateTime(2006, 6, 15);
			AssertEquals("CalculateAccountingPeriod(baseDateTime, 0)", 0, Calculator.CalculateAccountingPeriod(baseDateTime, 0));

			CreateAccPeriodTestData();
			//Factory.Save(); ?
			AssertEquals("CalculateAccountingPeriod(baseDateTime, -2)", 200601, Calculator.CalculateAccountingPeriod(baseDateTime, -2));
			AssertEquals("CalculateAccountingPeriod(baseDateTime, -1)", 200601, Calculator.CalculateAccountingPeriod(baseDateTime, -1));
			AssertEquals("CalculateAccountingPeriod(baseDateTime, 0)", 200604, Calculator.CalculateAccountingPeriod(baseDateTime, 0));
			AssertEquals("CalculateAccountingPeriod(baseDateTime, 1)", 200607, Calculator.CalculateAccountingPeriod(baseDateTime, 1));
			AssertEquals("CalculateAccountingPeriod(baseDateTime, 2)", 200610, Calculator.CalculateAccountingPeriod(baseDateTime, 2));
			AssertEquals("CalculateAccountingPeriod(baseDateTime, 20)", 200710, Calculator.CalculateAccountingPeriod(baseDateTime, 20));
			AssertEquals("CalculateAccountingPeriod(200604, 1)", 200607, Calculator.CalculateAccountingPeriod(200604, 1));
		}

		public void TestAccPeriodCalculator()
		{
			CreateAccPeriodTestData();
			Factory.Save();
			AssertEquals("AccPeriodCalculator.Factory", Factory, Calculator.AccPeriodCalculator.GetPeriodManagementFromDate(new ZDateTime(2006, 10, 1)).Factory);
			Assert("AccPeriodCalculator.Factory should be a different one.", new ScheduleCalculator().AccPeriodCalculator.GetPeriodManagementFromDate(new ZDateTime(2006, 10, 1)).Factory != Factory);
		}

		public void TestAccPeriodCalculatorIsConstructedWithCompanyWhenItIsFilled()
		{
			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			ScheduleCalculator sCalc = new ScheduleCalculator();
			Assert("ScheduleCalculator should not pass scheduleCompany to its AccPeriodCalculator if it was not passed to ScheduleCalculator's constructor", !(sCalc.AccountingPeriodCalculatorWasCreatedWithScheduledTaskCompany_ForTest));
			AccountingPeriodCalculator accPCalc = sCalc.AccPeriodCalculator; //this is needed to run AccPeriodCalculator.get
			AssertEquals("ScheduleCalculator.scheduledTaskCompany should be null when scheduleCompany was not passed to ScheduleCalculator's constructor", null, sCalc.scheduledTaskCompany);

			sCalc = new ScheduleCalculator(Factory);
			Assert("ScheduleCalculator should not pass scheduleCompany to its AccPeriodCalculator if it was not passed to ScheduleCalculator's constructor", !(sCalc.AccountingPeriodCalculatorWasCreatedWithScheduledTaskCompany_ForTest));
			accPCalc = sCalc.AccPeriodCalculator;
			AssertEquals("ScheduleCalculator.scheduledTaskCompany should be null when scheduleCompany was not passed to ScheduleCalculator's constructor", null, sCalc.scheduledTaskCompany);

			sCalc = new ScheduleCalculator(company);
			AssertEquals("scheduleCompany passed to ScheduleCalculator's constructor should be put to ScheduleCalculator.scheduledTaskCompany", company, sCalc.scheduledTaskCompany);
			accPCalc = sCalc.AccPeriodCalculator;
			Assert("ScheduleCalculator should pass scheduleCompany to its AccPeriodCalculator if it was passed to ScheduleCalculator's constructor", sCalc.AccountingPeriodCalculatorWasCreatedWithScheduledTaskCompany_ForTest);

			sCalc = new ScheduleCalculator(Factory, company);
			AssertEquals("scheduleCompany passed to ScheduleCalculator's constructor should be put to ScheduleCalculator.scheduledTaskCompany", company, sCalc.scheduledTaskCompany);
			accPCalc = sCalc.AccPeriodCalculator;
			Assert("ScheduleCalculator should pass scheduleCompany to its AccPeriodCalculator if it was passed to ScheduleCalculator's constructor", sCalc.AccountingPeriodCalculatorWasCreatedWithScheduledTaskCompany_ForTest);
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

		ScheduleCalculator Calculator
		{
			get
			{
				if (calculator == null)
				{
					calculator = new ScheduleCalculator(Factory);
				}
				return calculator;
			}
		}

		ScheduleCalculator calculator;
	}
}
