using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	[TestedType(typeof(DeltaCExportFREDIMessage))]
	public class DeltaCExportFREDIMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValue()
		{
			var messageEXP = Factory.New<DeltaCExportFREDIMessage>();
			AssertEquals("Default message type", MessageTypeList.Codes.EXC, messageEXP.EM_MessageType);
			AssertEquals("Default message sub type", MessageSubTypeList.Codes.EXC, messageEXP.EM_MessageSubType);
			AssertEquals("Default message direction", EDIMessage.Direction.Receive, messageEXP.EM_ReceiveTransmit);
			AssertType<DeltaCExportResponseMessageDataObject>(messageEXP.MessageDataObject);
		}

		public void TestMessageDataObject()
		{
			var message = Factory.New<DeltaCExportFREDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			AssertNull(message.MessageDataObject);
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			AssertType<DeltaCExportResponseMessageDataObject>(message.MessageDataObject);
		}
	}
}
