using System;
using System.Data;
using System.Globalization;
using NUnit.Framework;

namespace CargoWise.Types.Tests
{
	public class ZDateTimeTest : IZTypeTest
	{
		#region Constructors

		public void TestSecondConstructor()
		{
			ZDateTime testZDateTime = new ZDateTime(2012, 7, 27, 4, 2, 10);
			AssertEquals("Year", 2012, testZDateTime.Year);
			AssertEquals("Month", 7, testZDateTime.Month);
			AssertEquals("Day", 27, testZDateTime.Day);
			AssertEquals("Hour", 4, testZDateTime.Hour);
			AssertEquals("Minute", 2, testZDateTime.Minute);
			AssertEquals("Second", 10, testZDateTime.Second);
			AssertEquals("Kind", ZDateTime.DefaultKind, testZDateTime.Kind);
		}

		public void TestSecondWithKindConstructor()
		{
			ZDateTime testZDateTime = new ZDateTime(2012, 7, 27, 4, 2, 10, DateTimeKind.Utc);
			AssertEquals("Year", 2012, testZDateTime.Year);
			AssertEquals("Month", 7, testZDateTime.Month);
			AssertEquals("Day", 27, testZDateTime.Day);
			AssertEquals("Hour", 4, testZDateTime.Hour);
			AssertEquals("Minute", 2, testZDateTime.Minute);
			AssertEquals("Second", 10, testZDateTime.Second);
			AssertEquals("Kind", DateTimeKind.Utc, testZDateTime.Kind);
		}

		public void TestMillisecondConstructor()
		{
			ZDateTime testZDateTime = new ZDateTime(2012, 7, 27, 4, 2, 10, 901);
			AssertEquals("Year", 2012, testZDateTime.Year);
			AssertEquals("Month", 7, testZDateTime.Month);
			AssertEquals("Day", 27, testZDateTime.Day);
			AssertEquals("Hour", 4, testZDateTime.Hour);
			AssertEquals("Minute", 2, testZDateTime.Minute);
			AssertEquals("Second", 10, testZDateTime.Second);
			AssertEquals("Millisecond", 901, testZDateTime.Millisecond);
			AssertEquals("Kind", ZDateTime.DefaultKind, testZDateTime.Kind);
		}

		public void TestMillisecondWithKindConstructor()
		{
			ZDateTime testZDateTime = new ZDateTime(2012, 7, 27, 4, 2, 10, 901, DateTimeKind.Utc);
			AssertEquals("Year", 2012, testZDateTime.Year);
			AssertEquals("Month", 7, testZDateTime.Month);
			AssertEquals("Day", 27, testZDateTime.Day);
			AssertEquals("Hour", 4, testZDateTime.Hour);
			AssertEquals("Minute", 2, testZDateTime.Minute);
			AssertEquals("Second", 10, testZDateTime.Second);
			AssertEquals("Millisecond", 901, testZDateTime.Millisecond);
			AssertEquals("Kind", DateTimeKind.Utc, testZDateTime.Kind);
		}

		public void TestYearMonthDayConstructor()
		{
			ZDateTime testZDateTime = new ZDateTime(2003, 9, 18);
			AssertEquals("Year", 2003, testZDateTime.Year);
			AssertEquals("Month", 9, testZDateTime.Month);
			AssertEquals("Day", 18, testZDateTime.Day);
		}

		public void TestTicksConstructor()
		{
			ZDateTime testZDateTime = new ZDateTime(new DateTime(2005, 10, 06).Ticks);
			AssertEquals("Year", 2005, testZDateTime.Year);
			AssertEquals("Month", 10, testZDateTime.Month);
			AssertEquals("Day", 6, testZDateTime.Day);
		}

		public void TestConstructorWithValidZDate()
		{
			ZDate today = ZDate.Today;
			ZDateTime zDateTime = new ZDateTime(today);
			Assert(zDateTime.IsValid);
			Assert(!zDateTime.IsEmpty);
			Assert(zDateTime == today);
			AssertEquals("Kind", ZDateTime.DefaultKind, zDateTime.Kind);
		}

		public void TestConstructorWithEmptyZDate()
		{
			ZDateTime zDateTime = new ZDateTime(ZDate.Empty);
			Assert(zDateTime.IsEmpty);
		}

		public void TestConstructorWithInvalidZDate()
		{
			ZDateTime zDateTime = new ZDateTime(ZDate.Invalid);
			Assert(!zDateTime.IsValid);
		}

		public void TestNullConstructor()
		{
			AssertEquals(ZDateTime.Empty, new ZDateTime(null));
		}

		#endregion

		#region Static

		public void TestEmpty()
		{
			AssertEquals("IsEmpty", true, ZDateTime.Empty.IsEmpty);
			AssertEquals("IsValid", false, ZDateTime.Empty.IsValid);
			Assert("Equals", ZDateTime.Empty.Equals(ZDateTime.Empty));
		}

		public void TestInvalid()
		{
			AssertEquals("IsEmpty", false, ZDateTime.Invalid.IsEmpty);
			AssertEquals("IsValid", false, ZDateTime.Invalid.IsValid);
			Assert("Equals", ZDateTime.Invalid.Equals(ZDateTime.Invalid));
		}

		public void TestNow()
		{
			ZDateTime now = ZDateTime.Now;
			AssertEquals("IsEmpty", false, now.IsEmpty);
			AssertEquals("IsValid", true, now.IsValid);
			Assert("Equals", now.Equals(now));
			AssertEquals(DateTimeKind.Local, now.Kind);
		}

		public void TestToday()
		{
			ZDateTime today = ZDateTime.Today;
			AssertEquals("IsEmpty", false, today.IsEmpty);
			AssertEquals("IsValid", true, today.IsValid);
			Assert("Equals", today.Equals(today));
			AssertEquals(DateTimeKind.Local, today.Kind);
		}

		public void TestMinSmallDateTimeValue()
		{
			AssertEquals("IsEmpty", false, ZDateTime.MinSmallDateTimeValue.IsEmpty);
			AssertEquals("IsValid", true, ZDateTime.MinSmallDateTimeValue.IsValid);
			Assert("Equals", ZDateTime.MinSmallDateTimeValue.Equals(ZDateTime.MinSmallDateTimeValue));

			Assert(IsValidForDbType(SqlDbType.SmallDateTime, ZDateTime.MinSmallDateTimeValue));
			Assert(!IsValidForDbType(SqlDbType.SmallDateTime, ZDateTime.MinSmallDateTimeValue.AddSeconds(-31)));
		}

		public void TestMaxSmallDateTimeValue()
		{
			AssertEquals("IsEmpty", false, ZDateTime.MaxSmallDateTimeValue.IsEmpty);
			AssertEquals("IsValid", true, ZDateTime.MaxSmallDateTimeValue.IsValid);
			Assert("Equals", ZDateTime.MaxSmallDateTimeValue.Equals(ZDateTime.MaxSmallDateTimeValue));

			Assert(IsValidForDbType(SqlDbType.SmallDateTime, ZDateTime.MaxSmallDateTimeValue));
			Assert(!IsValidForDbType(SqlDbType.SmallDateTime, ZDateTime.MaxSmallDateTimeValue.AddSeconds(1)));
		}

		public void TestGetValidSmallDateTime()
		{
			AssertEquals("Brett's birthday is in the smalldatetime range", ZDateTime.BrettsBirthday, ZDateTime.GetValidSmallDateTime(ZDateTime.BrettsBirthday));
			AssertEquals("Earlier dates become min small date time", ZDateTime.MinSmallDateTimeValue, ZDateTime.GetValidSmallDateTime(ZDateTime.MinSmallDateTimeValue.AddDays(-1)));
			AssertEquals("Later dates become max small date time", ZDateTime.MaxSmallDateTimeValue, ZDateTime.GetValidSmallDateTime(ZDateTime.MaxSmallDateTimeValue.AddDays(1)));
		}

		public void TestUtcNow()
		{
			ZDateTime utcNow = ZDateTime.UtcNow;
			AssertEquals("IsEmpty", false, utcNow.IsEmpty);
			AssertEquals("IsValid", true, utcNow.IsValid);
			Assert("Equals", utcNow.Equals(utcNow));
			AssertEquals(DateTimeKind.Utc, utcNow.Kind);
		}

		public void TestFromSqlFormat()
		{
			ZDateTime myBirthday = new ZDateTime(2004, 9, 18, 12, 30, 31);
			ZString myBirthdayInSqlFormat = myBirthday.SqlFormat;
			ZDateTime myBirthdayAgain = ZDateTime.FromSqlFormat(myBirthdayInSqlFormat);
			AssertEquals("Value after passing both FromSqlFormat & SqlFormat", myBirthday, myBirthdayAgain);
		}

		public void TestTodayEqualsNowDate()
		{
			Assert("Today should be Now.Date", ZDateTime.Today == ZDateTime.Now.Date);
		}

		public void TestGet2DigitYearFrom4DigitYear()
		{
			AssertEquals("The year 50 should return 1950.", 1950, ZDateTime.Get4DigitYearFrom2DigitYear(50));
			AssertEquals("The year 70 should return 1950.", 1970, ZDateTime.Get4DigitYearFrom2DigitYear(70));
			AssertEquals("The year 90 should return 1990.", 1990, ZDateTime.Get4DigitYearFrom2DigitYear(90));
			AssertEquals("The year 99 should return 1999.", 1999, ZDateTime.Get4DigitYearFrom2DigitYear(99));
			AssertEquals("The year 00 should return 2000.", 2000, ZDateTime.Get4DigitYearFrom2DigitYear(0));
			AssertEquals("The year 20 should return 2020.", 2020, ZDateTime.Get4DigitYearFrom2DigitYear(20));
			AssertEquals("The year 40 should return 2040.", 2040, ZDateTime.Get4DigitYearFrom2DigitYear(40));
			AssertEquals("The year 49 should return 2049.", 2049, ZDateTime.Get4DigitYearFrom2DigitYear(49));
		}

		public void TestGet2DigitYearFrom4DigitYearThrowsExceptionOnOutOfRangeValue()
		{
			int exceptionsThrown = 0;
			try
			{
				ZDateTime.Get4DigitYearFrom2DigitYear(-1);
			}
			catch (ArgumentOutOfRangeException)
			{
				exceptionsThrown++;
			}

			try
			{
				ZDateTime.Get4DigitYearFrom2DigitYear(-1);
			}
			catch (ArgumentOutOfRangeException)
			{
				exceptionsThrown++;
			}

			AssertEquals("ZDateTime.Get4DigitYearFrom2DigitYear(-1) and ZDateTime.Get4DigitYearFrom2DigitYear(100) should each have thrown an exception.", 2, exceptionsThrown);
		}

		#endregion

		#region Object Overrides

		public void TestEqualsForSlightlyDifferentTimes()
		{
			AssertEquals(false, RecentDate.Equals(new ZDateTime(RecentDate.AddMilliseconds(1))));
		}

		#endregion

		#region Casting

		public void TestSetToEmpty()
		{
			ZDateTime myBirthDay = ZDateTime.Empty;
			AssertEquals("IsEmpty", myBirthDay.IsEmpty, true);
		}

		#endregion

		#region Operator Overloads

		public void TestEmptyEquals()
		{
			ZDateTime dateTime1 = ZDateTime.Empty;
			ZDateTime dateTime2 = ZDateTime.Empty;

			Assert("Empty DateTimes Equal", dateTime1 == dateTime2);
		}

		public void TestInvalidEquals()
		{
			ZDateTime dateTime1 = ZDateTime.Invalid;
			ZDateTime dateTime2 = ZDateTime.Invalid;

			Assert("Invalid DateTimes Equal", dateTime1 == dateTime2);
		}

		public void TestEmptyInvalidCombinationEquals()
		{
			ZDateTime dateTime1 = ZDateTime.Invalid;
			ZDateTime dateTime2 = ZDateTime.Empty;

			Assert("Empty Not Equals Invalid", dateTime1 != dateTime2);
		}

		public void TestEqualsOperatorWithDifferentKind()
		{
			//Kind property is ignored for datetime comparison
			AssertEquals(RecentDate, RecentDateUtc);
		}

		public void TestSubtractOperator()
		{
			ZDateTime testDate = new ZDateTime(2004, 1, 24, 14, 13, 30);
			ZDateTime fiveSecondsLaterDate = new ZDateTime(2004, 1, 24, 14, 13, 35);
			TimeSpan difference = new TimeSpan(0, 0, 5);
			AssertEquals(difference, fiveSecondsLaterDate - testDate);

			ZDateTime testDateUtc = new ZDateTime(2004, 1, 24, 14, 13, 30, DateTimeKind.Utc);
			ZDateTime fiveSecondsLaterDateUtc = new ZDateTime(2004, 1, 24, 14, 13, 35, DateTimeKind.Utc);
			AssertEquals(difference, fiveSecondsLaterDateUtc - testDateUtc);

			AssertEquals(difference, fiveSecondsLaterDate - testDateUtc);

			ZDateTime fiveSecondsLaterDateUnspecified = new ZDateTime(2004, 1, 24, 14, 13, 35, DateTimeKind.Unspecified);
			AssertEquals(difference, fiveSecondsLaterDateUnspecified - testDateUtc);
		}

		[ExpectException(typeof(OperationOnInvalidZDateTimeException))]
		public void TestSubstractOperatorWithInvalid()
		{
			ZDateTime testDate = new ZDateTime(2004, 1, 24, 14, 13, 30);
			TimeSpan result = testDate - ZDateTime.Invalid;
		}

		[ExpectException(typeof(OperationOnInvalidZDateTimeException))]
		public void TestSubstractOperatorWithEmpty()
		{
			ZDateTime testDate = new ZDateTime(2004, 1, 24, 14, 13, 30);
			TimeSpan result = ZDateTime.Empty - testDate;
		}

		public void TestAddTimeSpanOperator()
		{
			ZDateTime date = new ZDateTime(1999, 12, 31, 23, 59, 59) + new TimeSpan(0, 0, 1);
			AssertEquals("ZDateTime + TimeSpan = happy new year!", new ZDateTime(2000, 1, 1), date);

			ZDateTime dateUtc = new ZDateTime(1999, 12, 31, 23, 59, 59, DateTimeKind.Utc) + new TimeSpan(0, 0, 1);
			AssertEquals("ZDateTime + TimeSpan = happy new year!", new ZDateTime(2000, 1, 1), dateUtc);
		}

		public void TestOperatorsOnSameValue()
		{
			AssertOperatorsOnSameValue(null);
			AssertOperatorsOnSameValue(DBNull.Value);
			AssertOperatorsOnSameValue(DateTime.MinValue);
			AssertOperatorsOnSameValue(new DateTime());
			AssertOperatorsOnSameValue(new DateTime(0));
			AssertOperatorsOnSameValue(new DateTime(1));
			AssertOperatorsOnSameValue(RecentDate);
			AssertOperatorsOnSameValue(RecentDateUtc);
		}

		public void TestOperatorsOnDifferentValues()
		{
			AssertOperatorsOnDifferentValues(false, false, null, RecentDate);
			AssertOperatorsOnDifferentValues(false, false, RecentDate, null);
			AssertOperatorsOnDifferentValues(false, false, DateTime.MinValue, RecentDate);
			AssertOperatorsOnDifferentValues(false, false, RecentDate, DateTime.MinValue);
			AssertOperatorsOnDifferentValues(true, false, RecentDate, RecentDate.AddMilliseconds(1));
			AssertOperatorsOnDifferentValues(false, true, RecentDate.AddMilliseconds(1), RecentDate);

			AssertOperatorsOnDifferentValues(false, false, null, RecentDateUtc);
			AssertOperatorsOnDifferentValues(false, false, RecentDateUtc, null);
			AssertOperatorsOnDifferentValues(false, false, DateTime.MinValue, RecentDateUtc);
			AssertOperatorsOnDifferentValues(false, false, RecentDateUtc, DateTime.MinValue);
			AssertOperatorsOnDifferentValues(true, false, RecentDateUtc, RecentDateUtc.AddMilliseconds(1));
			AssertOperatorsOnDifferentValues(false, true, RecentDateUtc.AddMilliseconds(1), RecentDateUtc);
		}

		#endregion

		#region TryParse

		public void TestParseISO8601DateSafe()
		{
			AssertEquals(ZDateTime.Empty, ZDateTime.ParseISO8601DateSafe(""));
			AssertEquals(ZDateTime.Empty, ZDateTime.ParseISO8601DateSafe(null));
			AssertEquals(ZDateTime.Invalid, ZDateTime.ParseISO8601DateSafe("Some Stuff (c) David James"));
			AssertEquals(new ZDateTime(2004, 12, 17, 9, 30, 47), ZDateTime.ParseISO8601DateSafe("2004-12-17T09:30:47"));
			AssertEquals(new ZDateTime(2004, 12, 17), ZDateTime.ParseISO8601DateSafe("2004-12-17"));
		}

		public void TestTryParseISO8601Date()
		{
			bool success = ZDateTime.TryParseISO8601Date("Some Stuff (c) David James", out var parsedDateTime);
			AssertEquals(false, success);
			AssertEquals(ZDateTime.Invalid, parsedDateTime);

			success = ZDateTime.TryParseISO8601Date("2004-12-17T09:30:47", out parsedDateTime);
			AssertEquals(true, success);
			AssertEquals(new ZDateTime(2004, 12, 17, 9, 30, 47), parsedDateTime);

			success = ZDateTime.TryParseISO8601Date("2004-12-17", out parsedDateTime);
			AssertEquals(true, success);
			AssertEquals(new ZDateTime(2004, 12, 17), parsedDateTime);

			success = ZDateTime.TryParseISO8601Date(string.Empty, out parsedDateTime);
			AssertEquals(true, success);
			AssertEquals(ZDateTime.Empty, parsedDateTime);
		}

		public void TestTryParseExact()
		{
			bool success = ZDateTime.TryParseExact("Some Stuff (c) David James", out var parsedDateTime, "ddMMyy");
			AssertEquals(false, success);
			AssertEquals(ZDateTime.Invalid, parsedDateTime);

			success = ZDateTime.TryParseExact("2004-12-17T09:30:47", out parsedDateTime, "adfdasfsafasdfa");
			AssertEquals(false, success);
			AssertEquals(ZDateTime.Invalid, parsedDateTime);

			success = ZDateTime.TryParseExact("250198", out parsedDateTime, "ddMMyy");
			AssertEquals(true, success);
			AssertEquals(new ZDateTime(1998, 01, 25), parsedDateTime);

			success = ZDateTime.TryParseExact("982501", out parsedDateTime, "yyddMM");
			AssertEquals(true, success);
			AssertEquals(new ZDateTime(1998, 01, 25), parsedDateTime);

			success = ZDateTime.TryParseExact("25/01/98", out parsedDateTime, "dd/MM/yy");
			AssertEquals(true, success);
			AssertEquals(new ZDateTime(1998, 01, 25), parsedDateTime);

			success = ZDateTime.TryParseExact("25/98/01", out parsedDateTime, "dd/yy/MM");
			AssertEquals(true, success);
			AssertEquals(new ZDateTime(1998, 01, 25), parsedDateTime);

			success = ZDateTime.TryParseExact("98/25/01", out parsedDateTime, "yy/dd/MM");
			AssertEquals(true, success);
			AssertEquals(new ZDateTime(1998, 01, 25), parsedDateTime);

			success = ZDateTime.TryParseExact("25/01/2098", out parsedDateTime, "dd/MM/yyyy");
			AssertEquals(true, success);
			AssertEquals(new ZDateTime(2098, 01, 25), parsedDateTime);

			success = ZDateTime.TryParseExact("25/2098/01", out parsedDateTime, "dd/yyyy/MM");
			AssertEquals(true, success);
			AssertEquals(new ZDateTime(2098, 01, 25), parsedDateTime);

			success = ZDateTime.TryParseExact("2098/25/01", out parsedDateTime, "yyyy/dd/MM");
			AssertEquals(true, success);
			AssertEquals(new ZDateTime(2098, 01, 25), parsedDateTime);

			success = ZDateTime.TryParseExact("25/01/98", out parsedDateTime, "dd/MM/yy");
			AssertEquals(true, success);
			AssertEquals(new ZDateTime(1998, 01, 25), parsedDateTime);

			success = ZDateTime.TryParseExact("25-98-01", out parsedDateTime, "dd-yy-MM");
			AssertEquals(true, success);
			AssertEquals(new ZDateTime(1998, 01, 25), parsedDateTime);

			success = ZDateTime.TryParseExact("98-25-01", out parsedDateTime, "yy-dd-MM");
			AssertEquals(true, success);
			AssertEquals(new ZDateTime(1998, 01, 25), parsedDateTime);

			success = ZDateTime.TryParseExact("2004-12-17", out parsedDateTime, "yyyy-MM-dd");
			AssertEquals(true, success);
			AssertEquals(new ZDateTime(2004, 12, 17), parsedDateTime);

			success = ZDateTime.TryParseExact("", out parsedDateTime, "yyyy-MM-dd");
			AssertEquals(true, success);
			AssertEquals(ZDateTime.Empty, parsedDateTime);
		}

		[TestDate(2000, 6, 6, 12, 0, 0)]
		[TestUtcOffset(-12, 0, 0)]
		public void TestTryPassExact_TimeOnly()
		{
			for (var i = -12; i < 13; ++i)
			{
				var today = ZDateTime.Today;
				var success = ZDateTime.TryParseExact("16:17", out var parsedDateTime, "mm:ss");
				AssertEquals(expected: true, success);
				AssertEquals(new ZDateTime(today.Year, today.Month, today.Day, 0, 16, 17, DateTimeKind.Local), parsedDateTime);
				success = ZDateTime.TryParseExact("16:17:18", out parsedDateTime, "HH:mm:ss");
				AssertEquals(expected: true, success);
				AssertEquals(new ZDateTime(today.Year, today.Month, today.Day, 16, 17, 18, DateTimeKind.Local), parsedDateTime);
				success = ZDateTime.TryParseExact("08-15", out parsedDateTime, "MM-dd");
				AssertEquals(expected: true, success);
				AssertEquals(new ZDateTime(today.Year, 8, 15, 0, 0, 0, DateTimeKind.Local), parsedDateTime);
				success = ZDateTime.TryParseExact("08-15 11:31:32", out parsedDateTime, "MM-dd HH:mm:ss");
				AssertEquals(expected: true, success);
				AssertEquals(new ZDateTime(today.Year, 8, 15, 11, 31, 32, DateTimeKind.Local), parsedDateTime);
			}
		}

		#endregion

		#region ISO Week Date

		public void TestFromIsoWeekDateStringWC()
		{
			AssertEquals("ISO Week WC0150 should convert to 02 Jan 1950.", new ZDateTime(1950, 1, 2), ZDateTime.FromIsoWeekDateString("WC0150")); // test year round down

			AssertEquals("ISO Week WC0105 should convert to 03 Jan 2005.", new ZDateTime(2005, 1, 3), ZDateTime.FromIsoWeekDateString("WC0105"));
			AssertEquals("ISO Week WC0205 should convert to 10 Jan 2005.", new ZDateTime(2005, 1, 10), ZDateTime.FromIsoWeekDateString("WC0205"));

			AssertEquals("ISO Week WC0106 should convert to 02 Jan 2006.", new ZDateTime(2006, 1, 2), ZDateTime.FromIsoWeekDateString("WC0106"));
			AssertEquals("ISO Week WC0206 should convert to 09 Jan 2006.", new ZDateTime(2006, 1, 9), ZDateTime.FromIsoWeekDateString("WC0206"));

			AssertEquals("ISO Week WC0107 should convert to 01 Jan 2007.", new ZDateTime(2007, 1, 1), ZDateTime.FromIsoWeekDateString("WC0107"));
			AssertEquals("ISO Week WC0207 should convert to 08 Jan 2007.", new ZDateTime(2007, 1, 8), ZDateTime.FromIsoWeekDateString("WC0207"));

			AssertEquals("ISO Week WC0109 should convert to 29 Dec 2008.", new ZDateTime(2008, 12, 29), ZDateTime.FromIsoWeekDateString("WC0109"));
			AssertEquals("ISO Week WC0209 should convert to 05 Jan 2009.", new ZDateTime(2009, 1, 5), ZDateTime.FromIsoWeekDateString("WC0209"));

			AssertEquals("ISO Week WC5309 should convert to 28 Dec 2009.", new ZDateTime(2009, 12, 28), ZDateTime.FromIsoWeekDateString("WC5309")); // test week 53

			AssertNoExceptionThrown(delegate
			{ ZDateTime.FromIsoWeekDateString("WC"); });
		}

		public void TestFromIsoWeekDateStringWCWithInvalidValues()
		{
			AssertEquals("ISO Week WC5408 is not a valid ISO Week date.", ZDateTime.Invalid, ZDateTime.FromIsoWeekDateString("WC5408"));
			AssertEquals("ISO Week WC5308 is not a valid ISO Week date.", ZDateTime.Invalid, ZDateTime.FromIsoWeekDateString("WC5308")); // the Thursday of this week is in the following year
		}

		public void TestFromIsoWeekDateStringWE()
		{
			AssertEquals("ISO Week WE0150 should convert to 02 Jan 1950.", new ZDateTime(1950, 1, 8, 23, 59, 59), ZDateTime.FromIsoWeekDateString("WE0150")); // test year round down

			AssertEquals("ISO Week WE0105 should convert to 09 Jan 2005.", new ZDateTime(2005, 1, 9, 23, 59, 59), ZDateTime.FromIsoWeekDateString("WE0105"));
			AssertEquals("ISO Week WE0205 should convert to 16 Jan 2005.", new ZDateTime(2005, 1, 16, 23, 59, 59), ZDateTime.FromIsoWeekDateString("WE0205"));

			AssertEquals("ISO Week WE0106 should convert to 08 Jan 2006.", new ZDateTime(2006, 1, 8, 23, 59, 59), ZDateTime.FromIsoWeekDateString("WE0106"));
			AssertEquals("ISO Week WE0206 should convert to 15 Jan 2006.", new ZDateTime(2006, 1, 15, 23, 59, 59), ZDateTime.FromIsoWeekDateString("WE0206"));

			AssertEquals("ISO Week WE0107 should convert to 07 Jan 2007.", new ZDateTime(2007, 1, 7, 23, 59, 59), ZDateTime.FromIsoWeekDateString("WE0107"));
			AssertEquals("ISO Week WE0207 should convert to 14 Jan 2007.", new ZDateTime(2007, 1, 14, 23, 59, 59), ZDateTime.FromIsoWeekDateString("WE0207"));

			AssertEquals("ISO Week WE0109 should convert to 04 Jan 2009.", new ZDateTime(2009, 1, 4, 23, 59, 59), ZDateTime.FromIsoWeekDateString("WE0109"));
			AssertEquals("ISO Week WE0209 should convert to 11 Jan 2009.", new ZDateTime(2009, 1, 11, 23, 59, 59), ZDateTime.FromIsoWeekDateString("WE0209"));

			AssertEquals("ISO Week WE5309 should convert to 03 Jan 2010.", new ZDateTime(2010, 1, 3, 23, 59, 59), ZDateTime.FromIsoWeekDateString("WE5309")); // test week 53
		}

		public void TestFromIsoWeekDateStringWEWithInvalidValues()
		{
			AssertEquals("ISO Week WE5408 is not a valid ISO Week date.", ZDateTime.Invalid, ZDateTime.FromIsoWeekDateString("WE5408"));
			AssertEquals("ISO Week WE5308 is not a valid ISO Week date.", ZDateTime.Invalid, ZDateTime.FromIsoWeekDateString("WE5308")); // the Thursday of this week is in the following year
		}

		#endregion

		#region IsToday

		public void TestIsToday()
		{
			ZDateTime someTimeYesterday = ZDateTime.Today.AddDays(-1);
			ZDateTime someTimeLastMonth = ZDateTime.Today.AddMonths(-1);
			ZDateTime someTimeLastYear = ZDateTime.Today.AddYears(-1);

			Assert("Date was not today but yesterday", !someTimeYesterday.IsToday);
			Assert("Date was not today but last month", !someTimeLastMonth.IsToday);
			Assert("Date was not today but last year", !someTimeLastYear.IsToday);
			Assert("ZDateTime.Now is today", ZDateTime.Now.IsToday);
			Assert("Invalid dater cannot be today", !ZDateTime.Invalid.IsToday);
		}

		[TestDate(2006, 5, 7, 1, 1, 2)]
		[TestTimeZone]
		public void TestIsToday_WithUtcKind_DifferentDays()
		{
			ZDateTime now = ZDateTime.Now;
			ZDateTime utcNow = new ZDateTime(now.ToDateTime().ToUniversalTime(), DateTimeKind.Utc);

			AssertZTypeSerializesToXml("<a>2006-05-07T01:01:02+02:00</a>", now);
			Assert("ZDateTime.Now is today", now.IsToday);

			AssertZTypeSerializesToXml("<a>2006-05-06T23:01:02Z</a>", utcNow);
			Assert("ZDateTime.Now converted to UTC is not today", !utcNow.IsToday);
		}

		[TestDate(2006, 5, 7, 4, 1, 2)]
		[TestTimeZone]
		public void TestIsToday_WithUtcKind_SameDays()
		{
			ZDateTime now = ZDateTime.Now;
			ZDateTime utcNow = new ZDateTime(now.ToDateTime().ToUniversalTime(), DateTimeKind.Utc);

			AssertZTypeSerializesToXml("<a>2006-05-07T04:01:02+02:00</a>", now);
			Assert("ZDateTime.Now is today", now.IsToday);

			AssertZTypeSerializesToXml("<a>2006-05-07T02:01:02Z</a>", utcNow);
			Assert("ZDateTime.Now converted to UTC is not today", utcNow.IsToday);
		}

		#endregion

		#region Date/time Offset Conversion

		public void TestGetMinutesFromDateTimeSpan()
		{
			AssertEquals(0.0, new ZDateTime(2000, 1, 1).GetMinutesFromDateTimeSpan());
			AssertEquals(0.0, new ZDateTime(3001, 1, 1).GetMinutesFromDateTimeSpan());
			AssertEquals(1440.0, new ZDateTime(2000, 1, 2).GetMinutesFromDateTimeSpan());
			AssertEquals(1440.0, new ZDateTime(3001, 1, 2).GetMinutesFromDateTimeSpan());
			AssertEquals(1500.0, new ZDateTime(2000, 1, 2, 1, 0, 0).GetMinutesFromDateTimeSpan());
			AssertEquals(1500.0, new ZDateTime(3001, 1, 2, 1, 0, 0).GetMinutesFromDateTimeSpan());
			AssertEquals(1510.0, new ZDateTime(2000, 1, 2, 1, 10, 0).GetMinutesFromDateTimeSpan());
			AssertEquals(1510.0, new ZDateTime(3001, 1, 2, 1, 10, 0).GetMinutesFromDateTimeSpan());
		}

		#endregion

		#region Is in the future or is in the past

		[TestDate(2017, 07, 01, 8, 30, 0)]
		public void TestIsInTheFuture_Or_IsInThePast()
		{
			var timeNow = ZDateTime.Now;

			var todayOneHourAgo = timeNow.AddHours(-1);
			var yesterday = timeNow.AddDays(-1);
			var tomorrow = timeNow.AddDays(1);
			var minueAgo = timeNow.AddMinutes(-1);
			var secondAgo = timeNow.AddSeconds(-1);
			var milliSecondAgo = timeNow.AddMilliseconds(-1);
			var minuteLater = timeNow.AddMinutes(1);
			var secondLater = timeNow.AddSeconds(1);
			var milliSecondLater = timeNow.AddMilliseconds(1);

			Assert("Today one hour ago is in the past", !todayOneHourAgo.IsInTheFuture());
			Assert("yesterday is in the past", !yesterday.IsInTheFuture());
			Assert("tomorrow is not in the past, it's future", tomorrow.IsInTheFuture());

			Assert("Minute ago is in the past", !minueAgo.IsInTheFuture());
			Assert("Second ago is in the past", !secondAgo.IsInTheFuture());
			Assert("milliSecondAgo is in the past, it's future", !milliSecondAgo.IsInTheFuture());

			Assert("minuteLater is not in the past, it's future", minuteLater.IsInTheFuture());
			Assert("secondLater is not in the past, it's future", secondLater.IsInTheFuture());
			Assert("milliSecondLater is not in the past, it's future", milliSecondLater.IsInTheFuture());

			Assert("ZDateTime.Empty is not in the future, it's empty", !ZDateTime.Empty.IsInTheFuture());

			Assert("Today one hour ago is in the past", todayOneHourAgo.IsInThePast());
			Assert("yesterday is in the past", yesterday.IsInThePast());
			Assert("tomorrow is not in the past, it's future", !tomorrow.IsInThePast());

			Assert("Minute ago is in the past", minueAgo.IsInThePast());
			Assert("Second ago is in the past", secondAgo.IsInThePast());
			Assert("milliSecondAgo is in the past, it's future", milliSecondAgo.IsInThePast());

			Assert("minuteLater is not in the past, it's future", !minuteLater.IsInThePast());
			Assert("secondLater is not in the past, it's future", !secondLater.IsInThePast());
			Assert("milliSecondLater is not in the past, it's future", !milliSecondLater.IsInThePast());

			Assert("ZDateTime.Empty is not in the past, it's empty", !ZDateTime.Empty.IsInThePast());
			Assert("ZDateTime.Empty is not in the future, it's empty", !ZDateTime.Empty.IsInTheFuture());
		}

		[TestDate(2017, 07, 01, 11, 30, 0)]
		public void TestIsInTheFuture_WithDelta()
		{
			var hour = new TimeSpan(1, 0, 0).TotalMilliseconds;
			var minute = new TimeSpan(0, 1, 0).TotalMilliseconds;
			var second = new TimeSpan(0, 0, 1).TotalMilliseconds;
			var milliSecond = 1.0;

			var timeNow = ZDateTime.Now;

			var hourAgo = timeNow.AddHours(-1);
			var minutAgo = timeNow.AddMinutes(-1);
			var secondAgo = timeNow.AddSeconds(-1);
			var milliSecondAgo = timeNow.AddMilliseconds(-1);

			var hourLater = timeNow.AddHours(1);
			var minutLater = timeNow.AddMinutes(1);
			var secondLater = timeNow.AddSeconds(1);
			var milliSecondLater = timeNow.AddMilliseconds(1);

			Assert("one hour ago is in the past when error is set to hour", !hourAgo.IsInTheFuture(hour));
			Assert("one minute ago is in the past when error is set to minute", !minutAgo.IsInTheFuture(minute));
			Assert("one second ago is in the past when error is set to second", !secondAgo.IsInTheFuture(second));
			Assert("one millisecond ago is in the past when error is set to millisecond", !milliSecondAgo.IsInTheFuture(milliSecond));

			Assert("one hour later is in the future when error is set to hour", hourLater.IsInTheFuture(hour));
			Assert("one minute later is in the future when error is set to minute", minutLater.IsInTheFuture(minute));
			Assert("one second later is in the future when error is set to second", secondLater.IsInTheFuture(second));
			Assert("one millisecond later is in the future when error is set to millisecond", milliSecondLater.IsInTheFuture(milliSecond));
		}

		[TestDate(2017, 07, 01, 8, 30, 0)]
		public void TestIsInTheFutureUtc()
		{
			var timeNow = ZDateTime.UtcNow;

			var todayOneHourAgo = timeNow.AddHours(-1);
			var yesterday = timeNow.AddDays(-1);
			var tomorrow = timeNow.AddDays(1);
			var minueAgo = timeNow.AddMinutes(-1);
			var secondAgo = timeNow.AddSeconds(-1);
			var milliSecondAgo = timeNow.AddMilliseconds(-1);
			var minuteLater = timeNow.AddMinutes(1);
			var secondLater = timeNow.AddSeconds(1);
			var milliSecondLater = timeNow.AddMilliseconds(1);

			Assert("Today one hour ago is in the past", !todayOneHourAgo.IsInTheFuture());
			Assert("yesterday is in the past", !yesterday.IsInTheFuture());
			Assert("tomorrow is not in the past, it's future", tomorrow.IsInTheFuture());

			Assert("Minute ago is in the past", !minueAgo.IsInTheFuture());
			Assert("Second ago is in the past", !secondAgo.IsInTheFuture());
			Assert("milliSecondAgo is in the past, it's future", !milliSecondAgo.IsInTheFuture());

			Assert("minuteLater is not in the past, it's future", minuteLater.IsInTheFuture());
			Assert("secondLater is not in the past, it's future", secondLater.IsInTheFuture());
			Assert("milliSecondLater is not in the past, it's future", milliSecondLater.IsInTheFuture());

			Assert("ZDateTime.Empty is not in the future, it's empty", !ZDateTime.Empty.IsInTheFuture());
			Assert("ZDateTime.Empty is not in the past, it's empty", !ZDateTime.Empty.IsInThePast());
		}

		[TestDate(2017, 07, 01, 11, 30, 0)]
		public void TestIsInTheFutureUtc_WithDelta()
		{
			var hour = new TimeSpan(1, 0, 0).TotalMilliseconds;
			var minute = new TimeSpan(0, 1, 0).TotalMilliseconds;
			var second = new TimeSpan(0, 0, 1).TotalMilliseconds;
			var milliSecond = 1.0;

			var timeNow = ZDateTime.UtcNow;

			var hourAgo = timeNow.AddHours(-1);
			var minueAgo = timeNow.AddMinutes(-1);
			var secondAgo = timeNow.AddSeconds(-1);
			var milliSecondAgo = timeNow.AddMilliseconds(-1);

			var hourLater = timeNow.AddHours(1);
			var minutLater = timeNow.AddMinutes(1);
			var secondLater = timeNow.AddSeconds(1);
			var milliSecondLater = timeNow.AddMilliseconds(1);

			Assert("one hour ago is in the past when error is set to hour", !hourAgo.IsInTheFutureUtc(hour));
			Assert("one minute ago is in the past when error is set to minute", !minueAgo.IsInTheFutureUtc(minute));
			Assert("one second ago is in the past when error is set to second", !secondAgo.IsInTheFutureUtc(second));
			Assert("one millisecond ago is in the past when error is set to millisecond", !milliSecondAgo.IsInTheFutureUtc(milliSecond));

			Assert("one hour later is in the future when error is set to hour", hourLater.IsInTheFutureUtc(hour));
			Assert("one minute later is in the future when error is set to minute", minutLater.IsInTheFutureUtc(minute));
			Assert("one second later is in the future when error is set to second", secondLater.IsInTheFutureUtc(second));
			Assert("one millisecond later is in the future when error is set to millisecond", milliSecondLater.IsInTheFutureUtc(milliSecond));
		}

		[TestDate(2017, 07, 01, 8, 30, 0)]
		public void TestIsInTheFuture_WithReferenceTime()
		{
			var yesterday = ZDateTime.Now.AddDays(-1);
			var now = ZDateTime.Now;

			var oneHourAgoYesterday = yesterday.AddHours(-1);
			var dayBeforeYesterday = yesterday.AddDays(-1);
			var today = yesterday.AddDays(1);
			var minueAgoYesterday = yesterday.AddMinutes(-1);
			var secondAgoYesterday = yesterday.AddSeconds(-1);
			var milliSecondAgoYesterday = yesterday.AddMilliseconds(-1);
			var minuteLaterYesterday = yesterday.AddMinutes(1);
			var secondLaterYesterday = yesterday.AddSeconds(1);
			var milliSecondLaterYesterday = yesterday.AddMilliseconds(1);
			var tomorrow = yesterday.AddDays(2);

			Assert("All in the past, not in the future", !oneHourAgoYesterday.IsInTheFuture(now));
			Assert("All in the past, not in the future", !dayBeforeYesterday.IsInTheFuture(now));

			Assert("All in the past, not in the future", !minueAgoYesterday.IsInTheFuture(now));
			Assert("All in the past, not in the future", !secondAgoYesterday.IsInTheFuture(now));
			Assert("All in the past, not in the future", !milliSecondAgoYesterday.IsInTheFuture(now));

			Assert("All in the past, not in the future", !minuteLaterYesterday.IsInTheFuture(now));
			Assert("All in the past, not in the future", !secondLaterYesterday.IsInTheFuture(now));
			Assert("All in the past, not in the future", !milliSecondLaterYesterday.IsInTheFuture(now));

			Assert("Today just now is also in the past, not in the future", !today.IsInTheFuture(now));
			Assert("Tomorrow is in the future for sure", tomorrow.IsInTheFuture(now));
		}

		#endregion

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestToOffset()
		{
			//ZDateTime is assumed to already be utc so we just give it utc offset (0).
			AssertEquals(new ZDateTimeOffset(2000, 6, 6, 12, 0, 0, TimeSpan.FromHours(0)), new ZDateTime(2000, 6, 6, 12, 0, 0, DateTimeKind.Utc).ToOffset());
			//ZDateTime is assumed to already be local to the current branch's UNLOCO so we just give it the branch's UNLOCO's offset (10 in this case).
			AssertEquals(new ZDateTimeOffset(2000, 6, 6, 12, 0, 0, TimeSpan.FromHours(10)), new ZDateTime(2000, 6, 6, 12, 0, 0, DateTimeKind.Local).ToOffset());
			AssertEquals(new ZDateTimeOffset(2000, 6, 6, 12, 0, 0, TimeSpan.FromHours(10)), new ZDateTime(2000, 6, 6, 12, 0, 0, DateTimeKind.Unspecified).ToOffset());
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestUtcToDateTimeOffset()
		{
			//The kind is ignored, we will treat the ZDateTime as UTC, and convert it to a local time in the current branch's UNLOCO.
			AssertEquals(new ZDateTimeOffset(2000, 6, 6, 22, 0, 0, TimeSpan.FromHours(10)), new ZDateTime(2000, 6, 6, 12, 0, 0, DateTimeKind.Utc).UtcToDateTimeOffset());
			AssertEquals(new ZDateTimeOffset(2000, 6, 6, 22, 0, 0, TimeSpan.FromHours(10)), new ZDateTime(2000, 6, 6, 12, 0, 0, DateTimeKind.Local).UtcToDateTimeOffset());
			AssertEquals(new ZDateTimeOffset(2000, 6, 6, 22, 0, 0, TimeSpan.FromHours(10)), new ZDateTime(2000, 6, 6, 12, 0, 0, DateTimeKind.Unspecified).UtcToDateTimeOffset());
		}

		public void TestCastToZDateTimeFromTimeSpan()
		{
			AssertEquals(new ZDateTime(1900, 1, 1, 8, 0, 0), (ZDateTime)TimeSpan.FromHours(8));
		}

		public void TestToTimeSpan()
		{
			AssertEquals(TimeSpan.FromHours(8), new ZDateTime(1900, 1, 1, 8, 0, 0).ToTimeSpan());
			AssertEquals(TimeSpan.FromHours(8), new ZDateTime(2013, 1, 1, 8, 0, 0).ToTimeSpan());
		}

		public void TestSmallDateTime()
		{
			AssertEquals(new ZDateTime(2000, 1, 1, 1, 1, 0), new ZDateTime(2000, 1, 1, 1, 1, 29).ToSmallDateTime());
			AssertEquals(new ZDateTime(2000, 1, 1, 1, 1, 0), new ZDateTime(2000, 1, 1, 1, 1, 29).AddMilliseconds(998).ToSmallDateTime());
			AssertEquals(new ZDateTime(2000, 1, 1, 1, 2, 0), new ZDateTime(2000, 1, 1, 1, 1, 29).AddMilliseconds(999).ToSmallDateTime());
			AssertEquals(new ZDateTime(2000, 1, 1, 1, 2, 0), new ZDateTime(2000, 1, 1, 1, 1, 30).ToSmallDateTime());
			AssertEquals(new ZDateTime(2000, 1, 1, 1, 2, 0), new ZDateTime(2000, 1, 1, 1, 1, 59).AddMilliseconds(998).ToSmallDateTime());
			AssertEquals(new ZDateTime(2000, 1, 1, 1, 2, 0), new ZDateTime(2000, 1, 1, 1, 1, 59).AddMilliseconds(999).ToSmallDateTime());
			AssertEquals(new ZDateTime(2000, 1, 1, 1, 2, 0), new ZDateTime(2000, 1, 1, 1, 2, 0).ToSmallDateTime());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1122:DoNotUseDateTimeParse", Justification = "Testing")]
		public void TestToSmallDateTimeFloor()
		{
			AssertEquals(new ZDateTime(2000, 1, 1, 1, 1, 0), new ZDateTime(2000, 1, 1, 1, 1, 29).ToSmallDateTimeFloor());
			AssertEquals(new ZDateTime(2000, 1, 1, 1, 1, 0), new ZDateTime(2000, 1, 1, 1, 1, 29).AddMilliseconds(998).ToSmallDateTimeFloor());
			AssertEquals(new ZDateTime(2000, 1, 1, 1, 1, 0), new ZDateTime(2000, 1, 1, 1, 1, 29).AddMilliseconds(999).ToSmallDateTimeFloor());
			AssertEquals(new ZDateTime(2000, 1, 1, 1, 1, 0), new ZDateTime(2000, 1, 1, 1, 1, 30).ToSmallDateTimeFloor());
			AssertEquals(new ZDateTime(2000, 1, 1, 1, 1, 0), new ZDateTime(2000, 1, 1, 1, 1, 59).AddMilliseconds(998).ToSmallDateTimeFloor());
			AssertEquals(new ZDateTime(2000, 1, 1, 1, 1, 0), new ZDateTime(2000, 1, 1, 1, 1, 59).AddMilliseconds(999).ToSmallDateTimeFloor());
			AssertEquals(new ZDateTime(2000, 1, 1, 1, 2, 0), new ZDateTime(2000, 1, 1, 1, 2, 0).ToSmallDateTimeFloor());
			AssertEquals("Fractional milliseconds are removed", new ZDateTime(2000, 1, 1, 1, 1, 0), new ZDateTime(DateTime.Parse("2000-01-01T01:01:01.8019493")).ToSmallDateTimeFloor());
		}

		[TestDate(2006, 1, 1, 8, 30, 0)]
		public void TestIsInThePastDatePartOnly()
		{
			ZDateTime todayOneHourAgo = ZDateTime.Now.AddHours(-1);
			ZDateTime yesterday = ZDateTime.Now.AddDays(-1);
			ZDateTime tomorrow = ZDateTime.Now.AddDays(1);

			Assert("Today one hour ago is not InThePastDatePartOnly", !todayOneHourAgo.IsInThePastDatePartOnly);
			Assert("yesterday is InThePastDatePartOnly", yesterday.IsInThePastDatePartOnly);
			Assert("tomorrow is not in ThePastDatePartOnly", !tomorrow.IsInThePastDatePartOnly);
		}

		[TestDate(2006, 1, 1, 8, 30, 0)]
		public void TestIsInTheFutureDatePartOnly()
		{
			ZDateTime todayOneHourLater = ZDateTime.Now.AddHours(1);
			ZDateTime yesterday = ZDateTime.Now.AddDays(-1);
			ZDateTime tomorrow = ZDateTime.Now.AddDays(1);

			Assert("Today one hour later is not TheFutureDatePartOnly", !todayOneHourLater.IsInTheFutureDatePartOnly);
			Assert("yesterday is not TheFutureDatePartOnly", !yesterday.IsInTheFutureDatePartOnly);
			Assert("tomorrow is in TheFutureDatePartOnly", tomorrow.IsInTheFutureDatePartOnly);
		}

		public void TestIsValidSmallDateTime()
		{
			Assert("MinSmallDateTimeValue", ZDateTime.MinSmallDateTimeValue.IsValidSmallDateTime);
			Assert("One second earlyer than MinSmallDateTimeValue", !ZDateTime.MinSmallDateTimeValue.AddSeconds(-1).IsValidSmallDateTime);
			Assert("MaxSmallDateTimeValue", ZDateTime.MaxSmallDateTimeValue.IsValidSmallDateTime);
			Assert("One seccond after MaxSmallDateTimeValue", !ZDateTime.MaxSmallDateTimeValue.AddSeconds(1).IsValidSmallDateTime);
		}

		public void TestIsValidSqlDateTime()
		{
			Assert(new ZDateTime(2006, 01, 01).IsValidSqlDateTime);
			Assert(!new ZDateTime(SqlDateTime.MinValue.Value.AddSeconds(-1)).IsValidSqlDateTime);
			Assert(!new ZDateTime(DateTime.MaxValue).IsValidSqlDateTime);
			Assert(!new ZDateTime(DateTime.MinValue).IsValidSqlDateTime);
			Assert(!new ZDateTime(ZDateTime.Empty).IsValidSqlDateTime);
			Assert(!new ZDateTime(ZDateTime.Invalid).IsValidSqlDateTime);
			Assert(!new ZDateTime(new ZDateTime()).IsValidSqlDateTime);
		}

		public void TestTimeSpan6MonthsFromStartOfYear()
		{
			AssertEquals("TimeSpanFromStartOfYear for positive value", new TimeSpan(52, 5, 0), new ZDateTime(2006, 1, 1).AddHours(52).AddMinutes(5).TimeSpan6MonthsFromStartOfYear);
			AssertEquals("TimeSpanFromStartOfYear for negative value", new TimeSpan(-52, -5, 0), new ZDateTime(2006, 1, 1).AddHours(-52).AddMinutes(-5).TimeSpan6MonthsFromStartOfYear);
		}

		#region Wrapping the internal DateTime

		public void TestToShortDateString()
		{
			ZDateTime testDateTime = new ZDateTime(2004, 10, 12);
			AssertEquals("12-Oct-04", testDateTime.ToShortDateString());
		}

		public void TestToShortTimeString()
		{
			ZDateTime testDateTime = new ZDateTime(2004, 10, 12, 12, 34, 15);
			AssertEquals("12:34", testDateTime.ToShortTimeString());
		}

		public void TestToLongTimeString()
		{
			ZDateTime testDateTime = new ZDateTime(2004, 10, 12, 12, 34, 15);
			AssertEquals("12-Oct-04 12:34", testDateTime.ToLongTimeString());
		}

		public void TestToISO8601String()
		{
			ZDateTime dateTime = new ZDateTime(2004, 11, 25, 15, 25, 59);
			AssertEquals("2004-11-25T15:25:59", dateTime.ToISO8601String());

			ZDateTime.TryParseISO8601Date("2004-12-17", out dateTime);
			AssertEquals("2004-12-17T00:00:00", dateTime.ToISO8601String());

			dateTime = new DateTime(0);
			AssertEquals("<Invalid>", dateTime.ToISO8601String());
		}

		public void TestToISO8601ShortDateString()
		{
			ZDateTime testDateTime = new ZDateTime(2004, 10, 12);
			AssertEquals("2004-10-12", testDateTime.ToISO8601ShortDateString());
		}

		public void TestToBestReadableDateTimeString()
		{
			ZDateTime testDateTime = new ZDateTime(2017, 1, 11, 11, 30, 00);
			AssertEquals("Brett's Birthday in BestReadableDateTime Format", "18 Sep 1971 00:00", ZDateTime.BrettsBirthday.ToBestReadableDateTimeString());
			AssertEquals("ZDateTime(2017, 1, 11, 11, 30, 00) in BestReadableDateTime Format", "11 Jan 2017 11:30", testDateTime.ToBestReadableDateTimeString());
			AssertEquals("ZDateTime.Empty in BestReadableDateTime Format", "", ZDateTime.Empty.ToBestReadableDateTimeString());
		}

		public void TestToBestReadableDateString()
		{
			ZDateTime testDateTime = new ZDateTime(2017, 1, 11, 11, 30, 00);
			AssertEquals("Brett's Birthday in BestReadableDate Format", "18 Sep 1971", ZDateTime.BrettsBirthday.ToBestReadableDateString());
			AssertEquals("ZDateTime(2017, 1, 11, 11, 30, 00) in BestReadableDate Format", "11 Jan 2017", testDateTime.ToBestReadableDateString());
			AssertEquals("ZDateTime.Empty in BestReadableDate Format", "", ZDateTime.Empty.ToBestReadableDateString());
		}

		public void TestAddDateComponents()
		{
			foreach (DateTime value in new DateTime[] { new DateTime(3, 1, 1), RecentDate })
			{
				foreach (int addend in new int[] { -1, 0, 1, 100 })
				{
					string message = MessageForValue(value) + " + " + addend + " ";
					ZDateTime z = new ZDateTime(value);
					TimeSpan ticks = new TimeSpan(addend);

					AssertEquals(message + "time span", new ZDateTime(value.Add(ticks)), z.Add(ticks));
					AssertEquals(message + "ticks", new ZDateTime(value.AddTicks(addend)), z.AddTicks(addend));
					AssertEquals(message + "milliseconds", new ZDateTime(value.AddMilliseconds(addend)), z.AddMilliseconds(addend));
					AssertEquals(message + "seconds", new ZDateTime(value.AddSeconds(addend)), z.AddSeconds(addend));
					AssertEquals(message + "minutes", new ZDateTime(value.AddMinutes(addend)), z.AddMinutes(addend));
					AssertEquals(message + "hours", new ZDateTime(value.AddHours(addend)), z.AddHours(addend));
					AssertEquals(message + "days", new ZDateTime(value.AddDays(addend)), z.AddDays(addend));
					AssertEquals(message + "months", new ZDateTime(value.AddMonths(addend)), z.AddMonths(addend));
					AssertEquals(message + "years", new ZDateTime(value.AddYears(addend)), z.AddYears(addend));
				}
			}
		}

		public void TestSubtractYearToGetInvalidZDateTime()
		{
			var dt = new DateTime(2, 1, 1);
			ZDateTime zdt = new ZDateTime(dt);

			AssertEquals("Subtracting one year from 2,1,1 datetime should equal datetime.min", DateTime.MinValue, dt.AddYears(-1));
			AssertExceptionThrown<InvalidZDateTimeResultException>("Subtracting one year from 2,1,1 zdatetime equals zdt.invalid and should not be used", () => zdt.AddYears(-1));
		}

		public void TestAddHoursWithZDecimal()
		{
			foreach (DateTime value in new DateTime[] { new DateTime(2, 1, 1), RecentDate })
			{
				foreach (ZDecimal addend in new ZDecimal[] { -1.00m, -0.75m, -0.50m, -0.25m, 0, 0.25m, 0.75m, 0.50m, 1.00m })
				{
					string message = MessageForValue(value) + " + " + addend + " ";
					ZDateTime z = new ZDateTime(value);
					decimal decimalAddend = (decimal)addend;

					AssertEquals(message + "hours", new ZDateTime(value.AddHours((double)decimalAddend)), z.AddHours(addend));
				}
			}
		}

		public void TestDateComponents()
		{
			foreach (object validValue in ValidValues)
			{
				if (validValue is DateTime value)
				{
					string message = MessageForValue(value) + " ";
					ZDateTime z = new ZDateTime(value);

					AssertEquals(message + "Date", value.Date, z.Date);
					AssertEquals(message + "Ticks", value.Ticks, z.Ticks);
					AssertEquals(message + "TimeOfDay", value.TimeOfDay, z.TimeOfDay);
					AssertEquals(message + "Millisecond", value.Millisecond, z.Millisecond);
					AssertEquals(message + "Second", value.Second, z.Second);
					AssertEquals(message + "Minute", value.Minute, z.Minute);
					AssertEquals(message + "Hour", value.Hour, z.Hour);
					AssertEquals(message + "Day", value.Day, z.Day);
					AssertEquals(message + "DayOfWeek", value.DayOfWeek, z.DayOfWeek);
					AssertEquals(message + "DayOfYear", value.DayOfYear, z.DayOfYear);
					AssertEquals(message + "Month", value.Month, z.Month);
					AssertEquals(message + "Year", value.Year, z.Year);
				}
			}
		}

		public void TestShouldThrowCorrectInvalidOperationExceptionMessage_WhenAddInvalidDays_FixingIssue00917305()
		{
			DateTime value = new DateTime(1, 1, 1);
			ZDateTime z = new ZDateTime(value);
			AssertExceptionThrown("Expect should throw correct message in EnsureValidValueFor().",
				typeof(InvalidOperationException), "Cannot add days to an invalid date",
				() => z.AddDays(0));
		}

		public void TestAddThrowsInvalidOperationExceptionWhenInvalid()
		{
			foreach (object value in AllValues)
			{
				ZDateTime z = new ZDateTime(value);

				try
				{
					z.Add(new TimeSpan(0));
					Assert(MessageForValue(value) + " should throw InvalidOperationException", ValueIsValid(value));
				}
				catch (InvalidOperationException)
				{
					Assert(MessageForValue(value) + " should not throw InvalidOperationException", !ValueIsValid(value));
				}
			}
		}

		public void TestAddTicksThrowsInvalidOperationExceptionWhenInvalid()
		{
			foreach (object value in AllValues)
			{
				ZDateTime z = new ZDateTime(value);

				try
				{
					z.AddTicks(0);
					Assert(MessageForValue(value) + " should throw InvalidOperationException", ValueIsValid(value));
				}
				catch (InvalidOperationException)
				{
					Assert(MessageForValue(value) + " should not throw InvalidOperationException", !ValueIsValid(value));
				}
			}
		}

		public void TestAddMillisecondsThrowsInvalidOperationExceptionWhenInvalid()
		{
			foreach (object value in AllValues)
			{
				ZDateTime z = new ZDateTime(value);

				try
				{
					z.AddMilliseconds(0);
					Assert(MessageForValue(value) + " should throw InvalidOperationException", ValueIsValid(value));
				}
				catch (InvalidOperationException)
				{
					Assert(MessageForValue(value) + " should not throw InvalidOperationException", !ValueIsValid(value));
				}
			}
		}

		public void TestAddSecondsThrowsInvalidOperationExceptionWhenInvalid()
		{
			foreach (object value in AllValues)
			{
				ZDateTime z = new ZDateTime(value);

				try
				{
					z.AddSeconds(0);
					Assert(MessageForValue(value) + " should throw InvalidOperationException", ValueIsValid(value));
				}
				catch (InvalidOperationException)
				{
					Assert(MessageForValue(value) + " should not throw InvalidOperationException", !ValueIsValid(value));
				}
			}
		}

		public void TestAddMinutesThrowsInvalidOperationExceptionWhenInvalid()
		{
			foreach (object value in AllValues)
			{
				ZDateTime z = new ZDateTime(value);

				try
				{
					z.AddMinutes(0);
					Assert(MessageForValue(value) + " should throw InvalidOperationException", ValueIsValid(value));
				}
				catch (InvalidOperationException)
				{
					Assert(MessageForValue(value) + " should not throw InvalidOperationException", !ValueIsValid(value));
				}
			}
		}

		public void TestAddHoursThrowsInvalidOperationExceptionWhenInvalid()
		{
			foreach (object value in AllValues)
			{
				ZDateTime z = new ZDateTime(value);

				try
				{
					z.AddHours(0);
					Assert(MessageForValue(value) + " should throw InvalidOperationException", ValueIsValid(value));
				}
				catch (InvalidOperationException)
				{
					Assert(MessageForValue(value) + " should not throw InvalidOperationException", !ValueIsValid(value));
				}
			}
		}

		public void TestAddDaysThrowsInvalidOperationExceptionWhenInvalid()
		{
			foreach (object value in AllValues)
			{
				ZDateTime z = new ZDateTime(value);

				try
				{
					z.AddDays(0);
					Assert(MessageForValue(value) + " should throw InvalidOperationException", ValueIsValid(value));
				}
				catch (InvalidOperationException)
				{
					Assert(MessageForValue(value) + " should not throw InvalidOperationException", !ValueIsValid(value));
				}
			}
		}

		public void TestAddMonthsThrowsInvalidOperationExceptionWhenInvalid()
		{
			foreach (object value in AllValues)
			{
				ZDateTime z = new ZDateTime(value);

				try
				{
					z.AddMonths(0);
					Assert(MessageForValue(value) + " should throw InvalidOperationException", ValueIsValid(value));
				}
				catch (InvalidOperationException)
				{
					Assert(MessageForValue(value) + " should not throw InvalidOperationException", !ValueIsValid(value));
				}
			}
		}

		public void TestAddYearsThrowsInvalidOperationExceptionWhenInvalid()
		{
			foreach (object value in AllValues)
			{
				ZDateTime z = new ZDateTime(value);

				try
				{
					z.AddYears(0);
					Assert(MessageForValue(value) + " should throw InvalidOperationException", ValueIsValid(value));
				}
				catch (InvalidOperationException)
				{
					Assert(MessageForValue(value) + " should not throw InvalidOperationException", !ValueIsValid(value));
				}
			}
		}

		public void TestEndOfDay()
		{
			ZDateTime myBirthday = new ZDateTime(2004, 11, 25).EndOfDay();
			AssertEquals("End of Day is at 23:59:59am", myBirthday.Hour, 23);
			AssertEquals("End of Day is at 23:59:59am", myBirthday.Minute, 59);
			AssertEquals("End of Day is at 23:59:59am", myBirthday.Second, 59);

			AssertEquals("Empty Input, Empty Output", ZDateTime.Empty.EndOfDay(), ZDateTime.Empty);
		}

		public void TestDate()
		{
			AssertEquals("Invalid", ZDate.Invalid, ZDateTime.Invalid.Date);
			AssertEquals("Empty", ZDate.Empty, ZDateTime.Empty.Date);
			AssertEquals("Valid", new ZDate(2001, 1, 1), new ZDateTime(2001, 1, 1, 1, 1, 1).Date);
		}

		public void TestToStringUnformatted()
		{
			AssertToString("", null);
			AssertToString("", DBNull.Value);
			AssertToString("<Invalid>", DateTime.MinValue);
			AssertToString("<Invalid>", new DateTime());
			AssertToString("<Invalid>", new DateTime(0));
			AssertToString(DateTime.MinValue.ToString("dd-MMM-yy HH:mm:ss"), new DateTime(1)); // 100 nanoseconds is valid!
			AssertToString(RecentDate.ToString("dd-MMM-yy HH:mm:ss"), RecentDate);
		}

		public void TestToStringFormatted()
		{
			AssertToStringFormatted("", null, null);
			AssertToStringFormatted("", DBNull.Value, null);
			AssertToStringFormatted("<Invalid>", DateTime.MinValue, null);
			AssertToStringFormatted("<Invalid>", new DateTime(), null);
			AssertToStringFormatted("<Invalid>", new DateTime(0), null);
			AssertToStringFormatted(DateTime.MinValue.ToString("dd-MMM-yy HH:mm:ss"), new DateTime(1), null);
			AssertToStringFormatted(RecentDate.ToString("dd-MMM-yy HH:mm:ss"), RecentDate, null);

			string[] formats = new string[]
			{
				"d", "D",
				"f", "F",
				"g", "G",
				"m",
				"r",
				"s",
				"t", "T",
				"u", "U",
				"y",
				"dddd, MMMM dd yyyy",
				"ddd, MMM d \"'\"yy",
				"dddd, MMMM dd",
				"M/yy",
				"dd-MM-yy"
			};

			foreach (string format in formats)
			{
				AssertToStringFormatted(RecentDate.ToString(format), RecentDate, format);
			}
		}

		public void TestToStringDefaultFormatDoesNotUseSystemCulture()
		{
			var initialCulture = CultureInfo.CurrentCulture;
			try
			{
				CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("EN-AU");
				var au = ZDateTime.BrettsBirthday.ToString();
				CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("EN-US");
				var us = ZDateTime.BrettsBirthday.ToString();
				AssertEquals(au, us);
				us = ZDate.BrettsBirthday.ToString(null);
				AssertEquals(au, us);
			}
			finally
			{
				CultureInfo.CurrentCulture = initialCulture;
			}
		}

		public void TestStringFormatDateTimeFormat()
		{
			AssertEquals("Brett was born on " + ZDateTime.BrettsBirthday.ToString(), string.Format("Brett was born on {0}", ZDateTime.BrettsBirthday));
			AssertEquals("Brett was born on " + ZDateTime.BrettsBirthday.ToString(), string.Format(CultureInfo.InvariantCulture, "Brett was born on {0}", ZDateTime.BrettsBirthday));
		}

		public void TestToOleAutomationDate()
		{
			foreach (object validValue in ValidValues)
			{
				if (validValue is DateTime value)
				{
					AssertEquals(MessageForValue(value), value.ToOADate(), new ZDateTime(value).ToOleAutomationDate());
				}
			}
		}

		public void TestToOleAutomationDateWhenInvalid()
		{
			foreach (object value in InvalidValues)
			{
				AssertEquals(MessageForValue(value), 0.0, new ZDateTime(value).ToOleAutomationDate());
			}
		}

		public void TestToOleAutomationDateWhenEmpty()
		{
			foreach (object value in EmptyValues)
			{
				AssertEquals(MessageForValue(value), 0.0, new ZDateTime(value).ToOleAutomationDate());
			}
		}

		#endregion

		#region IFormattable

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestIFormattableToString()
		{
			AssertEquals("Empty DateTime", "", ((IFormattable)ZDateTime.Empty).ToString("dd-MMM-yy", new DateTimeFormatInfo()));
			AssertEquals("Valid DateTime", "25-Dec-04", new ZDateTime(2004, 12, 25).ToString("dd-MMM-yy", new DateTimeFormatInfo()));
			AssertEquals("Invalid DateTime", "", ZDateTime.Invalid.ToString("dd-MMM-yy", new DateTimeFormatInfo()));
		}

		#endregion

		#region XmlSerializableValue

		[TestTimeZone]
		public void TestXmlSerializable()
		{
			ZDateTime localDate = new ZDateTime(2005, 11, 2);
			AssertEquals(DateTimeKind.Local, localDate.Kind);
			AssertZTypeSerializesToXml("<a>2005-11-02T00:00:00+02:00</a>", localDate);

			ZDateTime localDateTime = new ZDateTime(2005, 11, 2, 13, 25, 17, 254);
			AssertEquals(DateTimeKind.Local, localDateTime.Kind);
			AssertZTypeSerializesToXml("<a>2005-11-02T13:25:17.254+02:00</a>", localDateTime);

			ZDateTime utcDate = new ZDateTime(2005, 11, 2, DateTimeKind.Utc);
			AssertEquals(DateTimeKind.Utc, utcDate.Kind);
			AssertZTypeSerializesToXml("<a>2005-11-02T00:00:00Z</a>", utcDate);

			ZDateTime utcDateTime = new ZDateTime(2005, 11, 2, 13, 25, 17, 254, DateTimeKind.Utc);
			AssertEquals(DateTimeKind.Utc, utcDateTime.Kind);
			AssertZTypeSerializesToXml("<a>2005-11-02T13:25:17.254Z</a>", utcDateTime);

			ZDateTime unspecifiedDate = new ZDateTime(2005, 11, 2, DateTimeKind.Unspecified);
			AssertEquals(DateTimeKind.Unspecified, unspecifiedDate.Kind);
			AssertZTypeSerializesToXml("<a>2005-11-02T00:00:00</a>", unspecifiedDate);

			ZDateTime unspecifiedDateTime = new ZDateTime(2005, 11, 2, 13, 25, 17, 254, DateTimeKind.Unspecified);
			AssertEquals(DateTimeKind.Unspecified, unspecifiedDateTime.Kind);
			AssertZTypeSerializesToXml("<a>2005-11-02T13:25:17.254</a>", unspecifiedDateTime);
		}

		#endregion

		public void TestKindFromDateTime()
		{
			DateTime dextersBirthday = new DateTime(1971, 2, 1, 11, 5, 0);
			AssertEquals(DateTimeKind.Unspecified, dextersBirthday.Kind);

			ZDateTime dextersBirthdayDefault = new ZDateTime(dextersBirthday);
			AssertEquals(DateTimeKind.Unspecified, dextersBirthdayDefault.Kind);

			dextersBirthday = new DateTime(1971, 2, 1, 11, 5, 0, DateTimeKind.Local);
			ZDateTime dextersBirthdayLocal = new ZDateTime(dextersBirthday);
			AssertEquals(DateTimeKind.Local, dextersBirthdayLocal.Kind);

			dextersBirthday = new DateTime(1971, 2, 1, 11, 5, 0, DateTimeKind.Utc);
			ZDateTime dextersBirthdayUtc = new ZDateTime(dextersBirthday);
			AssertEquals(DateTimeKind.Utc, dextersBirthdayUtc.Kind);

			dextersBirthday = new DateTime(1971, 2, 1, 11, 5, 0, DateTimeKind.Unspecified);
			ZDateTime dextersBirthdayUnspecified = new ZDateTime(dextersBirthday);
			AssertEquals(DateTimeKind.Unspecified, dextersBirthdayUnspecified.Kind);
		}

		public void TestKindFromDateTimeAndKind()
		{
			DateTime dextersBirthday = new DateTime(1971, 2, 1, 11, 5, 0);
			AssertEquals(DateTimeKind.Unspecified, dextersBirthday.Kind);

			ZDateTime dextersBirthdayLocal = new ZDateTime(dextersBirthday, DateTimeKind.Local);
			AssertEquals(DateTimeKind.Local, dextersBirthdayLocal.Kind);

			ZDateTime dextersBirthdayUtc = new ZDateTime(dextersBirthday, DateTimeKind.Utc);
			AssertEquals(DateTimeKind.Utc, dextersBirthdayUtc.Kind);

			ZDateTime dextersBirthdayUnspecified = new ZDateTime(dextersBirthday, DateTimeKind.Unspecified);
			AssertEquals(DateTimeKind.Unspecified, dextersBirthdayUnspecified.Kind);

			dextersBirthday = new DateTime(1971, 2, 1, 11, 5, 0, DateTimeKind.Local);
			dextersBirthdayLocal = new ZDateTime(dextersBirthday, DateTimeKind.Local);
			AssertEquals(DateTimeKind.Local, dextersBirthdayLocal.Kind);

			dextersBirthday = new DateTime(1971, 2, 1, 11, 5, 0, DateTimeKind.Local);
			dextersBirthdayUtc = new ZDateTime(dextersBirthday, DateTimeKind.Utc);
			AssertEquals(DateTimeKind.Utc, dextersBirthdayUtc.Kind);

			dextersBirthday = new DateTime(1971, 2, 1, 11, 5, 0, DateTimeKind.Local);
			dextersBirthdayUnspecified = new ZDateTime(dextersBirthday, DateTimeKind.Unspecified);
			AssertEquals(DateTimeKind.Unspecified, dextersBirthdayUnspecified.Kind);
		}

		public void TestDifferentMinutes()
		{
			AssertEquals(false, ZDateTime.DifferentMinutes(new ZDateTime(1999, 1, 1, 12, 0, 1), new ZDateTime(1999, 1, 1, 12, 0, 2)));
			AssertEquals(false, ZDateTime.DifferentMinutes(new ZDateTime(1999, 1, 1, 12, 0, 1), new ZDateTime(1999, 1, 1, 12, 0, 31)));
			AssertEquals(false, ZDateTime.DifferentMinutes(new ZDateTime(1999, 1, 1, 12, 0, 14), new ZDateTime(1999, 1, 1, 12, 0, 31)));
			AssertEquals(false, ZDateTime.DifferentMinutes(new ZDateTime(1999, 1, 1, 12, 0, 29), new ZDateTime(1999, 1, 1, 12, 0, 30)));
			AssertEquals(false, ZDateTime.DifferentMinutes(new ZDateTime(1999, 1, 1, 12, 0, 30), new ZDateTime(1999, 1, 1, 12, 0, 31)));
			AssertEquals(true, ZDateTime.DifferentMinutes(new ZDateTime(1999, 1, 1, 12, 0, 59), new ZDateTime(1999, 1, 1, 12, 1, 1)));
			AssertEquals(true, ZDateTime.DifferentMinutes(new ZDateTime(1999, 1, 1, 12, 0, 59, 192), new ZDateTime(1999, 1, 1, 12, 1, 1, 486)));
			AssertEquals(false, ZDateTime.DifferentMinutes(new ZDateTime(1999, 1, 1, 12, 0, 0, 889), new ZDateTime(1999, 1, 1, 12, 0, 0, 240)));
			AssertEquals(false, ZDateTime.DifferentMinutes(new ZDateTime(1999, 1, 1, 12, 0, 0, 120), new ZDateTime(1999, 1, 1, 12, 0, 0, 800)));
			AssertEquals(false, ZDateTime.DifferentMinutes(new ZDateTime(1999, 1, 1, 12, 0, 0, 120), new ZDateTime(1999, 1, 1, 12, 0, 0, 900)));
			AssertEquals(false, ZDateTime.DifferentMinutes(ZDateTime.Empty, ZDateTime.Empty));
		}

		#region IZTypeTest Overrides

		public override void TestToString()
		{
			foreach (var value in ValidValues)
			{
				if (value is DateTime dateTime)
				{
					AssertEquals(MessageForValue(value), dateTime.ToString("dd-MMM-yy HH:mm:ss"), NewZ(value).ToString());
				}
			}
		}

		protected override IZType NewZ(object value)
		{
			return new ZDateTime(value);
		}

		protected override object[] ValidValues
		{
			get { return new object[] { new DateTime(1), new DateTime(100), RecentDate, new ZDateTime(RecentDate) }; }
		}

		protected override object[] InvalidValues
		{
			get { return new object[] { DateTime.MinValue, new DateTime(), new DateTime(0) }; }
		}

		protected override object[] EmptyValues
		{
			get { return new object[] { null, DBNull.Value, ZDateTime.Empty, new ZDateTime() }; }
		}

		protected override object[] UnsupportedValues
		{
			get { return new object[] { new object(), 0, "20030826" }; }
		}

		protected override bool ValueIsValid(object value)
		{
			if (value is IZType type)
			{
				return type.IsValid;
			}
			else
			{
				return ArrayContainsValue(ValidValues, value);
			}
		}

		#endregion

		#region IConvertible Methods

		public void TestIConvertible_GetTypecode()
		{
			var zDatetime = new ZDateTime(2021, 3, 18, 1, 1, 2);
			AssertEquals("ZDateTime type code", TypeCode.Object, ((IConvertible)zDatetime).GetTypeCode());

			var emptyZDateTime = ZDateTime.Empty;
			AssertEquals("ZDateTime Empty type code", TypeCode.Empty, ((IConvertible)emptyZDateTime).GetTypeCode());
		}

		public void TestIConvertible_ConvertToZDateTime()
		{
			var zDatetime = new ZDateTime(2021, 3, 18, 1, 1, 2);

			AssertEquals("ZDateTime to DateTime conversion", zDatetime, ((IConvertible)zDatetime).ToType(typeof(ZDateTime), null));
		}

		public void TestIConvertible_ConvertToZDateTimeOffset()
		{
			var zDatetime = new ZDateTime(2021, 3, 18, 1, 1, 2);
			var zDatetimeOffset = new ZDateTimeOffset(zDatetime);

			AssertEquals("ZDateTime to DateTime conversion", zDatetimeOffset, ((IConvertible)zDatetime).ToType(typeof(ZDateTimeOffset), null));
		}

		public void TestIConvertible_ConvertToZDate()
		{
			var zDatetime = new ZDateTime(2021, 3, 18, 1, 1, 2);

			AssertEquals("ZDateTime to DateTime conversion", new ZDate(2021, 3, 18), ((IConvertible)zDatetime).ToType(typeof(ZDate), null));
		}

		public void TestIConvertible_ConvertToDateTime()
		{
			var zDatetime = new ZDateTime(2021, 3, 18, 1, 1, 2);
			var expected = new DateTime(2021, 3, 18, 1, 1, 2);

			AssertEquals("ZDateTime to DateTime conversion", expected, Convert.ToDateTime(zDatetime));
			AssertEquals("ZDateTime to DateTime conversion", expected, ((IConvertible)zDatetime).ToType(typeof(DateTime), null));
		}

		public void TestIConvertible_ConvertToString()
		{
			var zDatetime = new ZDateTime(2021, 3, 18, 1, 1, 0);
			var expected = "18-Mar-21 01:01:00";

			AssertToString(expected, Convert.ToString(zDatetime));
			AssertToString(expected, ((IConvertible)zDatetime).ToType(typeof(string), null));
		}

		public void TestIConvertible_ConvertToInvalidTypes()
		{
			ZDateTime zDatetime = new ZDateTime(2021, 3, 18, 1, 1, 2);

			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => Convert.ToBoolean(zDatetime));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => Convert.ToChar(zDatetime));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => Convert.ToSByte(zDatetime));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => Convert.ToByte(zDatetime));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => Convert.ToInt16(zDatetime));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => Convert.ToUInt16(zDatetime));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => Convert.ToInt32(zDatetime));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => Convert.ToUInt32(zDatetime));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => Convert.ToInt64(zDatetime));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => Convert.ToUInt64(zDatetime));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => Convert.ToSingle(zDatetime));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => Convert.ToDouble(zDatetime));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => Convert.ToDecimal(zDatetime));
		}

		public void TestIConvertible_ConvertToInvalidTypesUsingToType()
		{
			ZDateTime zDatetime = new ZDateTime(2021, 3, 18, 1, 1, 2);

			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => ((IConvertible)zDatetime).ToType(typeof(bool), null));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => ((IConvertible)zDatetime).ToType(typeof(char), null));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => ((IConvertible)zDatetime).ToType(typeof(sbyte), null));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => ((IConvertible)zDatetime).ToType(typeof(byte), null));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => ((IConvertible)zDatetime).ToType(typeof(short), null));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => ((IConvertible)zDatetime).ToType(typeof(ushort), null));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => ((IConvertible)zDatetime).ToType(typeof(int), null));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => ((IConvertible)zDatetime).ToType(typeof(long), null));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => ((IConvertible)zDatetime).ToType(typeof(ulong), null));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => ((IConvertible)zDatetime).ToType(typeof(float), null));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => ((IConvertible)zDatetime).ToType(typeof(double), null));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => ((IConvertible)zDatetime).ToType(typeof(decimal), null));
		}

		#endregion

		#region Implementation

		protected DateTime RecentDate;
		protected DateTime RecentDateUtc;

		bool IsValidForDbType(SqlDbType sqlDbType, ZDateTime value)
		{
			using (var command = Data.Db.Connection.Command("select @value"))
			{
				command.AddParameter("value", sqlDbType, value.ToDateTime());
				try
				{
					command.ExecuteNonQuery();
				}
				catch (SqlTypeException)
				{
					return false;
				}
				catch (OverflowException)
				{
					return false;
				}
				return true;
			}
		}

		protected override void SetUp()
		{
			RecentDate = new DateTime(2003, 6, 27, 16, 26, 43);
			RecentDateUtc = new DateTime(2003, 6, 27, 16, 26, 43, DateTimeKind.Utc);
		}

		protected void AssertToString(string expected, object value)
		{
			AssertEquals(MessageForValue(value), expected, new ZDateTime(value).ToString());
		}

		protected void AssertToStringFormatted(string expected, object value, string format)
		{
			AssertEquals(MessageForValue(value), expected, new ZDateTime(value).ToString(format));
		}

		protected void AssertOperatorsOnSameValue(object value)
		{
			ZDateTime lhs = new ZDateTime(value);
			string lhsMessage = MessageForValue(lhs) + "  ";
			string rhsMessage = "  itself";

#pragma warning disable 1718
			AssertEquals(lhsMessage + "==" + rhsMessage, true, lhs == lhs);
			AssertEquals(lhsMessage + "!=" + rhsMessage, false, lhs != lhs);
			AssertEquals(lhsMessage + "<" + rhsMessage, false, lhs < lhs);
			AssertEquals(lhsMessage + ">" + rhsMessage, false, lhs > lhs);
			AssertEquals(lhsMessage + "<=" + rhsMessage, true, lhs <= lhs);
			AssertEquals(lhsMessage + ">=" + rhsMessage, true, lhs >= lhs);
#pragma warning restore 1718

			if (value is DateTime dateTimeValue)
			{
				rhsMessage = "  " + MessageForValue(dateTimeValue);

				AssertEquals(lhsMessage + "==" + rhsMessage, true, lhs == dateTimeValue);
				AssertEquals(lhsMessage + "!=" + rhsMessage, false, lhs != dateTimeValue);
				AssertEquals(lhsMessage + "<" + rhsMessage, false, lhs < dateTimeValue);
				AssertEquals(lhsMessage + ">" + rhsMessage, false, lhs > dateTimeValue);
				AssertEquals(lhsMessage + "<=" + rhsMessage, true, lhs <= dateTimeValue);
				AssertEquals(lhsMessage + ">=" + rhsMessage, true, lhs >= dateTimeValue);

				lhsMessage = MessageForValue(dateTimeValue) + "  ";
				rhsMessage = "  " + MessageForValue(lhs);

				AssertEquals(lhsMessage + "==" + rhsMessage, true, dateTimeValue == lhs);
				AssertEquals(lhsMessage + "!=" + rhsMessage, false, dateTimeValue != lhs);
				AssertEquals(lhsMessage + "<" + rhsMessage, false, dateTimeValue < lhs);
				AssertEquals(lhsMessage + ">" + rhsMessage, false, dateTimeValue > lhs);
				AssertEquals(lhsMessage + "<=" + rhsMessage, true, dateTimeValue <= lhs);
				AssertEquals(lhsMessage + ">=" + rhsMessage, true, dateTimeValue >= lhs);
			}
		}

		protected void AssertOperatorsOnDifferentValues(bool expectedLessThan, bool expectedGreaterThan, object value, object otherValue)
		{
			ZDateTime lhs = new ZDateTime(value);
			ZDateTime rhs = new ZDateTime(otherValue);
			string lhsMessage = MessageForValue(lhs) + "  ";
			string rhsMessage = "  " + MessageForValue(rhs);

			AssertEquals(lhsMessage + "==" + rhsMessage, false, lhs == rhs);
			AssertEquals(lhsMessage + "!=" + rhsMessage, true, lhs != rhs);
			AssertEquals(lhsMessage + "<" + rhsMessage, expectedLessThan, lhs < rhs);
			AssertEquals(lhsMessage + ">" + rhsMessage, expectedGreaterThan, lhs > rhs);
			AssertEquals(lhsMessage + "<=" + rhsMessage, expectedLessThan, lhs <= rhs);
			AssertEquals(lhsMessage + ">=" + rhsMessage, expectedGreaterThan, lhs >= rhs);

			if (otherValue is DateTime)
			{
				DateTime dateTimeValue = (DateTime)otherValue;
				rhsMessage = "  " + MessageForValue(dateTimeValue);

				AssertEquals(lhsMessage + "==" + rhsMessage, false, lhs == dateTimeValue);
				AssertEquals(lhsMessage + "!=" + rhsMessage, true, lhs != dateTimeValue);
				AssertEquals(lhsMessage + "<" + rhsMessage, expectedLessThan, lhs < dateTimeValue);
				AssertEquals(lhsMessage + ">" + rhsMessage, expectedGreaterThan, lhs > dateTimeValue);
				AssertEquals(lhsMessage + "<=" + rhsMessage, expectedLessThan, lhs <= dateTimeValue);
				AssertEquals(lhsMessage + ">=" + rhsMessage, expectedGreaterThan, lhs >= dateTimeValue);
			}

			if (value is DateTime)
			{
				DateTime dateTimeValue = (DateTime)value;
				lhsMessage = MessageForValue(dateTimeValue) + "  ";
				rhsMessage = "  " + MessageForValue(rhs);

				AssertEquals(lhsMessage + "==" + rhsMessage, false, dateTimeValue == rhs);
				AssertEquals(lhsMessage + "!=" + rhsMessage, true, dateTimeValue != rhs);
				AssertEquals(lhsMessage + "<" + rhsMessage, expectedLessThan, dateTimeValue < rhs);
				AssertEquals(lhsMessage + ">" + rhsMessage, expectedGreaterThan, dateTimeValue > rhs);
				AssertEquals(lhsMessage + "<=" + rhsMessage, expectedLessThan, dateTimeValue <= rhs);
				AssertEquals(lhsMessage + ">=" + rhsMessage, expectedGreaterThan, dateTimeValue >= rhs);
			}
		}

		#endregion
	}
}
