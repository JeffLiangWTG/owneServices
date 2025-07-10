using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	[TestedType(typeof(NCTSInboundEDIMessage))]
	class NCTSInboundEDIMessageTest : EDIMessageTest
	{
		public void TestMessageDefaults()
		{
			var message = Factory.New<NCTSInboundEDIMessage>();
			AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
			AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.GbCustomsNCTS, message.EM_ApplicationCode);
		}

		public void TestCorrectlyTypeDeciding()
		{
			var message = Factory.New<NCTSInboundEDIMessage>();
			Factory.Save();
			AssertType<NCTSInboundEDIMessage>("Using base EDIMessage", NewFactory().Load<EDIMessage>(message.PK));
			AssertType<NCTSInboundEDIMessage>("Using GB EDIMessage", NewFactory().Load<GbEDIMessage>(message.PK));
		}
	}
}
