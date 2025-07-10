using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsEuOfficeCodeDepartureValidation))]
sealed class NctsEuOfficeCodeDepartureValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckEstimatedNumberOfDays()
	{
		const string errorMessage = "Estimated Days must be empty or a number between 0 and 99.";

		var customsOffice = Factory.New<NctsEuOfficeCode>();

		customsOffice.EstimatedNumberOfDays = ZString.Empty;
		AssertNoError(customsOffice.EstimatedNumberOfDaysInfo, errorMessage);

		customsOffice.EstimatedNumberOfDays = "0";
		AssertNoError(customsOffice.EstimatedNumberOfDaysInfo, errorMessage);

		customsOffice.EstimatedNumberOfDays = "99";
		AssertNoError(customsOffice.EstimatedNumberOfDaysInfo, errorMessage);

		customsOffice.EstimatedNumberOfDays = "X";
		AssertHasError(customsOffice.EstimatedNumberOfDaysInfo, errorMessage);

		customsOffice.EstimatedNumberOfDays = "-1";
		AssertHasError(customsOffice.EstimatedNumberOfDaysInfo, errorMessage);
	}

	public void TestEstimatedNumberOfDaysNS30113WithDifferentTypeOfSecurityValues()
	{
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
		{
			CreateSafetyAndSecurityTradeGroupCountries();
			customsOffice.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit;
			customsOffice.CY_Data = "IT123456";
			customsOffice.EstimatedNumberOfDays = string.Empty;

			CombineAssertions(() =>
			{
				nctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
				customsOffice.Validation.ValidateAll();
				AssertHasMessageError("BM_TypeOfSecurity is ENT, EstimatedNumberOfDays should be mandatory", customsOffice.EstimatedNumberOfDaysInfo, PassarValidationMessages.MessageNS30113);

				nctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
				customsOffice.Validation.ValidateAll();
				AssertHasMessageError("BM_TypeOfSecurity is BTH, EstimatedNumberOfDays should be mandatory", customsOffice.EstimatedNumberOfDaysInfo, PassarValidationMessages.MessageNS30113);

				customsOffice.EstimatedNumberOfDays = "0";
				AssertNoMessageError("BM_TypeOfSecurity is BTH but the EstimatedNumberOfDays is higher than 0, EstimatedNumberOfDays should not have an error message", customsOffice.EstimatedNumberOfDaysInfo, PassarValidationMessages.MessageNS30113);

				nctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
				customsOffice.EstimatedNumberOfDays = string.Empty;
				AssertNoMessageError("BM_TypeOfSecurity is not ENT or BTH, EstimatedNumberOfDays should not be mandatory", customsOffice.EstimatedNumberOfDaysInfo, PassarValidationMessages.MessageNS30113);
			});
		}
	}

	public void TestEstimatedNumberOfDaysNS30113WithDifferentCY_CodeValues()
	{
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
		{
			CreateSafetyAndSecurityTradeGroupCountries();
			nctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
			customsOffice.CY_Data = "IT123456";
			customsOffice.EstimatedNumberOfDays = string.Empty;

			CombineAssertions(() =>
			{
				customsOffice.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit;
				customsOffice.Validation.ValidateAll();
				AssertHasMessageError("CY_Code is TRA, EstimatedNumberOfDays should be mandatory", customsOffice.EstimatedNumberOfDaysInfo, PassarValidationMessages.MessageNS30113);

				customsOffice.EstimatedNumberOfDays = "0";
				AssertNoMessageError("CY_Code is TRA but the EstimatedNumberOfDays is higher than 0, EstimatedNumberOfDays should not have an error message", customsOffice.EstimatedNumberOfDaysInfo, PassarValidationMessages.MessageNS30113);

				nctsHeader.MovementHeader.BM_TypeOfSecurity = OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination;
				customsOffice.EstimatedNumberOfDays = string.Empty;
				AssertNoMessageError("BM_TypeOfSecurity is not TRA, EstimatedNumberOfDays should not be mandatory", customsOffice.EstimatedNumberOfDaysInfo, PassarValidationMessages.MessageNS30113);
			});
		}
	}

	public void TestEstimatedNumberOfDaysNS30113WithDifferentCY_DataValues()
	{
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
		{
			CreateSafetyAndSecurityTradeGroupCountries();
			customsOffice.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit;
			nctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
			customsOffice.EstimatedNumberOfDays = string.Empty;

			CombineAssertions(() =>
			{
				customsOffice.CY_Data = "IT123456";
				customsOffice.Validation.ValidateAll();
				AssertHasMessageError("The GlobalCode has a EUSEC country code, EstimatedNumberOfDays should be mandatory", customsOffice.EstimatedNumberOfDaysInfo, PassarValidationMessages.MessageNS30113);

				customsOffice.EstimatedNumberOfDays = "0";
				AssertNoMessageError("The GlobalCode has a EUSEC country code but the EstimatedNumberOfDays is higher than 0, EstimatedNumberOfDays should not have an error message", customsOffice.EstimatedNumberOfDaysInfo, PassarValidationMessages.MessageNS30113);

				customsOffice.CY_Data = "RS123456";
				customsOffice.EstimatedNumberOfDays = string.Empty;
				AssertNoMessageError("The GlobalCode does not have a EUSEC country code, EstimatedNumberOfDays should be mandatory", customsOffice.EstimatedNumberOfDaysInfo, PassarValidationMessages.MessageNS30113);
			});
		}
	}

	public void TestEstimatedNumberOfDaysNS30033WithDifferentTypeOfSecurityValues()
	{
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
		{
			customsOffice.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit;
			customsOffice.EstimatedNumberOfDays = string.Empty;

			CombineAssertions(() =>
			{
				nctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
				customsOffice.Validation.ValidateAll();
				AssertHasMessageError("BM_TypeOfSecurity is ENT, EstimatedNumberOfDays should be mandatory", customsOffice.EstimatedNumberOfDaysInfo, PassarValidationMessages.MessageNS30033);

				nctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
				customsOffice.Validation.ValidateAll();
				AssertHasMessageError("BM_TypeOfSecurity is BTH, EstimatedNumberOfDays should be mandatory", customsOffice.EstimatedNumberOfDaysInfo, PassarValidationMessages.MessageNS30033);

				nctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
				customsOffice.Validation.ValidateAll();
				AssertHasMessageError("BM_TypeOfSecurity is EXI, EstimatedNumberOfDays should be mandatory", customsOffice.EstimatedNumberOfDaysInfo, PassarValidationMessages.MessageNS30033);

				customsOffice.EstimatedNumberOfDays = "0";
				AssertNoMessageError("BM_TypeOfSecurity is EXI but the EstimatedNumberOfDays is higher than 0, EstimatedNumberOfDays should not have an error message", customsOffice.EstimatedNumberOfDaysInfo, PassarValidationMessages.MessageNS30033);

				nctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
				customsOffice.EstimatedNumberOfDays = string.Empty;
				AssertNoMessageError("BM_TypeOfSecurity is not ENT, ENS or BTH, EstimatedNumberOfDays should not be mandatory", customsOffice.EstimatedNumberOfDaysInfo, PassarValidationMessages.MessageNS30033);
			});
		}
	}

	public void TestEstimatedNumberOfDaysNS30033WithDifferentCY_CodeValues()
	{
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
		{
			nctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
			customsOffice.EstimatedNumberOfDays = string.Empty;

			CombineAssertions(() =>
			{
				customsOffice.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit;
				customsOffice.Validation.ValidateAll();
				AssertHasMessageError("CY_Code is TRA, EstimatedNumberOfDays should be mandatory", customsOffice.EstimatedNumberOfDaysInfo, PassarValidationMessages.MessageNS30033);

				customsOffice.EstimatedNumberOfDays = "0";
				AssertNoMessageError("CY_Code is TRA but the EstimatedNumberOfDays is higher than 0, EstimatedNumberOfDays should not have an error message", customsOffice.EstimatedNumberOfDaysInfo, PassarValidationMessages.MessageNS30033);

				nctsHeader.MovementHeader.BM_TypeOfSecurity = OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination;
				customsOffice.EstimatedNumberOfDays = string.Empty;
				AssertNoMessageError("BM_TypeOfSecurity is not TRA, EstimatedNumberOfDays should not be mandatory", customsOffice.EstimatedNumberOfDaysInfo, PassarValidationMessages.MessageNS30033);
			});
		}
	}

	public void TestCheckCHOfficeForNationalTransit() => CombineAssertions(() =>
	{
		var errorMessage = "You must enter a Customs Office starting with 'CH' (National Customs Office).";
		nctsHeader.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland;
		customsOffice.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination;
		customsOffice.CY_Data = "IT123456";
		AssertHasMessageError("CY_Data Italy", customsOffice.CY_DataInfo, errorMessage);

		customsOffice.CY_Data = "CH123456";
		AssertNoMessageError("CY_Data Switzerland", customsOffice.CY_DataInfo, errorMessage);

		nctsHeader.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
		customsOffice.CY_Data = "IT123456";
		AssertNoMessageError("DeclarationType T1", customsOffice.CY_DataInfo, errorMessage);

		nctsHeader.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland;
		customsOffice.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit;
		customsOffice.Validation.ValidateCY_Data();
		AssertNoMessageError("CY_Code TRA", customsOffice.CY_DataInfo, errorMessage);
	});

	void CreateSafetyAndSecurityTradeGroupCountries()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion);
		var safetyAndSecurityTradeGroup = helper.CreateTradeGroup(EconomicGroupList.Codes.EuropeanUnion, Core.Constants.Customs.Universal.RefCusTradeGroup.Codes.EUForSafetyAndSecurity, new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
		helper.AddCountry(safetyAndSecurityTradeGroup, Core.Constants.CountryCodes.Italy, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
		Factory.Save();
	}

	NctsHeader nctsHeader;

	NctsEuOfficeCode customsOffice;

	protected override void SetUp()
	{
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		customsOffice = nctsHeader.MovementHeader.CustomsOfficesForDeparture.AddNew();
		customsOffice.EstimatedNumberOfDays = "0";
	}
}
