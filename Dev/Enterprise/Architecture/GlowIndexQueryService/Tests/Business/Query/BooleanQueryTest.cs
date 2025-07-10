using GlowIndexQueryService.Business;
using NUnit.Framework;

namespace GlowIndexQueryService.Tests.Business
{
	public class BooleanQueryTest : TestCase
	{
		public void TestAnd()
		{
			var term = new Term("city", "Boston");
			var query1 = new EqualQuery(term);
			var query2 = new NotEqualQuery(term);
			var query3 = new PrefixQuery(term);
			var booleanQuery = new BooleanQuery(BooleanOperator.And, query1, query2, query3);
			AssertEquals("((city eq 'Boston') and (city ne 'Boston') and (startswith(city,'Boston')))", booleanQuery.ToUrlComponent());
		}

		public void TestOr()
		{
			var term = new Term("city", "Boston");
			var query1 = new EqualQuery(term);
			var query2 = new NotEqualQuery(term);
			var query3 = new PrefixQuery(term);
			var booleanQuery = new BooleanQuery(BooleanOperator.Or, query1, query2, query3);
			AssertEquals("((city eq 'Boston') or (city ne 'Boston') or (startswith(city,'Boston')))", booleanQuery.ToUrlComponent());
		}

		public void TestOnlyOneCondition()
		{
			var term = new Term("city", "Boston");
			var query = new EqualQuery(term);
			var booleanQuery = new BooleanQuery(BooleanOperator.And, query);
			AssertEquals("(city eq 'Boston')", booleanQuery.ToUrlComponent());
		}

		public void TestEmpty()
		{
			var booleanQuery = new BooleanQuery(BooleanOperator.And, new EmptyQuery());
			AssertEquals("", booleanQuery.ToUrlComponent());
		}
	}
}
