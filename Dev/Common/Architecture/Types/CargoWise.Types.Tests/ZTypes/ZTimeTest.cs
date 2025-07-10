
namespace CargoWise.Types.Tests
{
	using System;
	using NUnit.Framework;

	public class ZTimeTest : IZTypeTest
	{
		#region Constructors

		public void TestSecondConstructor()
		{
			ZTime testZTime = new ZTime(4, 2);
			AssertEquals("Hour", 4, testZTime.Hour);
			AssertEquals("Minute", 2, testZTime.Minute);
		}

		public void TestTicksConstructor()
		{
			var ticks = new DateTime(1900, 1, 1, 4, 2, 10, 0).Ticks;
			ZTime testZTime = new ZTime(ticks);
			AssertEquals("Hour", 4, testZTime.Hour);
			AssertEquals("Minute", 2, testZTime.Minute);
		}

		public void TestNullConstructor()
		{
			AssertEquals(ZTime.Empty, new ZTime(null));
		}

		#endregion

		#region Static

		public void TestEmpty()
		{
			AssertEquals("IsEmpty", true, ZTime.Empty.IsEmpty);
			AssertEquals("IsValid", false, ZTime.Empty.IsValid);
			Assert("Equals", ZTime.Empty.Equals(ZTime.Empty));
		}

		public void TestInvalid()
		{
			AssertEquals("IsEmpty", false, ZTime.Invalid.IsEmpty);
			AssertEquals("IsValid", false, ZTime.Invalid.IsValid);
			Assert("Equals", ZTime.Invalid.Equals(ZTime.Invalid));
		}

		public void TestFromSqlFormat()
		{
			ZTime myBirthtime = new ZTime(12, 30);
			ZString myBirthtimeInSqlFormat = myBirthtime.SqlFormat;
			ZTime myBirthtimeAgain = ZTime.FromSqlFormat(myBirthtimeInSqlFormat);
			AssertEquals("Value after passing both FromSqlFormat & SqlFormat", myBirthtime, myBirthtimeAgain);
		}

		#endregion

		#region Object Overrides

		public void TestEqualsForSlightlyDifferentTimes()
		{
			AssertEquals(false, RecentDate.Equals(new ZTime(RecentDate.AddMilliseconds(1))));
		}

		#endregion

		#region Casting

		public void TestSetToEmpty()
		{
			ZTime myBirthDay = ZTime.Empty;
			AssertEquals("IsEmpty", myBirthDay.IsEmpty, true);
		}

		#endregion

		#region Operator Overloads

		public void TestEmptyEquals()
		{
			ZTime dateTime1 = ZTime.Empty;
			ZTime dateTime2 = ZTime.Empty;

			Assert("Empty DateTimes Equal", dateTime1 == dateTime2);
		}

		public void TestInvalidEquals()
		{
			ZTime dateTime1 = ZTime.Invalid;
			ZTime dateTime2 = ZTime.Invalid;

			Assert("Invalid DateTimes Equal", dateTime1 == dateTime2);
		}

		public void TestEmptyInvalidCombinationEquals()
		{
			ZTime dateTime1 = ZTime.Invalid;
			ZTime dateTime2 = ZTime.Empty;

			Assert("Empty Not Equals Invalid", dateTime1 != dateTime2);
		}

		public void TestEqualsOperatorWithDifferentKind()
		{
			//Kind property is ignored for datetime comparison
			AssertEquals(RecentDate, RecentDateUtc);
		}

		public void TestSubtractOperator()
		{
			ZTime testTime = new ZTime(14, 13);
			ZTime fiveMinutesLaterTime = new ZTime(14, 18);
			TimeSpan difference = new TimeSpan(0, 5, 0);
			AssertEquals(difference, fiveMinutesLaterTime - testTime);
		}

		[ExpectException(typeof(OperationOnInvalidZTimeException))]
		public void TestSubstractOperatorWithInvalid()
		{
			ZTime testDate = new ZTime(14, 13);
			TimeSpan result = testDate - ZTime.Invalid;
		}

		[ExpectException(typeof(OperationOnInvalidZTimeException))]
		public void TestSubstractOperatorWithEmpty()
		{
			ZTime testDate = new ZTime(14, 13);
			TimeSpan result = ZTime.Empty - testDate;
		}

		public void TestAddTimeSpanOperator()
		{
			ZTime date = new ZTime(11, 59) + new TimeSpan(0, 1, 0);
			AssertEquals("ZTime + TimeSpan = PM!", new ZTime(12, 0), date);
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
			AssertOperatorsOnDifferentValues(true, false, DateTime.MinValue, RecentDate);
			AssertOperatorsOnDifferentValues(false, true, RecentDate, DateTime.MinValue);
			AssertOperatorsOnDifferentValues(true, false, RecentDate, RecentDate.AddMinutes(1));
			AssertOperatorsOnDifferentValues(false, true, RecentDate.AddMinutes(1), RecentDate);
		}

		#endregion

		#region TryParse

		public void TestTryParseExact()
		{
			bool success = ZTime.TryParseExact("Some Stuff (c) David James", out var parsedTime, "ddMMyy");
			AssertEquals(false, success);
			AssertEquals(ZTime.Invalid, parsedTime);

			success = ZTime.TryParseExact("09:30:47", out parsedTime, "adfdasfsafasdfa");
			AssertEquals(false, success);
			AssertEquals(ZTime.Invalid, parsedTime);

			success = ZTime.TryParseExact("082501", out parsedTime, "hhmmss");
			AssertEquals(true, success);
			AssertEquals(new ZTime(8, 25), parsedTime);

			success = ZTime.TryParseExact("25/01/08", out parsedTime, "ss'/'mm'/'hh");
			AssertEquals(true, success);
			AssertEquals(new ZTime(8, 01), parsedTime);

			success = ZTime.TryParseExact("25-08-01", out parsedTime, "ss'-'hh'-'mm");
			AssertEquals(true, success);
			AssertEquals(new ZTime(8, 01), parsedTime);

			success = ZTime.TryParseExact("", out parsedTime, "hh'-'mm'-'ss");
			AssertEquals(true, success);
			AssertEquals(ZTime.Empty, parsedTime);
		}

		#endregion

		public void TestCastToZTimeFromTimeSpan()
		{
			AssertEquals(new ZTime(8, 0), (ZTime)TimeSpan.FromHours(8));
		}

		public void TestToTimeSpan()
		{
			AssertEquals(TimeSpan.FromHours(8), new ZTime(8, 0).ToTimeSpan());
			AssertEquals(TimeSpan.FromHours(8), new ZTime(8, 0).ToTimeSpan());
		}

		#region Wrapping the internal DateTime

		public void TestToShortTimeString()
		{
			ZTime testDateTime = new ZTime(12, 34);
			AssertEquals("12:34", testDateTime.ToString());
		}

		public void TestAddTimeComponents()
		{
			foreach (DateTime value in new DateTime[] { new DateTime(1900, 1, 1, 3, 1, 0), new DateTime(1900, 1, 1, 12, 0, 0) })
			{
				foreach (int addend in new int[] { -1, 0, 1 })
				{
					string message = MessageForValue(value) + " + " + addend + " ";
					ZTime z = new ZTime(value);
					TimeSpan ticks = new TimeSpan(addend);

					AssertEquals(message + "time span", new ZTime(value.Add(ticks)), z.Add(ticks).TimeOfDay);
					AssertEquals(message + "minutes", new ZTime(value.AddMinutes(addend)), z.AddMinutes(addend).TimeOfDay);
					AssertEquals(message + "hours", new ZTime(value.AddHours(addend)), z.AddHours(addend).TimeOfDay);
				}
			}
		}

		public void TestAddHoursWithZDecimal()
		{
			foreach (DateTime value in new DateTime[] { new DateTime(2, 1, 1, 1, 2, 4), RecentDate })
			{
				foreach (ZDecimal addend in new ZDecimal[] { -1.00m, -0.75m, -0.50m, -0.25m, 0, 0.25m, 0.75m, 0.50m, 1.00m })
				{
					string message = MessageForValue(value) + " + " + addend + " ";
					ZTime z = new ZTime(value);
					decimal decimalAddend = (decimal)addend;

					AssertEquals(message + "hours", new ZTime(value.AddHours((double)decimalAddend)), z.AddHours(addend));
				}
			}
		}

		public void TestTimeComponents()
		{
			foreach (object validValue in ValidValues)
			{
				if (validValue is DateTime value)
				{
					string message = MessageForValue(value) + " ";
					ZTime z = new ZTime(value);

					AssertEquals(message + "Ticks", new TimeSpan(value.Hour, value.Minute, 0).Ticks, z.Ticks);
					AssertEquals(message + "TimeOfDay", new TimeSpan(value.Hour, value.Minute, 0), z.TimeOfDay);
					AssertEquals(message + "Minute", value.Minute, z.Minute);
					AssertEquals(message + "Hour", value.Hour, z.Hour);
				}
			}
		}

		public void TestShouldThrowCorrectInvalidOperationExceptionMessage_WhenAddInvalidDays_FixingIssue00917305()
		{
			DateTime value = new DateTime(2000, 1, 1);
			ZTime z = new ZTime(value);
			AssertExceptionThrown("Expect should throw correct message in EnsureValidValueFor().",
				typeof(InvalidOperationException), "Cannot add hours to an invalid time.",
				() => z.AddHours(0));
		}

		public void TestAddThrowsInvalidOperationExceptionWhenInvalid()
		{
			foreach (object value in AllValues)
			{
				ZTime z = new ZTime(value);

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

		public void TestAddMinutesThrowsInvalidOperationExceptionWhenInvalid()
		{
			foreach (object value in AllValues)
			{
				ZTime z = new ZTime(value);

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
				ZTime z = new ZTime(value);

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

		public void TestToStringUnformatted()
		{
			AssertToString("", null);
			AssertToString("", DBNull.Value);
			AssertToString("00:00", DateTime.MinValue);
			AssertToString("00:00", new DateTime());
			AssertToString("00:00", new DateTime(0));
			AssertToString(DateTime.MinValue.ToString("HH\\:mm"), new DateTime(1)); // 100 nanoseconds is valid!
			AssertToString(RecentDate.ToString("HH\\:mm"), RecentDate);
		}

		public void TestToStringFormatted()
		{
			AssertToStringFormatted("", null, null);
			AssertToStringFormatted("", DBNull.Value, null);
			AssertToStringFormatted("00:00", DateTime.MinValue, null);
			AssertToStringFormatted("00:00", new DateTime(), null);
			AssertToStringFormatted("00:00", new DateTime(0), null);
			AssertToStringFormatted(DateTime.MinValue.ToString("HH\\:mm"), new DateTime(1), null);
			AssertToStringFormatted(RecentDate.ToString("HH\\:mm"), RecentDate, null);

			string[] formats = new string[]
			{
				"g",
				"G",
				"hh",
				"hh\\-mm\\-ss"
			};

			foreach (string format in formats)
			{
				AssertToStringFormatted(new TimeSpan(RecentDate.Hour, RecentDate.Minute, 0).ToString(format), RecentDate, format);
			}
		}

		#endregion

		#region IFormattable

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestIFormattableToString()
		{
			AssertEquals("Empty DateTime", "", ((IFormattable)ZTime.Empty).ToString("hh\\:mm\\:ss", new System.Globalization.DateTimeFormatInfo()));
			AssertEquals("Valid DateTime", "12:34:00", new ZTime(12, 34).ToString("hh\\:mm\\:ss", new System.Globalization.DateTimeFormatInfo()));
			AssertEquals("Invalid DateTime", "", ZTime.Invalid.ToString("hh\\:mm\\:ss", new System.Globalization.DateTimeFormatInfo()));
		}

		#endregion

		#region XmlSerializableValue

		[TestTimeZone]
		public void TestXmlSerializable()
		{
			AssertZTypeSerializesToXml("<a>12:34:00</a>", new ZTime(12, 34));
		}

		#endregion

		#region IZTypeTest Overrides

		public override void TestToString()
		{
			foreach (var value in ValidValues)
			{
				if (value is DateTime dateTime)
				{
					AssertEquals(MessageForValue(value), dateTime.ToString("HH:mm"), NewZ(value).ToString());
				}
			}
		}

		public override void TestGetValue()
		{
			foreach (object value in AllValues)
			{
				string message = MessageForValue(value) + " ";
				IZType z = NewZ(value);

				object actualFalse = ((IZTypeInternals)z).GetValueForLogicalDataLayer(false);
				object actualTrue = ((IZTypeInternals)z).GetValueForLogicalDataLayer(true);

				if (z.IsEmpty)
				{
					AssertEquals(message + "GetValue(false) type when empty", UsualValueType, actualFalse.GetType());
					AssertEquals(message + "GetValue(true) when empty", DBNull.Value, actualTrue);
				}
				else if (ValueIsUsualType(value))
				{
					if (value is DateTime dtv)
					{
						var trimmedDtv = new TimeSpan(dtv.Hour, dtv.Minute, 0);
						// Because multiple input DateTimes map to the same time, we can't extract the complete DateTime from a ZTime, just the time.
						AssertEquals(message + "GetValue(false) when not empty", trimmedDtv, actualFalse);
						AssertEquals(message + "GetValue(true) when not empty", trimmedDtv, actualTrue);
					}
					else
					{
						AssertEquals(message + "GetValue(false) when not empty", value, actualFalse);
						AssertEquals(message + "GetValue(true) when not empty", value, actualTrue);
					}
				}
				else
				{
					AssertEquals(message + " when not empty", actualFalse, actualTrue);
				}
			}
		}

		protected override IZType NewZ(object value)
		{
			return new ZTime(value);
		}

		protected override object[] ValidValues
		{
			get { return new object[] { new TimeSpan(0, 1, 1, 0, 0), new DateTime(1900, 1, 1, 0, 1, 0), new DateTime(1900, 1, 1, 23, 59, 0), RecentDate, new ZTime(RecentDate), new DateTime(0) }; }
		}

		protected override object[] InvalidValues
		{
			get { return new object[] { new DateTime(2000, 1, 1, 0, 0, 0) }; }
		}

		protected override object[] EmptyValues
		{
			get { return new object[] { null, DBNull.Value, ZTime.Empty, new ZTime() }; }
		}

		protected override object[] UnsupportedValues => new object[] { -17, "Potato" };

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
			var zDatetime = new ZTime(1, 1);
			AssertEquals("ZTime type code", TypeCode.Object, ((IConvertible)zDatetime).GetTypeCode());

			var emptyZTime = ZTime.Empty;
			AssertEquals("ZTime Empty type code", TypeCode.Empty, ((IConvertible)emptyZTime).GetTypeCode());
		}

		public void TestIConvertible_ConvertToZTime()
		{
			var zDatetime = new ZTime(1, 1);

			AssertEquals("ZTime to DateTime conversion", zDatetime, ((IConvertible)zDatetime).ToType(typeof(ZTime), null));
		}

		public void TestIConvertible_ConvertToDateTime()
		{
			var zDatetime = new ZTime(1, 1);
			var expected = new DateTime(1900, 1, 1, 1, 1, 0);

			AssertEquals("ZTime to DateTime conversion", expected, Convert.ToDateTime(zDatetime));
			AssertEquals("ZTime to DateTime conversion", expected, ((IConvertible)zDatetime).ToType(typeof(DateTime), null));
		}

		public void TestIConvertible_ConvertToString()
		{
			var zDatetime = new ZTime(1, 1);
			var expected = "01:01";

			AssertToString(expected, Convert.ToString(zDatetime));
			AssertToString(expected, ((IConvertible)zDatetime).ToType(typeof(string), null));
		}

		public void TestIConvertible_ConvertToInvalidTypes()
		{
			ZTime zDatetime = new ZTime(1, 1);

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
			ZTime zDatetime = new ZTime(1, 1);

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

		protected override void SetUp()
		{
			RecentDate = new DateTime(1900, 1, 1, 16, 26, 43);
			RecentDateUtc = new DateTime(1900, 1, 1, 16, 26, 43, DateTimeKind.Utc);
		}

		protected void AssertToString(string expected, object value)
		{
			AssertEquals(MessageForValue(value), expected, new ZTime(value).ToString());
		}

		protected void AssertToStringFormatted(string expected, object value, string format)
		{
			AssertEquals(MessageForValue(value), expected, new ZTime(value).ToString(format));
		}

		protected void AssertOperatorsOnSameValue(object value)
		{
			ZTime lhs = new ZTime(value);
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
			ZTime lhs = new ZTime(value);
			ZTime rhs = new ZTime(otherValue);
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
