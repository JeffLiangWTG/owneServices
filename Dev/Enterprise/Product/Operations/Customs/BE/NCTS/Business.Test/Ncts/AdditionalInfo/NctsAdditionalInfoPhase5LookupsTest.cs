using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.BE.NCTS.Business.Testing;

sealed class NctsAdditionalInfoPhase5LookupsTest : BusinessObjectLookupsTestCase
{
	public void TestSubTypeList()
	{
		var list = goodsItemLookups.SubTypeList;
		CombineAssertions(() =>
		{
			AssertSubTypeListInTransitionPeriod("INF, REF", false);
			AssertSubTypeListInTransitionPeriod("INF, REF, TRA", true);
		});
	}

	void AssertSubTypeListInTransitionPeriod(string expectedList, bool isInTransitionPeriod)
	{
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, isInTransitionPeriod))
		{
			var list = goodsItemLookups.SubTypeList;
			AssertEquals("List", expectedList, list.CodesAsString);
			AssertSame("Cached", list, goodsItemLookups.SubTypeList);
		}
	}

	protected override void SetUp()
	{
		base.SetUp();

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		var bill = nctsHeader.Bills.AddNew();
		var goodsItem = bill.GoodsItems.AddNew();
		goodsItemAdditionalInfo = goodsItem.AdditionalInfos.AddNew();
		goodsItemLookups = new NctsAdditionalInfoPhase5Lookups(goodsItemAdditionalInfo);
	}

	NctsAdditionalInfo goodsItemAdditionalInfo;
	NctsAdditionalInfoPhase5Lookups goodsItemLookups;
}
