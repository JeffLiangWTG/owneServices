using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LPOUS.CCIEP01CONSV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Testing;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business.Testing
{
	class QueryT2LPOUSResponseMessageProcessorTest : T2LPOUSCommonResponseMessageProcessorTest<QueryT2LPOUSResponseMessageProcessor, IMessagePrettyFormatter, Iep01Cons>
	{
		public void TestProcessAcceptedMessage()
		{
			ProcessAndAssertAcceptedGreenCircuitDeclaration();
		}

		public void TestProcessAcceptedMessage_OrangeCircuit()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceOrangeCircuitTestFile(), InterchangeID);
			ProcessMessageForTest(message);

			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Register T2L (MRN):</td><td>&nbsp;&nbsp;</td><td>" + MRNCode + "</td></tr>" +
				"<tr><td>Register JEC (MRN):</td><td>&nbsp;&nbsp;</td><td>" + MRNCodeJec + "</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Request Date:</td><td>&nbsp;&nbsp;</td><td>" + acceptanceDate.ToCustomsFormatDateStringddMMyyyyWithDash() + "</td></tr>" +
				"<tr><td>Registration Date:</td><td>&nbsp;&nbsp;</td><td>" + RegistrationDate.ToCustomsFormatDateStringddMMyyyyWithDash() + "</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Proof Status:</td><td>&nbsp;&nbsp;</td><td>" + ProofStatus + "</td></tr></table>";

			AssertT2LQuery(message, expectedMessageInterpretation: expectedMessageInterpretationText, csvClearance: CSVClearance, entryStatus: EntryStatusCodes.CustomsDeclarationAccepted, circuit: CircuitCodeList.Codes.ORANGE);
		}

		public void TestProcessAcceptedMessage_RedCircuit()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceRedCircuitTestFile(), InterchangeID);
			ProcessMessageForTest(message);

			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Register T2L (MRN):</td><td>&nbsp;&nbsp;</td><td>" + MRNCode + "</td></tr>" +
				"<tr><td>Register JEC (MRN):</td><td>&nbsp;&nbsp;</td><td>" + MRNCodeJec + "</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F00000\">RED</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Request Date:</td><td>&nbsp;&nbsp;</td><td>" + acceptanceDate.ToCustomsFormatDateStringddMMyyyyWithDash() + "</td></tr>" +
				"<tr><td>Registration Date:</td><td>&nbsp;&nbsp;</td><td>" + RegistrationDate.ToCustomsFormatDateStringddMMyyyyWithDash() + "</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Proof Status:</td><td>&nbsp;&nbsp;</td><td>" + ProofStatus + "</td></tr></table>";

			AssertT2LQuery(message, expectedMessageInterpretation: expectedMessageInterpretationText, csvClearance: CSVClearance, entryStatus: EntryStatusCodes.CustomsDeclarationAccepted, circuit: CircuitCodeList.Codes.RED);
		}

		public void TestCreateDocumentCaptureRequestWhenCSVClearance_AllDocs()
		{
			ProcessAndAssertAcceptedGreenCircuitDeclaration();

			CombineAssertions(() =>
			{
				var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
				AssertEquals("Doc Messages sent number is", 1, docMessages.Length);

				AssertDocumentRequestEDIMessages(docMessages, new List<(ZString fileName, ZString urlParameter)>() { (MRNCode + "_I_AEAT_T2L_CLR.pdf", CSVClearance) });
			});
		}

		public void TestCreateDocumentCaptureRequestWhenCSVClearance_AllDocs_NewCSVClearance()
		{
			entryHeader.SetCSVClearanceNum("AAAAAAAAAAAAAAAA");

			var docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
			docManagerInfo.AddFileOrDocument(new byte[1], MRNCode + "_I_AEAT_T2L_CLR.pdf", "CLR");
			docManagerInfo.Save();

			Factory.Save();

			ProcessAndAssertAcceptedGreenCircuitDeclaration();

			CombineAssertions(() =>
			{
				AssertEquals("CSVClearance", CSVClearance, entryHeader.CSVClearance);

				docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
				var eDocs = docManagerInfo.GetRelatedEDocs();
				AssertEquals("Number of eDocs is correct", 1, eDocs.Count());

				var expectedeDocNames = new List<ZString>() { MRNCode + "_I_AEAT_T2L_CLR_OLD_AAAAAAAAAAAAAAAA.pdf" };
				AssertContainsExactElementsInAnyOrder("eDocs contains all documents with original names", expectedeDocNames, eDocs.Cast<IeDoc>().Select(x => x.FileName).ToList());

				entryHeader.Messages.Reload(true);
				var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
				AssertEquals("Doc Messages sent number is", 1, docMessages.Length);

				AssertDocumentRequestEDIMessages(docMessages, new List<(ZString fileName, ZString urlParameter)>() { (MRNCode + "_I_AEAT_T2L_CLR.pdf", CSVClearance) });
			});
		}

		public void TestCreateDocumentCaptureRequestWhenCSVClearance_NoDocs()
		{
			var docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
			docManagerInfo.AddFileOrDocument(new byte[1], MRNCode + "_I_AEAT_T2L_CLR.pdf", "CLR");
			docManagerInfo.Save();

			ProcessAndAssertAcceptedGreenCircuitDeclaration();

			var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
			AssertEquals("Doc Messages sent number is", 0, docMessages.Length);
		}

		void ProcessAndAssertAcceptedGreenCircuitDeclaration()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceGreenCircuitTestFile(), InterchangeID);
			ProcessMessageForTest(message, entryHeader.Messages);

			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Register T2L (MRN):</td><td>&nbsp;&nbsp;</td><td>" + MRNCode + "</td></tr>" +
				"<tr><td>Register JEC (MRN):</td><td>&nbsp;&nbsp;</td><td>" + MRNCodeJec + "</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Request Date:</td><td>&nbsp;&nbsp;</td><td>" + acceptanceDate.ToCustomsFormatDateStringddMMyyyyWithDash() + "</td></tr>" +
				"<tr><td>Registration Date:</td><td>&nbsp;&nbsp;</td><td>" + RegistrationDate.ToCustomsFormatDateStringddMMyyyyWithDash() + "</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Proof Status:</td><td>&nbsp;&nbsp;</td><td>" + ProofStatus + "</td></tr></table>";

			AssertT2LQuery(message, expectedMessageInterpretation: expectedMessageInterpretationText, csvClearance: CSVClearance, entryStatus: EntryStatusCodes.Cleared, circuit: CircuitCodeList.Codes.GREEN);
		}

		void AssertT2LQuery(TestEdiMessage message, string expectedMessageInterpretation = "", string entryStatus = null , string circuit = null, string csvClearance = null, string messageSubType = DeclarationMessageSubTypeList.Codes.AcceptedResponse)
		{
			AssertEquals("T2CMovementReferenceNumber is update with value in response", MRNCode, entryHeader.T2CMovementReferenceNumber);
			GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, circuit: circuit, messageNum: MessageNum, entryStatusCode: entryStatus, csvClearance: csvClearance, messageSubType: messageSubType);
		}

		string GetAcceptanceGreenCircuitTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.T2LPOUSQueryTestFilePath, "AcceptedMessageGreenCircuit.txt");

		string GetAcceptanceOrangeCircuitTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.T2LPOUSQueryTestFilePath, "AcceptedMessageOrangeCircuit.txt");

		string GetAcceptanceRedCircuitTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.T2LPOUSQueryTestFilePath, "AcceptedMessageRedCircuit.txt");

		protected override string GetRejectedTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.T2LPOUSQueryTestFilePath, "RejectedMessage.txt");

		protected override string GetAcceptanceTestFileWithLongSegmentId() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.T2LPOUSQueryTestFilePath, "AcceptedMessageWithLongSegmentId.txt");

		protected override ZString GetExpectedProcessorFriendlyName() => "T2L POUS Query Declaration Message Processor";

		protected override ZString CHStatusWhenWrongXMLOrMessageTextEmpty => "FAL";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.T2lQueryPous };

		protected override QueryT2LPOUSResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new QueryT2LPOUSResponseMessageProcessor(logger, new BranchCustomsMessageProcessorForTest());

		protected const string MRNCodeJec = "23ES009999L00026M6";
		protected readonly ZDateTime RegistrationDate = new ZDateTime(2023, 12, 27, 10, 07, 49);
		protected const string ProofStatus = "Saldado";
	}
}
