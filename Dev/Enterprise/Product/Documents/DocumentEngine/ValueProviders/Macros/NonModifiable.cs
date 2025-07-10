using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class NonModifiable : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<NonModifiable>",
				ResString.GetMultilingualString("80588018-1bd4-41ec-851f-e7ab0e988a8e",
				@"Used as a prefix to a subsequent macro. Makes the contents of this cell unable to be modified in visualization."),
				new List<(string example, object expectedResult)> { ("<NonModifiable><InvoiceLine.GoodsDescription>", null) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			return "";
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)Non(?:[\s]*)Modifiable(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
