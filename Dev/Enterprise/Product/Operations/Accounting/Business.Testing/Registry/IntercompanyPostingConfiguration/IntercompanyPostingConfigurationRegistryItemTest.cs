using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(IntercompanyPostingConfigurationRegistryItem))]
	class IntercompanyPostingConfigurationRegistryItemTest : StronglyTypedRegistryItemTestCase<IntercompanyPostingConfigurationCollection>
	{
		protected override StronglyTypedRegistryItem<IntercompanyPostingConfigurationCollection, IntercompanyPostingConfigurationCollection> GetNewRegistryItem()
		{
			return new IntercompanyPostingConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.Company);
		}
	}
}
