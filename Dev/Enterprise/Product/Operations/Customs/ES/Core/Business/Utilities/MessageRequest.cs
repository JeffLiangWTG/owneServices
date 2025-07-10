using CargoWise.Customs.ES.MessageDefinitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.EDIMessages;
using Enterprise.Customs.ES.Business.MessageSending;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.Business
{
	public static class MessageRequest
	{
		public static ZString GetInboxRequestBodyText(ZString mrn, OrgHeader declarant)
		{
			var body = new Body()
			{
				Mrn = mrn,
				DeclarantName = declarant?.OH_FullName ?? ZString.Empty,
				DeclarantId = declarant?.GetNIFCode()
			};
			return CargoWise.Customs.Shared.MessageContracts.XmlObjectSerializer.SerializeDefaultSettingsWithoutNamespaces(body);
		}

		public static void CreateEDIMessageForInboxRequest(BusinessObjectFactory requestFactory, IPollingTransactionParent boWithMessagesDeclarantAndIsTest, ZGuid boPK, ZString boTablePrefix, ZString mrn, ZString boReference, ZString messageType, ZString certificateName)
		{
			var isDirectXTInboxInterface = Registry.ESCustomsDataRegistry.Instance.EnableESInboxMessagesThroughDirectxTInterface.Value;

			if (isDirectXTInboxInterface)
			{
				RemoveExistingOPNCusPollingTransactions(requestFactory, messageType, mrn);
				CreateNewCusPollingTransaction(requestFactory, boPK, boTablePrefix, messageType, mrn);
			}
			else
			{
				var messageToSend = GetEDIMessageForEhubInboxRequest(requestFactory, messageType, certificateName, mrn, boWithMessagesDeclarantAndIsTest.Declarant, boWithMessagesDeclarantAndIsTest.IsTest, boReference);

				boWithMessagesDeclarantAndIsTest.MessageCollection.Add(messageToSend);
			}
		}

		static void RemoveExistingOPNCusPollingTransactions(BusinessObjectFactory requestFactory, ZString type, ZString transactionID)
		{
			var query = new ZQuery(CusPollingTransactionSchema.CPT_ApplicationCode, Enterprise.Messaging.Integration.ApplicationCodeList.Codes.ESCustomsMessage);
			query.AddToFilter(CusPollingTransactionSchema.CPT_Status, Core.Constants.Customs.CusPollingTransactionStatus.Codes.OPN);
			query.AddToFilter(CusPollingTransactionSchema.CPT_Type, type);
			query.AddToFilter(CusPollingTransactionSchema.CPT_TransactionID, transactionID);
			var transactionsToRemove = requestFactory.Load<CusPollingTransaction>(query);

			foreach (var transaction in transactionsToRemove)
			{
				transaction.Delete();
			}
		}

		static void CreateNewCusPollingTransaction(BusinessObjectFactory requestFactory, ZGuid boPK, ZString boTablePrefix, ZString type, ZString transactionID)
		{
			var transaction = requestFactory.New<CusPollingTransaction>();
			transaction.CPT_ApplicationCode = Enterprise.Messaging.Integration.ApplicationCodeList.Codes.ESCustomsMessage;
			transaction.CPT_Status = Core.Constants.Customs.CusPollingTransactionStatus.Codes.OPN;
			transaction.CPT_Type = type;
			transaction.CPT_TransactionID = transactionID;
			transaction.CPT_ParentID = boPK;
			transaction.CPT_ParentTableCode = boTablePrefix;
			transaction.CPT_NumberOfAttempts = 1;
		}

		static ESEDIMessage GetEDIMessageForEhubInboxRequest(BusinessObjectFactory requestFactory, ZString messageType, ZString certificateName, ZString mrn, OrgHeader declarant, ZBool isTest, ZString boReference)
		{
			var messageToSend = requestFactory.New<ESEDIMessage>();
			messageToSend.EM_MessageType = messageType;
			messageToSend.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			messageToSend.EM_MessageText = GetInboxRequestBodyText(mrn, declarant);
			messageToSend.EM_Status = EDIMessage.Status.Queued;
			messageToSend.EM_IsTestMessage = isTest;
			messageToSend.EM_ApplicationReference = certificateName;
			messageToSend.BusinessObjectReference = boReference;
			return messageToSend;
		}

		public static void CreateEDIMessageForEffectiveDepCertRequest(BusinessObjectFactory requestFactory, CusEntryHeader entryHeader, ZString certificateName)
		{
			var declaration = entryHeader.Declaration;
			var broker = declaration.CusAgent;
			if (broker != null)
			{
				var certificateObject = new CertificateObject(broker, certificateName, ZString.Empty);

				var messageBuilder = new DepartureCertReqAESMessageBuilder(new DepartureCertReqAESSendMessageWrapper(entryHeader, certificateObject), DeclarationMessageTypeList.Codes.RequestExportExitCertificate, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
				var messageCreator = new EDIMessageCreator(messageBuilder, requestFactory);
				var messageToSend = messageCreator.CreateMessage();
				entryHeader.Messages.Add(messageToSend);
			}
		}
	}
}
