using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class GoodsItemDifferencesDetailsLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestCodeListDEC() => AssertCodeList(NctsUnloadedStateList.Codes.DEC, "1, 2, 3, 4");
	public void TestCodeListDIF() => AssertCodeList(NctsUnloadedStateList.Codes.DIF, "1, 2, 3, 4");
	public void TestCodeListMIS() => AssertCodeList(NctsUnloadedStateList.Codes.MIS, "1, 2, 3, 4");
	public void TestCodeListNEW() => AssertCodeList(NctsUnloadedStateList.Codes.NEW, "2, 4");

	void AssertCodeList(string unloadedState, string expectedCodeList)
	{
		var goodsItem = lookups.Parent.Parent;
		goodsItem.BY_UnloadedState = unloadedState;
		var lookup = lookups.CY_CodeList;
		AssertEquals(goodsItem.BY_UnloadedState, expectedCodeList, lookup.CodesAsString);
	}

	protected override void SetUp()
	{
		base.SetUp();

		var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		var arrivalGoodsItem = nctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew();
		lookups = arrivalGoodsItem.GoodsItemDifferencesDetail.Lookups;
	}
	GoodsItemDifferencesDetailsLookups lookups;
}
