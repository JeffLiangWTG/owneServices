using System;

namespace Enterprise.DocumentEngine.Exceptions.Testing
{
	sealed class BODocDataProviderCollectionFormatExceptionTest : ExceptionTestCase<BODocDataProviderCollectionFormatException>
	{
		protected override BODocDataProviderCollectionFormatException GetNewExceptionToTest(string message)
		{
			return new BODocDataProviderCollectionFormatException(message, new Exception("Something wouldn't evaluate"));
		}
	}
}
