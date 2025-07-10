using CargoWise.Types;

namespace CargoWise.Macros.Testing
{
	class IfMacroTest : TestCaseWithMacros
	{
		public void TestIfMacroOnZBool()
		{
			const string macro = "If(ZBool,\"yes sir!\",\"no sir!\")";

			var macroRuns = new[]
			{
				new MacroRun
				{
					Data = new Dummy { ZBool = true },
					ExpectedResult = "yes sir!"
				},
				new MacroRun
				{
					Data = new Dummy { ZBool = false },
					ExpectedResult = "no sir!"
				}
			};

			AssertMacroRun(macro, macroRuns);
		}

		public void TestIfMacroOnNullableZBool()
		{
			const string macro = "If(NZBool,\"yes sir!\",\"no sir!\")";

			var macroRuns = new[]
			{
				new MacroRun
				{
					Data = new Dummy { NZBool = true },
					ExpectedResult = "yes sir!"
				},
				new MacroRun
				{
					Data = new Dummy { NZBool = false },
					ExpectedResult = "no sir!"
				},
				new MacroRun
				{
					Data = new Dummy { NZBool = null },
					ExpectedResult = "no sir!"
				}
			};

			AssertMacroRun(macro, macroRuns);
		}

		#region Implementation

		class Dummy
		{
			public ZBool ZBool { get; set; }
			public ZBool? NZBool { get; set; }
		}

		#endregion
	}
}