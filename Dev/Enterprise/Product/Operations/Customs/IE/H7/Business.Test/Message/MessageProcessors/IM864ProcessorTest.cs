using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM864;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.AIS;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	[TestedType(typeof(IM864Processor))]
	class IM864ProcessorTest : AISH7MessageProcessorTest<IM864Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IM864Provider>
	{
		protected override IM864Processor Processor => new IM864Processor(logger, typeof(Im864));

		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM864;

		protected override ZString MessageText => GetMessageText();

		protected override ZString MessageFriendlyName => "IM864: Invalidation Request Cancellation";

		protected override void AssertProcessResultCore(AsycudaBill messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			base.AssertProcessResultCore(messageAttachee, incomingMessage);
			AssertEquals("Logical status", LogicalStatusList.Codes.Accepted, messageAttachee.ABL_MessageStatus);
			AssertEquals("Entry status", AISEntryStatusList.Codes.Control, messageAttachee.ABL_BillStatus);
			AssertMessageInterpretation(incomingMessage, @"An Invalidation Request Cancellation (IM864) message has been received for Job H7D00000001.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr><tr><td>Case Id</td><td>1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ</td></tr><tr><td>Invalidation Request Cancellation Reason</td><td>Reason</td></tr></table>");
		}

		ZString GetMessageText()
		{
			return Serialize(
				new Im864
				{
					Declaration = new DeclarationType
					{
						Mrn = "12MRN345CDEFG678R9",
						CaseId = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ",
						InvalidationRequestCancellationReason = "Reason"
					},
				}
			);
		}
	}
}
