using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsArrivalCargoDescLookups))]
sealed class NctsArrivalCargoDescLookupsTest : BusinessObjectLookupsTestCase
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
			goodsItem = nctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew();
		}

	NctsArrivalCargoDesc goodsItem;
	NctsArrivalCargoDescLookups Lookups => (NctsArrivalCargoDescLookups)goodsItem.Lookups;
}
