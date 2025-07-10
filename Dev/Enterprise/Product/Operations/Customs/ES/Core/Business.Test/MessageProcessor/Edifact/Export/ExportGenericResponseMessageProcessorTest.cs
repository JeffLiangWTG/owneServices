using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Core;
using Moq;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business.Testing
{
	public abstract class ExportGenericResponseMessageProcessorTest<TResponse> : ESResponseMessageProcessorTest<TResponse, IExportResponseMessageProvider>
		where TResponse : ExportGenericResponseMessageProcessor
	{
		public void TestProcessAcceptedMessageGreenCircuitNotMocked()
		{
			SetSentInterchange(entryHeader, InterchangeID);
			var responseMessage = CreateNewEDIMessage(entryHeader.CH_BGMReference, MessageText, InterchangeID, false);

			ProcessMessageForTest(responseMessage);
			CommonSendAssertGreenCircuitAccepted(responseMessage, EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, ZString.Empty);
		}

		public void TestProcessAcceptedMessageGreenCircuit()
		{
			processor = GetMockedProcessor(GetMockedProvider("962", entryHeader.CH_BGMReference, MrnCode, acceptanceDate, "4", ZString.Empty, "A2", "1", limitDateOfArrival, CSVLevante, entryReleaseDate, CSVT2L));

			processor.ProcessMessage(message);
			CommonSendAssertGreenCircuitAccepted(message, EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, "1");
		}

		public void TestProcessMessageCAN()
		{
			processor = GetMockedProcessor(GetMockedProvider("962", entryHeader.CH_BGMReference, ZString.Empty, acceptanceDate, "4", "5", ZString.Empty, ZString.Empty, limitDateOfArrival, ZString.Empty, ZDateTime.Empty, ZString.Empty));

			processor.ProcessMessage(message);
			GenericCommonAssertProcessEntryData(message, entryHeader, messageSubType: "ACC", entryStatusCode: EntryStatusCodes.CustomsDeclarationAccepted, circuit: CircuitCodeList.Codes.GREEN, circuitCan: CircuitCodeList.Codes.RED, acceptanceDate: acceptanceDate, limitDateOfArrival: limitDateOfArrival, messageNum: "1");
		}

		public void TestProcessAmendmentAcceptedMessage_NotOverrideEntryStatus()
		{
			message.EM_MessageType = DeclarationMessageTypeList.Codes.ExportAmendment;
			entryHeader.CH_EntryStatus = EntryStatusCodes.Cleared;

			processor = GetMockedProcessor(GetMockedProvider("962", entryHeader.CH_BGMReference, MrnCode, acceptanceDate, "4", ZString.Empty, "A2", "1", limitDateOfArrival, CSVLevante, entryReleaseDate, CSVT2L));

			processor.ProcessMessage(message);
			CommonSendAssertGreenCircuitAccepted(message, EntryStatusCodes.Cleared, "1");
		}

		public void TestProcessComplementaryDue_WithAcceptanceDate()
		{
			processor = GetMockedProcessor(GetMockedProvider("962", entryHeader.CH_BGMReference, ZString.Empty, acceptanceDate, "3", ZString.Empty, ZString.Empty, ZString.Empty, limitDateOfArrival, ZString.Empty, ZDateTime.Empty, ZString.Empty));

			processor.ProcessMessage(message);
			GenericCommonAssertProcessEntryData(message, entryHeader, messageSubType: "ACC", entryStatusCode: EntryStatusCodes.Cleared, acceptanceDate: acceptanceDate, limitDateOfArrival: limitDateOfArrival, messageNum: "1");
		}

		public void TestProcessComplementaryDue_WithoutAcceptanceDate()
		{
			var interchange = Factory.New<EDIInterchange>();

			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_From = SpanishCustomsTypeCodeList.Codes.EdifactSpanishCustoms;
			interchange.EI_InterchangeType = DeclarationMessageTypeList.Codes.Export;
			interchange.EI_To = "TEST";
			interchange.EI_Status = EDIInterchange.Status.Queued;

			message.EM_EI = interchange.PK;

			Factory.Save();

			processor = GetMockedProcessor(GetMockedProvider("962", entryHeader.CH_BGMReference, ZString.Empty, ZDateTime.Empty, "3", ZString.Empty, ZString.Empty, ZString.Empty, limitDateOfArrival, ZString.Empty, ZDateTime.Empty, ZString.Empty));

			processor.ProcessMessage(message);
			GenericCommonAssertProcessEntryData(message, entryHeader, messageSubType: "ACC", entryStatusCode: EntryStatusCodes.Cleared, acceptanceDate: interchange.EI_SystemCreateTimeUtc.ToLocalBranchTime(), limitDateOfArrival: limitDateOfArrival, messageNum: "1");
		}

		public void TestProcessComplementaryDue_WithCSVClearance()
		{
			processor = GetMockedProcessor(GetMockedProvider("962", entryHeader.CH_BGMReference, MrnCode, acceptanceDate, "3", ZString.Empty, ZString.Empty, ZString.Empty, limitDateOfArrival, CSVLevante, ZDateTime.Empty, ZString.Empty));

			processor.ProcessMessage(message);
			GenericCommonAssertProcessEntryData(message, entryHeader, messageSubType: "ACC", entryStatusCode: EntryStatusCodes.Cleared, acceptanceDate: acceptanceDate, limitDateOfArrival: limitDateOfArrival, csvClearance: CSVLevante, movementReferenceNumber: MrnCode, messageNum: "1");
		}

		public void TestProcessComplementaryDue_WithoutData()
		{
			entryHeader.SetCSVClearanceNum(CSVLevante);
			entryHeader.ZG_LimitDateOfArrival = limitDateOfArrival;
			entryHeader.CH_EntryReleaseDate = entryReleaseDate;
			entryHeader.ZG_CSVT2L = CSVT2L;

			processor = GetMockedProcessor(GetMockedProvider("962", entryHeader.CH_BGMReference, ZString.Empty, acceptanceDate, "3", ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, ZString.Empty, ZDateTime.Empty, ZString.Empty));

			processor.ProcessMessage(message);
			GenericCommonAssertProcessEntryData(message, entryHeader, messageSubType: "ACC", entryStatusCode: EntryStatusCodes.Cleared, acceptanceDate: acceptanceDate, limitDateOfArrival: limitDateOfArrival, entryReleaseDate: entryReleaseDate, csvT2L: CSVT2L, messageNum: "1");
		}

		public void TestCreateInboxRequestEDIMessageForPreDUE_EHub()
		{
			using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(false))
			{
				processor = GetMockedProcessor(GetMockedProvider("962", entryHeader.CH_BGMReference, MrnCode, acceptanceDate, "P", ZString.Empty, ZString.Empty, ZString.Empty, limitDateOfArrival, ZString.Empty, entryReleaseDate, ZString.Empty));

				processor.ProcessMessage(message);
				GenericCommonAssertProcessEntryData(message, entryHeader, messageSubType: "ACC", entryStatusCode: EntryStatusCodes.PreDeclarationAccepted, acceptanceDate: acceptanceDate, limitDateOfArrival: limitDateOfArrival, entryReleaseDate: entryReleaseDate, movementReferenceNumber: MrnCode, messageNum: "1");

				AssertNewInboxMessages(entryHeader.Messages, new ZString[] { DeclarationMessageTypeList.Codes.InBoxNotificationForExport }, DeclarantId, DeclarantName, MrnCode);
			}
		}

		public void TestCreateInboxRequestEDIMessageForPreDUE_xT()
		{
			using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(true))
			{
				processor = GetMockedProcessor(GetMockedProvider("962", entryHeader.CH_BGMReference, MrnCode, acceptanceDate, "P", ZString.Empty, ZString.Empty, ZString.Empty, limitDateOfArrival, ZString.Empty, entryReleaseDate, ZString.Empty));

				processor.ProcessMessage(message);
				GenericCommonAssertProcessEntryData(message, entryHeader, messageSubType: "ACC", entryStatusCode: EntryStatusCodes.PreDeclarationAccepted, acceptanceDate: acceptanceDate, limitDateOfArrival: limitDateOfArrival, entryReleaseDate: entryReleaseDate, movementReferenceNumber: MrnCode, messageNum: "1");

				AssertNewCusPollingTransaction(entryHeader.PK, entryHeader.TablePrefix, new ZString[] { DeclarationMessageTypeList.Codes.InBoxNotificationForExport }, MrnCode);
			}
		}

		public void TestCreateDocumentCaptureRequestEDIMessageWhenCSVClearance_Export_AllDocs()
		{
			message.EM_MessageType = DeclarationMessageTypeList.Codes.Export;

			declaration.ZG_CTStatusID = "T2LF";
			entryHeader.ZG_CSVT2L = "CSVT2L";

			Factory.Save();

			processor = GetMockedProcessor(GetMockedProvider("962", entryHeader.CH_BGMReference, MrnCode, acceptanceDate, "4", ZString.Empty, "A2", "1", limitDateOfArrival, CSVLevante, entryReleaseDate, CSVT2L));

			processor.ProcessMessage(message);
			CommonSendAssertGreenCircuitCSVClearance(message);
			CombineAssertions(() =>
			{
				entryHeader.Messages.Reload(true);
				var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
				AssertEquals("Doc Messages sent number is", 3, docMessages.Length);

				AssertDocumentRequestEDIMessages(docMessages, new List<(ZString fileName, ZString urlParameter)>() { (MrnCode + "_E_AEAT_CLR.pdf", CSVLevante),
																													(MrnCode + "_E_AEAT_ead.pdf", MrnCode),
																													(MrnCode + "_E_AEAT_t2lf.pdf", CSVT2L) });
			});
		}

		public void TestCreateDocumentCaptureRequestEDIMessageWhenCSVClearance_Export_NoDocs()
		{
			message.EM_MessageType = DeclarationMessageTypeList.Codes.Export;

			declaration.ZG_CTStatusID = "T2LF";

			var docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
			var eDoc1 = docManagerInfo.AddFileOrDocument(new byte[1], MrnCode + "_E_AEAT_CLR.pdf", "CLR");
			var eDoc2 = docManagerInfo.AddFileOrDocument(new byte[1], MrnCode + "_E_AEAT_ead.pdf", "CAU");
			var eDoc3 = docManagerInfo.AddFileOrDocument(new byte[1], MrnCode + "_E_AEAT_t2lf.pdf", "CAU");
			docManagerInfo.Save();

			Factory.Save();

			processor = GetMockedProcessor(GetMockedProvider("962", entryHeader.CH_BGMReference, MrnCode, acceptanceDate, "4", ZString.Empty, "A2", "1", limitDateOfArrival, CSVLevante, entryReleaseDate, CSVT2L));

			processor.ProcessMessage(message);
			CommonSendAssertGreenCircuitCSVClearance(message);

			CombineAssertions(() =>
			{
				docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
				var eDocs = docManagerInfo.GetRelatedEDocs();
				AssertEquals("Number of eDocs is correct", 3, eDocs.Count());

				var eDocNamesChanged = new List<ZString>() { MrnCode + "_E_AEAT_CLR.pdf", MrnCode + "_E_AEAT_ead.pdf", MrnCode + "_E_AEAT_t2lf.pdf" };
				AssertContainsExactElementsInAnyOrder("eDocs contains all documents with original names", eDocNamesChanged, eDocs.Cast<IeDoc>().Select(x => x.FileName).ToList());

				entryHeader.Messages.Reload(true);
				var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
				AssertEquals("Doc Messages sent number is", 0, docMessages.Length);
			});
		}

		public void TestCreateDocumentCaptureRequestEDIMessageWhenCSVClearance_ExportAmendment_2Docs()
		{
			message.EM_MessageType = DeclarationMessageTypeList.Codes.ExportAmendment;

			Factory.Save();

			processor = GetMockedProcessor(GetMockedProvider("962", entryHeader.CH_BGMReference, MrnCode, acceptanceDate, "4", ZString.Empty, "A2", "1", limitDateOfArrival, CSVLevante, entryReleaseDate, CSVT2L));

			processor.ProcessMessage(message);
			CommonSendAssertGreenCircuitCSVClearance(message);

			CombineAssertions(() =>
			{
				entryHeader.Messages.Reload(true);
				var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
				AssertEquals("Doc Messages sent number is", 2, docMessages.Length);

				AssertDocumentRequestEDIMessages(docMessages, new List<(ZString fileName, ZString urlParameter)>() { (MrnCode + "_E_AEAT_CLR.pdf", CSVLevante),
																													(MrnCode + "_E_AEAT_ead.pdf", MrnCode) });
			});
		}

		public void TestCreateDocumentCaptureRequestEDIMessageWhenCSVClearance_ExportAmendment_NewCSVClearance()
		{
			SetSentInterchange(entryHeader, InterchangeID);
			var responseMessage = CreateNewEDIMessage(entryHeader.CH_BGMReference, MessageText, InterchangeID, false);

			responseMessage.EM_MessageType = DeclarationMessageTypeList.Codes.ExportAmendment;

			declaration.ZG_CTStatusID = "T2LF";
			entryHeader.SetCSVClearanceNum("AAAAAAAAAAAAAAAA");
			entryHeader.ZG_CSVT2L = "CSVT2L";

			Factory.Save();

			var docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
			var eDoc1 = docManagerInfo.AddFileOrDocument(new byte[1], MrnCode + "_E_AEAT_CLR.pdf", "CLR");
			var eDoc2 = docManagerInfo.AddFileOrDocument(new byte[1], MrnCode + "_E_AEAT_ead.pdf", "CAU");
			var eDoc3 = docManagerInfo.AddFileOrDocument(new byte[1], MrnCode + "_E_AEAT_t2lf.pdf", "CAU");
			docManagerInfo.Save();

			ProcessMessageForTest(responseMessage, entryHeader.Messages);
			CommonSendAssertGreenCircuitCSVClearance(responseMessage, messageNum: string.Empty);

			CombineAssertions(() =>
			{
				docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
				var eDocs = docManagerInfo.GetRelatedEDocs();
				AssertEquals("Number of eDocs is correct", 3, eDocs.Count());

				var eDocNamesChanged = new List<ZString>() { MrnCode + "_E_AEAT_CLR_OLD_AAAAAAAAAAAAAAAA.pdf", MrnCode + "_E_AEAT_ead.pdf", MrnCode + "_E_AEAT_t2lf_OLD_AAAAAAAAAAAAAAAA.pdf" };
				AssertContainsExactElementsInAnyOrder("eDocs contains all documents with original names", eDocNamesChanged, eDocs.Cast<IeDoc>().Select(x => x.FileName).ToList());

				entryHeader.Messages.Reload(true);
				var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
				AssertEquals("Doc Messages sent number is", 2, docMessages.Length);

				AssertDocumentRequestEDIMessages(docMessages, new List<(ZString fileName, ZString urlParameter)>() { (MrnCode + "_E_AEAT_CLR.pdf", CSVLevante),
																													(MrnCode + "_E_AEAT_t2lf.pdf", CSVT2L) });
			});
		}

		public void TestNotCreateDocumentCaptureRequestEDIMessageWhenCSVClearance_ExportComplementaryX()
		{
			message.EM_MessageType = DeclarationMessageTypeList.Codes.TypeXExport;

			entryHeader.SetCSVClearanceNum("AAAAAAAAAAAAAAAA");

			Factory.Save();

			processor = GetMockedProcessor(GetMockedProvider("962", entryHeader.CH_BGMReference, MrnCode, acceptanceDate, "3", ZString.Empty, "A2", "1", limitDateOfArrival, ZString.Empty, entryReleaseDate, CSVT2L));

			processor.ProcessMessage(message);
			CommonSendAssertCSVClearanceComplementary(message);
			CombineAssertions(() =>
			{
				entryHeader.Messages.Reload(true);
				var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
				AssertEquals("Doc Messages sent number is", 0, docMessages.Length);
			});
		}

		public void TestNotCreateDocumentCaptureRequestEDIMessageWhenCSVClearance_ExportComplementaryX_DocsChanged()
		{
			SetSentInterchange(entryHeader, InterchangeID);
			var responseMessage = CreateNewEDIMessage(entryHeader.CH_BGMReference, MessageTextComplementary, InterchangeID, false);

			responseMessage.EM_MessageType = DeclarationMessageTypeList.Codes.TypeXExport;

			declaration.ZG_CTStatusID = "T2LF";
			entryHeader.SetCSVClearanceNum("AAAAAAAAAAAAAAAA");

			var docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
			var eDoc1 = docManagerInfo.AddFileOrDocument(new byte[1], MrnCode + "_E_AEAT_CLR.pdf", "CLR");
			var eDoc2 = docManagerInfo.AddFileOrDocument(new byte[1], MrnCode + "_E_AEAT_ead.pdf", "CAU");
			var eDoc3 = docManagerInfo.AddFileOrDocument(new byte[1], MrnCode + "_E_AEAT_t2lf.pdf", "CAU");
			docManagerInfo.Save();

			Factory.Save();

			ProcessMessageForTest(responseMessage);
			CommonSendAssertCSVClearanceComplementary(responseMessage, messageNum: string.Empty);

			CombineAssertions(() =>
			{
				docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
				var eDocs = docManagerInfo.GetRelatedEDocs();
				AssertEquals("Number of eDocs is correct", 3, eDocs.Count());

				var eDocNamesChanged = new List<ZString>() { MrnCode + "_E_AEAT_CLR_OLD_AAAAAAAAAAAAAAAA.pdf", MrnCode + "_E_AEAT_ead.pdf", MrnCode + "_E_AEAT_t2lf_OLD_AAAAAAAAAAAAAAAA.pdf" };
				AssertContainsExactElementsInAnyOrder("eDocs contains all documents with original names", eDocNamesChanged, eDocs.Cast<IeDoc>().Select(x => x.FileName).ToList());

				entryHeader.Messages.Reload(true);
				var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
				AssertEquals("Doc Messages sent number is", 0, docMessages.Length);
			});
		}

		public void TestProcessMessageRejected()
		{
			var error1 = SetUpError("211", "CABECERA.TIPO DE PROCEDIMIENTO  (CAS. 1.2)..", "EL TIPO DE PROCEDIMIENTO NO ES VALIDO. -VALOR X-.");
			var error2 = SetUpError("00338", "PARTIDA(001).CASILLA 44 (001).Tipo de documento.Other Text", "Código  documento incorrecto.More Text");
			var error3 = SetUpError("55000", "Other Error.", "Other Description..");

			processor = GetMockedProcessor(GetMockedProvider("963", entryHeader.CH_BGMReference, MovementReferenceNumberReject, ZDateTime.Empty, "2", ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, ZString.Empty, ZDateTime.Empty, ZString.Empty, new List<ErrorMessage> { error1, error2, error3 }));

			var expectedMessageInterpretationTextRejected = "<H3>Rejected Declaration</H3>" +
					"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
					"<tr><td><strong>Error</strong></td><td><strong>Location</strong></td><td><strong>Description</strong></td></tr>" +
					"<tr><td>211</td><td>CABECERA.TIPO DE PROCEDIMIENTO  (CAS. 1.2)..</td><td>EL TIPO DE PROCEDIMIENTO NO ES VALIDO. -VALOR X-.</td></tr>" +
					"<tr><td>00338</td><td>PARTIDA(001).CASILLA 44 (001).Tipo de documento.Other Text</td><td>Código  documento incorrecto.More Text</td></tr>" +
					"<tr><td>55000</td><td>Other Error.</td><td>Other Description..</td></tr></table>";

			processor.ProcessMessage(message);
			GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretationTextRejected, messageSubType: "REJ", entryStatusCode: OriginalEntryStatus, movementReferenceNumber: MovementReferenceNumberReject, messageNum: "1");
		}

		public void TestProcessMessageRejectedNoNewCLDocs()
		{
			var suppDoc1 = invoiceLine.SupportingDocuments.AddNew();
			suppDoc1.CSI_Code = "N380";
			suppDoc1.CSI_ReferenceNumber = "ES3600000001";

			var suppDoc2 = invoiceLine.SupportingDocuments.AddNew();
			suppDoc2.CSI_Code = "X002";
			suppDoc2.CSI_ReferenceNumber = "ES3600000002";

			var error1 = SetUpError("211", "CABECERA.TIPO DE PROCEDIMIENTO  (CAS. 1.2)..", "EL TIPO DE PROCEDIMIENTO NO ES VALIDO. -VALOR X-.");
			var error2 = SetUpError("00338", "PARTIDA(001).CASILLA 44 (001).Tipo de documento.Other Text", "Código  documento incorrecto.More Text");
			var error3 = SetUpError("55000", "Other Error.", "Other Description..");

			processor = GetMockedProcessor(GetMockedProvider("963", entryHeader.CH_BGMReference, MovementReferenceNumberReject, ZDateTime.Empty, "2", ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, ZString.Empty, ZDateTime.Empty, ZString.Empty, new List<ErrorMessage> { error1, error2, error3 }));

			processor.ProcessMessage(message);
			GenericCommonAssertProcessEntryData(message, entryHeader, messageSubType: "REJ", entryStatusCode: OriginalEntryStatus, movementReferenceNumber: MovementReferenceNumberReject, messageNum: "1");

			CombineAssertions(() =>
			{
				var clSupDocs = GetCLSupportingDocuments();
				AssertEquals("There are 0 CL SupportingDocuments after processing", 0, clSupDocs.Length);
			});
		}

		public void TestMessageProcessingError()
		{
			message.EM_LinkedObject = null;

			processor = GetMockedProcessor(GetMockedProvider("962", entryHeader.CH_BGMReference, MrnCode, acceptanceDate, "4", ZString.Empty, "A2", "1", limitDateOfArrival, CSVLevante, entryReleaseDate, CSVT2L));

			processor.ProcessMessage(message);

			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessage.Status.Failed, message.EM_Status);
				AssertContains("Log has error", "Unable to read message text from message", logger.UserLogStrings[0]);
				AssertContains("Log has exception", "Message's Linked Object is null so can't continue with processing", logger.UserLogStrings[1]);
			});
		}

		public void TestErrorResponse()
		{
			SetSentInterchange(entryHeader, InterchangeID);
			var responseMessage = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetEdifactErrorMessageFile(), InterchangeID, serializeMessageText: false);

			ProcessMessageForTest(responseMessage);
			var expectedMessageInterpretationTextRejected = "<H3>Rejected Declaration</H3>" +
						"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
						"<tr><td><strong>Error</strong></td><td><strong>Description</strong></td></tr>" +
						"<tr><td>50052</td><td>Error Traducción:Segmento (TPL) Mensaje erroneo</td></tr>" +
						"</table>";
			GenericCommonAssertProcessEntryData(responseMessage, entryHeader, expectedMessageInterpretation: expectedMessageInterpretationTextRejected, messageSubType: "REJ", entryStatusCode: OriginalEntryStatus);
		}

		public void TestProcessMessageWrongXML()
		{
			SetSentInterchange(entryHeader, InterchangeID);
			var responseMessage = CreateNewEDIMessage(entryHeader.CH_BGMReference, WrongXMLTestFile, InterchangeID);

			ProcessMessageForTest(responseMessage);

			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessage.Status.Failed, responseMessage.EM_Status);
				AssertContains("logger", "Unable to read message text from message", logger.UserLogStrings[0]);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var staffWithCertificateHelperTest = new StaffWithCertificateTestHelper(Factory);

			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_GS_NKCusAgent = staffWithCertificateHelperTest.Staff.GS_Code;
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_FullName = DeclarantName;
			declarant.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, DeclarantId);
			declaration.Declarant.OA_OH = declarant.PK;
			var invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_SubStyle = "B";
			invoiceLine.JI_CEI = instruction.PK;
			entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			entryHeader.CH_EntryStatus = OriginalEntryStatus;
			entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine.JI_CL = entryLine.PK;

			receivedInterchange = CreateTestResponseInterchange(InterchangeID, EDIInterchange.TransportType.eHub);

			message = Factory.New<TestEdiMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			message.EM_MessageType = DeclarationMessageTypeList.Codes.Export;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = "Message text";
			message.EM_LinkedObject = entryHeader;
			message.EM_EI = receivedInterchange.PK;

			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "Customs Status");

			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(EsCode, parent: grouping);
			helper.CreateCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, OriginalEntryStatus, "Original Entry Status", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CLP", "Cleared with pending complementary declarations", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CLR", "Customs Declaration Accepted and Cleared", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CDA", "Customs Declaration accepted", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "PDA", "Customs Pre-Declaration accepted", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			Factory.Save();
			entryStatusList = RefCusCodeListTypes.GetCachedList(Factory, EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, ZDateTime.Today);
		}
		protected CodeDescriptionPairList entryStatusList;
		protected JobDeclaration declaration;
		protected CusEntryInstruction instruction;
		protected CusEntryHeader entryHeader;
		protected CusEntryLine entryLine;
		EDIInterchange receivedInterchange;
		protected TestEdiMessage message;
		protected JobComInvoiceLine invoiceLine;

		string GetEdifactErrorMessageFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.EdifactTestFilePath, "EdifactErrorMessage.txt");

		protected const string MrnCode = "20ES00999910000035";
		protected const string CSVT2L = "3J9MCS7TAM3KLZQC";
		protected const string CSVLevante = "LT6B5QJ98HC6DHNY";
		const string MovementReferenceNumberReject = "@E2018/00079";
		protected readonly ZDateTime entryReleaseDate = new ZDateTime(2020, 10, 20, 15, 50, 00);
		protected readonly ZDateTime acceptanceDate = new ZDateTime(2020, 01, 02, 11, 00, 00);
		protected readonly ZDateTime limitDateOfArrival = new ZDateTime(2020, 04, 01);

		const string DeclarantId = "NIF22222222";
		const string DeclarantName = "Declarant Full Name";

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

		protected ICUSRESMessageProvider GetMockedProvider(ZString messageName, ZString referenceNumber, ZString registrationNumber, ZDateTime admissionDate, ZString messageFunction, ZString messageFunctionCan, ZString customsClearanceStatus, ZString printActionRequired, ZDateTime transitMaxDate, ZString csvReleaseCode, ZDateTime csvReleaseCreationDate, ZString csvT2LFCode, List<ErrorMessage> errorList = null)
		{
			var mockTestHelper = new Mock<IExportResponseMessageProvider>();
			mockTestHelper.Setup(m => m.DocumentMessageName).Returns(messageName);
			mockTestHelper.Setup(m => m.UniqueReferenceNumber).Returns(referenceNumber);
			mockTestHelper.Setup(m => m.RegistrationNumber).Returns(registrationNumber);
			mockTestHelper.Setup(m => m.AdmissionDate).Returns(admissionDate);
			mockTestHelper.Setup(m => m.MessageFunction).Returns(messageFunction);
			mockTestHelper.Setup(m => m.MessageFunctionCAN).Returns(messageFunctionCan);
			mockTestHelper.Setup(m => m.CustomsClearanceStatus).Returns(customsClearanceStatus);
			mockTestHelper.Setup(m => m.PrintActionRequired).Returns(printActionRequired);
			mockTestHelper.Setup(m => m.TransitMaxDate).Returns(transitMaxDate);
			mockTestHelper.Setup(m => m.CSVReleaseCode).Returns(csvReleaseCode);
			mockTestHelper.Setup(m => m.CSVReleaseCreationDate).Returns(csvReleaseCreationDate);
			mockTestHelper.Setup(m => m.CSVT2LFCode).Returns(csvT2LFCode);
			mockTestHelper.Setup(m => m.FreeTextErrors).Returns(errorList);
			return mockTestHelper.Object;
		}

		protected string ExpectedMessageInterpretationText => "<H3>Accepted Declaration</H3>" +
			"<table border=\"0\">" +
			"<tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>02-01-2020, 11:00:00</td></tr>" +
			"<tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>" + MrnCode + "</td></tr>" +
			"</table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>LT6B5QJ98HC6DHNY</td></tr>" +
			"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>20-10-2020, 15:50:00</td></tr>" +
			"<tr><td>Clearance Criteria:</td><td>&nbsp;&nbsp;</td><td>(A2) Considered Satisfactory</td></tr>" +
			"<tr><td>Print Procedure:</td><td>&nbsp;&nbsp;</td><td>EAD printed by Customs authorities or through the Virtual Office</td></tr>" +
			"</table>";

		protected string ExpectedMessageInterpretationTextComplementary => "<H3>Accepted Declaration</H3>" +
			"<table border=\"0\">" +
			"<tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>02-01-2020, 11:00:00</td></tr>" +
			"<tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>" + MrnCode + "</td></tr>" +
			"</table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong>Complementary</strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>20-10-2020, 15:50:00</td></tr>" +
			"<tr><td>Clearance Criteria:</td><td>&nbsp;&nbsp;</td><td>(A2) Considered Satisfactory</td></tr>" +
			"<tr><td>Print Procedure:</td><td>&nbsp;&nbsp;</td><td>EAD printed by Customs authorities or through the Virtual Office</td></tr>" +
			"</table>";

		protected string MessageText => ZString.Format("UNH+02110053624233+CUSRES:1:921:UN:ECS003'BGM+962+S900052890/1+11'NAD+EX+A78587268:167:148'NAD+1+ESA78587268:167:148'DTM+148:2001021100:201'DTM+268:20200401:102'GIS+4:117:148'GIS+24:119:A2:1'RFF+ABT:{0}'AUT+LT6B5QJ98HC6DHNY+LEVA'DTM+204:2010201550:201'AUT+3J9MCS7TAM3KLZQC+T2LF'DTM+204:2001021100:201'UNT+14+02110053624233'UNZ+1+02110053624233'", MrnCode);
		protected string MessageTextComplementary => ZString.Format("UNH+02110053624233+CUSRES:1:921:UN:ECS003'BGM+962+S900052890/1+11'NAD+EX+A78587268:167:148'NAD+1+ESA78587268:167:148'DTM+148:2001021100:201'DTM+268:20200401:102'GIS+3:117:148'GIS+24:119:A2:1'RFF+ABT:{0}'AUT++LEVA'DTM+204:2010201550:201'AUT+3J9MCS7TAM3KLZQC+T2LF'DTM+204:2001021100:201'UNT+14+02110053624233'UNZ+1+02110053624233'", MrnCode);

		protected void CommonSendAssertGreenCircuitAccepted(TestEdiMessage message, string entryStatusCode, string messageNum)
		{
			GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: ExpectedMessageInterpretationText, messageSubType: "ACC", entryStatusCode: entryStatusCode, circuit: CircuitCodeList.Codes.GREEN, clearanceResult: ClearanceResultCodeList.Codes.A2, eadPrintProcedure: EADPrintProcedureCodeList.Codes._1EADPrintedByCustomsAuthorities, acceptanceDate: acceptanceDate, limitDateOfArrival: limitDateOfArrival, movementReferenceNumber: MrnCode, csvClearance: CSVLevante, entryReleaseDate: entryReleaseDate, csvT2L: CSVT2L, messageNum: messageNum);
		}

		protected void CommonSendAssertGreenCircuitCSVClearance(TestEdiMessage message, string messageNum = "1")
		{
			CommonSendAssertGreenCircuitAccepted(message, EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, messageNum);
		}

		protected void CommonSendAssertCSVClearanceComplementary(TestEdiMessage message, string messageNum = "1")
		{
			GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: ExpectedMessageInterpretationTextComplementary, messageSubType: "ACC", entryStatusCode: EntryStatusCodes.Cleared, clearanceResult: ClearanceResultCodeList.Codes.A2, eadPrintProcedure: EADPrintProcedureCodeList.Codes._1EADPrintedByCustomsAuthorities, acceptanceDate: acceptanceDate, limitDateOfArrival: limitDateOfArrival, movementReferenceNumber: MrnCode, entryReleaseDate: entryReleaseDate, csvT2L: CSVT2L, messageNum: messageNum);
		}

		protected abstract TResponse GetMockedProcessor(ICUSRESMessageProvider messageProvider);

		ErrorMessage SetUpError(ZString code, ZString location, ZString description)
		{
			return new ErrorMessage
			{
				Code = code,
				Location = location,
				Description = description
			};
		}
	}
}

