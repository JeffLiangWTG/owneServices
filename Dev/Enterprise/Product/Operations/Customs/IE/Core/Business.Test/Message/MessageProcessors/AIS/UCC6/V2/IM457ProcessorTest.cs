using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM457;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM457Processor))]
	sealed class IM457ProcessorTest : EntryHeaderMessageProcessorTest<IM457Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IM457Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM457;

		protected override ZString MessageText => AISInterchangeProcessorTestHelper.GetStandardIM457Text();

		protected override ZString MessageFriendlyName => "IM457: Presentation Notification Registration";

		protected override IM457Processor Processor => new IM457Processor(logger, typeof(Im457));

		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_EntryStatus", AISEntryStatusList.Codes.Registered, messageAttachee.CH_EntryStatus);
			AssertEquals("CH_Status", LogicalStatusList.Codes.Accepted, messageAttachee.CH_Status);

			AssertMessageInterpretation(incomingMessage, @"A Presentation Notification Registration (IM457) message has been received for Job B00001000.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN001</td></tr><tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr><tr><td>Presentation Notification Registration Date and Time</td><td>15-Aug-23 14:10</td></tr></table>");

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Presentation Notification Registration (IM457) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}
	}
}
