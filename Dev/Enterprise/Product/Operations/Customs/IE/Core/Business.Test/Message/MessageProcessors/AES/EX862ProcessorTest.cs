using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.EX862;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	[TestedType(typeof(EX862Processor))]
	class EX862ProcessorTest : EntryHeaderMessageProcessorTest<EX862Processor, AESInboundEDIMessage, AESOutboundEDIMessage, EX862Provider>
	{
		protected override void AssertProcessResultCore(CusEntryHeader entry, AESInboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_Status", LogicalStatusList.Codes.Accepted, entry.CH_Status);
			AssertEquals("EX562 will only be sent when CH_EntryStatus being CON, therefore EX862 only need to revert it to CON.", AESEntryStatusList.Codes.ControlledForExport, entry.CH_EntryStatus);

			var jobNumber = entry.Declaration.JE_DeclarationReference;
			var messageInterpretation = $@"A Declaration Amendment Request Cancellation message has been received from Customs for Job {jobNumber} through the EX862 message stating that declaration amendment request that was initiated through message EX562 has now been canceled. <br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Case Id</td><td>CASEID</td></tr><tr><td>Amendment Request Cancellation Reason</td><td>REASON</td></tr></table>";
			AssertMessageInterpretation(incomingMessage, messageInterpretation);
			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for " + jobNumber, new[] { messageInterpretation }, new string[] { "staff1@where.com" });
		}

		protected override ZString MessageFriendlyName => "EX862: Declaration Amendment Request Cancellation";

		protected override EX862Processor Processor => new EX862Processor(logger, typeof(Ex862));

		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.EX862;

		protected override ZString MessageText => AESInterchangeProcessorTestHelper.GetStandardEX862Text("21IEDUB11A782454R2");
	}
}
