using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(CDSInventoryLinkingQueryResponseEDIMessage))]
	class CDSInventoryLinkingQueryResponseEDIMessageTests : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var message = Factory.New<CDSInventoryLinkingQueryResponseEDIMessage>();
			AssertEquals(CDSEDIMessageTypeList.Codes.InventoryLinkingQueryResponse, message.EM_MessageType);
			AssertEquals(EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
		}

		public void TestMessageDataObjectAndInterpretation()
		{
			var message = Factory.New<CDSInventoryLinkingQueryResponseEDIMessage>();
			message.EM_MessageText = CDSInventoryLinkingQueryResponseEDIMessage.Serialize(InventoryLinkingQueryResponseTests.InventoryLinkingQueryResponseXMLForTest);
			var response = message.MessageDataObject;
			InventoryLinkingQueryResponseTests.AssertInventoryLinkingQueryResponseProperties(response);
			AssertEquals("Expected Message HTML Interpretation", InventoryLinkingQueryResponseTests.ExpectedHTMLInterpretation, message.EM_MessageInterpretation);
		}
	}
}
