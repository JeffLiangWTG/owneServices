using System;
using NUnit.Framework;

namespace CargoWise.Types.Tests
{
	public class ZDateTest : IZTypeTest
	{
		#region Constructors

		public void TestImplicitZDateTimeOperator()
		{
			ZDateTime dateTime = new ZDate(2000, 1, 2);
			AssertEquals(dateTime.Year, 2000);
			AssertEquals(dateTime.Month, 1);
			AssertEquals(dateTime.Day, 2);
			AssertEquals(dateTime.TimeOfDay.Ticks, 0);
		}

		public void TestConstructorWithObjectValue()
		{
			AssertNoExceptionThrown(() => new ZDate(null, ZDateTime.DefaultKind));
			AssertNoExceptionThrown(() => new ZDate(ZDateTime.Today, ZDateTime.DefaultKind));
			AssertNoExceptionThrown(() => new ZDate(ZDate.Today, ZDateTime.DefaultKind));
		}
		#endregion

		#region Static

		public void TestEmpty()
		{
			AssertEquals("IsEmpty", true, ZDate.Empty.IsEmpty);
			AssertEquals("IsValid", false, ZDate.Empty.IsValid);
			Assert("Equals", ZDate.Empty.Equals(ZDate.Empty));
		}

		public void TestInvalid()
		{
			AssertEquals("IsEmpty", false, ZDate.Invalid.IsEmpty);
			AssertEquals("IsValid", false, ZDate.Invalid.IsValid);
			Assert("Equals", ZDate.Invalid.Equals(ZDate.Invalid));
		}

		public void TestToday()
		{
			AssertEquals("Today.Year", ZDateTime.Now.Year, ZDate.Today.Year);
			AssertEquals("Today.Month", ZDateTime.Now.Month, ZDate.Today.Month);
			AssertEquals("Today.Day", ZDateTime.Now.Day, ZDate.Today.Day);
		}

		#endregion

		public void TestOverlaps()
		{
			AssertOverlap(new ZDateTime(2005, 2, 2), new ZDateTime(2010, 2, 2), new ZDateTime(2000, 2, 2), new ZDateTime(2005, 2, 1), false);
			AssertOverlap(new ZDateTime(2005, 2, 2), new ZDateTime(2010, 2, 2), new ZDateTime(2000, 2, 2), new ZDateTime(2005, 2, 2), true);
			AssertOverlap(new ZDateTime(2005, 2, 2), new ZDateTime(2010, 2, 2), new ZDateTime(2000, 2, 2), new ZDateTime(2010, 2, 2), true);
			AssertOverlap(new ZDateTime(2005, 2, 2), new ZDateTime(2010, 2, 2), new ZDateTime(2000, 2, 2), new ZDateTime(2015, 2, 2), true);
			AssertOverlap(new ZDateTime(2005, 2, 2), new ZDateTime(2010, 2, 2), new ZDateTime(2006, 2, 2), new ZDateTime(2007, 2, 2), true);
			AssertOverlap(new ZDateTime(2005, 2, 2), new ZDateTime(2010, 2, 2), new ZDateTime(2006, 2, 2), new ZDateTime(2015, 2, 2), true);
			AssertOverlap(new ZDateTime(2005, 2, 2), new ZDateTime(2010, 2, 2), new ZDateTime(2010, 2, 2), new ZDateTime(2015, 1, 1), true);
			AssertOverlap(new ZDateTime(2005, 2, 2), new ZDateTime(2010, 2, 2), new ZDateTime(2010, 2, 3), new ZDateTime(2015, 1, 1), false);

			AssertOverlap(new ZDateTime(2005, 2, 2), new ZDateTime(2010, 2, 2), new ZDateTime(2000, 2, 2), ZDateTime.Empty, true);
			AssertOverlap(new ZDateTime(2005, 2, 2), new ZDateTime(2010, 2, 2), new ZDateTime(2006, 2, 2), ZDateTime.Empty, true);
			AssertOverlap(new ZDateTime(2005, 2, 2), new ZDateTime(2010, 2, 2), new ZDateTime(2010, 2, 2), ZDateTime.Empty, true);
			AssertOverlap(new ZDateTime(2005, 2, 2), new ZDateTime(2010, 2, 2), new ZDateTime(2010, 2, 3), ZDateTime.Empty, false);

			AssertOverlap(new ZDateTime(2005, 2, 2), new ZDateTime(2010, 2, 2), ZDateTime.Empty, new ZDateTime(2005, 2, 1), false);
			AssertOverlap(new ZDateTime(2005, 2, 2), new ZDateTime(2010, 2, 2), ZDateTime.Empty, new ZDateTime(2005, 2, 2), true);
			AssertOverlap(new ZDateTime(2005, 2, 2), new ZDateTime(2010, 2, 2), ZDateTime.Empty, new ZDateTime(2015, 2, 2), true);

			AssertOverlap(ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, true);
			AssertOverlap(ZDateTime.Empty, ZDateTime.Empty, new ZDateTime(2005, 2, 2), new ZDateTime(2010, 2, 2), true);
			AssertOverlap(ZDateTime.Empty, new ZDateTime(2005, 2, 2), new ZDateTime(2005, 2, 2), ZDateTime.Empty, true);
			AssertOverlap(ZDateTime.Empty, new ZDateTime(2005, 2, 2), new ZDateTime(2005, 2, 3), ZDateTime.Empty, false);
		}

		void AssertOverlap(ZDateTime aStart, ZDateTime aEnd, ZDateTime bStart, ZDateTime bEnd, bool overlaps)
		{
			AssertEquals(aStart.ToShortDateString() + " " + aEnd.ToShortDateString() + " " + bStart.ToShortDateString() + " " + bEnd.ToShortDateString() + " ", overlaps, ZDateTime.Overlaps(aStart, aEnd, bStart, bEnd));
			AssertEquals(bStart.ToShortDateString() + " " + bEnd.ToShortDateString() + " " + aStart.ToShortDateString() + " " + aEnd.ToShortDateString() + " ", overlaps, ZDateTime.Overlaps(bStart, bEnd, aStart, aEnd));
		}

		public void TestTruncateSeconds()
		{
			var testDate_WithSeconds = new ZDateTime(2015, 06, 16, 3, 59, 10, 10);
			var testDate_WithoutSeconds = new ZDateTime(2015, 06, 16, 3, 59, 0, 0);

			AssertEquals("Should truncate seconds and milliseconds", testDate_WithoutSeconds, ZDateTime.TruncateSeconds(testDate_WithSeconds));
		}

		public void TestTruncateToDay()
		{
			var testDate_WithSeconds = new ZDateTime(2015, 06, 16, 3, 59, 10, 10);
			var testDate_ToDay = new ZDateTime(2015, 06, 16, 0, 0, 0, 0);

			AssertEquals("Should truncate hours, minutes, seconds and milliseconds", testDate_ToDay, ZDateTime.TruncateToDay(testDate_WithSeconds));
		}

		public void TestImplicitZDateZDateTimeConversion()
		{
			ZDate brettsBirthday = new ZDate(1971, 9, 18);
			ZDateTime brettsBirthTime = brettsBirthday;
			AssertEquals(1971, brettsBirthTime.Year);
			AssertEquals(9, brettsBirthTime.Month);
			AssertEquals(18, brettsBirthTime.Day);
			AssertEquals(new TimeSpan(0, 0, 0), brettsBirthTime.TimeOfDay);
		}

		public void TestToZDateTime()
		{
			var zDate = new ZDate(1971, 9, 18);
			var zDateTime = zDate.ToZDateTime();
			AssertEquals(1971, zDateTime.Year);
			AssertEquals(9, zDateTime.Month);
			AssertEquals(18, zDateTime.Day);
			AssertEquals(TimeSpan.Zero, zDateTime.TimeOfDay);
		}

		public void TestTryParseJulianDate()
		{
			Assert("Parsing 71001", ZDate.TryParseJulianDate("", out var date));
			AssertEquals("Parsing empty string", ZDate.Empty, date);
			Assert("Parsing 71001", ZDate.TryParseJulianDate(null, out date));
			AssertEquals("Parsing null string", ZDate.Empty, date);
			Assert("Parsing 71261", ZDate.TryParseJulianDate("71261", out date));
			AssertEquals("Parsing 71261", ZDate.BrettsBirthday, date);
			Assert("Parsing 71001", ZDate.TryParseJulianDate("71001", out date));
			AssertEquals("Parsing 71001", new ZDate(1971, 1, 1), date);
			Assert("Parsing invalid string", !ZDate.TryParseJulianDate("AD322", out date));
			AssertEquals("Parsing invalid string", ZDate.Invalid, date);
			Assert("Parsing invalid long string", !ZDate.TryParseJulianDate("710011", out date));
			AssertEquals("Parsing invalid long string", ZDate.Invalid, date);
		}

		public void TestToJulianDateString()
		{
			AssertEquals("Brett's Birthday in Julian Format", "71261", ZDate.BrettsBirthday.ToJulianDateString());
			AssertEquals("ZDate(1971, 9, 18) in Julian Format", "71001", new ZDate(1971, 1, 1).ToJulianDateString());
			AssertEquals("ZDate.Empty in Julian Format", "", ZDate.Empty.ToJulianDateString());
			AssertEquals("ZDate.Invalid in Julian Format", "<Invalid>", ZDate.Invalid.ToJulianDateString());
		}

		public void ToISO8601ShortDateString()
		{
			ZDate testDate = new ZDate(2004, 10, 12);
			AssertEquals("2004-10-12", testDate.ToISO8601ShortDateString());
			AssertEquals("ZDate.Empty in ISO8601ShortDateString Format", "", ZDate.Empty.ToISO8601ShortDateString());
			AssertEquals("ZDate.Invalid in ISO8601ShortDateString Format", "<Invalid>", ZDate.Invalid.ToISO8601ShortDateString());
		}

		#region Object Overrides

		public void TestEqualsOnSameDay()
		{
			Assert("ZDates should equal when on same day", (ZDate)new ZDateTime(2005, 1, 2) == (ZDate)new ZDateTime(2005, 1, 2));
		}

#pragma warning disable 1718
		public void TestEmptyEquals()
		{
			Assert("Empty DateTimes Equal", ZDate.Empty == ZDate.Empty);
		}

		public void TestInvalidEquals()
		{
			Assert("Invalid DateTimes Equal", ZDate.Invalid == ZDate.Invalid);
		}
#pragma warning restore 1718

		public void TestEmptyInvalidCombinationEquals()
		{
			Assert("Empty Not Equals Invalid", ZDate.Invalid != ZDate.Empty);
		}

		#endregion

		#region Casting

		public void TestExplicitCastFromZDateToDateTime()
		{
			ZDate date = (ZDate)new DateTime(2005, 1, 2);
			AssertEquals("Constructing a date from a DateTime should only include the date portion", new DateTime(2005, 1, 2), date.ToDateTime());
		}

		public void TestExplicitCastFromZDateToZDateTime()
		{
			ZDate date = (ZDate)new ZDateTime(2005, 1, 2);
			AssertEquals("Constructing a date from a DateTime should only include the date portion", new DateTime(2005, 1, 2), date.ToDateTime());
		}

		public void TestConversionFromZDateTime_NoDayTicks_Max()
		{
			var dateTime = new ZDateTime(DateTime.MaxValue.Ticks);
			var date = (ZDate)dateTime;
			AssertEquals("The hour, minutes and seconds ticks should all be zeros", DateTime.MaxValue.Ticks / TimeSpan.TicksPerDay * TimeSpan.TicksPerDay, date.ToDateTime().Ticks);
		}

		public void TestConversionFromZDateTime_NoDayTicks_WithSeconds()
		{
			var dateTime = new ZDateTime(2014, 06, 03, 13, 20, 22);
			var date = (ZDate)dateTime;
			AssertEquals("The hour, minutes and seconds ticks should all be zeros", dateTime.Ticks / TimeSpan.TicksPerDay * TimeSpan.TicksPerDay, date.ToDateTime().Ticks);
			AssertEquals("Constructing a date from a dateTime should only include the date portion", new DateTime(2014, 06, 03), date.ToDateTime());
		}

		public void TestSetZDateToEmptyFromZDateTime()
		{
			ZDate date = (ZDate)ZDateTime.Empty;
			AssertEquals("IsEmpty", true, date.IsEmpty);
		}

		public void TestSetZDateToInvalidFromZDateTime()
		{
			ZDate date = (ZDate)ZDateTime.Invalid;
			AssertEquals("not IsValid", false, date.IsValid);
		}

		public void TestExplicitCastFromZDateTimeToZDate()
		{
			ZDateTime dateTime = (ZDateTime)new ZDate(2005, 2, 3);
			AssertEquals("Should explicitly cast from ZDate to ZDateTime", new ZDateTime(2005, 2, 3), dateTime);
		}

		public void TestEqualsImplicit()
		{
			Assert(new ZDateTime(1971, 9, 18) == new ZDate(1971, 9, 18));
			Assert(new ZDate(1971, 9, 18) == new ZDate(1971, 9, 18));
			Assert(new ZDateTime(1971, 9, 18) == new ZDateTime(1971, 9, 18));
			Assert(new ZDate(1971, 9, 18) == new ZDateTime(1971, 9, 18));
		}

		public void TestGreaterThanImplicit()
		{
			Assert(new ZDateTime(1971, 9, 19) > new ZDate(1971, 9, 18));
			Assert(new ZDate(1971, 9, 19) > new ZDate(1971, 9, 18));
			Assert(new ZDateTime(1971, 9, 19) > new ZDateTime(1971, 9, 18));
			Assert(new ZDate(1971, 9, 19) > new ZDateTime(1971, 9, 18));
		}

		public void TestLessThanImplicit()
		{
			Assert(new ZDateTime(1971, 9, 18) < new ZDate(1971, 9, 19));
			Assert(new ZDate(1971, 9, 18) < new ZDate(1971, 9, 19));
			Assert(new ZDateTime(1971, 9, 18) < new ZDateTime(1971, 9, 19));
			Assert(new ZDate(1971, 9, 18) < new ZDateTime(1971, 9, 19));
		}

		public void TestSetZDateTimeToEmptyFromZDate()
		{
			ZDateTime date = ZDateTime.Empty;
			AssertEquals("IsEmpty", true, date.IsEmpty);
		}

		public void TestSetZDateTimeToInvalidFromZDate()
		{
			ZDateTime date = ZDateTime.Invalid;
			AssertEquals("not IsValid", false, date.IsValid);
		}

		public void TestSetToEmpty()
		{
			ZDate date = ZDate.Empty;
			AssertEquals("IsEmpty", true, date.IsEmpty);
		}

		public void TestSetToInvalid()
		{
			ZDate date = ZDate.Invalid;
			AssertEquals("IsEmpty", false, date.IsValid);
		}

		#endregion

		#region Operator Overloads

		public void TestSubtractOperator()
		{
			ZDate testDate = new ZDate(2004, 1, 25);
			ZDate fiveDaysLaterDate = new ZDate(2004, 1, 30);

			TimeSpan difference = new TimeSpan(5, 0, 0, 0);

			AssertEquals(difference, fiveDaysLaterDate - testDate);
		}

		[ExpectException(typeof(OperationOnInvalidZDateTimeException))]
		public void TestSubstractOperatorWithInvalid()
		{
			ZDate testDate = new ZDate(2004, 1, 24);
			TimeSpan result = testDate - ZDate.Invalid;
		}

		[ExpectException(typeof(OperationOnInvalidZDateTimeException))]
		public void TestSubstractOperatorWithEmpty()
		{
			ZDate testDate = new ZDate(2004, 1, 24);
			TimeSpan result = ZDate.Empty - testDate;
		}

		public void TestOperatorsOnSameValue()
		{
			AssertOperatorsOnSameValue(null);
			AssertOperatorsOnSameValue(DBNull.Value);
			AssertOperatorsOnSameValue(DateTime.MinValue);
			AssertOperatorsOnSameValue(new DateTime());
			AssertOperatorsOnSameValue(new DateTime(0));
			AssertOperatorsOnSameValue(new DateTime(1, 1, 1));
			AssertOperatorsOnSameValue(RecentDate);
		}

		public void TestOperatorsOnDifferentValues()
		{
			AssertOperatorsOnDifferentValues(false, false, null, RecentDate);
			AssertOperatorsOnDifferentValues(false, false, RecentDate, null);
			AssertOperatorsOnDifferentValues(false, false, DateTime.MinValue, RecentDate);
			AssertOperatorsOnDifferentValues(false, false, RecentDate, DateTime.MinValue);
			AssertOperatorsOnDifferentValues(true, false, RecentDate, RecentDate.AddDays(1));
			AssertOperatorsOnDifferentValues(false, true, RecentDate.AddDays(1), RecentDate);
		}

		#endregion

		#region Wrapping the internal DateTime

		public void TestYear()
		{
			AssertEquals(2005, new ZDate(2005, 1, 2).Year);
		}

		public void TestMonth()
		{
			AssertEquals(1, new ZDate(2005, 1, 2).Month);
		}

		public void TestDay()
		{
			AssertEquals(2, new ZDate(2005, 1, 2).Day);
		}

		public void TestDayOfWeek()
		{
			AssertEquals(DayOfWeek.Tuesday, new ZDate(2006, 2, 28).DayOfWeek);
		}

		public void TestDayOfYear()
		{
			AssertEquals(59, new ZDate(2006, 2, 28).DayOfYear);
		}

		public void TestAdd()
		{
			ZDate today = ZDate.Today;
			TimeSpan span = new TimeSpan(2, 36, 47);
			ZDateTime actual = today.Add(span);
			AssertEquals("Hour", 2, actual.Hour);
			AssertEquals("Minute", 36, actual.Minute);
			AssertEquals("Second", 47, actual.Second);
		}

		public void TestAddHours()
		{
			ZDate today = ZDate.Today;
			AssertEquals(3, today.AddHours(3).Hour);
		}

		public void TestAddMinutes()
		{
			ZDate today = ZDate.Today;
			AssertEquals(3, today.AddMinutes(3).Minute);
		}

		public void TestAddDays()
		{
			AssertEquals(new ZDate(2007, 1, 10), new ZDate(2006, 12, 31).AddDays(10));
		}

		public void TestAddMonths()
		{
			AssertEquals(new ZDate(2007, 1, 1), new ZDate(2006, 12, 1).AddMonths(1));
		}

		public void TestAddYears()
		{
			AssertEquals(new ZDate(2007, 1, 2), new ZDate(2006, 1, 2).AddYears(1));
		}

		#endregion

		#region XmlSerializableValue

		public void TestXmlSerializableFormat()
		{
			AssertZTypeSerializesToXml("<a>2005-11-02</a>", new ZDate(2005, 11, 2));
		}

		#endregion

		#region IFormattable

		public void TestToStringUnformatted()
		{
			AssertToString("", null);
			AssertToString("", DBNull.Value);
			AssertToString("<Invalid>", DateTime.MinValue);
			AssertToString("<Invalid>", new DateTime());
			AssertToString("<Invalid>", new DateTime(0));
			AssertToString(new DateTime(1, 1, 2).ToString(ZDateTime.ShortDateFormat), new DateTime(1, 1, 2));
			AssertToString(RecentDate.ToString(ZDateTime.ShortDateFormat), RecentDate);
		}

		public void TestToStringFormatted()
		{
			AssertToStringFormatted("", null, null);
			AssertToStringFormatted("", DBNull.Value, null);
			AssertToStringFormatted("<Invalid>", DateTime.MinValue, null);
			AssertToStringFormatted("<Invalid>", new DateTime(), null);
			AssertToStringFormatted("<Invalid>", new DateTime(0), null);
			AssertToStringFormatted(new DateTime(1, 1, 2).ToString("dd-MMM-yy HH:mm:ss"), new DateTime(1, 1, 2), null);
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestIFormattableToString()
		{
			AssertEquals("Empty DateTime", "", ((IFormattable)ZDate.Empty).ToString("dd-MMM-yy", new System.Globalization.DateTimeFormatInfo()));
			AssertEquals("Valid DateTime", "25-Dec-04", new ZDate(2004, 12, 25).ToString("dd-MMM-yy", new System.Globalization.DateTimeFormatInfo()));
			AssertEquals("Invalid DateTime", "", ((IFormattable)ZDate.Invalid).ToString("dd-MMM-yy", new System.Globalization.DateTimeFormatInfo()));
		}

		#endregion

		#region IZTypeTest Overrides

		public override void TestToString()
		{
			foreach (object value in ValidValues)
			{
				if (value is DateTime)
				{
					AssertEquals(MessageForValue(value), ((DateTime)value).ToString(ZDateTime.ShortDateFormat), NewZDate(value).ToString());
				}
			}
		}

		protected override IZType NewZ(object value)
		{
			ZDateTime dateTime = new ZDateTime(value);
			return dateTime.Date;
		}

		protected override object[] ValidValues
		{
			get { return new object[] { new DateTime(1, 1, 2), RecentDate, new ZDate(RecentDate.Year, RecentDate.Month, RecentDate.Day) }; }
		}

		protected override object[] InvalidValues
		{
			get { return new object[] { DateTime.MinValue, new DateTime(), new DateTime(0) }; }
		}

		protected override object[] EmptyValues
		{
			get { return new object[] { null, DBNull.Value, ZDate.Empty, new ZDate() }; }
		}

		protected override object[] UnsupportedValues
		{
			get { return new object[] { new object(), 0, "20030826" }; }
		}

		protected override bool ValueIsValid(object value)
		{
			if (value is IZType)
			{
				return ((IZType)value).IsValid;
			}
			else
			{
				return ArrayContainsValue(ValidValues, value);
			}
		}

		#endregion

		#region Implementation

		protected DateTime RecentDate;

		ZDate NewZDate(object value)
		{
			return (ZDate)NewZ(value);
		}

		protected override void SetUp()
		{
			RecentDate = new DateTime(2003, 6, 27);
		}

		protected void AssertToString(string expected, object value)
		{
			AssertEquals(MessageForValue(value), expected, NewZDate(value).ToString());
		}

		protected void AssertToStringFormatted(string expected, object value, string format)
		{
			AssertEquals(MessageForValue(value), expected, NewZDate(value).ToString(format));
		}

		protected void AssertOperatorsOnSameValue(object value)
		{
			ZDate lhs = NewZDate(value);
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
		}

		protected void AssertOperatorsOnDifferentValues(bool expectedLessThan, bool expectedGreaterThan, object value, object otherValue)
		{
			ZDate lhs = NewZDate(value);
			ZDate rhs = NewZDate(otherValue);
			string lhsMessage = MessageForValue(lhs) + "  ";
			string rhsMessage = "  " + MessageForValue(rhs);

			AssertEquals(lhsMessage + "==" + rhsMessage, false, lhs == rhs);
			AssertEquals(lhsMessage + "!=" + rhsMessage, true, lhs != rhs);
			AssertEquals(lhsMessage + "<" + rhsMessage, expectedLessThan, lhs < rhs);
			AssertEquals(lhsMessage + ">" + rhsMessage, expectedGreaterThan, lhs > rhs);
			AssertEquals(lhsMessage + "<=" + rhsMessage, expectedLessThan, lhs <= rhs);
			AssertEquals(lhsMessage + ">=" + rhsMessage, expectedGreaterThan, lhs >= rhs);
		}

		#endregion
	}
}
