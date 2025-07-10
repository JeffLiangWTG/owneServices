using System.IO;
using System.Linq;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EvvInboundMessageCreator))]
sealed class EvvInboundMessageCreatorTest : InboundMessageCreatorAbstractTest
{
	protected override IInboundMessageCreator GetMessageCreator() => new EvvInboundMessageCreator(Logger);

	protected override string ApplicationCode => EDIInterchange.ApplicationCodes.CHCustomsEdec;

	protected override string InterchangeType => MessageTypeCodeList.Codes.EVV;

	protected override string GetExpectedMessageText(string parsingResult) => parsingResult;

	protected override (string From, string BodyText)[] FailingDeserializations => System.Array.Empty<(string From, string BodyText)>();

	public void TestGenerateMessagesFromInterchangeDuties() => TestGenerateMessagesFromInterchange(TestingData.ReadManifestResourceContent("Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.eVVResponseDuties.txt"), MessageSubTypeCodeList.Codes.TaxationDecisionCustomsDuties);

	public void TestGenerateMessagesFromInterchangeVAT() => TestGenerateMessagesFromInterchange(TestingData.ReadManifestResourceContent("Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.eVVResponseVAT.txt"), MessageSubTypeCodeList.Codes.TaxationDecisionVat);

	public void TestGenerateMessagesFromInterchangeXMLSchemaErrors() => TestGenerateMessagesFromInterchange(TestingData.ReadManifestResourceContent("Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.eVVResponseXMLSchemaErrors.txt"), MessageSubTypeCodeList.Codes.XmlSchemaError);

	public void TestGenerateMessagesFromInterchangeRuleErrors() => TestGenerateMessagesFromInterchange(TestingData.ReadManifestResourceContent("Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.eVVResponseRuleErrors.txt"), MessageSubTypeCodeList.Codes.RuleError);

	public void TestGenerateMessageForInterchangeRefundVAT() => TestGenerateMessagesFromInterchange(TestingData.ReadManifestResourceContent("Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.eVVResponseRefundVAT.txt"), MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementVat);

	public void TestGenerateMessageForInterchangeRefundCustomsDuties() => TestGenerateMessagesFromInterchange(TestingData.ReadManifestResourceContent("Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.eVVResponseRefundCustomsDuties.txt"), MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementCustomsDuties);

	public void TestResponseMissingHeader()
	{
		EDIInterchange interchange = Factory.New<EDIInterchange>();

		interchange.EI_BodyText = "Test String";

		MessageCreator.CreateMessagesForInterchange(interchange);

		CombineAssertions(() =>
		{
			AssertEquals("Interchange should contain 1 EDI Message", 1, interchange.ContainedMessages.Count);
			AssertEquals($"EDI Message Status should be: {EDIMessageStatusList.Codes.Discarded}", EDIMessageStatusList.Codes.Discarded, interchange.ContainedMessages.Cast<EDIMessage>().FirstOrDefault().EM_Status);
		});
	}

	public void TestParseEVVResponse()
	{
		using (TextReader reader = new StringReader(TestingData.InputEvvResponse))
		{
			var splittedResponses = MIMETypeXTParser.ParseTextHttp(reader);
			AssertEquals(1, splittedResponses.Count());
			CombineAssertions("Xml", () =>
			{
				var parsingResult = splittedResponses.ElementAt(0);
				AssertResponseParsingResult(parsingResult, "text/xml", string.Empty, true, false);
				AssertEquals("Body Length", 14390, parsingResult.BodyText.Length);
			});
		}
	}

	protected override string UnsupportedSchemaVersion => @"HTTP/1.1 200 OK
Content-Type: text/xml; charset=UTF-8
Transfer-Encoding: chunked

<receiptRequestResponse schemaVersion=""1.0"" xsi:schemaLocation=""http://www.e-dec.ch/xml/schema/edecReceiptResponse/v3 http://www.ezv.admin.ch/pdf_linker.php?doc=edecReceiptResponse_v_3_0"" xmlns=""http://www.e-dec.ch/xml/schema/edecReceiptResponse/v3"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
   <requestorTraderIdentificationNumber>CHE326684996</requestorTraderIdentificationNumber>
   <receiptRequestRejection>
      <rejectionDate>2022-12-29</rejectionDate>
      <rejectionTime>09:34:45</rejectionTime>
      <errors>
         <ruleErrors>
            <error>
               <ruleName>v1</ruleName>
               <checkType>Receipt Check</checkType>
               <reference>goodsDeclaration</reference>
            </error>
         </ruleErrors>
      </errors>
   </receiptRequestRejection>
</receiptRequestResponse>
";
}
