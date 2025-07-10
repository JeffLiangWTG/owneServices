using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(AISInboundEDIMessage))]
	class AISInboundEDIMessageTest : Enterprise.Messaging.Testing.EDIMessageTest
	{
		public void TestSetDefaultValues()
		{
			var message = Factory.New<AISInboundEDIMessage>();
			CombineAssertions(() =>
			{
				AssertEquals("EM_ApplicationCode", Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.IECustomsImport, message.EM_ApplicationCode);
				AssertEquals("EM_ReceiveTransmit", Enterprise.Messaging.Business.EDIInterchange.Direction.Receive, message.EM_ReceiveTransmit);
			});
		}
	}
}
