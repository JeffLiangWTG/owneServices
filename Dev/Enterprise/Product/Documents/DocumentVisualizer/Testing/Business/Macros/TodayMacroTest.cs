using System;
using System.Collections.Generic;
using CargoWise.Macros;
using CargoWise.Macros.Testing;
using Enterprise.DocumentVisualizer.Business;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class TodayMacroTest : TestCaseWithMacros
	{
		[TestDate(2014, 12, 31, 12, 31, 2)]
		public void TestRunMacro()
		{
			const string macro = "Today";

			var macroRun = new MacroRun
			{
				Data = new object(),
				ExpectedResult = new DateTime(2014, 12, 31)
			};

			AssertMacroRun(macro, macroRun);
		}

		protected override IEnumerable<IMacroLibrary> Libraries
		{
			get { yield return new DataLibrary(); }
		}
	}
}