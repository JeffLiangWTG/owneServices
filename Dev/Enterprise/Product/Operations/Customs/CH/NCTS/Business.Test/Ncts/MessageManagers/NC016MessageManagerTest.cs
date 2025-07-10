using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NC016MessageManager))]
sealed class NC016MessageManagerTest : BasePassarMessageManagerTest<NC016MessageManager, NctsHeaderCommonMessageSendingObject>
{
	protected override string MessageSendingObjectMessageType => PassarMessageTypeList.Codes.NC016;

	protected override ZString MovementType => NctsMovementType.Codes.Departure;

	protected override string ExpectedMovementHeaderPhase => MessageSubTypeCodeList.Codes.PassarRequestDataJourney;

	protected override string ExpectedMessageSubType => MessageSubTypeCodeList.Codes.PassarRequestDataJourney;

	protected override Event ExpectedEventType => Events.DeclarationSentToCustoms;

	protected override string ExpectedEventReference => PassarMessageTypeList.Codes.NC016;

	protected override NC016MessageManager CreateMessageManager(NctsHeader nctsHeader) => new NC016MessageManager(new NctsHeaderDepartureMessageSendingObject(nctsHeader) { MessageType = MessageSendingObjectMessageType });

	protected override void AssertAfterGenerateMessage() => EventsTestHelper.AssertEventAdded(NctsHeader.CommonMovementHeader, Events.MessageStatusChange, NctsHeader.EffectiveMessageStatus);
}
