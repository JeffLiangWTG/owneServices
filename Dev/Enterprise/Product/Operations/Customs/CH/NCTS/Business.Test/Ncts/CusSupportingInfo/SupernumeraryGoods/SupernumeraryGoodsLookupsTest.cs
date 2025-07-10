using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class SupernumeraryGoodsLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestTarifList()
	{
		var refDataHelper = new RefDataTestHelper(Factory);
		refDataHelper.CreateTariffs(Universal.Constants.TariffTypes.HarmonizedSystem, RefDataGrouping.Codes.WorldCustomsOrganisationWCO).CreateTariff("000001").CreateTariff("000002");
		refDataHelper.CreateTariffs(Universal.Constants.TariffTypes.Commodity, RefDataGrouping.Codes.WorldCustomsOrganisationWCO).CreateTariff("000003");
		refDataHelper.CreateTariffs(Universal.Constants.TariffTypes.HarmonizedSystem, RefDataGrouping.Codes.EuropeanUnionEUN).CreateTariff("000004");
		Factory.Save();

		Lookups.TariffList.Load();
		AssertContainsExactElementsInAnyOrder(new[] { "000001", "000002" }, Lookups.TariffList.Select(t => t.ZZ1_TariffCode));
	}

	public void TestPackTypeList()
	{
		var refDataHelper = new RefDataTestHelper(Factory);
		refDataHelper.CreateCodeList(RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, RefDataGrouping.Codes.UnitedNationsRecommendations)
			.CreateCode("P01")
			.CreateCode("P02");
		Factory.Save();

		AssertEquals("P01, P02", Lookups.PackTypeList.CodesAsString);
	}

	SupernumeraryGoods SupernumeraryGoods => supernumeraryGoods ?? (supernumeraryGoods = CreateSupernumeraryGoods());
	SupernumeraryGoods supernumeraryGoods;

	SupernumeraryGoods CreateSupernumeraryGoods()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		return nctsHeader.ArrivalMovementHeader.SupernumeraryGoods.AddNew();
	}

	SupernumeraryGoodsLookups Lookups => SupernumeraryGoods.Lookups;
}
