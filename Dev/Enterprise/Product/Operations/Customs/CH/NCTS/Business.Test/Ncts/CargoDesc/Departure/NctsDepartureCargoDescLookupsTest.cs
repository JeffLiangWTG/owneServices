using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using static Enterprise.Core.Constants.Customs.Universal;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

class NctsDepartureCargoDescLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestCusCodeList() => CombineAssertions(() =>
	{
		const string EUN = RefDataGrouping.Codes.EuropeanUnionEUN;
		const string CH = Core.Constants.CountryCodes.Switzerland;
		const string BE = Core.Constants.CountryCodes.Belgium;

		var refDataTestHelper = new RefDataTestHelper(Factory);
		refDataTestHelper.CreateCodeList(RefCusCodeListType.Code.Code_ECICS, EUN).CreateCode("EU1");
		refDataTestHelper.CreateCodeList(RefCusCodeListType.Code.Code_ECICS, EUN).CreateCode("EU2").WithAttribute(RefCusCodeListTypes.Codes.CombinedNomenclatureCode, "123456");
		refDataTestHelper.CreateCodeList(RefCusCodeListType.Code.Code_ECICS, CH).CreateCode("CH1");
		refDataTestHelper.CreateCodeList(RefCusCodeListType.Code.Code_ECICS, CH).CreateCode("CH2").WithAttribute(RefCusCodeListTypes.Codes.CombinedNomenclatureCode, "123456");
		refDataTestHelper.CreateCodeList(RefCusCodeListType.Code.Code_ECICS, BE).CreateCode("BE1");
		refDataTestHelper.CreateCodeList(RefCusCodeListType.Code.Code_ECICS, BE).CreateCode("BE2").WithAttribute(RefCusCodeListTypes.Codes.CombinedNomenclatureCode, "123456");
		Factory.Save();

		CargoDesc.Lookups.CusCodeList.Load();
		AssertContainsExactElementsInAnyOrder("BY_HarmonisedTariff empty", new[] { "EU1", "EU2" }, CargoDesc.Lookups.CusCodeList.Select(c => c.ZZD_Code));
		AssertSame("BY_HarmonisedTariff empty cached", CargoDesc.Lookups.CusCodeList, CargoDesc.Lookups.CusCodeList);

		CargoDesc.BY_HarmonisedTariff = "1234567";
		CargoDesc.Lookups.CusCodeList.Load();
		AssertEquals("BY_HarmonisedTariff not empty", "123456", CargoDesc.Lookups.CusCodeList.FilterBusinessObjectDefaults["Attribute Value:Property"].Value);
		AssertSame("BY_HarmonisedTariff not empty cached", CargoDesc.Lookups.CusCodeList, CargoDesc.Lookups.CusCodeList);
	});

	NctsDepartureCargoDesc CreateNctsDepartureCargoDesc()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		var bill = nctsHeader.Bills.AddNew();
		departureCargoDesc = bill.GoodsItems.AddNew();
		return departureCargoDesc;
	}

	NctsDepartureCargoDesc CargoDesc => departureCargoDesc ?? (departureCargoDesc = CreateNctsDepartureCargoDesc());
	NctsDepartureCargoDesc departureCargoDesc;
}
