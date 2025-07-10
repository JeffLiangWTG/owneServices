using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.Business.Declaration;

sealed class PackageCheckR0219Strategy : IPackageCheckStrategy
{
	public PackageCheckR0219Strategy(IR0219CheckablePackage currentPackage)
	{
		this.currentPackage = Argument.NotNull(currentPackage, nameof(currentPackage));
		unitCountInfo = Argument.NotNull(currentPackage.UnitCountInfo, nameof(PackageCheckR0219Strategy.currentPackage.UnitCountInfo));
	}

	readonly IR0219CheckablePackage currentPackage;
	readonly ZPropertyInfo unitCountInfo;

	void IPackageCheckStrategy.Check()
	{
		if (currentPackage.GetRelatedEntryPackagesWithPacksEqualToZero().Any())
		{
			unitCountInfo.AddMessageError(ValidationCaptions.Package.NoOtherPackageHavePackageNumberGreaterThanZero);
		}
	}
}
