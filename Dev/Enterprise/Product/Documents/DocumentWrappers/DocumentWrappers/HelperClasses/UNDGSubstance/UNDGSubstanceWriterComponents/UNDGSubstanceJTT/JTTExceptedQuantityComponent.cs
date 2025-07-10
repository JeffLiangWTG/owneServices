using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.Freight.Forwarding.Business;
using static Enterprise.Freight.Forwarding.Business.ExceptedQuantityUtilities;

namespace Enterprise.DocumentWrappers
{
	class JTTExceptedQuantityComponent : IUNDGSummaryWriterComponent
	{
		bool IUNDGSummaryWriterComponent.IsDefault => true;

		ZString IUNDGSummaryWriterComponent.Write(UNDGSubstanceWrapper wrapper)
		{
			var dataItem = wrapper?.DGData as ForwardingUNDGDataItem;
			if (dataItem == null
				|| !IsSubstancePermittedInLimitedQuantities(dataItem.Substance))
			{
				return ZString.Empty;
			}

			var packType = dataItem.ParentPackLine?.UNDGs.Count == 1
				? UNDGPackType.SingleUNDGPack
				: UNDGPackType.MultiUNDGPack;

			var weightExceedsExceptedQuantity = JTTValidUNDGExceptedQuantityChecker
				.DoesDangerousGoodsQuantityExceedMaximumNetAllowedPerPack(dataItem, packType, ExceptedQuantityMeasurementType.Weight);

			if (weightExceedsExceptedQuantity)
			{
				return ZString.Empty;
			}

			var volumeExceedsExceptedQuantity = JTTValidUNDGExceptedQuantityChecker
				.DoesDangerousGoodsQuantityExceedMaximumNetAllowedPerPack(dataItem, packType, ExceptedQuantityMeasurementType.Volume);

			if (volumeExceedsExceptedQuantity)
			{
				return ZString.Empty;
			}

			return $"DANGEROUS GOODS IN EXCEPTED QUANTITIES: {dataItem.DI_PackageCount} {dataItem.DI_F3_NKPackType}";
		}
	}
}
