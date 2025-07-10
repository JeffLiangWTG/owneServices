using System;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.TS315V;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.AIS;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(TS315VProcessor))]
	sealed class TS315VProcessorTest : EntryHeaderMessageProcessorTest<TS315VProcessor, AISInboundEDIMessage, AISOutboundEDIMessage, TS315VProvider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.TS315V;

		protected override ZString MessageText => Serialize(new Ts315V
		{
			Declaration = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.DeclarationType12
			{
				Lrn = "LRN001",
				Mrn = "12MRN345ABCDE678R9",
				DeclarationAcknowledgementDate = new DateTime(2023, 08, 10, 14, 30, 45),
			},
			SupervisingCustomsOffice = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.SupervisingcustomofficeType { ReferenceNumber = "SCO12345" },
			CustomsOfficeLodgement = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MScoType01 { ReferenceNumber = "LCO12345" },
			Declarant = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.DeclarantType03 { IdentificationNumber = "ID1" },
		});

		protected override ZString MessageFriendlyName => "TS315V: [G4 | G4+G3 | Manifest] Declaration Registration";

		protected override TS315VProcessor Processor => new TS315VProcessor(logger, typeof(Ts315V));

		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_EntryStatus", AISEntryStatusList.Codes.Registered, messageAttachee.CH_EntryStatus);
			AssertEquals("CH_Status", LogicalStatusList.Codes.Acknowledged, messageAttachee.CH_Status);

			AssertMessageInterpretation(incomingMessage, @"A [G4 | G4+G3 | Manifest] Declaration Registration (TS315V) message has been received for Job B00001000.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN001</td></tr><tr><td>MRN</td><td>12MRN345ABCDE678R9</td></tr><tr><td>Declaration Acknowledgement Date</td><td>10-Aug-23</td></tr></table>");

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
	new[] { "A [G4 | G4+G3 | Manifest] Declaration Registration (TS315V) message has been received for Job B00001000." },
	new string[] { "staff1@where.com" });
		}
	}
}
