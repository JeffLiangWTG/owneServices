using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class DstRuleParserTest : TestCase
	{
		public void TestConstruction()
		{
			AssertParserThrowsExceptionWhenInvalidConstructionParametersAreProvided(0, "SUN", "JAN",
				"Unable to determine day of the month with the provided arguments: Year = 2000, Month = 1, Week = 0, Weekday = [SUN]");

			AssertParserThrowsExceptionWhenInvalidConstructionParametersAreProvided(1, "XXX", "JAN",
				"Invalid week-day code [XXX]");

			AssertParserThrowsExceptionWhenInvalidConstructionParametersAreProvided(1, "SUN", "YYY",
				"Unable to determine day of the month with the provided arguments: Year = 2000, Month = -1, Week = 1, Weekday = [SUN]");

			AssertParserThrowsExceptionWhenInvalidConstructionParametersAreProvided(1, "", "JAN",
				"Invalid week-day code []");

			AssertParserThrowsExceptionWhenInvalidConstructionParametersAreProvided(1, "SUN", "",
				"Unable to determine day of the month with the provided arguments: Year = 2000, Month = -1, Week = 1, Weekday = [SUN]");

			AssertParserThrowsExceptionWhenInvalidConstructionParametersAreProvided(-1, "SUN", "JAN",
				"Unable to determine day of the month with the provided arguments: Year = 2000, Month = 1, Week = -1, Weekday = [SUN]");

			AssertParserThrowsExceptionWhenInvalidConstructionParametersAreProvided(6, "SUN", "JAN",
				"Unable to determine day of the month with the provided arguments: Year = 2000, Month = 1, Week = 6, Weekday = [SUN]");

			DstRuleParser testParser = new DstRuleParser(
				2000, TimeZoneConstants.DstTransitionTypeStart, TimeZoneConstants.DstRuleWeekday,
				TimeZoneConstants.DstTimeBaseLocal, new DateTime(2000, 1, 1), 1, "SUN", "JAN");
			AssertEquals("Transition DateTime", new DateTime(2000, 1, 2), testParser.TransitionDateTime);
		}

		public void TestTransitionDateTime()
		{
			DstRuleParser testParser = new DstRuleParser(
				2006, TimeZoneConstants.DstTransitionTypeStart, TimeZoneConstants.DstRuleWeekday,
				TimeZoneConstants.DstTimeBaseLocal, new DateTime(2000, 1, 1), 2, "SUN", "MAR");
			AssertEquals("Transition DateTime (2nd Sunday of March 2006)", new DateTime(2006, 03, 12), testParser.TransitionDateTime);

			testParser = new DstRuleParser(
				2006, TimeZoneConstants.DstTransitionTypeStart, TimeZoneConstants.DstRuleWeekday,
				TimeZoneConstants.DstTimeBaseLocal, new DateTime(2000, 1, 1), 4, "SUN", "MAR");
			AssertEquals("Transition DateTime (4th Sunday of March 2006)", new DateTime(2006, 03, 26), testParser.TransitionDateTime);

			testParser = new DstRuleParser(
				2006, TimeZoneConstants.DstTransitionTypeStart, TimeZoneConstants.DstRuleWeekday,
				TimeZoneConstants.DstTimeBaseLocal, new DateTime(2000, 1, 1), 5, "SUN", "MAR");
			AssertEquals("Transition DateTime (Last Sunday of March 2006)", new DateTime(2006, 03, 26), testParser.TransitionDateTime);

			testParser = new DstRuleParser(
				2006, TimeZoneConstants.DstTransitionTypeStart, TimeZoneConstants.DstRuleWeekday,
				TimeZoneConstants.DstTimeBaseLocal, new DateTime(2000, 1, 1), 5, "SUN", "APR");
			AssertEquals("Transition DateTime (Last Sunday of April 2006)", new DateTime(2006, 04, 30), testParser.TransitionDateTime);

			testParser = new DstRuleParser(
				2006, TimeZoneConstants.DstTransitionTypeStart, TimeZoneConstants.DstRuleDayOfMonth,
				TimeZoneConstants.DstTimeBaseLocal, new DateTime(2000, 5, 15), -1, "XXX", "YYY");
			AssertEquals("Transition DateTime (15-May-2006)", new DateTime(2006, 05, 15), testParser.TransitionDateTime);
		}

		public void TestGetTransitionDateTimeInCurrentLocalTime()
		{
			DstRuleParser testParser = new DstRuleParser(
				2006, TimeZoneConstants.DstTransitionTypeStart, TimeZoneConstants.DstRuleDayOfMonth,
				TimeZoneConstants.DstTimeBaseLocal, new DateTime(2000, 5, 15, 0, 0, 0), -1, "XXX", "YYY");
			DateTime actualValue = testParser.GetTransitionDateTimeInCurrentLocalTime(1.5m, 2.5m);
			AssertEquals("Transition Local DateTime (Type: START / Base: LOCAL)", new DateTime(2006, 05, 15, 0, 0, 0), actualValue);

			testParser = new DstRuleParser(
				2006, TimeZoneConstants.DstTransitionTypeStart, TimeZoneConstants.DstRuleDayOfMonth,
				TimeZoneConstants.DstTimeBaseStandard, new DateTime(2000, 5, 15, 0, 0, 0), -1, "XXX", "YYY");
			actualValue = testParser.GetTransitionDateTimeInCurrentLocalTime(1.5m, 2.5m);
			AssertEquals("Transition Local DateTime (Type: START / Base: STANDARD)", new DateTime(2006, 05, 15, 0, 0, 0), actualValue);

			testParser = new DstRuleParser(
				2006, TimeZoneConstants.DstTransitionTypeStart, TimeZoneConstants.DstRuleDayOfMonth,
				TimeZoneConstants.DstTimeBaseUtc, new DateTime(2000, 5, 15, 0, 0, 0), -1, "XXX", "YYY");
			actualValue = testParser.GetTransitionDateTimeInCurrentLocalTime(1.5m, 2.5m);
			AssertEquals("Transition Local DateTime (Type: START / Base: UTC)", new DateTime(2006, 05, 15, 1, 30, 0), actualValue);

			testParser = new DstRuleParser(
				2006, TimeZoneConstants.DstTransitionTypeEnd, TimeZoneConstants.DstRuleDayOfMonth,
				TimeZoneConstants.DstTimeBaseLocal, new DateTime(2000, 5, 15, 0, 0, 0), -1, "XXX", "YYY");
			actualValue = testParser.GetTransitionDateTimeInCurrentLocalTime(1.5m, 2.5m);
			AssertEquals("Transition Local DateTime (Type: END / Base: LOCAL)", new DateTime(2006, 05, 15, 0, 0, 0), actualValue);

			testParser = new DstRuleParser(
				2006, TimeZoneConstants.DstTransitionTypeEnd, TimeZoneConstants.DstRuleDayOfMonth,
				TimeZoneConstants.DstTimeBaseStandard, new DateTime(2000, 5, 15, 0, 0, 0), -1, "XXX", "YYY");
			actualValue = testParser.GetTransitionDateTimeInCurrentLocalTime(1.5m, 2.5m);
			AssertEquals("Transition Local DateTime (Type: END / Base: STANDARD)", new DateTime(2006, 05, 15, 1, 0, 0), actualValue);

			testParser = new DstRuleParser(
				2006, TimeZoneConstants.DstTransitionTypeEnd, TimeZoneConstants.DstRuleDayOfMonth,
				TimeZoneConstants.DstTimeBaseUtc, new DateTime(2000, 5, 15, 0, 0, 0), -1, "XXX", "YYY");
			actualValue = testParser.GetTransitionDateTimeInCurrentLocalTime(1.5m, 2.5m);
			AssertEquals("Transition Local DateTime (Type: END / Base: UTC)", new DateTime(2006, 05, 15, 2, 30, 0), actualValue);
		}

		public void TestGetTransitionDateTimeInUtc()
		{
			DstRuleParser testParser = new DstRuleParser(
				2006, TimeZoneConstants.DstTransitionTypeStart, TimeZoneConstants.DstRuleDayOfMonth,
				TimeZoneConstants.DstTimeBaseLocal, new DateTime(2000, 5, 15, 0, 0, 0), -1, "XXX", "YYY");
			DateTime actualValue = testParser.GetTransitionDateTimeInCurrentLocalTime(1.5m, 2.5m);
			AssertEquals("Transition UTC (Type: START / Base: LOCAL)", new DateTime(2006, 05, 15, 0, 0, 0), actualValue);

			testParser = new DstRuleParser(
				2006, TimeZoneConstants.DstTransitionTypeStart, TimeZoneConstants.DstRuleDayOfMonth,
				TimeZoneConstants.DstTimeBaseStandard, new DateTime(2000, 5, 15, 0, 0, 0), -1, "XXX", "YYY");
			actualValue = testParser.GetTransitionDateTimeInCurrentLocalTime(1.5m, 2.5m);
			AssertEquals("Transition UTC (Type: START / Base: STANDARD)", new DateTime(2006, 05, 15, 0, 0, 0), actualValue);

			testParser = new DstRuleParser(
				2006, TimeZoneConstants.DstTransitionTypeStart, TimeZoneConstants.DstRuleDayOfMonth,
				TimeZoneConstants.DstTimeBaseUtc, new DateTime(2000, 5, 15, 0, 0, 0), -1, "XXX", "YYY");
			actualValue = testParser.GetTransitionDateTimeInCurrentLocalTime(1.5m, 2.5m);
			AssertEquals("Transition UTC (Type: START / Base: UTC)", new DateTime(2006, 05, 15, 1, 30, 0), actualValue);

			testParser = new DstRuleParser(
				2006, TimeZoneConstants.DstTransitionTypeEnd, TimeZoneConstants.DstRuleDayOfMonth,
				TimeZoneConstants.DstTimeBaseLocal, new DateTime(2000, 5, 15, 0, 0, 0), -1, "XXX", "YYY");
			actualValue = testParser.GetTransitionDateTimeInCurrentLocalTime(1.5m, 2.5m);
			AssertEquals("Transition UTC (Type: END / Base: LOCAL)", new DateTime(2006, 05, 15, 0, 0, 0), actualValue);

			testParser = new DstRuleParser(
				2006, TimeZoneConstants.DstTransitionTypeEnd, TimeZoneConstants.DstRuleDayOfMonth,
				TimeZoneConstants.DstTimeBaseStandard, new DateTime(2000, 5, 15, 0, 0, 0), -1, "XXX", "YYY");
			actualValue = testParser.GetTransitionDateTimeInCurrentLocalTime(1.5m, 2.5m);
			AssertEquals("Transition UTC (Type: END / Base: STANDARD)", new DateTime(2006, 05, 15, 1, 0, 0), actualValue);

			testParser = new DstRuleParser(
				2006, TimeZoneConstants.DstTransitionTypeEnd, TimeZoneConstants.DstRuleDayOfMonth,
				TimeZoneConstants.DstTimeBaseUtc, new DateTime(2000, 5, 15, 0, 0, 0), -1, "XXX", "YYY");
			actualValue = testParser.GetTransitionDateTimeInCurrentLocalTime(1.5m, 2.5m);
			AssertEquals("Transition UTC (Type: END / Base: UTC)", new DateTime(2006, 05, 15, 2, 30, 0), actualValue);
		}

		public void TestGetDayOfWeekFromCode()
		{
			DstRuleParserForTesting testParser = new DstRuleParserForTesting();

			DayOfWeek weekDay = testParser.GetDayOfWeekFromCode_Exposed("SUN");
			AssertEquals("Sunday", DayOfWeek.Sunday, weekDay);

			weekDay = testParser.GetDayOfWeekFromCode_Exposed("MON");
			AssertEquals("Monday", DayOfWeek.Monday, weekDay);

			weekDay = testParser.GetDayOfWeekFromCode_Exposed("TUE");
			AssertEquals("Tuesday", DayOfWeek.Tuesday, weekDay);

			weekDay = testParser.GetDayOfWeekFromCode_Exposed("WED");
			AssertEquals("Wednesday", DayOfWeek.Wednesday, weekDay);

			weekDay = testParser.GetDayOfWeekFromCode_Exposed("THU");
			AssertEquals("Thursday", DayOfWeek.Thursday, weekDay);

			weekDay = testParser.GetDayOfWeekFromCode_Exposed("FRI");
			AssertEquals("Friday", DayOfWeek.Friday, weekDay);

			weekDay = testParser.GetDayOfWeekFromCode_Exposed("SAT");
			AssertEquals("Saturday", DayOfWeek.Saturday, weekDay);

			try
			{
				weekDay = testParser.GetDayOfWeekFromCode_Exposed("X");
				Fail("Should have thrown an exception");
			}
			catch (DstRuleParsingException ex)
			{
				AssertEquals("Error Message", "Invalid week-day code [X]", ex.Message);
			}
		}

		void AssertParserThrowsExceptionWhenInvalidConstructionParametersAreProvided(int weekOfMonth, string weekdayCode, string monthCode, string expectedMessage)
		{
			try
			{
				DstRuleParser testParser = new DstRuleParser(
					2000, TimeZoneConstants.DstTransitionTypeStart, TimeZoneConstants.DstRuleWeekday,
					TimeZoneConstants.DstTimeBaseLocal, new DateTime(2000, 1, 1), weekOfMonth, weekdayCode, monthCode);

				Fail("Should have thrown an exception");
			}
			catch (DstRuleParsingException ex)
			{
				AssertEquals("Error Message", expectedMessage, ex.Message);
			}
		}
	}
}
