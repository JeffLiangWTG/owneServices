using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NC123DataProvider : BaseNctsMessageDataProvider<NctsHeaderDepartureMessageSendingObject>, INC123
{
	public NC123DataProvider(NctsHeaderDepartureMessageSendingObject sendingObject) : base(sendingObject)
	{
	}

	public IExportOperation ExportOperation => null;

	public IBaseTransitOperation TransitOperation => transitOperation ?? (transitOperation = GetTransitOperation());
	IBaseTransitOperation transitOperation;

	IBaseTransitOperation GetTransitOperation() => sendingObject.IsTransitOperation ? BaseTransitOperationDataProvider.New(nctsHeader) : null;

	public ITrader TraderAtDeparture => traderAtDeparture ?? (traderAtDeparture = TraderDataProvider.New(nctsHeader));
	ITrader traderAtDeparture;

	public string ApprovedLocationIdentificationNumber => sendingObject.GoodsLocation.Address.AuthorisationNumber;

	public IModeOfTransport ModeOfTransport => modeOfTransport ?? (modeOfTransport = ModeOfTransportDataProvider.New(nctsHeader?.MovementHeader));
	IModeOfTransport modeOfTransport;
}
