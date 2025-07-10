using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM464;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM464Processor))]
	class IM464ProcessorTest : EntryHeaderMessageProcessorTest<IM464Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IM464Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM464;

		protected override ZString MessageText => GetMessageText();

		protected virtual ZString GetMessageText() => Serialize(new Im464
		{
			ImportOperation = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MCciOperationType464
			{
				Mrn = "12MRN345CDEFG678R9",
				CaseId = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ",
				Remarks = "Remarks001",
			}
		});

		protected override ZString MessageFriendlyName => "IM464: Request Declaration Invalidation";

		protected override IM464Processor Processor => new IM464Processor(logger, typeof(Im464));

		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_EntryStatus", AISEntryStatusList.Codes.CancellationRequested, messageAttachee.CH_EntryStatus);

			AssertMessageInterpretation(incomingMessage, @"A Request Declaration Invalidation (IM464) message has been received for Job B00001000.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr><tr><td>Case Id</td><td>1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ</td></tr><tr><td>Remarks</td><td>Remarks001</td></tr></table>");

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Request Declaration Invalidation (IM464) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}
	}
}
