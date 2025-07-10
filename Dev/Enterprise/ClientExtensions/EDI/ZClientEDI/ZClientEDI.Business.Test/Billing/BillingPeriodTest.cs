using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	public class BillingPeriodTest : TestCaseWithFactory
	{
		public void TestUtcToPeriod()
		{
			var converter = new BillingPeriodConverter();

			AssertEquals(201601, converter.UtcToPeriod(new DateTime(2016, 1, 31, 23, 59, 59)));
			AssertEquals(201602, converter.UtcToPeriod(new DateTime(2016, 2, 1, 0, 0, 0)));
			AssertEquals(201602, converter.UtcToPeriod(new DateTime(2016, 2, 29, 12, 59, 59)));
			AssertEquals(201603, converter.UtcToPeriod(new DateTime(2016, 2, 29, 13, 0, 0)));
			AssertEquals(201603, converter.UtcToPeriod(new DateTime(2016, 3, 31, 12, 59, 59)));
			AssertEquals(201604, converter.UtcToPeriod(new DateTime(2016, 3, 31, 13, 0, 0)));

			AssertEquals(201709, converter.UtcToPeriod(new DateTime(2017, 9, 30, 13, 59, 59)));
			AssertEquals(201710, converter.UtcToPeriod(new DateTime(2017, 9, 30, 14, 0, 0)));
			AssertEquals(201710, converter.UtcToPeriod(new DateTime(2017, 10, 31, 12, 59, 59)));
			AssertEquals(201711, converter.UtcToPeriod(new DateTime(2017, 10, 31, 13, 0, 0)));

			AssertEquals(201803, converter.UtcToPeriod(new DateTime(2018, 3, 31, 12, 59, 59)));
			AssertEquals(201804, converter.UtcToPeriod(new DateTime(2018, 3, 31, 13, 0, 0)));
			AssertEquals(201804, converter.UtcToPeriod(new DateTime(2018, 4, 30, 13, 59, 59)));
			AssertEquals(201805, converter.UtcToPeriod(new DateTime(2018, 4, 30, 14, 0, 0)));
		}

		public void TestBillingPeriodStartAndEndTimeUtc()
		{
			var period = new BillingPeriod(new ZDateTime(2016, 1, 1));
			AssertEquals("Current behaviour: start with UTC time", new ZDateTime(2016, 1, 1, 0, 0, 0), period.StartTimeUtc);
			AssertEquals("Current behaviour: end with UTC time", new ZDateTime(2016, 2, 1, 0, 0, 0), period.EndTimeUtc);

			period = new BillingPeriod(new ZDateTime(2016, 2, 1));
			AssertEquals("Transition period: start with UTC time", new ZDateTime(2016, 2, 1, 0, 0, 0), period.StartTimeUtc);
			AssertEquals("Transition period: end with Sydney time (AEDT)", new ZDateTime(2016, 2, 29, 13, 0, 0), period.EndTimeUtc);

			period = new BillingPeriod(new ZDateTime(2016, 3, 1));
			AssertEquals("New behaviour: start with Sydney time (AEDT)", new ZDateTime(2016, 2, 29, 13, 0, 0), period.StartTimeUtc);
			AssertEquals("New behaviour: start with Sydney time (AEDT)", new ZDateTime(2016, 3, 31, 13, 0, 0), period.EndTimeUtc);

			period = new BillingPeriod(new ZDateTime(2016, 4, 1));
			AssertEquals("DST ends: start with Sydney time (AEDT)", new ZDateTime(2016, 3, 31, 13, 0, 0), period.StartTimeUtc);
			AssertEquals("DST ends: end with Sydney time (AEST)", new ZDateTime(2016, 4, 30, 14, 0, 0), period.EndTimeUtc);

			period = new BillingPeriod(new ZDateTime(2016, 5, 1));
			AssertEquals("New behaviour with standard time: start with Sydney time (AEST)", new ZDateTime(2016, 4, 30, 14, 0, 0), period.StartTimeUtc);
			AssertEquals("New behaviour with standard time: end with Sydney time (AEST)", new ZDateTime(2016, 5, 31, 14, 0, 0), period.EndTimeUtc);

			period = new BillingPeriod(new ZDateTime(2016, 10, 1));
			AssertEquals("DST begins: start with Sydney time (AEST)", new ZDateTime(2016, 9, 30, 14, 0, 0), period.StartTimeUtc);
			AssertEquals("DST begins: end with Sydney time (AEDT)", new ZDateTime(2016, 10, 31, 13, 0, 0), period.EndTimeUtc);

			period = new BillingPeriod(new ZDateTime(2016, 11, 1));
			AssertEquals("New behaviour with DST: start with Sydney time (AEDT)", new ZDateTime(2016, 10, 31, 13, 0, 0), period.StartTimeUtc);
			AssertEquals("New behaviour with DST: end with Sydney time (AEDT)", new ZDateTime(2016, 11, 30, 13, 0, 0), period.EndTimeUtc);
		}
	}
}