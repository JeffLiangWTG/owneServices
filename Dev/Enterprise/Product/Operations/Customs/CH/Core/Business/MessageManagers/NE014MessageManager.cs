using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.Business;

public class NE014MessageManager : BasePassarExportDeclarationMessageManager
{
	public NE014MessageManager(DeclarationMessageSendingObject messageSender) : base(messageSender)
	{
	}

	protected override Event DeclarationSentEvent => Events.DeclarationCancellationSent;
}
