using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsUnloadedCargoDescLookups))]
class NctsUnloadedCargoDescLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestCusCodeList()
	{
		var testHelper = new RefDataTestHelper(Factory);
		testHelper.CreateCodeList(RefCusCodeListType.Code.Code_ECICS, EconomicGroupList.Codes.EuropeanUnion).CreateCode("E1");
		testHelper.CreateCodeList(RefCusCodeListType.Code.Code_ECICS).CreateCode("C1");
		Factory.Save();

		Lookups.CusCodeList.Load();
		AssertContainsExactElementsInAnyOrder(new[] { "E1" }, Lookups.CusCodeList.Select(x => x.ZZD_Code));
	}

		protected override void SetUp()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(Common.EU.NctsMoveHeaderType.Codes.Arrival);
			var goodsItem = nctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew();
			goodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			unloadedGoodsItem = goodsItem.UnloadedGoodsItem;
		}

	NctsUnloadedCargoDesc unloadedGoodsItem;
	NctsUnloadedCargoDescLookups Lookups => (NctsUnloadedCargoDescLookups)unloadedGoodsItem.Lookups;
}
