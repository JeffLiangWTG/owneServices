namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

sealed class NctsHeaderCancellationMessageSendingObjectStrategy : NctsHeaderMessageSendingObjectStrategy
{
	public NctsHeaderCancellationMessageSendingObjectStrategy(NctsHeaderMessageSendingObject messageSendingObject) : base(messageSendingObject)
	{
	}

	protected override bool DoesMessageTypeAllowSending()
	{
		return true;
	}
}
