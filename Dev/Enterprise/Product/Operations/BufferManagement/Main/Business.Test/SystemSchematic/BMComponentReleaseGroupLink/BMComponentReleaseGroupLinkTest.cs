using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMComponentReleaseGroupLink))]
	class BMComponentReleaseGroupLinkTest : EnterpriseBusinessObjectTestCase
	{
		public void TestClone()
		{
			var component = Factory.New<BMComponent>();

			var glbGroup = Factory.New<GlbGroup>();

			var link = Factory.New<BMComponentReleaseGroupLink>();

			link.FO_GG_ReleaseGroup = glbGroup.PK;
			link.FO_FC_Component = component.PK;

			var clone = (BMComponentReleaseGroupLink)link.Clone();

			AssertEquals(link.FO_GG_ReleaseGroup, clone.FO_GG_ReleaseGroup);
			AssertEquals(link.FO_FC_Component, clone.FO_FC_Component);
		}

		public void TestAutoAssignTaskAge_Readonly()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system, "buffer", 200);
			var bucket = BMSTestHelper.CreateBucket(system, "bucket", sequence: 1);

			var glbGroup = Factory.New<GlbGroup>();

			var bufferLink = Factory.New<BMComponentReleaseGroupLink>();
			var bucketLink = Factory.New<BMComponentReleaseGroupLink>();

			bufferLink.FO_GG_ReleaseGroup = glbGroup.PK;
			bufferLink.FO_FC_Component = buffer.PK;

			bucketLink.FO_GG_ReleaseGroup = glbGroup.PK;
			bucketLink.FO_FC_Component = bucket.PK;

			AssertEquals(bucketLink.FO_AutoAssignTasksAgeInfo.ReadOnly, true);
			AssertEquals(bufferLink.FO_AutoAssignTasksAgeInfo.ReadOnly, false);
		}
	}
}
