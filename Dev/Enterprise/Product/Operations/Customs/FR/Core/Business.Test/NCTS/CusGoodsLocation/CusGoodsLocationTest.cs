using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.NCTS.Testing;

[TestedType(typeof(CusGoodsLocation))]
sealed class CusGoodsLocationTest : EnterpriseBusinessObjectTestCase
{
	public void TestAdressAuthorizationNumber()
	{
		var trader = Factory.NewWithValidTestData<OrgHeader>();
		CreateAuthorisationWithRule(trader, trader.MainAddress, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit, "111", "AAA", true);
		CreateAuthorisationWithRule(trader, trader.MainAddress, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir, "111", "BBB", true);
		CreateAuthorisationWithRule(trader, trader.MainAddress, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir, "111", "CCC", true);

		var header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		header.SetMovementType(NctsMovementType.Codes.Arrival);
		var movementHeader = header.ArrivalMovementHeader;
		movementHeader.AuthorizationCode = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
		movementHeader.AuthorizationNumber = "111";
		var office = movementHeader.CustomsOffices.AddNew();
		office.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival;
		office.CY_Data = "FR003333";
		header.DestinationTrader.E2_OA_Address = trader.MainAddress.PK;
		var location = header.CusGoodsLocation;
		location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
		location.CGL_Type = CusGoodsLocationTypeList.Codes.AuthorizedPlace;
		var address = location.Address;
		AssertEquals("Not apply to NCTS Phase4.", ZString.Empty,  address.AuthorisationNumber);

		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		location.CGL_Type = CusGoodsLocationTypeList.Codes.DesignatedLocation;
		location.CGL_Type = CusGoodsLocationTypeList.Codes.AuthorizedPlace;
		AssertEquals("When CGL_Qualifier is Y and CGL_Type is B, only one matching CusAuthorisationRule found，set AuthorisationNumber to CPR_ValueFrom by default.", "AAA", address.AuthorisationNumber);

		address.AuthorisationNumber = ZString.Empty;
		location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
		AssertEquals("CGL_Qualifier is not Y", ZString.Empty, address.AuthorisationNumber);

		location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
		address.AuthorisationNumber = ZString.Empty;
		location.CGL_Type = CusGoodsLocationTypeList.Codes.DesignatedLocation;
		AssertEquals("CGL_Type is not B", ZString.Empty, address.AuthorisationNumber);

		movementHeader.AuthorizationCode = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
		location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
		location.CGL_Type = CusGoodsLocationTypeList.Codes.AuthorizedPlace;
		AssertEquals("When CGL_Qualifier is Y and CGL_Type is B, found multiple CusAuthorisationRules，set AuthorisationNumber to empty by default.", ZString.Empty, address.AuthorisationNumber);
	}

	void CreateAuthorisationWithRule(OrgHeader holder, OrgAddress permitAddress, string authType, string authNumber, string ruleValue, bool hasCustomsOfficeLinkedRule)
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
			linkedRule.CPR_ValueFrom = "FR003333";
		}
	}

	public void TestIsAuthorizedPlaceWithAuthorization()
	{
		var testCase = new MultiFactorTestCase<CusGoodsLocation>(() => GetCusGoodsLocationForTesting(Factory));
		var jobShouldNotBePhase4 = new Preq<CusGoodsLocation>(location => ((NctsDepartureMovementHeader)location.Parent).Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5,
			failingAction: location => ((NctsDepartureMovementHeader)location.Parent).Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4);
		var qualifierShouldBeAuthorizationNumber = new FieldPreq<CusGoodsLocation>(location => location.CGL_QualifierInfo).Values(CusGoodsLocationQualifierList.Codes.AuthorizationNumber).NotValues(ZString.Empty).NotValues(CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier);
		var typeShouldBeAuthorizedPlace = new FieldPreq<CusGoodsLocation>(location => location.CGL_TypeInfo).Values(CusGoodsLocationTypeList.Codes.AuthorizedPlace).NotValues(ZString.Empty).NotValues(CusGoodsLocationTypeList.Codes.ApprovedPlace);
		testCase.SetUpCondition(jobShouldNotBePhase4 && qualifierShouldBeAuthorizationNumber && typeShouldBeAuthorizedPlace);
		testCase.RunAssertion(location => AssertEquals("IsAuthorizedPlaceWithAuthorization should be true if it's parent is not a Phase4 job and CGL_Qualifier is Y and CGL_Type is B", true, location.IsAuthorizedPlaceWithAuthorization),
			assertFailure: location => AssertEquals("IsAuthorizedPlaceWithAuthorization should be false if it's parent is a Phase4 job or CGL_Qualifier is not Y or CGL_Type is not B.", false, location.IsAuthorizedPlaceWithAuthorization));
	}

	public void TestAddressType()
	{
		var location = GetCusGoodsLocationForTesting(Factory);
		var address = location.Address;
		AssertType<CusGoodsLocationAddress>($"{typeof(CusGoodsLocation).FullName}.Address should be of type {typeof(CusGoodsLocationAddress).FullName}.", location.Address);
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetCusGoodsLocationForTesting(factory);

	internal static CusGoodsLocation GetCusGoodsLocationForTesting(BusinessObjectFactory factory)
	{
		var location = factory.New<CusGoodsLocation>();
		location.Parent = factory.NewWithValidTestData<NctsDepartureMovementHeader>();
		location.CGL_LocationUse = CusGoodsLocationUseList.Codes.Departure;
		return location;
	}
}
