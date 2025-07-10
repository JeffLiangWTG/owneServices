using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

sealed class NctsDepartureSafeCustomsStatusSetter : ICustomsStatusSetter
{
	public NctsDepartureSafeCustomsStatusSetter(NctsDepartureMovementHeader movementHeader)
	{
		this.movementHeader = Argument.NotNull(movementHeader, nameof(movementHeader));
	}

	bool ICustomsStatusSetter.TrySetAcceptedBySystem()
		=> TrySetCustomsStatusAndAcceptMessageIfSuccessful(NCTS5DepartureCustomsStatusList.Codes.AcceptedBySystem);

	bool ICustomsStatusSetter.TrySetAcknowledged()
		=> TrySetCustomsStatusAndAcceptMessageIfSuccessful(NCTS5DepartureCustomsStatusList.Codes.Acknowledged);

	bool ICustomsStatusSetter.TrySetAmended()
	{
		var customsStatusAnalyzer = new NctsDeparturePhase5CustomsStatusAnalyzer(movementHeader);
		if (customsStatusAnalyzer.TryDetermineCustomsStatusBasedOnLastMessageSent(out var customsStatus))
		{
			movementHeader.BM_CustomsStatus = customsStatus;
			movementHeader.BM_MessageStatus = LogicalStatusList.Codes.Accepted;
			return true;
		}

		return false;
	}

	bool ICustomsStatusSetter.TrySetCancelled()
		=> TrySetCustomsStatusAndAcceptMessageIfSuccessful(NCTS5DepartureCustomsStatusList.Codes.Cancelled);

	bool ICustomsStatusSetter.TrySetCleared(ZDateTime clearanceDateTime)
		=> TrySetCustomsStatusAndAcceptMessageIfEligibleAndSuccessful(NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit);

	bool ICustomsStatusSetter.TrySetDeposited() => false;

	bool ICustomsStatusSetter.TrySetErrorOriginal()
	{
		movementHeader.BM_CustomsStatus = ZString.Empty;
		movementHeader.BM_MessageStatus = LogicalStatusList.Codes.Error;
		return true;
	}

	bool ICustomsStatusSetter.TrySetExitCompleted() => false;

	bool ICustomsStatusSetter.TrySetRegistered()
		=> TrySetCustomsStatusAndAcceptMessageIfEligibleAndSuccessful(NCTS5DepartureCustomsStatusList.Codes.MrnAllocated);

	bool ICustomsStatusSetter.TrySetUnderControl()
		=> TrySetCustomsStatusAndAcceptMessageIfEligibleAndSuccessful(NCTS5DepartureCustomsStatusList.Codes.IntentionToControl);

	bool ICustomsStatusSetter.TrySetGoodsWrittenOffClosed()
		=> TrySetCustomsStatusAndAcceptMessageIfEligibleAndSuccessful(NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed);

	public static int LookupCustomsStatusValue(ZString customsStatus)
		=> customsStatusCollectionWithOrder.GetValueOrDefault(customsStatus, CustomsStatusValueInvalid);

	public static IEnumerable<string> GetCustomsStatusCodesForValue(int statusValue)
	{
		return customsStatusCollectionWithOrder
			.Where(x => x.Value == statusValue)
			.Select(x => x.Key);
	}

	#region Implementation

	bool TryChangeCustomsStatusSafe(string targetCustomsStatus)
	{
		if (!IsChangeAllowed(targetCustomsStatus, movementHeader.BM_CustomsStatus, customsStatusCollectionWithOrder))
		{
			return false;
		}

		movementHeader.BM_CustomsStatus = targetCustomsStatus;

		return true;
	}

	bool IsChangeAllowed(string statusBeingSet, ZString currentStatus, ImmutableDictionary<string, int> statusCollectionWithOrder)
	{
		if (currentStatus.IsEmpty || !statusCollectionWithOrder.TryGetValue(currentStatus, out var currentStatusOrder))
		{
			return true;
		}

		_ = statusCollectionWithOrder.TryGetValue(statusBeingSet, out var targetCustomsStatusOrder);
		return currentStatusOrder < targetCustomsStatusOrder;
	}

	bool TrySetCustomsStatusAndAcceptMessageIfSuccessful(string customsStatus)
	{
		if (TryChangeCustomsStatusSafe(customsStatus))
		{
			SetMessageStatusAsAccepted();
			return true;
		}
		return false;
	}

	bool TrySetCustomsStatusAndAcceptMessageIfEligibleAndSuccessful(string customsStatus)
		=> !IsInCancellationOrAmendment() && TrySetCustomsStatusAndAcceptMessageIfSuccessful(customsStatus);

	void SetMessageStatusAsAccepted()
		=> movementHeader.BM_MessageStatus = LogicalStatusList.Codes.Accepted;

	bool IsInCancellationOrAmendment()
		=> movementHeader.BM_Phase.ToString() is NCTS5DeparturePhaseList.Codes.Cancellation or NCTS5DeparturePhaseList.Codes.Amendment;

	readonly NctsDepartureMovementHeader movementHeader;

	readonly static ImmutableDictionary<string, int> customsStatusCollectionWithOrder = new Dictionary<string, int>
	{
		{ NCTS5DepartureCustomsStatusList.Codes.Acknowledged, 0 },
		{ NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, 1 },
		{ NCTS5DepartureCustomsStatusList.Codes.IntentionToControl, 2 },
		{ NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit, 3 },
		{ NCTS5DepartureCustomsStatusList.Codes.AcceptedBySystem, 4 },
		{ NCTS5DepartureCustomsStatusList.Codes.Cancelled, 5 },
		{ NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested, 5 },
		{ NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed, 6 },
	}.ToImmutableDictionary();

	public static readonly int CustomsStatusValueInvalid = -1;

	#endregion
}
