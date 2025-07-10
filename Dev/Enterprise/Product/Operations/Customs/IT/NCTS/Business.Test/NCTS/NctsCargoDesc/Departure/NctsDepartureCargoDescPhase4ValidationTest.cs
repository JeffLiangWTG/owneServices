using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Testing;
using static Enterprise.Core.Constants;
using ValidationCaptions = Enterprise.Customs.IT.Business.ValidationCaptions;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsDepartureCargoDescPhase4ValidationTest : TestCaseWithFactory
{
	public void TestCheckBY_RN_NKCountryOfDispatch()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.T2PlusSanMarino;
		var nctsDescOne = nctsHeader.MovementHeader.GoodsItems.AddNew();

		nctsDescOne.BY_RN_NKCountryOfDispatch = ZString.Empty;
		var nctsDescTwo = nctsHeader.MovementHeader.GoodsItems.AddNew();
		nctsDescTwo.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.France;
		AssertHasMessageErrorContaining("At least one good item dispatch country is empty", nctsDescTwo.BY_RN_NKCountryOfDispatchInfo, "All or none Goods Items Dispatch Country must be filled.");

		nctsDescTwo.BY_RN_NKCountryOfDispatch = ZString.Empty;
		AssertNoMessageErrorContaining("All good items dispatch country are empty", nctsDescTwo.BY_RN_NKCountryOfDispatchInfo, "All or none Goods Items Dispatch Country must be filled.");

		nctsDescOne.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.France;
		AssertHasMessageErrorContaining("At least one good item dispatch country is empty", nctsDescOne.BY_RN_NKCountryOfDispatchInfo, "All or none Goods Items Dispatch Country must be filled.");

		nctsHeader.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
		nctsHeader.MovementHeader.GoodsItems.DeleteAll();

		nctsDescOne = nctsHeader.MovementHeader.GoodsItems.AddNew();
		nctsDescOne.BY_RN_NKCountryOfDispatch = ZString.Empty;
		nctsDescOne = nctsHeader.MovementHeader.GoodsItems.AddNew();
		nctsDescTwo = nctsHeader.MovementHeader.GoodsItems.AddNew();

		nctsDescOne.BY_RN_NKCountryOfDispatch = ZString.Empty;
		nctsDescTwo.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.France;
		AssertNoMessageErrorContaining("Current status of each good items dispatch country wont be valuated", nctsDescTwo.BY_RN_NKCountryOfDispatchInfo, "All or none Goods Items Dispatch Country must be filled.");

		nctsDescTwo.BY_RN_NKCountryOfDispatch = ZString.Empty;
		AssertNoMessageErrorContaining("Current status of each good items dispatch country wont be valuated", nctsDescTwo.BY_RN_NKCountryOfDispatchInfo, "All or none Goods Items Dispatch Country must be filled.");

		nctsDescOne.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.France;
		AssertNoMessageErrorContaining("Current status of each good items dispatch country wont be valuated", nctsDescOne.BY_RN_NKCountryOfDispatchInfo, "All or none Goods Items Dispatch Country must be filled.");
	}

	public void TestCheckBY_RN_NKCountryOfDestination()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.T2PlusSanMarino;
		var nctsDescOne = nctsHeader.MovementHeader.GoodsItems.AddNew();

		nctsDescOne.BY_RN_NKCountryOfDestination = ZString.Empty;
		var nctsDescTwo = nctsHeader.MovementHeader.GoodsItems.AddNew();
		nctsDescTwo.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.France;
		AssertHasMessageErrorContaining("At least one good item country of destination is empty", nctsDescTwo.BY_RN_NKCountryOfDestinationInfo, "All or none Goods Items Destinations Country must be filled.");

		nctsDescTwo.BY_RN_NKCountryOfDestination = ZString.Empty;
		AssertNoMessageErrorContaining("All good items country of destination are empty", nctsDescTwo.BY_RN_NKCountryOfDestinationInfo, "All or none Goods Items Destinations Country must be filled.");

		nctsDescOne.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.France;
		AssertHasMessageErrorContaining("At least one good item country of destination is empty", nctsDescOne.BY_RN_NKCountryOfDestinationInfo, "All or none Goods Items Destinations Country must be filled.");
	}

	public void TestCheckBY_DescriptionMandatoryValidation()
	{
		nctsCargoDesc.Validation.ValidateBY_Description();
		AssertHasMessageErrorContaining("Description is required", nctsCargoDesc.BY_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);
		nctsCargoDesc.BY_Description = "XXX";
		AssertNoMessageErrorContaining("Description is valid", nctsCargoDesc.BY_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckBY_GrossWeightMandatoryValidation()
	{
		nctsCargoDesc.Validation.ValidateBY_GrossWeight();
		AssertHasMessageErrorContaining("Gross weight is required", nctsCargoDesc.BY_GrossWeightInfo, MandatoryValidation.YouHaveNotEntered);
		nctsCargoDesc.BY_GrossWeight = 10;
		AssertNoMessageErrorContaining("Gross weight is valid", nctsCargoDesc.BY_GrossWeightInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckBY_GrossWeightUnitMandatoryValidation()
	{
		nctsCargoDesc.BY_GrossWeightUnit = ZString.Empty;
		nctsCargoDesc.Validation.ValidateBY_GrossWeightUnit();
		AssertHasMessageErrorContaining("Gross weight unit is required", nctsCargoDesc.BY_GrossWeightUnitInfo, MandatoryValidation.YouHaveNotEntered);
		nctsCargoDesc.BY_GrossWeightUnit = "XX";
		AssertNoMessageErrorContaining("Gross weight unit is valid", nctsCargoDesc.BY_GrossWeightUnitInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckBY_GrossWeightNegativeValidation()
	{
		nctsCargoDesc.BY_GrossWeight = -5;
		nctsCargoDesc.Validation.ValidateBY_GrossWeight();
		AssertHasMessageErrorContaining("Gross weight cannot be negative", nctsCargoDesc.BY_GrossWeightInfo, MandatoryValidation.ValueCannotBeNegative);
		nctsCargoDesc.BY_GrossWeight = 5;
		AssertNoMessageErrorContaining("Gross weight is valid", nctsCargoDesc.BY_GrossWeightInfo, MandatoryValidation.ValueCannotBeNegative);
	}

	public void TestCheckBY_HarmonisedTariffInfoMandatoryValidation()
	{
		nctsCargoDesc.Validation.ValidateBY_HarmonisedTariff();
		AssertNoMessageErrors("Default Harmonised Tariff doesn't contains errors", nctsCargoDesc.BY_HarmonisedTariffInfo);

		previousDocument.CSI_Procedure = PreviousDocumentProcedureList.Codes.PartitaDiTemporaneaCustodiaA3;
		nctsCargoDesc.Validation.ValidateBY_HarmonisedTariff();
		AssertNoMessageErrors("Harmonised Tariff doesn't contains errors when previous doc. procedure = A3 and tariff = 0", nctsCargoDesc.BY_HarmonisedTariffInfo);
		previousDocument.CSI_Tariff = "12345";
		nctsCargoDesc.Validation.ValidateBY_HarmonisedTariff();
		AssertHasMessageErrorContaining("Harmonised Tariff is required when doc. procedure = A3 and Tariff defined", nctsCargoDesc.BY_HarmonisedTariffInfo, ValidationCaptions.NctsCargoDesc.CommodityCodeIsRequired);

		previousDocument.CSI_Procedure = PreviousDocumentProcedureList.Codes.Registro7DiIntroduzioneInDeposito;
		nctsCargoDesc.Validation.ValidateBY_HarmonisedTariff();
		AssertHasMessageErrorContaining("Harmonised Tariff is required when doc. procedure = 7", nctsCargoDesc.BY_HarmonisedTariffInfo, ValidationCaptions.NctsCargoDesc.CommodityCodeIsRequired);

		nctsCargoDesc.BY_HarmonisedTariff = "XXX";
		AssertNoMessageErrorContaining("Harmonised Tariff is valid", nctsCargoDesc.BY_HarmonisedTariffInfo, ValidationCaptions.NctsCargoDesc.CommodityCodeIsRequired);
	}

	public void TestCheckBY_NetWeightMandatoryValidation()
	{
		nctsCargoDesc.Validation.ValidateBY_NetWeight();
		AssertNoMessageErrors("Default Net Weight doesn't contains errors", nctsCargoDesc.BY_NetWeightInfo);

		previousDocument.CSI_Procedure = PreviousDocumentProcedureList.Codes.PartitaDiTemporaneaCustodiaA3;
		nctsCargoDesc.Validation.ValidateBY_NetWeight();
		AssertNoMessageErrors("Net Weight doesn't contains errors when previous doc. procedure = A3 and Net Weight = 0", nctsCargoDesc.BY_NetWeightInfo);
		previousDocument.CSI_Quantity = 10;
		nctsCargoDesc.Validation.ValidateBY_NetWeight();
		AssertHasMessageErrorContaining("Net weight is required when doc. procedure = A3 and Net Weight > 0", nctsCargoDesc.BY_NetWeightInfo, ValidationCaptions.NctsCargoDesc.NetWeightIsRequired);

		previousDocument.CSI_Procedure = PreviousDocumentProcedureList.Codes.Registro7DiIntroduzioneInDeposito;
		nctsCargoDesc.Validation.ValidateBY_NetWeight();
		AssertHasMessageErrorContaining("Net weight is required when doc. procedure = 7", nctsCargoDesc.BY_NetWeightInfo, ValidationCaptions.NctsCargoDesc.NetWeightIsRequired);

		nctsCargoDesc.BY_NetWeight = 10;
		AssertNoMessageErrorContaining("Net weight is valid", nctsCargoDesc.BY_NetWeightInfo, ValidationCaptions.NctsCargoDesc.NetWeightIsRequired);
	}

	public void TestCheckBY_NetWeightNegativeValidation()
	{
		nctsCargoDesc.BY_NetWeight = -5;
		nctsCargoDesc.Validation.ValidateBY_NetWeight();
		AssertHasMessageErrorContaining("Net weight cannot be negative", nctsCargoDesc.BY_NetWeightInfo, MandatoryValidation.ValueCannotBeNegative);
		nctsCargoDesc.BY_NetWeight = 5;
		AssertNoMessageErrorContaining("Net weight is valid", nctsCargoDesc.BY_NetWeightInfo, MandatoryValidation.ValueCannotBeNegative);
	}

	public void TestCheckMandatoryPackages()
	{
		nctsCargoDesc.Validation.ValidateAll();
		AssertHasRowMessageErrorContaining(nctsCargoDesc, ValidationCaptions.NctsPackage.GoodsItemMustHaveAtLeastOnePackage);
		nctsCargoDesc.Packages.AddNew();
		nctsCargoDesc.Validation.ValidateAll();
		AssertNoRowMessageErrorContaining(nctsCargoDesc, ValidationCaptions.NctsPackage.GoodsItemMustHaveAtLeastOnePackage);
	}

	public void TestCheckSupportingDocumentsRuleC901()
	{
		var nctsCargoDesc2 = nctsDepartureMovementHeader.GoodsItems.AddNew();
		var messageError = "A TIR declaration requires one Supporting Document of type 952 (TIR Carnet) on the first goods item";

		nctsCargoDesc.Validation.ValidateAll();
		AssertNoRowMessageErrorContaining(nctsCargoDesc, messageError);
		AssertNoRowMessageErrorContaining(nctsCargoDesc2, messageError);

		nctsHeader.MovementHeader.BM_InBondEntryType = "TIR";

		nctsCargoDesc.Validation.ValidateAll();
		AssertHasRowMessageErrorContaining(nctsCargoDesc, messageError);
		AssertNoRowMessageErrorContaining(nctsCargoDesc2, messageError);

		var supportingDocument = nctsCargoDesc.SupportingDocuments.AddNew();
		supportingDocument.CSI_Code = "952";

		nctsCargoDesc.Validation.ValidateAll();
		AssertNoRowMessageErrorContaining(nctsCargoDesc, messageError);
		AssertNoRowMessageErrorContaining(nctsCargoDesc2, messageError);
	}

	public void TestCheckCountryOfOriginListValidation()
	{
		nctsCargoDesc.BY_RN_NKCountryOfOrigin = "**";
		AssertHasMessageErrorContaining(nctsCargoDesc.BY_RN_NKCountryOfOriginInfo, ListValidation.InvalidCodeMessageError.ToString());

		nctsCargoDesc.BY_RN_NKCountryOfOrigin = CountryCodes.Belgium;
		AssertNoMessageErrorContaining(nctsCargoDesc.BY_RN_NKCountryOfOriginInfo, ListValidation.InvalidCodeMessageError.ToString());
	}

	public void TestCheckCountryOfOriginMandatoryValidation()
	{
		nctsCargoDesc.BY_RN_NKCountryOfDispatch = "";
		nctsCargoDesc.BY_RN_NKCountryOfOrigin = "";
		AssertNoMessageErrorContaining(nctsCargoDesc.BY_RN_NKCountryOfOriginInfo, MandatoryValidation.YouHaveNotEntered);

		nctsCargoDesc.BY_RN_NKCountryOfDispatch = CountryCodes.Belgium;
		nctsCargoDesc.Validation.ValidateBY_RN_NKCountryOfOrigin();
		AssertNoMessageErrorContaining(nctsCargoDesc.BY_RN_NKCountryOfOriginInfo, MandatoryValidation.YouHaveNotEntered);

		nctsCargoDesc.BY_RN_NKCountryOfDispatch = CountryCodes.Italy;
		nctsCargoDesc.Validation.ValidateBY_RN_NKCountryOfOrigin();
		AssertHasMessageErrorContaining(nctsCargoDesc.BY_RN_NKCountryOfOriginInfo, MandatoryValidation.YouHaveNotEntered);

		nctsCargoDesc.BY_RN_NKCountryOfOrigin = CountryCodes.UnitedStates;
		AssertNoMessageErrorContaining(nctsCargoDesc.BY_RN_NKCountryOfOriginInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckCountryOfOriginCN34Validation()
	{
		nctsHeader.BH_RL_NKImportLoadPort = "";
		nctsCargoDesc.BY_RN_NKCountryOfDispatch = "";
		nctsCargoDesc.BY_RN_NKCountryOfOrigin = "";
		AssertNoMessageErrorContaining("When Header.Country of Dispatch and GoodsItem.Country of Dispatch are both empty and BY_RN_NKCountryOfOrigin is empty", nctsCargoDesc.BY_RN_NKCountryOfOriginInfo, ValidationCaptions.NctsCargoDesc.FieldMustBeEmptyIfCountryOfDispatchIsIt);

		nctsCargoDesc.BY_RN_NKCountryOfOrigin = "IT";
		AssertHasMessageErrorContaining("When Header.Country of Dispatch is empty BY_RN_NKCountryOfOrigin is not empty", nctsCargoDesc.BY_RN_NKCountryOfOriginInfo, ValidationCaptions.NctsCargoDesc.FieldMustBeEmptyIfCountryOfDispatchIsIt);

		nctsHeader.BH_RL_NKImportLoadPort = "AU";
		nctsCargoDesc.Validation.ValidateBY_RN_NKCountryOfOrigin();
		AssertHasMessageErrorContaining("When Header.Country of Dispatch is not IT and BY_RN_NKCountryOfOrigin is not empty", nctsCargoDesc.BY_RN_NKCountryOfOriginInfo, ValidationCaptions.NctsCargoDesc.FieldMustBeEmptyIfCountryOfDispatchIsIt);

		nctsCargoDesc.BY_RN_NKCountryOfOrigin = "";
		AssertNoMessageErrorContaining("When Header.Country of Dispatch is not IT and BY_RN_NKCountryOfOrigin is empty", nctsCargoDesc.BY_RN_NKCountryOfOriginInfo, ValidationCaptions.NctsCargoDesc.FieldMustBeEmptyIfCountryOfDispatchIsIt);

		nctsCargoDesc.BY_RN_NKCountryOfOrigin = "US";
		nctsHeader.BH_RL_NKImportLoadPort = "";
		nctsCargoDesc.BY_RN_NKCountryOfOrigin = "DE";
		nctsCargoDesc.Validation.ValidateBY_RN_NKCountryOfOrigin();
		AssertHasMessageErrorContaining("When Goods Item.Country of Dispatch is not IT and BY_RN_NKCountryOfOrigin is not empty", nctsCargoDesc.BY_RN_NKCountryOfOriginInfo, ValidationCaptions.NctsCargoDesc.FieldMustBeEmptyIfCountryOfDispatchIsIt);

		nctsCargoDesc.BY_RN_NKCountryOfDispatch = "IT";
		nctsCargoDesc.Validation.ValidateBY_RN_NKCountryOfOrigin();
		AssertNoMessageErrorContaining("When Goods Item.Country of Dispatch is IT and BY_RN_NKCountryOfOrigin is not empty", nctsCargoDesc.BY_RN_NKCountryOfOriginInfo, ValidationCaptions.NctsCargoDesc.FieldMustBeEmptyIfCountryOfDispatchIsIt);

		nctsHeader.BH_RL_NKImportLoadPort = "FR";
		nctsCargoDesc.Validation.ValidateBY_RN_NKCountryOfOrigin();
		AssertNoMessageErrorContaining("When Goods Item.Country of Dispatch is IT but Header.Country of Dispatch is not and BY_RN_NKCountryOfOrigin is not empty (Edge Case)", nctsCargoDesc.BY_RN_NKCountryOfOriginInfo, ValidationCaptions.NctsCargoDesc.FieldMustBeEmptyIfCountryOfDispatchIsIt);
	}

	public void TestCheckOriginStateListValidation()
	{
		nctsCargoDesc.BY_RN_NKCountryOfOrigin = CountryCodes.Italy;
		nctsCargoDesc.BY_RW_NKOriginState = "*+*";
		AssertHasMessageErrorContaining(nctsCargoDesc.BY_RW_NKOriginStateInfo, ListValidation.InvalidCodeMessageError.ToString());

		nctsCargoDesc.BY_RN_NKCountryOfOrigin = CountryCodes.Belgium;
		AssertNoMessageErrorContaining(nctsCargoDesc.BY_RW_NKOriginStateInfo, ListValidation.InvalidCodeMessageError.ToString());
	}

	public void TestCheckOriginStateMandatoryValidation()
	{
		nctsCargoDesc.BY_RN_NKCountryOfDispatch = "";
		nctsCargoDesc.BY_RN_NKCountryOfOrigin = "";
		nctsCargoDesc.BY_RW_NKOriginState = "";
		AssertNoMessageErrorContaining(nctsCargoDesc.BY_RW_NKOriginStateInfo, MandatoryValidation.YouHaveNotEntered);

		nctsCargoDesc.BY_RN_NKCountryOfDispatch = CountryCodes.Belgium;
		nctsCargoDesc.BY_RN_NKCountryOfOrigin = "";
		nctsCargoDesc.Validation.ValidateBY_RW_NKOriginState();
		AssertNoMessageErrorContaining(nctsCargoDesc.BY_RW_NKOriginStateInfo, MandatoryValidation.YouHaveNotEntered);

		nctsCargoDesc.BY_RN_NKCountryOfDispatch = CountryCodes.Italy;
		nctsCargoDesc.Validation.ValidateBY_RW_NKOriginState();
		AssertNoMessageErrorContaining(nctsCargoDesc.BY_RW_NKOriginStateInfo, MandatoryValidation.YouHaveNotEntered);

		nctsCargoDesc.BY_RN_NKCountryOfDispatch = CountryCodes.Italy;
		nctsCargoDesc.BY_RN_NKCountryOfOrigin = CountryCodes.Italy;
		nctsCargoDesc.Validation.ValidateBY_RW_NKOriginState();
		AssertHasMessageErrorContaining(nctsCargoDesc.BY_RW_NKOriginStateInfo, MandatoryValidation.YouHaveNotEntered);

		nctsCargoDesc.BY_RW_NKOriginState = "PD";
		AssertNoMessageErrorContaining(nctsCargoDesc.BY_RW_NKOriginStateInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckOriginStateCN34Validation()
	{
		nctsHeader.BH_RL_NKImportLoadPort = "DE";
		nctsCargoDesc.BY_RN_NKCountryOfOrigin = "US";
		nctsCargoDesc.BY_RW_NKOriginState = "";
		AssertNoMessageErrorContaining("When Country of Dispatch and Country of Origin are not 'IT' and BY_RW_NKOriginState is empty", nctsCargoDesc.BY_RW_NKOriginStateInfo, ValidationCaptions.NctsCargoDesc.FieldMustBeEmptyIfCountryOfDispatchIsIt);

		nctsCargoDesc.BY_RW_NKOriginState = "IT";
		AssertHasMessageErrorContaining("When Country of Dispatch and Country of Origin are not 'IT' and BY_RW_NKOriginState is not empty", nctsCargoDesc.BY_RW_NKOriginStateInfo, ValidationCaptions.NctsCargoDesc.FieldMustBeEmptyIfCountryOfDispatchIsIt);

		nctsCargoDesc.BY_RN_NKCountryOfOrigin = "IT";
		nctsCargoDesc.BY_RW_NKOriginState = "";
		AssertNoMessageErrorContaining("When Country of Dispatch is not 'IT', Country of Origin is 'IT' and BY_RW_NKOriginState is empty", nctsCargoDesc.BY_RW_NKOriginStateInfo, ValidationCaptions.NctsCargoDesc.FieldMustBeEmptyIfCountryOfDispatchIsIt);

		nctsCargoDesc.BY_RW_NKOriginState = "IT";
		AssertHasMessageErrorContaining("When Country of Dispatch is not 'IT', Country of Origin is 'IT' and BY_RW_NKOriginState is not empty", nctsCargoDesc.BY_RW_NKOriginStateInfo, ValidationCaptions.NctsCargoDesc.FieldMustBeEmptyIfCountryOfDispatchIsIt);

		nctsHeader.BH_RL_NKImportLoadPort = "IT";
		nctsCargoDesc.BY_RN_NKCountryOfOrigin = "DE";
		nctsCargoDesc.BY_RW_NKOriginState = "";
		AssertNoMessageErrorContaining("When Country of Dispatch is 'IT', Country of Origin is not 'IT' and BY_RW_NKOriginState is empty", nctsCargoDesc.BY_RW_NKOriginStateInfo, ValidationCaptions.NctsCargoDesc.FieldMustBeEmptyIfCountryOfDispatchIsIt);

		nctsCargoDesc.BY_RW_NKOriginState = "IT";
		AssertHasMessageErrorContaining("When Country of Dispatch is 'IT', Country of Origin is not 'IT' and BY_RW_NKOriginState is not empty", nctsCargoDesc.BY_RW_NKOriginStateInfo, ValidationCaptions.NctsCargoDesc.FieldMustBeEmptyIfCountryOfDispatchIsIt);

		nctsCargoDesc.BY_RN_NKCountryOfOrigin = "IT";
		nctsCargoDesc.BY_RW_NKOriginState = "";
		AssertNoMessageErrorContaining("When Country of Dispatch and Country of Origin are 'IT' and BY_RW_NKOriginState is empty", nctsCargoDesc.BY_RW_NKOriginStateInfo, ValidationCaptions.NctsCargoDesc.FieldMustBeEmptyIfCountryOfDispatchIsIt);

		nctsCargoDesc.BY_RW_NKOriginState = "IT";
		AssertNoMessageErrorContaining("When Country of Dispatch and Country of Origin are 'IT' and BY_RW_NKOriginState is not empty", nctsCargoDesc.BY_RW_NKOriginStateInfo, ValidationCaptions.NctsCargoDesc.FieldMustBeEmptyIfCountryOfDispatchIsIt);
	}

	public void TestCheckBM_InBondEntryType_ConditionC547()
	{
		var validation = nctsDepartureMovementHeader.Validation;
		validation.ValidateBM_InBondEntryType();
		nctsDepartureMovementHeader.BM_InBondEntryType = "TIR";
		AssertNoMessageErrorContaining("When Security = false and Commercial reference number is empty, there shouldn't be any error message", nctsDepartureMovementHeader.BM_InBondEntryTypeInfo, "C547");

		nctsHeader.BH_FTZMove = true;
		nctsDepartureMovementHeader.BM_BTAIndicator = "A";
		validation.ValidateBM_InBondEntryType();
		AssertNoMessageErrorContaining("When Security = true and BTA Indicator = A, Commercial reference number is optional and there shouldn't be any error message", nctsDepartureMovementHeader.BM_InBondEntryTypeInfo, "C547");

		nctsDepartureMovementHeader.BM_BTAIndicator = "E";
		validation.ValidateBM_InBondEntryType();
		AssertHasMessageErrorContaining("Transport document should be present on the first goods item", nctsDepartureMovementHeader.BM_InBondEntryTypeInfo, "C547");

		nctsCargoDesc.SupportingDocuments.AddNew();
		validation.ValidateBM_InBondEntryType();
		AssertNoMessageErrorContaining("Transport document is present on the first goods item, there shouldn't be any error message", nctsDepartureMovementHeader.BM_InBondEntryTypeInfo, "C547");
	}

	public void TestCheckBY_ProcedureMandatoryValidation()
	{
		nctsCargoDesc.BY_Procedure = ZString.Empty;
		AssertHasMessageErrorContaining("Empty procedure", nctsCargoDesc.BY_ProcedureInfo, MandatoryValidation.YouHaveNotEntered);

		nctsCargoDesc.BY_Procedure = "1234";
		AssertNoMessageErrorContaining("Filled procedure", nctsCargoDesc.BY_ProcedureInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckBY_ProcedureListValidation()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateRefCusProcedure(CountryCodes.Italy, "A", "80", "00", "", "", "", false);

		nctsCargoDesc.BY_Procedure = "1234";
		AssertHasMessageErrorContaining("Invalid procedure", nctsCargoDesc.BY_ProcedureInfo, ListValidation.InvalidCodeMessageError.ToString());

		nctsCargoDesc.BY_Procedure = "8000";
		AssertNoMessageErrorContaining("Valid procedure", nctsCargoDesc.BY_ProcedureInfo, ListValidation.InvalidCodeMessageError.ToString());
	}

	public void TestCheckBY_CommercialReferenceNumberSafetyAndSecurityWithFilledBM_AdditionalText()
	{
		var validation = nctsCargoDesc.Validation;
		nctsHeader.BH_FTZMove = true;
		nctsDepartureMovementHeader.BM_AdditionalText = "12345";

		nctsCargoDesc.BY_CommercialReferenceNumber = ZString.Empty;
		validation.ValidateBY_CommercialReferenceNumber();
		AssertNoMessageErrorContaining("If single BY_CommercialReferenceNumber is empty then there should be no error message", nctsCargoDesc.BY_CommercialReferenceNumberInfo, "The field Commercial Reference Number cannot be set both at Header and Goods Items level");

		nctsCargoDesc.BY_CommercialReferenceNumber = "12345";
		validation.ValidateBY_CommercialReferenceNumber();
		AssertHasMessageErrorContaining("If single BY_CommercialReferenceNumber is filled then there should be an error message", nctsCargoDesc.BY_CommercialReferenceNumberInfo, "The field Commercial Reference Number cannot be set both at Header and Goods Items level");

		nctsCargoDesc.BY_CommercialReferenceNumber = ZString.Empty;
		var nctsCargoDesc2 = nctsDepartureMovementHeader.GoodsItems.AddNew();
		nctsCargoDesc2.BY_CommercialReferenceNumber = ZString.Empty;
		validation.ValidateBY_CommercialReferenceNumber();
		AssertNoMessageErrorContaining("If multiple BY_CommercialReferenceNumber are empty then there should be no error message", nctsCargoDesc.BY_CommercialReferenceNumberInfo, "The field Commercial Reference Number cannot be set both at Header and Goods Items level");
		AssertNoMessageErrorContaining("If multiple BY_CommercialReferenceNumber are empty then there should be no error message", nctsCargoDesc2.BY_CommercialReferenceNumberInfo, "The field Commercial Reference Number cannot be set both at Header and Goods Items level");

		nctsCargoDesc.BY_CommercialReferenceNumber = "12345";
		nctsCargoDesc2.BY_CommercialReferenceNumber = "12345";
		validation.ValidateBY_CommercialReferenceNumber();
		AssertHasMessageErrorContaining("If multiple BY_CommercialReferenceNumber are filled then there should be an error message", nctsCargoDesc.BY_CommercialReferenceNumberInfo, "The field Commercial Reference Number cannot be set both at Header and Goods Items level");
		AssertHasMessageErrorContaining("If multiple BY_CommercialReferenceNumber are filled then there should be an error message", nctsCargoDesc2.BY_CommercialReferenceNumberInfo, "The field Commercial Reference Number cannot be set both at Header and Goods Items level");

		nctsCargoDesc.BY_CommercialReferenceNumber = "12345";
		nctsCargoDesc2.BY_CommercialReferenceNumber = ZString.Empty;
		validation.ValidateBY_CommercialReferenceNumber();
		AssertHasMessageErrorContaining("If this BY_CommercialReferenceNumber is filled but not all of them then there should be an error message", nctsCargoDesc.BY_CommercialReferenceNumberInfo, "The field Commercial Reference Number cannot be set both at Header and Goods Items level");
		AssertNoMessageErrorContaining("If this BY_CommercialReferenceNumber are empty but not all of them then there should be no error message", nctsCargoDesc2.BY_CommercialReferenceNumberInfo, "The field Commercial Reference Number cannot be set both at Header and Goods Items level");
	}

	public void TestCheckBY_CommercialReferenceNumberSafetyAndSecurityWithEmptyBM_AdditionalText()
	{
		var validation = nctsCargoDesc.Validation;
		nctsHeader.BH_FTZMove = true;
		nctsDepartureMovementHeader.BM_AdditionalText = ZString.Empty;

		nctsCargoDesc.BY_CommercialReferenceNumber = ZString.Empty;
		validation.ValidateBY_CommercialReferenceNumber();
		AssertNoMessageErrors("If a single BY_CommercialReferenceNumber is empty then there should be no error message", nctsCargoDesc.BY_CommercialReferenceNumberInfo);

		nctsCargoDesc.BY_CommercialReferenceNumber = "12345";
		validation.ValidateBY_CommercialReferenceNumber();
		AssertNoMessageErrors("If a single BY_CommercialReferenceNumber is filled then there should be no error message", nctsCargoDesc.BY_CommercialReferenceNumberInfo);

		var nctsCargoDesc2 = nctsDepartureMovementHeader.GoodsItems.AddNew();
		nctsCargoDesc2.BY_CommercialReferenceNumber = "12345";
		validation.ValidateBY_CommercialReferenceNumber();
		AssertNoMessageErrors("If multiple BY_CommercialReferenceNumber are filled then there should be no error message", nctsCargoDesc.BY_CommercialReferenceNumberInfo);
		AssertNoMessageErrors("If multiple BY_CommercialReferenceNumber are filled then there should be no error message", nctsCargoDesc2.BY_CommercialReferenceNumberInfo);

		nctsCargoDesc2.BY_CommercialReferenceNumber = ZString.Empty;
		validation.ValidateBY_CommercialReferenceNumber();
		AssertNoMessageErrors("If one BY_CommercialReferenceNumber is filled but not all of them then there should be no error message", nctsCargoDesc.BY_CommercialReferenceNumberInfo);
		AssertNoMessageErrors("If one BY_CommercialReferenceNumber is filled but not all of them then there should be no error message", nctsCargoDesc2.BY_CommercialReferenceNumberInfo);

		nctsCargoDesc.BY_CommercialReferenceNumber = ZString.Empty;
		validation.ValidateBY_CommercialReferenceNumber();
		AssertNoMessageErrors("If multiple BY_CommercialReferenceNumber are empty but not all of them then there should be no error message", nctsCargoDesc.BY_CommercialReferenceNumberInfo);
		AssertNoMessageErrors("If multiple BY_CommercialReferenceNumber are empty but not all of them then there should be no error message", nctsCargoDesc2.BY_CommercialReferenceNumberInfo);
	}

	public void TestCheckBY_CommodityCode()
	{
		new ITUniversalReferenceTestDataHelper(Factory).SetupPortTaxRates();
		Factory.Save();

		nctsCargoDesc.BY_CommodityCode = "";
		AssertNoMessageErrorContaining(nctsCargoDesc.BY_CommodityCodeInfo, ListValidation.InvalidCodeMessageError);

		nctsCargoDesc.BY_CommodityCode = "A1";
		AssertNoMessageErrorContaining(nctsCargoDesc.BY_CommodityCodeInfo, ListValidation.InvalidCodeMessageError);

		nctsCargoDesc.BY_CommodityCode = "XX";
		AssertHasMessageErrorContaining(nctsCargoDesc.BY_CommodityCodeInfo, ListValidation.InvalidCodeMessageError);
	}

	public void TestCheckBY_CustomsSecondQuantity()
	{
		CombineAssertions(() =>
		{
			nctsCargoDesc.BY_CustomsSecondUnitQty = "NAR";
			nctsCargoDesc.BY_CustomsSecondQuantity = 0;
			AssertHasMessageErrorContaining("Has UOM and No Qty", nctsCargoDesc.BY_CustomsSecondQuantityInfo, MandatoryValidation.ValueCannotBeZero);

			nctsCargoDesc.BY_CustomsSecondQuantity = 12.3m;
			AssertNoMessageErrorContaining("Has Qty", nctsCargoDesc.BY_CustomsSecondQuantityInfo, MandatoryValidation.ValueCannotBeZero);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsDepartureMovementHeader = nctsHeader.MovementHeader;
		nctsCargoDesc = nctsDepartureMovementHeader.GoodsItems.AddNew();
		previousDocument = nctsCargoDesc.PreviousDocuments.AddNew();
	}

	NctsHeader nctsHeader;
	NctsDepartureMovementHeader nctsDepartureMovementHeader;
	NctsDepartureCargoDesc nctsCargoDesc;
	NctsPreviousDocument previousDocument;
}
