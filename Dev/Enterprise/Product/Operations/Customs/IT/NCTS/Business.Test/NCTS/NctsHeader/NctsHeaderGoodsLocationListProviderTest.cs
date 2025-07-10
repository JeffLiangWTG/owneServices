using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsHeaderGoodsLocationListProviderTest : GoodsLocationListProviderTest<NctsGoodsLocationList>
{
	public override void TestLocations()
	{
		nctsHeader.Authorization = "999999";
		AssertEquals("Valid authorisation number, but NO Customs Office of Departure", 2, goodsLocationProvider.Locations.Count);

		var departureOffice = nctsHeader.MovementHeader.CustomsOffices.Cast<EU.NCTS.Business.NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == EU.Business.EuOfficeCodesTypes.Codes.OfficeOfDeparture);
		departureOffice.CY_Data = "IT137100";

		AssertEquals("Values", "MYLOC1", goodsLocationProvider.Locations.CodesAsString);
	}

	public override void TestLocationsWithEmptyAuthorizationNumber()
	{
		nctsHeader.Authorization = ZString.Empty;
		AssertType<NctsGoodsLocationList>("When Authorization is empty, GoodsLocationList is returned", goodsLocationProvider.Locations);
		CombineAssertions(() =>
		{
			AssertEquals("Values", "D, F, FC", goodsLocationProvider.Locations.CodesAsString);
		});
	}

	public override void TestLocationsWithWrongAuthorizationNumber()
	{
		nctsHeader.Authorization = "123456";
		AssertEquals("Invalid authorisation number selected", 0, goodsLocationProvider.Locations.Count);
	}

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		goodsLocationProvider = new NctsHeaderGoodsLocationListProvider(nctsHeader, Factory);

		var orgHeader = Factory.NewWithValidTestData<MasterFiles.Business.OrgHeader>();
		var requirement = nctsHeader.DocAddresses.FindOrCreateWithRequirement(nctsHeader.ConsignorJobDocAddressRequirement);
		requirement.E2_OA_Address = orgHeader.MainAddress.PK;

		var authorisationHeader = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, permitHolder: nctsHeader.Consignor.OrganisationPK, "999999", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));

		var authorisationRule1 = CusAuthorisationRuleTestHelper.AddAuthorisationRule(authorisationHeader, ITCusAuthorisationRuleTypeList.Codes.Location, "MYLOC1", "MYLOC1 RULE DESCRIPTION");
		var linkedRule1 = authorisationRule1.LinkedCusAuthorisationRules.AddNew();
		linkedRule1.CPR_RuleCode = Customs.Business.LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice;
		linkedRule1.CPR_ValueFrom = "IT137100";

		var authorisationRule2 = CusAuthorisationRuleTestHelper.AddAuthorisationRule(authorisationHeader, ITCusAuthorisationRuleTypeList.Codes.Location, "MYLOC2", "MYLOC2 RULE DESCRIPTION");
		var linkedRule2 = authorisationRule2.LinkedCusAuthorisationRules.AddNew();
		linkedRule2.CPR_RuleCode = Customs.Business.LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice;
		linkedRule2.CPR_ValueFrom = "IT137101";
	}
	NctsHeader nctsHeader;
	NctsHeaderGoodsLocationListProvider goodsLocationProvider;

	protected override GoodsLocationListProvider<NctsGoodsLocationList> GetNewGoodsLocationListProvider(IAutHeaderWithCusOfficeProvider authorisationWithCustomsOfficeProvider, BusinessObjectFactory factory) => new NctsHeaderGoodsLocationListProvider(authorisationWithCustomsOfficeProvider, factory);
}
