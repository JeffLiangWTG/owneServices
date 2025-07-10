using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	public class ExchangeRateToleranceLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCurrencyList()
		{
			var newCur = Factory.NewWithValidTestData<RefCurrency>();
			newCur.RX_Code = "AAA";

			Factory.Save();

			var currencies = new RefCurrencyCollection(Factory).Cast<RefCurrency>()
				.Select(currency => currency.Code)
				.ToArray();

			AssertContainsExactElementsInAnyOrder(currencies, Parent.Lookups.CurrencyList.GetAllCodes());

			AssertEquals(true, Parent.Lookups.CurrencyList.GetAllCodes().Contains("AAA"));
		}

		public void TestAllCurrencyCode()
		{
			AssertEquals("All Currencies", ExchangeRateToleranceLookups.AllCurrencyCode);
		}

		ExchangeRateTolerance Parent => parent ?? (parent = new ExchangeRateTolerance());

		ExchangeRateTolerance parent;
	}
}
