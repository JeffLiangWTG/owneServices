using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NT014MessageManager : BasePassarDepartureMessageManager<NctsHeaderDepartureMessageSendingObject>
{
	public NT014MessageManager(NctsHeaderDepartureMessageSendingObject messageSender) : base(messageSender)
	{
	}

	public override string MessageFriendlyName => PassarMessageTypeList.Descriptions.NT014;

	protected override string MovementPhaseAfterSending => DeparturePhaseList.Codes.Cancellation;

	protected override Event EventTypeAfterSending => Events.DeclarationCancellationSent;
}
