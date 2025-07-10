using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.Business;

public class NE130MessageManager : BasePassarExportDeclarationMessageManager
{
	public NE130MessageManager(DeclarationMessageSendingObject messageSender) : base(messageSender)
	{
	}

	protected override bool ShouldUpdateStatus => true;

	protected override ZString DeclarationSentEventReference => PassarMessageTypeList.Codes.NE130;

	protected override Event DeclarationSentEvent => Events.DeclarationActivationSent;
}
