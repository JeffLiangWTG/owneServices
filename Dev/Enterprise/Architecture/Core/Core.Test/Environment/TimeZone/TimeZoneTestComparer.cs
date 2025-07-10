using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	class TimeZoneTestComparer
	{
		public TimeZoneTestComparer(
			DateTime testStartPcTime, DateTime utcDateTime, DateTime localDateTime, TimeSpan utcOffset, string location)
		{
			this.utcDateTime = utcDateTime;
			this.localDateTime = localDateTime;
			this.utcOffset = utcOffset;
			this.location = location;
		}

		readonly DateTime utcDateTime;
		readonly DateTime localDateTime;
		readonly TimeSpan utcOffset;
		readonly string location;

		public void AssertTimesRelativeToAnotherLocation(
			TimeSpan marginSpan, DateTime anotherUtcDateTime, DateTime anotherLocalDateTime, TimeSpan anotherUtcOffset, string anotherLocation)
		{
			TimeSpan utcSpan = anotherUtcDateTime.Subtract(utcDateTime);
			TimeSpan localSpan = anotherLocalDateTime.Subtract(localDateTime);
			TimeSpan utcOffsetDiff = anotherUtcOffset - utcOffset;

			AssertTimeSpan(utcSpan, TimeSpan.Zero, marginSpan, string.Format("UTC time should NOT vary from {0} to {1}", location, anotherLocation));
			AssertTimeSpan(localSpan, utcOffsetDiff, marginSpan, string.Format("Local time should vary from {0} to {1}", location, anotherLocation));
		}

		public static void AssertTimeSpan(TimeSpan timeSpamToAssert, TimeSpan bottom, TimeSpan margin, string failMessage)
		{
			Assertion.AssertEquals(failMessage, true, timeSpamToAssert >= bottom - margin && timeSpamToAssert < bottom + margin);
		}
	}
}
