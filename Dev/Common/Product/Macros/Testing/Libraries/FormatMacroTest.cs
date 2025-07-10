using CargoWise.Types;

namespace CargoWise.Macros.Testing
{
	class FormatMacroTest : TestCaseWithMacros
	{
		#region Format Formattable

		public void TestFormatZDateTime()
		{
			const string macro = "Format(ZDateTime, \"dd:MM:yy\")";

			var data = new Dummy
			{
				ZDateTime = new ZDateTime(2012, 1, 1, 7, 10, 5)
			};

			AssertMacroRun(macro, new MacroRun { Data = data, ExpectedResult = "01:01:12" });
		}

		public void TestFormatZDecimal()
		{
			const string macro = "Format(ZDecimal, \"0.00\")";

			var data = new Dummy
			{
				ZDecimal = 2.2m
			};

			AssertMacroRun(macro, new MacroRun { Data = data, ExpectedResult = "2.20" });
		}

		#endregion

		#region Implementation

		class Dummy
		{
			public ZDateTime ZDateTime { get; set; }

			public ZDecimal ZDecimal { get; set; }
		}

		#endregion
	}
}