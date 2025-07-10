using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(BranchCollectionRegistryItem))]
	sealed class BranchCollectionRegistryItemTest : StronglyTypedRegistryItemTestCase<BranchProxyMaster>
	{
		protected override StronglyTypedRegistryItem<BranchProxyMaster, BranchProxyMaster> GetNewRegistryItem()
		{
			return new BranchCollectionRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
