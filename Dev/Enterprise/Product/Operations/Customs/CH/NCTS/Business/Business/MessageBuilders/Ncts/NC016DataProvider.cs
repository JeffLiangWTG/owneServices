using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using Enterprise.Customs.CH.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NC016DataProvider : BaseNctsMessageDataProvider<NctsHeaderCommonMessageSendingObject>, INC016
{
	public NC016DataProvider(NctsHeaderCommonMessageSendingObject sendingObject) : base(sendingObject)
	{
	}

	public string ExportOperationGDRN => string.Empty;

	public string TransitOperationMRN => CusEntryNumberHelper.MovementReferenceNumberWithoutVersion(nctsHeader.MovementReferenceNumber);

	public string TransportOperationJRN => string.Empty;
}
