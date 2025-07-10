using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NT513DataProvider : BaseTransitDeclarationDataProvider, INT513
{
	public NT513DataProvider(NctsHeaderDepartureMessageSendingObject sendingObject) : base(sendingObject)
	{
	}

	public IJustification Justification => justification ?? (justification = JustificationDataProvider.New(sendingObject));
	IJustification justification;
}
