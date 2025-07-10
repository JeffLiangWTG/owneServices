using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.ExitControl.Business;

public class CusExitConsignmentPackageUcc6Lookups : CusExitConsignmentPackageLookups
{
	public CusExitConsignmentPackageUcc6Lookups(AutoCusExitConsignmentPackage parent) : base(parent)
	{
	}

	protected override CodeDescriptionPairList StatusListCore => Factory.GetCachedValue<DiscrepanciesStatusCodeList>();
}
