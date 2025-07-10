using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business
{
	public class InwardProcessingProvider : IInwardProcessing
	{
		public static InwardProcessingProvider NewOrNull(PreviousDocumentMaster previousDocumentMaster) => previousDocumentMaster == null || !previousDocumentMaster.HasPreviousDocuments ? null : new InwardProcessingProvider(previousDocumentMaster);

		InwardProcessingProvider(PreviousDocumentMaster previousDocumentMaster)
		{
			this.previousDocumentMaster = previousDocumentMaster;
		}
		readonly PreviousDocumentMaster previousDocumentMaster;

		public int GoodsItemQuantity => previousDocuments.Count;

		public string ProcessingOwnerIdentifier => previousDocumentMaster.AuthorizationNumber;

		public bool SimplifiedGrantAuthorisationFlag => previousDocumentMaster.SimplifiedGrantAuthorizationFlag;

		public string MonitoringCustomsOfficeReferenceNumber => previousDocumentMaster.CSI_CustomsOffice;

		public IReadOnlyCollection<IInwardProcessingGoodsItem> GoodsItems => goodsItems ?? (goodsItems = previousDocuments.Select(x => InwardProcessingGoodsItemProvider.NewOrNull(x)).ToArray());
		IReadOnlyCollection<IInwardProcessingGoodsItem> goodsItems;

		IReadOnlyCollection<PreviousDocument> previousDocuments => previousDocumentsCached ?? (previousDocumentsCached = previousDocumentMaster.Parent.PreviousDocuments.Cast<PreviousDocument>().ToArray());
		IReadOnlyCollection<PreviousDocument> previousDocumentsCached;
	}
}
