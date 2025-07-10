using Enterprise.Integration;
using Enterprise.Registry.Business.Customs;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Registry.Testing
{
	[TestedType(typeof(CAPackageTypePairsRegistryItem))]
	sealed class CAPackageTypePairsRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<PackageTypePairCollection<CAPackageTypePair>>
	{
		protected override StronglyTypedRegistryItem<PackageTypePairCollection<CAPackageTypePair>, PackageTypePairCollection<CAPackageTypePair>> GetNewRegistryItem()
		{
			return new CAPackageTypePairsRegistryItem("", null, null, null, RegistryStorageFlags.System, new CAPackageTypePairCollection());
		}
	}
}
