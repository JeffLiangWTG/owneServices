using System.Collections.Generic;
using CargoWise.Macros;
using CargoWise.Macros.Testing;
using Enterprise.DocumentVisualizer.Business;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class PortNameMacroTest : TestCaseWithMacros
	{
		public void TestRunMacro()
		{
			const string macro = "GetPortName(\"AUSYD\")";

			var macroRun = new MacroRun
			{
				Data = new object(),
				ExpectedResult = "Sydney"
			};

			AssertMacroRun(macro, macroRun);
		}

		public void TestRunMacro_InvalidCode()
		{
			const string macro = "GetPortName(\"11\")";

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
