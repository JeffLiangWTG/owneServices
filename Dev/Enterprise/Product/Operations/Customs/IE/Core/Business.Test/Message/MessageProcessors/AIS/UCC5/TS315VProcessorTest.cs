using System;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS315V;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.UCC5.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(TS315VProcessor))]
	sealed class TS315VProcessorTest : TemporaryStorageHeaderMessageProcessorTest<TS315VProcessor, AISUCC5InboundEDIMessage, AISUCC5OutboundEDIMessage, Messaging.UCC5.TS315VProvider>
	{
		protected override void AssertProcessResultCore(TemporaryStorageHeader header, AISUCC5InboundEDIMessage incomingMessage)
		{
			AssertEquals("Logical Status", LogicalStatusList.Codes.Accepted, header.AMA_MessageStatus);
			AssertEquals("Entry Status", AISEntryStatusList.Codes.Registered, header.CustomsStatus);
			AssertEquals("MRN", "21IEDUB11A782454R2", header.MRN);
			AssertEquals("Acknowledgement Date", new DateTime(2023, 9, 5), header.CustomsStatusDate);

			AssertMessageInterpretation(incomingMessage, @"A Temporary Storage Declaration Registration Acceptance message (TS315V) message has been received for Job MAN0001000.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN123</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Declaration Acknowledgement Date</td><td>05-Sep-23</td></tr></table>");

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for MAN0001000",
				new[] { "A Temporary Storage Declaration Registration Acceptance message (TS315V) message has been received for Job MAN0001000." },
				new string[] { "staff1@where.com" });
		}

		protected override ZString MessageType => AISInterchangeTypeList.Codes.TS315V;

		protected override ZString MessageFriendlyName => "TS315V: [G4 | G4+G3 | Manifest] Declaration Registration";

		protected override TS315VProcessor Processor => new TS315VProcessor(logger, typeof(Ts315V));

		protected override ZString MessageText => AISUCC5InterchangeProcessorTestHelper.GetTS315VText("21IEDUB11A782454R2", "LRN123", new DateTime(2023, 9, 5, 12, 30, 30));
	}
}
