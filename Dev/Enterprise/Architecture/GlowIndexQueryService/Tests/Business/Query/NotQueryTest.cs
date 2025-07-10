using GlowIndexQueryService.Business;
using NUnit.Framework;

namespace GlowIndexQueryService.Tests.Business
{
	public class NotQueryTest : TestCase
	{
		public void TestNotEqual()
		{
			var term = new Term("city", "Boston");
			var equalQuery = new EqualQuery(term);
			AssertUrlComponent("(not(city eq 'Boston'))", equalQuery);
		}

		public void TestNotPrefix()
		{
			var term = new Term("city", "Boston");
			var prefixQuery = new PrefixQuery(term);
			AssertUrlComponent("(not(startswith(city,'Boston')))", prefixQuery);
		}

		public void TestEmpty()
		{
			var term = new Term("city", "");
			var equalQuery = new EqualQuery(term);
			AssertUrlComponent("", equalQuery);
		}

		void AssertUrlComponent(string expected, IGlowQuery query)
		{
			var actual = new NotQuery(query).ToUrlComponent();
			AssertEquals(expected, actual);
		}
	}
}
