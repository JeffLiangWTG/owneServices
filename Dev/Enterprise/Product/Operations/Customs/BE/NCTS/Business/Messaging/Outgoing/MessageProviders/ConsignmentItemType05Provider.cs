using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class ConsignmentItemType05Provider : IConsignmentItemType05
	{
		readonly NctsCommonCargoDesc item;
		public ConsignmentItemType05Provider(NctsCommonCargoDesc item)
		{
			this.item = Argument.NotNull(item, nameof(item));
		}

		public int GoodsItemNumber => item.BY_LineNo;

		public int DeclarationGoodsItemNumber => item.BY_UnloadedState == NctsUnloadedStateList.Codes.NEW || item.BY_UnloadedState == NctsUnloadedStateList.Codes.DIF
			? item.BY_DeclarationGoodsItemNumber
			: ZInt.Zero;

		public ICommodityType03 Commodity => CachedValueHelper.GetValue(ref commodity,
			() => item.BY_UnloadedState == NctsUnloadedStateList.Codes.NEW || item.BY_UnloadedState == NctsUnloadedStateList.Codes.DIF
			? new CommodityType03Provider(UnloadedItemIfExists ?? item)
			: null);
		CachedValue<ICommodityType03> commodity;

		public IReadOnlyCollection<IPackaging> Packagings => packagings ?? (packagings =
			item.Packages
			.Where(p => p.B5_TypeOfDifference == NctsUnloadedStateList.Codes.NEW || p.B5_TypeOfDifference == NctsUnloadedStateList.Codes.MIS)
			.Select(p => new CC044CPackagingProvider(p))
			.ToArray());
		IReadOnlyCollection<IPackaging> packagings;

		public IReadOnlyCollection<ISupportingDocument> SupportingDocuments => supportingDocuments ?? (supportingDocuments =
			item.SupportingDocuments
			.Where(s => s.CSI_Status == NctsUnloadedStateList.Codes.NEW || s.CSI_Status == NctsUnloadedStateList.Codes.MIS)
			.Select(s => new CC044CSupportingDocumentProvider(s))
			.ToArray());
		IReadOnlyCollection<ISupportingDocument> supportingDocuments;

		public IReadOnlyCollection<IDocument> AdditionalReferences => additionalReferences ?? (additionalReferences = item.AdditionalInfos
			.Where(i => i.CSI_SubType == BEAdditionalDocTypeList.Codes.AdditionalReference && (i.CSI_Status == NctsUnloadedStateList.Codes.NEW || i.CSI_Status == NctsUnloadedStateList.Codes.MIS))
			.Select(i => new CC044CAdditionalReferenceProvider(i))
			.ToArray());
		IReadOnlyCollection<IDocument> additionalReferences;

		public IReadOnlyCollection<ITransportDocument> TransportDocuments => Array.Empty<ITransportDocument>();

		NctsCommonCargoDesc UnloadedItemIfExists => unloadedItemIfExists ?? (unloadedItemIfExists = item.BY_BY_Commodity.IsEmpty ? item : NctsUnloadedCargoDesc.Load((NctsArrivalCargoDesc)item));
		NctsCommonCargoDesc unloadedItemIfExists;
	}
}
