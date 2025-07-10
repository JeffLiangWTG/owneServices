using CargoWise.EntityFramework.Testing;

namespace Enterprise.DataTransfer.Native.WebServiceDelivery.Testing
{
	sealed class KnownErrorExceptionTest : TestCaseWithFactory
	{
		public void TestExceptionCanBeConstructed()
		{
			var exception = new KnownErrorException("Have a message");
			AssertEquals("exception.Message", "Have a message", exception.Message);
			AssertEquals("exception.InnerException", null, exception.InnerException);

			var exceptionWithInner = new KnownErrorException("Have another message", exception);
			AssertEquals("exceptionWithInner.Message", "Have another message", exceptionWithInner.Message);
			AssertNotNull("exceptionWithInner.InnerException", exceptionWithInner.InnerException);
			AssertEquals("exceptionWithInner.InnerException.Message", "Have a message", exceptionWithInner.InnerException.Message);
		}
	}
}
