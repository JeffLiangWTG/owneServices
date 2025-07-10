using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.Business;

public class NE013MessageManager : BasePassarExportDeclarationMessageManager
{
	public NE013MessageManager(ExportDeclarationMessageSendingObject messageSender) : base(messageSender)
	{
	}

	protected override Event DeclarationSentEvent => null;

	protected override void AfterGenerateMessage(DeclarationMessageSendingObject sendingObject, EDIMessage message)
	{
		base.AfterGenerateMessage(sendingObject, message);

		var entryHeader = SendingObject.Header;
		entryHeader.Logs.AddNew(Events.MessageStatusChange, entryHeader.CH_Status);
	}
}
