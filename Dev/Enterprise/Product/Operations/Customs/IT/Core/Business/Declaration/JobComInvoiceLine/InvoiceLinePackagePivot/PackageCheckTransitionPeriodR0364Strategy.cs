using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.Business.Declaration;

sealed class PackageCheckTransitionPeriodR0364Strategy : IPackageCheckStrategy
{
	public PackageCheckTransitionPeriodR0364Strategy(IR0364CheckablePackage currentPackage)
	{
		this.currentPackage = Argument.NotNull(currentPackage, nameof(currentPackage));
		unitCountInfo = Argument.NotNull(currentPackage.UnitCountInfo, nameof(PackageCheckTransitionPeriodR0364Strategy.currentPackage.UnitCountInfo));
	}

	readonly IR0364CheckablePackage currentPackage;
	readonly ZPropertyInfo unitCountInfo;

	void IPackageCheckStrategy.Check()
	{
		var otherPackagesCollection = currentPackage.GetRelatedEntryOtherPackagesWithSameTypeAndMarks();

		if (otherPackagesCollection.Any())
		{
			CheckHaveOtherPackagesWithSameTypeAndMarksAndUnitCountNoZeroR0364(otherPackagesCollection);
		}
		else
		{
			AddMessageErrorToUnitCount(unitCountInfo);
		}
	}

	void CheckHaveOtherPackagesWithSameTypeAndMarksAndUnitCountNoZeroR0364(IEnumerable<IR0364CheckablePackage> otherPackagesCollection)
	{
		if (!otherPackagesCollection.Any(x => x.UnitCount > 0))
		{
			AddMessageErrorToUnitCount(unitCountInfo);
		}
	}

	void AddMessageErrorToUnitCount(ZPropertyInfo unitCountInfo) => unitCountInfo.AddMessageError(ValidationCaptions.Package.QuantityCanBeZeroOnlyWhenOtherEntryLineHasSamePackTypeMarksAndQtyDifferentFromZero);
}
