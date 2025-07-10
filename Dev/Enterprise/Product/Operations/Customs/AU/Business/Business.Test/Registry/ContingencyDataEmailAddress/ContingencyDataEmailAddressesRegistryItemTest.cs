using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(ContingencyDataEmailAddressesRegistryItem))]
	sealed class ContingencyDataEmailAddressesRegistryItemTest : StronglyTypedRegistryItemTestCase<ContingencyDataEmailAddressCollection>
	{
		protected override StronglyTypedRegistryItem<ContingencyDataEmailAddressCollection, ContingencyDataEmailAddressCollection> GetNewRegistryItem()
		{
			return new ContingencyDataEmailAddressesRegistryItem("", null, null, null, RegistryStorageFlags.System, new ContingencyDataEmailAddressCollection());
		}
	}
}
