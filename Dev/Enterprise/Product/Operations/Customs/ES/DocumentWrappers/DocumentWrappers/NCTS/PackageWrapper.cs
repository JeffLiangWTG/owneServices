using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Business;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ES.DocumentWrappers.NCTS;

public class PackageWrapper : EU.NCTS.Business.PackageWrapper
{
	public PackageWrapper(NctsPackage package, ZBool isVehicles) : base(package)
	{
		this.isVehicles = isVehicles;
	}

	readonly ZBool isVehicles;
	const string VehiclesSeparator = ":";

	protected override ZString MarksAndNumbersOfPackagesCore => isVehicles ? package.B5_PackageID + VehiclesSeparator + package.B5_Brand + VehiclesSeparator + package.B5_Model : base.MarksAndNumbersOfPackagesCore;

	protected override ZString KindOfPackagesCore => isVehicles ? RefCusCodeList.PackageType.Frame : base.KindOfPackagesCore;

	protected override ZLong GetNumberOfPackages() => isVehicles ? 1 : base.GetNumberOfPackages();
}
