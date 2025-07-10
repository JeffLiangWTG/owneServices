using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM464;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.AIS;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	[TestedType(typeof(IM464Processor))]
	class IM464ProcessorTest : AISH7MessageProcessorTest<IM464Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IM464Provider>
	{
		protected override IM464Processor Processor => new IM464Processor(logger, typeof(Im464));

		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM464;

		protected override ZString MessageText => GetMessageText();

		protected override ZString MessageFriendlyName => AISInterchangeTypeList.Descriptions.IM464;

		protected override void AssertProcessResultCore(AsycudaBill messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			base.AssertProcessResultCore(messageAttachee, incomingMessage);
			AssertEquals("Logical status", string.Empty, messageAttachee.ABL_MessageStatus);
			AssertEquals("Entry status", AISEntryStatusList.Codes.CancellationRequested, messageAttachee.ABL_BillStatus);
			AssertMessageInterpretation(incomingMessage, @"A Request Declaration Invalidation (IM464) message has been received for Job H7D00000001.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr><tr><td>Case Id</td><td>1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ</td></tr><tr><td>Remarks</td><td>Remarks</td></tr></table>");
		}

		ZString GetMessageText()
		{
			return Serialize(
				new Im464
				{
					Declaration = new DeclarationType
					{
						Mrn = "12MRN345CDEFG678R9",
						CaseId = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ",
						Remarks = "Remarks"
					},
				}
			);
		}
	}
}
