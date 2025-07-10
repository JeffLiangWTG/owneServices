using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM962;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM462_IM962Processor))]
	class IM962ProcessorTest : EntryHeaderMessageProcessorTest<IM462_IM962Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IM462_IM962Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM962;

		protected override ZString MessageText => GetMessageText();

		protected virtual ZString GetMessageText()
		{
			return Serialize(new Im962
			{
				ImportOperation = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MCciOperationType462
				{
					Mrn = "12MRN345CDEFG678R9",
					CaseId = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ",
					AmendReason = "AmendReason001",
				}
			});
		}

		protected override ZString MessageFriendlyName => "IM962: Amendment Request";

		protected override IM462_IM962Processor Processor => new IM462_IM962Processor(logger, typeof(Im962));

		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_EntryStatus", AISEntryStatusList.Codes.AmendmentRequested, messageAttachee.CH_EntryStatus);

			AssertMessageInterpretation(incomingMessage, @"An Amendment Request (IM962) message has been received for Job B00001000.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr><tr><td>Case Id</td><td>1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ</td></tr><tr><td>Amend Reason</td><td>AmendReason001</td></tr></table>");

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "An Amendment Request (IM962) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}
	}
}
