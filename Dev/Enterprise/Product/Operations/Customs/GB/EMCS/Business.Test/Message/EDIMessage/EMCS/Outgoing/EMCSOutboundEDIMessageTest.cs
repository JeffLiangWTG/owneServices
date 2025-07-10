using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestedType(typeof(EMCSOutboundEDIMessage))]
	sealed class EMCSOutboundEDIMessageTest : EDIMessageTest
	{
		public void TestMessageDefaults()
		{
			var message = Factory.New<EMCSOutboundEDIMessage>();
			AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
			AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.GbCustomsEMCS, message.EM_ApplicationCode);
		}

		public void TestGetMessageReferenceNumber()
		{
			var message1 = Factory.New<EMCSOutboundEDIMessage>();
			Factory.Save();
			AssertEquals("Should have created MessageNumber(001) from IMessageNumberStrategy sequence", "1", message1.EM_MessageNum);

			var message2 = Factory.New<EMCSOutboundEDIMessage>();
			Factory.Save();
			AssertEquals("Should have created MessageNumber(002) from IMessageNumberStrategy sequence", "2", message2.EM_MessageNum);
		}
	}
}
