using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM451;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Customs.IE.Messaging.UCC5;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(IM451Processor))]
	sealed class IM451ProcessorTest : EntryHeaderMessageProcessorTest<IM451Processor, AISUCC5InboundEDIMessage, AISUCC5OutboundEDIMessage, IM451Provider>
	{
		protected override ZString MessageType => Messaging.AISInterchangeTypeList.Codes.IM451;

		protected override ZString MessageFriendlyName => "IM451: Release Rejection";

		protected override IM451Processor Processor => new IM451Processor(logger, typeof(Im451));

		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISUCC5InboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_Status", LogicalStatusList.Codes.Accepted, messageAttachee.CH_Status);
			AssertEquals("CH_EntryStatus", AISEntryStatusList.Codes.NotReleased, messageAttachee.CH_EntryStatus);

			AssertMessageInterpretation(incomingMessage, @"A Release Rejection (IM451) message has been received for Job B00001000.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Additional Declaration Type</td><td>A</td></tr><tr><td>LRN</td><td>LRN123</td></tr><tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr><tr><td>Rejection Reason</td><td>Rejection Reason</td></tr></table>");
			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Release Rejection (IM451) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}

		protected override ZString MessageText => AISInterchangeProcessorTestHelper.GetStandardUCC5IM451Text();
	}
}
