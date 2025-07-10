using CargoWise.Customs.DE.MessageContracts;

namespace Enterprise.Customs.DE.Business.Testing
{
	class ProductProviderTest : Customs.Business.Testing.DataProviderTestCase<ProductProvider>
	{
		public void TestNew()
		{
			CombineAssertions(() =>
			{
				AssertNull("Argument == null", ProductProvider.NewOrNull(null));
				AssertNotNull("Valid argument", dataProvider);
			});
		}

		public void TestCommodityCode() => AssertEquals("123456", dataProvider.CommodityCode);

		public void TestGoodsDescription() => AssertEquals("DESCRIPTION", dataProvider.GoodsDescription);

		public void TestHarmonizedSystemSubHeadingCode() => AssertEquals("807010", dataProvider.HarmonizedSystemSubHeadingCode);

		public void TestCombinedNomenclatureCode() => AssertEquals("10", dataProvider.CombinedNomenclatureCode);

		protected override void SetUp()
		{
			base.SetUp();
			var productSupportingInfo = Factory.New<ProductSupportingInfo>();
			productSupportingInfo.CSI_Code = "123456";
			productSupportingInfo.CSI_Description = "DESCRIPTION";
			productSupportingInfo.CSI_Tariff = "80701010";
			dataProvider = ProductProvider.NewOrNull(productSupportingInfo);
		}
		IProduct dataProvider;

		protected override ProductProvider GetProvider() => (ProductProvider)dataProvider;
	}
}
