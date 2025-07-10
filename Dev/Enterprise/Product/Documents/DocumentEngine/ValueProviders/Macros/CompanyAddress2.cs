using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class CompanyAddress2 : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<CompanyAddress2>",
				ResString.GetMultilingualString("172a5e9a-6e11-406c-baa8-9767f4fd6efa", "Returns the second line of address from the current Company's registration in {0}.", "GlbCompany"),
				new List<(string example, object expectedResult)> { ("<CompanyAddress2>", "Address2") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			return GlbCompany.CurrentCompany.GC_Address2;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)Company(?:[\s]*)Address(?:[\s]*)2(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
