using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;

namespace Enterprise.Customs.CH.Business;

public class NC123DataProvider : BasePassarDeclarationMessageDataProvider, INC123
{
	public NC123DataProvider(DeclarationMessageSendingObject sendingObject) : base(sendingObject)
	{
	}

	public IExportOperation ExportOperation => exportOperation ??= new ExportOperationDataProvider(entryHeader);
	IExportOperation exportOperation;

	public IBaseTransitOperation TransitOperation => null;

	public ITrader TraderAtDeparture => traderAtDeparture ??= TraderDataProvider.New(declaration);
	ITrader traderAtDeparture;

	public string ApprovedLocationIdentificationNumber => declaration.JE_LocationOfGoods;

	public IModeOfTransport ModeOfTransport => declaration.JE_TransportMode.IsEmpty ? null : modeOfTransport ??= ModeOfTransportDataProvider.New(declaration);
	IModeOfTransport modeOfTransport;
}
