using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NT141MessageManager : BasePassarDepartureMessageManager<NctsHeaderDepartureMessageSendingObject>
{
	public NT141MessageManager(NctsHeaderDepartureMessageSendingObject messageSender) : base(messageSender)
	{
	}

	public override string MessageFriendlyName => PassarMessageTypeList.Descriptions.NT141;

	protected override string MovementPhaseAfterSending => DeparturePhaseList.Codes.NonArrivedMovement;

	protected override Event EventTypeAfterSending => Events.DeclarationSentToCustoms;

	protected override string EventReferenceAfterSending => PassarMessageTypeList.Codes.NT141;
}
