using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.Interfaces.AIS.H7V1;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1
{
	public class IM413AndIM415GoodsShipmentProvider : IIM413AndIM415GoodsShipment
	{
		public IM413AndIM415GoodsShipmentProvider(AsycudaBill bill)
		{
			this.bill = bill;
		}
		readonly AsycudaBill bill;

		public IDocumentsAuth DocumentsAuth => CachedValueHelper.GetValue(ref documentsAuthCached, () => new DocumentsAuthProvider(bill));
		CachedValue<IDocumentsAuth> documentsAuthCached;

		public IParties02 Parties => CachedValueHelper.GetValue(ref partiesCached, () => new Parties02Provider(bill));
		CachedValue<IParties02> partiesCached;

		public IValuationInformation ValuationInformation => CachedValueHelper.GetValue(ref valuationInformationCached, () => new ValuationInformationProvider(bill));
		CachedValue<IValuationInformation> valuationInformationCached;

		public IGoodsInformation02 GoodsInformation => CachedValueHelper.GetValue(ref goodsInformationCached, () => new GoodsInformation02Provider(bill));
		CachedValue<IGoodsInformation02> goodsInformationCached;

		public ILocationOfGoodsGNSS DatesPlaces => CachedValueHelper.GetValue(ref datesPlacesCached, () => new LocationOfGoodsGNSSProvider(bill.CusGoodsLocation));
		CachedValue<ILocationOfGoodsGNSS> datesPlacesCached;

		public IReadOnlyCollection<IIM413AndIM415GoodsShipmentItem> GoodsShipmentItems => goodsShipmentItems ??=
			bill.PackedItems.Select(x => new IM413AndIM415GoodsShipmentItemProvider(x)).ToArray();
		IReadOnlyCollection<IIM413AndIM415GoodsShipmentItem> goodsShipmentItems;
	}
}
