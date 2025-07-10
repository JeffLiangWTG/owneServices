using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM862;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.AIS;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	[TestedType(typeof(IM862Processor))]
	class IM862ProcessorTest : AISH7MessageProcessorTest<IM862Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IM862Provider>
	{
		protected override IM862Processor Processor => new IM862Processor(logger, typeof(Im862));

		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM862;

		protected override ZString MessageText => GetMessageText();

		protected override ZString MessageFriendlyName => AISInterchangeTypeList.Descriptions.IM862;

		protected override void AssertProcessResultCore(AsycudaBill messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			base.AssertProcessResultCore(messageAttachee, incomingMessage);
			AssertEquals("Logical status", LogicalStatusList.Codes.Accepted, messageAttachee.ABL_MessageStatus);
			AssertEquals("Entry status", AISEntryStatusList.Codes.Control, messageAttachee.ABL_BillStatus);
			AssertMessageInterpretation(incomingMessage, @"A Declaration Amendment Request Cancellation (IM862) message has been received for Job H7D00000001.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr><tr><td>Case Id</td><td>1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ</td></tr><tr><td>Amendment Request Cancellation Reason</td><td>Reason</td></tr></table>");
		}

		ZString GetMessageText()
		{
			return Serialize(
				new Im862
				{
					Declaration = new DeclarationType
					{
						Mrn = "12MRN345CDEFG678R9",
						CaseId = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ",
						AmendmentRequestCancellationReason = "Reason"
					},
				}
			);
		}
	}
}
