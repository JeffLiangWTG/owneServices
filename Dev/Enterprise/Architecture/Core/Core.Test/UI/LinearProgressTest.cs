using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class LinearProgressTest : TestCase
	{
		public void TestEstimateTimeRemaining()
		{
			AssertEquals(TimeSpan.FromTicks(1000), ProgressStatus.EstimateRemainingTime(TimeSpan.FromTicks(1000), 5, 10));
			AssertEquals(TimeSpan.FromTicks(500), ProgressStatus.EstimateRemainingTime(TimeSpan.FromTicks(1500), 15, 20));
			AssertEquals(TimeSpan.FromTicks(100), ProgressStatus.EstimateRemainingTime(TimeSpan.FromTicks(1900), 19, 20));
		}

		public void TestPrettyPrint()
		{
			AssertEquals("5.5 days", ProgressStatus.FormatTime(TimeSpan.FromDays(5.4894841)));
			AssertEquals("5.1 hours", ProgressStatus.FormatTime(TimeSpan.FromHours(5.1)));
			AssertEquals("5.1 minutes", ProgressStatus.FormatTime(TimeSpan.FromMinutes(5.1)));

			// No point updating for milliseconds, its too short a timeframe to be useful information for the user
			AssertEquals("5 seconds", ProgressStatus.FormatTime(TimeSpan.FromSeconds(5.1)));
			AssertEquals("0 seconds", ProgressStatus.FormatTime(TimeSpan.FromMilliseconds(5.1)));

			AssertEquals("30 hours", ProgressStatus.FormatTime(TimeSpan.FromHours(30)));
			AssertEquals("119 minutes", ProgressStatus.FormatTime(TimeSpan.FromMinutes(119)));
			AssertEquals("119 seconds", ProgressStatus.FormatTime(TimeSpan.FromSeconds(119)));
		}
	}
}
