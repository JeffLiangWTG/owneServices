using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NT013DataProvider : BaseTransitDeclarationDataProvider, INT013
{
	public NT013DataProvider(NctsHeaderDepartureMessageSendingObject sendingObject) : base(sendingObject)
	{
	}

	public IJustification Justification => justification ?? (justification = JustificationDataProvider.New(sendingObject));
	IJustification justification;
}
