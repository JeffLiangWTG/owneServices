using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class RecipientFaxNumber : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<RecipientFaxNumber>",
				ResString.GetMultilingualString("967266b3-2099-4b56-9fe7-af4c4d8daab5", "Returns the Fax Number of the Contact the Report or Document is supposed to be sent to."),
				new List<(string example, object expectedResult)> { ("<RecipientFaxNumber>", "+61 2 9025 1199") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var contact = report.DeliveryContact;
			return (contact != null) ? contact.Fax.ToString() : "";
		}

		public override Regex Regex
		{
			get { return regex; }
		}
		static readonly Regex regex = new Regex(@"^<\s*recipient\s*fax\s*number\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
