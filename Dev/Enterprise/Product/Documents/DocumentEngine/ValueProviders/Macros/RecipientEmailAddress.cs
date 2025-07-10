using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class RecipientEmailAddress : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<RecipientEmailAddress>",
				ResString.GetMultilingualString("6e41fe8f-9a2e-47ca-a87f-9313359c7479", "Returns the Email Address of the Contact the Report or Document is supposed to be sent to."),
				new List<(string example, object expectedResult)> { ("<RecipientEmailAddress>", "mail@mail.com") });
		}

		protected sealed override object GetReplacementCore(string macro, Report report)
		{
			return (report.DeliveryContact != null) ? report.DeliveryContact.Email.ToString() : "";
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<\s*recipient\s*email\s*address\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
