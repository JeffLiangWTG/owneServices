using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.UniversalDataBuss.Management.Testing
{
	class OriginalValueMacroTest : TestCaseWithFactory
	{
		public void TestMacro()
		{
			var dummyBizo = Factory.New<DummyBusinessObject>();
			dummyBizo.Z0_Code = "123";
			Factory.Save();
			dummyBizo.Z0_Code = "456";

			const string macro = "OriginalValue({Z0_Code})";
			var expr = macro
				.With<UniversalMacroLibrary>()
				.CreateExpression();

			using (var scope = new MacroScope(dummyBizo))
			{
				var expectedHits = new Dictionary<string, int>
				{
					{ DummyBizoSchema.Constants.TableName, 1 },
				};
				using (AssertDbHitsForAllFactories(expectedHits))
				{
					var result = expr.Evaluate(scope);
					AssertEquals("result", "123", result.ToString());
					AssertMultilineASCIIEquals("no errors", "", expr.ToFormatString());
				}

				// Expect no new hits since original bizo is cached
				var expectedHits2 = new Dictionary<string, int>
				{
					{ DummyBizoSchema.Constants.TableName, 0 },
				};
				using (AssertDbHitsForAllFactories(expectedHits2))
				{
					var resultAgain = expr.Evaluate(scope);
					AssertEquals("result", "123", resultAgain.ToString());
				}
			}
		}
	}
}
