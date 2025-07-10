using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IL.Business
{
	public class GlbILStaffExternalPasswordLookups : GlbExternalPasswordLookups
	{
		public GlbILStaffExternalPasswordLookups(AutoGlbExternalPassword parent) : base(parent)
		{
		}

		public CodeDescriptionPairList CertificateAuthoritiesList => Factory.GetCachedValue<CertificateAuthoritiesList>();
	}
}
