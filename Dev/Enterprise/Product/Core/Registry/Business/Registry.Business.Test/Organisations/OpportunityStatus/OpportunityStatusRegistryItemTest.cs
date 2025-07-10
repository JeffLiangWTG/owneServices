using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(OpportunityStatusRegistryItem))]
	sealed class OpportunityStatusRegistryItemTest : StronglyTypedRegistryItemTestCase<OpportunityStatusCollection>
	{
		protected override StronglyTypedRegistryItem<OpportunityStatusCollection, OpportunityStatusCollection> GetNewRegistryItem()
		{
			var defaultStatuses = new OpportunityStatusCollection();
			defaultStatuses.Add("AAA", (NoResString)"AAA Description", true);
			defaultStatuses.Add("ZZZ", (NoResString)"ZZZ Description", false);
			return new OpportunityStatusRegistryItem("name", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", RegistryStorageFlags.System, defaultStatuses);
		}
	}
}
