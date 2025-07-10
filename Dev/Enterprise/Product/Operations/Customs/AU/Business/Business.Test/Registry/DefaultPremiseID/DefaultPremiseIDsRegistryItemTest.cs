using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(DefaultPremiseIDsRegistryItem))]
	sealed class DefaultPremiseIDsRegistryItemTest : StronglyTypedRegistryItemTestCase<DefaultPremiseIDCollection>
	{
		protected override StronglyTypedRegistryItem<DefaultPremiseIDCollection, DefaultPremiseIDCollection> GetNewRegistryItem()
		{
			return new DefaultPremiseIDsRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
