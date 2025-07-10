using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.EDI
{
	internal class DateRangeTest : TestCase
	{
		public void HasOverlap()
		{
			AssertHasOverlap(true, new ZDateTime(), new ZDateTime(), new ZDateTime(), new ZDateTime());
			AssertHasOverlap(true, new ZDateTime(2010, 01, 01), new ZDateTime(), new ZDateTime(), new ZDateTime());
			AssertHasOverlap(true, new ZDateTime(2010, 01, 01), new ZDateTime(2010, 12, 31), new ZDateTime(), new ZDateTime());
			AssertHasOverlap(true, new ZDateTime(2010, 01, 01), new ZDateTime(2010, 12, 31), new ZDateTime(2010, 08, 01), new ZDateTime());
			AssertHasOverlap(false, new ZDateTime(2010, 01, 01), new ZDateTime(2010, 12, 31), new ZDateTime(2011, 01, 01), new ZDateTime());
			AssertHasOverlap(true, new ZDateTime(2010, 01, 01), new ZDateTime(), new ZDateTime(2011, 01, 01), new ZDateTime());
			AssertHasOverlap(false, new ZDateTime(2011, 01, 01), new ZDateTime(2012, 12, 31), new ZDateTime(2010, 01, 01), new ZDateTime(2010, 12, 31));
			AssertHasOverlap(false, new ZDateTime(2011, 01, 01), new ZDateTime(), new ZDateTime(2010, 01, 01), new ZDateTime(2010, 12, 31));
			AssertHasOverlap(true, new ZDateTime(2011, 01, 01), new ZDateTime(), new ZDateTime(2010, 01, 01), ZDateTime.Empty);
		}

		public void IsConsecutive()
		{
			AssertIsConsecutive(true, new ZDate(), new ZDate(2018, 4, 30), new ZDate(2018, 5, 1), new ZDate());
			AssertIsConsecutive(true, new ZDate(), new ZDate(2018, 4, 29), new ZDate(2018, 4, 30), new ZDate());
			AssertIsConsecutive(true, new ZDate(), new ZDate(2017, 12, 31), new ZDate(2018, 1, 1), new ZDate());
			AssertIsConsecutive(false, new ZDate(), new ZDate(2017, 12, 31), new ZDate(2018, 1, 2), new ZDate());

			AssertIsConsecutive(false, new ZDate(), new ZDate(), new ZDate(), new ZDate());
			AssertIsConsecutive(false, new ZDate(2010, 01, 01), new ZDate(), new ZDate(), new ZDate());
			AssertIsConsecutive(false, new ZDate(2010, 01, 01), new ZDate(2010, 12, 31), new ZDate(), new ZDate());
			AssertIsConsecutive(false, new ZDate(2010, 01, 01), new ZDate(2010, 12, 31), new ZDate(2010, 08, 01), new ZDate());
			AssertIsConsecutive(false, new ZDate(2010, 01, 01), new ZDate(2010, 12, 31), new ZDate(2011, 01, 01), new ZDate());
			AssertIsConsecutive(false, new ZDate(2010, 01, 01), new ZDate(), new ZDate(2011, 01, 01), new ZDate());
			AssertIsConsecutive(false, new ZDate(2011, 01, 01), new ZDate(2012, 12, 31), new ZDate(2010, 01, 01), new ZDate(2010, 12, 31));
			AssertIsConsecutive(false, new ZDate(2011, 01, 01), new ZDate(), new ZDate(2010, 01, 01), new ZDate(2010, 12, 31));
			AssertIsConsecutive(false, new ZDate(2011, 01, 01), new ZDate(), new ZDate(2010, 01, 01), ZDate.Empty);
		}

		void AssertHasOverlap(bool expected, ZDateTime start1, ZDateTime end1, ZDateTime start2, ZDateTime end2)
		{
			AssertEquals(start1.ToString() + " to " + end1.ToString() + " vs " + start2.ToString() + " to " + end2.ToString(),
				expected, DateRange.HasOverlap(start1, end1, start2, end2));
		}

		void AssertIsConsecutive(bool expected, ZDate start1, ZDate end1, ZDate start2, ZDate end2)
		{
			AssertEquals(start1.ToString() + " to " + end1.ToString() + " vs " + start2.ToString() + " to " + end2.ToString(),
				expected, DateRange.IsConsecutive(start1, end1, start2, end2));

			AssertEquals(start1.ToString() + " to " + end1.ToString() + " reverse " + start2.ToString() + " to " + end2.ToString(),
				expected, DateRange.IsConsecutive(start2, end2, start1, end1));
		}
	}
}