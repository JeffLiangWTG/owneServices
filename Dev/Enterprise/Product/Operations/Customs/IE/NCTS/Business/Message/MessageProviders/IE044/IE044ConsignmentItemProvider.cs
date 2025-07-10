using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IE.Business;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class IE044ConsignmentItemProvider : IIE044ConsignmentItem
	{
		public IE044ConsignmentItemProvider(NctsArrivalCargoDesc goodsItem)
		{
			this.goodsItem = Argument.NotNull(goodsItem, nameof(goodsItem));
		}
		readonly NctsArrivalCargoDesc goodsItem;

		public string GoodsItemNumber => goodsItem.BY_LineNo.ToString();

		public string DeclarationGoodsItemNumber => goodsItem.BY_LineNo.ToString();

		public IIE044CommodityType Commodity => commodity ?? (commodity = new IE044CommodityTypeProvider(goodsItem));
		IIE044CommodityType commodity;

		public IReadOnlyCollection<IPackaging> Packages => packages ?? (packages =
				goodsItem.Packages
				.Where(p => !p.B5_TypeOfDifference.EqualsIgnoringCase(NctsUnloadedStateList.Codes.DEC))
				.Select(p => new PackagingProvider(p.B5_UnitType, p.B5_UnitCount.ToZInt(), p.B5_MarksAndNumbers))
				.ToArray());
		IReadOnlyCollection<IPackaging> packages;

		public IReadOnlyCollection<ISupportingDocument> SupportingDocuments => supportingDocuments ?? (supportingDocuments =
				goodsItem.SupportingDocuments
				.Where(x => x.CSI_Type == CusSupportingInfoTypeList.Codes.SupportingDocument && x.CSI_Status != NctsUnloadedStateList.Codes.DEC)
				.Select(s => new SupportingDocumentProvider(s))
				.ToArray());
		IReadOnlyCollection<ISupportingDocument> supportingDocuments;

		public IReadOnlyCollection<IDocument> TransportDocuments => transportDocuments ?? (transportDocuments =
			goodsItem.AdditionalInfos.Where(x => x.CSI_SubType.EqualsIgnoringCase(AdditionalInfoSubTypeList.Codes.TransportDocument) &&
				x.CSI_Status != NctsUnloadedStateList.Codes.DEC)
				.Select(x => new DocumentProvider(x))
				.ToArray());
		IReadOnlyCollection<IDocument> transportDocuments;

		public IReadOnlyCollection<IDocument> AdditionalReferences => additionalReferences ?? (additionalReferences =
				goodsItem.AdditionalInfos.Where(x => x.CSI_SubType.EqualsIgnoringCase(AdditionalInfoSubTypeList.Codes.AdditionalReference) &&
				x.CSI_Status != NctsUnloadedStateList.Codes.DEC)
				.Select(x => new DocumentProvider(x))
				.ToArray());
		IReadOnlyCollection<IDocument> additionalReferences;
	}
}
