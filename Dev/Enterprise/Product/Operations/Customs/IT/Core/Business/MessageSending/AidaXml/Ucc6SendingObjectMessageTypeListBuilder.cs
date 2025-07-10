using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business;

sealed class Ucc6SendingObjectMessageTypeListBuilder
{
	public Ucc6SendingObjectMessageTypeListBuilder(CusEntryHeader entryHeader)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
	}

	readonly CusEntryHeader entryHeader;

	public CodeDescriptionPairList GetMessageTypeList()
	{
		var isNewDeclarationAllowed = IsNewDeclarationMessageTypeAllowed();
		var isCancellationAllowed = IsCancellationMessageTypeAllowed();
		var isAmendmentAllowed = IsAmendmentMessageTypeAllowed();

		var cacheKey = FormattableString.Invariant($"IT.SendingObjectMessageTypeListBuilder_{isNewDeclarationAllowed}_{isCancellationAllowed}_{isAmendmentAllowed}");
		return entryHeader.Factory.GetCachedValue(cacheKey, () => GetMessageTypeList(isNewDeclarationAllowed, isCancellationAllowed, isAmendmentAllowed));
	}

	CodeDescriptionPairList GetMessageTypeList(bool isNewDeclarationAllowed, bool isCancellationAllowed, bool isAmendmentAllowed)
	{
		var result = new CodeDescriptionPairList();

		if (isNewDeclarationAllowed)
		{
			result.AddPair(EDIMessageTypeList.Codes.NewDeclaration, EDIMessageTypeList.Descriptions.NewDeclaration);
		}

		if (isCancellationAllowed)
		{
			result.AddPair(EDIMessageTypeList.Codes.Cancellation, EDIMessageTypeList.Descriptions.Cancellation);
		}

		if (isAmendmentAllowed)
		{
			result.AddPair(EDIMessageTypeList.Codes.Amendment, EDIMessageTypeList.Descriptions.Amendment);
		}

		return result;
	}

	bool IsNewDeclarationMessageTypeAllowed() => !entryHeader.IsInAmendingStatus;

	bool IsCancellationMessageTypeAllowed()
	{
		if (entryHeader.MovementReferenceNumber.IsEmpty)
		{
			return false;
		}

		switch (entryHeader.CH_EntryStatus)
		{
			case ITEntryStatusList.Codes.Registered:
			case ITEntryStatusList.Codes.ImportCleared:
			case ITEntryStatusList.Codes.ExportCleared:
			case ITEntryStatusList.Codes.UnderControl:
			case ITEntryStatusList.Codes.Deposited:
			case ITEntryStatusList.Codes.Amended:
				return true;

			case ITEntryStatusList.Codes.Canceling:
				return IsFailedForTransmissionOrError(entryHeader.CH_Status);

			case ITEntryStatusList.Codes.Amending:
				return IsFailedForTransmissionOrErrorOrEmpty(entryHeader.CH_Status);

			default:
				return false;
		}
	}

	bool IsFailedForTransmissionOrError(ZString messageStatus)
	{
		return messageStatus == ITMessageStatusList.Codes.FailedFromTransmission
			|| messageStatus == ITMessageStatusList.Codes.ErrorOriginal;
	}

	bool IsFailedForTransmissionOrErrorOrEmpty(ZString messageStatus)
	{
		return messageStatus.IsEmpty || IsFailedForTransmissionOrError(messageStatus);
	}

	bool IsAmendmentMessageTypeAllowed()
	{
		var messageStatus = entryHeader.CH_Status;

		return entryHeader.IsInAmendingStatus
			&& (messageStatus.IsEmpty || new CusEntryHeaderAmendmentWrapper(entryHeader).IsFailedSubmissionAttempt);
	}
}
