using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business
{
	public sealed class CustomsWarehouseProvider : ICustomsWarehouse
	{
		public static CustomsWarehouseProvider NewOrNull(PreviousDocumentMaster previousDocumentMaster) => previousDocumentMaster == null || !previousDocumentMaster.HasPreviousDocuments ? null : new CustomsWarehouseProvider(previousDocumentMaster);

		CustomsWarehouseProvider(PreviousDocumentMaster previousDocumentMaster)
		{
			this.previousDocumentMaster = previousDocumentMaster;
		}
		readonly PreviousDocumentMaster previousDocumentMaster;

		public int GoodsItemQuantity => previousDocuments.Count;

		public string WarehouseOwnerIdentifier => previousDocumentMaster.AuthorizationNumber;

		public string LocalReferenceNumber => previousDocumentMaster.CSI_ReferenceNumber2;

		public IReadOnlyCollection<ICustomsWarehouseGoodsItem> GoodsItems => goodsItems ?? (goodsItems = previousDocuments.Select(x => CustomsWarehouseGoodsItemProvider.NewOrNull(x)).ToArray());
		IReadOnlyCollection<ICustomsWarehouseGoodsItem> goodsItems;

		IReadOnlyCollection<PreviousDocument> previousDocuments => previousDocumentsCached ?? (previousDocumentsCached = previousDocumentMaster.Parent.PreviousDocuments.Cast<PreviousDocument>().ToArray());
		IReadOnlyCollection<PreviousDocument> previousDocumentsCached;
	}
}
