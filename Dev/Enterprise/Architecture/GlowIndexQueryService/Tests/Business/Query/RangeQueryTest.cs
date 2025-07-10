using GlowIndexQueryService.Business;
using NUnit.Framework;

namespace GlowIndexQueryService.Tests.Business
{
	public class RangeQueryTest : TestCase
	{
		public void TestDateCloseInterval()
		{
			var rangeQuery = CreateDateRangeQuery(true, true);
			AssertEquals("((begin ge 2023-01-01T00:00:00Z) and (end le 2023-01-31T00:00:00Z))", rangeQuery.ToUrlComponent());
		}

		public void TestDateOpenClose()
		{
			var rangeQuery = CreateDateRangeQuery(false, true);
			AssertEquals("((begin gt 2023-01-01T00:00:00Z) and (end le 2023-01-31T00:00:00Z))", rangeQuery.ToUrlComponent());
		}

		public void TestDateCloseOpen()
		{
			var rangeQuery = CreateDateRangeQuery(true, false);
			AssertEquals("((begin ge 2023-01-01T00:00:00Z) and (end lt 2023-01-31T00:00:00Z))", rangeQuery.ToUrlComponent());
		}

		public void TestDateOpenInterval()
		{
			var rangeQuery = CreateDateRangeQuery(false, false);
			AssertEquals("((begin gt 2023-01-01T00:00:00Z) and (end lt 2023-01-31T00:00:00Z))", rangeQuery.ToUrlComponent());
		}

		public void TestBeginEmpty()
		{
			var beginTerm = new Term("begin", "");
			var endTerm = new Term("end", "12");
			var rangeQuery = new RangeQuery(beginTerm, endTerm, true, false);
			AssertEquals("(end lt 12)", rangeQuery.ToUrlComponent());
		}

		public void TestEndEmpty()
		{
			var beginTerm = new Term("begin", "1");
			var endTerm = new Term("end", "");
			var rangeQuery = new RangeQuery(beginTerm, endTerm, true, false);
			AssertEquals("(begin ge 1)", rangeQuery.ToUrlComponent());
		}

		public void TestBothEmpty()
		{
			var beginTerm = new Term("begin", "");
			var endTerm = new Term("end", "");
			var rangeQuery = new RangeQuery(beginTerm, endTerm, true, false);
			AssertEquals("", rangeQuery.ToUrlComponent());
		}

		RangeQuery CreateDateRangeQuery(bool beginInclusive = true, bool endInclusive = true)
		{
			var beginTerm = new Term("begin", "2023-01-01T00:00:00Z");
			var endTerm = new Term("end", "2023-01-31T00:00:00Z");
			return new RangeQuery(beginTerm, endTerm, beginInclusive, endInclusive);
		}
	}
}
