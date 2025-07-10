using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Customs.US.Testing
{
	[TestedType(typeof(USPackageTypePairsRegistryItem))]
	sealed class USPackageTypePairsRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<PackageTypePairCollection<USPackageTypePair>>
	{
		protected override StronglyTypedRegistryItem<PackageTypePairCollection<USPackageTypePair>, PackageTypePairCollection<USPackageTypePair>> GetNewRegistryItem()
		{
			return new USPackageTypePairsRegistryItem("", null, null, null, RegistryStorageFlags.System, new USPackageTypePairCollection());
		}
	}
}
