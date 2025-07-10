using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.Business.Declaration;

sealed class PackageCheckR0364Strategy : IPackageCheckStrategy
{
	public PackageCheckR0364Strategy(IR0364CheckablePackage currentPackage)
	{
		this.currentPackage = Argument.NotNull(currentPackage, nameof(currentPackage));
		unitCountInfo = Argument.NotNull(currentPackage.UnitCountInfo, nameof(PackageCheckR0364Strategy.currentPackage.UnitCountInfo));
	}

	readonly IR0364CheckablePackage currentPackage;
	readonly ZPropertyInfo unitCountInfo;

	void IPackageCheckStrategy.Check()
	{
		if (!currentPackage.GetRelatedEntryPackagesWithSameMarksAndPacksGreaterThanZero().Any())
		{
			unitCountInfo.AddMessageError(ValidationCaptions.Package.QuantityCanBeZeroOnlyWhenOtherLineWithSameMarksAndBulkPackType);
		}
	}
}
