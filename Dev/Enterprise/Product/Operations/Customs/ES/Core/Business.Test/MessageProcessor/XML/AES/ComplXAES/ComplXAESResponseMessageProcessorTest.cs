using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CC515X_v514.CC515XV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.EU.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class ComplXAESResponseMessageProcessorTest : AESCommonResponseMessageProcessorTest<ComplXAESResponseMessageProcessor, Cc515Xv1Sal>
	{
		public void TestProcessAcceptedMessage()
		{
			var responseMessage = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(responseMessage);
			var expectedMessageInterpretationTextAccepted = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>20ES00999930006184</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td>Complementary</td></tr></table>" +
				"<table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>5XP9J5VAV2KCV2VR</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>DE - Indirect Dispatch</td></tr></table>" +
				"<table border=\"0\"><tr><td>Complementary In Term</td></tr></table>";

			AssertExportComplXDeclaration(responseMessage, entryStatusCode: EntryStatusCodes.Cleared, messageSubType: "ACC", expectedMessageInterpretation: expectedMessageInterpretationTextAccepted);
		}

		void AssertExportComplXDeclaration(TestEdiMessage message, string entryStatusCode, string messageSubType, string circuit = "", string circuitCan = "", ZDateTime? acceptanceDate = null, string csvClearance = "", ZDateTime? entryReleaseDate = null, string csvT2L = "", string expectedMessageInterpretation = "", string movementReferenceNumber = MRNCode)
		{
			GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, messageSubType: messageSubType, entryStatusCode: entryStatusCode, circuit: circuit, circuitCan: circuitCan, acceptanceDate: acceptanceDate, movementReferenceNumber: movementReferenceNumber, csvClearance: csvClearance, entryReleaseDate: entryReleaseDate, csvT2L: csvT2L, messageNum: MessageNum);
		}

		public void TestProcessAcceptedMessageCreatesEntryLineSupportingDocuments()
		{
			CreateSupportingDocument("X001", "ES3600000001");
			CreateSupportingDocument("X002", "ES3600000002");
			CreateSupportingDocument("X003", "ES3600000003");
			CreateSupportingDocument("N380", "ES36000N3801");

			entryLine.AddEntryLineDocument<SupportingDocument>("X004", "ES3600000004");
			entryLine.AddEntryLineDocument<SupportingDocument>("X005", "ES3600000005");

			CombineAssertions(() =>
			{
				var clSupDocs = GetCLSupportingDocuments();
				AssertEquals("There are 2 CL SupportingDocuments before calling ProcessComplXExportEntryLineSupportingDocuments", 2, clSupDocs.Length);
				AssertContainsExactElementsInAnyOrder("The 2 CL SupportingDocuments before calling ProcessComplXExportEntryLineSupportingDocuments have the correct CSI_ReferenceNumber", new ZString[] { "ES3600000004", "ES3600000005" }, clSupDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
				AssertEquals("The 2 EntryLine SupportingDocuments before calling ProcessComplXExportEntryLineSupportingDocuments have the correct CSI_Status", false, clSupDocs.Any(x => x.CSI_Status == "ACC"));

				var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);
				ProcessMessageForTest(message);

				clSupDocs = GetCLSupportingDocuments();
				AssertEquals("There are 3 CL SupportingDocuments after calling ProcessComplXExportEntryLineSupportingDocuments", 3, clSupDocs.Length);
				AssertContainsExactElementsInAnyOrder("The 3 CL SupportingDocuments after calling ProcessComplXExportEntryLineSupportingDocuments have the correct CSI_ReferenceNumber", new ZString[] { "ES3600000004", "ES3600000005", "ES36000N3801" }, clSupDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
				AssertEquals("The 3 EntryLine SupportingDocuments after calling ProcessComplXExportEntryLineSupportingDocuments have the correct CSI_Status", 3, clSupDocs.Select(x => x.CSI_Status == "ACC").Count());
			});
		}

		public void TestProcessAcceptedMessageCreatesEntryLinePreviousDocuments()
		{
			CreatePreviousDocument("X001", "ES3600000001");
			CreatePreviousDocument("X002", "ES3600000002");
			CreatePreviousDocument("X003", "ES3600000003");
			CreatePreviousDocument("C651", "ESC651000001");
			CreatePreviousDocument("C651", "ESC651000002");
			CreatePreviousDocument("C651", "ESC651000001");

			CombineAssertions(() =>
			{
				var clPrevDocs = GetCLPreviousDocuments();
				AssertEquals("There are 0 CL PreviousDocuments before calling ProcessComplXExportEntryLinePreviousDocuments", 0, clPrevDocs.Length);

				var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);
				ProcessMessageForTest(message);

				clPrevDocs = GetCLPreviousDocuments();
				AssertEquals("There are 2 CL PreviousDocuments after calling ProcessComplXExportEntryLinePreviousDocuments", 2, clPrevDocs.Length);
				AssertContainsExactElementsInAnyOrder("The 2 CL PreviousDocuments after calling ProcessComplXExportEntryLinePreviousDocuments have the correct CSI_ReferenceNumber", new ZString[] { "ESC651000001", "ESC651000002" }, clPrevDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
				AssertEquals("The 2 EntryLine PreviousDocuments after calling ProcessComplXExportEntryLinePreviousDocuments have the correct CSI_Status", 2, clPrevDocs.Select(x => x.CSI_Status == "ACC").Count());
			});
		}

		public void TestProcessAcceptedMessageCreatesEntryLinePreviousDocumentsIfExistCL()
		{
			entryLine.AddEntryLineDocument<PreviousDocument>("C651", "ESC651000001", status: "BBB");
			CreatePreviousDocument("C651", "ESC651000002");
			CombineAssertions(() =>
			{
				var clPrevDocs = GetCLPreviousDocuments();
				AssertEquals("There are 1 CL PreviousDocuments before calling ProcessComplXExportEntryLinePreviousDocuments", 1, clPrevDocs.Length);

				var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);
				ProcessMessageForTest(message);

				clPrevDocs = GetCLPreviousDocuments();
				AssertEquals("There are 1 CL PreviousDocuments after calling ProcessComplXExportEntryLinePreviousDocuments", 1, clPrevDocs.Length);
				AssertContainsExactElementsInAnyOrder("The 1 CL PreviousDocuments after calling ProcessComplXExportEntryLinePreviousDocuments have the correct CSI_ReferenceNumber", new ZString[] { "ESC651000001" }, clPrevDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
				AssertEquals("The 1 EntryLine PreviousDocuments after calling ProcessComplXExportEntryLinePreviousDocuments have the correct CSI_Status", 1, clPrevDocs.Select(x => x.CSI_Status == "BBB").Count());
			});
		}

		public void TestProcessAcceptedMessageCreatesEntryLinePreviousDocumentsIfNotExistCL()
		{
			CreatePreviousDocument("C651", "ESC651000001");
			CombineAssertions(() =>
			{
				var clPrevDocs = GetCLPreviousDocuments();
				AssertEquals("There are 0 CL PreviousDocuments before calling ProcessComplXExportEntryLinePreviousDocuments", 0, clPrevDocs.Length);

				var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);
				ProcessMessageForTest(message);

				clPrevDocs = GetCLPreviousDocuments();
				AssertEquals("There are 1 CL PreviousDocuments after calling ProcessComplXExportEntryLinePreviousDocuments", 1, clPrevDocs.Length);
				AssertContainsExactElementsInAnyOrder("The 1 CL PreviousDocuments after calling ProcessComplXExportEntryLinePreviousDocuments have the correct CSI_ReferenceNumber", new ZString[] { "ESC651000001" }, clPrevDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
				AssertEquals("The 1 EntryLine PreviousDocuments after calling ProcessComplXExportEntryLinePreviousDocuments have the correct CSI_Status", 1, clPrevDocs.Select(x => x.CSI_Status == "ACC").Count());
			});
		}

		public void TestProcessAcceptedMessageCreatesEntryLinePreviousDocumentsWithOfficePresentation()
		{
			CreatePreviousDocument("C651", "ESC651000001");
			CombineAssertions(() =>
			{
				var clPrevDocs = GetCLPreviousDocuments();
				AssertEquals("There are 0 CL PreviousDocuments before calling ProcessComplXExportEntryLinePreviousDocuments", 0, clPrevDocs.Length);

				var customOffice = newDeclaration.CustomsOffices.AddNew();
				customOffice.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfPresentation;

				var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);
				ProcessMessageForTest(message);

				clPrevDocs = GetCLPreviousDocuments();
				AssertEquals("There are 0 CL PreviousDocuments after calling ProcessComplXExportEntryLinePreviousDocuments", 0, clPrevDocs.Length);
			});
		}

		public void TestProcessAcceptedMessageCreatesEntryLinePreviousDocumentsWithoutOfficePresentation()
		{
			CreatePreviousDocument("C651", "ESC651000001");
			CombineAssertions(() =>
			{
				var clPrevDocs = GetCLPreviousDocuments();
				AssertEquals("There are 0 CL PreviousDocuments before calling ProcessComplXExportEntryLinePreviousDocuments", 0, clPrevDocs.Length);

				var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);
				ProcessMessageForTest(message);

				clPrevDocs = GetCLPreviousDocuments();
				AssertEquals("There are 1 CL PreviousDocuments after calling ProcessComplXExportEntryLinePreviousDocuments", 1, clPrevDocs.Length);
				AssertContainsExactElementsInAnyOrder("The 1 CL PreviousDocuments after calling ProcessComplXExportEntryLinePreviousDocuments have the correct CSI_ReferenceNumber", new ZString[] { "ESC651000001" }, clPrevDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
				AssertEquals("The 1 EntryLine PreviousDocuments after calling ProcessComplXExportEntryLinePreviousDocuments have the correct CSI_Status", 1, clPrevDocs.Select(x => x.CSI_Status == "ACC").Count());
			});
		}

		public void TestProcessMessageEntryInstructionSubStyle()
		{
			CombineAssertions("When EDX message type is accepted, update the sub style value in the related entry instruction from B to X", () =>
			{
				AssertEntryInstruction("B", "X");
				AssertEntryInstruction("A", "A");
			});

			void AssertEntryInstruction(ZString initialSubStyle, ZString expectedSubStyle)
			{
				instruction.CEI_SubStyle = initialSubStyle;
				instruction.Factory.Save();

				var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

				ProcessMessageForTest(message);

				AssertEquals($"Entry Instruction SubStyle '{initialSubStyle}'", expectedSubStyle, instruction.CEI_SubStyle);
			}
		}

		PreviousDocument CreatePreviousDocument(ZString code, ZString reference)
		{
			var prevDoc = invoiceLine.PreviousDocuments.AddNew();
			prevDoc.CSI_Code = code;
			prevDoc.CSI_ReferenceNumber = reference;
			prevDoc.CSI_Status = ZString.Empty;

			return prevDoc;
		}

		SupportingDocument CreateSupportingDocument(ZString code, ZString reference)
		{
			var suppDoc = invoiceLine.SupportingDocuments.AddNew();
			suppDoc.CSI_Code = code;
			suppDoc.CSI_ReferenceNumber = reference;
			suppDoc.CSI_Status = ZString.Empty;

			return suppDoc;
		}

		protected override ZString GetExpectedProcessorFriendlyName() => "Export Type X Declaration Message Processor";

		protected override ZString RejectedMRN => ZString.Empty;
		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.TypeXExportUcc6 };

		protected override ZDateTime RejectedAcceptanceDate => ZDateTime.Empty;

		protected override ZString RejectedMessageStatus => EDIMessage.Status.Rejected;

		protected override ZString ErrorMRN => ZString.Empty;

		protected override ZDateTime ErrorAcceptanceDate => ZDateTime.Empty;

		protected override ZString ErrorMessageInterpretation => "<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Line / Column</strong></td><td><strong>Location</strong></td><td><strong>Code</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>14 / 34</td><td>782</td><td>18</td><td>Se esperaba nodo {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CC515XV1Ent.xsd}security y ha venido {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CC515XV1Ent.xsd}totalAmountInvoiced</td><td>&nbsp;</td></tr>" +
				"</table>";

		protected override ComplXAESResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new ComplXAESResponseMessageProcessor(logger, new BranchCustomsMessageProcessorForTest());

		protected override string GetRejectedTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ComplXAESTestFilePath, "RejectedMessage.txt");

		protected override string GetErrorTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ComplXAESTestFilePath, "ErrorMessage.txt");
		protected override string GetAcceptanceTestFileWithLongSegmentId() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ComplXAESTestFilePath, "AcceptedMessageWithLongSegmentId.txt");

		string GetAcceptanceTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ComplXAESTestFilePath, "AcceptedMessage.txt");
	}
}
