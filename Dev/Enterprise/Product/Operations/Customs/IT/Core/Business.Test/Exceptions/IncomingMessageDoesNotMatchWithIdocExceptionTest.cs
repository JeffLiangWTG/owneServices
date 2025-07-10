using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class IncomingMessageDoesNotMatchWithIdocExceptionTest : TestCaseWithFactory
{
	public void TestIncomingMessageDoesNotMatchWithIdocException()
	{
		var ex = new IncomingMessageDoesNotMatchWithIdocException();
		AssertEquals("Message", "Incoming message does not match with associated transmit IDOC interchange.", ex.Message);

		ex = new IncomingMessageDoesNotMatchWithIdocException("Icntrl message does not match with the associated transmit message.");
		AssertEquals("Custom error Message", "Icntrl message does not match with the associated transmit message.", ex.Message);
	}
}
