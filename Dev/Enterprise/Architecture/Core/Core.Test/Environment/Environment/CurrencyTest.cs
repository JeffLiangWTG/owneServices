using CargoWise.Data;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class CurrencyTest : NUnit.Framework.TestCase
	{
		public void TestCurrencyLoadingCached()
		{
			Currency.CurrencyCache.Clear();

			Currency testCurrency = new Currency("AUD");
			int dbCountBefore = Db.Connection.ExecutedCommandCount;
			Currency testCurrency2 = new Currency("AUD");
			AssertEquals(dbCountBefore, Db.Connection.ExecutedCommandCount);

			Currency testCurrency3 = new Currency("USD");
			AssertEquals(dbCountBefore + 1, Db.Connection.ExecutedCommandCount);
		}

		public void TestCurrencyConstructor()
		{
			Currency testCurrency = new Currency("AUD");
			AssertEquals("Code", "AUD", testCurrency.Code);
			AssertEquals("SubUnitRatio", 100, testCurrency.SubUnitRatio);
			AssertEquals("Decimals", 2, testCurrency.Decimals);
		}
	}
}
