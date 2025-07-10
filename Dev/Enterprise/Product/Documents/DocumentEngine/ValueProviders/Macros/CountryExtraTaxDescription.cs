using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.ValueProviders.Macros.UtilityClasses;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class CountryExtraTaxDescription : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<CountryExtraTaxDescription>",
				ResString.GetMultilingualString("5e5608b1-e059-416e-ad53-061f09136fdf", "Returns the extra tax's description for the current Company's login country/region."),
				new List<(string example, object expectedResult)> { ("<CountryExtraTaxDescription>", (NoResString)"Extra Tax") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			return CountryExtraTaxDescriptionHelper.GetDescription();
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<\s*Country\s*Extra\s*Tax\s*Description\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
