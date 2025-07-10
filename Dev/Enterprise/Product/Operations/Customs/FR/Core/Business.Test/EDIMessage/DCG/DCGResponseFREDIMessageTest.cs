using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	[TestedType(typeof(DCGResponseFREDIMessage))]
	public class DCGResponseFREDIMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValue()
		{
			var messageEXP = Factory.New<DCGResponseFREDIMessage>();
			AssertEquals("Default message type", MessageTypeList.Codes.DCG, messageEXP.EM_MessageType);
			AssertEquals("Default message sub type", MessageSubTypeList.Codes.DCG, messageEXP.EM_MessageSubType);
			AssertEquals("Default message direction", EDIMessage.Direction.Receive, messageEXP.EM_ReceiveTransmit);
			AssertType<DCGResponseMessageDataObject>(messageEXP.MessageDataObject);
		}

		public void TestMessageDataObject()
		{
			var message = Factory.New<DCGResponseFREDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			AssertNull(message.MessageDataObject);
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			AssertType<DCGResponseMessageDataObject>(message.MessageDataObject);
		}
	}
}
