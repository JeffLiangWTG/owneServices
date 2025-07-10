using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	class ExceptedQuantityComponent : IUNDGSummaryWriterComponent
	{
		bool IUNDGSummaryWriterComponent.IsDefault => false;

		ZString IUNDGSummaryWriterComponent.Write(UNDGSubstanceWrapper wrapper)
		{
			var dataItem = wrapper?.DGData;
			if (dataItem == null)
			{
				return ZString.Empty;
			}

			var exceptedQuantity = dataItem.ExceptedQuantity;
			if (ExceptedQuantityCodes.Contains(exceptedQuantity)
				&& dataItem is ForwardingUNDGDataItem forwardingDataItem)
			{
				var packType = forwardingDataItem.ParentPackLine?.UNDGs.Count == 1
					? ExceptedQuantityUtilities.UNDGPackType.SingleUNDGPack
					: ExceptedQuantityUtilities.UNDGPackType.MultiUNDGPack;

				if (!ValidUNDGExceptedQuantityChecker.DoesDangerousGoodsQuantityExceedMaximumNetAllowedPerPack(forwardingDataItem, packType))
				{
					return $"DANGEROUS GOODS IN EXCEPTED QUANTITIES: {forwardingDataItem.DI_PackageCount} {forwardingDataItem.DI_F3_NKPackType}";
				}
			}

			return ZString.Empty;
		}

		ZString[] ExceptedQuantityCodes => exceptedQuantityCodes ?? (exceptedQuantityCodes = new ZString[]
		{
			UNDGSubstanceLookups.ExceptedQuantity.Code.E1,
			UNDGSubstanceLookups.ExceptedQuantity.Code.E2,
			UNDGSubstanceLookups.ExceptedQuantity.Code.E3,
			UNDGSubstanceLookups.ExceptedQuantity.Code.E4,
			UNDGSubstanceLookups.ExceptedQuantity.Code.E5
		});

		ZString[] exceptedQuantityCodes;
	}
}
