using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class RecipientPhoneNumber : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<RecipientPhoneNumber>",
				ResString.GetMultilingualString("c45635f4-9c71-493b-9dd6-ea0d61b91c03", "Returns the Phone Number of the Contact the Report or Document is supposed to be sent to."),
				new List<(string example, object expectedResult)> { ("<RecipientPhoneNumber>", "+61 2 9025 1100") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			DocDeliveryContact contact = report.DeliveryContact;
			return (contact != null) ? contact.Phone.ToString() : "";
		}

		public override Regex Regex
		{
			get { return regex; }
		}
		static readonly Regex regex = new Regex(@"^<\s*recipient\s*phone\s*number\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
