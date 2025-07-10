using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(WorkQueueMembershipLinkCollection))]
	class WorkQueueMembershipLinkCollectionTest : ActiveBusinessObjectCollectionTestCase<WorkQueueMembershipLinkCollection>
	{
		protected override WorkQueueMembershipLinkCollection GetCollectionToTest()
		{
			return new WorkQueueMembershipLinkCollection(BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa"));
		}
	}
}
