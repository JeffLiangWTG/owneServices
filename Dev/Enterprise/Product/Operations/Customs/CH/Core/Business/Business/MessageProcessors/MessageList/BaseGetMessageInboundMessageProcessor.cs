using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public abstract class BaseGetMessageInboundMessageProcessor : BaseMessageProcessor
{
	public BaseGetMessageInboundMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeCodeList.Codes.MSG };

	internal protected virtual bool LinkMessageToCompany => false;

	internal protected virtual bool UpdateApplicationReference => true;

	protected override void ProcessMessageCore(CHEDIMessage incomingEdiMessage)
	{
		var (company, messageId, transaction) = LoadCompanyAndTransactionFromOutgoingMessage(incomingEdiMessage);

		if (!messageId.IsEmpty && UpdateApplicationReference)
		{
			incomingEdiMessage.EM_ApplicationReference = messageId;
		}

		if (company != null)
		{
			if (LinkMessageToCompany)
			{
				incomingEdiMessage.EM_LinkedObject = company;
			}

			if (transaction != null)
			{
				if (MustProcessInOrder && CompanyPollingTransaction.PendingStatusCodes.Contains(transaction.CPT_Status.ToString()))
				{
					var pendingTransaction = company.LoadFirstPendingMessageIdTransaction(ApplicationCode);
					if (pendingTransaction != null && transaction.PK != pendingTransaction.PK)
					{
						var mustInOrderMessage = $"MSG Messages must be processed in order, current Message Id: {transaction.CPT_TransactionID} - next pending Message Id: {pendingTransaction.CPT_TransactionID}";
						Logger.LogWarning(mustInOrderMessage);

						throw new MessageProcessLockException(mustInOrderMessage);
					}
				}
				UpdateTransaction(transaction);
			}

			ProcessMessageCore(company, incomingEdiMessage);
		}
	}

	protected virtual void ProcessMessageCore(GlbCompany company, CHEDIMessage incomingEdiMessage) { }

	protected internal virtual bool MustProcessInOrder => true;

	protected abstract void UpdateTransaction(CusPollingTransaction transaction);

	protected override void SetHeldUntilDate(EDIMessage message, IEnumerable<EDIMessage> unprocessedMessages)
	{
		message.EM_HeldUntilDate = ZDateTime.UtcNow.AddMinutes(1);
	}

	protected override void PostProcessOnExceptionCore(EDIMessage message)
	{
		base.PostProcessOnExceptionCore(message);

		if (MustProcessInOrder)
		{
			var (_, _, transaction) = LoadCompanyAndTransactionFromOutgoingMessage(message);
			if (transaction != null)
			{
				transaction.CPT_Status = CompanyPollingTransaction.StatusCodes.Skip;
				transaction.CPT_StatusReason = (NoResString)"Message processing failed";
			}
		}
	}

	(GlbCompany, ZString, CusPollingTransaction) LoadCompanyAndTransactionFromOutgoingMessage(EDIMessage incomingEdiMessage)
	{
		CusPollingTransaction transaction = null;
		GlbCompany company = null;
		var messageId = ZString.Empty;

		if (incomingEdiMessage.Interchange != null)
		{
			var outgoingEdiMessage = incomingEdiMessage.Factory.GetOutgoingMessageFromSessionId(incomingEdiMessage.Interchange.EI_SessionGUID);

			if (outgoingEdiMessage?.EM_LinkedObject is GlbCompany)
			{
				company = outgoingEdiMessage?.EM_LinkedObject as GlbCompany;
				messageId = outgoingEdiMessage.EM_ApplicationReference;
				if (messageId.IsEmpty)
				{
					Logger.LogError($"Cannot get Message Id (EM_ApplicationReference) from outgoing Message: PK={outgoingEdiMessage.PK}");
				}
				else
				{
					transaction = company.LoadTransactionByMessageId(ApplicationCode, messageId);
					if (transaction == null)
					{
						Logger.LogError($"Cannot find Transaction by Message Id {messageId}");
					}
				}
			}
		}
		return (company, messageId, transaction);
	}
}
