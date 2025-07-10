using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

public class NctsLookupsHelperTest : TestCaseWithFactory
{
	public void TestTransportTypeOfIdList() => CombineAssertions(() =>
	{
		AssertEquals("Codes", "10, 21, 30, 40, 41, 80, 81, 99", NctsLookupsHelper.TransportTypeOfIdList(Factory).CodesAsString);
		AssertSame("Cached", NctsLookupsHelper.TransportTypeOfIdList(Factory), NctsLookupsHelper.TransportTypeOfIdList(Factory));
	});

	public void TestNationalityList() => CombineAssertions(() =>
	{
		var refDataTestHelper = new RefDataTestHelper(Factory);
		refDataTestHelper.CreateCodeList(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NCNAT, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN).CreateCode("N1");
		refDataTestHelper.CreateCodeList(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NCNAT, Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations).CreateCode("N2");
		Factory.Save();

		NctsLookupsHelper.NationalityList(Factory).Load();
		AssertContainsExactElementsInAnyOrder(new[] { "N1" }, NctsLookupsHelper.NationalityList(Factory).Select(c => c.ZZD_Code));
		AssertSame("Cached", NctsLookupsHelper.NationalityList(Factory), NctsLookupsHelper.NationalityList(Factory));
	});

	public void TestLoadTariffForTransit() => CombineAssertions(() =>
	{
		new RefDataTestHelper(Factory).CreateTariffsForTransit();

		AssertNull(NctsLookupsHelper.LoadTariffForTransit(Factory, "999999", ZDateTime.Today));
		AssertEquals("710121", NctsLookupsHelper.LoadTariffForTransit(Factory, "710121", ZDateTime.Today).ZZ1_TariffCode);
		AssertNull(NctsLookupsHelper.LoadTariffForTransit(Factory, "99999999", ZDateTime.Today));
		AssertEquals("84061000", NctsLookupsHelper.LoadTariffForTransit(Factory, "84061000", ZDateTime.Today).ZZ1_TariffCode);
		AssertEquals("04069099001", NctsLookupsHelper.LoadTariffForTransit(Factory, "04069099", ZDateTime.Today).ZZ1_TariffCode);
		AssertNull(NctsLookupsHelper.LoadTariffForTransit(Factory, "99999999999", ZDateTime.Today));
		AssertEquals("04069099001", NctsLookupsHelper.LoadTariffForTransit(Factory, "04069099001", ZDateTime.Today).ZZ1_TariffCode);
	});
}
