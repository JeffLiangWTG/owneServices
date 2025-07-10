using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class DaylightAnnualInfoTest : TestCase
	{
		[ExpectException(typeof(ArgumentException))]
		public void TestConstructorThrowsExceptionIfYearsDoNotMatch()
		{
			new DaylightAnnualInfo(new DateTime(2006, 10, 28), new DateTime(2005, 3, 29));
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestIsDaylightSavingBasedOnUtcThrowsExceptionIfYearsDoNotMatch()
		{
			DaylightAnnualInfo dstInfo = new DaylightAnnualInfo(new DateTime(2006, 10, 28), new DateTime(2006, 4, 1));
			dstInfo.IsDaylightSavingBasedOnUtc(new DateTime(2007, 1, 1));
		}

		public void TestIsDaylightSavingBasedOnUtc()
		{
			DaylightAnnualInfo dstInfo = new DaylightAnnualInfo(new DateTime(2007, 03, 01, 5, 0, 0), new DateTime(2007, 09, 30, 5, 0, 0));

			// DST START = 01/03/2007 @5am

			DateTime utcDateTime = new DateTime(2007, 03, 01, 4, 0, 0);
			AssertEquals("UTC 01/03/2007 @4am - Daylight Saving:", false, dstInfo.IsDaylightSavingBasedOnUtc(utcDateTime));

			utcDateTime = new DateTime(2007, 03, 01, 4, 1, 0);
			AssertEquals("UTC 01/03/2007 @4:01am - Daylight Saving:", false, dstInfo.IsDaylightSavingBasedOnUtc(utcDateTime));

			utcDateTime = new DateTime(2007, 03, 01, 4, 59, 0);
			AssertEquals("UTC 01/03/2007 @4:59am - Daylight Saving:", false, dstInfo.IsDaylightSavingBasedOnUtc(utcDateTime));

			utcDateTime = new DateTime(2007, 03, 01, 5, 0, 0);
			AssertEquals("UTC 01/03/2007 @5am - Daylight Saving:", true, dstInfo.IsDaylightSavingBasedOnUtc(utcDateTime));

			utcDateTime = new DateTime(2007, 03, 01, 5, 1, 0);
			AssertEquals("UTC 01/03/2007 @5:01am - Daylight Saving:", true, dstInfo.IsDaylightSavingBasedOnUtc(utcDateTime));

			utcDateTime = new DateTime(2007, 03, 01, 6, 0, 0);
			AssertEquals("UTC 01/03/2007 @6am - Daylight Saving:", true, dstInfo.IsDaylightSavingBasedOnUtc(utcDateTime));

			utcDateTime = new DateTime(2007, 03, 01, 6, 1, 0);
			AssertEquals("UTC 01/03/2007 @6:01am - Daylight Saving:", true, dstInfo.IsDaylightSavingBasedOnUtc(utcDateTime));

			// DST END = 30/09/2007 @5am

			utcDateTime = new DateTime(2007, 09, 30, 4, 0, 0);
			AssertEquals("UTC 30/09/2007 @4am - Daylight Saving:", true, dstInfo.IsDaylightSavingBasedOnUtc(utcDateTime));

			utcDateTime = new DateTime(2007, 09, 30, 4, 1, 0);
			AssertEquals("UTC 30/09/2007 @4:01am - Daylight Saving:", true, dstInfo.IsDaylightSavingBasedOnUtc(utcDateTime));

			utcDateTime = new DateTime(2007, 09, 30, 4, 59, 0);
			AssertEquals("UTC 30/09/2007 @4:59am - Daylight Saving:", true, dstInfo.IsDaylightSavingBasedOnUtc(utcDateTime));

			utcDateTime = new DateTime(2007, 09, 30, 5, 0, 0);
			AssertEquals("UTC 30/09/2007 @5am - Daylight Saving:", false, dstInfo.IsDaylightSavingBasedOnUtc(utcDateTime));

			utcDateTime = new DateTime(2007, 09, 30, 5, 1, 0);
			AssertEquals("UTC 30/09/2007 @5:01am - Daylight Saving:", false, dstInfo.IsDaylightSavingBasedOnUtc(utcDateTime));

			utcDateTime = new DateTime(2007, 09, 30, 6, 0, 0);
			AssertEquals("UTC 30/09/2007 @6am - Daylight Saving:", false, dstInfo.IsDaylightSavingBasedOnUtc(utcDateTime));

			utcDateTime = new DateTime(2007, 09, 30, 6, 1, 0);
			AssertEquals("UTC 30/09/2007 @6:01am - Daylight Saving:", false, dstInfo.IsDaylightSavingBasedOnUtc(utcDateTime));
		}

		public void TestIsDaylightSavingBasedOnUtcWhenStartAndEndTransitionsAreTheSame()
		{
			DaylightAnnualInfo dstInfo = new DaylightAnnualInfo(new DateTime(2007, 01, 01, 0, 0, 0), new DateTime(2007, 01, 01, 0, 0, 0));
			DateTime utcDateTime = new DateTime(2007, 01, 01, 0, 0, 0);
			AssertEquals("UTC 01/01/2007 @12am - Daylight Saving:", false, dstInfo.IsDaylightSavingBasedOnUtc(utcDateTime));
			utcDateTime = new DateTime(2007, 06, 01, 1, 0, 0);
			AssertEquals("UTC 01/06/2007 @1am - Daylight Saving:", false, dstInfo.IsDaylightSavingBasedOnUtc(utcDateTime));
			utcDateTime = new DateTime(2007, 12, 31, 23, 59, 59);
			AssertEquals("UTC 31/12/2007 @23:59:59am - Daylight Saving:", false, dstInfo.IsDaylightSavingBasedOnUtc(utcDateTime));
		}
	}
}
