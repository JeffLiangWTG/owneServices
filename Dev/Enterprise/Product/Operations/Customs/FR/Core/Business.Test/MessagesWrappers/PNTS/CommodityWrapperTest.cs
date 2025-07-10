using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS.Testing
{
	class CommodityWrapperTest : Customs.Business.Testing.DataProviderTestCase<CommodityWrapper>
	{
		public void TestCusCode()
		{
			AssertEquals("Wrapper CusCode should be equal to item.API_ChemicalSubstanceCode.", "76", Provider.CusCode);
		}

		public void TestDescriptionOfGoods()
		{
			AssertEquals("Wrapper DescriptionOfGoods should equal Item API_GoodsDescription.", "GoodsDescription", Provider.DescriptionOfGoods);
		}

		public void TestCommodityCode()
		{
			CombineAssertions("Wrapper CommodityCode should use CommodityCodeWrapper as sub wrapper", () =>
			{
				var commodityCodeWrapper = CommodityCodeWrapper.New(GetTestItem());
				AssertEquals("CombinedNomenclatureCode property.", "78", Provider.CommodityCode.CombinedNomenclatureCode);
				AssertEquals("HarmonizedSystemSubHeadingCode property.", "234516", Provider.CommodityCode.HarmonizedSystemSubHeadingCode);
			});
		}

		protected override CommodityWrapper GetProvider() => CommodityWrapper.New(GetTestItem());

		TemporaryStoragePackedItem GetTestItem()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var item = header.Bills.AddNew().PackedItems.AddNew();
			item.API_GoodsDescription = "GoodsDescription";
			item.API_Tariff = "2345167890";
			item.API_ChemicalSubstanceCode = "76";
			return item;
		}
	}
}
