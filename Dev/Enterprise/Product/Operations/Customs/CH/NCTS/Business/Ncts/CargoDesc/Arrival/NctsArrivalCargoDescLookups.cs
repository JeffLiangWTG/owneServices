
using CargoWise.Types;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.CH.NCTS.Business;

public sealed class NctsArrivalCargoDescLookups : EU.NCTS.Business.NctsArrivalCargoDescLookups
{
	public NctsArrivalCargoDescLookups(NctsArrivalCargoDesc parent) : base(parent)
	{
	}

	protected override ZString CusCodeListDataGroupingCodeCore => RefDataGrouping.Codes.EuropeanUnionEUN;
}
