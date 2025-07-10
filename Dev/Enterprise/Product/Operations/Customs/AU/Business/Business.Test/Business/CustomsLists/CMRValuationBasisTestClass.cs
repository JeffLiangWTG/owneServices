using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CMRValuationBasisTestClass : TestCase
	{
		public void TestCMRValuationBasisList()
		{
			CMRValuationBasisList codeList = new CMRValuationBasisList();
			AssertEquals("6 codes in the list", 6, codeList.Count);
		}
	}
}
