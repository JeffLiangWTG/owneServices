using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(TestRigRegistryItem))]
	class TestRigRegistryItemTest : StronglyTypedRegistryItemTestCase<TestRigRegistryHeader>
	{
		protected override StronglyTypedRegistryItem<TestRigRegistryHeader, TestRigRegistryHeader> GetNewRegistryItem()
		{
			return new TestRigRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System);
		}
	}
}
