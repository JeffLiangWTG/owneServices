using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NT009ResponseMessageProcessor : BasePassarNctsMSGMessageProcessor<INT009ResponseDetail>
{
	public NT009ResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => (NoResString)"NT009 - Departure Withdrawal Response Message Processor";

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.PassarDepartureWithdrawalResponse };

	protected override string MovementType => NctsMovementType.Codes.Departure;

	protected override bool ShouldFindLinkedObjectByMRN(INT009ResponseDetail customsResponse) => customsResponse.InitiatedByCustoms;

	protected override bool FindLinkedObjectByMRNAnyVersion => true;

	protected override void ProcessResponseMessageCore(CHEDIMessage message, INT009ResponseDetail customsResponse, NctsHeader header)
	{
		if (customsResponse.IsReceived)
		{
			header.EffectiveMessageStatus = CHLogicalStatusList.Codes.Accepted;
			header.Logs.AddNew(Events.DeclarationCancellationQueued);
		}
		else if (customsResponse.IsAccepted)
		{
			header.EffectiveMessageStatus = CHLogicalStatusList.Codes.Accepted;
			header.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.Cancelled;
		}
		else if (customsResponse.IsRejected)
		{
			header.EffectiveMessageStatus = CHLogicalStatusList.Codes.Invalid;
			header.MovementHeader.BM_Phase = DeparturePhaseList.Codes.Declaration;
			header.Logs.AddNew(Events.DeclarationCancellationRejected);
		}
		else
		{
			header.EffectiveMessageStatus = CHLogicalStatusList.Codes.Invalid;
		}
	}
}
