using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC571C;
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
	[TestedType(typeof(CC571CProcessor))]
	class CC571CProcessorTest : EntryHeaderMessageProcessorTest<CC571CProcessor, AESInboundEDIMessage, AESOutboundEDIMessage, CC571CProvider>
	{
		protected override void AssertProcessResultCore(CusEntryHeader entry, AESInboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_EntryStatus should have been set ReleasedForExport.", AESEntryStatusList.Codes.ReleasedForExport, entry.CH_EntryStatus);
			AssertEquals("CH_Status should have been set Accepted.", LogicalStatusList.Codes.Accepted, entry.CH_Status);
			AssertEquals("MovementReferenceNumber", "21IEDUB11A782454R2", entry.MovementReferenceNumber);
			AssertEquals("MovementReferenceNumberIssueDate", new ZDateTime(1971, 9, 18), entry.MovementReferenceNumberIssueDate);

			var jobNumber = entry.Declaration.JE_DeclarationReference;
			var messageInterpretation = $@"A Re-Export Notification Registration message has been received from Customs for Job {jobNumber} through the IE571 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Status</td><td>REL- Released for Export</td></tr><tr><td>LRN</td><td>LRN123456789</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Re-Export Notification Registration Date</td><td>18-Sep-71</td></tr></table>";
			AssertMessageInterpretation(incomingMessage, messageInterpretation);
			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for " + jobNumber, new[] { messageInterpretation }, new string[] { "staff1@where.com" });
		}

		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.IE571;

		protected override ZString MessageText => AESInterchangeProcessorTestHelper.GetStandardCC571CText("LRN123456789", "21IEDUB11A782454R2");

		protected override ZString MessageFriendlyName => "CC571C: Re-Export Notification Registration Notification";

		protected override CC571CProcessor Processor => new CC571CProcessor(logger, typeof(Cc571C));
	}
}
