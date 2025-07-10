using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	sealed class EDIMessageTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForBinding()
		{
			AssertNull(new EDIMessageTypeDecider().GetTypeForBinding());
		}

		public void TestGetTypeForNew()
		{
			AssertNull(new EDIMessageTypeDecider().GetTypeForNew());
		}

		public void TestGetTypeForLoad()
		{
			var message = Factory.New<Enterprise.Messaging.Business.EDIMessage>();
			var row = ((INeedRow)message).Row;
			var typeDecider = new EDIMessageTypeDecider();
			message.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Transmit;
			AssertEquals(typeof(NCTSOutboundEDIMessage), typeDecider.GetTypeForLoad(row, NewFactory()));
			message.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Receive;
			AssertEquals(typeof(NCTSInboundEDIMessage), typeDecider.GetTypeForLoad(row, NewFactory()));
		}
	}
}
