using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class MenuTitle : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<MenuTitle[({Translatable})]>",
				ResString.GetMultilingualString("8271f38a-6275-462a-96a6-87aedb0b642b", @"Returns the document title. An optional parameter will translate the document title from English to the language used in the document delivery.
Note: Language translation is only supported in DocBuilder documents."),
				new List<(string example, object expectedResult)>
				{
						("<MenuTitle>", (NoResString)"Pre Alert"),
						("<MenuTitle(Y)>", (NoResString)"货况预报")
				});
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var result = "";

			if (report.Parent.StmMenuCommand != null)
			{
				var match = Regex.Match(macro);
				if (match.Groups["Translatable"].Value == "Y")
				{
					result = report.Parent.StmMenuCommand.SU_MenuNameMultilingual;
				}
				else
				{
					result = report.Parent.StmMenuCommand.SU_MenuName;
				}
			}

			return result;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)Menu(?:[\s]*)Title(?:[\s]*)(\((?:[\s]*)(?<Translatable>.*)(?:[\s]*)\)(?:[\s]*))?>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
