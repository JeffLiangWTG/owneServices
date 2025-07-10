using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LPOUS.CCIEP01V1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business.Testing
{
	class RequestT2LPOUSResponseMessageProcessorTest : T2LPOUSCommonResponseMessageProcessorTest<RequestAndReceptionT2LPOUSResponseMessageProcessor, IMessagePrettyFormatter, Iep01SalType>
	{
		public void TestProcessAcceptedMessage_GreenCircuit()
		{
			ProcessAndAssertAcceptedGreenCircuitDeclaration();
		}

		public void TestProcessAcceptedMessage_OrangeCircuit()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceOrangeCircuitTestFile(), InterchangeID, DeclarationMessageTypeList.Codes.T2lRequestPous);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>06-07-2023</td></tr></table>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>" + MRNCode + "</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>";
			AssertT2LPOUSDeclaration(message, entryStatusCode: EntryStatusCodes.CustomsDeclarationAccepted, circuit: CircuitCodeList.Codes.ORANGE, acceptanceDate: acceptanceDate, mrn: MRNCode, expectedMessageInterpretation: expectedMessageInterpretationText);
		}

		public void TestProcessAcceptedMessage_RedCircuit()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceRedCircuitTestFile(), InterchangeID, DeclarationMessageTypeList.Codes.T2lRequestPous);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>06-07-2023</td></tr></table>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>" + MRNCode + "</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F00000\">RED</font></strong></td></tr></table>";
			AssertT2LPOUSDeclaration(message, entryStatusCode: EntryStatusCodes.CustomsDeclarationAccepted, circuit: CircuitCodeList.Codes.RED, acceptanceDate: acceptanceDate, mrn: MRNCode, expectedMessageInterpretation: expectedMessageInterpretationText);
		}

		public void TestCreateDocumentCaptureRequestWhenCSVClearance_AllDocs()
		{
			ProcessAndAssertAcceptedGreenCircuitDeclaration();

			CombineAssertions(() =>
			{
				var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
				AssertEquals("Doc Messages sent number is", 1, docMessages.Length);

				AssertDocumentRequestEDIMessages(docMessages, new List<(ZString fileName, ZString urlParameter)>() { (MRNCode + "_E_AEAT_T2L_CLR.pdf", CSVClearance) });
			});
		}

		public void TestCreateDocumentCaptureRequestWhenCSVClearance_AllDocs_NewCSVClearance()
		{
			entryHeader.SetCSVClearanceNum("AAAAAAAAAAAAAAAA");

			var docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
			docManagerInfo.AddFileOrDocument(new byte[1], MRNCode + "_E_AEAT_T2L_CLR.pdf", "CLR");
			docManagerInfo.Save();

			Factory.Save();

			ProcessAndAssertAcceptedGreenCircuitDeclaration();

			CombineAssertions(() =>
			{
				AssertEquals("CSVClearance", CSVClearance, entryHeader.CSVClearance);

				docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
				var eDocs = docManagerInfo.GetRelatedEDocs();
				AssertEquals("Number of eDocs is correct", 1, eDocs.Count());

				var expectedeDocNames = new List<ZString>() { MRNCode + "_E_AEAT_T2L_CLR_OLD_AAAAAAAAAAAAAAAA.pdf" };
				AssertContainsExactElementsInAnyOrder("eDocs contains all documents with original names", expectedeDocNames, eDocs.Cast<IeDoc>().Select(x => x.FileName).ToList());

				entryHeader.Messages.Reload(true);
				var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
				AssertEquals("Doc Messages sent number is", 1, docMessages.Length);

				AssertDocumentRequestEDIMessages(docMessages, new List<(ZString fileName, ZString urlParameter)>() { (MRNCode + "_E_AEAT_T2L_CLR.pdf", CSVClearance) });
			});
		}

		public void TestCreateDocumentCaptureRequestWhenCSVClearance_NoDocs()
		{
			var docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
			docManagerInfo.AddFileOrDocument(new byte[1], MRNCode + "_E_AEAT_T2L_CLR.pdf", "CLR");
			docManagerInfo.Save();

			ProcessAndAssertAcceptedGreenCircuitDeclaration();

			var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
			AssertEquals("Doc Messages sent number is", 0, docMessages.Length);
		}

		void ProcessAndAssertAcceptedGreenCircuitDeclaration()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceGreenCircuitTestFile(), InterchangeID, DeclarationMessageTypeList.Codes.T2lRequestPous);

			ProcessMessageForTest(message, entryHeader.Messages);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>06-07-2023</td></tr></table>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>" + MRNCode + "</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>379CJX9DEWWYLP4Z</td></tr>" +
				"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>06-07-2023</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>E339FPQGYGUFC3WG</td></tr></table>";
			AssertT2LPOUSDeclaration(message, entryStatusCode: EntryStatusCodes.Cleared, circuit: CircuitCodeList.Codes.GREEN, acceptanceDate: acceptanceDate, mrn: MRNCode, csvClearance: CSVClearance, entryReleaseDate: acceptanceDate, expectedMessageInterpretation: expectedMessageInterpretationText);
		}

		string GetAcceptanceGreenCircuitTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.T2LPOUSRequestAndReceptionTestFilePath, "AcceptedMessageGreenCircuit.txt");

		string GetAcceptanceOrangeCircuitTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.T2LPOUSRequestAndReceptionTestFilePath, "AcceptedMessageOrangeCircuit.txt");

		string GetAcceptanceRedCircuitTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.T2LPOUSRequestAndReceptionTestFilePath, "AcceptedMessageRedCircuit.txt");

		protected override string GetRejectedTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.T2LPOUSRequestAndReceptionTestFilePath, "RejectedMessage.txt");

		protected override string GetAcceptanceTestFileWithLongSegmentId() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.T2LPOUSRequestAndReceptionTestFilePath, "AcceptedMessageWithLongSegmentId.txt");

		protected override ZString GetExpectedProcessorFriendlyName() => "T2L POUS Request and Reception Declaration Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.T2lRequestPous, DeclarationMessageTypeList.Codes.T2lReceptionPous };

		protected override RequestAndReceptionT2LPOUSResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new RequestAndReceptionT2LPOUSResponseMessageProcessor(logger, new BranchCustomsMessageProcessorForTest());
	}
}
