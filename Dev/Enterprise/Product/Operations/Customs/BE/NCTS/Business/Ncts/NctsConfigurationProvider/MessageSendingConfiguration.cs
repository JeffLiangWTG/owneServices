using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business;

sealed class MessageSendingConfiguration : EU.NCTS.Business.MessageSendingConfiguration
{
	protected override INctsHeaderMessageSendingObjectValidationDecider GetValidationDeciderCore()
	{
		return new NctsHeaderMessageSendingObjectValidationDecider();
	}
}
