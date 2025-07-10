using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	[TestedType(typeof(DeltaDImportFREDIMessage))]
	public class DeltaDImportFREDIMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValue()
		{
			var messageIMP = Factory.New<DeltaDImportFREDIMessage>();
			AssertEquals("Default message type", MessageTypeList.Codes.IMD, messageIMP.EM_MessageType);
			AssertEquals("Default message sub type", MessageSubTypeList.Codes.IMD, messageIMP.EM_MessageSubType);
			AssertEquals("Default message direction", EDIMessage.Direction.Receive, messageIMP.EM_ReceiveTransmit);
			AssertType<DeltaDImportResponseMessageDataObject>(messageIMP.MessageDataObject);
		}
		public void TestMessageDataObject()
		{
			var message = Factory.New<DeltaDImportFREDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			AssertNull(message.MessageDataObject);
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			AssertType<DeltaDImportResponseMessageDataObject>(message.MessageDataObject);
		}
	}
}
