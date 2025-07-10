using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(CusContainer))]
	class CusContainerTest : EU.Business.Declaration.Testing.CusContainerTest
	{
		public void TestLookups()
		{
			AssertType<CusContainerLookups>(Factory.New<CusContainer>().Lookups);
		}
	}
}
