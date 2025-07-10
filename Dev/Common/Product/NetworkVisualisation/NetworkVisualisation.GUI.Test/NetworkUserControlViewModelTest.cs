using System.Linq;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using NUnit.Framework;
using Color = System.Drawing.Color;

namespace CargoWise.NetworkVisualisation.GUI.Test
{
	class NetworkUserControlViewModelTest : TestCase
	{
		public void TestContextMenuShouldBeObservable()
		{
			var model = new NetworkUserControlViewModel();
			Assert("Context menu should be implemented as an observable collection otherwise menu won't be updated on repetitive right clicks", model.MenuItems is IObservableReloadableCollection<NetworkActionMenuItem>);
		}

		public void TestIsNetworkEnabled()
		{
			var diagramEntity = new Entity();
			var network = new DummyNetwork { DiagramEntity = diagramEntity };
			var viewModel = new NetworkUserControlViewModel(network);
			viewModel.HiddenEntities.Add(new Entity());

			AssertEquals(false, network.IsReadOnly);
			AssertEquals(true, viewModel.IsNetworkEnabled);

			network.IsReadOnly = true;
			AssertEquals(false, viewModel.IsNetworkEnabled);
		}

		public void TestStatusMessage()
		{
			var diagramEntity = new Entity();
			var network = new DummyNetwork { DiagramEntity = diagramEntity };
			var viewModel = new NetworkUserControlViewModel(network);

			AssertEquals(null, viewModel.StatusMessage);

			viewModel.HiddenEntities.Add(new Entity());
			AssertEquals("There are hidden items within this diagram.", viewModel.StatusMessage);

			network.IsReadOnly = true;
			AssertEquals("This diagram is read-only.", viewModel.StatusMessage);
		}

		public void TestSelectRelevantNodes_ShouldSelectAllChildEntities()
		{
			var network = new DummyNetwork();
			var entity_grandparent = new Entity();
			var entity_parent = new Entity { Parent = entity_grandparent };
			var entity = new Entity { Parent = entity_parent };

			network.Entities.Add(entity_grandparent);
			network.Entities.Add(entity_parent);
			network.Entities.Add(entity);

			var viewModel = new NetworkUserControlViewModel(network);
			foreach (var node in viewModel.NetworkViewModel.Nodes)
			{
				AssertEquals(false, node.IsSelected);
			}

			viewModel.NetworkViewModel.ScheduledNodes[2].IsSelected = true;
			viewModel.SelectRelevantNodes();

			AssertEquals(false, viewModel.NetworkViewModel.ScheduledNodes[0].IsSelected);
			AssertEquals(false, viewModel.NetworkViewModel.ScheduledNodes[1].IsSelected);
			AssertEquals(true, viewModel.NetworkViewModel.ScheduledNodes[2].IsSelected);

			viewModel.NetworkViewModel.ScheduledNodes[1].IsSelected = true;
			viewModel.NetworkViewModel.ScheduledNodes[2].IsSelected = false;
			viewModel.SelectRelevantNodes();

			AssertEquals(false, viewModel.NetworkViewModel.ScheduledNodes[0].IsSelected);
			AssertEquals(true, viewModel.NetworkViewModel.ScheduledNodes[1].IsSelected);
			AssertEquals(true, viewModel.NetworkViewModel.ScheduledNodes[2].IsSelected);

			viewModel.NetworkViewModel.ScheduledNodes[0].IsSelected = true;
			viewModel.NetworkViewModel.ScheduledNodes[1].IsSelected = false;
			viewModel.NetworkViewModel.ScheduledNodes[2].IsSelected = false;
			viewModel.SelectRelevantNodes();

			AssertEquals(true, viewModel.NetworkViewModel.ScheduledNodes[0].IsSelected);
			AssertEquals(true, viewModel.NetworkViewModel.ScheduledNodes[1].IsSelected);
			AssertEquals(true, viewModel.NetworkViewModel.ScheduledNodes[2].IsSelected);
		}

		public void TestSelectRelevantNodes_DeSelectsChildrenOfFixedParents()
		{
			var network = new DummyNetwork();
			var entity_parent = new Entity();
			entity_parent.EntityState = EntityState.Fixed;
			var entity = new Entity { Parent = entity_parent };

			network.Entities.Add(entity_parent);
			network.Entities.Add(entity);

			var viewModel = new NetworkUserControlViewModel(network);
			foreach (var node in viewModel.NetworkViewModel.Nodes)
			{
				AssertEquals(false, node.IsSelected);
			}

			viewModel.NetworkViewModel.ScheduledNodes[0].IsSelected = true;
			viewModel.NetworkViewModel.ScheduledNodes[1].IsSelected = true;
			viewModel.SelectRelevantNodes();

			AssertEquals(true, viewModel.NetworkViewModel.ScheduledNodes[0].IsSelected);
			AssertEquals(false, viewModel.NetworkViewModel.ScheduledNodes[1].IsSelected);
		}

		[ExpectNoExceptions]
		public void TestConstructor_WithNullNetworkDiagramEntity()
		{
			var network = new DummyNetwork { DiagramEntity = null };
			var viewModel = new NetworkUserControlViewModel(network);
		}

		public void TestDiagramEntityJobName_ShouldNotifyViewModel()
		{
			var entity = new Entity();
			var network = new DummyNetwork { DiagramEntity = entity };
			var viewModel = new NetworkUserControlViewModel(network);

			viewModel.PropertyChanged += (s, e) =>
			{
				AssertEquals("JobName", e.PropertyName);
				AssertEquals("Dis Job", (string)viewModel.GetType().GetProperty(e.PropertyName).GetValue(viewModel, null));
			};

			entity.JobName = "Dis Job";
		}

		public void TestSupportsDiagramVisualStyles()
		{
			var network = new DummyNetwork();
			network.DiagramEntity = new Entity { SupportedActions = NetworkActions.StyleDiagram };
			var viewModel = new NetworkUserControlViewModel(network);
			AssertEquals(true, viewModel.SupportsDiagramVisualStyles);

			((Entity)network.DiagramEntity).SupportedActions = NetworkActions.None;
			AssertEquals(false, viewModel.SupportsDiagramVisualStyles);
		}

		public void TestSupportsEditEntity()
		{
			var network = new DummyNetwork();
			network.DiagramEntity = new Entity { SupportedActions = NetworkActions.EditEntity };
			var viewModel = new NetworkUserControlViewModel(network);
			AssertEquals(true, viewModel.SupportsEditEntity);

			((Entity)network.DiagramEntity).SupportedActions = NetworkActions.None;
			AssertEquals(false, viewModel.SupportsEditEntity);
		}

		public void TestBuildNetwork_ExistingLayoutValues()
		{
			var network = new DummyNetwork();
			var entity1 = new Entity { X = 35, Y = 45 };
			var entity2 = new Entity { X = 155, Y = 65 };
			network.Entities.Add(entity1);
			network.Entities.Add(entity2);

			var viewModel = new NetworkUserControlViewModel(network);
			AssertEquals(2, viewModel.NetworkViewModel.Nodes.Count());

			var node1 = viewModel.NetworkViewModel.ScheduledNodes[0];
			var node2 = viewModel.NetworkViewModel.ScheduledNodes[1];

			AssertEquals(35d, node1.X);
			AssertEquals(45d, node1.Y);
			AssertEquals(155d, node2.X);
			AssertEquals(65d, node2.Y);
		}

		public void TestBuildNetwork_AutoLayout_NewItemsConsiderExistingDefaultItems()
		{
			var rootEntity = new Entity();
			var network = new DummyNetwork { DiagramEntity = rootEntity };
			var entity1 = new Entity { Parent = rootEntity, X = 0, Y = 1 };
			var entity2 = new Entity { Parent = rootEntity, X = 0, Y = 75 };
			var entity3 = new Entity { Parent = rootEntity };
			network.Entities.Add(entity1);
			network.Entities.Add(entity2);
			network.Entities.Add(entity3);

			var viewModel = new NetworkUserControlViewModel(network);
			AssertEquals(3, viewModel.NetworkViewModel.Nodes.Count());

			var node1 = viewModel.NetworkViewModel.ScheduledNodes[0];
			var node2 = viewModel.NetworkViewModel.ScheduledNodes[1];
			var node3 = viewModel.NetworkViewModel.ScheduledNodes[2];

			AssertEquals(0d, node1.X);
			AssertEquals(1d, node1.Y);
			AssertEquals(0d, node2.X);
			AssertEquals(75d, node2.Y);
			AssertEquals(75d, node2.Height);
			AssertEquals("Should use default values when none provided", 0d, node3.X);
			AssertEquals("Default value should not overlap with existing nodes", 90d, node3.Y);
		}

		public void TestBuildNetwork_NoExistingLayoutValues()
		{
			var rootEntity = new Entity();
			var network = new DummyNetwork { DiagramEntity = rootEntity };

			var entity1 = new Entity { Parent = rootEntity, };
			var entity2 = new Entity { Parent = rootEntity, };
			network.Entities.Add(entity1);
			network.Entities.Add(entity2);

			var viewModel = new NetworkUserControlViewModel(network);
			AssertEquals(2, viewModel.NetworkViewModel.Nodes.Count());

			var node1 = viewModel.NetworkViewModel.ScheduledNodes[0];
			var node2 = viewModel.NetworkViewModel.ScheduledNodes[1];

			AssertEquals("Should use default values when none provided", 0d, node1.X);
			AssertEquals("Should use default values when none provided", 0d, node1.Y);
			AssertEquals("Should use default values when none provided", 0d, node2.X);
			AssertEquals("Should use default values when none provided", 90d, node2.Y);
		}

		public void TestCompletionCriteria()
		{
			var entity = new Entity();
			entity.CompletionCriteria = "Completion Criteria";
			entity.ForeColor = Color.Black;
			var network = new DummyNetwork { DiagramEntity = entity };

			var viewModel = new NetworkUserControlViewModel(network);

			AssertEquals(true, viewModel.DiagramEntityViewModel.IsCompletionCriteriaPlaceholder(viewModel.DiagramEntityViewModel.CompletionCriteriaPlaceholder));
			AssertEquals(true, viewModel.DiagramEntityViewModel.IsCompletionCriteriaPlaceholder(viewModel.DiagramEntityViewModel.CompletionCriteria));
			AssertEquals(Color.Gray, viewModel.DiagramEntityViewModel.CompletionCriteriaTextColor);

			var criteria = "Bananas are real pancakes";

			viewModel.DiagramEntityViewModel.CompletionCriteria = criteria;

			AssertEquals(criteria, viewModel.DiagramEntityViewModel.CompletionCriteria);
			AssertEquals(false, viewModel.DiagramEntityViewModel.IsCompletionCriteriaPlaceholder(viewModel.DiagramEntityViewModel.CompletionCriteria));
			AssertEquals(Color.Black, viewModel.DiagramEntityViewModel.CompletionCriteriaTextColor);
		}

		public void TestNotes_Annotation()
		{
			var entity = new Entity { ShapeType = ShapeTypes.Annotation };
			entity.Notes = "Notes";
			var network = new DummyNetwork { DiagramEntity = entity };
			var viewModel = new NetworkUserControlViewModel(network);

			AssertEquals(true, viewModel.DiagramEntityViewModel.IsNotesPlaceholder(viewModel.DiagramEntityViewModel.NotesPlaceholder));
			AssertEquals(true, viewModel.DiagramEntityViewModel.IsNotesPlaceholder(viewModel.DiagramEntityViewModel.Notes));

			var notes = "some other notes";
			entity.Notes = notes;

			AssertEquals(notes, viewModel.DiagramEntityViewModel.Notes);
			AssertEquals(false, viewModel.DiagramEntityViewModel.IsNotesPlaceholder(viewModel.DiagramEntityViewModel.Notes));
		}

		public void TestNotes_Shape()
		{
			var entity = new Entity();
			entity.Notes = "Notes";
			var network = new DummyNetwork { DiagramEntity = entity };
			var viewModel = new NetworkUserControlViewModel(network);

			AssertEquals(true, viewModel.DiagramEntityViewModel.IsNotesPlaceholder(viewModel.DiagramEntityViewModel.NotesPlaceholder));
			AssertEquals(true, viewModel.DiagramEntityViewModel.IsNotesPlaceholder(viewModel.DiagramEntityViewModel.Notes));

			var notes = "some other notes";
			entity.Notes = notes;

			AssertEquals(notes, viewModel.DiagramEntityViewModel.Notes);
			AssertEquals(false, viewModel.DiagramEntityViewModel.IsNotesPlaceholder(viewModel.DiagramEntityViewModel.Notes));
		}
	}
}

