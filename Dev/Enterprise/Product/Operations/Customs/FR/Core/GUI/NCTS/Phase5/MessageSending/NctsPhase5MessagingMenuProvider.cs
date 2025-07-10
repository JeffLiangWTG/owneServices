using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.DataTransfer;
using Enterprise.Customs.FR.NCTS.Messaging;
using NctsHeader = Enterprise.Customs.FR.Business.NCTS.NctsHeader;

namespace Enterprise.Customs.FR.GUI.NCTS;

public class NctsPhase5MessagingMenuProvider : EU.NCTS.GUI.Phase5MessagingMenuProvider
{
	public NctsPhase5MessagingMenuProvider(NctsHeader header)
		: base(header)
	{
	}

	public NctsPhase5MessagingMenuProvider(EU.NCTS.Business.NctsHeader header, NctsMovementForm nctsMovementForm)
		: base(header)
	{
		ParentForm = nctsMovementForm;
	}

	public NctsPhase5MessagingMenuProvider(EU.NCTS.Business.NctsHeader header, NctsHeaderUniversalMessagingHelper ntcsHeaderUniversalMessagingHelper)
		: base(header, ntcsHeaderUniversalMessagingHelper)
	{
	}

	public new NctsHeader Header => (NctsHeader)base.Header;

	protected override EU.NCTS.GUI.MessageSendingForm GetMessageSendingFormCore(NctsHeaderMessageSendingObjectParent messageSendingobjectParent)
	{
		return new MessageSendingForm((TP5MessageSendingObjectParent)messageSendingobjectParent);
	}
}

