using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Messaging.MessageStructure.IRISP;

namespace Enterprise.Customs.IT.Business;

public abstract class SadIrispMessageSubProcessor : SadMessageSubProcessorBase
{
	protected SadIrispMessageSubProcessor(ISadCustomsLinkedObjectAdapter entryAdapter) : base(entryAdapter)
	{
	}

	public void PerformActionsForPositiveIrisp(SadPositiveResponseMessage sadPositiveResponseMessage)
	{
		Argument.NotNull(sadPositiveResponseMessage, nameof(sadPositiveResponseMessage));
		var (newMessageStatus, newEntryCustomsStatus) = CheckAllowedPerformActionsForPositiveIrisp(EntryAdapter, sadPositiveResponseMessage);

		EntryAdapter.SetMessageStatus(newMessageStatus);
		EntryAdapter.SetEntryCustomsStatus(newEntryCustomsStatus);
		ApproveAllSentOrRejectedCustomsLinesIfCleared();
		SetEntryReleaseDateIfCleared(sadPositiveResponseMessage.IdocElaborationDateTime);
		UpdateOrInsertEntryNumbers(sadPositiveResponseMessage, EntryAdapter.GetEntryNumbers());
		QueueSingleWindowUpdatesRequest();
		PerformActionsForPositiveIrispCore();
		GenerateDocuments();
	}

	public void PerformActionsForNegativeIrisp(SadNegativeResponseMessage negativeResponseMessage)
	{
		Argument.NotNull(negativeResponseMessage, nameof(negativeResponseMessage));
		CheckAllowedPerformActionsForNegativeIrisp();

		EntryAdapter.SetMessageStatus(EntryAdapter.StatusProvider.ErrorMessageStatus);
		RejectAllSentCustomsLines();
		PerformActionsForNegativeIrispCore();
	}

	public void UpdateEntryStatusAfterChildMessagesProcessing()
	{
		UpdateEntryStatusAfterNbMessageProcessing();
		UpdateEntryStatusAfterChildMessagesProcessingCore();
	}

	protected void UpdateOrInsertEntryNum(IEnumerable<CusEntryNumber> entryNumbers, ZString entryType, ZString entryNum, ZString entryLineReference, ZDateTime issueDate)
	{
		var entryNumber = entryNumbers.SingleOrDefault(x => x.CE_EntryType == entryType && x.CE_EntryLineReference == entryLineReference);
		if (entryNumber == null)
		{
			entryNumber = EntryAdapter.GetNewCusEntryNumber();
			entryNumber.CE_EntryType = entryType;
		}

		entryNumber.CE_EntryNum = entryNum;
		entryNumber.CE_EntryLineReference = entryLineReference;
		entryNumber.CE_IssueDate = issueDate;
	}

	protected override IEnumerable<CustomsStatusOrder> LoadCustomsStatusWithInformationOrderCollection() => EntryAdapter.StatusProvider.StatusWithInformationOrderCollection;

	protected virtual void UpdateOrInsertSpecificEntryNumbers(SadPositiveResponseMessage responseMessage, IEnumerable<CusEntryNumber> entryNumberCollection) { }

	protected virtual void PerformActionsForPositiveIrispCore() { }

	protected virtual void PerformActionsForNegativeIrispCore() { }

	protected virtual void UpdateEntryStatusAfterChildMessagesProcessingCore() { }

	#region Implementation

	(ZString newMessageStatus, ZString newEntryCustomsStatus) CheckAllowedPerformActionsForPositiveIrisp(ISadCustomsLinkedObjectAdapter entryAdapter, SadPositiveResponseMessage responseMessage)
	{
		var (newMessageStatus, newEntryCustomsStatus) = GetNewEntryStatus(responseMessage);

		if (!IsChangeStatusAllowed(entryAdapter.EntryCustomsStatus, newEntryCustomsStatus))
		{
			throw new IncomingMessageDoesNotMatchWithStatusException(GetInvalidAttemptToChangeStatusExceptionMessage());
		}
		return (newMessageStatus, newEntryCustomsStatus);
	}

	void CheckAllowedPerformActionsForNegativeIrisp()
	{
		var messageStatus = EntryAdapter.MessageStatus;
		var isAwaitingOrAcknowledgedMessageStatus = messageStatus == EntryAdapter.StatusProvider.AwaitingMessageStatus || messageStatus == EntryAdapter.StatusProvider.AcknowledgedMessageStatus;
		if (!EntryAdapter.EntryCustomsStatus.IsEmpty || !isAwaitingOrAcknowledgedMessageStatus)
		{
			throw new IncomingMessageDoesNotMatchWithStatusException(GetInvalidAttemptToChangeStatusExceptionMessage());
		}
	}

	(ZString newMessageStatus, ZString newEntryCustomsStatus) GetNewEntryStatus(SadPositiveResponseMessage responseMessage)
	{
		ZString newEntryCustomsStatus;
		ZString newMessageStatus;
		switch (responseMessage.ReleaseNotes)
		{
			case Empty:
			case SadPositiveResponseMessage.AwaitReponseReleaseNote:
				newMessageStatus = EntryAdapter.StatusProvider.AcknowledgedMessageStatus;
				newEntryCustomsStatus = EntryAdapter.StatusProvider.RegisteredCustomsStatus;
				break;
			case SadPositiveResponseMessage.UnderControlReleaseNote:
				newMessageStatus = EntryAdapter.StatusProvider.AcknowledgedMessageStatus;
				newEntryCustomsStatus = EntryAdapter.StatusProvider.UnderControlCustomsStatus;
				break;
			case SadPositiveResponseMessage.ClereanceReleaseNote:
				newMessageStatus = EntryAdapter.StatusProvider.ClearedMessageStatus;
				newEntryCustomsStatus = EntryAdapter.StatusProvider.ClearedCustomsStatus;
				break;
			default:
				throw new CustomsMessageProcessorException(Res.GetString("151F807E-22B7-4D6B-B20B-96034ECA5070", "Unable to manage the SAD IRISP Release Note: \"{0}\"", responseMessage.ReleaseNotes));
		}
		return (newMessageStatus, newEntryCustomsStatus);
	}

	void SetEntryReleaseDateIfCleared(ZDateTime releaseDateTime)
	{
		if (EntryAdapter.MessageStatus == EntryAdapter.StatusProvider.ClearedMessageStatus)
		{
			EntryAdapter.SetEntryReleaseDate(releaseDateTime);
		}
	}

	void UpdateOrInsertEntryNumForRegistrationNumber(SadPositiveResponseMessage responseMessage, IEnumerable<CusEntryNumber> entryNumbers) => UpdateOrInsertEntryNum(entryNumbers, CusEntryNumberConstants.EntryTypes.RegistrationNumber, responseMessage.RegistrationInfo, responseMessage.CustomsOffice, responseMessage.RegistrationDate);

	void UpdateOrInsertEntryNumbers(SadPositiveResponseMessage responseMessage, IEnumerable<CusEntryNumber> entryNumberCollection)
	{
		UpdateOrInsertEntryNumForRegistrationNumber(responseMessage, entryNumberCollection);
		UpdateOrInsertEntryNumForReleaseCodeIfCleared(responseMessage, entryNumberCollection);
		EntryAdapter.InsertOrUpdateA93Numbers(responseMessage);
		UpdateOrInsertSpecificEntryNumbers(responseMessage, entryNumberCollection);
	}

	void UpdateOrInsertEntryNumForReleaseCodeIfCleared(SadPositiveResponseMessage responseMessage, IEnumerable<CusEntryNumber> entryNumbers)
	{
		if (responseMessage.IsCleared)
		{
			UpdateOrInsertEntryNum(entryNumbers, CusEntryNumberConstants.EntryTypes.ClereanceCode, responseMessage.ReleaseCode, ZString.Empty, responseMessage.IdocElaborationDateTime);
		}
	}

	void ApproveAllSentOrRejectedCustomsLinesIfCleared()
	{
		if (EntryAdapter.EntryCustomsStatus == EntryAdapter.StatusProvider.ClearedCustomsStatus)
		{
			var customsLinesToBeApproved = EntryAdapter.CustomsLines.Where(x => x.NBStatus == EntryLineCustomsStatusList.Codes.Sent || x.NBStatus == EntryLineCustomsStatusList.Codes.Rejected);
			customsLinesToBeApproved.ForEach(x => x.SetNBStatus(EntryLineCustomsStatusList.Codes.Approved));
		}
	}

	void RejectAllSentCustomsLines()
	{
		var customsLinesToBeRejected = EntryAdapter.CustomsLines.Where(x => x.NBStatus == EntryLineCustomsStatusList.Codes.Sent);
		customsLinesToBeRejected.ForEach(x => x.SetNBStatus(EntryLineCustomsStatusList.Codes.Rejected));
	}

	void QueueSingleWindowUpdatesRequest()
	{
		new ElectronicFolderUpdatesRequester(EntryAdapter.SingleWindowRequestDataProvider, EntryAdapter.Factory).RequestUpdates();
	}

	void UpdateEntryStatusAfterNbMessageProcessing()
	{
		if (EntryAdapter.IsEntryRegisteredOrNbRejected)
		{
			var newEntryCustomsStatus = EntryAdapter.CustomsLines.Any(x => x.NBStatus == EntryLineCustomsStatusList.Codes.Rejected) ? EntryAdapter.StatusProvider.NbRejectedCustomsStatus : EntryAdapter.StatusProvider.RegisteredCustomsStatus;
			EntryAdapter.SetEntryCustomsStatus(newEntryCustomsStatus);
		}
	}

	void GenerateDocuments()
	{
		try
		{
			EntryAdapter.GenerateDocuments();
		}
		catch (Exception ex)
		{
			throw new CustomsMessageProcessorException(Res.GetString("8DDBDDF0-F6D2-4A82-A8FB-0A279610D1BF", "Generation documents process failed"), ex);
		}
	}

	#endregion
}
