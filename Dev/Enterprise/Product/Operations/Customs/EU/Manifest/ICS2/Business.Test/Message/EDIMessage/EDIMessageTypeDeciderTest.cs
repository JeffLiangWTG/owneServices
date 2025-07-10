using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	public class EDIMessageTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForBinding()
		{
			AssertNull(typeDecider.GetTypeForBinding());
		}

		public void TestGetTypeForNew()
		{
			AssertNull(typeDecider.GetTypeForNew());
		}

		public void TestGetTypeForLoad()
		{
			var message = Factory.New<EDIMessage>();
			var row = ((INeedRow)message).Row;
			var typeDecider = new EDIMessageTypeDecider();
			message.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Transmit;
			AssertEquals(typeof(ICS2OutboundEDIMessage), typeDecider.GetTypeForLoad(row, NewFactory()));
			message.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Receive;
			AssertEquals(typeof(ICS2InboundEDIMessage), typeDecider.GetTypeForLoad(row, NewFactory()));
		}

		protected override void SetUp()
		{
			base.SetUp();
			typeDecider = new EDIMessageTypeDecider();
		}

		EDIMessageTypeDecider typeDecider;
	}
}
