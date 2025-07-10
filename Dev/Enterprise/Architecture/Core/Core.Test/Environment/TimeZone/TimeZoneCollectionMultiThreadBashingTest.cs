using System;
using System.Collections.Concurrent;
using System.Threading;
using Enterprise.ZArchitecture.Core.Testing;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class TimeZoneCollectionMultiThreadBashingTest : MultiThreadBasherBaseTest
	{
		protected override void SetupThreadSafeInstances()
		{
			timeZones = new TimeZoneCollectionForTesting();
			Assert("Should be thread safe", timeZones.TimeZones_Exposed is ConcurrentDictionary<string, ITimeZone>);
		}

		protected override void RestoreSingleThreadInstances()
		{
		}

		protected override void ThreadBashingMethod()
		{
			DateTime testTime = DateTime.Now.ToUniversalTime();
			DateTime resultTime;

			for (int i = 0; i < 50; i++)
			{
				timeZones.TimeZones_Exposed.Clear();
				Thread.Sleep(0);
				resultTime = timeZones.ToLocationTimeFromUtc("AUSYD", testTime);
				Thread.Sleep(0);
				resultTime = timeZones.ToLocationTimeFromUtc("HKHKG", testTime);
				Thread.Sleep(0);
				resultTime = timeZones.ToLocationTimeFromUtc("RUDME", testTime);
				Thread.Sleep(1);
			}
		}

		TimeZoneCollectionForTesting timeZones;
	}
}
