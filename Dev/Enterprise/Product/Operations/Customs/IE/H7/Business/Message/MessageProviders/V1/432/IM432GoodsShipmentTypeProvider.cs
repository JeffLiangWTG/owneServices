using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces.AIS.H7V1;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1
{
	public class IM432GoodsShipmentTypeProvider : IIM432GoodsShipmentType
	{
		readonly AsycudaBill bill;
		public IM432GoodsShipmentTypeProvider(AsycudaBill bill)
		{
			this.bill = Argument.NotNull(bill, nameof(bill));
		}

		public IReadOnlyCollection<IDocument> DocumentsAuthorisations => documentsAuthorisationsCached
			?? (documentsAuthorisationsCached = bill.PreviousDocuments
				.Select(document => new DocumentProvider(document))
				.ToArray());
		IReadOnlyCollection<IDocument> documentsAuthorisationsCached;

		public ILocationOfGoodsGNSS DatesPlaces => CachedValueHelper.GetValue(ref datesPlacesCached, () => new LocationOfGoodsGNSSProvider(bill.CusGoodsLocation));
		CachedValue<ILocationOfGoodsGNSS> datesPlacesCached;

		public IGoodsInformation02 GoodsInformation => CachedValueHelper.GetValue(ref goodsInformationCached, () => new GoodsInformation02Provider(bill));
		CachedValue<IGoodsInformation02> goodsInformationCached;

		public IReadOnlyCollection<IIM432GoodsShipmentItemType> GovernmentAgencyGoodsItem => governmentAgencyGoodsItem ?? (governmentAgencyGoodsItem =
			bill.PackedItems.Cast<AsycudaPackedItem>().Select(x => new IM432GoodsShipmentItemTypeProvider(x)).ToArray());
		IReadOnlyCollection<IIM432GoodsShipmentItemType> governmentAgencyGoodsItem;
	}
}
