using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Business;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class IsFranceOrTerritoryMacroTest : TestCaseWithFactory
	{
		public void TestIsFranceOrTerritoryMacroTest()
		{
			AssertLocationCode("FR", true);
			AssertLocationCode("FRP", true);
			AssertLocationCode("FRPA", true);
			AssertLocationCode("FRPAR", true);

			AssertLocationCode("GF", true);
			AssertLocationCode("PF", true);
			AssertLocationCode("RE", true);
			AssertLocationCode("GF", true);
			AssertLocationCode("GP", true);
			AssertLocationCode("MQ", true);
			AssertLocationCode("YT", true);
			AssertLocationCode("NC", true);
			AssertLocationCode("BL", true);
			AssertLocationCode("MF", true);
			AssertLocationCode("PM", true);
			AssertLocationCode("WF", true);

			AssertLocationCode(null, false);
			AssertLocationCode(string.Empty, false);
			AssertLocationCode("F", false);
		}

		void AssertLocationCode(string location, bool expected)
		{
			var res = Expression.Evaluate(location);

			AssertEquals("expected no errors", string.Empty, Expression.ToFormatString());
			AssertEquals($"Expected value for: {location}", res, expected);
		}

		IMacroExpression Expression
		{
			get
			{
				if (expression == null)
				{
					const string macro = "IsFranceOrTerritory()";
					expression = macro.With<FilterLibrary>().CreateExpression();
				}

				return expression;
			}
		}

		IMacroExpression expression;
	}
}
