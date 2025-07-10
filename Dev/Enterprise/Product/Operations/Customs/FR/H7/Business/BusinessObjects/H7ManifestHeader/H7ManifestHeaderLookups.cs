using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.H7.Business
{
	public class H7ManifestHeaderLookups : AsycudaManifestHeaderLookups
	{
		public H7ManifestHeaderLookups(AsycudaManifestHeader parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList AgentTypeList => Factory.GetCachedValue<RepresentationTypeList>();
	}
}
