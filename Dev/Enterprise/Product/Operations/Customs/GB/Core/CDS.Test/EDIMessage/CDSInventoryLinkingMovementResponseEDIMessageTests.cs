using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(CDSInventoryLinkingMovementResponseEDIMessage))]
	class CDSInventoryLinkingMovementResponseEDIMessageTests : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var message = Factory.New<CDSInventoryLinkingMovementResponseEDIMessage>();
			AssertEquals(CDSEDIMessageTypeList.Codes.InventoryLinkingMovementResponse, message.EM_MessageType);
			AssertEquals(EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
		}

		public void TestMessageDataObjectAndInterpretation()
		{
			var message = Factory.New<CDSInventoryLinkingMovementResponseEDIMessage>();
			message.EM_MessageText = CDSInventoryLinkingMovementResponseEDIMessage.Serialize(InventoryLinkingMovementResponseTests.InventoryLinkingMovementResponseXMLForTest);
			var response = message.MessageDataObject;
			InventoryLinkingMovementResponseTests.AssertInventoryLinkingMovementResponseProperties(response);
			AssertEquals("Expected Message HTML Interpretation", InventoryLinkingMovementResponseTests.ExpectedHTMLInterpretation, message.EM_MessageInterpretation);
		}
	}
}
