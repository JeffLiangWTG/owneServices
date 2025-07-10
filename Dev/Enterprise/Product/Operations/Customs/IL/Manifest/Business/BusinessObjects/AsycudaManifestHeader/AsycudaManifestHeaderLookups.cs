using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public sealed class AsycudaManifestHeaderLookups : ASYCUDA.Business.AsycudaManifestHeaderLookups
	{
		public AsycudaManifestHeaderLookups(AsycudaManifestHeader parent)
			: base(parent)
		{
		}

		public OrganisationsFindBoxCollection DeclarantList => Factory.GetCachedValue("IL.AsycudaManifestHeaderLookups.DeclarantList", () => new OrganisationsFindBoxCollection(Factory));
	}
}
