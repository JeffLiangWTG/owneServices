using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC551C;
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
	[TestedType(typeof(CC551CProcessor))]
	class CC551CProcessorTest : EntryHeaderMessageProcessorTest<CC551CProcessor, AESInboundEDIMessage, AESOutboundEDIMessage, CC551CProvider>
	{
		protected override void AssertProcessResultCore(CusEntryHeader entry, AESInboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_Status", LogicalStatusList.Codes.Accepted, entry.CH_Status);
			AssertEquals("CH_EntryStatus", "REJ", entry.CH_EntryStatus);

			var jobNumber = entry.Declaration.JE_DeclarationReference;
			var messageInterpretation = $@"An Export No Release message has been received from Customs for Job {jobNumber} through the IE551 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Status</td><td>Export Release Rejected (REJ)</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Issues Reported</td><td>Test Report</td></tr><tr><td>Result Date</td><td>18-Sep-71</td></tr><tr><td>Result Text</td><td>Text of a control result</td></tr></table>";
			AssertMessageInterpretation(incomingMessage, messageInterpretation);
			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for " + jobNumber, new[] { messageInterpretation }, new string[] { "staff1@where.com" });
		}

		protected override ZString MessageFriendlyName => "CC551C: EXPORT NO RELEASE";

		protected override CC551CProcessor Processor => new CC551CProcessor(logger, typeof(Cc551C));

		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.IE551;

		protected override ZString MessageText => AESInterchangeProcessorTestHelper.GetStandardCC551CText("21IEDUB11A782454R2");
	}
}
