using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	sealed class MCommodityCodeProviderTest : DataProviderTestCase<MCommodityCodeProvider>
	{
		public void TestHarmonizedSystemSubheadingCode()
		{
			AssertEquals("Harmonized System Subheading Code", "123456", mCommodityCodeProvider.HarmonizedSystemSubheadingCode);
		}

		public void TestCombinedNomenclatureCode()
		{
			AssertNull("Combined Nomenclature Code", mCommodityCodeProvider.CombinedNomenclatureCode);
		}

		public void TestTaricCode()
		{
			AssertNull("Taric Code", mCommodityCodeProvider.TaricCode);
		}

		public void TestTaricAdditionalCode()
		{
			AssertEquals("Taric Additional Code", 0, mCommodityCodeProvider.TaricAdditionalCode.Count);
		}

		public void TestNationalAdditionalCode()
		{
			AssertEquals("National Additional Code", 0, mCommodityCodeProvider.NationalAdditionalCode.Count);
		}

		public void TestTypeGoods()
		{
			AssertNull("Type Goods", mCommodityCodeProvider.TypeGoods);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			packedItem = Factory.New<AsycudaPackedItem>();
			packedItem.API_ABL_Bill = bill.PK;
			packedItem.API_Tariff = "12345678";

			mCommodityCodeProvider = MCommodityCodeProvider.NewOrNull(packedItem);
		}
		AsycudaPackedItem packedItem;
		MCommodityCodeProvider mCommodityCodeProvider;

		protected override MCommodityCodeProvider GetProvider()
		{
			return mCommodityCodeProvider;
		}
	}
}
