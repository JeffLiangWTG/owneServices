using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

sealed class NctsDepartureMovementHeaderPhase5RuleB1893_1ValidationTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When departureMovementHeader is null",
			() => new NctsDepartureMovementHeaderPhase5RuleB1893_1Validation(null));
	}

	public void TestCheckBM_PortOfPresentationCode_MustBeEmpty_WhenSecurityIsNON()
	{
		const string messageError = "[B1893-1] Place of Loading must be empty if Security field = NON.";

		using var temporarilySetFunctionality = ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true);
		using var ruleTestContext = new MovementHeaderValidationDeciderTestContext<INctsDepartureMovementHeaderPhase5ValidationDecider>(Factory);

		ruleTestContext.EnableRule(x => x.IsRuleB1893_1Active);
		var departureMovement = CreateDepartureMovement();
		var portOfPresentationCodeInfo = departureMovement.BM_PortOfPresentationCodeInfo;
		CombineAssertions("RuleB1893_1: Active", () =>
		{
			departureMovement.BM_TypeOfSecurity = "NON";
			departureMovement.BM_PortOfPresentationCode = "";
			AssertNoMessageError("When BM_TypeOfSecurity = 'NON' and BM_PortOfPresentationCode is empty", portOfPresentationCodeInfo, messageError);

			departureMovement.BM_TypeOfSecurity = "NON";
			departureMovement.BM_PortOfPresentationCode = "IT";
			AssertHasMessageError("When BM_TypeOfSecurity = 'NON' and BM_PortOfPresentationCode is filled", portOfPresentationCodeInfo, messageError);

			departureMovement.BM_TypeOfSecurity = "ENT";
			departureMovement.BM_PortOfPresentationCode = "IT";
			AssertNoMessageError("When BM_TypeOfSecurity is not 'NON' and BM_PortOfPresentationCode is filled", portOfPresentationCodeInfo, messageError);
		});

		ruleTestContext.DisableRule(x => x.IsRuleB1893_1Active);
		CombineAssertions("RuleB1893_1: Disable", () =>
		{
			departureMovement.BM_TypeOfSecurity = "NON";
			departureMovement.BM_PortOfPresentationCode = "IT";
			AssertNoMessageError("When BM_TypeOfSecurity = 'NON' and BM_PortOfPresentationCode is filled", portOfPresentationCodeInfo, messageError);
		});
	}

	public void TestCheckBM_PlaceOfLoading_MustBeEmpty_WhenSecurityIsNON()
	{
		const string messageError = "[B1893-1] Place of Loading must be empty if Security field = NON.";

		using var temporarilySetFunctionality = ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true);
		using var ruleTestContext = new MovementHeaderValidationDeciderTestContext<INctsDepartureMovementHeaderPhase5ValidationDecider>(Factory);

		ruleTestContext.EnableRule(x => x.IsRuleB1893_1Active);
		var departureMovement = CreateDepartureMovement();
		var placeOfLoadingInfo = departureMovement.BM_PlaceOfLoadingInfo;
		CombineAssertions("RuleB1893_1: Active", () =>
		{
			departureMovement.BM_TypeOfSecurity = "NON";
			departureMovement.BM_PlaceOfLoading = "";
			AssertNoMessageError("When BM_TypeOfSecurity = 'NON' and BM_PlaceOfLoading is empty", placeOfLoadingInfo, messageError);

			departureMovement.BM_TypeOfSecurity = "NON";
			departureMovement.BM_PlaceOfLoading = "IT";
			AssertHasMessageError("When BM_TypeOfSecurity = 'NON' and BM_PlaceOfLoading is filled", placeOfLoadingInfo, messageError);

			departureMovement.BM_TypeOfSecurity = "ENT";
			departureMovement.BM_PlaceOfLoading = "IT";
			AssertNoMessageError("When BM_TypeOfSecurity is not 'NON' and BM_PlaceOfLoading is filled", placeOfLoadingInfo, messageError);
		});

		ruleTestContext.DisableRule(x => x.IsRuleB1893_1Active);
		CombineAssertions("RuleB1893_1: Disable", () =>
		{
			departureMovement.BM_TypeOfSecurity = "NON";
			departureMovement.BM_PlaceOfLoading = "IT";
			AssertNoMessageError("When BM_TypeOfSecurity = 'NON' and BM_PlaceOfLoading is filled", placeOfLoadingInfo, messageError);
		});
	}

	NctsDepartureMovementHeader CreateDepartureMovement()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		return nctsHeader.MovementHeader;
	}
}
