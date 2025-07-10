using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class CurrentCompany : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<CurrentCompany>",
				ResString.GetMultilingualString("5f9d6d81-9210-4ace-a37d-c0b5a05bdfa9", "Returns the PK for the Current Company ({0}) the current user is logged into.", "GlbCompany"),
				new List<(string example, object expectedResult)> { ("<CurrentCompany>", GlbCompany.CurrentCompany.PK) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var company = GlbCompany.CurrentCompany;
			return company != null ? company.PK : Guid.Empty;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<\s*Current\s*Company\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
