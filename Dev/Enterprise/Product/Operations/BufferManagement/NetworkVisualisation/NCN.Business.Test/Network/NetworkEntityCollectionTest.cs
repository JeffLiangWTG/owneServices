using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	class NetworkEntityCollectionTest : NetworkTestCase
	{
		public void TestDoNotCallCollectionChangedBillionsOfTimes()
		{
			var diagramEntity = CreateDiagram(Factory, name: "Hodor");
			var network = CreateNetwork(diagramEntity);
			var collection = network.Entities;

			int touches = 0;
			collection.ItemsAdded += (s, e) => touches++;

			using (ActiveBusinessObjectCollection.DelayListChangedEvents(Factory))
			{
				Enumerable.Range(0, 5).ForEach(i => CreateShape(diagramEntity, name: "Hodor"));
			}

			AssertEquals(1, touches);
		}

		public void TestCreateNetworkEntities_NoDeletedShapes()
		{
			var diagramEntity = CreateDiagram(Factory, name: "Penrose");
			var network = CreateNetwork(diagramEntity);
			var shapes = network.Shapes;
			var collection = new NetworkEntityCollection(shapes, network);

			var shape1 = CreateShape(diagramEntity, shapeType: DiagramShapeTypeList.Codes.Buffer);

			Factory.Save();

			int countChangedCalled = 0;
			bool containsDeleted = false;
			shapes.CountChanged += (s, e) =>
			{
				countChangedCalled++;
				collection.Update();
				containsDeleted |= !collection.OfType<BusinessObject>().All(o => !o.IsDeleted);
			};

			var secondFactory = Factory.CreateNewFactory();
			var shape1Copy = secondFactory.Load<BMNCNShape>(shape1.PK);
			var shapeNetworkEntity = collection.GetInstance(shape1Copy);
			Assert("ShapeNetworkEntity points to second Factory's shape object", object.ReferenceEquals(shapeNetworkEntity.Shape, shape1Copy));

			using (ActiveBusinessObjectCollection.DelayListChangedEvents(Factory))
			{
				var shapeCreatedToTriggerCountChanged = CreateShape(diagramEntity, shapeType: DiagramShapeTypeList.Codes.Buffer);

				shapeNetworkEntity.Delete();

				Factory.Save();
				Assert("ShapeNetworkEntity should be deleted", shapeNetworkEntity.IsDeleted);
				AssertEquals("No CountChanged events have yet been called", 0, countChangedCalled);
			}

			Assert("Should not contain deleted ShapeNetworkEntities in the CountChanged event", !containsDeleted);
			AssertEquals("Should have hit the CountChanged event", 1, countChangedCalled);
		}
	}
}
