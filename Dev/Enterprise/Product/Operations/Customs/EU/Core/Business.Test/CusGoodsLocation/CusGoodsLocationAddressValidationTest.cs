using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class CusGoodsLocationAddressValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateE2_Address1()
		{
			validation.ValidateE2_Address1();
			AssertNoNotifications("No validation should be triggered", locationAddress.E2_Address1Info);
		}

		public void TestValidateE2_Address1AndE2_Address2()
		{
			validation.ValidateE2_Address1AndE2_Address2();
			AssertNoNotifications("No validation should be triggered", locationAddress.E2_Address1AndE2_Address2Info);
		}

		public void TestValidateE2_Postcode()
		{
			var expectedWarning = "The entered postcode does not comply with the postcode format rules of the country/region (LV-NNNN). The postcode validation rule for the country/region Latvia is currently set to \"Must Be Entered\".\r\n\r\nIf you wish to change this setting, please contact your system administrator to change the validation rule field on the country/region record in Maintain->Locations->Countries/Regions.";

			var goodsLocation = Factory.New<CusGoodsLocation>();
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
			var locationAddress = goodsLocation.Address;
			locationAddress.E2_AddressOverride = ZBool.True;
			locationAddress.IgnoreValidationStatusError = true;
			locationAddress.E2_Postcode = "712523";
			locationAddress.Validation.ValidateE2_Postcode();
			AssertHasWarningContaining("Validation should be triggered", locationAddress.E2_PostcodeInfo, expectedWarning);

			locationAddress.E2_Postcode = "LV-1073";
			locationAddress.Validation.ValidateE2_Postcode();
			AssertNoWarningContaining("Validation should be triggered", locationAddress.E2_PostcodeInfo, expectedWarning);
		}

		public void TestValidateE2_City()
		{
			validation.ValidateE2_City();
			AssertNoNotifications("No validation should be triggered", locationAddress.E2_CityInfo);
		}

		public void TestValidateE2_RN_NKCountryCode()
		{
			validation.ValidateE2_RN_NKCountryCode();
			AssertNoNotifications("No validation should be triggered", locationAddress.E2_RN_NKCountryCodeInfo);
		}

		public void TestValidateE2_Email()
		{
			CombineAssertions(() =>
			{
				locationAddress.E2_Email = "a bc@email.com";
				AssertHasNotifications("Invalid email address", locationAddress.E2_EmailInfo);

				locationAddress.E2_Email = "abc@email.com";
				AssertNoNotifications("Valid email address", locationAddress.E2_EmailInfo);
			});
		}

		public void TestValidateE2_GovRegNum()
		{
			validation.ValidateE2_GovRegNum();
			AssertNoNotifications("No validation should be triggered", locationAddress.E2_GovRegNumInfo);
		}

		public void TestRule063()
		{
			var temporaryStorageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();

			var goodslocation = temporaryStorageHeader.GoodsLocation;
			var latitudePropertyInfo = goodslocation.Address.E2_LatitudeInfo;
			var longitudePropertyInfo = goodslocation.Address.E2_LongitudeInfo;
			var messageLatitude = "[C0063] Location: GNSS Latitude required when qualifier is 'W'.";
			var messageLongitude = "[C0063] Location: GNSS Longitude required when qualifier is 'W'.";

			CombineAssertions(() =>
			{
				goodslocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.GnssCoordinates;
				locationAddress.Validation.ValidateE2_GeoLocation();
				AssertHasMessageError("No GNSS value, latitudeInfo: " + messageLatitude, latitudePropertyInfo, messageLatitude);
				AssertHasMessageError("No GNSS value, longitudeInfo: " + messageLongitude, longitudePropertyInfo, messageLongitude);

				goodslocation.Address.E2_Latitude = 10;
				locationAddress.Validation.ValidateE2_GeoLocation();
				AssertNoMessageError("There is only a latitude: , latitudeInfo: " + messageLatitude, latitudePropertyInfo, messageLatitude);
				AssertHasMessageError("There is only a latitude: , longitudeInfo: " + messageLongitude, longitudePropertyInfo, messageLongitude);

				goodslocation.Address.E2_Longitude = 10;
				locationAddress.Validation.ValidateE2_GeoLocation();
				AssertNoMessageError("There is latitude and longitude , latitudeInfo: " + messageLatitude, latitudePropertyInfo, messageLatitude);
				AssertNoMessageError("There is latitude and longitude , longitudeInfo: " + messageLongitude, longitudePropertyInfo, messageLongitude);

				goodslocation.Address.E2_Latitude = 0;
				locationAddress.Validation.ValidateE2_GeoLocation();
				AssertHasMessageError("There is only a longitude: , latitudeInfo: " + messageLatitude, latitudePropertyInfo, messageLatitude);
				AssertNoMessageError("There is only a longitude: , longitudeInfo: " + messageLongitude, longitudePropertyInfo, messageLongitude);

				goodslocation.Address.E2_Longitude = 0;
				goodslocation.Address.E2_Latitude = 0;
				goodslocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
				locationAddress.Validation.ValidateE2_GeoLocation();
				AssertNoMessageError("On empty when qualifier is GNSS and qualifier is changed, message error should no longer be added, latitudeInfo: " + messageLatitude, latitudePropertyInfo, messageLatitude);
				AssertNoMessageError("On empty when qualifier is GNSS and qualifier is changed, message error should no longer be added, longitudeInfo: " + messageLongitude, longitudePropertyInfo, messageLongitude);
			});
		}

		public void TestRule064()
		{
			var temporaryStorageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();

			var goodslocation = temporaryStorageHeader.GoodsLocation;
			var propertyInfoEori = goodslocation.Address.E2_GovRegNumInfo;
			var propertyInfoOrganisation = goodslocation.Address.E2_AdditionalAddressInformationInfo;
			var messageEORI = "[C0064] Location: EORI Number required when qualifier is 'X'.";
			var messageOrganisation = "[C0064] Location: Organization required when qualifier is 'X'.";

			goodslocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
			AssertHasMessageError("Neither EORI nor organisation, eori: " + messageEORI, propertyInfoEori, messageEORI);
			AssertHasMessageError("Neither EORI nor organisation, organisation: " + messageOrganisation, propertyInfoOrganisation, messageOrganisation);

			goodslocation.Address.E2_GovRegNum = "eori";
			AssertNoMessageError("There is only an EORI, eori: " + messageEORI, propertyInfoEori, messageEORI);
			AssertHasMessageError("There is only an EORI, organisation: " + messageOrganisation, propertyInfoOrganisation, messageOrganisation);

			goodslocation.Address.E2_AdditionalAddressInformation = ZGuid.BrettsGuid.ToString();
			goodslocation.Address.E2_GovRegNum = "eori";
			AssertNoMessageError("There is EORI and organization, eori: " + messageEORI, propertyInfoEori, messageEORI);
			AssertNoMessageError("There is EORI and organisation, organisation: " + messageOrganisation, propertyInfoOrganisation, messageOrganisation);

			goodslocation.Address.E2_GovRegNum = ZString.Empty;
			AssertHasMessageError("There is only an organization, eori: " + messageEORI, propertyInfoEori, messageEORI);
			AssertNoMessageError("There is only an organization, organisation: " + messageOrganisation, propertyInfoOrganisation, messageOrganisation);
		}

		public void TestRule065()
		{
			var temporaryStorageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();

			var goodslocation = temporaryStorageHeader.GoodsLocation;
			var propertyInfoAuthorisation = goodslocation.Address.E2_GovRegNumInfo;
			var propertyInfoOrganisation = goodslocation.Address.E2_AdditionalAddressInformationInfo;
			var messageAuthorisation = "[C0065] Location: Authorization No. required when qualifier is 'Y'.";
			var messageOrganisation = "[C0065] Location: Organization required when qualifier is 'Y'.";

			goodslocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			AssertHasMessageError("Neither authorisation number nor organisation: " + messageAuthorisation, propertyInfoAuthorisation, messageAuthorisation);
			AssertHasMessageError("Neither EORI nor organisation, organisation: " + messageOrganisation, propertyInfoOrganisation, messageOrganisation);

			goodslocation.Address.AuthorisationNumber = "auth";
			AssertNoMessageError("There is only an authorisation number: " + messageAuthorisation, propertyInfoAuthorisation, messageAuthorisation);
			AssertHasMessageError("There is only an EORI, organisation: " + messageOrganisation, propertyInfoOrganisation, messageOrganisation);

			goodslocation.Address.E2_AdditionalAddressInformation = ZGuid.BrettsGuid.ToString();
			goodslocation.Address.AuthorisationNumber = "auth";
			AssertNoMessageError("There is authorisation number and organization: " + messageAuthorisation, propertyInfoAuthorisation, messageAuthorisation);
			AssertNoMessageError("There is EORI and organisation, organisation: " + messageOrganisation, propertyInfoOrganisation, messageOrganisation);

			goodslocation.Address.AuthorisationNumber = ZString.Empty;
			AssertHasMessageError("There is only an organization: " + messageAuthorisation, propertyInfoAuthorisation, messageAuthorisation);
			AssertNoMessageError("There is only an organization, organisation: " + messageOrganisation, propertyInfoOrganisation, messageOrganisation);
		}

		protected override void SetUp()
		{
			base.SetUp();
			locationAddress = Factory.New<CusGoodsLocationAddress>();
			locationAddress.E2_AddressOverride = ZBool.True;
			locationAddress.OverrideRequirement = new JobDocAddressRequirementForTest();
			validation = locationAddress.Validation;
		}
		CusGoodsLocationAddress locationAddress;
		CusGoodsLocationAddressValidation validation;

		class JobDocAddressRequirementForTest : JobDocAddressRequirement
		{
			public JobDocAddressRequirementForTest()
			{
				ValidateAddress1 = ValidateE2_Address1;
				ValidatePostCode = ValidateE2_Postcode;
				ValidateCity = ValidateE2_City;
				ValidateCountry = ValidateE2_RN_NKCountryCode;
				ValidateGovRegNo = ValidateE2_GovRegNum;
			}

			void ValidateE2_Address1(JobDocAddressValidation validation) => ValidateAddressProperty(validation, a => a.E2_Address1Info);

			void ValidateE2_Postcode(JobDocAddressValidation validation) => ValidateAddressProperty(validation, a => a.E2_PostcodeInfo);

			void ValidateE2_City(JobDocAddressValidation validation) => ValidateAddressProperty(validation, a => a.E2_CityInfo);

			void ValidateE2_RN_NKCountryCode(JobDocAddressValidation validation) => ValidateAddressProperty(validation, a => a.E2_RN_NKCountryCodeInfo);

			void ValidateE2_GovRegNum(JobDocAddressValidation validation) => ValidateAddressProperty(validation, a => a.E2_GovRegNumInfo);

			void ValidateAddressProperty(JobDocAddressValidation validation, Func<JobDocAddress, ZPropertyInfo> getPropertyInfo)
			{
				var docAddress = validation.Parent;
				getPropertyInfo(docAddress).AddMessageError("Unexpected validation");
			}
		}
	}
}
