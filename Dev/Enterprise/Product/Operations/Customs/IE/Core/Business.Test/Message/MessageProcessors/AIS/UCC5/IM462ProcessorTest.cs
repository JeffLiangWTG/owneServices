using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM462;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Customs.IE.Messaging.UCC5;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(IM462Processor))]
	sealed class IM462ProcessorTest : EntryHeaderMessageProcessorTest<IM462Processor, AISUCC5InboundEDIMessage, AISUCC5OutboundEDIMessage, IM462Provider>
	{
		protected override ZString MessageType => Messaging.AISInterchangeTypeList.Codes.IM462;

		protected override ZString MessageFriendlyName => "IM462: Request Declaration Amendment";

		protected override IM462Processor Processor => new IM462Processor(logger, typeof(Im462));

		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISUCC5InboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_Status", LogicalStatusList.Codes.Accepted, messageAttachee.CH_Status);
			AssertEquals("CH_EntryStatus", AISEntryStatusList.Codes.AmendmentRequested, messageAttachee.CH_EntryStatus);

			AssertMessageInterpretation(incomingMessage, @"A Request Declaration Amendment message (IM462) message has been received for Job B00001000.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr><tr><td>Case Id</td><td>11111111-1111-1111-1111-111111111111</td></tr><tr><td>Amendment Reason</td><td>Amend Reason</td></tr></table>");
			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Request Declaration Amendment message (IM462) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}

		protected override ZString MessageText => AISInterchangeProcessorTestHelper.GetStandardUCC5IM462Text();
	}
}
