using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	internal class MacroUntranslatedValueProvider : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<MacroNameUntranslated>",
				ResString.GetMultilingualString("d83db4ff-1830-40b0-b747-61a13fcc1619", @"Returns the untranslated value of <MacroName>."),
				new List<(string example, object expectedResult)> { ("<CompanyCountryUntranslated>", (NoResString)"Germany") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var match = Regex.Match(macro);
			if (match.Success)
			{
				var originalMacro = "<" + match.Groups["macro"].Value + ">";
				var currentPass = report.Renderer?.CurrentPass ?? Passes.FirstPass;
				using (Res.TemporarilySwitchLanguage(Res.DefaultLanguage))
				using (Culture.SetTemporarily(Culture.Default))
				{
					return report.MacroTranslator.GetValue(originalMacro, currentPass);
				}
			}

			return string.Empty;
		}

		protected override string ReplaceNestedMacro(string macro, IMacroTranslator macroTranslator, Passes currentPass)
		{
			using (Res.TemporarilySwitchLanguage(Res.DefaultLanguage))
			using (Culture.SetTemporarily(Culture.Default))
			{
				return base.ReplaceNestedMacro(macro, macroTranslator, currentPass);
			}
		}

		public override Regex Regex => regex;

		static readonly Regex regex = new Regex(@"^<(?<macro>.+?)(?:[\s]*)Untranslated(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
