using CargoWise.Macros;
using CargoWise.Macros.Testing;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class GetCountryMacroTest : TestCaseWithMacros
	{
		public void TestRunMacro()
		{
			const string macro = "GetCountry(\"4A039A4C-DCF4-472A-872D-CA545B12AC79\")";

			using (var scope = new MacroScope())
			{
				var expr = macro.With<DataLibrary>().CreateExpression();

				var result = (ICountry)expr.Evaluate(scope);

				AssertMultilineASCIIEquals("expected no errors", "", expr.ToFormatString());
				AssertNotNull("macro expr returned a result", result);
				AssertEquals("macro returned correct country", "Andorra", result.Description);
			}
		}

		public void TestRunMacro_InvalidCode()
		{
			const string macro = "GetCountry(\"xxx\")";

			using (var scope = new MacroScope())
			{
				var expr = macro.With<DataLibrary>().CreateExpression();

				var result = (ICountry)expr.Evaluate(scope);

				AssertMultilineASCIIEquals("expected no errors", "", expr.ToFormatString());
				AssertNull("macro expr returned nothing", result);
			}
		}
	}
}
