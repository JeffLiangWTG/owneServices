using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class TimeZoneCollectionTest : TestCase
	{
		public void TestCurrentUtcIsCorrectKind()
		{
			TimeZoneCollectionForTesting testZoneCollection = new TimeZoneCollectionForTesting();
			AssertEquals("Kind returned by testZoneCollection.CurrentUtc() is correct", DateTimeKind.Utc, testZoneCollection.CurrentUtc().Kind);
		}

		public void TestTimeZoneInformationIsCachedAcrossThreads()
		{
			var unloco = EnvProxy.Instance.CurrentNKUNLOCO;
			var timeZoneCollectionForInitialization = new TimeZoneCollectionForTesting();
			timeZoneCollectionForInitialization.TimeZones_Exposed.Clear();

			var tracker = SqlEventTracker.Instance;
			tracker.Clear();
			var timeZoneDbHits = 0;
			tracker.SqlCommandExecutedEvent += (SqlCommandExecutedEventArgs obj) =>
			{
				if (obj.Text.Contains("RefTimeZoneSet"))
				{
					Interlocked.Increment(ref timeZoneDbHits);
				}
			};

			var lockObject = new object();
			var readDateAction = new ThreadStart(() =>
			{
				lock (lockObject)
				{
					using (Db.DisposableActionForDbConnection())
					{
						var timeZoneCollection = new TimeZoneCollectionForTesting();
						timeZoneCollection.ToLocationTimeFromAnotherLocationTime(unloco, DateTime.Now, unloco);
						timeZoneCollection.ToLocationTimeFromUtc(unloco, DateTime.Now);
						timeZoneCollection.ToUtcFromLocationTime(unloco, DateTime.Now);
						timeZoneCollection.GetUtcOffsetBasedOnLocal(unloco, DateTime.Now);
						timeZoneCollection.GetUtcOffsetBasedOnUtc(unloco, DateTime.Now);
					}
				}
			});

			var threads = new List<Thread>();
			int numberOfThreads = 10;
			for (int i = 0; i < numberOfThreads; i++)
			{
				var thread = new Thread(readDateAction);
				threads.Add(thread);
				thread.Start();
			}
			threads.ForEach((t) => t.Join());

			AssertEquals("Number of time zone related db hits should be one", 1, timeZoneDbHits);
		}

		[SnailTest]
		public void TestGetCurrentUtc()
		{
			// Read current time information
			var clientUtcTimeBefore = DateTime.UtcNow;
			Thread.Sleep(4);

			var testUtcCache = new TimeZoneCollectionForTesting();
			var utcTimeFromCache = testUtcCache.CurrentUtc();
			var utcAndServerZoneWrapper = testUtcCache.UtcAndServerZoneWrapperForTest;
			Thread.Sleep(4);
			var clientUtcTimeAfter = DateTime.UtcNow;

			Assert("Cached time calculated from ClientTime",
				utcTimeFromCache >= clientUtcTimeBefore.Add(utcAndServerZoneWrapper.ClientPcToDbTimeDelta) && utcTimeFromCache <= clientUtcTimeAfter.Add(utcAndServerZoneWrapper.ClientPcToDbTimeDelta));
			Assert("Last cache refresh time should have been set",
				utcAndServerZoneWrapper.ClientUtcTime >= clientUtcTimeBefore && utcAndServerZoneWrapper.ClientUtcTime <= clientUtcTimeAfter);
			AssertEquals("Kind returned by DateTime.UtcNow is correct", DateTimeKind.Utc, clientUtcTimeAfter.Kind);
			AssertEquals("Kind returned by GetCurrentUtc() is correct", DateTimeKind.Utc, utcTimeFromCache.Kind);

			// Reserve LastRefresh and TimeDelta and read current time information again
			Thread.Sleep(4);
			var testLastRefresh = utcAndServerZoneWrapper.ClientUtcTime;
			var testTimeFactoryDelta = utcAndServerZoneWrapper.ClientPcToDbTimeDelta;
			testUtcCache.CurrentUtc();

			AssertEquals("[Last cache refresh] should be the same as in 1st CurrentLocalDateTime read", testLastRefresh, utcAndServerZoneWrapper.ClientUtcTime);
			AssertEquals("Client and DB time difference should be the same as in 1st CurrentLocalDateTime read", testTimeFactoryDelta, utcAndServerZoneWrapper.ClientPcToDbTimeDelta);
		}

		[SnailTest]
		public void TestCacheExpiration()
		{
			var testUtcCache = new TimeZoneCollectionForTesting();
			var dateTimeUtcBefore = testUtcCache.CurrentUtc();

			var utcAndServerZoneWrapper = new UtcDateTimeCacheForTesting();
			Thread.Sleep(4);
			utcAndServerZoneWrapper.UtcNow_Override = DateTime.UtcNow;

			testUtcCache.UtcAndServerZoneWrapperForTest = utcAndServerZoneWrapper;
			Assert(dateTimeUtcBefore < DateTime.UtcNow);
			AssertEquals("Date is not expired", false, testUtcCache.UtcAndServerZoneWrapperForTest.IsExpired);

			utcAndServerZoneWrapper.UtcNow_Override = DateTime.UtcNow.AddHours(1);

			testUtcCache.UtcAndServerZoneWrapperForTest = utcAndServerZoneWrapper;
			AssertEquals("Date is expired", true, testUtcCache.UtcAndServerZoneWrapperForTest.IsExpired);

			var dateTimeUtcAfter = testUtcCache.CurrentUtc();
			Assert("Expired date time has been updated.", dateTimeUtcAfter > dateTimeUtcBefore);
		}

		[SnailTest]
		public void TestCacheNeverExpires()
		{
			var testUtcCache = new TimeZoneCollectionForTesting();
			testUtcCache.CacheNeverExpires = true;
			var dateTimeUtcBefore = testUtcCache.CurrentUtc();

			var utcAndServerZoneWrapper = new UtcDateTimeCacheForTesting();
			Thread.Sleep(4);
			utcAndServerZoneWrapper.UtcNow_Override = DateTime.UtcNow;

			testUtcCache.UtcAndServerZoneWrapperForTest = utcAndServerZoneWrapper;
			Assert(dateTimeUtcBefore < DateTime.UtcNow);
			AssertEquals("Date is not expired", false, testUtcCache.UtcAndServerZoneWrapperForTest.IsExpired);
			AssertEquals("same cache", true, ReferenceEquals(utcAndServerZoneWrapper, testUtcCache.UtcAndServerZoneWrapperForTest));

			utcAndServerZoneWrapper.UtcNow_Override = DateTime.UtcNow.AddHours(1);

			testUtcCache.UtcAndServerZoneWrapperForTest = utcAndServerZoneWrapper;
			AssertEquals("Date is expired", true, testUtcCache.UtcAndServerZoneWrapperForTest.IsExpired);

			var dateTimeUtcAfter = testUtcCache.CurrentUtc();
			Assert("date time has been updated.", dateTimeUtcAfter > dateTimeUtcBefore);
			AssertEquals("cache has not been updated", true, ReferenceEquals(utcAndServerZoneWrapper, testUtcCache.UtcAndServerZoneWrapperForTest));
		}

		public void TestRefreshCacheNow()
		{
			var testUtcCache = new TimeZoneCollectionForTesting();
			testUtcCache.RefreshCacheNow();
			var cache1 = testUtcCache.UtcAndServerZoneWrapperForTest;
			testUtcCache.RefreshCacheNow();
			var cache2 = testUtcCache.UtcAndServerZoneWrapperForTest;

			AssertEquals("cache was refreshed", false, ReferenceEquals(cache1, cache2));
		}

		public void TestToCurrentBranchLocalTimeFromUtc()
		{
			TimeZoneCollectionForTesting testZoneCollection = new TimeZoneCollectionForTesting();
			DateTime utcDateTime = DateTime.UtcNow; // Getting a random UTC from client PC for tests

			// Johannesburg - ZAJNB
			DateTime localTime = testZoneCollection.ToLocationTimeFromUtc("ZAJNB", utcDateTime);
			TimeSpan actualUtcOffset = localTime.Subtract(utcDateTime);
			TimeSpan expectedUtcOffset = new TimeSpan(2, 0, 0);
			AssertEquals("UTC Offset - Johannesburg", expectedUtcOffset, actualUtcOffset);

			// Porto Alegre - BRPOA
			localTime = testZoneCollection.ToLocationTimeFromUtc("BRPOA", utcDateTime);
			actualUtcOffset = localTime.Subtract(utcDateTime);
			ITimeZone zone = testZoneCollection.TimeZones_Exposed["BRPOA"];
			expectedUtcOffset = zone.IsDaylightSavingBasedOnUtc(utcDateTime) ? new TimeSpan(-2, 0, 0) : new TimeSpan(-3, 0, 0);
			AssertEquals("UTC Offset - Porto Alegre", expectedUtcOffset, actualUtcOffset);
		}

		public void TestToUtcFromLocationTime_TimeZoneAheadOfUtc()
		{
			TimeZoneCollectionForTesting testZoneCollection = new TimeZoneCollectionForTesting();

			// --------------
			// Sydney - AUSYD
			// --------------
			string zoneUnloco = "AUSYD";

			// DST START IN 2006 - Last Sunday of October @2am in Standard time => 29/10/2006 @2am
			// Local Time = 29/10/2006 @2am

			// Before DST Start
			DateTime localDateTime = new DateTime(2006, 10, 29, 1, 30, 0);
			DateTime utcDateTime = testZoneCollection.ToUtcFromLocationTime(zoneUnloco, localDateTime);
			AssertEquals("Local Time = 29/10/2006 @1:30am => UTC:", new DateTime(2006, 10, 28, 15, 30, 0), utcDateTime);

			// After DST Start
			localDateTime = new DateTime(2006, 10, 29, 3, 0, 0);
			utcDateTime = testZoneCollection.ToUtcFromLocationTime(zoneUnloco, localDateTime);
			AssertEquals("Local Time = 29/10/2006 @3am => UTC:", new DateTime(2006, 10, 28, 16, 0, 0), utcDateTime);

			// DST END IN 2006 - 1st Sunday of April @2am in Standard time => 02/04/2006 @2am
			// Local Time = 02/04/2006 @3am

			// Before DST End
			localDateTime = new DateTime(2006, 4, 2, 1, 0, 0);
			utcDateTime = testZoneCollection.ToUtcFromLocationTime(zoneUnloco, localDateTime);
			AssertEquals("Local Time = 02/04/2006 @1am => UTC:", new DateTime(2006, 4, 1, 14, 0, 0), utcDateTime);

			// After DST End
			localDateTime = new DateTime(2006, 4, 2, 2, 30, 0);
			utcDateTime = testZoneCollection.ToUtcFromLocationTime(zoneUnloco, localDateTime);
			AssertEquals("Local Time = 02/04/2006 @2:30am => UTC:", new DateTime(2006, 4, 1, 16, 30, 0), utcDateTime);

			// After DST End
			localDateTime = new DateTime(2006, 4, 2, 3, 0, 0);
			utcDateTime = testZoneCollection.ToUtcFromLocationTime(zoneUnloco, localDateTime);
			AssertEquals("Local Time = 02/04/2006 @3am => UTC:", new DateTime(2006, 4, 1, 17, 0, 0), utcDateTime);
		}

		public void TestToUtcFromLocationTime_TimeZoneBehindUtc()
		{
			TimeZoneCollectionForTesting testZoneCollection = new TimeZoneCollectionForTesting();

			// --------------------
			// Porto Alegre - BRPOA
			// --------------------
			string zoneUnloco = "BRPOA";

			// DST START IN 2005 - 3rd Sunday of October @12am(0:00)
			// Local time => 16/10/2005 @12am

			DateTime localDateTime = new DateTime(2005, 10, 15, 23, 30, 0);
			DateTime utcDateTime = testZoneCollection.ToUtcFromLocationTime(zoneUnloco, localDateTime);
			AssertEquals("Local Time = 15/10/2005 @23:30 => UTC:", new DateTime(2005, 10, 16, 2, 30, 0), utcDateTime);

			localDateTime = new DateTime(2005, 10, 16, 1, 0, 0);
			utcDateTime = testZoneCollection.ToUtcFromLocationTime(zoneUnloco, localDateTime);
			AssertEquals("Local Time = 16/10/2005 @1am => UTC:", new DateTime(2005, 10, 16, 3, 0, 0), utcDateTime);

			// DST END IN 2005 - 3rd Sunday of February @12am(0:00)
			// Local time => 20/02/2005 @12am

			localDateTime = new DateTime(2005, 2, 19, 22, 30, 0);
			utcDateTime = testZoneCollection.ToUtcFromLocationTime(zoneUnloco, localDateTime);
			AssertEquals("Local Time = 19/02/2005 @22:30 => UTC:", new DateTime(2005, 2, 20, 0, 30, 0), utcDateTime);

			//The new behaviour when local date time is ambiguous (due to DST ending and the local hour repeating) is to pick the second (non-DST) hour. This is standard with how C# does it.
			localDateTime = new DateTime(2005, 2, 19, 23, 30, 0);
			utcDateTime = testZoneCollection.ToUtcFromLocationTime(zoneUnloco, localDateTime);
			AssertEquals("Local Time = 19/02/2005 @23:30 => UTC:", new DateTime(2005, 2, 20, 2, 30, 0), utcDateTime);

			localDateTime = new DateTime(2005, 2, 20, 0, 1, 0);
			utcDateTime = testZoneCollection.ToUtcFromLocationTime(zoneUnloco, localDateTime);
			AssertEquals("Local Time = 20/02/2005 @0:00 => UTC:", new DateTime(2005, 2, 20, 3, 1, 0), utcDateTime);
		}

		public void TestToUtcFromLocationTime_NoDstTimeZone()
		{
			TimeZoneCollectionForTesting testZoneCollection = new TimeZoneCollectionForTesting();

			// --------------
			// Bombay - INBOM
			// --------------
			string zoneUnloco = "INBOM";

			DateTime localDateTime = DateTime.Now; // Getting a random time from client PC for tests
			DateTime utcDateTime = testZoneCollection.ToUtcFromLocationTime(zoneUnloco, localDateTime);

			AssertEquals("LocalTime-UTC", new TimeSpan(5, 30, 0), localDateTime.Subtract(utcDateTime));
		}

		public void TestGetLocationToUtcOffsetBasedOnUtc()
		{
			TimeZoneCollectionForTesting testZoneCollection = new TimeZoneCollectionForTesting();
			DateTime utcDateTime = DateTime.UtcNow; // Getting a random UTC from client PC for tests

			// Sydney - AUSYD
			string ausydUnloco = "AUSYD";

			TimeSpan utcOffset = testZoneCollection.GetUtcOffsetBasedOnUtc(ausydUnloco, utcDateTime);
			ITimeZone sydZone = testZoneCollection.TimeZones_Exposed[ausydUnloco];
			TimeSpan expectedUtcOffset = sydZone.IsDaylightSavingBasedOnUtc(utcDateTime) ? new TimeSpan(11, 0, 0) : new TimeSpan(10, 0, 0);
			AssertEquals("UTC Offset - Sydney", expectedUtcOffset, utcOffset);

			// Johannesburg - ZAJNB
			string zajnbUnloco = "ZAJNB";

			utcOffset = testZoneCollection.GetUtcOffsetBasedOnUtc(zajnbUnloco, utcDateTime);
			expectedUtcOffset = new TimeSpan(2, 0, 0);
			AssertEquals("UTC Offset - Johannesburg", expectedUtcOffset, utcOffset);
		}

		public void TestDstPeriods()
		{
			string ausydUnloco = "AUSYD";
			TimeZoneCollectionForTesting testCollection = new TimeZoneCollectionForTesting();

			DateTime testLocalTime = new DateTime(2006, 09, 30, 10, 30, 00);
			DateTime testUtc = testCollection.ToUtcFromLocationTime(ausydUnloco, testLocalTime);

			AssertEquals("UTC Time (local zone NOT in Daylight Saving)", testLocalTime.AddHours(-10), testUtc);
			AssertEquals("UTC offset (local zone NOT in Daylight Saving)", new TimeSpan(0, 0, 10 * 3600), testCollection.GetUtcOffsetBasedOnUtc(ausydUnloco, testUtc));

			testLocalTime = new DateTime(2006, 11, 05, 13, 50, 10);
			testUtc = testCollection.ToUtcFromLocationTime(ausydUnloco, testLocalTime);

			AssertEquals("UTC Time (local zone in Daylight Saving)", testLocalTime.AddHours(-11), testUtc);
			AssertEquals("UTC offset (local zone in Daylight Saving)", new TimeSpan(0, 0, 11 * 3600), testCollection.GetUtcOffsetBasedOnUtc(ausydUnloco, testUtc));
		}

		public void TestIsValidUNLOCO()
		{
			TimeZoneCollectionForTesting testCollection = new TimeZoneCollectionForTesting();
			testCollection.TimeZones_Exposed.Clear();

			var invalidUnloco = "XXX";
			Assert("Must not return true for invalid UNLOCO", !testCollection.IsValidUNLOCO(invalidUnloco));
			Assert("Must not cache invalid UNLOCO", !testCollection.TimeZones_Exposed.ContainsKey(invalidUnloco));

			var validUnloco = "AUSYD";
			Assert("UNLOCO is not cached yet", !testCollection.TimeZones_Exposed.ContainsKey(validUnloco));
			Assert("Must return true for valid UNLOCO", testCollection.IsValidUNLOCO(validUnloco));
			Assert("Must cache valid UNLOCO", testCollection.TimeZones_Exposed.ContainsKey(validUnloco));
		}
		public void TestIsValidUNLOCOCache()
		{
			TimeZoneCollectionForTesting testCollection = new TimeZoneCollectionForTesting();
			testCollection.TimeZones_Exposed.Clear();

			var invalidUnloco = "XXX";
			AssertEquals(TimeSpan.Zero, testCollection.GetUtcOffsetBasedOnUtc(invalidUnloco, DateTime.UtcNow));

			Assert("Must not return true for invalid UNLOCO", !testCollection.IsValidUNLOCO(invalidUnloco));
			Assert("Must not cache invalid UNLOCO", !testCollection.TimeZones_Exposed.ContainsKey(invalidUnloco));
		}
	}
}
