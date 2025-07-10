using System.Linq;
using CargoWise.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	class RecursiveManyToManyRelationshipTest : NetworkTestCase
	{
		public void TestRelationship_LoopedHierarchyBetweenDiagramAndItsOwnShape_ShouldNotStackOverflow()
		{
			var jobNotAppearingInThisFilm = CreateJobHeader<OrgHeader>();
			var diagramNotAppearingInThisFilm = CreateDiagram(jobNotAppearingInThisFilm);

			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();

			var diagram = CreateDiagram(jobHeader1, name: "Diagraham");
			var subDiagram = CreateShape(jobHeader2, diagram, name: "Sub-diagraham");

			AssertShapeNamesInDescendantShapesCollection("Diagram has just one child shape", diagram, "Sub-diagraham");

			diagram.MakeChildOf(subDiagram);
			ErrorReporter.Clear(); // Making diagrams embedded within other shapes is invalid. This test is a relic of a bygone era when this was possible. It's stil here because maybe one day it'll be possible again.

			AssertShapeNamesInDescendantShapesCollection("The circular hierarchy means the diagram contains itself too.", diagram, "Diagraham", "Sub-diagraham");
		}

		public void TestRelationship_LoopedHierarchyBetweenShapesWithinDiagram_ShouldNotStackOverflow()
		{
			var diagram = CreateDiagram(Factory, name: "Le");
			var shape1 = CreateShape(diagram, name: "boop");
			var shape2 = CreateShape(shape1, name: "teh");
			var shape3 = CreateShape(shape2, name: "snoot");

			AssertShapeNamesInDescendantShapesCollection("Descendant shapes for diagram", diagram, "boop", "teh", "snoot");
			AssertShapeNamesInDescendantShapesCollection("Descendant shapes for shape1", shape1, "teh", "snoot");
			AssertShapeNamesInDescendantShapesCollection("Descendant shapes for shape2", shape2, "snoot");
			AssertShapeNamesInDescendantShapesCollection("Descendant shapes for shape3", shape3);

			shape1.MakeChildOf(shape3); // Not possible within the diagrammer, yet.

			AssertShapeNamesInDescendantShapesCollection("Descendant shapes for diagram", diagram, "boop", "teh", "snoot");
			AssertShapeNamesInDescendantShapesCollection("Descendant shapes for shape1", shape1, "teh", "snoot");
			AssertShapeNamesInDescendantShapesCollection("Descendant shapes for shape2", shape2, "snoot");
			AssertShapeNamesInDescendantShapesCollection("Descendant shapes for shape3", shape3);
		}

		public void TestRelationship_AddAndDeleteChild()
		{
			var jobNotAppearingInThisFilm = CreateJobHeader<OrgHeader>();
			var diagramNotAppearingInThisFilm = CreateDiagram(jobNotAppearingInThisFilm);

			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var jobHeader3 = CreateJobHeader<OrgHeader>();

			var diagram = CreateDiagram(jobHeader1);
			var subDiagram = CreateShape(jobHeader2, diagram);
			var subSubDiagram = CreateShape(jobHeader3, subDiagram);
			var shape = CreateShape(subSubDiagram);

			var bizoCollection = BMNCNShapeCollection.ForDescendantShapes(diagram);

			AssertEquals(3, bizoCollection.Count);
			AssertCollectionContains(subDiagram, bizoCollection);
			AssertCollectionContains(subSubDiagram, bizoCollection);
			AssertCollectionContains(shape, bizoCollection);

			var shape2 = CreateShape(subSubDiagram);

			AssertEquals(4, bizoCollection.Count);
			AssertCollectionContains(shape2, bizoCollection);

			shape2.Delete();

			AssertEquals(3, bizoCollection.Count);
			AssertCollectionNotContains(shape2, bizoCollection);
		}

		public void TestRelationship_AddAndDeleteTree()
		{
			var jobNotAppearingInThisFilm = CreateJobHeader<OrgHeader>();
			var diagramNotAppearingInThisFilm = CreateDiagram(jobNotAppearingInThisFilm);

			var diagramTree1 = CreateDiagram(CreateJobHeader<OrgHeader>());
			var diagramTree1Sub = CreateShape(CreateJobHeader<OrgHeader>(), diagramTree1);
			var diagramTree1SubSub = CreateShape(CreateJobHeader<OrgHeader>(), diagramTree1Sub);

			var tree1Shape = CreateShape(diagramTree1);

			var diagramTree2 = CreateDiagram(CreateJobHeader<OrgHeader>());
			var diagramTree2Sub = CreateShape(CreateJobHeader<OrgHeader>(), diagramTree2);
			var diagramTree2SubSub = CreateShape(CreateJobHeader<OrgHeader>(), diagramTree2Sub);

			AssertContainsExactElementsInAnyOrder(new[] { diagramTree1Sub, diagramTree1SubSub, tree1Shape }, BMNCNShapeCollection.ForDescendantShapes(diagramTree1));
			AssertContainsExactElementsInAnyOrder(new[] { diagramTree1SubSub }, BMNCNShapeCollection.ForDescendantShapes(diagramTree1Sub));
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<BMNCNShape>(), BMNCNShapeCollection.ForDescendantShapes(diagramTree1SubSub));

			AssertContainsExactElementsInAnyOrder(new[] { diagramTree2Sub, diagramTree2SubSub }, BMNCNShapeCollection.ForDescendantShapes(diagramTree2));
			AssertContainsExactElementsInAnyOrder(new[] { diagramTree2SubSub }, BMNCNShapeCollection.ForDescendantShapes(diagramTree2Sub));
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<BMNCNShape>(), BMNCNShapeCollection.ForDescendantShapes(diagramTree2SubSub));
		}

		static void AssertShapeNamesInDescendantShapesCollection(string assertionMessage, BMNCNShape shape, params string[] expectedDescendantShapeNames)
		{
			AssertContainsExactElementsInAnyOrder(assertionMessage, expectedDescendantShapeNames, BMNCNShapeCollection.ForDescendantShapes(shape).Select(s => s.BNS_Name));
		}
	}
}
