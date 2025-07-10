using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.CO.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.CO.Manifest.Business
{
	internal class OpCharacteristicsWrapper : IOperationCharacteristics
	{
		internal OpCharacteristicsWrapper(AsycudaBill bill, bool isHouseBill = false)
		{
			this.bill = Argument.NotNull(bill, "AsycudaBill cannot be null");
			header = bill.Header;
			this.isHouseBill = isHouseBill;
		}
		readonly AsycudaBill bill;
		readonly AsycudaManifestHeader header;
		readonly ZBool isHouseBill;

		bool IOperationCharacteristics.Conditions => bill.Multimodal;

		bool IOperationCharacteristics.CarrierResponsability => bill.CarriersLiability;

		string IOperationCharacteristics.NegotiationType
		{
			get
			{
				var result = COWrappersConstants.LCLSlashLCL;

				var containers = header.Containers.Cast<AsycudaContainer>();
				var count = containers.Count();
				if (count > 0)
				{
					if (containers.All(x => x.ACN_EmptyFullIndicator == EmptyFullIndicatorList.Codes.FullContainerLoad))
					{
						result = COWrappersConstants.FCLSlashFCL;
					}
					else if (containers.All(x => x.ACN_EmptyFullIndicator == EmptyFullIndicatorList.Codes.LessThanFullContainerLoad))
					{
						result = COWrappersConstants.LCLSlashLCL;
					}
				}

				return result;
			}
		}

		string IOperationCharacteristics.LoadType => isHouseBill ? bill.ABL_ContainerMode : COWrappersHelper.GetLoadType(header);

		bool IOperationCharacteristics.Precursors => header.Precursors;

		decimal IOperationCharacteristics.USDFOBValue => bill.ABL_GoodsValue.Truncate(2);

		decimal IOperationCharacteristics.USDFreightValue => bill.ABL_FreightValue.Truncate(2);

		string IOperationCharacteristics.Marks => bill.ABL_MarksAndNumbers;

		int IOperationCharacteristics.ContainerQty => isHouseBill
			? (from AsycudaPack p in bill.Packs select p.Container?.ACN_ContainerNumber).Distinct().Count()
			: header.Containers.Count;

		int IOperationCharacteristics.PackageQty => isHouseBill
			? (int)bill.ABL_ManifestQty
			: (from AsycudaBill p in header.Bills select (int)p.ABL_ManifestQty).Sum();

		decimal IOperationCharacteristics.TotalGrossWeight => isHouseBill
			? Core.Constants.Weight.ConvertSafe(bill.ABL_GrossWeight, bill.ABL_GrossWeightUQ, Core.Constants.Weight.Kilograms)
			: (from AsycudaBill p in header.Bills select Core.Constants.Weight.ConvertSafe(p.ABL_GrossWeight, p.ABL_GrossWeightUQ, Core.Constants.Weight.Kilograms)).Sum();

		decimal IOperationCharacteristics.TotalVolume => isHouseBill
			? Core.Constants.Volume.ConvertSafe(bill.ABL_Volume, bill.ABL_VolumeUQ, Core.Constants.Volume.CubicMetres)
			: (from AsycudaBill p in header.Bills select Core.Constants.Volume.ConvertSafe(p.ABL_Volume, p.ABL_VolumeUQ, Core.Constants.Volume.CubicMetres)).Sum();

		string IOperationCharacteristics.LoadingCountryCode => isHouseBill ? bill.ABL_RL_NKOrigin.Left(2) : bill.ABL_RL_NKPortOfLoading.Left(2);

		string IOperationCharacteristics.LoadingPlaceCode => isHouseBill ? bill.ABL_RL_NKOrigin : bill.ABL_RL_NKPortOfLoading;

		string IOperationCharacteristics.DeliveryMode => header.DeliveryMode;
	}
}
