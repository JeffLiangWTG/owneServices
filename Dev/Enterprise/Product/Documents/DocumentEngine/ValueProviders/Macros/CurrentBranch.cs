using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class CurrentBranch : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<CurrentBranch>",
				ResString.GetMultilingualString("031bbc07-622a-4219-9140-1d9f3b6fa6f9", "Returns the PK for the Current Branch ({0}) the current user is logged into.", "GlbBranch"),
				new List<(string example, object expectedResult)> { ("<CurrentBranch>", GlbBranch.CurrentBranch.PK) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var branch = GlbBranch.CurrentBranch;
			return branch != null ? branch.PK : Guid.Empty;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<\s*Current\s*Branch\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
