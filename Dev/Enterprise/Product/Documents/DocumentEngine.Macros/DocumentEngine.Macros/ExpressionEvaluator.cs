using System;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.Macros;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.Macros
{
	public static class ExpressionEvaluator
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message")]
		public static bool EvaluateDocEngineExpression(this string expression, bool isJsEvaluator)
		{
			var result = expression.EvaluateExpression(isJsEvaluator);

			if (result.IsRight)
			{
				return result.Right;
			}

			var message = result.Left == Utilities.ExpressionEvaluator.ErrorCode.JScriptEngineError || result.Left == Utilities.ExpressionEvaluator.ErrorCode.InvalidExpression
				? string.Format(CultureInfo.CurrentCulture, "Error evaluating {0}", expression)
				: string.Format(CultureInfo.CurrentCulture, "Result of {0} is not a True/False expression", expression);

			throw new InvalidOperationException(message);
		}

		public static Either<Utilities.ExpressionEvaluator.ErrorCode, bool> EvaluateExpression(this string expression, bool isJsEvaluator)
		{
			if (isJsEvaluator)
			{
				return Utilities.ExpressionEvaluator.EvaluateJS(expression);
			}
			return expression.EvaluateDocEngineExpressionNoJS();
		}

		public static Either<Utilities.ExpressionEvaluator.ErrorCode, bool> EvaluateDocEngineExpressionNoJS(this string docEngineExpression, bool escapeQuotesAndRetryToEvaluate = true)
		{
			var res = docEngineExpression.Evaluate();

			if (res.IsLeft)
			{
				if (escapeQuotesAndRetryToEvaluate
					&& res.Left == ExprErrorType.CannotParseExpression)
				{
					var exprToEscape = docEngineExpression
						.Replace("&gt;", ">")
						.Replace("&lt;", "<");

					var escapedExpression = Utilities.ExpressionEvaluator.EscapeDoubleQuotesAndBackslashesWithinExpressionValues(exprToEscape);

					if (string.CompareOrdinal(exprToEscape, escapedExpression) != 0)
					{
						var res2 = escapedExpression.EvaluateDocEngineExpressionNoJS(false);

						if (res2.IsRight)
						{
							return res2.Right;
						}
					}
				}

				return Utilities.ExpressionEvaluator.ErrorCode.InvalidExpression;
			}

			if (res.Right is bool resBool)
			{
				return resBool;
			}

			return Utilities.ExpressionEvaluator.ErrorCode.NotTrueFalseExpression;
		}

		static readonly Regex OldDocumentFilterRegex = new Regex("[A-Z]+=[A-Z]+", RegexOptions.Compiled);

		public static bool IsValidDocumentEngineLogicalExpression(this string docEngineExpression)
		{
			if (OldDocumentFilterRegex.IsMatch(docEngineExpression))
			{
				return false;
			}

			var result = RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.Value
				? Utilities.ExpressionEvaluator.EvaluateJS(docEngineExpression)
				: docEngineExpression.EvaluateDocEngineExpressionNoJS();

			return result.IsRight;
		}
	}
}
