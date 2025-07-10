using System;
using System.Collections.Generic;
using CargoWise.Macros;
using CargoWise.Macros.Testing;
using Enterprise.DocumentVisualizer.Business;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class EmptyDateMacroTest : TestCaseWithMacros
	{
		public void TestRunMacro()
		{
			const string macro = "EmptyDate";

			var macroRun = new MacroRun()
			{
				Data = new object(),
				ExpectedResult = default(DateTime)
			};

			AssertMacroRun(macro, macroRun);
		}

		protected override IEnumerable<IMacroLibrary> Libraries
		{
			get { yield return new DataLibrary(); }
		}
	}
}
