using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	[TestedType(typeof(DeltaCImportFREDIMessage))]
	public class DeltaCImportFREDIMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValue()
		{
			var messageIMP = Factory.New<DeltaCImportFREDIMessage>();
			AssertEquals("Default message type", MessageTypeList.Codes.IMC, messageIMP.EM_MessageType);
			AssertEquals("Default message sub type", MessageTypeList.Codes.IMC, messageIMP.EM_MessageSubType);
			AssertEquals("Default message direction", EDIMessage.Direction.Receive, messageIMP.EM_ReceiveTransmit);
			AssertType<DeltaCImportResponseMessageDataObject>(messageIMP.MessageDataObject);
		}

		public void TestMessageDataObject()
		{
			var message = Factory.New<DeltaCImportFREDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			AssertNull(message.MessageDataObject);
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			AssertType<DeltaCImportResponseMessageDataObject>(message.MessageDataObject);
		}
	}
}
