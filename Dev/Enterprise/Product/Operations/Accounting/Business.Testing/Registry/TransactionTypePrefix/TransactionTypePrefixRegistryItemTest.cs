using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(TransactionTypePrefixRegistryItem))]
	class TransactionTypePrefixRegistryItemTest : StronglyTypedRegistryItemTestCase<TransactionTypePrefixCollection>
	{
		protected override StronglyTypedRegistryItem<TransactionTypePrefixCollection, TransactionTypePrefixCollection> GetNewRegistryItem()
		{
			return new TransactionTypePrefixRegistryItem("TransactionTypePrefix", null, null, null, RegistryStorageFlags.Company);
		}
	}
}
