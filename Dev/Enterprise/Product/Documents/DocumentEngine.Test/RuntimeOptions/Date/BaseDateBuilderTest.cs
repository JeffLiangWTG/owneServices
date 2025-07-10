using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class BaseDateBuilderTest : TestCaseWithFactory
	{
		TestBaseDateBuilder builder;
		TestBaseDateBuilder Builder { get { return builder ?? (builder = new TestBaseDateBuilder(Factory)); } }

		[TestDate(2015, 1, 1)]
		public void TestToday()
		{
			var expectedTime = ZDateTime.Today;
			var replacementTime = Builder.GetMacroDateReplacement_Exposed("today");
			AssertEquals(expectedTime, replacementTime);
		}

		[TestDate(2015, 1, 1)]
		public void TestNow()
		{
			var expectedTime = ZDateTime.Now;
			var replacementTime = Builder.GetMacroDateReplacement_Exposed("now");
			AssertEquals(expectedTime, replacementTime);
		}

		[TestDate(2015, 1, 10)]
		public void TestTomorrow()
		{
			var expectedTime = new ZDateTime(2015, 1, 11);
			var replacementTime = Builder.GetMacroDateReplacement_Exposed("tomorrow");
			AssertEquals(expectedTime, replacementTime);
		}

		[TestDate(2015, 1, 10)]
		public void TestYesterday()
		{
			var expectedTime = new ZDateTime(2015, 1, 9);
			var replacementTime = Builder.GetMacroDateReplacement_Exposed("yesterday");
			AssertEquals(expectedTime, replacementTime);
		}

		[TestDate(2015, 1, 10)]
		public void TestLastWeek()
		{
			var expectedTime = new ZDateTime(2015, 1, 3);
			var replacementTime = Builder.GetMacroDateReplacement_Exposed("lastweek");
			AssertEquals(expectedTime, replacementTime);
		}

		[TestDate(2015, 1, 10)]
		public void TestNextWeek()
		{
			var expectedTime = new ZDateTime(2015, 1, 17);
			var replacementTime = Builder.GetMacroDateReplacement_Exposed("nextweek");
			AssertEquals(expectedTime, replacementTime);
		}

		[TestDate(2015, 1, 10)]
		public void TestLastMonth()
		{
			var expectedTime = new ZDateTime(2014, 12, 10);
			var replacementTime = Builder.GetMacroDateReplacement_Exposed("lastmonth");
			AssertEquals(expectedTime, replacementTime);
		}

		[TestDate(2015, 1, 10)]
		public void TestNextMonth()
		{
			var expectedTime = new ZDateTime(2015, 2, 10);
			var replacementTime = Builder.GetMacroDateReplacement_Exposed("nextmonth");
			AssertEquals(expectedTime, replacementTime);
		}

		[TestDate(2015, 1, 10)]
		public void TestLastYear()
		{
			var expectedTime = new ZDateTime(2014, 1, 10);
			var replacementTime = Builder.GetMacroDateReplacement_Exposed("lastyear");
			AssertEquals(expectedTime, replacementTime);
		}

		[TestDate(2015, 1, 10)]
		public void TestNextYear()
		{
			var expectedTime = new ZDateTime(2016, 1, 10);
			var replacementTime = Builder.GetMacroDateReplacement_Exposed("nextyear");
			AssertEquals(expectedTime, replacementTime);
		}

		[TestDate(2018, 1, 10)]
		public void TestFirstDayOfLastCalendarYear()
		{
			var expectedTime = new ZDateTime(2017, 1, 1);
			var replacementTime = Builder.GetMacroDateReplacement_Exposed("FirstDayOfLastCalendarYear");
			AssertEquals(expectedTime, replacementTime);
		}

		[TestDate(2018, 1, 10)]
		public void TestLastDayOfLastCalendarYear()
		{
			var expectedTime = new ZDateTime(2017, 12, 31);
			var replacementTime = Builder.GetMacroDateReplacement_Exposed("LastDayOfLastCalendarYear");
			AssertEquals(expectedTime, replacementTime);
		}

		[TestDate(2015, 1, 10)]
		public void TestMacroReplacement()
		{
			var expectedTime = new ZDateTime(2016, 1, 10);
			var replacementTime = Builder.GetMacroDateReplacement_Exposed("<nextyear>");
			AssertEquals(expectedTime, replacementTime);
		}

		[TestDate(2015, 1, 10)]
		public void TestForgivingReplacement()
		{
			var expectedTime = new ZDateTime(2016, 1, 10);
			var replacementTime = Builder.GetMacroDateReplacement_Exposed("  <     nEXtYeAr >     ");
			AssertEquals(expectedTime, replacementTime);
		}

		public void TestDateReplacement()
		{
			var expectedTime = new ZDateTime(2013, 7, 22);
			var replacementTime = Builder.GetMacroDateReplacement_Exposed("<22/7/2013>");
			AssertEquals(expectedTime, replacementTime);
		}

		public void TestGetDateDefaultedOptionNameString()
		{
			AssertEquals("now, today, lastyear, nextyear, lastmonth, nextmonth, lastthreemonths, nextthreemonths, lastweek, nextweek, yesterday, tomorrow, firstdayoflastcalendaryear, lastdayoflastcalendaryear", Builder.GetDateDefaultedOptionsNameStringForTest());
		}
	}
}
