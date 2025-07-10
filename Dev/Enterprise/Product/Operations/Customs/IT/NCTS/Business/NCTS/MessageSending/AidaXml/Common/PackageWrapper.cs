using System;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

sealed class PackageWrapper : IPackage
{
	public PackageWrapper(NctsPackage package)
	{
		Argument.NotNull(package, nameof(package));

		lazyPackageType = new Lazy<string>(() => package.B5_UnitType);
		lazyNumberOfPacks = new Lazy<int?>(() => GetNumberOfPacks(package.B5_UnitCount));
		lazyMarksAndNumbers = new Lazy<string>(() => package.B5_MarksAndNumbers);
	}

	#region IPackage Members

	string IPackage.PackageType => lazyPackageType.Value;

	int? IPackage.NumberOfPacks => lazyNumberOfPacks.Value;

	string IPackage.MarksAndNumbers => lazyMarksAndNumbers.Value;

	#endregion

	#region Implementation

	int GetNumberOfPacks(ZLong unitCount)
	{
		const int maxAllowedValueInMessage = 99999999;
		const int invalidValueThatWillCauseMessageRejection = -1;

		return unitCount <= maxAllowedValueInMessage
			? (int)unitCount.ToZInt()
			: invalidValueThatWillCauseMessageRejection;
	}

	readonly Lazy<string> lazyPackageType;
	readonly Lazy<int?> lazyNumberOfPacks;
	readonly Lazy<string> lazyMarksAndNumbers;

	#endregion
}
