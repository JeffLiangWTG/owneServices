using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(SystemDefinedOrganisationRegistryItem))]
	sealed class SystemDefinedOrganisationRegistryItemTest : StronglyTypedRegistryItemTestCase<SystemDefinedOrganisation>
	{
		protected override StronglyTypedRegistryItem<SystemDefinedOrganisation, SystemDefinedOrganisation> GetNewRegistryItem()
		{
			return new SystemDefinedOrganisationRegistryItem("", null, null, null, RegistryStorageFlags.System, typeof(MiscOrganisation));
		}
	}
}
