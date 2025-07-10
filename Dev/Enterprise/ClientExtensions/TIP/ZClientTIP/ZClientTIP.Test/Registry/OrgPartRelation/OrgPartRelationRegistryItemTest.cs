using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.TIP.Testing
{
	[TestedType(typeof(OrgPartRelationRegistryItem))]
	public class OrgPartRelationRegistryItemTest : StronglyTypedRegistryItemTestCase<OrgPartRelationRegistryBusinessObjectCollection>
	{
		protected override StronglyTypedRegistryItem<OrgPartRelationRegistryBusinessObjectCollection, OrgPartRelationRegistryBusinessObjectCollection> GetNewRegistryItem()
		{
			return new OrgPartRelationRegistryItem("", "", "", "", RegistryStorageFlags.System);
		}
	}
}
