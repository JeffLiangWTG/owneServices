using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

public class PackageCheckRN22Strategy : IPackageCheckStrategy
{
	public PackageCheckRN22Strategy(IRN22CheckablePackage currentPackage)
	{
		this.currentPackage = Argument.NotNull(currentPackage, nameof(currentPackage));
		unitCountInfo = Argument.NotNull(currentPackage.UnitCountInfo, nameof(PackageCheckRN22Strategy.currentPackage.UnitCountInfo));
	}

	readonly IRN22CheckablePackage currentPackage;
	readonly ZPropertyInfo unitCountInfo;

	protected virtual ZString QuantityCanBeZeroOnlyWhenTypeAndMarkAreTheSame => ValidationCaptions.Package.QuantityCanBeZeroOnlyWhenTypeAndMarkAreTheSame;

	void IPackageCheckStrategy.Check()
	{
		var previousPackagesCollection = currentPackage.GetRelatedEntryPreviousPackages();

		if (previousPackagesCollection.Any())
		{
			CheckHavePreviousPackageWithSameTypeAndMarksAndUnitCountNoZeroRN22(previousPackagesCollection);
		}
		else
		{
			AddMessageErrorToUnitCount(unitCountInfo);
		}
	}

	void CheckHavePreviousPackageWithSameTypeAndMarksAndUnitCountNoZeroRN22(IEnumerable<IRN22CheckablePackage> previousPackagesCollection)
	{
		foreach (var previousPackage in previousPackagesCollection)
		{
			if (previousPackage.UnitType != currentPackage.UnitType || previousPackage.MarksAndNumbers != currentPackage.MarksAndNumbers)
			{
				AddMessageErrorToUnitCount(unitCountInfo);
				break;
			}

			if (previousPackage.UnitCount > 0)
			{
				break;
			}
		}
	}

	void AddMessageErrorToUnitCount(ZPropertyInfo unitCountInfo) => unitCountInfo.AddMessageError(QuantityCanBeZeroOnlyWhenTypeAndMarkAreTheSame);
}
