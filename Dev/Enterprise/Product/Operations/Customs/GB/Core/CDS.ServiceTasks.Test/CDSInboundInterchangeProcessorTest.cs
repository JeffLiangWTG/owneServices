using System.Xml;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.CDS.ServiceTasks.Testing
{
	class CDSInboundInterchangeProcessorTest : TestCaseWithFactory
	{
		public void TestProcessInterchangeForDocumentUploadConfirmation()
		{
			var inboundInterchange = Factory.New<CDSInterchange>();
			inboundInterchange.EI_ReceiveTransmit = EDIInterchange.Status.Received;
			inboundInterchange.EI_Status = EDIInterchange.Status.Queued;
			inboundInterchange.EI_From = "eHub";
			inboundInterchange.EI_To = "WTG";
			inboundInterchange.EI_BodyText = @"<s0:GBCustomsBusinessResponse xmlns:s0=""http://cargowise.com/ehub/products/GBCustoms"">
  <s0:ResponseHeader>
    <s0:ConversationID>622bc0f8-ef14-48dd-b1ca-0dbdeea96c24</s0:ConversationID>
    <s0:eHubTrackingId>f127bdc2-b97b-4305-aaba-8026dc82949a</s0:eHubTrackingId>
  </s0:ResponseHeader>
  <s0:ResponseBody>
    <Root xmlns=""hmrc:fileupload"">
      <FileReference>622bc0f8-ef14-48dd-b1ca-0dbdeea96c24</FileReference>
      <BatchId>91dd2f2f-56cb-425b-a6fe-abd3ce8e7adc</BatchId>
      <FileName>CDS Entry Document - 2GB427168118378-B60004374.pdf</FileName>
      <Outcome>SUCCESS</Outcome>
      <Details>Thank you for submitting your documents. Typical clearance times are 2 hours for air and 3 hours for maritime declarations. During busy periods wait times may be longer.</Details>
    </Root>
  </s0:ResponseBody>
</s0:GBCustomsBusinessResponse>";

			var processor = new CDSInboundInterchangeProcessorForTest(new LoggingInformation());
			processor.ProcessInterchangeForTest(inboundInterchange);

			CombineAssertions("New generated EDI message", () =>
			{
				AssertEquals("Has new EDI message", 1, inboundInterchange.ContainedMessages.Count);
				var expectedDocumentUploadConfirmationResponse = new XmlDocument();
				expectedDocumentUploadConfirmationResponse.LoadXml(@"<Root xmlns=""hmrc:fileupload"">
  <FileReference>622bc0f8-ef14-48dd-b1ca-0dbdeea96c24</FileReference>
  <BatchId>91dd2f2f-56cb-425b-a6fe-abd3ce8e7adc</BatchId>
  <FileName>CDS Entry Document - 2GB427168118378-B60004374.pdf</FileName>
  <Outcome>SUCCESS</Outcome>
  <Details>Thank you for submitting your documents. Typical clearance times are 2 hours for air and 3 hours for maritime declarations. During busy periods wait times may be longer.</Details>
</Root>");
				var newMessage = inboundInterchange.ContainedMessages[0];
				AssertType<CDSDocumentUploadConfirmationResponse>(newMessage);
				AssertEquals("XML response", expectedDocumentUploadConfirmationResponse.OuterXml, newMessage.EM_MessageText);
				AssertEquals("Should be eHubId not the Conversation ID", GetApplicationReference("f127bdc2-b97b-4305-aaba-8026dc82949a"), newMessage.EM_ApplicationReference);
			});
		}

		ZString GetApplicationReference(string referenceNumber) => new ZString(referenceNumber).KeepAlphanumericCharacters();

		class CDSInboundInterchangeProcessorForTest : CDSInboundInterchangeProcessor
		{
			public CDSInboundInterchangeProcessorForTest(LoggingInformation logger) : base(logger)
			{
			}

			public bool ProcessInterchangeForTest(EDIInterchange interchange) => ProcessInterchange(interchange);
		}
	}
}
