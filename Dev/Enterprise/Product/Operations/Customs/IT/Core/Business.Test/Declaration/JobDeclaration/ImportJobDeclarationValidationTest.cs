using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.IT.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class ImportJobDeclarationValidationTest : CommonJobDeclarationValidationTest
{
	public void TestCheckJE_RL_NKOrigin()
	{
		declaration.JE_RL_NKOrigin = "";
		AssertHasWarningContaining(declaration.JE_RL_NKOriginInfo, MandatoryValidation.YouHaveNotEntered);
		declaration.JE_RL_NKOrigin = "ITMIL";
		AssertNoWarningContaining(declaration.JE_RL_NKOriginInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckJE_RN_NKTransportNationality_BasedOnTransportModeAndDeclarationType()
	{
		SetUpRefCusProcedures();

		entryInstruction.CEI_Procedure = "71";

		declaration.JE_TransportMode = "SEA";
		declaration.JE_RN_NKTransportNationality = "";
		AssertNoMessageErrorContaining(declaration.JE_RN_NKTransportNationalityInfo, MandatoryValidation.YouHaveNotEntered);

		declaration.JE_TransportMode = "ROA";
		declaration.JE_RN_NKTransportNationality = "";
		AssertNoMessageErrorContaining(declaration.JE_RN_NKTransportNationalityInfo, MandatoryValidation.YouHaveNotEntered);

		declaration.JE_TransportMode = "AIR";
		declaration.JE_RN_NKTransportNationality = "";
		AssertNoMessageErrorContaining(declaration.JE_RN_NKTransportNationalityInfo, MandatoryValidation.YouHaveNotEntered);

		entryInstruction.CEI_Procedure = "40";
		declaration.JE_TransportMode = "RAI";
		declaration.JE_RN_NKTransportNationality = "";
		AssertNoMessageErrorContaining(declaration.JE_RN_NKTransportNationalityInfo, MandatoryValidation.YouHaveNotEntered);
		declaration.JE_TransportMode = "MAI";
		declaration.JE_RN_NKTransportNationality = "";
		AssertNoMessageErrorContaining(declaration.JE_RN_NKTransportNationalityInfo, MandatoryValidation.YouHaveNotEntered);
		declaration.JE_TransportMode = "FIX";
		declaration.JE_RN_NKTransportNationality = "";
		AssertNoMessageErrorContaining(declaration.JE_RN_NKTransportNationalityInfo, MandatoryValidation.YouHaveNotEntered);
		declaration.JE_TransportMode = "SEA";
		declaration.JE_RN_NKTransportNationality = "";
		AssertHasMessageErrorContaining(declaration.JE_RN_NKTransportNationalityInfo, MandatoryValidation.YouHaveNotEntered);
		declaration.JE_TransportMode = "ROA";
		declaration.JE_RN_NKTransportNationality = "";
		AssertHasMessageErrorContaining(declaration.JE_RN_NKTransportNationalityInfo, MandatoryValidation.YouHaveNotEntered);
		declaration.JE_RN_NKTransportNationality = "1";
		AssertNoMessageErrorContaining(declaration.JE_RN_NKTransportNationalityInfo, MandatoryValidation.YouHaveNotEntered);

		entryInstruction.CEI_Procedure = "71";
		declaration.JE_RN_NKTransportNationality = "";
		AssertNoMessageErrorContaining(declaration.JE_RN_NKTransportNationalityInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckJE_RL_NKFinalDestination()
	{
		SetUpRefCusProcedures();

		declaration.JE_MessageType = "IMP";
		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();

		entryInstruction.CEI_Procedure = "71";
		entryInstruction2.CEI_Procedure = "40";

		var expectedMissedDestinationMessageError = "You have not entered a Destination.";

		CombineAssertions("Final Destination Country", () =>
		{
			declaration.JE_RL_NKFinalDestination = ZString.Empty;
			AssertHasMessageError(declaration.JE_RL_NKFinalDestinationInfo, expectedMissedDestinationMessageError);

			declaration.JE_RL_NKFinalDestination = "IT";
			AssertNoMessageError(declaration.JE_RL_NKFinalDestinationInfo, expectedMissedDestinationMessageError);

			entryInstruction2.CEI_Procedure = "71";
			declaration.JE_RL_NKFinalDestination = ZString.Empty;
			AssertNoMessageError(declaration.JE_RL_NKFinalDestinationInfo, expectedMissedDestinationMessageError);
		});

		var expectedMissedProvinceMessageError = "Destination doesn't have a province (State).";

		var refCountryState = Factory.New<RefCountryStates>();
		refCountryState.RW_Code = "XX";
		refCountryState.RW_RN_NKCountryCode = "IT";

		var testUnloco = Factory.New<RefUNLOCO>();
		testUnloco.RL_RW = ZGuid.Empty;
		testUnloco.RL_Code = "ITXYZ";

		CombineAssertions("Final Destination Province", () =>
		{
			entryInstruction2.CEI_Procedure = "40";
			declaration.JE_RL_NKFinalDestination = "ITXYZ";
			AssertHasMessageError(declaration.JE_RL_NKFinalDestinationInfo, expectedMissedProvinceMessageError);

			testUnloco.RL_RW = refCountryState.PK;
			declaration.JE_RL_NKFinalDestination = "ITXYZ";
			AssertNoMessageError(declaration.JE_RL_NKFinalDestinationInfo, expectedMissedProvinceMessageError);

			declaration.JE_RL_NKFinalDestination = "IT";
			AssertNoMessageError(declaration.JE_RL_NKFinalDestinationInfo, expectedMissedProvinceMessageError);
		});
	}

	public void TestCheckJE_DefermentAccountNumberForPaymentParty1() => AssertJE_DefermentAccountNumberForPaymentParty("1", "Declarant", (OrgHeader owner) => declaration.JE_OA_DeclarantAddress = owner.MainAddress.PK, () => declaration.JE_OA_DeclarantAddress = ZGuid.Empty);

	public void TestCheckJE_DefermentAccountNumberForPaymentParty2() => AssertJE_DefermentAccountNumberForPaymentParty("2", "Importer", (OrgHeader owner) => declaration.JE_OH_Importer = owner.PK, () => declaration.JE_OA_ImporterAddress = ZGuid.Empty);

	public void TestCheckJE_DefermentAccountNumberForPaymentParty3() => AssertJE_DefermentAccountNumberForPaymentParty("3", "Forwarder", (OrgHeader owner) => declaration.JE_OH_Forwarder = owner.PK, () => declaration.JE_OH_Forwarder = ZGuid.Empty);

	public void TestCheckJE_DefermentAccountNumberForPaymentParty4() => AssertJE_DefermentAccountNumberForPaymentParty("4", "Representative", (OrgHeader owner) => declaration.JE_OA_Representative = owner.MainAddress.PK, () => declaration.JE_OA_Representative = ZGuid.Empty);

	public void TestCheckJE_RL_NKPortOfArrival()
	{
		var declaration = Factory.New<JobDeclaration>();
		var expectedMessageError = "Provide a port code to determine a port tax rate";
		declaration.JE_MessageType = "IMP";
		declaration.JE_TransportMode = "SEA";

		ITCustomsDataRegistry.Instance.ITGeneratePortTaxes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
		declaration.JE_RL_NKPortOfArrival = "";
		declaration.Validation.ValidateJE_RL_NKPortOfArrival();
		AssertNoWarningContaining(declaration.JE_RL_NKPortOfArrivalInfo, expectedMessageError);

		ITCustomsDataRegistry.Instance.ITGeneratePortTaxes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
		declaration.JE_RL_NKPortOfArrival = "";
		declaration.Validation.ValidateJE_RL_NKPortOfArrival();
		AssertHasWarningContaining(declaration.JE_RL_NKPortOfArrivalInfo, expectedMessageError);

		declaration.JE_RL_NKPortOfArrival = "ITVCE";
		declaration.Validation.ValidateJE_RL_NKPortOfArrival();
		AssertNoWarningContaining(declaration.JE_RL_NKPortOfArrivalInfo, expectedMessageError);

		declaration.JE_RL_NKPortOfArrival = "XXXX";
		declaration.Validation.ValidateJE_RL_NKPortOfArrival();
		AssertHasWarningContaining(declaration.JE_RL_NKPortOfArrivalInfo, expectedMessageError);

		declaration.JE_TransportMode = "AIR";
		declaration.JE_RL_NKPortOfArrival = "";
		declaration.Validation.ValidateJE_RL_NKPortOfArrival();
		AssertNoWarningContaining(declaration.JE_RL_NKPortOfArrivalInfo, expectedMessageError);
	}

	public void TestFinalDestinationValidationAddsWarningNotification()
	{
		AssertListValidationAddsWarningNotification(declaration.JE_RL_NKFinalDestinationInfo);
	}

	public void TestOriginValidationAddsWarningNotification()
	{
		AssertListValidationAddsWarningNotification(declaration.JE_RL_NKOriginInfo);

		CombineAssertions("Empty Origin", () =>
		{
			declaration.JE_RL_NKOrigin = "";
			AssertHasWarningContaining(declaration.JE_RL_NKOriginInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(declaration.JE_RL_NKOriginInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestPortOfArrivalValidationAddsWarningNotification()
	{
		AssertListValidationAddsWarningNotification(declaration.JE_RL_NKPortOfArrivalInfo);
	}

	public void TestPortOfFirstArrivalValidationAddsWarningNotification()
	{
		AssertListValidationAddsWarningNotification(declaration.JE_RL_NKPortOfFirstArrivalInfo);
	}

	public void TestPortOfLoadingValidationAddsWarningNotification()
	{
		AssertListValidationAddsWarningNotification(declaration.JE_RL_NKPortOfLoadingInfo);
	}

	public new void TestCheckJE_ShipmentIncoTerm()
	{
		var expectedMessageError = "This code is not valid for Italian Customs.";

		declaration.JE_ShipmentIncoTerm = "";
		AssertNoMessageErrorContaining(declaration.JE_ShipmentIncoTermInfo, expectedMessageError);

		declaration.JE_ShipmentIncoTerm = "CFR";
		AssertNoMessageErrorContaining(declaration.JE_ShipmentIncoTermInfo, expectedMessageError);

		declaration.JE_ShipmentIncoTerm = "FCA";
		AssertNoMessageErrorContaining(declaration.JE_ShipmentIncoTermInfo, expectedMessageError);

		declaration.JE_ShipmentIncoTerm = "FC1";
		AssertHasMessageErrorContaining(declaration.JE_ShipmentIncoTermInfo, expectedMessageError);

		declaration.JE_ShipmentIncoTerm = "FC2";
		AssertHasMessageErrorContaining(declaration.JE_ShipmentIncoTermInfo, expectedMessageError);
	}

	public void TestCheckJE_GoodsOriginFromIT()
	{
		var helper = new ITUniversalReferenceTestDataHelper(Factory);
		var grouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, parent: grouping);

		helper.CreateNewOrGetExistingCusCodeType("IM15", "List validation for origin country/territory for entry style IM");
		helper.CreateNewOrGetExistingCusCodeType("CO15", "List validation for origin country/territory for entry style CO");

		helper.CreateCusCodeList("IT", "IM15", "XA", "Test A", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		helper.CreateCusCodeList("IT", "IM15", "XC", "Test C", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

		Factory.Save();

		var dec = Factory.New<JobDeclaration>();
		dec.JE_EntryStyle = EntryStyleListImport.Codes.ImportNormal;
		dec.JE_MessageType = "IMP";
		dec.JE_GoodsOrigin = ZString.Empty;
		AssertHasMessageErrorContaining(dec.JE_GoodsOriginInfo, MandatoryValidation.YouHaveNotEntered + " a " + dec.JE_GoodsOriginInfo.HumanReadableName);
		dec.JE_GoodsOrigin = "XA";
		AssertNoMessageErrors(dec.JE_GoodsOriginInfo);
	}

	public void TestCheckJE_GoodsDestinationFromIT()
	{
		var helper = new ITUniversalReferenceTestDataHelper(Factory);
		var grouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, parent: grouping);

		helper.CreateNewOrGetExistingCusCodeType("IM17", "List validation for destination country/territory for entry style IM");
		helper.CreateNewOrGetExistingCusCodeType("CO17", "List validation for destination country/territory for entry style CO");

		helper.CreateCusCodeList("IT", "IM17", "XA", "Test A", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		helper.CreateCusCodeList("IT", "IM17", "XC", "Test C", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

		Factory.Save();

		var dec = Factory.New<JobDeclaration>();
		dec.JE_EntryStyle = EntryStyleListImport.Codes.ImportNormal;
		dec.JE_MessageType = "IMP";
		dec.JE_GoodsDestination = ZString.Empty;
		dec.Validation.ValidateJE_GoodsDestination();
		AssertHasMessageErrorContaining(dec.JE_GoodsDestinationInfo, MandatoryValidation.YouHaveNotEntered + " a " + dec.JE_GoodsDestinationInfo.HumanReadableName);

		dec.JE_GoodsDestination = "XA";
		dec.Validation.ValidateJE_GoodsDestination();
		AssertNoMessageErrors(dec.JE_GoodsDestinationInfo);
	}

	public override void TestCheckJE_OA_DeclarantAddress()
	{
		base.TestCheckJE_OA_DeclarantAddress();

		var expectedMessage = "EORI code for Declarant is mandatory: please press F3, go to details -> config and fill a 'Registration Number / code' with Type ='EOR'";
		var declarant = Factory.NewWithValidTestData<OrgHeader>();
		declarant.MainAddress.OA_RN_NKCountryCode = "IT";
		var declarantAddress = Factory.NewWithValidTestData<OrgAddress>();
		declarantAddress.OA_OH = declarant.PK;

		AssertEquals("Pre: for MessageType = IMP, IsUCC6", true, declaration.Configuration.IsUCC6(declaration));

		declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
		AssertNoMessageErrorContaining("When Declarant is empty", declaration.JE_OA_DeclarantAddressInfo, expectedMessage);

		declaration.JE_OA_DeclarantAddress = declarantAddress.PK;
		AssertHasMessageErrorContaining("When no Customs Code added", declaration.JE_OA_DeclarantAddressInfo, expectedMessage);

		declarant.CustomsCodes.AddNew("EOR", "EOR CODE", "IT");
		var importJobDeclarationValidation = declaration.Validation as ImportJobDeclarationValidation;
		AssertNotNull("When MessageType = IMP, Validation", importJobDeclarationValidation);

		importJobDeclarationValidation.ValidateJE_OA_DeclarantAddress();
		AssertNoMessageErrorContaining("When Customs Code added", declaration.JE_OA_DeclarantAddressInfo, expectedMessage);

		declarant.CustomsCodes.RemoveAndDeleteAll();
		declarant.CustomsCodes.AddNew("EOR", "", "IT");
		importJobDeclarationValidation.ValidateJE_OA_DeclarantAddress();
		AssertHasMessageErrorContaining("When EOR Code added without number", declaration.JE_OA_DeclarantAddressInfo, expectedMessage);

		declaration.JE_MessageType = "EXP";
		AssertEquals("For MessageType = EXP, IsUCC6", false, declaration.Configuration.IsUCC6(declaration));

		declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
		AssertNoMessageErrorContaining("When Declarant is empty and MessageType = EXP", declaration.JE_OA_DeclarantAddressInfo, expectedMessage);

		declaration.JE_OA_DeclarantAddress = declarantAddress.PK;
		AssertNoMessageErrorContaining("When no Customs Code added and MessageType = EXP", declaration.JE_OA_DeclarantAddressInfo, expectedMessage);

		declarant.CustomsCodes.AddNew("EOR", "EOR CODE", "IT");
		importJobDeclarationValidation.ValidateJE_OA_DeclarantAddress();
		AssertNoMessageErrorContaining("When Customs Code added  and MessageType = EXP", declaration.JE_OA_DeclarantAddressInfo, expectedMessage);

		declarant.CustomsCodes.RemoveAndDeleteAll();
		declarant.CustomsCodes.AddNew("EOR", "", "IT");
		importJobDeclarationValidation.ValidateJE_OA_DeclarantAddress();
		AssertNoMessageErrorContaining("When EOR Code added without number", declaration.JE_OA_DeclarantAddressInfo, expectedMessage);
	}

	public void TestCheckJE_OA_RepresentativeAddress()
	{
		var representative = Factory.NewWithValidTestData<OrgHeader>();
		declaration.JE_OA_Representative = representative.MainAddress.PK;
		AssertOrganizationAddress(representative, () => declaration.Validation.ValidateJE_OA_Representative(), declaration.JE_OA_RepresentativeInfo);
	}

	public void TestCheckJE_OA_SellerAddress()
	{
		var seller = Factory.NewWithValidTestData<OrgHeader>();
		declaration.JE_OA_SellerAddress = seller.MainAddress.PK;
		AssertOrganizationAddress(seller, () => declaration.Validation.ValidateJE_OA_SellerAddress(), declaration.JE_OA_SellerAddressInfo);
	}

	public void TestCheckJE_OH_Buyer()
	{
		var buyer = Factory.NewWithValidTestData<OrgHeader>();
		declaration.JE_OH_Buyer = buyer.PK;
		AssertOrganizationAddress(buyer, () => declaration.Validation.ValidateJE_OH_Buyer(), declaration.JE_OH_BuyerInfo);
	}

	void AssertOrganizationAddress(OrgHeader organization, Action validateMethod, ZPropertyInfo propertyInfo)
	{
		var mainAddress = organization.MainAddress;
		var organizationName = propertyInfo.HumanReadableName;

		declaration.JE_MessageType = "IMP";

		var expectedAddressWarningMessage = $"{organizationName} Address is longer than 70 characters, it will be truncated in the message.";
		mainAddress.OA_Address1 = "".PadRight(35, 'A');
		validateMethod();
		AssertNoWarningContaining(propertyInfo, expectedAddressWarningMessage);

		mainAddress.OA_Address2 = "".PadRight(36, 'A');

		validateMethod();
		AssertHasWarningContaining(propertyInfo, expectedAddressWarningMessage);

		var expectedCityWarningMessage = $"{organizationName} City is longer than 35 characters, it will be truncated in the message.";
		mainAddress.OA_City = "Milan";
		validateMethod();
		AssertNoWarningContaining(propertyInfo, expectedCityWarningMessage);

		mainAddress.OA_City = "Llanfairpwllgwyngyllgogerychwyrndrobwllllantysilio";
		validateMethod();
		AssertHasWarningContaining(propertyInfo, expectedCityWarningMessage);

		organization.OH_FullName = "".PadRight(75, 'A');
		validateMethod();
		var expectedCompanyNameWarningMessageForImport = $"{organizationName} Company Name is longer than 70 characters, it will be truncated in the message.";
		AssertHasWarningContaining(propertyInfo, expectedCompanyNameWarningMessageForImport);

		organization.OH_FullName = "SHORT";
		validateMethod();
		AssertNoWarningContaining(propertyInfo, expectedCompanyNameWarningMessageForImport);

		mainAddress.OA_PostCode = "1234567890";
		validateMethod();
		var expectedPostCodeWarningMessageForImport = $"{organizationName} Postcode is longer than 9 characters, it will be truncated in the message.";
		AssertHasWarningContaining(propertyInfo, expectedPostCodeWarningMessageForImport);

		mainAddress.OA_PostCode = "20154";
		validateMethod();
		AssertNoWarningContaining(propertyInfo, expectedPostCodeWarningMessageForImport);
	}

	public void TestCheckJE_LocationQualifierNotLB_WithAuthorizationTypeChange()
	{
		var expectedMessage = "The selected Authorization requires a Location qualifier of type LB";
		var organisation = Factory.NewWithValidTestData<OrgHeader>();
		SetupAuthorisationsForOrg(organisation);

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.JE_OH_Importer = organisation.PK;

		declaration.ZG_AuthorisationNumber = "1111CWP";
		declaration.JE_LocationQualifier = "XY";
		AssertHasMessageErrorContaining("When LocationQualifier is not LB and AuthorisationType=CWP", declaration.JE_LocationQualifierInfo, expectedMessage);
		declaration.JE_LocationQualifier = "LB";
		AssertNoMessageErrorContaining("When LocationQualifier is LB and AuthorisationType=CWP", declaration.JE_LocationQualifierInfo, expectedMessage);

		declaration.ZG_AuthorisationNumber = "1111CW1";
		declaration.JE_LocationQualifier = "XY";
		AssertHasMessageErrorContaining("When LocationQualifier is not LB and AuthorisationType=CW1", declaration.JE_LocationQualifierInfo, expectedMessage);
		declaration.JE_LocationQualifier = "LB";
		AssertNoMessageErrorContaining("When LocationQualifier is LB and AuthorisationType=CW1", declaration.JE_LocationQualifierInfo, expectedMessage);

		declaration.ZG_AuthorisationNumber = "1111CW2";
		declaration.JE_LocationQualifier = "XY";
		AssertHasMessageErrorContaining("When LocationQualifier is not LB and AuthorisationType=CW2", declaration.JE_LocationQualifierInfo, expectedMessage);
		declaration.JE_LocationQualifier = "LB";
		AssertNoMessageErrorContaining("When LocationQualifier is LB and AuthorisationType=CW2", declaration.JE_LocationQualifierInfo, expectedMessage);

		declaration.ZG_AuthorisationNumber = "SomeXYZ";
		AssertNull("Pre: When AuthorisationNumber is invalid", declaration.Authorization);
		declaration.JE_LocationQualifier = "XY";
		AssertNoMessageErrorContaining("When Authorisation is null, No validation related to AuthorisationType", declaration.JE_LocationQualifierInfo, expectedMessage);
	}

	public void TestCheckJE_LocationQualifierNotLC_WithAuthorizationTypeChange()
	{
		var expectedMessage = "The selected Authorization requires a Location qualifier of type LC";
		var organisation = Factory.NewWithValidTestData<OrgHeader>();
		SetupAuthorisationsForOrg(organisation);

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.JE_OH_Importer = organisation.PK;

		declaration.ZG_AuthorisationNumber = "1111ALI";
		declaration.JE_LocationQualifier = "XY";
		AssertHasMessageErrorContaining("When LocationQualifier is not LC and AuthorisationType=ALI", declaration.JE_LocationQualifierInfo, expectedMessage);
		declaration.JE_LocationQualifier = "LC";
		AssertNoMessageErrorContaining("When LocationQualifier is LC and AuthorisationType=ALI", declaration.JE_LocationQualifierInfo, expectedMessage);

		declaration.ZG_AuthorisationNumber = "1111ALE";
		declaration.JE_LocationQualifier = "XY";
		AssertHasMessageErrorContaining("When LocationQualifier is not LC and AuthorisationType=ALE", declaration.JE_LocationQualifierInfo, expectedMessage);
		declaration.JE_LocationQualifier = "LC";
		AssertNoMessageErrorContaining("When LocationQualifier is LC and AuthorisationType=ALE", declaration.JE_LocationQualifierInfo, expectedMessage);

		declaration.ZG_AuthorisationNumber = "SomeXYZ";
		AssertNull("Pre: When AuthorisationNumber is invalid", declaration.Authorization);
		declaration.JE_LocationQualifier = "XY";
		AssertNoMessageErrorContaining("When Authorisation is null, No validation related to AuthorisationType", declaration.JE_LocationQualifierInfo, expectedMessage);
	}

	public void TestCheckJE_OA_RepresentativeMandatory()
	{
		CombineAssertions("When Representative is empty", () =>
		{
			declaration.JE_DeclarantType = "DIR";
			declaration.Validation.ValidateJE_OA_Representative();
			AssertHasMessageErrorContaining("When DeclarantType = DIR", declaration.JE_OA_RepresentativeInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_DeclarantType = "IND";
			declaration.Validation.ValidateJE_OA_Representative();
			AssertHasMessageErrorContaining("When DeclarantType = DIR", declaration.JE_OA_RepresentativeInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_DeclarantType = "XYZ";
			declaration.Validation.ValidateJE_OA_Representative();
			AssertHasMessageErrorContaining("When DeclarantType invalid", declaration.JE_OA_RepresentativeInfo, MandatoryValidation.YouHaveNotEntered);
		});

		CombineAssertions("When Representative is filled", () =>
		{
			var representative = Factory.NewWithValidTestData<OrgHeader>();
			representative.MainAddress.OA_RN_NKCountryCode = "IT";
			var representativeAddress = Factory.NewWithValidTestData<OrgAddress>();
			representativeAddress.OA_OH = representative.PK;
			declaration.JE_OA_Representative = representativeAddress.PK;

			declaration.JE_DeclarantType = "SEL";
			declaration.Validation.ValidateJE_OA_Representative();
			AssertNoMessageErrorContaining("When DeclarantType = SEL and Representative is filled", declaration.JE_OA_RepresentativeInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_DeclarantType = "DIR";
			declaration.Validation.ValidateJE_OA_Representative();
			AssertNoMessageErrorContaining("When DeclarantType = DIR and Representative is filled", declaration.JE_OA_RepresentativeInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_DeclarantType = "IND";
			declaration.Validation.ValidateJE_OA_Representative();
			AssertNoMessageErrorContaining("When DeclarantType = DIR and Representative is filled", declaration.JE_OA_RepresentativeInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_DeclarantType = "XYZ";
			declaration.Validation.ValidateJE_OA_Representative();
			AssertNoMessageErrorContaining("When DeclarantType invalid and Representative is filled", declaration.JE_OA_RepresentativeInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckJE_OA_RepresentativeForEORICode()
	{
		var expectedMessage = "EORI code for Representative is mandatory: please press F3, go to details -> config and fill a 'Registration Number / code' with Type ='EOR'";
		var representative = Factory.NewWithValidTestData<OrgHeader>();
		representative.MainAddress.OA_RN_NKCountryCode = "IT";
		var representativeAddress = Factory.NewWithValidTestData<OrgAddress>();
		representativeAddress.OA_OH = representative.PK;
		declaration.JE_OA_Representative = representativeAddress.PK;

		CombineAssertions("When Representative not mandatory", () =>
		{
			declaration.JE_DeclarantType = "SEL";

			declaration.Validation.ValidateJE_OA_Representative();
			AssertNoMessageErrorContaining("When EORI code not present", declaration.JE_OA_RepresentativeInfo, expectedMessage);

			representative.CustomsCodes.AddNew("EOR", "EOR CODE", "IT");
			declaration.Validation.ValidateJE_OA_Representative();
			AssertNoMessageErrorContaining("When EORI code present", declaration.JE_OA_RepresentativeInfo, expectedMessage);
		});

		representative.CustomsCodes.RemoveAndDeleteAll();
		CombineAssertions("When Representative is mandatory", () =>
		{
			declaration.JE_DeclarantType = "DIR";

			declaration.Validation.ValidateJE_OA_Representative();
			AssertHasMessageErrorContaining("When EORI code not present", declaration.JE_OA_RepresentativeInfo, expectedMessage);

			representative.CustomsCodes.AddNew("EOR", "EOR CODE", "IT");
			declaration.Validation.ValidateJE_OA_Representative();
			AssertNoMessageErrorContaining("When EORI code present", declaration.JE_OA_RepresentativeInfo, expectedMessage);
		});
	}

	public void TestCheckJE_LocationQualifier()
	{
		var expectedMessage = "You have not entered a Goods Location qualifier";
		var organisation = Factory.NewWithValidTestData<OrgHeader>();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.JE_OH_Importer = organisation.PK;

		CombineAssertions("When Authorisation empty", () =>
		{
			AssertHasMessageErrorContaining("When LocationQualifier is empty", declaration.JE_LocationQualifierInfo, expectedMessage);
			declaration.JE_LocationQualifier = "D";
			AssertNoMessageErrorContaining("When LocationQualifier is filled", declaration.JE_LocationQualifierInfo, expectedMessage);
		});

		CombineAssertions("When Authorisation filled", () =>
		{
			declaration.ZG_AuthorisationNumber = "SomeXYZ";
			AssertNull("Pre: When AuthorisationNumber is invalid", declaration.Authorization);
			declaration.JE_LocationQualifier = "";
			AssertHasMessageErrorContaining("When Authorisation is null(not in list) and LocationQualifier is empty", declaration.JE_LocationQualifierInfo, expectedMessage);

			declaration.JE_LocationQualifier = "XY";
			AssertNoMessageErrorContaining("When Authorisation is null(not in list) and LocationQualifier is filled", declaration.JE_LocationQualifierInfo, expectedMessage);
		});
	}

	public void TestZG_AgreedPlaceCodeValidation_NeverRequired()
	{
		CombineAssertions("Empty ZG_AgreedPlaceCode validation never triggered in Import declaration", () =>
		{
			declaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.Other;
			declaration.AddInfoValidation.ValidateZG_AgreedPlaceCode();
			AssertNoMessageErrors("When JE_ShipmentIncoTerm is XXX (Other)", declaration.ZG_AgreedPlaceCodeInfo);

			declaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			declaration.AddInfoValidation.ValidateZG_AgreedPlaceCode();
			AssertNoNotifications("When JE_ShipmentIncoTerm is not XXX (Other)", declaration.ZG_AgreedPlaceCodeInfo);
		});
	}

	public void TestEUD_AgreedPlaceCodeValidation_IsRequired()
	{
		const string messageError = "Incoterm Place Code or Country Code is required";

		CombineAssertions("Empty EUD_AgreedPlaceCode validation always triggered in Import declaration", () =>
		{
			declaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.CarriageAndInsurancePaidTo;
			declaration.AddInfoChildValidation.ValidateEUD_AgreedPlaceCode();
			AssertHasMessageError("When JE_ShipmentIncoTerm is not XXX (Other)", declaration.EUD_AgreedPlaceCodeInfo, messageError);

			declaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.Other;
			declaration.AddInfoChildValidation.ValidateEUD_AgreedPlaceCode();
			AssertHasNotifications("When JE_ShipmentIncoTerm is XXX (Other)", declaration.EUD_AgreedPlaceCodeInfo);
		});
	}

	public void TestJE_ShipmentIncoTermMandatoryValidation()
	{
		const string messageError = "You have not entered a [20.1] Incoterm.";

		CombineAssertions("JE_ShipmentIncoTerm mandatory validation", () =>
		{
			declaration.JE_ShipmentIncoTerm = string.Empty;
			declaration.Validation.ValidateJE_ShipmentIncoTerm();
			AssertHasMessageError("When JE_ShipmentIncoTerm is empty, warning message expected", declaration.JE_ShipmentIncoTermInfo, messageError);

			declaration.JE_ShipmentIncoTerm = "XXX";
			declaration.Validation.ValidateJE_ShipmentIncoTerm();
			AssertNoMessageErrorContaining("When JE_ShipmentIncoTerm is filled, no warning message expected", declaration.JE_ShipmentIncoTermInfo, messageError);
		});
	}

	public void TestCheckJE_TransportMeans_IMP()
	{
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		declaration.JE_TransportModeInland = TransportTypeList.Codes.Air;
		var cusEntryInstruction = declaration.CustomsEntryInstructions.AddNew();

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			CombineAssertions("For IMP and isUCC6:True, AND ZG_Box18TransportID Filled", () =>
			{
				declaration.ZG_Box18TransportID = "FILLED";
				cusEntryInstruction.CEI_Style = ImportUCC6DeclarationTypeList.Codes.ImmissioneLiberaPraticaH1;
				declaration.JE_TransportMeans = "";
				AssertHasMessageErrorContaining("Code not entered", declaration.JE_TransportMeansInfo, MandatoryValidation.YouHaveNotEntered);
				cusEntryInstruction.CEI_Style = ImportUCC6DeclarationTypeList.Codes.RegimeSpecialeAmmissioneTemporaneaH3;
				declaration.JE_TransportMeans = "";
				AssertHasMessageErrorContaining("Code not entered", declaration.JE_TransportMeansInfo, MandatoryValidation.YouHaveNotEntered);
				cusEntryInstruction.CEI_Style = ImportUCC6DeclarationTypeList.Codes.RegimeSpecialePerfezionamentoAttivoH4;
				declaration.JE_TransportMeans = "";
				AssertHasMessageErrorContaining("Code not entered", declaration.JE_TransportMeansInfo, MandatoryValidation.YouHaveNotEntered);
				cusEntryInstruction.CEI_Style = ImportUCC6DeclarationTypeList.Codes.DichiarazioneScambiTerritoriFiscaliSpecialiH5;
				declaration.JE_TransportMeans = "";
				AssertHasMessageErrorContaining("Code not entered", declaration.JE_TransportMeansInfo, MandatoryValidation.YouHaveNotEntered);

				cusEntryInstruction.CEI_Style = "IFD";
				declaration.JE_TransportMeans = "";
				AssertNoMessageErrorContaining("No Error Message", declaration.JE_TransportMeansInfo, MandatoryValidation.YouHaveNotEntered);

				cusEntryInstruction.CEI_Style = ImportUCC6DeclarationTypeList.Codes.ImmissioneLiberaPraticaH1;
				declaration.JE_TransportMeans = "40";
				AssertNoMessageErrorContaining("No Error Message", declaration.JE_TransportMeansInfo, MandatoryValidation.YouHaveNotEntered);
			});

			CombineAssertions("For IMP and isUCC6:True, ZG_Box18TransportID Empty", () =>
			{
				declaration.ZG_Box18TransportID = string.Empty;
				cusEntryInstruction.CEI_Style = ImportUCC6DeclarationTypeList.Codes.ImmissioneLiberaPraticaH1;
				declaration.JE_TransportMeans = "";
				AssertNoMessageErrorContaining("Code not entered", declaration.JE_TransportMeansInfo, MandatoryValidation.YouHaveNotEntered);
				cusEntryInstruction.CEI_Style = ImportUCC6DeclarationTypeList.Codes.RegimeSpecialeAmmissioneTemporaneaH3;
				declaration.JE_TransportMeans = "";
				AssertNoMessageErrorContaining("Code not entered", declaration.JE_TransportMeansInfo, MandatoryValidation.YouHaveNotEntered);
				cusEntryInstruction.CEI_Style = ImportUCC6DeclarationTypeList.Codes.RegimeSpecialePerfezionamentoAttivoH4;
				declaration.JE_TransportMeans = "";
				AssertNoMessageErrorContaining("Code not entered", declaration.JE_TransportMeansInfo, MandatoryValidation.YouHaveNotEntered);
				cusEntryInstruction.CEI_Style = ImportUCC6DeclarationTypeList.Codes.DichiarazioneScambiTerritoriFiscaliSpecialiH5;
				declaration.JE_TransportMeans = "";
				AssertNoMessageErrorContaining("Code not entered", declaration.JE_TransportMeansInfo, MandatoryValidation.YouHaveNotEntered);

				cusEntryInstruction.CEI_Style = "IFD";
				declaration.JE_TransportMeans = "";
				AssertNoMessageErrorContaining("Code not entered", declaration.JE_TransportMeansInfo, MandatoryValidation.YouHaveNotEntered);

				cusEntryInstruction.CEI_Style = ImportUCC6DeclarationTypeList.Codes.ImmissioneLiberaPraticaH1;
				declaration.JE_TransportMeans = "40";
				AssertNoMessageErrorContaining("Code not entered", declaration.JE_TransportMeansInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}
	}

	void AssertJE_DefermentAccountNumberForPaymentParty(string paymentParty, string ownerNameForMessages, Action<OrgHeader> setOwner, Action clearOwner)
	{
		CombineAssertions($"For Payment Party = {paymentParty}", () =>
		{
			var expectedMessage = $"For Payment Party = {paymentParty} an Entry Instruction > Authorization must be present, with Code = DPO and Owner = {ownerNameForMessages}";
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();

			setOwner(orgHeader1);
			declaration.JE_PaymentMethod = paymentParty;
			declaration.JE_DefermentAccountNumber = "12345678A";
			var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages[0];
			AssertEquals($"Pre-Conditon:CusAuthorization owner and {ownerNameForMessages}", orgHeader1.PK, cusAuthorizationUsage.Owner.PK);
			AssertNoMessageErrorContaining($"When DPO Authorization with {ownerNameForMessages} present", declaration.JE_DefermentAccountNumberInfo, expectedMessage);

			entryInstruction.CusAuthorizationUsages.RemoveAndDeleteAll();
			declaration.Validation.ValidateJE_DefermentAccountNumber();
			AssertHasMessageErrorContaining($"When DPO Authorization with {ownerNameForMessages} - entry missing", declaration.JE_DefermentAccountNumberInfo, expectedMessage);

			cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			cusAuthorizationUsage.AGC_OH_Owner = orgHeader2.PK;
			cusAuthorizationUsage.AGC_Code = "DPO";
			declaration.Validation.ValidateJE_DefermentAccountNumber();
			AssertHasMessageErrorContaining($"When DPO Authorization with {ownerNameForMessages} and owner different", declaration.JE_DefermentAccountNumberInfo, expectedMessage);

			clearOwner();
			declaration.JE_DefermentAccountNumber = "12345678A";
			entryInstruction.CusAuthorizationUsages.RemoveAndDeleteAll();
			cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			cusAuthorizationUsage.AGC_Code = "DPO";
			cusAuthorizationUsage.AGC_OH_Owner = ZGuid.Empty;
			declaration.Validation.ValidateJE_DefermentAccountNumber();
			AssertHasMessageErrorContaining($"When DPO Authorization with {ownerNameForMessages} and owner are empty", declaration.JE_DefermentAccountNumberInfo, expectedMessage);
		});
	}

	void SetupAuthorisationsForOrg(OrgHeader organisation)
	{
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, permitHolder: organisation.PK, "1111CWP", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, permitHolder: organisation.PK, "1111CW1", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, permitHolder: organisation.PK, "1111CW2", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForImport, permitHolder: organisation.PK, "1111ALI", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForExport, permitHolder: organisation.PK, "1111ALE", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));
	}

	void SetUpRefCusProcedures()
	{
		new ITUniversalReferenceTestDataHelper(Factory).CreateRefCusProcedure40And71ForCurrentCountry();
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
	}

	new JobDeclaration declaration;
	CusEntryInstruction entryInstruction;
}
