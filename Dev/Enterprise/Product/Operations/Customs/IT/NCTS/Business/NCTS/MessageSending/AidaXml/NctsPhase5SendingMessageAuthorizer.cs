using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

sealed class NctsPhase5SendingMessageAuthorizer
{
	public NctsPhase5SendingMessageAuthorizer(NctsHeader header)
	{
		this.header = Argument.NotNull(header, nameof(header));
		movementHeader = Argument.NotNull(header.MovementHeader, nameof(header.MovementHeader));
	}

	public IEnumerable<string> GetAllowedSendingMessageTypes()
	{
		if (!movementHeader.IsInAmendmentPhase)
		{
			yield return EDIMessageTypeList.Codes.NewDeclaration;
		}

		if (IsCancellationAllowed())
		{
			yield return EDIMessageTypeList.Codes.Cancellation;
		}

		if (IsAmendmentAllowed())
		{
			yield return EDIMessageTypeList.Codes.Amendment;
		}
	}

	bool IsCancellationAllowed()
	{
		if (header.MovementReferenceNumber.IsEmpty)
		{
			return false;
		}

			var customsStatus = movementHeader.BM_CustomsStatus;
			var effectiveMessageStatus = header.EffectiveMessageStatus;
			var phase = movementHeader.BM_Phase;

			var statusString = $"{customsStatus}{effectiveMessageStatus}{phase}";
			var statusStringWildcard1 = $"{"*"}{effectiveMessageStatus}{phase}";
			var statusStringWildcard2 = $"{customsStatus}{"*"}{phase}";
			var isInCombinationsThatAllowCancellation = statusString.In(combinationsThatAllowCancellation) || statusStringWildcard1.In(combinationsThatAllowCancellation) || statusStringWildcard2.In(combinationsThatAllowCancellation);

			return isInCombinationsThatAllowCancellation && !header.MessageHasBeenSent;
		}

	bool IsAmendmentAllowed()
	{
		return movementHeader.IsInAmendmentPhase
			&& StatusAllowAmendment();

		bool StatusAllowAmendment()
		{
			var customsStatus = movementHeader.BM_CustomsStatus;
			var messageStatus = movementHeader.BM_MessageStatus;

			return (customsStatus.IsEmpty && messageStatus.IsEmpty)
				|| messageStatus.ToString() is LogicalStatusList.Codes.Error or LogicalStatusList.Codes.Failed;
		}
	}

	readonly NctsHeader header;
	readonly NctsDepartureMovementHeader movementHeader;

	readonly ImmutableArray<string> combinationsThatAllowCancellation = new string[]
	{
		$"{NCTS5DepartureCustomsStatusList.Codes.MrnAllocated}{LogicalStatusList.Codes.Accepted}{NCTS5DeparturePhaseList.Codes.Declaration}",
		$"{NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit}{LogicalStatusList.Codes.Accepted}{NCTS5DeparturePhaseList.Codes.Declaration}",
		$"{NCTS5DepartureCustomsStatusList.Codes.NotReleasedForTransit}{LogicalStatusList.Codes.Accepted}{NCTS5DeparturePhaseList.Codes.Declaration}",
		$"{NCTS5DepartureCustomsStatusList.Codes.IntentionToControl}{"*"}{NCTS5DeparturePhaseList.Codes.Declaration}",
		$"{ITEntryStatusList.Codes.Deposited}{"*"}{NCTS5DeparturePhaseList.Codes.Declaration}",
		$"{NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed}{LogicalStatusList.Codes.Accepted}{NCTS5DeparturePhaseList.Codes.Declaration}",
		$"{"*"}{LogicalStatusList.Codes.Failed}{NCTS5DeparturePhaseList.Codes.Cancellation}",
		$"{"*"}{LogicalStatusList.Codes.Error}{NCTS5DeparturePhaseList.Codes.Cancellation}",
		$"{""}{""}{NCTS5DeparturePhaseList.Codes.Amendment}",
		$"{"*"}{LogicalStatusList.Codes.Failed}{NCTS5DeparturePhaseList.Codes.Amendment}",
		$"{"*"}{LogicalStatusList.Codes.Error}{NCTS5DeparturePhaseList.Codes.Amendment}",
		$"{NCTS5DepartureCustomsStatusList.Codes.AcceptedBySystem}{LogicalStatusList.Codes.Accepted}{NCTS5DeparturePhaseList.Codes.Amendment}",
	}.ToImmutableArray();
}
