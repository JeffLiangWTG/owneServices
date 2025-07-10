using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class UrlHyperlink : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<UrlHyperlink({urllocation},{displaytext},{tooltiptext})>",
				ResString.GetMultilingualString("9a09336f-a2f1-47e9-8602-57634b39fec1",
				@"Presents a properly formatted URL or Hyperlink in the resulting output that shows using the specified display text and tool tip, and will navigate to the specified URL location if the user clicks on the link."),
				new List<(string example, object expectedResult)>
				{ ((NoResString)"<UrlHyperlink(http://www.cargowise.com,CargoWise WebSite,Click to navigate to the CargoWise Website)>", new ExcelHyperlink(FlexCel.Core.THyperLinkType.URL, (NoResString)"CargoWise WebSite", "http://www.cargowise.com", "", "", (NoResString)"Click to navigate to the CargoWise Website")) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			string linkLocation = Regex.Match(macro).Groups[1].ToString().Trim();
			string name = Regex.Match(macro).Groups[2].ToString().Trim();
			string tooltip = Regex.Match(macro).Groups[3].ToString().Trim();

			name = GetTranslatedString(name);
			tooltip = GetTranslatedString(tooltip);

			return new ExcelHyperlink(FlexCel.Core.THyperLinkType.URL, name, linkLocation, "", "", tooltip);
		}

		string GetTranslatedString(string translatableText)
		{
			var quotes = "\"";
			if (translatableText.StartsWith(quotes) && translatableText.EndsWith(quotes))
			{
				translatableText = translatableText.Trim(quotes[0]);
				if (!string.IsNullOrEmpty(translatableText))
				{
					var stringDataName = DocBuilderResourceStrings.GetDocLabelStringData(translatableText);
					if (stringDataName != null && !string.IsNullOrEmpty(stringDataName.Caption))
					{
						translatableText = stringDataName.Caption;
					}
				}
			}
			return translatableText;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)UrlHyperlink(?:[\s]*)\((?:[\s]*)([^\s]*)(?:[\s]*),(?:[\s]*)(.+)(?:[\s]*),(?:[\s]*)(.*)(?:[\s]*)\)(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
