using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class NctsDepartureCargoDescValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckBY_NetWeight()
	{
		var helper = new RefCusTariffTestHelper(Factory);
		var tariffMineralFuels = helper.CreateExportTariff("27160000911");
		var tariffFruitJuice = helper.CreateExportTariffWithAttribute("20096119000", CH.Business.UniversalReferenceConstants.TariffAttributes.NetMassOptional, CH.Business.UniversalReferenceConstants.TariffAttributes.Values.Yes);
		var tariffToysGamesAndSports = helper.CreateExportTariffWithAttribute("95049000000", CH.Business.UniversalReferenceConstants.TariffAttributes.NetMassOptional, CH.Business.UniversalReferenceConstants.TariffAttributes.Values.No);
		Factory.Save();

		var errorMessage = PassarValidationMessages.MessageNotEntered(PassarValidationMessages.NS30092, DepartureCargoDesc.BY_NetWeightInfo.HumanReadableName);

		CombineAssertions(() =>
		{
			DepartureCargoDesc.BY_HarmonisedTariff = "123456789012";
			DepartureCargoDesc.Validation.ValidateBY_NetWeight();
			AssertNoMessageError("Invalid tariff - not mandatory", DepartureCargoDesc.BY_NetWeightInfo, errorMessage);

			DepartureCargoDesc.BY_HarmonisedTariff = tariffMineralFuels.ZZ1_TariffCode;
			DepartureCargoDesc.Validation.ValidateBY_NetWeight();
			AssertNoMessageError("no NetMassOptional code - not mandatory", DepartureCargoDesc.BY_NetWeightInfo, errorMessage);

			DepartureCargoDesc.BY_HarmonisedTariff = tariffFruitJuice.ZZ1_TariffCode;
			DepartureCargoDesc.Validation.ValidateBY_NetWeight();
			AssertNoMessageError("NetMassOptional Y - not mandatory", DepartureCargoDesc.BY_NetWeightInfo, errorMessage);

			DepartureCargoDesc.BY_HarmonisedTariff = tariffToysGamesAndSports.ZZ1_TariffCode;
			DepartureCargoDesc.Validation.ValidateBY_NetWeight();
			AssertHasMessageError("NetMassOptional N - mandatory - empty", DepartureCargoDesc.BY_NetWeightInfo, errorMessage);

			DepartureCargoDesc.BY_HarmonisedTariff = tariffToysGamesAndSports.ZZ1_TariffCode;
			DepartureCargoDesc.BY_NetWeight = 100;
			AssertNoMessageError("NetMassOptional N - mandatory - entered", DepartureCargoDesc.BY_NetWeightInfo, errorMessage);
		});
	}

	public void TestCheckBY_HarmonisedTariffDataGroupSkipped()
	{
		var wcoGroupCode = "013456";
		var testHelper = new RefDataTestHelper(Factory);
		testHelper.CreateTariffs(Universal.Constants.TariffTypes.HarmonizedSystem, dataGrouping: Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO).CreateTariff(wcoGroupCode);

		Factory.Save();

		CombineAssertions(() =>
		{
			DepartureCargoDesc.BY_FormattedHarmonisedTariff = wcoGroupCode;
			DepartureCargoDesc.Validation.ValidateBY_HarmonisedTariff();
			AssertNoMessageErrors("Valid WCO code", DepartureCargoDesc.BY_FormattedHarmonisedTariffInfo);

			DepartureCargoDesc.BY_FormattedHarmonisedTariff = "123456";
			DepartureCargoDesc.Validation.ValidateBY_HarmonisedTariff();
			AssertHasMessageError("CH validation error is presented", DepartureCargoDesc.BY_FormattedHarmonisedTariffInfo, PassarValidationMessages.MessageNP70205);
			AssertEquals("Has only one error", 1, DepartureCargoDesc.BY_FormattedHarmonisedTariffInfo.Notifications.GetMessageErrors().Count());
		});
	}

	public void TestBY_HarmonisedTariffCharacterCheck() => CargoDescTestHelper.AssertHarmonisedTariffCharacterCheck(Factory, DepartureCargoDesc);

	public void TestBY_RN_NKCountryOfDispatch_NS30162() => CombineAssertions(() =>
	{
		var errorMessage = PassarValidationMessages.MessageNS30162;

		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.CHNT015V4, Core.Constants.CountryCodes.Switzerland, ZDate.Today, false))
		{
			DepartureCargoDesc.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.Switzerland;
			DepartureCargoDesc.Bill.B0_RN_NKCountryOfExport = Core.Constants.CountryCodes.Switzerland;
			DepartureCargoDesc.MoveHeader.BM_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.Switzerland;
			DepartureCargoDesc.MoveHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
			DepartureCargoDesc.Validation.ValidateBY_RN_NKCountryOfDispatch();
			AssertNoMessageError("Country of export/dispatch not empty at all levels", DepartureCargoDesc.BY_RN_NKCountryOfDispatchInfo, errorMessage);

			DepartureCargoDesc.BY_RN_NKCountryOfDispatch = ZString.Empty;
			DepartureCargoDesc.Validation.ValidateBY_RN_NKCountryOfDispatch();
			AssertNoMessageError("Country of dispatch empty only at DepartureCargoDesc level", DepartureCargoDesc.BY_RN_NKCountryOfDispatchInfo, errorMessage);

			DepartureCargoDesc.Bill.B0_RN_NKCountryOfExport = ZString.Empty;
			DepartureCargoDesc.Validation.ValidateBY_RN_NKCountryOfDispatch();
			AssertNoMessageError("Country of dispatch not empty only at Movement header", DepartureCargoDesc.BY_RN_NKCountryOfDispatchInfo, errorMessage);

			DepartureCargoDesc.MoveHeader.BM_RN_NKCountryOfDispatch = ZString.Empty;
			DepartureCargoDesc.Validation.ValidateBY_RN_NKCountryOfDispatch();
			AssertHasMessageError("Country of export/dispatch empty at all levels", DepartureCargoDesc.BY_RN_NKCountryOfDispatchInfo, errorMessage);

			DepartureCargoDesc.MoveHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland;
			DepartureCargoDesc.Validation.ValidateBY_RN_NKCountryOfDispatch();
			AssertNoMessageError("Country of export/dispatch empty at all levels, National Transit", DepartureCargoDesc.BY_RN_NKCountryOfDispatchInfo, errorMessage);
		}

		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.CHNT015V4, Core.Constants.CountryCodes.Switzerland, ZDate.Today, true))
		{
			DepartureCargoDesc.Validation.ValidateBY_RN_NKCountryOfDispatch();
			AssertNoMessageError("CHNT015V4 active", DepartureCargoDesc.BY_RN_NKCountryOfDispatchInfo, errorMessage);
		}
	});

	public void TestBY_RN_NKCountryOfDispatch_NS30040() => CombineAssertions(() =>
	{
		var errorMessage = "[NS30040] Destination Country/Region must be filled either on Declaration, House Consignment or Goods Item Level.";

		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.CHNT015V4, Core.Constants.CountryCodes.Switzerland, ZDate.Today, false))
		{
			DepartureCargoDesc.BY_RN_NKCountryOfDestination = ZString.Empty;
			DepartureCargoDesc.Bill.B0_RN_NKCountryOfDestination = ZString.Empty;
			DepartureCargoDesc.MoveHeader.BM_RL_NKDestinationPort = ZString.Empty;

			DepartureCargoDesc.MoveHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
			DepartureCargoDesc.Validation.ValidateBY_RN_NKCountryOfDestination();
			AssertHasMessageError("Country of destination empty at all levels", DepartureCargoDesc.BY_RN_NKCountryOfDestinationInfo, errorMessage);

			DepartureCargoDesc.MoveHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland;
			DepartureCargoDesc.Validation.ValidateBY_RN_NKCountryOfDestination();
			AssertNoMessageError("Country of destination empty at all levels, National Transit", DepartureCargoDesc.BY_RN_NKCountryOfDestinationInfo, errorMessage);
		}
	});

		NctsDepartureCargoDesc CreateNctsDepartureCargoDesc()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		var bill = nctsHeader.Bills.AddNew();
		departureCargoDesc = bill.GoodsItems.AddNew();
		return departureCargoDesc;
	}

	NctsDepartureCargoDesc DepartureCargoDesc => departureCargoDesc ?? (departureCargoDesc = CreateNctsDepartureCargoDesc());
	NctsDepartureCargoDesc departureCargoDesc;
}
