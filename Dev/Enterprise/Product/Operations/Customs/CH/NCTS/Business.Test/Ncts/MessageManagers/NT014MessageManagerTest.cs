using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class NT014MessageManagerTest : BasePassarDepartureMessageManagerTest<NT014MessageManager>
{
	protected override string MessageSendingObjectMessageType => PassarMessageTypeList.Codes.NT014;

	protected override string ExpectedMovementHeaderPhase => "014";
	protected override string ExpectedMessageSubType => "014";
	protected override Event ExpectedEventType => Events.DeclarationCancellationSent;
	protected override string ExpectedEventReference => string.Empty;

	protected override NT014MessageManager CreateMessageManager(NctsHeader nctsHeader) => new NT014MessageManager(CreateMessageSendingObject(nctsHeader));
}
