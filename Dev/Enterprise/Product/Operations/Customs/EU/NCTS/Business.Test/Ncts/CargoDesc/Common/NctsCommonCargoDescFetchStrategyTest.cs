using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business.Testing.Ncts.CargoDesc.Common
{
	sealed class NctsCommonCargoDescFetchStrategyTest : BusinessObjectFetchStrategyTestCase
	{
		protected override IBusinessObjectCollection CreateCollectionToTest(BusinessObjectFactory factory)
		{
			return new NctsCommonCargoDescCollection<NctsCommonCargoDesc>(factory.Load<NctsBill>(Bill.PK));
		}

		public void TestFetchForView()
		{
			var goodsItem1 = Bill.GoodsItems.AddNew();
			goodsItem1.AdditionalSupplementaryCodes.AddNew("A");
			goodsItem1.AdditionalSupplementaryCodes.AddNew("C");

			var goodsItem2 = Bill.GoodsItems.AddNew();
			goodsItem2.AdditionalSupplementaryCodes.AddNew("B");
			goodsItem2.AdditionalSupplementaryCodes.AddNew("D");
			Factory.Save();

			TestFetchForView(goodsItem1, goodsItem2);
		}

		public void TestFetchForLoadChildEditableObjects()
		{
			var goodsItem1 = Bill.GoodsItems.AddNew();
			var goodsItem2 = Bill.GoodsItems.AddNew();
			var goodsItemPks = new[] { goodsItem1.PK, goodsItem2.PK };
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			foreach (var pk in goodsItemPks)
			{
				var goodsItem = newFactory.Load<NctsDepartureCargoDesc>(pk);
				goodsItem.FetchStrategy.FetchForLoadChildEditableObjects();
			}

			newFactory.ResetDatabaseLoadCount();

			foreach (var pk in goodsItemPks)
			{
				var goodsItem = newFactory.Load<NctsDepartureCargoDesc>(pk);
				goodsItem.LoadChildEditableObjects();
			}
			AssertEquals("FetchForLoadChildEditableObjects Hints should be added for CusInvPack", 1, newFactory.GetTableHitCount(CusInvPackSchema.Constants.TableName));
		}

		NctsBill Bill => bill ?? (bill = Factory.New<NctsHeader>().Bills.AddNew());
		NctsBill bill;
	}
}
