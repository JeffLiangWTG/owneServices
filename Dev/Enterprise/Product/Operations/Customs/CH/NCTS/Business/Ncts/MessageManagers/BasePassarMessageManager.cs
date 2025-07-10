using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public abstract class BasePassarMessageManager<TMessageSendingObject> : BaseMessageManager<TMessageSendingObject>
	where TMessageSendingObject : BusinessObject, IMessageSendingObjectParent, IMessageSendingObject, INctsMessageSendingObject
{
	public BasePassarMessageManager(TMessageSendingObject messageSender) : base(messageSender)
	{
	}

	ZString lastNctsHeaderMessageStatus;
	ZString lastMovementHeaderMessageStatus;

	protected virtual string MessageStatusAfterSending => NctsMessageStatusList.Codes.SentToCustoms;
	protected abstract string MovementPhaseAfterSending { get; }
	protected abstract Event EventTypeAfterSending { get; }
	protected virtual string EventReferenceAfterSending => ZString.Empty;

	protected override void AfterGenerateMessage(TMessageSendingObject sendingObject, EDIMessage message)
	{
		var nctsHeader = sendingObject.NctsHeader;
		var parentCollection = nctsHeader.IsDepartureMovement ? nctsHeader.MovementHeader.Messages : nctsHeader.Messages;
		parentCollection.Add(message);

		lastNctsHeaderMessageStatus = nctsHeader.EffectiveMessageStatus;
		nctsHeader.EffectiveMessageStatus = MessageStatusAfterSending;

		lastMovementHeaderMessageStatus = nctsHeader.CommonMovementHeader.BM_Phase;
		nctsHeader.CommonMovementHeader.BM_Phase = MovementPhaseAfterSending;

		if (EventTypeAfterSending != null)
		{
			sendingObject.NctsHeader.Logs.AddNew(EventTypeAfterSending, EventReferenceAfterSending);
		}
	}

	public override void RollbackOnSaveFailed()
	{
		var nctsHeader = SendingObject.NctsHeader;

		nctsHeader.EffectiveMessageStatus = lastNctsHeaderMessageStatus;
		nctsHeader.CommonMovementHeader.BM_Phase = lastMovementHeaderMessageStatus;

		nctsHeader.Logs.LogsNotInDB.DeleteAll();
	}
}
