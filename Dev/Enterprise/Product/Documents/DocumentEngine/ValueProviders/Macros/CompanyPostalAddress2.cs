using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class CompanyPostalAddress2 : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<CompanyPostalAddress2>",
				ResString.GetMultilingualString("ed400a5d-3ea0-47c8-9d87-9a623973603f", "Returns the second line of the Postal Address from Organization Proxy on the current Company's registration in {0}.", "GlbCompany"),
				new List<(string example, object expectedResult)> { ("<CompanyPostalAddress2>", (NoResString)"Postal Address Line 2") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			OrgAddress postalAddress = GlbCompany.CurrentCompany.OrgProxy.Addresses.DefaultAddressOfType(OrgAddressType.Postal, false, report.Language);
			return (postalAddress == null) ? "" : (string)postalAddress.OA_Address2;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)Company(?:[\s]*)Postal(?:[\s]*)Address(?:[\s]*)2(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
