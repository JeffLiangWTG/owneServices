using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

sealed class NctsHeaderNewDeclarationMessageSendingObjectStrategy : NctsHeaderMessageSendingObjectStrategy
{
	public NctsHeaderNewDeclarationMessageSendingObjectStrategy(NctsHeaderMessageSendingObject messageSendingObject) : base(messageSendingObject)
	{
		AddValidMessageStateForSending(NctsMovementHeaderTransactionStatusList.Codes.Declaration, LogicalStatusList.Codes.Failed, null);
		AddValidMessageStateForSending(NctsMovementHeaderTransactionStatusList.Codes.Declaration, LogicalStatusList.Codes.Error, null);
	}
}
