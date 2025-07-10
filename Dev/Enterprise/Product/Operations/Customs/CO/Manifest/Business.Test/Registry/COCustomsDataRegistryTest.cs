
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CO.Manifest.Business.Testing
{
	[TestedType(typeof(COCustomsDataRegistry))]
	sealed class COCustomsDataRegistryTest : RegistryItemSetTestCaseWithFactory<COCustomsDataRegistry>
	{
		public void TestEnableCOManifests()
		{
			TestRegistryItem(
				ItemSet.EnableCOManifests,
				"EnableCOManifests",
				COCustomsDataRegistry.Categories.Customs_Colombia,
				"Enable Colombia Manifest",
				"Enable Colombia Manifest?",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
				false);
		}
	}
}
