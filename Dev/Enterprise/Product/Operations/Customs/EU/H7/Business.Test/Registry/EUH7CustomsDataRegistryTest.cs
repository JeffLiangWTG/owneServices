using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.ZArchitecture.Environment.RegistryItemSet;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	[TestedType(typeof(EUH7CustomsDataRegistry))]
	class EUH7CustomsDataRegistryTest : RegistryItemSetTestCaseWithFactory<EUH7CustomsDataRegistry>
	{
		public void TestIsForProductivityWise()
		{
			AssertEquals(false, EUH7CustomsDataRegistry.Instance.IsForProductivityWise);
		}

		public void TestEUH7JobNumberCustomization()
		{
			TestRegistryItem(ItemSet.EUH7JobNumberCustomization,
							"EUH7JobNumberCustomization",
							RawDataRegistry.Categories.Customs_EuropeanUnionCommon_H7,
							"Low Value (H7) Job Number Customization",
							"Override this value to customize how low value (H7) job numbers are formatted.",
							RegistryStorageFlags.Company,
							RegistryOptions.IsOnlyForDevelopers,
							(x) =>
							{
								AssertEquals(NumberCustomisationElementCategories.Standard, x.Categories);
								AssertEquals("Low Value (H7) Job Number", x.GeneratedNumberName);
								AssertEquals(AsycudaManifestHeaderSchema.AMA_JobReference.MaxLength, x.MaxLength);
							});
			AssertSequencesEqual("Should Only Show EU Companies.", CountryFilterPKs.EuropeanUnion, EUH7CustomsDataRegistry.Instance.EUH7JobNumberCustomization.CountryFilterPKs);
		}
	}
}
