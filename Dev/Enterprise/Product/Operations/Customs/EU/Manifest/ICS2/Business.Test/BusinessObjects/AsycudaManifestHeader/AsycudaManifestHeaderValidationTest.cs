using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class AsycudaManifestHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAMA_Voyage()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			CombineAssertions(() =>
			{
				manifestHeader.AMA_TransportMode = TransportModes.Air;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(manifestHeader.AMA_VoyageInfo);

				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F44;
				ValidationTestHelper.AssertFieldIsNotMandatory(manifestHeader.AMA_VoyageInfo);
				manifestHeader.SpecificCircumstanceIndicator = ZString.Empty;

				manifestHeader.AMA_TransportMode = TransportModes.Sea;
				ValidationTestHelper.AssertFieldIsNotMandatory(manifestHeader.AMA_VoyageInfo);

				manifestHeader.AMA_TransportMode = TransportModes.InlandWaterwayTransport;
				ValidationTestHelper.AssertFieldIsNotMandatory(manifestHeader.AMA_VoyageInfo);
			});
		}

		public void TestCarrierAndShippingAgentEORIEntered()
		{
			var orgCarrier = Factory.NewWithValidTestData<OrgHeader>();
			var orgShipping = Factory.NewWithValidTestData<OrgHeader>();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.RunPreSaveValidation();

			CombineAssertions("pre-condition", () =>
			{
				AssertNull("AMA_OA_Carrier", header.Carrier);
				AssertNull("AMA_OA_ShippingAgent", header.ShippingAgent);
			});

			header.AMA_OA_Carrier = orgCarrier.MainAddress.PK;

			AssertHasMessageError(header.AMA_OA_CarrierInfo, "Carrier must have EORI entered");
			AssertNoMessageError(header.AMA_OA_ShippingAgentInfo, "Shipping Agent must have EORI entered");

			orgCarrier.MainAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "654321", CountryCodes.Germany);
			orgCarrier.MainAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456", CountryCodes.UnitedKingdom);
			header.RunPreSaveValidation();

			AssertNoMessageError(header.AMA_OA_CarrierInfo, "Carrier must have EORI entered");

			header.AMA_OA_ShippingAgent = orgShipping.MainAddress.PK;

			AssertHasMessageError(header.AMA_OA_ShippingAgentInfo, "Shipping Agent must have EORI entered");

			orgShipping.MainAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456", CountryCodes.Germany);
			orgShipping.MainAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "654321", CountryCodes.UnitedKingdom);
			header.RunPreSaveValidation();

			AssertNoMessageError(header.AMA_OA_ShippingAgentInfo, "Shipping Agent must have EORI entered");

			orgCarrier.MainAddress.CustomsCodes.RemoveAllFromRelationship();

			AssertNoMessageError(header.AMA_OA_ShippingAgentInfo, "Shipping Agent must have EORI entered");
		}

		public void TestDeclarantEntered()
		{
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			var header = Factory.New<AsycudaManifestHeader>();

			header.AMA_OA_Declarant = ZGuid.Empty;

			AssertNull("pre-condition", header.Declarant);
			AssertHasMessageError(header.AMA_OA_DeclarantInfo, "You have not entered a Declarant.");

			header.AMA_OA_Declarant = declarant.MainAddress.PK;

			AssertNoMessageError(header.AMA_OA_DeclarantInfo, "You have not entered a Declarant.");
		}

		public void TestCheckDeclarantHasEori()
		{
			var orgDeclarant = Factory.NewWithValidTestData<OrgHeader>();
			var header = Factory.New<AsycudaManifestHeader>();
			header.RunPreSaveValidation();

			header.AMA_OA_Declarant = orgDeclarant.MainAddress.PK;

			AssertHasMessageError(header.AMA_OA_DeclarantInfo, "Declarant must have EORI entered.");

			orgDeclarant.MainAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "654321", CountryCodes.Germany);
			orgDeclarant.MainAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456", CountryCodes.UnitedKingdom);

			header.Validation.ValidateAMA_OA_Declarant();

			AssertNoMessageError(header.AMA_OA_DeclarantInfo, "Declarant must have EORI entered.");
		}

		public void TestCarrierEntered()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			ValidationTestHelper.AssertFieldIsMandatory(header.AMA_OA_CarrierInfo, "You have not entered a Carrier.");

			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F44;
			ValidationTestHelper.AssertFieldIsNotMandatory(header.AMA_OA_CarrierInfo);
		}

		public void TestCheckAMA_OA_ShippingAgent_ValidPhoneNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var address = GetOrgAddress();
			header.AMA_OA_ShippingAgent = address.PK;
			Factory.Save();
			header.Validation.ValidateAMA_OA_ShippingAgent();
			AssertNoMessageErrorContaining(header.AMA_OA_ShippingAgentInfo, "Phone Number of Shipping Agent is in an invalid format. The phone number is required to start with + and have 6 to 14 characters (digits and spaces) E.g. +14155552671, +1 415 555 2671, +44 7911 123456, +44 7911 123 456, +33 6 12 34 56 78, +33123456789");

			address.OA_Phone = "123456";
			Factory.Save();
			header.Validation.ValidateAMA_OA_ShippingAgent();
			AssertHasMessageError(header.AMA_OA_ShippingAgentInfo, "Phone Number of Shipping Agent is in an invalid format. The phone number is required to start with + and have 6 to 14 characters (digits and spaces) E.g. +14155552671, +1 415 555 2671, +44 7911 123456, +44 7911 123 456, +33 6 12 34 56 78, +33123456789");
		}

		public void TestCheckAMA_OA_ShippingAgent_HasTelOrEmail()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var address = GetOrgAddress();
			header.AMA_OA_ShippingAgent = address.PK;
			Factory.Save();
			AssertNoMessageErrorContaining(header.AMA_OA_ShippingAgentInfo, "The Organization must have either a Phone or Email recorded against it.");

			address.OA_Phone = string.Empty;
			address.OA_Email = string.Empty;
			Factory.Save();
			header.Validation.ValidateAMA_OA_ShippingAgent();
			AssertHasMessageErrorContaining(header.AMA_OA_ShippingAgentInfo, "The Organization must have either a Phone or Email recorded against it.");
		}

		public void TestCheckSpecificCircumstanceIndicator_InvalidCode()
		{
			manifestHeader.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
			manifestHeader.AMA_TransportMode = TransportModes.Air;
			ValidationTestHelper.AssertInvalidCodeMessageError(manifestHeader.SpecificCircumstanceIndicatorInfo, "AAA", EUICS2SpecificCircumstanceList.Codes.F24);
		}

		public void TestCheckSpecificCircumstanceIndicator_Mandatory()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(manifestHeader.SpecificCircumstanceIndicatorInfo);
		}

		public void TestCheckAMA_TransportMeans()
		{
			ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyHasValue(manifestHeader.AMA_TransportMeansInfo, manifestHeader.AMA_TransportModeInfo, new IZType[]
			{
				(ZString)TransportModes.Road,
				(ZString)TransportModes.Rail,
			});

			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F44;
			ValidationTestHelper.AssertFieldIsNotMandatory(manifestHeader.AMA_TransportMeansInfo);
		}

		public void TestCheckAMA_TransportMeans_ListValidation()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(manifestHeader.AMA_TransportModeInfo, new ZString[] { "X", "Y" }, manifestHeader.Lookups.MeansOfTransportTypeList.GetAllCodesZString());
		}

		public void TestCheckSpecificCircumstanceIndicator_ShowMessageErrorWhenValidationFailed()
		{
			manifestHeader.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			manifestHeader.AMA_TransportMode = TransportModes.Sea;
			CombineAssertions(() =>
			{
				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F41;
				AssertHasMessageError("Invalid Transport and Specific Circumsatance combination", manifestHeader.SpecificCircumstanceIndicatorInfo, SpecificCircumstanceIndicatorMessageError);
				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F10;
				AssertNoMessageError("Valid Transport and Specific Circumsatance combination", manifestHeader.SpecificCircumstanceIndicatorInfo, SpecificCircumstanceIndicatorMessageError);
			});
		}

		public void TestCheckSpecificCircumstanceIndicator_ShowMessageErrorWhenItineraryLessThanTwo()
		{
			manifestHeader.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
			manifestHeader.AMA_TransportMode = TransportModes.Air;

			const string expectedError = "The selected ICS2 submission may fail, at least 2 records on Itinerary Tab are required.";

			CombineAssertions(() =>
			{
				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F22;
				AssertHasMessageError("Show error when itinerary records less than 2.", manifestHeader.SpecificCircumstanceIndicatorInfo, expectedError);
				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F26;
				AssertHasMessageError("Show error when itinerary records less than 2.", manifestHeader.SpecificCircumstanceIndicatorInfo, expectedError);
				manifestHeader.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F40;
				AssertHasMessageError("Show error when itinerary records less than 2.", manifestHeader.SpecificCircumstanceIndicatorInfo, expectedError);
				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F50;
				AssertHasMessageError("Show error when itinerary records less than 2.", manifestHeader.SpecificCircumstanceIndicatorInfo, expectedError);
				manifestHeader.AMA_TransportMode = TransportModes.Sea;
				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F14;
				AssertHasMessageError("Show error when itinerary records less than 2.", manifestHeader.SpecificCircumstanceIndicatorInfo, expectedError);
				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F15;
				AssertHasMessageError("Show error when itinerary records less than 2.", manifestHeader.SpecificCircumstanceIndicatorInfo, expectedError);

				manifestHeader.Itinerary.AddNew();
				manifestHeader.Itinerary.AddNew();
				manifestHeader.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F22;
				AssertNoMessageError(manifestHeader.SpecificCircumstanceIndicatorInfo, expectedError);
				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F26;
				AssertNoMessageError(manifestHeader.SpecificCircumstanceIndicatorInfo, expectedError);
				manifestHeader.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F40;
				AssertNoMessageError(manifestHeader.SpecificCircumstanceIndicatorInfo, expectedError);
				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F50;
				AssertNoMessageError(manifestHeader.SpecificCircumstanceIndicatorInfo, expectedError);
				manifestHeader.AMA_TransportMode = TransportModes.Sea;
				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F14;
				AssertNoMessageError(manifestHeader.SpecificCircumstanceIndicatorInfo, expectedError);
				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F15;
				AssertNoMessageError(manifestHeader.SpecificCircumstanceIndicatorInfo, expectedError);
			});
		}

		public void TestCheckSpecificCircumstanceIndicator_AfterChangeTransportMode()
		{
			manifestHeader.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			manifestHeader.AMA_TransportMode = TransportModes.Air;
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F10;
			CombineAssertions(() =>
			{
				AssertHasMessageError("Invalid Transport and Specific Circumsatance combination", manifestHeader.SpecificCircumstanceIndicatorInfo, SpecificCircumstanceIndicatorMessageError);
				manifestHeader.AMA_TransportMode = TransportModes.Sea;
				AssertNoMessageError("Valif Transport and Specific Circumsatance combination", manifestHeader.SpecificCircumstanceIndicatorInfo, SpecificCircumstanceIndicatorMessageError);
			});
		}

		public void TestMOTIdentifierType()
		{
			manifestHeader.MOTIdentifierType = EUICS2ModeOfTransportIdentifierTypeList.Codes.CL750_80;
			AssertNoMessageError("Valid Code", manifestHeader.MOTIdentifierTypeInfo, ListValidation.InvalidCodeMessageError);

			manifestHeader.MOTIdentifierType = "1";
			AssertHasMessageError("InValid Code", manifestHeader.MOTIdentifierTypeInfo, ListValidation.InvalidCodeMessageError);

			manifestHeader.MOTIdentifierType = "";
			AssertNoMessageError("Code is empty", manifestHeader.MOTIdentifierTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestMOTIdentifierType_CarrierManifest()
		{
			ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyHasValue(manifestHeader.MOTIdentifierTypeInfo, manifestHeader.SpecificCircumstanceIndicatorInfo,
				(ZString)EUICS2SpecificCircumstanceList.Codes.F40, true, "You have not entered a Mode of Transport Identifier Type.");
		}

		public void TestAddressedMemberState()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var startDate = ZDateTime.Today.AddYears(-2);
			var endDate = ZDateTime.Today.AddYears(2);
			var europeanUnionCode = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			var eunId = helper.CreateNewOrGetExistingDataGrouping(europeanUnionCode);
			var codeType = EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_IC2MS;
			helper.CreateNewOrGetExistingCusCodeList(europeanUnionCode, codeType, "DE", "Germany", startDate, endDate);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var header = newFactory.New<AsycudaManifestHeader>();

			header.AddressedMemberState = string.Empty;
			AssertHasError(header.AddressedMemberStateInfo, "Please enter a valid Country.");

			header.AddressedMemberState = "EU";
			AssertHasError(header.AddressedMemberStateInfo, "Please enter a valid Country.");

			header.AddressedMemberState = "AA";
			AssertHasError(header.AddressedMemberStateInfo, "Please enter a valid Country.");

			header.AddressedMemberState = CountryCodes.Germany;
			AssertNoError(header.AddressedMemberStateInfo, "Please enter a valid Country.");
		}

		public void TestCheckAMA_CustomsOffice()
		{
			ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyHasValue(manifestHeader.AMA_CustomsOfficeInfo, manifestHeader.SpecificCircumstanceIndicatorInfo,
				new IZType[] { (ZString)EUICS2SpecificCircumstanceList.Codes.F40, (ZString)EUICS2SpecificCircumstanceList.Codes.F50 }, true,
				"You have not entered a Customs Office of First Entry.");
		}

		public void TestCheckReceptacles()
		{
			const string expectedError = "You have not entered a Receptacle Identification Number.";

			manifestHeader.Validation.ValidateReceptacleId();
			AssertNotEquals("Precondition", EUICS2SpecificCircumstanceList.Codes.F25, manifestHeader.SpecificCircumstanceIndicator);
			AssertNoMessageError(manifestHeader.ReceptacleIdInfo, expectedError);

			manifestHeader.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			manifestHeader.AMA_TransportMode = TransportModes.Rail;
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F41;
			manifestHeader.Validation.ValidateReceptacleId();
			AssertHasMessageError(manifestHeader.ReceptacleIdInfo, expectedError);

			manifestHeader.AMA_TransportMode = TransportModes.Road;
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F40;
			manifestHeader.Validation.ValidateReceptacleId();
			AssertHasMessageError(manifestHeader.ReceptacleIdInfo, expectedError);

			var receptacle = manifestHeader.Receptacles.AddNew();
			receptacle.CY_Data = "342516";
			manifestHeader.Validation.ValidateReceptacleId();
			AssertNoMessageError(manifestHeader.ReceptacleIdInfo, expectedError);
		}

		public void TestCheckPreviousMRN()
		{
			AssertNotEquals("Precondition", EUICS2SpecificCircumstanceList.Codes.F25, manifestHeader.SpecificCircumstanceIndicator);
			ValidationTestHelper.AssertFieldIsNotMandatory(manifestHeader.PreviousMRNInfo);

			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F25;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(manifestHeader.PreviousMRNInfo);
		}

		public void TestCheckAMA_RN_NKConveyanceNationality()
		{
			AssertNotEquals("Precondition", "VR1234", manifestHeader.AMA_VehicleRegistration);
			ValidationTestHelper.AssertFieldIsNotMandatory(manifestHeader.AMA_RN_NKConveyanceNationalityInfo);

			manifestHeader.AMA_TransportMode = TransportModes.Road;
			manifestHeader.AMA_VehicleRegistration = "VR1234";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(manifestHeader.AMA_RN_NKConveyanceNationalityInfo, "Vehicle Registration Country is mandatory for Transport Mode Road");
		}

		public void TestCheckAMA_MasterBill()
		{
			ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyHasValue(manifestHeader.AMA_MasterBillInfo, manifestHeader.SpecificCircumstanceIndicatorInfo, (ZString)EUICS2SpecificCircumstanceList.Codes.F40, true, "You have not entered a Manifest Number.");

			manifestHeader.AMA_TransportMode = TransportModes.Road;
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F44;
			ValidationTestHelper.AssertFieldIsNotMandatory(manifestHeader.AMA_MasterBillInfo);
		}

		public void TestCheckAMA_MasterBillBase()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_MasterBill = ZString.Empty;
			AssertHasMessageError(header.AMA_MasterBillInfo, ASYCUDA.Business.ValidationConstants.ManifestNumberIsRequired(header.MasterBillLabel.Caption));
			header.AMA_MasterBill = "MB1";
			AssertNoMessageErrors(header.AMA_MasterBillInfo);
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			header.AMA_MasterBill = ZString.Empty;
			AssertNoMessageErrors(header.AMA_MasterBillInfo);

			header.AMA_TransportMode = "AIR";
			header.AMA_MasterBill = "123-12345629";
			AssertHasWarningContaining(header.AMA_MasterBillInfo, "Invalid check digit. The last digit should be '0'");
			header.AMA_MasterBill = "123-12345620";
			AssertNoWarningContaining(header.AMA_MasterBillInfo, "Invalid check digit. The last digit should be '0'");
			header.AMA_TransportMode = "SEA";
			header.AMA_MasterBill = "123-12345629";
			AssertNoWarningContaining(header.AMA_MasterBillInfo, "Invalid check digit. The last digit should be '0'");
			header.AMA_TransportMode = "AIR";
			header.Validation.ValidateAMA_MasterBill();
			AssertHasWarningContaining(header.AMA_MasterBillInfo, "Invalid check digit. The last digit should be '0'");
		}

		public void TestCheckAMA_PaymentMethod()
		{
			CombineAssertions(() =>
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
				header.AMA_TransportMode = TransportModes.Road;
				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F50;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(header.AMA_PaymentMethodInfo, "You have not entered a Method of Payment");
				ValidationTestHelper.AssertInvalidCodeMessageError(header.AMA_PaymentMethodInfo, "???", EUICS2PaymentMethodList.Codes.A);

				header.AMA_TransportMode = TransportModes.Rail;
				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F51;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(header.AMA_PaymentMethodInfo, "You have not entered a Method of Payment");
				ValidationTestHelper.AssertInvalidCodeMessageError(header.AMA_PaymentMethodInfo, "???", EUICS2PaymentMethodList.Codes.A);
			});
		}

		public void TestCheckAMA_VehicleRegistration()
		{
			const string msgError = "Vehicle Registration Number is mandatory for Transport Mode Road";
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
			header.AMA_TransportMode = TransportModes.Road;
			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F50;
			header.AMA_VehicleRegistration = ZString.Empty;
			AssertHasMessageError(header.AMA_VehicleRegistrationInfo, msgError);

			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F44;
			header.AMA_VehicleRegistration = ZString.Empty;
			AssertNoMessageError(header.AMA_VehicleRegistrationInfo, msgError);
		}

		const string SpecificCircumstanceIndicatorMessageError = "The selected ICS2 submission may fail, based on the Type of Manifest and Mode of Transport entered.";

		protected override void SetUp()
		{
			base.SetUp();
			manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
		}

		OrgAddress GetOrgAddress()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_Address1 = "72 O'Riordan Street";
			orgAddress.OA_City = "SYDNEY";
			orgAddress.OA_Phone = "+14155552671";
			return orgAddress;
		}

		AsycudaManifestHeader manifestHeader;
	}
}

