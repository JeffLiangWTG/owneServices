using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class TimeFactoryDateAttributeTest : TestCase
	{
		public void TestDateTimeKindsAreCorrect()
		{
			AssertEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.CurrentUtcDate.Kind);
			AssertEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.CurrentUtcDateTime.Kind);
			AssertEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.GetUtcFromLocalTime(DateTime.Now).Kind);
			AssertEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.GetUtcFromUnlocoTime("AUSYD", DateTime.Now).Kind);

			//Local or unspecified OK
			AssertNotEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.CurrentLocalDate.Kind);
			AssertNotEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.CurrentLocalDateTime.Kind);
			AssertNotEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.GetLocalTimeFromUtc(DateTime.UtcNow).Kind);
			AssertNotEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.GetUnlocoTimeFromUtc("AUSYD", DateTime.UtcNow).Kind);
			AssertNotEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.GetTimeInOneZoneFromTimeInAnotherZone("AUSYD", DateTime.Now, "USERI").Kind);

			TestDateAttribute.Date = new DateTime(2000, 1, 1);

			AssertEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.CurrentUtcDate.Kind);
			AssertEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.CurrentUtcDateTime.Kind);
			AssertEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.GetUtcFromLocalTime(DateTime.Now).Kind);
			AssertEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.GetUtcFromUnlocoTime("AUSYD", DateTime.Now).Kind);

			//Local or unspecified OK
			AssertNotEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.CurrentLocalDate.Kind);
			AssertNotEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.CurrentLocalDateTime.Kind);
			AssertNotEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.GetLocalTimeFromUtc(DateTime.UtcNow).Kind);
			AssertNotEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.GetUnlocoTimeFromUtc("AUSYD", DateTime.UtcNow).Kind);
			AssertNotEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.GetTimeInOneZoneFromTimeInAnotherZone("AUSYD", DateTime.Now, "USERI").Kind);
		}

		[TestUtcOffset(10, 0, 0)]
		public void TestDateTimeKindsAreCorrect_UsingTestUtcOffset()
		{
			AssertEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.CurrentUtcDate.Kind);
			AssertEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.CurrentUtcDateTime.Kind);
			AssertEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.GetUtcFromLocalTime(DateTime.Now).Kind);
			AssertEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.GetUtcFromUnlocoTime("AUSYD", DateTime.Now).Kind);

			//Local or unspecified OK
			AssertNotEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.CurrentLocalDate.Kind);
			AssertNotEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.CurrentLocalDateTime.Kind);
			AssertNotEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.GetLocalTimeFromUtc(DateTime.UtcNow).Kind);
			AssertNotEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.GetUnlocoTimeFromUtc("AUSYD", DateTime.UtcNow).Kind);
			AssertNotEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.GetTimeInOneZoneFromTimeInAnotherZone("AUSYD", DateTime.Now, "USERI").Kind);

			TestDateAttribute.Date = new DateTime(2000, 1, 1);

			AssertEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.CurrentUtcDate.Kind);
			AssertEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.CurrentUtcDateTime.Kind);
			AssertEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.GetUtcFromLocalTime(DateTime.Now).Kind);
			AssertEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.GetUtcFromUnlocoTime("AUSYD", DateTime.Now).Kind);

			//Local or unspecified OK
			AssertNotEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.CurrentLocalDate.Kind);
			AssertNotEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.CurrentLocalDateTime.Kind);
			AssertNotEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.GetLocalTimeFromUtc(DateTime.UtcNow).Kind);
			AssertNotEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.GetUnlocoTimeFromUtc("AUSYD", DateTime.UtcNow).Kind);
			AssertNotEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.GetTimeInOneZoneFromTimeInAnotherZone("AUSYD", DateTime.Now, "USERI").Kind);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestDateTimeKindsAreCorrect_UsingTestUNLOCO()
		{
			AssertEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.CurrentUtcDate.Kind);
			AssertEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.CurrentUtcDateTime.Kind);
			AssertEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.GetUtcFromLocalTime(DateTime.Now).Kind);
			AssertEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.GetUtcFromUnlocoTime("AUSYD", DateTime.Now).Kind);

			//Local or unspecified OK
			AssertNotEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.CurrentLocalDate.Kind);
			AssertNotEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.CurrentLocalDateTime.Kind);
			AssertNotEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.GetLocalTimeFromUtc(DateTime.UtcNow).Kind);
			AssertNotEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.GetUnlocoTimeFromUtc("AUSYD", DateTime.UtcNow).Kind);
			AssertNotEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.GetTimeInOneZoneFromTimeInAnotherZone("AUSYD", DateTime.Now, "USERI").Kind);

			TestDateAttribute.Date = new DateTime(2000, 1, 1);

			AssertEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.CurrentUtcDate.Kind);
			AssertEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.CurrentUtcDateTime.Kind);
			AssertEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.GetUtcFromLocalTime(DateTime.Now).Kind);
			AssertEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.GetUtcFromUnlocoTime("AUSYD", DateTime.Now).Kind);

			//Local or unspecified OK
			AssertNotEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.CurrentLocalDate.Kind);
			AssertNotEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.CurrentLocalDateTime.Kind);
			AssertNotEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.GetLocalTimeFromUtc(DateTime.UtcNow).Kind);
			AssertNotEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.GetUnlocoTimeFromUtc("AUSYD", DateTime.UtcNow).Kind);
			AssertNotEquals(DateTimeKind.Utc, EnvProxy.Instance.Time.GetTimeInOneZoneFromTimeInAnotherZone("AUSYD", DateTime.Now, "USERI").Kind);
		}

		[TestUtcOffset(8, 0, 0)]
		public void TestGetLocalTimeFromDateTimeOffset_UsingTestUtcOffset()
		{
			var offsetToTest = new DateTimeOffset(2024, 06, 28, 12, 30, 15, TimeSpan.FromHours(2));
			var convertedOffset = EnvProxy.Instance.Time.GetLocalTimeFromDateTimeOffset(offsetToTest);

			AssertEquals("Offset should represent same time.", offsetToTest, convertedOffset);
			AssertEquals("Offset should match Test Attribute Offset.", TimeSpan.FromHours(8), convertedOffset.Offset);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestGetLocalTimeFromDateTimeOffset_UsingTestUNLOCO()
		{
			var offsetToTest = new DateTimeOffset(2024, 06, 28, 12, 30, 15, TimeSpan.FromHours(2));
			var convertedOffset = EnvProxy.Instance.Time.GetLocalTimeFromDateTimeOffset(offsetToTest);

			AssertEquals("Offset should represent same time.", offsetToTest, convertedOffset);
			AssertEquals("Offset should match Test Attribute UNLOCO.", TimeSpan.FromHours(10), convertedOffset.Offset);
		}

		[TestDate(2024, 06, 28, 12, 30, 15)]
		public void TestGetLocalTimeFromDateTimeOffset_NotUsingUNLOCO()
		{
			TestDateAttribute.UseUNLOCO = false;

			var offsetToTest = new DateTimeOffset(2024, 06, 28, 12, 30, 15, TimeSpan.FromHours(2));
			var convertedOffset = EnvProxy.Instance.Time.GetLocalTimeFromDateTimeOffset(offsetToTest);

			AssertEquals("Offset should represent same time.", offsetToTest, convertedOffset);
			AssertEquals("Offset should represent same offset.", TimeSpan.FromHours(2), convertedOffset.Offset);
		}

		public void TestFormatDateTimeOffset()
		{
			DateTimeOffset dateTimeOffset1 = new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(1));
			AssertEquals("Format should be DD-Mon-YY hh:mm [{+|-}hh:mm]", "02-Mar-03 13:45 +01:00", EnvProxy.Instance.Time.FormatDateTimeOffset(dateTimeOffset1));
			DateTimeOffset dateTimeOffset2 = new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(-1));
			AssertEquals("Format should be DD-Mon-YY hh:mm [{+|-}hh:mm]", "02-Mar-03 13:45 -01:00", EnvProxy.Instance.Time.FormatDateTimeOffset(dateTimeOffset2));
			DateTimeOffset dateTimeOffset3 = new DateTimeOffset(2003, 03, 02, 13, 45, 55, new TimeSpan());
			AssertEquals("Format should be DD-Mon-YY hh:mm [{+|-}hh:mm]", "02-Mar-03 13:45 +00:00", EnvProxy.Instance.Time.FormatDateTimeOffset(dateTimeOffset3));
			DateTimeOffset dateTimeOffset4 = new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(1).Add(TimeSpan.FromMinutes(30)));
			AssertEquals("Format should be DD-Mon-YY hh:mm [{+|-}hh:mm]", "02-Mar-03 13:45 +01:30", EnvProxy.Instance.Time.FormatDateTimeOffset(dateTimeOffset4));
			DateTimeOffset dateTimeOffset5 = new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(-1).Subtract(TimeSpan.FromMinutes(30)));
			AssertEquals("Format should be DD-Mon-YY hh:mm [{+|-}hh:mm]", "02-Mar-03 13:45 -01:30", EnvProxy.Instance.Time.FormatDateTimeOffset(dateTimeOffset5));
			DateTimeOffset dateTimeOffset6 = new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromMinutes(-30));
			AssertEquals("Format should be DD-Mon-YY hh:mm [{+|-}hh:mm]", "02-Mar-03 13:45 -00:30", EnvProxy.Instance.Time.FormatDateTimeOffset(dateTimeOffset6));
			DateTimeOffset dateTimeOffset7 = new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromMinutes(-20));
			AssertEquals("Format should be DD-Mon-YY hh:mm [{+|-}hh:mm]", "02-Mar-03 13:45 -00:20", EnvProxy.Instance.Time.FormatDateTimeOffset(dateTimeOffset7));
			DateTimeOffset dateTimeOffset8 = new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(-1).Subtract(TimeSpan.FromMinutes(20)));
			AssertEquals("Format should be DD-Mon-YY hh:mm [{+|-}hh:mm]", "02-Mar-03 13:45 -01:20", EnvProxy.Instance.Time.FormatDateTimeOffset(dateTimeOffset8));
		}

		public void TestFormatDateTimeOffsetWithSeconds()
		{
			DateTimeOffset dateTimeOffset1 = new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(1));
			AssertEquals("Format should be DD-Mon-YY hh:mm:ss [{+|-}hh:mm]", "02-Mar-03 13:45:55 +01:00", EnvProxy.Instance.Time.FormatDateTimeOffsetWithSeconds(dateTimeOffset1));
			DateTimeOffset dateTimeOffset2 = new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(-1));
			AssertEquals("Format should be DD-Mon-YY hh:mm:ss [{+|-}hh:mm]", "02-Mar-03 13:45:55 -01:00", EnvProxy.Instance.Time.FormatDateTimeOffsetWithSeconds(dateTimeOffset2));
			DateTimeOffset dateTimeOffset3 = new DateTimeOffset(2003, 03, 02, 13, 45, 55, new TimeSpan());
			AssertEquals("Format should be DD-Mon-YY hh:mm:ss [{+|-}hh:mm]", "02-Mar-03 13:45:55 +00:00", EnvProxy.Instance.Time.FormatDateTimeOffsetWithSeconds(dateTimeOffset3));
			DateTimeOffset dateTimeOffset4 = new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(1).Add(TimeSpan.FromMinutes(30)));
			AssertEquals("Format should be DD-Mon-YY hh:mm:ss [{+|-}hh:mm]", "02-Mar-03 13:45:55 +01:30", EnvProxy.Instance.Time.FormatDateTimeOffsetWithSeconds(dateTimeOffset4));
			DateTimeOffset dateTimeOffset5 = new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(-1).Subtract(TimeSpan.FromMinutes(30)));
			AssertEquals("Format should be DD-Mon-YY hh:mm:ss [{+|-}hh:mm]", "02-Mar-03 13:45:55 -01:30", EnvProxy.Instance.Time.FormatDateTimeOffsetWithSeconds(dateTimeOffset5));
			DateTimeOffset dateTimeOffset6 = new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromMinutes(-30));
			AssertEquals("Format should be DD-Mon-YY hh:mm:ss [{+|-}hh:mm]", "02-Mar-03 13:45:55 -00:30", EnvProxy.Instance.Time.FormatDateTimeOffsetWithSeconds(dateTimeOffset6));
			DateTimeOffset dateTimeOffset7 = new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromMinutes(-20));
			AssertEquals("Format should be DD-Mon-YY hh:mm:ss [{+|-}hh:mm]", "02-Mar-03 13:45:55 -00:20", EnvProxy.Instance.Time.FormatDateTimeOffsetWithSeconds(dateTimeOffset7));
			DateTimeOffset dateTimeOffset8 = new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(-1).Subtract(TimeSpan.FromMinutes(20)));
			AssertEquals("Format should be DD-Mon-YY hh:mm:ss [{+|-}hh:mm]", "02-Mar-03 13:45:55 -01:20", EnvProxy.Instance.Time.FormatDateTimeOffsetWithSeconds(dateTimeOffset8));
		}

		[TestDateIncremental(1, 0, 0, 0)]
		public void TestTestDateIncrementalAttributeDays()
		{
			DateTime currentTime1 = EnvProxy.Instance.Time.CurrentLocalDateTime;
			DateTime currentTime2 = EnvProxy.Instance.Time.CurrentLocalDateTime;
			TimeSpan difference = currentTime2 - currentTime1;
			Assert(difference.TotalDays >= 1);
		}

		[TestDateIncremental(0, 1, 0, 0)]
		public void TestTestDateIncrementalAttributeHours()
		{
			DateTime currentTime1 = EnvProxy.Instance.Time.CurrentLocalDateTime;
			DateTime currentTime2 = EnvProxy.Instance.Time.CurrentLocalDateTime;
			TimeSpan difference = currentTime2 - currentTime1;
			Assert(difference.TotalHours >= 1);
		}

		[TestDateIncremental(0, 0, 1, 0)]
		public void TestTestDateIncrementalAttributeMinutes()
		{
			DateTime currentTime1 = EnvProxy.Instance.Time.CurrentLocalDateTime;
			DateTime currentTime2 = EnvProxy.Instance.Time.CurrentLocalDateTime;
			TimeSpan difference = currentTime2 - currentTime1;
			Assert(difference.TotalMinutes >= 1);
		}

		[TestDateIncremental(0, 0, 0, 1)]
		public void TestTestDateIncrementalAttributeSeconds()
		{
			DateTime currentTime1 = EnvProxy.Instance.Time.CurrentLocalDateTime;
			DateTime currentTime2 = EnvProxy.Instance.Time.CurrentLocalDateTime;
			TimeSpan difference = currentTime2 - currentTime1;
			Assert(difference.TotalSeconds >= 1);
		}

		[TestDate(2005, 2, 1)]
		public void TestTestDateAttribute()
		{
			AssertEquals("CurrentYear", 2005, EnvProxy.Instance.Time.CurrentLocalDateTime.Year);
			AssertEquals("CurrentMonth", 2, EnvProxy.Instance.Time.CurrentLocalDateTime.Month);
			AssertEquals("CurrentDay", 1, EnvProxy.Instance.Time.CurrentLocalDateTime.Day);
			AssertEquals("Hour", 0, EnvProxy.Instance.Time.CurrentLocalDateTime.Hour);
			AssertEquals("Minute", 0, EnvProxy.Instance.Time.CurrentLocalDateTime.Minute);
			AssertEquals("Second", 0, EnvProxy.Instance.Time.CurrentLocalDateTime.Second);
		}

		[TestDate(2005, 2, 1, 14, 15, 10)]
		public void TestTestDateAttributeWithTime()
		{
			AssertEquals("CurrentYear", 2005, EnvProxy.Instance.Time.CurrentLocalDateTime.Year);
			AssertEquals("CurrentMonth", 2, EnvProxy.Instance.Time.CurrentLocalDateTime.Month);
			AssertEquals("CurrentDay", 1, EnvProxy.Instance.Time.CurrentLocalDateTime.Day);
			AssertEquals("Hour", 14, EnvProxy.Instance.Time.CurrentLocalDateTime.Hour);
			AssertEquals("Minute", 15, EnvProxy.Instance.Time.CurrentLocalDateTime.Minute);
			AssertEquals("Second", 10, EnvProxy.Instance.Time.CurrentLocalDateTime.Second);
		}

		[TestTimeZoneUNLOCO("USERI")]
		[TestDate]
		public void TestUNLOCO()
		{
			TestDateAttribute.UseUNLOCO = true;

			// http://www.timeanddate.com/time/zone/usa/philadelphia

			//UTCNOW immediately before DST starting in 2014 (going from UTC-5h to UTC-4h)
			TestDateAttribute.Date = new DateTime(2014, 3, 9, 6, 30, 0);
			AssertEquals(new DateTime(2014, 3, 9, 1, 30, 0), EnvProxy.Instance.Time.CurrentLocalDateTime);
			AssertEquals(TimeSpan.FromHours(-5), EnvProxy.Instance.Time.GetUtcOffsetBasedOnLocal(EnvProxy.Instance.Time.CurrentLocalDateTime));
			AssertEquals(TimeSpan.FromHours(-5), EnvProxy.Instance.Time.GetUtcOffsetBasedOnUtc(EnvProxy.Instance.Time.CurrentUtcDateTime));

			//UTCNOW immediately after DST starting in 2014 (going from UTC-5h to UTC-4h)
			TestDateAttribute.Date = new DateTime(2014, 3, 9, 7, 30, 0);
			AssertEquals(new DateTime(2014, 3, 9, 3, 30, 0), EnvProxy.Instance.Time.CurrentLocalDateTime);
			AssertEquals(TimeSpan.FromHours(-4), EnvProxy.Instance.Time.GetUtcOffsetBasedOnLocal(EnvProxy.Instance.Time.CurrentLocalDateTime));
			AssertEquals(TimeSpan.FromHours(-4), EnvProxy.Instance.Time.GetUtcOffsetBasedOnUtc(EnvProxy.Instance.Time.CurrentUtcDateTime));

			//UTCNOW immediately before DST ending in 2014 (going from UTC-4h to UTC-5h)
			TestDateAttribute.Date = new DateTime(2014, 11, 2, 5, 30, 0);
			AssertEquals(new DateTime(2014, 11, 2, 1, 30, 0), EnvProxy.Instance.Time.CurrentLocalDateTime);
			AssertEquals(TimeSpan.FromHours(-5), EnvProxy.Instance.Time.GetUtcOffsetBasedOnLocal(EnvProxy.Instance.Time.CurrentLocalDateTime));
			AssertEquals(TimeSpan.FromHours(-4), EnvProxy.Instance.Time.GetUtcOffsetBasedOnUtc(EnvProxy.Instance.Time.CurrentUtcDateTime));

			//UTCNOW immediately after DST ending in 2014 (going from UTC-4h to UTC-5h)
			TestDateAttribute.Date = new DateTime(2014, 11, 2, 6, 30, 0);
			AssertEquals(new DateTime(2014, 11, 2, 1, 30, 0), EnvProxy.Instance.Time.CurrentLocalDateTime);
			AssertEquals(TimeSpan.FromHours(-5), EnvProxy.Instance.Time.GetUtcOffsetBasedOnLocal(EnvProxy.Instance.Time.CurrentLocalDateTime));
			AssertEquals(TimeSpan.FromHours(-5), EnvProxy.Instance.Time.GetUtcOffsetBasedOnUtc(EnvProxy.Instance.Time.CurrentUtcDateTime));

			//UTCNOW immediately after the looped hour of DST ending in 2014 (going from UTC-4h to UTC-5h)
			TestDateAttribute.Date = new DateTime(2014, 11, 2, 7, 30, 0);
			AssertEquals(new DateTime(2014, 11, 2, 2, 30, 0), EnvProxy.Instance.Time.CurrentLocalDateTime);
			AssertEquals(TimeSpan.FromHours(-5), EnvProxy.Instance.Time.GetUtcOffsetBasedOnLocal(EnvProxy.Instance.Time.CurrentLocalDateTime));
			AssertEquals(TimeSpan.FromHours(-5), EnvProxy.Instance.Time.GetUtcOffsetBasedOnUtc(EnvProxy.Instance.Time.CurrentUtcDateTime));

			//UTCNOW immediately before DST starting in 2015 (going from UTC-5h to UTC-4h)
			TestDateAttribute.Date = new DateTime(2015, 3, 8, 6, 30, 0);
			AssertEquals(new DateTime(2015, 3, 8, 1, 30, 0), EnvProxy.Instance.Time.CurrentLocalDateTime);
			AssertEquals(TimeSpan.FromHours(-5), EnvProxy.Instance.Time.GetUtcOffsetBasedOnLocal(EnvProxy.Instance.Time.CurrentLocalDateTime));
			AssertEquals(TimeSpan.FromHours(-5), EnvProxy.Instance.Time.GetUtcOffsetBasedOnUtc(EnvProxy.Instance.Time.CurrentUtcDateTime));

			//UTCNOW immediately after DST starting in 2015 (going from UTC-5h to UTC-4h)
			TestDateAttribute.Date = new DateTime(2015, 3, 8, 7, 30, 0);
			AssertEquals(new DateTime(2015, 3, 8, 3, 30, 0), EnvProxy.Instance.Time.CurrentLocalDateTime);
			AssertEquals(TimeSpan.FromHours(-4), EnvProxy.Instance.Time.GetUtcOffsetBasedOnLocal(EnvProxy.Instance.Time.CurrentLocalDateTime));
			AssertEquals(TimeSpan.FromHours(-4), EnvProxy.Instance.Time.GetUtcOffsetBasedOnUtc(EnvProxy.Instance.Time.CurrentUtcDateTime));

			//UTCNOW immediately before DST ending in 2015 (going from UTC-4h to UTC-5h)
			TestDateAttribute.Date = new DateTime(2015, 11, 1, 5, 30, 0);
			AssertEquals(new DateTime(2015, 11, 1, 1, 30, 0), EnvProxy.Instance.Time.CurrentLocalDateTime);
			AssertEquals(TimeSpan.FromHours(-5), EnvProxy.Instance.Time.GetUtcOffsetBasedOnLocal(EnvProxy.Instance.Time.CurrentLocalDateTime));
			AssertEquals(TimeSpan.FromHours(-4), EnvProxy.Instance.Time.GetUtcOffsetBasedOnUtc(EnvProxy.Instance.Time.CurrentUtcDateTime));

			//UTCNOW immediately after DST ending in 2015 (going from UTC-4h to UTC-5h)
			TestDateAttribute.Date = new DateTime(2015, 11, 1, 6, 30, 0);
			AssertEquals(new DateTime(2015, 11, 1, 1, 30, 0), EnvProxy.Instance.Time.CurrentLocalDateTime);
			AssertEquals(TimeSpan.FromHours(-5), EnvProxy.Instance.Time.GetUtcOffsetBasedOnLocal(EnvProxy.Instance.Time.CurrentLocalDateTime));
			AssertEquals(TimeSpan.FromHours(-5), EnvProxy.Instance.Time.GetUtcOffsetBasedOnUtc(EnvProxy.Instance.Time.CurrentUtcDateTime));

			//UTCNOW immediately after the looped hour of DST ending in 2015 (going from UTC-4h to UTC-5h)
			TestDateAttribute.Date = new DateTime(2015, 11, 1, 7, 30, 0);
			AssertEquals(new DateTime(2015, 11, 1, 2, 30, 0), EnvProxy.Instance.Time.CurrentLocalDateTime);
			AssertEquals(TimeSpan.FromHours(-5), EnvProxy.Instance.Time.GetUtcOffsetBasedOnLocal(EnvProxy.Instance.Time.CurrentLocalDateTime));
			AssertEquals(TimeSpan.FromHours(-5), EnvProxy.Instance.Time.GetUtcOffsetBasedOnUtc(EnvProxy.Instance.Time.CurrentUtcDateTime));

			TestDateAttribute.UseUNLOCO = false;

			TestDateAttribute.Date = new DateTime(2014, 3, 9, 6, 30, 0);
			AssertEquals("UNLOCO conversion turns off and we just re-use UTC", new DateTime(2014, 3, 9, 6, 30, 0), EnvProxy.Instance.Time.CurrentLocalDateTime);
			AssertEquals(TimeSpan.Zero, EnvProxy.Instance.Time.GetUtcOffsetBasedOnLocal(EnvProxy.Instance.Time.CurrentLocalDateTime));
			AssertEquals(TimeSpan.Zero, EnvProxy.Instance.Time.GetUtcOffsetBasedOnUtc(EnvProxy.Instance.Time.CurrentUtcDateTime));
		}

		[TestTimeZoneUNLOCO("USERI")]
		public void TestUNLOCOWithRealDate()
		{
			Assert("UNLOCO conversion works for attribute when TestDateAttribute.IsActive is false", EnvProxy.Instance.Time.CurrentLocalDateTime < DateTime.UtcNow);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate]
		public void TestUNLOCO_2()
		{
			TestDateAttribute.UseUNLOCO = true;

			// http://www.timeanddate.com/time/zone/australia/sydney

			//UTCNOW immediately before DST ending in 2014 (going from UTC+11h to UTC+10h)
			TestDateAttribute.Date = new DateTime(2014, 4, 5, 15, 30, 0);
			AssertEquals(new DateTime(2014, 4, 6, 2, 30, 0), EnvProxy.Instance.Time.CurrentLocalDateTime);
			AssertEquals(TimeSpan.FromHours(10), EnvProxy.Instance.Time.GetUtcOffsetBasedOnLocal(EnvProxy.Instance.Time.CurrentLocalDateTime));
			AssertEquals(TimeSpan.FromHours(11), EnvProxy.Instance.Time.GetUtcOffsetBasedOnUtc(EnvProxy.Instance.Time.CurrentUtcDateTime));

			//UTCNOW immediately after DST ending in 2014 (going from UTC+11h to UTC+10h)
			TestDateAttribute.Date = new DateTime(2014, 4, 5, 16, 30, 0);
			AssertEquals(new DateTime(2014, 4, 6, 2, 30, 0), EnvProxy.Instance.Time.CurrentLocalDateTime);
			AssertEquals(TimeSpan.FromHours(10), EnvProxy.Instance.Time.GetUtcOffsetBasedOnLocal(EnvProxy.Instance.Time.CurrentLocalDateTime));
			AssertEquals(TimeSpan.FromHours(10), EnvProxy.Instance.Time.GetUtcOffsetBasedOnUtc(EnvProxy.Instance.Time.CurrentUtcDateTime));

			//UTCNOW  immediately after the looped hour of DST ending in 2014 (going from UTC+11h to UTC+10h)
			TestDateAttribute.Date = new DateTime(2014, 4, 5, 17, 30, 0);
			AssertEquals(new DateTime(2014, 4, 6, 3, 30, 0), EnvProxy.Instance.Time.CurrentLocalDateTime);
			AssertEquals(TimeSpan.FromHours(10), EnvProxy.Instance.Time.GetUtcOffsetBasedOnLocal(EnvProxy.Instance.Time.CurrentLocalDateTime));
			AssertEquals(TimeSpan.FromHours(10), EnvProxy.Instance.Time.GetUtcOffsetBasedOnUtc(EnvProxy.Instance.Time.CurrentUtcDateTime));

			//UTCNOW immediately before DST starting in 2014 (going from UTC+10h to UTC+11h)
			TestDateAttribute.Date = new DateTime(2014, 10, 4, 15, 30, 0);
			AssertEquals(new DateTime(2014, 10, 5, 1, 30, 0), EnvProxy.Instance.Time.CurrentLocalDateTime);
			AssertEquals(TimeSpan.FromHours(10), EnvProxy.Instance.Time.GetUtcOffsetBasedOnLocal(EnvProxy.Instance.Time.CurrentLocalDateTime));
			AssertEquals(TimeSpan.FromHours(10), EnvProxy.Instance.Time.GetUtcOffsetBasedOnUtc(EnvProxy.Instance.Time.CurrentUtcDateTime));

			//UTCNOW immediately after DST starting in 2014 (going from UTC+10h to UTC+11h)
			TestDateAttribute.Date = new DateTime(2014, 10, 4, 16, 30, 0);
			AssertEquals(new DateTime(2014, 10, 5, 3, 30, 0), EnvProxy.Instance.Time.CurrentLocalDateTime);
			AssertEquals(TimeSpan.FromHours(11), EnvProxy.Instance.Time.GetUtcOffsetBasedOnLocal(EnvProxy.Instance.Time.CurrentLocalDateTime));
			AssertEquals(TimeSpan.FromHours(11), EnvProxy.Instance.Time.GetUtcOffsetBasedOnUtc(EnvProxy.Instance.Time.CurrentUtcDateTime));

			//UTCNOW immediately before DST ending in 2015 (going from UTC+11h to UTC+10h)
			TestDateAttribute.Date = new DateTime(2015, 4, 4, 15, 30, 0);
			AssertEquals(new DateTime(2015, 4, 5, 2, 30, 0), EnvProxy.Instance.Time.CurrentLocalDateTime);
			AssertEquals(TimeSpan.FromHours(10), EnvProxy.Instance.Time.GetUtcOffsetBasedOnLocal(EnvProxy.Instance.Time.CurrentLocalDateTime));
			AssertEquals(TimeSpan.FromHours(11), EnvProxy.Instance.Time.GetUtcOffsetBasedOnUtc(EnvProxy.Instance.Time.CurrentUtcDateTime));

			//UTCNOW immediately after DST ending in 2015 (going from UTC+11h to UTC+10h)
			TestDateAttribute.Date = new DateTime(2015, 4, 4, 16, 30, 0);
			AssertEquals(new DateTime(2015, 4, 5, 2, 30, 0), EnvProxy.Instance.Time.CurrentLocalDateTime);
			AssertEquals(TimeSpan.FromHours(10), EnvProxy.Instance.Time.GetUtcOffsetBasedOnLocal(EnvProxy.Instance.Time.CurrentLocalDateTime));
			AssertEquals(TimeSpan.FromHours(10), EnvProxy.Instance.Time.GetUtcOffsetBasedOnUtc(EnvProxy.Instance.Time.CurrentUtcDateTime));

			//UTCNOW immediately after the looped hour of DST ending in 2015 (going from UTC+11h to UTC+10h)
			TestDateAttribute.Date = new DateTime(2015, 4, 4, 17, 30, 0);
			AssertEquals(new DateTime(2015, 4, 5, 3, 30, 0), EnvProxy.Instance.Time.CurrentLocalDateTime);
			AssertEquals(TimeSpan.FromHours(10), EnvProxy.Instance.Time.GetUtcOffsetBasedOnLocal(EnvProxy.Instance.Time.CurrentLocalDateTime));
			AssertEquals(TimeSpan.FromHours(10), EnvProxy.Instance.Time.GetUtcOffsetBasedOnUtc(EnvProxy.Instance.Time.CurrentUtcDateTime));

			//UTCNOW immediately before DST starting in 2015 (going from UTC+10h to UTC+11h)
			TestDateAttribute.Date = new DateTime(2015, 10, 3, 15, 30, 0);
			AssertEquals(new DateTime(2015, 10, 4, 1, 30, 0), EnvProxy.Instance.Time.CurrentLocalDateTime);
			AssertEquals(TimeSpan.FromHours(10), EnvProxy.Instance.Time.GetUtcOffsetBasedOnLocal(EnvProxy.Instance.Time.CurrentLocalDateTime));
			AssertEquals(TimeSpan.FromHours(10), EnvProxy.Instance.Time.GetUtcOffsetBasedOnUtc(EnvProxy.Instance.Time.CurrentUtcDateTime));

			//UTCNOW immediately after DST starting in 2015 (going from UTC+10h to UTC+11h)
			TestDateAttribute.Date = new DateTime(2015, 10, 3, 16, 30, 0);
			AssertEquals(new DateTime(2015, 10, 4, 3, 30, 0), EnvProxy.Instance.Time.CurrentLocalDateTime);
			AssertEquals(TimeSpan.FromHours(11), EnvProxy.Instance.Time.GetUtcOffsetBasedOnLocal(EnvProxy.Instance.Time.CurrentLocalDateTime));
			AssertEquals(TimeSpan.FromHours(11), EnvProxy.Instance.Time.GetUtcOffsetBasedOnUtc(EnvProxy.Instance.Time.CurrentUtcDateTime));

			TestDateAttribute.UseUNLOCO = false;

			TestDateAttribute.Date = new DateTime(2014, 3, 9, 6, 30, 0);
			AssertEquals("UNLOCO conversion turns off and we just re-use UTC", new DateTime(2014, 3, 9, 6, 30, 0), EnvProxy.Instance.Time.CurrentLocalDateTime);
			AssertEquals(TimeSpan.Zero, EnvProxy.Instance.Time.GetUtcOffsetBasedOnLocal(EnvProxy.Instance.Time.CurrentLocalDateTime));
			AssertEquals(TimeSpan.Zero, EnvProxy.Instance.Time.GetUtcOffsetBasedOnUtc(EnvProxy.Instance.Time.CurrentUtcDateTime));
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestUNLOCO_2WithRealDate()
		{
			Assert("UNLOCO conversion works for attribute when TestDateAttribute.IsActive is false", EnvProxy.Instance.Time.CurrentLocalDateTime > DateTime.UtcNow);
		}

		[TestTimeZoneUNLOCO("USERI")]
		[TestDate]
		public void TestLocalToUTCBehaviour_AroundDSTChanges_IsConsistent()
		{
			TestDateAttribute.UseUNLOCO = true;

			//The behaviour of GetUtcFromLocalTime (and thus GetUtcFromUnlocoTime and all methods they call) is to pick the EARLIER hour if the local time is BEHIND UTC, because it uses the pre-DST offset (which is smaller) and thus stays behind.
			//On the other hand, if the local time is AHEAD of UTC, it picks the LATER hour because it uses the post-DST offset (which is smaller) and thus stays ahead.
			//This means it acts differently based on your time zone. I shudder to think about how it acts if you're in a time zone that flips between being ahead of/behind UTC during DST changes.

			// http://www.timeanddate.com/time/zone/usa/philadelphia

			//UTCNOW immediately before DST starting in 2014 (going from UTC-5h to UTC-4h)
			TestDateAttribute.Date = new DateTime(2014, 3, 9, 6, 30, 0);
			AssertEquals(new DateTime(2014, 3, 9, 6, 30, 0), EnvProxy.Instance.Time.GetUtcFromLocalTime(new DateTime(2014, 3, 9, 1, 30, 0)));

			//UTCNOW immediately after DST starting in 2014 (going from UTC-5h to UTC-4h)
			TestDateAttribute.Date = new DateTime(2014, 3, 9, 7, 30, 0);
			AssertEquals(new DateTime(2014, 3, 9, 7, 30, 0), EnvProxy.Instance.Time.GetUtcFromLocalTime(new DateTime(2014, 3, 9, 3, 30, 0)));

			//One hour earlier...
			TestDateAttribute.Date = new DateTime(2014, 11, 2, 4, 30, 0);
			AssertEquals(new DateTime(2014, 11, 2, 4, 30, 0), EnvProxy.Instance.Time.GetUtcFromLocalTime(new DateTime(2014, 11, 2, 0, 30, 0)));

			//UTCNOW immediately before DST ending in 2014 (going from UTC-4h to UTC-5h)
			//There are two possible UTC values that this local hour could convert to! We want the second UTC hour.
			TestDateAttribute.Date = new DateTime(2014, 11, 2, 5, 30, 0);
			AssertEquals(new DateTime(2014, 11, 2, 6, 30, 0), EnvProxy.Instance.Time.GetUtcFromLocalTime(new DateTime(2014, 11, 2, 1, 30, 0)));

			//UTCNOW immediately after DST ending in 2014 (going from UTC-4h to UTC-5h)
			//There are two possible UTC values that this local hour could convert to! We want the second UTC hour.
			TestDateAttribute.Date = new DateTime(2014, 11, 2, 6, 30, 0);
			AssertEquals(new DateTime(2014, 11, 2, 6, 30, 0), EnvProxy.Instance.Time.GetUtcFromLocalTime(new DateTime(2014, 11, 2, 1, 30, 0)));

			//One hour later...
			TestDateAttribute.Date = new DateTime(2014, 11, 2, 7, 30, 0);
			AssertEquals(new DateTime(2014, 11, 2, 7, 30, 0), EnvProxy.Instance.Time.GetUtcFromLocalTime(new DateTime(2014, 11, 2, 2, 30, 0)));

			//UTCNOW immediately before DST starting in 2015 (going from UTC-5h to UTC-4h)
			TestDateAttribute.Date = new DateTime(2015, 3, 8, 6, 30, 0);
			AssertEquals(new DateTime(2015, 3, 8, 6, 30, 0), EnvProxy.Instance.Time.GetUtcFromLocalTime(new DateTime(2015, 3, 8, 1, 30, 0)));

			//UTCNOW immediately after DST starting in 2015 (going from UTC-5h to UTC-4h)
			TestDateAttribute.Date = new DateTime(2015, 3, 8, 7, 30, 0);
			AssertEquals(new DateTime(2015, 3, 8, 7, 30, 0), EnvProxy.Instance.Time.GetUtcFromLocalTime(new DateTime(2015, 3, 8, 3, 30, 0)));

			//One hour earlier...
			TestDateAttribute.Date = new DateTime(2015, 11, 1, 4, 30, 0);
			AssertEquals(new DateTime(2015, 11, 1, 4, 30, 0), EnvProxy.Instance.Time.GetUtcFromLocalTime(new DateTime(2015, 11, 1, 0, 30, 0)));

			//UTCNOW immediately before DST ending in 2015 (going from UTC-4h to UTC-5h)
			//There are two possible UTC values that this local hour could convert to! We want the second UTC hour.
			TestDateAttribute.Date = new DateTime(2015, 11, 1, 5, 30, 0);
			AssertEquals(new DateTime(2015, 11, 1, 6, 30, 0), EnvProxy.Instance.Time.GetUtcFromLocalTime(new DateTime(2015, 11, 1, 1, 30, 0)));

			//UTCNOW immediately after DST ending in 2015 (going from UTC-4h to UTC-5h)
			//There are two possible UTC values that this local hour could convert to! We want the second UTC hour.
			TestDateAttribute.Date = new DateTime(2015, 11, 1, 6, 30, 0);
			AssertEquals(new DateTime(2015, 11, 1, 6, 30, 0), EnvProxy.Instance.Time.GetUtcFromLocalTime(new DateTime(2015, 11, 1, 1, 30, 0)));

			//One hour later..
			TestDateAttribute.Date = new DateTime(2015, 11, 1, 7, 30, 0);
			AssertEquals(new DateTime(2015, 11, 1, 7, 30, 0), EnvProxy.Instance.Time.GetUtcFromLocalTime(new DateTime(2015, 11, 1, 2, 30, 0)));
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate]
		public void TestLocalToUTCBehaviour_AroundDSTChanges_IsConsistent_2()
		{
			TestDateAttribute.UseUNLOCO = true;

			//The behaviour of GetUtcFromLocalTime (and thus GetUtcFromUnlocoTime and all methods they call) is to pick the EARLIER hour if the local time is BEHIND UTC, because it uses the pre-DST offset (which is smaller) and thus stays behind.
			//On the other hand, if the local time is AHEAD of UTC, it picks the LATER hour because it uses the post-DST offset (which is smaller) and thus stays ahead.
			//This means it acts differently based on your time zone. I shudder to think about how it acts if you're in a time zone that flips between being ahead of/behind UTC during DST changes.

			// http://www.timeanddate.com/time/zone/australia/sydney

			//One hour earlier...
			TestDateAttribute.Date = new DateTime(2014, 4, 5, 14, 30, 0);
			AssertEquals(new DateTime(2014, 4, 5, 14, 30, 0), EnvProxy.Instance.Time.GetUtcFromLocalTime(new DateTime(2014, 4, 6, 1, 30, 0)));

			//UTCNOW immediately before DST ending in 2014 (going from UTC+11h to UTC+10h)
			//There are two possible UTC values that this local hour could convert to! We want the second UTC hour.
			TestDateAttribute.Date = new DateTime(2014, 4, 5, 15, 30, 0);
			AssertEquals(new DateTime(2014, 4, 5, 16, 30, 0), EnvProxy.Instance.Time.GetUtcFromLocalTime(new DateTime(2014, 4, 6, 2, 30, 0)));

			//UTCNOW immediately after DST ending in 2014 (going from UTC+11h to UTC+10h)
			//There are two possible UTC values that this local hour could convert to! We want the second UTC hour.
			TestDateAttribute.Date = new DateTime(2014, 4, 5, 16, 30, 0);
			AssertEquals(new DateTime(2014, 4, 5, 16, 30, 0), EnvProxy.Instance.Time.GetUtcFromLocalTime(new DateTime(2014, 4, 6, 2, 30, 0)));

			//One hour later...
			TestDateAttribute.Date = new DateTime(2014, 4, 5, 17, 30, 0);
			AssertEquals(new DateTime(2014, 4, 5, 17, 30, 0), EnvProxy.Instance.Time.GetUtcFromLocalTime(new DateTime(2014, 4, 6, 3, 30, 0)));

			//UTCNOW immediately before DST starting in 2014 (going from UTC+10h to UTC+11h)
			TestDateAttribute.Date = new DateTime(2014, 10, 4, 15, 30, 0);
			AssertEquals(new DateTime(2014, 10, 4, 15, 30, 0), EnvProxy.Instance.Time.GetUtcFromLocalTime(new DateTime(2014, 10, 5, 1, 30, 0)));

			//UTCNOW immediately before DST starting in 2014 (going from UTC+10h to UTC+11h)
			TestDateAttribute.Date = new DateTime(2014, 10, 4, 16, 30, 0);
			AssertEquals(new DateTime(2014, 10, 4, 16, 30, 0), EnvProxy.Instance.Time.GetUtcFromLocalTime(new DateTime(2014, 10, 5, 3, 30, 0)));

			//One hour earlier...
			TestDateAttribute.Date = new DateTime(2015, 4, 4, 14, 30, 0);
			AssertEquals(new DateTime(2015, 4, 4, 14, 30, 0), EnvProxy.Instance.Time.GetUtcFromLocalTime(new DateTime(2015, 4, 5, 1, 30, 0)));

			//UTCNOW immediately before DST ending in 2015 (going from UTC+11h to UTC+10h)
			//There are two possible UTC values that this local hour could convert to! We want the second UTC hour.
			TestDateAttribute.Date = new DateTime(2015, 4, 4, 15, 30, 0);
			AssertEquals(new DateTime(2015, 4, 4, 16, 30, 0), EnvProxy.Instance.Time.GetUtcFromLocalTime(new DateTime(2015, 4, 5, 2, 30, 0)));

			//UTCNOW immediately after DST ending in 2015 (going from UTC+11h to UTC+10h)
			//There are two possible UTC values that this local hour could convert to! We want the second UTC hour.
			TestDateAttribute.Date = new DateTime(2015, 4, 4, 16, 30, 0);
			AssertEquals(new DateTime(2015, 4, 4, 16, 30, 0), EnvProxy.Instance.Time.GetUtcFromLocalTime(new DateTime(2015, 4, 5, 2, 30, 0)));

			//One hour later...
			TestDateAttribute.Date = new DateTime(2015, 4, 4, 17, 30, 0);
			AssertEquals(new DateTime(2015, 4, 4, 17, 30, 0), EnvProxy.Instance.Time.GetUtcFromLocalTime(new DateTime(2015, 4, 5, 3, 30, 0)));

			//UTCNOW immediately before DST starting in 2015 (going from UTC+10h to UTC+11h)
			TestDateAttribute.Date = new DateTime(2015, 10, 3, 15, 30, 0);
			AssertEquals(new DateTime(2015, 10, 3, 15, 30, 0), EnvProxy.Instance.Time.GetUtcFromLocalTime(new DateTime(2015, 10, 4, 1, 30, 0)));

			//UTCNOW immediately before DST starting in 2015 (going from UTC+10h to UTC+11h)
			TestDateAttribute.Date = new DateTime(2015, 10, 3, 16, 30, 0);
			AssertEquals(new DateTime(2015, 10, 3, 16, 30, 0), EnvProxy.Instance.Time.GetUtcFromLocalTime(new DateTime(2015, 10, 4, 3, 30, 0)));
		}

		[TestUtcOffset(15, 46, 0)]
		public void TestTestUtcTimeAttribute()
		{
			DateTime utcNow = EnvProxy.Instance.Time.CurrentUtcDateTime;

			DateTime localTime1 = EnvProxy.Instance.Time.GetUnlocoTimeFromUtc("AUSYD", utcNow);
			DateTime localTime2 = EnvProxy.Instance.Time.GetUnlocoTimeFromUtc("USCHI", utcNow);
			DateTime localTime3 = EnvProxy.Instance.Time.GetUnlocoTimeFromUtc("GBLON", utcNow);

			TimeSpan utcDiff = localTime1 - utcNow;
			AssertEquals("UTC offset", 946D, utcDiff.TotalMinutes);

			AssertEquals("UTC Offset attribute is active. Time is the same regardless of the UNLOCO", localTime1, localTime2);
			AssertEquals("UTC Offset attribute is active. Time is the same regardless of the UNLOCO", localTime1, localTime3);

			DateTime localNow = EnvProxy.Instance.Time.CurrentLocalDateTime;
			TimeSpan localUtcDiff = localNow - utcNow;
			AssertEquals("Current local is calculated based on UTC Offset Attribute?", true, utcDiff.TotalMinutes >= 946D);
		}

		public void TestGetTimeInOneZoneFromTimeInAnotherZone()
		{
			AssertEquals("Convert Perth time to Sydney time", new DateTime(2012, 09, 02, 00, 30, 00), EnvProxy.Instance.Time.GetTimeInOneZoneFromTimeInAnotherZone("AUPER", new DateTime(2012, 09, 01, 22, 30, 00), "AUSYD"));
		}

		[TestUtcOffset(-15, 30, 0)]
		[TestDate(2020, 6, 29, 7, 0, 0)]
		public void TestShowDateTimeWithGMTFormat()
		{
			AssertEquals("28-Jun-20 16:30 GMT-14:30", EnvProxy.Instance.Time.CurrentLocalDateTimeIncludingGmt);

			TestUtcOffsetAttribute.Time = new TimeSpan(10, 0, 0);
			AssertEquals("29-Jun-20 17:00 GMT+10:00", EnvProxy.Instance.Time.CurrentLocalDateTimeIncludingGmt);

			TestUtcOffsetAttribute.Time = new TimeSpan(10, 1, 0);
			AssertEquals("29-Jun-20 17:01 GMT+10:01", EnvProxy.Instance.Time.CurrentLocalDateTimeIncludingGmt);

			TestUtcOffsetAttribute.Time = new TimeSpan(8, 0, 0);
			AssertEquals("29-Jun-20 15:00 GMT+08:00", EnvProxy.Instance.Time.CurrentLocalDateTimeIncludingGmt);

			TestUtcOffsetAttribute.Time = new TimeSpan(-8, 0, 0);
			AssertEquals("28-Jun-20 23:00 GMT-08:00", EnvProxy.Instance.Time.CurrentLocalDateTimeIncludingGmt);
		}
	}
}
