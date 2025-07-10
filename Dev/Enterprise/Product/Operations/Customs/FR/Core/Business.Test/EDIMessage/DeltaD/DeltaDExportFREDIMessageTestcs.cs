using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	[TestedType(typeof(DeltaDExportFREDIMessage))]
	public class DeltaDExportFREDIMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValue()
		{
			var messageEXP = Factory.New<DeltaDExportFREDIMessage>();
			AssertEquals("Default message type", MessageTypeList.Codes.EXD, messageEXP.EM_MessageType);
			AssertEquals("Default message sub type", MessageSubTypeList.Codes.EXD, messageEXP.EM_MessageSubType);
			AssertEquals("Default message direction", EDIMessage.Direction.Receive, messageEXP.EM_ReceiveTransmit);
			AssertType<DeltaDExportResponseMessageDataObject>(messageEXP.MessageDataObject);
		}

		public void TestMessageDataObject()
		{
			var message = Factory.New<DeltaDExportFREDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			AssertNull(message.MessageDataObject);
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			AssertType<DeltaDExportResponseMessageDataObject>(message.MessageDataObject);
		}
	}
}
