using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Business.Testing
{
	class ExitNotificationMessageSenderTest : TestCaseWithFactory
	{
		public void TestSend()
		{
			var errorMessage = new ExitNotificationMessageSender().Send();
			AssertEquals(ZString.Empty, errorMessage);
		}
	}
}
