using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.MacroValueProviders.Utilities;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class NumberToWords : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<NumberToWords({decimalnumber}[, \"{languagecode}\"])>",
				ResString.GetMultilingualString("4a8d1823-57bd-4e0e-9fd4-3707b4c2c7f7",
				@"Represents any integer numeric value in words. Defaults to the language of the document, if supported, or you can specify a language using the optional second parameter."),
				new List<(string example, object expectedResult)> { ("<NumberToWords(11)>", (NoResString)"eleven"), ((NoResString)"<NumberToWords(12, ZH-CN)>", (NoResString)"壹拾贰") });
		}

		#region Properties

		public override Regex Regex
		{
			get { return regex; }
		}

		static readonly Regex regex = new Regex(@"^<(?:[\s]*)number(?:[\s]*)to(?:[\s]*)words(?:[\s]*)\((?:[\s]*)(?<decimalamount>[^,]*)(?:[\s]*)(?:,(?:[\s]*)(?<languagecode>[^\s]*)(?:[\s]*))*\)(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		#endregion

		protected override object GetReplacementCore(string macro, Report report)
		{
			Match match = regex.Match(macro.Replace("\"", ""));

			string decimalNumberOrFieldName = match.Groups["decimalamount"].Value;
			string languageCode = match.Groups["languagecode"].Value;

			return TryParseWithReportOnFail(
				report: report,
				defaultValue: string.Empty,
				parseFunc: () =>
				{
					var result = string.Empty;
					var inputNumber = 0m;

					if (!decimal.TryParse(decimalNumberOrFieldName, NumberStyles.Any, Culture.Default.NumberFormat, out inputNumber))
					{
						if (decimalNumberOrFieldName.ToUpperInvariant().IndexOfAny("ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray()) >= 0)
						{
							decimalNumberOrFieldName = report.Renderer.CurrentAreaToProcess.GetColumnValue(report.Renderer.CurrentDataRow, decimalNumberOrFieldName).ToString();
							if (!decimal.TryParse(decimalNumberOrFieldName, out inputNumber))
							{
								return string.Empty;
							}
						}
					}

					if (string.IsNullOrEmpty(languageCode))
					{
						languageCode = report.Language;
					}
					if (Res.IsEnglish(languageCode))
					{
						languageCode = Res.DefaultLanguage;
					}

					if (!string.IsNullOrEmpty(decimalNumberOrFieldName))
					{
						result = DecimalNumberToStringConvertor.ConvertDecimalToString(inputNumber, languageCode);
					}
					return result;
				});
		}

		protected override string ReplaceNestedMacro(string macro, IMacroTranslator macroTranslator, Passes currentPass)
		{
			using (Culture.SetTemporarily(Culture.Default))
			{
				return base.ReplaceNestedMacro(macro, macroTranslator, currentPass);
			}
		}
	}
}
