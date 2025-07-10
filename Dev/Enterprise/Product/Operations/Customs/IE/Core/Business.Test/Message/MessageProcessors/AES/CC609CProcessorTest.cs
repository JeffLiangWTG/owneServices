using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC609C;
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
	[TestedType(typeof(CC609CProcessor))]
	class CC609CProcessorTest : EntryHeaderMessageProcessorTest<CC609CProcessor, AESInboundEDIMessage, AESOutboundEDIMessage, CC609CProvider>
	{
		protected override void AssertProcessResultCore(CusEntryHeader entry, AESInboundEDIMessage incomingMessage)
		{
			AssertMessageInterpretation(incomingMessage, @"An EXS/REN Invalidation Decision message has been received from Customs for Job B00001000 through the IE609 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Invalidation Decision Date and Time</td><td>19-Sep-71 00:00</td></tr><tr><td>Invalidation Request Date and Time</td><td>18-Sep-71 00:00</td></tr></table>");
			MessageProcessorNotificationTestHelper.AssertEmail(
				incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "An EXS/REN Invalidation Decision message has been received from Customs for Job B00001000 through the IE609 message." },
				new string[] { "staff1@where.com" }
				);
			AssertEquals("Entry Status.", AESEntryStatusList.Codes.Cancelled, entry.CH_EntryStatus);
			AssertEquals("Not set Logical Status", LogicalStatusList.Codes.Accepted, entry.CH_Status);
		}

		protected override ZString MessageFriendlyName => "CC609C: EXIT SUMMARY INVALIDATION/CANCELLATION";

		protected override CC609CProcessor Processor => new CC609CProcessor(logger, typeof(Cc609C));

		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.IE609;

		protected override ZString MessageText => AESInterchangeProcessorTestHelper.GetStandardAESCC609CText("LRN123456789", "21IEDUB11A782454R2");
	}
}
