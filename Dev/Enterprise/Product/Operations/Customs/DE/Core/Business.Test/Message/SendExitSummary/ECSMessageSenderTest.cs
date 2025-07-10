using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Business.Testing
{
	class ECSMessageSenderTest : TestCaseWithFactory
	{
		public void TestSend()
		{
			var errorMessage = new ECSMessageSender().Send();
			AssertEquals(ZString.Empty, errorMessage);
		}
	}
}
