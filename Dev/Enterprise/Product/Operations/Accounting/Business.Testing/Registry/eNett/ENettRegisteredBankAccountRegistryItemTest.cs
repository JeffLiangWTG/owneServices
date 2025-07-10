using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ENettRegisteredBankAccountRegistryItem))]
	class ENettRegisteredBankAccountRegistryItemTest : StronglyTypedRegistryItemTestCase<ENettRegisteredBankAccountCollection>
	{
		protected override StronglyTypedRegistryItem<ENettRegisteredBankAccountCollection, ENettRegisteredBankAccountCollection> GetNewRegistryItem()
		{
			return new ENettRegisteredBankAccountRegistryItem("", null, null, null, RegistryStorageFlags.Company);
		}
	}
}
