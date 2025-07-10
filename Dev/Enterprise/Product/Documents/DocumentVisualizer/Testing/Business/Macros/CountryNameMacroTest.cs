using System.Collections.Generic;
using CargoWise.Macros;
using CargoWise.Macros.Testing;
using Enterprise.DocumentVisualizer.Business;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class CountryNameMacroTest : TestCaseWithMacros
	{
		public void TestRunMacro()
		{
			const string macro = "GetCountryName(\"AU\")";

			var macroRun = new MacroRun
			{
				Data = new object(),
				ExpectedResult = "Australia"
			};

			AssertMacroRun(macro, macroRun);
		}

		public void TestRunMacro_InvalidCode()
		{
			const string macro = "GetCountryName(\"11\")";

			var macroRun = new MacroRun
			{
				Data = new object(),
				ExpectedResult = ""
			};

			AssertMacroRun(macro, macroRun);
		}

		protected override IEnumerable<IMacroLibrary> Libraries
		{
			get { yield return new DataLibrary(); }
		}
	}
}
