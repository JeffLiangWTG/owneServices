using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(RelatedDiagramViewCollection))]
	class RelatedDiagramViewCollectionTest : NonPersistentBusinessObjectCollectionTestCase<RelatedDiagramViewCollection>
	{
		[TestDate(2017, 6, 1)]
		public void TestPopulateCollectionAndProperties()
		{
			var diagramShape = NetworkTestCase.CreateDiagram(Factory, name: "Test Diagram");
			var shape1 = NetworkTestCase.CreateShape(diagramShape);
			var shape2 = NetworkTestCase.CreateShape(diagramShape);

			var subDiagram = NetworkTestCase.CreateShape(diagramShape);
			var shapeInSubDiagram2 = NetworkTestCase.CreateShape(subDiagram);

			var mocks = new MockRepository(MockBehavior.Default);
			var controller = NetworkTestCase.CreateMockableController(mocks);
			var controllerForScaledDiagram = NetworkTestCase.CreateMockableController(mocks);

			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagramShape, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();

			var action = new SwitchToScaledModeAction(networkViewModel);
			Factory.Save();

			action.ExecuteForEntityWithoutAccessCheck(diagramShape);

			var scaledDiagram = action.FactoryForSpawnedNetwork_ExposedForTest.LoadTop1<BMNCNShape>(new ZQuery(BMNCNShapeSchema.BNS_Name, "Test Diagram (scaled copy)"));
			scaledDiagram.ScheduledStartTimeLocal = ZDateTime.Now;
			scaledDiagram.ScheduledFinishTimeLocal = ZDateTime.Now.AddMonths(2);

			var networkViewModelForScaledDiagram = NetworkTestCase.CreateNetworkViewModel(scaledDiagram, controller: controllerForScaledDiagram.Object);
			var networkForScaledDiagram = networkViewModelForScaledDiagram.GetJobNetwork();

			new ApproveDiagramAction(networkViewModelForScaledDiagram).ExecuteForEntityWithoutAccessCheck(scaledDiagram);
			action.FactoryForSpawnedNetwork_ExposedForTest.Save();

			AssertEquals("Should be one related diagram for 'diagramShape' - scaled copy.", 1, diagramShape.RelatedDiagrams.Count);
			var relatedDiagram = diagramShape.RelatedDiagrams[0]; //this is scaled diagram
			Assert(scaledDiagram.IsSameEntity(relatedDiagram.Shape));

			AssertEquals("Test Diagram (scaled copy)", relatedDiagram.Name);
			AssertEquals(RelatedDiagramView.ScaledCopy, relatedDiagram.RelationshipType);
			AssertEquals(true, relatedDiagram.IsScaled);
			AssertEquals(true, relatedDiagram.IsApproved);
			AssertEquals(ZDateTime.Now, relatedDiagram.ScheduledStartTimeLocal);
			AssertEquals(ZDateTime.Now.AddMonths(2), relatedDiagram.ScheduledFinishTimeLocal);

			AssertEquals("Should be one related diagram for 'scaledDiagram' - non-scaled source.", 1, scaledDiagram.RelatedDiagrams.Count);
			relatedDiagram = scaledDiagram.RelatedDiagrams[0]; //this is non-scaled diagram
			Assert(diagramShape.IsSameEntity(relatedDiagram.Shape));

			AssertEquals("Test Diagram", relatedDiagram.Name);
			AssertEquals(RelatedDiagramView.NonScaledSource, relatedDiagram.RelationshipType);
			AssertEquals(false, relatedDiagram.IsScaled);
			AssertEquals(false, relatedDiagram.IsApproved);
			AssertEquals(ZDateTime.Empty, relatedDiagram.ScheduledStartTimeLocal);
			AssertEquals(ZDateTime.Empty, relatedDiagram.ScheduledFinishTimeLocal);

			AssertEquals("Should be one parent diagram for sub-diagram.", 1, subDiagram.RelatedDiagrams.Count);
			relatedDiagram = subDiagram.RelatedDiagrams[0]; //this is parent diagram
			Assert(diagramShape.IsSameEntity(relatedDiagram.Shape));

			AssertEquals("Test Diagram", relatedDiagram.Name);
			AssertEquals(RelatedDiagramView.ParentRelation, relatedDiagram.RelationshipType);
			AssertEquals(false, relatedDiagram.IsScaled);
			AssertEquals(false, relatedDiagram.IsApproved);
			AssertEquals(ZDateTime.Empty, relatedDiagram.ScheduledStartTimeLocal);
			AssertEquals(ZDateTime.Empty, relatedDiagram.ScheduledFinishTimeLocal);
		}

		public void TestPopulateCollection_WithSingleShape_ShouldContainSingleLinkedShape()
		{
			var referenceDiagram = NetworkTestCase.CreateDiagram(Factory, name: "Reference Diagram");
			var linkedDiagram = NetworkTestCase.CreateDiagram(Factory, name: "Linked Diagram");
			var linkedShape = NetworkTestCase.CreateShape(linkedDiagram);

			var linkedJobNetwork = NetworkTestCase.CreateNetwork(linkedDiagram);
			linkedJobNetwork.LinkEntity(linkedShape, referenceDiagram);

			Factory.Save();

			CombineAssertions(() =>
			{
				Assert("Diagram should be linked correctly", referenceDiagram.IsSameEntity((IProposedNetworkEntity)linkedShape.LinkedEntity));
				AssertEquals("There should be a single related diagram", 1, referenceDiagram.RelatedDiagrams.Count);
				Assert("Related diagram should be same entity as original diagram", referenceDiagram.RelatedDiagrams[0].Shape.IsSameEntity(linkedShape));
				AssertEquals("Related diagram should have a LinkedDiagram relation type", RelatedDiagramView.LinkedRelation, referenceDiagram.RelatedDiagrams[0].RelationshipType);
			});
		}

		public void TestPopulateCollection_WithMultipleShapes_ShouldContainCorrectNumberofLinkedShapes()
		{
			var referenceDiagram = NetworkTestCase.CreateDiagram(Factory, name: "Reference Diagram");

			var linkedDiagram1 = NetworkTestCase.CreateDiagram(Factory, name: "Linked Diagram 1");
			var linkedShape1 = NetworkTestCase.CreateShape(linkedDiagram1);
			var linkedJobNetwork1 = NetworkTestCase.CreateNetwork(linkedDiagram1);

			var linkedDiagram2 = NetworkTestCase.CreateDiagram(Factory, name: "Linked Diagram 2");
			var linkedShape2 = NetworkTestCase.CreateShape(linkedDiagram2);
			var linkedJobNetwork2 = NetworkTestCase.CreateNetwork(linkedDiagram2);

			var linkedDiagram3 = NetworkTestCase.CreateDiagram(Factory, name: "Linked Diagram 3");
			var linkedShape3 = NetworkTestCase.CreateShape(linkedDiagram3);
			var linkedJobNetwork3 = NetworkTestCase.CreateNetwork(linkedDiagram3);

			JobNetworkTest.LinkDiagramToShapeExpectNoPrompts(linkedJobNetwork1, linkedShape1, referenceDiagram);
			JobNetworkTest.LinkDiagramToShapeExpectNoPrompts(linkedJobNetwork2, linkedShape2, referenceDiagram);
			JobNetworkTest.LinkDiagramToShapeExpectNoPrompts(linkedJobNetwork3, linkedShape3, referenceDiagram);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("There should be three linked diagrams", 3, referenceDiagram.RelatedDiagrams.Count);
				AssertEquals("First related diagram should have a LinkedDiagram relation type", RelatedDiagramView.LinkedRelation, referenceDiagram.RelatedDiagrams[0].RelationshipType);
				AssertEquals("Second related diagram should have a LinkedDiagram relation type", RelatedDiagramView.LinkedRelation, referenceDiagram.RelatedDiagrams[1].RelationshipType);
				AssertEquals("Third related diagram should have a LinkedDiagram relation type", RelatedDiagramView.LinkedRelation, referenceDiagram.RelatedDiagrams[2].RelationshipType);
			});
		}

		public void TestPopulateCollection_WithMultipleShapes_ThenRemoveSome_ShouldContainCorrectRelatedShapes()
		{
			var referenceDiagram = NetworkTestCase.CreateDiagram(Factory, name: "Reference Diagram");

			var linkedDiagram1 = NetworkTestCase.CreateDiagram(Factory, name: "Linked Diagram 1");
			var linkedShape1 = NetworkTestCase.CreateShape(linkedDiagram1);
			var linkedJobNetwork1 = NetworkTestCase.CreateNetwork(linkedDiagram1);

			var linkedDiagram2 = NetworkTestCase.CreateDiagram(Factory, name: "Linked Diagram 2");
			var linkedShape2 = NetworkTestCase.CreateShape(linkedDiagram2);
			var linkedJobNetwork2 = NetworkTestCase.CreateNetwork(linkedDiagram2);

			var linkedDiagram3 = NetworkTestCase.CreateDiagram(Factory, name: "Linked Diagram 3");
			var linkedShape3 = NetworkTestCase.CreateShape(linkedDiagram3);
			var linkedJobNetwork3 = NetworkTestCase.CreateNetwork(linkedDiagram3);

			JobNetworkTest.LinkDiagramToShapeExpectNoPrompts(linkedJobNetwork1, linkedShape1, referenceDiagram);
			JobNetworkTest.LinkDiagramToShapeExpectNoPrompts(linkedJobNetwork2, linkedShape2, referenceDiagram);
			JobNetworkTest.LinkDiagramToShapeExpectNoPrompts(linkedJobNetwork3, linkedShape3, referenceDiagram);

			Factory.Save();

			linkedJobNetwork1.UnlinkEntity(linkedShape1);
			linkedJobNetwork2.UnlinkEntity(linkedShape2);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("There should be one linked diagram remaning", 1, referenceDiagram.RelatedDiagrams.Count);
				Assert("Diagram3 should still be linked", referenceDiagram.IsSameEntity((IProposedNetworkEntity)linkedShape3.LinkedEntity));
			});
		}

		public void TestPopulateCollection_WithMultipleShapes_ThenChangeLinks_ShouldUpdateRelatedShapesCorrectly()
		{
			var referenceDiagram = NetworkTestCase.CreateDiagram(Factory, name: "Reference Diagram");

			var linkedDiagram1 = NetworkTestCase.CreateDiagram(Factory, name: "Linked Diagram 1");
			var linkedShape1 = NetworkTestCase.CreateShape(linkedDiagram1);
			var linkedJobNetwork1 = NetworkTestCase.CreateNetwork(linkedDiagram1);

			var linkedDiagram2 = NetworkTestCase.CreateDiagram(Factory, name: "Linked Diagram 2");
			var linkedShape2 = NetworkTestCase.CreateShape(linkedDiagram2);
			var linkedJobNetwork2 = NetworkTestCase.CreateNetwork(linkedDiagram2);

			var linkedDiagram3 = NetworkTestCase.CreateDiagram(Factory, name: "Linked Diagram 3");
			var linkedShape3 = NetworkTestCase.CreateShape(linkedDiagram3);
			var linkedJobNetwork3 = NetworkTestCase.CreateNetwork(linkedDiagram3);

			JobNetworkTest.LinkDiagramToShapeExpectNoPrompts(linkedJobNetwork1, linkedShape1, referenceDiagram);
			JobNetworkTest.LinkDiagramToShapeExpectNoPrompts(linkedJobNetwork2, linkedShape2, referenceDiagram);
			JobNetworkTest.LinkDiagramToShapeExpectNoPrompts(linkedJobNetwork3, linkedShape3, referenceDiagram);

			Factory.Save();

			var newReferenceDiagram = NetworkTestCase.CreateDiagram(Factory, name: "New Reference Diagram");

			linkedJobNetwork1.UnlinkEntity(linkedShape1);
			linkedJobNetwork2.UnlinkEntity(linkedShape2);

			linkedJobNetwork1.LinkEntity(linkedShape1, newReferenceDiagram);
			JobNetworkTest.LinkDiagramToShapeExpectNoPrompts(linkedJobNetwork2, linkedShape2, newReferenceDiagram);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("New diagram should have two linked diagrams", 2, newReferenceDiagram.RelatedDiagrams.Count);
				AssertEquals("Initial diagram should have one linked diagrams", 1, referenceDiagram.RelatedDiagrams.Count);

				AssertEquals("First new related diagram should have a LinkedDiagram relation type", RelatedDiagramView.LinkedRelation, newReferenceDiagram.RelatedDiagrams[0].RelationshipType);
				AssertEquals("Second new related diagram should have a LinkedDiagram relation type", RelatedDiagramView.LinkedRelation, newReferenceDiagram.RelatedDiagrams[1].RelationshipType);

				AssertEquals("Initial related diagram should have a LinkedDiagram relation type", RelatedDiagramView.LinkedRelation, newReferenceDiagram.RelatedDiagrams[0].RelationshipType);

				Assert("Diagram1 should be relinked correctly", newReferenceDiagram.IsSameEntity((IProposedNetworkEntity)linkedShape1.LinkedEntity));
				Assert("Diagram2 should be relinked correctly", newReferenceDiagram.IsSameEntity((IProposedNetworkEntity)linkedShape2.LinkedEntity));
				Assert("Diagram3 should not have been relinked", referenceDiagram.IsSameEntity((IProposedNetworkEntity)linkedShape3.LinkedEntity));
			});
		}

		public void TestPopulateCollection_WithMultipleLinkedShapesAndParentShape_ShouldContainCorrectRelatedShapes()
		{
			var parentDiagram = NetworkTestCase.CreateDiagram(Factory, name: "Parent Diagram");

			var linkedDiagram1 = NetworkTestCase.CreateDiagram(Factory, name: "Linked Diagram 1");
			var linkedShape1 = NetworkTestCase.CreateShape(linkedDiagram1);
			var linkedJobNetwork1 = NetworkTestCase.CreateNetwork(linkedDiagram1);

			var linkedDiagram2 = NetworkTestCase.CreateDiagram(Factory, name: "Linked Diagram 2");
			var linkedShape2 = NetworkTestCase.CreateShape(linkedDiagram2);
			var linkedJobNetwork2 = NetworkTestCase.CreateNetwork(linkedDiagram2);

			var linkedDiagram3 = NetworkTestCase.CreateDiagram(Factory, name: "Linked Diagram 3");
			var linkedShape3 = NetworkTestCase.CreateShape(linkedDiagram3);
			var linkedJobNetwork3 = NetworkTestCase.CreateNetwork(linkedDiagram3);

			JobNetworkTest.LinkDiagramToShapeExpectNoPrompts(linkedJobNetwork1, linkedShape1, parentDiagram);
			JobNetworkTest.LinkDiagramToShapeExpectNoPrompts(linkedJobNetwork2, linkedShape2, parentDiagram);
			JobNetworkTest.LinkDiagramToShapeExpectNoPrompts(linkedJobNetwork3, linkedShape3, parentDiagram);

			Factory.Save();

			AssertEquals("There should be three related diagrams", 3, parentDiagram.RelatedDiagrams.Count);
		}

		#region Implemenatation

		protected override RelatedDiagramViewCollection GetCollectionToTest()
		{
			var diagramShape = NetworkTestCase.CreateDiagram(Factory);
			return new RelatedDiagramViewCollection(diagramShape);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new RelatedDiagramView(Factory.New<BMNCNShape>(), Factory.New<BMNCNShape>());
		}

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
		}

		#endregion
	}
}
