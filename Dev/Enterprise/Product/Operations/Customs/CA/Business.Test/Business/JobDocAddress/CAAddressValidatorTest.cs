using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CAAddressValidatorTest : BusinessObjectValidationTestCase
	{
		public void TestAddressIsEmptyOrWithoutData()
		{
			const string capiton = "Caption";

			docAddress = Factory.New<JobDocAddress>();

			using (docAddress.SuspendValidationTesting())
			using (docAddress.GetValidationSuspender())
			{
				docAddress.E2_OA_AddressInfo.ClearAllNotifications();
				CAAddressValidator.ValidateMandatory(docAddress, docAddress.E2_OA_AddressInfo, capiton);
				AssertHasMessageError(docAddress.E2_OA_AddressInfo, CAAddressValidator.GetAddressNotConfiguredMessageError(capiton));

				docAddress.OrganisationPK = Factory.New<OrgHeader>().PK;
				docAddress.E2_OA_AddressInfo.ClearAllNotifications();
				CAAddressValidator.ValidateMandatory(docAddress, docAddress.E2_OA_AddressInfo, capiton);
				AssertHasMessageErrorContaining(docAddress.E2_OA_AddressInfo, CAAddressValidator.GetAddressIsInvalidMessageError(capiton));

				docAddress.OrganisationPK = ZGuid.Empty;
				docAddress.E2_AddressOverride = true;
				docAddress.E2_RN_NKCountryCode = "";
				docAddress.E2_OA_AddressInfo.ClearAllNotifications();
				CAAddressValidator.ValidateMandatory(docAddress, docAddress.E2_OA_AddressInfo, capiton);
				AssertHasMessageErrorContaining(docAddress.E2_OA_AddressInfo, CAAddressValidator.GetAddressIsInvalidMessageError(capiton));

				docAddress.E2_CompanyName = "X";
				docAddress.E2_Address1 = "X";
				docAddress.E2_City = "X";
				docAddress.E2_RN_NKCountryCode = "X";
				docAddress.E2_OA_AddressInfo.ClearAllNotifications();
				CAAddressValidator.ValidateMandatory(docAddress, docAddress.E2_OA_AddressInfo, capiton);
				AssertNoMessageErrors(docAddress.E2_OA_AddressInfo);
			}
		}

		public void TestAddressValidationName()
		{
			using (docAddress.SuspendValidationTesting())
			using (docAddress.GetValidationSuspender())
			{
				docAddress.E2_OA_AddressInfo.ClearAllNotifications();
				CAAddressValidator.ValidateMandatory(docAddress, docAddress.E2_OA_AddressInfo);
				AssertHasMessageErrorContaining(docAddress.E2_OA_AddressInfo, "You have not entered a name");
				docAddress.E2_CompanyName = "12345678901234567890123456789012345678901234567890";
				docAddress.E2_OA_AddressInfo.ClearAllNotifications();
				CAAddressValidator.ValidateMandatory(docAddress, docAddress.E2_OA_AddressInfo);
				AssertNoMessageErrorContaining(docAddress.E2_OA_AddressInfo, "You have not entered a name");
				AssertHasWarningContaining(docAddress.E2_OA_AddressInfo, "The length of data entered into name (50) exceeds the maximum allowed. So name will be split and the excess data transmitted to Customs in the second line");
				docAddress.E2_CompanyName = "X";
				docAddress.E2_Address1 = "X";
				docAddress.E2_City = "X";
				docAddress.E2_RN_NKCountryCode = "X";
				docAddress.E2_OA_AddressInfo.ClearAllNotifications();
				CAAddressValidator.ValidateMandatory(docAddress, docAddress.E2_OA_AddressInfo);
				AssertNoWarningContaining(docAddress.E2_OA_AddressInfo, "The length of data entered into name (50) exceeds the maximum allowed. So name will be split and the excess data transmitted to Customs in the second line");
				AssertNoMessageErrors(docAddress.E2_OA_AddressInfo);
			}
		}

		public void TestAddressValidationAddress1()
		{
			using (docAddress.SuspendValidationTesting())
			using (docAddress.GetValidationSuspender())
			{
				docAddress.E2_Address1 = "123456789012345678901234567890123456";
				docAddress.E2_OA_AddressInfo.ClearAllNotifications();
				CAAddressValidator.Validate(docAddress, docAddress.E2_OA_AddressInfo);
				AssertHasWarningContaining(docAddress.E2_OA_AddressInfo, "The length of data entered into address line 1 (36) exceeds the maximum allowed. So address line 1 will be split and the excess data transmitted to Customs in the second line");
				docAddress.E2_Address1 = "12345678901234567890123456789012345";
				docAddress.E2_OA_AddressInfo.ClearAllNotifications();
				CAAddressValidator.Validate(docAddress, docAddress.E2_OA_AddressInfo);
				AssertNoWarningContaining(docAddress.E2_OA_AddressInfo, "The length of data entered into address line 1 (36) exceeds the maximum allowed. So address line 1 will be split and the excess data transmitted to Customs in the second line");
				docAddress.E2_Address1 = "";
				docAddress.E2_OA_AddressInfo.ClearAllNotifications();
				CAAddressValidator.ValidateMandatory(docAddress, docAddress.E2_OA_AddressInfo);
				AssertHasMessageErrorContaining(docAddress.E2_OA_AddressInfo, "You have not entered an address line 1");
				docAddress.E2_Address1 = "12345678901234567890123456789012345";
				docAddress.E2_OA_AddressInfo.ClearAllNotifications();
				CAAddressValidator.ValidateMandatory(docAddress, docAddress.E2_OA_AddressInfo);
				AssertNoMessageErrorContaining(docAddress.E2_OA_AddressInfo, "You have not entered an address line 1");
			}
		}

		public void TestAddressValidationAddress2()
		{
			using (docAddress.SuspendValidationTesting())
			using (docAddress.GetValidationSuspender())
			{
				docAddress.E2_Address2 = "123456789012345678901234567890123456";
				docAddress.E2_OA_AddressInfo.ClearAllNotifications();
				CAAddressValidator.Validate(docAddress, docAddress.E2_OA_AddressInfo);
				AssertHasWarningContaining(docAddress.E2_OA_AddressInfo, "The length of data entered into address line 2 (36) exceeds the maximum allowed. Only the first 35 characters will be transmitted to Customs");
				docAddress.E2_Address2 = "12345678901234567890123456789012345";
				docAddress.E2_OA_AddressInfo.ClearAllNotifications();
				CAAddressValidator.Validate(docAddress, docAddress.E2_OA_AddressInfo);
				AssertNoWarningContaining(docAddress.E2_OA_AddressInfo, "The length of data entered into address line 2 (36) exceeds the maximum allowed. Only the first 35 characters will be transmitted to Customs");
			}
		}

		public void TestAddressValidationCity()
		{
			using (docAddress.SuspendValidationTesting())
			using (docAddress.GetValidationSuspender())
			{
				docAddress.E2_City = "";
				docAddress.E2_OA_AddressInfo.ClearAllNotifications();
				CAAddressValidator.ValidateMandatory(docAddress, docAddress.E2_OA_AddressInfo);
				AssertHasMessageErrorContaining(docAddress.E2_OA_AddressInfo, "You have not entered a city");
				docAddress.E2_City = "12345";
				docAddress.E2_OA_AddressInfo.ClearAllNotifications();
				CAAddressValidator.ValidateMandatory(docAddress, docAddress.E2_OA_AddressInfo);
				AssertNoMessageErrorContaining(docAddress.E2_OA_AddressInfo, "You have not entered a city");
			}
		}

		public void TestAddressValidationCountry()
		{
			using (docAddress.SuspendValidationTesting())
			using (docAddress.GetValidationSuspender())
			{
				docAddress.E2_RN_NKCountryCode = "";
				docAddress.E2_OA_AddressInfo.ClearAllNotifications();
				CAAddressValidator.ValidateMandatory(docAddress, docAddress.E2_OA_AddressInfo);
				AssertHasMessageErrorContaining(docAddress.E2_OA_AddressInfo, "You have not entered a country/region code");
				docAddress.E2_RN_NKCountryCode = "CA";
				docAddress.E2_OA_AddressInfo.ClearAllNotifications();
				CAAddressValidator.ValidateMandatory(docAddress, docAddress.E2_OA_AddressInfo);
				AssertNoMessageErrorContaining(docAddress.E2_OA_AddressInfo, "You have not entered a country/region code");
			}
		}

		public void TestAddressValidationState()
		{
			using (docAddress.SuspendValidationTesting())
			using (docAddress.GetValidationSuspender())
			{
				docAddress.E2_State = "1234567890";
				docAddress.E2_OA_AddressInfo.ClearAllNotifications();
				CAAddressValidator.Validate(docAddress, docAddress.E2_OA_AddressInfo);
				AssertHasWarningContaining(docAddress.E2_OA_AddressInfo, "The length of data entered into province / state (10) exceeds the maximum allowed. Only the first 9 characters will be transmitted to Customs");
				docAddress.E2_State = "123456789";
				docAddress.E2_OA_AddressInfo.ClearAllNotifications();
				CAAddressValidator.Validate(docAddress, docAddress.E2_OA_AddressInfo);
				AssertNoWarningContaining(docAddress.E2_OA_AddressInfo, "The length of data entered into province / state (10) exceeds the maximum allowed. Only the first 9 characters will be transmitted to Customs");

				docAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
				docAddress.E2_OA_AddressInfo.ClearAllNotifications();
				CAAddressValidator.Validate(docAddress, docAddress.E2_OA_AddressInfo);
				AssertHasMessageErrorContaining(docAddress.E2_OA_AddressInfo, "Invalid Canadian province code");
				docAddress.E2_State = "ON";
				docAddress.E2_OA_AddressInfo.ClearAllNotifications();
				CAAddressValidator.Validate(docAddress, docAddress.E2_OA_AddressInfo);
				AssertNoMessageErrorContaining(docAddress.E2_OA_AddressInfo, "Invalid Canadian province code");

				docAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
				docAddress.E2_OA_AddressInfo.ClearAllNotifications();
				docAddress.E2_State = "XX";
				CAAddressValidator.Validate(docAddress, docAddress.E2_OA_AddressInfo);
				AssertHasMessageErrorContaining(docAddress.E2_OA_AddressInfo, "Invalid US state code");
				docAddress.E2_State = "MI";
				docAddress.E2_OA_AddressInfo.ClearAllNotifications();
				CAAddressValidator.Validate(docAddress, docAddress.E2_OA_AddressInfo);
				AssertNoMessageErrorContaining(docAddress.E2_OA_AddressInfo, "Invalid US state code");

				docAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
				docAddress.E2_OA_AddressInfo.ClearAllNotifications();
				docAddress.E2_State = "XX";
				CAAddressValidator.Validate(docAddress, docAddress.E2_OA_AddressInfo);
				AssertNoMessageErrors(docAddress.E2_OA_AddressInfo);
			}
		}

		public void TestAddressValidationPostCode()
		{
			using (docAddress.SuspendValidationTesting())
			using (docAddress.GetValidationSuspender())
			{
				docAddress.E2_Postcode = "1234567890";
				docAddress.E2_OA_AddressInfo.ClearAllNotifications();
				CAAddressValidator.Validate(docAddress, docAddress.E2_OA_AddressInfo);
				AssertHasWarningContaining(docAddress.E2_OA_AddressInfo, "The length of data entered into postal / zip code (10) exceeds the maximum allowed. Only the first 9 characters will be transmitted to Customs");
				docAddress.E2_Postcode = "123456789";
				docAddress.E2_OA_AddressInfo.ClearAllNotifications();
				CAAddressValidator.Validate(docAddress, docAddress.E2_OA_AddressInfo);
				AssertNoWarningContaining(docAddress.E2_OA_AddressInfo, "The length of data entered into postal / zip code (10) exceeds the maximum allowed. Only the first 9 characters will be transmitted to Customs");

				docAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
				docAddress.E2_Postcode = "";
				docAddress.E2_OA_AddressInfo.ClearAllNotifications();
				CAAddressValidator.Validate(docAddress, docAddress.E2_OA_AddressInfo);
				AssertHasMessageErrorContaining(docAddress.E2_OA_AddressInfo, "A postal/zip code is required when the country/region is Canada or United States");
				docAddress.E2_Postcode = "999999";
				docAddress.E2_OA_AddressInfo.ClearAllNotifications();
				CAAddressValidator.Validate(docAddress, docAddress.E2_OA_AddressInfo);
				AssertNoMessageErrorContaining(docAddress.E2_OA_AddressInfo, "A postal/zip code is required when the country/region is Canada or United States");
				AssertHasMessageErrorContaining(docAddress.E2_OA_AddressInfo, "A Canadian postal code should be in the following format: A9A9A9");
				docAddress.E2_Postcode = "M5P1A2";
				docAddress.E2_OA_AddressInfo.ClearAllNotifications();
				CAAddressValidator.Validate(docAddress, docAddress.E2_OA_AddressInfo);
				AssertNoMessageErrorContaining(docAddress.E2_OA_AddressInfo, "A Canadian postal code should be in the following format: A9A9A9");

				const string usMsg = "A US Zip code should be in the following format: 99999 or 99999-9999";
				docAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
				docAddress.E2_Postcode = "";
				docAddress.E2_OA_AddressInfo.ClearAllNotifications();
				CAAddressValidator.Validate(docAddress, docAddress.E2_OA_AddressInfo);
				AssertHasMessageErrorContaining(docAddress.E2_OA_AddressInfo, "A postal/zip code is required when the country/region is Canada or United States");
				docAddress.E2_Postcode = "9999A";
				docAddress.E2_OA_AddressInfo.ClearAllNotifications();
				CAAddressValidator.Validate(docAddress, docAddress.E2_OA_AddressInfo);
				AssertHasMessageErrorContaining(docAddress.E2_OA_AddressInfo, usMsg);
				docAddress.E2_Postcode = "12345";
				docAddress.E2_OA_AddressInfo.ClearAllNotifications();
				CAAddressValidator.Validate(docAddress, docAddress.E2_OA_AddressInfo);
				AssertNoMessageErrorContaining(docAddress.E2_OA_AddressInfo, usMsg);
				docAddress.E2_Postcode = "12345-1234";
				docAddress.E2_OA_AddressInfo.ClearAllNotifications();
				CAAddressValidator.Validate(docAddress, docAddress.E2_OA_AddressInfo);
				AssertNoMessageErrorContaining(docAddress.E2_OA_AddressInfo, usMsg);
				docAddress.E2_Postcode = "1234";
				docAddress.E2_OA_AddressInfo.ClearAllNotifications();
				CAAddressValidator.Validate(docAddress, docAddress.E2_OA_AddressInfo);
				AssertHasMessageErrorContaining(docAddress.E2_OA_AddressInfo, usMsg);
				docAddress.E2_Postcode = "12345 1234";
				docAddress.E2_OA_AddressInfo.ClearAllNotifications();
				CAAddressValidator.Validate(docAddress, docAddress.E2_OA_AddressInfo);
				AssertHasMessageErrorContaining(docAddress.E2_OA_AddressInfo, usMsg);
				docAddress.E2_Postcode = "123456";
				docAddress.E2_OA_AddressInfo.ClearAllNotifications();
				CAAddressValidator.Validate(docAddress, docAddress.E2_OA_AddressInfo);
				AssertHasMessageErrorContaining(docAddress.E2_OA_AddressInfo, usMsg);
			}
		}

		public void TestAddressValidationName30Only()
		{
			using (docAddress.SuspendValidationTesting())
			using (docAddress.GetValidationSuspender())
			{
				docAddress.E2_OA_AddressInfo.ClearAllNotifications();
				CAAddressValidator.ValidateMandatoryName30Only(docAddress, docAddress.E2_OA_AddressInfo, "Vendor Name");
				AssertHasMessageErrorContaining(docAddress.E2_OA_AddressInfo, "You have not entered a name");
				docAddress.E2_CompanyName = "1234567890123456789012345678901";
				docAddress.E2_OA_AddressInfo.ClearAllNotifications();
				CAAddressValidator.ValidateMandatoryName30Only(docAddress, docAddress.E2_OA_AddressInfo, "Vendor Name");
				AssertNoMessageErrorContaining(docAddress.E2_OA_AddressInfo, "You have not entered a name");
				AssertHasWarningContaining(docAddress.E2_OA_AddressInfo, "The length of data entered into name (31) exceeds the maximum allowed. Only the first 30 characters will be transmitted to Customs");
				docAddress.E2_CompanyName = "X";
				docAddress.E2_Address1 = "X";
				docAddress.E2_City = "X";
				docAddress.E2_OA_AddressInfo.ClearAllNotifications();
				CAAddressValidator.ValidateMandatoryName30Only(docAddress, docAddress.E2_OA_AddressInfo, "Vendor Name");
				AssertNoWarningContaining(docAddress.E2_OA_AddressInfo, "The length of data entered into name (31) exceeds the maximum allowed. Only the first 30 characters will be transmitted to Customs");
				AssertNoMessageErrors(docAddress.E2_OA_AddressInfo);
			}
		}

		public void TestAddressValidateCityNameOnly()
		{
			using (docAddress.SuspendValidationTesting())
			using (docAddress.GetValidationSuspender())
			{
				docAddress.E2_City = "";
				docAddress.E2_OA_AddressInfo.ClearAllNotifications();
				CAAddressValidator.ValidateMandatoryCityNameOnly(docAddress, docAddress.E2_OA_AddressInfo, "Vendor Name");
				AssertHasMessageErrorContaining(docAddress.E2_OA_AddressInfo, "You have not entered a city");
				docAddress.E2_City = "12345";
				docAddress.E2_OA_AddressInfo.ClearAllNotifications();
				CAAddressValidator.ValidateMandatoryCityNameOnly(docAddress, docAddress.E2_OA_AddressInfo, "Vendor Name");
				AssertNoMessageErrorContaining(docAddress.E2_OA_AddressInfo, "You have not entered a city");
				docAddress.E2_OA_AddressInfo.ClearAllNotifications();
			}
		}

		public void TestAddressValidateMHHouseValidationMandatory()
		{
			using (docAddress.SuspendValidationTesting())
			using (docAddress.GetValidationSuspender())
			{
				docAddress.E2_CompanyName = string.Empty;
				docAddress.E2_Address1 = string.Empty;
				docAddress.E2_City = string.Empty;
				docAddress.E2_RN_NKCountryCode = string.Empty;
				docAddress.E2_OA_AddressInfo.ClearAllNotifications();
				CAAddressValidator.ValidateMHHouseValidationMandatory(docAddress, docAddress.E2_OA_AddressInfo, "Vendor Name");
				AssertHasMessageErrorContaining(docAddress.E2_OA_AddressInfo, "You have not entered a name");
				AssertHasMessageErrorContaining(docAddress.E2_OA_AddressInfo, "You have not entered an address line 1");
				AssertHasMessageErrorContaining(docAddress.E2_OA_AddressInfo, "You have not entered a city");
				AssertHasMessageErrorContaining(docAddress.E2_OA_AddressInfo, "You have not entered a country/region code");

				docAddress.E2_CompanyName = "TEST COMPANY NAME";
				docAddress.E2_Address1 = "TEST ADDRESS 1";
				docAddress.E2_City = "TEST CITY";
				docAddress.E2_RN_NKCountryCode = "ZZ";
				docAddress.E2_OA_AddressInfo.ClearAllNotifications();
				CAAddressValidator.ValidateMHHouseValidationMandatory(docAddress, docAddress.E2_OA_AddressInfo, "Vendor Name");
				AssertNoMessageErrorContaining(docAddress.E2_OA_AddressInfo, "You have not entered a name");
				AssertNoMessageErrorContaining(docAddress.E2_OA_AddressInfo, "You have not entered an address line 1");
				AssertNoMessageErrorContaining(docAddress.E2_OA_AddressInfo, "You have not entered a city");
				AssertNoMessageErrorContaining(docAddress.E2_OA_AddressInfo, "You have not entered a country/region code");
				docAddress.E2_OA_AddressInfo.ClearAllNotifications();

				docAddress.E2_OA_AddressInfo.ClearAllNotifications();
				docAddress.E2_Address1 = "TEST ADDRESS 1";
				CAAddressValidator.ValidateMHHouseValidationMandatory(docAddress, docAddress.E2_OA_AddressInfo, "Vendor Name");
				AssertEquals("No warning on Address", 0, docAddress.E2_OA_AddressInfo.Notifications.GetWarnings().Count());

				docAddress.E2_Address1 = "TEST ADDRESS 1 AAAAAAAAAASSSSSSSSSSDDDDDDDDDFFFFF";
				CAAddressValidator.ValidateMHHouseValidationMandatory(docAddress, docAddress.E2_OA_AddressInfo, "Vendor Name");
				AssertEquals("Split warning on Address", 1, docAddress.E2_OA_AddressInfo.Notifications.GetWarnings().Count());
			}
		}

		public static void AssertMainAddressUsesCAAddressValidationIfOrgSpecified(ZPropertyInfo orgPkInfo, string addressCaption)
		{
			string importerDocAddressIsInvalidMessageError = CAAddressValidator.GetAddressIsInvalidMessageError(addressCaption);

			orgPkInfo.Value = ZGuid.Empty;
			AssertNoMessageErrorContaining(orgPkInfo, importerDocAddressIsInvalidMessageError);

			var org = orgPkInfo.BizObj.Factory.NewWithValidTestData<OrgHeader>();
			orgPkInfo.Value = org.PK;
			AssertHasMessageErrorContaining(orgPkInfo, importerDocAddressIsInvalidMessageError);

			org = orgPkInfo.BizObj.Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_Address1 = "Address";
			org.MainAddress.OA_City = "City";
			org.MainAddress.OA_CompanyNameOverride = "Company";
			org.MainAddress.OA_RN_NKCountryCode = "CA";
			orgPkInfo.Value = org.PK;
			AssertNoMessageErrorContaining(orgPkInfo, importerDocAddressIsInvalidMessageError);
		}

		public static void AssertAddressUsesCAAddressValidationIfOrgSpecified(JobDocAddress address, string addressCaption)
		{
			string addressNotConfiguredMessageError = CAAddressValidator.GetAddressNotConfiguredMessageError(addressCaption);

			address.OrganisationPK = ZGuid.Empty;
			AssertNoMessageErrorContaining(address.E2_OA_AddressInfo, addressNotConfiguredMessageError);

			var org = address.Factory.NewWithValidTestData<OrgHeader>();
			address.OrganisationPK = org.PK;
			address.E2_OA_Address = ZGuid.Empty;
			AssertHasMessageErrorContaining(address.E2_OA_AddressInfo, addressNotConfiguredMessageError);

			org = address.Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_Address1 = "Address";
			org.MainAddress.OA_City = "City";
			org.MainAddress.OA_CompanyNameOverride = "Company";
			address.OrganisationPK = org.PK;
			address.E2_OA_Address = org.MainAddress.PK;
			AssertNoMessageErrorContaining(address.E2_OA_AddressInfo, addressNotConfiguredMessageError);
		}

		public static void AssertAddressCityNameValidationIfNotEntered(JobDocAddress address)
		{
			const string CityNotEntered = "You have not entered a city";

			var org = address.Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_Address1 = "Address";
			org.MainAddress.OA_City = ZString.Empty;
			org.MainAddress.OA_CompanyNameOverride = "Company";
			address.OrganisationPK = org.PK;
			address.E2_OA_Address = org.MainAddress.PK;
			AssertHasMessageErrorContaining(address.E2_OA_AddressInfo, CityNotEntered);
			org = address.Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_Address1 = "Address";
			org.MainAddress.OA_City = "City";
			org.MainAddress.OA_CompanyNameOverride = "Company";
			address.OrganisationPK = org.PK;
			address.E2_OA_Address = org.MainAddress.PK;
			AssertNoMessageErrorContaining(address.E2_OA_AddressInfo, CityNotEntered);
		}

		#region SetUp

		protected override void SetUp()
		{
			base.SetUp();
			docAddress = Factory.New<JobDocAddress>();
			docAddress.OrganisationPK = Factory.New<OrgHeader>().PK;
			docAddress.E2_AddressOverride = true;
			docAddress.E2_RN_NKCountryCode = "";
		}

		JobDocAddress docAddress;

		#endregion
	}
}
