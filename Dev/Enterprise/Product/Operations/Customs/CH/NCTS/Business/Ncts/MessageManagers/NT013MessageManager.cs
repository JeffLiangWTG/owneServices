using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NT013MessageManager : BasePassarDepartureMessageManager<NctsHeaderDepartureMessageSendingObject>
{
	public NT013MessageManager(NctsHeaderDepartureMessageSendingObject messageSender) : base(messageSender)
	{
	}

	public override string MessageFriendlyName => PassarMessageTypeList.Descriptions.NT013;

	protected override string MovementPhaseAfterSending => DeparturePhaseList.Codes.Amendment;

	protected override Event EventTypeAfterSending => Events.DeclarationAmendmentSent;

	protected override void BeforeGenerateMessage(NctsHeaderDepartureMessageSendingObject sendingObject)
	{
		base.BeforeGenerateMessage(sendingObject);
		NctsMovementHeaderValuationDateHelper.SetValuationDate(sendingObject.MovementHeader, true);
	}
}
