using System;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public abstract class ESNCTSResponseMessageProcessor<TResponseProvider> : ESCommonResponseMessageProcessor<NctsHeader, TResponseProvider>
	{
		protected ESNCTSResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver = null) : base(logger, eDocsSaver)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"ES NCTS Generic Response Message Processor";

		protected void CreateOrUpdateCusEntryNumber(NctsHeader header, ZString entryType, ZString entryNum, ZString entryStatus, ZDateTime issueDate, ZDateTime expiryDate, bool shouldChangeStatus = true, bool shouldChangeIssueDate = true)
		{
			var newEntryNumber = CusEntryNumber.LoadOrCreate(header, entryType, Core.Constants.CountryCodes.Spain);
			newEntryNumber.CE_EntryNum = entryNum;
			if (shouldChangeStatus)
			{
				newEntryNumber.CE_EntryStatus = entryStatus;
			}
			if (shouldChangeIssueDate)
			{
				newEntryNumber.CE_IssueDate = (entryType == CusEntryNumberTypes.Standard.MovementReferenceNumber) ? SetEntryIssueDate(newEntryNumber.CE_IssueDate, issueDate) : issueDate;
			}
			newEntryNumber.CE_ExpiryDate = expiryDate;
			newEntryNumber.CE_EntryIsSystemGenerated = true;
		}

		protected void UpdateGuaranteeTransactionsIfNeeded(EDIMessage message, ZString status, Action<Customs.Business.SharedCusPermitLineTransaction> updateTransactionAction = null)
		{
			var sentMessage = MessageProcessorHelper.GetOutgoingMessage(message);
			Customs.Business.PermitHelper.UpdatePendingTransactions(message, sentMessage, (msg) => NctsPermitHelper.GetPermitAppIdForMessage(sentMessage), Core.Constants.CountryCodes.Spain, false, status, updateTransactionAction);
		}

		protected static Action<Customs.Business.SharedCusPermitLineTransaction> UpdateTransactionReference(string mrn)
		{
			return transaction =>
			{
				if (!string.IsNullOrEmpty(mrn))
				{
					transaction.CPL_Reference = mrn;
				}
			};
		}

		protected ZString SetNctsMessageFailedLogDescription(NctsHeader header, ZString expectedType) => Res.GetString("6E97F3DD-50A0-4D16-B4FC-D603261A36E6", "Header type is {0} so can't process {1} response message", header.BH_HeaderType, expectedType);
	}
}
