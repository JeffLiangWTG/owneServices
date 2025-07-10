using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsDepartureMovementHeaderPhase4LookupsTest : BusinessObjectLookupsTestCase
{
	public void TestPaymentPartyList()
	{
		CombineAssertions(() =>
		{
			var paymentPartyList = departureMovement.ITLookups.PaymentPartyList;
			AssertType<NctsPaymentPartyList>("Type", paymentPartyList);
			AssertSame("Cached", paymentPartyList, departureMovement.ITLookups.PaymentPartyList);
			AssertEquals("CodesAsString", "A, B", paymentPartyList.CodesAsString);
		});
	}

	public void TestDefermentApprovalNumberListWhenPaymentPartyIsDeclarantAndRepTypeNotSelf()
	{
		var declarant = CreateOrgHeaderWithCustomsCodes();
		departureMovement.PaymentParty = NctsPaymentPartyList.Codes.DeclarantsAccount;
		header.RepresentationType = ZString.Empty;
		AssertEmptyDefermentApprovalNumberList("When PaymentParty = 'A' && RepresentationType != 'SEL' && Declarant == NULL");

		header.DeclarantAddressPK = declarant.MainAddress.PK;
		AssertExpectedDefermentApprovalNumberList("When PaymentParty = 'A' && RepresentationType != 'SEL' && Declarant NOT NULL");
	}

	public void TestDefermentApprovalNumberListWhenPaymentPartyIsDeclarantAndRepTypeIsSelf()
	{
		var consignor = CreateOrgHeaderWithCustomsCodes();
		departureMovement.PaymentParty = NctsPaymentPartyList.Codes.DeclarantsAccount;
		header.RepresentationType = RepresentationTypeList.Codes._1Self;
		AssertEmptyDefermentApprovalNumberList("When PaymentParty = 'A' && RepresentationType == 'SEL' && Consignor == NULL");

		var requirement = header.DocAddresses.FindOrCreateWithRequirement(header.ConsignorJobDocAddressRequirement);
		requirement.E2_OA_Address = consignor.MainAddress.PK;
		AssertExpectedDefermentApprovalNumberList("When PaymentParty = 'A' && RepresentationType == 'SEL' && Consignor NOT NULL");
	}

	public void TestDefermentApprovalNumberListWhenPaymentPartyIsConsegnee()
	{
		var consignee = CreateOrgHeaderWithCustomsCodes();
		departureMovement.PaymentParty = NctsPaymentPartyList.Codes.ConsigneesAccount;
		AssertEmptyDefermentApprovalNumberList("When PaymentParty = 'B' && Consignee == NULL");

		var requirement = header.DocAddresses.FindOrCreateWithRequirement(header.ConsigneeJobDocAddressRequirement);
		requirement.E2_OA_Address = consignee.MainAddress.PK;
		AssertExpectedDefermentApprovalNumberList("When PaymentParty = 'B' && Consignee NOT NULL");
	}

	public void TestDefermentApprovalNumberListWhenPaymentPartyAndRepTypeAreInvalid()
	{
		departureMovement.PaymentParty = ZString.Empty;
		header.RepresentationType = ZString.Empty;
		AssertEmptyDefermentApprovalNumberList("When both PaymentParty and RepresentationType are invalid");
	}

	public void TestDefermentApprovalNumberListWhenNoNctsHeaderFound()
	{
		var departureMovement = Factory.New<NctsDepartureMovementHeader>();
		departureMovement.PaymentParty = NctsPaymentPartyList.Codes.DeclarantsAccount;
		AssertEquals("When PaymentParty is 'A', DefermentApprovalNumberList Count", 0, departureMovement.ITLookups.DefermentApprovalNumberList.Count);

		departureMovement.PaymentParty = NctsPaymentPartyList.Codes.ConsigneesAccount;
		AssertEquals("When PaymentParty is 'B', DefermentApprovalNumberList Count", 0, departureMovement.ITLookups.DefermentApprovalNumberList.Count);
	}

	public void TestBondedWarehouseCollection()
	{
		AssertType<BondedWarehouseCollection>(departureMovement.ITLookups.BondedWarehouseCollection);
	}

	public void TestCustomsChannelCodeList()
	{
		CombineAssertions(() =>
		{
			var customsChannelCodeList = departureMovement.ITLookups.CustomsChannelCodeList;
			AssertType<CustomsChannelCodeList>("CustomsChannelCodeList type", customsChannelCodeList);
			AssertSame("CustomsChannelCodeList should be cached", customsChannelCodeList, departureMovement.ITLookups.CustomsChannelCodeList);
		});
	}

	public void TestNctsTransitStatusList()
	{
		CombineAssertions(() =>
		{
			var nctsTransitStatusList = departureMovement.Lookups.NctsTransitStatusList;
			AssertType<NctsTransitStatusList>("NctsTransitStatusList type", nctsTransitStatusList);
			AssertSame("NctsTransitStatusList should be cached", nctsTransitStatusList, departureMovement.Lookups.NctsTransitStatusList);
		});
	}

	public void TestNctsParticipantTypeList()
	{
		var nctsParticipantTypeList = departureMovement.ITLookups.NctsParticipantTypeList;
		CombineAssertions(() =>
		{
			AssertType<NctsParticipantTypeList>("Type", nctsParticipantTypeList);
			AssertSame("Cache", nctsParticipantTypeList, nctsParticipantTypeList);
		});
	}

	public void TestLocationOfGoodsCodeList()
	{
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		var requirement = header.DocAddresses.FindOrCreateWithRequirement(header.ConsignorJobDocAddressRequirement);
		requirement.E2_OA_Address = orgHeader.MainAddress.PK;

		var authorisationHeader = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, permitHolder: header.Consignor.OrganisationPK, "999999", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));

		var authorisationRule1 = CusAuthorisationRuleTestHelper.AddAuthorisationRule(authorisationHeader, ITCusAuthorisationRuleTypeList.Codes.Location, "MYLOC1", "MYLOC1 RULE DESCRIPTION");
		var linkedRule1 = authorisationRule1.LinkedCusAuthorisationRules.AddNew();
		linkedRule1.CPR_RuleCode = Customs.Business.LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice;
		linkedRule1.CPR_ValueFrom = "IT137100";

		var authorisationRule2 = CusAuthorisationRuleTestHelper.AddAuthorisationRule(authorisationHeader, ITCusAuthorisationRuleTypeList.Codes.Location, "MYLOC2", "MYLOC2 RULE DESCRIPTION");
		var linkedRule2 = authorisationRule2.LinkedCusAuthorisationRules.AddNew();
		linkedRule2.CPR_RuleCode = Customs.Business.LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice;
		linkedRule2.CPR_ValueFrom = "IT137101";

		CombineAssertions("When Authorization is empty", () =>
		{
			header.Authorization = ZString.Empty;
			AssertType<NctsGoodsLocationList>("GoodsLocationList is returned", departureMovement.Lookups.LocationOfGoodsCodeList);
			AssertEquals("GoodsLocationList Values", "D, F, FC", departureMovement.Lookups.LocationOfGoodsCodeList.CodesAsString);
		});

		header.Authorization = "123456";
		AssertEquals("Invalid authorisation number selected", 0, departureMovement.Lookups.LocationOfGoodsCodeList.Count);

		header.Authorization = "999999";
		AssertEquals("Valid authorisation number, but NO Customs Office of Departure", 2, departureMovement.Lookups.LocationOfGoodsCodeList.Count);

		header.CustomsOffices.AddNew("DEP", "IT137100");
		NctsLookupsTestUtility.AssertLookups("Valid locations are filtered by Customs Office equal to DEP", departureMovement.Lookups.LocationOfGoodsCodeList, ("MYLOC1", "MYLOC1 RULE DESCRIPTION"));
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.NewDepartureNctsHeader();
		departureMovement = header.MovementHeader;
	}

	NctsHeader header;
	NctsDepartureMovementHeader departureMovement;

	#region Implementation

	void AssertExpectedDefermentApprovalNumberList(ZString assertionMessage)
	{
		AssertEquals(assertionMessage, "1, 2", departureMovement.ITLookups.DefermentApprovalNumberList.CodesAsString);
		AssertSame("Cached", departureMovement.ITLookups.DefermentApprovalNumberList, departureMovement.ITLookups.DefermentApprovalNumberList);
	}

	void AssertEmptyDefermentApprovalNumberList(ZString assertionMessage)
	{
		AssertEquals(assertionMessage, 0, departureMovement.ITLookups.DefermentApprovalNumberList.Count);
	}

	OrgHeader CreateOrgHeaderWithCustomsCodes()
	{
		var trader = Factory.NewWithValidTestData<OrgHeader>();
		trader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, "1", Core.Constants.CountryCodes.Italy);
		trader.CustomsCodes.AddNew(ItalyOrgCusCodeInfo.OrgCusCodes.DefermentApprovaNumberForTrieste, "2", Core.Constants.CountryCodes.Italy);
		trader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, "3", Core.Constants.CountryCodes.Germany);
		return trader;
	}

	#endregion
}
