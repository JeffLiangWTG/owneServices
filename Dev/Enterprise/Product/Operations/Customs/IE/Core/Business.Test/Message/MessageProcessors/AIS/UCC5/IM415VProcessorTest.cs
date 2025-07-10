using System;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM415V;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging.UCC5;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(IM415VProcessor))]
	sealed class IM415VProcessorTest : EntryHeaderMessageProcessorTest<IM415VProcessor, AISUCC5InboundEDIMessage, AISUCC5OutboundEDIMessage, IM415VProvider>
	{
		protected override ZString MessageType => Messaging.AISInterchangeTypeList.Codes.IM415V;

		protected override ZString MessageText => Serialize(new Im415V()
		{
			Declaration = new DeclarationType()
			{
				DeclarationType11 = "IM",
				AdditionalDeclarationType12 = "D",
				Lrn25 = "ACPTESTIM0990446123456",
				Mrn = "21IEDUB11A782454R2",
				DeclarationAcknowledgementDate = "20210215",
				CustomsOffices = new DeclarationTypeCustomsOffices
				{
					CustomsOfficeLodgement = "IEDUB100",
				}
			},
		});

		protected override ZString MessageFriendlyName => "IM415V: Customs Declaration Acknowledgment";

		protected override IM415VProcessor Processor => new IM415VProcessor(logger, typeof(Im415V));

		protected override void AssertProcessResultCore(CusEntryHeader entry, AISUCC5InboundEDIMessage incomingMessage)
		{
			AssertEquals("MovementReferenceNumber", "21IEDUB11A782454R2", entry.MovementReferenceNumber);
			AssertEquals("MovementReferenceNumberIssueDate", new DateTime(2021, 02, 15), entry.MovementReferenceNumberIssueDate);
			AssertEquals("CH_Status", LogicalStatusList.Codes.Acknowledged, entry.CH_Status);
			AssertEquals("CH_EntryStatus", AISEntryStatusList.Codes.Prelodged, entry.CH_EntryStatus);

			AssertMessageInterpretation(incomingMessage, @"A Customs Declaration Acknowledgement (IM415V) message has been received from customs for Job B00001000.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Additional Declaration Type</td><td>D</td></tr><tr><td>LRN</td><td>ACPTESTIM0990446123456</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Declaration Acknowledgement Date</td><td>2021-02-15</td></tr></table>");

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Customs Declaration Acknowledgement (IM415V) message has been received from customs for Job B00001000." },
				new string[] { "staff1@where.com" });
		}
	}
}
