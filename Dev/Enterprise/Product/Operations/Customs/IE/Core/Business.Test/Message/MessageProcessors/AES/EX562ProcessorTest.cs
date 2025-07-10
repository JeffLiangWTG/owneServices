using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.EX562;
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
	[TestedType(typeof(EX562Processor))]
	class EX562ProcessorTest : EntryHeaderMessageProcessorTest<EX562Processor, AESInboundEDIMessage, AESOutboundEDIMessage, EX562Provider>
	{
		protected override void AssertProcessResultCore(CusEntryHeader entry, AESInboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_Status should have been set Accepted.", LogicalStatusList.Codes.Accepted, entry.CH_Status);
			AssertEquals("CH_EntryStatus should have been set AMR.", AESEntryStatusList.Codes.AmendmentRequested, entry.CH_EntryStatus);

			MessageProcessorNotificationTestHelper.AssertEmail(
				incomingMessage.MessageTypeWithDescription + " Response (Failure) for B00001000",
				new[] { "An Amendment Request message has been received from Customs for Job B00001000 through the EX562 message." },
				new string[] { "staff1@where.com" });

			AssertMessageInterpretation(incomingMessage, @"An Amendment Request message has been received from Customs for Job B00001000 through the EX562 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Case Id</td><td>CASE ID</td></tr><tr><td>Remarks</td><td>REMARKS</td></tr></table>");
		}

		protected override ZString MessageFriendlyName => "EX562: REQUEST DECLARATION AMENDMENT";

		protected override EX562Processor Processor => new EX562Processor(logger, typeof(Ex562));

		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.EX562;

		protected override ZString MessageText => AESInterchangeProcessorTestHelper.GetStandardEX562Text("21IEDUB11A782454R2");
	}
}
