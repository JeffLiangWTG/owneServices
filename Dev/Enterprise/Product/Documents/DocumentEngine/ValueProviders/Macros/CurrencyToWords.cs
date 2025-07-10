using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.MacroValueProviders.Utilities;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class CurrencyToWords : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<CurrencyToWords({decimalamount},{currencycode}[,{outputlanguagecode} [,{numberformat}]])>",
				ResString.GetMultilingualString("66c57d4b-2c26-4438-a005-c75f07e1450e", @"Inserts the decimal value expressed as words appropriate for the language and currency specified. Defaults to the language of the document, if supported, or you can specify a language using the optional third parameter.
You can also specify an optional fourth parameter to specify the special number format the amount should be shown in. Currently this function supports South Asia Lake Crore Format, and it will only be effective when the language code is ENG.
Decimal values are shown as numbers by default and can be shown as words by specifying LTR in the macro."),
				new List<(string example, object expectedResult)> {
					("<CurrencyToWords(1.5,USD)>", (NoResString)"one dollar and 50 cents"),
					((NoResString)"<CurrencyToWords(<Charges.Value>, <Charges.CurrencyCode>,ZH-CN)>", (NoResString)"美元壹拾贰元伍角整"),
					((NoResString)"<CurrencyToWords(<Charges.Value>, <Charges.CurrencyCode>, ENG, SAF)>", (NoResString)"twelve and cents fifty"),
					((NoResString)"<CurrencyToWords(<Charges.Value>, <Charges.CurrencyCode>, AR-AE, LTR)>", (NoResString)"اثنا عشر دولار و خمسون سنتات") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			Match match = fRegex.Match(macro);
			if (string.IsNullOrEmpty(match.Groups["Amount"].Value))
			{
				return "";
			}
			double amount = 0;
			try
			{
				amount = Convert.ToDouble(match.Groups["Amount"].Value, Culture.Default.NumberFormat);
			}
			catch (FormatException ex)
			{
				ReportMacroError(report, ex.Message);
				return "";
			}
			string currencyCode = match.Groups["Currency"].Value;
			string languageCode = match.Groups["Language"].Value.Trim();
			if (string.IsNullOrEmpty(languageCode))
			{
				languageCode = report.Language;
			}
			else if (Culture.LanguageCodeMapping.ContainsKey(languageCode))
			{
				report.ErrorManager.Add(new ReportProcessingError(GetLanguageCodeErrorMessage(macro, languageCode, Culture.LanguageCodeMapping[languageCode]), ReportProcessingErrorSeverity.Warning));
				languageCode = Culture.LanguageCodeMapping[languageCode];
			}

			if (Res.IsEnglish(languageCode))
			{
				languageCode = Res.DefaultLanguage;
			}
			string format = match.Groups["Format"] != null && !string.IsNullOrEmpty(match.Groups["Format"].Value) ? match.Groups["Format"].Value : "";

			string result = "";
			CurrencyConvertor currencyConvertorToUse = null;
			Type converterType;
			var specialFormat = CurrencyConvertor.SpecialFormat.None;
			Enum.TryParse<CurrencyConvertor.SpecialFormat>(format, true, out specialFormat);

			if (specialFormat == CurrencyConvertor.SpecialFormat.SAF && languageCode == Res.DefaultLanguage)
			{
				converterType = GetConverter("ENG_SAF");
			}
			else
			{
				converterType = GetConverter(languageCode);
			}
			if (converterType == null)
			{
				converterType = GetConverter(Core.Constants.Languages.English);
			}
			if (converterType == null)
			{
				return "";
			}
			currencyConvertorToUse = Activator.CreateInstance(converterType) as CurrencyConvertor;
			result = currencyConvertorToUse.ConvertToWords(amount, currencyCode, specialFormat);
			return result;
		}

		protected override string ReplaceNestedMacro(string macro, IMacroTranslator macroTranslator, Passes currentPass)
		{
			using (Culture.SetTemporarily(Culture.Default))
			{
				return base.ReplaceNestedMacro(macro, macroTranslator, currentPass);
			}
		}

		Type GetConverter(String languageCode)
		{
			return Type.GetType(GetType().Namespace + ".Utilities.CurrencyToWords_" + languageCode.Replace('-', '_'), false);
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:\s*)currency(?:\s*)to(?:\s*)words(?:\s*)\((?:\s*)(?<Amount>[^,]+)(?:\s*),(?:\s*)(?<Currency>[^\s\,]+)(?:\s*)(,(?:\s*)(?<Language>[^\,]*)(?:\s*)(,(?:\s*)(?<Format>[^\s\,]+))?)?(?:\s*)\)(?:\s*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
