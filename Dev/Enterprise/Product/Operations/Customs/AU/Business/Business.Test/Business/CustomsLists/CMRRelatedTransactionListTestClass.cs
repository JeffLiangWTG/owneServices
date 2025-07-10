using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CMRRelatedTransactionListTestClass : TestCase
	{
		public void TestCMRRelatedTransactionList()
		{
			CMRRelatedTransactionList codeList = new CMRRelatedTransactionList();
			AssertEquals("3 codes in the list", 3, codeList.Count);
		}
	}
}
