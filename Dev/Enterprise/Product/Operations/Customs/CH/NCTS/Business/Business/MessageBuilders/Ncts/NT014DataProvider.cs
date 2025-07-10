using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NT014DataProvider : BaseNctsMessageDataProvider<NctsHeaderDepartureMessageSendingObject>, INT014
{
	public NT014DataProvider(NctsHeaderDepartureMessageSendingObject sendingObject) : base(sendingObject)
	{
	}

	public IBaseTransitOperation TransitOperation => transitOperation ?? (transitOperation = BaseTransitOperationDataProvider.New(nctsHeader));
	IBaseTransitOperation transitOperation;

	public IJustification Justification => justification ?? (justification = JustificationDataProvider.New(sendingObject));
	IJustification justification;
}
