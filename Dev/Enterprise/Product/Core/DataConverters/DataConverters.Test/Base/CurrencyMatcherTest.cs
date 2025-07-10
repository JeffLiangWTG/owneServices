using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DataConverters.Testing.Base
{
	sealed internal class CurrencyMatcherTest : TestCaseWithFactory
	{
		public void TestMatchToCode()
		{
			var currency = RefCurrency.New(Factory);
			currency.RX_Code = "123";

			var matcher = new CurrencyMatcher("123", Factory);
			AssertEquals(currency.PK, matcher.MatchedPK);
			AssertEquals("123", matcher.MatchedCode);
		}

		public void TestNoMatch()
		{
			var matcher = new CurrencyMatcher("123", Factory);
			AssertEquals(ZGuid.Empty, matcher.MatchedPK);
			AssertEquals(ZString.Empty, matcher.MatchedCode);
		}

		public void TestMatcherWithEmptyCode()
		{
			var matcher = new CurrencyMatcher(ZString.Empty, Factory);
			AssertEquals(ZGuid.Empty, matcher.MatchedPK);
			AssertEquals(ZString.Empty, matcher.MatchedCode);
		}
	}
}
