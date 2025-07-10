using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants.Customs.Universal;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeList;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class AsycudaBillValidationForMasterChildTest : BusinessObjectValidationTestCase
	{
		public void TestCheckMandatoryABL_E_DEP()
		{
			CombineAssertions(() =>
			{
				var message = "An Estimated Departure Time is required";
				manifestHeader.AMA_TransportMode = TransportTypeList.Codes.Sea;
				ValidationTestHelper.AssertFieldIsNotMandatory(bill.ABL_E_DEPInfo, message);

				manifestHeader.AMA_TransportMode = TransportTypeList.Codes.InlandWaterwayTransport;
				ValidationTestHelper.AssertFieldIsNotMandatory(bill.ABL_E_DEPInfo, message);
			});
		}

		public void TestCheckMandatoryABL_E_DEP_CarrierManifest()
		{
			var message = "An Estimated Departure Time is required";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");
			var validationRule = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.EstimatedDepartureTime, message, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(validationRule.PK, ManifestValidationRuleCodes.Mandatory, ZString.Empty);

			var deCon = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, RefCusCodeListTypes.Codes.ManifestValidationRule, "ZZDNAME", "ZZDNAME", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTransportModeForCusCodeList(deCon.PK, Core.Constants.TransportModes.Road);
			helper.CreateTransportModeForCusCodeList(deCon.PK, Core.Constants.TransportModes.Rail);
			helper.CreateTransportModeForCusCodeList(deCon.PK, Core.Constants.TransportModes.Air);
			Factory.Save();

			manifestHeader.AMA_TransportMode = TransportTypeList.Codes.Road;
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F40;
			ValidationTestHelper.AssertFieldIsNotMandatory(bill.ABL_E_DEPInfo, message);

			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F50;
			ValidationTestHelper.AssertFieldIsNotMandatory(bill.ABL_E_DEPInfo, message);

			manifestHeader.AMA_TransportMode = TransportTypeList.Codes.Rail;
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F41;
			ValidationTestHelper.AssertFieldIsNotMandatory(bill.ABL_E_DEPInfo, message);

			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F51;
			ValidationTestHelper.AssertFieldIsNotMandatory(bill.ABL_E_DEPInfo, message);

			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F44;
			ValidationTestHelper.AssertFieldIsNotMandatory(bill.ABL_E_DEPInfo, message);

			manifestHeader.SuspendCheckBusinessObjectType();
			manifestHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.Germany;
			manifestHeader.AMA_TransportMode = TransportTypeList.Codes.Air;
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F21;
			bill.ABL_E_DEP = ZDateTime.Empty;
			AssertHasMessageErrorContaining(bill.ABL_E_DEPInfo, message);
		}

		public void TestCheckMandatoryABL_E_ARV()
		{
			CombineAssertions(() =>
			{
				var message = "An Estimated Departure Time is required";
				manifestHeader.AMA_TransportMode = TransportTypeList.Codes.Sea;
				ValidationTestHelper.AssertFieldIsNotMandatory(bill.ABL_E_ARVInfo, message);

				manifestHeader.AMA_TransportMode = TransportTypeList.Codes.InlandWaterwayTransport;
				ValidationTestHelper.AssertFieldIsNotMandatory(bill.ABL_E_ARVInfo, message);

				manifestHeader.AMA_TransportMode = TransportTypeList.Codes.Road;
				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F44;
				ValidationTestHelper.AssertFieldIsNotMandatory(bill.ABL_E_ARVInfo, message);
			});
		}

		public void TestCheckAMA_RL_NKPortOfLoading()
		{
			const string messageError = "The Load port must not be an EU port.";

			foreach (var country in CountriesToTest)
			{
				manifestHeader.MasterBill.ABL_RL_NKPortOfLoading = country + "XXX";
				AssertHasMessageError(bill.ABL_RL_NKPortOfLoadingInfo, messageError);
			}

			bill.ABL_RL_NKPortOfLoading = "USLAX";
			AssertNoMessageError(bill.ABL_RL_NKPortOfLoadingInfo, messageError);

			bill.ABL_RL_NKPortOfLoading = "GB";
			AssertNoMessageError(bill.ABL_RL_NKPortOfLoadingInfo, messageError);
		}

		public void TestCheckABL_RL_NKPortOfLoading_CarrierManifest()
		{
			var message = "You have not entered a Port Of Loading.";

			ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyHasValue(bill.ABL_RL_NKPortOfLoadingInfo, bill.Header.SpecificCircumstanceIndicatorInfo, (ZString)EUICS2SpecificCircumstanceList.Codes.F40, true, message);
		}

		public void TestCheckABL_RL_NKPortOfDischarge_CarrierManifest()
		{
			var message = "You have not entered a Port of Unloading.";

			ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyHasValue(bill.ABL_RL_NKPortOfDischargeInfo, bill.Header.SpecificCircumstanceIndicatorInfo, (ZString)EUICS2SpecificCircumstanceList.Codes.F40, true, message);
		}

		public void TestCheckTransportDocumentType()
		{
			var message = "You have not entered a Transport Document Type.";
			var validation = bill.Validation as AsycudaBillValidationForMasterChild;

			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F40;
			validation.ValidateTransportDocumentType();
			AssertHasMessageError(bill.TransportDocumentTypeInfo, message);

			bill.TransportDocumentType = "N741";
			validation.ValidateTransportDocumentType();
			AssertNoMessageError(bill.TransportDocumentTypeInfo, message);

			bill.TransportDocumentType = ZString.Empty;
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F41;
			validation.ValidateTransportDocumentType();
			AssertNoMessageError(bill.TransportDocumentTypeInfo, message);

			manifestHeader.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;

			manifestHeader.AMA_TransportMode = TransportTypeList.Codes.Air;
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F14;
			AssertNoMessageErrors(bill.TransportDocumentTypeInfo);

			manifestHeader.AMA_TransportMode = TransportTypeList.Codes.Sea;
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F10;
			AssertNoMessageErrors(bill.TransportDocumentTypeInfo);

			foreach (var specificCircumstanceIndicator in new[] { EUICS2SpecificCircumstanceList.Codes.F14, EUICS2SpecificCircumstanceList.Codes.F15, EUICS2SpecificCircumstanceList.Codes.F16, EUICS2SpecificCircumstanceList.Codes.F17 })
			{
				manifestHeader.SpecificCircumstanceIndicator = specificCircumstanceIndicator;

				bill.TransportDocumentType = ZString.Empty;
				AssertHasMessageErrorContaining(bill.TransportDocumentTypeInfo, MandatoryValidation.YouHaveNotEntered);

				bill.TransportDocumentType = "XXXX";
				AssertHasMessageErrorContaining(bill.TransportDocumentTypeInfo, ListValidation.InvalidCodeMessageError);

				bill.TransportDocumentType = "N741";
				AssertNoMessageErrors(bill.TransportDocumentTypeInfo);
			}
		}

		public void TestCheckABL_RL_NKOrigin()
		{
			manifestHeader.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			var validation = bill.Validation as AsycudaBillValidationForMasterChild;
			var targetInfo = bill.ABL_RL_NKOriginInfo;

			manifestHeader.AMA_TransportMode = TransportTypeList.Codes.Road;
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F40;
			ValidationTestHelper.AssertFieldIsNotMandatory(targetInfo);

			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F11;
			ValidationTestHelper.AssertFieldIsNotMandatory(targetInfo);

			manifestHeader.AMA_TransportMode = TransportTypeList.Codes.Sea;
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F11;
			ValidationTestHelper.AssertFieldIsMandatory(targetInfo, MandatoryValidation.YouHaveNotEntered);

			manifestHeader.AMA_TransportMode = TransportTypeList.Codes.InlandWaterwayTransport;
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F11;
			ValidationTestHelper.AssertFieldIsMandatory(targetInfo, MandatoryValidation.YouHaveNotEntered);

			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F12;
			ValidationTestHelper.AssertFieldIsMandatory(targetInfo, MandatoryValidation.YouHaveNotEntered);

			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F40;
			ValidationTestHelper.AssertFieldIsNotMandatory(targetInfo, MandatoryValidation.YouHaveNotEntered);

			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F12;
			bill.ABL_RL_NKOrigin = "XXX";
			AssertHasMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);

			bill.ABL_RL_NKOrigin = "AUSYD";
			AssertNoMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckABL_RL_NKFinalDestination()
		{
			manifestHeader.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			var validation = bill.Validation as AsycudaBillValidationForMasterChild;
			var targetInfo = bill.ABL_RL_NKFinalDestinationInfo;

			manifestHeader.AMA_TransportMode = TransportTypeList.Codes.Road;
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F40;
			ValidationTestHelper.AssertFieldIsNotMandatory(targetInfo);

			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F11;
			ValidationTestHelper.AssertFieldIsNotMandatory(targetInfo);

			manifestHeader.AMA_TransportMode = TransportTypeList.Codes.Sea;
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F11;
			ValidationTestHelper.AssertFieldIsMandatory(targetInfo, MandatoryValidation.YouHaveNotEntered);

			manifestHeader.AMA_TransportMode = TransportTypeList.Codes.InlandWaterwayTransport;
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F11;
			ValidationTestHelper.AssertFieldIsMandatory(targetInfo, MandatoryValidation.YouHaveNotEntered);

			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F12;
			ValidationTestHelper.AssertFieldIsMandatory(targetInfo, MandatoryValidation.YouHaveNotEntered);

			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F40;
			ValidationTestHelper.AssertFieldIsNotMandatory(targetInfo, MandatoryValidation.YouHaveNotEntered);

			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F12;
			bill.ABL_RL_NKFinalDestination = "XXX";
			AssertHasMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);

			bill.ABL_RL_NKFinalDestination = "AUSYD";
			AssertNoMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckShipper()
		{
			const string messageError = "A Shipper is required.";
			manifestHeader.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			manifestHeader.AMA_TransportMode = TransportTypeList.Codes.Sea;
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F11;

			AssertRequiredFieldsHaveMessageErrors();

			manifestHeader.AMA_TransportMode = TransportTypeList.Codes.Road;
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F11;
			AssertNoFieldsHaveMessageError("Header parties not visible conditions");

			manifestHeader.AMA_TransportMode = TransportTypeList.Codes.Sea;
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F11;
			AssertRequiredFieldsHaveMessageErrors();

			bill.ABL_OA_Shipper = Factory.New<OrgHeader>().MainAddress.PK;
			bill.ABL_ShipperName = "Shipper_Name";
			bill.ABL_ShipperCity = "Shipper_City";
			bill.ABL_RN_NKShipperCountry = Core.Constants.CountryCodes.Germany;
			bill.ABL_ShipperPostcode = "123456";
			AssertNoFieldsHaveMessageError("Shipper details filled");

			bill.ABL_ShipperPostcode = string.Empty;
			AssertHasMessageError("Shipper Post code when country filled", bill.ABL_ShipperPostcodeInfo, messageError);

			void AssertRequiredFieldsHaveMessageErrors()
			{
				bill.Validation.ValidateAll();
				CombineAssertions("Header parties visible conditions", () =>
				{
					AssertHasMessageError("Shipper name", bill.ABL_ShipperNameInfo, messageError);
					AssertHasMessageError("Shipper City", bill.ABL_ShipperCityInfo, messageError);
					AssertHasMessageError("Shipper Country", bill.ABL_RN_NKShipperCountryInfo, messageError);
					AssertNoMessageError("Shipper Street1", bill.ABL_ShipperStreet1Info, messageError);
					AssertNoMessageError("Shipper Post code", bill.ABL_ShipperPostcodeInfo, messageError);
					AssertHasMessageError("Shipper Person type", bill.ShipperPersonTypeInfo, messageError);
					AssertHasMessageError("Shipper Org", bill.ABL_OA_ShipperInfo, messageError);
				});
			}
			void AssertNoFieldsHaveMessageError(string message)
			{
				bill.Validation.ValidateAll();
				CombineAssertions(message, () =>
				{
					AssertNoMessageError("Shipper name", bill.ABL_ShipperNameInfo, messageError);
					AssertNoMessageError("Shipper City", bill.ABL_ShipperCityInfo, messageError);
					AssertNoMessageError("Shipper Country", bill.ABL_RN_NKShipperCountryInfo, messageError);
					AssertNoMessageError("Shipper Street1", bill.ABL_ShipperStreet1Info, messageError);
					AssertNoMessageError("Shipper Post code", bill.ABL_ShipperPostcodeInfo, messageError);
					AssertNoMessageError("Shipper Person type", bill.ShipperPersonTypeInfo, messageError);
					AssertNoMessageError("Shipper Org", bill.ABL_OA_ShipperInfo, messageError);
				});
			}
		}

		public void TestCheckShipperCountryInList()
		{
			manifestHeader.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			manifestHeader.AMA_TransportMode = TransportTypeList.Codes.Sea;
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F11;
			ValidationTestHelper.AssertInvalidCodeMessageError(bill.ABL_RN_NKShipperCountryInfo, "AA", Core.Constants.CountryCodes.Sweden);
		}

		public void TestCheckABL_BillNumber()
		{
			const string msgError = "Manifest No. is required.";

			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_TransportMode = TransportTypeList.Codes.Road;
			manifestHeader.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F40;

			var bill = manifestHeader.MasterBill;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(bill.ABL_BillNumberInfo, msgError);

			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F44;
			ValidationTestHelper.AssertFieldIsNotMandatory(bill.ABL_BillNumberInfo);
		}

		IEnumerable<string> CountriesToTest
		{
			get { return Factory.GetEuropeanUnionForCustomsMembers().Union([Core.Constants.CountryCodes.Switzerland, Core.Constants.CountryCodes.Norway]); }
		}

		protected override void SetUp()
		{
			base.SetUp();
			manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			bill = manifestHeader.MasterBill;
		}

		AsycudaManifestHeader manifestHeader;
		AsycudaBill bill;
	}
}
