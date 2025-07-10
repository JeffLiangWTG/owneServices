using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(AsycudaCustomsDataRegistry))]
	class AsycudaCustomsDataRegistryTest : RegistryItemSetTestCaseWithFactory<AsycudaCustomsDataRegistry>
	{
		public void TestIsForProductivityWise()
		{
			AssertEquals(false, ItemSet.IsForProductivityWise);
		}

		public override void TestItemsAreAddedProperly()
		{
			base.TestItemsAreAddedProperly();
			Assert(true);
		}
	}
}
