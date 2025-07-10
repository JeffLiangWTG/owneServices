using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Registry.Testing
{
	[TestedType(typeof(AutomatedModificationRegistryItem))]
	class AutomatedModificationRegistryItemTest : StronglyTypedRegistryItemTestCase<AutomatedModification>
	{
		protected override ZArchitecture.Environment.StronglyTypedRegistryItem<AutomatedModification, AutomatedModification> GetNewRegistryItem()
		{
			return new AutomatedModificationRegistryItem("", null, null, null, RegistryStorageFlags.Company);
		}
	}
}
