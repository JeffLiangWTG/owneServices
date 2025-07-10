using Enterprise.Client.UPE.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Registry.Testing
{
	[TestedType(typeof(ChaseQueueValidationRegistryItem))]
	internal class ChaseQueueValidationRegistryItemTest : StronglyTypedRegistryItemTestCase<ChaseQueueValidationCollection>
	{
		protected override StronglyTypedRegistryItem<ChaseQueueValidationCollection, ChaseQueueValidationCollection> GetNewRegistryItem()
		{
			return new ChaseQueueValidationRegistryItem("Test", "Category", "Caption", "Hint");
		}

		protected new ChaseQueueValidationRegistryItem Item
		{
			get
			{
				return (ChaseQueueValidationRegistryItem)base.Item;
			}
		}
	}
}
