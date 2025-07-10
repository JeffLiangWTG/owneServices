using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.NCTS.Business;

public sealed class NctsBillLookups : EU.NCTS.Business.NctsBillLookups
{
	public NctsBillLookups(NctsBill parent) : base (parent)
	{
	}

	public CodeDescriptionPairList StatusList => Factory.GetCachedValue<NctsDeletionStatusList>();
}
