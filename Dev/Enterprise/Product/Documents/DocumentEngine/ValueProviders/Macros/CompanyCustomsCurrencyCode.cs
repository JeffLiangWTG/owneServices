using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class CompanyCustomsCurrencyCode : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<CompanyCustomsCurrencyCode>",
				ResString.GetMultilingualString("e419a6ee-a771-4d72-b51a-ed20bd3455f2", "Returns the ISO Currency Code from Customs' perspective from the current Company's registration in {0}.", "GlbCompany"),
				new List<(string example, object expectedResult)> { ("<CompanyCustomsCurrencyCode>", "AUD") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			return GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<\s*Company\s*Customs\s*Currency\s*Code\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
