using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(CDSDeclarationInfoResponseEDIMessage))]
	class CDSDeclarationInfoResponseEDIMessageTests : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var message = Factory.New<CDSDeclarationInfoResponseEDIMessage>();
			AssertEquals(CDSEDIMessageTypeList.Codes.QueryResponse, message.EM_MessageType);
			AssertEquals(EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
		}

		public void TestMessageDataObjectAndInterpretation()
		{
			var message = Factory.New<CDSDeclarationInfoResponseEDIMessage>();
			message.EM_MessageText = CDSDeclarationInfoResponseEDIMessage.Serialize(CDSDeclarationInfoResponseTests.CDSDeclarationInfoResponseXMLForTest);
			var response = message.MessageDataObject as DeclarationStatusResponse;
			CDSDeclarationInfoResponseTests.AssertDeclarationInfoQueryResponseProperties(response);
			AssertEquals("Expected Message HTML Interpretation", CDSDeclarationInfoResponseTests.ExpectedHTMLInterpretation, message.EM_MessageInterpretation);
		}

		public void TestEhubTrackingId()
		{
			var message = Factory.New<CDSDeclarationInfoResponseEDIMessageForTesting>();

			var testGuid = ZGuid.NewZGuid();
			var testGuidStr = new ZString($"{testGuid}").KeepAlphanumericCharacters();

			message.EM_ApplicationReference = "";
			AssertEquals(ZGuid.Empty, message.EHubTrackingIdExposed);

			message.EM_ApplicationReference = "abcdefghijklmnop";
			AssertEquals(ZGuid.Empty, message.EHubTrackingIdExposed);

			message.EM_ApplicationReference = testGuidStr;
			AssertEquals(testGuid, message.EHubTrackingIdExposed);
		}
	}

	class CDSDeclarationInfoResponseEDIMessageForTesting : CDSDeclarationInfoResponseEDIMessage
	{
		public CDSDeclarationInfoResponseEDIMessageForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZGuid EHubTrackingIdExposed => base.EHubTrackingId;
	}
}
