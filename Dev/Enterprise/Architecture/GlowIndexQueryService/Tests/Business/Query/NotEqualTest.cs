using GlowIndexQueryService.Business;
using NUnit.Framework;

namespace GlowIndexQueryService.Tests.Business
{
	public class NotEqualTest : TestCase
	{
		public void TestNormal()
		{
			var term = new Term("city", "Boston");
			var notEqualQuery = new NotEqualQuery(term);
			AssertEquals("(city ne 'Boston')", notEqualQuery.ToUrlComponent());
		}

		public void TestFlags()
		{
			var term = new Term("isActive", "true");
			var equalQuery = new NotEqualQuery(term, useQuotes: false);
			AssertEquals("(isActive ne true)", equalQuery.ToUrlComponent());
		}

		public void TestEmpty()
		{
			var term = new Term("city", "");
			var notEqualQuery = new NotEqualQuery(term);
			AssertEquals("", notEqualQuery.ToUrlComponent());
		}
	}
}
