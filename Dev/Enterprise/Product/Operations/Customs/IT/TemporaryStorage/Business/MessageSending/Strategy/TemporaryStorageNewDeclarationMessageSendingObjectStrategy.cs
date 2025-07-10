using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

sealed class TemporaryStorageNewDeclarationMessageSendingObjectStrategy : TemporaryStorageMessageSendingObjectStrategy
{
	public TemporaryStorageNewDeclarationMessageSendingObjectStrategy(TemporaryStorageMessageSendingObject messageSendingObject) : base(messageSendingObject)
	{
	}

	protected override bool DoesMessageTypeAllowSending()
	{
		var isCustomsStatusEmptyMessageEligibleForSending = Header.CustomsStatus.IsEmpty
				&& (Header.AMA_MessageStatus.IsEmpty
				|| Header.AMA_MessageStatus == PNTSMessageStatusList.Codes.TechnicalFailure
				|| Header.AMA_MessageStatus == PNTSMessageStatusList.Codes.FunctionalRejection);

		var isCustomsStatusTPAMessageEligibleForSending = Header.CustomsStatus == PNTSCustomsStatusList.Codes.PartialActivated
				&& (Header.AMA_MessageStatus == PNTSMessageStatusList.Codes.Acknowledged
				|| Header.AMA_MessageStatus == PNTSMessageStatusList.Codes.TechnicalFailure
				|| Header.AMA_MessageStatus == PNTSMessageStatusList.Codes.FunctionalRejection);

		return isCustomsStatusEmptyMessageEligibleForSending
				|| isCustomsStatusTPAMessageEligibleForSending;
	}
}
