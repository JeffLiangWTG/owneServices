using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(SalesRelationDirectionRulesRegistryItem))]
	sealed class SalesRelationDirectionRulesRegistryItemTest : StronglyTypedRegistryItemTestCase<SalesRelationDirectionRuleCollection>
	{
		protected override StronglyTypedRegistryItem<SalesRelationDirectionRuleCollection, SalesRelationDirectionRuleCollection> GetNewRegistryItem()
		{
			return new SalesRelationDirectionRulesRegistryItem("", null, null, null, RegistryStorageFlags.Company, new SalesRelationDirectionRuleCollection());
		}
	}
}
