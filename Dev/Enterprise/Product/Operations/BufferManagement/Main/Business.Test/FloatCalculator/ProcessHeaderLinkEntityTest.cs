using System.Linq;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(ProcessHeader))]
	class ProcessHeaderLinkEntityTest : LinkEntityTestCase<ProcessHeader>
	{
		public void TestParents_ForChildWorkflow()
		{
			var jobHeader = CreateJobHeader<DummyWithWorkflow>();
			var parentWorkflow = CreateWorkflow(jobHeader, "parentWorkflow");
			var childWorkflow = CreateWorkflow(jobHeader, "childWorkflow");
			var grandchildWorkflow = CreateWorkflow(jobHeader, "grandchildWorkflow");

			childWorkflow.GetOrCreateLinkToParent(parentWorkflow);
			grandchildWorkflow.GetOrCreateLinkToParent(childWorkflow);

			AssertContainsExactElementsInAnyOrder(new[] { jobHeader }, parentWorkflow.Parents(DescendantsStrategy));
			AssertContainsExactElementsInAnyOrder(new[] { parentWorkflow }, childWorkflow.Parents(DescendantsStrategy));
			AssertContainsExactElementsInAnyOrder(new[] { childWorkflow }, grandchildWorkflow.Parents(DescendantsStrategy));
		}

		public void TestChildren_ForParentWorkflowWithChildren()
		{
			var job = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);
			var parentWorkflow1 = CreateWorkflow(jobHeader, "parentWorkflow1");
			var parentWorkflow2 = CreateWorkflow(jobHeader, "parentWorkflow2");
			var childWorkflow1 = CreateWorkflow(jobHeader, "childWorkflow1");
			var childWorkflow2 = CreateWorkflow(jobHeader, "childWorkflow2");
			var grandchildWorkflow1 = CreateWorkflow(jobHeader, "grandchildWorkflow1");
			var grandchildWorkflow2 = CreateWorkflow(jobHeader, "grandchildWorkflow2");

			childWorkflow1.GetOrCreateLinkToParent(parentWorkflow1);
			childWorkflow2.GetOrCreateLinkToParent(parentWorkflow1);

			grandchildWorkflow1.GetOrCreateLinkToParent(childWorkflow1);
			grandchildWorkflow2.GetOrCreateLinkToParent(childWorkflow1);

			AssertContainsExactElementsInAnyOrder(new[] { parentWorkflow1, parentWorkflow2 }, jobHeader.Children(DescendantsStrategy));
			AssertContainsExactElementsInAnyOrder(new[] { childWorkflow1, childWorkflow2 }, parentWorkflow1.Children(DescendantsStrategy));
			AssertContainsExactElementsInAnyOrder(new[] { grandchildWorkflow1, grandchildWorkflow2 }, childWorkflow1.Children(DescendantsStrategy));
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<ILinkEntity>(), childWorkflow2.Children(DescendantsStrategy));
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<ILinkEntity>(), grandchildWorkflow1.Children(DescendantsStrategy));
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<ILinkEntity>(), grandchildWorkflow2.Children(DescendantsStrategy));
		}

		public override void TestAgreedDeliveryDateInUtc()
		{
			var jobHeader = CreateJobHeader<DummyWithWorkflow>();
			var workflow = CreateWorkflow(jobHeader, "workflow");

			jobHeader.FH_AgreedDeliveryDate = new ZDateTime(2014, 8, 12);
			workflow.FH_AgreedDeliveryDate = new ZDateTime(2014, 8, 13);

			AssertEquals(new ZDateTime(2014, 8, 12), ((ILinkEntity)jobHeader).AgreedDeliveryDateInUtc);
			AssertEquals(new ZDateTime(2014, 8, 13), ((ILinkEntity)workflow).AgreedDeliveryDateInUtc);
		}

		public override void TestDisplayName()
		{
			var job = Factory.NewWithValidTestData<OrgHeader>();
			job.OH_Code = "MAIORGSYD";

			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow = CreateWorkflow(jobHeader, "workflow");

			AssertEquals("Job Organization (MAIORGSYD) is complete.", ((ILinkEntity)jobHeader).DisplayName);
			AssertEquals("workflow", ((ILinkEntity)workflow).DisplayName);
		}

		protected override bool IsLeaf(ILinkEntity entity)
		{
			return !((ProcessHeader)entity).ChildHeaders.Any();
		}

		protected override ILinkEntity CreateNonLeafEntity()
		{
			return ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory, addDefaultProcessHeaderIfNone: false);
		}

		protected override ILinkEntity CreateLeafEntity(ILinkEntity parent)
		{
			return CreateWorkflow((ProcessJobHeader)parent, "workflow");
		}

		protected override ILink CreateDependencyLink(ILinkEntity prerequisite, ILinkEntity postrequisite)
		{
			return ((ProcessHeader)prerequisite).GetOrCreateDependencyLink((ProcessHeader)postrequisite);
		}

		protected override ILinkDescendantsStrategy DescendantsStrategy
		{
			get { return new ProcessHeaderDescendantsStrategy(); }
		}
	}
}
