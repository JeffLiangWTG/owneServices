using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using static Enterprise.Integration.Customs.Shared;

namespace Enterprise.Customs.FR.Business.NCTS.Testing;

sealed class CusGoodsLocationAddressLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestAuthorisationNumberList_ForDeparture_Phase5()
	{
		var consignor = Factory.NewWithValidTestData<OrgHeader>();
		var principal = Factory.NewWithValidTestData<OrgHeader>();
		var anotherOrg = Factory.NewWithValidTestData<OrgHeader>();

		CreateAuthorisationWithRule(principal, consignor.MainAddress,CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, "111", "AAA", true);
		CreateAuthorisationWithRule(principal, consignor.MainAddress, CusAuthorizationHeaderTypeList.Codes.AuthorizedWeighersOfBananas, "111", "BBB", true);
		CreateAuthorisationWithRule(principal, anotherOrg.MainAddress, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, "111", "CCC", true);
		CreateAuthorisationWithRule(anotherOrg, consignor.MainAddress, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, "111", "DDD", true);
		CreateAuthorisationWithRule(principal, consignor.MainAddress, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, "111", "EEE", false);

		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		header.MovementHeader.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture).CY_Data = customsOfficeCode;
		header.Principal.E2_OA_Address = principal.MainAddress.PK;
		header.Consignor.E2_OA_Address = consignor.MainAddress.PK;
		var location = header.CusGoodsLocation;
		location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
		location.CGL_Type = CusGoodsLocationTypeList.Codes.AuthorizedPlace;
		var address = location.Address;
		AssertContainsExactElementsInAnyOrder("LOC rule with OFC rule of DEP office code linked, and belonging to a ACR authorisation should be loaded.", new[] { "AAA" }, address.Lookups.AuthorisationNumberList.Cast<CodeDescriptionPair>().Select(x => x.Code));

		CombineAssertions("If CGL_Qualifier is not AuthorizationNumber or CGL_Type is not AuthorizedPlace, base logic should be invoked.", () =>
		{
			location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
			AssertEquals("If CGL_Qualifier is not AuthorizationNumber, base logic should be invoked hence no elements.", 0, address.Lookups.AuthorisationNumberList.Count);

			location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			location.CGL_Type = CusGoodsLocationTypeList.Codes.DesignatedLocation;
			AssertEquals("If CGL_Type is not AuthorizedPlace, base logic should be invoked hence no elements.", 0, address.Lookups.AuthorisationNumberList.Count);
		});
	}

	public void TestAuthorisationNumberList_ForDeparture_Phase4()
	{
		var consignor = Factory.NewWithValidTestData<OrgHeader>();
		var principal = Factory.NewWithValidTestData<OrgHeader>();
		var anotherOrg = Factory.NewWithValidTestData<OrgHeader>();

		CreateAuthorisationWithRule(principal, consignor.MainAddress, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, "111", "AAA", true);
		CreateAuthorisationWithRule(principal, consignor.MainAddress, CusAuthorizationHeaderTypeList.Codes.AuthorizedWeighersOfBananas, "111", "BBB", true);
		CreateAuthorisationWithRule(principal, anotherOrg.MainAddress, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, "111", "CCC", true);
		CreateAuthorisationWithRule(anotherOrg, consignor.MainAddress, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, "111", "DDD", true);
		CreateAuthorisationWithRule(principal, consignor.MainAddress, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, "111", "EEE", false);

		var mockSettings = new Mock<INctsSettings>();
		mockSettings.Setup(x => x.IsNctsEnabled).Returns(true);
		mockSettings.Setup(x => x.IsUsingPhase5(It.IsAny<string>())).Returns(false);
		using (ObjectFactory.Substitute(mockSettings.Object))
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var office = header.MovementHeader.CustomsOffices.AddNew();
			office.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
			office.CY_Data = customsOfficeCode;
			header.Principal.E2_OA_Address = principal.MainAddress.PK;
			header.Consignor.E2_OA_Address = consignor.MainAddress.PK;
			var address = header.CusGoodsLocation.Address;
			var result = address.Lookups.AuthorisationNumberList;
			AssertEquals("For NC4, it should use base logic.", 0, result.Count);
		}
	}

	public void TestAuthorisationNumberList_ForArrival_Phase5()
	{
		var trader = Factory.NewWithValidTestData<OrgHeader>();
		CreateAuthorisationWithRule(trader, trader.MainAddress, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit, "111", "AAA", true);
		CreateAuthorisationWithRule(trader, trader.MainAddress, CusAuthorizationHeaderTypeList.Codes.AuthorizedWeighersOfBananas, "111", "BBB", true);
		CreateAuthorisationWithRule(trader, trader.MainAddress, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir, "111", "CCC", true);
		CreateAuthorisationWithRule(trader, trader.MainAddress, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit, "222","DDD", true);
		CreateAuthorisationWithRule(trader, trader.MainAddress, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit, "111", "EEE", false);

		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Arrival);
		var movementHeader = header.ArrivalMovementHeader;
		movementHeader.AuthorizationCode = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
		var office = movementHeader.CustomsOffices.AddNew();
		office.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival;
		office.CY_Data = customsOfficeCode;
		header.DestinationTrader.E2_OA_Address = trader.MainAddress.PK;
		movementHeader.AuthorizationNumber = "111";
		var location = header.CusGoodsLocation;
		location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
		location.CGL_Type = CusGoodsLocationTypeList.Codes.AuthorizedPlace;
		var address = location.Address;
		CombineAssertions("LOC rule with OFC rule of DSA office code linked, and belonging to an authorisation whose type equals movementHeader.AuthorizationCode should be loaded.", () =>
		{
			AssertContainsExactElementsInAnyOrder("AAA belonging to an ACE authorisation header should be loaded.", new[] { "AAA" }, address.Lookups.AuthorisationNumberList.Cast<CodeDescriptionPair>().Select(x => x.Code));

			movementHeader.AuthorizationCode = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
			movementHeader.AuthorizationNumber = "111";
			AssertContainsExactElementsInAnyOrder("AAA belonging to an ACT authorisation header should be loaded.", new[] { "CCC" }, address.Lookups.AuthorisationNumberList.Cast<CodeDescriptionPair>().Select(x => x.Code));
		});

		CombineAssertions("If CGL_Qualifier is not AuthorizationNumber or CGL_Type is not AuthorizedPlace, base logic should be invoked.", () =>
		{
			location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
			AssertEquals("If CGL_Qualifier is not AuthorizationNumber, base logic should be invoked hence 5 elements.", 5, address.Lookups.AuthorisationNumberList.Count);

			location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			location.CGL_Type = CusGoodsLocationTypeList.Codes.DesignatedLocation;
			AssertEquals("If CGL_Type is not AuthorizedPlace, base logic should be invoked hence 5 elements.", 5, address.Lookups.AuthorisationNumberList.Count);
		});
	}

	public void TestAuthorisationNumberList_ForArrival_Phase4()
	{
		var trader = Factory.NewWithValidTestData<OrgHeader>();
		CreateAuthorisationWithRule(trader, trader.MainAddress, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit, "111", "AAA", true);
		CreateAuthorisationWithRule(trader, trader.MainAddress, CusAuthorizationHeaderTypeList.Codes.AuthorizedWeighersOfBananas, "111", "BBB", true);
		CreateAuthorisationWithRule(trader, trader.MainAddress, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir, "111", "CCC", true);
		CreateAuthorisationWithRule(trader, trader.MainAddress, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit, "222", "DDD", true);
		CreateAuthorisationWithRule(trader, trader.MainAddress, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit, "111", "EEE", false);

		var mockSettings = new Mock<INctsSettings>();
		mockSettings.Setup(x => x.IsNctsEnabled).Returns(true);
		mockSettings.Setup(x => x.IsUsingPhase5(It.IsAny<string>())).Returns(false);
		using (ObjectFactory.Substitute(mockSettings.Object))
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var office = header.ArrivalMovementHeader.CustomsOffices.AddNew();
			office.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival;
			office.CY_Data = customsOfficeCode;
			header.Consignor.E2_OA_Address = trader.MainAddress.PK;
			var address2 = header.CusGoodsLocation.Address;
			var result = address2.Lookups.AuthorisationNumberList;
			AssertEquals("For NC4, it should use base logic.", 0, result.Count);
		}
	}

	void CreateAuthorisationWithRule(OrgHeader holder, OrgAddress permitAddress, string authType, string authNumber,string ruleValue, bool hasCustomsOfficeLinkedRule)
	{
		var authHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
		authHeader.CPH_Type = authType;
		authHeader.CPH_Number = authNumber;
		authHeader.CPH_OH_PermitHolder = holder.PK;
		authHeader.CPH_OA_AppliesTo = permitAddress.PK;
		var rule = authHeader.CusAuthorisationRules.AddNew();
		rule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
		rule.CPR_ValueFrom = ruleValue;
		if (hasCustomsOfficeLinkedRule)
		{
			var linkedRule = rule.LinkedCusAuthorisationRules.AddNew();
			linkedRule.CPR_RuleCode = Customs.Business.LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice;
			linkedRule.CPR_ValueFrom = customsOfficeCode;
		}
	}
	const string customsOfficeCode = "FR003333";
}
