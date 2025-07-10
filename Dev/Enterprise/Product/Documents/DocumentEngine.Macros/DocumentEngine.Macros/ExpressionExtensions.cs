using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using CargoWise.Macros;

namespace Enterprise.DocumentEngine.Macros
{
	static class ExpressionExtensions
	{
		public static Either<ExprErrorType, object> Evaluate(this string expression)
		{
			if (string.IsNullOrWhiteSpace(expression))
			{
				return ExprErrorType.EmptyExpression;
			}

			var input = new AntlrInputStream(expression);
			var lexer = new LegacyMacroLexer(input);
			lexer.RemoveErrorListener(ConsoleErrorListener<int>.Instance);

			var tokens = new CommonTokenStream(lexer);

			var parser = new LegacyMacroParser(tokens)
			{
				Interpreter =
				{
					PredictionMode = PredictionMode.Sll
				},
				ErrorHandler = new BailErrorStrategy()
			};
			parser.RemoveErrorListener(ConsoleErrorListener<IToken>.Instance);

			LegacyMacroParser.CompilationUnitContext compilationUnit;

			try
			{
				compilationUnit = parser.compilationUnit();
			}
			catch (Antlr4.Runtime.Misc.ParseCanceledException)
			{
				return ExprErrorType.CannotParseExpression;
			}

			var visitor = new LegacyMacroExpressionVisitor();
			return visitor.VisitCompilationUnit(compilationUnit);
		}
	}
}
