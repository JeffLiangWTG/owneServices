using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(AISUCC5InboundEDIMessage))]
	class AISUCC5InboundEDIMessageTest : Enterprise.Messaging.Testing.EDIMessageTest
	{
		public void TestSetDefaultValues()
		{
			var message = Factory.New<AISUCC5InboundEDIMessage>();
			CombineAssertions(() =>
			{
				AssertEquals("EM_ApplicationCode", Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.IECustomsUCC5Import, message.EM_ApplicationCode);
				AssertEquals("EM_ReceiveTransmit", Enterprise.Messaging.Business.EDIInterchange.Direction.Receive, message.EM_ReceiveTransmit);
			});
		}
	}
}
