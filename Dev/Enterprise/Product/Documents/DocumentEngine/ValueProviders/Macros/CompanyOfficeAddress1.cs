using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class CompanyOfficeAddress1 : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<CompanyOfficeAddress1>",
				ResString.GetMultilingualString("42a9ed3e-d724-495b-a8d7-de9b3d7a7da3", "Returns the first line of address from Organization Proxy on the current Company's registration in {0}.", "GlbCompany"),
				new List<(string example, object expectedResult)> { ("<CompanyOfficeAddress1>", (NoResString)"Office Address Line 1") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			return GlbCompany.CurrentCompany.OrgProxy.MainAddress.OA_Address1;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)Company(?:[\s]*)Office(?:[\s]*)Address(?:[\s]*)1(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
