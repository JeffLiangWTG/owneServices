using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM426;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM426Processor))]
	sealed class IM426ProcessorTest : EntryHeaderMessageProcessorTest<IM426Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IM426Provider>
	{
		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_EntryStatus", "ACC", messageAttachee.CH_EntryStatus);
			AssertEquals("CH_Status", "ACC", messageAttachee.CH_Status);
			AssertEquals("CustomsRegistrationNumber", "21IEDUB11A782454R2", messageAttachee.CRN);
			AssertMessageInterpretation(incomingMessage, @"A Registration Notification (IM426) message has been received for Job B00001000.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN123456789</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Customs Registration Number</td><td>21IEDUB11A782454R2</td></tr><tr><td>Declaration Registration Date and Time</td><td>17-Aug-23 14:38</td></tr><tr><td>Presentation Notification Due Date</td><td>2023-05-20</td></tr></table>");
			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Registration Notification (IM426) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}

		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM426;

		protected override ZString MessageText => AISInterchangeProcessorTestHelper.GetStandardIM426Text("LRN123456789", "21IEDUB11A782454R2");

		protected override ZString MessageFriendlyName => "IM426 – Registration Notification";

		protected override IM426Processor Processor => new IM426Processor(logger, typeof(Im426));
	}
}
