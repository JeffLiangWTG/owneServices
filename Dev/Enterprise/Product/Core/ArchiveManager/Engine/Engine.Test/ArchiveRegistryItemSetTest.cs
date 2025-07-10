using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ArchiveManager.Engine.Test
{
	[TestedType(typeof(ArchiveManagerDataRegistry))]
	class ArchiveRegistryItemSetTest : RegistryItemSetTestCaseWithFactory<ArchiveManagerDataRegistry>
	{
		public override void TestItemsAreAddedProperly()
		{
			base.TestItemsAreAddedProperly();
			Assert(true);
		}
	}
}
