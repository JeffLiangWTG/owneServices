using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(DirectDebitFileCreationURLRegistryItem))]
	class DirectDebitFileCreationURLRegistryItemTest : StronglyTypedRegistryItemTestCase<DirectDebitFileCreationURLCollection>
	{
		protected override StronglyTypedRegistryItem<DirectDebitFileCreationURLCollection, DirectDebitFileCreationURLCollection> GetNewRegistryItem()
		{
			return new DirectDebitFileCreationURLRegistryItem("", null, null, null, RegistryStorageFlags.Company);
		}
	}
}
