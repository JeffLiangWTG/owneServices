using System;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Business.Test;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	abstract class LinkedEntityActionTestCase<TAction> : JobNetworkActionTestCase<TAction> where TAction : JobNetworkAction
	{
		#region Main Properties

		protected void AssertGetName(string nameForUnlinked, string nameForLinkedToProcessHeader, string nameForLinkedToDiagram)
		{
			AssertProperty(nameForUnlinked, nameForLinkedToProcessHeader, nameForLinkedToDiagram, (action, shape) => action.GetNameAfterActivatingEntity_ForTest(shape));
		}

		protected void AssertGetDescription(string descriptionForUnlinked, string descriptionForProcessHeader, string descriptionForShape)
		{
			AssertProperty(descriptionForUnlinked, descriptionForProcessHeader, descriptionForShape, (action, shape) => action.GetDescriptionAfterActivatingEntity_ForTest(shape));
		}

		protected void AssertProperty<TValue>(string valueForUnlinked, string valueForLinkedToProcessHeader, string valueForLinkedToDiagram, Func<TAction, BMNCNShape, TValue> valueGetter)
		{
			AssertEquals("Non linked diagram", valueForUnlinked, valueGetter(NonLinkedDiagramTestConfig.ActionForNormalDiagram, NonLinkedDiagramTestConfig.NormalDiagram));
			AssertEquals("Non linked shape", valueForUnlinked, valueGetter(NonLinkedDiagramTestConfig.ActionForNormalDiagram, NonLinkedDiagramTestConfig.NonLinkedShape));

			AssertEquals("Precondition", ProcessHeaderSchema.Constants.Prefix, DiagramLinkedToJobHeaderTestConfig.NormalDiagram.BNS_RelatedEntityTableCode);
			AssertEquals("Precondition", ProcessHeaderSchema.Constants.Prefix, DiagramLinkedToJobHeaderTestConfig.ShapeLinkedToProcessHeader.BNS_RelatedEntityTableCode);
			AssertEquals("Diagram linked to job header", valueForLinkedToProcessHeader, valueGetter(DiagramLinkedToJobHeaderTestConfig.ActionForNormalDiagram, DiagramLinkedToJobHeaderTestConfig.NormalDiagram));
			AssertEquals("Shape linked to process header", valueForLinkedToProcessHeader, valueGetter(DiagramLinkedToJobHeaderTestConfig.ActionForNormalDiagram, DiagramLinkedToJobHeaderTestConfig.ShapeLinkedToProcessHeader));

			AssertEquals("Precondition", BMNCNShapeSchema.Constants.Prefix, DiagramLinkedToDiagramTestConfig.NormalDiagram.BNS_RelatedEntityTableCode);
			AssertEquals("Precondition", BMNCNShapeSchema.Constants.Prefix, DiagramLinkedToDiagramTestConfig.ShapeLinkedToDiagram.BNS_RelatedEntityTableCode);
			AssertEquals("Diagram linked to diagram", valueForLinkedToDiagram, valueGetter(DiagramLinkedToDiagramTestConfig.ActionForNormalDiagram, DiagramLinkedToDiagramTestConfig.NormalDiagram));
			AssertEquals("Shape linked to shape", valueForLinkedToDiagram, valueGetter(DiagramLinkedToDiagramTestConfig.ActionForNormalDiagram, DiagramLinkedToDiagramTestConfig.ShapeLinkedToDiagram));
		}

		#endregion

		#region Accessibility

		public void TestCannotExecuteInWorkflowRelationshipDesigner()
		{
			AssertCannotExecuteInWorkflowRelationshipDesigner();
		}

		public void TestNotApplicableToBuffersAndAnnotations()
		{
			AssertNotApplicableToBuffers();
			AssertNotApplicableToAnnotations();
		}

		protected void AssertApplicableToShape_WhenItIsLinkedOnly()
		{
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("The shape should be linked to an entity", NonLinkedDiagramTestConfig.ActionForNormalDiagram.IsApplicableAfterActivatingEntity_ForTest(NonLinkedDiagramTestConfig.NonLinkedShape));
			NetworkActionAccessibilityTest.AssertAllowed(DiagramLinkedToDiagramTestConfig.ActionForNormalDiagram.IsApplicableAfterActivatingEntity_ForTest(DiagramLinkedToDiagramTestConfig.ShapeLinkedToDiagram));
			NetworkActionAccessibilityTest.AssertAllowed(DiagramLinkedToJobHeaderTestConfig.ActionForNormalDiagram.IsApplicableAfterActivatingEntity_ForTest(DiagramLinkedToJobHeaderTestConfig.ShapeLinkedToProcessHeader));
		}

		protected void AssertApplicableToDiagram_WhenItIsLinkedOnly()
		{
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("The shape should be linked to an entity", NonLinkedDiagramTestConfig.ActionForNormalDiagram.IsApplicableAfterActivatingEntity_ForTest(NonLinkedDiagramTestConfig.NormalDiagram));
			NetworkActionAccessibilityTest.AssertAllowed(DiagramLinkedToDiagramTestConfig.ActionForNormalDiagram.IsApplicableAfterActivatingEntity_ForTest(DiagramLinkedToDiagramTestConfig.NormalDiagram));
			NetworkActionAccessibilityTest.AssertAllowed(DiagramLinkedToJobHeaderTestConfig.ActionForNormalDiagram.IsApplicableAfterActivatingEntity_ForTest(DiagramLinkedToJobHeaderTestConfig.NormalDiagram));
		}

		protected void AssertEnabledToLinkedShape()
		{
			NetworkActionAccessibilityTest.AssertAllowed(DiagramLinkedToDiagramTestConfig.ActionForNormalDiagram.IsEnabledAfterActivatingEntity_ForTest(DiagramLinkedToDiagramTestConfig.ShapeLinkedToDiagram));
			NetworkActionAccessibilityTest.AssertAllowed(DiagramLinkedToJobHeaderTestConfig.ActionForNormalDiagram.IsEnabledAfterActivatingEntity_ForTest(DiagramLinkedToJobHeaderTestConfig.ShapeLinkedToProcessHeader));
		}

		protected void AssertEnabledToLinkedDiagram()
		{
			NetworkActionAccessibilityTest.AssertAllowed(DiagramLinkedToDiagramTestConfig.ActionForNormalDiagram.IsEnabledAfterActivatingEntity_ForTest(DiagramLinkedToDiagramTestConfig.NormalDiagram));
			NetworkActionAccessibilityTest.AssertAllowed(DiagramLinkedToJobHeaderTestConfig.ActionForNormalDiagram.IsEnabledAfterActivatingEntity_ForTest(DiagramLinkedToJobHeaderTestConfig.NormalDiagram));
		}

		#endregion

		#region Setup and Tear Down

		protected LinkedEntityActionTestConfig NonLinkedDiagramTestConfig;
		protected LinkedEntityActionTestConfig DiagramLinkedToJobHeaderTestConfig;
		protected LinkedEntityActionTestConfig DiagramLinkedToDiagramTestConfig;

		protected override void SetUp()
		{
			base.SetUp();

			NonLinkedDiagramTestConfig = new LinkedEntityActionTestConfig(Factory, GetAction);
			DiagramLinkedToJobHeaderTestConfig = new LinkedEntityActionTestConfig(Factory, GetAction);
			DiagramLinkedToDiagramTestConfig = new LinkedEntityActionTestConfig(Factory, GetAction);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			DiagramLinkedToJobHeaderTestConfig.NetworkForNormalDiagram.LinkEntity(DiagramLinkedToJobHeaderTestConfig.NormalDiagram, jobHeader);

			DiagramLinkedToDiagramTestConfig.NetworkForNormalDiagram.LinkEntity(DiagramLinkedToDiagramTestConfig.NormalDiagram, NonLinkedDiagramTestConfig.NormalDiagram);
		}

		#endregion

		protected class LinkedEntityActionTestConfig : ActionTestingSetup
		{
			public LinkedEntityActionTestConfig(BusinessObjectFactory factory, Func<NetworkViewModel, TAction> actionGetter)
				: base(factory, actionGetter)
			{
				var anotherDiagramConfig = new ActionTestingSetup(factory, actionGetter);

				var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(factory);

				NonLinkedShape = CreateShape(NormalDiagram, name: "NonLinkedShape");
				ShapeLinkedToProcessHeader = CreateShape(NormalDiagram, name: "ShapeLinkedToProcessHeader");
				ShapeLinkedToDiagram = CreateShape(NormalDiagram, name: "ShapeLinkedToProcessHeader");

				NetworkForNormalDiagram.LinkEntity(ShapeLinkedToProcessHeader, jobHeader.ProcessHeaders[0]);
				NetworkForNormalDiagram.LinkEntity(ShapeLinkedToDiagram, anotherDiagramConfig.NormalDiagram);

				NetworkViewModelForNormalDiagram.Refresh();
			}

			public BMNCNShape NonLinkedShape { get; }
			public BMNCNShape ShapeLinkedToProcessHeader { get; }
			public BMNCNShape ShapeLinkedToDiagram { get; }
		}
	}
}
