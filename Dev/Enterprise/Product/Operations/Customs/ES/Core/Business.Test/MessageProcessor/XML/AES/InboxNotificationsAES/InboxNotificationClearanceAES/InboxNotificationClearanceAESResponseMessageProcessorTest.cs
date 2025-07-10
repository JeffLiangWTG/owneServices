using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ComunicaLevanteExporV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class InboxNotificationClearanceAESResponseMessageProcessorTest : XMLResponseMessageProcessorTest<InboxNotificationClearanceAESResponseMessageProcessor, IMessagePrettyFormatter, ComunicaLevanteExporV1Sal>
	{
		public void TestProcessAcceptedMessage_CLR()
		{
			AddMessageProcessAndAssertResult_CLR();
		}

		public void TestProcessAcceptedMessage_CLP()
		{
			entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.B;
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);
			ProcessMessageForTest(message);

			AssertAcceptedDeclaration(message, entryStatusCode: EntryStatusCodes.ClearedWithPendingComplementaryDeclarations);
		}

		public void TestCreateDocumentCaptureRequestEDIMessageWhenCSVClearance_Export_AllDocs()
		{
			declaration.ZG_CTStatusID = "T2LF";
			entryHeader.ZG_CSVExitCertificate = CsvExitCertificate;
			entryHeader.IndirectExport = true;

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);

			CombineAssertions(() =>
			{
				entryHeader.Messages.Reload(true);
				var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
				AssertEquals("Doc Messages sent number is", 4, docMessages.Length);

				AssertDocumentRequestEDIMessages(docMessages, new List<(ZString fileName, ZString urlParameter)>() { (MRNCodeClearedNotPending + "_E_AEAT_CLR.pdf", CsvClearance),
																													(MRNCodeClearedNotPending + "_E_AEAT_CLR_EXT.pdf", CsvExitCertificate),
																													(MRNCodeClearedNotPending + "_E_AEAT_t2lf.pdf", CsvT2L),
																													(MRNCodeClearedNotPending + "_E_AEAT_ead.pdf", MRNCodeClearedNotPending) });
			});
		}

		public void TestCreateDocumentCaptureRequestEDIMessageWhenCSVClearance_Export_NoDocs()
		{
			declaration.ZG_CTStatusID = "T2LF";
			declaration.ZG_IsTrainingDeclaration = false;
			entryHeader.ZG_CSVExitCertificate = CsvExitCertificate;
			entryHeader.IndirectExport = true;

			var docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
			var eDoc1 = docManagerInfo.AddFileOrDocument(new byte[1], MRNCodeClearedNotPending + "_E_AEAT_CLR.pdf", "CLR");
			var eDoc2 = docManagerInfo.AddFileOrDocument(new byte[1], MRNCodeClearedNotPending + "_E_AEAT_CLR_EXT.pdf", "CLR");
			var eDoc3 = docManagerInfo.AddFileOrDocument(new byte[1], MRNCodeClearedNotPending + "_E_AEAT_t2lf.pdf", "CAU");
			var eDoc4 = docManagerInfo.AddFileOrDocument(new byte[1], MRNCodeClearedNotPending + "_E_AEAT_ead.pdf", "EAD");
			docManagerInfo.Save();

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);

			CombineAssertions(() =>
			{
				docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
				var eDocs = docManagerInfo.GetRelatedEDocs();
				AssertEquals("Number of eDocs is correct", 4, eDocs.Count());

				var eDocNames = new List<ZString>() 
									{ MRNCodeClearedNotPending + "_E_AEAT_CLR.pdf", 
										MRNCodeClearedNotPending + "_E_AEAT_CLR_EXT.pdf", 
										MRNCodeClearedNotPending + "_E_AEAT_t2lf.pdf",
										MRNCodeClearedNotPending + "_E_AEAT_ead.pdf" };
				AssertContainsExactElementsInAnyOrder("eDocs contains all documents with original names", eDocNames, eDocs.Cast<IeDoc>().Select(x => x.FileName).ToList());

				entryHeader.Messages.Reload(true);
				var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
				AssertEquals("Doc Messages sent number is", 0, docMessages.Length);
			});
		}

		public void TestProcessAcceptedMessageAndRemoveCusPollingTransactionsWhenxT()
		{
			AssertProcessAcceptedMessageAndRemoveCusPollingTransactionsWhenxT(MRNCodeClearedNotPending, AddMessageProcessAndAssertResult_CLR);
		}

		void AddMessageProcessAndAssertResult_CLR()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);

			AssertAcceptedDeclaration(message, entryStatusCode: EntryStatusCodes.Cleared);
		}

		void AssertAcceptedDeclaration(TestEdiMessage message, string entryStatusCode)
		{
			var expectedMessageInterpretationText = "<H3>Inbox Communication</H3>" +
				"<br><table border=\"0\"><tr><td>Message Type:</td><td>&nbsp;&nbsp;</td><td>CLEVEX - Clearance Information</td></tr></table>" +
				"<table border=\"0\"><tr><td>Received:</td><td>&nbsp;&nbsp;</td><td>18-02-2022, 07:30:22</td></tr></table>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>22ES000101100047B1</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>MYT5CUUEVP4QF7CJ</td></tr>" +
				"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>18-02-2022</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Exit Type:</td><td>&nbsp;&nbsp;</td><td>[I] Indirect</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>DE - Indirect Dispatch</td></tr></table>";

			GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretationText, messageSubType: "ACC", entryStatusCode: entryStatusCode, movementReferenceNumber: MRNCodeClearedNotPending, csvClearance: CsvClearance, entryReleaseDate: entryReleaseDate, csvT2L: CsvT2L, messageNum: MessageNum);
		}

		protected override void SetUp()
		{
			base.SetUp();

			entryHeader.CH_EntryStatus = OriginalEntryStatus;

			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.A;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			entryHeader.MovementReferenceNumberSetter(MRNCodeClearedNotPending);

			SetSentInterchange(entryHeader, InterchangeID);

			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "Customs Status");

			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(EsCode, parent: grouping);
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, OriginalEntryStatus, "Original Entry Status", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CLR", "Customs Declaration Accepted and Cleared", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CLP", "Customs Declaration Cleared with pending complementary declarations", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			Factory.Save();
		}
		CusEntryInstruction entryInstruction;

		string GetAcceptanceTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotificationClearanceAESTestFilePath, "AcceptedMessage.txt");
		protected override string GetAcceptanceTestFileWithLongSegmentId() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotificationClearanceAESTestFilePath, "AcceptedMessageWithLongSegmentId.txt");

		const string MRNCodeClearedNotPending = "22ES000101100047B1";
		const string MessageNum = "20220218073022844089";
		readonly ZDateTime entryReleaseDate = new ZDateTime(2022, 02, 18);
		const string CsvClearance = "MYT5CUUEVP4QF7CJ";
		const string CsvT2L = "PNS6NA3WMAUC4J8W";
		const string CsvExitCertificate = "A1234567890";

		protected override ZString CHStatusWhenWrongXMLOrMessageTextEmpty => ZString.Empty;

		protected override TestEdiMessage SetDataForCorrectPreProcessing(ZString interchangeTransportType)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter(MRNCodeClearedNotPending);

			var interchangeID = ZGuid.NewZGuid();

			SetSentInterchange(entryHeader, interchangeID);

			var responseInterchange = Factory.New<EDIInterchange>();
			responseInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			responseInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			responseInterchange.EI_SessionGUID = interchangeID;
			responseInterchange.EI_TransportType = interchangeTransportType;

			var message = Factory.New<TestEdiMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = MessageType;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageSubType = "AAA";
			message.EM_ApplicationReference = "TEST";
			message.EM_MessageText = GetAcceptanceTestFile();

			responseInterchange.ContainedMessages.Add(message);

			return message;
		}

		protected override TestEdiMessage SetDataForIncorrectApplicationReferencePreProcessing(ZString interchangeTransportType)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();

			var interchangeID = ZGuid.NewZGuid();

			SetSentInterchange(entryHeader, interchangeID);

			var responseInterchange = Factory.New<EDIInterchange>();
			responseInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			responseInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			responseInterchange.EI_SessionGUID = interchangeID;
			responseInterchange.EI_TransportType = interchangeTransportType;

			var message = Factory.New<TestEdiMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = MessageType;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageSubType = "AAA";
			message.EM_ApplicationReference = "TEST";
			message.EM_MessageText = GetAcceptanceTestFile();

			responseInterchange.ContainedMessages.Add(message);
			return message;
		}

		protected override void AssertLoggerMessagesWhenProcessMessageWrongXML()
		{
			AssertContains("logger", "Unable to find business object for message", GetAllConcatenatedUserLogStrings());
		}

		protected override void AssertLoggerMessagesWhenMessageProcessingError()
		{
			base.AssertLoggerMessagesWhenMessageProcessingError();

			AssertContains("logger exception", "Message Text is empty so can't continue with processing", GetAllConcatenatedUserLogStrings());
		}

		protected override ZString GetExpectedProcessorFriendlyName() => "Inbox Notification Export Clearance Declaration Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.ExportClearanceCommunication };

		protected override InboxNotificationClearanceAESResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new InboxNotificationClearanceAESResponseMessageProcessor(logger, new BranchCustomsMessageProcessorForTest());
	}
}
