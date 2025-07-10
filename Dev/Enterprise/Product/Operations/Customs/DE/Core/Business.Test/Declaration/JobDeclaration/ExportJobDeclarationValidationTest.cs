using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Internal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class ExportJobDeclarationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJE_TransportModeMandatory() => CombineAssertions(() =>
		{
			const string message = "You have not entered a [25] Transport.";

			var property = jobDeclaration.JE_TransportModeInfo;
			jobDeclaration.CustomsEntryInstructions.RemoveAll();
			var instruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "000000";
			jobDeclaration.JE_TransportMode = ZString.Empty;

			AssertHasMessageError("CEI_Style: ***0**, JE_TransportMode: Empty", property, message);

			jobDeclaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;

			AssertNoMessageError("CEI_Style: ***0**, JE_TransportMode: NotEmpty", property, message);

			instruction.CEI_Style = "001400";
			jobDeclaration.JE_TransportMode = ZString.Empty;

			AssertHasMessageError("CEI_Style: **1*0*, JE_TransportMode: Empty", property, message);

			jobDeclaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;

			AssertNoMessageError("CEI_Style: **1*0*, JE_TransportMode: NotEmpty", property, message);

			instruction.CEI_Style = "004400";
			jobDeclaration.JE_TransportMode = ZString.Empty;

			AssertHasWarning("CEI_Style: other, JE_TransportMode: Empty", property, message);

			jobDeclaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;

			AssertNoWarning("CEI_Style: other, JE_TransportMode: NotEmpty", property, message);
		});

		public void TestCheckJE_ContainerMode_Mandatory_TransitionPeriodAES30()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
			{
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(jobDeclaration.JE_ContainerModeInfo);
			}
		}

		public void TestCheckJE_ContainerMode_Mandatory() => CombineAssertions(() =>
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
			{
				jobDeclaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
				ValidationTestHelper.AssertNoWarningIfNotEntered(jobDeclaration.JE_ContainerModeInfo, "Container type is required for sea shipment.", testCaseDescription: "When non sea ContainerMode is not mandatory");

				jobDeclaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(jobDeclaration.JE_ContainerModeInfo, "Container type is required for sea shipment.", testCaseDescription: "When sea ContainerMode is mandatory");
			}
		});

		public void TestCheckJE_GoodsOrigin_Mandatory()
		{
			CombineAssertions(() =>
			{
				jobDeclaration.Validation.ValidateJE_GoodsOrigin();
				AssertNoMessageErrorContaining("No entry instruction", jobDeclaration.JE_GoodsOriginInfo, MandatoryValidation.YouHaveNotEntered);

				var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000901;
				jobDeclaration.Validation.ValidateJE_GoodsOrigin();
				AssertNoMessageErrorContaining("Has entry instruction with CEI_Style '***9**'", jobDeclaration.JE_GoodsOriginInfo, MandatoryValidation.YouHaveNotEntered);

				var entryInstruction2 = jobDeclaration.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000410;
				jobDeclaration.Validation.ValidateJE_GoodsOrigin();
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(jobDeclaration.JE_GoodsOriginInfo);
			});
		}

		public void TestCheckJE_GoodsOrigin()
		{
			const string errorMessage = "For the selected Type(Procedure) Country/Region of Origin must be 'DE'.";
			CombineAssertions(() =>
			{
				jobDeclaration.Validation.ValidateJE_GoodsOrigin();
				AssertNoMessageError("No entry instruction", jobDeclaration.JE_GoodsOriginInfo, errorMessage);

				var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000901;
				jobDeclaration.Validation.ValidateJE_GoodsOrigin();
				AssertNoMessageError("Has entry instruction with CEI_Style '***9**'", jobDeclaration.JE_GoodsOriginInfo, errorMessage);

				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000410;
				jobDeclaration.Validation.ValidateJE_GoodsOrigin();
				AssertNoMessageError("Has entry instruction with CEI_Style '***4**'", jobDeclaration.JE_GoodsOriginInfo, errorMessage);

				var entryInstruction2 = jobDeclaration.CustomsEntryInstructions.AddNew();
				jobDeclaration.Validation.ValidateJE_GoodsOrigin();
				AssertHasMessageError("Has entry instruction with CEI_Style not '***9**' or '***4**'", jobDeclaration.JE_GoodsOriginInfo, errorMessage);

				jobDeclaration.JE_GoodsOrigin = Core.Constants.CountryCodes.Germany;
				AssertNoMessageError("JE_GoodsOrigin is 'DE'", jobDeclaration.JE_GoodsOriginInfo, errorMessage);
			});
		}

		public void TestCheckJE_GoodsDestination_Mandatory()
		{
			var propertyInfo = jobDeclaration.JE_GoodsDestinationInfo;
			CombineAssertions(() =>
			{
				AssertEquals("Property not readonly", false, propertyInfo.ReadOnly);
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(propertyInfo);
			});
		}

		public void TestCheckJE_OA_DeclarantAddressEmpty()
		{
			const string errorMessage = "Declarant Address is required";
			CombineAssertions(() =>
			{
				jobDeclaration.JE_OA_DeclarantAddress = ZGuid.Empty;
				AssertNoMessageError("No entry instruction", jobDeclaration.JE_OA_DeclarantAddressInfo, errorMessage);

				var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000100;
				jobDeclaration.Validation.ValidateJE_OA_DeclarantAddress();
				AssertHasMessageError("Has entry instruction", jobDeclaration.JE_OA_DeclarantAddressInfo, errorMessage);
			});
		}

		public void TestCheckJE_OA_DeclarantAddressCountryCode()
		{
			var errorMsg1 = "The Declarant must be resident in the EU";
			var errorMsg2 = "The Declarant must be resident in the EU, Switzerland or Liechtenstein";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, parent: eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Liechtenstein);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = org.Addresses.AddNew();
			orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			entryInstruction.ZG_PartyConstellation = "0001";
			jobDeclaration.JE_OA_DeclarantAddress = orgAddress.PK;
			AssertNoMessageError(jobDeclaration.JE_OA_DeclarantAddressInfo, errorMsg1);

			orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			jobDeclaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertHasMessageError(jobDeclaration.JE_OA_DeclarantAddressInfo, errorMsg1);

			entryInstruction.ZG_PartyConstellation = "0000";
			jobDeclaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertHasMessageError(jobDeclaration.JE_OA_DeclarantAddressInfo, errorMsg2);

			orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Liechtenstein;
			jobDeclaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertNoMessageError(jobDeclaration.JE_OA_DeclarantAddressInfo, errorMsg2);
		}

		public void TestCheckDeclarantAndRepresentativeIdentical()
		{
			var errorMsg = "[14] Representative and [14] Declarant must not be equal";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress1 = org1.Addresses.AddNew();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress2 = org2.Addresses.AddNew();
			jobDeclaration.JE_OA_Representative = orgAddress1.PK;
			jobDeclaration.JE_OA_DeclarantAddress = orgAddress1.PK;

			jobDeclaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertHasMessageError(jobDeclaration.JE_OA_DeclarantAddressInfo, errorMsg);
			jobDeclaration.Validation.ValidateJE_OA_Representative();
			AssertHasMessageError(jobDeclaration.JE_OA_RepresentativeInfo, errorMsg);

			jobDeclaration.JE_OA_Representative = orgAddress2.PK;
			jobDeclaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertNoMessageError(jobDeclaration.JE_OA_DeclarantAddressInfo, errorMsg);
			jobDeclaration.Validation.ValidateJE_OA_Representative();
			AssertNoMessageError(jobDeclaration.JE_OA_RepresentativeInfo, errorMsg);
		}

		public void TestCheckJE_OA_Representative_Mandatory()
		{
			var message = "The chosen Party Constellation requires a [14] Representative to be entered.";
			var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();

			CombineAssertions(() =>
			{
				AssertNoMessageError("Representative has been defaulted, it isn't empty, no message error", jobDeclaration.JE_OA_RepresentativeInfo, message);

				jobDeclaration.JE_OA_Representative = ZGuid.Empty;
				AssertNoMessageError("Representative is empty and party constellation is empty", jobDeclaration.JE_OA_RepresentativeInfo, message);

				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0000;
				AssertNoMessageError("Representative is empty and third digit of ZG_PartyConstellation isn't 1", jobDeclaration.JE_OA_RepresentativeInfo, message);

				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0010;
				AssertHasMessageError("Representative is empty and third digit of ZG_PartyConstellation is 1", jobDeclaration.JE_OA_RepresentativeInfo, message);
			});
		}

		public void TestCheckJE_OA_SellerAddress_Mandatory()
		{
			var message = "The chosen Party Constellation requires a [2] Subcontractor to be entered.";
			var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();

			CombineAssertions(() =>
			{
				AssertNoMessageError("Subcontractor is empty and party constellation is empty", jobDeclaration.JE_OA_SellerAddressInfo, message);

				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0000;
				AssertNoMessageError("Subcontractor is empty and fourth digit of ZG_PartyConstellation isn't 1", jobDeclaration.JE_OA_SellerAddressInfo, message);

				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0001;
				AssertHasMessageError("Subcontractor is empty and fourth digit of ZG_PartyConstellation is 1", jobDeclaration.JE_OA_SellerAddressInfo, message);

				jobDeclaration.JE_OA_SellerAddress = Factory.New<OrgHeader>().MainAddress.PK;
				AssertNoMessageError("Subcontractor isn't empty, no message error", jobDeclaration.JE_OA_SellerAddressInfo, message);
			});
		}

		public void TestCheckJE_OA_SellerAddress_MustNotHaveSameEoriAsDeclarant()
		{
			const string message = "[2] Subcontractor and [14] Declarant must not be equal.";
			var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0011;
			var sellerOrgHeader = Factory.New<OrgHeader>();
			sellerOrgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "TCU001", Core.Constants.CountryCodes.Italy);
			sellerOrgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EOR001", Core.Constants.CountryCodes.Italy);
			jobDeclaration.JE_OA_DeclarantAddress = ZGuid.Empty;

			CombineAssertions(() =>
			{
				jobDeclaration.JE_OA_SellerAddress = sellerOrgHeader.MainAddress.PK;
				AssertNoMessageError("DeclarantAddress empty", jobDeclaration.JE_OA_SellerAddressInfo, message);

				jobDeclaration.JE_OA_DeclarantAddress = sellerOrgHeader.MainAddress.PK;
				jobDeclaration.Validation.ValidateJE_OA_SellerAddress();
				AssertHasMessageError("Declarant == Subcontractor", jobDeclaration.JE_OA_SellerAddressInfo, message);

				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0101;
				jobDeclaration.Validation.ValidateJE_OA_SellerAddress();
				AssertNoMessageError("Declarant == Subcontractor but PartyConstellation <> 'X0X1'", jobDeclaration.JE_OA_SellerAddressInfo, message);

				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0011;
				var declarantOrgHeader = Factory.New<OrgHeader>();
				declarantOrgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "TCU001", Core.Constants.CountryCodes.Italy);
				var eor2 = declarantOrgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EOR001", Core.Constants.CountryCodes.Belgium);
				jobDeclaration.JE_OA_SellerAddress = declarantOrgHeader.MainAddress.PK;
				AssertNoMessageError("EOR1 == EOR2 but the issuing countries are different", jobDeclaration.JE_OA_SellerAddressInfo, message);

				eor2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Italy;
				jobDeclaration.Validation.ValidateJE_OA_SellerAddress();
				AssertHasMessageError("EOR1 == EOR2 and the issuing countries are the same", jobDeclaration.JE_OA_SellerAddressInfo, message);
			});
		}

		public void TestCheckJE_OA_RepresentativeCountryCode()
		{
			var errorMsg = "The Representative must be resident in the EU, Switzerland or Liechtenstein";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, parent: eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Liechtenstein);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = org.Addresses.AddNew();
			orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			jobDeclaration.JE_OA_Representative = orgAddress.PK;
			AssertNoMessageError(jobDeclaration.JE_OA_RepresentativeInfo, errorMsg);

			orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			jobDeclaration.Validation.ValidateJE_OA_Representative();
			AssertHasMessageError(jobDeclaration.JE_OA_RepresentativeInfo, errorMsg);

			orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Liechtenstein;
			jobDeclaration.Validation.ValidateJE_OA_Representative();
			AssertNoMessageError(jobDeclaration.JE_OA_RepresentativeInfo, errorMsg);
		}

		public void TestCheckJE_VesselName_MaxLength()
		{
			jobDeclaration.JE_VesselName = ZString.Empty.PadLeft(28, 'A');
			AssertHasWarning("28 characters", jobDeclaration.JE_VesselNameInfo, "The maximum length for [21] Vessel is 27 characters.");
		}

		public void TestCheckJE_VoyageFlightNo_Mandatory_BorderTransportMeansIs40()
		{
			const string message = "You have not entered a Flight Number.";
			jobDeclaration.JE_TransportMode = TransportTypeList.Codes.Air;
			jobDeclaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._40;
			CombineAssertions(() =>
			{
				jobDeclaration.Validation.ValidateJE_VoyageFlightNo();
				AssertHasMessageError("JE_VoyageFlightNo is empty", jobDeclaration.JE_VoyageFlightNoInfo, message);

				jobDeclaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				jobDeclaration.Validation.ValidateJE_VoyageFlightNo();
				AssertNoMessageError("JE_VoyageFlightNo is empty and JE_TransportMode isn't 'AIR'", jobDeclaration.JE_VoyageFlightNoInfo, message);

				jobDeclaration.JE_TransportMode = TransportTypeList.Codes.Air;
				jobDeclaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._41;
				jobDeclaration.Validation.ValidateJE_VoyageFlightNo();
				AssertNoMessageError("JE_VoyageFlightNo is empty and ZG_BorderTransportMeans isn't '40'", jobDeclaration.JE_VoyageFlightNoInfo, message);

				jobDeclaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._40;
				jobDeclaration.JE_VoyageFlightNo = "123456";
				AssertNoMessageError("JE_VoyageFlightNo isn't empty", jobDeclaration.JE_VoyageFlightNoInfo, message);
			});
		}

		public void TestCheckJE_VoyageFlightNo_Mandatory_BorderTransportMeansIs41()
		{
			const string message = "You have not entered a Registration Number of the Aircraft.";
			jobDeclaration.JE_TransportMode = TransportTypeList.Codes.Air;
			jobDeclaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._41;
			CombineAssertions(() =>
			{
				jobDeclaration.Validation.ValidateJE_VoyageFlightNo();
				AssertHasMessageError("JE_VoyageFlightNo is empty", jobDeclaration.JE_VoyageFlightNoInfo, message);

				jobDeclaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				jobDeclaration.Validation.ValidateJE_VoyageFlightNo();
				AssertNoMessageError("JE_VoyageFlightNo is empty and JE_TransportMode isn't 'AIR'", jobDeclaration.JE_VoyageFlightNoInfo, message);

				jobDeclaration.JE_TransportMode = TransportTypeList.Codes.Air;
				jobDeclaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._40;
				jobDeclaration.Validation.ValidateJE_VoyageFlightNo();
				AssertNoMessageError("JE_VoyageFlightNo is empty and ZG_BorderTransportMeans isn't '41'", jobDeclaration.JE_VoyageFlightNoInfo, message);

				jobDeclaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._41;
				jobDeclaration.JE_VoyageFlightNo = "123456";
				AssertNoMessageError("JE_VoyageFlightNo isn't empty", jobDeclaration.JE_VoyageFlightNoInfo, message);
			});
		}

		public void TestCheckJE_RN_NKTransportNationality_Mandatory()
		{
			const string message = "You have not entered a [21] Nationality of Means of Transport at the Border.";
			jobDeclaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._41;
			CombineAssertions(() =>
			{
				jobDeclaration.Validation.ValidateJE_RN_NKTransportNationality();
				AssertHasMessageError("JE_RN_NKTransportNationality is empty", jobDeclaration.JE_RN_NKTransportNationalityInfo, message);

				jobDeclaration.ZG_BorderTransportMeans = ZString.Empty;
				jobDeclaration.Validation.ValidateJE_RN_NKTransportNationality();
				AssertNoMessageError("JE_RN_NKTransportNationality is empty and ZG_BorderTransportMeans is empty", jobDeclaration.JE_RN_NKTransportNationalityInfo, message);

				jobDeclaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._41;
				jobDeclaration.JE_RN_NKTransportNationality = Core.Constants.CountryCodes.Germany;
				AssertNoMessageError("JE_RN_NKTransportNationality isn't empty", jobDeclaration.JE_RN_NKTransportNationalityInfo, message);
			});
		}

		public void TestCheckJE_TransportModeInland()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(jobDeclaration.JE_TransportModeInlandInfo, "AAA", TransportTypeList.Codes.Air);
		}

		public void TestCheckJE_RN_NKTransportNationalityInland_Mandatory()
		{
			CombineAssertions(() =>
			{
				foreach (var transportMode in new ZString[] { TransportTypeList.Codes.Sea, TransportTypeList.Codes.OwnPropulsion, TransportTypeList.Codes.InlandWaterwayTransport, TransportTypeList.Codes.Road, TransportTypeList.Codes.Rail })
				{
					var transportModeDescription = $"Transport Mode: {transportMode}";
					jobDeclaration.JE_TransportModeInland = transportMode;
					ValidationTestHelper.AssertFieldIsNotMandatory(jobDeclaration.JE_RN_NKTransportNationalityInlandInfo, transportModeDescription);
					jobDeclaration.JE_TransportIDInland = "ABCD1234";
					ValidationTestHelper.AssertYouHaveNotEnteredMessageError(jobDeclaration.JE_RN_NKTransportNationalityInlandInfo, "Nationality", transportModeDescription);
				}
			});
		}

		public void TestCheckJE_RN_NKTransportNationalityInland_Mandatory_UCC6Validation()
		{
			jobDeclaration.JE_TransportModeInland = TransportTypeList.Codes.FixedTransportInstallations;
			jobDeclaration.JE_TransportMeans = ExportInlandTransportTypeList.Codes._10;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(jobDeclaration.JE_RN_NKTransportNationalityInlandInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJE_RN_NKTransportNationalityInland_List()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EXNAT, "Export Nationality");
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EXNAT, "DE", "DEGUO", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			ValidationTestHelper.AssertInvalidCodeMessageError(jobDeclaration.JE_RN_NKTransportNationalityInlandInfo, "ZZ", Core.Constants.CountryCodes.Germany);
		}

		public void CheckJE_RN_NKTransportNationalityInland_Air()
		{
			CombineAssertions(() =>
			{
				jobDeclaration.JE_TransportModeInland = TransportTypeList.Codes.Air;
				jobDeclaration.JE_TransportIDInland = string.Empty;
				jobDeclaration.JE_AircraftRegistrationInland = "12345566";
				jobDeclaration.JE_RN_NKTransportNationalityInland = string.Empty;
				AssertHasMessageErrorContaining("Not entered", jobDeclaration.JE_RN_NKTransportNationalityInlandInfo, MandatoryValidation.YouHaveNotEntered);
				jobDeclaration.JE_RN_NKTransportNationalityInland = Core.Constants.CountryCodes.Germany;
				AssertNoMessageErrorContaining("Entered", jobDeclaration.JE_RN_NKTransportNationalityInlandInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void CheckJE_RN_NKTransportNationalityInland_Air_JE_AircraftRegistrationInland()
		{
			CombineAssertions(() =>
			{
				jobDeclaration.JE_TransportModeInland = TransportTypeList.Codes.Air;
				jobDeclaration.JE_TransportIDInland = string.Empty;
				jobDeclaration.JE_AircraftRegistrationInland = "12345566";
				jobDeclaration.JE_RN_NKTransportNationalityInland = string.Empty;
				AssertHasMessageErrorContaining("Not entered", jobDeclaration.JE_RN_NKTransportNationalityInlandInfo, MandatoryValidation.YouHaveNotEntered);
				jobDeclaration.JE_AircraftRegistrationInland = string.Empty;
				jobDeclaration.Validation.ValidateJE_RN_NKTransportNationalityInland();
				AssertNoMessageErrorContaining("No Transport ID Inland", jobDeclaration.JE_RN_NKTransportNationalityInlandInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckJE_TransportIDInland_MandatoryAndUseHumanReadableName()
		{
			jobDeclaration.JE_TransportIDInlandInfo.HumanReadableName = "ABC.";

			var transportModes = new[] {
					TransportTypeList.Codes.Sea,
					TransportTypeList.Codes.InlandWaterwayTransport,
					TransportTypeList.Codes.OwnPropulsion,
					TransportTypeList.Codes.Road,
					TransportTypeList.Codes.Air };

			CombineAssertions(() =>
			{
				foreach (var transportMode in transportModes)
				{
					jobDeclaration.JE_TransportModeInland = transportMode;
					ValidationTestHelper.AssertYouHaveNotEnteredMessageError(jobDeclaration.JE_TransportIDInlandInfo, "ABC", $"Transport Mode: {transportMode}");
				}
			});
		}

		public void TestCheckJE_TransportIDInland_MandatoryAndUseHumanReadableName_Rail()
		{
			jobDeclaration.JE_TransportIDInlandInfo.HumanReadableName = "ABC.";

			jobDeclaration.JE_TransportModeInland = TransportTypeList.Codes.Rail;

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
			{
				jobDeclaration.Validation.ValidateJE_TransportIDInland();
				AssertNoMessageErrorContaining(jobDeclaration.JE_TransportIDInlandInfo, "ABC");
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
			{
				jobDeclaration.Validation.ValidateJE_TransportIDInland();
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(jobDeclaration.JE_TransportIDInlandInfo, "ABC");
			}
		}

		public void TestCheckJE_TransportIDInland_Mandatory_UCC6Validation()
		{
			jobDeclaration.JE_TransportModeInland = TransportTypeList.Codes.FixedTransportInstallations;
			jobDeclaration.JE_TransportMeans = TransportMeansList.Codes.ImoShipIdentificationNumber;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(jobDeclaration.JE_TransportIDInlandInfo);
		}

		public void TestCheckJE_TransportMeans_Mandatory()
		{
			var allTransportModes = new TransportTypeList().GetAllCodes();
			var notMandatoryModes = new[] { TransportTypeList.Codes.FixedTransportInstallations, TransportTypeList.Codes.Mail };

			CombineAssertions(() =>
			{
				foreach (var transportMode in allTransportModes)
				{
					jobDeclaration.JE_TransportModeInland = transportMode;
					jobDeclaration.JE_TransportMeans = ZString.Empty;
					if (notMandatoryModes.Contains(transportMode))
					{
						AssertNoMessageErrorContaining(jobDeclaration.JE_TransportMeansInfo, MandatoryValidation.YouHaveNotEntered);
					}
					else
					{
						AssertHasMessageErrorContaining(jobDeclaration.JE_TransportMeansInfo, MandatoryValidation.YouHaveNotEntered);
					}
				}
			});
		}

		public void TestCheckJE_TransportMeans_List()
		{
			jobDeclaration.JE_TransportModeInland = TransportTypeList.Codes.OwnPropulsion;
			ValidationTestHelper.AssertInvalidCodeMessageError(jobDeclaration.JE_TransportMeansInfo, "AA", TransportMeansList.Codes.ImoShipIdentificationNumber);
		}

		public void TestCheckJE_TransportIDInland_LowerCaseLettersProhibitedForAllInlandTransportModesAndTransportMeansExcept11and81()
		{
			const string expectedMessageError = "No lower case letters may be specified.";
			var transportTypesThatAllowMixedCase = new HashSet<string> { TransportMeansList.Codes.NameOfTheSeaGoingVessel, TransportMeansList.Codes.NameOfTheInlandWaterwaysVessel };
			foreach (var transportMode in new TransportTypeList().GetAllCodes())
			{
				jobDeclaration.JE_TransportModeInland = transportMode;
				foreach (var inlandTransportType in new ExportInlandTransportTypeList().GetAllCodes())
				{
					jobDeclaration.JE_TransportMeans = inlandTransportType;
					jobDeclaration.JE_TransportIDInland = "test1234TEST";

					if (transportTypesThatAllowMixedCase.Contains(inlandTransportType))
					{
						AssertNoMessageError(jobDeclaration.JE_TransportIDInlandInfo, expectedMessageError);
					}
					else
					{
						AssertHasMessageError(jobDeclaration.JE_TransportIDInlandInfo, expectedMessageError);
					}
				}
			}
		}

		public void TestCheckJE_RN_NKTrailer1Nationality_Mandatory()
		{
			CombineAssertions(() =>
			{
				jobDeclaration.JE_Trailer1RegNo = "1234ABCD";
				jobDeclaration.Validation.ValidateJE_RN_NKTrailer1Nationality();
				AssertHasMessageErrorContaining("Trailer Number", jobDeclaration.JE_RN_NKTrailer1NationalityInfo, MandatoryValidation.YouHaveNotEntered);
				jobDeclaration.JE_Trailer1RegNo = string.Empty;
				jobDeclaration.Validation.ValidateJE_RN_NKTrailer1Nationality();
				AssertNoMessageErrorContaining("No Trailer Number", jobDeclaration.JE_RN_NKTrailer1NationalityInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckJE_RN_NKTrailer2Nationality_Mandatory()
		{
			CombineAssertions(() =>
			{
				jobDeclaration.JE_Trailer2RegNo = "1234ABCD";
				jobDeclaration.Validation.ValidateJE_RN_NKTrailer2Nationality();
				AssertHasMessageErrorContaining("Trailer Number", jobDeclaration.JE_RN_NKTrailer2NationalityInfo, MandatoryValidation.YouHaveNotEntered);
				jobDeclaration.JE_Trailer2RegNo = string.Empty;
				jobDeclaration.Validation.ValidateJE_RN_NKTrailer2Nationality();
				AssertNoMessageErrorContaining("No Trailer Number", jobDeclaration.JE_RN_NKTrailer2NationalityInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckJE_TransportIDInland_Sea_VesselID_ListWarning()
		{
			jobDeclaration.JE_TransportModeInland = TransportTypeList.Codes.Sea;

			const string expectedWarningMessage = "Warning: No reference file for this Vessel was found.";
			CombineAssertions(() =>
			{
				jobDeclaration.JE_TransportMeans = TransportMeansList.Codes.NameOfTheSeaGoingVessel;
				jobDeclaration.JE_TransportIDInland = "Sea Shepard";
				AssertHasWarning("Inland vessel", jobDeclaration.JE_TransportIDInlandInfo, expectedWarningMessage);

				jobDeclaration.JE_TransportMeans = TransportMeansList.Codes.ImoShipIdentificationNumber;
				jobDeclaration.Validation.ValidateJE_TransportIDInland();
				AssertNoWarning("Transport mean is not name of the vessel", jobDeclaration.JE_TransportIDInlandInfo, expectedWarningMessage);

				jobDeclaration.JE_TransportMeans = TransportMeansList.Codes.NameOfTheSeaGoingVessel;
				var vessel = Factory.New<RefVessel>();
				vessel.RV_Name = "MY NEW SHIP";
				jobDeclaration.JE_TransportIDInland = vessel.RV_Name;
				AssertNoWarning("RefVessel", jobDeclaration.JE_TransportIDInlandInfo, expectedWarningMessage);
			});
		}

		public void TestCheckJE_OwnerRef_MaxLength()
		{
			const string message = "The maximum length for [7] Declarant's Ref is 22 characters.";
			CombineAssertions(() =>
			{
				jobDeclaration.JE_OwnerRef = ZString.Empty.PadLeft(23, 'A');
				AssertHasWarning("23 characters", jobDeclaration.JE_OwnerRefInfo, message);
				jobDeclaration.JE_OwnerRef = ZString.Empty.PadLeft(22, 'A');
				AssertNoWarning("22 characters", jobDeclaration.JE_OwnerRefInfo, message);
			});
		}

		public void TestCheckJE_CustomsOffice_EXT() => CombineAssertions(() =>
		{
			var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000901;

			SetupCustomsOffices();

			jobDeclaration.Validation.ValidateJE_CustomsOffice();

			AssertHasMessageError("Default", jobDeclaration.JE_CustomsOfficeInfo, "Please enter an Office of Export with role 'EXT'");

			jobDeclaration.JE_CustomsOffice = "DE000EXP";

			AssertHasMessageError("EXP - error", jobDeclaration.JE_CustomsOfficeInfo, "Please enter an Office of Export with role 'EXT'");
			jobDeclaration.JE_CustomsOffice = "DE000EXT";

			AssertNoMessageError("EXT - no error", jobDeclaration.JE_CustomsOfficeInfo, "Please enter an Office of Export with role 'EXT'");
		});

		public void TestCheckJE_CustomsOffice_EXP()
		{
			var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000410;

			SetupCustomsOffices();

			jobDeclaration.Validation.ValidateJE_CustomsOffice();

			AssertHasMessageError(jobDeclaration.JE_CustomsOfficeInfo, "Please enter an Office of Export with role 'EXP'");

			jobDeclaration.JE_CustomsOffice = "DE000EXT";

			AssertHasMessageError(jobDeclaration.JE_CustomsOfficeInfo, "Please enter an Office of Export with role 'EXP'");
			jobDeclaration.JE_CustomsOffice = "DE000EXP";

			AssertNoMessageError(jobDeclaration.JE_CustomsOfficeInfo, "Please enter an Office of Export with role 'EXP'");
		}

		public void TestCheckJE_CustomsOffice_MultipleDoNotThrow()
		{
			var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000410;

			// manually create a customs office because it's not yet in reference data
			var cusofZZ = Factory.New<ZZRefCusCodeList>();
			cusofZZ.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
			cusofZZ.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Germany;
			cusofZZ.ZZD_Code = "DE000EXP ";
			cusofZZ.ZZD_StartDate = ZDateTime.Today.AddDays(-1);
			cusofZZ.ZZD_EndDate = ZDateTime.Today.AddDays(1);
			Factory.Save(); // save the manual created duplicate

			jobDeclaration.JE_CustomsOffice = "DE000EXP"; // trigger load cache and validation

			// now reference data gets updated
			SetupCustomsOffices();

			AssertNoExceptionThrown(() =>
			{
				jobDeclaration.Validation.ValidateJE_CustomsOffice();
			});
		}

		public void TestCheckJE_OwnerRefEmptyWarning() => CombineAssertions(() =>
		{
			const string warningMessage = "If Declarant's Reference is empty, Declaration Reference (B00069) will be determined as Local Reference Number and sent to Customs.";
			jobDeclaration.JE_DeclarationReference = "B00069";

			jobDeclaration.Validation.ValidateJE_OwnerRef();
			AssertHasWarning("Has warning", jobDeclaration.JE_OwnerRefInfo, warningMessage);

			jobDeclaration.JE_OwnerRef = "Owner reference";
			AssertNoWarning("Has no warning", jobDeclaration.JE_OwnerRefInfo, warningMessage);
		});

		public void TestCheckJE_GS_NKCusAgent()
		{
			const string missingWorkPhoneErrorMessage = "Please add the 'Work Phone' to the Broker's user profile. This is mandatory information for customs messages.";
			var staff = CreateStaffRecord();

			CombineAssertions(() =>
			{
				jobDeclaration.Validation.ValidateJE_GS_NKCusAgent();
				AssertNoMessageError(jobDeclaration.JE_GS_NKCusAgentInfo, missingWorkPhoneErrorMessage);

				jobDeclaration.JE_GS_NKCusAgent = staff.GS_Code;
				AssertNoMessageError(jobDeclaration.JE_GS_NKCusAgentInfo, missingWorkPhoneErrorMessage);

				staff.GS_WorkPhone = string.Empty;
				jobDeclaration.Validation.ValidateJE_GS_NKCusAgent();
				AssertHasMessageError(jobDeclaration.JE_GS_NKCusAgentInfo, missingWorkPhoneErrorMessage);
			});
		}

		public void TestCheckJE_PresentationStartDate()
		{
			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals(ZDateTime.Empty, jobDeclaration.JE_PresentationStartDate);
			jobDeclaration.Validation.ValidateJE_PresentationStartDate();
			AssertNoMessageError(jobDeclaration.JE_PresentationStartDateInfo, "The Start Date of the Presentation must not be earlier than the current date.");
			AssertNoMessageError(jobDeclaration.JE_PresentationStartDateInfo, "The Start Date of the Presentation must be earlier than the End date.");
			jobDeclaration.JE_PresentationStartDate = ZDateTime.Today.AddDays(-1);
			jobDeclaration.Validation.ValidateJE_PresentationStartDate();
			AssertHasMessageError(jobDeclaration.JE_PresentationStartDateInfo, "The Start Date of the Presentation must not be earlier than the current date.");
			AssertNoMessageError(jobDeclaration.JE_PresentationStartDateInfo, "The Start Date of the Presentation must be earlier than the End date.");
			jobDeclaration.JE_PresentationStartDate = ZDateTime.Today;
			jobDeclaration.Validation.ValidateJE_PresentationStartDate();
			AssertNoMessageError(jobDeclaration.JE_PresentationStartDateInfo, "The Start Date of the Presentation must not be earlier than the current date.");
			AssertNoMessageError(jobDeclaration.JE_PresentationStartDateInfo, "The Start Date of the Presentation must be earlier than the End date.");
			jobDeclaration.JE_PresentationEndDate = ZDateTime.Today;
			jobDeclaration.Validation.ValidateJE_PresentationStartDate();
			AssertNoMessageError(jobDeclaration.JE_PresentationStartDateInfo, "The Start Date of the Presentation must not be earlier than the current date.");
			AssertHasMessageError(jobDeclaration.JE_PresentationStartDateInfo, "The Start Date of the Presentation must be earlier than the End date.");
			jobDeclaration.JE_PresentationEndDate = ZDateTime.Today.AddDays(1);
			jobDeclaration.Validation.ValidateJE_PresentationStartDate();
			AssertNoMessageError(jobDeclaration.JE_PresentationStartDateInfo, "The Start Date of the Presentation must not be earlier than the current date.");
			AssertNoMessageError(jobDeclaration.JE_PresentationStartDateInfo, "The Start Date of the Presentation must be earlier than the End date.");
			jobDeclaration.JE_PresentationStartDate = ZDateTime.Today.AddDays(-1);
			jobDeclaration.JE_PresentationEndDate = ZDateTime.Today.AddDays(-1);
			jobDeclaration.Validation.ValidateJE_PresentationStartDate();
			AssertHasMessageError(jobDeclaration.JE_PresentationStartDateInfo, "The Start Date of the Presentation must not be earlier than the current date.");
			AssertHasMessageError(jobDeclaration.JE_PresentationStartDateInfo, "The Start Date of the Presentation must be earlier than the End date.");
			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
			jobDeclaration.Validation.ValidateJE_PresentationStartDate();
			AssertNoMessageError(jobDeclaration.JE_PresentationStartDateInfo, "The Start Date of the Presentation must not be earlier than the current date.");
			AssertNoMessageError(jobDeclaration.JE_PresentationStartDateInfo, "The Start Date of the Presentation must be earlier than the End date.");

			jobDeclaration.Logs.AddNew(Events.CustomsCommenced, "DE Import");
			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
			jobDeclaration.JE_PresentationStartDate = ZDateTime.Empty;
			jobDeclaration.JE_PresentationEndDate = ZDateTime.Empty;
			AssertEquals(ZDateTime.Empty, jobDeclaration.JE_PresentationStartDate);
			jobDeclaration.Validation.ValidateJE_PresentationStartDate();
			AssertNoMessageError(jobDeclaration.JE_PresentationStartDateInfo, "The Start Date of the Presentation must not be earlier than the current date.");
			AssertNoMessageError(jobDeclaration.JE_PresentationStartDateInfo, "The Start Date of the Presentation must be earlier than the End date.");
			jobDeclaration.JE_PresentationStartDate = ZDateTime.Today.AddDays(-1);
			jobDeclaration.Validation.ValidateJE_PresentationStartDate();
			AssertHasMessageError(jobDeclaration.JE_PresentationStartDateInfo, "The Start Date of the Presentation must not be earlier than the current date.");
			AssertNoMessageError(jobDeclaration.JE_PresentationStartDateInfo, "The Start Date of the Presentation must be earlier than the End date.");
			jobDeclaration.JE_PresentationStartDate = ZDateTime.Today;
			jobDeclaration.Validation.ValidateJE_PresentationStartDate();
			AssertNoMessageError(jobDeclaration.JE_PresentationStartDateInfo, "The Start Date of the Presentation must not be earlier than the current date.");
			AssertNoMessageError(jobDeclaration.JE_PresentationStartDateInfo, "The Start Date of the Presentation must be earlier than the End date.");
			jobDeclaration.JE_PresentationEndDate = ZDateTime.Today;
			jobDeclaration.Validation.ValidateJE_PresentationStartDate();
			AssertNoMessageError(jobDeclaration.JE_PresentationStartDateInfo, "The Start Date of the Presentation must not be earlier than the current date.");
			AssertHasMessageError(jobDeclaration.JE_PresentationStartDateInfo, "The Start Date of the Presentation must be earlier than the End date.");
			jobDeclaration.JE_PresentationEndDate = ZDateTime.Today.AddDays(1);
			jobDeclaration.Validation.ValidateJE_PresentationStartDate();
			AssertNoMessageError(jobDeclaration.JE_PresentationStartDateInfo, "The Start Date of the Presentation must not be earlier than the current date.");
			AssertNoMessageError(jobDeclaration.JE_PresentationStartDateInfo, "The Start Date of the Presentation must be earlier than the End date.");
			jobDeclaration.JE_PresentationStartDate = ZDateTime.Today.AddDays(-1);
			jobDeclaration.JE_PresentationEndDate = ZDateTime.Today.AddDays(-1);
			jobDeclaration.Validation.ValidateJE_PresentationStartDate();
			AssertHasMessageError(jobDeclaration.JE_PresentationStartDateInfo, "The Start Date of the Presentation must not be earlier than the current date.");
			AssertHasMessageError(jobDeclaration.JE_PresentationStartDateInfo, "The Start Date of the Presentation must be earlier than the End date.");
			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
			jobDeclaration.Validation.ValidateJE_PresentationStartDate();
			AssertNoMessageError(jobDeclaration.JE_PresentationStartDateInfo, "The Start Date of the Presentation must not be earlier than the current date.");
			AssertNoMessageError(jobDeclaration.JE_PresentationStartDateInfo, "The Start Date of the Presentation must be earlier than the End date.");

			jobDeclaration.Logs.AddNew(Events.ExportCustomsCommenced, "DE Export");
			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
			jobDeclaration.JE_PresentationStartDate = ZDateTime.Empty;
			jobDeclaration.JE_PresentationEndDate = ZDateTime.Empty;
			AssertEquals(ZDateTime.Empty, jobDeclaration.JE_PresentationStartDate);
			jobDeclaration.Validation.ValidateJE_PresentationStartDate();
			AssertNoMessageError(jobDeclaration.JE_PresentationStartDateInfo, "The Start Date of the Presentation must not be earlier than the current date.");
			AssertNoMessageError(jobDeclaration.JE_PresentationStartDateInfo, "The Start Date of the Presentation must be earlier than the End date.");
			jobDeclaration.JE_PresentationStartDate = ZDateTime.Today.AddDays(-1);
			jobDeclaration.Validation.ValidateJE_PresentationStartDate();
			AssertNoMessageError(jobDeclaration.JE_PresentationStartDateInfo, "The Start Date of the Presentation must not be earlier than the current date.");
			AssertNoMessageError(jobDeclaration.JE_PresentationStartDateInfo, "The Start Date of the Presentation must be earlier than the End date.");
			jobDeclaration.JE_PresentationStartDate = ZDateTime.Today;
			jobDeclaration.Validation.ValidateJE_PresentationStartDate();
			AssertNoMessageError(jobDeclaration.JE_PresentationStartDateInfo, "The Start Date of the Presentation must not be earlier than the current date.");
			AssertNoMessageError(jobDeclaration.JE_PresentationStartDateInfo, "The Start Date of the Presentation must be earlier than the End date.");
			jobDeclaration.JE_PresentationEndDate = ZDateTime.Today;
			jobDeclaration.Validation.ValidateJE_PresentationStartDate();
			AssertNoMessageError(jobDeclaration.JE_PresentationStartDateInfo, "The Start Date of the Presentation must not be earlier than the current date.");
			AssertHasMessageError(jobDeclaration.JE_PresentationStartDateInfo, "The Start Date of the Presentation must be earlier than the End date.");
			jobDeclaration.JE_PresentationEndDate = ZDateTime.Today.AddDays(1);
			jobDeclaration.Validation.ValidateJE_PresentationStartDate();
			AssertNoMessageError(jobDeclaration.JE_PresentationStartDateInfo, "The Start Date of the Presentation must not be earlier than the current date.");
			AssertNoMessageError(jobDeclaration.JE_PresentationStartDateInfo, "The Start Date of the Presentation must be earlier than the End date.");
			jobDeclaration.JE_PresentationStartDate = ZDateTime.Today.AddDays(-1);
			jobDeclaration.JE_PresentationEndDate = ZDateTime.Today.AddDays(-1);
			jobDeclaration.Validation.ValidateJE_PresentationStartDate();
			AssertNoMessageError(jobDeclaration.JE_PresentationStartDateInfo, "The Start Date of the Presentation must not be earlier than the current date.");
			AssertHasMessageError(jobDeclaration.JE_PresentationStartDateInfo, "The Start Date of the Presentation must be earlier than the End date.");
			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
			jobDeclaration.Validation.ValidateJE_PresentationStartDate();
			AssertNoMessageError(jobDeclaration.JE_PresentationStartDateInfo, "The Start Date of the Presentation must not be earlier than the current date.");
			AssertNoMessageError(jobDeclaration.JE_PresentationStartDateInfo, "The Start Date of the Presentation must be earlier than the End date.");
		}

		public void TestCheckJE_PresentationStartDate_Mandatory_Export()
		{
			jobDeclaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			var instruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			CombineAssertions(() =>
			{
				jobDeclaration.Validation.ValidateJE_PresentationStartDate();
				AssertNoMessageErrorContaining("CEI_Style is empty", jobDeclaration.JE_PresentationStartDateInfo, MandatoryValidation.YouHaveNotEntered);

				instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000200;
				jobDeclaration.Validation.ValidateJE_PresentationStartDate();
				AssertHasMessageErrorContaining("CEI_Style is '***2**'", jobDeclaration.JE_PresentationStartDateInfo, MandatoryValidation.YouHaveNotEntered);

				var instruction2 = jobDeclaration.CustomsEntryInstructions.AddNew();
				instruction2.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000100;
				jobDeclaration.Validation.ValidateJE_PresentationStartDate();
				AssertHasMessageErrorContaining("Has CEI_Style '***2**'", jobDeclaration.JE_PresentationStartDateInfo, MandatoryValidation.YouHaveNotEntered);

				instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._201300;
				jobDeclaration.Validation.ValidateJE_PresentationStartDate();
				AssertNoMessageErrorContaining("No CEI_Style '***2**'", jobDeclaration.JE_PresentationStartDateInfo, MandatoryValidation.YouHaveNotEntered);

				instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000200;
				jobDeclaration.JE_PresentationStartDate = ZDateTime.Today;
				AssertNoMessageErrorContaining("JE_PresentationStartDate isn't empty", jobDeclaration.JE_PresentationStartDateInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckJE_PresentationEndDate()
		{
			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals(ZDateTime.Empty, jobDeclaration.JE_PresentationEndDate);
			jobDeclaration.Validation.ValidateJE_PresentationEndDate();
			AssertNoMessageError(jobDeclaration.JE_PresentationEndDateInfo, "The End Date of the Presentation must not be more than 7 days in the future.");
			AssertNoMessageError(jobDeclaration.JE_PresentationEndDateInfo, "The End Date of the Presentation must be after the Start date.");
			jobDeclaration.JE_PresentationEndDate = ZDateTime.Today.AddDays(7);
			jobDeclaration.Validation.ValidateJE_PresentationEndDate();
			AssertNoMessageError(jobDeclaration.JE_PresentationEndDateInfo, "The End Date of the Presentation must not be more than 7 days in the future.");
			AssertNoMessageError(jobDeclaration.JE_PresentationEndDateInfo, "The End Date of the Presentation must be after the Start date.");
			jobDeclaration.JE_PresentationEndDate = ZDateTime.Today.AddDays(8);
			jobDeclaration.Validation.ValidateJE_PresentationEndDate();
			AssertHasMessageError(jobDeclaration.JE_PresentationEndDateInfo, "The End Date of the Presentation must not be more than 7 days in the future.");
			AssertNoMessageError(jobDeclaration.JE_PresentationEndDateInfo, "The End Date of the Presentation must be after the Start date.");
			jobDeclaration.JE_PresentationStartDate = ZDateTime.Today.AddDays(8);
			jobDeclaration.Validation.ValidateJE_PresentationEndDate();
			AssertHasMessageError(jobDeclaration.JE_PresentationEndDateInfo, "The End Date of the Presentation must not be more than 7 days in the future.");
			AssertHasMessageError(jobDeclaration.JE_PresentationEndDateInfo, "The End Date of the Presentation must be after the Start date.");
			jobDeclaration.JE_PresentationStartDate = ZDateTime.Today.AddDays(7);
			jobDeclaration.Validation.ValidateJE_PresentationEndDate();
			AssertHasMessageError(jobDeclaration.JE_PresentationEndDateInfo, "The End Date of the Presentation must not be more than 7 days in the future.");
			AssertNoMessageError(jobDeclaration.JE_PresentationEndDateInfo, "The End Date of the Presentation must be after the Start date.");
			jobDeclaration.JE_PresentationStartDate = ZDateTime.Today.AddDays(8);
			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
			jobDeclaration.Validation.ValidateJE_PresentationEndDate();
			AssertNoMessageError(jobDeclaration.JE_PresentationEndDateInfo, "The End Date of the Presentation must not be more than 7 days in the future.");
			AssertNoMessageError(jobDeclaration.JE_PresentationEndDateInfo, "The End Date of the Presentation must be after the Start date.");

			jobDeclaration.Logs.AddNew(Events.CustomsCommenced, "DE Import");
			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
			jobDeclaration.JE_PresentationStartDate = ZDateTime.Empty;
			jobDeclaration.JE_PresentationEndDate = ZDateTime.Empty;
			AssertEquals(ZDateTime.Empty, jobDeclaration.JE_PresentationEndDate);
			jobDeclaration.Validation.ValidateJE_PresentationEndDate();
			AssertNoMessageError(jobDeclaration.JE_PresentationEndDateInfo, "The End Date of the Presentation must not be more than 7 days in the future.");
			AssertNoMessageError(jobDeclaration.JE_PresentationEndDateInfo, "The End Date of the Presentation must be after the Start date.");
			jobDeclaration.JE_PresentationEndDate = ZDateTime.Today.AddDays(7);
			jobDeclaration.Validation.ValidateJE_PresentationEndDate();
			AssertNoMessageError(jobDeclaration.JE_PresentationEndDateInfo, "The End Date of the Presentation must not be more than 7 days in the future.");
			AssertNoMessageError(jobDeclaration.JE_PresentationEndDateInfo, "The End Date of the Presentation must be after the Start date.");
			jobDeclaration.JE_PresentationEndDate = ZDateTime.Today.AddDays(8);
			jobDeclaration.Validation.ValidateJE_PresentationEndDate();
			AssertHasMessageError(jobDeclaration.JE_PresentationEndDateInfo, "The End Date of the Presentation must not be more than 7 days in the future.");
			AssertNoMessageError(jobDeclaration.JE_PresentationEndDateInfo, "The End Date of the Presentation must be after the Start date.");
			jobDeclaration.JE_PresentationStartDate = ZDateTime.Today.AddDays(8);
			jobDeclaration.Validation.ValidateJE_PresentationEndDate();
			AssertHasMessageError(jobDeclaration.JE_PresentationEndDateInfo, "The End Date of the Presentation must not be more than 7 days in the future.");
			AssertHasMessageError(jobDeclaration.JE_PresentationEndDateInfo, "The End Date of the Presentation must be after the Start date.");
			jobDeclaration.JE_PresentationStartDate = ZDateTime.Today.AddDays(7);
			jobDeclaration.Validation.ValidateJE_PresentationEndDate();
			AssertHasMessageError(jobDeclaration.JE_PresentationEndDateInfo, "The End Date of the Presentation must not be more than 7 days in the future.");
			AssertNoMessageError(jobDeclaration.JE_PresentationEndDateInfo, "The End Date of the Presentation must be after the Start date.");
			jobDeclaration.JE_PresentationStartDate = ZDateTime.Today.AddDays(8);
			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
			jobDeclaration.Validation.ValidateJE_PresentationEndDate();
			AssertNoMessageError(jobDeclaration.JE_PresentationEndDateInfo, "The End Date of the Presentation must not be more than 7 days in the future.");
			AssertNoMessageError(jobDeclaration.JE_PresentationEndDateInfo, "The End Date of the Presentation must be after the Start date.");

			jobDeclaration.Logs.AddNew(Events.ExportCustomsCommenced, "DE Export");
			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
			jobDeclaration.JE_PresentationStartDate = ZDateTime.Empty;
			jobDeclaration.JE_PresentationEndDate = ZDateTime.Empty;
			AssertEquals(ZDateTime.Empty, jobDeclaration.JE_PresentationEndDate);
			jobDeclaration.Validation.ValidateJE_PresentationEndDate();
			AssertNoMessageError(jobDeclaration.JE_PresentationEndDateInfo, "The End Date of the Presentation must not be more than 7 days in the future.");
			AssertNoMessageError(jobDeclaration.JE_PresentationEndDateInfo, "The End Date of the Presentation must be after the Start date.");
			jobDeclaration.JE_PresentationEndDate = ZDateTime.Today.AddDays(7);
			jobDeclaration.Validation.ValidateJE_PresentationEndDate();
			AssertNoMessageError(jobDeclaration.JE_PresentationEndDateInfo, "The End Date of the Presentation must not be more than 7 days in the future.");
			AssertNoMessageError(jobDeclaration.JE_PresentationEndDateInfo, "The End Date of the Presentation must be after the Start date.");
			jobDeclaration.JE_PresentationEndDate = ZDateTime.Today.AddDays(8);
			jobDeclaration.Validation.ValidateJE_PresentationEndDate();
			AssertNoMessageError(jobDeclaration.JE_PresentationEndDateInfo, "The End Date of the Presentation must not be more than 7 days in the future.");
			AssertNoMessageError(jobDeclaration.JE_PresentationEndDateInfo, "The End Date of the Presentation must be after the Start date.");
			jobDeclaration.JE_PresentationStartDate = ZDateTime.Today.AddDays(8);
			jobDeclaration.Validation.ValidateJE_PresentationEndDate();
			AssertNoMessageError(jobDeclaration.JE_PresentationEndDateInfo, "The End Date of the Presentation must not be more than 7 days in the future.");
			AssertHasMessageError(jobDeclaration.JE_PresentationEndDateInfo, "The End Date of the Presentation must be after the Start date.");
			jobDeclaration.JE_PresentationStartDate = ZDateTime.Today.AddDays(7);
			jobDeclaration.Validation.ValidateJE_PresentationEndDate();
			AssertNoMessageError(jobDeclaration.JE_PresentationEndDateInfo, "The End Date of the Presentation must not be more than 7 days in the future.");
			AssertNoMessageError(jobDeclaration.JE_PresentationEndDateInfo, "The End Date of the Presentation must be after the Start date.");
			jobDeclaration.JE_PresentationStartDate = ZDateTime.Today.AddDays(8);
			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
			jobDeclaration.Validation.ValidateJE_PresentationEndDate();
			AssertNoMessageError(jobDeclaration.JE_PresentationEndDateInfo, "The End Date of the Presentation must not be more than 7 days in the future.");
			AssertNoMessageError(jobDeclaration.JE_PresentationEndDateInfo, "The End Date of the Presentation must be after the Start date.");
		}

		public void TestCheckJE_PresentationEndDate_Mandatory_Export()
		{
			jobDeclaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			var instruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			CombineAssertions(() =>
			{
				jobDeclaration.Validation.ValidateJE_PresentationEndDate();
				AssertNoMessageErrorContaining("CEI_Style is empty", jobDeclaration.JE_PresentationEndDateInfo, MandatoryValidation.YouHaveNotEntered);

				instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000200;
				jobDeclaration.Validation.ValidateJE_PresentationEndDate();
				AssertHasMessageErrorContaining("CEI_Style is '***2**'", jobDeclaration.JE_PresentationEndDateInfo, MandatoryValidation.YouHaveNotEntered);

				instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000100;
				jobDeclaration.Validation.ValidateJE_PresentationEndDate();
				AssertNoMessageErrorContaining("CEI_Style isn't '***2**'", jobDeclaration.JE_PresentationEndDateInfo, MandatoryValidation.YouHaveNotEntered);

				instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000200;
				jobDeclaration.JE_PresentationEndDate = ZDateTime.Today;
				AssertNoMessageErrorContaining("JE_PresentationEndDate isn't empty", jobDeclaration.JE_PresentationEndDateInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		GlbStaff CreateStaffRecord()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "BOB";
			staff.GS_WorkPhone = "06131474747";
			return staff;
		}

		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
		}
		JobDeclaration jobDeclaration;

		void SetupCustomsOffices()
		{
			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateCusCodeType(Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "ROLE");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE000EXP", "Office of Export", yesterday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExport);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE000EXT", "Office of Export", yesterday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExit);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "ES000EXT", "Office of Export", yesterday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExit);
			Factory.Save();
		}
	}
}
