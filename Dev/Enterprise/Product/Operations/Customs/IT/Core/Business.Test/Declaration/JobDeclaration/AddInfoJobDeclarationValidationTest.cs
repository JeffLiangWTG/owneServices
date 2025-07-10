using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using SpecificCircumstance = Enterprise.Customs.EU.Business.SpecificCircumstanceIndicator.Codes;
using TransportMode = Enterprise.Customs.Business.TransportTypeList.Codes;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class AddInfoJobDeclarationValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckZG_SpecificCircumstanceIndicator()
	{
		var mustHaveValidCertificatesMessageError = "For Circumstance 'E' the Declarant and all Suppliers must have a valid AEO of type AEOF or AEOS. Consider adding the appropriate AEO in Organization>Config>Registration numbers/codes";
		var circumstanceAndTransportAreIncongruentMessageError = "Circumstance and [25] Transport are in-congruent";

		var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
		orgHeader1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "AEOF1234", Core.Constants.CountryCodes.Italy);

		var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
		orgHeader2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "AEO", Core.Constants.CountryCodes.Italy);

		var orgHeader3 = Factory.NewWithValidTestData<OrgHeader>();

		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
		declaration.MessageVersion = "TXT";

		CombineAssertions("When Declaration is EXP TXT and Circumstance is E", () =>
		{
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			declaration.ZG_SpecificCircumstanceIndicator = SpecificCircumstanceIndicator.Codes.AuthorizedEconomicOperators;
			AssertNoMessageError(declaration.ZG_SpecificCircumstanceIndicatorInfo, mustHaveValidCertificatesMessageError);

			declaration.JE_OH_Supplier = orgHeader1.PK;
			declaration.AddInfoValidation.ValidateZG_SpecificCircumstanceIndicator();
			AssertNoMessageError(declaration.ZG_SpecificCircumstanceIndicatorInfo, mustHaveValidCertificatesMessageError);

			declaration.JE_OA_DeclarantAddress = orgHeader1.MainAddress.PK;
			invoice.JZ_OH_Supplier = orgHeader1.PK;
			declaration.AddInfoValidation.ValidateZG_SpecificCircumstanceIndicator();
			AssertNoMessageError(declaration.ZG_SpecificCircumstanceIndicatorInfo, mustHaveValidCertificatesMessageError);

			declaration.JE_OH_Supplier = orgHeader2.PK;
			declaration.AddInfoValidation.ValidateZG_SpecificCircumstanceIndicator();
			AssertHasMessageError(declaration.ZG_SpecificCircumstanceIndicatorInfo, mustHaveValidCertificatesMessageError);

			declaration.JE_OH_Supplier = orgHeader3.PK;
			declaration.AddInfoValidation.ValidateZG_SpecificCircumstanceIndicator();
			AssertHasMessageError(declaration.ZG_SpecificCircumstanceIndicatorInfo, mustHaveValidCertificatesMessageError);
		});

		CombineAssertions("When Declaration is EXP TXT", () =>
		{
			AssertCircumstanceAndTransportAreIncongruentMessageError(TransportMode.Mail, SpecificCircumstance.PostalAndExpressConsignments, expectedError: false);
			AssertCircumstanceAndTransportAreIncongruentMessageError(TransportMode.InlandWaterwayTransport, SpecificCircumstance.PostalAndExpressConsignments, expectedError: true);
			AssertCircumstanceAndTransportAreIncongruentMessageError(TransportMode.Rail, SpecificCircumstance.RailModeOfTransport, expectedError: false);
			AssertCircumstanceAndTransportAreIncongruentMessageError(TransportMode.InlandWaterwayTransport, SpecificCircumstance.RailModeOfTransport, expectedError: true);
			AssertCircumstanceAndTransportAreIncongruentMessageError(TransportMode.Road, SpecificCircumstance.RoadModeOfTransport, expectedError: false);
			AssertCircumstanceAndTransportAreIncongruentMessageError(TransportMode.InlandWaterwayTransport, SpecificCircumstance.RoadModeOfTransport, expectedError: true);
			AssertCircumstanceAndTransportAreIncongruentMessageError(TransportMode.Sea, SpecificCircumstance.ShipAndAircraftSupplies, expectedError: false);
			AssertCircumstanceAndTransportAreIncongruentMessageError(TransportMode.Air, SpecificCircumstance.ShipAndAircraftSupplies, expectedError: false);
			AssertCircumstanceAndTransportAreIncongruentMessageError(TransportMode.InlandWaterwayTransport, SpecificCircumstance.ShipAndAircraftSupplies, expectedError: true);
		});

		declaration.MessageVersion = "XML";
		CombineAssertions("When Declaration is EXP", () =>
		{
			declaration.JE_OH_Supplier = orgHeader2.PK;
			declaration.ZG_SpecificCircumstanceIndicator = SpecificCircumstanceIndicator.Codes.AuthorizedEconomicOperators;
			declaration.AddInfoValidation.ValidateZG_SpecificCircumstanceIndicator();
			AssertNoMessageError(declaration.ZG_SpecificCircumstanceIndicatorInfo, mustHaveValidCertificatesMessageError);

			AssertCircumstanceAndTransportAreIncongruentMessageError(TransportMode.InlandWaterwayTransport, SpecificCircumstance.PostalAndExpressConsignments, expectedError: false);
		});

		void AssertCircumstanceAndTransportAreIncongruentMessageError(ZString transportMode, ZString specificCircumstanceIndicator, bool expectedError)
		{
			declaration.JE_TransportMode = transportMode;
			declaration.ZG_SpecificCircumstanceIndicator = specificCircumstanceIndicator;
			declaration.AddInfoValidation.ValidateZG_SpecificCircumstanceIndicator();
			if (expectedError)
			{
				AssertHasMessageError(declaration.ZG_SpecificCircumstanceIndicatorInfo, circumstanceAndTransportAreIncongruentMessageError);
			}
			else
			{
				AssertNoMessageError(declaration.ZG_SpecificCircumstanceIndicatorInfo, circumstanceAndTransportAreIncongruentMessageError);
			}
		}
	}

	public void TestCheckZG_SpecificCircumstanceIndicator_ListValidation()
	{
		declaration.ZG_SpecificCircumstanceIndicator = "XX";
		AssertHasMessageErrorContaining(declaration.ZG_SpecificCircumstanceIndicatorInfo, ListValidation.InvalidCodeMessageError);
	}

	public void TestCheckZG_IsSecurityDeclaration()
	{
		declaration.JE_MessageType = "EXP";
		const string errorMessage = "[C0211] Please insert Itinerary Countries in Misc tab";

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, false))
			{
				declaration.ZG_IsSecurityDeclaration = true;
				AssertHasMessageErrorContaining("Security checkbox checked, No Country is added in Itinerary Countries in misc", declaration.ZG_IsSecurityDeclarationInfo, errorMessage);

				declaration.ZG_IsSecurityDeclaration = false;
				AssertNoMessageErrorContaining("Security checkbox is not checked, Country is not added in Itinerary Countries in misc", declaration.ZG_IsSecurityDeclarationInfo, errorMessage);

				var itineraryCountry = declaration.ItineraryCountries.AddNew();
				itineraryCountry.CY_Order = 1;
				itineraryCountry.CY_Code = "IN";
				declaration.ZG_IsSecurityDeclaration = true;
				AssertNoMessageErrorContaining("Security checkbox is checked, Country is added in Itinerary Countries in misc", declaration.ZG_IsSecurityDeclarationInfo, errorMessage);

				declaration.ZG_IsSecurityDeclaration = false;
				AssertNoMessageErrorContaining("Security checkbox is not checked, Country is added in Itinerary Countries in misc", declaration.ZG_IsSecurityDeclarationInfo, errorMessage);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
			{
				declaration.ZG_IsSecurityDeclaration = true;
				AssertNoMessageErrorContaining("Security checkbox checked, No Country is added in Itinerary Countries in misc", declaration.ZG_IsSecurityDeclarationInfo, errorMessage);

				declaration.ZG_IsSecurityDeclaration = false;
				AssertNoMessageErrorContaining("Security checkbox is not checked, Country is not added in Itinerary Countries in misc", declaration.ZG_IsSecurityDeclarationInfo, errorMessage);
			}
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
			{
				declaration.ZG_IsSecurityDeclaration = true;
				AssertNoMessageErrorContaining("Security checkbox checked, No Country is added in Itinerary Countries in misc", declaration.ZG_IsSecurityDeclarationInfo, errorMessage);

				declaration.ZG_IsSecurityDeclaration = false;
				AssertNoMessageErrorContaining("Security checkbox is not checked, Country is not added in Itinerary Countries in misc", declaration.ZG_IsSecurityDeclarationInfo, errorMessage);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, false))
			{
				declaration.ZG_IsSecurityDeclaration = true;
				AssertNoMessageErrorContaining("Security checkbox checked, No Country is added in Itinerary Countries in misc", declaration.ZG_IsSecurityDeclarationInfo, errorMessage);

				declaration.ZG_IsSecurityDeclaration = false;
				AssertNoMessageErrorContaining("Security checkbox is not checked, Country is not added in Itinerary Countries in misc", declaration.ZG_IsSecurityDeclarationInfo, errorMessage);
			}
		}
	}

	public void TestCheckZG_AuthorisationNumber_ListValidation()
	{
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForImport, permitHolder: orgHeader.PK, "999999", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
		declaration.JE_OH_Importer = orgHeader.PK;

		declaration.ZG_AuthorisationNumber = ZString.Empty;
		AssertNoMessageErrorContaining(declaration.ZG_AuthorisationNumberInfo, ListValidation.InvalidCodeMessageError);

		declaration.ZG_AuthorisationNumber = "123456";
		AssertHasMessageErrorContaining(declaration.ZG_AuthorisationNumberInfo, ListValidation.InvalidCodeMessageError);

		declaration.ZG_AuthorisationNumber = "999999";
		AssertNoMessageErrorContaining(declaration.ZG_AuthorisationNumberInfo, ListValidation.InvalidCodeMessageError);

		declaration.ZG_AuthorisationNumber = "888888";
		AssertHasMessageErrorContaining(declaration.ZG_AuthorisationNumberInfo, ListValidation.InvalidCodeMessageError);
	}

	public void TestCheckZG_AuthorisationNumber_MandatoryValidation()
	{
		var authorisationRequiredForColEntryInstructions = ValidationCaptions.JobDeclaration.AuthorisationIsRequiredForEntryInstructionsAtPlace;
		var authorisationMustBeEmptyForCodEntryInstructions = ValidationCaptions.JobDeclaration.AuthorisationMustBeEmptyForEntryInstructionsAtCustoms;

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		entryInstruction.CEI_Style = ZString.Empty;
		declaration.ZG_AuthorisationNumber = ZString.Empty;
		AssertNoMessageErrorContaining(declaration.ZG_AuthorisationNumberInfo, authorisationRequiredForColEntryInstructions);
		AssertNoMessageErrorContaining(declaration.ZG_AuthorisationNumberInfo, authorisationMustBeEmptyForCodEntryInstructions);

		entryInstruction.CEI_Style = SADDeclarationTypeList.Codes.ProceduraOrdinariaCOLuogo;
		declaration.ZG_AuthorisationNumber = ZString.Empty;
		AssertHasMessageErrorContaining(declaration.ZG_AuthorisationNumberInfo, authorisationRequiredForColEntryInstructions);
		AssertNoMessageErrorContaining(declaration.ZG_AuthorisationNumberInfo, authorisationMustBeEmptyForCodEntryInstructions);

		declaration.ZG_AuthorisationNumber = "AAA";
		AssertNoMessageErrorContaining(declaration.ZG_AuthorisationNumberInfo, authorisationRequiredForColEntryInstructions);
		AssertNoMessageErrorContaining(declaration.ZG_AuthorisationNumberInfo, authorisationMustBeEmptyForCodEntryInstructions);

		entryInstruction.CEI_Style = SADDeclarationTypeList.Codes.ProceduraOrdinariaCODogana;
		declaration.ZG_AuthorisationNumber = ZString.Empty;
		AssertNoMessageErrorContaining(declaration.ZG_AuthorisationNumberInfo, authorisationRequiredForColEntryInstructions);
		AssertNoMessageErrorContaining(declaration.ZG_AuthorisationNumberInfo, authorisationMustBeEmptyForCodEntryInstructions);

		declaration.ZG_AuthorisationNumber = "BBB";
		AssertNoMessageErrorContaining(declaration.ZG_AuthorisationNumberInfo, authorisationRequiredForColEntryInstructions);
		AssertHasMessageErrorContaining(declaration.ZG_AuthorisationNumberInfo, authorisationMustBeEmptyForCodEntryInstructions);
	}

	public void TestCheckZG_CTStatusID_ListValidation()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
		declaration.ZG_CTStatusID = ZString.Empty;
		AssertNoMessageErrorContaining("Empty is not an invalid code", declaration.ZG_CTStatusIDInfo, ListValidation.InvalidCodeMessageError.ToString());

		declaration.ZG_CTStatusID = "F";
		AssertHasMessageErrorContaining("Invalid code", declaration.ZG_CTStatusIDInfo, ListValidation.InvalidCodeMessageError.ToString());

		declaration.ZG_CTStatusID = ITExportCommunityTransitStatusList.Codes.T2L;
		AssertNoMessageErrorContaining("Valid code", declaration.ZG_CTStatusIDInfo, ListValidation.InvalidCodeMessageError.ToString());
	}

	public void TestCheckZG_TransportID_IMP()
	{
		declaration.JE_MessageType = MessageTypeList.Codes.Import;

		CombineAssertions("For IMP and isUCC6: True", () =>
		{
			declaration.JE_TransportMeans = ZString.Empty;
			declaration.AddInfoValidation.ValidateZG_Box18TransportID();
			AssertNoMessageErrorContaining("If transport code is empty too, no error expected", declaration.ZG_Box18TransportIDInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_TransportMeans = "XX";
			declaration.AddInfoValidation.ValidateZG_Box18TransportID();
			AssertHasMessageErrorContaining("If transport code is not empty, error is expected", declaration.ZG_Box18TransportIDInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestZG_AgreedPlacedCodeMandatoryValidationIsNotTriggered_Ucc6()
	{
		var validation = declaration.AddInfoValidation;
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		declaration.ZG_AgreedPlaceCode = ZString.Empty;
		declaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.Other;

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			validation.ValidateZG_AgreedPlaceCode();
			AssertNoMessageErrors("When UCC6, EXP and ZG_AgreedPlaceCode is Empty", declaration.ZG_AgreedPlaceCodeInfo);
		}

		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		validation.ValidateZG_AgreedPlaceCode();
		AssertNoMessageErrors("When IMP and ZG_AgreedPlaceCode is Empty", declaration.ZG_AgreedPlaceCodeInfo);
	}

	public void TestCheckZG_DeliveryTerms()
	{
		declaration.JE_MessageType = "EXP";
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			declaration.JE_ShipmentIncoTerm = "XXX";
			declaration.ZG_AdditionalDeliveryTerms = "";
			AssertHasMessageErrorContaining(declaration.ZG_AdditionalDeliveryTermsInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.ZG_AdditionalDeliveryTerms = "DELIVERY TERMS";
			AssertNoMessageErrorContaining(declaration.ZG_AdditionalDeliveryTermsInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_ShipmentIncoTerm = "EXW";
			declaration.ZG_AdditionalDeliveryTerms = "";
			AssertNoMessageErrorContaining(declaration.ZG_AdditionalDeliveryTermsInfo, MandatoryValidation.YouHaveNotEntered);
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			declaration.JE_ShipmentIncoTerm = "XXX";
			declaration.ZG_AdditionalDeliveryTerms = "";
			AssertNoMessageErrorContaining(declaration.ZG_AdditionalDeliveryTermsInfo, MandatoryValidation.YouHaveNotEntered);
		}

		declaration.JE_MessageType = "IMP";
		declaration.JE_ShipmentIncoTerm = "XXX";
		declaration.ZG_AdditionalDeliveryTerms = "";
		AssertNoMessageErrorContaining(declaration.ZG_AdditionalDeliveryTermsInfo, MandatoryValidation.YouHaveNotEntered);
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
	}

	JobDeclaration declaration;
}
