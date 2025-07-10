using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using static Enterprise.Customs.GB.Business.NCTS.Constants;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class ConsignmentItemType05Provider : IConsignmentItemType05
	{
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
			? new CommodityType03Provider(UnloadedItemIfExists)
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
		.Select(s => new CC044CSupportingDocumentProvider(s, IsInPhase5TransitionPeriod))
			.ToArray());
		IReadOnlyCollection<ISupportingDocument> supportingDocuments;

		public IReadOnlyCollection<IAdditionalReference> AdditionalReferences => additionalReferences ?? (additionalReferences = item.AdditionalInfos
			.Where(i => i.CSI_SubType == CusSupportingInfoSubTypes.AdditionalReference && (i.CSI_Status == NctsUnloadedStateList.Codes.NEW || i.CSI_Status == NctsUnloadedStateList.Codes.MIS))
			.Select(i => new CC044CAdditionalReferenceProvider(i, IsInPhase5TransitionPeriod))
			.ToArray());
		IReadOnlyCollection<IAdditionalReference> additionalReferences;
		public IReadOnlyCollection<ITransportDocument> TransportDocuments => Array.Empty<ITransportDocument>();

		NctsCommonCargoDesc UnloadedItemIfExists => unloadedItemIfExists ?? (unloadedItemIfExists = item.BY_BY_Commodity.IsEmpty ? item : NctsUnloadedCargoDesc.Load((NctsArrivalCargoDesc)item));
		NctsCommonCargoDesc unloadedItemIfExists;

		bool IsInPhase5TransitionPeriod => CachedValueHelper.GetValue(ref isInPhase5TransitionPeriod, () => item.IsInPhase5TransitionPeriod);
		CachedValue<bool> isInPhase5TransitionPeriod;

		readonly NctsCommonCargoDesc item;
	}
}
