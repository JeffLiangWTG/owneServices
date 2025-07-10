using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Testing;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class EXSResponseMessageProcessorTest : XMLResponseMessageProcessorTest<EXSResponseMessageProcessor, IMessagePrettyFormatter, EXSResponse>
	{
		public void TestProcessAcceptedMessage_GreenCircuit_Presentation()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileGreenCircuit, InterchangeID);

			ProcessMessageForTest(message);

			AssertEXSMessage(message, expectedMessageInterpretation: acceptedGreenCircuitMessageInterpretation, isTestMessage: true, entryStatusCode: EntryStatusCodes.Cleared, csvClearance: CSVClearance, acceptanceDate: acceptanceDate, circuit: CircuitCodeList.Codes.GREEN);
		}

		public void TestProcessAcceptedMessage_GreenCircuit_Amendment()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileGreenCircuitAmendment, InterchangeID);

			ProcessMessageForTest(message);

			AssertEXSMessage(message, expectedMessageInterpretation: acceptedGreenCircuitMessageInterpretation, entryStatusCode: EntryStatusCodes.Cleared, csvClearance: CSVClearance, acceptanceDate: acceptanceDate, circuit: CircuitCodeList.Codes.GREEN);
		}

		public void TestProcessAcceptedMessage_RedCircuit()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileRedCircuit, InterchangeID);

			var expectedMessageInterpretation =
				"<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>ZJVWHZ8FV9E5S8HY</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Type:</td><td>&nbsp;&nbsp;</td><td>Exit Summary Declaration Express Consignment</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>09-08-2022, 11:58:00</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>MRN:</td><td>&nbsp;&nbsp;</td><td>22ES00999960005384</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F00000\">RED</font></strong></td></tr></table>";

			ProcessMessageForTest(message);

			AssertEXSMessage(message, expectedMessageInterpretation: expectedMessageInterpretation, isTestMessage: true, entryStatusCode: EntryStatusCodes.CustomsDeclarationAccepted, acceptanceDate: acceptanceDate, circuit: CircuitCodeList.Codes.RED);
		}

		public void TestProcessAcceptedMessage_OrangeCircuit()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileOrangeCircuit, InterchangeID);

			var expectedMessageInterpretation =
				"<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>ZJVWHZ8FV9E5S8HY</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Type:</td><td>&nbsp;&nbsp;</td><td>Reshipment Notification</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>09-08-2022, 11:58:00</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>MRN:</td><td>&nbsp;&nbsp;</td><td>22ES00999960005384</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>";

			ProcessMessageForTest(message);

			AssertEXSMessage(message, expectedMessageInterpretation: expectedMessageInterpretation, entryStatusCode: EntryStatusCodes.CustomsDeclarationAccepted, acceptanceDate: acceptanceDate, circuit: CircuitCodeList.Codes.ORANGE);
		}

		public void TestProcessAcceptedMessage_Cancellation()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileCancellation, InterchangeID);

			var expectedMessageInterpretation =
				"<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>ZJVWHZ8FV9E5S8HY</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Type:</td><td>&nbsp;&nbsp;</td><td>Exit Summary Declaration</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>09-08-2022, 11:58:00</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>MRN:</td><td>&nbsp;&nbsp;</td><td>22ES00999960005384</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>";

			ProcessMessageForTest(message);

			AssertEXSMessage(message, expectedMessageInterpretation: expectedMessageInterpretation, entryStatusCode: EntryStatusCodes.Cancelled, acceptanceDate: acceptanceDate);
		}

		public void TestCreateDocumentCaptureRequestWhenCSVClearanceNew_AllDocs()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileGreenCircuit, InterchangeID);

			ProcessMessageForTest(message);

			AssertEXSMessage(message, expectedMessageInterpretation: acceptedGreenCircuitMessageInterpretation, isTestMessage: true, entryStatusCode: EntryStatusCodes.Cleared, csvClearance: CSVClearance, acceptanceDate: acceptanceDate, circuit: CircuitCodeList.Codes.GREEN);

			CombineAssertions(() =>
			{
				entryHeader.Messages.Reload(true);
				var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
				AssertEquals("Doc Messages sent number is", 1, docMessages.Length);

				AssertDocumentRequestEDIMessages(docMessages, new List<(ZString fileName, ZString urlParameter)>() { (MRNCode + "_E_AEAT_EXS_CLR.pdf", CSVClearance) });
			});
		}

		public void TestCreateDocumentCaptureRequestWhenCSVClearanceChanged_AllDocs()
		{
			var docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
			docManagerInfo.AddFileOrDocument(new byte[1], MRNCode + "_E_AEAT_EXS_CLR.pdf", "CLR");
			docManagerInfo.Save();

			entryHeader.SetCSVClearanceNum("AAAAAAAAAAAAAAAA");

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileGreenCircuitAmendment, InterchangeID);

			ProcessMessageForTest(message);

			AssertEXSMessage(message, expectedMessageInterpretation: acceptedGreenCircuitMessageInterpretation, entryStatusCode: EntryStatusCodes.Cleared, csvClearance: CSVClearance, acceptanceDate: acceptanceDate, circuit: CircuitCodeList.Codes.GREEN);

			CombineAssertions(() =>
			{
				entryHeader.Messages.Reload(true);
				var eDocs = docManagerInfo.GetRelatedEDocs();
				AssertEquals("Number of eDocs is correct", 1, eDocs.Count());

				var eDocNamesChanged = new List<ZString>() { MRNCode + "_E_AEAT_EXS_CLR_OLD_AAAAAAAAAAAAAAAA.pdf" };
				AssertContainsExactElementsInAnyOrder("eDocs contains existing document with name changed", eDocNamesChanged, eDocs.Cast<IeDoc>().Select(x => x.FileName).ToList());

				var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
				AssertEquals("Doc Messages sent number is", 1, docMessages.Length);

				AssertDocumentRequestEDIMessages(docMessages, new List<(ZString fileName, ZString urlParameter)>() { (MRNCode + "_E_AEAT_EXS_CLR.pdf", CSVClearance) });
			});
		}

		public void TestCreateDocumentCaptureRequestWhenCSVClearance_NoDocs()
		{
			var docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
			docManagerInfo.AddFileOrDocument(new byte[1], MRNCode + "_E_AEAT_EXS_CLR.pdf", "CLR");
			docManagerInfo.Save();

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileGreenCircuit, InterchangeID);

			ProcessMessageForTest(message);

			AssertEXSMessage(message, expectedMessageInterpretation: acceptedGreenCircuitMessageInterpretation, isTestMessage: true, entryStatusCode: EntryStatusCodes.Cleared, csvClearance: CSVClearance, acceptanceDate: acceptanceDate, circuit: CircuitCodeList.Codes.GREEN);

			var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
			AssertEquals("Doc Messages sent number is", 0, docMessages.Length);
		}

		public void TestProcessRejectedMessage_616()
		{
			AddMessageProcessAndAssertResult_RejectedMessage616();
		}

		public void TestProcessRejectedMessage_919()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, RejectedWithXMLErrorTestFile, InterchangeID);

			var expectedMessageInterpretation =
				"<H3>Rejected Declaration (XML Error)</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Code</strong></td><td><strong>Place</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td><td><strong>Location</strong></td></tr>" +
				"<tr><td>18</td><td>&nbsp;</td><td>Unspecified Error / Other</td><td>Etiqueta MesRecMES6 no encontrada</td><td>MES.MesRecMES6</td></tr>" +
				"<tr><td>52</td><td>&nbsp;</td><td>cvc-complex-type.2.4.a: se ha encontrado un contenido no válido que comienza con el elemento 'MesSenMES3'. Se esperaba uno de 'MesSenMES3'.</td><td>&nbsp;</td><td>CC615A</td></tr>" +
				"<tr><td>35</td><td>1</td><td>Too many repetitions</td><td>501</td><td>MES.HEA.GOOITEGDS</td></tr>" +
				"<tr><td>35</td><td>1</td><td>Too many repetitions</td><td>601</td><td>MES.HEA.ITI</td></tr>" +
				"</table>";

			ProcessMessageForTest(message);

			AssertEXSMessage(message, expectedMessageInterpretation: expectedMessageInterpretation, messageSubType: "REJ", movementReferenceNumber: ZString.Empty);
		}

		public void TestProcessWithoutValidationWhenValidationError()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileGreenCircuitWithoutFirstTag, InterchangeID);

			ProcessMessageForTest(message);

			var concatenatedUserLogStrings = GetAllConcatenatedUserLogStrings();
			AssertContains("logger", "Validation error for message: Type:EXS, Ref:" + entryHeader.CH_BGMReference + ", Text:", concatenatedUserLogStrings);
			AssertContains("logger exception", "MesSenMES3", concatenatedUserLogStrings);

			AssertEXSMessage(message, expectedMessageInterpretation: acceptedGreenCircuitMessageInterpretation, isTestMessage: true, entryStatusCode: EntryStatusCodes.Cleared, csvClearance: CSVClearance, acceptanceDate: acceptanceDate, circuit: CircuitCodeList.Codes.GREEN);
		}

		public void TestProcessRejectedMessage_EntryStatusEmpty_TemporaryStorageRegisterNotEnabled()
		{
			var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var (regLineTransaction1, regLineTransaction2, regLineTransaction3) = SetUpTransactionsForRejectedTestForTemporaryStorageGoodsConsumption(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);

				entryHeader.CH_EntryStatus = ZString.Empty;

				AddMessageProcessAndAssertResult_RejectedMessage616(true);

				CombineAssertions(() =>
				{
					AssertEquals("regLineTransaction1's SRT_TransactionType was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction1.SRT_TransactionStatus);
					AssertEquals("regLineTransaction2's SRT_TransactionType was not changed (not PND)", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction2.SRT_TransactionStatus);
					AssertEquals("regLineTransaction3's SRT_TransactionType was not changed (wrong internalRefType)", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction3.SRT_TransactionStatus);
				});
			}
		}

		public void TestProcessRejectedMessage_EntryStatusEmpty_TemporaryStorageEnabled()
		{
			var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (regLineTransaction1, regLineTransaction2, regLineTransaction3) = SetUpTransactionsForRejectedTestForTemporaryStorageGoodsConsumption(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);

				entryHeader.CH_EntryStatus = ZString.Empty;

				AddMessageProcessAndAssertResult_RejectedMessage616(true);

				CombineAssertions(() =>
				{
					AssertEquals("regLineTransaction1's SRT_TransactionType was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
					AssertEquals("regLineTransaction2's SRT_TransactionType was not changed (not PND)", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction2.SRT_TransactionStatus);
					AssertEquals("regLineTransaction3's SRT_TransactionType was not changed (wrong internalRefType)", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction3.SRT_TransactionStatus);
				});
			}
		}

		public void TestProcessRejectedMessage_EntryStatusNotEmpty_TemporaryStorageEnabled()
		{
			var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (regLineTransaction1, regLineTransaction2, regLineTransaction3) = SetUpTransactionsForRejectedTestForTemporaryStorageGoodsConsumption(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);

				AddMessageProcessAndAssertResult_RejectedMessage616(false);

				CombineAssertions(() =>
				{
					AssertEquals("regLineTransaction1's SRT_TransactionType was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
					AssertEquals("regLineTransaction2's SRT_TransactionType was not changed (not PND)", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction2.SRT_TransactionStatus);
					AssertEquals("regLineTransaction3's SRT_TransactionType was not changed (wrong internalRefType)", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction3.SRT_TransactionStatus);
				});
			}
		}

		public void TestLoggerWriteOffTransactionError()
		{
			var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				SetUpDataForConfirmTemporaryStorageGoodsConsumptionLoggerWriteOffTransactionError(entryHeader, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);

				var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileGreenCircuit, InterchangeID);

				ProcessMessageForTest(message);
			}

			var expectedError = "Reference reference has a positive balance of 2.00 EUR. Please check the existing transactions for this reference and create a manual adjustment if needed.";
			AssertContains("logger exception", expectedError, GetAllConcatenatedUserLogStrings());
		}

		void AddMessageProcessAndAssertResult_RejectedMessage616(bool isEntryStatusEmpty = false)
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetRejectedTestFile(), InterchangeID);

			var expectedMessageInterpretation =
				"<H3>Rejected Declaration</H3>" +
				"<table border=\"0\"><tr><td>Reason:</td><td>&nbsp;&nbsp;</td><td>EXS00011 Razon</td></tr></table>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Code</strong></td><td><strong>Place</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>83</td><td>MES.HEA.GDS(1).DocRefPD12</td><td>&nbsp;</td><td>9999012345600001. DECLARACION SUMARIA INEXISTENTE Sumaria: 20ES00999981234560</td></tr>" +
				"<tr><td>84</td><td>MES.HEA.GDS(1).PACGS2</td><td>Razon</td><td>Valor original error</td></tr>" +
				"</table>";

			ProcessMessageForTest(message);

			AssertEXSMessage(message, expectedMessageInterpretation: expectedMessageInterpretation, messageSubType: "REJ", isTestMessage: true, entryStatusCode: isEntryStatusEmpty ? ZString.Empty : OriginalEntryStatus, movementReferenceNumber: ZString.Empty);
		}

		void AssertEXSMessage(TestEdiMessage message, string expectedMessageInterpretation = "", string messageSubType = "ACC", bool isTestMessage = false, string entryStatusCode = "INI", string movementReferenceNumber = MRNCode, string csvClearance = "", ZDateTime? acceptanceDate = null, string circuit = "")
		{
			AssertEquals("EM_IsTestMessage", isTestMessage, message.EM_IsTestMessage);

			GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, messageNum: MessageNum, messageSubType: messageSubType, chStatus: "RCV", entryStatusCode: entryStatusCode, movementReferenceNumber: movementReferenceNumber, csvClearance: csvClearance, acceptanceDate: acceptanceDate, entryReleaseDate: csvClearance.IsNullOrEmpty() ? null : acceptanceDate, circuit: circuit);
		}

		protected override void AssertLoggerMessagesWhenProcessMessageWrongText(TestEdiMessage message, BusinessObject businessObject)
		{
			var concatenatedUserLogStrings = GetAllConcatenatedUserLogStrings();
			AssertContains("logger", "Unable to read message text from message ", concatenatedUserLogStrings);
			AssertContains("logger exception", "The Response is not a correct EXS response", concatenatedUserLogStrings);
			AssertEquals("EM_MessageInterpretation",
					string.Format("<H3>Processor Failure</H3><br>" +
					"<H4>Failure: Unable to read message text from message (Number:{0}, Type:{1}, Sub:{2}, Ref:{3}); message status set to Failed.</H4>" +
					"<H4>Exception: The Response is not a correct EXS response</H4>", message.EM_MessageNum, message.EM_MessageType, message.EM_MessageSubType, message.EM_ApplicationReference)
					, message.EM_MessageInterpretation);
		}

		protected override void SetUp()
		{
			base.SetUp();

			entryHeader.CH_EntryStatus = OriginalEntryStatus;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = Declaration.ExsEntrySubStyleList.Codes.EXS;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			var sentInterchange = SetSentInterchange(entryHeader, InterchangeID);
			sentMessage = (TestEdiMessage)sentInterchange.ContainedMessages[0];

			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "Customs Status");

			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(EsCode, parent: grouping);
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "INI", "Original Entry Status", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CAN", "Cancelled", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CLR", "Customs Declaration Accepted and Cleared", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CDA", "Customs Declaration Accepted", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			Factory.Save();
			RefCusCodeListTypes.GetCachedList(Factory, EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, ZDateTime.Today);
		}

		protected TestEdiMessage sentMessage;

		const string MRNCode = "22ES00999960005384";
		const string MessageNum = "20220000002370";
		const string CSVClearance = "DZ9VFA2VYQVMXL45";
		readonly ZDateTime acceptanceDate = new ZDateTime(2022, 08, 09, 11, 58, 00);

		string AcceptanceTestFileGreenCircuit => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.EXSTestFilePath, "AcceptedGreenCircuitMessage.txt");
		string AcceptanceTestFileGreenCircuitAmendment => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.EXSTestFilePath, "AcceptedGreenCircuitAmendmentMessage.txt");
		string AcceptanceTestFileRedCircuit => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.EXSTestFilePath, "AcceptedRedCircuitMessage.txt");
		string AcceptanceTestFileOrangeCircuit => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.EXSTestFilePath, "AcceptedOrangeCircuitMessage.txt");
		string AcceptanceTestFileCancellation => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.EXSTestFilePath, "AcceptedCancellationMessage.txt");
		string GetRejectedTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.EXSTestFilePath, "RejectedMessage.txt");
		string RejectedWithXMLErrorTestFile => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.EXSTestFilePath, "RejectedWithXMLErrorMessage.txt");
		string AcceptanceTestFileGreenCircuitWithoutFirstTag => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.EXSTestFilePath, "AcceptedGreenCircuitMessageWithoutFirstTag.txt");
		protected override string GetAcceptanceTestFileWithLongSegmentId() => string.Empty;

		readonly ZString acceptedGreenCircuitMessageInterpretation = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>ZJVWHZ8FV9E5S8HY</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Type:</td><td>&nbsp;&nbsp;</td><td>Exit Summary Declaration</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>09-08-2022, 11:58:00</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>MRN:</td><td>&nbsp;&nbsp;</td><td>22ES00999960005384</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>DZ9VFA2VYQVMXL45</td></tr></table>";

		protected override ZString GetExpectedProcessorFriendlyName() => "Exit Summary Declaration Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.ExitSummaryDeclaration };

		protected override EXSResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new EXSResponseMessageProcessor(logger, new BranchCustomsMessageProcessorForTest());
	}
}
