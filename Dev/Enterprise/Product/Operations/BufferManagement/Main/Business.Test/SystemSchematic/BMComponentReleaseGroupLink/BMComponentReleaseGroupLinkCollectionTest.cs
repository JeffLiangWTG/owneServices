using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMComponentReleaseGroupLinkCollection))]
	class BMComponentReleaseGroupLinkCollectionTest : ActiveBusinessObjectCollectionTestCase<BMComponentReleaseGroupLinkCollection>
	{
		public void TestMasterRelationship_Component()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var component1 = BMSTestHelper.CreateBucket(system, "bucket1");
			var component2 = BMSTestHelper.CreateBucket(system, "bucket2");

			var link1 = component1.ReleaseGroupLinks.AddNew();
			var link2 = component2.ReleaseGroupLinks.AddNew();

			AssertEquals(1, component1.ReleaseGroupLinks.Count);
			AssertEquals(1, component2.ReleaseGroupLinks.Count);

			component1.Delete();
			AssertEquals(true, link1.IsDeleted);
			AssertEquals(false, link2.IsDeleted);
		}

		public void TestMasterRelationship_ReleaseGroup()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var component1 = BMSTestHelper.CreateBucket(system);

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			var group2 = Factory.NewWithValidTestData<GlbGroup>();

			var releaseGroup1 = BMSTestHelper.CreateReleaseGroup(system, group1);
			var releaseGroup2 = BMSTestHelper.CreateReleaseGroup(system, group2);

			var link1 = releaseGroup1.ComponentLinks.AddNew();
			var link2 = releaseGroup2.ComponentLinks.AddNew();

			AssertEquals(1, releaseGroup1.ComponentLinks.Count);
			AssertEquals(1, releaseGroup2.ComponentLinks.Count);

			releaseGroup1.Delete();
			AssertEquals(true, link1.IsDeleted);
			AssertEquals(false, link2.IsDeleted);
		}

		protected override BMComponentReleaseGroupLinkCollection GetCollectionToTest()
		{
			return new BMComponentReleaseGroupLinkCollection(Factory, new ZQuery());
		}
	}
}
