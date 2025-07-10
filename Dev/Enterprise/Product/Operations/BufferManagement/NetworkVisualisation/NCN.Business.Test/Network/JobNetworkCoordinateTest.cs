using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	class JobNetworkCoordinateTest : NetworkTestCase
	{
		#region Pin

		public void TestPinLocationPersistence()
		{
			var diagram = CreateDiagram(Factory, name: "Iron");
			var subDiagram = CreateShape(diagram, name: "Steel");
			var shape = CreateShape(subDiagram, name: "Chain Glass");
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var subDiagramEntity = subDiagram.AsEntity(network);
			var shapeEntity = shape.AsEntity(network);

			subDiagramEntity.X = 100;
			subDiagramEntity.Y = 100;
			subDiagramEntity.Width = 300;
			subDiagramEntity.Height = 300;
			shapeEntity.X = 200;
			shapeEntity.Y = 200;
			shapeEntity.Width = 30;
			shapeEntity.Height = 30;

			var shapeViewModel = CreateNodeViewModel(shapeEntity, networkViewModel);

			shapeViewModel.X = 205d;
			shapeViewModel.Y = 205d;

			AssertEquals(205d, shapeEntity.X);
			AssertEquals(205d, shapeEntity.Y);

			shape.PinShape(networkViewModel);

			shapeViewModel.X = 210d;
			shapeViewModel.Y = 210d;

			AssertEquals("Shape is now pinned, so changes made to its location should have no effect", 205d, shapeEntity.X);
			AssertEquals("Shape is now pinned, so changes made to its location should have no effect", 205d, shapeEntity.Y);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedNetwork = CreateNetwork(newFactory.Load<BMNCNShape>(diagram.PK));

			var loadedShape = loadedNetwork.Entities.ShapeEntities.Single(s => s.Name == "Chain Glass");

			AssertEquals(205d, loadedShape.X);
			AssertEquals(205d, loadedShape.Y);
		}

		#endregion

		#region Entity Locations

		public void TestLinkedSubDiagramLocation_ShouldDependOnOwner()
		{
			var diagram1 = CreateJobAndDiagram(Factory);
			var diagram2 = CreateJobAndDiagram(Factory);
			var subDiagram = CreateJobAndDiagram(Factory, name: "subDiagram");

			Factory.Save();

			var network1 = CreateJobNetwork(diagram1.PK, subDiagram.PK);
			var network2 = CreateJobNetwork(diagram2.PK, subDiagram.PK);

			var entity1Layout = network1.Entities.ShapeEntities.First(e => e.Name == "subDiagram");
			var entity2Layout = network2.Entities.ShapeEntities.First(e => e.Name == "subDiagram");

			AssertEquals(0.0, entity1Layout.X);
			AssertEquals(0.0, entity1Layout.Y);
			AssertEquals(0.0, entity2Layout.X);
			AssertEquals(0.0, entity2Layout.Y);

			entity1Layout.X = 50;
			entity1Layout.Y = 80;

			network1.DiagramShape.Factory.Save();

			AssertEquals(50.0, entity1Layout.X);
			AssertEquals(80.0, entity1Layout.Y);
			AssertEquals(0.0, entity2Layout.X);
			AssertEquals(0.0, entity2Layout.Y);

			entity2Layout.X = 500;
			entity2Layout.Y = 800;

			network2.DiagramShape.Factory.Save();

			AssertEquals(50.0, entity1Layout.X);
			AssertEquals(80.0, entity1Layout.Y);
			AssertEquals(500.0, entity2Layout.X);
			AssertEquals(800.0, entity2Layout.Y);
		}

		JobNetwork CreateJobNetwork(ZGuid diagramPK, ZGuid subDiagramToImportPK)
		{
			var factory = new BusinessObjectFactory();
			var diagram = factory.Load<BMNCNShape>(diagramPK);
			var subDiagram = factory.Load<BMNCNShape>(subDiagramToImportPK);

			JobNetwork network = null;
			var controller = new Mock<IBMNetworkEntityController>();
			controller.Setup(m => m.PickEntity(It.IsAny<ModuleIdentifier>(), It.IsAny<bool>())).Returns(subDiagram);

			using (ObjectFactory.Substitute(controller.Object))
			{
				network = CreateNetwork(diagram);
				network.PickAndImportEntities(diagram);

				return network;
			}
		}

		#endregion

		#region Scaled Entity Locations

		[TestDate(2016, 2, 8)]
		public void TestScaledShapesUseDatesForCoordinates_RescaleWhenScaleChanges()
		{
			WorkingDaysTestHelper.UpdateAllStaffDays(Factory, Env.CurrentUser.PK, new string('*', 24 * 2));
			var fourHoursOfMinutes = new ZInt(60 * 4);

			var diagram = CreateDiagram(Factory, isScaled: true, scheduledStartTimeUTC: ZDateTime.UtcNow);
			diagram.ResolutionIncrement = diagram.Scale = fourHoursOfMinutes.GetDateTimeFromMinutes();
			var network = CreateNetwork(diagram);

			var shape1 = CreateShape(diagram).AsEntity(network);
			shape1.X = shape1.Y = shape1.Width = shape1.Height = 400;

			AssertEquals("The shape is 16 hours long.", fourHoursOfMinutes * 4, shape1.ExplicitDurationMinutes);
			AssertEquals("The shape is startable after 16 hours.", ZDateTime.UtcNow.AddHours(16), shape1.Shape.ScheduledStartTimeUtc);

			diagram.Scale = new ZInt(fourHoursOfMinutes / 2).GetDateTimeFromMinutes();

			AssertEquals("The shape is still 16 hours long.", fourHoursOfMinutes * 4, shape1.ExplicitDurationMinutes);
			AssertEquals("The shape is still startable after 16 hours.", ZDateTime.UtcNow.AddHours(16), shape1.Shape.ScheduledStartTimeUtc);

			AssertEquals("The X coordinate moves to match the resolution increment.", 800d, shape1.X);
			AssertEquals("The width expands to match the resolution increment.", 800d, shape1.Width);
		}

		#endregion
	}
}
