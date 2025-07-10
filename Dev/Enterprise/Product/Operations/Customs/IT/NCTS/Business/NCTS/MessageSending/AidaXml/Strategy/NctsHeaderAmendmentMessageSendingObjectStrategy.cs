using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

sealed class NctsHeaderAmendmentMessageSendingObjectStrategy : NctsHeaderMessageSendingObjectStrategy
{
	public NctsHeaderAmendmentMessageSendingObjectStrategy(NctsHeaderMessageSendingObject messageSendingObject) : base(messageSendingObject)
	{
		AddValidMessageStateForSending(NctsMovementHeaderTransactionStatusList.Codes.Amendment, LogicalStatusList.Codes.Failed, null);
		AddValidMessageStateForSending(NctsMovementHeaderTransactionStatusList.Codes.Amendment, LogicalStatusList.Codes.Error, null);
	}
}
