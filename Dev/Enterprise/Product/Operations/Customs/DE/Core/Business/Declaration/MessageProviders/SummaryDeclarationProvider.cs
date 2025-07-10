using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business
{
	public sealed class SummaryDeclarationProvider : ISummaryDeclaration
	{
		public static SummaryDeclarationProvider NewOrNull(PreviousDocumentMaster previousDocumentMaster) => previousDocumentMaster == null || !previousDocumentMaster.HasPreviousDocuments ? null : new SummaryDeclarationProvider(previousDocumentMaster);

		SummaryDeclarationProvider(PreviousDocumentMaster previousDocumentMaster)
		{
			previousDocuments = previousDocumentMaster.Parent.PreviousDocuments.Cast<PreviousDocument>().ToArray();
		}
		readonly PreviousDocument[] previousDocuments;

		public string IdentificationIndicator => previousDocuments[0].CSI_SubType;

		public IReadOnlyCollection<ISummaryDeclarationGoodsItem> GoodsItems => goodsItems ?? (goodsItems = previousDocuments.Select(x => SummaryDeclarationGoodsItemProvider.NewOrNull(x)).ToArray());
		IReadOnlyCollection<ISummaryDeclarationGoodsItem> goodsItems;
	}
}
