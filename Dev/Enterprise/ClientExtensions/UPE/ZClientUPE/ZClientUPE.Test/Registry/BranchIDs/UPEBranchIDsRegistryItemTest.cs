using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Registry.Testing
{
	[TestedType(typeof(UPEBranchIDsRegistryItem))]
	class UPEBranchIDsRegistryItemTest : StronglyTypedRegistryItemTestCase<UPEBranchIDsRegistryObjectCollection>
	{
		protected override StronglyTypedRegistryItem<UPEBranchIDsRegistryObjectCollection, UPEBranchIDsRegistryObjectCollection> GetNewRegistryItem()
		{
			return new UPEBranchIDsRegistryItem("", null, null, RegistryStorageFlags.System, new UPEBranchIDsRegistryObjectCollection(), null);
		}
	}
}
