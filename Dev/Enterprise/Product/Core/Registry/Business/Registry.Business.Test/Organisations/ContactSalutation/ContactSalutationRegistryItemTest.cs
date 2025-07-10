using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ContactSalutationRegistryItem))]
	sealed class ContactSalutationRegistryItemTest : StronglyTypedRegistryItemTestCase<ContactSalutationCollection>
	{
		protected override StronglyTypedRegistryItem<ContactSalutationCollection, ContactSalutationCollection> GetNewRegistryItem()
		{
			return new ContactSalutationRegistryItem("", null, null, null, RegistryStorageFlags.System, new ContactSalutationCollection());
		}
	}
}
