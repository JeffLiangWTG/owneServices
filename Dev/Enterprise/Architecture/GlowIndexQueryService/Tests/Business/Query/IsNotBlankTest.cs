using GlowIndexQueryService.Business;
using NUnit.Framework;

namespace GlowIndexQueryService.Tests.Business
{
	public class IsNotBlankQueryTest : TestCase
	{
		public void TestNormal()
		{
			var term = new Term("city", "");
			var query = new IsNotBlankQuery(term);
			AssertEquals("((city ne null) and (city ne ''))", query.ToUrlComponent());
		}

		public void TestIncludeEmptyString()
		{
			var term = new Term("ETA", null);
			var query = new IsNotBlankQuery(term, false);
			AssertEquals("(ETA ne null)", query.ToUrlComponent());
		}
	}
}
