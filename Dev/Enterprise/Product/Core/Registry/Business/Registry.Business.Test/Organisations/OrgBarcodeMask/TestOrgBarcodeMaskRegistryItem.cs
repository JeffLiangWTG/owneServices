using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(OrgBarcodeMaskRegistryItem))]
	sealed class TestOrgBarcodeMaskRegistryItem : StronglyTypedRegistryItemTestCase<OrgBarcodeMaskCollection>
	{
		protected override StronglyTypedRegistryItem<OrgBarcodeMaskCollection, OrgBarcodeMaskCollection> GetNewRegistryItem()
		{
			return new OrgBarcodeMaskRegistryItem("", null, null, null, RegistryStorageFlags.All);
		}
	}
}
