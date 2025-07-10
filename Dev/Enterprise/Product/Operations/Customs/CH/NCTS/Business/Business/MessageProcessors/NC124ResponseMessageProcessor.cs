using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using Enterprise.BatchProcessor;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NC124ResponseMessageProcessor : CH.Business.NC124ResponseMessageProcessor
{
	public NC124ResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override bool ProcessResponseMessageCore(CHEDIMessage message, INC124ResponseDetail customsResponse)
	{
		var result = base.ProcessResponseMessageCore(message, customsResponse);
		if (!result && NctsHeader.GetLinkedNctsHeader(message.EM_LinkedObject) is NctsHeader nctsHeader)
		{
			var movementHeader = nctsHeader.MovementHeader;

			if (customsResponse.IsAccepted)
			{
				var decisionDateAndTimeCH = customsResponse.DecisionDateAndTime.UtcToLocalBranchTime();

				nctsHeader.EffectiveMessageStatus = CHLogicalStatusList.Codes.Accepted;
				movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.CHActive;
				movementHeader.BM_Phase = DeparturePhaseList.Codes.Activation;
				movementHeader.BM_EntryDate = decisionDateAndTimeCH;
			}
			else if (customsResponse.IsRejected)
			{
				nctsHeader.EffectiveMessageStatus = CHLogicalStatusList.Codes.Invalid;
				movementHeader.BM_Phase = DeparturePhaseList.Codes.Declaration;
				nctsHeader.Logs.AddNew(Events.DeclarationActivationRejected);
			}
			else
			{
				nctsHeader.EffectiveMessageStatus = CHLogicalStatusList.Codes.Invalid;
			}
			result = true;
		}
		return result;
	}
}
