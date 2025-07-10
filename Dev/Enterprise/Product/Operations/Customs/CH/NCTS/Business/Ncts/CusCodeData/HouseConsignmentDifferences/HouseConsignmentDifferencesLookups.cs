using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.NCTS.Business;

public class HouseConsignmentDifferencesLookups : CusCodeDataLookups
{
	public HouseConsignmentDifferencesLookups(AutoCusCodeData parent) : base(parent)
	{
	}

	public new HouseConsignmentDifferences Parent => (HouseConsignmentDifferences)base.Parent;

	public override CodeDescriptionPairList CY_CodeList => Factory.GetCachedValue<UnloadingRemarkCodeList>();
}
