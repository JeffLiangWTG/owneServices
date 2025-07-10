using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.ExitControl.Business;

public class CusExitContainerUcc6Lookups : CusExitContainerLookups
{
	public CusExitContainerUcc6Lookups(AutoCusExitContainer parent) : base(parent)
	{
	}

	protected override CodeDescriptionPairList StatusListCore => Factory.GetCachedValue<DiscrepanciesStatusCodeList>();
}
