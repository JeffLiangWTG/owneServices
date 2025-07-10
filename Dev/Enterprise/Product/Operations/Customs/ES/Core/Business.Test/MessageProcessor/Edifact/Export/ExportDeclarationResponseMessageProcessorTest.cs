using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Moq;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class ExportDeclarationResponseMessageProcessorTest : ExportGenericResponseMessageProcessorTest<ExportDeclarationResponseMessageProcessor>
	{
		protected override ZString GetExpectedProcessorFriendlyName() => "Export Declaration Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.Export, DeclarationMessageTypeList.Codes.ExportAmendment };

		protected override ExportDeclarationResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new ExportDeclarationResponseMessageProcessor(logger, new BranchCustomsMessageProcessorForTest());

		public void TestProcessAcceptedMessageCreatesEntryLineSupportingDocuments()
		{
			var suppDoc1 = declaration.SupportingDocuments.AddNew();
			suppDoc1.CSI_Code = "X001";
			suppDoc1.CSI_ReferenceNumber = "ES3600000001";

			var suppDoc2 = instruction.SupportingDocuments.AddNew();
			suppDoc2.CSI_Code = "X002";
			suppDoc2.CSI_ReferenceNumber = "ES3600000002";

			var suppDoc3 = invoiceLine.SupportingDocuments.AddNew();
			suppDoc3.CSI_Code = "X003";
			suppDoc3.CSI_ReferenceNumber = "ES3600000003";

			var suppDoc4 = invoiceLine.SupportingDocuments.AddNew();
			suppDoc4.CSI_Code = "N380";
			suppDoc4.CSI_ReferenceNumber = "ES36000N3801";
			suppDoc4.CSI_Status = ZString.Empty;

			entryLine.AddEntryLineDocument<SupportingDocument>("X004", "ES3600000004");
			entryLine.AddEntryLineDocument<SupportingDocument>("X005", "ES3600000005");
			entryLine.AddEntryLineDocument<SupportingDocument>("X006", "ES3600000006", subType: "LIQ", status: ZString.Empty);

			Factory.Save();

			processor = GetMockedProcessor(GetMockedProvider("962", entryHeader.CH_BGMReference, MrnCode, acceptanceDate, "4", ZString.Empty, "A2", "1", limitDateOfArrival, CSVLevante, entryReleaseDate, CSVT2L));

			processor.ProcessMessage(message);
			CommonSendAssertGreenCircuitAccepted(message, EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, "1");

			CombineAssertions(() =>
			{
				var clSupDocs = GetCLSupportingDocuments();
				AssertEquals("There are 5 CL SupportingDocuments after calling SaveSupportingDocumentsForCL", 5, clSupDocs.Length);
				AssertContainsExactElementsInAnyOrder("The 5 CL SupportingDocuments after calling SaveSupportingDocumentsForCL have the correct CSI_ReferenceNumber", new ZString[] { "ES3600000001", "ES3600000002", "ES3600000003", "ES36000N3801", "ES3600000006" }, clSupDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
				AssertEquals("The 5 EntryLine SupportingDocuments after calling SaveSupportingDocumentsForEntryLine have the correct CSI_Status", false, clSupDocs.Any(x => x.CSI_Status != "ACC"));
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

			Factory.Save();

			CombineAssertions("Before", () =>
			{
				var clPrevDocs = GetCLPreviousDocuments();
				AssertEquals("There are 2 CL PreviousDocuments before calling ProcessExportEntryLinePreviousDocuments", 2, clPrevDocs.Length);
				AssertContainsExactElementsInAnyOrder("The 2 CL PreviousDocuments before calling ProcessExportEntryLinePreviousDocuments have the correct CSI_ReferenceNumber", new ZString[] { "ES3600000004", "ES3600000005" }, clPrevDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
				AssertEquals("The 2 EntryLine PreviousDocuments before calling ProcessExportEntryLinePreviousDocuments have the correct CSI_Status", false, clPrevDocs.Any(x => x.CSI_Status == "ACC"));
			});

			processor = GetMockedProcessor(GetMockedProvider("962", entryHeader.CH_BGMReference, MrnCode, acceptanceDate, "4", ZString.Empty, "A2", "1", limitDateOfArrival, CSVLevante, entryReleaseDate, CSVT2L));

			processor.ProcessMessage(message);
			CommonSendAssertGreenCircuitAccepted(message, EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, "1");

			CombineAssertions("After", () =>
			{
				var clPrevDocs = GetCLPreviousDocuments();
				AssertEquals("There are 4 CL PreviousDocuments after calling ProcessExportEntryLinePreviousDocuments", 4, clPrevDocs.Length);
				AssertContainsExactElementsInAnyOrder("The 4 CL PreviousDocuments after calling ProcessExportEntryLinePreviousDocuments have the correct CSI_ReferenceNumber", new ZString[] { "ES3600000001", "ES3600000002", "ES3600000003", "ES36000N3801" }, clPrevDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
				AssertEquals("The 4 EntryLine PreviousDocuments after calling ProcessExportEntryLinePreviousDocuments have the correct CSI_Status", false, clPrevDocs.Any(x => x.CSI_Status != "ACC"));
			});
		}

		protected override ExportDeclarationResponseMessageProcessor GetMockedProcessor(ICUSRESMessageProvider messageProvider)
		{
			var exportGenericResponseMessageProcessorMock = new Mock<ExportDeclarationResponseMessageProcessorForMockTest>(new object[] { logger, new BranchCustomsMessageProcessorForTest() });
			exportGenericResponseMessageProcessorMock.CallBase = true;
			exportGenericResponseMessageProcessorMock.Setup(c => c.GetMessageProviderForTest).Returns((IExportResponseMessageProvider)messageProvider);
			return exportGenericResponseMessageProcessorMock.Object;
		}

		public class ExportDeclarationResponseMessageProcessorForMockTest : ExportDeclarationResponseMessageProcessor
		{
			public ExportDeclarationResponseMessageProcessorForMockTest(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver = null) : base(logger, eDocsSaver)
			{
			}

			public virtual IExportResponseMessageProvider GetMessageProviderForTest
			{
				get;
			}
			protected override IExportResponseMessageProvider GetMessageProviderCore(EDIMessage message) => GetMessageProviderForTest;
		}
	}
}
