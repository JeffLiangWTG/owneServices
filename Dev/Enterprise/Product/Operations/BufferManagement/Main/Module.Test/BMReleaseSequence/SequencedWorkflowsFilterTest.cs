using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(SequencedWorkflowsFilter))]
	class SequencedWorkflowsFilterTest : ModuleFilterTestCase<SequencedWorkflowsFilter>
	{
		public void TestFilter_WhenDirectlySequencedFilterIsApplied()
		{
			var bizo = new ProcessHeaderFilterBusinessObject();
			var filter = bizo.AddFilterStrip<SequencedWorkflowsFilter>(ProcessHeader.ModuleFilterConstants.SequencedWorkflows);
			filter.Property = SequencedWorkflowsList.Codes.DirectlySequenced;

			var results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder("All of the workflows that are directly sequenced.", new ProcessHeader[] { DirectlySequencedWorkflow }, results);
		}

		public void TestFilter_WhenIndirectlySequencedFilterIsApplied()
		{
			var bizo = new ProcessHeaderFilterBusinessObject();
			var filter = bizo.AddFilterStrip<SequencedWorkflowsFilter>(ProcessHeader.ModuleFilterConstants.SequencedWorkflows);
			filter.Property = SequencedWorkflowsList.Codes.IndirectlySequenced;

			var results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder("All of the workflows that are indirectly sequenced.", new ProcessHeader[] { IndirectlySequencedWorkflowRelatedToJobWorkflow, IndirectlySequencedChildWorkflow, ChildOfAChildWorkflow, IndirectlySequencedJobWorkflow, WorkflowOfIndirectlySequencedJobWorkflow }, results);
		}

		public void TestFilter_WhenBothDirectlyAndIndirectlySequencedFilterIsApplied()
		{
			var bizo = new ProcessHeaderFilterBusinessObject();
			var filter = bizo.AddFilterStrip<SequencedWorkflowsFilter>(ProcessHeader.ModuleFilterConstants.SequencedWorkflows);
			filter.Property = SequencedWorkflowsList.Codes.BothDirectlyAndIndirectlySequenced;

			var results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder("All of the workflows that are both directly and indirectly sequenced.", new ProcessHeader[] { DirectlySequencedWorkflow, IndirectlySequencedWorkflowRelatedToJobWorkflow, IndirectlySequencedChildWorkflow, ChildOfAChildWorkflow, IndirectlySequencedJobWorkflow, WorkflowOfIndirectlySequencedJobWorkflow }, results);
		}

		public void TestFilter_WhenNotSequencedFilterIsApplied()
		{
			var bizo = new ProcessHeaderFilterBusinessObject();
			var filter = bizo.AddFilterStrip<SequencedWorkflowsFilter>(ProcessHeader.ModuleFilterConstants.SequencedWorkflows);
			filter.Property = SequencedWorkflowsList.Codes.NotSequenced;

			var results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder("All of the workflows that are not sequenced.", new ProcessHeader[] { NonSequencedWorkflow, NonSequencedJobWorkflow, DirectlySequencedInactiveWorkflow }, results);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			DirectlySequencedWorkflow = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "direct");
			DirectlySequencedInactiveWorkflow = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "inactive");
			NonSequencedJobWorkflow = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "another");
			IndirectlySequencedJobWorkflow = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "indirectjob");
			WorkflowOfIndirectlySequencedJobWorkflow = BMSTestHelper.CreateWorkflow(IndirectlySequencedJobWorkflow, "wow");
			IndirectlySequencedWorkflowRelatedToJobWorkflow = BMSTestHelper.CreateWorkflow(DirectlySequencedWorkflow, "indirect");
			IndirectlySequencedChildWorkflow = BMSTestHelper.CreateWorkflow(NonSequencedJobWorkflow, "child");
			ChildOfAChildWorkflow = BMSTestHelper.CreateWorkflow(NonSequencedJobWorkflow, "childofachild");
			NonSequencedWorkflow = BMSTestHelper.CreateWorkflow(NonSequencedJobWorkflow, "not");

			BMSTestHelper.CreateParentChildLink(DirectlySequencedWorkflow, IndirectlySequencedChildWorkflow);
			BMSTestHelper.CreateParentChildLink(DirectlySequencedWorkflow, IndirectlySequencedJobWorkflow);
			BMSTestHelper.CreateParentChildLink(IndirectlySequencedChildWorkflow, ChildOfAChildWorkflow);

			var activeGroup = Factory.New<GlbGroup>();
			activeGroup.GG_Code = "ACTGRP";

			var inactiveGroup = Factory.New<GlbGroup>();
			inactiveGroup.GG_Code = "INAGRP";

			var activeSequence = BMSTestHelper.CreateReleaseSequence(Factory, activeGroup.PK, "Active Sequence");
			BMSTestHelper.CreateReleaseSequenceItem(activeSequence, DirectlySequencedWorkflow);

			var inactiveSequence = BMSTestHelper.CreateReleaseSequence(Factory, inactiveGroup.PK, "Inactive Sequence", isActive: false);
			BMSTestHelper.CreateReleaseSequenceItem(inactiveSequence, DirectlySequencedInactiveWorkflow);

			Factory.Save();
		}

		ProcessJobHeader DirectlySequencedWorkflow;
		ProcessJobHeader DirectlySequencedInactiveWorkflow;
		ProcessJobHeader NonSequencedJobWorkflow;
		ProcessJobHeader IndirectlySequencedJobWorkflow;
		ProcessHeader ChildOfAChildWorkflow;
		ProcessHeader IndirectlySequencedWorkflowRelatedToJobWorkflow;
		ProcessHeader IndirectlySequencedChildWorkflow;
		ProcessHeader NonSequencedWorkflow;
		ProcessHeader WorkflowOfIndirectlySequencedJobWorkflow;

		#endregion

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;
		protected override ZString ExpectedDescription => "Sequenced Workflows";

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override SequencedWorkflowsFilter GetNewModuleFilter()
		{
			return new SequencedWorkflowsFilter();
		}
	}
}
