using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC504C;
using CargoWise.EntityFramework;
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
	[TestedType(typeof(CC504CProcessor))]
	class CC504CProcessorTest : EntryHeaderMessageProcessorTest<CC504CProcessor, AESInboundEDIMessage, AESOutboundEDIMessage, CC504CProvider>
	{
		public void TestAcceptanceOfUnrequestedAmendment()
		{
			var testData = base.CreateSetupData();
			testData.messageAttachee.CH_EntryStatus = AESEntryStatusList.Codes.Prelodged;
			var incomingMessage = testData.incomingMessage;

			using (incomingMessage.Factory.AddDisposableService())
			{
				Processor.PreProcessMessage(incomingMessage);
				Processor.ProcessMessage(incomingMessage);
			}
			AssertEquals("Not change CH_EntryStatus if an unrequested ExportAmendment is accepted.", AESEntryStatusList.Codes.Prelodged, testData.messageAttachee.CH_EntryStatus);
		}

		protected override (JobDeclaration declaration, CusEntryHeader messageAttachee, EDIMessage outgoingMessage, AESInboundEDIMessage incomingMessage) CreateSetupData()
		{
			var result = base.CreateSetupData();
			result.messageAttachee.CH_EntryStatus = AESEntryStatusList.Codes.AmendmentRequested;
			return result;
		}

		protected override void AssertProcessResultCore(CusEntryHeader entry, AESInboundEDIMessage incomingMessage)
		{
			AssertEquals("Not change CH_Status", LogicalStatusList.Codes.Accepted, entry.CH_Status);
			AssertEquals(
				"When 504 comes(i.e. the previous outgoing ExportAmendment is accepted) and the previous CH_EntryStatus is AmendmentRequested," +
				"We directly set it CON, because the Customs only sends Amendment Requests when the Entry is Under Control(CH_EntryStatus=CON)" +
				"We take the simple solution(rather than get EntryStatus from the history messages) and allow any minor contradictions for now.",
				AESEntryStatusList.Codes.ControlledForExport,
				entry.CH_EntryStatus
			);

			AssertMessageInterpretation(incomingMessage, @"An Amendment Acceptance message has been received from Customs for Job B00001000 through the IE504 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN123456789</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Amendment Submission Date and Time</td><td>18-Sep-71 00:00</td></tr><tr><td>Amendment Acceptance Date and Time</td><td>01-Jul-22 00:00</td></tr></table>");

			MessageProcessorNotificationTestHelper.AssertEmail(
				incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "An Amendment Acceptance message has been received from Customs for Job B00001000 through the IE504 message." },
				new string[] { "staff1@where.com" }
			);
		}

		protected override ZString MessageFriendlyName => "CC504C: EXPORT DECLARATION AMENDMENT ACCEPTANCE";

		protected override CC504CProcessor Processor => new CC504CProcessor(logger, typeof(Cc504C));

		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.IE504;

		protected override ZString MessageText => AESInterchangeProcessorTestHelper.GetStandardCC504CText("LRN123456789", "21IEDUB11A782454R2");
	}
}
