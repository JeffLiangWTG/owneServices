using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	public class JobDocAddressValidationHelperTest : TestCaseWithFactory
	{
		public void TestValidationsApplied()
		{
			testAddress.OverrideRequirement = new JobDocAddressRequirement();
			JobDocAddressValidationHelper.ApplyRequirementForOverriddenValidation(testAddress.Requirement);
			AssertOverrideValidations(testAddress);
		}

		public static void AssertOverrideValidations(JobDocAddress address)
		{
			CombineAssertions(() =>
			{
				AssertPhoneValidation(address);
				AssertContactValidation(address);
				AssertCityValidation(address);
				AssertCountryValidation(address);
				AssertCompanyNameValidation(address);
				AssertPostcodeValidation(address);
			});
		}

		public static void AssertTR0079Validation(JobDocAddress address, NctsHeader parent)
		{
			const string msgError = "[TR0079] Work Phone Number must be entered in selected Contact.";

			using (var context = new NctsHeaderValidationDeciderTestContext<INctsHeaderDeparturePhase5ValidationDecider>(address.Factory))
			{
				context.ClearCachedValidationDecider(parent);
				context.DisableRule(x => x.IsRuleTR0079Active);

				address.E2_OA_Address = address.Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
				address.E2_AddressOverride = false;

				var contact = address.Organisation.Contacts.AddNew();
				contact.OC_ContactName = "TEST";
				var contactInfo = address.E2_ContactInfo;

				address.E2_Contact = contact.OC_ContactName;
				AssertNoMessageError("Rule is not enabled", contactInfo, msgError);

				context.EnableRule(x => x.IsRuleTR0079Active);
				address.Validation.ValidateE2_Contact();
				AssertHasMessageError(contactInfo, msgError);

				contact.OC_Phone_Formatted = "+49 1234 1234";
				address.Validation.ValidateE2_Contact();

				AssertNoMessageError(contactInfo, msgError);
			}
		}

		static void AssertPhoneValidation(JobDocAddress address)
		{
			ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(address.E2_Phone_FormattedInfo, address.E2_ContactInfo, false, "You have not entered a Phone Number.");
		}

		static void AssertContactValidation(JobDocAddress address)
		{
			var msgError = MandatoryValidation.YouHaveNotEnteredMessage("Contact Name");
			UniversalValidationHelperTest.AssertMaxLength(address.E2_ContactInfo, 70);
			ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(address.E2_ContactInfo, address.E2_EmailInfo, false, msgError);
			ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(address.E2_ContactInfo, address.E2_Phone_FormattedInfo, false, msgError);
		}

		static void AssertCityValidation(JobDocAddress address)
		{
			UniversalValidationHelperTest.AssertMaxLength(address.E2_CityInfo, 35);
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(address.E2_CityInfo, MandatoryValidation.YouHaveNotEnteredMessage("City"));
		}

		static void AssertCountryValidation(JobDocAddress address)
		{
			address.E2_RN_NKCountryCode = "DE";
			var info = address.E2_RN_NKCountryCodeInfo;
			var notEnteredMessageError = MandatoryValidation.YouHaveNotEnteredMessage("Country/Region");
			AssertNoMessageError(info, notEnteredMessageError);
			address.E2_RN_NKCountryCode = string.Empty;
			AssertHasMessageError(info, notEnteredMessageError);

			ValidationTestHelper.AssertInvalidCodeMessageError(info, "XY", "DE");
		}

		static void AssertCompanyNameValidation(JobDocAddress address)
		{
			UniversalValidationHelperTest.AssertMaxLength(address.E2_CompanyNameInfo, 70);
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(address.E2_CompanyNameInfo, MandatoryValidation.YouHaveNotEnteredMessage("Company Name"));
		}

		static void AssertPostcodeValidation(JobDocAddress address)
		{
			address.E2_RN_NKCountryCode = "DE";
			var deCountry = RefCountry.LoadFromCountryCode(address.Factory, "DE");

			deCountry.RN_PostcodeValidationRule = CountryAddressValidationRuleList.Codes.MustBeEntered;

			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(address.E2_PostcodeInfo, MandatoryValidation.YouHaveNotEnteredMessage("Postcode"));

			deCountry.RN_PostcodeValidationRule = CountryAddressValidationRuleList.Codes.NoValidationRule;
			address.E2_Postcode = ZString.Empty;

			AssertNoMessageError(address.E2_PostcodeInfo, MandatoryValidation.YouHaveNotEnteredMessage("Postcode"));
		}

		protected override void SetUp()
		{
			base.SetUp();

			testAddress = Factory.New<JobDocAddress>();
			testAddress.E2_AddressOverride = true;
		}

		JobDocAddress testAddress;
	}
}
