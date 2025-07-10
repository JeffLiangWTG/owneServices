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
	public class ComplXExportDeclarationResponseMessageProcessorTest : ExportGenericResponseMessageProcessorTest<ComplXExportDeclarationResponseMessageProcessor>
	{
		protected override ZString GetExpectedProcessorFriendlyName() => "TypeXExport Declaration Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.TypeXExport };

		protected override ComplXExportDeclarationResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new ComplXExportDeclarationResponseMessageProcessor(logger, new BranchCustomsMessageProcessorForTest());

		public void TestProcessAcceptedMessageCreatesEntryLineSupportingDocuments()
		{
			var suppDoc1 = declaration.SupportingDocuments.AddNew();
			suppDoc1.CSI_Code = "N380";
			suppDoc1.CSI_ReferenceNumber = "ES3600000001";

			var suppDoc2 = instruction.SupportingDocuments.AddNew();
			suppDoc2.CSI_Code = "N380";
			suppDoc2.CSI_ReferenceNumber = "ES3600000002";

			var suppDoc3 = invoiceLine.SupportingDocuments.AddNew();
			suppDoc3.CSI_Code = "N380";
			suppDoc3.CSI_ReferenceNumber = "ES3600000003";

			entryLine.AddEntryLineDocument<SupportingDocument>("N380", "ES3600000003");
			entryLine.AddEntryLineDocument<SupportingDocument>("N380", "ES3600000004");

			Factory.Save();

			processor = GetMockedProcessor(GetMockedProvider("962", entryHeader.CH_BGMReference, "20ES00999910000035", new ZDateTime(2020, 01, 02, 11, 00, 00), "4", ZString.Empty, "A2", "1", new ZDateTime(2020, 04, 01), "LT6B5QJ98HC6DHNY", new ZDateTime(2020, 10, 20, 15, 50, 00), "3J9MCS7TAM3KLZQC"));

			processor.ProcessMessage(message);

			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessage.Status.Received, message.EM_Status);
				AssertEquals("EM_MessageSubType", "ACC", message.EM_MessageSubType);

				AssertEquals("CH_EntryStatus", EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, entryHeader.CH_EntryStatus);
				AssertEquals("EntryHeaderStatusDescription", entryStatusList.GetDescriptionFromCode(EntryStatusCodes.ClearedWithPendingComplementaryDeclarations), entryHeader.EntryHeaderStatusDescription);

				var clSupDocs = GetCLSupportingDocuments();
				AssertEquals("There are 4 CL SupportingDocuments after calling SaveSupportingDocumentsForCL", 4, clSupDocs.Length);
				AssertContainsExactElementsInAnyOrder("The 4 CL SupportingDocuments after calling SaveSupportingDocumentsForCL have the correct CSI_ReferenceNumber", new ZString[] { "ES3600000001", "ES3600000002", "ES3600000003", "ES3600000004" }, clSupDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
			});
		}

		protected override ComplXExportDeclarationResponseMessageProcessor GetMockedProcessor(ICUSRESMessageProvider messageProvider)
		{
			var exportGenericResponseMessageProcessorMock = new Mock<ComplXExportDeclarationResponseMessageProcessorForMockTest>(new object[] { logger, new BranchCustomsMessageProcessorForTest() });
			exportGenericResponseMessageProcessorMock.CallBase = true;
			exportGenericResponseMessageProcessorMock.Setup(c => c.GetMessageProviderForTest).Returns((IExportResponseMessageProvider)messageProvider);
			return exportGenericResponseMessageProcessorMock.Object;
		}

		public class ComplXExportDeclarationResponseMessageProcessorForMockTest : ComplXExportDeclarationResponseMessageProcessor
		{
			public ComplXExportDeclarationResponseMessageProcessorForMockTest(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver = null) : base(logger, eDocsSaver)
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
