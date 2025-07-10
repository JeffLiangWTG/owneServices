using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(ReleaseGroupGlbGroupCollection))]
	class ReleaseGroupGlbGroupCollectionTest : ActiveBusinessObjectCollectionTestCase<ReleaseGroupGlbGroupCollection>
	{
		protected override ReleaseGroupGlbGroupCollection GetCollectionToTest()
		{
			return new ReleaseGroupGlbGroupCollection(BMSTestHelper.CreateSystem(Factory));
		}
	}
}
