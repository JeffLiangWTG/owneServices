using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsDepartureCargoDescLookups : NctsDepartureCargoDescPhase5Lookups
{
	public NctsDepartureCargoDescLookups(NctsDepartureCargoDesc parent) : base(parent)
	{
	}

	protected override ZString CusCodeListDataGroupingCodeCore => RefDataGrouping.Codes.EuropeanUnionEUN;
}
