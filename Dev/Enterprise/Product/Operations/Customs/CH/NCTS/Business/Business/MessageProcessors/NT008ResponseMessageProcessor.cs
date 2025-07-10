using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NT008ResponseMessageProcessor : BasePassarNctsMSGMessageProcessor<INT008ResponseDetail>
{
	public NT008ResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => (NoResString)"NT008 - Arrival Response Message Processor";

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.PassarArrivalResponse };

	protected override string MovementType => NctsMovementType.Codes.Arrival;

	protected override void ProcessResponseMessageCore(CHEDIMessage message, INT008ResponseDetail customsResponse, NctsHeader header)
	{
		if (customsResponse.IsAccepted)
		{
			var decisionDateAndTimeCH = customsResponse.DecisionDateAndTime.UtcToLocalBranchTime();
			header.EffectiveMessageStatus = CHLogicalStatusList.Codes.Accepted;
			var arrivalMovementHeader = header.ArrivalMovementHeader;
			if (!IsNT061Received(arrivalMovementHeader))
			{
				arrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.CHActive;
			}
			arrivalMovementHeader.BM_EntryDate = decisionDateAndTimeCH;
			var arrivalReferenceEntryNumber = header.ArrivalReferenceEntryNumber;
			arrivalReferenceEntryNumber.CE_EntryNum = customsResponse.ArrivalReferenceNumber;
			arrivalReferenceEntryNumber.CE_IssueDate = decisionDateAndTimeCH;
		}
		else if (customsResponse.IsReceived)
		{
			header.EffectiveMessageStatus = CHLogicalStatusList.Codes.Accepted;
			header.Logs.AddNew(Events.DeclarationQueued);
		}
		else if (customsResponse.IsRejected)
		{
			header.Logs.AddNew(Events.DeclarationRejected);
			header.EffectiveMessageStatus = CHLogicalStatusList.Codes.Invalid;
		}
		else
		{
			header.EffectiveMessageStatus = CHLogicalStatusList.Codes.Invalid;
		}
	}

	static bool IsNT061Received(NctsArrivalMovementHeader movementHeader) => movementHeader.BM_CustomsStatus == NCTS5ArrivalCustomsStatusList.Codes.CHClear || movementHeader.BM_CustomsStatus == NCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionNoRelease;
}
