using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	sealed class MCommodityProviderTest : DataProviderTestCase<MCommodityProvider>
	{
		public void TestDescriptionOfGoods()
		{
			AssertEquals("Description Of Goods", "GoodsDescription", mCommodityProvider.DescriptionOfGoods);
			var providerWithNullPack = new MCommodityProvider(Factory.New<AsycudaPackedItem>());
			AssertNotNull("Description of Goods", providerWithNullPack.DescriptionOfGoods);
		}

		public void TestCusCode()
		{
			AssertNull("Description Of Goods", mCommodityProvider.CusCode);
		}

		public void TestQuotaOrderNumber()
		{
			AssertNull("Quota Order Number", mCommodityProvider.QuotaOrderNumber);
		}

		public void TestCommodityCode()
		{
			AssertNotNull("Commodity Code", mCommodityProvider.CommodityCode);
		}

		public void TestGoodsMeasure()
		{
			AssertNotNull("Goods Measure", mCommodityProvider.GoodsMeasure);
		}

		public void TestInvoiceLine()
		{
			AssertNotNull("Invoice Line", mCommodityProvider.InvoiceLine);
		}

		public void TestCalculationOfTaxes()
		{
			AssertNull("Calculation Of Taxes", mCommodityProvider.CalculationOfTaxes);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			pack = bill.Packs.AddNew();

			var packedItem = bill.PackedItems.AddNew();
			packedItem.AsycudaPackPackedItemPivots.AddPivotFor(pack);
			packedItem.API_GoodsDescription = "GoodsDescription";

			mCommodityProvider = new MCommodityProvider(packedItem);
		}
		AsycudaPack pack;
		MCommodityProvider mCommodityProvider;

		protected override MCommodityProvider GetProvider()
		{
			return mCommodityProvider;
		}
	}
}
