using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM462;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.AIS;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	[TestedType(typeof(IM462_IM962Processor))]
	class IM462ProcessorTest : AISH7MessageProcessorTest<IM462_IM962Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IM462_IM962Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM462;

		protected override IM462_IM962Processor Processor => new IM462_IM962Processor(logger, typeof(Im462));

		protected override ZString MessageText => GetMessageText();

		protected override ZString MessageFriendlyName => "IM462: Amendment Request";

		protected override void AssertProcessResultCore(AsycudaBill messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			base.AssertProcessResultCore(messageAttachee, incomingMessage);
			AssertEquals("Logical status", string.Empty, messageAttachee.ABL_MessageStatus);
			AssertEquals("Entry status", AISEntryStatusList.Codes.AmendmentRequested, messageAttachee.ABL_BillStatus);

			AssertMessageInterpretation(incomingMessage, @"An Amendment Request (IM462) message has been received for Job H7D00000001.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr><tr><td>Case Id</td><td>1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ</td></tr><tr><td>Amend Reason</td><td>AmendReason001</td></tr></table>");
		}

		ZString GetMessageText()
		{
			return Serialize(new Im462
			{
				Declaration = new DeclarationType
				{
					Mrn = "12MRN345CDEFG678R9",
					CaseId = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ",
					AmendReason = "AmendReason001",
				},
			});
		}
	}
}
