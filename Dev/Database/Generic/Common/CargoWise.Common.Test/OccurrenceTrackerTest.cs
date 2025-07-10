using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CargoWise.Common
{
	class OccurrenceTrackerTest : NUnit.Framework.TestCase
	{
		public void TestTracking()
		{
			var startTime = DateTime.UtcNow;
			var currentTime = startTime;
			var tracker = new OccurrenceTracker(TimeSpan.FromHours(1), () => currentTime);
			AssertNull(tracker.Latest);
			AssertEquals(0, tracker.Count);
			currentTime = startTime.AddHours(-2);
			tracker.Add();
			currentTime = startTime;
			AssertEquals(startTime.AddHours(-2), tracker.Latest);
			AssertEquals(0, tracker.Count);
			currentTime = startTime.AddMinutes(-30);
			tracker.Add();
			currentTime = startTime;
			AssertEquals(startTime.AddMinutes(-30), tracker.Latest.Value);
			AssertEquals(1, tracker.Count);
			currentTime = startTime.AddMinutes(-25);
			tracker.Add();
			currentTime = startTime;
			AssertEquals(startTime.AddMinutes(-25), tracker.Latest.Value);
			AssertEquals(2, tracker.Count);
			currentTime = startTime.AddMinutes(-15);
			tracker.Add();
			currentTime = startTime;
			AssertEquals(startTime.AddMinutes(-15), tracker.Latest.Value);
			AssertEquals(3, tracker.Count);
			currentTime = currentTime.AddMinutes(31);
			AssertEquals(startTime.AddMinutes(-15), tracker.Latest.Value);
			AssertEquals(2, tracker.Count);
			currentTime = currentTime.AddMinutes(30);
			AssertEquals(startTime.AddMinutes(-15), tracker.Latest.Value);
			AssertEquals(0, tracker.Count);
		}

		public void TestDropExpiredMultiThreading()
		{
			var startTime = DateTime.UtcNow;
			var currentTime = startTime;
			var tracker = new OccurrenceTracker(TimeSpan.FromHours(1), () => currentTime);
			var stopWatch = Stopwatch.StartNew();
			for (int i = 0; i < 60; i++)
			{
				tracker.Add();
				currentTime = currentTime.AddMinutes(1);
			}

			AssertEquals(60, tracker.Count);
			while (stopWatch.Elapsed < TimeSpan.FromSeconds(1))
			{
				tracker.Add();
				currentTime = currentTime.AddMinutes(1);
				Parallel.For(0, 20, (i) =>
				{
					var count = tracker.Count;
				});
				AssertEquals(60, tracker.Count);
			}
		}
	}
}