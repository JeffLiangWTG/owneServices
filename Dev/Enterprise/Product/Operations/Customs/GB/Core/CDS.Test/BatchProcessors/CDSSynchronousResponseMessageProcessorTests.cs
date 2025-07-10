using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	sealed class CDSSynchronousResponseMessageProcessorTests : TestCaseWithFactory
	{
		[TestDate(2019, 11, 11, 12, 0, 0)]
		public void TestProcess()
		{
			var (outgoingInterchange, outgoingMessage) = AddInterchangeAndOutgoingMessage();
			var inboundMessage = AddInterchangeAndIncomingMessage(outgoingInterchange, "77685C3D14D53E25E0540003BA9676AB", $@"<SynchronousResponse>
               <status>202</status>
               <code>ACCEPTED</code>
               <ResponseHeaders>
                              <x-conversation-id>77685C3D14D53E25E0540003BA9676AB</x-conversation-id>
               </ResponseHeaders>
</SynchronousResponse>");
			Factory.Save();

			var processor = new CDSSynchronousResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(inboundMessage);
			Factory.Save();

			var expectedInterpretation = @$"<style>
body, p, td {{font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}}
tr {{
  height: 50px;
  vertical-align: center;
}}
.Status {{
padding-left: 40px;
padding-right: 10px;
}}
.StepProgress-item {{
  margin-top: 15px;
}}
.StepProgress-item{{
  content: '';
  width: 12px;
  height: 12px;
}}
.StepProgress-item.is-done {{
  font-size: 16px;
  color: green;
  text-align: center;
  font-weight: bold;
}}
.StepProgress-item.current{{
  font-size: 12px;
  text-align: center;
  color: grey;
}}
.StepProgress-item.rejected {{
  font-size: 16px;
  color: red;
  text-align: center;
  font-weight: bold;
}}
</style><p><h4>Message 1 was uploaded to CDS and received Conversation ID 77685C3D14D53E25E0540003BA9676AB</h4></p><p><h5></h5</p><table>
<tr>
	<td class=""StepProgress-item is-done"">&#10004;</td>
	<td class=""Status"">Message created</td>
	<td><small>Message number = 1 at 11/11/2019 12:00 (UTC)</small></td>
</tr>
<tr>
	<td class=""StepProgress-item is-done"">&#10004;</td>
	<td class=""Status"">Sent to CCSUK</td>
	<td><small>CCSUK Tracking ID = {outgoingInterchange.PK}</small></td>
</tr>
<tr>
	<td class=""StepProgress-item is-done"">&#10004;</td>
	<td class=""Status"">Sent to CDS</td>
	<td><small>HMRC Conversation ID = 77685C3D14D53E25E0540003BA9676AB</small></td>
</tr>
</table><p/><table>
<tr style=""height: 20px;"">
<td>Status update time:      </td>
<td><small>{inboundMessage.EM_SystemCreateTimeUtc:dd/MM/yyyy HH:mm} (UTC)</small></td>
</tr>
<tr style=""height: 20px;"">
<td>DUCR (2/4):</td>
<td><small>8GB123456789000-S0001000</small></td>
</tr>
<tr style=""height: 20px;"">
<td>LRN (2/5):</td>
<td><small>{entry.LRN}</small></td>
</tr>
<tr style=""height: 20px;"">
<td>Job number:</td>
<td><small>B123</small></td>
</tr>
</table>";

			outgoingMessage = new BusinessObjectFactory().Load<CDSEDIMessage>(outgoingMessage.PK);
			AssertContains(expectedInterpretation, inboundMessage.EM_MessageInterpretation);
			AssertEquals(entry, inboundMessage.LinkedEntry);
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, inboundMessage.EM_Status);
			AssertEquals(EDIMessageStatusList.Codes.Acknowledged, outgoingMessage.EM_Status);
			AssertEquals("77685C3D14D53E25E0540003BA9676AB", outgoingMessage.EM_ApplicationReference);
		}

		public void TestProcessNot202()
		{
			ProcessNot202Test("77685C3D14D53E25E0540003BA9676AB");
			ProcessNot202Test("00000000000000000000000000000000");
		}

		void ProcessNot202Test(ZString conversationID)
		{
			var (outgoingInterchange, outgoingMessage) = AddInterchangeAndOutgoingMessage();
			var inboundMessage = AddInterchangeAndIncomingMessage(outgoingInterchange, conversationID, $@"<SynchronousResponse CCSUK_Notification=""true"">
	<status>401</status>
	<code>UNAUTHORIZED</code>
	<description>SENDER NOT CONFIGURED TO SEND THIS MESSAGE TYPE-VERSION-RELEASE.</description>
</SynchronousResponse>");
			Factory.Save();

			var processor = new CDSSynchronousResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(inboundMessage);
			Factory.Save();

			var expectedInterpretation = conversationID == "00000000000000000000000000000000"
											? $@"Message {outgoingMessage.EM_MessageNum} was '401' - 'UNAUTHORIZED' - 'SENDER NOT CONFIGURED TO SEND THIS MESSAGE TYPE-VERSION-RELEASE.'"
											: $@"Message {outgoingMessage.EM_MessageNum} was '401' - 'UNAUTHORIZED' - 'SENDER NOT CONFIGURED TO SEND THIS MESSAGE TYPE-VERSION-RELEASE.' and received Conversation ID '{conversationID}'";

			outgoingMessage = new BusinessObjectFactory().Load<CDSEDIMessage>(outgoingMessage.PK);
			AssertContains(expectedInterpretation, inboundMessage.EM_MessageInterpretation);
			AssertEquals(entry, inboundMessage.LinkedEntry);
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, inboundMessage.EM_Status);
			AssertEquals(EDIMessageStatusList.Codes.Rejected, outgoingMessage.EM_Status);
		}

		public void TestBadRequestResponse()
		{
			var (outgoingInterchange, outgoingMessage) = AddInterchangeAndOutgoingMessage();
			var message = AddInterchangeAndIncomingMessage(outgoingInterchange, "ba160297-eddc-46be-bf3b-fab45a430fc9",
				"<SynchronousResponse xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"SynchronousResponse.xsd\">\r\n"
				+ "  <status>400</status>\r\n"
				+ "  <code>Bad Request</code>\r\n"
				+ "  <ResponseHeaders>\r\n"
				+ "    <x-conversation-id>ba160297-eddc-46be-bf3b-fab45a430fc9</x-conversation-id>\r\n"
				+ "  </ResponseHeaders>\r\n"
				+ "</SynchronousResponse>");
			Factory.Save();

			var processor = new CDSSynchronousResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);

			var expectedInterpretation = $"Message {outgoingMessage.EM_MessageNum} was '400' - 'Bad Request' and received Conversation ID 'ba160297-eddc-46be-bf3b-fab45a430fc9'<br /><small>This is likely "
				+ "caused by a rejection by CDS due to an XML schema failure, i.e. bad data. CCS-UK do not send back details of such failures, they only report that there was a failure, therefore details of "
				+ "the failures cannot be shown here. Retrying the same request via another CSP or directly to CDS (for imports, don't forget to temporarily remove the inventory consignment reference first) "
				+ "is likely to reveal much more detail about the nature of the failure, allowing you to resolve it (and restore the original CSP and inventory reference). Common data-entry errors include: "
				+ "missing unit code for supporting document quantities, missing currency code, missing measurement code for taxes, and new line or other non-standard characters in text fields. Examine the "
				+ "original outgoing XML text to find such cases and remedy it.</small>";
			AssertEquals(expectedInterpretation, message.EM_MessageInterpretation);
			AssertEquals("Incoming message Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			AssertEquals("Outgoing message Status", EDIMessageStatusList.Codes.Rejected, outgoingMessage.EM_Status);
		}

		public void TestBadRequestResponseWithErrors()
		{
			var (outgoingInterchange, outgoingMessage) = AddInterchangeAndOutgoingMessage();
			var message = AddInterchangeAndIncomingMessage(outgoingInterchange, "ba160297-eddc-46be-bf3b-fab45a430fc9",
				"<SynchronousResponse xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"SynchronousResponse.xsd\">\r\n"
				+ "  <status>400</status>\r\n"
				+ "  <code>Bad Request</code>\r\n"
				+ "  <errors>\r\n"
				+ "    <error>\r\n"
				+ "      <code>some_error_code</code>\r\n"
				+ "      <message>:A useful error message that is actually helpful.</message>\r\n"
				+ "    </error>\r\n"
				+ "  </errors>\r\n"
				+ "  <ResponseHeaders>\r\n"
				+ "    <x-conversation-id>ba160297-eddc-46be-bf3b-fab45a430fc9</x-conversation-id>\r\n"
				+ "  </ResponseHeaders>\r\n"
				+ "</SynchronousResponse>");
			Factory.Save();

			var processor = new CDSSynchronousResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);

			var expectedInterpretation = $"Message {outgoingMessage.EM_MessageNum} was '400' - 'Bad Request' and received Conversation ID 'ba160297-eddc-46be-bf3b-fab45a430fc9'" +
				"<h3>The following errors were returned</h3>" +
				"<li>A useful error message that is actually helpful.</li>";

			AssertEquals(expectedInterpretation, message.EM_MessageInterpretation);
			AssertEquals("Incoming message Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			AssertEquals("Outgoing message Status", EDIMessageStatusList.Codes.Rejected, outgoingMessage.EM_Status);
		}

		protected override void SetUp()
		{
			base.SetUp();
			dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "B123";
			dec.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "8GB123456789000-S0001000";
		}

		(CDSInterchange, CDSEDIMessage) AddInterchangeAndOutgoingMessage()
		{
			var outgoingInterchange = Factory.New<CDSInterchange>();
			outgoingInterchange.EI_From = "CCSUK";
			outgoingInterchange.EI_To = "WISETECHGLOBAL";
			var outgoingMessage = (CDSEDIMessage)outgoingInterchange.ContainedMessages.AddNew(typeof(CDSEDIMessage));
			entry.Messages.Add(outgoingMessage);
			return (outgoingInterchange, outgoingMessage);
		}

		CDSEDIMessage AddInterchangeAndIncomingMessage(CDSInterchange outgoingInterchange, string conversationID, string messageBody)
		{
			var inboundInterchange = Factory.New<CDSInterchange>();
			inboundInterchange.EI_From = "CCSUK";
			inboundInterchange.EI_To = "WISETECHGLOBAL";
			inboundInterchange.EI_BodyText =
$@"<GBCustomsBusinessResponse>
	<ResponseHeader>
		<ConversationID>{conversationID}</ConversationID>
	</ResponseHeader>
	<ResponseBody>
		{messageBody}
	</ResponseBody>
</GBCustomsBusinessResponse>";
			inboundInterchange.EI_FooterText = $@"<?ccsuk senderid=""CCSUK"" recipientid=""WISETECHGLOBAL"" ext-correlation-id=""{outgoingInterchange.PK}"" x-conversation-id=""{conversationID}"" ?>";
			var inboundMessage = (CDSSynchronousResponseEDIMessage)inboundInterchange.ContainedMessages.AddNew(typeof(CDSSynchronousResponseEDIMessage));
			inboundMessage.EM_ApplicationReference = conversationID;
			inboundMessage.EM_MessageText = $@"{messageBody}
<?ccsuk senderid=""CCSUK"" recipientid=""WISETECHGLOBAL"" ext-correlation-id=""{outgoingInterchange.PK}"" x-conversation-id=""{conversationID}"" ?>";

			return inboundMessage;
		}

		JobDeclaration dec;
		CusEntryHeader entry;
	}
}
