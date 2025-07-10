using GlowIndexQueryService.Business;
using NUnit.Framework;

namespace GlowIndexQueryService.Tests.Business
{
	public class EmptyQueryTest : TestCase
	{
		public void TestUri()
		{
			var emptyQuery = new EmptyQuery();
			AssertEquals(string.Empty, emptyQuery.ToUrlComponent());
		}
	}
}
