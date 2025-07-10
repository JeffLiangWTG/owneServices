using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces.AIS.H7V1;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.AIS;
using Enterprise.ZArchitecture.Core;
using IDocument = CargoWise.Customs.IE.MessageContracts.Interfaces.IDocument;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1
{
	public class ItemAmountInvoicedIntrinsicValueProvider : IValuationInformation02
	{
		public ItemAmountInvoicedIntrinsicValueProvider(AsycudaPackedItem packedItem)
		{
			this.packedItem = packedItem;
		}
		readonly AsycudaPackedItem packedItem;

		public IMoney ItemAmount => CachedValueHelper.GetValue(ref itemAmountCached, () => new MoneyProvider(Utilities.Round(packedItem.API_GoodsValue, 2), packedItem.API_RX_NKGoodsValueCurrency));
		CachedValue<IMoney> itemAmountCached;

		public IMoney TransportCosts => CachedValueHelper.GetValue(ref transportCostsCached, () => TransportCostsProvider.NewOrNullPerPackedItem(packedItem.Bill));
		CachedValue<IMoney> transportCostsCached;

		public IReadOnlyCollection<IDocument> TransportDocuments => transportDocuments ??=
			packedItem.AdditionalDocuments.Where(x => x.IsATransportDocument).Select(x => new DocumentProvider(x)).ToArray<IDocument>();
		IReadOnlyCollection<IDocument> transportDocuments;
	}
}
