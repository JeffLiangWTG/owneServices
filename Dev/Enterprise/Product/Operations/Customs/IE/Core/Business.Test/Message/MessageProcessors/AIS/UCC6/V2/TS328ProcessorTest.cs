using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.TS328;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(TS328Processor))]
	sealed class TS328ProcessorTest : TemporaryStorageHeaderMessageProcessorTest<TS328Processor, AISInboundEDIMessage, AISOutboundEDIMessage, TS328Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.TS328;

		protected override ZString MessageText => Serialize(new Ts328()
		{
			Declaration = new DeclarationType13
			{
				Lrn = "LRN123456789",
				Mrn = "21IEDUB11A782454R2",
				AcceptanceDate = new DateOfAcceptanceType
				{
					DateOfAcceptance = new System.DateTime(2023, 09, 05),
				},
				ResponseDateLimit = new System.DateTime(2023, 10, 05),
				PreviousDocument = new System.Collections.ObjectModel.Collection<PreviousdocumentType08>
				{
					new PreviousdocumentType08
					{
						DateAndTimeOfPresentationOfTheGoods = new System.DateTime(2023, 09, 01, 00, 00, 00),
					}
				},
				Remarks = "Sample text 123",
			},
			SupervisingCustomsOffice = new SupervisingcustomofficeType
			{
				ReferenceNumber = "IEDUB100",
			},
			CustomsOfficeLodgement = new MScoType01
			{
				ReferenceNumber = "IEDUB999",
			},
			PresentationOffice = new PresentationOfficeType01
			{
				ReferenceNumber = "IEDUB555"
			},
			Declarant = new DeclarantType03
			{
				IdentificationNumber = "IE1234567",
			}
		});

		protected override ZString MessageFriendlyName => "TS328 - [G4 | G4+G3 | Manifest] Declaration Acceptance";

		protected override TS328Processor Processor => new TS328Processor(logger, typeof(Ts328));

		protected override void AssertProcessResultCore(TemporaryStorageHeader messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			AssertEquals("Message status", "ACC", messageAttachee.AMA_MessageStatus);
			AssertEquals("Customs Status", "ACC", messageAttachee.CustomsStatus);
			AssertEquals("MRN", "21IEDUB11A782454R2", messageAttachee.MRN);
			AssertEquals("Customs Status Date", new ZDateTime(2023, 9, 5), messageAttachee.CustomsStatusDate);
			AssertMessageInterpretation(incomingMessage, @"A [G4 | G4+G3 | Manifest] Declaration Acceptance (TS328) message has been received for Job MAN0001000.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN123456789</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Date of Acceptance</td><td>05-Sep-23</td></tr><tr><td>Response Date Limit</td><td>05-Oct-23</td></tr><tr><td>Date and Time of Presentation of the Goods</td><td>01-Sep-23 00:00</td></tr><tr><td>Remarks</td><td>Sample text 123</td></tr></table>");
			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for MAN0001000",
				new[] { "A [G4 | G4+G3 | Manifest] Declaration Acceptance (TS328) message has been received for Job MAN0001000." },
				new string[] { "staff1@where.com" });
		}
	}
}
