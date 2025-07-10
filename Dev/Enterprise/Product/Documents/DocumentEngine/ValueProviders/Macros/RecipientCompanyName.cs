using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class RecipientCompanyName : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<RecipientCompanyName>",
				ResString.GetMultilingualString("31da17e5-6371-4284-9445-2149f9fc4971", "Returns the Company Name of the Contact the Report or Document is supposed to be sent to."),
				new List<(string example, object expectedResult)> { ("<RecipientCompanyName>", (NoResString)"WiseTech Global") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			return (report.DeliveryContact != null) ? report.DeliveryContact.CompanyName.ToString() : "";
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<\s*recipient\s*company\s*name\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
