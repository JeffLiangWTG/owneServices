using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.ExitControl.Business;

public class CusExitSealUcc6Lookups : CusExitSealLookups
{
	public CusExitSealUcc6Lookups(AutoCusSeal parent) : base(parent)
	{
	}

	protected override CodeDescriptionPairList StatusListCore => Factory.GetCachedValue<DiscrepanciesStatusCodeList>();
}
