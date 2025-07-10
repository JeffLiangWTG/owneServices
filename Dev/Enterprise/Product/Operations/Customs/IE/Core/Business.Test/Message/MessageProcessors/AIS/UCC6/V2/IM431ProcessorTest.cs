using System;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM431;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM431Processor))]
	class IM431ProcessorTest : EntryHeaderMessageProcessorTest<IM431Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IM431Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM431;

		protected override ZString MessageText => Serialize(new Im431
		{
			ImportOperation = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MCciOperationType01
			{
				Lrn = "LRN001",
				Mrn = "12MRN345CDEFG678R9",
			},
			SupervisingCustomsOffice = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MScoType { ReferenceNumber = "SCO12345" },
			CustomsOfficeLodgement = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MScoType { ReferenceNumber = "LCO12345" },
			Declarant = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MDeclarantType { IdentificationNumber = "ID1" },
			TimerExpiryForSupplementaryDeclaration = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MTimerExpiryType
			{
				LodgementOfSupplementaryDeclarationStartDate = new DateTime(2023, 08, 10, 14, 30, 45),
				LodgementOfSupplementaryDeclarationExpiryDate = new DateTime(2023, 08, 11, 14, 30, 45),
				TimerExpiryInformation = "Timer Expiry Information",
			},
		});

		protected override ZString MessageFriendlyName => "IM431: Expiration of Timer for Supplementary Declaration";

		protected override IM431Processor Processor => new IM431Processor(logger, typeof(Im431));

		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_Status", LogicalStatusList.Codes.Accepted, messageAttachee.CH_Status);

			AssertEquals("MovementReferenceNumberExpiryDate", new DateTime(2023, 08, 11), messageAttachee.MovementReferenceNumberExpiryDate);

			AssertMessageInterpretation(incomingMessage, @"An Expiration of Timer for Supplementary Declaration (IM431) message has been received for Job B00001000.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN001</td></tr><tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr><tr><td>Lodgement of Supplementary Declaration Start Date</td><td>10-Aug-23</td></tr><tr><td>Lodgement of Supplementary Declaration Expiry Date</td><td>11-Aug-23</td></tr><tr><td>Timer Expiry Information</td><td>Timer Expiry Information</td></tr></table>");

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "An Expiration of Timer for Supplementary Declaration (IM431) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}
	}
}
