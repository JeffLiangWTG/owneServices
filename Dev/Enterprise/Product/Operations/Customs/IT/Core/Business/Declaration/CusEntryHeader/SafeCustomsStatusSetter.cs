using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

sealed class SafeCustomsStatusSetter : ICustomsStatusSetter
{
	public SafeCustomsStatusSetter(CusEntryHeader entryHeader)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
	}

	bool ICustomsStatusSetter.TrySetDeposited() => TryChangeEntryStatusSafe(ITEntryStatusList.Codes.Deposited);

	bool ICustomsStatusSetter.TrySetRegistered() => TryChangeEntryStatusSafe(ITEntryStatusList.Codes.Registered);

	bool ICustomsStatusSetter.TrySetUnderControl() => TryChangeEntryStatusSafe(ITEntryStatusList.Codes.UnderControl);

	bool ICustomsStatusSetter.TrySetCleared(ZDateTime clearanceDateTime)
	{
		var clearanceStatus = entryHeader.IsImport ? ITEntryStatusList.Codes.ImportCleared : ITEntryStatusList.Codes.ExportCleared;
		var statusChanged = TryChangeEntryStatusSafe(clearanceStatus);
		if (statusChanged)
		{
			SetCleared();
		}
		entryHeader.CH_EntryReleaseDate = clearanceDateTime;

		return statusChanged;
	}

	bool ICustomsStatusSetter.TrySetExitCompleted() => TryChangeEntryStatusSafe(ITEntryStatusList.Codes.Exit);

	bool ICustomsStatusSetter.TrySetCancelled() => TryChangeEntryStatusSafe(ITEntryStatusList.Codes.Canceled);

	bool ICustomsStatusSetter.TrySetAmended() => TryChangeEntryStatusSafe(ITEntryStatusList.Codes.Amended);

	bool ICustomsStatusSetter.TrySetErrorOriginal() => TryChangeMessageStatusSafe(ITMessageStatusList.Codes.ErrorOriginal);

	bool ICustomsStatusSetter.TrySetAcknowledged() => TryChangeMessageStatusSafe(ITMessageStatusList.Codes.AcknowledgedOriginal);

	bool ICustomsStatusSetter.TrySetAcceptedBySystem() => TryChangeMessageStatusSafe(ITMessageStatusList.Codes.AcceptedBySystem);

	bool ICustomsStatusSetter.TrySetGoodsWrittenOffClosed() => false;

	#region Implementation

	bool TryChangeEntryStatusSafe(string targetCustomsStatus)
	{
		if (!IsChangeAllowed(targetCustomsStatus, entryHeader.CH_EntryStatus, customsStatusCollectionWithOrder))
		{
			return false;
		}

		(this as ICustomsStatusSetter).TrySetAcknowledged();
		entryHeader.CH_EntryStatus = targetCustomsStatus;

		return true;
	}

	bool TryChangeMessageStatusSafe(string targetMessageStatus)
	{
		if (!IsChangeAllowed(targetMessageStatus, entryHeader.CH_Status, messageStatusCollectionWithOrder))
		{
			return false;
		}

		entryHeader.CH_Status = targetMessageStatus;
		return true;
	}

	bool IsChangeAllowed(string statusBeingSet, ZString currentStatus, ImmutableDictionary<string, int> statusCollectionWithOrder)
	{
		if (currentStatus.IsEmpty || !statusCollectionWithOrder.TryGetValue(currentStatus, out var currentStatusOrder))
		{
			return true;
		}

		statusCollectionWithOrder.TryGetValue(statusBeingSet, out var targetCustomsStatusOrder);
		return currentStatusOrder < targetCustomsStatusOrder;
	}

	void SetCleared() => TryChangeMessageStatusSafe(ITMessageStatusList.Codes.ClearOriginal);

	readonly CusEntryHeader entryHeader;

	readonly ImmutableDictionary<string, int> customsStatusCollectionWithOrder = new Dictionary<string, int>
	{
		{ ITEntryStatusList.Codes.Deposited, 0 },
		{ ITEntryStatusList.Codes.Registered, 1 },
		{ ITEntryStatusList.Codes.UnderControl, 2 },
		{ ITEntryStatusList.Codes.ImportCleared, 3 },
		{ ITEntryStatusList.Codes.ExportCleared, 3 },
		{ ITEntryStatusList.Codes.Exit, 4 },
		{ ITEntryStatusList.Codes.Amending, 5 },
		{ ITEntryStatusList.Codes.Canceling, 5 },
		{ ITEntryStatusList.Codes.Amended, 6 },
		{ ITEntryStatusList.Codes.Canceled, 6 },
	}.ToImmutableDictionary();

	readonly ImmutableDictionary<string, int> messageStatusCollectionWithOrder = new Dictionary<string, int>
	{
		{ ITMessageStatusList.Codes.AwaitingOriginal, 0 },
		{ ITMessageStatusList.Codes.AcknowledgedOriginal, 1 },
		{ ITMessageStatusList.Codes.ClearOriginal, 2 },
		{ ITMessageStatusList.Codes.AcceptedBySystem, 3 },
		{ ITMessageStatusList.Codes.ErrorOriginal, 4 },
		{ ITMessageStatusList.Codes.FailedFromTransmission, 4 },
	}.ToImmutableDictionary();

	#endregion
}
