using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.Business.CommonGoodsItemsIntegration;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants.RefCusCodeList;

namespace Enterprise.Customs.ES.Business.Declaration;

public class ESCommonGoodsItemsIntegrator : EU.Business.Declaration.EuCommonGoodsItemsIntegrator
{
	public ESCommonGoodsItemsIntegrator(EU.Business.Declaration.CusEntryHeader entryHeader) : base(entryHeader)
	{
	}

	protected override ICommonGoodsItem ConvertToCommonGoodsItem(Customs.Business.BaseJobComInvoiceLine baseLine)
	{
		ICommonGoodsItem commonGoodsItem = base.ConvertToCommonGoodsItem(baseLine);
		if (baseLine is JobComInvoiceLine line)
		{
			var vehicles = line.Vehicles.Cast<CusVehicle>();
			if (line.PackagesPivot.IsNullOrEmpty() && vehicles.Any())
			{
				var packages = line.Vehicles.Cast<CusVehicle>().Select(p => new CommonPackage
				{
					PackageType = PackageType.Frame,
					PackageCount = 1,
					VehicleIdentificationNumber = p.CVH_VehicleIdentificationNumber,
					BrandName = p.CVH_BrandName,
					ModelName = p.CVH_ModelName
				});
				commonGoodsItem.Packages = packages.ToList<ICommonPackage>();
			}
		}

		return commonGoodsItem;
	}
}
