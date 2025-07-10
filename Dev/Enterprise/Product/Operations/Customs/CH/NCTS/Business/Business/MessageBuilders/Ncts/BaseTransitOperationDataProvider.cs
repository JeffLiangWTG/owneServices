using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using Enterprise.Customs.CH.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class BaseTransitOperationDataProvider : IBaseTransitOperation
{
	public static BaseTransitOperationDataProvider New(NctsHeader nctsHeader) => nctsHeader == null ? null : new BaseTransitOperationDataProvider(nctsHeader);

	protected BaseTransitOperationDataProvider(NctsHeader nctsHeader)
	{
		this.nctsHeader = nctsHeader;
	}
	protected readonly NctsHeader nctsHeader;

	public string MRN => IncludeMRN ? CusEntryNumberHelper.MovementReferenceNumberWithoutVersion(nctsHeader.MovementReferenceNumber) : null;

	public int? MRNVersion => IncludeMRN ? CusEntryNumberHelper.MovementReferenceNumberVersion(nctsHeader.MovementReferenceNumber) : null;

	protected virtual bool IncludeMRN => true;

	public virtual string OtherThingsToReport => nctsHeader.ArrivalMovementHeader?.OtherThingsToReport.ReturnNullIfEmpty();
}
