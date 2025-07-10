using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NC123MessageManager : BasePassarDepartureMessageManager<NctsHeaderDepartureMessageSendingObject>
{
	public NC123MessageManager(NctsHeaderDepartureMessageSendingObject messageSender) : base(messageSender)
	{
	}

	public override string MessageFriendlyName => PassarMessageTypeList.Descriptions.NC123;

	protected override string MovementPhaseAfterSending => DeparturePhaseList.Codes.Activation;

	protected override Event EventTypeAfterSending => Events.DeclarationActivationSent;

	protected override void BeforeGenerateMessage(NctsHeaderDepartureMessageSendingObject sendingObject)
	{
		base.BeforeGenerateMessage(sendingObject);

		var movement = sendingObject.MovementHeader;
		movement.BM_InlandTransportMode = sendingObject.InlandTransportModeAtDeparture;
		movement.BM_RN_NKTransportAtDepartureCountry = sendingObject.TransportCountryAtDeparture;
		movement.Header.BH_CommunicationLanguage = sendingObject.CommunicationLanguage;
		movement.BM_TransportAtDeparture = sendingObject.TransportAtDeparture;
		movement.BM_AircraftIDAtDeparture = sendingObject.AircraftIDAtDeparture;
		movement.BM_TransportAtDepartureType = sendingObject.TransportTypeAtDeparture;
		movement.GoodsLocation.CopyPersistentValuesFrom(sendingObject.GoodsLocation);
		movement.GoodsLocation.Address.CopyPersistentValuesFrom(sendingObject.GoodsLocation.Address);
	}
}
