using System.Drawing;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.Business.Test
{
	public class ExtensionMethodsTest : TestCase
	{
		public void TestGetPreRequisiteDepth()
		{
			var network = new DummyNetwork();
			var entity1 = new Entity() { Name = "Entity1" };
			var entity2 = new Entity() { Name = "Entity2" };
			var entity3 = new Entity() { Name = "Entity3" };

			network.CreateRelationship(entity1, entity2);
			network.CreateRelationship(entity2, entity3);

			AssertEquals(2, entity3.GetPreRequisiteDepth());
		}

		public void TestGetPreRequisiteDepth_NoPrerequisites()
		{
			var network = new DummyNetwork();
			var entity1 = new Entity() { Name = "Entity1" };

			AssertEquals(0, entity1.GetPreRequisiteDepth());
		}

		public void TestGetPreRequisiteDepth_CircularDependency()
		{
			var network = new DummyNetwork();

			var entity1 = new Entity() { Name = "Entity1" };
			var entity2 = new Entity() { Name = "Entity2" };
			var entity3 = new Entity() { Name = "Entity3" };

			network.CreateRelationship(entity1, entity2);
			network.CreateRelationship(entity2, entity3);
			network.CreateRelationship(entity3, entity1); // Circular dependency

			AssertEquals(-1, entity1.GetPreRequisiteDepth());
		}

		public void TestGetMemberChannel()
		{
			var diagramEntity = new Entity();
			var network = new DummyNetwork { DiagramEntity = diagramEntity };

			var channel1 = new DummyChannel("Luminous", 100, Color.PapayaWhip);
			var channel2 = new DummyChannel("Beings", 100, Color.Goldenrod);

			diagramEntity.DiagramChannels = new[] { channel1, channel2 };

			var parentEntity = new Entity { Width = 100, Height = 100, X = 100, Y = 100 };
			var childEntity = new Entity { Width = 50, Height = 50, X = 10, Y = 10, Parent = parentEntity };

			network.Entities.Add(parentEntity);
			network.Entities.Add(childEntity);

			var viewModel = new NetworkViewModel(network);

			AssertEquals(channel2, parentEntity.GetMemberChannel(viewModel));
			AssertEquals(channel2, childEntity.GetMemberChannel(viewModel));

			parentEntity.Y = 0;

			AssertEquals(channel1, parentEntity.GetMemberChannel(viewModel));
			AssertEquals(channel1, childEntity.GetMemberChannel(viewModel));

			parentEntity.Y = 1000;

			AssertEquals(channel2, parentEntity.GetMemberChannel(viewModel));
			AssertEquals(channel2, childEntity.GetMemberChannel(viewModel));
		}
	}
}
