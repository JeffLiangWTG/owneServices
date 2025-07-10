using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC574C;
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
	[TestedType(typeof(CC574CProcessor))]
	class CC574CProcessorTest : EntryHeaderMessageProcessorTest<CC574CProcessor, AESInboundEDIMessage, AESOutboundEDIMessage, CC574CProvider>
	{
		protected override void AssertProcessResultCore(CusEntryHeader entry, AESInboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_Status", LogicalStatusList.Codes.Accepted, entry.CH_Status);
			AssertEquals("CH_EntryStatus", AESEntryStatusList.Codes.ReleasedForExport, entry.CH_EntryStatus);

			var jobNumber = entry.Declaration.JE_DeclarationReference;
			var messageInterpretation = $@"An Amendment Acceptance message has been received from Customs for Job {jobNumber} through the IE574 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Status</td><td>REL- Released for Export</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Amendment Submission Date and Time</td><td>18-Sep-71 00:00</td></tr><tr><td>Amendment Acceptance Date and Time</td><td>18-Sep-71 00:00</td></tr></table>";
			AssertMessageInterpretation(incomingMessage, messageInterpretation);
			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for " + jobNumber, new[] { messageInterpretation }, new string[] { "staff1@where.com" });
		}

		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.IE574;

		protected override ZString MessageText => AESInterchangeProcessorTestHelper.GetStandardCC574CText("21IEDUB11A782454R2");

		protected override ZString MessageFriendlyName => "CC574C: RE-EXPORT DECLARATION AMENDMENT ACCEPTANCE";

		protected override CC574CProcessor Processor => new CC574CProcessor(logger, typeof(Cc574C));
	}
}
