using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.Core;
using EuNcts = Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsHeaderLookups : EuNcts.NctsHeaderLookups
{
	public NctsHeaderLookups(NctsHeader parent) : base(parent)
	{
	}

	public override CodeDescriptionPairList CommunicationLanguageList => Factory.GetCachedValue<SwissCustomsLanguageList>();
}
