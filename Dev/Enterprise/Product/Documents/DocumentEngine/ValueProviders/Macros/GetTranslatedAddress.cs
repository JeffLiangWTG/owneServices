using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.ValueProviders.Macros
{
	class GetTranslatedAddress : ValueProvider
	{
		const string documentLanguage = "DocumentLanguage";

		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<GetTranslatedAddress({AddressPK},{LanguageCode}[,{AddressComponent}])>",
				ResString.GetMultilingualString("97bba8fe-65bf-4ac1-a030-ec70c014c3da",
				@"Returns the address in the specified language. 
{0} is the PK of the Address ({1}) you want to get the translation from. 
{2} is the ISO language code or the constant string '{3}'.
Using '{3}' should return a translated address based on the language of the document being generated.
The original value will be returned if the address does not have a linked translated address in the specified language.
Optional Parameters:
{4} is an address component name. If it is used, the macro will retrieve and return only one component from the address.
Address component list: {5}.",
				"{AddressPK}", "OrgAddress", "{LanguageCode}", documentLanguage, "{AddressComponent}", "CompanyName, AddressLine1, AddressLine2, City, State, PostCode"),
				new List<(string example, object expectedResult)> {
					((NoResString)"<GetTranslatedAddress(<AddressPK>, ZH-CN)>", (NoResString)"第五大道\n麦斯考特\n悉尼 新南威尔士 2100\n澳大利亚"),
					((NoResString)"<GetTranslatedAddress(<AddressPK>, DocumentLanguage, AddressLine1)>", (NoResString)"5th road") });
		}

		#region GetReplacement
		protected override object GetReplacementCore(string macro, Report report)
		{
			var addressPk = Guid.Empty;
			var match = Regex.Match(macro);

			var parsedValue = ValueProviderHelper.ParseGuid(report, match.Groups[1].Value);
			var languageCode = match.Groups[2].Value?.Trim();
			var addressComponent = match.Groups.Count > 3 ? match.Groups[3].Value?.Trim()?.ToUpperInvariant() : string.Empty;

			if (parsedValue is Guid)
			{
				addressPk = (Guid)parsedValue;
			}
			else
			{
				return parsedValue;
			}

			if (addressPk != Guid.Empty)
			{
				var address = GetOrgAddress(addressPk);
				if (address != null)
				{
					ISupportWebAddressValidation addressToDisplay = address;
					if (address.Language != languageCode)
					{
						languageCode = languageCode.Equals(documentLanguage, StringComparison.OrdinalIgnoreCase) ? (string)report.Language : languageCode;
						var translatedAddress = address.GetTranslatedAddressInSpecificLanguage(languageCode);
						if (translatedAddress != null)
						{
							addressToDisplay = translatedAddress;
						}
					}

					if (string.IsNullOrEmpty(addressComponent))
					{
						return new AddressFormatter(address.Factory, addressToDisplay, address.CountryName, address.OA_RN_NKCountryCode, false).PostalAddress();
					}
					else
					{
						switch (addressComponent)
						{
							case "COMPANYNAME":
								return addressToDisplay.CompanyName;
							case "ADDRESSLINE1":
								return addressToDisplay.Address1;
							case "ADDRESSLINE2":
								return addressToDisplay.Address2;
							case "CITY":
								return addressToDisplay.City;
							case "STATE":
								return addressToDisplay.State;
							case "POSTCODE":
								return addressToDisplay.Postcode;
							default:
								ReportMacroError(report, string.Format(CultureInfo.InvariantCulture, (NoResString)"({0}) is not a valid component from the address.", addressComponent));
								return "";
						}
					}
				}
				return "";
			}

			return "";
		}

		OrgAddress GetOrgAddress(Guid orgAddressPK)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			return factory.Load<OrgAddress>(orgAddressPK);
		}

		#endregion

		public override Regex Regex
		{
			get { return regex; }
		}
		static readonly Regex regex = new Regex(@"^<(?:[\s]*)get(?:[\s]*)translated(?:[\s]*)address(?:[\s]*)\((?:[\s]*)([^\s,]+)(?:[\s]*),\s*([^\s,]+)\s*(?:,\s*([^\s,]*)\s*)?(?:[\s]*)\)(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
