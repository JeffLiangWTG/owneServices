using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class CurrencyLanguageName : ValueProviderWithLoadControlFactory
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<CurrencyLanguageName({currencycode},{languagecode})>",
				ResString.GetMultilingualString("299ff570-a1f1-4d0d-a72c-b08b2dbd9a9f", @"Get currency name from language code."),
				new List<(string example, object expectedResult)> {
					((NoResString)"<CurrencyLanguageName(USD, ZH-CN)>", (NoResString)"美元"),
					((NoResString)"<CurrencyLanguageName(<CurrencyCode>, ZH-CN)>", (NoResString)"加元") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var match = Regex.Match(macro);
			var currencyCode = match.Groups[1].Value;
			var language = match.Groups[2].Value;

			if (string.IsNullOrEmpty(currencyCode))
			{
				return string.Empty;
			}

			if (string.IsNullOrEmpty(language))
			{
				language = Core.SharedConstants.Languages.English;
			}
			else if (Culture.LanguageCodeMapping.ContainsKey(language))
			{
				report.ErrorManager.Add(new ReportProcessingError(GetLanguageCodeErrorMessage(macro, language, Culture.LanguageCodeMapping[language]), ReportProcessingErrorSeverity.Warning));
				language = Culture.LanguageCodeMapping[language];
			}

			var currency = (RefCurrency)Factory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, currencyCode);

			return currency == null ? string.Empty : currency.RX_DescMultilingual.ToString(language);
		}

		protected override bool PassNestedMacroFormulaAsText => true;

		public override Regex Regex => regex;

		static readonly Regex regex = new Regex(@"^<\s*Currency\s*Language\s*Name\s*\(\s*\""*([^\""]+)\""*\s*\,\s*\""*([^\""]+)\""*\s*\)\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
