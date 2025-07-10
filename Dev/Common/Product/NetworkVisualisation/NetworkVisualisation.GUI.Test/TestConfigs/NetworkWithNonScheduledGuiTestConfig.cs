using System.Collections.Generic;
using System.Linq;
using CargoWise.NetworkVisualisation.Business;

namespace CargoWise.NetworkVisualisation.GUI.Test
{
	class NetworkWithNonScheduledGuiTestConfig : NetworkGuiTestConfig
	{
		#region Construction

		public new static NetworkWithNonScheduledGuiTestConfig Create(Entity diagramEntity = null, bool shouldShowWindow = true, bool shouldSetDataContext = true)
		{
			diagramEntity = InitializeDiagram(diagramEntity);
			return new NetworkWithNonScheduledGuiTestConfig(diagramEntity, shouldShowWindow, childEntities: null);
		}

		public new static NetworkWithNonScheduledGuiTestConfig CreateWithNodes(Entity diagramEntity = null, bool shouldShowWindow = true)
		{
			diagramEntity = InitializeDiagram(diagramEntity);
			return new NetworkWithNonScheduledGuiTestConfig(diagramEntity, shouldShowWindow, GetChildEntities(diagramEntity));
		}

		protected NetworkWithNonScheduledGuiTestConfig(Entity diagramEntity, bool shouldShowWindow, IEnumerable<Entity> childEntities)
			: base(diagramEntity, shouldShowWindow, childEntities)
		{
		}

		static Entity InitializeDiagram(Entity diagramEntity)
		{
			diagramEntity = diagramEntity ?? new Entity { IsDiagramScaled = true };
			diagramEntity.ShouldShowNonScheduledSection = true;

			return diagramEntity;
		}

		static IEnumerable<Entity> GetChildEntities(Entity diagramEntity)
		{
			var entity1 = CreateEntity("Regular Entity 1", 10, 20, 300, 150, isNonScheduled: false);
			var entity2 = CreateEntity("Regular Entity 2", 350, 20, 300, 150, isNonScheduled: false);
			var entity3 = CreateEntity("Nonscheduled Entity 1", 10, 200, 300, 150, isNonScheduled: true);
			var entity4 = CreateEntity("Nonscheduled Entity 2", 10, 400, 300, 150, isNonScheduled: true);

			var network = new DummyNetwork { DiagramEntity = diagramEntity };
			network.CreateRelationship(entity1, entity2);
			network.CreateRelationship(entity3, entity4);

			return new[] { entity1, entity2, entity3, entity4 };
		}

		#endregion

		#region Properties

		public NodeViewModel RegularNode1 => Control.MainDiagramControl.NetworkViewModel.Nodes.Single(x => x.Name == "Regular Entity 1");
		public NodeViewModel RegularNode2 => Control.MainDiagramControl.NetworkViewModel.Nodes.Single(x => x.Name == "Regular Entity 2");
		public NodeViewModel NonScheduledNode1 => Control.NonScheduledDiagramControl.NetworkViewModel.Nodes.Single(x => x.Name == "Nonscheduled Entity 1");
		public NodeViewModel NonScheduledNode2 => Control.NonScheduledDiagramControl.NetworkViewModel.Nodes.Single(x => x.Name == "Nonscheduled Entity 2");
		public NetworkView NonScheduledNetworkView => (NetworkView)Control.NonScheduledDiagramControl.FindName("NetworkControl");

		#endregion
	}
}
