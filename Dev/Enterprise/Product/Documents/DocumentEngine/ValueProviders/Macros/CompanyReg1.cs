using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class CompanyReg1 : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<CompanyReg1>",
				ResString.GetMultilingualString("3425c3ed-18fa-40ad-a35f-1da8c935b2f9", "Returns the primary Business Registration Number from the current Company's registration in {0}.", "GlbCompany"),
				new List<(string example, object expectedResult)> { ("<CompanyReg1>", "41 065 894 724") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			return GlbCompany.CurrentCompany.GC_BusinessRegNo;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)Company(?:[\s]*)reg(?:[\s]*)1(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
