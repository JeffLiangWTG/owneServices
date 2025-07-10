using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class IncomingMessageDoesNotMatchWithStatusExceptionTest : TestCaseWithFactory
{
	public void TestIncomingMessageDoesNotMatchWithStatusException()
	{
		var ex = new IncomingMessageDoesNotMatchWithStatusException("Entry XXXXXX for job XXXX not processed: CH_Status already set.");
		AssertEquals("Custom error Message", "Entry XXXXXX for job XXXX not processed: CH_Status already set.", ex.Message);
	}
}
