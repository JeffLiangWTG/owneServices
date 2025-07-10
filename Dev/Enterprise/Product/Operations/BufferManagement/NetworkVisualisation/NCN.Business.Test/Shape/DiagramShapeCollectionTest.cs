using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business.Test;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(DiagramShapeCollection))]
	public class DiagramShapeCollectionTest : ActiveBusinessObjectCollectionTestCase<DiagramShapeCollection>
	{
		public void TestNoProcessHeadersWereLoaded()
		{
			var jobHeader1 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader1.ProcessHeaders.AddNew();

			var diagramShape = NetworkTestCase.CreateDiagram(jobHeader1);
			var defaultDiagramShape = NetworkTestCase.CreateDefaultDiagram(jobHeader2, "Retrospecticus");
			var workflowShape = NetworkTestCase.CreateShape(workflow, diagramShape, ShapeTypeList.Codes.Shape);

			Factory.Save();

			var cleanFactory = Factory.CreateNewFactory();
			var collection = new DiagramShapeCollection(cleanFactory);

			AssertEquals(3, collection.Count);

			AssertEquals(0, cleanFactory.GetTableHitCount(AutoProcessHeader.Schema.TableName));
			AssertEquals(1, cleanFactory.GetTableHitCount(AutoBMNCNShape.Schema.TableName));
			AssertEquals(0, cleanFactory.GetTableHitCount(AutoBMNCNAttachment.Schema.TableName));
		}

		public void TestCollectionShouldContainAllowedShapesOnly()
		{
			var jobHeader1 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader1.ProcessHeaders.AddNew();

			// Searchable shapes
			var diagramShape = NetworkTestCase.CreateDiagram(jobHeader1, name: "diagram");
			var defaultDiagramShape = NetworkTestCase.CreateDefaultDiagram(jobHeader2, name: "defaultDiagram");
			var shape = NetworkTestCase.CreateShape(workflow, diagramShape, name: "shape");
			var buffer = NetworkTestCase.CreateShape(diagramShape, name: "buffer", ShapeTypeList.Codes.Buffer);

			// Non-searchable shapes
			var annotation = NetworkTestCase.CreateShape(diagramShape, name: "annotation", ShapeTypeList.Codes.Annotation);
			var defaultWorkflow = NetworkTestCase.CreateShape(defaultDiagramShape, name: "defaultWorkflow", ShapeTypeList.Codes.DefaultWorkflow);

			var allShapes = new[] { defaultDiagramShape, diagramShape, shape, annotation, defaultWorkflow, buffer };
			AssertEquals("All shape types should be tested here. If any new types are added, consider whether they should appear in the Network Diagrams module by default or excluded.", allShapes.Length, new ShapeTypeList().Count);

			Factory.Save();

			var collection = new DiagramShapeCollection(Factory);

			AssertContainsExactElementsInAnyOrder(new[] { defaultDiagramShape, diagramShape, shape, buffer }, collection);
		}

		#region Implementation

		protected override DiagramShapeCollection GetCollectionToTest()
		{
			return new DiagramShapeCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return NetworkTestCase.CreateDiagram(VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory));
		}

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
		}

		#endregion
	}
}
