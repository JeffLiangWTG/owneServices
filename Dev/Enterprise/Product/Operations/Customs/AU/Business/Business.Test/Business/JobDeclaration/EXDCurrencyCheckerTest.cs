using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class EXDCurrencyCheckerTest : TestCaseWithFactory
	{
		public void TestNullCurrency()
		{
			Assert(!EXDCurrencyChecker.IsValidCurrency(null));
		}

		public void TestValidCurrency()
		{
			Assert(EXDCurrencyChecker.IsValidCurrency(RefCurrency.LoadFromCurrencyCode(Factory, "USD")));
		}

		public void TestInvalidCurrency()
		{
			Assert(!EXDCurrencyChecker.IsValidCurrency(RefCurrency.LoadFromCurrencyCode(Factory, "XOF")));
		}
	}
}
