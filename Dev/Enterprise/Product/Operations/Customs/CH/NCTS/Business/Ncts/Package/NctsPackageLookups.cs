using CargoWise.Types;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsPackageLookups : EU.NCTS.Business.NctsPackageLookups
{
	public NctsPackageLookups(NctsPackage parent) : base(parent)
	{
	}

	protected override ZBool ShouldIncludeDIFInUnloadedStatesList => true;
}
