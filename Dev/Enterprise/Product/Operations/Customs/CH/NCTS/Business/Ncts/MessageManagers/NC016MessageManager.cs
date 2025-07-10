using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NC016MessageManager : BasePassarMessageManager<NctsHeaderCommonMessageSendingObject>
{
	public NC016MessageManager(NctsHeaderCommonMessageSendingObject messageSender) : base(messageSender)
	{
	}

	public override string MessageFriendlyName => PassarMessageTypeList.Descriptions.NC016;

	protected override string MovementPhaseAfterSending => MessageSubTypeCodeList.Codes.PassarRequestDataJourney;

	protected override Event EventTypeAfterSending => Events.DeclarationSentToCustoms;

	protected override string EventReferenceAfterSending => PassarMessageTypeList.Codes.NC016;
}
