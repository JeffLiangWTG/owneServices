using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class BranchProxyCode : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<BranchProxyCode>",
				ResString.GetMultilingualString("839152d8-6b34-4284-a886-8eb9424f350b", "Returns the Organization Proxy Code for the Current Branch. If the Organization Proxy is not set on the Current Branch, falls back to the Organization Proxy Code for the Current Company."),
				new List<(string example, object expectedResult)> { ("<BranchProxyCode>", GlbCompany.CurrentCompany.OrgProxy.OH_Code) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			if (GlbBranch.CurrentBranch != null && !GlbBranch.CurrentBranch.IsNull && !GlbBranch.CurrentBranch.GB_OH_OrgProxy.IsEmpty)
			{
				return GlbBranch.CurrentBranch.OrgProxy.OH_Code;
			}

			return GlbCompany.CurrentCompany.OrgProxy.OH_Code;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<\s*Branch\s*Proxy\s*Code\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
