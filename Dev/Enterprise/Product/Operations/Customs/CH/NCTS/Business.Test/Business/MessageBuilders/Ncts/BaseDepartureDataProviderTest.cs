using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

abstract class BaseDepartureDataProviderTest<TDataProvider, TMessageSendingObject> : BaseTransitDataProviderTest<TDataProvider, TMessageSendingObject>
	where TDataProvider : class
	where TMessageSendingObject : class, INctsMessageSendingObject
{
	protected override string MovementType => NctsMovementType.Codes.Departure;

	protected override void AddGoodsItem(NctsBill bill) => bill.GoodsItems.AddNew();

	protected RelatedExportEntryHeaderGenPivot RelatedExportEntryHeaderGenPivot
	{
		get
		{
			var relatedExportEntryHeaders = NctsHeader.MovementHeader.RelatedExportEntryHeaders;
			if (relatedExportEntryHeaders.Count == 0)
			{
				var jobDeclaration = Factory.New<JobDeclaration>();
				var cusEntryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
				relatedExportEntryHeaders.AddPivotFor(cusEntryHeader);
			}
			return relatedExportEntryHeaders[0];
		}
	}
}
