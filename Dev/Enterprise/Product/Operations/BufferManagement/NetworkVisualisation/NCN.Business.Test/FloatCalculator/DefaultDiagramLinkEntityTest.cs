using System.Linq;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(BMNCNShapeDefaultDiagram))]
	class DefaultDiagramLinkEntityTest : LinkEntityTestCase<BMNCNShapeDefaultDiagram>
	{
		public override void TestAgreedDeliveryDateInUtc()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var diagram = jobHeader.GetDefaultDiagram();
			jobHeader.FH_AgreedDeliveryDate = new ZDateTime(2014, 8, 12);

			AssertEquals(new ZDateTime(2014, 8, 12), ((ILinkEntity)diagram).AgreedDeliveryDateInUtc);
		}

		public override void TestDisplayName()
		{
			var entity = CreateNonLeafEntity();
			AssertEquals("Job Organization (MAIORGSYD) is complete.", entity.DisplayName);
		}

		protected override ILinkEntity CreateNonLeafEntity()
		{
			var job = Factory.NewWithValidTestData<OrgHeader>();
			job.OH_Code = "MAIORGSYD";
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);

			return jobHeader.GetDefaultDiagram();
		}

		protected override ILinkEntity CreateLeafEntity(ILinkEntity parent)
		{
			var diagram = (BMNCNShapeDefaultDiagram)parent;
			var jobHeader = diagram.ProcessJobHeader;
			var workflow = jobHeader.ProcessHeaders.AddNew();

			diagram.UpdateDefaultNetwork();

			return ((BMNCNShapeDefaultDiagram)parent).ChildShapes[0];
		}

		protected override ILink CreateDependencyLink(ILinkEntity prerequisite, ILinkEntity postrequisite)
		{
			var preShape = (BMNCNShape)prerequisite;
			var postShape = (BMNCNShape)postrequisite;
			var parent = postShape.Parents(new BMNCNShapeDescendantsStrategy()).Cast<BMNCNShape>().FirstOrDefault() ?? preShape.Parents(new BMNCNShapeDescendantsStrategy()).Cast<BMNCNShape>().FirstOrDefault();
			return preShape.MakeVisiblePrerequisiteOf(postShape, parent);
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
