using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(CDSInventoryLinkingMovementTotalsResponseEDIMessage))]
	class CDSInventoryLinkingMovementTotalsResponseEDIMessageTests : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var message = Factory.New<CDSInventoryLinkingMovementTotalsResponseEDIMessage>();
			AssertEquals(CDSEDIMessageTypeList.Codes.InventoryLinkingMovementTotalsResponse, message.EM_MessageType);
			AssertEquals(EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
		}

		public void TestMessageDataObjectAndInterpretation()
		{
			var message = Factory.New<CDSInventoryLinkingMovementTotalsResponseEDIMessage>();
			message.EM_MessageText = CDSInventoryLinkingMovementTotalsResponseEDIMessage.Serialize(InventoryLinkingMovementTotalsResponseTests.InventoryLinkingMovementTotalsResponseXMLForTest);
			var response = message.MessageDataObject;
			InventoryLinkingMovementTotalsResponseTests.AssertInventoryLinkingMovementTotalsResponseProperties(response);
			AssertEquals("Expected Message HTML Interpretation", InventoryLinkingMovementTotalsResponseTests.ExpectedHTMLInterpretation, message.EM_MessageInterpretation);
		}
	}
}
