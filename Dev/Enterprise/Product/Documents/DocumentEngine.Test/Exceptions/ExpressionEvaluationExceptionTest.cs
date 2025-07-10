using System;

namespace Enterprise.DocumentEngine.Exceptions.Testing
{
	sealed class ExpressionEvaluationExceptionTest : ExceptionTestCase<ExpressionEvaluationException>
	{
		protected override ExpressionEvaluationException GetNewExceptionToTest(string message)
		{
			return new ExpressionEvaluationException(message, new Exception("Something wouldn't evaluate"));
		}
	}
}
