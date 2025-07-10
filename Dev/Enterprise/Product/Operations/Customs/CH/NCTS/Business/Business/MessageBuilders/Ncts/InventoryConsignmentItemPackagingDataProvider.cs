using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class InventoryConsignmentItemPackagingDataProvider : ConsignmentItemPackagingDataProvider
{
	public new static IEnumerable<InventoryConsignmentItemPackagingDataProvider> NewCollection(INctsPackageCollection<EU.NCTS.Business.NctsPackage, NctsCommonCargoDesc> packages)
	{
		return packages?.Cast<NctsPackage>()
			.Where(x => x.B5_TypeOfDifference.IsUnloadingStateNEWorMISorDIF())
			.Select((package, index) => new InventoryConsignmentItemPackagingDataProvider(package));
	}

	InventoryConsignmentItemPackagingDataProvider(NctsPackage package) : base(package, package.B5_SequenceNumber)
	{
		isMIS = package.B5_TypeOfDifference == NctsUnloadedStateList.Codes.MIS;
		isDIFWithDifferences = (package.Parent as NctsArrivalCargoDesc)?.IsDIFWithDifferencesIncludingPackages ?? false;
	}
	readonly bool isMIS;
	readonly bool isDIFWithDifferences;

	protected override T? GetPackageValue<T>(Func<NctsPackage, T> getter)
	{
		if (isMIS)
		{
			return null;
		}
		if (package.PackDifference == null || !isDIFWithDifferences)
		{
			return getter(package);
		}
		var differenceValue = getter(package.PackDifference);
		if (differenceValue.IsEmpty || differenceValue.Equals(getter(package)))
		{
			return null;
		}
		return differenceValue;
	}

	protected override bool IsBulkCore
	{
		get
		{
			if (package.B5_TypeOfDifference == NctsUnloadedStateList.Codes.DIF && package.PackDifference is NctsPackage packDifference && !package.PackDifference.B5_UnitType.IsEmpty)
			{
				return packDifference.IsBulk;
			}
			return package.IsBulk;
		}
	}
}
