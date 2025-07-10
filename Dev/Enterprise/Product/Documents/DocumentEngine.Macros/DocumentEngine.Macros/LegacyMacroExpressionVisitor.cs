using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Macros;

#pragma warning disable IDE0005 //Failing in AnalyzerRunner yet it is required, Andrew Hong Investigating
using static Enterprise.DocumentEngine.Macros.LegacyMacroParser;
#pragma warning restore IDE0005

namespace Enterprise.DocumentEngine.Macros
{
	#region SuppressResourceStringsCheckRegion

	sealed class LegacyMacroExpressionVisitor : LegacyMacroParserBaseVisitor<Either<ExprErrorType, object>>
	{
		public override Either<ExprErrorType, object> VisitCompilationUnit(CompilationUnitContext context)
		{
			return Visit(context.expr);
		}

		public override Either<ExprErrorType, object> VisitNumber(NumberContext context)
		{
			var text = context.GetText();

			if (text.Contains("."))
			{
				if (Double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out double resDouble))
				{
					return resDouble;
				}
			}
			else if (Int32.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out int resInt))
			{
				return resInt;
			}

			return ExprErrorType.InvalidNumber;
		}

		public override Either<ExprErrorType, object> VisitText(TextContext context)
		{
			var text = context.GetText();
			return string.Concat(text.Skip(1).Take(text.Length - 2));
		}

		public override Either<ExprErrorType, object> VisitLiteral(LiteralContext context)
		{
			return context.TRUE() != null;
		}

		public override Either<ExprErrorType, object> VisitUnaryExpr(UnaryExprContext context)
		{
			var expr = Visit(context.expression());

			if (expr.IsLeft)
			{
				return expr.Left;
			}

			if (expr.Right is bool rightBool)
			{
				return !rightBool;
			}

			return false;
		}

		public override Either<ExprErrorType, object> VisitBracketExpr(BracketExprContext context)
		{
			return Visit(context.expression());
		}

		public override Either<ExprErrorType, object> VisitMethodCallExpr(MethodCallExprContext context)
		{
			var targetEither = Visit(context.target);

			if (targetEither.IsLeft)
			{
				return targetEither.Left;
			}

			var target = Convert.ToString(targetEither.Right, CultureInfo.InvariantCulture);

			var substr = context.call.substr();
			if (substr != null)
			{
				return EvaluateSubstr(target, substr);
			}

			var indexOf = context.call.indexOf();
			if (indexOf != null)
			{
				return EvaluateIndexOf(target, indexOf);
			}

			var contains = context.call.contains();
			if (contains != null)
			{
				return EvaluateContains(target, contains);
			}

			var toLower = context.call.toLower();
			if (toLower != null)
			{
				return EvaluateToLower(target);
			}

			var startsWith = context.call.startsWith();
			if (startsWith != null)
			{
				return EvaluateStartsWith(target, startsWith);
			}

			var endsWith = context.call.endsWith();
			if (endsWith != null)
			{
				return EvaluateEndsWith(target, endsWith);
			}

			return ExprErrorType.UnknownFunctionCall;
		}

		Either<ExprErrorType, object> EvaluateSubstr(string target, SubstrContext context)
		{
			if (string.IsNullOrEmpty(target))
			{
				return string.Empty;
			}

			var startIndexEither = context.startIndex != null
				? VisitNumber(context.startIndex)
				: 0;

			if (startIndexEither.IsLeft)
			{
				return startIndexEither;
			}

			var lengthEither = VisitNumber(context.length);

			var startIndex = Convert.ToInt32(startIndexEither.Right, CultureInfo.InvariantCulture);
			var length = Convert.ToInt32(lengthEither.Right, CultureInfo.InvariantCulture);

			return string.Concat(target.Skip(startIndex).Take(length));
		}

		Either<ExprErrorType, object> EvaluateIndexOf(string target, IndexOfContext context)
		{
			if (string.IsNullOrEmpty(target))
			{
				return -1;
			}

			var textToSearchEither = VisitText(context.textToSearchFor);

			if (textToSearchEither.IsLeft)
			{
				return textToSearchEither;
			}

			var startIndexEither = context.startIndex != null
				? VisitNumber(context.startIndex)
				: 0;

			if (startIndexEither.IsLeft)
			{
				return startIndexEither;
			}

			var textToSearch = Convert.ToString(textToSearchEither.Right, CultureInfo.InvariantCulture);
			var startIndex = Convert.ToInt32(startIndexEither.Right, CultureInfo.InvariantCulture);

			return target.IndexOf(textToSearch, startIndex, StringComparison.OrdinalIgnoreCase);
		}

		Either<ExprErrorType, object> EvaluateContains(string target, ContainsContext context)
		{
			if (string.IsNullOrEmpty(target))
			{
				return false;
			}

			var textToSearchEither = VisitText(context.textToSearchFor);

			if (textToSearchEither.IsLeft)
			{
				return textToSearchEither;
			}

			var startIndexEither = context.startIndex != null
				? VisitNumber(context.startIndex)
				: 0;

			if (startIndexEither.IsLeft)
			{
				return startIndexEither;
			}

			var textToSearch = Convert.ToString(textToSearchEither.Right, CultureInfo.InvariantCulture);
			var startIndex = Convert.ToInt32(startIndexEither.Right, CultureInfo.InvariantCulture);

			if (startIndex > 0)
			{
				target = string.Concat(target.Skip(startIndex));
			}

			return target.Contains(textToSearch);
		}

		Either<ExprErrorType, object> EvaluateStartsWith(string target, StartsWithContext context)
		{
			if (string.IsNullOrEmpty(target))
			{
				return false;
			}

			var textToSearchEither = VisitText(context.textToSearchFor);

			if (textToSearchEither.IsLeft)
			{
				return textToSearchEither;
			}

			var textToSearch = Convert.ToString(textToSearchEither.Right, CultureInfo.InvariantCulture);

			return target.StartsWith(textToSearch, StringComparison.Ordinal);
		}

		Either<ExprErrorType, object> EvaluateEndsWith(string target, EndsWithContext context)
		{
			if (string.IsNullOrEmpty(target))
			{
				return false;
			}

			var textToSearchEither = VisitText(context.textToSearchFor);

			if (textToSearchEither.IsLeft)
			{
				return textToSearchEither;
			}

			var textToSearch = Convert.ToString(textToSearchEither.Right, CultureInfo.InvariantCulture);

			return target.EndsWith(textToSearch, StringComparison.Ordinal);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
		Either<ExprErrorType, object> EvaluateToLower(string target)
		{
			if (string.IsNullOrEmpty(target))
			{
				return string.Empty;
			}

			return target.ToLowerInvariant();
		}

		public override Either<ExprErrorType, object> VisitNullPrimaryExpr(NullPrimaryExprContext context)
		{
			return null;
		}

		public override Either<ExprErrorType, object> VisitBinaryExpr(BinaryExprContext context)
		{
			var leftEither = Visit(context.left);

			if (leftEither.IsLeft)
			{
				return leftEither;
			}

			var left = leftEither.Right;

			var rightEither = Visit(context.right);

			if (rightEither.IsLeft)
			{
				return rightEither;
			}

			var right = rightEither.Right;

			return EvaluateBinaryExpr(left, right, context.op.Type);
		}

		readonly static Regex IsNumberInQuotesRegex = new Regex("^-?[0-9]+(\\.[0-9]*)?$", RegexOptions.Compiled);

		Either<ExprErrorType, object> EvaluateBinaryExpr(object left, object right, int op)
		{
			var leftType = left?.GetType() ?? typeof(object);
			var rightType = right?.GetType() ?? typeof(object);

			if (leftType != rightType)
			{
				if (op == LOGICALOR)
				{
					return left;
				}
				else if (op == LOGICALAND)
				{
					return right;
				}
			}

			if (leftType == typeof(string))
			{
				return EvaluateBinaryExprOnLeftString(Convert.ToString(left, CultureInfo.InvariantCulture), right, rightType, op);
			}
			else if (rightType == typeof(string))
			{
				return EvaluateBinaryExprOnRightString(left, leftType, Convert.ToString(right, CultureInfo.InvariantCulture), op);
			}
			else if (leftType == rightType)
			{
				return EvaluateBinaryExprOnSameTypes(left, right, leftType, op);
			}
			else if (leftType == typeof(int) && rightType == typeof(double)
				|| leftType == typeof(double) && rightType == typeof(int))
			{
				return EvaluateExpression(Convert.ToDouble(left, CultureInfo.InvariantCulture), Convert.ToDouble(right, CultureInfo.InvariantCulture), op);
			}

			return false;
		}

		Either<ExprErrorType, object> EvaluateBinaryExprOnSameTypes(object left, object right, Type type, int op)
		{
			if (type == typeof(bool))
			{
				return EvaluateExpression(Convert.ToBoolean(left, CultureInfo.InvariantCulture), Convert.ToBoolean(right, CultureInfo.InvariantCulture), op);
			}

			if (type == typeof(int))
			{
				return EvaluateExpression(Convert.ToInt32(left, CultureInfo.InvariantCulture), Convert.ToInt32(right, CultureInfo.InvariantCulture), op);
			}

			if (type == typeof(double))
			{
				return EvaluateExpression(Convert.ToDouble(left, CultureInfo.InvariantCulture), Convert.ToDouble(right, CultureInfo.InvariantCulture), op);
			}

			return ExprErrorType.InvalidBinaryExpr;
		}

		Either<ExprErrorType, object> EvaluateBinaryExprOnLeftString(string left, object right, Type rightType, int op)
		{
			if (rightType == typeof(string))
			{
				return EvaluateExpression(left, Convert.ToString(right, CultureInfo.InvariantCulture), op);
			}

			if (rightType == typeof(bool))
			{
				if (Boolean.TryParse(left, out bool leftBool))
				{
					return EvaluateExpression(leftBool, Convert.ToBoolean(right, CultureInfo.InvariantCulture), op);
				}
				return false;
			}

			if (rightType == typeof(int)
				|| rightType == typeof(double))
			{
				var rightDouble = Convert.ToDouble(right, CultureInfo.InvariantCulture);
				if (IsNumberInQuotesRegex.IsMatch(left)
					&& Double.TryParse(left, NumberStyles.Any, CultureInfo.InvariantCulture, out double leftDouble))
				{
					return EvaluateExpression(leftDouble, rightDouble, op);
				}
				return false;
			}

			return ExprErrorType.InvalidBinaryExpr;
		}

		Either<ExprErrorType, object> EvaluateBinaryExprOnRightString(object left, Type leftType, string right, int op)
		{
			if (leftType == typeof(string))
			{
				return EvaluateExpression(Convert.ToString(left, CultureInfo.InvariantCulture), right, op);
			}

			if (leftType == typeof(bool))
			{
				if (Boolean.TryParse(right, out bool rightBool))
				{
					return EvaluateExpression(Convert.ToBoolean(left, CultureInfo.InvariantCulture), rightBool, op);
				}

				return false;
			}

			if (leftType == typeof(int)
				|| leftType == typeof(double))
			{
				var leftDouble = Convert.ToDouble(left, CultureInfo.InvariantCulture);
				if (IsNumberInQuotesRegex.IsMatch(right)
					&& Double.TryParse(right, NumberStyles.Any, CultureInfo.InvariantCulture, out double rightDouble))
				{
					return EvaluateExpression(leftDouble, rightDouble, op);
				}
				return false;
			}

			return ExprErrorType.InvalidBinaryExpr;
		}

		Either<ExprErrorType, object> EvaluateExpression(bool left, bool right, int op)
		{
			switch (op)
			{
				case LOGICALEQUALS:
				case STRICTLOGICALEQUALS:
					return left == right;

				case LOGICALNOTEQUALS:
				case STRICTLOGICALNOTEQUALS:
					return left != right;

				case LOGICALOR:
					return left || right;

				case LOGICALAND:
					return left && right;

				default:
					if (IsArithmeticOperator(op))
					{
						return EvaluateExpression(Convert.ToInt32(left), Convert.ToInt32(right), op);
					}
					break;
			}

			return ExprErrorType.InvalidBinaryExpr;
		}

		Either<ExprErrorType, object> EvaluateExpression(string left, string right, int op)
		{
			switch (op)
			{
				case LOGICALEQUALS:
				case STRICTLOGICALEQUALS:
					return string.Compare(left, right, StringComparison.Ordinal) == 0;

				case LOGICALNOTEQUALS:
				case STRICTLOGICALNOTEQUALS:
					return string.Compare(left, right, StringComparison.Ordinal) != 0;

				case GREATERTHAN:
				case HTMLGREATERTHAN:
					return string.Compare(left, right, StringComparison.Ordinal) > 0;

				case GREATERTHANOREQUALS:
				case HTMLGREATERTHANOREQUALS:
					return string.Compare(left, right, StringComparison.Ordinal) >= 0;

				case LESSTHAN:
				case HTMLLESSTHAN:
					return string.Compare(left, right, StringComparison.Ordinal) < 0;

				case LESSTHANOREQUALS:
				case HTMLLESSTHANOREQUALS:
					return string.Compare(left, right, StringComparison.Ordinal) <= 0;

				case PLUS:
					return string.Concat(left, right);

				case LOGICALAND:
					return right;

				case LOGICALOR:
					return left;

				default:
					if (IsArithmeticOperator(op)
						&& IsNumberInQuotesRegex.IsMatch(left)
						&& IsNumberInQuotesRegex.IsMatch(right)
						&& Double.TryParse(left, NumberStyles.Any, CultureInfo.InvariantCulture, out double leftDouble)
						&& Double.TryParse(right, NumberStyles.Any, CultureInfo.InvariantCulture, out double rightDouble))
					{
						return EvaluateBinaryExpr(leftDouble, rightDouble, op);
					}
					break;
			}

			return ExprErrorType.InvalidBinaryExpr;
		}

		Either<ExprErrorType, object> EvaluateExpression(double left, double right, int op)
		{
			switch (op)
			{
				case HTMLGREATERTHAN:
				case GREATERTHAN:
					return left > right;

				case HTMLGREATERTHANOREQUALS:
				case GREATERTHANOREQUALS:
					return left >= right;

				case HTMLLESSTHAN:
				case LESSTHAN:
					return left < right;

				case HTMLLESSTHANOREQUALS:
				case LESSTHANOREQUALS:
					return left <= right;

				case LOGICALEQUALS:
				case STRICTLOGICALEQUALS:
					return left == right;

				case LOGICALNOTEQUALS:
				case STRICTLOGICALNOTEQUALS:
					return left != right;

				case PLUS:
					return left + right;

				case LOGICALAND:
					return right;

				case LOGICALOR:
					return left;

				case MINUS:
					return left - right;

				case MULTIPLY:
					return left * right;

				case DIVIDE:
					if (left == 0d)
					{
						return Double.PositiveInfinity;
					}
					return left / right;
			}

			return ExprErrorType.InvalidBinaryExpr;
		}

		Either<ExprErrorType, object> EvaluateExpression(int left, int right, int op)
		{
			switch (op)
			{
				case HTMLGREATERTHAN:
				case GREATERTHAN:
					return left > right;

				case HTMLGREATERTHANOREQUALS:
				case GREATERTHANOREQUALS:
					return left >= right;

				case HTMLLESSTHAN:
				case LESSTHAN:
					return left < right;

				case HTMLLESSTHANOREQUALS:
				case LESSTHANOREQUALS:
					return left <= right;

				case LOGICALEQUALS:
				case STRICTLOGICALEQUALS:
					return left == right;

				case LOGICALNOTEQUALS:
				case STRICTLOGICALNOTEQUALS:
					return left != right;

				case PLUS:
					return left + right;

				case LOGICALAND:
					return right;

				case LOGICALOR:
					return left;

				case MINUS:
					return left - right;

				case MULTIPLY:
					return left * right;

				case DIVIDE:
					if (left == 0)
					{
						return Double.PositiveInfinity;
					}
					return left / right;
			}

			return ExprErrorType.InvalidBinaryExpr;
		}

		bool IsArithmeticOperator(int op)
		{
			switch (op)
			{
				case MULTIPLY:
				case DIVIDE:
				case PLUS:
				case MINUS:
					return true;

				default:
					return false;
			}
		}
	}

	#endregion
}
