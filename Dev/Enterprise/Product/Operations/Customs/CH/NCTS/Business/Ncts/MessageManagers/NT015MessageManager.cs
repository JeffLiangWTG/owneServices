using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NT015MessageManager : BasePassarDepartureMessageManager<NctsHeaderDepartureMessageSendingObject>
{
	public NT015MessageManager(NctsHeaderDepartureMessageSendingObject messageSender) : base(messageSender)
	{
	}

	public override string MessageFriendlyName => PassarMessageTypeList.Descriptions.NT015;

	protected override string MovementPhaseAfterSending => DeparturePhaseList.Codes.Declaration;

	protected override Event EventTypeAfterSending => Events.CustomsCommenced;

	protected override void BeforeGenerateMessage(NctsHeaderDepartureMessageSendingObject sendingObject)
	{
		base.BeforeGenerateMessage(sendingObject);
		NctsMovementHeaderValuationDateHelper.SetValuationDate(sendingObject.MovementHeader, false);
	}
}
