using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class MoneyProviderTest : DataProviderTestCase<MoneyProvider>
	{
		public void TestIMoney()
		{
			Assert("Should implement IMoney", Provider is IMoney);
		}

		public void TestAmount()
		{
			AssertEquals(100m, Provider.Amount);
		}

		public void TestCurrency()
		{
			AssertEquals("EUR", Provider.Currency);
		}

		protected override MoneyProvider GetProvider()
		{
			return new MoneyProvider(100m, "EUR");
		}
	}
}
