namespace Enterprise.ProductRegistration.Common.Test
{
	using System;
	using NUnit.Framework;

	class BillingTimeZoneInfoTest : TestCase
	{
		public void TestBillingTimeZoneInfo()
		{
			var utcTime1 = new DateTime(2016, 1, 19, 11, 15, 20);
			var info1 = new BillingTimeZoneInfo(utcTime1);
			CombineAssertions(() =>
			{
				AssertEquals("Current offset", 11.0d, info1.CurrentBillingTimeZoneUtcOffset);
				AssertEquals("Next offset", 10.0d, info1.NextBillingTimeZoneUtcOffset);
				AssertEquals("Effective time UTC", new DateTime(2016, 4, 2, 16, 0, 0), info1.NextUtcOffsetEffectiveTimeUtc);
			});

			var utcTime2 = new DateTime(2016, 5, 1, 15, 12, 37);
			var info2 = new BillingTimeZoneInfo(utcTime2);
			CombineAssertions(() =>
			{
				AssertEquals("Current offset", 10.0d, info2.CurrentBillingTimeZoneUtcOffset);
				AssertEquals("Next offset", 11.0d, info2.NextBillingTimeZoneUtcOffset);
				AssertEquals("Effective time UTC", new DateTime(2016, 10, 1, 16, 0, 0), info2.NextUtcOffsetEffectiveTimeUtc);
			});
		}
	}
}
