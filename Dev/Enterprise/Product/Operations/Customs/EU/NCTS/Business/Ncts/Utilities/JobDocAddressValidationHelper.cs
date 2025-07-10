using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public static class JobDocAddressValidationHelper
	{
		public static void ApplyRequirementForOverriddenValidation(JobDocAddressRequirement requirement)
		{
			requirement.ValidateCompanyName += ValidateCompanyName;
			requirement.ValidateCountry += ValidateCountry;
			requirement.ValidateCity += ValidateCity;
			requirement.ValidatePostCode += ValidatePostCode;

			requirement.ValidateContact += ValidateContactName;
			requirement.ValidatePhoneFormatted += ValidatePhone;

			requirement.ValidateState += ValidateState;
			requirement.ValidateAddress1 += ValidateAddress1;
		}

		public static void ApplyRequirementForContactWorkPhoneValidation_TR0079(JobDocAddressRequirement requirement)
		{
			requirement.ValidateContact += ValidateWorkPhoneEntered_TR0079;
		}

		static void ValidateAddress1(JobDocAddressValidation validation)
		{
		}

		static void ValidateState(JobDocAddressValidation validation)
		{
		}

		static void ValidateWorkPhoneEntered_TR0079(JobDocAddressValidation validation)
		{
			var parent = validation.Parent;

			var header = parent.Parent switch
			{
				NctsHeader nctsHeader => nctsHeader,
				NctsBill bill => bill.Header,
				NctsDepartureCargoDesc cargoDesc => cargoDesc.Header,
				_ => null,
			};

			if (header.ValidationDecider is INctsHeaderDeparturePhase5ValidationDecider { IsRuleTR0079Active: true } && !parent.E2_AddressOverride && parent.Contact is { OC_Phone_Formatted.IsEmpty: true })
			{
				parent.E2_ContactInfo.AddMessageError(header.Configuration.ValidationRuleConfiguration.Messages.TR0079Message);
			}
		}

		static void ValidatePhone(JobDocAddressValidation validation)
		{
			var parent = validation.Parent;

			if (parent.E2_AddressOverride)
			{
				MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(parent.E2_Phone_FormattedInfo, parent.E2_ContactInfo, Res.GetString("25B94800-E6BF-4D46-8A5C-BEE01B1ECC0E", "You have not entered a Phone Number."));
			}
		}

		static void ValidateContactName(JobDocAddressValidation validation)
		{
			var parent = validation.Parent;

			if (parent.E2_AddressOverride)
			{
				UniversalValidationHelper.CheckMaxLength(parent.E2_ContactInfo, ContactNameMaxLength);
				if (!parent.E2_Phone_Formatted.IsEmpty || !parent.E2_Email.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.E2_ContactInfo, Res.GetString("B2007649-AF9A-411B-A189-0FBFDD21B8CC", "Contact Name"));
				}
			}
		}

		static void ValidateCity(JobDocAddressValidation validation)
		{
			var parent = validation.Parent;
			if (parent.E2_AddressOverride)
			{
				UniversalValidationHelper.CheckMaxLength(parent.E2_CityInfo, CityMaxLength);
				MandatoryValidation.MessageErrorIfNotEntered(parent.CityInfo, Res.GetString("3A382D05-55A5-4ADC-8872-645348A9E756", "City"));
			}
		}

		static void ValidateCountry(JobDocAddressValidation validation)
		{
			var parent = validation.Parent;
			if (parent.E2_AddressOverride)
			{
				var countryCodeInfo = parent.E2_RN_NKCountryCodeInfo;
				MandatoryValidation.MessageErrorIfNotEntered(countryCodeInfo, Res.GetString("54DF3101-325B-41F7-9F5A-937185F5B818", "Country/Region"));

				if (!countryCodeInfo.Value.IsEmpty)
				{
					ListValidation.MessageErrorIfInvalidCode(countryCodeInfo);
				}
			}
		}

		static void ValidateCompanyName(JobDocAddressValidation validation)
		{
			var parent = validation.Parent;
			if (parent.E2_AddressOverride)
			{
				UniversalValidationHelper.CheckMaxLength(parent.E2_CompanyNameInfo, CompanyNameMaxLength);
				MandatoryValidation.MessageErrorIfNotEntered(parent.CompanyNameInfo, Res.GetString("C2020A7B-2359-440A-ABF7-503EAE83A581", "Company Name"));
			}
		}

		static void ValidatePostCode(JobDocAddressValidation validation)
		{
			var parent = validation.Parent;
			if (parent.E2_AddressOverride)
			{
				var country = RefCountry.LoadFromCountryCode(parent.Factory, parent.E2_RN_NKCountryCode);
				if (country != null && country.RN_PostcodeValidationRule == CountryAddressValidationRuleList.Codes.MustBeEntered)
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.PostcodeInfo, Res.GetString("7D278F1F-8BE8-4890-8511-7538FBF173A9", "Postcode"));
				}
			}
		}

		const int CompanyNameMaxLength = 70;
		const int CityMaxLength = 35;
		const int ContactNameMaxLength = 70;
	}
}
