using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(SummarizeULDSLACConfigRegistryItem))]
	sealed class SummarizeULDSLACConfigRegistryItemTest : StronglyTypedRegistryItemTestCase<SummarizeULDSLACConfigCollection>
	{
		protected override StronglyTypedRegistryItem<SummarizeULDSLACConfigCollection, SummarizeULDSLACConfigCollection> GetNewRegistryItem()
		{
			return new SummarizeULDSLACConfigRegistryItem("", null, null, null, RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.Default);
		}
	}
}
