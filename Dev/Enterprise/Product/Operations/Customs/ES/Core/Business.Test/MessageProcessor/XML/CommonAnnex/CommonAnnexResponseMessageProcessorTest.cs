using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.CommonAnnex.EnvioDeDocumentosV1Sal;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ES.Business.Testing
{
	class CommonAnnexResponseMessageProcessorTest : XMLResponseMessageProcessorTest<CommonAnnexResponseMessageProcessor, IMessagePrettyFormatter, EnvioDeDocumentosV1Sal>
	{
		public void TestProcessAcceptedMessage_NotLastAnnex_RequestDispatchN()
		{
			entryHeader.ZG_RequestDispatch = "N";
			AddAnnexes(true);
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);
			AssertAcceptedT2LAnnex(message, expectedMessageInterpretation: acceptedDeclarationInterpretation);

			AssertNoTrigger();
		}

		public void TestProcessAcceptedMessage_NotLastAnnex_RequestDispatchY()
		{
			entryHeader.ZG_RequestDispatch = "Y";
			AddAnnexes(true);
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);
			AssertAcceptedT2LAnnex(message, chStatus: "AWR", expectedMessageInterpretation: acceptedDeclarationInterpretation, requestDispatch: "Y");

			CombineAssertions(() =>
			{
				entryHeader.Messages.Reload(true);
				var msgs = entryHeader.Messages.Cast<EDIMessage>().Where(x => x.EM_ReceiveTransmit == "TRX" && x.EM_MessageText != SentMessageText).ToList();
				AssertEquals("There are 2 extra TRX EDIMessages created for the entryHeader", 2, msgs.Count);
				AssertEquals("New EDIMessages are type T2D", true, msgs.All(x => x.EM_MessageType == MessageType));
				AssertContains("New EDIMessages have dispatch request flag to N because there is one annex left to send, first", "<Valor>N</Valor>", msgs[0].EM_MessageText);
				AssertContains("New EDIMessages have dispatch request flag to N because there is one annex left to send, second", "<Valor>N</Valor>", msgs[1].EM_MessageText);
			});
		}

		public void TestProcessAcceptedMessage_NotLastAnnex_RequestDispatchY_LastResponseCHStatusRejected()
		{
			entryHeader.ZG_RequestDispatch = "Y";
			AddAnnexes(true);
			entryHeader.CH_Status = "REJ";
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);
			AssertAcceptedT2LAnnex(message, chStatus: "REJ", expectedMessageInterpretation: acceptedDeclarationInterpretation, requestDispatch: "Y");

			AssertNoTrigger();
		}

		public void TestProcessAcceptedMessage_NotLastAnnex_RequestDispatchY_LastResponseCHStatusFailed()
		{
			entryHeader.ZG_RequestDispatch = "Y";
			AddAnnexes(true);
			entryHeader.CH_Status = "FAL";
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);
			AssertAcceptedT2LAnnex(message, chStatus: "FAL", expectedMessageInterpretation: acceptedDeclarationInterpretation, requestDispatch: "Y");

			AssertNoTrigger();
		}

		public void TestProcessAcceptedMessage_NotLastAnnex_RequestDispatchY_AnnexWaitingForResponse()
		{
			entryHeader.ZG_RequestDispatch = "Y";
			AddAnnexes(true);
			AddExtraAnnexSent();
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);
			AssertAcceptedT2LAnnex(message, chStatus: ZString.Empty, expectedMessageInterpretation: acceptedDeclarationInterpretation, requestDispatch: "Y");

			AssertNoTrigger();
		}

		public void TestProcessAcceptedMessage_LastAnnex_RequestDispatchN()
		{
			entryHeader.ZG_RequestDispatch = "N";
			AddAnnexes(false);
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);
			AssertAcceptedT2LAnnex(message, expectedMessageInterpretation: acceptedDeclarationInterpretation);

			AssertNoTrigger();
		}

		public void TestProcessAcceptedMessage_LastAnnex_RequestDispatchY()
		{
			entryHeader.ZG_RequestDispatch = "Y";
			AddAnnexes(false);
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);
			AssertAcceptedT2LAnnex(message, chStatus: "AWR", expectedMessageInterpretation: acceptedDeclarationInterpretation, requestDispatch: "Y");

			CombineAssertions(() =>
			{
				entryHeader.Messages.Reload(true);
				var msgs = entryHeader.Messages.Cast<EDIMessage>().Where(x => x.EM_ReceiveTransmit == "TRX" && x.EM_MessageText != SentMessageText).ToList();
				AssertEquals("There is 1 extra TRX EDIMessages created for the entryHeader", 1, msgs.Count);
				AssertEquals("New EDIMessages are type T2D", true, msgs.All(x => x.EM_MessageType == MessageType));
				AssertContains("New EDIMessage has dispatch request flag to S cause they are sent as last", "<Valor>S</Valor>", msgs[0].EM_MessageText);

				var pivotsForFirstMessage = entryHeader.EDocPivotCollection.Cast<CusStorageDocPivot>().Where(x => x.Message == msgs[0]);
				AssertEquals("First new EDIMessage is associated to 1 of the sent annexes", 1, pivotsForFirstMessage.Count());
			});
		}

		public void TestProcessAcceptedMessage_NoExtraAnnex_RequestDispatchY()
		{
			entryHeader.ZG_RequestDispatch = "Y";
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);
			AssertAcceptedT2LAnnex(message, expectedMessageInterpretation: acceptedDeclarationInterpretation, requestDispatch: "Y");

			CombineAssertions(() =>
			{
				AssertNoTrigger();

				AssertEquals("logger has no exceptions", ZString.Empty, GetAllConcatenatedUserLogStrings());
			});
		}

		public void TestProcessAcceptedMessage_LastAnnex_RequestDispatchY_LastResponseCHStatusRejected()
		{
			entryHeader.ZG_RequestDispatch = "Y";
			AddAnnexes(false);
			entryHeader.CH_Status = "REJ";
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);
			AssertAcceptedT2LAnnex(message, chStatus: "REJ", expectedMessageInterpretation: acceptedDeclarationInterpretation, requestDispatch: "Y");

			AssertNoTrigger();
		}

		public void TestProcessAcceptedMessage_LastAnnex_RequestDispatchY_LastResponseCHStatusFailed()
		{
			entryHeader.ZG_RequestDispatch = "Y";
			AddAnnexes(false);
			entryHeader.CH_Status = "FAL";
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);
			AssertAcceptedT2LAnnex(message, chStatus: "FAL", expectedMessageInterpretation: acceptedDeclarationInterpretation, requestDispatch: "Y");

			AssertNoTrigger();
		}

		public void TestProcessAcceptedMessage_LastAnnex_RequestDispatchY_AnnexWaitingForResponse()
		{
			entryHeader.ZG_RequestDispatch = "Y";
			AddAnnexes(false);
			AddExtraAnnexSent();
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);
			AssertAcceptedT2LAnnex(message, chStatus: ZString.Empty, expectedMessageInterpretation: acceptedDeclarationInterpretation, requestDispatch: "Y");

			AssertNoTrigger();
		}

		public void TestProcessRejectedMessage_RequestDispatchN()
		{
			entryHeader.ZG_RequestDispatch = "N";
			var responseMessage = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetRejectedTestFile(), InterchangeID);

			ProcessMessageForTest(responseMessage);
			AssertRejectedAndErrorT2LAnnex(responseMessage, expectedMessageInterpretation: rejectedMessageInterpretation);
		}

		public void TestProcessRejectedMessage_RequestDispatchY()
		{
			entryHeader.ZG_RequestDispatch = "Y";
			var responseMessage = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetRejectedTestFile(), InterchangeID);

			ProcessMessageForTest(responseMessage);
			AssertRejectedAndErrorT2LAnnex(responseMessage, expectedMessageInterpretation: rejectedMessageInterpretation);
		}

		void AssertAcceptedT2LAnnex(TestEdiMessage message, string chStatus = "RCV", string expectedMessageInterpretation = "", string requestDispatch = "N")
		{
			AssertT2LAnnex(message, messageSubType: "ACC", chStatus: chStatus, expectedMessageInterpretation: expectedMessageInterpretation, requestDispatch: requestDispatch);

			CombineAssertions(() =>
			{
				AssertEquals("Sent message status is changed to received", EDIMessage.Status.Received, sentMessage.EM_Status);
				var pivotQuery = new ZQuery(GenPivotSchema.XX_RelationType, GenPivotTypes.CusStorageDocPivotEdiMessage);
				pivotQuery.AddToFilter(GenPivotSchema.XX_Relation2ID, message.PK);
				var genPivotResponseMessages = entryHeader.Factory.Load<GenPivot>(pivotQuery);
				AssertEquals("Received message is related to sent annexes through new GenPivot", 2, genPivotResponseMessages.Length);
			});
		}

		void AssertRejectedAndErrorT2LAnnex(TestEdiMessage message, string expectedMessageInterpretation = "")
		{
			AssertT2LAnnex(message, messageSubType: "REJ", emStatus: EDIMessage.Status.Rejected, chStatus: "REJ", expectedMessageInterpretation: expectedMessageInterpretation);

			CombineAssertions(() =>
			{
				AssertEquals("Sent message status is changed to rejected", EDIMessage.Status.Rejected, sentMessage.EM_Status);
				var pivotQuery = new ZQuery(GenPivotSchema.XX_RelationType, GenPivotTypes.CusStorageDocPivotEdiMessage);
				pivotQuery.AddToFilter(GenPivotSchema.XX_Relation2ID, message.PK);
				var genPivotResponseMessages = entryHeader.Factory.Load<GenPivot>(pivotQuery);
				AssertEquals("Received message is related to sent annexes through new GenPivot", 2, genPivotResponseMessages.Length);
			});
		}

		void AssertT2LAnnex(TestEdiMessage message, string messageSubType, string emStatus = EDIMessage.Status.Received, string chStatus = "RCV", string expectedMessageInterpretation = "", string requestDispatch = "")
		{
			AssertEquals("ZG_RequestDispatch", requestDispatch, entryHeader.ZG_RequestDispatch);
			GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, emStatus: emStatus, chStatus: chStatus, messageSubType: messageSubType, entryStatusCode: OriginalEntryStatus, movementReferenceNumber: OriginalMRNCode, messageNum: MessageNum);
		}

		void AssertNoTrigger()
		{
			entryHeader.Messages.Reload(true);
			var msgs = entryHeader.Messages.Cast<EDIMessage>().Where(x => x.EM_ReceiveTransmit == "TRX" && x.EM_MessageText != SentMessageText).ToList();
			AssertEquals("There are no extra TRX EDIMessages created for the entryHeader", 0, msgs.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var staffWithCertificateHelperTest = new StaffWithCertificateTestHelper(Factory);
			declaration.JE_GS_NKCusAgent = staffWithCertificateHelperTest.Staff.GS_Code;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;

			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryHeader.MovementReferenceNumber = OriginalMRNCode;
			entryHeader.CH_EntryStatus = OriginalEntryStatus;
			entryHeader.ZG_POUSVersion = 1;

			sentMessage = AddSentMessageAndEDocs();
		}
		TestEdiMessage sentMessage;

		void AddAnnexes(ZBool addExtraAnnex)
		{
			var eDoc1 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Doc1.txt", "CIV");
			var pivot1 = entryHeader.EDocPivotCollection.AddNew();
			pivot1.CSD_StorageDocReference = eDoc1.UniqueKey;

			if (addExtraAnnex)
			{
				var eDoc2 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Doc2.txt", "CIV");
				var pivot2 = entryHeader.EDocPivotCollection.AddNew();
				pivot2.CSD_StorageDocReference = eDoc2.UniqueKey;

				var eDoc3 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Doc3.txt", "MSC");
				var pivot3 = entryHeader.EDocPivotCollection.AddNew();
				pivot3.CSD_StorageDocReference = eDoc3.UniqueKey;
			}

			Factory.Save();
			declaration.DocManagerInfo.Save();
			entryHeader.EDocPivotCollection.Reload(true);
		}

		void AddExtraAnnexSent()
		{
			var eDoc1 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Extra1.txt", "CIV");
			var pivot1 = entryHeader.EDocPivotCollection.AddNew();
			pivot1.CSD_StorageDocReference = eDoc1.UniqueKey;

			var messageAnnexSent = Factory.New<TestEdiMessage>();
			messageAnnexSent.EM_MessageNum = SentMessageNumber;
			messageAnnexSent.EM_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			messageAnnexSent.EM_MessageType = MessageType;
			messageAnnexSent.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			messageAnnexSent.EM_Status = EDIMessage.Status.Sent;
			messageAnnexSent.EM_MessageText = SentMessageText;
			entryHeader.Messages.Add(messageAnnexSent);

			var messagePivot1 = Factory.New<GenPivot>();
			messagePivot1.XX_RelationType = GenPivotTypes.CusStorageDocPivotEdiMessage;
			messagePivot1.XX_Relation1ID = pivot1.PK;
			messagePivot1.XX_Relation1TableCode = pivot1.TablePrefix;
			messagePivot1.XX_Relation2ID = messageAnnexSent.PK;
			messagePivot1.XX_Relation2TableCode = messageAnnexSent.TablePrefix;
		}

		string GetAcceptanceTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.T2LPOUSAnnexTestFilePath, "AcceptedMessage.txt");

		string GetRejectedTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.T2LPOUSAnnexTestFilePath, "RejectedMessage.txt");

		protected override string GetAcceptanceTestFileWithLongSegmentId() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.T2LPOUSAnnexTestFilePath, "AcceptedMessageWithLongSegmentId.txt");

		protected override ZString GetExpectedProcessorFriendlyName() => "Common Annex Declaration Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.T2lDocumentationPous };

		protected override CommonAnnexResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new CommonAnnexResponseMessageProcessor(logger);

		readonly ZString rejectedMessageInterpretation = "<H3>Rejected Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Code:</td><td>&nbsp;&nbsp;</td><td>1021</td></tr></table>" +
				"<table border=\"0\"><tr><td>Error:</td><td>&nbsp;&nbsp;</td><td>En caso de estar anexando por Derecho a Ser Oido debe Solicitar Despacho</td></tr></table>";

		readonly ZString acceptedDeclarationInterpretation = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>VRMNB3HSMAFTYUDR</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Request Dispatch:</td><td>&nbsp;&nbsp;</td><td>S</td></tr></table>" +
				"<br><H4>CSV Electronic Documents</H4>" +
				"<br><table border=\"0\"><tr><td>CSV Document:</td><td>&nbsp;&nbsp;</td><td>SMPU4X96QKZZTZG9</td></tr></table>";

		const string OriginalMRNCode = "TestMRN";
		const string MessageNum = "20221014115635534071";
	}
}
