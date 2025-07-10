using CargoWise.EntityFramework;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public abstract class BasePassarDepartureMessageManager<TMessageSendingObject> : BasePassarMessageManager<TMessageSendingObject>
	where TMessageSendingObject : BusinessObject, IMessageSendingObjectParent, IMessageSendingObject, INctsMessageSendingObject
{
	protected BasePassarDepartureMessageManager(TMessageSendingObject messageSender) : base(messageSender)
	{
	}

	protected override void BeforeGenerateMessage(TMessageSendingObject sendingObject)
	{
		NctsHeaderDeclarationGoodsItemNumbersHelper.AssignUnassignedDeclarationGoodsItemNumbers(sendingObject.NctsHeader);
		base.BeforeGenerateMessage(sendingObject);
	}
}
