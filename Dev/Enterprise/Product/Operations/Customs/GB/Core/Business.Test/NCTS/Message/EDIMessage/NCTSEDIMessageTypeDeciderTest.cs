using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	class NCTSEDIMessageTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var typeDecider = new NCTSEDIMessageTypeDecider();
			var message = Factory.New<GbEDIMessage>();
			var row = ((INeedRow)message).Row;

			CombineAssertions(() =>
			{
				message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				AssertEquals("Transmit", typeof(NCTSOutboundEDIMessage), typeDecider.GetTypeForLoad(row, Factory));

				message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				AssertEquals("Receive", typeof(NCTSInboundEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
			});
		}
	}
}
