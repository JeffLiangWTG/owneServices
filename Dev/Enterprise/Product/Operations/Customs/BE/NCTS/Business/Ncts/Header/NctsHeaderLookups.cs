using Enterprise.Customs.BE.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.NCTS.Business;

public class NctsHeaderLookups : EU.NCTS.Business.NctsHeaderLookups
{
	public NctsHeaderLookups(EU.NCTS.Business.NctsHeader parent) : base(parent)
	{
	}

	public override CodeDescriptionPairList CommunicationLanguageList => LookupsHelper.GetBELanguageList(Factory);
}

