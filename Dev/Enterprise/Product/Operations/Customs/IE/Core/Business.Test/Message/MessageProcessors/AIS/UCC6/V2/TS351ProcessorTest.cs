using System;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.TS351;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(TS351Processor))]
	class TS351ProcessorTest : EntryHeaderMessageProcessorTest<TS351Processor, AISInboundEDIMessage, AISOutboundEDIMessage, TS351Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.TS351;

		protected override ZString MessageText => Serialize(new Ts351
		{
			Declaration = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.DeclarationType351
			{
				Lrn = "LRN001",
				Mrn = "12MRN345ABCDE678R9",
				SpecificCircumstanceIndicator = "SCI",
				PreviousDocument = new System.Collections.ObjectModel.Collection<CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.PreviousdocumentType08>
				{
					new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.PreviousdocumentType08 { DateAndTimeOfPresentationOfTheGoods = new DateTime(2023, 08, 10, 14, 30, 45) },
					new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.PreviousdocumentType08 { DateAndTimeOfPresentationOfTheGoods = new DateTime(2023, 08, 11, 14, 30, 45) },
				},
				ControlResult = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MControlResultType06
				{
					Code = "CR",
					Date = new DateTime(2023, 08, 15, 14, 30, 45),
					Remarks = "Control Result Remarks",
				},
				Remarks = "Remarks001",
			},
			SupervisingCustomsOffice = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.SupervisingcustomofficeType { ReferenceNumber = "SCO12345" },
			CustomsOfficeLodgement = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MScoType01 { ReferenceNumber = "LCO12345" },
		});

		protected override ZString MessageFriendlyName => "TS351: [G4 | G4+G3 | Manifest] Refusal";

		protected override TS351Processor Processor => new TS351Processor(logger, typeof(Ts351));

		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_Status", LogicalStatusList.Codes.Accepted, messageAttachee.CH_Status);

			AssertEquals("CH_EntryStatus", AISEntryStatusList.Codes.NotReleased, messageAttachee.CH_EntryStatus);

			AssertMessageInterpretation(incomingMessage, @"A [G4 | G4+G3 | Manifest] Refusal (TS351) message has been received for Job B00001000.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN001</td></tr><tr><td>MRN</td><td>12MRN345ABCDE678R9</td></tr><tr><td>Specific Circumstance Indicator</td><td>SCI</td></tr><tr><td>Date and Time of Presentation of the Goods</td><td>10-Aug-23</td></tr><tr><td>&nbsp;</td><td>11-Aug-23</td></tr><tr><td>Control Result Code</td><td>CR</td></tr><tr><td>Control Result Date</td><td>15-Aug-23</td></tr><tr><td>Control Result Remarks</td><td>Control Result Remarks</td></tr><tr><td>Remarks</td><td>Remarks001</td></tr></table>");

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A [G4 | G4+G3 | Manifest] Refusal (TS351) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}
	}
}
