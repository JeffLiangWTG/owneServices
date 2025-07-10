using CargoWise.Common;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

abstract class TemporaryStorageMessageSendingObjectStrategy : ITemporaryStorageMessageSendingObjectStrategy
{
	protected TemporaryStorageMessageSendingObjectStrategy(TemporaryStorageMessageSendingObject messageSendingObject)
	{
		this.messageSendingObject = Argument.NotNull(messageSendingObject, nameof(messageSendingObject));
	}

	public bool DoesStatusAllowSending() => DoesMessageTypeAllowSending();

	protected virtual bool DoesMessageTypeAllowSending() => true;

	protected TemporaryStorageHeader Header => messageSendingObject.Header;

	readonly TemporaryStorageMessageSendingObject messageSendingObject;
}
