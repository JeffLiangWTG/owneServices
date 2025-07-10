using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM460;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Customs.IE.Messaging.UCC5;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(IM460Processor))]
	sealed class IM460ProcessorTest : EntryHeaderMessageProcessorTest<IM460Processor, AISUCC5InboundEDIMessage, AISUCC5OutboundEDIMessage, IM460Provider>
	{
		protected override ZString MessageType => Messaging.AISInterchangeTypeList.Codes.IM460;

		protected override ZString MessageFriendlyName => "IM460: Control Notice";

		protected override IM460Processor Processor => new IM460Processor(logger, typeof(Im460));

		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISUCC5InboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_Status", LogicalStatusList.Codes.Accepted, messageAttachee.CH_Status);
			AssertEquals("CH_EntryStatus", AISEntryStatusList.Codes.Control, messageAttachee.CH_EntryStatus);

			AssertMessageInterpretation(incomingMessage, @"A Control (IM460) message has been received for Job B00001000.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr><tr><td>Control Notification Date &amp; Time</td><td>20-Feb-24 23:59</td></tr><tr><td>Time Limit for Control</td><td>07-Mar-24 14:37</td></tr><tr><td>Customs Office Lodgement</td><td>OF123456</td></tr><tr><td>Overall Control Type Coded</td><td>O</td></tr></table><br /><br />Control Type<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Control Type Coded</td><td>O</td></tr><tr><td>Control Type Agency</td><td>Revenue</td></tr></table><br /><br />Control Type<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Control Type Coded</td><td>1</td></tr><tr><td>Control Type Agency</td><td>Revenue1</td></tr></table>");
			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Control (IM460) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}

		protected override ZString MessageText => AISInterchangeProcessorTestHelper.GetStandardUCC5IM460Text();
	}
}
