using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business
{
	sealed class DateRangeSpanPairTest : TestCase
	{
		[TestDate(2007, 1, 1)]
		public void TestDescription()
		{
			AssertEquals("31-Dec-06 to 06-Jan-07", new ModuleDateFilter.DateRangeSpanPair((NoResString)"", ModuleDateFilter.DateRangeSpan.CurrentWeek).Description);
			AssertEquals("24-Dec-06 to 30-Dec-06", new ModuleDateFilter.DateRangeSpanPair((NoResString)"", ModuleDateFilter.DateRangeSpan.LastWeek).Description);
			AssertEquals("07-Jan-07 to 13-Jan-07", new ModuleDateFilter.DateRangeSpanPair((NoResString)"", ModuleDateFilter.DateRangeSpan.NextWeek).Description);

			AssertEquals("01-Jan-07 to 31-Jan-07", new ModuleDateFilter.DateRangeSpanPair((NoResString)"", ModuleDateFilter.DateRangeSpan.CurrentMonth).Description);
			AssertEquals("01-Dec-06 to 31-Dec-06", new ModuleDateFilter.DateRangeSpanPair((NoResString)"", ModuleDateFilter.DateRangeSpan.LastMonth).Description);
			AssertEquals("01-Feb-07 to 28-Feb-07", new ModuleDateFilter.DateRangeSpanPair((NoResString)"", ModuleDateFilter.DateRangeSpan.NextMonth).Description);
		}

		[TestDate]
		public void TestToDateForNextMonth()
		{
			TestDateAttribute.Date = new DateTime(2011, 1, 14);
			AssertEquals(new ZDateTime(2011, 2, 28, 23, 59, 59, 997), new ModuleDateFilter.DateRangeSpanPair((NoResString)"", ModuleDateFilter.DateRangeSpan.NextMonth).ToDate);

			TestDateAttribute.Date = new DateTime(2011, 2, 14);
			AssertEquals(new ZDateTime(2011, 3, 31, 23, 59, 59, 997), new ModuleDateFilter.DateRangeSpanPair((NoResString)"", ModuleDateFilter.DateRangeSpan.NextMonth).ToDate);
		}
	}
}
