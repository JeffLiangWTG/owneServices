using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Business.Test
{
	class BMComponentLinkLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestComponentFroms()
		{
			var system = Factory.New<BMSystem>();
			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket2");
			var bucket3 = BMSTestHelper.CreateBucket(system, "bucket3");
			var system2 = Factory.New<BMSystem>();
			var buffer = BMSTestHelper.CreateBuffer(system2, "buffer");

			var link = Factory.New<BMComponentLink>();
			var lookups = new BMComponentLinkLookups(link);

			AssertNotNull("Precondition", lookups.ComponentFroms);
			AssertContainsExactElementsInAnyOrder("Any component may be ComponentFrom if componentTo is not set.", new[] { bucket1, bucket2, bucket3, buffer }, lookups.ComponentFroms);

			bucket3.FC_FS_System = ZGuid.Empty;
			link.FL_FC_ComponentTo = bucket3.PK;

			AssertContainsExactElementsInAnyOrder("Any component may be ComponentFrom if componentTo's system is not set.", new[] { bucket1, bucket2, bucket3, buffer }, lookups.ComponentFroms);

			link.FL_FC_ComponentTo = bucket1.PK;

			AssertContainsExactElementsInAnyOrder("ComponentFrom must be a component from the same system as ComponentTo.", new[] { bucket1, bucket2 }, lookups.ComponentFroms);
		}

		public void TestComponentTos()
		{
			var system = Factory.New<BMSystem>();
			var bucket = BMSTestHelper.CreateBucket(system, "bucket");
			var childComponent = bucket.ChildComponents.AddNew();
			var buffer = BMSTestHelper.CreateBuffer(system, "buffer");

			var link = BMSTestHelper.LinkComponents(bucket, buffer);

			var lookups = new BMComponentLinkLookups(link);
			AssertNotNull(lookups.ComponentTos);

			AssertEquals("Should not return child components", 2, lookups.ComponentTos.Count);
			AssertContainsExactElementsInAnyOrder(new[] { bucket, buffer }, lookups.ComponentTos);

			link.ComponentToSystemPK = ZGuid.Empty;

			lookups = new BMComponentLinkLookups(link);
			AssertEquals("Should not return child components", 2, lookups.ComponentTos.Count);
			AssertContainsExactElementsInAnyOrder(new[] { bucket, buffer }, lookups.ComponentTos);
		}
	}
}
