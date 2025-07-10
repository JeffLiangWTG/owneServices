using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	class JPAFRBillsJobDocAddressValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckE2_OA_Address()
		{
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_FullName = "T.T.";
			var testOrgAddress = Factory.NewWithValidTestData<OrgAddress>();
			testOrgAddress.OA_OH = testOrg.PK;
			testOrgAddress.OA_City = string.Empty;
			Address.E2_CompanyName = "T.T.";
			Address.E2_AddressOverride = false;
			Address.E2_OA_Address = testOrgAddress.PK;
			Address.AdditionalValidation.ValidateAll();
			AssertHasMessageErrorContaining(Address.E2_OA_AddressInfo, ValidationConstants.JobDocAddress.AddressFieldIsRequired(Address.E2_PhoneInfo.HumanReadableName));
			AssertHasMessageErrorContaining(Address.E2_OA_AddressInfo, ValidationConstants.JobDocAddress.AddressFieldIsRequired(Address.E2_CityInfo.HumanReadableName));
			AssertHasWarningContaining(Address.E2_OA_AddressInfo, ValidationConstants.JobDocAddress.InappropriateCompanyInfo(Address.E2_CompanyNameInfo.HumanReadableName));
			AssertHasWarningContaining(Address.E2_OA_AddressInfo, ValidationConstants.JobDocAddress.InappropriateCompanyInfo(Address.E2_Address1Info.HumanReadableName));

			testOrgAddress.OA_City = "City";
			testOrgAddress.OA_Phone = "+1 201-555-5555";
			testOrgAddress.OA_RN_NKCountryCode = ZString.Empty;
			Address.E2_AddressOverride = false;
			Address.E2_OA_Address = testOrgAddress.PK;
			Address.AdditionalValidation.ValidateAll();
			AssertHasMessageErrorContaining(Address.E2_OA_AddressInfo, ValidationConstants.JobDocAddress.AddressFieldIsRequired(Address.E2_RN_NKCountryCodeInfo.HumanReadableName));
			AssertHasWarningContaining(Address.E2_OA_AddressInfo, ValidationConstants.JobDocAddress.InappropriateCompanyInfo(Address.E2_CompanyNameInfo.HumanReadableName));
			AssertHasWarningContaining(Address.E2_OA_AddressInfo, ValidationConstants.JobDocAddress.InappropriateCompanyInfo(Address.E2_Address1Info.HumanReadableName));

			testOrgAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			Address.AdditionalValidation.ValidateAll();
			AssertNoMessageErrors(Address.E2_OA_AddressInfo);
			AssertHasWarningContaining(Address.E2_OA_AddressInfo, ValidationConstants.JobDocAddress.InappropriateCompanyInfo(Address.E2_CompanyNameInfo.HumanReadableName));
			AssertHasWarningContaining(Address.E2_OA_AddressInfo, ValidationConstants.JobDocAddress.InappropriateCompanyInfo(Address.E2_Address1Info.HumanReadableName));

			testOrgAddress.OA_Address2 = "12345678901234567890123456789012345678901234567890";
			Address.AdditionalValidation.ValidateAll();
			AssertNoMessageErrors(Address.E2_OA_AddressInfo);
			AssertHasWarningContaining(Address.E2_OA_AddressInfo, ValidationConstants.Shared.FieldLengthReachedMaxAllowedWillBeTruncated(Address.E2_Address2Info.HumanReadableName, ValidationConstants.Constants.Address2MaxLenth));
			AssertHasWarningContaining(Address.E2_OA_AddressInfo, ValidationConstants.JobDocAddress.InappropriateCompanyInfo(Address.E2_CompanyNameInfo.HumanReadableName));
			AssertHasWarningContaining(Address.E2_OA_AddressInfo, ValidationConstants.JobDocAddress.InappropriateCompanyInfo(Address.E2_Address1Info.HumanReadableName));

			testOrgAddress.OA_Address2 = "12345678901234567890123456";
			testOrgAddress.OA_PostCode = "1234567890";
			Address.AdditionalValidation.ValidateAll();
			AssertNoMessageErrors(Address.E2_OA_AddressInfo);
			AssertNoWarningContaining(Address.E2_OA_AddressInfo, ValidationConstants.Shared.FieldLengthReachedMaxAllowedWillBeTruncated(Address.E2_Address2Info.HumanReadableName, ValidationConstants.Constants.Address2MaxLenth));
			AssertHasWarningContaining(Address.E2_OA_AddressInfo, ValidationConstants.Shared.FieldLengthReachedMaxAllowedWillBeTruncated(Address.E2_PostcodeInfo.HumanReadableName, ValidationConstants.Constants.PostCodeMaxLenth));
			AssertHasWarningContaining(Address.E2_OA_AddressInfo, ValidationConstants.JobDocAddress.InappropriateCompanyInfo(Address.E2_CompanyNameInfo.HumanReadableName));
			AssertHasWarningContaining(Address.E2_OA_AddressInfo, ValidationConstants.JobDocAddress.InappropriateCompanyInfo(Address.E2_Address1Info.HumanReadableName));

			testOrgAddress.OA_PostCode = "12345678";
			Address.AdditionalValidation.ValidateAll();
			AssertNoMessageErrors(Address.E2_OA_AddressInfo);
			AssertNoWarningContaining(Address.E2_OA_AddressInfo, ValidationConstants.Shared.FieldLengthReachedMaxAllowedWillBeTruncated(Address.E2_PostcodeInfo.HumanReadableName, ValidationConstants.Constants.PostCodeMaxLenth));
			AssertHasWarningContaining(Address.E2_OA_AddressInfo, ValidationConstants.JobDocAddress.InappropriateCompanyInfo(Address.E2_CompanyNameInfo.HumanReadableName));
			AssertHasWarningContaining(Address.E2_OA_AddressInfo, ValidationConstants.JobDocAddress.InappropriateCompanyInfo(Address.E2_Address1Info.HumanReadableName));

			testOrg.OH_FullName = "T.T.T.";
			Address.AdditionalValidation.ValidateAll();
			AssertNoWarningContaining(Address.E2_OA_AddressInfo, ValidationConstants.JobDocAddress.InappropriateCompanyInfo(Address.E2_CompanyNameInfo.HumanReadableName));
			AssertHasWarningContaining(Address.E2_OA_AddressInfo, ValidationConstants.JobDocAddress.InappropriateCompanyInfo(Address.E2_Address1Info.HumanReadableName));

			testOrgAddress.OA_Address1 = "TESTADD1";
			Address.AdditionalValidation.ValidateAll();
			AssertNoWarningContaining(Address.E2_OA_AddressInfo, ValidationConstants.JobDocAddress.InappropriateCompanyInfo(Address.E2_CompanyNameInfo.HumanReadableName));
			AssertNoWarningContaining(Address.E2_OA_AddressInfo, ValidationConstants.JobDocAddress.InappropriateCompanyInfo(Address.E2_Address1Info.HumanReadableName));
			AssertNoMessageErrors(Address.E2_OA_AddressInfo);
			AssertNoWarnings(Address.E2_OA_AddressInfo);

			var testCountry = Factory.NewWithValidTestData<RefCountry>();
			testCountry.RN_Code = "A]";
			var testPort = Factory.NewWithValidTestData<RefUNLOCO>();
			testPort.RL_RN_NKCountryCode = testCountry.Code;
			testOrg.OH_FullName = "T.T.T[";
			testOrgAddress.OA_Address1 = "TESTADD1[";
			testOrgAddress.OA_Address2 = "TESTADD2[";
			testOrgAddress.OA_PostCode = "1234567[";
			testOrgAddress.OA_City = "Cit]";
			testOrgAddress.OA_State = "Stat]";
			testOrgAddress.OA_RL_NKRelatedPortCode = testPort.Code;
			testOrgAddress.OA_RN_NKCountryCode = testCountry.Code;
			Address.AdditionalValidation.ValidateAll();
			AssertHasMessageErrorContaining(Address.E2_OA_AddressInfo, ValidationConstants.Shared.InvalidNACCSChar(Address.E2_CompanyNameInfo.HumanReadableName));
			AssertHasMessageErrorContaining(Address.E2_OA_AddressInfo, ValidationConstants.Shared.InvalidNACCSChar(Address.E2_Address1Info.HumanReadableName));
			AssertHasMessageErrorContaining(Address.E2_OA_AddressInfo, ValidationConstants.Shared.InvalidNACCSChar(Address.E2_Address2Info.HumanReadableName));
			AssertHasMessageErrorContaining(Address.E2_OA_AddressInfo, ValidationConstants.Shared.InvalidNACCSChar(Address.E2_PostcodeInfo.HumanReadableName));
			AssertHasMessageErrorContaining(Address.E2_OA_AddressInfo, ValidationConstants.Shared.InvalidNACCSChar(Address.E2_CityInfo.HumanReadableName));
			AssertHasMessageErrorContaining(Address.E2_OA_AddressInfo, ValidationConstants.Shared.InvalidNACCSChar(Address.E2_StateInfo.HumanReadableName));
			AssertHasMessageErrorContaining(Address.E2_OA_AddressInfo, ValidationConstants.Shared.InvalidNACCSChar(Address.E2_RN_NKCountryCodeInfo.HumanReadableName));
		}

		public void TestCheckE2_City()
		{
			Address.E2_AddressOverride = true;
			Address.E2_CompanyName = "CompanyName";
			Address.E2_Address1 = "Address1";
			Address.E2_Address2 = "Address2";
			Address.E2_City = string.Empty;
			Address.AdditionalValidation.ValidateAll();
			AssertHasMessageErrorContaining(Address.E2_CityInfo, MandatoryValidation.YouHaveNotEntered);

			Address.E2_City = "City";
			Address.AdditionalValidation.ValidateAll();
			AssertNoMessageErrors(Address.E2_CityInfo);

			Address.E2_City = "C[ty";
			Address.AdditionalValidation.ValidateAll();
			AssertHasMessageErrorContaining(Address.E2_CityInfo, ValidationConstants.Shared.InvalidNACCSChar(Address.E2_CityInfo.HumanReadableName));
		}

		public void TestCheckE2_State()
		{
			Address.E2_AddressOverride = true;
			Address.E2_State = "State";
			Address.AdditionalValidation.ValidateAll();
			AssertNoErrors(Address.E2_StateInfo);
			AssertNoMessageErrors(Address.E2_StateInfo);

			Address.E2_State = "Stat[e";
			Address.AdditionalValidation.ValidateAll();
			AssertNoErrors(Address.E2_StateInfo);
			AssertHasMessageErrorContaining(Address.E2_StateInfo, ValidationConstants.Shared.InvalidNACCSChar(Address.E2_StateInfo.HumanReadableName));

			Address.E2_AddressOverride = false;
			Address.AdditionalValidation.ValidateAll();
			AssertNoErrors(Address.E2_StateInfo);
			AssertNoMessageErrors(Address.E2_StateInfo);
		}

		public void TestCheckE2_RN_NKCountryCode()
		{
			Address.E2_AddressOverride = true;
			Address.E2_CompanyName = "CompanyName";
			Address.E2_Address1 = "Address1";
			Address.E2_Address2 = "Address2";
			Address.E2_City = ZString.Empty;
			Address.E2_RN_NKCountryCode = ZString.Empty;
			Address.AdditionalValidation.ValidateAll();
			AssertHasMessageErrorContaining(Address.E2_RN_NKCountryCodeInfo, MandatoryValidation.YouHaveNotEntered);

			Address.E2_RN_NKCountryCode = "AU";
			Address.AdditionalValidation.ValidateAll();
			AssertNoMessageErrors(Address.E2_RN_NKCountryCodeInfo);

			var testCountry = Factory.NewWithValidTestData<RefCountry>();
			testCountry.Code = "A]";
			Address.E2_RN_NKCountryCode = "A]";
			Address.AdditionalValidation.ValidateAll();
			AssertHasMessageErrorContaining(Address.E2_RN_NKCountryCodeInfo, ValidationConstants.Shared.InvalidNACCSChar(Address.E2_RN_NKCountryCodeInfo.HumanReadableName));
		}

		public void TestCheckE2_CompanyName()
		{
			Address.E2_AddressOverride = true;
			Address.E2_CompanyName = "CompanyName";
			Address.AdditionalValidation.ValidateAll();
			AssertNoMessageErrors(Address.E2_CompanyNameInfo);
			AssertNoWarnings(Address.E2_CompanyNameInfo);

			Address.E2_CompanyName = "T------T";
			Address.AdditionalValidation.ValidateAll();
			AssertNoMessageErrors(Address.E2_CompanyNameInfo);
			AssertHasWarningContaining(Address.E2_CompanyNameInfo, ValidationConstants.JobDocAddress.InappropriateCompanyInfo(Address.E2_CompanyNameInfo.HumanReadableName));

			Address.E2_CompanyName = "BMW";
			Address.AdditionalValidation.ValidateAll();
			AssertNoMessageErrors(Address.E2_CompanyNameInfo);
			AssertNoWarnings(Address.E2_CompanyNameInfo);

			Address.E2_CompanyName = "BMW[";
			Address.AdditionalValidation.ValidateAll();
			AssertHasMessageErrorContaining(Address.E2_CompanyNameInfo, ValidationConstants.Shared.InvalidNACCSChar(Address.E2_CompanyNameInfo.HumanReadableName));
			AssertNoWarnings(Address.E2_CompanyNameInfo);
		}

		public void TestCheckE2_Address1()
		{
			Address.E2_AddressOverride = true;
			Address.E2_Address1 = "Address1";
			Address.AdditionalValidation.ValidateAll();
			AssertNoMessageErrors(Address.E2_Address1Info);
			AssertNoWarnings(Address.E2_Address1Info);

			Address.E2_Address1 = "Address1][";
			Address.AdditionalValidation.ValidateAll();
			AssertHasMessageErrorContaining(Address.E2_Address1Info, ValidationConstants.Shared.InvalidNACCSChar(Address.E2_Address1Info.HumanReadableName));
			AssertNoWarnings(Address.E2_Address1Info);

			Address.E2_Address1 = "T------T";
			Address.AdditionalValidation.ValidateAll();
			AssertNoMessageErrors(Address.E2_Address1Info);
			AssertHasWarningContaining(Address.E2_Address1Info, ValidationConstants.JobDocAddress.InappropriateCompanyInfo(Address.E2_Address1Info.HumanReadableName));

			Address.E2_Address1 = "BMW";
			Address.AdditionalValidation.ValidateAll();
			AssertNoMessageErrors(Address.E2_Address1Info);
			AssertNoWarnings(Address.E2_Address1Info);
		}

		public void TestCheckE2_Address2()
		{
			Address.E2_AddressOverride = true;
			Address.E2_Address2 = "Address2";
			Address.AdditionalValidation.ValidateAll();
			AssertNoMessageErrors(Address.E2_Address2Info);

			Address.E2_Address2 = "Address2[]";
			Address.AdditionalValidation.ValidateAll();
			AssertHasMessageErrorContaining(Address.E2_Address2Info, ValidationConstants.Shared.InvalidNACCSChar(Address.E2_Address2Info.HumanReadableName));

			Address.E2_Address2 = "THISISAADDRESS2THATISOVER35CHARACTERS";
			Address.AdditionalValidation.ValidateAll();
			AssertHasWarningContaining(Address.E2_Address2Info, ValidationConstants.Shared.FieldLengthReachedMaxAllowedWillBeTruncated(Address.E2_Address2Info.HumanReadableName, 35));
		}

		public void TestCheckE2_Postcode()
		{
			Address.E2_AddressOverride = true;
			Address.E2_Postcode = "210000";
			Address.AdditionalValidation.ValidateAll();
			AssertHasWarningContaining(Address.E2_PostcodeInfo, "The entered postcode does not comply with the postcode format rules of the country/region");
			AssertNoErrors(Address.E2_PostcodeInfo);
			AssertNoMessageErrors(Address.E2_PostcodeInfo);

			Address.E2_Postcode = "210[00";
			Address.AdditionalValidation.ValidateAll();
			AssertNoErrors(Address.E2_PostcodeInfo);
			AssertHasMessageErrorContaining(Address.E2_PostcodeInfo, ValidationConstants.Shared.InvalidNACCSChar(Address.E2_PostcodeInfo.HumanReadableName));
			AssertHasWarningContaining(Address.E2_PostcodeInfo, "The entered postcode does not comply with the postcode format rules of the country/region");

			Address.E2_Postcode = "2100001234";
			Address.AdditionalValidation.ValidateAll();
			AssertHasWarningContaining(Address.E2_PostcodeInfo, ValidationConstants.Shared.FieldLengthReachedMaxAllowedWillBeTruncated(Address.E2_PostcodeInfo.HumanReadableName, 9));
			AssertNoErrors(Address.E2_PostcodeInfo);
			AssertNoMessageErrors(Address.E2_PostcodeInfo);

			Address.E2_Postcode = "123-4567";
			Address.AdditionalValidation.ValidateAll();
			AssertNoWarnings(Address.E2_PostcodeInfo);
			AssertNoErrors(Address.E2_PostcodeInfo);
			AssertNoMessageErrors(Address.E2_PostcodeInfo);
		}

		public void TestCheckE2_Contact()
		{
			var testOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var testOrgAddress = Factory.NewWithValidTestData<OrgAddress>();
			var testContact = Factory.NewWithValidTestData<OrgContact>();
			testOrgAddress.OA_OH = testOrgHeader.PK;
			testContact.OC_OH = testOrgHeader.PK;
			testContact.OC_ContactName = "CIV";
			testContact.OC_Phone = "0412345678";

			Address.E2_AddressOverride = false;
			Address.E2_OA_Address = testOrgAddress.PK;
			Address.E2_Contact = "CIV";
			Address.AdditionalValidation.ValidateAll();
			AssertNoMessageErrors(Address.E2_ContactInfo);
			AssertNoErrors(Address.E2_ContactInfo);
			AssertNoWarnings(Address.E2_ContactInfo);

			testContact.OC_Phone = "04123456789012345";
			Address.AdditionalValidation.ValidateAll();
			AssertNoMessageErrors(Address.E2_ContactInfo);
			AssertNoErrors(Address.E2_ContactInfo);
			AssertHasWarningContaining(Address.E2_ContactInfo, ValidationConstants.JobDocAddress.PhoneNumberLengthReachedMaxAllowed(Address.E2_PhoneInfo.HumanReadableName, 14));

			testContact.OC_Phone = "04123456789[";
			Address.AdditionalValidation.ValidateAll();
			AssertHasMessageErrorContaining(Address.E2_ContactInfo, ValidationConstants.Shared.InvalidNACCSChar(Address.E2_PhoneInfo.HumanReadableName));
			AssertNoErrors(Address.E2_ContactInfo);
			AssertNoWarnings(Address.E2_ContactInfo);
		}

		public void TestCheckE2_Phone()
		{
			Address.E2_AddressOverride = true;
			Address.E2_Phone = "";
			Address.AdditionalValidation.ValidateAll();
			AssertHasMessageErrorContaining(Address.E2_PhoneInfo, MandatoryValidation.YouHaveNotEntered);

			Address.E2_AddressOverride = true;
			Address.E2_Phone = "210000";
			Address.AdditionalValidation.ValidateAll();
			AssertNoWarnings(Address.E2_PhoneInfo);
			AssertNoErrors(Address.E2_PhoneInfo);
			AssertNoMessageErrors(Address.E2_PhoneInfo);

			Address.E2_Phone = "210000123412345";
			Address.AdditionalValidation.ValidateAll();
			AssertHasWarningContaining(Address.E2_PhoneInfo, ValidationConstants.Shared.FieldLengthReachedMaxAllowedWillBeTruncated(Address.E2_PhoneInfo.HumanReadableName, 14));
			AssertNoErrors(Address.E2_PhoneInfo);
			AssertNoMessageErrors(Address.E2_PhoneInfo);

			Address.E2_Phone = "210000123412345[";
			Address.AdditionalValidation.ValidateAll();
			AssertHasMessageErrorContaining(Address.E2_PhoneInfo, ValidationConstants.Shared.InvalidNACCSChar(Address.E2_PhoneInfo.HumanReadableName));
			AssertHasWarningContaining(Address.E2_PhoneInfo, ValidationConstants.Shared.FieldLengthReachedMaxAllowedWillBeTruncated(Address.E2_PhoneInfo.HumanReadableName, 14));
			AssertNoErrors(Address.E2_PhoneInfo);
		}

		#region Preperation

		JobDocAddress Address
		{
			get
			{
				if (this.address == null)
				{
					this.address = Factory.New<JobDocAddress>();
					this.address.AdditionalValidation = new JPAFRBillsJobDocAddressValidation(this.address);
				}
				return this.address;
			}
		}

		JobDocAddress address;

		#endregion
	}
}
