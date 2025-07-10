using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LPOUS.CCIEP01V1Sal;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business.Testing
{
	class ReceptionT2LPOUSResponseMessageProcessorTest : XMLResponseMessageProcessorTest<RequestAndReceptionT2LPOUSResponseMessageProcessor, IMessagePrettyFormatter, Iep01SalType>
	{
		public void TestProcessReceptionAcceptedMessage_GreenCircuit()
		{
			ProcessAndAssertAcceptedGreenCircuitDeclaration();
		}

		public void TestProcessReceptionAcceptedMessage_OrangeCircuit()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceOrangeCircuitTestFile(), InterchangeID, DeclarationMessageTypeList.Codes.T2lReceptionPous);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>06-07-2023</td></tr></table>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>" + MRNCode + "</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>";
			AssertT2LPOUSDeclaration(message, entryStatusCode: EntryStatusCodes.CustomsDeclarationAccepted, circuit: CircuitCodeList.Codes.ORANGE, acceptanceDate: acceptanceDate, mrn: MRNCode2, expectedMessageInterpretation: expectedMessageInterpretationText);
		}

		public void TestProcessReceptionAcceptedMessage_RedCircuit()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceRedCircuitTestFile(), InterchangeID, DeclarationMessageTypeList.Codes.T2lReceptionPous);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>06-07-2023</td></tr></table>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>" + MRNCode + "</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F00000\">RED</font></strong></td></tr></table>";
			AssertT2LPOUSDeclaration(message, entryStatusCode: EntryStatusCodes.CustomsDeclarationAccepted, circuit: CircuitCodeList.Codes.RED, acceptanceDate: acceptanceDate, mrn: MRNCode2, expectedMessageInterpretation: expectedMessageInterpretationText);
		}

		public void TestProcessReceptionRejectedMessage()
		{
			var responseMessage = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetRejectedTestFile(), InterchangeID, DeclarationMessageTypeList.Codes.T2lReceptionPous);

			ProcessMessageForTest(responseMessage);
			var expectedMessageInterpretationTextRejected = "<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Code</strong></td><td><strong>Place</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>39</td><td>EP01/GoodsShipmentForT2LT2LF</td><td>El elemento GoodsShipmentForT2LT2LF es obligatorio</td><td>GoodsShipmentForT2LT2LF</td></tr>" +
				"</table>";
			AssertT2LPOUSDeclaration(responseMessage, entryStatusCode: OriginalEntryStatus, messageSubType: "REJ", mrn: MRNCode2, expectedMessageInterpretation: expectedMessageInterpretationTextRejected, emStatus: EDIMessage.Status.Rejected, sentMessageStatus: EDIMessage.Status.Rejected);
		}

		public void TestCreateDocumentCaptureRequestWhenCSVClearance_AllDocs()
		{
			ProcessAndAssertAcceptedGreenCircuitDeclaration();

			CombineAssertions(() =>
			{
				entryHeader.Messages.Reload(true);
				var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
				AssertEquals("Doc Messages sent number is", 1, docMessages.Length);

				AssertDocumentRequestEDIMessages(docMessages, new List<(ZString fileName, ZString urlParameter)>() { (MRNCode + "_I_AEAT_T2LR_CLR.pdf", CSVClearance) });
			});
		}

		public void TestCreateDocumentCaptureRequestWhenCSVClearance_AllDocs_NewCSVClearance()
		{
			entryHeader.SetCSVClearanceNum("AAAAAAAAAAAAAAAA");

			var docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
			docManagerInfo.AddFileOrDocument(new byte[1], MRNCode + "_I_AEAT_T2LR_CLR.pdf", "CLR");
			docManagerInfo.Save();

			Factory.Save();

			ProcessAndAssertAcceptedGreenCircuitDeclaration();

			CombineAssertions(() =>
			{
				entryHeader.Messages.Reload(true);
				AssertEquals("CSVClearance", CSVClearance, entryHeader.CSVClearance);

				docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
				var eDocs = docManagerInfo.GetRelatedEDocs();
				AssertEquals("Number of eDocs is correct", 1, eDocs.Count());

				var expectedeDocNames = new List<ZString>() { MRNCode + "_I_AEAT_T2LR_CLR_OLD_AAAAAAAAAAAAAAAA.pdf" };
				AssertContainsExactElementsInAnyOrder("eDocs contains all documents with original names", expectedeDocNames, eDocs.Cast<IeDoc>().Select(x => x.FileName).ToList());

				entryHeader.Messages.Reload(true);
				var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
				AssertEquals("Doc Messages sent number is", 1, docMessages.Length);

				AssertDocumentRequestEDIMessages(docMessages, new List<(ZString fileName, ZString urlParameter)>() { (MRNCode + "_I_AEAT_T2LR_CLR.pdf", CSVClearance) });
			});
		}

		public void TestCreateDocumentCaptureRequestWhenCSVClearance_NoDocs()
		{
			var docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
			docManagerInfo.AddFileOrDocument(new byte[1], MRNCode + "_I_AEAT_T2LR_CLR.pdf", "CLR");
			docManagerInfo.Save();

			ProcessAndAssertAcceptedGreenCircuitDeclaration();

			entryHeader.Messages.Reload(true);
			var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
			AssertEquals("Doc Messages sent number is", 0, docMessages.Length);
		}

		void ProcessAndAssertAcceptedGreenCircuitDeclaration()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceGreenCircuitTestFile(), InterchangeID, DeclarationMessageTypeList.Codes.T2lReceptionPous);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>06-07-2023</td></tr></table>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>" + MRNCode + "</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>379CJX9DEWWYLP4Z</td></tr>" +
				"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>06-07-2023</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>E339FPQGYGUFC3WG</td></tr></table>";
			AssertT2LPOUSDeclaration(message, entryStatusCode: EntryStatusCodes.Cleared, circuit: CircuitCodeList.Codes.GREEN, acceptanceDate: acceptanceDate, mrn: MRNCode2, csvClearance: CSVClearance, entryReleaseDate: acceptanceDate, expectedMessageInterpretation: expectedMessageInterpretationText);
		}

		protected void AssertT2LPOUSDeclaration(TestEdiMessage message, string entryStatusCode, string messageSubType = "ACC", string circuit = "", ZDateTime? acceptanceDate = null, string mrn = "", string csvClearance = "", ZDateTime? entryReleaseDate = null, string expectedMessageInterpretation = "", string emStatus = EDIMessage.Status.Received, string sentMessageStatus = EDIMessage.Status.Received)
		{
			GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, messageSubType: messageSubType, entryStatusCode: entryStatusCode, circuit: circuit, acceptanceDate: acceptanceDate, movementReferenceNumber: mrn, csvClearance: csvClearance, entryReleaseDate: entryReleaseDate, messageNum: MessageNum, emStatus: emStatus);
			AssertT2LAnnexAndSentMessage(message, sentMessageStatus);
			if (messageSubType == "ACC")
			{
				AssertEquals("T2CMovementReferenceNumber is update with value in response", MRNCode, entryHeader.T2CMovementReferenceNumber);
			}
		}

		void AssertT2LAnnexAndSentMessage(TestEdiMessage message, string emStatus)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Sent message status changed", emStatus, sentMessage.EM_Status);
				var pivotQuery = new ZQuery(GenPivotSchema.XX_RelationType, GenPivotTypes.CusStorageDocPivotEdiMessage);
				pivotQuery.AddToFilter(GenPivotSchema.XX_Relation2ID, message.PK);
				var genPivotResponseMessages = entryHeader.Factory.Load<GenPivot>(pivotQuery);
				AssertEquals("Received message is related to sent annexes through new GenPivot", 2, genPivotResponseMessages.Length);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var staffWithCertificateHelperTest = new StaffWithCertificateTestHelper(Factory);
			declaration.JE_GS_NKCusAgent = staffWithCertificateHelperTest.Staff.GS_Code;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;

			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryHeader.MovementReferenceNumber = MRNCode2;
			entryHeader.CH_EntryStatus = OriginalEntryStatus;
			entryHeader.ZG_POUSVersion = 1;

			sentMessage = AddSentMessageAndEDocs();

			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "Customs Status");

			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(EsCode, parent: grouping);
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, OriginalEntryStatus, "Original Entry Status", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CLR", "Customs Declaration Accepted and Cleared", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CDA", "Customs Declaration accepted", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			Factory.Save();
		}
		TestEdiMessage sentMessage;

		string GetAcceptanceGreenCircuitTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.T2LPOUSRequestAndReceptionTestFilePath, "AcceptedMessageGreenCircuit.txt");

		string GetAcceptanceOrangeCircuitTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.T2LPOUSRequestAndReceptionTestFilePath, "AcceptedMessageOrangeCircuit.txt");

		string GetAcceptanceRedCircuitTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.T2LPOUSRequestAndReceptionTestFilePath, "AcceptedMessageRedCircuit.txt");

		string GetRejectedTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.T2LPOUSRequestAndReceptionTestFilePath, "RejectedMessage.txt");

		protected override string GetAcceptanceTestFileWithLongSegmentId() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.T2LPOUSRequestAndReceptionTestFilePath, "AcceptedMessageWithLongSegmentId.txt");

		protected override ZString GetExpectedProcessorFriendlyName() => "T2L POUS Request and Reception Declaration Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.T2lRequestPous, DeclarationMessageTypeList.Codes.T2lReceptionPous };

		protected override RequestAndReceptionT2LPOUSResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new RequestAndReceptionT2LPOUSResponseMessageProcessor(logger, new BranchCustomsMessageProcessorForTest());

		const string MRNCode = "23ES009999L00026M6";
		const string MRNCode2 = "24ES00999912345678";
		protected const string CSVClearance = "379CJX9DEWWYLP4Z";
		protected const string MessageNum = "20221014115635534071";
		protected readonly ZDateTime acceptanceDate = new ZDateTime(2023, 07, 06, 11, 25, 49);
	}
}
