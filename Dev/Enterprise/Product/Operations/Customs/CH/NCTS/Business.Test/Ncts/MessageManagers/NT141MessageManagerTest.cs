using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class NT141MessageManagerTest : BasePassarDepartureMessageManagerTest<NT141MessageManager>
{
	protected override string MessageSendingObjectMessageType => PassarMessageTypeList.Codes.NT141;

	protected override string ExpectedMovementHeaderPhase => "141";
	protected override string ExpectedMessageSubType => "141";
	protected override Event ExpectedEventType => Events.DeclarationSentToCustoms;
	protected override string ExpectedEventReference => "NT141";

	protected override NT141MessageManager CreateMessageManager(NctsHeader nctsHeader) => new NT141MessageManager(CreateMessageSendingObject(nctsHeader));
}
