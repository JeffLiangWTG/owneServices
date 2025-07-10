using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class DocDeliveryContactAddressFormatter : AddressFormatter
	{
		readonly string LanguageCode;

		public DocDeliveryContactAddressFormatter(BusinessObjectFactory factory, DocDeliveryContact recipient, GlbCompany senderCompany, bool includeCountryEvenIfSame = false, string languageCode = "")
			: base(factory, (OrgHeader)null, senderCompany, includeCountryEvenIfSame)
		{
			this.Recipient = recipient;
			LanguageCode = languageCode;
		}
		readonly DocDeliveryContact Recipient;

		protected override void RetrieveOrganisationDetails()
		{
			Name = Recipient.CompanyName;

			if (TakePADPostalAddress && Recipient.OrgHeader != null)
			{
				var pADAddresses = Recipient.OrgHeader.Addresses.AddressesOfType(OrgAddressType.PickupAndDelivery);

				if (pADAddresses.Count > 0)
				{
					var address = pADAddresses[0];
					FillAddressDetails(address, address.OA_CompanyNameOverride);
				}
				else
				{
					FillAddressDetails(Recipient.OrgHeader.MainAddress, Recipient.OrgHeader.OH_FullName);
				}
			}
			else
			{
				if (!string.IsNullOrEmpty(LanguageCode) && Recipient.OrgAddress != null)
				{
					FillAddressDetails(Recipient.OrgAddress, Recipient.CompanyName);
				}
				else
				{
					Address1 = Recipient.Address1;
					Address2 = Recipient.Address2;
					City = Recipient.City;
					State = Recipient.State;
					PostCode = Recipient.PostCode;
					Language = Recipient.Language;
					AdditionalAddressInformation = Recipient.AdditionalAddress;
				}
			}

			if (Recipient.UNLOCO != null && Recipient.UNLOCO.Country != null)
			{
				CountryName = Recipient.UNLOCO.Country.RN_DescMultilingual;
				CountryCode = Recipient.UNLOCO.RL_RN_NKCountryCode;
			}
		}

		void FillAddressDetails(OrgAddress address, string name)
		{
			var translatedAddress = !string.IsNullOrEmpty(LanguageCode) ? address.GetTranslatedAddressInSpecificLanguage(LanguageCode) : null;
			if (translatedAddress != null)
			{
				Name = translatedAddress.CompanyName;
				Address1 = translatedAddress.Address1;
				Address2 = translatedAddress.Address2;
				City = translatedAddress.City;
				State = translatedAddress.State;
				Language = translatedAddress.Language;
				PostCode = translatedAddress.Postcode;
				AdditionalAddressInformation = translatedAddress.OTA_AdditionalAddressInformation;
			}
			else
			{
				Name = name;
				Address1 = address.OA_Address1;
				Address2 = address.OA_Address2;
				City = address.OA_City;
				State = address.OA_State;
				Language = address.OA_Language;
				PostCode = address.OA_PostCode;
				AdditionalAddressInformation = address.OA_AdditionalAddressInformation;
			}
		}

		public bool TakePADPostalAddress { get; set; }
	}
}
