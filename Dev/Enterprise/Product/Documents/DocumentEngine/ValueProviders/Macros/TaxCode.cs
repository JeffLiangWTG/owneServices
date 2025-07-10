using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	internal class TaxCode : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<TaxCode>",
				ResString.GetMultilingualString("dccb4a2b-3a74-451f-bddc-a4385bc998ba", "Returns the local description for Consumption Tax based on the Country/Region Code set on the Current Company's Registration ({0}).", "GlbCompany"),
				new List<(string example, object expectedResult)> { ("<TaxCode>", "GST") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			return GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)Tax(?:[\s]*)Code(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
