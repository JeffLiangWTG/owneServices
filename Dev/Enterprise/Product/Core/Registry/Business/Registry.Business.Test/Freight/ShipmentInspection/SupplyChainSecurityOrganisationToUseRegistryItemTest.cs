using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(SupplyChainSecurityOrganisationToUseRegistryItem))]
	sealed class SupplyChainSecurityOrganisationToUseRegistryItemTest : StronglyTypedRegistryItemTestCase<SupplyChainSecurityOrganisationToUseCollection>
	{
		protected override StronglyTypedRegistryItem<SupplyChainSecurityOrganisationToUseCollection, SupplyChainSecurityOrganisationToUseCollection> GetNewRegistryItem()
		{
			return new SupplyChainSecurityOrganisationToUseRegistryItem("Hello", (NoResString)"Hello", (NoResString)"Hello", (NoResString)"hello", RegistryStorageFlags.Company, RegistryOptions.Default, new SupplyChainSecurityOrganisationToUseCollection());
		}
	}
}
