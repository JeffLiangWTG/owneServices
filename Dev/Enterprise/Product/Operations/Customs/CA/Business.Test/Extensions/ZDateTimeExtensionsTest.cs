using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ZDateTimeExtensionsTest : TestCase
	{
		JobDeclaration GetTestDeclaration()
		{
			return new BusinessObjectFactory().New<JobDeclaration>();
		}

		public void TestAddWorkingDays()
		{
			var holidaysList = GetTestDeclaration().GetHardCodedCAHolidays(string.Empty);
			var dateTime = new ZDateTime(2012, 08, 03);
			AssertEquals(new ZDateTime(2012, 08, 06), dateTime.AddWorkingDaysFromNearestWorkingDay(1, holidaysList));
			AssertEquals(new ZDateTime(2012, 08, 07), dateTime.AddWorkingDaysFromNearestWorkingDay(2, holidaysList));
			AssertEquals(new ZDateTime(2012, 08, 10), dateTime.AddWorkingDaysFromNearestWorkingDay(5, holidaysList));
			AssertEquals(new ZDateTime(2012, 08, 13), dateTime.AddWorkingDaysFromNearestWorkingDay(6, holidaysList));
			AssertEquals(new ZDateTime(2012, 08, 02), dateTime.AddWorkingDaysFromNearestWorkingDay(-1, holidaysList));
			AssertEquals(new ZDateTime(2012, 07, 27), dateTime.AddWorkingDaysFromNearestWorkingDay(-5, holidaysList));

			dateTime = new ZDateTime(2012, 08, 04);
			AssertEquals(new ZDateTime(2012, 08, 06), dateTime.AddWorkingDaysFromNearestWorkingDay(0, holidaysList));
			AssertEquals(new ZDateTime(2012, 08, 02), dateTime.AddWorkingDaysFromNearestWorkingDay(-1, holidaysList));
			AssertEquals(new ZDateTime(2012, 08, 07), dateTime.AddWorkingDaysFromNearestWorkingDay(1, holidaysList));

			dateTime = new ZDateTime(2012, 08, 05);
			AssertEquals(new ZDateTime(2012, 08, 06), dateTime.AddWorkingDaysFromNearestWorkingDay(0, holidaysList));
			AssertEquals(new ZDateTime(2012, 08, 02), dateTime.AddWorkingDaysFromNearestWorkingDay(-1, holidaysList));
		}

		[TestDate(2012, 08, 06)]
		public void TestGetWorkingDaysTo()
		{
			var holidaysList = GetTestDeclaration().GetHardCodedCAHolidays(string.Empty);
			var dateTime = new ZDateTime(2012, 08, 03);
			AssertEquals(0, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2012, 08, 04), holidaysList));
			AssertEquals(0, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2012, 08, 05), holidaysList));
			AssertEquals(1, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2012, 08, 06), holidaysList));
			AssertEquals(6, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2012, 08, 13), holidaysList));
			AssertEquals(-5, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2012, 07, 27), holidaysList));

			dateTime = new ZDateTime(2012, 07, 27);
			AssertEquals(6, dateTime.GetCountOfWorkingDaysTo(ZDateTime.Today, holidaysList));

			dateTime = new ZDateTime(2012, 08, 04);
			AssertEquals(0, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2012, 08, 06), holidaysList));
			AssertEquals(-1, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2012, 08, 02), holidaysList));
			AssertEquals(4, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2012, 08, 12), holidaysList));

			dateTime = new ZDateTime(2012, 08, 05);
			AssertEquals(0, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2012, 08, 06), holidaysList));
			AssertEquals(-1, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2012, 08, 02), holidaysList));
			AssertEquals(4, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2012, 08, 12), holidaysList));

			dateTime = new ZDateTime(2016, 06, 17);
			AssertEquals(0, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2016, 06, 18), holidaysList));
			AssertEquals(0, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2016, 06, 19), holidaysList));
			AssertEquals(1, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2016, 06, 20), holidaysList));
			AssertEquals(2, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2016, 06, 21), holidaysList));

			dateTime = new ZDateTime(2016, 06, 18);
			AssertEquals(0, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2016, 06, 19), holidaysList));
			AssertEquals(0, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2016, 06, 20), holidaysList));
			AssertEquals(1, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2016, 06, 21), holidaysList));

			dateTime = new ZDateTime(2016, 06, 19);
			AssertEquals(0, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2016, 06, 20), holidaysList));
			AssertEquals(1, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2016, 06, 21), holidaysList));

			dateTime = new ZDateTime(2016, 06, 20);
			AssertEquals(0, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2016, 06, 20), holidaysList));
			AssertEquals(1, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2016, 06, 21), holidaysList));

			dateTime = new ZDateTime(2016, 01, 01);
			AssertEquals(0, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2016, 01, 04), holidaysList));
			AssertEquals(1, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2016, 01, 05), holidaysList));
		}

		public void TestGetWorkingDaysTo_DayZoreStartOnTheFirstBusinessDay()
		{
			var holidaysList = GetTestDeclaration().GetHardCodedCAHolidays(string.Empty);
			AssertEquals(8, new ZDateTime(2022, 04, 30).GetCountOfWorkingDaysTo(new ZDateTime(2022, 05, 12), holidaysList));
			AssertEquals(8, new ZDateTime(2022, 05, 01).GetCountOfWorkingDaysTo(new ZDateTime(2022, 05, 12), holidaysList));
			AssertEquals(8, new ZDateTime(2022, 05, 02).GetCountOfWorkingDaysTo(new ZDateTime(2022, 05, 12), holidaysList));
			AssertEquals(7, new ZDateTime(2022, 05, 03).GetCountOfWorkingDaysTo(new ZDateTime(2022, 05, 12), holidaysList));

			AssertEquals(0, new ZDateTime(2022, 04, 30).GetCountOfWorkingDaysTo(new ZDateTime(2022, 04, 30), holidaysList));
			AssertEquals(0, new ZDateTime(2022, 04, 30).GetCountOfWorkingDaysTo(new ZDateTime(2022, 05, 01), holidaysList));
			AssertEquals(0, new ZDateTime(2022, 04, 30).GetCountOfWorkingDaysTo(new ZDateTime(2022, 05, 02), holidaysList));
			AssertEquals(1, new ZDateTime(2022, 04, 30).GetCountOfWorkingDaysTo(new ZDateTime(2022, 05, 03), holidaysList));
			AssertEquals(2, new ZDateTime(2022, 04, 30).GetCountOfWorkingDaysTo(new ZDateTime(2022, 05, 04), holidaysList));
			AssertEquals(3, new ZDateTime(2022, 04, 30).GetCountOfWorkingDaysTo(new ZDateTime(2022, 05, 05), holidaysList));
			AssertEquals(4, new ZDateTime(2022, 04, 30).GetCountOfWorkingDaysTo(new ZDateTime(2022, 05, 06), holidaysList));
			AssertEquals(4, new ZDateTime(2022, 04, 30).GetCountOfWorkingDaysTo(new ZDateTime(2022, 05, 07), holidaysList));
			AssertEquals(4, new ZDateTime(2022, 04, 30).GetCountOfWorkingDaysTo(new ZDateTime(2022, 05, 08), holidaysList));
			AssertEquals(5, new ZDateTime(2022, 04, 30).GetCountOfWorkingDaysTo(new ZDateTime(2022, 05, 09), holidaysList));

			AssertEquals(0, new ZDateTime(2024, 11, 30).GetCountOfWorkingDaysTo(new ZDateTime(2024, 12, 02), holidaysList));
			AssertEquals(0, new ZDateTime(2024, 12, 01).GetCountOfWorkingDaysTo(new ZDateTime(2024, 12, 02), holidaysList));
			AssertEquals(0, new ZDateTime(2024, 12, 02).GetCountOfWorkingDaysTo(new ZDateTime(2024, 12, 02), holidaysList));

			AssertEquals(4, new ZDateTime(2024, 11, 30).GetCountOfWorkingDaysTo(new ZDateTime(2024, 12, 06), holidaysList));
			AssertEquals(4, new ZDateTime(2024, 12, 01).GetCountOfWorkingDaysTo(new ZDateTime(2024, 12, 06), holidaysList));
			AssertEquals(4, new ZDateTime(2024, 12, 02).GetCountOfWorkingDaysTo(new ZDateTime(2024, 12, 06), holidaysList));
			AssertEquals(3, new ZDateTime(2024, 12, 03).GetCountOfWorkingDaysTo(new ZDateTime(2024, 12, 06), holidaysList));
			AssertEquals(2, new ZDateTime(2024, 12, 04).GetCountOfWorkingDaysTo(new ZDateTime(2024, 12, 06), holidaysList));
		}

		public void TestGetWorkingDaysTo_ConsideringHoliday()
		{
			CombineAssertions(() =>
			{
				var holidaysList = GetTestDeclaration().GetHardCodedCAHolidays(string.Empty);
				var dateTime = new ZDateTime(2014, 04, 17);
				AssertEquals("From 0417 to 0417", 0, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 17), holidaysList));
				AssertEquals("From 0417 to 0418", 0, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 18), holidaysList));
				AssertEquals("From 0417 to 0419", 0, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 19), holidaysList));
				AssertEquals("From 0417 to 0420", 0, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 20), holidaysList));
				AssertEquals("From 0417 to 0421", 0, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 21), holidaysList));
				AssertEquals("From 0417 to 0422", 1, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 22), holidaysList));
				AssertEquals("From 0417 to 0423", 2, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 23), holidaysList));
				AssertEquals("From 0417 to 0424", 3, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 24), holidaysList));
				AssertEquals("From 0417 to 0425", 4, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 25), holidaysList));

				dateTime = new ZDateTime(2014, 04, 25);
				AssertEquals("From 0425 to 0417", -4, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 17), holidaysList));
				AssertEquals("From 0425 to 0418", -3, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 18), holidaysList));
				AssertEquals("From 0425 to 0419", -3, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 19), holidaysList));
				AssertEquals("From 0425 to 0420", -3, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 20), holidaysList));
				AssertEquals("From 0425 to 0421", -3, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 21), holidaysList));
				AssertEquals("From 0425 to 0422", -3, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 22), holidaysList));
				AssertEquals("From 0425 to 0423", -2, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 23), holidaysList));
				AssertEquals("From 0425 to 0424", -1, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 24), holidaysList));
				AssertEquals("From 0425 to 0425", 0, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 25), holidaysList));

				dateTime = new ZDateTime(2014, 04, 18);
				AssertEquals("From 0418 to 0418", -2, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 18), holidaysList));
				AssertEquals("From 0418 to 0419", -2, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 19), holidaysList));
				AssertEquals("From 0418 to 0420", -2, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 20), holidaysList));
				AssertEquals("From 0418 to 0421", -2, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 21), holidaysList));
				AssertEquals("From 0418 to 0422", 0, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 22), holidaysList));
				AssertEquals("From 0418 to 0423", 1, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 23), holidaysList));
				AssertEquals("From 0418 to 0424", 2, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 24), holidaysList));
				AssertEquals("From 0418 to 0425", 3, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 25), holidaysList));
				AssertEquals("From 0418 to 0426", 3, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 26), holidaysList));
			});
		}

		public void TestGetCountOfWorkingDaysTo_StartDateIsWeekend()
		{
			CombineAssertions(() =>
			{
				var holidaysList = GetTestDeclaration().GetHardCodedCAHolidays(string.Empty);
				var dateTime = new ZDateTime(2014, 04, 05);
				AssertEquals("From 0405 to 0416", 7, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 16), holidaysList));
				AssertEquals("From 0405 to 0417", 8, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 17), holidaysList));
				AssertEquals("From 0405 to 0418", 8, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 18), holidaysList));
				AssertEquals("From 0405 to 0419", 8, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 19), holidaysList));
				AssertEquals("From 0405 to 0420", 8, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 20), holidaysList));
				AssertEquals("From 0405 to 0421", 8, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 21), holidaysList));
				AssertEquals("From 0405 to 0422", 9, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 22), holidaysList));

				dateTime = new ZDateTime(2014, 04, 06);
				AssertEquals("From 0406 to 0416", 7, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 16), holidaysList));
				AssertEquals("From 0406 to 0417", 8, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 17), holidaysList));
				AssertEquals("From 0406 to 0418", 8, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 18), holidaysList));
				AssertEquals("From 0406 to 0419", 8, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 19), holidaysList));
				AssertEquals("From 0406 to 0420", 8, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 20), holidaysList));
				AssertEquals("From 0406 to 0421", 8, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 21), holidaysList));
				AssertEquals("From 0406 to 0422", 9, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 22), holidaysList));

				dateTime = new ZDateTime(2014, 04, 25);
				AssertEquals("From 0425 to 0405", -12, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 05), holidaysList));
				AssertEquals("From 0425 to 0406", -12, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 06), holidaysList));
				AssertEquals("From 0425 to 0412", -7, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 12), holidaysList));
				AssertEquals("From 0425 to 0413", -7, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 13), holidaysList));
				AssertEquals("From 0425 to 0419", -3, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 19), holidaysList));
				AssertEquals("From 0425 to 0420", -3, dateTime.GetCountOfWorkingDaysTo(new ZDateTime(2014, 04, 20), holidaysList));
			});
		}

		public void TestIsWorkingDay()
		{
			var startDate = new ZDateTime(2014, 08, 21);
			var length = 365;
			CombineAssertions("Checking no Province Specified",
				() =>
				{
					var holidaysList = GetTestDeclaration().GetHardCodedCAHolidays(string.Empty);
					for (int i = 0; i < length; i++)
					{
						var testDate = startDate.AddDays(i);
						var expectedResult = true;
						expectedResult = !(
							testDate.DayOfWeek == DayOfWeek.Saturday ||
							testDate.DayOfWeek == DayOfWeek.Sunday ||
							testDate.Equals(new ZDateTime(2014, 09, 01)) ||
							testDate.Equals(new ZDateTime(2014, 10, 13)) ||
							testDate.Equals(new ZDateTime(2014, 11, 11)) ||
							testDate.Equals(new ZDateTime(2014, 12, 25)) ||
							testDate.Equals(new ZDateTime(2014, 12, 26)) ||
							testDate.Equals(new ZDateTime(2015, 01, 01)) ||
							testDate.Equals(new ZDateTime(2015, 04, 03)) ||
							testDate.Equals(new ZDateTime(2015, 04, 06)) ||
							testDate.Equals(new ZDateTime(2015, 05, 18)) ||
							testDate.Equals(new ZDateTime(2015, 07, 01)));
						AssertEquals("Testing " + testDate.ToString(), expectedResult, testDate.IsWorkingDay(holidaysList));
					}
				});

			CombineAssertions("Checking for Albota",
				() =>
				{
					var holidaysList = GetTestDeclaration().GetHardCodedCAHolidays("AB");
					for (int i = 0; i < length; i++)
					{
						var testDate = startDate.AddDays(i);
						var expectedResult = true;
						expectedResult = !(
							testDate.DayOfWeek == DayOfWeek.Saturday ||
							testDate.DayOfWeek == DayOfWeek.Sunday ||
							testDate.Equals(new ZDateTime(2014, 09, 01)) ||
							testDate.Equals(new ZDateTime(2014, 10, 13)) ||
							testDate.Equals(new ZDateTime(2014, 11, 11)) ||
							testDate.Equals(new ZDateTime(2014, 12, 25)) ||
							testDate.Equals(new ZDateTime(2014, 12, 26)) ||
							testDate.Equals(new ZDateTime(2015, 01, 01)) ||
							testDate.Equals(new ZDateTime(2015, 02, 16)) ||
							testDate.Equals(new ZDateTime(2015, 04, 03)) ||
							testDate.Equals(new ZDateTime(2015, 04, 06)) ||
							testDate.Equals(new ZDateTime(2015, 05, 18)) ||
							testDate.Equals(new ZDateTime(2015, 07, 01)) ||
							testDate.Equals(new ZDateTime(2015, 08, 03)));
						AssertEquals("Testing " + testDate.ToString(), expectedResult, testDate.IsWorkingDay(holidaysList));
					}
				});
		}
	}
}
