using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(AVSQueryMessage))]
	sealed class AVSQueryMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var message = Factory.New<AVSQueryMessage>();
			AssertEquals("EM_ApplicationCode", ApplicationCodeList.Codes.CFIAQuery, message.EM_ApplicationCode);
			AssertEquals("EM_MessageType", MessageTypeList.Codes.AVSQuery, message.EM_MessageType);
			AssertEquals("EM_MessageSubType", MessageTypeList.Codes.AVSQuery, message.EM_MessageSubType);
			AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
		}

		public void TestMessageReferenceNumber()
		{
			var message1 = Factory.New<AVSQueryMessage>();
			var message2 = Factory.New<AVSQueryMessage>();
			Factory.Save();

			AssertEquals("EM_MessageNum", "1", message1.EM_MessageNum);
			AssertEquals("EM_MessageNum", "1", message2.EM_MessageNum);
		}
	}
}
