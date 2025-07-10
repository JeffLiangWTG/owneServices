using System;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS351;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.UCC5.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(TS351Processor))]
	sealed class TS351ProcessorTest : TemporaryStorageHeaderMessageProcessorTest<TS351Processor, AISUCC5InboundEDIMessage, AISUCC5OutboundEDIMessage, Messaging.UCC5.TS351Provider>
	{
		protected override void AssertProcessResultCore(TemporaryStorageHeader messageAttachee, AISUCC5InboundEDIMessage incomingMessage)
		{
			AssertEquals("Message status", LogicalStatusList.Codes.Accepted, messageAttachee.AMA_MessageStatus);
			AssertEquals("Customs Status", AISEntryStatusList.Codes.NotReleased, messageAttachee.CustomsStatus);

			AssertMessageInterpretation(incomingMessage, @"A Temporary Storage Refusal (TS351) message has been received for Job MAN0001000.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN123</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Control Result Date</td><td>05-Sep-23</td></tr><tr><td>Control Result Remarks</td><td>control remarks</td></tr><tr><td>Remarks</td><td>remarks</td></tr></table>");

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for MAN0001000",
				new[] { "A Temporary Storage Refusal (TS351) message has been received for Job MAN0001000." },
				new string[] { "staff1@where.com" });
		}

		protected override ZString MessageType => AISInterchangeTypeList.Codes.TS351;

		protected override ZString MessageFriendlyName => "TS351: [G4 | G4+G3 | Manifest] Refusal";

		protected override TS351Processor Processor => new TS351Processor(logger, typeof(Ts351));

		protected override ZString MessageText => AISUCC5InterchangeProcessorTestHelper.GetTS351Text("21IEDUB11A782454R2", "LRN123", new DateTime(2023, 9, 5, 12, 30, 30), "remarks");
	}
}
