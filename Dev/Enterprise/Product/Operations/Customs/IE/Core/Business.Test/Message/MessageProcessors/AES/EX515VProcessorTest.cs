using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.EX515V;
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
	[TestedType(typeof(EX515VProcessor))]
	class EX515VProcessorTest : EntryHeaderMessageProcessorTest<EX515VProcessor, AESInboundEDIMessage, AESOutboundEDIMessage, EX515VProvider>
	{
		protected override void AssertProcessResultCore(CusEntryHeader entry, AESInboundEDIMessage incomingMessage)
		{
			AssertEquals("Entry Status", AESEntryStatusList.Codes.Prelodged, entry.CH_EntryStatus);
			AssertEquals("Logical Status", LogicalStatusList.Codes.Accepted, entry.CH_Status);
			AssertEquals("MovementReferenceNumber", "21IEDUB11A782454R2", entry.MovementReferenceNumber);
			var jobNumber = entry.Declaration.JE_DeclarationReference;
			var messageInterpretation = $@"An Export Declaration Acknowledgement message has been received from Customs for Job {jobNumber} through the EX515V message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Status</td><td>Acknowledged by Customs(MRN Allocated)</td></tr><tr><td>LRN</td><td>LRN123456789</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Declaration Acknowledgement Date</td><td>22-Dec-20 00:00</td></tr></table>";
			AssertMessageInterpretation(incomingMessage, messageInterpretation);
			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for " + jobNumber, new[] { messageInterpretation }, new string[] { "staff1@where.com" });
		}

		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.EX515V;

		protected override ZString MessageText => AESInterchangeProcessorTestHelper.GetStandardEX515VText("LRN123456789", "21IEDUB11A782454R2");

		protected override ZString MessageFriendlyName => "EX515V: Export Declaration Acknowledgment";

		protected override EX515VProcessor Processor => new EX515VProcessor(logger, typeof(Ex515V));
	}
}
