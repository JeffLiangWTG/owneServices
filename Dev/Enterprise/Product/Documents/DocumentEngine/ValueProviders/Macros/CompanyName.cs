using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class CompanyName : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<CompanyName>",
				ResString.GetMultilingualString("04bf1cdc-1362-4aff-be94-d7b0e2c7638f", "Returns the Company Name from the current Company's registration in {0}.", "GlbCompany"),
				new List<(string example, object expectedResult)> { ("<CompanyName>", (NoResString)"WiseTech Global") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			return GlbCompany.CurrentCompany.GC_Name;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)Company(?:[\s]*)Name(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
