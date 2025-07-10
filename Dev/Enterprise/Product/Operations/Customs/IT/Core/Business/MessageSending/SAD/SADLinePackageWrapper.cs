using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.SAD;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.IT.Business;

public class SADLinePackageWrapper : IPackage
{
	public SADLinePackageWrapper(IPackageProvider packageProvider)
	{
		this.packageProvider = Argument.NotNull(packageProvider, nameof(packageProvider));
	}

	readonly IPackageProvider packageProvider;

	public ZInt? NumberOfPacks => !IsBulkPackageType(PackageType) ? packageProvider.NumberOfPackages : null;

	public ZString PackageType => packageProvider.PackageType;

	public ZInt? NumberOfPieces => PackageType == RefCusCodeUnPackedPackageUnitType.Unpacked ? packageProvider.NumberOfPackages : null;

	public ZString MarksAndNumbers => packageProvider.MarksAndNumbers.Left(SADConstants.CustomsFieldMaxLength.EntryLine.MarksAndNumbers);

	#region Implementation

	ZBool IsBulkPackageType(ZString packageType)
	{
		switch (packageType)
		{
			case RefCusCodeBulkPackageUnitType.BulkLiquidGas:
			case RefCusCodeBulkPackageUnitType.BulkGas:
			case RefCusCodeBulkPackageUnitType.BulkLiquid:
			case RefCusCodeBulkPackageUnitType.BulkPowders:
			case RefCusCodeBulkPackageUnitType.BulkGrains:
			case RefCusCodeBulkPackageUnitType.BulkNodules:
			case RefCusCodeUnPackedPackageUnitType.Unpacked:
			case RefCusCodeUnPackedPackageUnitType.UnpackedSingle:
			case RefCusCodeUnPackedPackageUnitType.UnpackedMultiple:
				return true;
			default:
				return false;
		}
	}

	#endregion
}
