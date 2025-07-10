using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AmountOrPercentageBasedThreeLevelAuthorisationRequirementRegistryItem))]
	sealed class AmountOrPercentageBasedThreeLevelAuthorisationRequirementRegistryItemTest : StronglyTypedRegistryItemTestCase<AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection>
	{
		protected override StronglyTypedRegistryItem<AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection, AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection> GetNewRegistryItem()
		{
			return new AmountOrPercentageBasedThreeLevelAuthorisationRequirementRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
