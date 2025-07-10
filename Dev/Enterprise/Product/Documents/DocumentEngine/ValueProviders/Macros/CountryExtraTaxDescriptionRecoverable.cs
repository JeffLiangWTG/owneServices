using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.ValueProviders.Macros.UtilityClasses;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class CountryExtraTaxDescriptionRecoverable : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<CountryExtraTaxDescriptionRecoverable>",
				ResString.GetMultilingualString("20005005-8b61-4bb7-a53a-bed4f0e2b4fc", "Returns the extra tax's description for the current Company's login country/region + ' Payable / Recoverable' phrase."),
				new List<(string example, object expectedResult)> { ("<CountryExtraTaxDescriptionRecoverable>", (NoResString)"Extra Tax Payable / Recoverable") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			return Res.GetString("92EB57A9-2DF3-403B-AA4F-0C8B7F55441E", "{0} Payable / Recoverable", CountryExtraTaxDescriptionHelper.GetDescription(addInputPerfix: false));
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<\s*Country\s*Extra\s*Tax\s*Description\s*Recoverable\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
