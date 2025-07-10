using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.Business;

public class NE069MessageManager : BasePassarExportDeclarationMessageManager
{
	public NE069MessageManager(ExportDeclarationMessageSendingObject messageSender) : base(messageSender)
	{
	}

	protected override bool ShouldUpdateStatus => true;

	protected override Event DeclarationSentEvent => null;
}
