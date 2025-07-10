using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC628C;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	[TestedType(typeof(CC628CProcessor))]
	class CC628CProcessorTest : EntryHeaderMessageProcessorTest<CC628CProcessor, AESInboundEDIMessage, AESOutboundEDIMessage, CC628CProvider>
	{
		protected override void AssertProcessResultCore(CusEntryHeader entry, AESInboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_Status", LogicalStatusList.Codes.Accepted, entry.CH_Status);
			AssertEquals("CH_EntryStatus", AESEntryStatusList.Codes.ReleasedForExport, entry.CH_EntryStatus);
			AssertEquals("MovementReferenceNumber", "21IEDUB11A782454R2", entry.MovementReferenceNumber);
			AssertEquals("MovementReferenceNumberIssueDate", new ZDateTime(2022, 6, 9), entry.MovementReferenceNumberIssueDate);

			var jobNumber = entry.Declaration.JE_DeclarationReference;
			var messageInterpretation = $@"An Exit Summary Declaration Acknowledgement message has been received from Customs for Job {jobNumber} through the IE628 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Status</td><td>Released for Export (REL)</td></tr><tr><td>LRN</td><td>LRN123456789</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Acceptance Date</td><td>09-Jun-22</td></tr></table>";
			AssertMessageInterpretation(incomingMessage, messageInterpretation);
			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for " + jobNumber, new[] { messageInterpretation }, new string[] { "staff1@where.com" });
		}

		protected override ZString MessageFriendlyName => "CC628C: Exit Summary Declaration Acknowledgement";

		protected override CC628CProcessor Processor => new CC628CProcessor(logger, typeof(Cc628C));

		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.IE628;

		protected override ZString MessageText => AESInterchangeProcessorTestHelper.GetStandardCC628CText("LRN123456789", "21IEDUB11A782454R2");
	}
}
