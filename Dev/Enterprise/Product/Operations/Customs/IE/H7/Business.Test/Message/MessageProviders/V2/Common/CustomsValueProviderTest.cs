using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	sealed class CustomsValueProviderTest : DataProviderTestCase<CustomsValueProvider>
	{
		public void TestAmount()
		{
			AssertEquals("Amount", expectedGoodsValue, Provider.Amount);
		}

		public void TestCurrency()
		{
			AssertEquals("Currency", expectedCurrency, Provider.Currency);
		}

		protected sealed override CustomsValueProvider GetProvider()
		{
			return new CustomsValueProvider(packedItem);
		}

		protected override void SetUp()
		{
			base.SetUp();

			bill = Factory.New<AsycudaBill>();
			packedItem = bill.PackedItems.AddNew();
			packedItem.API_GoodsValue = expectedGoodsValue;
			packedItem.API_RX_NKGoodsValueCurrency = expectedCurrency;
		}

		AsycudaBill bill;
		AsycudaPackedItem packedItem;
		readonly decimal expectedGoodsValue = 123456;
		readonly string expectedCurrency = "EUR";
	}
}

