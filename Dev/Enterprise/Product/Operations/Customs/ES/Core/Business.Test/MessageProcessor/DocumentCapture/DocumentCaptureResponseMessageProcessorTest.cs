using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class DocumentCaptureResponseMessageProcessorTest : ESCommonResponseMessageProcessorTest<DocumentCaptureResponseMessageProcessor, BusinessObject, AttachedDocument>
	{
		public void TestProcessDocumentCaptureMessageForCusEntryHeader()
		{
			var entryHeader = SetUpEntryHeader();
			Factory.Save();
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetDocumentCaptureMessageFileContent(), InterchangeID, serializeMessageText: false);

			ProcessMessageForTest(message);

			var expectedMessageInterpretation = "<H3>Document " + documentName + " received and saved on eDocs.</H3>";
			GenericCommonAssertProcessResponseEntryHeader(message, entryHeader: entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, emStatus: EDIMessage.Status.Received);

			var document = AssertDocumentWasAdded(entryHeader);
			AssertDDAEventWasAdded(entryHeader, document, $"{entryHeader.CH_BGMReference} for {entryHeader.Declaration.JobNumber}");
		}

		public void TestProcessDocumentCaptureMessageForNctsHeader()
		{
			var mrn = "24ES009999502031K3";
			var nctsHeader = Factory.Load(CusInBondHeaderSchema.Constants.Prefix, ProcessorTestHelper.CreateNctsHeader(headerType: "D", referenceNum: ApplicationReference, mrn: mrn));

			SetSentInterchange(nctsHeader, InterchangeID);

			var message = CreateNewEDIMessage(ApplicationReference, GetDocumentCaptureMessageFileContent(), InterchangeID, serializeMessageText: false);

			ProcessMessageForTest(message);
			var expectedMessageInterpretation = "<H3>Document " + documentName + " received and saved on eDocs.</H3>";
			GenericCommonAssertProcessResponseOthers(message, expectedMessageInterpretation: expectedMessageInterpretation, emStatus: EDIMessage.Status.Received);

			var document = AssertDocumentWasAdded(nctsHeader as IDocManagerSupport);
			AssertDDAEventWasAdded(nctsHeader as EnterpriseBusinessObject, document, mrn);
		}

		public void TestProcessDocumentCaptureMessageForCusExitDetail()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
			exitHeader.CEH_Parent = declaration;
			var exitDetail = exitHeader.CusExitDetails.AddNew();
			exitDetail.CED_MovementReferenceNumber = ApplicationReference;

			SetSentInterchange(exitDetail, InterchangeID);

			var message = CreateNewEDIMessage(ApplicationReference, GetDocumentCaptureMessageFileContent(), InterchangeID, serializeMessageText: false);

			ProcessMessageForTest(message);

			var expectedMessageInterpretation = "<H3>Document " + documentName + " received and saved on eDocs.</H3>";
			GenericCommonAssertProcessResponseOthers(message, expectedMessageInterpretation: expectedMessageInterpretation, emStatus: EDIMessage.Status.Received);

			AssertDocumentWasAdded(exitDetail);
		}

		IeDoc AssertDocumentWasAdded(IDocManagerSupport businessObject)
		{
			var docManagerInfo = businessObject.DocManagerInfo;
			var eDocs = docManagerInfo.GetRelatedEDocs();

			var document = eDocs.FirstOrDefault(x => x.FileName.EqualsIgnoringCase(documentName));

			AssertNotNull("Document is not null", document);
			CombineAssertions(() =>
			{
				AssertEquals("DocType", "CLR", document.DocType);
				AssertEquals("Description", "Clearance Document", document.Description);
				AssertEquals("ImageData", true, document.ImageData.Length > 0);
			});

			return document;
		}

		void AssertDDAEventWasAdded(EnterpriseBusinessObject header, IeDoc document, string expectedDocumentJobReference)
		{
			var ddaEvent = header.Logs.Find(x => x.Event.SE_Code == Events.DocumentAllocatedCode).SingleOrDefault();
			AssertNotNull("DDA (Document Allocated) event added", ddaEvent);

			var expectedReference = $"{document.DocType}|{document.FileNameOnly} for {expectedDocumentJobReference}|{document.UniqueKey}";
			AssertEquals("DDA Event Reference", expectedReference, ddaEvent.ReferenceFreeText);
		}

		public void TestProcessMessageWrongData()
		{
			var entryHeader = SetUpEntryHeader();
			Factory.Save();
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetDocumentCaptureMessageWrongDataFileContent(), InterchangeID, serializeMessageText: false);

			ProcessMessageForTest(message);

			var expectedMessageInterpretation = "<H3>EDI Message response processing for requested document failed.</H3>";
			GenericCommonAssertProcessResponseOthers(message, expectedMessageInterpretation: expectedMessageInterpretation, loggerDesc: "Unable to read message text from message");
		}

		public void TestProcessMessageWrongXML()
		{
			var entryHeader = SetUpEntryHeader();
			Factory.Save();
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, WrongXMLTestFile, InterchangeID);

			ProcessMessageForTest(message);
			GenericCommonAssertProcessResponseOthers(message, loggerDesc: "Unable to read message text from message");
		}

		public void TestMessageProcessingError()
		{
			SetUpEntryHeader();
			var message = CreateNewEDIMessage("AAAAAAAA", ZString.Empty, InterchangeID, false);

			ProcessMessageForTest(message);
			GenericCommonAssertProcessResponseOthers(message, loggerDesc: "Unable to find business object for message");
		}

		public void TestMessageProcessingErrorNoReference()
		{
			SetUpEntryHeader();
			var message = CreateNewEDIMessage(ZString.Empty, ZString.Empty, InterchangeID, false);

			ProcessMessageForTest(message);
			GenericCommonAssertProcessResponseOthers(message, loggerDesc: "No Application Reference found for message");
		}

		readonly string documentName = "21ES00999910003247_E_AEAT_CLR.PDF";

		string GetDocumentCaptureMessageFileContent()
		{
			return ESTestFileReader.GetEmbeddedFileText(documentCaptureFilePath, "DocumentCaptureMessage.txt");
		}

		string GetDocumentCaptureMessageWrongDataFileContent()
		{
			return ESTestFileReader.GetEmbeddedFileText(documentCaptureFilePath, "DocumentCaptureMessageWrongData.txt");
		}

		const string documentCaptureFilePath = "Enterprise.Customs.ES.Business.Testing.MessageProcessor.TestFiles.DocumentCapture";

		protected override DocumentCaptureResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new DocumentCaptureResponseMessageProcessor(logger, new BranchCustomsMessageProcessorForTest());

		protected override ZString GetExpectedProcessorFriendlyName() => "Document Capture Declaration Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.EsDocumentRequest };

		protected override TestEdiMessage SetDataForCorrectPreProcessing(ZString interchangeTransportType)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			Factory.Save();

			entryHeader.CH_BGMReference = "TEST";
			Factory.Save();

			var interchangeID = ZGuid.NewZGuid();

			SetSentInterchange(entryHeader, interchangeID);

			var message = Factory.New<TestEdiMessage>();
			message.EM_ApplicationReference = "TEST";
			var responseInterchange = CreateTestResponseInterchange(interchangeID, interchangeTransportType);
			responseInterchange.ContainedMessages.Add(message);
			return message;
		}

		protected override void AssertLoggerMessagesWhenProcessMessageWrongText(TestEdiMessage message, BusinessObject businessObject)
		{
			var concatenatedUserLogStrings = GetAllConcatenatedUserLogStrings();
			AssertContains("logger", "Unable to read message text from message ", concatenatedUserLogStrings);
			AssertContains("logger exception", "There is an error in XML document (1, 1).", concatenatedUserLogStrings);
			AssertEquals("EM_MessageInterpretation",
					string.Format("<H3>Processor Failure</H3><br>" +
					"<H4>Failure: Unable to read message text from message (Number:{0}, Type:{1}, Sub:{2}, Ref:{3}); message status set to Failed.</H4>" +
					"<H4>Exception: There is an error in XML document (1, 1).</H4>", message.EM_MessageNum, message.EM_MessageType, message.EM_MessageSubType, message.EM_ApplicationReference)
					, message.EM_MessageInterpretation);
		}

		CusEntryHeader SetUpEntryHeader()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();

			SetSentInterchange(entryHeader, InterchangeID);

			return entryHeader;
		}
	}
}
