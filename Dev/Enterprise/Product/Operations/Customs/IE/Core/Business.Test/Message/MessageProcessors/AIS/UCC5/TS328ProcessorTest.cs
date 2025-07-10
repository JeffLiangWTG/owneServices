using System;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS328;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.UCC5.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(TS328Processor))]
	sealed class TS328ProcessorTest : TemporaryStorageHeaderMessageProcessorTest<TS328Processor, AISUCC5InboundEDIMessage, AISUCC5OutboundEDIMessage, Messaging.UCC5.TS328Provider>
	{
		protected override void AssertProcessResultCore(TemporaryStorageHeader messageAttachee, AISUCC5InboundEDIMessage incomingMessage)
		{
			AssertEquals("Message status", LogicalStatusList.Codes.Accepted, messageAttachee.AMA_MessageStatus);
			AssertEquals("Customs Status", AISEntryStatusList.Codes.Accepted, messageAttachee.CustomsStatus);
			AssertEquals("MRN", "21IEDUB11A782454R2", messageAttachee.MRN);
			AssertEquals("Customs Status Date", new ZDateTime(2023, 9, 5), messageAttachee.CustomsStatusDate);

			AssertMessageInterpretation(incomingMessage, @"A TSD Acceptance message (TS328) message has been received for Job MAN0001000.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN123</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Acceptance Date</td><td>05-Sep-23</td></tr><tr><td>Response Date Limit</td><td>05-Oct-23</td></tr><tr><td>Remarks</td><td>remarks</td></tr></table>");

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for MAN0001000",
				new[] { "A TSD Acceptance message (TS328) message has been received for Job MAN0001000." },
				new string[] { "staff1@where.com" });
		}

		protected override ZString MessageType => AISInterchangeTypeList.Codes.TS328;

		protected override ZString MessageFriendlyName => "TS328 - [G4 | G4+G3 | Manifest] Declaration Acceptance";

		protected override TS328Processor Processor => new TS328Processor(logger, typeof(Ts328));

		protected override ZString MessageText => AISUCC5InterchangeProcessorTestHelper.GetTS328Text("21IEDUB11A782454R2", "LRN123", new DateTime(2023, 9, 5, 12, 30, 30));
	}
}
