using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM433GoodsShipmentProvider : IM433GoodsShipment
	{
		public IM433GoodsShipmentProvider(EntryHeaderWrapper entryHeaderWrapper)
		{
			this.entryHeaderWrapper = entryHeaderWrapper;
		}

		readonly EntryHeaderWrapper entryHeaderWrapper;

		public IReadOnlyCollection<ICcQualifierDocument> PreviousDocuments => previousDocumentsCached ?? (previousDocumentsCached = entryHeaderWrapper.Instruction.PreviousDocuments.Select(x => new CcQualifierDocumentProvider(x)).ToArray());
		IReadOnlyCollection<ICcQualifierDocument> previousDocumentsCached;

		public IMConsignment03 Consignment => CachedValueHelper.GetValue(ref consignmentCached, () => new MConsignment03Provider(entryHeaderWrapper));
		CachedValue<IMConsignment03> consignmentCached;

		public IReadOnlyCollection<IM433GoodsShipmentItem> GoodsShipmentItems => goodsShipmentItem ?? (goodsShipmentItem = entryHeaderWrapper.EntryHeader.MergedLines.Select(x => new IM433GoodsShipmentItemProvider(x, entryHeaderWrapper)).ToArray());
		IReadOnlyCollection<IM433GoodsShipmentItem> goodsShipmentItem;
	}
}
