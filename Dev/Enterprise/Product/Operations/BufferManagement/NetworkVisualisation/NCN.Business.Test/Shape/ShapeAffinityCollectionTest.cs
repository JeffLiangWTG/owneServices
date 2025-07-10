using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business.Test;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	class ShapeAffinityCollectionTest : NetworkTestCase
	{
		public void TestRemoveElementRemovesLinks()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];

			var diagram = CreateDiagram(jobHeader);
			var network = CreateNetwork(diagram);
			var diagramShape = network.DiagramEntity;
			var workflow1Shape = CreateShape(workflow1, diagramShape);

			Factory.Save();

			var shapeAffinity1 = new ShapeAffinity("Colour1", "Benedict", ZGuid.NewZGuid());
			var shapeAffinity2 = new ShapeAffinity("Another Colour", "Steve", ZGuid.NewZGuid());
			network.DiagramShape.ShapeAffinities.Add(shapeAffinity1);
			network.DiagramShape.ShapeAffinities.Add(shapeAffinity2);

			((IDiagramEntity)network.DiagramEntity).CreateAffinityLink(workflow1Shape, shapeAffinity1);
			((IDiagramEntity)network.DiagramEntity).CreateAffinityLink(workflow1Shape, shapeAffinity2);

			AssertEquals(2, network.DiagramShape.ShapeAffinityLinks.Count);

			network.DiagramShape.ShapeAffinities.Remove(shapeAffinity1);

			AssertEquals(1, network.DiagramShape.ShapeAffinityLinks.Count);
		}

		public void TestSetDefaults()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];

			var diagramShape = CreateDiagram(jobHeader);
			var workflow1Shape = CreateShape(workflow1, diagramShape);

			var newAffinity = diagramShape.ShapeAffinities.AddNew();

			AssertNotEquals(ZGuid.Empty, newAffinity.AffinityPK);
		}
	}

	[TestedType(typeof(ShapeAffinityCollection))]
	class ShapeAffinityCollectionTestCase : NonPersistentBusinessObjectCollectionTestCase<ShapeAffinityCollection>
	{
		protected override ShapeAffinityCollection GetCollectionToTest()
		{
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var shape = NetworkTestCase.CreateDiagram(jobHeader);

			return new ShapeAffinityCollection(Factory, shape);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ShapeAffinity(Factory);
		}
	}
}
