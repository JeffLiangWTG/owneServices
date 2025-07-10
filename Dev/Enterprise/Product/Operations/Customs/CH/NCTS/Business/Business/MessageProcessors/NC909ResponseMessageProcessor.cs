using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using Enterprise.BatchProcessor;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NC909ResponseMessageProcessor : CH.Business.NC909ResponseMessageProcessor
{
	public NC909ResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override bool ProcessResponseMessageCore(CHEDIMessage message, INC909ResponseDetail customsResponse)
	{
		var result = base.ProcessResponseMessageCore(message, customsResponse);
		if (!result && NctsHeader.GetLinkedNctsHeader(message.EM_LinkedObject) is NctsHeader nctsHeader)
		{
			UpdateMovementStatus(nctsHeader);
			result = true;
		}
		return result;
	}

	void UpdateMovementStatus(NctsHeader header)
	{
		switch (header.CommonMovementHeader.BM_Phase)
		{
			case DeparturePhaseList.Codes.Amendment:
				header.Logs.AddNew(Events.DeclarationAmendmentRejected);
				break;
			case DeparturePhaseList.Codes.Activation:
				header.Logs.AddNew(Events.DeclarationActivationRejected);
				break;
			case DeparturePhaseList.Codes.Cancellation:
				header.Logs.AddNew(Events.DeclarationCancellationRejected);
				break;
			case DeparturePhaseList.Codes.NonArrivedMovement:
				break;
			default:
				header.Logs.AddNew(Events.DeclarationRejected);
				break;
		}
		header.EffectiveMessageStatus = CHLogicalStatusList.Codes.Invalid;
	}
}
