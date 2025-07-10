using System.Collections.Generic;
using CargoWise.Macros;
using CargoWise.Macros.Testing;
using Enterprise.DocumentVisualizer.Business;
using Res = Enterprise.DocumentVisualizer.Business.Res;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class NumberToWordsMacroTest : TestCaseWithMacros
	{
		public void TestNumberToWords_FallbackEnglishWhenResultIsEmpty()
		{
			AssertConvertNumberToWords("NumberToWords(12,\"XXX\")", "twelve");
			AssertConvertNumberToWords("NumberToWords(12, \"ZH-CN\")", "壹拾贰");
		}

		public void TestNumberToWords()
		{
			AssertConvertNumberToWords("NumberToWords(12)", "twelve");
			AssertConvertNumberToWords("NumberToWords(\"12\")", "twelve");
			AssertConvertNumberToWords("NumberToWords(12,\"EN\")", "twelve");
			AssertConvertNumberToWords("NumberToWords(12.25,\"EN-US\")", "twelve point twenty five");
			AssertConvertNumberToWords("NumberToWords(\"12.25\",\"EN-US\")", "twelve point twenty five");
			AssertConvertNumberToWords("NumberToWords(12.25)", "twelve point twenty five");

			using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.ChineseSimplified))
			{
				AssertConvertNumberToWords("NumberToWords(12)", "壹拾贰");
			}
		}

		protected override IEnumerable<IMacroLibrary> Libraries
		{
			get { yield return new DataLibrary(); }
		}

		void AssertConvertNumberToWords(string macro, string expectedWords)
		{
			var macroRun = new MacroRun
			{
				Data = new object(),
				ExpectedResult = expectedWords
			};

			AssertMacroRun(macro, macroRun);
		}
	}
}
