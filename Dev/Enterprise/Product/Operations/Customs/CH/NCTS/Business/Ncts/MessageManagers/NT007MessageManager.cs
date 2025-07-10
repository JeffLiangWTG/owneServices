using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NT007MessageManager : BasePassarMessageManager<NctsHeaderArrivalMessageSendingObject>
{
	public NT007MessageManager(NctsHeaderArrivalMessageSendingObject messageSender) : base(messageSender)
	{
	}

	public override string MessageFriendlyName => PassarMessageTypeList.Descriptions.NT007;

	protected override string MovementPhaseAfterSending => NCTS5ArrivalPhaseList.Codes.Arrival;

	protected override Event EventTypeAfterSending => Events.DeclarationSentToCustoms;

	protected override string EventReferenceAfterSending => nameof(PassarMessageTypeList.Codes.NT007);
}
