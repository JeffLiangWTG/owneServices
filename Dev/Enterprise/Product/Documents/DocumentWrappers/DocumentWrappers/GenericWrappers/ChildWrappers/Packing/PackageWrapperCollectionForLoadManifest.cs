using CargoWise.EntityFramework;
using Enterprise.Packing.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class PackageWrapperCollectionForLoadManifest : PackageWrapperCollection
	{
		public PackageWrapperCollectionForLoadManifest(PkgPackageJob packageJob, BusinessObjectFactory factory)
			: base(factory)
		{
			var packages = packageJob?.Packages;
			if (packages != null)
			{
				foreach (var package in packages)
				{
					if (!package.KP_PackageID.IsEmpty)
					{
						Add(new PackageWrapperForLoadManifest(package, Factory));
					}
				}
			}
		}
	}
}
