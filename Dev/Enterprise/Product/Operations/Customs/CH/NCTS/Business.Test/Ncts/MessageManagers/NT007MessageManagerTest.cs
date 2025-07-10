using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class NT007MessageManagerTest : BasePassarMessageManagerTest<NT007MessageManager, NctsHeaderArrivalMessageSendingObject>
{
	protected override ZString MovementType => NctsMovementType.Codes.Arrival;

	protected override string MessageSendingObjectMessageType => PassarMessageTypeList.Codes.NT007;

	protected override string ExpectedMovementHeaderPhase => "007";
	protected override string ExpectedMessageSubType => "007";
	protected override Event ExpectedEventType => Events.DeclarationSentToCustoms;
	protected override string ExpectedEventReference => "NT007";

	protected override NT007MessageManager CreateMessageManager(NctsHeader nctsHeader) => new NT007MessageManager(new NctsHeaderArrivalMessageSendingObject(nctsHeader) { MessageType = MessageSendingObjectMessageType });
}
