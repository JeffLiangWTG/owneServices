using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.PeriodManagement
{
	public class PeriodCalculatorTest : TestCase
	{
		public void TestGetDayNo()
		{
			TestCalculator = new TestPeriodCalculator("CAL", DateTime.Now, "FRI");
			AssertEquals(0, TestCalculator.TestGetDayNo("SUN"));
			AssertEquals(1, TestCalculator.TestGetDayNo("MON"));
			AssertEquals(2, TestCalculator.TestGetDayNo("TUE"));
			AssertEquals(3, TestCalculator.TestGetDayNo("WED"));
			AssertEquals(4, TestCalculator.TestGetDayNo("THU"));
			AssertEquals(5, TestCalculator.TestGetDayNo("FRI"));
			AssertEquals(6, TestCalculator.TestGetDayNo("SAT"));
		}

		public void TestGetFirstPeriodLength()
		{
			AssertEquals(28, new TestPeriodCalculator("445", DateTime.Now, "FRI").TestFirstPeriodLength);
			AssertEquals(28, new TestPeriodCalculator("4WK", DateTime.Now, "FRI").TestFirstPeriodLength);
			AssertEquals(-1, new TestPeriodCalculator("CAL", DateTime.Now, "FRI").TestFirstPeriodLength);
			AssertEquals(7, new TestPeriodCalculator("1WK", DateTime.Now, "FRI").TestFirstPeriodLength);
		}

		public void TestFirstPeriodEndDate()
		{
			AssertEquals(new DateTime(2003, 5, 9), new PeriodCalculator("1WK", new DateTime(2003, 5, 5), "FRI").FirstPeriodEndDate().Date);
			AssertEquals(new DateTime(2003, 5, 12), new PeriodCalculator("1WK", new DateTime(2003, 5, 5), "MON").FirstPeriodEndDate().Date);
			AssertEquals(new DateTime(2003, 5, 13), new PeriodCalculator("1WK", new DateTime(2003, 5, 5), "TUE").FirstPeriodEndDate().Date);
			AssertEquals(new DateTime(2003, 5, 11), new PeriodCalculator("1WK", new DateTime(2003, 5, 5), "SUN").FirstPeriodEndDate().Date);
			AssertEquals(new DateTime(2003, 6, 1), new PeriodCalculator("4WK", new DateTime(2003, 5, 5), "SUN").FirstPeriodEndDate().Date);
			AssertEquals(new DateTime(2003, 5, 30), new PeriodCalculator("4WK", new DateTime(2003, 5, 5), "FRI").FirstPeriodEndDate().Date);
			AssertEquals(new DateTime(2003, 6, 1), new PeriodCalculator("445", new DateTime(2003, 5, 5), "SUN").FirstPeriodEndDate().Date);
			AssertEquals(new DateTime(2003, 5, 30), new PeriodCalculator("445", new DateTime(2003, 5, 5), "FRI").FirstPeriodEndDate().Date);
			AssertEquals(new DateTime(2003, 7, 25), new PeriodCalculator("445", new DateTime(2003, 7, 1), "FRI").FirstPeriodEndDate().Date);
		}

		public void TestConstructors()
		{
			PeriodCalculator calculatorWithNoYearOverride = new PeriodCalculator("CAL", new DateTime(2006, 1, 6), "FRI");
			AssertEquals("Without overriden date, Period will be based on end date of Year", 200702, calculatorWithNoYearOverride.GetPeriodFromDate(new DateTime(2006, 2, 7)));
			PeriodCalculator calculatorWithYearOverride = new PeriodCalculator("CAL", new DateTime(2006, 1, 6), "FRI", 2006);
			AssertEquals("With overriden date, Period will be based on override", 200602, calculatorWithYearOverride.GetPeriodFromDate(new DateTime(2006, 2, 7)));
		}

		public void TestGetPeriodFromDateMonthly()
		{
			int result;
			DateTime yearStartDate = new DateTime(2003, 7, 1);
			TestCalculator = new TestPeriodCalculator("CAL", yearStartDate, "FRI");
			//-1 because we don't really use it. In real life they must be 0..6 (SUN..SAT) 
			result = TestCalculator.GetPeriodFromDate(new DateTime(2003, 7, 8));
			AssertEquals(200401, result);
			result = TestCalculator.GetPeriodFromDate(new DateTime(2004, 1, 8));
			AssertEquals(200407, result);
			result = TestCalculator.GetPeriodFromDate(new DateTime(2004, 6, 30));
			AssertEquals(200412, result);
			yearStartDate = new DateTime(2003, 7, 20);
			result = TestCalculator.GetPeriodFromDate(new DateTime(2004, 1, 8));
			AssertEquals(200407, result);
		}

		public void TestGetPeriodFromDateWeekly()
		{
			int result;
			DateTime yearStartDate = new DateTime(2003, 7, 1);
			TestCalculator = new TestPeriodCalculator("1WK", yearStartDate, "FRI");
			result = TestCalculator.GetPeriodFromDate(new DateTime(2003, 7, 8));
			AssertEquals(200402, result);
			result = TestCalculator.GetPeriodFromDate(new DateTime(2003, 7, 11));
			AssertEquals(200402, result);
			result = TestCalculator.GetPeriodFromDate(new DateTime(2004, 6, 30));
			AssertEquals(200452, result);
			result = TestCalculator.GetPeriodFromDate(new DateTime(2003, 7, 2));
			AssertEquals(200401, result);
			TestCalculator = new TestPeriodCalculator("1WK", yearStartDate, "WED");
			result = TestCalculator.GetPeriodFromDate(new DateTime(2003, 7, 16));
			AssertEquals(200402, result);
			result = TestCalculator.GetPeriodFromDate(new DateTime(2003, 7, 31));
			AssertEquals(200405, result);
		}

		public void TestGetPeriodFromDateFourWeekly()
		{
			int result;
			DateTime yearStartDate = new DateTime(2003, 7, 1);
			TestCalculator = new TestPeriodCalculator("4WK", yearStartDate, "FRI");
			result = TestCalculator.GetPeriodFromDate(new DateTime(2003, 7, 8));
			AssertEquals(200401, result);
			result = TestCalculator.GetPeriodFromDate(new DateTime(2003, 7, 25));
			AssertEquals(200401, result);
			result = TestCalculator.GetPeriodFromDate(new DateTime(2003, 7, 26));
			AssertEquals(200402, result);
			result = TestCalculator.GetPeriodFromDate(new DateTime(2004, 6, 30));
			AssertEquals(200413, result);
			result = TestCalculator.GetPeriodFromDate(new DateTime(2004, 5, 28));
			AssertEquals(200412, result);
		}

		public void TestGetPeriodFromDate445()
		{
			int result;
			DateTime yearStartDate = new DateTime(2003, 7, 1);
			TestCalculator = new TestPeriodCalculator("445", yearStartDate, "FRI");
			result = TestCalculator.GetPeriodFromDate(new DateTime(2003, 7, 8));
			AssertEquals(200401, result);
			result = TestCalculator.GetPeriodFromDate(new DateTime(2003, 7, 25));
			AssertEquals(200401, result);
			result = TestCalculator.GetPeriodFromDate(new DateTime(2003, 7, 26));
			AssertEquals(200402, result);
			result = TestCalculator.GetPeriodFromDate(new DateTime(2004, 6, 30));
			AssertEquals(200412, result);
		}

		public void TestGetStartDayOfPeriod()
		{
			DateTime yearStart = new DateTime(2003, 7, 1);
			DateTime result;
			//monthly
			TestCalculator = new TestPeriodCalculator("CAL", yearStart, "FRI");
			result = TestCalculator.GetStartDayOfPeriod(200401);
			AssertEquals(yearStart, result);
			result = TestCalculator.GetStartDayOfPeriod(200402);
			AssertEquals(new DateTime(2003, 8, 1), result);
			result = TestCalculator.GetStartDayOfPeriod(200412);
			AssertEquals(new DateTime(2004, 6, 1), result);
			//weekly
			TestCalculator = new TestPeriodCalculator("1WK", yearStart, "FRI");
			result = TestCalculator.GetStartDayOfPeriod(200401);
			AssertEquals(yearStart, result);
			result = TestCalculator.GetStartDayOfPeriod(200402);
			AssertEquals(new DateTime(2003, 7, 5), result);
			result = TestCalculator.GetStartDayOfPeriod(200452);
			AssertEquals(new DateTime(2004, 6, 19), result);
			//four weekly
			TestCalculator = new TestPeriodCalculator("4WK", yearStart, "FRI");
			result = TestCalculator.GetStartDayOfPeriod(200401);
			AssertEquals(yearStart, result);
			result = TestCalculator.GetStartDayOfPeriod(200402);
			AssertEquals(new DateTime(2003, 7, 26), result);
			result = TestCalculator.GetStartDayOfPeriod(200413);
			AssertEquals(new DateTime(2004, 5, 29), result);
			//4-4-5
			TestCalculator = new TestPeriodCalculator("445", yearStart, "FRI");
			result = TestCalculator.GetStartDayOfPeriod(200401);
			AssertEquals(yearStart, result);
			result = TestCalculator.GetStartDayOfPeriod(200402);
			AssertEquals(new DateTime(2003, 7, 26), result);
			result = TestCalculator.GetStartDayOfPeriod(200404);
			AssertEquals(new DateTime(2003, 9, 27), result);
			TestCalculator = new TestPeriodCalculator("445", new DateTime(2003, 1, 1), "FRI");
			result = TestCalculator.GetStartDayOfPeriod(200312);
			AssertEquals(new DateTime(2003, 11, 29), result);
		}

		public void TestGetEndDayOfPeriod()
		{
			DateTime yearStart = new DateTime(2003, 7, 1);
			DateTime result;
			//monthly
			TestCalculator = new TestPeriodCalculator("CAL", yearStart, "FRI");
			result = TestCalculator.GetEndDayOfPeriod(200401);
			AssertEquals(new DateTime(2003, 7, 31), result.Date);
			result = TestCalculator.GetEndDayOfPeriod(200402);
			AssertEquals(new DateTime(2003, 8, 31), result.Date);
			result = TestCalculator.GetEndDayOfPeriod(200412);
			AssertEquals(new DateTime(2004, 6, 30), result.Date);
			TestCalculator = new TestPeriodCalculator("CAL", new DateTime(2003, 7, 2), "FRI");
			result = TestCalculator.GetEndDayOfPeriod(200401);
			AssertEquals(new DateTime(2003, 8, 1), result.Date);
			//weekly
			TestCalculator = new TestPeriodCalculator("1WK", yearStart, "FRI");
			result = TestCalculator.GetEndDayOfPeriod(200401);
			AssertEquals(new DateTime(2003, 7, 4), result.Date);
			result = TestCalculator.GetEndDayOfPeriod(200402);
			AssertEquals(new DateTime(2003, 7, 11), result.Date);
			result = TestCalculator.GetEndDayOfPeriod(200452);
			AssertEquals(new DateTime(2004, 6, 30), result.Date);
			//four weekly
			TestCalculator = new TestPeriodCalculator("4WK", yearStart, "FRI");
			result = TestCalculator.GetEndDayOfPeriod(200401);
			AssertEquals(new DateTime(2003, 7, 25), result.Date);
			result = TestCalculator.GetEndDayOfPeriod(200402);
			AssertEquals(new DateTime(2003, 8, 22), result.Date);
			result = TestCalculator.GetEndDayOfPeriod(200413);
			AssertEquals(new DateTime(2004, 6, 30), result.Date);
			//4-4-5
			TestCalculator = new TestPeriodCalculator("445", yearStart, "FRI");
			result = TestCalculator.GetEndDayOfPeriod(200401);
			AssertEquals(new DateTime(2003, 7, 25), result.Date);
			result = TestCalculator.GetEndDayOfPeriod(200402);
			AssertEquals(new DateTime(2003, 8, 22), result.Date);
			result = TestCalculator.GetEndDayOfPeriod(200404);
			AssertEquals(new DateTime(2003, 10, 24), result.Date);
			result = TestCalculator.GetEndDayOfPeriod(200412);
			AssertEquals(new DateTime(2004, 6, 30), result.Date);
		}

		public void TestGetEndDayOfPeriodForExtendedLastYear()
		{
			var yearStart = new DateTime(2003, 7, 1);
			ZDateTime result;
			//monthly
			TestCalculator = new TestPeriodCalculator("CAL", yearStart, "FRI");
			result = TestCalculator.GetEndDayOfPeriodForExtend(200402, new ZDateTime(2003, 8, 1));
			AssertEquals(new DateTime(2003, 8, 31), result.Date);
			result = TestCalculator.GetEndDayOfPeriodForExtend(200402, new ZDateTime(2003, 9, 3));
			AssertEquals(new DateTime(2003, 9, 30), result.Date);
			result = TestCalculator.GetEndDayOfPeriodForExtend(200412, new ZDateTime(2004, 6, 1));
			AssertEquals(new DateTime(2004, 6, 30), result.Date);
			//weekly
			TestCalculator = new TestPeriodCalculator("1WK", yearStart, "FRI");
			result = TestCalculator.GetEndDayOfPeriodForExtend(200402, new ZDateTime(2003, 7, 5));
			AssertEquals(new DateTime(2003, 7, 11), result.Date);
			result = TestCalculator.GetEndDayOfPeriodForExtend(200452, new ZDateTime(2004, 6, 19));
			AssertEquals(new DateTime(2004, 6, 25), result.Date);
			//four weekly
			TestCalculator = new TestPeriodCalculator("4WK", yearStart, "FRI");
			result = TestCalculator.GetEndDayOfPeriodForExtend(200402, new ZDateTime(2003, 7, 26));
			AssertEquals(new DateTime(2003, 8, 22), result.Date);
			result = TestCalculator.GetEndDayOfPeriodForExtend(200413, new ZDateTime(2004, 5, 29));
			AssertEquals(new DateTime(2004, 6, 25), result.Date);
			//4-4-5
			TestCalculator = new TestPeriodCalculator("445", yearStart, "FRI");
			result = TestCalculator.GetEndDayOfPeriodForExtend(200402, new ZDateTime(2003, 7, 26));
			AssertEquals(new DateTime(2003, 8, 22), result.Date);
			result = TestCalculator.GetEndDayOfPeriodForExtend(200404, new ZDateTime(2003, 9, 27));
			AssertEquals(new DateTime(2003, 10, 24), result.Date);
			result = TestCalculator.GetEndDayOfPeriodForExtend(200412, new ZDateTime(2004, 5, 22));
			AssertEquals(new DateTime(2004, 6, 25), result.Date);
		}

		public void TestGetNextPeriod()
		{
			AssertEquals(200302, new PeriodCalculator("1WK", DateTime.Now, "FRI").GetNextPeriod(200301));
			AssertEquals(200401, new PeriodCalculator("CAL", DateTime.Now, "FRI").GetNextPeriod(200312));
		}

		public void TestGetNextPeriodForExtendedLastYear()
		{
			AssertEquals(200353, new PeriodCalculator("1WK", DateTime.Now, "FRI").GetNextPeriod(200352, true));
		}

		public void TestGetPreviousPeriod()
		{
			int periodToTest = 200301;
			AssertEquals("Previous Period", 200212, new PeriodCalculator("CAL", DateTime.Now, "FRI").GetPreviousPeriod(periodToTest));
			AssertEquals("Previous Period", 200252, new PeriodCalculator("1WK", DateTime.Now, "FRI").GetPreviousPeriod(periodToTest));
			AssertEquals("Previous Period", 200203, new PeriodCalculator("4WK", DateTime.Now, "FRI").GetPreviousPeriod(200204));
		}

		/// <summary>
		/// Test class to expose protected functions
		/// </summary>
		protected class TestPeriodCalculator : PeriodCalculator
		{
			public TestPeriodCalculator(string periodFormat, DateTime startDate, string periodEndWeekDay) : base(periodFormat, startDate, periodEndWeekDay)
			{
			}

			public int TestGetDayNo(string dayString)
			{
				return GetDayNo(dayString);
			}

			public int TestFirstPeriodLength
			{
				get
				{
					return FirstPeriodLength;
				}
			}
		}

		protected TestPeriodCalculator TestCalculator;
	}
}
