using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CC513C_v514.CC513CV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class AESAmendmentResponseMessageProcessorTest : AESCommonResponseMessageProcessorTest<AESAmendmentResponseMessageProcessor, Cc513Cv1Sal>
	{
		public void TestProcessAcceptedMessage()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>20ES00999930006184</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>FE8NHU8HTA44GPGE</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>DE - Indirect Dispatch</td></tr></table>";
			AssertExportAmendment(message, messageSubType: "ACC", expectedMessageInterpretation: expectedMessageInterpretationText);
		}

		public void TestProcessAcceptedMessageCreatesEntryLineSupportingDocuments()
		{
			var suppDoc1 = newDeclaration.SupportingDocuments.AddNew();
			suppDoc1.CSI_Code = "X001";
			suppDoc1.CSI_ReferenceNumber = "ES3600000001";

			var suppDoc11 = newDeclaration.SupportingDocuments.AddNew();
			suppDoc11.CSI_Code = "X001";
			suppDoc11.CSI_ReferenceNumber = "es3600000001";

			var suppDoc2 = invoiceLine.SupportingDocuments.AddNew();
			suppDoc2.CSI_Code = "X002";
			suppDoc2.CSI_ReferenceNumber = "ES3600000002";

			var suppDoc3 = invoiceLine.SupportingDocuments.AddNew();
			suppDoc3.CSI_Code = "X003";
			suppDoc3.CSI_ReferenceNumber = "ES3600000003";

			var suppDoc31 = invoiceLine.SupportingDocuments.AddNew();
			suppDoc31.CSI_Code = "X003";
			suppDoc31.CSI_ReferenceNumber = "es3600000003";

			var suppDoc4 = invoiceLine.SupportingDocuments.AddNew();
			suppDoc4.CSI_Code = "N380";
			suppDoc4.CSI_ReferenceNumber = "ES36000N3801";
			suppDoc4.CSI_Status = ZString.Empty;

			entryLine.AddEntryLineDocument<SupportingDocument>("X004", "ES3600000004");
			entryLine.AddEntryLineDocument<SupportingDocument>("X005", "ES3600000005");
			entryLine.AddEntryLineDocument<SupportingDocument>("X006", "ES3600000006", subType: "LIQ", status: ZString.Empty);
			entryLine.Header.AddEntryHeaderDocument<SupportingDocument>("AAA", "ES3600000025");
			entryLine.Header.AddEntryHeaderDocument<SupportingDocument>("5018", "ES3600000022", subType: "LIQ", status: ZString.Empty);
			entryLine.Header.AddEntryHeaderDocument<SupportingDocument>("BBB", "ES3600000023", subType: "LIQ", status: "REJ");

			CombineAssertions("Before", () =>
			{
				var clSupDocs = GetCLSupportingDocuments();
				AssertEquals("There are 3 CL SupportingDocuments before calling ProcessExportEntryLineSupportingDocuments", 3, clSupDocs.Length);
				AssertContainsExactElementsInAnyOrder("The 3 CL SupportingDocuments before calling ProcessExportEntryLineSupportingDocuments have the correct CSI_ReferenceNumber", new ZString[] { "ES3600000004", "ES3600000005", "ES3600000006" }, clSupDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
				AssertEquals("The 3 EntryLine SupportingDocuments before calling ProcessExportEntryLineSupportingDocuments have the correct CSI_Status", false, clSupDocs.Any(x => x.CSI_Status == "ACC"));

				var chSupDocs = GetCHSupportingDocuments();
				AssertEquals("There are 3 CH SupportingDocuments before calling ProcessExportEntryLineSupportingDocuments", 3, chSupDocs.Length);
				AssertContainsExactElementsInAnyOrder("The 3 CH SupportingDocuments before calling ProcessExportEntryLineSupportingDocuments have the correct CSI_ReferenceNumber", new ZString[] { "ES3600000025", "ES3600000022", "ES3600000023" }, chSupDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
				AssertEquals("The 3 EntryHeader SupportingDocuments before calling ProcessExportEntryLineSupportingDocuments have the correct CSI_Status", false, chSupDocs.Any(x => x.CSI_Status == "ACC"));
			});

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);
			ProcessMessageForTest(message);

			CombineAssertions("For CL", () =>
			{
				var clSupDocs = GetCLSupportingDocuments();
				AssertEquals("There are 5 CL SupportingDocuments after calling ProcessExportEntryLineSupportingDocuments", 5, clSupDocs.Length);
				AssertContainsExactElementsInAnyOrder("The 5 CL SupportingDocuments after calling ProcessExportEntryLineSupportingDocuments have the correct CSI_ReferenceNumber", new ZString[] { "ES3600000001", "ES3600000002", "ES3600000003", "ES36000N3801", "ES3600000006" }, clSupDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
				AssertEquals("The 5 EntryLine SupportingDocuments after calling ProcessExportEntryLineSupportingDocuments have the correct CSI_Status", false, clSupDocs.Any(x => x.CSI_Status != "ACC"));
			});
			CombineAssertions("For CH", () =>
			{
				var chSupDocs = GetCHSupportingDocuments();
				AssertEquals("There are 2 CH SupportingDocuments after calling ProcessExportEntryLineSupportingDocuments", 2, chSupDocs.Length);
				AssertContainsExactElementsInAnyOrder("The 2 CH SupportingDocuments after calling ProcessExportEntryLineSupportingDocuments have the correct CSI_ReferenceNumber", new ZString[] { "ES3600000022", "ES3600000023" }, chSupDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
				AssertEquals("The 2 EntryHeader SupportingDocuments after calling ProcessExportEntryLineSupportingDocuments have the correct CSI_Status", false, chSupDocs.Any(x => x.CSI_Status != "ACC"));
			});
		}

		public void TestProcessAcceptedMessageCreatesEntryLinePreviousDocuments()
		{
			var prevDoc1 = invoiceLine.PreviousDocuments.AddNew();
			prevDoc1.CSI_Code = "X001";
			prevDoc1.CSI_ReferenceNumber = "ES3600000001";

			var prevDoc2 = invoiceLine.PreviousDocuments.AddNew();
			prevDoc2.CSI_Code = "X002";
			prevDoc2.CSI_ReferenceNumber = "ES3600000002";

			var prevDoc3 = invoiceLine.PreviousDocuments.AddNew();
			prevDoc3.CSI_Code = "X003";
			prevDoc3.CSI_ReferenceNumber = "ES3600000003";

			var prevDoc4 = invoiceLine.PreviousDocuments.AddNew();
			prevDoc4.CSI_Code = "N380";
			prevDoc4.CSI_ReferenceNumber = "ES36000N3801";
			prevDoc4.CSI_Status = ZString.Empty;

			entryLine.AddEntryLineDocument<PreviousDocument>("X004", "ES3600000004");
			entryLine.AddEntryLineDocument<PreviousDocument>("X005", "ES3600000005");

			CombineAssertions("Before", () =>
			{
				var clPrevDocs = GetCLPreviousDocuments();
				AssertEquals("There are 2 CL PreviousDocuments before calling ProcessExportEntryLinePreviousDocuments", 2, clPrevDocs.Length);
				AssertContainsExactElementsInAnyOrder("The 2 CL PreviousDocuments before calling ProcessExportEntryLinePreviousDocuments have the correct CSI_ReferenceNumber", new ZString[] { "ES3600000004", "ES3600000005" }, clPrevDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
				AssertEquals("The 2 EntryLine PreviousDocuments before calling ProcessExportEntryLinePreviousDocuments have the correct CSI_Status", false, clPrevDocs.Any(x => x.CSI_Status == "ACC"));
			});

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			Factory.Save();

			ProcessMessageForTest(message);

			CombineAssertions("After", () =>
			{
				var clPrevDocs = GetCLPreviousDocuments();
				AssertEquals("There are 4 CL PreviousDocuments after calling ProcessExportEntryLinePreviousDocuments", 4, clPrevDocs.Length);
				AssertContainsExactElementsInAnyOrder("The 4 CL PreviousDocuments after calling ProcessExportEntryLinePreviousDocuments have the correct CSI_ReferenceNumber", new ZString[] { "ES3600000001", "ES3600000002", "ES3600000003", "ES36000N3801" }, clPrevDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
				AssertEquals("The 4 EntryLine PreviousDocuments after calling ProcessExportEntryLinePreviousDocuments have the correct CSI_Status", false, clPrevDocs.Any(x => x.CSI_Status != "ACC"));
			});
		}

		public void TestProcessAcceptedMessageCreatesEntryLineAdditionalInfos()
		{
			var prevDoc1 = invoiceLine.AdditionalInfos.AddNew();
			prevDoc1.CSI_Code = "X001";
			prevDoc1.CSI_ReferenceNumber = "ES3600000001";

			var prevDoc2 = invoiceLine.AdditionalInfos.AddNew();
			prevDoc2.CSI_Code = "X002";
			prevDoc2.CSI_ReferenceNumber = "ES3600000002";

			var prevDoc3 = invoiceLine.AdditionalInfos.AddNew();
			prevDoc3.CSI_Code = "X003";
			prevDoc3.CSI_ReferenceNumber = "ES3600000003";

			var prevDoc4 = invoiceLine.AdditionalInfos.AddNew();
			prevDoc4.CSI_Code = "N380";
			prevDoc4.CSI_ReferenceNumber = "ES36000N3801";
			prevDoc4.CSI_Status = ZString.Empty;

			entryLine.AddEntryLineDocument<AdditionalInfo>("X004", "ES3600000004");
			entryLine.AddEntryLineDocument<AdditionalInfo>("X005", "ES3600000005");

			CombineAssertions("Before", () =>
			{
				var clAddInfos = GetCLAdditionalInfos();
				AssertEquals("There are 2 CL AdditionalInfos before calling ProcessExportEntryLineAdditionalInfos", 2, clAddInfos.Length);
				AssertContainsExactElementsInAnyOrder("The 2 CL AdditionalInfos before calling ProcessExportEntryLineAdditionalInfos have the correct CSI_ReferenceNumber", new ZString[] { "ES3600000004", "ES3600000005" }, clAddInfos.Select(x => x.CSI_ReferenceNumber).ToArray());
				AssertEquals("The 2 EntryLine AdditionalInfos before calling ProcessExportEntryLineAdditionalInfos have the correct CSI_Status", false, clAddInfos.Any(x => x.CSI_Status == "ACC"));
			});

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID, serializeMessageText: false);

			ProcessMessageForTest(message);

			CombineAssertions("After", () =>
			{
				var clAddInfos = GetCLAdditionalInfos();
				AssertEquals("There are 4 CL AdditionalInfos after calling ProcessExportEntryLineAdditionalInfos", 4, clAddInfos.Length);
				AssertContainsExactElementsInAnyOrder("The 4 CL AdditionalInfos after calling ProcessExportEntryLineAdditionalInfos have the correct CSI_ReferenceNumber", new ZString[] { "ES3600000001", "ES3600000002", "ES3600000003", "ES36000N3801" }, clAddInfos.Select(x => x.CSI_ReferenceNumber).ToArray());
				AssertEquals("The 4 EntryLine AdditionalInfos after calling ProcessExportEntryLineAdditionalInfos have the correct CSI_Status", false, clAddInfos.Any(x => x.CSI_Status != "ACC"));
			});
		}

		void AssertExportAmendment(TestEdiMessage message, string messageSubType, string expectedMessageInterpretation = "", string movementReferenceNumber = MRNCode)
		{
			GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, messageSubType: messageSubType, movementReferenceNumber: movementReferenceNumber, entryStatusCode: OriginalEntryStatus, messageNum: MessageNum);
		}

		string GetAcceptanceTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.AmendmentAESTestFilePath, "AcceptedAndClearedMessage.txt");
		protected override string GetRejectedTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.AmendmentAESTestFilePath, "RejectedMessage.txt");
		protected override string GetErrorTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.AmendmentAESTestFilePath, "ErrorMessage.txt");
		protected override string GetAcceptanceTestFileWithLongSegmentId() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.AmendmentAESTestFilePath, "AcceptedMessageWithLongSegmentId.txt");

		protected override ZString RejectedMRN => ZString.Empty;

		protected override ZDateTime RejectedAcceptanceDate => ZDateTime.Empty;

		protected override ZString RejectedMessageStatus => EDIMessage.Status.Rejected;

		protected override ZString ErrorMRN => ZString.Empty;

		protected override ZDateTime ErrorAcceptanceDate => ZDateTime.Empty;

		protected override ZString ErrorMessageInterpretation => "<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Line / Column</strong></td><td><strong>Location</strong></td><td><strong>Code</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>14 / 34</td><td>782</td><td>18</td><td>Se esperaba nodo {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CC513CV1Ent.xsd}security y ha venido {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CC513CV1Ent.xsd}totalAmountInvoiced</td><td>Wrong value</td></tr>" +
				"</table>";
		protected override ZString GetExpectedProcessorFriendlyName() => "Export Amendment Declaration Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.ExportAmendmentUcc6 };

		protected override AESAmendmentResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new AESAmendmentResponseMessageProcessor(logger);
	}
}
