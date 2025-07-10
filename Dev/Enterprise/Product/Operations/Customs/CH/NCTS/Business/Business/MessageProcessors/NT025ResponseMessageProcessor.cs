using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.NCTS.Business;

internal class NT025ResponseMessageProcessor : BasePassarNctsMSGMessageProcessor<INT025ResponseDetail>
{
	public NT025ResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => (NoResString)"NT025 - Arrival Indication Message Processor";

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.PassarArrivalIndication };

	protected override string MovementType => NctsMovementType.Codes.Arrival;

	protected override bool FindLinkedObjectByMRNAnyVersion => true;

	protected override bool ShouldUpdateMRNVersion => false;

	protected override void ProcessResponseMessageCore(CHEDIMessage message, INT025ResponseDetail customsResponse, NctsHeader header)
	{
		if (customsResponse.IsFullRelease)
		{
			header.ArrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
			header.MovementReferenceEntryNumber.CE_IssueDate = customsResponse.ReleaseDate;
			UpdateMasterMovement(header.ArrivalMovementHeader, customsResponse);
		}
		else if (customsResponse.IsPartialRelease)
		{
			header.ArrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.ClosedPartialRelease;
		}
	}

	void UpdateMasterMovement(NctsArrivalMovementHeader movementHeader, INT025ResponseDetail customsResponse)
	{
		var masterMovement = movementHeader.MasterArrivalMovementHeader;
		if (masterMovement != null && masterMovement.RelatedArrivalMovements.Cast<RelatedArrivalMovementGenPivot>().All(x => IsNT025Received(x.ChildMovement)))
		{
			masterMovement.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
			masterMovement.Header.ArrivalReferenceEntryNumber.CE_ExpiryDate = customsResponse.ReleaseDate;
		}
	}

	bool IsNT025Received(NctsArrivalMovementHeader movementHeader) => movementHeader.BM_CustomsStatus == NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
}
