using System;

namespace Enterprise.DocumentEngine.Exceptions.Testing
{
	sealed class InvalidMenuTemplateFilterExceptionTest : ExceptionTestCase<InvalidMenuTemplateFilterException>
	{
		protected override InvalidMenuTemplateFilterException GetNewExceptionToTest(string message)
		{
			return new InvalidMenuTemplateFilterException(message, new Exception("Invalid Filter Expression"));
		}
	}
}
