using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

sealed class TemporaryStorageAmendmentMessageSendingObjectStrategy : TemporaryStorageMessageSendingObjectStrategy
{
	public TemporaryStorageAmendmentMessageSendingObjectStrategy(TemporaryStorageMessageSendingObject messageSendingObject) : base(messageSendingObject)
	{
	}

	protected override bool DoesMessageTypeAllowSending()
	{
		return Header.CustomsStatus == ITEntryStatusList.Codes.Amending
				&& (Header.AMA_MessageStatus.IsEmpty
				|| Header.AMA_MessageStatus == PNTSMessageStatusList.Codes.TechnicalFailure
				|| Header.AMA_MessageStatus == PNTSMessageStatusList.Codes.FunctionalRejection);
	}
}
