using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	[TestedType(typeof(EMCSInboundEDIMessage))]
	public class EMCSInboundEDIMessageTest : EDIMessageTest
	{
		public void TestMessageDefaults()
		{
			AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
			AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.IECustomsEMCS, message.EM_ApplicationCode);
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = Factory.New<EMCSInboundEDIMessage>();
		}

		EMCSInboundEDIMessage message;
	}
}
