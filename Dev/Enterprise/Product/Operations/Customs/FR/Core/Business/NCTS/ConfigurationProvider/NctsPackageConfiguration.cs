using System.Linq;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.Business.NCTS;

public sealed class NctsPackageConfiguration : EU.NCTS.Business.NctsPackageConfiguration
{
	protected override INctsPackageValidationDecider GetValidationDeciderCore(NctsCommonCargoDesc goodsItem)
	{
		INctsPackageValidationDecider result = null;
		if (goodsItem != null)
		{
			if (goodsItem.IsPhase5Departure)
			{
				result = GetDeparturePhase5ValidationDecider(goodsItem.Packages.FirstOrDefault());
			}
			else if (goodsItem.IsPhase5Arrival)
			{
				result = GetArrivalPhase5ValidationDecider();
			}
		}
		return result;
	}

	INctsPackagePhase5ValidationDecider GetDeparturePhase5ValidationDecider(EU.NCTS.Business.NctsPackage nctsPackage) => new NctsPackageDeparturePhase5ValidationDecider(nctsPackage as NctsPackage);
}
