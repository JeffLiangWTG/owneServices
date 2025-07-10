using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.DocumentosSimplifiV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.EU.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using NUnit.Framework;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business.Testing
{
	sealed class DJPImportResponseMessageProcessorTest : XMLResponseMessageProcessorTest<DJPImportResponseMessageProcessor, IMessagePrettyFormatter, DocumentosSimplifiV1Sal>
	{
		public void TestUpdateDocuments()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			var entryLine = entryHeader.MergedLines.AddNew();

			var invLine = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
			invLine.JI_JZ = declaration.Invoices.AddNew().PK;
			var doc = invLine.SupportingDocuments.AddNew();
			doc.CSI_Code = "COD1";
			doc.CSI_Procedure = "N";

			doc = invLine.SupportingDocuments.AddNew();
			doc.CSI_Code = "COD2";
			doc.CSI_Procedure = "A";

			doc = invLine.SupportingDocuments.AddNew();
			doc.CSI_Code = "COD3";
			doc.CSI_Procedure = "R";

			Factory.Save();

			CombineAssertions(() =>
			{
				ProcessMessageForTest(message);

				AssertEquals("EM_Status", EDIMessage.Status.Received, message.EM_Status);

				var suppDocuments = entryHeader.MergedLines.FirstOrDefault().GetPreviouslySentSupportingDocuments();
				AssertEquals("Added 3 new supporting documents", 3, suppDocuments.Length);
				AssertEquals("Status cancelled for COD1", DocumentStatus.Cancelled, suppDocuments.FirstOrDefault(x => x.CSI_Code == "COD1").CSI_Status);
				AssertEquals("Status accepted for COD2", DocumentStatus.Accepted, suppDocuments.FirstOrDefault(x => x.CSI_Code == "COD2").CSI_Status);
				AssertEquals("Status accepted for COD3", DocumentStatus.Accepted, suppDocuments.FirstOrDefault(x => x.CSI_Code == "COD3").CSI_Status);

				doc.Delete();

				AssertEquals("Prereq: deleted COD3 doc, 2 documents", 2, invLine.SupportingDocuments.Count);
				doc = invLine.SupportingDocuments.AddNew();
				doc.CSI_Code = "COD2";
				doc.CSI_Procedure = "A";

				doc = invLine.SupportingDocuments.AddNew();
				doc.CSI_Code = "COD4";
				doc.CSI_Procedure = "R";

				Factory.Save();
				AssertEquals("Prereq: added duplicated document and new document, 4 documents", 4, invLine.SupportingDocuments.Count);

				ProcessMessageForTest(message);

				AssertEquals("EM_Status", EDIMessage.Status.Received, message.EM_Status);

				suppDocuments = entryHeader.MergedLines.FirstOrDefault().GetPreviouslySentSupportingDocuments();
				AssertEquals("Added 1 new supporting document and 1 updated", 4, suppDocuments.Length);

				doc = suppDocuments.FirstOrDefault(x => x.CSI_Code == "COD4");
				doc.CSI_Status = string.Empty;

				AssertEquals("Status empty for COD4", string.Empty, suppDocuments.FirstOrDefault(x => x.CSI_Code == "COD4").CSI_Status);

				ProcessMessageForTest(message);

				AssertEquals("EM_Status", EDIMessage.Status.Received, message.EM_Status);
				AssertEquals("EM_MessageSubType", "ACC", message.EM_MessageSubType);

				suppDocuments = entryHeader.MergedLines.FirstOrDefault().GetPreviouslySentSupportingDocuments();
				AssertEquals("1 document updated", 4, suppDocuments.Length);
				AssertEquals("Status accepted for COD4", DocumentStatus.Accepted, suppDocuments.FirstOrDefault(x => x.CSI_Code == "COD4").CSI_Status);
			});
		}

		public void TestUpdateDocumentsWhenCLHasDocsWithoutProcedure()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			var entryLine = entryHeader.MergedLines.AddNew();

			var invLine = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
			invLine.JI_JZ = declaration.Invoices.AddNew().PK;
			var doc = invLine.SupportingDocuments.AddNew();
			doc.CSI_Code = "COD1";
			doc.CSI_Procedure = "N";
			doc.CSI_ReferenceNumber = "ES3600000001";

			doc = invLine.SupportingDocuments.AddNew();
			doc.CSI_Code = "COD2";
			doc.CSI_Procedure = "A";
			doc.CSI_ReferenceNumber = "ES3600000002";

			doc = invLine.SupportingDocuments.AddNew();
			doc.CSI_Code = "COD3";
			doc.CSI_Procedure = "R";

			entryLine.AddEntryLineDocument<SupportingDocument>("COD1", "ES3600000001");
			entryLine.AddEntryLineDocument<SupportingDocument>("COD2", "ES3600000002");
			Factory.Save();

			CombineAssertions(() =>
			{
				var suppDocuments = entryHeader.MergedLines.FirstOrDefault().GetPreviouslySentSupportingDocuments();
				AssertEquals("2 supporting documents in CL before processing", 2, suppDocuments.Length);

				ProcessMessageForTest(message);

				AssertEquals("EM_Status", EDIMessage.Status.Received, message.EM_Status);

				suppDocuments = entryHeader.MergedLines.FirstOrDefault().GetPreviouslySentSupportingDocuments();
				AssertEquals("3 supporting documents in CL after processing", 3, suppDocuments.Length);

				var doc1 = suppDocuments.FirstOrDefault(x => x.CSI_Code == "COD1");
				AssertEquals("Status accepted for COD1", DocumentStatus.Cancelled, doc1.CSI_Status);
				AssertEquals("Procedure A for COD1", "N", doc1.CSI_Procedure);

				var doc2 = suppDocuments.FirstOrDefault(x => x.CSI_Code == "COD2");
				AssertEquals("Status accepted for COD2", DocumentStatus.Accepted, doc2.CSI_Status);
				AssertEquals("Procedure A for COD2", "A", doc2.CSI_Procedure);

				AssertEquals("Status accepted for COD3", DocumentStatus.Accepted, suppDocuments.FirstOrDefault(x => x.CSI_Code == "COD3").CSI_Status);
			});
		}

		[TestDate(2020, 10, 05, 09, 36, 0)]
		public void TestProcessAcceptedMessage()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);

			var expectedMessageInterpretation = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Description:</td><td>&nbsp;&nbsp;</td><td>(0000)Envío Aceptado, se han tramitado todas las importaciones declaradas.</td></tr></table>" +
				"<table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>30-03-2021, 16:15:20</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>21ES00999930002964</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>";

			AssertEquals("ZG_DJPMRN", "21ES00999930002964", entryHeader.ZG_DJPMRN);
			AssertDJPMessage(message, expectedMessageInterpretation: expectedMessageInterpretation, messageSubType: "ACC", entryStatusCode: EntryStatusCodes.Cleared);
		}

		public void TestProcessMessageRejected()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetRejectedTestFile(), InterchangeID);

			ProcessMessageForTest(message);

			var expectedMessageInterpretation = "<H3>Rejected Declaration</H3>" +
				"<table border=\"0\">" +
				"<tr><td>1003:</td><td>&nbsp;&nbsp;</td><td>Envío Aceptado sin tramitarse importaciones por tener todas errores.</td></tr>" +
				"</table><br>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><td><strong>Error</strong></td><td><strong>Location/Description</strong></td></tr>" +
				"<tr><td>6112</td><td>0.0<br>Hay un documento declarado no necesario..N740</td></tr>" +
				"</table>";
			AssertDJPMessage(message, expectedMessageInterpretation: expectedMessageInterpretation);
		}

		public void TestProcessAcceptedMessageVariousEntries()
		{
			var entryHeader2 = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader2.CH_EntryStatus = InitialEntryStatus;
			entryHeader2.MovementReferenceNumber = "20ES00999912345678";

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);

			CombineAssertions(() =>
			{
				AssertEquals("Response message assign to the first Entry", 2, entryHeader.Messages.Count);
				AssertEquals("Response message does not assign to the second Entry", 0, entryHeader2.Messages.Count);
			});
		}

		public void TestProcessMessageEntryInstructionSubStyle()
		{
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			CombineAssertions("When DJP message type is accepted, update the sub style value in the related entry instruction from B to X", () =>
			{
				AssertEntryInstruction("B", "X");
				AssertEntryInstruction("A", "A");
			});

			void AssertEntryInstruction(ZString initialSubStyle, ZString expectedSubStyle)
			{
				entryInstruction.CEI_SubStyle = initialSubStyle;
				entryInstruction.Factory.Save();

				var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

				ProcessMessageForTest(message);

				AssertEquals($"Entry Instruction SubStyle '{initialSubStyle}'", expectedSubStyle, entryInstruction.CEI_SubStyle);
			}
		}

		public void TestProcessMessageRejectedWithoutNumeroReferenciaDUA()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetTestFileNoMRN(), InterchangeID);

			ProcessMessageForTest(message);

			var expectedMessageInterpretation = "<H3>Rejected Declaration</H3>" +
				"<table border=\"0\">" +
				"<tr><td>0301:</td><td>&nbsp;&nbsp;</td><td>La estructura del mensaje es incorrecta no aporta los elementos necesarios para su tratamiento. Se esperaba nodo: NumeroReferenciaDUA</td></tr>" +
				"</table><br>";
			AssertDJPMessage(message, expectedMessageInterpretation: expectedMessageInterpretation);
		}

		protected override TestEdiMessage SetDataForCorrectPreProcessing(ZString interchangeTransportType)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter(MRNCode, ZDateTime.Today);

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

		protected override ZString CHStatusWhenWrongXMLOrMessageTextEmpty => ZString.Empty;

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_EntryStatus = InitialEntryStatus;
			entryHeader.MovementReferenceNumberSetter(MRNCode, entryStatus: CircuitCodeList.Codes.ORANGE);
			entryHeader.SetCSVClearanceNum("TEST444444444444");

			SetSentInterchange(entryHeader, InterchangeID);

			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "Customs Status");

			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(EsCode, parent: grouping);
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CLR", "Customs Declaration Accepted and Cleared", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CLP", "Customs Declaration Cleared with pending complementary declarations", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

			Factory.Save();
		}

		string GetAcceptanceTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DJPTestFilePath, "DJP-accept.txt");
		string GetRejectedTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DJPTestFilePath, "DJP-reject.txt");
		string GetTestFileNoMRN() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DJPTestFilePath, "DJP-reject-withoutMRN.txt");
		protected override string GetAcceptanceTestFileWithLongSegmentId() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DJPTestFilePath, "AcceptedMessageWithLongSegmentId.txt");

		void AssertDJPMessage(TestEdiMessage message, string expectedMessageInterpretation = "", string messageSubType = "REJ", string entryStatusCode = InitialEntryStatus)
		{
			GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, messageSubType: messageSubType, messageNum: MessageNum, entryStatusCode: entryStatusCode, circuit: CircuitCodeList.Codes.ORANGE, csvClearance: CsvClearance, movementReferenceNumber: MRNCode);
		}

		const string MessageNum = "TARIC20201020141554350002";
		const string MRNCode = "20ES00999930006184";
		const string CsvClearance = "TEST444444444444";
		const string InitialEntryStatus = "CLP";

		protected override DJPImportResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new DJPImportResponseMessageProcessor(logger);

		protected override ZString GetExpectedProcessorFriendlyName() => "Pending Supporting Documents Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.PendingSupportingDocuments };
	}
}
