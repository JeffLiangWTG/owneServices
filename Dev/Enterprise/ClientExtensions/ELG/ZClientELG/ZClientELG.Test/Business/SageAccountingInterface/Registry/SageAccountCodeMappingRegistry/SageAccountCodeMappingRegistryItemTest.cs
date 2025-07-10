using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.ELG.Testing
{
	[TestedType(typeof(SageAccountCodeMappingRegistryItem))]
	class SageAccountCodeMappingRegistryItemTest : StronglyTypedRegistryItemTestCase<SageAccountCodeMappingRegistryBusinessObjectCollection>
	{
		protected override StronglyTypedRegistryItem<SageAccountCodeMappingRegistryBusinessObjectCollection, SageAccountCodeMappingRegistryBusinessObjectCollection> GetNewRegistryItem()
		{
			return new SageAccountCodeMappingRegistryItem("", "", "", "", RegistryStorageFlags.System);
		}
	}
}
