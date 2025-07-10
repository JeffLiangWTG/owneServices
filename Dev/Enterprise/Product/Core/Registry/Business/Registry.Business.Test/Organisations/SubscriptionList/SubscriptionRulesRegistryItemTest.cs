using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(SubscriptionRulesRegistryItem))]
	sealed class SubscriptionRulesRegistryItemTest : StronglyTypedRegistryItemTestCase<SubscriptionRuleCollection>
	{
		protected override StronglyTypedRegistryItem<SubscriptionRuleCollection, SubscriptionRuleCollection> GetNewRegistryItem()
		{
			return new SubscriptionRulesRegistryItem("", null, null, null, RegistryStorageFlags.Company);
		}
	}
}
