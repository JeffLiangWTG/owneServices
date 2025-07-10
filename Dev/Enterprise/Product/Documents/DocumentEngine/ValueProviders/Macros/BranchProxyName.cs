using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class BranchProxyName : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<BranchProxyName>",
				ResString.GetMultilingualString("7e0ff44c-7834-4319-b9c0-01abbe70a385", "Returns the Organization Proxy Name for the Current Branch. If the Organization Proxy is not set on the Current Branch, falls back to the Organization Proxy Name for the Current Company."),
				new List<(string example, object expectedResult)> { ("<BranchProxyName>", GlbCompany.CurrentCompany.OrgProxy.OH_FullName) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			if (GlbBranch.CurrentBranch != null && !GlbBranch.CurrentBranch.IsNull && !GlbBranch.CurrentBranch.GB_OH_OrgProxy.IsEmpty)
			{
				return GlbBranch.CurrentBranch.OrgProxy.OH_FullName;
			}

			if (GlbCompany.CurrentCompany != null && !GlbCompany.CurrentCompany.IsNull && !GlbCompany.CurrentCompany.GC_OH_OrgProxy.IsEmpty)
			{
				return GlbCompany.CurrentCompany.OrgProxy.OH_FullName;
			}

			return string.Empty;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<\s*Branch\s*Proxy\s*Name\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
