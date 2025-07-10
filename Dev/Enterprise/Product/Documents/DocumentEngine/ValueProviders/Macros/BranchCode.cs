using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class BranchCode : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<BranchCode>",
				ResString.GetMultilingualString("de77482c-273d-4cce-86c1-82e96a081221", "Returns the Branch Code for the Current Branch."),
				new List<(string example, object expectedResult)> { ("<BranchCode>", "WTG") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			return GlbBranch.CurrentBranch.GB_Code;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<\s*Branch\s*Code\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
