using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class JobDeclarationExtensionsTest : TestCase
	{
		public void TestGetHardCodedHolidays()
		{
			var testDeclaration = new BusinessObjectFactory().New<JobDeclaration>();
			CombineAssertions(() =>
			{
				AssertEquals("Test For Empty", 162, testDeclaration.GetHardCodedCAHolidays(string.Empty).Count());
				AssertEquals("Test For XX", 162, testDeclaration.GetHardCodedCAHolidays("XX").Count());
				AssertEquals("Test For Alberta", 194, testDeclaration.GetHardCodedCAHolidays(CanadianProvinceList.Codes.Alberta).Count());
				AssertEquals("Test For BritishColumbia", 194, testDeclaration.GetHardCodedCAHolidays(CanadianProvinceList.Codes.BritishColumbia).Count());
				AssertEquals("Test For Manitoba", 194, testDeclaration.GetHardCodedCAHolidays(CanadianProvinceList.Codes.Manitoba).Count());
				AssertEquals("Test For NewBrunswick", 189, testDeclaration.GetHardCodedCAHolidays(CanadianProvinceList.Codes.NewBrunswick).Count());
				AssertEquals("Test For NewfoundlandAndLabrador", 178, testDeclaration.GetHardCodedCAHolidays(CanadianProvinceList.Codes.NewfoundlandAndLabrador).Count());
				AssertEquals("Test For NorthwestTerritories", 178, testDeclaration.GetHardCodedCAHolidays(CanadianProvinceList.Codes.NorthwestTerritories).Count());
				AssertEquals("Test For NovaScotia", 190, testDeclaration.GetHardCodedCAHolidays(CanadianProvinceList.Codes.NovaScotia).Count());
				AssertEquals("Test For Nunavut", 190, testDeclaration.GetHardCodedCAHolidays(CanadianProvinceList.Codes.Nunavut).Count());
				AssertEquals("Test For Ontario", 194, testDeclaration.GetHardCodedCAHolidays(CanadianProvinceList.Codes.Ontario).Count());
				AssertEquals("Test For PrinceEdwardIsland", 193, testDeclaration.GetHardCodedCAHolidays(CanadianProvinceList.Codes.PrinceEdwardIsland).Count());
				AssertEquals("Test For Quebec", 178, testDeclaration.GetHardCodedCAHolidays(CanadianProvinceList.Codes.Quebec).Count());
				AssertEquals("Test For Saskatchewan", 194, testDeclaration.GetHardCodedCAHolidays(CanadianProvinceList.Codes.Saskatchewan).Count());
				AssertEquals("Test For YukonTerritory", 178, testDeclaration.GetHardCodedCAHolidays(CanadianProvinceList.Codes.YukonTerritory).Count());
			});
		}

		public void TestChristmasHolidaysForCanadaThrough2029()
		{
			AssertExpectedChristmasHoliday(2021, 27, 28);
			AssertExpectedChristmasHoliday(2022, 26, 27);
			AssertExpectedChristmasHoliday(2023, 25, 26);
			AssertExpectedChristmasHoliday(2024, 25, 26);
			AssertExpectedChristmasHoliday(2025, 25, 26);
			AssertExpectedChristmasHoliday(2026, 25, 28);
			AssertExpectedChristmasHoliday(2027, 27, 28);
			AssertExpectedChristmasHoliday(2028, 25, 26);
			AssertExpectedChristmasHoliday(2029, 25, 26);
		}

		void AssertExpectedChristmasHoliday(int year, int firstDayOfHoliday, int secondDayOfHoliday)
		{
			var christmasDay = new ZDateTime(year, 12, 25);
			var dayOfWeek = christmasDay.DayOfWeek;
			ZDateTime expectedFirstDay;
			ZDateTime expectedSecondDay;
			if (dayOfWeek == DayOfWeek.Monday ||
				dayOfWeek == DayOfWeek.Tuesday ||
				dayOfWeek == DayOfWeek.Wednesday ||
				dayOfWeek == DayOfWeek.Thursday)
			{
				expectedFirstDay = christmasDay;
				expectedSecondDay = christmasDay.AddDays(1);
			}
			else if (dayOfWeek == DayOfWeek.Saturday)
			{
				expectedFirstDay = christmasDay.AddDays(2);
				expectedSecondDay = christmasDay.AddDays(3);
			}
			else if (dayOfWeek == DayOfWeek.Sunday)
			{
				expectedFirstDay = christmasDay.AddDays(1);
				expectedSecondDay = christmasDay.AddDays(2);
			}
			else
			{
				expectedFirstDay = christmasDay;
				expectedSecondDay = christmasDay.AddDays(3);
			}

			var firstDay = new ZDateTime(year, 12, firstDayOfHoliday);
			var secondDay = new ZDateTime(year, 12, secondDayOfHoliday);
			AssertEquals(expectedFirstDay, firstDay);
			AssertEquals(expectedSecondDay, secondDay);
		}

		public void TestNewYearHoliday()
		{
			AssertNewYearHoliday(2022, 3);
			AssertNewYearHoliday(2023, 2);
		}

		void AssertNewYearHoliday(int year, int holiday)
		{
			var newYear = new ZDateTime(year, 1, 1);
			var dayOfWeek = newYear.DayOfWeek;
			ZDateTime expectedHoliday;
			if (dayOfWeek == DayOfWeek.Saturday)
			{
				expectedHoliday = newYear.AddDays(2);
			}
			else if (dayOfWeek == DayOfWeek.Sunday)
			{
				expectedHoliday = newYear.AddDays(1);
			}
			else
			{
				expectedHoliday = newYear;
			}

			var holidayDate = new ZDateTime(year, 1, holiday);
			AssertEquals(expectedHoliday, holidayDate);
		}
	}
}
