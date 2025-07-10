using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NT057ResponseMessageProcessor : BasePassarNctsMSGMessageProcessor<INT057ResponseDetail>
{
	public NT057ResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => (NoResString)"NT057 - Unloading Remarks Response Processor";

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.PassarUnloadingRemarksResponse };

	protected override string MovementType => NctsMovementType.Codes.Arrival;

	protected override void ProcessResponseMessageCore(CHEDIMessage message, INT057ResponseDetail customsResponse, NctsHeader nctsHeader)
	{
		if (customsResponse.IsAccepted)
		{
			nctsHeader.EffectiveMessageStatus = CHLogicalStatusList.Codes.Accepted;
			nctsHeader.Logs.AddNew(Events.MessageStatusChange, CHLogicalStatusList.Codes.Accepted);
			UpdateMasterMovement(nctsHeader.ArrivalMovementHeader);
		}
		else if (customsResponse.IsRejected)
		{
			nctsHeader.EffectiveMessageStatus = CHLogicalStatusList.Codes.Invalid;
			nctsHeader.Logs.AddNew(Events.DeclarationRejected);
		}
	}

	void UpdateMasterMovement(NctsArrivalMovementHeader movementHeader)
	{
		var masterMovement = movementHeader.MasterArrivalMovementHeader;
		if (masterMovement != null && masterMovement.RelatedArrivalMovements.Cast<RelatedArrivalMovementGenPivot>().All(x => IsNT057Accepted(x.ChildMovement)))
		{
			masterMovement.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks;
			masterMovement.BM_Phase = NCTS5ArrivalPhaseList.Codes.UnloadingRemarks;
		}
	}

	bool IsNT057Accepted(NctsArrivalMovementHeader movementHeader)
	{
		switch (movementHeader.BM_CustomsStatus)
		{
			case NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks:
				return movementHeader.Header.EffectiveMessageStatus == CHLogicalStatusList.Codes.Accepted;
			case NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease:
				return true;
		}
		return false;
	}
}
