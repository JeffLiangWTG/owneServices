using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.MacroValueProviders.Utilities;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class RecipientNameAndIntendedRecipientAddress : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<RecipientNameAndIntendedRecipientAddress[({pkOfIntendedRecipientOrgAddressRecord}, {IncludeCountryIfSameToCurrent: Y or N})]>",
				ResString.GetMultilingualString("5cf58741-56e1-49df-93a6-451c9cb1f609",
				@"Returns the Address of the Contact the Report or Document is supposed to be sent to with an 'ATTENTION:' prefix showing the Contact Name it's being sent to. {0}
{1} is optional, if it is not specified, it will be N.", DocumentExtension, "IncludeCountryIfSameToCurrent"),
				new List<(string example, object expectedResult)> {
					("<RecipientNameAndIntendedRecipientAddress>", ""),
					((NoResString)"<RecipientNameAndIntendedRecipientAddress(<AddressPK>, N)>", (NoResString)"WTGCOMPANY\r\nWTG ADDRESS1\r\nWTG ADDRESS2\r\nSYD NSW 2000"),
					("<RecipientNameAndIntendedRecipientAddress(<AddressPK> ,Y)>", (NoResString)"WTGCOMPANY\r\nWTG ADDRESS1\r\nWTG ADDRESS2\r\nSYD NSW 2000\r\nAUSTRALIA") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			RecipientAddressFormatter formatter = new RecipientAddressFormatter(regex);
			var includeCountryEvenIfSame = Regex.Match(macro).Groups["IncludeCountryEvenIfSame"].Value.Trim().Equals((NoResString)"y", StringComparison.OrdinalIgnoreCase);
			object formatResult = string.Empty;
			try
			{
				formatResult = formatter.Format(macro, report, false, includeCountryEvenIfSame);
			}
			catch (FormatException ex)
			{
				ReportMacroError(report, ex.Message);
			}

			return formatResult;
		}

		public override Regex Regex
		{
			get { return regex; }
		}

		protected string DocumentExtension
		{
			get { return Res.GetString("f6c1316b-a527-4918-9bfe-6ff2457b425b", "When the current companies country/region is Icelandic and the contacts organization has a Kennitala, 'Kennitala' for non-Icelandic debtors or '{0}' for Icelandic debtors is display beneath the address should the debtor have entered a Kennitala.", "Kennitala greiðanda"); }
		}
		static readonly Regex regex = new Regex(
			@"^<\s*recipient\s*name\s*and\s*intended\s*recipient\s*address\s*(\(\s*(?<OrgAddressPK>[^,\r\n]*),?\s*(?<IncludeCountryEvenIfSame>[Y|N]?)\s*\)\s*|)>$",
			RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
