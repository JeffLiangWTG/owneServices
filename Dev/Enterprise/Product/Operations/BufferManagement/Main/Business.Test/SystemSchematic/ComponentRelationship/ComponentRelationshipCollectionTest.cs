using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(ComponentRelationshipCollection))]
	class ComponentRelationshipCollectionTest : ActiveBusinessObjectCollectionTestCase<ComponentRelationshipCollection>
	{
		public void TestRelationshipFilter_FilterByType()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var component = BMSTestHelper.CreateBucket(system);
			var componentRelationship = BMSTestHelper.CreateComponentRelationship(Factory);

			Factory.Save();

			var componentRelationshipCollection = new ComponentRelationshipCollection(Factory);

			AssertContainsExactElementsInAnyOrder("Collection should filter for components with type component relationships.", new[] { componentRelationship }, componentRelationshipCollection);
		}
	}
}
