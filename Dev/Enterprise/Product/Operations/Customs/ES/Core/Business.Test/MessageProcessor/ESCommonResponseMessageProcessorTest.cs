using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;
using Enterprise.Customs.ManifestBase;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Schema;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.Business.Testing
{
	public abstract class ESCommonResponseMessageProcessorTest<T, TBusinessObject, TResponseProvider> : ESBranchCustomsApplicationTypeMessageProcessorTest<T, TResponseProvider>
		where T : ESCommonResponseMessageProcessor<TBusinessObject, TResponseProvider>
		where TBusinessObject : BusinessObject
	{
		public void TestPreProcessMessage_NullApplicationReference_EHub()
		{
			var message = Factory.New<TestEdiMessage>();
			var responseInterchange = CreateTestResponseInterchange(ZGuid.Empty, EDIInterchange.TransportType.eHub);
			responseInterchange.ContainedMessages.Add(message);

			CombineAssertions(() =>
			{
				message.EM_ApplicationReference = null;
				processor.PreProcessMessage(message);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, message.EM_Status);
				AssertContains("logger", "No Application Reference found for message ", logger.UserLogStrings[0]);
			});
		}

		public void TestPreProcessMessage_IncorrectApplicationReference_EHub()
		{
			var message = SetDataForIncorrectApplicationReferencePreProcessing(EDIInterchange.TransportType.eHub);

			CombineAssertions(() =>
			{
				processor.PreProcessMessage(message);
				processor.PreProcessMessage(message);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, message.EM_Status);

				var concatenatedUserLogStrings = GetAllConcatenatedUserLogStrings();
				AssertContains("logger", "Unable to find business object for message ", concatenatedUserLogStrings);
			});
		}

		public void TestPreProcessMessage_IncorrectSessionGuid_EHub()
		{
			var message = Factory.New<TestEdiMessage>();
			var responseInterchange = CreateTestResponseInterchange(ZGuid.Empty, EDIInterchange.TransportType.eHub);
			responseInterchange.ContainedMessages.Add(message);

			AssertIncorrectSessionGuid(message, false);
		}

		public void TestPreProcessMessage_IncorrectSessionGuid_DirectxT()
		{
			var message = Factory.New<TestEdiMessage>();
			var responseInterchange = CreateTestResponseInterchange(ZGuid.Empty, EDIInterchange.TransportType.xT);
			responseInterchange.ContainedMessages.Add(message);

			AssertIncorrectSessionGuid(message, true);
		}

		void AssertIncorrectSessionGuid(TestEdiMessage message, bool isDirectxT)
		{
			CombineAssertions(() =>
			{
				var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
				entryHeader.CH_BGMReference = "TEST";
				message.EM_ApplicationReference = isDirectxT ? string.Empty : "TEST";
				processor.PreProcessMessage(message);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, message.EM_Status);
				AssertLoggerMessagesWhenPreProcessMessageWrongSessionGUID();
			});
		}

		public void TestPreProcessMessage_CorrectData_EHub()
		{
			var message = SetDataForCorrectPreProcessing(EDIInterchange.TransportType.eHub);

			AssertCorrectDataPreProcessing(message);
		}

		public void TestPreProcessMessage_CorrectData_DirectxT()
		{
			var message = SetDataForCorrectPreProcessing(EDIInterchange.TransportType.xT);

			AssertCorrectDataPreProcessing(message);
		}

		void AssertCorrectDataPreProcessing(TestEdiMessage message)
		{
			CombineAssertions(() =>
			{
				processor.PreProcessMessage(message);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.PreProcessedOK, message.EM_Status);
				AssertEquals("EM_GB", BranchPK, message.EM_GB);
				AssertEquals("UserLogs empty", 0, logger.UserLogStrings.Count);
			});
		}

		public void TestProcessMessage_EmptyMessageText_EHub()
		{
			var message = SetDataForCorrectPreProcessing(EDIInterchange.TransportType.eHub);
			processor.PreProcessMessage(message);

			AssertEmptyMessageText(message, message.EM_LinkedObject);
		}

		public void TestProcessMessage_EmptyMessageText_DirectxT()
		{
			var message = SetDataForCorrectPreProcessing(EDIInterchange.TransportType.xT);
			processor.PreProcessMessage(message);

			AssertEmptyMessageText(message, message.EM_LinkedObject);
		}

		void AssertEmptyMessageText(TestEdiMessage message, BusinessObject businessObject)
		{
			CombineAssertions(() =>
			{
				message.EM_MessageText = ZString.Empty;
				processor.ProcessMessage(message);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, message.EM_Status);
				if (businessObject is CusEntryHeader entryHeader1)
				{
					AssertEquals("CH_Status", "FAL", entryHeader1.CH_Status);
				}

				var concatenatedUserLogStrings = GetAllConcatenatedUserLogStrings();
				AssertContains("logger", "Unable to read message text from message ", concatenatedUserLogStrings);
				AssertContains("logger exception", "Message Text is empty so can't continue with processing", concatenatedUserLogStrings);
				AssertEquals("EM_MessageInterpretation",
						string.Format("<H3>Processor Failure</H3><br>" +
						"<H4>Failure: Unable to read message text from message (Number:{0}, Type:{1}, Sub:{2}, Ref:{3}); message status set to Failed.</H4>" +
						"<H4>Exception: Message Text is empty so can't continue with processing</H4>", message.EM_MessageNum, message.EM_MessageType, message.EM_MessageSubType, message.EM_ApplicationReference)
						, message.EM_MessageInterpretation);
			});
		}

		public void TestProcessMessage_IncorrectMessageText_EHub()
		{
			var message = SetDataForCorrectPreProcessing(EDIInterchange.TransportType.eHub);
			processor.PreProcessMessage(message);

			AssertIncorrectMessageText(message, message.EM_LinkedObject);
		}

		public void TestProcessMessage_IncorrectMessageText_DirectxT()
		{
			var message = SetDataForCorrectPreProcessing(EDIInterchange.TransportType.xT);
			processor.PreProcessMessage(message);

			AssertIncorrectMessageText(message, message.EM_LinkedObject);
		}

		void AssertIncorrectMessageText(TestEdiMessage message, BusinessObject businessObject)
		{
			CombineAssertions(() =>
			{
				message.EM_MessageText = "Text";
				message.EM_MessageInterpretation = ZString.Empty;
				processor.ProcessMessage(message);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, message.EM_Status);
				if (businessObject is CusEntryHeader entryHeader1)
				{
					AssertEquals("CH_Status", "FAL", entryHeader1.CH_Status);
				}

				AssertLoggerMessagesWhenProcessMessageWrongText(message, businessObject);
			});
		}

		public void TestProcessMessage_NullLinkedObject_EHub()
		{
			var message = SetDataForCorrectPreProcessing(EDIInterchange.TransportType.eHub);
			processor.PreProcessMessage(message);

			AssertNullLinkedObject(message);
		}

		public void TestProcessMessage_NullLinkedObject_DirectxT()
		{
			var message = SetDataForCorrectPreProcessing(EDIInterchange.TransportType.xT);
			processor.PreProcessMessage(message);

			AssertNullLinkedObject(message);
		}

		void AssertNullLinkedObject(TestEdiMessage message)
		{
			CombineAssertions(() =>
			{
				message.EM_LinkedObject = null;
				message.EM_MessageText = "Text";
				message.EM_MessageInterpretation = ZString.Empty;
				processor.ProcessMessage(message);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, message.EM_Status);
				AssertContains("logger", "Unable to read message text from message ", logger.UserLogStrings[0]);
				AssertContains("logger exception", "Message's Linked Object is null so can't continue with processing", logger.UserLogStrings[1]);
				AssertEquals("EM_MessageInterpretation",
						string.Format("<H3>Processor Failure</H3><br>" +
						"<H4>Failure: Unable to read message text from message (Number:{0}, Type:{1}, Sub:{2}, Ref:{3}); message status set to Failed.</H4>" +
						"<H4>Exception: Message's Linked Object is null so can't continue with processing</H4>", message.EM_MessageNum, message.EM_MessageType, message.EM_MessageSubType, message.EM_ApplicationReference)
						, message.EM_MessageInterpretation);
			});
		}

		protected ZString GetExpectedNewInboxInterchangeBodyText_EHub(ZString mrnCode, ZString id, ZString name) => ZString.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<Body>
  <MRN>{0}</MRN>
  <DeclarantName>{1}</DeclarantName>
  <DeclarantID>{2}</DeclarantID>
</Body>", mrnCode, name, id);

		protected void AssertNewInboxMessages(EDIMessageCollection messages, ZString[] messageTypes, ZString id, ZString name, ZString mrn)
		{
			CombineAssertions("Check new InboxRequest EDIMessage Properties", () =>
			{
				messages.Reload(true);
				var newRequestMessages = messages.Cast<EDIMessage>().Where(x => x.EM_ReceiveTransmit == EDIMessage.Direction.Transmit && !x.EM_MessageText.Contains(SentMessageText) && messageTypes.Contains(x.EM_MessageType));

				AssertEquals("messages count is correct", messageTypes.Length, newRequestMessages.Count());

				foreach (EDIMessage message in newRequestMessages)
				{
					AssertEquals(message.EM_MessageType + " message.EM_ApplicationCode", ApplicationCodeList.Codes.ESCustomsMessage, message.EM_ApplicationCode);
					AssertEquals(message.EM_MessageType + " message.EM_MessageSubType", ZString.Empty, message.EM_MessageSubType);
					AssertEquals(message.EM_MessageType + " message.EM_IsTestMessage", true, message.EM_IsTestMessage);
					AssertEquals(message.EM_MessageType + " message.EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
					AssertEquals(message.EM_MessageType + " message.EM_Status", EDIMessage.Status.Queued, message.EM_Status);
					AssertEquals(message.EM_MessageType + " message.EM_ApplicationReference", CertificateName, message.EM_ApplicationReference);
					AssertMultilineASCIIEquals(message.EM_MessageType + " message.EM_MessageText", GetExpectedNewInboxInterchangeBodyText_EHub(mrn, id, name), message.EM_MessageText);
					AssertNull(message.EM_MessageType + " message doesn't have interchange", message.Interchange);
				}
			});
		}

		protected void AssertNewCusPollingTransaction(ZGuid parentID, ZString parentTablePrefix, ZString[] messageTypes, string mrn)
		{
			CombineAssertions("Check new ESC CusPollingTransactions", () =>
			{
				Factory.Save();
				var query = new ZQuery(CusPollingTransactionSchema.CPT_ApplicationCode, Enterprise.Messaging.Integration.ApplicationCodeList.Codes.ESCustomsMessage);
				var newTransactions = Factory.Load<CusPollingTransaction>(query);

				AssertEquals("There should only be 1 ESC transaction for each messageType", messageTypes.Length, newTransactions.Length);

				foreach (var transaction in newTransactions)
				{
					AssertEquals(transaction.CPT_Type + " transaction.CPT_ApplicationCode", Enterprise.Messaging.Integration.ApplicationCodeList.Codes.ESCustomsMessage, transaction.CPT_ApplicationCode);
					AssertEquals(transaction.CPT_Type + " transaction.CPT_Status", Core.Constants.Customs.CusPollingTransactionStatus.Codes.OPN, transaction.CPT_Status);
					AssertEquals(transaction.CPT_Type + " transaction.CPT_Type is in list", true, messageTypes.Contains(transaction.CPT_Type));
					AssertEquals(transaction.CPT_Type + " transaction.CPT_TransactionID", mrn, transaction.CPT_TransactionID);
					AssertEquals(transaction.CPT_Type + " transaction.CPT_ParentID", parentID, transaction.CPT_ParentID);
					AssertEquals(transaction.CPT_Type + " transaction.CPT_ParentTableCode", parentTablePrefix, transaction.CPT_ParentTableCode);
				}
			});
		}

		protected virtual string WrongXMLTestFile => ESTestFileReader.GetEmbeddedFileText("Enterprise.Customs.ES.Business.Testing.MessageProcessor.TestFiles.XML", "WrongXML.txt");

		protected virtual TestEdiMessage SetDataForCorrectPreProcessing(ZString interchangeTransportType)
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var correctBO = Factory.New<TBusinessObject>();
				if (correctBO is CusEntryHeader entryHeader)
				{
					entryHeader.CH_BGMReference = "TEST";
				}

				if (correctBO is Declaration.CusExitDetail exitDetail)
				{
					exitDetail.CED_MovementReferenceNumber = "TEST";
				}

				if (correctBO is AsycudaBill bill)
				{
					bill.ABL_BillNumber = "TEST";
				}

				if (correctBO is AsycudaManifestHeader header)
				{
					header.AMA_JobReference = "TEST";
				}

				var interchangeID = ZGuid.NewZGuid();

				SetSentInterchange(correctBO, interchangeID);

				var message = Factory.New<TestEdiMessage>();
				message.EM_ApplicationReference = "TEST";
				var responseInterchange = CreateTestResponseInterchange(interchangeID, interchangeTransportType);
				responseInterchange.ContainedMessages.Add(message);
				return message;
			}
		}

		protected virtual TestEdiMessage SetDataForIncorrectApplicationReferencePreProcessing(ZString interchangeTransportType)
		{
			var message = SetDataForCorrectPreProcessing(interchangeTransportType);
			message.EM_ApplicationReference = "TEST2";
			return message;
		}

		protected string GetAllConcatenatedUserLogStrings() => string.Concat(logger.UserLogStrings.Cast<string>());

		protected virtual void AssertLoggerMessagesWhenPreProcessMessageWrongSessionGUID()
		{
			AssertContains("logger", "Unable to locate the related sent interchange", logger.UserLogStrings[0]);
		}

		protected abstract void AssertLoggerMessagesWhenProcessMessageWrongText(TestEdiMessage message, BusinessObject businessObject);

		protected ZString ApplicationReference => "ES000001";

		protected ZGuid BranchPK => Env.CurrentBranchPK;

		protected virtual ZString OriginalEntryStatus => "INI";

		protected virtual ZGuid InterchangeID => new ZGuid("5E5A9120-1794-4251-86AC-8038077F5BA6");

		protected ZString SentMessageNumber => "10";

		protected ZString MessageType => GetExpectedProcessorMessageTypesToInclude()[0];

		protected TestEdiMessage CreateNewEDIMessage(ZString reference, ZString messageText, ZGuid interchangeID, bool serializeMessageText = false, string interchangeTransportType = EDIInterchange.TransportType.eHub)
		{
			return CreateNewEDIMessage(reference, messageText, interchangeID, MessageType, serializeMessageText, interchangeTransportType);
		}

		protected TestEdiMessage CreateNewEDIMessage(ZString reference, ZString messageText, ZGuid interchangeID, ZString messageType, bool serializeMessageText = false, string interchangeTransportType = EDIInterchange.TransportType.eHub)
		{
			var message = Factory.New<TestEdiMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = messageType;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageSubType = "AAA";
			message.EM_ApplicationReference = reference;
			message.EM_GB = new ZGuid();
			message.EM_MessageText = serializeMessageText ? TestFileHelper.SerializeResponse(messageText) : messageText;

			var responseInterchange = CreateTestResponseInterchange(interchangeID, interchangeTransportType);
			responseInterchange.ContainedMessages.Add(message);

			Factory.Save();
			return message;
		}

		protected virtual EDIInterchange CreateTestResponseInterchange(ZGuid interchangeID, ZString interchangeTransportType)
		{
			var responseInterchange = Factory.New<EDIInterchange>();
			responseInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			responseInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			responseInterchange.EI_SessionGUID = interchangeID;
			responseInterchange.EI_From = SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub;
			responseInterchange.EI_To = "CW1";
			responseInterchange.EI_TransportType = interchangeTransportType;
			if (interchangeTransportType != EDIInterchange.TransportType.xT)
			{
				responseInterchange.EI_HeaderText = string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<Headers>
  <BrokerCode>AZ</BrokerCode>
  <CertificateName>{0}</CertificateName>
  <CertificateThumbPrint>CertThumbPrint</CertificateThumbPrint>
  <EntryReferenceNumber>1234123444</EntryReferenceNumber>
  <TestMessage>N</TestMessage>
  <Service>Service</Service>
  <Operation>Operation</Operation>
  <SentEDIMessageNumber>{1}</SentEDIMessageNumber>
</Headers>", CertificateName, SentMessageNumber);
			}

			return responseInterchange;
		}

		protected EDIInterchange SetSentInterchange(BusinessObject businessObject, ZGuid interchangeID)
		{
			var sentInterchange = Factory.New<EDIInterchange>();
			sentInterchange.EI_SessionGUID = interchangeID;
			sentInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			sentInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			sentInterchange.EI_From = "CW1";
			sentInterchange.EI_To = "ESCustoms";

			sentMessage = Factory.New<TestEdiMessage>();
			sentMessage.EM_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			sentMessage.EM_MessageType = MessageType;
			sentMessage.EM_Status = EDIMessage.Status.Queued;
			sentMessage.EM_MessageText = SentMessageText;
			sentMessage.EM_LinkedObject = businessObject;
			sentMessage.EM_MessageNum = SentMessageNumber;
			sentMessage.EM_MessageSubType = SentMessageSubType;
			sentInterchange.ContainedMessages.Add(sentMessage);

			return sentInterchange;
		}

		TestEdiMessage sentMessage;
		protected TestEdiMessage SentMessage => sentMessage;

		protected ZString SentMessageText => "Message text";

		protected virtual ZString SentMessageSubType => DeclarationMessageSubTypeList.Codes.OriginalDeclaration;

		protected ZString CertificateName => "CertName";

		protected void AssertDocumentRequestEDIMessages(IEnumerable<EDIMessage> messages, List<(ZString fileName, ZString urlParameter)> xmlFieldsList)
		{
			ZString GetExpectedMessageText(ZString filename, ZString urlParameter) => ZString.Format(@$"<?xml version=""1.0"" encoding=""utf-8""?>
<{XMLTestFileConstants.XmlElementNamespace}ESCustoms xmlns{XMLTestFileConstants.XmlElementNamespaceSuffix}=""http://www.wisetechglobal.com/eServices/Schemas/ESCustoms/DocumentRequest"">
  <{XMLTestFileConstants.XmlElementNamespace}File>
    <{XMLTestFileConstants.XmlElementNamespace}name>{filename}</{XMLTestFileConstants.XmlElementNamespace}name>
    <{XMLTestFileConstants.XmlElementNamespace}url_parameter>{urlParameter}</{XMLTestFileConstants.XmlElementNamespace}url_parameter>
  </{XMLTestFileConstants.XmlElementNamespace}File>
</{XMLTestFileConstants.XmlElementNamespace}ESCustoms>");

			foreach (var mes in messages)
			{
				AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.ESCustomsMessage, mes.EM_ApplicationCode);
				AssertEquals("message.EM_MessageType", DeclarationMessageTypeList.Codes.EsDocumentRequest, mes.EM_MessageType);
				AssertEquals("message.EM_MessageSubType", DeclarationMessageSubTypeList.Codes.EsDocumentRequest, mes.EM_MessageSubType);
				AssertEquals("message.EM_IsTestMessage", true, mes.EM_IsTestMessage);
				AssertEquals("message.EM_ReceiveTransmit", EDIMessage.Direction.Transmit, mes.EM_ReceiveTransmit);
				AssertEquals("message.EM_Status", EDIMessage.Status.Queued, mes.EM_Status);
				AssertEquals("message.EM_ApplicationReference", CertificateName, mes.EM_ApplicationReference);
				AssertNull("message doesn't have interchange", mes.Interchange);
			}

			var messageTextsList = new List<ZString>();
			foreach (var xmlFields in xmlFieldsList)
			{
				messageTextsList.Add(GetExpectedMessageText(xmlFields.fileName, xmlFields.urlParameter));
			}
			AssertContainsExactElementsInAnyOrder("messages created with correct EM_MessageText", messageTextsList, messages.Select(x => x.EM_MessageText).ToArray());
		}

		protected void GenericCommonAssertProcessResponse(TestEdiMessage responseMessage, string expectedMessageInterpretation = "", string emStatus = EDIMessage.Status.Received, string messageNum = "", string messageSubType = "")
		{
			AssertContains("Expected Accepted declaration message interpretation text", string.IsNullOrEmpty(expectedMessageInterpretation) ? responseMessage.EM_MessageInterpretation.ToString() : expectedMessageInterpretation, responseMessage.EM_MessageInterpretation);
			AssertEquals("EM_Status", emStatus, responseMessage.EM_Status);
			AssertEquals("EM_MessageNum", messageNum, responseMessage.EM_MessageNum);
			AssertEquals("EM_MessageSubType", messageSubType, responseMessage.EM_MessageSubType);
		}

		protected void GenericCommonAssertProcessResponseOthers(TestEdiMessage responseMessage, string expectedMessageInterpretation = "", string emStatus = EDIMessage.Status.Failed, string messageNum = "", string messageSubType = "AAA", string loggerDesc = "")
		{
			GenericCommonAssertProcessResponse(responseMessage, expectedMessageInterpretation, emStatus, messageNum, messageSubType);
			if (logger.UserLogStrings.Count > 0)
			{
				AssertContains("logger", loggerDesc, GetAllConcatenatedUserLogStrings());
			}
		}

		protected void GenericCommonAssertProcessResponseEntryHeader(TestEdiMessage responseMessage, CusEntryHeader entryHeader = null, string expectedMessageInterpretation = "", string emStatus = EDIMessage.Status.Failed, string messageNum = "", string messageSubType = "AAA", string chStatus = "", string loggerDesc = "", string chEntryStatus = "", string csvClearance = "", string csvImportCertificate = "", ZDateTime? clearanceDate = null, ZDateTime? arrivalLimit = null, string clearanceResult = "")
		{
			CombineAssertions(() =>
			{
				GenericCommonAssertProcessResponseOthers(responseMessage, expectedMessageInterpretation: expectedMessageInterpretation, emStatus: emStatus, messageNum: messageNum, messageSubType: messageSubType, loggerDesc: loggerDesc);
				AssertEquals("Message is associated correctly to the entryHeader", entryHeader.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("CH_Status", chStatus, entryHeader.CH_Status);
				AssertEquals("CH_EntryStatus", chEntryStatus, entryHeader.CH_EntryStatus);
				AssertEquals("CSVClearance", csvClearance, entryHeader.CSVClearance);
				AssertEquals("ZG_CSVImportCertificate", csvImportCertificate, entryHeader.ZG_CSVImportCertificate);
				AssertEquals("CH_EntryReleaseDate", clearanceDate == null ? ZDateTime.Empty : clearanceDate, entryHeader.CH_EntryReleaseDate);
				AssertEquals("ZG_LimitDateOfArrival", arrivalLimit == null ? ZDateTime.Empty : arrivalLimit, entryHeader.ZG_LimitDateOfArrival);
				AssertEquals("ZG_ClearanceResult", clearanceResult, entryHeader.ZG_ClearanceResult);
			});
		}

		protected void AssertProcessAcceptedMessageAndRemoveCusPollingTransactionsWhenxT(ZString mrnCode, Action createMessageAndProcessMethod)
		{
			using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(true))
			{
				AddCusPollingTransactionsToRemove(mrnCode);
				var transactionsESC = GetCusPollingTransactionsForCode();
				AssertEquals("There are 4 ESC transactions in total", 4, transactionsESC.Length);
				AssertEquals("There are 2 ESC transactions with messageType + mrn", 2, transactionsESC.Count(x => x.CPT_Type == MessageType && x.CPT_TransactionID == mrnCode));
				AssertEquals("There are 2 ESC transactions without messageType + mrn", 2, transactionsESC.Count(x => x.CPT_Type != MessageType || x.CPT_TransactionID != mrnCode));
				var transactionsTRC = GetCusPollingTransactionsForCode("TRC");
				AssertEquals("There should still be 1 TRC transaction", 1, transactionsTRC.Length);

				createMessageAndProcessMethod();

				transactionsESC = GetCusPollingTransactionsForCode();
				AssertEquals("There are only 2 ESC transactions in total after processing", 2, transactionsESC.Length);
				AssertEquals("There should be NO ESC transaction with messageType + mrn", false, transactionsESC.Any(x => x.CPT_Type == MessageType && x.CPT_TransactionID == mrnCode));
				AssertEquals("There should still be 2 ESC transactions without messageType + mrn", 2, transactionsESC.Count(x => x.CPT_Type != MessageType || x.CPT_TransactionID != mrnCode));
				transactionsTRC = GetCusPollingTransactionsForCode("TRC");
				AssertEquals("There should still be 1 TRC transaction", 1, transactionsTRC.Length);
			}
		}

		protected void AddCusPollingTransactionsToRemove(ZString transactionID)
		{
			var otherMessageType = MessageType == "NPI" ? "NPE" : "NPI";

			AddCusPollingTransaction("OPN", MessageType, transactionID);
			AddCusPollingTransaction("OPN", "T2O", transactionID, applicationCode: "TRC");
			AddCusPollingTransaction("CLS", MessageType, transactionID);
			AddCusPollingTransaction("OPN", otherMessageType, transactionID);
			AddCusPollingTransaction("OPN", MessageType, "AAA");
		}

		protected void AddCusPollingTransaction(ZString status, ZString type, ZString transactionID, string applicationCode = Enterprise.Messaging.Integration.ApplicationCodeList.Codes.ESCustomsMessage)
		{
			var transaction = Factory.NewWithValidTestData<CusPollingTransaction>();
			transaction.CPT_ApplicationCode = applicationCode;
			transaction.CPT_Status = status;
			transaction.CPT_Type = type;
			transaction.CPT_TransactionID = transactionID;
			transaction.CPT_NumberOfAttempts = 1;
		}

		protected CusPollingTransaction[] GetCusPollingTransactionsForCode(string applicationCode = Enterprise.Messaging.Integration.ApplicationCodeList.Codes.ESCustomsMessage)
		{
			var query = new ZQuery(CusPollingTransactionSchema.CPT_ApplicationCode, applicationCode);
			return Factory.Load<CusPollingTransaction>(query);
		}

		protected CusGuaranteeHeader[] LoadCusGuaranteeHeaderList()
		{
			var query = new ZQuery(CusPermitHeaderSchema.CPH_ApplicationCode, CusPermitHeaderApplicationCodeList.Codes.Guarantee);
			return Factory.Load<CusGuaranteeHeader>(query);
		}

		protected void SetUpGuarantee(string reference, string type, string mrnCode, string entryReference, decimal transactionValue, decimal oblTransactionValue, decimal balance, bool addTransactions = false, bool addOBLTransaction = true, string subType = "")
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789000", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			SetUpGuarantee(reference, type, mrnCode, entryReference, transactionValue, oblTransactionValue, balance, org1.PK, addTransactions: addTransactions, addOBLTransaction: addOBLTransaction, subType: subType);
		}
		protected void SetUpGuarantee(string reference, string type, string mrnCode, string entryReference, decimal transactionValue, decimal oblTransactionValue, decimal balance, ZGuid holderPK, bool addTransactions = false, bool addOBLTransaction = true, string subType = "")
		{
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Number = reference;
			guaranteeHeader.CPH_OH_PermitHolder = holderPK;
			guaranteeHeader.CPH_StartDate = new ZDate(2020, 7, 15);
			guaranteeHeader.CPH_EndDate = ZDate.Today.AddMonths(1);
			guaranteeHeader.CPH_SystemCreateTimeUtc = new ZDate(2020, 7, 15);
			guaranteeHeader.CPH_Type = type;
			guaranteeHeader.CPH_RN_NKCountryCode = EsCode;
			guaranteeHeader.CPH_Balance = balance;
			guaranteeHeader.CPH_UnitOfMeasure = "EUR";
			guaranteeHeader.CPH_SubType = subType;

			if (addOBLTransaction)
			{
				AddOBLTransaction(guaranteeHeader, oblTransactionValue);
			}

			if (addTransactions)
			{
				AddTransaction(guaranteeHeader, mrnCode, entryReference, "First-CON", transactionValue, PermitTransactionStatusList.Codes.Confirmed);
				AddTransaction(guaranteeHeader, mrnCode, entryReference, "Second-CON", 30m, PermitTransactionStatusList.Codes.Confirmed);
				AddTransaction(guaranteeHeader, mrnCode, entryReference, "Third-CON", -90m, PermitTransactionStatusList.Codes.Confirmed);
				AddTransaction(guaranteeHeader, mrnCode, entryReference, "Fourth-PEN", -40m, PermitTransactionStatusList.Codes.Pending);
			}

			Factory.Save();
		}

		protected void AddOBLTransaction(CusGuaranteeHeader guaranteeHeader, ZDecimal value)
		{
			var transaction = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			transaction.CPL_Reference = "OPENING";
			transaction.CPL_TranValue = value;
			transaction.CPL_Comment = "OPENING";
			transaction.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
		}

		protected void AddTransaction(CusGuaranteeHeader guaranteeHeader, ZString mrnCode, ZString entryReference, ZString comment, ZDecimal value, ZString status)
		{
			var halfValue = value / 2;
			var transaction1 = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			transaction1.CPL_Reference = mrnCode;
			transaction1.CPL_TranValue = halfValue;
			transaction1.CPL_TransactionStatus = status;
			transaction1.CPL_Comment = comment;
			transaction1.CPL_TransactionType = PermitTransactionTypeList.Codes.TRA;

			var transaction2 = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			transaction2.CPL_Reference = entryReference;
			transaction2.CPL_TranValue = halfValue;
			transaction2.CPL_TransactionStatus = status;
			transaction2.CPL_Comment = comment;
			transaction2.CPL_TransactionType = PermitTransactionTypeList.Codes.TRA;

			var transaction3 = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			transaction3.CPL_Reference = "OtherRef";
			transaction3.CPL_TranValue = value;
			transaction3.CPL_TransactionStatus = status;
			transaction3.CPL_Comment = comment;
			transaction3.CPL_TransactionType = PermitTransactionTypeList.Codes.TRA;
		}

		protected void AssertNewTransaction(string message, ZString commentPrefix, SharedCusPermitLineTransaction transaction, ZString mrnCode, ZString entryReference, ZDecimal amount, ZDateTime acceptanceDate, bool containsAdjSuffix = true, bool containsCancelledSuffix = false)
		{
			var comment = commentPrefix + " " + entryReference + (containsAdjSuffix ? "(Customs Adj)" : (containsCancelledSuffix ? "(Canceled)" : ""));

			AssertEquals(message + ".CPL_Reference", mrnCode, transaction.CPL_Reference);
			AssertEquals(message + ".CPL_Comment", comment, transaction.CPL_Comment);
			AssertEquals(message + ".CPL_TranValue", amount, transaction.CPL_TranValue);
			AssertEquals(message + ".CPL_TransactionDate", acceptanceDate, transaction.CPL_TransactionDate);
			AssertEquals(message + ".CPL_TransactionStatus", PermitTransactionStatusList.Codes.Confirmed, transaction.CPL_TransactionStatus);
		}

		protected class BranchCustomsMessageProcessorForTest : BranchCustomsMessageProcessor
		{
			public BranchCustomsMessageProcessorForTest()
			: base(new ZString[] { ApplicationCodeList.Codes.ESCustomsMessage }, Enumerable.Empty<ZString>())
			{
			}
		}
	}
}
