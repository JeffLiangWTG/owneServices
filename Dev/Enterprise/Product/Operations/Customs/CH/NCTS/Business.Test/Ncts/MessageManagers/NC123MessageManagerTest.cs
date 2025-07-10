using System.Linq;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class NC123MessageManagerTest : BasePassarDepartureMessageManagerTest<NC123MessageManager, NctsHeaderDepartureMessageSendingObject>
{
	protected override string MessageSendingObjectMessageType => PassarMessageTypeList.Codes.NC123;

	protected override string ExpectedMovementHeaderPhase => "123";
	protected override string ExpectedMessageSubType => MessageSubTypeCodeList.Codes.NctsActivationAtDomicile;
	protected override Event ExpectedEventType => Events.DeclarationActivationSent;
	protected override string ExpectedEventReference => string.Empty;

	protected override NC123MessageManager CreateMessageManager(NctsHeader nctsHeader) => new NC123MessageManager(new NctsHeaderDepartureMessageSendingObject(nctsHeader) { MessageType = PassarMessageTypeList.Codes.NC123 });

	public void TestPropertiesWrittenBackToMovementNC123() => CombineAssertions("BeforeGenerateMessage", () =>
	{
		var manager = CreateMessageManager(NctsHeader);

		manager.SendingObject.MessageType = PassarMessageTypeList.Codes.NC123;
		manager.SendingObject.InlandTransportModeAtDeparture = "1";
		manager.SendingObject.TransportTypeAtDeparture = "20";
		manager.SendingObject.AircraftIDAtDeparture = "30";
		manager.SendingObject.TransportAtDeparture = "40";
		manager.SendingObject.TransportCountryAtDeparture = "50";
		manager.SendingObject.CommunicationLanguage = "80";
		manager.SendingObject.GoodsLocation.CGL_AdditionalIdentifier = "91";
		manager.SendingObject.GoodsLocation.Address.AuthorisationNumber = "92";

		var message = manager.GenerateMessages().First();

		AssertEquals("BM_InlandTransportMode", "1", manager.SendingObject.MovementHeader.BM_InlandTransportMode);
		AssertEquals("BM_RN_NKTransportAtDepartureCountry", "50", manager.SendingObject.MovementHeader.BM_RN_NKTransportAtDepartureCountry);
		AssertEquals("BH_CommunicationLanguage", "80", manager.SendingObject.NctsHeader.BH_CommunicationLanguage);

		AssertEquals("BM_TransportAtDepartureType", "20", manager.SendingObject.MovementHeader.BM_TransportAtDepartureType);
		AssertEquals("BM_AircraftIDAtDeparture", "30", manager.SendingObject.MovementHeader.BM_AircraftIDAtDeparture);
		AssertEquals("BM_TransportAtDeparture", "40", manager.SendingObject.MovementHeader.BM_TransportAtDeparture);

		AssertEquals("CGL_AdditionalIdentifier", "91", manager.SendingObject.MovementHeader.GoodsLocation.CGL_AdditionalIdentifier);
		AssertEquals("E2_GovRegNum", "92", manager.SendingObject.MovementHeader.GoodsLocation.Address.E2_GovRegNum);
	});
}
