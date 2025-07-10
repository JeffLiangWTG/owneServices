using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class RecipientName : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<RecipientName>",
				ResString.GetMultilingualString("719857c2-c0de-4600-8ab9-a6203d55387b", "Returns the Name of the Contact the Report or Document is supposed to be sent to."),
				new List<(string example, object expectedResult)> { ("<RecipientName>", (NoResString)"John Doe") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			return (report.DeliveryContact != null) ? report.DeliveryContact.Name.ToString() : "";
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<\s*recipient\s*name\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
