using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class UnableToInterpretCustomsMessageExceptionTest : TestCaseWithFactory
{
	public void UnableToInterpretCustomsMessageException()
	{
		var unableToInterpretCustomsMessageException = new UnableToInterpretCustomsMessageException("Test Message");
		AssertEquals("UnableToInterpretCustomsMessageException Message", "Test Message", unableToInterpretCustomsMessageException.Message);
	}
}
