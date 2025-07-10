using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NT044MessageManager : BasePassarMessageManager<NctsHeaderArrivalMessageSendingObject>
{
	public NT044MessageManager(NctsHeaderArrivalMessageSendingObject messageSender) : base(messageSender)
	{
	}

	ZString lastNctsHeaderCustomsStatus;

	public override string MessageFriendlyName => PassarMessageTypeList.Descriptions.NT044;

	protected override string MovementPhaseAfterSending => NCTS5ArrivalPhaseList.Codes.UnloadingRemarks;

	protected override Event EventTypeAfterSending => Events.DeclarationSentToCustoms;

	protected override string EventReferenceAfterSending => nameof(PassarMessageTypeList.Codes.NT044);

	protected override void BeforeGenerateMessage(NctsHeaderArrivalMessageSendingObject sendingObject)
	{
		NctsHeaderDeclarationGoodsItemNumbersHelper.AssignUnassignedDeclarationGoodsItemNumbers(sendingObject.NctsHeader, filter: goodsItem => goodsItem.BY_UnloadedState == NctsUnloadedStateList.Codes.NEW);
		base.BeforeGenerateMessage(sendingObject);
	}

	protected override void AfterGenerateMessage(NctsHeaderArrivalMessageSendingObject sendingObject, EDIMessage message)
	{
		base.AfterGenerateMessage(sendingObject, message);
		var nctsHeader = sendingObject.NctsHeader;
		lastNctsHeaderCustomsStatus = nctsHeader.CommonMovementHeader.BM_CustomsStatus;
		nctsHeader.CommonMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks;
	}

	public override void RollbackOnSaveFailed()
	{
		base.RollbackOnSaveFailed();
		var nctsHeader = SendingObject.NctsHeader;
		nctsHeader.CommonMovementHeader.BM_CustomsStatus = lastNctsHeaderCustomsStatus;
	}
}
