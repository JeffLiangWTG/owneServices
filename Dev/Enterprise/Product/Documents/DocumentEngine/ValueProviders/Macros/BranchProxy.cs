using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class BranchProxy : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<BranchProxy>",
				ResString.GetMultilingualString("ee4dfea5-f9ec-455f-b65a-ff750d13c644", "Returns the Organization Proxy for the Current Branch. If the Organization Proxy is not set on the Current Branch, falls back to the Organization Proxy for the Current Company."),
				new List<(string example, object expectedResult)> { ("<BranchProxy>", GlbCompany.CurrentCompany.OrgProxy.PK) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			if (GlbBranch.CurrentBranch != null && !GlbBranch.CurrentBranch.IsNull && !GlbBranch.CurrentBranch.GB_OH_OrgProxy.IsEmpty)
			{
				return GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			}

			if (GlbCompany.CurrentCompany != null && !GlbCompany.CurrentCompany.IsNull && GlbCompany.CurrentCompany.OrgProxy != null)
			{
				return GlbCompany.CurrentCompany.OrgProxy.PK;
			}

			return string.Empty;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<\s*Branch\s*Proxy\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
