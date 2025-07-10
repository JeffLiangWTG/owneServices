using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.H7.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;
using AsycudaManifestHeader = Enterprise.Customs.GB.H7.Business.AsycudaManifestHeader;

namespace Enterprise.Customs.GB.CDS.Testing
{
	sealed class CDSH7SynchronousResponseMessageProcessorTest : TestCaseWithFactory
	{
		[TestDate(2019, 11, 11, 12, 0, 0)]
		public void TestProcess()
		{
			var outgoingInterchange = Factory.New<CDSInterchange>();
			outgoingInterchange.EI_From = "CCSUK";
			outgoingInterchange.EI_To = "WISETECHGLOBAL";
			var outgoingMessage = (CDSEDIMessage)outgoingInterchange.ContainedMessages.AddNew(typeof(CDSEDIMessage));
			bill.Messages.Add(outgoingMessage);

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
<td><small>{bill.LocalReferenceNumber}</small></td>
</tr>
<tr style=""height: 20px;"">
<td>Job number:</td>
<td><small>B123</small></td>
</tr>
</table>";

			outgoingMessage = new BusinessObjectFactory().Load<CDSEDIMessage>(outgoingMessage.PK);
			AssertContains(expectedInterpretation, inboundMessage.EM_MessageInterpretation);
			AssertEquals("Message should be attached to bill", bill, inboundMessage.EM_LinkedObject);
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, inboundMessage.EM_Status);
			AssertEquals(EDIMessageStatusList.Codes.Acknowledged, outgoingMessage.EM_Status);
			AssertEquals("77685C3D14D53E25E0540003BA9676AB", outgoingMessage.EM_ApplicationReference);
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

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "B123";
			bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "8GB123456789000-S0001000";
		}

		AsycudaManifestHeader header;
		AsycudaBill bill;
	}
}
