using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IE.PBN.Business.Testing
{
	sealed class EDIMessageTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForBinding()
		{
			AssertNull(TypeDecider.GetTypeForBinding());
		}

		public void TestGetTypeForLoad()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsPBN;
			var row = ((INeedRow)message).Row;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			AssertEquals(typeof(PBNOutboundEDIMessage), TypeDecider.GetTypeForLoad(row, NewFactory()));

			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			AssertEquals(typeof(PBNInboundEDIMessage), TypeDecider.GetTypeForLoad(row, NewFactory()));
		}

		public void TestGetTypeForNew()
		{
			AssertNull(TypeDecider.GetTypeForNew());
		}

		EDIMessageTypeDecider TypeDecider => typeDecider ??= new EDIMessageTypeDecider();
		EDIMessageTypeDecider typeDecider;
	}
}
