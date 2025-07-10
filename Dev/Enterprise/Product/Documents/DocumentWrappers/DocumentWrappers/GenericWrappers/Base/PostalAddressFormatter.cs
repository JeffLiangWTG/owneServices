using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers.Base
{
	public class PostalAddressFormatter : AddressFormatter
	{
		public PostalAddressFormatter(OrgAddress address, string overrideAdditionalInfo = null)
			: base(address.Factory, (OrgHeader)null, null, false)
		{
			SetupFromOrgAddress(address, overrideAdditionalInfo);
		}

		public PostalAddressFormatter(OrgAddress address, string countryCode, MultilingualString countryDescription, string overrideAdditionalInfo = null)
			: base(address.Factory, (OrgHeader)null, null, false)
		{
			SetupFromOrgAddressWithDefaultCountry(address, countryCode, countryDescription, overrideAdditionalInfo);
		}

		public PostalAddressFormatter(JobDocAddress docAddress)
			: base(docAddress.Factory, (OrgHeader)null, null, false)
		{
			OrgAddress address = docAddress.Address;
			if (docAddress.E2_AddressOverride || address == null)
			{
				SetupCompanyNameFrom(docAddress.E2_CompanyName);
				SetupAddressFrom(docAddress.E2_Address1, docAddress.E2_Address2, docAddress.E2_City, docAddress.E2_State, docAddress.E2_Postcode);
				SetupCountryFrom(docAddress.Country);
			}
			else
			{
				SetupFromOrgAddress(address);
			}
		}

		void SetupFromOrgAddress(OrgAddress address, string overrideAdditionalInfo = null)
		{
			SetupCompanyNameFrom(address.EffectiveCompanyName);
			SetupAddressFrom(address, overrideAdditionalInfo);
			if (address.Country == null)
			{
				SetupCountryFrom(address.EffectiveRelatedPortCode);
			}
			else
			{
				SetupCountryFrom(address.Country);
			}
		}

		void SetupFromOrgAddressWithDefaultCountry(OrgAddress address, string countryCode, MultilingualString countryDescription, string overrideAdditionalInfo = null)
		{
			SetupCompanyNameFrom(address.EffectiveCompanyName);
			SetupAddressFrom(address, overrideAdditionalInfo);
			SetupCountryFrom(countryCode, countryDescription);
		}

		#region Internal Setup Methods
		void SetupCountryFrom(RefUNLOCO location)
		{
			SetupCountryFrom(location != null ? location.Country : null);
		}

		void SetupCountryFrom(RefCountry country)
		{
			if (country != null)
			{
				SetupCountryFrom(country.RN_Code, country.RN_DescMultilingual);
			}
			CountryIsSet = true;
		}

		void SetupCountryFrom(string countryCode, MultilingualString countryName)
		{
			CountryCode = countryCode;
			CountryName = countryName;

			CountryIsSet = true;
		}

		void SetupAddressFrom(OrgAddress address, string overrideAdditionalInfo = null)
		{
			if (address != null)
			{
				SetupAddressFrom(string.IsNullOrWhiteSpace(overrideAdditionalInfo) ? (string)address.OA_AdditionalAddressInformation : overrideAdditionalInfo, address.OA_Address1, address.OA_Address2, address.OA_City, address.OA_State, address.OA_PostCode);
			}
			AddressIsSet = true;
		}

		void SetupAddressFrom(string address1, string address2, string city, string state, string postCode)
		{
			Address1 = address1;
			Address2 = address2;
			City = city;
			State = state;
			PostCode = postCode;

			AddressIsSet = true;
		}

		void SetupAddressFrom(string additionalAddressInformation, string address1, string address2, string city, string state, string postCode)
		{
			SetupAddressFrom(address1, address2, city, state, postCode);
			AdditionalAddressInformation = additionalAddressInformation;
		}

		void SetupCompanyNameFrom(string companyName)
		{
			Name = companyName;

			CompanyIsSet = true;
		}
		#endregion

		#region Implementation
		protected override void RetrieveCompanyDetails()
		{
			// Do Nothing, Company is Set in the Constructor or not at all. Only Used for working out which Country you're in anyway.
		}

		protected override void RetrieveOrganisationDetails()
		{
			// Do Nothing, Organisation Details are set in a wrapping GetPostalAddressFrom() Method.
		}

		protected override void BuildAddress()
		{
			if (!CompanyIsSet || !AddressIsSet || !CountryIsSet)
			{
				throw new Exception("Company, Address AND Contry must all be set before calling PostalAddress(). Please check the Implementation of the PostalAddressFormatter(xxx) constructor overload used.");
			}

			base.BuildAddress();
		}
		bool CompanyIsSet;
		bool AddressIsSet;
		bool CountryIsSet;
		#endregion
	}
}
