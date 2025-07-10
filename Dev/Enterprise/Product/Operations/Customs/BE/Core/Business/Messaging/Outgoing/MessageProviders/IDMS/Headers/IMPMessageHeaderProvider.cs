using CargoWise.Customs.BE.MessageContracts.Interfaces;

namespace Enterprise.Customs.BE.Business;

public abstract class IMPMessageHeaderProvider : MessageHeaderProvider, IMessageHeader
{
	protected IMPMessageHeaderProvider(BEJobDeclarationMessageSendingObject messageSendingAction) : base(messageSendingAction)
	{
	}

	public override string MessageSender => BECustomsRegistry.Instance.SenderIDs.GetTargetSystemName(SendMessageTypes.Codes.IMP);

	public override string MessageRecipient => BECustomsRegistry.Instance.CustomsMessageVersion.GetTargetSystemName(SendMessageTypes.Codes.IMP);
}
