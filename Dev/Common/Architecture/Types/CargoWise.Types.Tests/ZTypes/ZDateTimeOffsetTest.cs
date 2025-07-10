namespace CargoWise.Types.Tests
{
	using System;
	using System.Globalization;
	using NUnit.Framework;
	public class ZDateTimeOffsetTest : IZTypeTest
	{
		//TODO: add a bunch more tests, for constructors, properties, functions, operators, casts, static functions/properties

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate]
		public void TestDateTimeConstructor()
		{
			TestDateAttribute.UseUNLOCO = true;

			// http://www.timeanddate.com/time/zone/australia/sydney

			//Local time immediately before DST ending in 2014
			var offset0 = new ZDateTimeOffset(new DateTime(2014, 4, 6, 1, 30, 0, DateTimeKind.Local));
			AssertEquals(new DateTime(2014, 4, 6, 1, 30, 0, DateTimeKind.Unspecified), offset0.ToDateTime());
			AssertEquals(new DateTime(2014, 4, 5, 14, 30, 0, DateTimeKind.Utc), offset0.ToUtcDateTime());
			AssertEquals(TimeSpan.FromHours(11), offset0.Offset);

			//UTCNOW immediately before DST ending in 2014 (going from UTC+11h to UTC+10h)
			var offset1 = new ZDateTimeOffset(new DateTime(2014, 4, 6, 2, 30, 0, DateTimeKind.Local));
			AssertEquals(new DateTime(2014, 4, 6, 2, 30, 0, DateTimeKind.Unspecified), offset1.ToDateTime());
			AssertEquals(new DateTime(2014, 4, 5, 16, 30, 0, DateTimeKind.Utc), offset1.ToUtcDateTime());
			AssertEquals(TimeSpan.FromHours(10), offset1.Offset);

			//UTCNOW  immediately after the looped hour of DST ending in 2014 (going from UTC+11h to UTC+10h)
			var offset2 = new ZDateTimeOffset(new DateTime(2014, 4, 6, 3, 30, 0, DateTimeKind.Local));
			AssertEquals(new DateTime(2014, 4, 6, 3, 30, 0, DateTimeKind.Unspecified), offset2.ToDateTime());
			AssertEquals(new DateTime(2014, 4, 5, 17, 30, 0, DateTimeKind.Utc), offset2.ToUtcDateTime());
			AssertEquals(TimeSpan.FromHours(10), offset2.Offset);

			//UTCNOW immediately before DST starting in 2014 (going from UTC+10h to UTC+11h)
			var offset3 = new ZDateTimeOffset(new DateTime(2014, 10, 5, 1, 30, 0, DateTimeKind.Unspecified));
			AssertEquals(new DateTime(2014, 10, 5, 1, 30, 0, DateTimeKind.Unspecified), offset3.ToDateTime());
			AssertEquals(new DateTime(2014, 10, 4, 15, 30, 0, DateTimeKind.Utc), offset3.ToUtcDateTime());
			AssertEquals(TimeSpan.FromHours(10), offset3.Offset);

			//UTCNOW immediately after DST starting in 2014 (going from UTC+10h to UTC+11h)
			var offset4 = new ZDateTimeOffset(new DateTime(2014, 10, 5, 3, 30, 0, DateTimeKind.Unspecified));
			AssertEquals(new DateTime(2014, 10, 5, 3, 30, 0, DateTimeKind.Unspecified), offset4.ToDateTime());
			AssertEquals(new DateTime(2014, 10, 4, 16, 30, 0, DateTimeKind.Utc), offset4.ToUtcDateTime());
			AssertEquals(TimeSpan.FromHours(11), offset4.Offset);

			//test utc constructor
			var utcOffset = new ZDateTimeOffset(new DateTime(2014, 10, 4, 16, 30, 0, DateTimeKind.Utc));
			AssertEquals(new DateTime(2014, 10, 4, 16, 30, 0, DateTimeKind.Utc), utcOffset.ToDateTime());
			AssertEquals(new DateTime(2014, 10, 4, 16, 30, 0, DateTimeKind.Utc), utcOffset.ToUtcDateTime());
			AssertEquals(TimeSpan.Zero, utcOffset.Offset);

			//test invalid values
			AssertEquals(ZDateTimeOffset.Invalid, new ZDateTimeOffset(DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Unspecified)));
			AssertEquals(ZDateTimeOffset.Invalid, new ZDateTimeOffset(DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Unspecified)));
			AssertEquals(ZDateTimeOffset.Invalid, new ZDateTimeOffset(DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc)));
			AssertEquals(ZDateTimeOffset.Invalid, new ZDateTimeOffset(DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc)));
			AssertEquals(ZDateTimeOffset.Invalid, new ZDateTimeOffset(DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Local)));
			AssertEquals(ZDateTimeOffset.Invalid, new ZDateTimeOffset(DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Local)));
		}

		[TestTimeZoneUNLOCO("USERI")]
		[TestDate]
		public void TestDateTimeConstructor2()
		{
			TestDateAttribute.UseUNLOCO = true;

			// http://www.timeanddate.com/time/zone/usa/philadelphia

			//UTCNOW immediately before DST starting in 2014 (going from UTC-5h to UTC-4h)
			var offset0 = new ZDateTimeOffset(new DateTime(2014, 3, 9, 1, 30, 0, DateTimeKind.Local));
			AssertEquals(new DateTime(2014, 3, 9, 1, 30, 0, DateTimeKind.Unspecified), offset0.ToDateTime());
			AssertEquals(new DateTime(2014, 3, 9, 6, 30, 0, DateTimeKind.Utc), offset0.ToUtcDateTime());
			AssertEquals(TimeSpan.FromHours(-5), offset0.Offset);

			//UTCNOW immediately after DST starting in 2014 (going from UTC-5h to UTC-4h)
			var offset1 = new ZDateTimeOffset(new DateTime(2014, 3, 9, 3, 30, 0, DateTimeKind.Local));
			AssertEquals(new DateTime(2014, 3, 9, 3, 30, 0, DateTimeKind.Unspecified), offset1.ToDateTime());
			AssertEquals(new DateTime(2014, 3, 9, 7, 30, 0, DateTimeKind.Utc), offset1.ToUtcDateTime());
			AssertEquals(TimeSpan.FromHours(-4), offset1.Offset);

			//Local time immediately before DST ending in 2014
			var offset2 = new ZDateTimeOffset(new DateTime(2014, 11, 2, 0, 30, 0, DateTimeKind.Local));
			AssertEquals(new DateTime(2014, 11, 2, 0, 30, 0, DateTimeKind.Unspecified), offset2.ToDateTime());
			AssertEquals(new DateTime(2014, 11, 2, 4, 30, 0, DateTimeKind.Utc), offset2.ToUtcDateTime());
			AssertEquals(TimeSpan.FromHours(-4), offset2.Offset);

			//UTCNOW immediately before DST ending in 2014 (going from UTC-4h to UTC-5h)
			var offset3 = new ZDateTimeOffset(new DateTime(2014, 11, 2, 1, 30, 0, DateTimeKind.Unspecified));
			AssertEquals(new DateTime(2014, 11, 2, 1, 30, 0, DateTimeKind.Unspecified), offset3.ToDateTime());
			AssertEquals(new DateTime(2014, 11, 2, 6, 30, 0, DateTimeKind.Utc), offset3.ToUtcDateTime());
			AssertEquals(TimeSpan.FromHours(-5), offset3.Offset);

			//UTCNOW immediately after the looped hour of DST ending in 2014 (going from UTC-4h to UTC-5h)
			var offset4 = new ZDateTimeOffset(new DateTime(2014, 11, 2, 2, 30, 0, DateTimeKind.Unspecified));
			AssertEquals(new DateTime(2014, 11, 2, 2, 30, 0, DateTimeKind.Unspecified), offset4.ToDateTime());
			AssertEquals(new DateTime(2014, 11, 2, 7, 30, 0, DateTimeKind.Utc), offset4.ToUtcDateTime());
			AssertEquals(TimeSpan.FromHours(-5), offset4.Offset);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate]
		public void TestZDateTimeConstructor()
		{
			TestDateAttribute.UseUNLOCO = true;

			// http://www.timeanddate.com/time/zone/australia/sydney

			//Local time immediately before DST ending in 2014
			var offset0 = new ZDateTimeOffset(new ZDateTime(2014, 4, 6, 1, 30, 0, DateTimeKind.Local));
			AssertEquals(new DateTime(2014, 4, 6, 1, 30, 0, DateTimeKind.Unspecified), offset0.ToDateTime());
			AssertEquals(new DateTime(2014, 4, 5, 14, 30, 0, DateTimeKind.Utc), offset0.ToUtcDateTime());
			AssertEquals(TimeSpan.FromHours(11), offset0.Offset);

			//UTCNOW immediately before DST ending in 2014 (going from UTC+11h to UTC+10h)
			var offset1 = new ZDateTimeOffset(new ZDateTime(2014, 4, 6, 2, 30, 0, DateTimeKind.Local));
			AssertEquals(new DateTime(2014, 4, 6, 2, 30, 0, DateTimeKind.Unspecified), offset1.ToDateTime());
			AssertEquals(new DateTime(2014, 4, 5, 16, 30, 0, DateTimeKind.Utc), offset1.ToUtcDateTime());
			AssertEquals(TimeSpan.FromHours(10), offset1.Offset);

			//UTCNOW  immediately after the looped hour of DST ending in 2014 (going from UTC+11h to UTC+10h)
			var offset2 = new ZDateTimeOffset(new ZDateTime(2014, 4, 6, 3, 30, 0, DateTimeKind.Local));
			AssertEquals(new DateTime(2014, 4, 6, 3, 30, 0, DateTimeKind.Unspecified), offset2.ToDateTime());
			AssertEquals(new DateTime(2014, 4, 5, 17, 30, 0, DateTimeKind.Utc), offset2.ToUtcDateTime());
			AssertEquals(TimeSpan.FromHours(10), offset2.Offset);

			//UTCNOW immediately before DST starting in 2014 (going from UTC+10h to UTC+11h)
			var offset3 = new ZDateTimeOffset(new ZDateTime(2014, 10, 5, 1, 30, 0, DateTimeKind.Unspecified));
			AssertEquals(new DateTime(2014, 10, 5, 1, 30, 0, DateTimeKind.Unspecified), offset3.ToDateTime());
			AssertEquals(new DateTime(2014, 10, 4, 15, 30, 0, DateTimeKind.Utc), offset3.ToUtcDateTime());
			AssertEquals(TimeSpan.FromHours(10), offset3.Offset);

			//UTCNOW immediately after DST starting in 2014 (going from UTC+10h to UTC+11h)
			var offset4 = new ZDateTimeOffset(new ZDateTime(2014, 10, 5, 3, 30, 0, DateTimeKind.Unspecified));
			AssertEquals(new DateTime(2014, 10, 5, 3, 30, 0, DateTimeKind.Unspecified), offset4.ToDateTime());
			AssertEquals(new DateTime(2014, 10, 4, 16, 30, 0, DateTimeKind.Utc), offset4.ToUtcDateTime());
			AssertEquals(TimeSpan.FromHours(11), offset4.Offset);

			//test utc constructor
			var utcOffset = new ZDateTimeOffset(new ZDateTime(2014, 10, 4, 16, 30, 0, DateTimeKind.Utc));
			AssertEquals(new DateTime(2014, 10, 4, 16, 30, 0, DateTimeKind.Utc), utcOffset.ToDateTime());
			AssertEquals(new DateTime(2014, 10, 4, 16, 30, 0, DateTimeKind.Utc), utcOffset.ToUtcDateTime());
			AssertEquals(TimeSpan.Zero, utcOffset.Offset);

			//test invalid values
			AssertEquals(ZDateTimeOffset.Invalid, new ZDateTimeOffset(ZDateTime.Invalid));
			AssertEquals(ZDateTimeOffset.Empty, new ZDateTimeOffset(ZDateTime.Empty));
		}

		[TestTimeZoneUNLOCO("USERI")]
		[TestDate]
		public void TestZDateTimeConstructor2()
		{
			TestDateAttribute.UseUNLOCO = true;

			// http://www.timeanddate.com/time/zone/usa/philadelphia

			//UTCNOW immediately before DST starting in 2014 (going from UTC-5h to UTC-4h)
			var offset0 = new ZDateTimeOffset(new ZDateTime(2014, 3, 9, 1, 30, 0, DateTimeKind.Local));
			AssertEquals(new DateTime(2014, 3, 9, 1, 30, 0, DateTimeKind.Unspecified), offset0.ToDateTime());
			AssertEquals(new DateTime(2014, 3, 9, 6, 30, 0, DateTimeKind.Utc), offset0.ToUtcDateTime());
			AssertEquals(TimeSpan.FromHours(-5), offset0.Offset);

			//UTCNOW immediately after DST starting in 2014 (going from UTC-5h to UTC-4h)
			var offset1 = new ZDateTimeOffset(new ZDateTime(2014, 3, 9, 3, 30, 0, DateTimeKind.Local));
			AssertEquals(new DateTime(2014, 3, 9, 3, 30, 0, DateTimeKind.Unspecified), offset1.ToDateTime());
			AssertEquals(new DateTime(2014, 3, 9, 7, 30, 0, DateTimeKind.Utc), offset1.ToUtcDateTime());
			AssertEquals(TimeSpan.FromHours(-4), offset1.Offset);

			//Local time immediately before DST ending in 2014
			var offset2 = new ZDateTimeOffset(new ZDateTime(2014, 11, 2, 0, 30, 0, DateTimeKind.Local));
			AssertEquals(new DateTime(2014, 11, 2, 0, 30, 0, DateTimeKind.Unspecified), offset2.ToDateTime());
			AssertEquals(new DateTime(2014, 11, 2, 4, 30, 0, DateTimeKind.Utc), offset2.ToUtcDateTime());
			AssertEquals(TimeSpan.FromHours(-4), offset2.Offset);

			//UTCNOW immediately before DST ending in 2014 (going from UTC-4h to UTC-5h)
			var offset3 = new ZDateTimeOffset(new ZDateTime(2014, 11, 2, 1, 30, 0, DateTimeKind.Unspecified));
			AssertEquals(new DateTime(2014, 11, 2, 1, 30, 0, DateTimeKind.Unspecified), offset3.ToDateTime());
			AssertEquals(new DateTime(2014, 11, 2, 6, 30, 0, DateTimeKind.Utc), offset3.ToUtcDateTime());
			AssertEquals(TimeSpan.FromHours(-5), offset3.Offset);

			//UTCNOW immediately after the looped hour of DST ending in 2014 (going from UTC-4h to UTC-5h)
			var offset4 = new ZDateTimeOffset(new ZDateTime(2014, 11, 2, 2, 30, 0, DateTimeKind.Unspecified));
			AssertEquals(new DateTime(2014, 11, 2, 2, 30, 0, DateTimeKind.Unspecified), offset4.ToDateTime());
			AssertEquals(new DateTime(2014, 11, 2, 7, 30, 0, DateTimeKind.Utc), offset4.ToUtcDateTime());
			AssertEquals(TimeSpan.FromHours(-5), offset4.Offset);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate]
		public void TestDateTimeAndKindConstructor()
		{
			TestDateAttribute.UseUNLOCO = true;

			// http://www.timeanddate.com/time/zone/australia/sydney

			//Local time immediately before DST ending in 2014
			var offset0 = new ZDateTimeOffset(new DateTime(2014, 4, 6, 1, 30, 0), DateTimeKind.Local);
			AssertEquals(new DateTime(2014, 4, 6, 1, 30, 0, DateTimeKind.Unspecified), offset0.ToDateTime());
			AssertEquals(new DateTime(2014, 4, 5, 14, 30, 0, DateTimeKind.Utc), offset0.ToUtcDateTime());
			AssertEquals(TimeSpan.FromHours(11), offset0.Offset);

			//UTCNOW immediately before DST ending in 2014 (going from UTC+11h to UTC+10h)
			var offset1 = new ZDateTimeOffset(new DateTime(2014, 4, 6, 2, 30, 0), DateTimeKind.Local);
			AssertEquals(new DateTime(2014, 4, 6, 2, 30, 0, DateTimeKind.Unspecified), offset1.ToDateTime());
			AssertEquals(new DateTime(2014, 4, 5, 16, 30, 0, DateTimeKind.Utc), offset1.ToUtcDateTime());
			AssertEquals(TimeSpan.FromHours(10), offset1.Offset);

			//UTCNOW  immediately after the looped hour of DST ending in 2014 (going from UTC+11h to UTC+10h)
			var offset2 = new ZDateTimeOffset(new DateTime(2014, 4, 6, 3, 30, 0), DateTimeKind.Local);
			AssertEquals(new DateTime(2014, 4, 6, 3, 30, 0, DateTimeKind.Unspecified), offset2.ToDateTime());
			AssertEquals(new DateTime(2014, 4, 5, 17, 30, 0, DateTimeKind.Utc), offset2.ToUtcDateTime());
			AssertEquals(TimeSpan.FromHours(10), offset2.Offset);

			//UTCNOW immediately before DST starting in 2014 (going from UTC+10h to UTC+11h)
			var offset3 = new ZDateTimeOffset(new DateTime(2014, 10, 5, 1, 30, 0), DateTimeKind.Unspecified);
			AssertEquals(new DateTime(2014, 10, 5, 1, 30, 0, DateTimeKind.Unspecified), offset3.ToDateTime());
			AssertEquals(new DateTime(2014, 10, 4, 15, 30, 0, DateTimeKind.Utc), offset3.ToUtcDateTime());
			AssertEquals(TimeSpan.FromHours(10), offset3.Offset);

			//UTCNOW immediately after DST starting in 2014 (going from UTC+10h to UTC+11h)
			var offset4 = new ZDateTimeOffset(new DateTime(2014, 10, 5, 3, 30, 0), DateTimeKind.Unspecified);
			AssertEquals(new DateTime(2014, 10, 5, 3, 30, 0, DateTimeKind.Unspecified), offset4.ToDateTime());
			AssertEquals(new DateTime(2014, 10, 4, 16, 30, 0, DateTimeKind.Utc), offset4.ToUtcDateTime());
			AssertEquals(TimeSpan.FromHours(11), offset4.Offset);

			//test utc constructor
			var utcOffset = new ZDateTimeOffset(new DateTime(2014, 10, 4, 16, 30, 0), DateTimeKind.Utc);
			AssertEquals(new DateTime(2014, 10, 4, 16, 30, 0, DateTimeKind.Utc), utcOffset.ToDateTime());
			AssertEquals(new DateTime(2014, 10, 4, 16, 30, 0, DateTimeKind.Utc), utcOffset.ToUtcDateTime());
			AssertEquals(TimeSpan.Zero, utcOffset.Offset);

			//test invalid values
			AssertEquals(ZDateTimeOffset.Invalid, new ZDateTimeOffset(DateTime.MinValue, DateTimeKind.Unspecified));
			AssertEquals(ZDateTimeOffset.Invalid, new ZDateTimeOffset(DateTime.MaxValue, DateTimeKind.Unspecified));
			AssertEquals(ZDateTimeOffset.Invalid, new ZDateTimeOffset(DateTime.MinValue, DateTimeKind.Utc));
			AssertEquals(ZDateTimeOffset.Invalid, new ZDateTimeOffset(DateTime.MaxValue, DateTimeKind.Utc));
			AssertEquals(ZDateTimeOffset.Invalid, new ZDateTimeOffset(DateTime.MinValue, DateTimeKind.Local));
			AssertEquals(ZDateTimeOffset.Invalid, new ZDateTimeOffset(DateTime.MaxValue, DateTimeKind.Local));
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate]
		public void TestZDateTimeAndKindConstructor()
		{
			TestDateAttribute.UseUNLOCO = true;

			// http://www.timeanddate.com/time/zone/australia/sydney

			//Local time immediately before DST ending in 2014
			var offset0 = new ZDateTimeOffset(new ZDateTime(2014, 4, 6, 1, 30, 0), DateTimeKind.Local);
			AssertEquals(new DateTime(2014, 4, 6, 1, 30, 0, DateTimeKind.Unspecified), offset0.ToDateTime());
			AssertEquals(new DateTime(2014, 4, 5, 14, 30, 0, DateTimeKind.Utc), offset0.ToUtcDateTime());
			AssertEquals(TimeSpan.FromHours(11), offset0.Offset);

			//UTCNOW immediately before DST ending in 2014 (going from UTC+11h to UTC+10h)
			var offset1 = new ZDateTimeOffset(new ZDateTime(2014, 4, 6, 2, 30, 0), DateTimeKind.Local);
			AssertEquals(new DateTime(2014, 4, 6, 2, 30, 0, DateTimeKind.Unspecified), offset1.ToDateTime());
			AssertEquals(new DateTime(2014, 4, 5, 16, 30, 0, DateTimeKind.Utc), offset1.ToUtcDateTime());
			AssertEquals(TimeSpan.FromHours(10), offset1.Offset);

			//UTCNOW  immediately after the looped hour of DST ending in 2014 (going from UTC+11h to UTC+10h)
			var offset2 = new ZDateTimeOffset(new ZDateTime(2014, 4, 6, 3, 30, 0), DateTimeKind.Local);
			AssertEquals(new DateTime(2014, 4, 6, 3, 30, 0, DateTimeKind.Unspecified), offset2.ToDateTime());
			AssertEquals(new DateTime(2014, 4, 5, 17, 30, 0, DateTimeKind.Utc), offset2.ToUtcDateTime());
			AssertEquals(TimeSpan.FromHours(10), offset2.Offset);

			//UTCNOW immediately before DST starting in 2014 (going from UTC+10h to UTC+11h)
			var offset3 = new ZDateTimeOffset(new ZDateTime(2014, 10, 5, 1, 30, 0), DateTimeKind.Unspecified);
			AssertEquals(new DateTime(2014, 10, 5, 1, 30, 0, DateTimeKind.Unspecified), offset3.ToDateTime());
			AssertEquals(new DateTime(2014, 10, 4, 15, 30, 0, DateTimeKind.Utc), offset3.ToUtcDateTime());
			AssertEquals(TimeSpan.FromHours(10), offset3.Offset);

			//UTCNOW immediately after DST starting in 2014 (going from UTC+10h to UTC+11h)
			var offset4 = new ZDateTimeOffset(new ZDateTime(2014, 10, 5, 3, 30, 0), DateTimeKind.Unspecified);
			AssertEquals(new DateTime(2014, 10, 5, 3, 30, 0, DateTimeKind.Unspecified), offset4.ToDateTime());
			AssertEquals(new DateTime(2014, 10, 4, 16, 30, 0, DateTimeKind.Utc), offset4.ToUtcDateTime());
			AssertEquals(TimeSpan.FromHours(11), offset4.Offset);

			//test utc constructor
			var utcOffset = new ZDateTimeOffset(new ZDateTime(2014, 10, 4, 16, 30, 0), DateTimeKind.Utc);
			AssertEquals(new DateTime(2014, 10, 4, 16, 30, 0, DateTimeKind.Utc), utcOffset.ToDateTime());
			AssertEquals(new DateTime(2014, 10, 4, 16, 30, 0, DateTimeKind.Utc), utcOffset.ToUtcDateTime());
			AssertEquals(TimeSpan.Zero, utcOffset.Offset);

			//test invalid values
			AssertEquals(ZDateTimeOffset.Invalid, new ZDateTimeOffset(ZDateTime.Invalid, DateTimeKind.Unspecified));
			AssertEquals(ZDateTimeOffset.Empty, new ZDateTimeOffset(ZDateTime.Empty, DateTimeKind.Unspecified));
			AssertEquals(ZDateTimeOffset.Invalid, new ZDateTimeOffset(ZDateTime.Invalid, DateTimeKind.Utc));
			AssertEquals(ZDateTimeOffset.Empty, new ZDateTimeOffset(ZDateTime.Empty, DateTimeKind.Utc));
			AssertEquals(ZDateTimeOffset.Invalid, new ZDateTimeOffset(ZDateTime.Invalid, DateTimeKind.Local));
			AssertEquals(ZDateTimeOffset.Empty, new ZDateTimeOffset(ZDateTime.Empty, DateTimeKind.Local));
		}

		public void TestDateTimeAndOffsetConstructor()
		{
			var utcOffset = new ZDateTimeOffset(new DateTime(2014, 10, 4, 16, 30, 0, DateTimeKind.Utc), TimeSpan.Zero);
			AssertEquals(new DateTime(2014, 10, 4, 16, 30, 0, DateTimeKind.Utc), utcOffset.ToDateTime());
			AssertEquals(new DateTime(2014, 10, 4, 16, 30, 0, DateTimeKind.Utc), utcOffset.ToUtcDateTime());
			AssertEquals(TimeSpan.Zero, utcOffset.Offset);

			var localOffset1 = new ZDateTimeOffset(new DateTime(2014, 4, 6, 1, 30, 0, DateTimeKind.Local), TimeSpan.FromHours(11));
			AssertEquals(new DateTime(2014, 4, 6, 1, 30, 0, DateTimeKind.Unspecified), localOffset1.ToDateTime());
			AssertEquals(new DateTime(2014, 4, 5, 14, 30, 0, DateTimeKind.Utc), localOffset1.ToUtcDateTime());
			AssertEquals(TimeSpan.FromHours(11), localOffset1.Offset);

			var localOffset2 = new ZDateTimeOffset(new DateTime(2014, 4, 6, 1, 30, 0, DateTimeKind.Local), TimeSpan.Zero);
			AssertEquals(new DateTime(2014, 4, 6, 1, 30, 0, DateTimeKind.Utc), localOffset2.ToDateTime());
			AssertEquals(new DateTime(2014, 4, 6, 1, 30, 0, DateTimeKind.Utc), localOffset2.ToUtcDateTime());
			AssertEquals(TimeSpan.Zero, localOffset2.Offset);

			var unspecifiedOffset1 = new ZDateTimeOffset(new DateTime(2014, 4, 6, 1, 30, 0, DateTimeKind.Unspecified), TimeSpan.FromHours(11));
			AssertEquals(new DateTime(2014, 4, 6, 1, 30, 0, DateTimeKind.Unspecified), unspecifiedOffset1.ToDateTime());
			AssertEquals(new DateTime(2014, 4, 5, 14, 30, 0, DateTimeKind.Utc), unspecifiedOffset1.ToUtcDateTime());
			AssertEquals(TimeSpan.FromHours(11), unspecifiedOffset1.Offset);

			var unspecifiedOffset2 = new ZDateTimeOffset(new DateTime(2014, 4, 6, 1, 30, 0, DateTimeKind.Local), TimeSpan.Zero);
			AssertEquals(new DateTime(2014, 4, 6, 1, 30, 0, DateTimeKind.Utc), unspecifiedOffset2.ToDateTime());
			AssertEquals(new DateTime(2014, 4, 6, 1, 30, 0, DateTimeKind.Utc), unspecifiedOffset2.ToUtcDateTime());
			AssertEquals(TimeSpan.Zero, unspecifiedOffset2.Offset);
		}

		public void TestZDateTimeAndOffsetConstructor()
		{
			var utcOffset = new ZDateTimeOffset(new ZDateTime(2014, 10, 4, 16, 30, 0, DateTimeKind.Utc), TimeSpan.Zero);
			AssertEquals(new DateTime(2014, 10, 4, 16, 30, 0, DateTimeKind.Utc), utcOffset.ToDateTime());
			AssertEquals(new DateTime(2014, 10, 4, 16, 30, 0, DateTimeKind.Utc), utcOffset.ToUtcDateTime());
			AssertEquals(TimeSpan.Zero, utcOffset.Offset);

			var localOffset1 = new ZDateTimeOffset(new ZDateTime(2014, 4, 6, 1, 30, 0, DateTimeKind.Local), TimeSpan.FromHours(11));
			AssertEquals(new DateTime(2014, 4, 6, 1, 30, 0, DateTimeKind.Unspecified), localOffset1.ToDateTime());
			AssertEquals(new DateTime(2014, 4, 5, 14, 30, 0, DateTimeKind.Utc), localOffset1.ToUtcDateTime());
			AssertEquals(TimeSpan.FromHours(11), localOffset1.Offset);

			var localOffset2 = new ZDateTimeOffset(new ZDateTime(2014, 4, 6, 1, 30, 0, DateTimeKind.Local), TimeSpan.Zero);
			AssertEquals(new DateTime(2014, 4, 6, 1, 30, 0, DateTimeKind.Utc), localOffset2.ToDateTime());
			AssertEquals(new DateTime(2014, 4, 6, 1, 30, 0, DateTimeKind.Utc), localOffset2.ToUtcDateTime());
			AssertEquals(TimeSpan.Zero, localOffset2.Offset);

			var unspecifiedOffset1 = new ZDateTimeOffset(new ZDateTime(2014, 4, 6, 1, 30, 0, DateTimeKind.Unspecified), TimeSpan.FromHours(11));
			AssertEquals(new DateTime(2014, 4, 6, 1, 30, 0, DateTimeKind.Unspecified), unspecifiedOffset1.ToDateTime());
			AssertEquals(new DateTime(2014, 4, 5, 14, 30, 0, DateTimeKind.Utc), unspecifiedOffset1.ToUtcDateTime());
			AssertEquals(TimeSpan.FromHours(11), unspecifiedOffset1.Offset);

			var unspecifiedOffset2 = new ZDateTimeOffset(new ZDateTime(2014, 4, 6, 1, 30, 0, DateTimeKind.Local), TimeSpan.Zero);
			AssertEquals(new DateTime(2014, 4, 6, 1, 30, 0, DateTimeKind.Utc), unspecifiedOffset2.ToDateTime());
			AssertEquals(new DateTime(2014, 4, 6, 1, 30, 0, DateTimeKind.Utc), unspecifiedOffset2.ToUtcDateTime());
			AssertEquals(TimeSpan.Zero, unspecifiedOffset2.Offset);
		}

		public void TestDateTimeAndKindAndOffsetConstructor()
		{
			var utcOffset = new ZDateTimeOffset(new DateTime(2014, 10, 4, 16, 30, 0), DateTimeKind.Utc, TimeSpan.Zero);
			AssertEquals(new DateTime(2014, 10, 4, 16, 30, 0, DateTimeKind.Utc), utcOffset.ToDateTime());
			AssertEquals(new DateTime(2014, 10, 4, 16, 30, 0, DateTimeKind.Utc), utcOffset.ToUtcDateTime());
			AssertEquals(TimeSpan.Zero, utcOffset.Offset);

			var localOffset1 = new ZDateTimeOffset(new DateTime(2014, 4, 6, 1, 30, 0), DateTimeKind.Local, TimeSpan.FromHours(11));
			AssertEquals(new DateTime(2014, 4, 6, 1, 30, 0, DateTimeKind.Unspecified), localOffset1.ToDateTime());
			AssertEquals(new DateTime(2014, 4, 5, 14, 30, 0, DateTimeKind.Utc), localOffset1.ToUtcDateTime());
			AssertEquals(TimeSpan.FromHours(11), localOffset1.Offset);

			var localOffset2 = new ZDateTimeOffset(new DateTime(2014, 4, 6, 1, 30, 0), DateTimeKind.Local, TimeSpan.Zero);
			AssertEquals(new DateTime(2014, 4, 6, 1, 30, 0, DateTimeKind.Utc), localOffset2.ToDateTime());
			AssertEquals(new DateTime(2014, 4, 6, 1, 30, 0, DateTimeKind.Utc), localOffset2.ToUtcDateTime());
			AssertEquals(TimeSpan.Zero, localOffset2.Offset);

			var unspecifiedOffset1 = new ZDateTimeOffset(new DateTime(2014, 4, 6, 1, 30, 0), DateTimeKind.Unspecified, TimeSpan.FromHours(11));
			AssertEquals(new DateTime(2014, 4, 6, 1, 30, 0, DateTimeKind.Unspecified), unspecifiedOffset1.ToDateTime());
			AssertEquals(new DateTime(2014, 4, 5, 14, 30, 0, DateTimeKind.Utc), unspecifiedOffset1.ToUtcDateTime());
			AssertEquals(TimeSpan.FromHours(11), unspecifiedOffset1.Offset);

			var unspecifiedOffset2 = new ZDateTimeOffset(new DateTime(2014, 4, 6, 1, 30, 0), DateTimeKind.Local, TimeSpan.Zero);
			AssertEquals(new DateTime(2014, 4, 6, 1, 30, 0, DateTimeKind.Utc), unspecifiedOffset2.ToDateTime());
			AssertEquals(new DateTime(2014, 4, 6, 1, 30, 0, DateTimeKind.Utc), unspecifiedOffset2.ToUtcDateTime());
			AssertEquals(TimeSpan.Zero, unspecifiedOffset2.Offset);
		}

		public void TestZDateTimeAndKindAndOffsetConstructor()
		{
			var utcOffset = new ZDateTimeOffset(new ZDateTime(2014, 10, 4, 16, 30, 0), DateTimeKind.Utc, TimeSpan.Zero);
			AssertEquals(new DateTime(2014, 10, 4, 16, 30, 0, DateTimeKind.Utc), utcOffset.ToDateTime());
			AssertEquals(new DateTime(2014, 10, 4, 16, 30, 0, DateTimeKind.Utc), utcOffset.ToUtcDateTime());
			AssertEquals(TimeSpan.Zero, utcOffset.Offset);

			var localOffset1 = new ZDateTimeOffset(new ZDateTime(2014, 4, 6, 1, 30, 0), DateTimeKind.Local, TimeSpan.FromHours(11));
			AssertEquals(new DateTime(2014, 4, 6, 1, 30, 0, DateTimeKind.Unspecified), localOffset1.ToDateTime());
			AssertEquals(new DateTime(2014, 4, 5, 14, 30, 0, DateTimeKind.Utc), localOffset1.ToUtcDateTime());
			AssertEquals(TimeSpan.FromHours(11), localOffset1.Offset);

			var localOffset2 = new ZDateTimeOffset(new ZDateTime(2014, 4, 6, 1, 30, 0), DateTimeKind.Local, TimeSpan.Zero);
			AssertEquals(new DateTime(2014, 4, 6, 1, 30, 0, DateTimeKind.Utc), localOffset2.ToDateTime());
			AssertEquals(new DateTime(2014, 4, 6, 1, 30, 0, DateTimeKind.Utc), localOffset2.ToUtcDateTime());
			AssertEquals(TimeSpan.Zero, localOffset2.Offset);

			var unspecifiedOffset1 = new ZDateTimeOffset(new ZDateTime(2014, 4, 6, 1, 30, 0), DateTimeKind.Unspecified, TimeSpan.FromHours(11));
			AssertEquals(new DateTime(2014, 4, 6, 1, 30, 0, DateTimeKind.Unspecified), unspecifiedOffset1.ToDateTime());
			AssertEquals(new DateTime(2014, 4, 5, 14, 30, 0, DateTimeKind.Utc), unspecifiedOffset1.ToUtcDateTime());
			AssertEquals(TimeSpan.FromHours(11), unspecifiedOffset1.Offset);

			var unspecifiedOffset2 = new ZDateTimeOffset(new ZDateTime(2014, 4, 6, 1, 30, 0), DateTimeKind.Local, TimeSpan.Zero);
			AssertEquals(new DateTime(2014, 4, 6, 1, 30, 0, DateTimeKind.Utc), unspecifiedOffset2.ToDateTime());
			AssertEquals(new DateTime(2014, 4, 6, 1, 30, 0, DateTimeKind.Utc), unspecifiedOffset2.ToUtcDateTime());
			AssertEquals(TimeSpan.Zero, unspecifiedOffset2.Offset);
		}

		public void TestDateTimeOffsetConstructor()
		{
			var offset1 = new ZDateTimeOffset(new DateTimeOffset(2014, 10, 4, 15, 30, 0, TimeSpan.FromHours(11)));
			AssertEquals(new DateTime(2014, 10, 4, 15, 30, 0, DateTimeKind.Unspecified), offset1.ToDateTime());
			AssertEquals(new DateTime(2014, 10, 4, 4, 30, 0, DateTimeKind.Utc), offset1.ToUtcDateTime());
			AssertEquals(offset1.ToDateTimeOffset(), new DateTimeOffset(2014, 10, 4, 15, 30, 0, TimeSpan.FromHours(11)));
			AssertEquals(TimeSpan.FromHours(11), offset1.Offset);
		}

		[TestDate(2021, 7, 3)]
		public void TestYearMonthDayConstructor()
		{
			var offset = new ZDateTimeOffset(2021, 7, 3);
			AssertEquals(ZDateTime.Today, offset.ToZDateTime());
		}

		public void TestYearMonthDayHoursMinuteSeconds()
		{
			var offset = new ZDateTimeOffset(2021, 7, 3, 12, 13, 12);
			AssertEquals(new ZDateTime(2021, 7, 3, 12, 13, 12), offset.ToZDateTime());
			AssertEquals(10d, offset.Offset.TotalHours);
		}

		public void TestTicksAndOffsetConstructor()
		{
			var ticks = DateTime.Now.Ticks;
			var offset1 = new ZDateTimeOffset(ticks, TimeSpan.FromHours(1));
			AssertEquals(TimeSpan.FromHours(1), offset1.Offset);
			AssertEquals(ticks, offset1.Ticks);
		}

		public void TestFieldsAndOffsetConstructor()
		{
			var offset1 = new ZDateTimeOffset(2014, 10, 4, 15, 30, 2, TimeSpan.FromHours(11));
			AssertEquals(new DateTime(2014, 10, 4, 15, 30, 2, DateTimeKind.Unspecified), offset1.ToDateTime());
			AssertEquals(new DateTime(2014, 10, 4, 4, 30, 2, DateTimeKind.Utc), offset1.ToUtcDateTime());
			AssertEquals(offset1.ToDateTimeOffset(), new DateTimeOffset(2014, 10, 4, 15, 30, 2, TimeSpan.FromHours(11)));
			AssertEquals(TimeSpan.FromHours(11), offset1.Offset);
			AssertEquals(2014, offset1.Year);
			AssertEquals(10, offset1.Month);
			AssertEquals(4, offset1.Day);
			AssertEquals(15, offset1.Hour);
			AssertEquals(30, offset1.Minute);
			AssertEquals(2, offset1.Second);
			AssertEquals(0, offset1.Millisecond);
			AssertEquals(new ZDate(2014, 10, 4), offset1.Date);
		}

		public void TestFieldsAndMillisecondsAndOffsetConstructor()
		{
			var offset1 = new ZDateTimeOffset(2014, 10, 4, 15, 30, 2, 321, TimeSpan.FromHours(11));
			AssertEquals(new DateTime(2014, 10, 4, 15, 30, 2, 321, DateTimeKind.Unspecified), offset1.ToDateTime());
			AssertEquals(new DateTime(2014, 10, 4, 4, 30, 2, 321, DateTimeKind.Utc), offset1.ToUtcDateTime());
			AssertEquals(offset1.ToDateTimeOffset(), new DateTimeOffset(2014, 10, 4, 15, 30, 2, 321, TimeSpan.FromHours(11)));
			AssertEquals(TimeSpan.FromHours(11), offset1.Offset);
			AssertEquals(2014, offset1.Year);
			AssertEquals(10, offset1.Month);
			AssertEquals(4, offset1.Day);
			AssertEquals(15, offset1.Hour);
			AssertEquals(30, offset1.Minute);
			AssertEquals(2, offset1.Second);
			AssertEquals(321, offset1.Millisecond);
			AssertEquals(new ZDate(2014, 10, 4), offset1.Date);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate]
		public void TestNow_UtcNow_Today_UtcToday()
		{
			TestDateAttribute.UseUNLOCO = true;

			// http://www.timeanddate.com/time/zone/australia/sydney

			//UTCNOW immediately before DST ending in 2014 (going from UTC+11h to UTC+10h)
			TestDateAttribute.Date = new DateTime(2014, 4, 5, 15, 30, 0);
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2014, 4, 5, 15, 30, 0, TimeSpan.FromHours(0)), ZDateTimeOffset.UtcNow);
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2014, 4, 6, 2, 30, 0, TimeSpan.FromHours(11)), ZDateTimeOffset.Now);
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2014, 4, 5, 0, 0, 0, TimeSpan.FromHours(0)), ZDateTimeOffset.UtcToday);
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2014, 4, 6, 0, 0, 0, TimeSpan.FromHours(11)), ZDateTimeOffset.Today);

			//UTCNOW immediately after DST ending in 2014 (going from UTC+11h to UTC+10h)
			TestDateAttribute.Date = new DateTime(2014, 4, 5, 16, 30, 0);
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2014, 4, 5, 16, 30, 0, TimeSpan.FromHours(0)), ZDateTimeOffset.UtcNow);
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2014, 4, 6, 2, 30, 0, TimeSpan.FromHours(10)), ZDateTimeOffset.Now);
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2014, 4, 5, 0, 0, 0, TimeSpan.FromHours(0)), ZDateTimeOffset.UtcToday);
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2014, 4, 6, 0, 0, 0, TimeSpan.FromHours(11)), ZDateTimeOffset.Today);

			//UTCNOW  immediately after the looped hour of DST ending in 2014 (going from UTC+11h to UTC+10h)
			TestDateAttribute.Date = new DateTime(2014, 4, 5, 17, 30, 0);
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2014, 4, 5, 17, 30, 0, TimeSpan.FromHours(0)), ZDateTimeOffset.UtcNow);
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2014, 4, 6, 3, 30, 0, TimeSpan.FromHours(10)), ZDateTimeOffset.Now);
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2014, 4, 5, 0, 0, 0, TimeSpan.FromHours(0)), ZDateTimeOffset.UtcToday);
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2014, 4, 6, 0, 0, 0, TimeSpan.FromHours(11)), ZDateTimeOffset.Today);

			//UTCNOW immediately before DST starting in 2014 (going from UTC+10h to UTC+11h)
			TestDateAttribute.Date = new DateTime(2014, 10, 4, 15, 30, 0);
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2014, 10, 4, 15, 30, 0, TimeSpan.FromHours(0)), ZDateTimeOffset.UtcNow);
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2014, 10, 5, 1, 30, 0, TimeSpan.FromHours(10)), ZDateTimeOffset.Now);
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2014, 10, 4, 0, 0, 0, TimeSpan.FromHours(0)), ZDateTimeOffset.UtcToday);
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2014, 10, 5, 0, 0, 0, TimeSpan.FromHours(10)), ZDateTimeOffset.Today);

			//UTCNOW immediately after DST starting in 2014 (going from UTC+10h to UTC+11h)
			TestDateAttribute.Date = new DateTime(2014, 10, 4, 16, 30, 0);
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2014, 10, 4, 16, 30, 0, TimeSpan.FromHours(0)), ZDateTimeOffset.UtcNow);
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2014, 10, 5, 3, 30, 0, TimeSpan.FromHours(11)), ZDateTimeOffset.Now);
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2014, 10, 4, 0, 0, 0, TimeSpan.FromHours(0)), ZDateTimeOffset.UtcToday);
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2014, 10, 5, 0, 0, 0, TimeSpan.FromHours(10)), ZDateTimeOffset.Today);
		}

		public void TestSqlFormat()
		{
			ZDateTimeOffset dateTimeOffset1 = new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(1));
			AssertEquals("Format should be YYYY-MM-DD hh:mm:ss[.fffffff] [{+|-}hh:mm]", "2003-03-02 13:45:55.0000000 +01:00", dateTimeOffset1.SqlFormat);
			AssertEquals(dateTimeOffset1, ZDateTimeOffset.FromSqlFormat("2003-03-02 13:45:55.0000000 +01:00"));
			ZDateTimeOffset dateTimeOffset2 = new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(-1));
			AssertEquals("Format should be YYYY-MM-DD hh:mm:ss[.fffffff] [{+|-}hh:mm]", "2003-03-02 13:45:55.0000000 -01:00", dateTimeOffset2.SqlFormat);
			AssertEquals(dateTimeOffset2, ZDateTimeOffset.FromSqlFormat("2003-03-02 13:45:55.0000000 -01:00"));
			ZDateTimeOffset dateTimeOffset3 = new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, new TimeSpan());
			AssertEquals("Format should be YYYY-MM-DD hh:mm:ss[.fffffff] [{+|-}hh:mm]", "2003-03-02 13:45:55.0000000 +00:00", dateTimeOffset3.SqlFormat);

			//have checked and +00:00 is the canonical format for no/unspecified utc offset, returned from SQL Server	
			AssertEquals(dateTimeOffset3, ZDateTimeOffset.FromSqlFormat("2003-03-02 13:45:55.0000000 +00:00"));
			AssertEquals(dateTimeOffset3, ZDateTimeOffset.FromSqlFormat("2003-03-02 13:45:55.0000000 -00:00"));

			//TODO: SQL server returns .0000000 but we expect .000 in other places. Will this be a problem?
			/*AssertEquals(dateTimeOffset3, SqlFormatInfo.FromSqlDateTimeOffset("2003-03-02 13:45:55.0000000 +00:00"));
			AssertEquals(dateTimeOffset3, SqlFormatInfo.FromSqlDateTimeOffset("2003-03-02 13:45:55.0000000 -00:00"));*/

			//Other formats we don't currently support, I guess...
			/*AssertEquals(dateTimeOffset3, SqlFormatInfo.FromSqlDateTimeOffset("2003-03-02 13:45:55 +00:00"));
			AssertEquals(dateTimeOffset3, SqlFormatInfo.FromSqlDateTimeOffset("2003-03-02 13:45:55 -00:00"));
			AssertEquals(dateTimeOffset3, SqlFormatInfo.FromSqlDateTimeOffset("2003-03-02 13:45:55.0000000"));
			AssertEquals(dateTimeOffset3, SqlFormatInfo.FromSqlDateTimeOffset("2003-03-02 13:45:55.000"));
			AssertEquals(dateTimeOffset3, SqlFormatInfo.FromSqlDateTimeOffset("2003-03-02 13:45:55"));*/
		}

		public void TestToAndFromStringAndFormatting()
		{
			ZDateTimeOffset offsetScratch = ZDateTimeOffset.Invalid;

			ZDateTimeOffset dateTimeOffset1 = new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(1));
			AssertEquals("02-Mar-03 13:45:55 +01:00", dateTimeOffset1.ToString());
			AssertEquals(true, ZDateTimeOffset.TryParse(dateTimeOffset1.ToString(), out offsetScratch));
			AssertDateTimeOffsetEquals(dateTimeOffset1, offsetScratch);

			ZDateTimeOffset dateTimeOffset2 = new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(-1));
			AssertEquals("02-Mar-03 13:45:55 -01:00", dateTimeOffset2.ToString());
			AssertEquals(true, ZDateTimeOffset.TryParse(dateTimeOffset2.ToString(), out offsetScratch));
			AssertDateTimeOffsetEquals(dateTimeOffset2, offsetScratch);

			ZDateTimeOffset dateTimeOffset3 = new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, new TimeSpan());
			AssertEquals("02-Mar-03 13:45:55 +00:00", dateTimeOffset3.ToString());
			AssertEquals(true, ZDateTimeOffset.TryParse(dateTimeOffset3.ToString(), out offsetScratch));
			AssertDateTimeOffsetEquals(dateTimeOffset3, offsetScratch);

			ZDateTimeOffset dateTimeOffset4 = new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(1).Add(TimeSpan.FromMinutes(30)));
			AssertEquals("02-Mar-03 13:45:55 +01:30", dateTimeOffset4.ToString());
			AssertEquals(true, ZDateTimeOffset.TryParse(dateTimeOffset4.ToString(), out offsetScratch));
			AssertDateTimeOffsetEquals(dateTimeOffset4, offsetScratch);

			ZDateTimeOffset dateTimeOffset5 = new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(-1).Subtract(TimeSpan.FromMinutes(30)));
			AssertEquals("02-Mar-03 13:45:55 -01:30", dateTimeOffset5.ToString());
			AssertEquals(true, ZDateTimeOffset.TryParse(dateTimeOffset5.ToString(), out offsetScratch));
			AssertDateTimeOffsetEquals(dateTimeOffset5, offsetScratch);

			ZDateTimeOffset dateTimeOffset6 = new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromMinutes(-30));
			AssertEquals(true, ZDateTimeOffset.TryParse(dateTimeOffset6.ToString(), out offsetScratch));
			AssertDateTimeOffsetEquals(dateTimeOffset6, offsetScratch);

			ZDateTimeOffset dateTimeOffset7 = new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromMinutes(-20));
			AssertEquals(true, ZDateTimeOffset.TryParse(dateTimeOffset7.ToString(), out offsetScratch));
			AssertDateTimeOffsetEquals(dateTimeOffset7, offsetScratch);

			ZDateTimeOffset dateTimeOffset8 = new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(-1).Subtract(TimeSpan.FromMinutes(20)));
			AssertEquals(true, ZDateTimeOffset.TryParse(dateTimeOffset8.ToString(), out offsetScratch));
			AssertDateTimeOffsetEquals(dateTimeOffset8, offsetScratch);

			AssertEquals("<Invalid>", ZDateTimeOffset.Invalid.ToString());
			AssertEquals("<Invalid>", ZDateTimeOffset.Invalid.ToString(null));
			AssertEquals("<Invalid>", ZDateTimeOffset.Invalid.ToString(null, null));
			AssertEquals("<Invalid>", ZDateTimeOffset.Invalid.ToString(null, CultureInfo.InvariantCulture));
			AssertEquals("03-Mar-02 01:45:55 +01:00", dateTimeOffset1.ToString("yy-MMM-dd hh:mm:ss zzz", CultureInfo.InvariantCulture));
			AssertEquals("03-Mar-02 01:45:55 +01:00", dateTimeOffset1.ToString("yy-MMM-dd hh:mm:ss zzz", null));
			AssertEquals("03-Mar-02 01:45:55 +01:00", dateTimeOffset1.ToString("yy-MMM-dd hh:mm:ss zzz"));
		}

		public void TestToISO8601String()
		{
			ZDateTimeOffset dateTimeOffsetScratch = ZDateTimeOffset.Invalid;

			ZDateTimeOffset dateTimeOffset1 = new ZDateTimeOffset(2004, 11, 25, 15, 25, 59, 123, TimeSpan.FromHours(11));
			AssertEquals("2004-11-25T15:25:59.1230000+11:00", dateTimeOffset1.ToISO8601String());
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(ZDateTimeOffset.Invalid, "2004-11-25T15:25:59.123+11:00", out dateTimeOffsetScratch));
			AssertDateTimeOffsetEquals(dateTimeOffset1, dateTimeOffsetScratch);

			ZDateTimeOffset dateTimeOffset2 = new ZDateTimeOffset(2004, 11, 25, 15, 25, 59, 123, TimeSpan.FromHours(-11));
			AssertEquals("2004-11-25T15:25:59.1230000-11:00", dateTimeOffset2.ToISO8601String());
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(ZDateTimeOffset.Invalid, "2004-11-25T15:25:59.123-11:00", out dateTimeOffsetScratch));
			AssertDateTimeOffsetEquals(dateTimeOffset2, dateTimeOffsetScratch);

			ZDateTimeOffset dateTimeOffset3 = new ZDateTimeOffset(2004, 11, 25, 15, 25, 59, 123, TimeSpan.FromMinutes(30));
			AssertEquals("2004-11-25T15:25:59.1230000+00:30", dateTimeOffset3.ToISO8601String());
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(ZDateTimeOffset.Invalid, "2004-11-25T15:25:59.123+00:30", out dateTimeOffsetScratch));
			AssertDateTimeOffsetEquals(dateTimeOffset3, dateTimeOffsetScratch);

			ZDateTimeOffset dateTimeOffset4 = new ZDateTimeOffset(2004, 11, 25, 15, 25, 59, 123, TimeSpan.FromMinutes(-30));
			AssertEquals("2004-11-25T15:25:59.1230000-00:30", dateTimeOffset4.ToISO8601String());
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(ZDateTimeOffset.Invalid, "2004-11-25T15:25:59.123-00:30", out dateTimeOffsetScratch));
			AssertDateTimeOffsetEquals(dateTimeOffset4, dateTimeOffsetScratch);

			ZDateTimeOffset dateTimeOffset5 = new ZDateTimeOffset(2004, 11, 25, 15, 25, 59, 123, TimeSpan.FromHours(1).Add(TimeSpan.FromMinutes(30)));
			AssertEquals("2004-11-25T15:25:59.1230000+01:30", dateTimeOffset5.ToISO8601String());
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(ZDateTimeOffset.Invalid, "2004-11-25T15:25:59.123+01:30", out dateTimeOffsetScratch));
			AssertDateTimeOffsetEquals(dateTimeOffset5, dateTimeOffsetScratch);

			ZDateTimeOffset dateTimeOffset6 = new ZDateTimeOffset(2004, 11, 25, 15, 25, 59, 123, TimeSpan.FromHours(-1).Add(TimeSpan.FromMinutes(-30)));
			AssertEquals("2004-11-25T15:25:59.1230000-01:30", dateTimeOffset6.ToISO8601String());
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(ZDateTimeOffset.Invalid, "2004-11-25T15:25:59.123-01:30", out dateTimeOffsetScratch));
			AssertDateTimeOffsetEquals(dateTimeOffset6, dateTimeOffsetScratch);

			ZDateTimeOffset dateTimeOffset7 = new ZDateTimeOffset(2004, 11, 25, 15, 25, 59, 123, TimeSpan.Zero);
			AssertEquals("2004-11-25T15:25:59.1230000+00:00", dateTimeOffset7.ToISO8601String());
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(ZDateTimeOffset.Invalid, "2004-11-25T15:25:59.123Z", out dateTimeOffsetScratch));
			AssertDateTimeOffsetEquals(dateTimeOffset7, dateTimeOffsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(ZDateTimeOffset.Invalid, "2004-11-25T15:25:59.123+00:00", out dateTimeOffsetScratch));
			AssertDateTimeOffsetEquals(dateTimeOffset7, dateTimeOffsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(ZDateTimeOffset.Invalid, "2004-11-25T15:25:59.1230000-00:00", out dateTimeOffsetScratch));
			AssertDateTimeOffsetEquals(dateTimeOffset7, dateTimeOffsetScratch);

			AssertEquals("<Invalid>", ZDateTimeOffset.Invalid.ToISO8601String());
			AssertEquals("", ZDateTimeOffset.Empty.ToISO8601String());
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestTryParseForUpdate()
		{
			ZDateTimeOffset offsetScratch = ZDateTimeOffset.Invalid;

			ZDateTimeOffset previousOffset1 = new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(1));
			ZDateTimeOffset previousOffset2 = new ZDateTimeOffset(2003, 04, 03, 14, 46, 56, TimeSpan.Zero);

			//null, empty

			AssertEquals(true, ZDateTimeOffset.TryParse("", out offsetScratch));
			AssertDateTimeOffsetEquals(ZDateTimeOffset.Empty, offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(ZDateTimeOffset.Empty, "", out offsetScratch));
			AssertDateTimeOffsetEquals(ZDateTimeOffset.Empty, offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParse(null, out offsetScratch));
			AssertDateTimeOffsetEquals(ZDateTimeOffset.Empty, offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(previousOffset1, null, out offsetScratch));
			AssertDateTimeOffsetEquals(ZDateTimeOffset.Empty, offsetScratch);

			//invalid

			AssertEquals(false, ZDateTimeOffset.TryParse("fdgfdjkhgjkdf", out offsetScratch));
			AssertDateTimeOffsetEquals(ZDateTimeOffset.Invalid, offsetScratch);
			AssertEquals(false, ZDateTimeOffset.TryParseForUpdate(ZDateTimeOffset.Empty, "fdgfdjkhgjkdf", out offsetScratch));
			AssertDateTimeOffsetEquals(ZDateTimeOffset.Invalid, offsetScratch);
			AssertEquals(false, ZDateTimeOffset.TryParse("111111111", out offsetScratch));
			AssertDateTimeOffsetEquals(ZDateTimeOffset.Invalid, offsetScratch);
			AssertEquals(false, ZDateTimeOffset.TryParseForUpdate(previousOffset1, "111111111", out offsetScratch));
			AssertDateTimeOffsetEquals(ZDateTimeOffset.Invalid, offsetScratch);

			AssertEquals(false, ZDateTimeOffset.TryParseForUpdate(ZDateTimeOffset.Empty, "3/28/2007 12:13:50 PM -07:00", out offsetScratch)); //invalid for current culture
			AssertDateTimeOffsetEquals(ZDateTimeOffset.Invalid, offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(ZDateTimeOffset.Empty, "3/28/2007 12:13:50 PM -07:00", out offsetScratch, CultureInfo.InvariantCulture)); //valid if you pass invariant culture
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(-7)), offsetScratch);

			//standard formats with offset (including +00:00)

			AssertEquals(true, ZDateTimeOffset.TryParse("28/03/2007 12:13:50 PM -07:00", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(-7)), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(ZDateTimeOffset.Empty, "28/03/2007 12:13:50 PM -07:00", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(-7)), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(ZDateTimeOffset.Invalid, "28/03/2007 12:13:50 PM -07:00", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(-7)), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(previousOffset1, "28/03/2007 12:13:50 PM -07:00", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(-7)), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(previousOffset2, "28/03/2007 12:13:50 PM -07:00", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(-7)), offsetScratch);

			AssertEquals(true, ZDateTimeOffset.TryParse("2007-03-28 12:13:50.000 -07:00", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(-7)), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(ZDateTimeOffset.Empty, "2007-03-28 12:13:50.000 -07:00", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(-7)), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(ZDateTimeOffset.Invalid, "2007-03-28 12:13:50.000 -07:00", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(-7)), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(previousOffset1, "2007-03-28 12:13 -07:00", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 00, TimeSpan.FromHours(-7)), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(previousOffset2, "2007-03-28 12:13:50.000 -07:00", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(-7)), offsetScratch);

			AssertEquals(true, ZDateTimeOffset.TryParse("28/03/2007 12:13:50 PM +00:00", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.Zero), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(ZDateTimeOffset.Empty, "28/03/2007 12:13:50 PM +00:00", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.Zero), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(ZDateTimeOffset.Invalid, "28/03/2007 12:13:50 PM +00:00", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.Zero), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(previousOffset1, "28/03/2007 12:13:50 PM +00:00", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.Zero), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(previousOffset2, "28/03/2007 12:13:50 PM +00:00", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.Zero), offsetScratch);

			AssertEquals(true, ZDateTimeOffset.TryParse("28/03/2007 12:13:50 PM +8:00", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(8)), offsetScratch);

			//standard formats without offset

			AssertEquals(true, ZDateTimeOffset.TryParse("28/03/2007 12:13:50 PM", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(10)), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(ZDateTimeOffset.Empty, "28/03/2007 12:13:50 PM", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(10)), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(ZDateTimeOffset.Invalid, "28/03/2007 12:13:50 PM", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(10)), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(previousOffset1, "28/03/2007 12:13 PM", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 00, TimeSpan.FromHours(1)), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(previousOffset2, "28/03/2007 12:13:50 PM", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.Zero), offsetScratch);

			AssertEquals(true, ZDateTimeOffset.TryParse("2007-03-28 12:13:50", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(10)), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(ZDateTimeOffset.Empty, "2007-03-28 12:13:50", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(10)), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(ZDateTimeOffset.Invalid, "2007-03-28 12:13:50", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(10)), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(previousOffset1, "2007-03-28 12:13", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 00, TimeSpan.FromHours(1)), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(previousOffset2, "2007-03-28 12:13:50", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.Zero), offsetScratch);

			//CW1 display format. Used in xls export.

			AssertEquals(true, ZDateTimeOffset.TryParse("28-MAR-07 12:13:50", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(10)), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(ZDateTimeOffset.Empty, "28-MAR-07 12:13:50", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(10)), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(ZDateTimeOffset.Invalid, "28-MAR-07 12:13:50", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(10)), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(previousOffset1, "28-MAR-07 12:13", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 00, TimeSpan.FromHours(1)), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(previousOffset2, "28-MAR-07 12:13:50", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.Zero), offsetScratch);

			AssertEquals(true, ZDateTimeOffset.TryParse("28-MAR-91 12:13:50", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(1991, 3, 28, 12, 13, 50, TimeSpan.FromHours(10)), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(ZDateTimeOffset.Empty, "28-MAR-91 12:13:50", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(1991, 3, 28, 12, 13, 50, TimeSpan.FromHours(10)), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(ZDateTimeOffset.Invalid, "28-MAR-91", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(1991, 3, 28, 00, 00, 00, TimeSpan.FromHours(10)), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(previousOffset1, "28-MAR-91 12:13", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(1991, 3, 28, 12, 13, 00, TimeSpan.FromHours(1)), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(previousOffset2, "28-MAR-91 12:13:50", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(1991, 3, 28, 12, 13, 50, TimeSpan.Zero), offsetScratch);

			//ISO format: 2007-10-31T21:00:00 , 2016-01-28T23:28:00+11:00. Used in XML export/import

			AssertEquals(true, ZDateTimeOffset.TryParse("2007-03-28T12:13:50", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(10)), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(ZDateTimeOffset.Empty, "2007-03-28T12:13:50", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(10)), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(ZDateTimeOffset.Invalid, "2007-03-28T12:13:50", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(10)), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(previousOffset1, "2007-03-28T12:13:50", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(1)), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(previousOffset2, "2007-03-28T12:13:50", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.Zero), offsetScratch);

			AssertEquals(true, ZDateTimeOffset.TryParse("2007-03-28T12:13:50+00:00", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.Zero), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(ZDateTimeOffset.Empty, "2007-03-28T12:13:50+00:00", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.Zero), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(ZDateTimeOffset.Invalid, "2007-03-28T12:13:50-00:00", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.Zero), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(previousOffset1, "2007-03-28T12:13:50+00:00", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.Zero), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(previousOffset2, "2007-03-28T12:13:50-00:00", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.Zero), offsetScratch);

			AssertEquals(true, ZDateTimeOffset.TryParse("2007-03-28T12:13:50-07:00", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(-7)), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(ZDateTimeOffset.Empty, "2007-03-28T12:13:50-07:00", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(-7)), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(ZDateTimeOffset.Invalid, "2007-03-28T12:13:50+07:00", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(7)), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(previousOffset1, "2007-03-28T12:13:50-07:00", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(-7)), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(previousOffset2, "2007-03-28T12:13:50+07:00", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(7)), offsetScratch);

			AssertEquals(true, ZDateTimeOffset.TryParse("2004-11-25T15:25:59.123+11:00", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2004, 11, 25, 15, 25, 59, 123, TimeSpan.FromHours(11)), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(ZDateTimeOffset.Empty, "2004-11-25T15:25:59.123+11:00", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2004, 11, 25, 15, 25, 59, 123, TimeSpan.FromHours(11)), offsetScratch);

			//"u" format: These are UTC. 2007-11-01 05:00:00Z , 2016-01-28T22:29:01.32Z. Used in XML export/import

			AssertEquals(true, ZDateTimeOffset.TryParse("2008-03-28 12:13:50Z", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2008, 3, 28, 12, 13, 50, TimeSpan.Zero), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(ZDateTimeOffset.Empty, "2008-03-28 12:13:50Z", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2008, 3, 28, 12, 13, 50, TimeSpan.Zero), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(ZDateTimeOffset.Invalid, "2008-03-28 12:13:50Z", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2008, 3, 28, 12, 13, 50, TimeSpan.Zero), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(previousOffset1, "2008-03-28 12:13:50Z", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2008, 3, 28, 12, 13, 50, TimeSpan.Zero), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(previousOffset2, "2008-03-28 12:13:50Z", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2008, 3, 28, 12, 13, 50, TimeSpan.Zero), offsetScratch);

			AssertEquals(true, ZDateTimeOffset.TryParse("2008-03-28T12:13:50Z", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2008, 3, 28, 12, 13, 50, TimeSpan.Zero), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(ZDateTimeOffset.Empty, "2008-03-28T12:13:50Z", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2008, 3, 28, 12, 13, 50, TimeSpan.Zero), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(ZDateTimeOffset.Invalid, "2008-03-28T12:13:50Z", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2008, 3, 28, 12, 13, 50, TimeSpan.Zero), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(previousOffset1, "2008-03-28T12:13:50Z", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2008, 3, 28, 12, 13, 50, TimeSpan.Zero), offsetScratch);
			AssertEquals(true, ZDateTimeOffset.TryParseForUpdate(previousOffset2, "2008-03-28T12:13:50Z", out offsetScratch));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2008, 3, 28, 12, 13, 50, TimeSpan.Zero), offsetScratch);
		}

		public void TestFormatDateTimeOffset()
		{
			ZDateTimeOffset dateTimeOffset1 = new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(1));
			AssertEquals("Format should be DD-Mon-YY hh:mm [{+|-}hh:mm]", "02-Mar-03 13:45 +01:00", dateTimeOffset1.FormatDateTimeOffset());
			ZDateTimeOffset dateTimeOffset2 = new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(-1));
			AssertEquals("Format should be DD-Mon-YY hh:mm [{+|-}hh:mm]", "02-Mar-03 13:45 -01:00", dateTimeOffset2.FormatDateTimeOffset());
			ZDateTimeOffset dateTimeOffset3 = new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, new TimeSpan());
			AssertEquals("Format should be DD-Mon-YY hh:mm [{+|-}hh:mm]", "02-Mar-03 13:45 +00:00", dateTimeOffset3.FormatDateTimeOffset());
			ZDateTimeOffset dateTimeOffset4 = new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(1).Add(TimeSpan.FromMinutes(30)));
			AssertEquals("Format should be DD-Mon-YY hh:mm [{+|-}hh:mm]", "02-Mar-03 13:45 +01:30", dateTimeOffset4.FormatDateTimeOffset());
			ZDateTimeOffset dateTimeOffset5 = new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(-1).Subtract(TimeSpan.FromMinutes(30)));
			AssertEquals("Format should be DD-Mon-YY hh:mm [{+|-}hh:mm]", "02-Mar-03 13:45 -01:30", dateTimeOffset5.FormatDateTimeOffset());
			ZDateTimeOffset dateTimeOffset6 = new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromMinutes(-30));
			AssertEquals("Format should be DD-Mon-YY hh:mm [{+|-}hh:mm]", "02-Mar-03 13:45 -00:30", dateTimeOffset6.FormatDateTimeOffset());
			ZDateTimeOffset dateTimeOffset7 = new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromMinutes(-20));
			AssertEquals("Format should be DD-Mon-YY hh:mm [{+|-}hh:mm]", "02-Mar-03 13:45 -00:20", dateTimeOffset7.FormatDateTimeOffset());
			ZDateTimeOffset dateTimeOffset8 = new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(-1).Subtract(TimeSpan.FromMinutes(20)));
			AssertEquals("Format should be DD-Mon-YY hh:mm [{+|-}hh:mm]", "02-Mar-03 13:45 -01:20", dateTimeOffset8.FormatDateTimeOffset());
		}

		public void TestFormatDateTimeOffsetWithSeconds()
		{
			ZDateTimeOffset dateTimeOffset1 = new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(1));
			AssertEquals("Format should be DD-Mon-YY hh:mm:ss [{+|-}hh:mm]", "02-Mar-03 13:45:55 +01:00", dateTimeOffset1.FormatDateTimeOffsetWithSeconds());
			ZDateTimeOffset dateTimeOffset2 = new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(-1));
			AssertEquals("Format should be DD-Mon-YY hh:mm:ss [{+|-}hh:mm]", "02-Mar-03 13:45:55 -01:00", dateTimeOffset2.FormatDateTimeOffsetWithSeconds());
			ZDateTimeOffset dateTimeOffset3 = new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, new TimeSpan());
			AssertEquals("Format should be DD-Mon-YY hh:mm:ss [{+|-}hh:mm]", "02-Mar-03 13:45:55 +00:00", dateTimeOffset3.FormatDateTimeOffsetWithSeconds());
			ZDateTimeOffset dateTimeOffset4 = new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(1).Add(TimeSpan.FromMinutes(30)));
			AssertEquals("Format should be DD-Mon-YY hh:mm:ss [{+|-}hh:mm]", "02-Mar-03 13:45:55 +01:30", dateTimeOffset4.FormatDateTimeOffsetWithSeconds());
			ZDateTimeOffset dateTimeOffset5 = new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(-1).Subtract(TimeSpan.FromMinutes(30)));
			AssertEquals("Format should be DD-Mon-YY hh:mm:ss [{+|-}hh:mm]", "02-Mar-03 13:45:55 -01:30", dateTimeOffset5.FormatDateTimeOffsetWithSeconds());
			ZDateTimeOffset dateTimeOffset6 = new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromMinutes(-30));
			AssertEquals("Format should be DD-Mon-YY hh:mm:ss [{+|-}hh:mm]", "02-Mar-03 13:45:55 -00:30", dateTimeOffset6.FormatDateTimeOffsetWithSeconds());
			ZDateTimeOffset dateTimeOffset7 = new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromMinutes(-20));
			AssertEquals("Format should be DD-Mon-YY hh:mm:ss [{+|-}hh:mm]", "02-Mar-03 13:45:55 -00:20", dateTimeOffset7.FormatDateTimeOffsetWithSeconds());
			ZDateTimeOffset dateTimeOffset8 = new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(-1).Subtract(TimeSpan.FromMinutes(20)));
			AssertEquals("Format should be DD-Mon-YY hh:mm:ss [{+|-}hh:mm]", "02-Mar-03 13:45:55 -01:20", dateTimeOffset8.FormatDateTimeOffsetWithSeconds());
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestValueForUpdate()
		{
			ZDateTimeOffset previousOffset1 = new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(1));
			ZDateTimeOffset previousOffset2 = new ZDateTimeOffset(2003, 04, 03, 14, 46, 56, TimeSpan.Zero);

			//invalid and empty

			AssertDateTimeOffsetEquals(ZDateTimeOffset.Empty, ZDateTimeOffset.ValueForUpdate(ZDateTimeOffset.Empty, ZDateTimeOffset.Empty));
			AssertDateTimeOffsetEquals(ZDateTimeOffset.Invalid, ZDateTimeOffset.ValueForUpdate(ZDateTimeOffset.Empty, ZDateTimeOffset.Invalid));
			AssertDateTimeOffsetEquals(ZDateTimeOffset.Empty, ZDateTimeOffset.ValueForUpdate(ZDateTimeOffset.Empty, ZDateTime.Empty));
			AssertDateTimeOffsetEquals(ZDateTimeOffset.Invalid, ZDateTimeOffset.ValueForUpdate(ZDateTimeOffset.Empty, ZDateTime.Invalid));
			AssertDateTimeOffsetEquals(ZDateTimeOffset.Empty, ZDateTimeOffset.ValueForUpdate(ZDateTimeOffset.Invalid, ZDateTimeOffset.Empty));
			AssertDateTimeOffsetEquals(ZDateTimeOffset.Invalid, ZDateTimeOffset.ValueForUpdate(ZDateTimeOffset.Invalid, ZDateTimeOffset.Invalid));
			AssertDateTimeOffsetEquals(ZDateTimeOffset.Empty, ZDateTimeOffset.ValueForUpdate(ZDateTimeOffset.Invalid, ZDateTime.Empty));
			AssertDateTimeOffsetEquals(ZDateTimeOffset.Invalid, ZDateTimeOffset.ValueForUpdate(ZDateTimeOffset.Invalid, ZDateTime.Invalid));
			AssertDateTimeOffsetEquals(ZDateTimeOffset.Empty, ZDateTimeOffset.ValueForUpdate(previousOffset1, ZDateTimeOffset.Empty));
			AssertDateTimeOffsetEquals(ZDateTimeOffset.Invalid, ZDateTimeOffset.ValueForUpdate(previousOffset1, ZDateTimeOffset.Invalid));
			AssertDateTimeOffsetEquals(ZDateTimeOffset.Empty, ZDateTimeOffset.ValueForUpdate(previousOffset1, ZDateTime.Empty));
			AssertDateTimeOffsetEquals(ZDateTimeOffset.Invalid, ZDateTimeOffset.ValueForUpdate(previousOffset1, ZDateTime.Invalid));

			///ZDateTimeOffset

			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2003, 05, 05, 10, 11, 12, TimeSpan.FromHours(5)), ZDateTimeOffset.ValueForUpdate(ZDateTimeOffset.Empty, new ZDateTimeOffset(2003, 05, 05, 10, 11, 12, TimeSpan.FromHours(5))));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2003, 05, 05, 10, 11, 12, TimeSpan.FromHours(5)), ZDateTimeOffset.ValueForUpdate(ZDateTimeOffset.Invalid, new ZDateTimeOffset(2003, 05, 05, 10, 11, 12, TimeSpan.FromHours(5))));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2003, 05, 05, 10, 11, 12, TimeSpan.FromHours(1)), ZDateTimeOffset.ValueForUpdate(previousOffset1, new ZDateTimeOffset(2003, 05, 05, 10, 11, 12, TimeSpan.FromHours(5))));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2003, 05, 05, 10, 11, 12, TimeSpan.Zero), ZDateTimeOffset.ValueForUpdate(previousOffset2, new ZDateTimeOffset(2003, 05, 05, 10, 11, 12, TimeSpan.FromHours(5))));

			//ZDateTimeOffset with offset 00:00 not treated specially because it could be a local time in that time zone.

			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2003, 05, 05, 10, 11, 12, TimeSpan.Zero), ZDateTimeOffset.ValueForUpdate(ZDateTimeOffset.Empty, new ZDateTimeOffset(2003, 05, 05, 10, 11, 12, TimeSpan.Zero)));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2003, 05, 05, 10, 11, 12, TimeSpan.Zero), ZDateTimeOffset.ValueForUpdate(ZDateTimeOffset.Invalid, new ZDateTimeOffset(2003, 05, 05, 10, 11, 12, TimeSpan.Zero)));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2003, 05, 05, 10, 11, 12, TimeSpan.FromHours(1)), ZDateTimeOffset.ValueForUpdate(previousOffset1, new ZDateTimeOffset(2003, 05, 05, 10, 11, 12, TimeSpan.Zero)));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2003, 05, 05, 10, 11, 12, TimeSpan.Zero), ZDateTimeOffset.ValueForUpdate(previousOffset2, new ZDateTimeOffset(2003, 05, 05, 10, 11, 12, TimeSpan.Zero)));

			//ZDateTime (non-UTC)

			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2003, 05, 05, 10, 11, 12, TimeSpan.FromHours(10)), ZDateTimeOffset.ValueForUpdate(ZDateTimeOffset.Empty, new ZDateTime(2003, 05, 05, 10, 11, 12)));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2003, 05, 05, 10, 11, 12, TimeSpan.FromHours(10)), ZDateTimeOffset.ValueForUpdate(ZDateTimeOffset.Invalid, new ZDateTime(2003, 05, 05, 10, 11, 12)));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2003, 05, 05, 10, 11, 12, TimeSpan.FromHours(1)), ZDateTimeOffset.ValueForUpdate(previousOffset1, new ZDateTime(2003, 05, 05, 10, 11, 12)));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2003, 05, 05, 10, 11, 12, TimeSpan.Zero), ZDateTimeOffset.ValueForUpdate(previousOffset2, new ZDateTime(2003, 05, 05, 10, 11, 12)));

			//ZDateTime (UTC)

			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2003, 05, 05, 10, 11, 12, TimeSpan.Zero), ZDateTimeOffset.ValueForUpdate(ZDateTimeOffset.Empty, new ZDateTime(2003, 05, 05, 10, 11, 12, DateTimeKind.Utc)));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2003, 05, 05, 10, 11, 12, TimeSpan.Zero), ZDateTimeOffset.ValueForUpdate(ZDateTimeOffset.Invalid, new ZDateTime(2003, 05, 05, 10, 11, 12, DateTimeKind.Utc)));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2003, 05, 05, 10, 11, 12, TimeSpan.Zero), ZDateTimeOffset.ValueForUpdate(previousOffset1, new ZDateTime(2003, 05, 05, 10, 11, 12, DateTimeKind.Utc)));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2003, 05, 05, 10, 11, 12, TimeSpan.Zero), ZDateTimeOffset.ValueForUpdate(previousOffset2, new ZDateTime(2003, 05, 05, 10, 11, 12, DateTimeKind.Utc)));
		}

		public void TestCastsAndConversions()
		{
			Assert(new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(1)).ToDateTimeOffset() == new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(1)));
			Assert(new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(1)).ToDateTimeOffset() == (ZDateTimeOffset)(new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(1))));
			Assert(new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(1)) == (ZDateTimeOffset)(new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(1))));
			Assert(new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(1)) == new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(1)).ToDateTimeOffset());
			Assert(new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(1)).ToUtcZDateTime() == new ZDateTime(2003, 03, 02, 12, 45, 55, DateTimeKind.Utc));
			Assert(new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(1)).ToZDateTime() == new ZDateTime(2003, 03, 02, 13, 45, 55, DateTimeKind.Unspecified));
		}

		[TestDate(2017, 07, 03)]
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestLocalTime()
		{
			TestDateAttribute.UseUNLOCO = true;
			AssertEquals(new ZDateTime(2003, 03, 01, 14, 45, 55, DateTimeKind.Local), new ZDateTimeOffset(2003, 03, 01, 6, 45, 55, TimeSpan.FromHours(3)).ToLocalZDateTime());
		}

		[TestDate(2017, 07, 03)]
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestLocalTime_UTC0()
		{
			TestDateAttribute.UseUNLOCO = true;
			AssertEquals(new ZDateTime(2003, 03, 01, 14, 45, 55, DateTimeKind.Local), new ZDateTimeOffset(2003, 03, 01, 3, 45, 55, TimeSpan.FromHours(0)).ToLocalZDateTime());
		}

		public void TestDifferentMinutes()
		{
			var d1 = new ZDateTimeOffset(2003, 03, 02, 6, 45, 55, TimeSpan.FromHours(3));
			var d2 = new ZDateTimeOffset(2003, 03, 02, 6, 49, 55, TimeSpan.FromHours(3));
			var d3 = new ZDateTimeOffset(2003, 03, 02, 6, 49, 55, TimeSpan.FromHours(1));
			var d4 = new ZDateTimeOffset(2003, 03, 02, 3, 45, 55, TimeSpan.FromHours(0));
			AssertEquals(false, ZDateTimeOffset.DifferentMinutes(d1, d1));
			AssertEquals(true, ZDateTimeOffset.DifferentMinutes(d1, d2));
			AssertEquals(true, ZDateTimeOffset.DifferentMinutes(d2, d1));
			AssertEquals(true, ZDateTimeOffset.DifferentMinutes(d2, d3));
			AssertEquals(true, ZDateTimeOffset.DifferentMinutes(d3, d2));
		}

		[TestDate(2000, 6, 6, 12, 0, 0)]
		public void TestTryPassExact_TimeOnly()
		{
			for (var i = -12; i < 13; ++i)
			{
				var today = new ZDateTimeOffset(ObjectCache.DateTimeProvider.CurrentLocalDate, new TimeSpan(i, 0, 0));
				var success = ZDateTimeOffset.TryParseExact("16:17", out var parsedDateTime, "mm:ss");
				AssertEquals(expected: true, success);
				AssertEquals(new ZDateTimeOffset(today.Year, today.Month, today.Day, 0, 16, 17), parsedDateTime);

				success = ZDateTimeOffset.TryParseExact("16:17:18", out parsedDateTime, "HH:mm:ss");
				AssertEquals(expected: true, success);
				AssertEquals(new ZDateTimeOffset(today.Year, today.Month, today.Day, 16, 17, 18), parsedDateTime);

				success = ZDateTimeOffset.TryParseExact("16:17", out parsedDateTime, "HH:mm");
				AssertEquals(expected: true, success);
				AssertEquals(new ZDateTimeOffset(today.Year, today.Month, today.Day, 16, 17, 0), parsedDateTime);
			}
		}

		public void TestEquality()
		{
			//Comparison is done by UTC time.

			Assert(ZDateTimeOffset.Invalid == ZDateTimeOffset.Invalid);
			Assert(ZDateTimeOffset.Empty == new ZDateTimeOffset());
			Assert(new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(1)) == new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(1)));
			Assert(new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(1)) == new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(1)));
			Assert(new ZDateTimeOffset(2003, 03, 02, 14, 45, 55, TimeSpan.FromHours(1)) == new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(0)));
			Assert(new ZDateTimeOffset(2003, 03, 02, 14, 45, 55, TimeSpan.FromHours(1)) == new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(0)));
			Assert(new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(1)) != new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(-1)));
			Assert(new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(1)) != new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(-1)));
			Assert(new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(1)) != new DateTimeOffset(2003, 03, 02, 13, 45, 56, TimeSpan.FromHours(1)));
			Assert(new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(1)) != new ZDateTimeOffset(2003, 03, 02, 13, 45, 56, TimeSpan.FromHours(1)));
		}

		public void TestComparisons()
		{
			//Comparison is done by UTC time.

			AssertEquals(-1, new ZDateTimeOffset(2003, 03, 02, 12, 45, 55, TimeSpan.FromHours(0)).CompareTo(new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(-1))));
			AssertEquals(-1, new ZDateTimeOffset(2003, 03, 02, 12, 45, 55, TimeSpan.FromHours(0)).CompareTo(new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(0))));
			AssertEquals(0, new ZDateTimeOffset(2003, 03, 02, 12, 45, 55, TimeSpan.FromHours(0)).CompareTo(new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(1))));
			AssertEquals(-1, new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(0)).CompareTo(new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(-1))));
			AssertEquals(0, new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(0)).CompareTo(new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(0))));
			AssertEquals(1, new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(0)).CompareTo(new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(1))));
			AssertEquals(0, new ZDateTimeOffset(2003, 03, 02, 14, 45, 55, TimeSpan.FromHours(0)).CompareTo(new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(-1))));
			AssertEquals(1, new ZDateTimeOffset(2003, 03, 02, 14, 45, 55, TimeSpan.FromHours(0)).CompareTo(new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(0))));
			AssertEquals(1, new ZDateTimeOffset(2003, 03, 02, 14, 45, 55, TimeSpan.FromHours(0)).CompareTo(new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(1))));

			Assert(new ZDateTimeOffset(2003, 03, 02, 12, 45, 55, TimeSpan.FromHours(0)) < new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(-1)));
			Assert(new ZDateTimeOffset(2003, 03, 02, 12, 45, 55, TimeSpan.FromHours(0)) <= new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(-1)));
			Assert(new ZDateTimeOffset(2003, 03, 02, 12, 45, 55, TimeSpan.FromHours(0)) < new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(0)));
			Assert(new ZDateTimeOffset(2003, 03, 02, 12, 45, 55, TimeSpan.FromHours(0)) <= new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(0)));
			Assert(new ZDateTimeOffset(2003, 03, 02, 12, 45, 55, TimeSpan.FromHours(0)) == new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(1)));
			Assert(new ZDateTimeOffset(2003, 03, 02, 12, 45, 55, TimeSpan.FromHours(0)) >= new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(1)));
			Assert(new ZDateTimeOffset(2003, 03, 02, 12, 45, 55, TimeSpan.FromHours(0)) <= new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(1)));
			Assert(!(new ZDateTimeOffset(2003, 03, 02, 12, 45, 55, TimeSpan.FromHours(0)) > new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(1))));
			Assert(!(new ZDateTimeOffset(2003, 03, 02, 12, 45, 55, TimeSpan.FromHours(0)) < new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(1))));
			Assert(new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(0)) < new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(-1)));
			Assert(new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(0)) <= new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(-1)));
			Assert(new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(0)) == new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(0)));
			Assert(!(new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(0)) > new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(0))));
			Assert(!(new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(0)) < new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(0))));
			Assert(new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(0)) > new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(1)));
			Assert(new ZDateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(0)) >= new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(1)));
			Assert(new ZDateTimeOffset(2003, 03, 02, 14, 45, 55, TimeSpan.FromHours(0)) == new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(-1)));
			Assert(!(new ZDateTimeOffset(2003, 03, 02, 14, 45, 55, TimeSpan.FromHours(0)) > new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(-1))));
			Assert(!(new ZDateTimeOffset(2003, 03, 02, 14, 45, 55, TimeSpan.FromHours(0)) < new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(-1))));
			Assert(new ZDateTimeOffset(2003, 03, 02, 14, 45, 55, TimeSpan.FromHours(0)) > new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(0)));
			Assert(new ZDateTimeOffset(2003, 03, 02, 14, 45, 55, TimeSpan.FromHours(0)) >= new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(0)));
			Assert(new ZDateTimeOffset(2003, 03, 02, 14, 45, 55, TimeSpan.FromHours(0)) > new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(1)));
			Assert(new ZDateTimeOffset(2003, 03, 02, 14, 45, 55, TimeSpan.FromHours(0)) >= new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(1)));
		}

		public void TestDateAndOffset()
		{
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2003, 03, 02, 0, 0, 0, TimeSpan.FromHours(-1)), new ZDateTimeOffset(2003, 03, 02, 12, 45, 55, TimeSpan.FromHours(-1)).DateAndOffset);
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2003, 03, 02, 0, 0, 0, TimeSpan.FromHours(0)), new ZDateTimeOffset(2003, 03, 02, 12, 45, 55, TimeSpan.FromHours(0)).DateAndOffset);
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2003, 03, 02, 0, 0, 0, TimeSpan.FromHours(1)), new ZDateTimeOffset(2003, 03, 02, 12, 45, 55, TimeSpan.FromHours(1)).DateAndOffset);
			AssertDateTimeOffsetEquals(new DateTimeOffset(2003, 03, 02, 0, 0, 0, TimeSpan.FromHours(-1)), ZDateTimeOffset.DateAndOffsetHelper(new DateTimeOffset(2003, 03, 02, 12, 45, 55, TimeSpan.FromHours(-1))));
			AssertDateTimeOffsetEquals(new DateTimeOffset(2003, 03, 02, 0, 0, 0, TimeSpan.FromHours(0)), ZDateTimeOffset.DateAndOffsetHelper(new DateTimeOffset(2003, 03, 02, 12, 45, 55, TimeSpan.FromHours(0))));
			AssertDateTimeOffsetEquals(new DateTimeOffset(2003, 03, 02, 0, 0, 0, TimeSpan.FromHours(1)), ZDateTimeOffset.DateAndOffsetHelper(new DateTimeOffset(2003, 03, 02, 12, 45, 55, TimeSpan.FromHours(1))));
		}

		public void TestKeepUtcChangeOffset()
		{
			var offset1 = new ZDateTimeOffset(2003, 03, 02, 12, 45, 55, TimeSpan.FromHours(11));

			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2003, 03, 02, 1, 45, 55, TimeSpan.Zero), offset1.KeepUtcChangeOffset(TimeSpan.Zero));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2003, 03, 02, 2, 45, 55, TimeSpan.FromHours(1)), offset1.KeepUtcChangeOffset(TimeSpan.FromHours(1)));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2003, 03, 02, 12, 45, 55, TimeSpan.FromHours(11)), offset1.KeepUtcChangeOffset(TimeSpan.FromHours(11)));
			AssertDateTimeOffsetEquals(new ZDateTimeOffset(2003, 03, 01, 14, 45, 55, TimeSpan.FromHours(-11)), offset1.KeepUtcChangeOffset(TimeSpan.FromHours(-11)));
		}

		public void TestExceptionThrownForInvalidOffsets()
		{
			AssertExceptionThrown<ArgumentException>(() => { new ZDateTimeOffset(DateTime.Today, TimeSpan.FromHours(14).Add(TimeSpan.FromSeconds(1))); });
			AssertExceptionThrown<ArgumentException>(() => { new ZDateTimeOffset(DateTime.Today, TimeSpan.FromHours(-14).Add(TimeSpan.FromSeconds(-1))); });
		}

		public void TestExceptionThrownForInvalidCastsAndConversions()
		{
			AssertExceptionThrown<OperationOnInvalidZDateTimeException>(() => { ZDateTimeOffset.Empty.ToDateTime(); });
			AssertExceptionThrown<OperationOnInvalidZDateTimeException>(() => { ZDateTimeOffset.Invalid.ToDateTime(); });
			AssertExceptionThrown<OperationOnInvalidZDateTimeException>(() => { ZDateTimeOffset.Empty.ToDateTimeOffset(); });
			AssertExceptionThrown<OperationOnInvalidZDateTimeException>(() => { ZDateTimeOffset.Invalid.ToDateTimeOffset(); });
			AssertExceptionThrown<OperationOnInvalidZDateTimeException>(() => { ZDateTimeOffset.Empty.ToUtcDateTime(); });
			AssertExceptionThrown<OperationOnInvalidZDateTimeException>(() => { ZDateTimeOffset.Invalid.ToUtcDateTime(); });
		}

		[TestDate(2017, 07, 01, 8, 30, 0)]
		public void TestIsInTheFuture_Or_IsInThePast()
		{
			var timeNow = ZDateTimeOffset.Now;

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

			Assert("ZDateTime.Empty is not in the future, it's empty", !ZDateTimeOffset.Empty.IsInTheFuture());

			Assert("Today one hour ago is in the past", todayOneHourAgo.IsInThePast());
			Assert("yesterday is in the past", yesterday.IsInThePast());
			Assert("tomorrow is not in the past, it's future", !tomorrow.IsInThePast());

			Assert("Minute ago is in the past", minueAgo.IsInThePast());
			Assert("Second ago is in the past", secondAgo.IsInThePast());
			Assert("milliSecondAgo is in the past, it's future", milliSecondAgo.IsInThePast());

			Assert("minuteLater is not in the past, it's future", !minuteLater.IsInThePast());
			Assert("secondLater is not in the past, it's future", !secondLater.IsInThePast());
			Assert("milliSecondLater is not in the past, it's future", !milliSecondLater.IsInThePast());

			Assert("ZDateTime.Empty is not in the past, it's empty", !ZDateTimeOffset.Empty.IsInThePast());
			Assert("ZDateTime.Empty is not in the future, it's empty", !ZDateTimeOffset.Empty.IsInTheFuture());
		}

		public void TestIsValidOffset()
		{
			TestValidTimeSpan(TimeSpan.FromHours(-14));
			TestValidTimeSpan(TimeSpan.FromMinutes(-60));
			TestValidTimeSpan(TimeSpan.FromMinutes(-1));
			TestValidTimeSpan(TimeSpan.FromMinutes(0));
			TestValidTimeSpan(TimeSpan.FromMinutes(1));
			TestValidTimeSpan(TimeSpan.FromMinutes(60));
			TestValidTimeSpan(TimeSpan.FromHours(14));

			// Offsets too large
			TestInvalidTimeSpan(TimeSpan.FromHours(14).Add(TimeSpan.FromMinutes(1)));
			TestInvalidTimeSpan(TimeSpan.FromHours(-14).Add(TimeSpan.FromMinutes(-1)));

			// Offsets not integral number of minutes
			TestInvalidTimeSpan(TimeSpan.FromMinutes(60).Add(TimeSpan.FromSeconds(1)));
			TestInvalidTimeSpan(TimeSpan.FromMinutes(60).Add(TimeSpan.FromSeconds(-1)));
			TestInvalidTimeSpan(TimeSpan.FromMinutes(60).Add(TimeSpan.FromMilliseconds(1)));
			TestInvalidTimeSpan(TimeSpan.FromMinutes(60).Add(TimeSpan.FromMilliseconds(-1)));
			TestInvalidTimeSpan(TimeSpan.FromMinutes(60).Add(TimeSpan.FromTicks(1)));
			TestInvalidTimeSpan(TimeSpan.FromMinutes(60).Add(TimeSpan.FromTicks(-1)));

			void TestValidTimeSpan(TimeSpan offset) =>
				AssertEquals($"{offset} is valid", true, ZDateTimeOffset.IsValidOffset(offset));

			void TestInvalidTimeSpan(TimeSpan offset) =>
				AssertEquals($"{offset} is invalid", false, ZDateTimeOffset.IsValidOffset(offset));
		}

		public void TestIsValidSmallDateTime()
		{
			Assert("MinSmallDateTimeValue", new ZDateTimeOffset(ZDateTime.MinSmallDateTimeValue).IsValidSmallDateTime);
			Assert("One second earlyer than MinSmallDateTimeValue", !new ZDateTimeOffset(ZDateTime.MinSmallDateTimeValue).AddSeconds(-1).IsValidSmallDateTime);
			Assert("MaxSmallDateTimeValue", new ZDateTimeOffset(ZDateTime.MaxSmallDateTimeValue).IsValidSmallDateTime);
			Assert("One seccond after MaxSmallDateTimeValue", !new ZDateTimeOffset(ZDateTime.MaxSmallDateTimeValue).AddSeconds(1).IsValidSmallDateTime);
		}

		public void TestEndOfDay()
		{
			ZDateTimeOffset myBirthday = new ZDateTimeOffset(new ZDateTime(2004, 11, 25), DateTimeKind.Local).EndOfDay();
			AssertEquals("End of Day is at 23:59:59am", myBirthday.Hour, 23);
			AssertEquals("End of Day is at 23:59:59am", myBirthday.Minute, 59);
			AssertEquals("End of Day is at 23:59:59am", myBirthday.Second, 59);

			AssertEquals("Empty Input, Empty Output", ZDateTimeOffset.Empty.EndOfDay(), ZDateTimeOffset.Empty);
		}

		public void TestEndOfDayThrowsOnInvalidValue()
		{
			ZDateTimeOffset invalidDateTimeOffset = new ZDateTimeOffset(new DateTimeOffset(DateTimeOffset.MinValue.Ticks, TimeSpan.Zero));
			AssertExceptionThrown<InvalidOperationException>(() => invalidDateTimeOffset.EndOfDay());
		}

		public virtual void TestToStringEqualsIFormattable()
		{
			foreach (var value in ValidValues)
			{
				var date = new ZDateTimeOffset(value);
				AssertEquals(date.ToString(), string.Format("{0}", date));
			}
		}

		public void TestTruncateMilliseconds()
		{
			var testDate_WithMilliseconds = new ZDateTimeOffset(2015, 06, 16, 3, 59, 10, 500, TimeSpan.FromHours(10));
			var testDate_WithoutMilliseconds = new ZDateTimeOffset(2015, 06, 16, 3, 59, 10, 0, TimeSpan.FromHours(10));

			AssertEquals("Should truncate milliseconds", testDate_WithoutMilliseconds, ZDateTimeOffset.TruncateMilliseconds(testDate_WithMilliseconds));
		}

		public void TestTruncateMilliseconds_InvalidOffset()
		{
			var emptyOffset = ZDateTimeOffset.Empty;
			var invalidOffset = new ZDateTimeOffset(0001, 01, 01);

			AssertExceptionThrown<OperationOnInvalidZDateTimeException>("Cannot truncate an invalid ZDateTimeOffset.", () => ZDateTimeOffset.TruncateMilliseconds(emptyOffset));
			AssertExceptionThrown<OperationOnInvalidZDateTimeException>("Cannot truncate an invalid ZDateTimeOffset.", () => ZDateTimeOffset.TruncateMilliseconds(invalidOffset));
		}

		#region IZTypeTest Overrides
		public override void TestToString()
		{
			foreach (var value in ValidValues)
			{
				AssertEquals(MessageForValue(value), ((DateTimeOffset)value).ToString("dd-MMM-yy HH:mm:ss zzz"), NewZ(value).ToString());
			}
		}

		protected override IZType NewZ(object value)
		{
			return new ZDateTimeOffset(value);
		}

		protected override object[] ValidValues
		{
			get { return new object[] { RecentDate1, RecentDateUtc1, RecentDate2, RecentDateUtc2, new DateTimeOffset(new DateTime(1, DateTimeKind.Utc)), new DateTimeOffset(new DateTime(100, DateTimeKind.Utc)) }; }
		}

		protected override object[] InvalidValues
		{
			get { return new object[] { DateTimeOffset.MinValue, new DateTimeOffset(0, TimeSpan.Zero), new DateTimeOffset(new DateTime(0, DateTimeKind.Utc)) }; }
		}

		protected override object[] EmptyValues
		{
			get { return new object[] { null, DBNull.Value, ZDateTimeOffset.Empty, new ZDateTimeOffset() }; }
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
			var zDatetimeOffset = new ZDateTimeOffset(2021, 3, 18, 1, 1, 2, TimeSpan.FromHours(-10));
			AssertEquals("ZDateTimeOffset type code", TypeCode.Object, ((IConvertible)zDatetimeOffset).GetTypeCode());

			var emptyZDateTimeOffset = ZDateTimeOffset.Empty;
			AssertEquals("ZDateTimeOffset Empty type code", TypeCode.Empty, ((IConvertible)emptyZDateTimeOffset).GetTypeCode());
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestIConvertible_ConvertToZDateTime()
		{
			var zDatetimeOffset = new ZDateTimeOffset(2021, 3, 18, 1, 1, 2);
			var zDateTime = zDatetimeOffset.ToDateTime();

			AssertEquals("ZDateTimeOffset to DateTime conversion", zDateTime, ((IConvertible)zDatetimeOffset).ToType(typeof(ZDateTime), null));
		}

		public void TestIConvertible_ConvertToZDateTimeOffset()
		{
			var zDatetimeOffset = new ZDateTimeOffset(2021, 3, 18, 1, 1, 2, TimeSpan.FromHours(-10));

			AssertEquals("ZDateTimeOffset to DateTime conversion", zDatetimeOffset, ((IConvertible)zDatetimeOffset).ToType(typeof(ZDateTimeOffset), null));
		}

		public void TestIConvertible_ConvertToZDate()
		{
			var zDatetimeOffset = new ZDateTimeOffset(2021, 3, 18, 1, 1, 2, TimeSpan.FromHours(-10));

			AssertEquals("ZDateTimeOffset to DateTime conversion", new ZDate(2021, 3, 18), ((IConvertible)zDatetimeOffset).ToType(typeof(ZDate), null));
		}

		public void TestIConvertible_ConvertToDateTime()
		{
			var zDatetimeOffset = new ZDateTimeOffset(2021, 3, 18, 1, 1, 2, TimeSpan.FromHours(-10));
			var expected = new DateTime(2021, 3, 18, 1, 1, 2);

			AssertEquals("zDatetimeOffset to DateTime conversion", expected, Convert.ToDateTime(zDatetimeOffset));
			AssertEquals("zDatetimeOffset to DateTime conversion", expected, ((IConvertible)zDatetimeOffset).ToType(typeof(DateTime), null));
		}

		public void TestIConvertible_ConvertToString()
		{
			var zDatetimeOffset = new ZDateTimeOffset(2021, 3, 18, 1, 1, 0, TimeSpan.FromHours(-10));
			var expected = "18-Mar-21 01:01:00 -10:00";

			AssertToString(expected, Convert.ToString(zDatetimeOffset));
			AssertToString(expected, ((IConvertible)zDatetimeOffset).ToType(typeof(string), null));
		}

		public void TestIConvertible_ConvertToInvalidTypes()
		{
			ZDateTimeOffset zDatetimeOffset = new ZDateTimeOffset(2021, 3, 18, 1, 1, 2, TimeSpan.FromHours(-10));

			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => Convert.ToBoolean(zDatetimeOffset));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => Convert.ToChar(zDatetimeOffset));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => Convert.ToSByte(zDatetimeOffset));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => Convert.ToByte(zDatetimeOffset));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => Convert.ToInt16(zDatetimeOffset));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => Convert.ToUInt16(zDatetimeOffset));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => Convert.ToInt32(zDatetimeOffset));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => Convert.ToUInt32(zDatetimeOffset));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => Convert.ToInt64(zDatetimeOffset));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => Convert.ToUInt64(zDatetimeOffset));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => Convert.ToSingle(zDatetimeOffset));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => Convert.ToDouble(zDatetimeOffset));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => Convert.ToDecimal(zDatetimeOffset));
		}

		public void TestIConvertible_ConvertToInvalidTypesUsingToType()
		{
			ZDateTimeOffset zDatetimeOffset = new ZDateTimeOffset(2021, 3, 18, 1, 1, 2,TimeSpan.FromHours(-10));

			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => ((IConvertible)zDatetimeOffset).ToType(typeof(bool), null));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => ((IConvertible)zDatetimeOffset).ToType(typeof(char), null));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => ((IConvertible)zDatetimeOffset).ToType(typeof(sbyte), null));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => ((IConvertible)zDatetimeOffset).ToType(typeof(byte), null));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => ((IConvertible)zDatetimeOffset).ToType(typeof(short), null));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => ((IConvertible)zDatetimeOffset).ToType(typeof(ushort), null));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => ((IConvertible)zDatetimeOffset).ToType(typeof(int), null));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => ((IConvertible)zDatetimeOffset).ToType(typeof(long), null));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => ((IConvertible)zDatetimeOffset).ToType(typeof(ulong), null));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => ((IConvertible)zDatetimeOffset).ToType(typeof(float), null));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => ((IConvertible)zDatetimeOffset).ToType(typeof(double), null));
			AssertExceptionThrown<InvalidCastException>("Invalid conversion", () => ((IConvertible)zDatetimeOffset).ToType(typeof(decimal), null));
		}

		#endregion

		#region Implementation

		protected DateTimeOffset RecentDate1;
		protected DateTimeOffset RecentDateUtc1;
		protected DateTimeOffset RecentDate2;
		protected DateTimeOffset RecentDateUtc2;

		void AssertDateTimeOffsetEquals(ZDateTimeOffset a, ZDateTimeOffset b)
		{
			if (a.IsEmpty)
			{
				Assert("A is Empty - B should be too", b.IsEmpty);
			}
			else if (!a.IsValid)
			{
				Assert("A is Invalid - B should be too", !b.IsValid);
			}
			else if (b.IsEmpty)
			{
				Assert("B is Empty - A should be too", a.IsEmpty);
			}
			else if (!b.IsValid)
			{
				Assert("B is Invalid - A should be too", !a.IsValid);
			}
			else
			{
				AssertEquals("Offsets are equal", a.Offset, b.Offset);
				AssertEquals("DateTimes are equal", a.ToDateTime(), b.ToDateTime());
				AssertEquals("UtcDateTimes are equal", a.ToUtcDateTime(), b.ToUtcDateTime());
			}
		}

		protected override void SetUp()
		{
			RecentDateUtc1 = new DateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.Zero);
			RecentDate1 = new DateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(11));
			RecentDateUtc2 = new DateTimeOffset(2016, 04, 05, 02, 03, 04, TimeSpan.Zero);
			RecentDate2 = new DateTimeOffset(2016, 04, 05, 02, 03, 04, TimeSpan.FromHours(10));
		}

		protected void AssertToString(string expected, object value)
		{
			AssertEquals(MessageForValue(value), expected, new ZDateTimeOffset(value).ToString());
		}

		protected void AssertToStringFormatted(string expected, object value, string format)
		{
			AssertEquals(MessageForValue(value), expected, new ZDateTimeOffset(value).ToString(format));
		}

		#endregion
	}
}
