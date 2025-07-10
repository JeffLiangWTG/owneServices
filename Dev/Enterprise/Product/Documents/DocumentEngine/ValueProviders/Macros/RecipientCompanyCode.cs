using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class RecipientCompanyCode : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<RecipientCompanyCode>",
				ResString.GetMultilingualString("6abbf63d-2ffe-4d82-8591-d9fd3fc38eaa", "Returns the Company Code of the Contact the Report or Document is supposed to be sent to."),
				new List<(string example, object expectedResult)> { ("<RecipientCompanyCode>", "WTG") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			ZString result = ZString.Empty;
			if (report.DeliveryContact != null && report.DeliveryContact.OrgHeader != null)
			{
				result = report.DeliveryContact.OrgHeader.OH_Code;
			}
			return result;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<\s*recipient\s*company\s*code\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
