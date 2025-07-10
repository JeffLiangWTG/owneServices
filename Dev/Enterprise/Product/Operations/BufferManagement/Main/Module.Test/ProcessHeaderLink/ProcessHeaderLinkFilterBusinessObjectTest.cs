using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(ProcessHeaderLinkFilterBusinessObject))]
	class ProcessHeaderLinkFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Headers To and From

		public void TestHeadersToAndFrom()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3");
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow4");
			var workflow5 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow5");

			var link1_2 = workflow1.GetOrCreateDependencyLink(workflow2);
			var link2_3 = workflow2.GetOrCreateDependencyLink(workflow3);
			var link3_4 = workflow3.GetOrCreateDependencyLink(workflow4);
			var link1_3 = workflow1.GetOrCreateDependencyLink(workflow3);

			var parentChildLink = workflow4.GetOrCreateLinkToParent(workflow5);

			var bizo = new ProcessHeaderLinkFilterBusinessObject();
			var headerFromFilter = (ModuleGuidFilter)bizo[ProcessHeaderLink.ModuleFilterConstants.HeaderFrom];
			var headerToFilter = (ModuleGuidFilter)bizo[ProcessHeaderLink.ModuleFilterConstants.HeaderTo];
			var linkTypeFilter = (ModuleTextFilter)bizo[ProcessHeaderLink.ModuleFilterConstants.LinkType];

			headerFromFilter.IsActive = true;
			headerToFilter.IsActive = true;
			linkTypeFilter.IsActive = true;

			headerFromFilter.Property = workflow1.PK;
			AssertContainsExactElementsInAnyOrder(new[] { link1_2, link1_3 }, Factory.Load<ProcessHeaderLink>(bizo.Filter));

			headerToFilter.Property = workflow2.PK;
			AssertContainsExactElementsInAnyOrder(new[] { link1_2 }, Factory.Load<ProcessHeaderLink>(bizo.Filter));

			headerFromFilter.Property = workflow2.PK;
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<ProcessHeaderLink>(), Factory.Load<ProcessHeaderLink>(bizo.Filter));

			headerToFilter.Property = workflow3.PK;
			AssertContainsExactElementsInAnyOrder(new[] { link2_3 }, Factory.Load<ProcessHeaderLink>(bizo.Filter));

			headerFromFilter.Property = workflow4.PK;
			headerToFilter.Property = workflow5.PK;
			AssertContainsExactElementsInAnyOrder(new[] { parentChildLink }, Factory.Load<ProcessHeaderLink>(bizo.Filter));

			linkTypeFilter.Property = ProcessHeaderLinkTypeList.Codes.Dependency;
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<ProcessHeaderLink>(), Factory.Load<ProcessHeaderLink>(bizo.Filter));

			linkTypeFilter.Property = ProcessHeaderLinkTypeList.Codes.ParentChild;
			AssertContainsExactElementsInAnyOrder(new[] { parentChildLink }, Factory.Load<ProcessHeaderLink>(bizo.Filter));
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ProcessHeaderLinkFilterBusinessObject();
		}

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
		}

		#endregion
	}
}
