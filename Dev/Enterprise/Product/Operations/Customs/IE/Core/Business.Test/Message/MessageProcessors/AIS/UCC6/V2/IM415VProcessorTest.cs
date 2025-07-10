using System;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM415V;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM415VProcessor))]
	class IM415VProcessorTest : EntryHeaderMessageProcessorTest<IM415VProcessor, AISInboundEDIMessage, AISOutboundEDIMessage, IM415VProvider>
	{
		public void TestDataProviderNullDueToInvalidMessage()
		{
			(var declaration, var entry, var outgoingMessage, var incomingMessage) = CreateSetupData();
			incomingMessage.EM_MessageText = "<GREETING>HELLO</GREETING>";
			using (incomingMessage.Factory.AddDisposableService())
			{
				var processor = Processor;
				processor.PreProcessMessage(incomingMessage);
				AssertNoExceptionThrown(() => processor.ProcessMessage(incomingMessage));
				CombineAssertions("Process", () =>
				{
					AssertEquals("MovementReferenceNumber", ZString.Empty, entry.MovementReferenceNumber);
					AssertEquals("CH_EntryStatus", ZString.Empty, entry.CH_EntryStatus);
					AssertEquals("EM_Status", EDIMessage.Status.Failed, incomingMessage.EM_Status);
				});
			}
		}

		protected override ZString MessageFriendlyName => "IM415V: Customs Declaration Acknowledgment";

		protected override IM415VProcessor Processor => new IM415VProcessor(logger, typeof(Im415V));

		protected override void AssertProcessResultCore(CusEntryHeader entry, AISInboundEDIMessage incomingMessage)
		{
			AssertEquals("MovementReferenceNumber", "21IEDUB11A782454R2", entry.MovementReferenceNumber);
			AssertEquals("MovementReferenceNumberIssueDate", new DateTime(2021, 02, 15), entry.MovementReferenceNumberIssueDate);
			AssertEquals("CH_Status", LogicalStatusList.Codes.Acknowledged, entry.CH_Status);
			AssertEquals("CH_EntryStatus", AISEntryStatusList.Codes.Prelodged, entry.CH_EntryStatus);

			AssertMessageInterpretation(incomingMessage, @"A Customs Declaration Acknowledgment (IM415V) message has been received for Job B00001000.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Additional Declaration Type</td><td>D</td></tr><tr><td>LRN</td><td>ACPTESTIM0990446123456</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Declaration Acknowledgement Date</td><td>2021-02-15</td></tr></table>");

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Customs Declaration Acknowledgment (IM415V) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}

		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM415V;

		protected override ZString MessageText => GetMessageText();

		protected virtual ZString GetMessageText()
		{
			return Serialize(
				new Im415V
				{
					ImportOperation = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MCciOperationType415V
					{
						AdditionalDeclarationType = "D",
						Lrn = "ACPTESTIM0990446123456",
						Mrn = "21IEDUB11A782454R2",
						DeclarationAcknowledgementDate = new DateTime(2021, 02, 15),
					},
					CustomsOfficeLodgement = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MScoType { ReferenceNumber = "LCO12345" },
					Representative = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MRepresentativeType { IdentificationNumber = "ID1", Status = "0" },
					Declarant = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MDeclarantType { IdentificationNumber = "ID2" },
				});
		}
	}
}
