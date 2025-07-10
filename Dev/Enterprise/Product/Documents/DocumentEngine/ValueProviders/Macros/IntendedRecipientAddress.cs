using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.MacroValueProviders.Utilities;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class IntendedRecipientAddress : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<IntendedRecipientAddress[({pkOfIntendedRecipientOrgAddressRecord})]>",
				ResString.GetMultilingualString("901da4c7-e52e-4112-a2d3-2c43dbbd8d21", "Returns the Postal Address of the Contact the Report or Document is supposed to be sent to without an 'ATTENTION:' prefix showing the Contact Name it's being sent to. {0}.", DocumentExtension),
				new List<(string example, object expectedResult)> {
					("<IntendedRecipientAddress>",
					(NoResString)@"FOO CORP
ADDR1
ADDR2
SYDNEY NSW 2000"),
					("<IntendedRecipientAddress(<AddressPK>)>",
					(NoResString)@"APPLECOMPANY T/AS MICROSOFT
MICROSOFTADDRESS1
MICROSOFTADDRESS2
MICROSOFTVILLE MIC 9MIC9
UNITED STATES") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			RecipientAddressFormatter formatter = new RecipientAddressFormatter(regex);
			return formatter.Format(macro, report, true);
		}

		public override Regex Regex
		{
			get { return regex; }
		}

		protected string DocumentExtension
		{
			get { return Res.GetString("50f402a5-2cd2-4d8a-a36c-29faeb09e6f6", "When the current companies country/region is Icelandic and the contacts organization has a Kennitala, 'Kennitala' for non-Icelandic debtors or '{0}' for Icelandic debtors is display beneath the address should the debtor have entered a Kennitala.", "Kennitala greiðanda"); }
		}
		static readonly Regex regex = new Regex(@"^<\s*intended\s*recipient\s*address\s*(\(\s*(?<OrgAddressPK>.*)\s*\)|)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
