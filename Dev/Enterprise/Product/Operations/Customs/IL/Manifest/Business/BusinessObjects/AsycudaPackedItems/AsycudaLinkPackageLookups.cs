using CargoWise.EntityFramework;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class AsycudaLinkPackageLookups : ZLookups
	{
		public AsycudaLinkPackageLookups(AsycudaLinkPackage parent) : base(parent)
		{
			this.parent = parent;
		}

		public IAsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader> Containers => parent.Package?.Bill?.Header?.Containers;

		readonly AsycudaLinkPackage parent;
	}
}
