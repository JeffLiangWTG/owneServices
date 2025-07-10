using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class CompanyNameChina : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<CompanyNameChina>",
				ResString.GetMultilingualString("b72dedca-4c16-4e26-ae14-081bdef2ffdc", "Returns the Company Name from the main Accounts Receivable Address against the Organization Proxy on the current Company's registration in {0}. That has the capability to store Chinese Characters.",
				"GlbCompany"),
				new List<(string example, object expectedResult)> { ("<CompanyNameChina>", "CHINACOMPANY") });
		}

		#region GetReplacement
		protected override object GetReplacementCore(string macro, Report report)
		{
			return LocalCompanyName.GetCurrentCompanyLocalName();
		}
		#endregion

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)Company(?:[\s]*)Name(?:[\s]*)China(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
