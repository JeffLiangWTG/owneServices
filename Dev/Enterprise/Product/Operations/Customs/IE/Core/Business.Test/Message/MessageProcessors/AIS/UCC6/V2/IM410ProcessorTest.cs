using System;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM410;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM410Processor))]
	class IM410ProcessorTest : EntryHeaderMessageProcessorTest<IM410Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IM410Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM410;

		protected override ZString MessageText => Serialize(new Im410
		{
			ImportOperation = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MCciOperationType23
			{
				Lrn = "LRN001",
				CustomsRegistrationNumber = "12CRN345ABCDE678R9",
				Mrn = "12MRN345ABCDE678R9",
				InvalidationDecisionDateAndTime = new DateTime(2023, 08, 10, 14, 30, 45),
				InvalidationRequestDateAndTime = new DateTime(2023, 08, 11, 14, 30, 45),
				InvalidationInitiatedByCustoms = "0",
				InvalidationJustification = "Invalidation Justification",
			},
			SupervisingCustomsOffice = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MScoType { ReferenceNumber = "SCO12345" },
			CustomsOfficeLodgement = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MScoType { ReferenceNumber = "LCO12345" },
			Declarant = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MDeclarantType { IdentificationNumber = "DECLARANT" },
		});

		protected override ZString MessageFriendlyName => "IM410: Invalidation of Customs Declaration";

		protected override IM410Processor Processor => new IM410Processor(logger, typeof(Im410));

		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_Status", LogicalStatusList.Codes.Accepted, messageAttachee.CH_Status);

			AssertEquals("CH_EntryStatus", AISEntryStatusList.Codes.Cancelled, messageAttachee.CH_EntryStatus);

			AssertEquals("CRN", "12CRN345ABCDE678R9", messageAttachee.CRN);

			AssertMessageInterpretation(incomingMessage, @"An Invalidation of Customs Declaration (IM410) message has been received for Job B00001000.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN001</td></tr><tr><td>Customs Registration Number</td><td>12CRN345ABCDE678R9</td></tr><tr><td>MRN</td><td>12MRN345ABCDE678R9</td></tr><tr><td>Invalidation Decision Date and Time</td><td>10-Aug-23 14:30</td></tr><tr><td>Invalidation Request Date and Time</td><td>11-Aug-23 14:30</td></tr><tr><td>Invalidation Initiated by Customs</td><td>0</td></tr><tr><td>Invalidation Justification</td><td>Invalidation Justification</td></tr></table>");

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "An Invalidation of Customs Declaration (IM410) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}
	}
}
