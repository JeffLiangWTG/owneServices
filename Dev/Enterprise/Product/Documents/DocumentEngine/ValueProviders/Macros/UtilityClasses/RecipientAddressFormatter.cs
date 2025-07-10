using System;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities
{
	internal class RecipientAddressFormatter
	{
		readonly Regex regex;

		public RecipientAddressFormatter(Regex regex)
		{
			this.regex = regex;
		}

		bool DocDeliveryContactsAreProbablyTheSame(DocDeliveryContact a, DocDeliveryContact b)
		{
			return a.Name == b.Name && a.Email == b.Email && a.OrgAddressPK == b.OrgAddressPK;
		}

		[ReturnsResourceString]
		public object Format(string macro, Report report, bool hideContactNameLine, bool includeCountryEvenIfSame = false)
		{
			if (report == null)
			{
				throw new ArgumentNullException(nameof(report), "Report must not be null.");
			}

			//go down the list, except if GetDeliveryContactFromMacro(macro) and report.MostOfficialContact have the same OrgAddressPK,
			//swap to report.MostOfficialContact instead since it's strictly better
			DocDeliveryContact officialContact = GetDeliveryContactFromMacro(macro);
			if (officialContact == null)
			{
				officialContact = report.MostOfficialContact;
				if (officialContact == null)
				{
					officialContact = report.DeliveryContact;
				}
			}
			else if (report.MostOfficialContact != null && officialContact.OrgAddressPK == report.MostOfficialContact.OrgAddressPK)
			{
				officialContact = report.MostOfficialContact;
			}
			DocDeliveryContact deliveryContact = report.DeliveryContact ?? officialContact;
			DocDeliveryContact contactToUse = GetContactToUse(officialContact, deliveryContact);

			string officialPostalAddress = GetPostalAddress(officialContact, includeCountryEvenIfSame);
			string deliveryPostalAddress = GetPostalAddress(deliveryContact, includeCountryEvenIfSame);
			string postalAddressToUse = GetPostalAddress(contactToUse, includeCountryEvenIfSame);

			if (String.IsNullOrWhiteSpace(officialPostalAddress) || officialPostalAddress.Contains(DocDeliveryContact.NoOrganizationDetailsFoundMessage.ToString(officialContact.DeliveryLanguage)))
			{
				officialPostalAddress = deliveryPostalAddress;
				postalAddressToUse = deliveryPostalAddress;
			}

			if (String.IsNullOrWhiteSpace(deliveryPostalAddress) || deliveryPostalAddress.Contains(DocDeliveryContact.NoOrganizationDetailsFoundMessage.ToString(officialContact.DeliveryLanguage)))
			{
				deliveryPostalAddress = officialPostalAddress;
				postalAddressToUse = officialPostalAddress;
			}

			bool isRedirected = officialContact != null && deliveryContact != null &&
				!DocDeliveryContactsAreProbablyTheSame(officialContact, deliveryContact);

			report.SetCoverSheetRequired(isRedirected && DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.Value == RedirectedDocumentAddressFormattingOptionList.Codes.OfficialContactWithCoverPage);

			ZStringBuilder result = new ZStringBuilder();

			if (contactToUse != null)
			{
				var contactNameLine = GetDeliveryContactNameLine(deliveryContact.Name, contactToUse, isRedirected, hideContactNameLine);

				var lines = postalAddressToUse.Split('\n');
				if (isRedirected && DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.Value == RedirectedDocumentAddressFormattingOptionList.Codes.DeliveryContactWithRedirectTo)
				{
					if (officialContact != null)
					{
						result.AppendIfNotEmpty(officialContact.CompanyName.ToUpper());
					}

					result.AppendIfNotEmpty(contactNameLine);

					for (int i = 0; i < lines.Length; i++)
					{
						result.AppendIfNotEmpty(lines[i].Trim());
					}
				}
				else
				{
					result.AppendIfNotEmpty(lines[0].Trim());
					result.AppendIfNotEmpty(contactNameLine);

					for (int i = 1; i < lines.Length; i++)
					{
						result.AppendIfNotEmpty(lines[i].Trim());
					}
				}

				AppendCountrySpecificSuffix(contactToUse, result);
			}

			return result.ToStringWithNewLineBetweenAppends();
		}

		static void AppendCountrySpecificSuffix(DocDeliveryContact contactToUse, ZStringBuilder result)
		{
			if (GlbCompany.CurrentCompany.Country.IsIceland() && contactToUse.OrgHeader != null)
			{
				ZString kennitalaCode = contactToUse.OrgHeader.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.Kennitala);
				if (!kennitalaCode.IsEmpty)
				{
					ZString kennitalaLabel = (NoResString)"Kennitala ";
					if (contactToUse.OrgHeader.CountryCode == Core.Constants.CountryCodes.Iceland)
					{
						kennitalaLabel += (NoResString)"greiðanda ";
					}

					result.AppendIfNotEmpty(kennitalaLabel + kennitalaCode);
				}
			}
		}

		ZGuid GetOrgAddressPK(string macro)
		{
			ZGuid result = ZGuid.Empty;
			Match match = regex.Match(macro);
			string orgAddressPK = match.Groups["OrgAddressPK"].Value;
			ZGuid.TryParse(orgAddressPK, out result);

			return result;
		}

		DocDeliveryContact GetDeliveryContactFromMacro(string macro)
		{
			DocDeliveryContact result = null;
			ZGuid orgAddressPK = GetOrgAddressPK(macro);

			if (!orgAddressPK.IsEmpty)
			{
				BusinessObjectFactory factory = new BusinessObjectFactory();

				DocDeliveryContact deliveryContact = new DocDeliveryContact(factory);
				deliveryContact.OrgAddressPK = orgAddressPK;

				if (deliveryContact.OrgAddress != null)
				{
					result = deliveryContact;
				}
			}

			return result;
		}

		string GetPostalAddress(DocDeliveryContact deliveryContact, bool includeCountryEvenIfSame = false)
		{
			string result = "";

			if (deliveryContact != null)
			{
				var languageCode = "";
				if(deliveryContact.Language != Res.CurrentLanguage)
				{
					languageCode = Res.CurrentLanguage;
				}
				var formatter = new DocDeliveryContactAddressFormatter(deliveryContact.Factory, deliveryContact, GlbCompany.CurrentCompany, includeCountryEvenIfSame, languageCode)
				{
					TakePADPostalAddress = deliveryContact.IsFromIceland() && GlbCompany.CurrentCompany.Country.IsIceland()
				};
				result = formatter.PostalAddress();
			}

			return result;
		}

		DocDeliveryContact GetContactToUse(DocDeliveryContact mostOfficialContact, DocDeliveryContact deliveryContact)
		{
			DocDeliveryContact result = null;

			if (mostOfficialContact == null)
			{
				result = deliveryContact;
			}
			else if (deliveryContact != null && !deliveryContact.OrgHeaderPK.IsEmpty && deliveryContact.OrgHeaderPK == mostOfficialContact.OrgHeaderPK)
			{
				result = mostOfficialContact;
			}
			else
			{
				switch (DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.Value)
				{
					case RedirectedDocumentAddressFormattingOptionList.Codes.OfficialContactOnly:
					case RedirectedDocumentAddressFormattingOptionList.Codes.OfficialContactWithCoverPage:
						result = mostOfficialContact;
						break;

					case RedirectedDocumentAddressFormattingOptionList.Codes.DeliveryContactWithRedirectTo:
					case RedirectedDocumentAddressFormattingOptionList.Codes.DeliveryContactOnly:
						if (deliveryContact != null)
						{
							result = deliveryContact;
						}
						break;
				}
			}

			return result;
		}

#if DEBUG
		internal
#endif
		string GetDeliveryContactNameLine(string contactName, DocDeliveryContact intendedDeliveryContact, bool isRedirected, bool hideContactNameLine)
		{
			string result = "";

			if (DocumentsDataRegistry.Instance.ContactNameUpperCase.Value)
			{
				contactName = contactName.ToUpper();
			}

			if (!string.IsNullOrEmpty(contactName) && !hideContactNameLine)
			{
				if (intendedDeliveryContact.IsFromIceland() && GlbCompany.CurrentCompany.Country.IsIceland())
				{
					if (contactName == (NoResString)"THE ACCOUNTS PAYABLE MANAGER")
					{
						contactName = (NoResString)"bókhaldsdeildar";
					}

					result = (NoResString)"b/t: " + contactName;
				}
				else
				{
					var registryItem = (isRedirected && DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.Value == RedirectedDocumentAddressFormattingOptionList.Codes.DeliveryContactWithRedirectTo) ? DocumentsDataRegistry.Instance.RedirectToPrefixForAddressContactNameLineText : DocumentsDataRegistry.Instance.AttentionPrefixForAddressContactNameLineText;
					var value = registryItem.Value.ToStringWithParameters();
					var ogriginalValue = value;
					if (string.IsNullOrEmpty(value))
					{
						value = "{0}";
					}

					if (!value.Contains("{0}"))
					{
						value += " {0}";
					}
					try
					{
						result = string.Format(CultureInfo.CurrentCulture, value, contactName);
					}
					catch (FormatException)
					{
						var message = Res.GetString("46d15fbb-de3f-4cfd-8a97-985ae91297d5", "The input value '{0}' is invalid for address formatting, only '{{0}}' can be treated as a replacement string, check your input in Registry -> {1}.", ogriginalValue, registryItem.Location());
						throw new FormatException(message);
					}
				}
			}

			return result;
		}
	}

	static class ExtensionMethods
	{
		public static bool IsIceland(this RefCountry country)
		{
			return country.Code == Core.Constants.CountryCodes.Iceland;
		}

		public static bool IsFromIceland(this DocDeliveryContact deliveryContact)
		{
			return deliveryContact != null && deliveryContact.OrgHeader != null && deliveryContact.OrgHeader.CountryCode == Core.Constants.CountryCodes.Iceland;
		}
	}
}
