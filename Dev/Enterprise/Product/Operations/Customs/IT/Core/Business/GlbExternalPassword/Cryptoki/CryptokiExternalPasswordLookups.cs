using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business;

public class CryptokiExternalPasswordLookups : MasterFiles.Business.GlbExternalPasswordLookups
{
	public CryptokiExternalPasswordLookups(CryptokiExternalPassword parent)
		: base(parent)
	{
	}

	public CodeDescriptionPairList CertificateAuthorityList
	{
		get { return Factory.GetCachedValue<CertificateAuthorityList>(); }
	}

	public CodeDescriptionPairList ChipsetList
	{
		get { return Factory.GetCachedValue<ChipsetList>(); }
	}
}
