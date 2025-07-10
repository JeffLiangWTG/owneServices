using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace CargoWise.Types.Test
{
	public class DateRangeTest : TestCase
	{
		public void TestRangeAsExpected()
		{
			var d1 = DateTime.Today;
			var d2 = DateTime.Today.AddDays(5);
			var range = new DateRange(d1, d2);

			AssertEquals("Start", d1, range.Start);
			AssertEquals("End", d2, range.End);
		}

		public void TestTimesAreStripped()
		{
			var d1 = DateTime.Now;
			var d2 = DateTime.Now.AddDays(10).AddHours(4);
			var range = new DateRange(d1, d2);

			AssertEquals("Start", d1.Date, range.Start);
			AssertEquals("End", d2.Date, range.End);
		}

		public void TestStructuralEquality()
		{
			var d1 = DateTime.Today;
			var d2 = DateTime.Today.AddDays(5);
			var r1 = new DateRange(d1, d2);
			var r2 = new DateRange(d1, d2);

			AssertEquals("r1 eq r2", r1, r2);
			Assert("r1 == r2", r1 == r2);
		}

		public void TestDateTimesInRange()
		{
			var start = DateTime.Today;
			var end = DateTime.Today.AddDays(5);
			var range = new DateRange(start, end);
			var datetimes = new List<DateTime>
			{
				start,
				start.AddDays(1),
				end,
				end.AddDays(1).AddSeconds(-1)
			};

			datetimes.ForEach(dt => Assert($"{dt} in {range}", range.Contains(dt)));
		}

		public void TestDateTimesOutOfRange()
		{
			var start = DateTime.Today;
			var end = DateTime.Today.AddDays(5);
			var range = new DateRange(start, end);
			var datetimes = new List<DateTime>
			{
				start.AddSeconds(-1),
				end.AddDays(1)
			};

			datetimes.ForEach(dt => Assert($"{dt} in {range}", !range.Contains(dt)));
		}
	}
}
