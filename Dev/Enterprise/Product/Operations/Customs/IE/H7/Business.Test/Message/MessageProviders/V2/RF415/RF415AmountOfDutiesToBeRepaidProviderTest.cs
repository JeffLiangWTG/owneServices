using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	sealed class RF415AmountOfDutiesToBeRepaidProviderTest : DataProviderTestCase<RF415AmountOfDutiesToBeRepaidProvider>
	{
		public void TestAmount()
		{
			AssertEquals("Amount", expectedAmount, Provider.Amount);
		}

		public void TestCurrency()
		{
			AssertEquals("Currency", expectedDefaultCurrency, Provider.Currency);
		}

		protected sealed override RF415AmountOfDutiesToBeRepaidProvider GetProvider()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var sendingObject = new RF415MessageSendingObject(bill);
			sendingObject.Amount = expectedAmount;

			return new RF415AmountOfDutiesToBeRepaidProvider(sendingObject);
		}

		readonly decimal expectedAmount = 2000;
		readonly string expectedDefaultCurrency = "EUR";
	}
}

