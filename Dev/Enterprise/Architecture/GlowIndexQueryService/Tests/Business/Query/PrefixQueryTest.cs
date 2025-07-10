using GlowIndexQueryService.Business;
using NUnit.Framework;

namespace GlowIndexQueryService.Tests.Business
{
	public class PrefixQueryTest : TestCase
	{
		public void TestNormal()
		{
			var term = new Term("city", "Boston");
			var prefixQuery = new PrefixQuery(term);
			AssertEquals("(startswith(city,'Boston'))", prefixQuery.ToUrlComponent());
		}

		public void TestEmpty()
		{
			var term = new Term("city", "");
			var prefixQuery = new PrefixQuery(term);
			AssertEquals("", prefixQuery.ToUrlComponent());
		}
	}
}
