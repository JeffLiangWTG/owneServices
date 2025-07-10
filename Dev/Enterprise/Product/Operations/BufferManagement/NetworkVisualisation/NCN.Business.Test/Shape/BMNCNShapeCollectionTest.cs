using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(BMNCNShapeCollection))]
	class BMNCNShapeCollectionTest : ActiveBusinessObjectCollectionTestCase<BMNCNShapeCollection>
	{
		public void TestCollectionForDescendentShapes_ShouldContainDescendantsOnly()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, name: "diagram");
			var shape1 = NetworkTestCase.CreateShape(diagram, name: "shape1");
			var shape2 = NetworkTestCase.CreateShape(shape1, name: "shape2");
			var shape3 = NetworkTestCase.CreateShape(shape2, name: "shape3");
			var shape4 = NetworkTestCase.CreateShape(shape3, name: "shape4");

			var diagramCollection = BMNCNShapeCollection.ForDescendantShapes(diagram);
			var shape1Collection = BMNCNShapeCollection.ForDescendantShapes(shape1);
			var shape2Collection = BMNCNShapeCollection.ForDescendantShapes(shape2);
			var shape3Collection = BMNCNShapeCollection.ForDescendantShapes(shape3);
			var shape4Collection = BMNCNShapeCollection.ForDescendantShapes(shape4);

			AssertContainsExactElementsInAnyOrder(new[] { shape1, shape2, shape3, shape4 }, diagramCollection);
			AssertContainsExactElementsInAnyOrder(new[] { shape2, shape3, shape4 }, shape1Collection);
			AssertContainsExactElementsInAnyOrder(new[] { shape3, shape4 }, shape2Collection);
			AssertContainsExactElementsInAnyOrder(new[] { shape4 }, shape3Collection);
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<BMNCNShape>(), shape4Collection);

			var shape5 = NetworkTestCase.CreateShape(shape4, name: "shape5");
			var shape5Collection = BMNCNShapeCollection.ForDescendantShapes(shape5);

			AssertContainsExactElementsInAnyOrder(new[] { shape1, shape2, shape3, shape4, shape5 }, diagramCollection);
			AssertContainsExactElementsInAnyOrder(new[] { shape2, shape3, shape4, shape5 }, shape1Collection);
			AssertContainsExactElementsInAnyOrder(new[] { shape3, shape4, shape5 }, shape2Collection);
			AssertContainsExactElementsInAnyOrder(new[] { shape4, shape5 }, shape3Collection);
			AssertContainsExactElementsInAnyOrder(new[] { shape5 }, shape4Collection);
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<BMNCNShape>(), shape5Collection);
		}

		public void TestCollectionForChildShapes_ShouldContainDirectChildrenOnly()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, name: "diagram");
			var shape1 = NetworkTestCase.CreateShape(diagram, name: "shape1");
			var shape2 = NetworkTestCase.CreateShape(shape1, name: "shape2");
			var shape3 = NetworkTestCase.CreateShape(shape2, name: "shape3");
			var shape4 = NetworkTestCase.CreateShape(shape3, name: "shape4");

			AssertContainsExactElementsInAnyOrder(new[] { shape1 }, BMNCNShapeCollection.ForChildShapes(diagram));
			AssertContainsExactElementsInAnyOrder(new[] { shape2 }, BMNCNShapeCollection.ForChildShapes(shape1));
			AssertContainsExactElementsInAnyOrder(new[] { shape3 }, BMNCNShapeCollection.ForChildShapes(shape2));
			AssertContainsExactElementsInAnyOrder(new[] { shape4 }, BMNCNShapeCollection.ForChildShapes(shape3));
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<BMNCNShape>(), BMNCNShapeCollection.ForChildShapes(shape4));
		}

		public void TestDontHitTheDatabaseRedundantly()
		{
			var jobHeader1 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader1.ProcessHeaders.AddNew();

			var diagramShape = NetworkTestCase.CreateDiagram(jobHeader1);
			var defaultDiagramShape = jobHeader2.GetDefaultDiagram();
			var workflowShape = NetworkTestCase.CreateShape(workflow, diagramShape, ShapeTypeList.Codes.Shape);

			defaultDiagramShape.Name = "Pliz actually save thx";

			Factory.Save();

			var cleanFactory = Factory.CreateNewFactory();
			var collection = new BMNCNShapeCollection(cleanFactory);

			AssertEquals(3, collection.Count);

			AssertEquals(0, cleanFactory.GetTableHitCount(ProcessHeaderSchema.Constants.TableName));
			AssertEquals(1, cleanFactory.GetTableHitCount(BMNCNShapeSchema.Constants.TableName));
			AssertEquals(0, cleanFactory.GetTableHitCount(BMNCNAttachmentSchema.Constants.TableName));

			cleanFactory = Factory.CreateNewFactory();
			collection = new BMNCNShapeCollection(cleanFactory, new ZQuery());

			AssertEquals(3, collection.Count);

			AssertEquals(0, cleanFactory.GetTableHitCount(ProcessHeaderSchema.Constants.TableName));
			AssertEquals(1, cleanFactory.GetTableHitCount(BMNCNShapeSchema.Constants.TableName));
			AssertEquals(0, cleanFactory.GetTableHitCount(BMNCNAttachmentSchema.Constants.TableName));
		}

		public void TestDontHitTheDatabaseRedundantly_ParentShape()
		{
			var jobHeader1 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader1.ProcessHeaders.AddNew();

			var diagramShape = NetworkTestCase.CreateDiagram(jobHeader1);
			var defaultDiagramShape = jobHeader2.GetDefaultDiagram();
			var workflowShape = NetworkTestCase.CreateShape(workflow, diagramShape, "Pelican");
			var regularOldShape = NetworkTestCase.CreateShape(diagramShape, "Square Souled Shoes");

			defaultDiagramShape.Name = "Pliz actually save thx";

			Factory.Save();

			var cleanFactory = Factory.CreateNewFactory();
			var collection = cleanFactory.Load<BMNCNShape>(diagramShape.PK).ChildShapes;

			AssertEquals(2, collection.Count);

			AssertEquals(0, cleanFactory.GetTableHitCount(ProcessHeaderSchema.Constants.TableName));
			AssertEquals(2, cleanFactory.GetTableHitCount(BMNCNShapeSchema.Constants.TableName));
			AssertEquals(0, cleanFactory.GetTableHitCount(BMNCNAttachmentSchema.Constants.TableName));
		}

		public void TestCollection_WhenParentShapeNotSpecified()
		{
			var collection = new BMNCNShapeCollection(Factory);
			var shape = collection.AddNew();
			AssertEquals(ShapeTypeList.Codes.Diagram, shape.BNS_ShapeType);
		}

		public void TestCollectionRelationship_WhenDependencyArrowWithNoProcessHeaderLinkSpecified_ShouldNotReportError()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var child = NetworkTestCase.CreateShape(diagram);

			var buffer = Factory.New<BMNCNShape>();
			buffer.BNS_ShapeType = ShapeTypeList.Codes.Buffer;
			NetworkTestCase.AttachChildToParent(buffer, diagram);

			var arrow = Factory.New<BMNCNAttachment>();
			arrow.BNA_BNS_FromShape = child.PK;
			arrow.BNA_BNS_ToShape = buffer.PK;
			arrow.BNA_BNS_Owner = diagram.PK;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedDiagram = newFactory.Load<BMNCNShape>(diagram.PK);

			AssertEquals(2, loadedDiagram.ChildShapes.Count);
			AssertEquals(1, loadedDiagram.ChildShapes[0].DependencyAttachments.Count);
			AssertEquals(1, loadedDiagram.ChildShapes[1].DependencyAttachments.Count);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
		}

		#endregion
	}
}
