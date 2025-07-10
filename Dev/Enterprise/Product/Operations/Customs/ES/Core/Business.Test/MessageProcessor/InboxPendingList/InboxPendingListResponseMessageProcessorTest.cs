using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.InboxNotification.LISTADECV4SAL;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.ES.Business.ESConstants;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class InboxPendingListResponseMessageProcessorTest : ESBranchCustomsApplicationTypeMessageProcessorTest<InboxPendingListResponseMessageProcessor, ListaDecV4Sal>
	{
		public void TestProcessMessageWithOneElement_NPE()
		{
			ProcessOneElementForEntryHeader(DeclarationMessageTypeList.Codes.InBoxNotificationForExport, GetNPEMessageSingleElementTestFile(), extraType: DeclarationMessageTypeList.Codes.InBoxNotificationForImport, detailTypeIsV5: false);
		}

		public void TestProcessMessageWithMultipleElements_NPE()
		{
			ProcessMultipleElementsForEntryHeader(DeclarationMessageTypeList.Codes.InBoxNotificationForExport, GetNPEMessageMultipleElementsTestFile(), extraType: DeclarationMessageTypeList.Codes.InBoxNotificationForImport, detailTypeIsV5: false);
		}

		public void TestProcessMessageWithNoElementsInResponse_NPE()
		{
			ProcessNoElementsInResponseForEntryHeader(DeclarationMessageTypeList.Codes.InBoxNotificationForExport, InboxNotificationResponseTypes.Export, extraType: DeclarationMessageTypeList.Codes.InBoxNotificationForImport);
		}

		public void TestProcessMessageWithOneElement_NPI()
		{
			ProcessOneElementForEntryHeader(DeclarationMessageTypeList.Codes.InBoxNotificationForImport, GetNPIMessageSingleElementTestFile(), detailTypeIsV5: false);
		}

		public void TestProcessMessageWithMultipleElements_NPI()
		{
			ProcessMultipleElementsForEntryHeader(DeclarationMessageTypeList.Codes.InBoxNotificationForImport, GetNPIMessageMultipleElementsTestFile(), detailTypeIsV5: false);
		}

		public void TestProcessMessageWithNoElementsInResponse_NPI()
		{
			ProcessNoElementsInResponseForEntryHeader(DeclarationMessageTypeList.Codes.InBoxNotificationForImport, InboxNotificationResponseTypes.Import);
		}

		public void TestProcessMessageWithOneElement_INE()
		{
			ProcessOneElementForEntryHeader(DeclarationMessageTypeList.Codes.ExportInvalidationCommunication, GetINEMessageSingleElementTestFile());
		}

		public void TestProcessMessageWithMultipleElements_INE()
		{
			ProcessMultipleElementsForEntryHeader(DeclarationMessageTypeList.Codes.ExportInvalidationCommunication, GetINEMessageMultipleElementsTestFile());
		}

		public void TestProcessMessageWithNoElementsInResponse_INE()
		{
			ProcessNoElementsInResponseForEntryHeader(DeclarationMessageTypeList.Codes.ExportInvalidationCommunication, InboxNotificationResponseTypes.AESInvalidation);
		}

		public void TestProcessMessageWithOneElement_LVE()
		{
			ProcessOneElementForEntryHeader(DeclarationMessageTypeList.Codes.ExportClearanceCommunication, GetLVEMessageSingleElementTestFile());
		}

		public void TestProcessMessageWithMultipleElements_LVE()
		{
			ProcessMultipleElementsForEntryHeader(DeclarationMessageTypeList.Codes.ExportClearanceCommunication, GetLVEMessageMultipleElementsTestFile());
		}

		public void TestProcessMessageWithNoElementsInResponse_LVE()
		{
			ProcessNoElementsInResponseForEntryHeader(DeclarationMessageTypeList.Codes.ExportClearanceCommunication, InboxNotificationResponseTypes.AESClearance);
		}

		public void TestProcessMessageWithOneElement_DIE()
		{
			ProcessOneElementForEntryHeader(DeclarationMessageTypeList.Codes.ExportNonConformityCommunication, GetDIEMessageSingleElementTestFile());
		}

		public void TestProcessMessageWithMultipleElements_DIE()
		{
			ProcessMultipleElementsForEntryHeader(DeclarationMessageTypeList.Codes.ExportNonConformityCommunication, GetDIEMessageMultipleElementsTestFile());
		}

		public void TestProcessMessageWithNoElementsInResponse_DIE()
		{
			ProcessNoElementsInResponseForEntryHeader(DeclarationMessageTypeList.Codes.ExportNonConformityCommunication, InboxNotificationResponseTypes.AESNonConformity);
		}

		public void TestProcessMessageWithOneElement_CCE()
		{
			ProcessOneElementForEntryHeader(DeclarationMessageTypeList.Codes.ExportCceControlCommunication, GetCCEMessageSingleElementTestFile());
		}

		public void TestProcessMessageWithMultipleElements_CCE()
		{
			ProcessMultipleElementsForEntryHeader(DeclarationMessageTypeList.Codes.ExportCceControlCommunication, GetCCEMessageMultipleElementsTestFile());
		}

		public void TestProcessMessageWithNoElementsInResponse_CCE()
		{
			ProcessNoElementsInResponseForEntryHeader(DeclarationMessageTypeList.Codes.ExportCceControlCommunication, InboxNotificationResponseTypes.AESCceControl);
		}

		public void TestProcessMessageWithOneElement_RES()
		{
			ProcessOneElementForEntryHeader(DeclarationMessageTypeList.Codes.ExportExitResultCommunication, GetRESMessageSingleElementTestFile());
		}

		public void TestProcessMessageWithMultipleElements_RES()
		{
			ProcessMultipleElementsForEntryHeader(DeclarationMessageTypeList.Codes.ExportExitResultCommunication, GetRESMessageMultipleElementsTestFile());
		}

		public void TestProcessMessageWithNoElementsInResponse_RES()
		{
			ProcessNoElementsInResponseForEntryHeader(DeclarationMessageTypeList.Codes.ExportExitResultCommunication, InboxNotificationResponseTypes.AESExitResult);
		}

		public void TestProcessMessageWithOneElement_NPD()
		{
			ProcessOneElementForEntryHeader(DeclarationMessageTypeList.Codes.InboxNotificationForDvdH2, GetNPDMessageSingleElementTestFile(), detailTypeIsV5: false);
		}

		public void TestProcessMessageWithMultipleElements_NPD()
		{
			ProcessMultipleElementsForEntryHeader(DeclarationMessageTypeList.Codes.InboxNotificationForDvdH2, GetNPDMessageMultipleElementsTestFile(), detailTypeIsV5: false);
		}

		public void TestProcessMessageWithNoElementsInResponse_NPD()
		{
			ProcessNoElementsInResponseForEntryHeader(DeclarationMessageTypeList.Codes.InboxNotificationForDvdH2, InboxNotificationResponseTypes.DVD);
		}

		void ProcessOneElementForEntryHeader(ZString type, ZString messageText, string extraType = DeclarationMessageTypeList.Codes.InBoxNotificationForExport, bool detailTypeIsV5 = true)
		{
			var mrnList = new ZString[] { SingleMRN };
			var isTest = true;
			var entryheader = SetUpCusPollingTransactionsForEntryHeader(mrnList, type, extraType, isTest: isTest);

			var message = CreateNewEDIMessage(messageText, InterchangeID);

			ProcessMessageForTest(message);

			AssertProcessMessage(type, mrnList, new ZString[] { SingleKey }, entryheader.Messages, isTest, detailTypeIsV5);
		}

		void ProcessMultipleElementsForEntryHeader(ZString type, ZString messageText, string extraType = DeclarationMessageTypeList.Codes.InBoxNotificationForExport, bool detailTypeIsV5 = true)
		{
			var mrnList = new ZString[] { MultipleMRN1, MultipleMRN2, MultipleMRN3, MultipleMRN4 };
			var isTest = false;
			var entryheader = SetUpCusPollingTransactionsForEntryHeader(mrnList, type, extraType, isTest: isTest);

			var message = CreateNewEDIMessage(messageText, InterchangeID);

			ProcessMessageForTest(message);

			AssertProcessMessage(type, mrnList, new ZString[] { MultipleKey1, MultipleKey2, MultipleKey3, MultipleKey4 }, entryheader.Messages, isTest, detailTypeIsV5);
		}

		void ProcessNoElementsInResponseForEntryHeader(ZString type, ZString url, string extraType = DeclarationMessageTypeList.Codes.InBoxNotificationForExport)
		{
			var mrnList = new ZString[] { SingleMRN };
			var isTest = true;
			var sentMessageText = GetExpectedSentInboxMessageBodyText(url);
			var entryheader = SetUpCusPollingTransactionsForEntryHeader(mrnList, type, extraType, isTest: isTest, messageText: sentMessageText);

			AssertProcessMessagenWhenNoElementsInResponse(type, sentMessageText, entryheader.Messages);
		}

		public void TestProcessMessageWithOneElement_LVS()
		{
			ProcessOneElementForExitReport(DeclarationMessageTypeList.Codes.ExportExitClearanceNotification, GetLVSMessageSingleElementTestFile());
		}

		public void TestProcessMessageWithMultipleElements_LVS()
		{
			ProcessMultipleElementsForExitReport(DeclarationMessageTypeList.Codes.ExportExitClearanceNotification, GetLVSMessageMultipleElementsTestFile());
		}

		public void TestProcessMessageWithNoElementsInResponse_LVS()
		{
			ProcessNoElementsInResponseForExitReport(DeclarationMessageTypeList.Codes.ExportExitClearanceNotification, InboxNotificationResponseTypes.AESExitClearance);
		}

		public void TestProcessMessageWithOneElement_DIS()
		{
			ProcessOneElementForExitReport(DeclarationMessageTypeList.Codes.ExportExitNonConformityNotification, GetDISMessageSingleElementTestFile());
		}

		public void TestProcessMessageWithMultipleElements_DIS()
		{
			ProcessMultipleElementsForExitReport(DeclarationMessageTypeList.Codes.ExportExitNonConformityNotification, GetDISMessageMultipleElementsTestFile());
		}

		public void TestProcessMessageWithNoElementsInResponse_DIS()
		{
			ProcessNoElementsInResponseForExitReport(DeclarationMessageTypeList.Codes.ExportExitNonConformityNotification, InboxNotificationResponseTypes.AESExitNonConformity);
		}

		void ProcessOneElementForExitReport(ZString type, ZString messageText, string extraType = DeclarationMessageTypeList.Codes.InBoxNotificationForExport)
		{
			var mrnList = new ZString[] { SingleMRN };
			var exitReport = SetUpCusPollingTransactionsForExitReport(mrnList, type, extraType);

			var message = CreateNewEDIMessage(messageText, InterchangeID);

			ProcessMessageForTest(message);

			AssertProcessMessage(type, mrnList, new ZString[] { SingleKey }, exitReport.MessageCollection, false, true);
		}

		void ProcessMultipleElementsForExitReport(ZString type, ZString messageText, string extraType = DeclarationMessageTypeList.Codes.InBoxNotificationForExport)
		{
			var mrnList = new ZString[] { MultipleMRN1, MultipleMRN2, MultipleMRN3, MultipleMRN4 };
			var exitReport = SetUpCusPollingTransactionsForExitReport(mrnList, type, extraType);

			var message = CreateNewEDIMessage(messageText, InterchangeID);

			ProcessMessageForTest(message);

			AssertEquals("EM_Status", EDIMessageStatusList.Codes.Received, message.EM_Status);
			AssertProcessMessage(type, mrnList, new ZString[] { MultipleKey1, MultipleKey2, MultipleKey3, MultipleKey4 }, exitReport.MessageCollection, false, true);
		}

		void ProcessNoElementsInResponseForExitReport(ZString type, ZString url, string extraType = DeclarationMessageTypeList.Codes.InBoxNotificationForExport)
		{
			var mrnList = new ZString[] { SingleMRN };
			var sentMessageText = GetExpectedSentInboxMessageBodyText(url);
			var exitReport = SetUpCusPollingTransactionsForExitReport(mrnList, type, extraType, messageText: sentMessageText);

			AssertProcessMessagenWhenNoElementsInResponse(type, sentMessageText, exitReport.MessageCollection);
		}

		public void TestProcessMessageWithOneElement_LVT()
		{
			ProcessOneElementForNctsHeader(DeclarationMessageTypeList.Codes.InboxNotificationNctsDepartureClearance, GetLVTMessageSingleElementTestFile());
		}

		public void TestProcessMessageWithMultipleElements_LVT()
		{
			ProcessMultipleElementsForNctsHeader(DeclarationMessageTypeList.Codes.InboxNotificationNctsDepartureClearance, GetLVTMessageMultipleElementsTestFile());
		}

		public void TestProcessMessageWithNoElementsInResponse_LVT()
		{
			ProcessNoElementsInResponseForNctsHeader(DeclarationMessageTypeList.Codes.InboxNotificationNctsDepartureClearance, InboxNotificationResponseTypes.NCTSClearance);
		}

		public void TestProcessMessageWithOneElement_DIT()
		{
			ProcessOneElementForNctsHeader(DeclarationMessageTypeList.Codes.InboxNotificationForNonConformityNctsDeparture, GetDITMessageSingleElementTestFile());
		}

		public void TestProcessMessageWithMultipleElements_DIT()
		{
			ProcessMultipleElementsForNctsHeader(DeclarationMessageTypeList.Codes.InboxNotificationForNonConformityNctsDeparture, GetDITMessageMultipleElementsTestFile());
		}

		public void TestProcessMessageWithNoElementsInResponse_DIT()
		{
			ProcessNoElementsInResponseForNctsHeader(DeclarationMessageTypeList.Codes.InboxNotificationForNonConformityNctsDeparture, InboxNotificationResponseTypes.NCTSNonConformity);
		}

		public void TestProcessMessageWithOneElement_CCT()
		{
			ProcessOneElementForNctsHeader(DeclarationMessageTypeList.Codes.InboxNotificationNctsControls, GetCCTMessageSingleElementTestFile());
		}

		public void TestProcessMessageWithMultipleElements_CCT()
		{
			ProcessMultipleElementsForNctsHeader(DeclarationMessageTypeList.Codes.InboxNotificationNctsControls, GetCCTMessageMultipleElementsTestFile());
		}

		public void TestProcessMessageWithNoElementsInResponse_CCT()
		{
			ProcessNoElementsInResponseForNctsHeader(DeclarationMessageTypeList.Codes.InboxNotificationNctsControls, InboxNotificationResponseTypes.NCTSCceControl);
		}

		public void TestProcessMessageWithOneElement_INT()
		{
			ProcessOneElementForNctsHeader(DeclarationMessageTypeList.Codes.InboxNotificationNctsDepartureInvalidation, GetINTMessageSingleElementTestFile());
		}

		public void TestProcessMessageWithMultipleElements_INT()
		{
			ProcessMultipleElementsForNctsHeader(DeclarationMessageTypeList.Codes.InboxNotificationNctsDepartureInvalidation, GetINTMessageMultipleElementsTestFile());
		}

		public void TestProcessMessageWithNoElementsInResponse_INT()
		{
			ProcessNoElementsInResponseForNctsHeader(DeclarationMessageTypeList.Codes.InboxNotificationNctsDepartureInvalidation, InboxNotificationResponseTypes.NCTSInvalidation);
		}

		void ProcessOneElementForNctsHeader(ZString type, ZString messageText, string extraType = DeclarationMessageTypeList.Codes.InBoxNotificationForExport)
		{
			var mrnList = new ZString[] { SingleMRN };
			var nctsHeader = SetUpCusPollingTransactionsForNctsHeader(mrnList, type, extraType);

			var message = CreateNewEDIMessage(messageText, InterchangeID);

			ProcessMessageForTest(message);

			AssertProcessMessage(type, mrnList, new ZString[] { SingleKey }, nctsHeader.MessageCollection, false, true);
		}

		void ProcessMultipleElementsForNctsHeader(ZString type, ZString messageText, string extraType = DeclarationMessageTypeList.Codes.InBoxNotificationForExport)
		{
			var mrnList = new ZString[] { MultipleMRN1, MultipleMRN2, MultipleMRN3, MultipleMRN4 };
			var nctsHeader = SetUpCusPollingTransactionsForNctsHeader(mrnList, type, extraType);

			var message = CreateNewEDIMessage(messageText, InterchangeID);

			ProcessMessageForTest(message);

			AssertEquals("EM_Status", EDIMessageStatusList.Codes.Received, message.EM_Status);
			AssertProcessMessage(type, mrnList, new ZString[] { MultipleKey1, MultipleKey2, MultipleKey3, MultipleKey4 }, nctsHeader.MessageCollection, false, true);
		}

		void ProcessNoElementsInResponseForNctsHeader(ZString type, ZString url, string extraType = DeclarationMessageTypeList.Codes.InBoxNotificationForExport)
		{
			var mrnList = new ZString[] { SingleMRN };
			var sentMessageText = GetExpectedSentInboxMessageBodyText(url);
			var nctsHeader = SetUpCusPollingTransactionsForNctsHeader(mrnList, type, extraType, messageText: sentMessageText);

			AssertProcessMessagenWhenNoElementsInResponse(type, sentMessageText, nctsHeader.MessageCollection);
		}

		public void TestProcessMessageWrongXML()
		{
			var message = CreateNewEDIMessage(WrongXMLTestFile, InterchangeID);

			ProcessMessageForTest(message);

			AssertEquals("EM_Status", EDIMessage.Status.Failed, message.EM_Status);
			AssertContains("logger", "Unable to read message text from message", GetAllConcatenatedUserLogStrings());
		}

		public void TestProcessMessage_EmptyMessageText()
		{
			var message = CreateNewEDIMessage(ZString.Empty, InterchangeID);

			ProcessMessageForTest(message);

			AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, message.EM_Status);
			var concatenatedUserLogStrings = GetAllConcatenatedUserLogStrings();
			AssertContains("logger", "Unable to read message text from message ", concatenatedUserLogStrings);
			AssertContains("logger exception", "Message Text is empty so can't continue with processing", concatenatedUserLogStrings);
			AssertEquals("EM_MessageInterpretation",
					string.Format("<H3>Processor Failure</H3><br>" +
					"<H4>Failure: Unable to read message text from message (Number:{0}, Type:{1}, Sub:{2}, Ref:{3}); message status set to Failed.</H4>" +
					"<H4>Exception: Message Text is empty so can't continue with processing</H4>", message.EM_MessageNum, message.EM_MessageType, message.EM_MessageSubType, message.EM_ApplicationReference)
					, message.EM_MessageInterpretation);
		}

		void AssertProcessMessage(ZString type, ZString[] mrnList, ZString[] keyList, EDIMessageCollection messages, ZBool isTest, bool detailTypeIsV5)
		{
			CombineAssertions(() =>
			{
				var transactions = GetCusPollingTransactions();
				foreach (var mrn in mrnList)
				{
					AssertEquals("transaction with correct mrn (" + mrn + ") and type has changed status to CLS because it has been processed", "CLS", transactions.First(t => t.CPT_TransactionID == mrn && t.CPT_Type == type).CPT_Status);
					AssertEquals("transaction with correct mrn (" + mrn + ") but different type has not changed status", "PND", transactions.First(t => t.CPT_TransactionID == mrn && t.CPT_Type != type).CPT_Status);
				}
				AssertEquals("transaction with ExtraMRN1 (same type) has changed status so it can be sent again", "OPN", transactions.First(t => t.CPT_TransactionID == ExtraMRN1).CPT_Status);
				AssertEquals("transaction with ExtraMRN2 (different type) has not changed status", "PND", transactions.First(t => t.CPT_TransactionID == ExtraMRN2).CPT_Status);
				AssertEquals("transaction with ExtraMRN3 (same type different certificate) has changed status so it can be sent again", "OPN", transactions.First(t => t.CPT_TransactionID == ExtraMRN3).CPT_Status);
				AssertEquals("transaction with ExtraMRN4 (same type different isTest or certificate) has changed status so it can be sent again", "OPN", transactions.First(t => t.CPT_TransactionID == ExtraMRN4).CPT_Status);

				AssertNewMessages(messages, type, keyList, isTest, detailTypeIsV5);
			});
		}

		void AssertNewMessages(EDIMessageCollection messages, ZString type, ZString[] keyList, bool isTest, bool detailTypeIsV5)
		{
			messages.Reload(true);
			var newRequestMessages = messages.Cast<EDIMessage>().Where(x => x.EM_ReceiveTransmit == EDIMessage.Direction.Transmit && !x.EM_MessageText.Contains(SentMessageText));

			AssertEquals("messages count is correct", keyList.Length, newRequestMessages.Count());

			foreach (EDIMessage message in newRequestMessages)
			{
				AssertEquals(message.EM_MessageType + " message.EM_ApplicationCode", ApplicationCodeList.Codes.ESCustomsMessage, message.EM_ApplicationCode);
				AssertEquals(message.EM_MessageType + " message.EM_MessageSubType", DeclarationMessageSubTypeList.Codes.OriginalDeclaration, message.EM_MessageSubType);
				AssertEquals(message.EM_MessageType + " message.EM_MessageType", type, message.EM_MessageType);
				AssertEquals(message.EM_MessageType + " message.EM_IsTestMessage", isTest, message.EM_IsTestMessage);
				AssertEquals(message.EM_MessageType + " message.EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
				AssertEquals(message.EM_MessageType + " message.EM_Status", EDIMessage.Status.Queued, message.EM_Status);
				AssertEquals(message.EM_MessageType + " message.EM_ApplicationReference", CertName1, message.EM_ApplicationReference);
				AssertNull(message.EM_MessageType + " message doesn't have interchange", message.Interchange);

				var key = keyList.First(x => message.EM_MessageText.Contains(x));
				AssertMultilineASCIIEquals(message.EM_MessageType + " message.EM_MessageText", detailTypeIsV5 ? GetExpectedNewMessageBodyTextDetailV5(key) : GetExpectedNewMessageBodyTextDetailV4(key), message.EM_MessageText);
			}
		}

		void AssertProcessMessagenWhenNoElementsInResponse(ZString type, ZString sentMessageText, EDIMessageCollection messages)
		{
			var message = CreateNewEDIMessage(GetMessageNoElementsTestFile(), InterchangeID);

			ProcessMessageForTest(message);

			CombineAssertions(() =>
			{
				var transactions = GetCusPollingTransactions();
				AssertEquals("transaction with correct mrn (" + SingleMRN + ") and type has changed status so it can be sent again", "OPN", transactions.First(t => t.CPT_TransactionID == SingleMRN && t.CPT_Type == type).CPT_Status);
				AssertEquals("transaction with correct mrn (" + SingleMRN + ") but different type has not changed status", "PND", transactions.First(t => t.CPT_TransactionID == SingleMRN && t.CPT_Type != type).CPT_Status);
				AssertEquals("transaction with ExtraMRN1 (same type) has changed status so it can be sent again", "OPN", transactions.First(t => t.CPT_TransactionID == ExtraMRN1).CPT_Status);
				AssertEquals("transaction with ExtraMRN2 (different type) has not changed status", "PND", transactions.First(t => t.CPT_TransactionID == ExtraMRN2).CPT_Status);
				AssertEquals("transaction with ExtraMRN3 (same type different certificate) has changed status so it can be sent again", "OPN", transactions.First(t => t.CPT_TransactionID == ExtraMRN3).CPT_Status);
				AssertEquals("transaction with ExtraMRN4 (same type different isTest or certificate) has changed status so it can be sent again", "OPN", transactions.First(t => t.CPT_TransactionID == ExtraMRN4).CPT_Status);

				messages.Reload(true);
				var newRequestMessages = messages.Cast<EDIMessage>().Where(x => x.EM_ReceiveTransmit == EDIMessage.Direction.Transmit && !x.EM_MessageText.Contains(sentMessageText));

				AssertEquals("No new TRX messages", false, newRequestMessages.Any());
			});
		}

		CusEntryHeader SetUpCusPollingTransactionsForEntryHeader(ZString[] mrnList, ZString type, ZString extraType, bool isTest = false, string messageText = SentMessageText)
		{
			SetUpStaffWithCertificate(StaffCode1, CertName1);

			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration1.JE_GS_NKCusAgent = StaffCode1;
			declaration1.ZG_IsTrainingDeclaration = isTest;
			var entryHeader1 = (CusEntryHeader)declaration1.ActiveEntryHeaders.AddNew();

			foreach (var mrn in mrnList)
			{
				AddCusPollingTransaction("ESC", "PND", ZDateTime.Now, mrn, type, bo: entryHeader1);
				AddCusPollingTransaction("ESC", "PND", ZDateTime.Now, mrn, extraType, bo: entryHeader1);
			}
			AddCusPollingTransaction("ESC", "PND", ZDateTime.Now, ExtraMRN1, type, bo: entryHeader1);
			AddCusPollingTransaction("ESC", "PND", ZDateTime.Now, ExtraMRN2, extraType, bo: entryHeader1);

			SetUpStaffWithCertificate("AZ2", "CERTNAME2");
			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration2.JE_GS_NKCusAgent = "AZ2";
			declaration2.ZG_IsTrainingDeclaration = isTest;
			var entryHeader2 = (CusEntryHeader)declaration2.ActiveEntryHeaders.AddNew();

			AddCusPollingTransaction("ESC", "PND", ZDateTime.Now, ExtraMRN3, type, bo: entryHeader2);

			var declaration3 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration3.JE_GS_NKCusAgent = StaffCode1;
			declaration3.ZG_IsTrainingDeclaration = !isTest;
			var entryHeader3 = (CusEntryHeader)declaration3.ActiveEntryHeaders.AddNew();

			AddCusPollingTransaction("ESC", "PND", ZDateTime.Now, ExtraMRN4, type, bo: entryHeader3);

			SetSentInterchange(InterchangeID, messageText: messageText);

			return entryHeader1;
		}

		IESResponseBusinessObject SetUpCusPollingTransactionsForExitReport(ZString[] mrnList, ZString type, ZString extraType, string messageText = SentMessageText)
		{
			SetUpStaffWithCertificate(StaffCode1, CertName1);

			var report1 = Factory.Load(CusExitReportSchema.Constants.Prefix, ProcessorTestHelper.CreateCusExitReport(clusterKey: 1, referenceNum: "E00000001", brokerCode: StaffCode1, certName: CertName1));

			foreach (var mrn in mrnList)
			{
				AddCusPollingTransaction("ESC", "PND", ZDateTime.Now, mrn, type, bo: report1);
				AddCusPollingTransaction("ESC", "PND", ZDateTime.Now, mrn, extraType, bo: report1);
			}
			AddCusPollingTransaction("ESC", "PND", ZDateTime.Now, ExtraMRN1, type, bo: report1);
			AddCusPollingTransaction("ESC", "PND", ZDateTime.Now, ExtraMRN2, extraType, bo: report1);

			SetUpStaffWithCertificate("AZ2", "CERTNAME2");
			var report2 = Factory.Load(CusExitReportSchema.Constants.Prefix, ProcessorTestHelper.CreateCusExitReport(clusterKey: 2, referenceNum: "E00000002", brokerCode: "AZ2", certName: "CERTNAME2"));

			AddCusPollingTransaction("ESC", "PND", ZDateTime.Now, ExtraMRN3, type, bo: report2);
			AddCusPollingTransaction("ESC", "PND", ZDateTime.Now, ExtraMRN4, type, bo: report2);

			SetSentInterchange(InterchangeID, messageText: messageText);

			return (IESResponseBusinessObject)report1;
		}

		IESResponseBusinessObject SetUpCusPollingTransactionsForNctsHeader(ZString[] mrnList, ZString type, ZString extraType, string messageText = SentMessageText)
		{
			SetUpStaffWithCertificate(StaffCode1, CertName1);

			var nctsHeader1 = Factory.Load(CusInBondHeaderSchema.Constants.Prefix, ProcessorTestHelper.CreateNctsHeader(headerType: "D", referenceNum: "E00000001", brokerCode: StaffCode1, certName: CertName1));

			foreach (var mrn in mrnList)
			{
				AddCusPollingTransaction("ESC", "PND", ZDateTime.Now, mrn, type, bo: nctsHeader1);
				AddCusPollingTransaction("ESC", "PND", ZDateTime.Now, mrn, extraType, bo: nctsHeader1);
			}
			AddCusPollingTransaction("ESC", "PND", ZDateTime.Now, ExtraMRN1, type, bo: nctsHeader1);
			AddCusPollingTransaction("ESC", "PND", ZDateTime.Now, ExtraMRN2, extraType, bo: nctsHeader1);

			SetUpStaffWithCertificate("AZ2", "CERTNAME2");
			var nctsHeader2 = Factory.Load(CusInBondHeaderSchema.Constants.Prefix, ProcessorTestHelper.CreateNctsHeader(headerType: "D", referenceNum: "E00000002", brokerCode: "AZ2", certName: "CERTNAME2"));

			AddCusPollingTransaction("ESC", "PND", ZDateTime.Now, ExtraMRN3, type, bo: nctsHeader2);
			AddCusPollingTransaction("ESC", "PND", ZDateTime.Now, ExtraMRN4, type, bo: nctsHeader2);

			SetSentInterchange(InterchangeID, messageText: messageText);

			return (IESResponseBusinessObject)nctsHeader1;
		}

		GlbStaff SetUpStaffWithCertificate(ZString staffCode, ZString certName)
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = staffCode;
			staff.GS_LoginName = staffCode + "test";
			var wrapper = ES.Business.GlbStaffWrapper.Get(staff);
			var cert = wrapper.ESBPasswordCollection.AddNew();
			cert.GP_Name = certName;
			cert.GP_MailBoxID = "Test";
			cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			return staff;
		}

		protected TestEdiMessage CreateNewEDIMessage(ZString messageText, ZGuid interchangeID)
		{
			var message = Factory.New<TestEdiMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = MessageType;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageSubType = "AAA";
			message.EM_ApplicationReference = "certName";
			message.EM_GB = new ZGuid();
			message.EM_MessageText = messageText;

			var responseInterchange = CreateTestResponseInterchange(interchangeID);
			responseInterchange.ContainedMessages.Add(message);

			Factory.Save();
			return message;
		}

		protected virtual EDIInterchange CreateTestResponseInterchange(ZGuid interchangeID)
		{
			var responseInterchange = Factory.New<EDIInterchange>();
			responseInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			responseInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			responseInterchange.EI_SessionGUID = interchangeID;
			responseInterchange.EI_From = SpanishCustomsTypeCodeList.Codes.AsynchronousTestSpanishCustomsForDirectXt;
			responseInterchange.EI_To = "CW1";
			responseInterchange.EI_TransportType = EDIInterchange.TransportType.xT;

			return responseInterchange;
		}

		protected EDIInterchange SetSentInterchange(ZGuid interchangeID, string messageText = SentMessageText)
		{
			var sentInterchange = Factory.New<EDIInterchange>();
			sentInterchange.EI_SessionGUID = interchangeID;
			sentInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			sentInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			sentInterchange.EI_From = "CW1";
			sentInterchange.EI_To = "ESCustoms";

			var sentMessage = Factory.New<TestEdiMessage>();
			sentMessage.EM_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			sentMessage.EM_MessageType = MessageType;
			sentMessage.EM_Status = EDIMessage.Status.Queued;
			sentMessage.EM_MessageText = messageText;
			sentMessage.EM_MessageNum = "10";
			sentMessage.EM_MessageSubType = DeclarationMessageSubTypeList.Codes.OriginalDeclaration;
			sentInterchange.ContainedMessages.Add(sentMessage);

			return sentInterchange;
		}

		CusPollingTransaction AddCusPollingTransaction(ZString applicationCode, ZString status, ZDateTime createTime, ZString transactionID, ZString type, BusinessObject bo = null)
		{
			var transaction = Factory.NewWithValidTestData<CusPollingTransaction>();
			transaction.CPT_ApplicationCode = applicationCode;
			transaction.CPT_Status = status;
			transaction.CPT_SystemCreateTimeUtc = createTime;
			transaction.CPT_TransactionID = transactionID;
			transaction.CPT_Type = type;
			transaction.CPT_NumberOfAttempts = 1;
			if (bo != null)
			{
				transaction.CPT_ParentID = bo.PK;
				transaction.CPT_ParentTableCode = bo.TablePrefix;
			}
			return transaction;
		}
		protected override InboxPendingListResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new InboxPendingListResponseMessageProcessor(logger);

		protected override ZString GetExpectedProcessorFriendlyName() => "Inbox Pending List Declaration Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.InboxPendingList };

		protected void ProcessMessageForTest(TestEdiMessage message)
		{
			processor.PreProcessMessage(message);
			processor.ProcessMessage(message);

			Factory.Save();
		}

		ZString GetExpectedNewMessageBodyTextDetailV4(ZString key) => ZString.Format(@$"<?xml version=""1.0"" encoding=""utf-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
<{XMLTestFileConstants.XmlElementNamespace}DetalleV4Ent xmlns{XMLTestFileConstants.XmlElementNamespaceSuffix}=""https://www3.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adht/band/ws/det/DetalleV4Ent.xsd"">
  <{XMLTestFileConstants.XmlElementNamespace}clave>{key}</{XMLTestFileConstants.XmlElementNamespace}clave>
</{XMLTestFileConstants.XmlElementNamespace}DetalleV4Ent>
  </soapenv:Body>
</soapenv:Envelope>");

		ZString GetExpectedNewMessageBodyTextDetailV5(ZString key) => ZString.Format(@$"<?xml version=""1.0"" encoding=""utf-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
<{XMLTestFileConstants.XmlElementNamespace}DetalleV5Ent xmlns{XMLTestFileConstants.XmlElementNamespaceSuffix}=""https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adht/band/ws/det/DetalleV5Ent.xsd"">
  <{XMLTestFileConstants.XmlElementNamespace}clave>{key}</{XMLTestFileConstants.XmlElementNamespace}clave>
</{XMLTestFileConstants.XmlElementNamespace}DetalleV5Ent>
  </soapenv:Body>
</soapenv:Envelope>");

		ZString GetExpectedSentInboxMessageBodyText(ZString url) => ZString.Format(@$"<?xml version=""1.0"" encoding=""utf-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
<{XMLTestFileConstants.XmlElementNamespace}ListaDecV4Ent tipoRespuesta=""{url}"" xmlns{XMLTestFileConstants.XmlElementNamespaceSuffix}=""https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adht/band/ws/li/ListaDecV4Ent.xsd"">
  <{XMLTestFileConstants.XmlElementNamespace}declarante>
    <{XMLTestFileConstants.XmlElementNamespace}NifDeclarante>NIF22222222</{XMLTestFileConstants.XmlElementNamespace}NifDeclarante>
    <{XMLTestFileConstants.XmlElementNamespace}NombreDeclarante>Declarant Full Name</{XMLTestFileConstants.XmlElementNamespace}NombreDeclarante>
  </{XMLTestFileConstants.XmlElementNamespace}declarante>
</{XMLTestFileConstants.XmlElementNamespace}ListaDecV4Ent>
  </soapenv:Body>
</soapenv:Envelope>");

		CusPollingTransaction[] GetCusPollingTransactions()
		{
			var query = new ZQuery(CusPollingTransactionSchema.CPT_ApplicationCode, Enterprise.Messaging.Integration.ApplicationCodeList.Codes.ESCustomsMessage);
			return NewFactory().Load<CusPollingTransaction>(query);
		}

		string GetAllConcatenatedUserLogStrings() => string.Concat(logger.UserLogStrings.Cast<string>());

		const string SentMessageText = "Message text";
		ZGuid InterchangeID => new ZGuid("5E5A9120-1794-4251-86AC-8038077F5BA6");
		ZString MessageType => DeclarationMessageTypeList.Codes.InboxPendingList;
		ZString StaffCode1 => "AZ1";
		ZString CertName1 => "CERTNAME1";
		ZString SingleMRN => "22ES000101100083B3";
		ZString MultipleMRN1 => "22ES000101100084B2";
		ZString MultipleMRN2 => "22ES000101100040B8";
		ZString MultipleMRN3 => "22ES000101100045B3";
		ZString MultipleMRN4 => "22ES000101100110B1";
		ZString ExtraMRN1 => "AAA";
		ZString ExtraMRN2 => "BBB";
		ZString ExtraMRN3 => "CCC";
		ZString ExtraMRN4 => "DDD";
		ZString SingleKey => "20220328155506010681";
		ZString MultipleKey1 => "20220328155604813418";
		ZString MultipleKey2 => "20221118221937572940";
		ZString MultipleKey3 => "20221118222248525973";
		ZString MultipleKey4 => "20221118222452362581";

		string GetNPEMessageSingleElementTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxPendingListTestFilePath, "NPEMessageSingleElement.txt");
		string GetNPEMessageMultipleElementsTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxPendingListTestFilePath, "NPEMessageMultipleElements.txt");
		string GetNPIMessageSingleElementTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxPendingListTestFilePath, "NPIMessageSingleElement.txt");
		string GetNPIMessageMultipleElementsTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxPendingListTestFilePath, "NPIMessageMultipleElements.txt");
		string GetINEMessageSingleElementTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxPendingListTestFilePath, "INEMessageSingleElement.txt");
		string GetINEMessageMultipleElementsTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxPendingListTestFilePath, "INEMessageMultipleElements.txt");
		string GetLVEMessageSingleElementTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxPendingListTestFilePath, "LVEMessageSingleElement.txt");
		string GetLVEMessageMultipleElementsTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxPendingListTestFilePath, "LVEMessageMultipleElements.txt");
		string GetDIEMessageSingleElementTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxPendingListTestFilePath, "DIEMessageSingleElement.txt");
		string GetDIEMessageMultipleElementsTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxPendingListTestFilePath, "DIEMessageMultipleElements.txt");
		string GetCCEMessageSingleElementTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxPendingListTestFilePath, "CCEMessageSingleElement.txt");
		string GetCCEMessageMultipleElementsTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxPendingListTestFilePath, "CCEMessageMultipleElements.txt");
		string GetRESMessageSingleElementTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxPendingListTestFilePath, "RESMessageSingleElement.txt");
		string GetRESMessageMultipleElementsTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxPendingListTestFilePath, "RESMessageMultipleElements.txt");
		string GetNPDMessageSingleElementTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxPendingListTestFilePath, "NPDMessageSingleElement.txt");
		string GetNPDMessageMultipleElementsTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxPendingListTestFilePath, "NPDMessageMultipleElements.txt");
		string GetLVSMessageSingleElementTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxPendingListTestFilePath, "LVSMessageSingleElement.txt");
		string GetLVSMessageMultipleElementsTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxPendingListTestFilePath, "LVSMessageMultipleElements.txt");
		string GetDISMessageSingleElementTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxPendingListTestFilePath, "DISMessageSingleElement.txt");
		string GetDISMessageMultipleElementsTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxPendingListTestFilePath, "DISMessageMultipleElements.txt");
		string GetLVTMessageSingleElementTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxPendingListTestFilePath, "LVTMessageSingleElement.txt");
		string GetLVTMessageMultipleElementsTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxPendingListTestFilePath, "LVTMessageMultipleElements.txt");
		string GetDITMessageSingleElementTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxPendingListTestFilePath, "DITMessageSingleElement.txt");
		string GetDITMessageMultipleElementsTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxPendingListTestFilePath, "DITMessageMultipleElements.txt");
		string GetCCTMessageSingleElementTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxPendingListTestFilePath, "CCTMessageSingleElement.txt");
		string GetCCTMessageMultipleElementsTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxPendingListTestFilePath, "CCTMessageMultipleElements.txt");
		string GetINTMessageSingleElementTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxPendingListTestFilePath, "INTMessageSingleElement.txt");
		string GetINTMessageMultipleElementsTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxPendingListTestFilePath, "INTMessageMultipleElements.txt");
		string GetMessageNoElementsTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxPendingListTestFilePath, "MessageNoElements.txt");
		string WrongXMLTestFile => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.XMLTestFilePath, "WrongXML.txt");
	}
}
