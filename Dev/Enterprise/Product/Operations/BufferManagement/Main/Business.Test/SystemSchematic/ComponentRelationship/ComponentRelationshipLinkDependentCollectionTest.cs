using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(ComponentRelationshipLinkDependentCollection))]
	class ComponentRelationshipLinkDependentCollectionTest : ActiveBusinessObjectCollectionTestCase<ComponentRelationshipLinkDependentCollection>
	{
		protected override ComponentRelationshipLinkDependentCollection GetCollectionToTest()
		{
			return new ComponentRelationshipLinkDependentCollection(BMSTestHelper.CreateComponentRelationship(Factory), BMComponentLinkSchema.FL_FC_ComponentFrom);
		}
	}
}
