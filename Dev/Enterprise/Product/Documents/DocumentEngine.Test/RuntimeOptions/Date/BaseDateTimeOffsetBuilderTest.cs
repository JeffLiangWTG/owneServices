using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class BaseDateTimeOffsetBuilderTest : TestCaseWithFactory
	{
		[TestDate(2024, 3, 5)]
		public void TestToday()
		{
			var expectedTime = ZDateTimeOffset.Today;
			var replacementTime = Builder.GetMacroDateReplacement_Exposed("today");
			AssertEquals(expectedTime, replacementTime);
		}

		[TestDate(2024, 3, 5)]
		public void TestNow()
		{
			var expectedTime = ZDateTimeOffset.Now;
			var replacementTime = Builder.GetMacroDateReplacement_Exposed("now");
			AssertEquals(expectedTime, replacementTime);
		}

		[TestDate(2024, 3, 5)]
		public void TestTomorrow()
		{
			var expectedTime = new ZDateTimeOffset(2024, 3, 6);
			var replacementTime = Builder.GetMacroDateReplacement_Exposed("tomorrow");
			AssertEquals(expectedTime, replacementTime);
		}

		[TestDate(2024, 3, 5)]
		public void TestYesterday()
		{
			var expectedTime = new ZDateTimeOffset(2024, 3, 4);
			var replacementTime = Builder.GetMacroDateReplacement_Exposed("yesterday");
			AssertEquals(expectedTime, replacementTime);
		}

		[TestDate(2024, 3, 5)]
		public void TestLastWeek()
		{
			var expectedTime = new ZDateTimeOffset(2024, 2, 27);
			var replacementTime = Builder.GetMacroDateReplacement_Exposed("lastweek");
			AssertEquals(expectedTime, replacementTime);
		}

		[TestDate(2024, 3, 5)]
		public void TestNextWeek()
		{
			var expectedTime = new ZDateTimeOffset(2024, 3, 12);
			var replacementTime = Builder.GetMacroDateReplacement_Exposed("nextweek");
			AssertEquals(expectedTime, replacementTime);
		}

		[TestDate(2024, 3, 5)]
		public void TestLastMonth()
		{
			var expectedTime = new ZDateTimeOffset(2024, 2, 5);
			var replacementTime = Builder.GetMacroDateReplacement_Exposed("lastmonth");
			AssertEquals(expectedTime, replacementTime);
		}

		[TestDate(2024, 3, 5)]
		public void TestNextMonth()
		{
			var expectedTime = new ZDateTimeOffset(2024, 4, 5);
			var replacementTime = Builder.GetMacroDateReplacement_Exposed("nextmonth");
			AssertEquals(expectedTime, replacementTime);
		}

		[TestDate(2024, 3, 5)]
		public void TestLastYear()
		{
			var expectedTime = new ZDateTimeOffset(2023, 3, 5);
			var replacementTime = Builder.GetMacroDateReplacement_Exposed("lastyear");
			AssertEquals(expectedTime, replacementTime);
		}

		[TestDate(2024, 3, 5)]
		public void TestNextYear()
		{
			var expectedTime = new ZDateTimeOffset(2025, 3, 5);
			var replacementTime = Builder.GetMacroDateReplacement_Exposed("nextyear");
			AssertEquals(expectedTime, replacementTime);
		}

		[TestDate(2024, 3, 5)]
		public void TestFirstDayOfLastCalendarYear()
		{
			var expectedTime = new ZDateTimeOffset(2023, 1, 1);
			var replacementTime = Builder.GetMacroDateReplacement_Exposed("FirstDayOfLastCalendarYear");
			AssertEquals(expectedTime, replacementTime);
		}

		[TestDate(2024, 3, 5)]
		public void TestLastDayOfLastCalendarYear()
		{
			var expectedTime = new ZDateTimeOffset(2023, 12, 31);
			var replacementTime = Builder.GetMacroDateReplacement_Exposed("LastDayOfLastCalendarYear");
			AssertEquals(expectedTime, replacementTime);
		}

		[TestDate(2024, 3, 5)]
		public void TestMacroReplacement()
		{
			var expectedTime = new ZDateTimeOffset(2025, 3, 5);
			var replacementTime = Builder.GetMacroDateReplacement_Exposed("<nextyear>");
			AssertEquals(expectedTime, replacementTime);
		}

		[TestDate(2024, 3, 5)]
		public void TestForgivingReplacement()
		{
			var expectedTime = new ZDateTimeOffset(2025, 3, 5);
			var replacementTime = Builder.GetMacroDateReplacement_Exposed("  <     nEXtYeAr >     ");
			AssertEquals(expectedTime, replacementTime);
		}

		public void TestDateReplacement()
		{
			var expectedTime = new ZDateTimeOffset(2013, 7, 22);
			var replacementTime = Builder.GetMacroDateReplacement_Exposed("<22/7/2013>");
			AssertEquals(expectedTime, replacementTime);
		}

		public void TestGetDateDefaultedOptionNameString()
		{
			AssertEquals("now, today, lastyear, nextyear, lastmonth, nextmonth, lastthreemonths, nextthreemonths, lastweek, nextweek, yesterday, tomorrow, firstdayoflastcalendaryear, lastdayoflastcalendaryear", Builder.GetDateDefaultedOptionsNameStringForTest());
		}
		TestBaseDateTimeOffsetBuilder builder;
		TestBaseDateTimeOffsetBuilder Builder { get { return builder ?? (builder = new TestBaseDateTimeOffsetBuilder(Factory)); } }
		sealed class TestBaseDateTimeOffsetBuilder : BaseDateBuilder<ZDateTimeOffset>
		{
			public TestBaseDateTimeOffsetBuilder(BusinessObjectFactory factory)
				: base(new ValidatorPack(), factory, (x) => "", ReportRunningType.Report)
			{
			}

			protected override IReportDocumenter GetDateFilterDocumentation(List<string> supportedProperties)
			{
				throw new NotImplementedException();
			}

			public ZDateTimeOffset GetMacroDateReplacement_Exposed(string value)
			{
				return GetMacroDateReplacement(value);
			}

			public override bool CanBuild(string filterType)
			{
				throw new NotImplementedException();
			}

			protected override FilterField GetFilterField()
			{
				throw new NotImplementedException();
			}

			public string GetDateDefaultedOptionsNameStringForTest()
			{
				return base.GetDateDefaultedOptionsNameString();
			}
		}
	}
}
