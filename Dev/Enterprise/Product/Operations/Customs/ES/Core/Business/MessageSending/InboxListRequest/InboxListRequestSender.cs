using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.EDIMessages;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.Business.MessageSending
{
	public class InboxListRequestSender
	{
		public InboxListRequestSender(ILogger serviceLogger)
		{
			ServiceLogger = serviceLogger;
		}
		ILogger ServiceLogger { get; set; }

		public void RemoveExpiredCusPollingTransactions(BusinessObjectFactory factory)
		{
			try
			{
				var transactions = GetCusPollingTransactionList(factory, getExpiredOnes: true);

				var removedTransactionsDataForLog = new List<ZString>();

				foreach (var transaction in transactions)
				{
					removedTransactionsDataForLog.Add(GetDeletedTransactionMessage(transaction.CPT_Type, transaction.CPT_TransactionID, transaction.CPT_SystemCreateTimeUtc));
					transaction.Delete();
				}
				factory.Save();

				ServiceLogger.Log(LogType.Information, GetRemovedTransactions(removedTransactionsDataForLog.Count));
				foreach (var removedTransactionData in removedTransactionsDataForLog)
				{
					ServiceLogger.Log(LogType.Information, removedTransactionData);
				}
			}
			catch (Exception ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
		}

		public void CreateListPollingEDIMessages(BusinessObjectFactory factory, ZString[] specificTypes = null)
		{
			try
			{
				var createdMessagesDataForLog = new List<ZString>();
				var isInboxDirectXTInterface = Registry.ESCustomsDataRegistry.Instance.EnableESInboxMessagesThroughDirectxTInterface.Value;
				if (isInboxDirectXTInterface)
				{
					var transactions = GetCusPollingTransactionList(factory, specificTypes: specificTypes);

					var sentPollingDataList = new List<ZString>();

					foreach (var transaction in transactions)
					{
						var type = transaction.CPT_Type;
						var parent = transaction.ParentObject as IPollingTransactionParent;
						if (parent != null && !type.IsEmpty && !parent.CertificateName.IsEmpty && parent.Broker != null)
						{
							var pollingData = new PollingData
							{
								TransactionType = type,
								CertificateName = parent.CertificateName,
								IsTest = parent.IsTest
							};

							var pollingDataStringForList = pollingData.TransactionType + pollingData.CertificateName + pollingData.IsTest;
							if (!sentPollingDataList.Contains(pollingDataStringForList))
							{
								var message = CreateNewPollingEDIMessage(factory, pollingData, parent.Broker, parent.Declarant);
								sentPollingDataList.Add(pollingDataStringForList);
								createdMessagesDataForLog.Add(GetCreatedMessageTextForLog(message.EM_MessageType, message.EM_ReceiveTransmit, message.EM_IsTestMessage, type, parent.CertificateName));
							}
							transaction.CPT_Status = Core.Constants.Customs.CusPollingTransactionStatus.Codes.PND;
						}
					}
					factory.Save();
				}

				if (ServiceLogger != null)
				{
					ServiceLogger.Log(LogType.Information, GetCreatedMessagesTextForLog(createdMessagesDataForLog.Count));
					foreach (var createdMessagesData in createdMessagesDataForLog)
					{
						ServiceLogger.Log(LogType.Information, createdMessagesData);
					}
				}
			}
			catch (Exception ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
		}

		CusPollingTransaction[] GetCusPollingTransactionList(BusinessObjectFactory factory, bool getExpiredOnes = false, ZString[] specificTypes = null)
		{
			var query = new ZQuery(CusPollingTransactionSchema.CPT_ApplicationCode, Enterprise.Messaging.Integration.ApplicationCodeList.Codes.ESCustomsMessage);
			query.AddToFilter(CusPollingTransactionSchema.CPT_Status, SQLComparisonOperator.Equal, "OPN");

			if (getExpiredOnes)
			{
				query.AddToFilter(CusPollingTransactionSchema.CPT_SystemCreateTimeUtc, SQLComparisonOperator.LessThan, ZDateTime.Now.AddDays(-(ESCustomsDataRegistry.Instance.InboxXTExpirationPeriodDays.Value)));
			}

			if (!specificTypes.IsNullOrEmpty())
			{
				var typeSubQuery = new ZDBOnlyQuery(typeof(CusPollingTransaction));
				specificTypes.ForEach(t => typeSubQuery.AddToFilter(JoinCondition.Or, CusPollingTransactionSchema.CPT_Type, t));

				query.AddToFilter(typeSubQuery, JoinCondition.And);
			}

			return factory.Load<CusPollingTransaction>(query);
		}

		ESEDIMessage CreateNewPollingEDIMessage(BusinessObjectFactory factory, PollingData pollingData, GlbStaff broker, OrgHeader declarant)
		{
			var certificateObject = new CertificateObject(broker, pollingData.CertificateName, ZString.Empty);

			var messageBuilder = new InboxNotificationMessageBuilder(new InboxNotificationSendMessageWrapper(factory, declarant, pollingData.IsTest, pollingData.TransactionType, certificateObject), DeclarationMessageTypeList.Codes.InboxPendingList, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
			var messageCreator = new EDIMessageCreator(messageBuilder, factory);
			return messageCreator.CreateMessage();
		}

		ZString GetDeletedTransactionMessage(ZString type, ZString transactionID, ZDateTime createTime) => Res.GetString("D29AB1F4-45A2-488C-8EC0-B8DF3B9F44C2", "Polling Transaction removed with Type: {0}, Transaction ID: {1}, System Create Time (universal time): {2}", type, transactionID, createTime.ToLongTimeString());

		ZString GetRemovedTransactions(int numberOfTransactions) => numberOfTransactions != 1 ? Res.GetString("15C2534D-BC68-4984-A4F8-33AD7E7F8391", "Removed {0} Polling Transactions", numberOfTransactions) : Res.GetString("A82BF54B-3964-4C6F-BBD6-0D61B4FA4E71", "Removed {0} Polling Transaction", numberOfTransactions);

		ZString GetCreatedMessageTextForLog(ZString messageType, ZString receiveTransmit, ZBool isTest, ZString transactionType, ZString certName) => Res.GetString("261B135D-0248-4B99-9028-421FEC7486FB", "EDI message created with Message Type: {0}, Receive Transmit: {1}, Is Test Message: {2} for Polling Transaction.Type: {3} and Certificate: {4}", messageType, receiveTransmit, isTest, transactionType, certName);

		ZString GetCreatedMessagesTextForLog(int numberOfmessages) => numberOfmessages != 1 ? Res.GetString("E0AB243C-75AC-4B01-B1E5-DED8A796F49E", "Created {0} EDI Messages", numberOfmessages) : Res.GetString("14988FD5-88ED-4EF7-9DA2-D8F0971AA4F8", "Created {0} EDI Message", numberOfmessages);

		class PollingData
		{
			public ZString TransactionType { get; set; }
			public ZString CertificateName { get; set; }
			public ZBool IsTest { get; set; }
		}
	}
}
