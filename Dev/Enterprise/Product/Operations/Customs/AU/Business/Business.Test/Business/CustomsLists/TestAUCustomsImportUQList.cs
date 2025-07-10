using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class TestAUCustomsImportUQList : TestCase
	{
		public void TestListCount()
		{
			AssertEquals(17, new AUCustomsImportUQList().Count);
		}
	}
}
