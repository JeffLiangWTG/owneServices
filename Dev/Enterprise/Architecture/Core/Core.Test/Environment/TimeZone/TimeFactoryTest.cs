using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class TimeFactoryTest : TestCase
	{
		public void TestDateTimeReturnsSqlDataBaseTimePrecision()
		{
			for (int i = 0; i < 10; i++)
			{
				DateTime testDateTime = EnvProxy.Instance.Time.CurrentLocalDateTime;
				int milliseconds = testDateTime.Millisecond;

				Assert(
					"Must end with either 0 or 3 or 7 milliseconds for SqlDatabase time precision, but was " + milliseconds.ToString(),
					(milliseconds % 10 == 0) || (milliseconds % 10 == 3) || (milliseconds % 10 == 7));

				Assert(
					"Precision must not be more accurate than the millisecond, but was ." + testDateTime.ToString("fffffff") + " seconds",
					testDateTime.ToString("fffffff").EndsWith("0000"));

				System.Threading.Thread.Sleep(1);
			}
		}

		public void TestLocalTimeAndUtc()
		{
			DateTime utcDateTime = EnvProxy.Instance.Time.CurrentUtcDateTime;
			DateTime localDateTime = EnvProxy.Instance.Time.GetLocalTimeFromUtc(utcDateTime);
			TimeZoneCollectionForTesting testCollection = new TimeZoneCollectionForTesting();
			AssertEquals("Local Time", localDateTime, testCollection.ToLocationTimeFromUtc(EnvProxy.Instance.CurrentBranch.NKUNLOCO, utcDateTime));
			AssertEquals("UTC", utcDateTime, testCollection.ToUtcFromLocationTime(EnvProxy.Instance.CurrentBranch.NKUNLOCO, localDateTime));
		}

		public void TestGetUtcAndLocalDateTime()
		{
			// Gets reference start time (allow 1 second margin for rounding)
			DateTime startTime = DateTime.Now.AddSeconds(-1);

			TimeZoneCollectionForTesting testCollection = new TimeZoneCollectionForTesting();

			// ----------------------
			// AUSYD = Sydney        
			// ----------------------
			string ausydUnloco = "AUSYD";

			DateTime utcDateTime = EnvProxy.Instance.Time.CurrentUtcDateTime;
			DateTime localDateTime_SydneyBranch = EnvProxy.Instance.Time.GetUnlocoTimeFromUtc(ausydUnloco, utcDateTime);
			TimeSpan utcOffset_SydneyBranch = testCollection.GetUtcOffsetBasedOnUtc(ausydUnloco, utcDateTime);
			TimeSpan spanMargin = DateTime.Now.Subtract(startTime);

			TimeZoneTestComparer sydneyBasedTimeComparer = new TimeZoneTestComparer(
				startTime,
				utcDateTime,
				localDateTime_SydneyBranch,
				utcOffset_SydneyBranch,
				"Sydney");

			TimeSpan expectedUtcOffsetStandard = new TimeSpan(0, 0, 10 * 3600);
			TimeSpan expectedUtcOffsetDst = new TimeSpan(0, 0, 11 * 3600);

			AssertEquals(
				"UTC offset Sydney should be +10h or +11h but was " + utcOffset_SydneyBranch.Hours.ToString(), true,
				(utcOffset_SydneyBranch == expectedUtcOffsetStandard || utcOffset_SydneyBranch == expectedUtcOffsetDst));

			TimeSpan calculatedUtcOffsetSpan = localDateTime_SydneyBranch.Subtract(utcDateTime);
			TimeZoneTestComparer.AssertTimeSpan(calculatedUtcOffsetSpan, utcOffset_SydneyBranch, spanMargin, "Local-UTC difference (Sydney)");

			// ----------------------
			// ZAJNB = Johannesburg  
			// ----------------------
			string zajnbUnloco = "ZAJNB";

			utcDateTime = EnvProxy.Instance.Time.CurrentUtcDateTime;
			DateTime localDateTime_JohannesburgBranch = EnvProxy.Instance.Time.GetUnlocoTimeFromUtc(zajnbUnloco, utcDateTime);
			TimeSpan utcOffset_JohannesburgBranch = testCollection.GetUtcOffsetBasedOnUtc(zajnbUnloco, utcDateTime);
			spanMargin = DateTime.Now.Subtract(startTime);

			sydneyBasedTimeComparer.AssertTimesRelativeToAnotherLocation(
				spanMargin,
				utcDateTime,
				localDateTime_JohannesburgBranch,
				utcOffset_JohannesburgBranch,
				"Johannesburg");

			expectedUtcOffsetStandard = new TimeSpan(0, 0, 2 * 3600);
			AssertEquals("UTC offset - Johannesburg", expectedUtcOffsetStandard, utcOffset_JohannesburgBranch);

			calculatedUtcOffsetSpan = localDateTime_JohannesburgBranch.Subtract(utcDateTime);
			TimeZoneTestComparer.AssertTimeSpan(calculatedUtcOffsetSpan, utcOffset_JohannesburgBranch, spanMargin, "Local-UTC difference (Johannesburg)");

			// ----------------------
			// USCHI = Chicago       
			// ----------------------
			string uschiUnloco = "USCHI";

			utcDateTime = EnvProxy.Instance.Time.CurrentUtcDateTime;
			DateTime localDateTime_ChicagoBranch = EnvProxy.Instance.Time.GetUnlocoTimeFromUtc(uschiUnloco, utcDateTime);
			TimeSpan utcOffset_ChicagoBranch = testCollection.GetUtcOffsetBasedOnUtc(uschiUnloco, utcDateTime);
			spanMargin = DateTime.Now.Subtract(startTime);

			sydneyBasedTimeComparer.AssertTimesRelativeToAnotherLocation(
				spanMargin,
				utcDateTime,
				localDateTime_ChicagoBranch,
				utcOffset_ChicagoBranch,
				"Chicago");

			expectedUtcOffsetStandard = new TimeSpan(0, 0, -6 * 3600);
			expectedUtcOffsetDst = new TimeSpan(0, 0, -5 * 3600);
			AssertEquals(
				"UTC offset Chicago should be -6h or -5h but was " + utcOffset_ChicagoBranch.Hours.ToString(), true,
				(utcOffset_ChicagoBranch == expectedUtcOffsetStandard || utcOffset_ChicagoBranch == expectedUtcOffsetDst));

			calculatedUtcOffsetSpan = localDateTime_ChicagoBranch.Subtract(utcDateTime);
			TimeZoneTestComparer.AssertTimeSpan(calculatedUtcOffsetSpan, utcOffset_ChicagoBranch, spanMargin, "Local-UTC difference (Chicago)");
		}

		public void TestTimeFactoryIsThreadSafe_RunSameInstance()
		{
			var task1 = Task.Run(() => EnvProxy.Instance.Time.GetHashCode()).Result;

			var task2 = Task.Run(() => EnvProxy.Instance.Time.GetHashCode()).Result;

			AssertEquals(task1, task2);
		}

		public void TestDateTimeFormatUsesGetLocalizedFormatString()
		{
			var date = DateTime.Now;
			AssertEquals(date.ToString(DateTimeFormatStrings.GetLocalizedFormatString(null)), EnvProxy.Instance.Time.Format(date, null));
		}

		public void TestDateTimeOffsetFormatUsesGetLocalizedFormatString()
		{
			var date = DateTimeOffset.Now;
			AssertEquals(date.ToString(DateTimeOffsetFormatStrings.GetLocalizedFormatString(null)), EnvProxy.Instance.Time.Format(date, null));
		}

		public void TestDateTimeFormatUsesTemporaryDateFormat()
		{
			var date = DateTime.Now;
			AssertEquals(date.ToString(DateTimeFormatStrings.GetLocalizedFormatString(null)), EnvProxy.Instance.Time.Format(date, null));

			using (EnvProxy.Instance.Time.SetTemporaryDateFormat("dd-MMM-yyyy HH:mm:ss"))
			{
				AssertEquals(date.ToString("dd-MMM-yyyy HH:mm:ss"), EnvProxy.Instance.Time.Format(date, null));
				AssertEquals(date.ToString(DateTimeOffsetFormatStrings.LongTimeFormat), EnvProxy.Instance.Time.Format(date, DateTimeOffsetFormatStrings.LongTimeFormat));
			}
		}
	}
}
