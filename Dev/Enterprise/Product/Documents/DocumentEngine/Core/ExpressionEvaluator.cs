using System;
using System.Globalization;
using CargoWise.Common;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.Macros;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine
{
	static class ExpressionEvaluator
	{
		internal static bool Evaluate(string expression, bool useJs)
		{
			try
			{
				return expression.EvaluateDocEngineExpression(useJs);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				var message = string.Format(CultureInfo.InvariantCulture, (NoResString)"Result of {0} is not a True/False expression", expression);
				throw new ExpressionEvaluationException(message, ex);
			}
		}

		internal static bool IsValidLogicalExpression(string expression) => expression.IsValidDocumentEngineLogicalExpression();
	}
}
