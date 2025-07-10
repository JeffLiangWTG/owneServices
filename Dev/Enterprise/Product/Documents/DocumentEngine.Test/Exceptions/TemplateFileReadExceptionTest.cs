using System;

namespace Enterprise.DocumentEngine.Exceptions.Testing
{
	sealed class TemplateFileReadExceptionTest : ExceptionTestCase<TemplateFileReadException>
	{
		protected override TemplateFileReadException GetNewExceptionToTest(string message)
		{
			return new TemplateFileReadException(message, new Exception());
		}
	}
}
