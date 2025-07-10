using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(GlowTempOrgRegistryItem))]
	sealed class GlowTempOrgRegistryItemTest : StronglyTypedRegistryItemTestCase<GlowTempOrgRequiredFieldCollection, GlowTempOrgRequiredFieldCollection>
	{
		protected override StronglyTypedRegistryItem<GlowTempOrgRequiredFieldCollection, GlowTempOrgRequiredFieldCollection> GetNewRegistryItem()
		{
			var defaultValues = new GlowTempOrgRequiredFieldCollection();
			defaultValues.Add("AAA", (NoResString)"AAA Description", enabled: true, isMandatory: true);
			defaultValues.Add("ZZZ", (NoResString)"ZZZ Description", enabled: true, isMandatory: false);

			return new GlowTempOrgRegistryItem("name", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", RegistryStorageFlags.System, RegistryOptions.Default, defaultValues);
		}
	}
}
