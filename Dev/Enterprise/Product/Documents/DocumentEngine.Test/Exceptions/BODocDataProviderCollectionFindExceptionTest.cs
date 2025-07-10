using System;

namespace Enterprise.DocumentEngine.Exceptions.Testing
{
	sealed class BODocDataProviderCollectionFindExceptionTest : ExceptionTestCase<BODocDataProviderCollectionFindException>
	{
		protected override BODocDataProviderCollectionFindException GetNewExceptionToTest(string message)
		{
			return new BODocDataProviderCollectionFindException(message, new Exception("Something wouldn't evaluate"));
		}
	}
}
