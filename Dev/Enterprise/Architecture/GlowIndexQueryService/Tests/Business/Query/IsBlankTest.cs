using GlowIndexQueryService.Business;
using NUnit.Framework;

namespace GlowIndexQueryService.Tests.Business
{
	public class IsBlankTest : TestCase
	{
		public void TestNormal()
		{
			var term = new Term("city", "");
			var query = new IsBlankQuery(term);
			AssertEquals("((city eq null) or (city eq ''))", query.ToUrlComponent());
		}

		public void TestIncludeEmptyString()
		{
			var term = new Term("ETA", null);
			var query = new IsBlankQuery(term, false);
			AssertEquals("(ETA eq null)", query.ToUrlComponent());
		}
	}
}
