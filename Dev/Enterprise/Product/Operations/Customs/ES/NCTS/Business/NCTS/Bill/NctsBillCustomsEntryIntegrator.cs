using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.Declaration;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants.RefCusCodeList;

namespace Enterprise.Customs.ES.NCTS.Business;

sealed class NctsBillCustomsEntryIntegrator : EU.NCTS.Business.NctsBillCustomsEntryIntegrator
{
	public NctsBillCustomsEntryIntegrator(NctsBill houseConsignment) : base(houseConsignment)
	{
	}

	protected override bool ShouldAssignItemNumberToN830PreviousDocument => false;

	protected override void CreatePackagingDetailsCore(EU.Business.Declaration.CusEntryLine entryLine, EU.NCTS.Business.NctsDepartureCargoDesc goodsItem)
	{
		base.CreatePackagingDetailsCore(entryLine, goodsItem);

		var packagingDetailCollection = entryLine.PackagingDetails;
		var baseLine = entryLine.FirstLine;

		if (baseLine is JobComInvoiceLine line && goodsItem is NctsDepartureCargoDesc target)
		{
			var vehicles = line.Vehicles.Cast<CusVehicle>();

			if (packagingDetailCollection.IsNullOrEmpty() && vehicles.Any())
			{
				target.IsVehicles = vehicles.Any();

				foreach (var vehicle in vehicles)
				{
					var nctsPackage = goodsItem.Packages.AddNew();
					nctsPackage.B5_UnitType = PackageType.Frame;
					nctsPackage.B5_UnitCount = 1;
					nctsPackage.B5_PackageID = vehicle.CVH_VehicleIdentificationNumber;
					nctsPackage.B5_Brand = vehicle.CVH_BrandName;
					nctsPackage.B5_Model = vehicle.CVH_ModelName;
				}
			}
		}
	}
}
