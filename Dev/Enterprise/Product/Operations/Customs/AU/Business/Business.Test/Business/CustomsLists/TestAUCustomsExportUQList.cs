using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class TestAUCustomsExportUQList : TestCase
	{
		public void TestListCount()
		{
			AssertEquals(15, new AUCustomsExportUQList().Count);
		}
	}
}
