using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.ValueProviders.Macros.UtilityClasses;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class CountryExtraTaxDescriptionNotRecoverable : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<CountryExtraTaxDescriptionNotRecoverable>",
				ResString.GetMultilingualString("fd4fcca5-db4c-437e-9cb0-2a31224afd8a", "Returns the extra tax's description for the current Company's login country/region + ' Not Recoverable' phrase."),
				new List<(string example, object expectedResult)> { ("<CountryExtraTaxDescriptionNotRecoverable>", (NoResString)"Input Extra Tax Not Recoverable") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			return Res.GetString("e9715d71-f748-4bfa-9cf0-d382338b2027", "{0} Not Recoverable", CountryExtraTaxDescriptionHelper.GetDescription(addInputPerfix: true));
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<\s*Country\s*Extra\s*Tax\s*Description\s*Not\s*Recoverable\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
