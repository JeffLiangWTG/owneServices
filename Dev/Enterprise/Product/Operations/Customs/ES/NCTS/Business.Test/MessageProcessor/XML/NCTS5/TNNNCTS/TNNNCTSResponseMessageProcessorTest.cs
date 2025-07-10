using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CCTNNC_v515.CCTNNCV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class TNNNCTSResponseMessageProcessorTest : NCTS5CommonResponseMessageProcessorTest<TNNNCTSResponseMessageProcessor, TNNNCTSMessagePrettyFormatter, Cctnncv1Sal>
	{
		public void TestProcessAcceptedMessage()
		{
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>SYLLV39DBRPW3DKV</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>DE - Dispatch</td></tr></table>" +
				"<H4>CSV Electronic Document</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Reference</strong></td><td><strong>CSV Document</strong></td></tr>" +
				"<tr><td>DOC1</td><td>CSVDoc1</td></tr></table>";
			AssertNCTSTNN(message, ESNCTS5DepartureCustomsStatusList.Codes.MrnAllocated, expectedMessageInterpretationText);
		}

		public void TestProcessAcceptedMessageWhenResponseCodeNotA()
		{
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptanceResponseCodeNotATestFile(), InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>SYLLV39DBRPW3DKV</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>DE - Dispatch</td></tr></table>" +
				"<H4>CSV Electronic Document</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Reference</strong></td><td><strong>CSV Document</strong></td></tr>" +
				"<tr><td>DOC1</td><td>CSVDoc1</td></tr></table>";
			AssertNCTSTNN(message, OriginalEntryStatus, expectedMessageInterpretationText);
		}

		public void TestMessageProcessingErrorHeaderNotArrival()
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.MovementHeader.BM_Phase = InitialPhaseStatus;
			nctsHeader.MovementHeader.BM_CustomsStatus = OriginalEntryStatus;
			var responseMessage = CreateNewEDIMessage(ApplicationReference, GetAcceptanceTestFile(), InterchangeID);

			responseMessage.EM_LinkedObject = nctsHeader;

			ProcessMessageForTest(responseMessage);
			var expectedMessageInterpretation = string.Format("<H3>Processor Failure</H3><br>" +
						"<H4>Failure: Unable to read message text from message (Number:{0}, Type:{1}, Sub:{2}, Ref:{3}); message status set to Failed.</H4>" +
						"<H4>Exception: Header type is D so can't process Departure response message</H4>", responseMessage.EM_MessageNum, responseMessage.EM_MessageType, responseMessage.EM_MessageSubType, responseMessage.EM_ApplicationReference);

			AssertNCTSTNN(responseMessage, OriginalEntryStatus, expectedMessageInterpretation, messageSubType: "AAA", emStatus: "FAL", messageStatus: "FAL");
		}

		public void TestMessageProcessingErrorNoHeaderTNN()
		{
			nctsHeader.ESNctsHeader.CEN_TNNArrival = false;
			var responseMessage = CreateNewEDIMessage(ApplicationReference, GetAcceptanceTestFile(), InterchangeID);

			responseMessage.EM_LinkedObject = nctsHeader;

			ProcessMessageForTest(responseMessage);
			var expectedMessageInterpretation = string.Format("<H3>Processor Failure</H3><br>" +
						"<H4>Failure: Unable to read message text from message (Number:{0}, Type:{1}, Sub:{2}, Ref:{3}); message status set to Failed.</H4>" +
						"<H4>Exception: Arrival has no TNN Departure associated so can't process response message</H4>", responseMessage.EM_MessageNum, responseMessage.EM_MessageType, responseMessage.EM_MessageSubType, responseMessage.EM_ApplicationReference);

			AssertNCTSTNN(responseMessage, OriginalEntryStatus, expectedMessageInterpretation, messageSubType: "AAA", emStatus: "FAL", messageStatus: "FAL");
		}

		void AssertNCTSTNN(TestEdiMessage message, string tnnDepartureStatus, string expectedMessageInterpretation, string messageSubType = "ACC", string emStatus = EDIMessage.Status.Received, string messageStatus = "")
		{
			var headerTNN = nctsHeader.ArrivalMovementHeader?.HeaderTNN;
			if (headerTNN != null)
			{
				AssertEquals("headerTNN.MovementHeader.BM_CustomsStatus", tnnDepartureStatus, headerTNN.MovementHeader.BM_CustomsStatus);
				AssertEquals("headerTNN.MovementHeader.BM_MessageStatus", messageStatus, headerTNN.MovementHeader.BM_MessageStatus);
			}
			AssertNCTSDeclaration(message, messageSubType: messageSubType, expectedMessageInterpretation: expectedMessageInterpretation, mrnEntrynum: MRNCode, commonCustomsStatus: OriginalEntryStatus, emStatus: emStatus, messageStatus: messageStatus, arrivalMovementHeader: nctsHeader.ArrivalMovementHeader);
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.ArrivalMrnFromUser = MRNCode;
			nctsHeader.ESNctsHeader.CEN_TNNArrival = true;

			nctsHeaderTNN = Factory.New<NctsHeader>();
			nctsHeaderTNN.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeaderTNN.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeaderTNN.ESNctsHeader.CEN_TNNArrival = true;
			nctsHeaderTNN.MovementReferenceEntryNumber.CE_EntryNum = MRNCode;
			var tnnMovement = nctsHeaderTNN.MovementHeader;
			tnnMovement.BM_CustomsStatus = OriginalEntryStatus;
			tnnMovement.BM_Phase = DeclarationMessageTypeList.Codes.Ncts5IndirectDepartureRegistration;
			tnnMovement.BM_MessageStatus = LogicalStatusList.Codes.Sent;

			nctsHeader.ArrivalMovementHeader.BM_BM_DepartureMovement = tnnMovement.PK;
		}
		NctsHeader nctsHeaderTNN;

		const string MRNCode = "22ES000101500674J7";

		protected override ZString TypeDeclaration => NctsMovementType.Codes.Arrival;
		protected override ZString MRNCodeWhenRejectedOrError => MRNCode;
		protected override ZString GetExpectedProcessorFriendlyName() => "NCTS TNN Declaration Message Processor";
		string GetAcceptanceTestFile() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.TNNNCTSTestFilePath, "AcceptedMessage.txt");
		string GetAcceptanceResponseCodeNotATestFile() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.TNNNCTSTestFilePath, "AcceptedMessageResponseCodeNotA.txt");
		protected override string GetRejectedTestFile() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.TNNNCTSTestFilePath, "RejectedMessage.txt");
		protected override string GetErrorTestFile() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.TNNNCTSTestFilePath, "ErrorMessage.txt");
		protected override string GetAcceptanceTestFileWithLongSegmentId() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.TNNNCTSTestFilePath, "AcceptedMessageWithLongSegmentId.txt");

		protected override TNNNCTSResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new TNNNCTSResponseMessageProcessor(logger);

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.Ncts5IndirectDepartureRegistration };
	}
}
