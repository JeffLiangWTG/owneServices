using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business.Test;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(BMNCNShape))]
	class ShapeLinkEntityTest : ShapeLinkEntityTestCase<BMNCNShape>
	{
	}

	[TestedType(typeof(BMNCNBufferShape))]
	class BufferLinkEntityTest : ShapeLinkEntityTestCase<BMNCNBufferShape>
	{
	}

	abstract class ShapeLinkEntityTestCase<T> : LinkEntityTestCase<T>
		where T : BMNCNShape
	{
		public override void TestAgreedDeliveryDateInUtc()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = CreateWorkflow(jobHeader, "workflow");

			var diagram = NetworkTestCase.CreateDiagram(jobHeader);
			var shape = NetworkTestCase.CreateShape(workflow, diagram);

			jobHeader.FH_AgreedDeliveryDate = new ZDateTime(2014, 8, 12);
			workflow.FH_AgreedDeliveryDate = new ZDateTime(2014, 8, 13);

			AssertEquals(new ZDateTime(2014, 8, 12), ((ILinkEntity)diagram).AgreedDeliveryDateInUtc);
			AssertEquals(new ZDateTime(2014, 8, 13), ((ILinkEntity)shape).AgreedDeliveryDateInUtc);
		}

		public override void TestDisplayName()
		{
			var shape = NetworkTestCase.CreateDiagram(Factory, name: "Dat Shape");
			AssertEquals("Dat Shape", ((ILinkEntity)shape).DisplayName);
		}

		protected override ILinkEntity CreateNonLeafEntity()
		{
			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);

			return NetworkTestCase.CreateDiagram(jobHeader);
		}

		protected override ILinkEntity CreateLeafEntity(ILinkEntity parent)
		{
			var diagram = (BMNCNShape)parent;
			var jobHeader = diagram.ProcessJobHeader;
			var workflow = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow");

			return NetworkTestCase.CreateShape(workflow, diagram);
		}

		protected override ILink CreateDependencyLink(ILinkEntity prerequisite, ILinkEntity postrequisite)
		{
			var pre = (BMNCNShape)prerequisite;
			var post = (BMNCNShape)postrequisite;
			return pre.MakeVisiblePrerequisiteOf(post, pre);
		}

		protected override bool IsLeaf(ILinkEntity entity)
		{
			return ((BMNCNShape)entity).ChildShapes.Count == 0;
		}

		protected override ILinkDescendantsStrategy DescendantsStrategy
		{
			get { return new BMNCNShapeDescendantsStrategy(); }
		}
	}
}
