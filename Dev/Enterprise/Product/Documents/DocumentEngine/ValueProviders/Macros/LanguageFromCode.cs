using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class LanguageFromCode : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<LanguageFromCode({code})>",
				ResString.GetMultilingualString("1853d0fc-d4f7-4914-b421-68e62e4103ea", @"Returns the description of a language when passed a valid language code."),
				new List<(string example, object expectedResult)> { ("<LanguageFromCode(EN-US)>", (NoResString)"English (American)") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			// Arg 1: Language 3-char code
			string langCode = fRegex.Match(macro).Groups[1].Value;
			CodeDescriptionPairList languageList = new CodeDescriptionPairList(OLookUpEditType.Language);

			string description = languageList.GetDescriptionFromCode(langCode);

			if (!string.IsNullOrEmpty(langCode) && description == null)
			{
				var message = Res.GetString("dacb286c-cd7e-4922-bb10-083c25eb7d44", "The language code [{0}] has no corresponding description in the macro {1}.", langCode, "<LanguageFromCode>");
				report.ErrorManager.Add(new ReportProcessingError(message, ReportProcessingErrorSeverity.Error));
				description = "";
			}
			else if (string.IsNullOrEmpty(langCode))
			{
				description = "";
			}

			return description;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)LanguageFromCode(?:[\s]*)\((?:[\s]*)([^\s]*)(?:[\s]*)\)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
