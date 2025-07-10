using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	[TestedType(typeof(CINImportResponseFREDIMessage))]
	public class CINImportResponseFREDIMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValue()
		{
			var messageCIN = Factory.New<CINImportResponseFREDIMessage>();
			AssertEquals("Default message type", MessageTypeList.Codes.CIN, messageCIN.EM_MessageType);
			AssertEquals("Default message sub type", MessageSubTypeList.Codes.CIN, messageCIN.EM_MessageSubType);
			AssertEquals("Default message direction", EDIMessage.Direction.Receive, messageCIN.EM_ReceiveTransmit);
			AssertType<CINImportResponseMessageDataObject>(messageCIN.MessageDataObject);
		}

		public void TestMessageDataObject()
		{
			var message = Factory.New<CINImportResponseFREDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			AssertNull(message.MessageDataObject);
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			AssertType<CINImportResponseMessageDataObject>(message.MessageDataObject);
		}
	}
}
