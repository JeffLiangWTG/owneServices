using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM484;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM484Processor))]
	class IM484ProcessorTest : EntryHeaderMessageProcessorTest<IM484Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IIM484Provider>
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

		protected override ZString MessageFriendlyName => "IM484: Document Presentation Request";

		protected override IM484Processor Processor => new IM484Processor(logger, typeof(Im484));

		protected override void AssertProcessResultCore(CusEntryHeader entry, AISInboundEDIMessage incomingMessage)
		{
			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Document Presentation Request (IM484) message has been received for Job B00001000" },
				new string[] { "staff1@where.com" });
		}

		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM484;

		protected override ZString MessageText => GetMessageText();

		protected virtual ZString GetMessageText() => Serialize(new Im484()
		{
			ImportOperation = new MCciOperationType55()
			{
				Mrn = "12MRN345CDEFG678R9",
				Lrn = "LRN",
				RequestDate = new DateTime(2023, 09, 20),
				DateLimit = new DateTime(2023, 09, 21),
			},
			GoodsShipment = new Collection<DocumentAdditionalInformationType>()
			{
				new DocumentAdditionalInformationType()
				{
					DocumentType = "D001",
					DocumentComplementaryInformation = "DocInfo1"
				},
				new DocumentAdditionalInformationType()
				{
					DocumentType = "D002",
					DocumentComplementaryInformation = "DocInfo2"
				}
			}
		});
	}
}
