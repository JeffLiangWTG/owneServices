using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class CompanyPostalAddress1 : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<CompanyPostalAddress1>",
				ResString.GetMultilingualString("27b1cf79-b063-43e3-bfa9-780b2a5a16bf", "Returns the first line of the Postal Address from Organization Proxy on the current Company's registration in {0}.", "GlbCompany"),
				new List<(string example, object expectedResult)> { ("<CompanyPostalAddress1>", (NoResString)"Postal Address Line 1") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			OrgAddress postalAddress = GlbCompany.CurrentCompany.OrgProxy.Addresses.DefaultAddressOfType(OrgAddressType.Postal, false, report.Language);
			return (postalAddress == null) ? "" : (string)postalAddress.OA_Address1;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)Company(?:[\s]*)Postal(?:[\s]*)Address(?:[\s]*)1(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
