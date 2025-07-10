using System;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

sealed class CusEntryHeaderAmendmentWrapper
{
	public CusEntryHeaderAmendmentWrapper(CusEntryHeader entryHeader)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));

		isInAmendableStatusLazy = new Lazy<bool>(GetIsInAmendableStatus);
		isFailedSubmissionAttemptLazy = new Lazy<bool>(GetIsFailedSubmissionAttempt);
	}

	readonly CusEntryHeader entryHeader;
	readonly Lazy<bool> isInAmendableStatusLazy;
	readonly Lazy<bool> isFailedSubmissionAttemptLazy;

	public bool IsInAmendableStatus => isInAmendableStatusLazy.Value;

	public bool IsAmending => EntryStatus == ITEntryStatusList.Codes.Amending;

	public bool IsFailedSubmissionAttempt => isFailedSubmissionAttemptLazy.Value;

	#region Implementation

	bool GetIsInAmendableStatus()
	{
		if (!IsUcc6 || IsI2Declaration())
		{
			return false;
		}

		if (entryHeader.HasMrn)
		{
			return AreEntryAndMessageInAmendableStatus();
		}

		return entryHeader.CH_Status.IsEmpty && !entryHeader.Declaration.IsPluggedIntoShipment;
	}

	bool IsUcc6 => entryHeader.Declaration?.IsUCC6 ?? false;

	bool IsI2Declaration()
	{
		var style = entryHeader.EntryInstruction?.CEI_Style ?? ZString.Empty;
		return style == ImportUCC6DeclarationTypeList.Codes.NotificaDiPresentazioneInDoganaDelleMerciI2;
	}

	ZString EntryStatus => entryHeader.CH_EntryStatus;

	bool AreEntryAndMessageInAmendableStatus()
	{
		switch (EntryStatus)
		{
			case ITEntryStatusList.Codes.Registered:
			case ITEntryStatusList.Codes.ExportCleared:
			case ITEntryStatusList.Codes.ImportCleared:
			case ITEntryStatusList.Codes.UnderControl:
			case ITEntryStatusList.Codes.Deposited:
			case ITEntryStatusList.Codes.Amended:
			case ITEntryStatusList.Codes.Canceling when IsFailedSubmissionAttempt:
			case ITEntryStatusList.Codes.Exit:
				return true;

			default:
				return false;
		}
	}

	bool GetIsFailedSubmissionAttempt()
	{
		var status = entryHeader.CH_Status;
		return status == Common.Shared.MessageStatusList.Codes.ErrorOriginal
			|| status == Common.EU.MessageStatusList.Codes.FailedFromTransmission;
	}

	#endregion
}
