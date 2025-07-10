using Antlr4.Runtime;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CARateFormulaVisitorTest : TestCaseWithFactory
	{
		public void TestRateFormulaExtractionVisitor()
		{
			AssertRate("MAX(1.65*VFD,2.94*[KGM])", new[] { (2.94m, 0m, 1.65m, RateTypes.Codes.Specific, "", RateTypes.Codes.AdValorem) });
			AssertRate("15.90*[TNE] + 0.07*VFD", new[] { (0m, 0m, 15.90m, "", "", RateTypes.Codes.Specific), (0m, 0m, 0.07m, "", "", RateTypes.Codes.AdValorem) });
			AssertRate("0.08*VFD", new[] { (0m, 0m, 0.08m, "", "", RateTypes.Codes.AdValorem) });
			AssertRate("3.32 * [KGM]", new[] { (0m, 0m, 3.32m, "", "", RateTypes.Codes.Specific) });
			AssertRate("MIN(9.48*[KGM], MAX(0.05*VFD,4.74*[KGM]))", new[] { (4.74m, 9.48m, 0.05m, RateTypes.Codes.Specific, RateTypes.Codes.Specific, RateTypes.Codes.AdValorem) });
			AssertRate("MAX(5.62*[KGM],0.105*VFD) + 0.04*VFD", new[] { (0.105m, 0m, 5.62m, RateTypes.Codes.AdValorem, "", RateTypes.Codes.Specific), (0m, 0m, 0.04m, "", "", RateTypes.Codes.AdValorem) });
			AssertRate("0", new[] { (0m, 0m, 0m, "", "", "") });
		}

		void AssertRate(string formular, (decimal min, decimal max, decimal regular, string minUQ, string maxUQ, string regularUQ)[] values)
		{
			var errorListener = new FormulaErrorListener();
			var visitor = new CARateFormulaVisitor(errorListener);

			// Act
			var result = visitor.VisitExpression(GetExpressionTree(formular));
			AssertEquals(values.Length, visitor.Results.Count);
			for (var i = 0; i < values.Length; i++)
			{
				var rateResult = visitor.Results[i];
				var expected = values[i];

				AssertEquals(expected.min, rateResult.DutyRateMin);
				AssertEquals(expected.max, rateResult.DutyRateMax);
				AssertEquals(expected.regular, rateResult.DutyRateRegular);
				AssertEquals(expected.minUQ, rateResult.DutyRateMinUOM);
				AssertEquals(expected.maxUQ, rateResult.DutyRateMaxUOM);
				AssertEquals(expected.regularUQ, rateResult.DutyRateRegularUOM);
			}
		}

		RateFormulaParser.ExpressionContext GetExpressionTree(ZString formulaString)
		{
			var input = new AntlrInputStream(formulaString);
			var lexer = new RateFormulaLexer(input);
			var tokens = new CommonTokenStream(lexer);
			var parser = new RateFormulaParser(tokens);
			return parser.expression();
		}
	}
}
