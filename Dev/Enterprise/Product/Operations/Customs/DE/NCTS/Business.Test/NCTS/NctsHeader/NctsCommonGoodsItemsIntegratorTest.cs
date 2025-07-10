using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.CommonGoodsItemsIntegration;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NctsCommonGoodsItemsIntegratorTest : TestCaseWithFactory
	{
		public void TestCopySupplementaryQuantityFromCommonGoodsItem()
		{
			var goodsItem = new CommonGoodsItem()
			{
				SupplementaryQuantity = 1.1m,
				SupplementaryQuantityUnit = "NAR"
			};

			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			var commonGoodsItemsIntegrator = new NctsCommonGoodsItemsIntegrator(nctsHeader);
			var target = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			commonGoodsItemsIntegrator.CopyFromCommonGoodsItem(goodsItem, target);

			CombineAssertions("Value should have been copied to the goodsitem.", () =>
			{
				AssertEquals("SupplementaryQuantity", 0m, target.BY_CustomsSecondQuantity);
				AssertEquals("SupplementaryUnitQuantity", "", target.BY_CustomsSecondUnitQty);
			});
		}
	}
}
