using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class RecipientNameAndAddress : ValueProvider
	{
		const string DocumentLanguage = "DocumentLanguage";
		const string IncludeCountryIfSameToCurrent = "IncludeCountryIfSameToCurrent";
		const string LanguageCode = "LanguageCode";

		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<RecipientNameAndAddress[({IncludeCountryIfSameToCurrent: Y or N} [,{LanguageCode}])]>",
				ResString.GetMultilingualString("bd1115c1-02bc-4048-85f8-55646d163570",
				@"Returns the Address of the Contact the Report or Document is supposed to be sent to with an 'ATTN:' prefix showing the Contact's Name, based on the document's Address Category.
{0} is optional, if it is not specified, it will be N.
{1} is optional, if it is specified, the parameter {0} must be specified as well, it will translate the address if it has a linked translated address in the specified language.
{1} is the language code or the constant string '{2}'.
Using '{2}' should return a translated address based on the language of the document being generated.
The original value will be returned if the address does not have a linked translated address in the specified language.",
				IncludeCountryIfSameToCurrent, LanguageCode, DocumentLanguage),
				new List<(string example, object expectedResult)> {
					("<RecipientNameAndAddress>", (NoResString)"BOO LTD.\nADDR1\nADDR2\nSYDNEY NSW 2000\nATTENTION: MIKE"),
					("<RecipientNameAndAddress(Y)>", (NoResString)"BOO LTD.\nADDR1\nADDR2\nSYDNEY NSW 2000\nAUSTRALIA\nATTENTION: MIKE"),
					((NoResString)"<RecipientNameAndAddress(N, PT-BR)>", (NoResString)"BOO LTD.\nADDR1\nADDR2\nSYDNEY NSW 2000\nATTENTION: MIKE"),
					((NoResString)"<RecipientNameAndAddress(Y, DocumentLanguage)>", (NoResString)"BOO LTD.\nADDR1\nADDR2\nSYDNEY NSW 2000\nAUSTRALIA\nATTENTION: MIKE") });
		}

		[CodeStringFinderHint(typeof(DocDeliveryContact), "NoOrganizationDetailsFoundMessage")]
		protected override object GetReplacementCore(string macro, Report report)
		{
			var contact = report.DeliveryContact;
			var address = "";

			if (contact != null)
			{
				var matchGroups = Regex.Match(macro).Groups;
				var includeCountryIfSameToCurrentParameter = matchGroups[IncludeCountryIfSameToCurrent].Value.Trim();
				var languageCode = matchGroups[LanguageCode].Value.Trim();

				if (!string.IsNullOrEmpty(languageCode) && string.IsNullOrEmpty(includeCountryIfSameToCurrentParameter))
				{
					ReportMacroError(report, Res.GetString("8C07338F-C601-47DD-A948-B2BD0F74B202", "Parameter {0} is missing. When {1} is specified, the parameter {0} must be specified as well", IncludeCountryIfSameToCurrent, LanguageCode));
					return "";
				}

				if (languageCode.Equals(DocumentLanguage, StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(languageCode))
				{
					languageCode = report.Language;
				}

				var includeCountryIfSameToCurrent = includeCountryIfSameToCurrentParameter.Equals((NoResString)"y", StringComparison.OrdinalIgnoreCase);

				//PostalAddress: this is *NOT* necessarily an OrgAddress with the Postal Address capability. It is just *a* postal adress, which could have any capability
				var postalAddress = new DocDeliveryContactAddressFormatter(Factory, contact, GlbCompany.CurrentCompany, includeCountryIfSameToCurrent, languageCode).PostalAddress();
				var contactName = contact.Name;

				if (DocumentsDataRegistry.Instance.ContactNameUpperCase.Value)
				{
					contactName = contactName.ToUpper();
				}

				if (report.TypeOfContact != null && report.TypeOfContact == ContactType.Receivables)
				{
					address = postalAddress;
					if (!string.IsNullOrEmpty(contactName))
					{
						address += "\n" + Res.GetString("5fdb8ae4-9773-419f-b74a-e0ddc53fde55", "ATTENTION: {0}", contactName);
					}
				}
				else
				{
					if (!string.IsNullOrEmpty(contactName))
					{
						address = contactName + "\n";
					}

					address += postalAddress;
				}
			}

			return address;
		}

		public override Regex Regex => fRegex;

		static readonly Regex fRegex = new Regex(@"^<\s*recipient\s*name\s*and\s*address\s*(\(\s*(?<IncludeCountryIfSameToCurrent>[Y|N]?)(\s*,\s*(?<LanguageCode>[\-A-Za-z]+))?\s*\)\s*|)>$",
			RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;
	}
}
