using CargoWise.Types;
using Enterprise.Client.UPE.Registry.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Registry.Testing
{
	[TestedType(typeof(UPEGlbGroupsRegistryItem))]
	class UPEGlbGroupsRegistryItemTest : StronglyTypedRegistryItemTestCase<UPEGlbGroupsRegistryObjectCollection>
	{
		protected override StronglyTypedRegistryItem<UPEGlbGroupsRegistryObjectCollection, UPEGlbGroupsRegistryObjectCollection> GetNewRegistryItem()
		{
			return new UPEGlbGroupsRegistryItem(ZString.Empty, null, null, RegistryStorageFlags.System, new UPEGlbGroupsRegistryObjectCollection());
		}
	}
}
