using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM464;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(IM464Processor))]
	sealed class IM464ProcessorTest : EntryHeaderMessageProcessorTest<IM464Processor, AISUCC5InboundEDIMessage, AISUCC5OutboundEDIMessage, IM464Provider>
	{
		protected override ZString MessageType => Messaging.AISInterchangeTypeList.Codes.IM464;

		protected override ZString MessageFriendlyName => "IM464: Request Declaration Invalidation";

		protected override IM464Processor Processor => new IM464Processor(logger, typeof(Im464));

		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISUCC5InboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_Status", LogicalStatusList.Codes.Accepted, messageAttachee.CH_Status);
			AssertEquals("CH_EntryStatus", AISEntryStatusList.Codes.CancellationRequested, messageAttachee.CH_EntryStatus);

			AssertMessageInterpretation(incomingMessage, @"A Request Declaration Invalidation (IM464) message has been received for Job B00001000.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr><tr><td>Case Id</td><td>11111111-1111-1111-1111-111111111111</td></tr><tr><td>Remarks</td><td>Remarks</td></tr></table>");
			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Request Declaration Invalidation (IM464) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}

		protected override ZString MessageText => AISInterchangeProcessorTestHelper.GetStandardUCC5IM464Text();
	}
}
