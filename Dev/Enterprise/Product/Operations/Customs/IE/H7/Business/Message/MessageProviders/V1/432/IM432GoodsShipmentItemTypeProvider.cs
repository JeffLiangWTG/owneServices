using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces.AIS.H7V1;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1
{
	public class IM432GoodsShipmentItemTypeProvider : IIM432GoodsShipmentItemType
	{
		readonly AsycudaPackedItem packedItem;

		public IM432GoodsShipmentItemTypeProvider(AsycudaPackedItem packedItem)
		{
			this.packedItem = packedItem;
		}

		public string GoodsItemNumber => packedItem.API_LineNo.ToString();

		public IReadOnlyCollection<IDocument> DocumentsAuthorisations => documentsAuthorisationsCached
			?? (documentsAuthorisationsCached = packedItem.PreviousDocuments
				.Select(document => new DocumentProvider(document))
				.ToArray());
		IReadOnlyCollection<IDocument> documentsAuthorisationsCached;

		public IIM432GoodsInformation GoodsInformation => CachedValueHelper.GetValue(ref goodsInformationCached, () => new IM432GoodsInformationProvider(packedItem));
		CachedValue<IIM432GoodsInformation> goodsInformationCached;
	}
}
