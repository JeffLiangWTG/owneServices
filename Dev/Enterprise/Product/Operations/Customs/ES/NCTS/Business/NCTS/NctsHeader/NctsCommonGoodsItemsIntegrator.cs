using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.CommonGoodsItemsIntegration;

namespace Enterprise.Customs.ES.NCTS.Business;

public class NctsCommonGoodsItemsIntegrator : EU.NCTS.Business.NctsCommonGoodsItemsIntegrator
{
	public NctsCommonGoodsItemsIntegrator(NctsHeader header) : base(header)
	{
	}

	protected override void CopyFromCommonGoodsItemCore(ICommonGoodsItem source, BusinessObject targetItem)
	{
		base.CopyFromCommonGoodsItemCore(source, targetItem);

		var vehicle = source.Packages.FirstOrDefault();
		var hasVehicle = !vehicle.VehicleIdentificationNumber.IsEmpty && !vehicle.BrandName.IsEmpty && !vehicle.ModelName.IsEmpty;
		if (targetItem is NctsDepartureCargoDesc target && hasVehicle)
		{
			target.IsVehicles = hasVehicle;

			foreach (var package in source.Packages)
			{
				var nctsPackage = target.Packages.AddNew();
				nctsPackage.B5_PackageID = package.VehicleIdentificationNumber;
				nctsPackage.B5_Brand = package.BrandName;
				nctsPackage.B5_Model = package.ModelName;
			}
		}
	}
}
