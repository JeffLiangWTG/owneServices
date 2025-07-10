using CargoWise.Types;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsUnloadedCargoDescLookups : EU.NCTS.Business.NctsUnloadedCargoDescLookups
{
	public NctsUnloadedCargoDescLookups(EU.NCTS.Business.NctsUnloadedCargoDesc parent) : base(parent)
	{
	}

	protected override ZString CusCodeListDataGroupingCodeCore => RefDataGrouping.Codes.EuropeanUnionEUN;
}
