using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	class CloneDiagramAndAllDescendantsTest : NetworkTestCase
	{
		public void TestCloneDiagramAndAllDescendants_NoParent_ForShapes_ShouldThrowException()
		{
			AssertExceptionThrown<InvalidOperationException>("Can't clone a non-diagram without passing in the new parent. There is currently no feature for 'make a copy of this shape and descendents into its own diagram'.", () => shapeEntity.CloneDiagramAndAllDescendants());
			AssertNoExceptionThrown("It's fine to clone a shape into a new parent - that's how the 'import and copy' feature works.", () => shapeEntity.CloneShapeAndAllDescendantsIntoNewParent(diagram2, false));
		}

		public void TestCloneDiagramAndAllDescendants_NoParent_ForDiagram_ShouldNotThrowException()
		{
			AssertNoExceptionThrown("It's fine to take a copy of an entire diagram - that's how the 'clone diagram' feature works.", () => network.DiagramEntity.CloneDiagramAndAllDescendants());
		}

		public void TestCloneDiagramAndAllDescendants_WithParent_ForDiagram_ShouldNotThrowException()
		{
			AssertNoExceptionThrown("It's fine to clone an entire diagram into a new parent - that's how the 'import and copy' feature works for diagram shapes.", () => network.DiagramEntity.CloneShapeAndAllDescendantsIntoNewParent(diagram2, false));
		}

		public void TestCloneDiagramAndAllDescendants_ForApprovedDiagram_ShouldCreateCompletelyUnapprovedDiagram()
		{
			diagram1.SwitchToScaled();
			diagram1.ScheduleBizo.BNC_ScheduledStartUtc = ZDateTime.UtcNow.AddDays(1);

			var shape1 = CreateShape(diagram1, name: "Shmlonathan");
			var shape2 = CreateShape(diagram1, name: "Shmlangela");
			shape1.MakeVisiblePrerequisiteOf(shape2, diagram1);

			Factory.Save();

			var jobNetwork = CreateNetwork(diagram1);
			var viewModel = CreateNetworkViewModel(diagram1);
			var entity = jobNetwork.Entities.GetInstance(diagram1);
			var action = new ApproveDiagramAction(viewModel);
			action.Execute();
			Factory.Save();

			AssertEquals(true, diagram1.IsApproved);

			foreach (var childShape in diagram1.ChildShapes)
			{
				AssertEquals(true, childShape.IsApproved);
			}

			foreach (var attachment in diagram1.AllAttachments)
			{
				AssertEquals(true, attachment.IsApproved);
			}

			var clone = entity.CloneDiagramAndAllDescendants();

			AssertEquals("The cloned diagram should not be approved. SAD!", false, clone.IsApproved);

			foreach (var childShape in clone.ChildShapes)
			{
				AssertEquals("The cloned shapes should not be approved. SAD!", false, childShape.IsApproved);
			}

			foreach (var attachment in clone.AllAttachments)
			{
				AssertEquals("The cloned arrows should not be approved. SAD!", false, attachment.IsApproved);
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Cancel);
				jobNetwork.DeleteRelationship(attachment);
				AssertEquals("The user should not be told that the arrow is approved. SAD!", "Delete link between shapes 'Shmlonathan' and 'Shmlangela'?", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertNoExceptionThrown("The diagram should be able to be deleted without any complaining about approved arrows. SAD!", () => clone.Delete());
		}

		BMNCNShape diagram1, diagram2, shape;
		JobNetwork network;
		ShapeNetworkEntity shapeEntity;

		protected override void SetUp()
		{
			base.SetUp();

			diagram1 = CreateDiagram(Factory, name: "diagram1");
			diagram2 = CreateDiagram(Factory, name: "diagram2");
			shape = CreateShape(diagram1, name: "shape");

			network = CreateNetwork(diagram1);
			shapeEntity = network.Entities.GetInstance(shape);
		}
	}
}
