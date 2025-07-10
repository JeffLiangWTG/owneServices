namespace Enterprise.Customs.IT.TemporaryStorage.Business;

interface ITemporaryStorageMessageSendingObjectStrategy
{
	public bool DoesStatusAllowSending();
}
