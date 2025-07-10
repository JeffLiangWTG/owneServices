using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC604C;
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
	[TestedType(typeof(CC604CProcessor))]
	class CC604CProcessorTest : EntryHeaderMessageProcessorTest<CC604CProcessor, AESInboundEDIMessage, AESOutboundEDIMessage, CC604CProvider>
	{
		protected override void AssertProcessResultCore(CusEntryHeader entry, AESInboundEDIMessage incomingMessage)
		{
			AssertEquals("Entry Status", AESEntryStatusList.Codes.ReleasedForExport, entry.CH_EntryStatus);
			AssertEquals("Logical Status", LogicalStatusList.Codes.Accepted, entry.CH_Status);

			AssertMessageInterpretation(incomingMessage, @"An Exit Summary Declaration Amendment Acceptance message has been received from Customs for Job B00001000 through the IE604 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Status</td><td>Amendment Accepted by Customs (ACC)</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Amendment Submission Date and Time</td><td>18-Sep-71 00:00</td></tr><tr><td>Amendment Acceptance Date and Time</td><td>19-Sep-71 00:00</td></tr></table>");
			MessageProcessorNotificationTestHelper.AssertEmail(
				incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "An Exit Summary Declaration Amendment Acceptance message has been received from Customs for Job B00001000 through the IE604 message." },
				new string[] { "staff1@where.com" }
				);
		}

		protected override ZString MessageFriendlyName => "CC604C: EXIT SUMMARY DECLARATION AMENDMENT ACCEPTANCE";

		protected override CC604CProcessor Processor => new CC604CProcessor(logger, typeof(Cc604C));

		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.IE604;

		protected override ZString MessageText => AESInterchangeProcessorTestHelper.GetStandardAESCC604CText("21IEDUB11A782454R2");
	}
}
