using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.GUI.Test
{
	class NetworkGuiTestConfig : IDisposable
	{
		#region Construction

		public static NetworkGuiTestConfig Create(Entity diagramEntity = null, bool shouldShowWindow = true, bool shouldSetDataContext = true)
		{
			return new NetworkGuiTestConfig(diagramEntity, shouldShowWindow, childEntities: null, shouldSetDataContext);
		}

		public static NetworkGuiTestConfig CreateWithNodes(Entity diagramEntity = null, bool shouldShowWindow = true)
		{
			return new NetworkGuiTestConfig(diagramEntity, shouldShowWindow, GetChildEntities(diagramEntity));
		}

		public static NetworkGuiTestConfig CreateWithNodesAndGrandChildren(Entity diagramEntity = null, bool shouldShowWindow = true)
		{
			return new NetworkGuiTestConfig(diagramEntity, shouldShowWindow, GetChildEntities(diagramEntity, createGrandChildren: true));
		}

		protected NetworkGuiTestConfig(Entity diagramEntity, bool shouldShowWindow, IEnumerable<INetworkEntity> childEntities, bool shouldSetDataContext = true)
		{
			diagramEntity = diagramEntity ?? new Entity();

			if (childEntities != null)
			{
				diagramEntity.AddChildEntities(childEntities);
			}

			Window = new Window();
			Refresher = new NetworkRefresher();
			Network = new DummyNetwork { Refresher = Refresher, DiagramEntity = diagramEntity };

			foreach (var child in diagramEntity.Children)
			{
				Network.Entities.Add(child);
			}

			Refresher.AssociateWithNetwork(Network);
			Network.DiagramEntity = diagramEntity;

#pragma warning disable CS0618
			Control = new NetworkUserControl(diagramEntity, Refresher, ribbonDataProvider: new TestRibbonDataProvider());
#pragma warning restore CS0618

			if (shouldSetDataContext)
			{
				Control.SetDataContext(Network, isReloading: false);
			}

			Window.Content = Control;

			Control.Height = 600;
			Control.Width = 800;
			Network.DiagramEntity.CornerRadius = 10;

			NetworkView = (NetworkView)Control.MainDiagramControl.FindName("NetworkControl");
			MouseController = NetworkView.MouseController;

			NetworkUserControlViewModel = (NetworkUserControlViewModel)Control.DataContext;

			if (shouldShowWindow)
			{
				ShowWindow();
			}
		}

		static IEnumerable<Entity> GetChildEntities(Entity diagramEntity, bool createGrandChildren = false)
		{
			var node1 = CreateEntity("Node1", 10, 20, 300, 150, false);
			yield return node1;

			var node2 = CreateEntity("Node2", 9, 19, 300, 150, false);
			yield return node2;

			var node3 = CreateEntity("Node3", 10, 20, 300, 250, false);
			node3.Parent = diagramEntity;
			yield return node3;

			if (createGrandChildren)
			{
				var node3c = CreateEntity("Node3Child", 50, 50, 200, 150, false);
				node3.AddChildEntity(node3c);
				node3c.Parent = node3;
				yield return node3c;

				var node3gc = CreateEntity("Node3GrandChild", 60, 60, 100, 50, false);
				node3c.AddChildEntity(node3gc);
				node3gc.Parent = node3c;
				yield return node3gc;
			}
		}

		protected static Entity CreateEntity(string name, double x, double y, double width, double height, bool isNonScheduled)
		{
			return new Entity
			{
				Name = name,
				X = x,
				Y = y,
				IsNonScheduled = isNonScheduled,
				Width = width,
				Height = height,
				IsDiagramScaled = true
			};
		}

		#endregion

		public void ShowWindow()
		{
			Window.Show();
			ApplicationHelper.DoEvents();
		}

		#region Properties

		public DummyNetwork Network { get; }
		public NetworkRefresher Refresher { get; }
#pragma warning disable CS0618
		public NetworkUserControl Control { get; }
#pragma warning restore CS0618
		public NetworkView NetworkView { get; }
		public NetworkViewMouseController MouseController { get; }
		public Window Window { get; }
		public NetworkUserControlViewModel NetworkUserControlViewModel { get; }
		public NetworkViewModel NetworkViewModel => NetworkUserControlViewModel.NetworkViewModel;
		public RibbonViewModel RibbonViewModel => NetworkUserControlViewModel.RibbonViewModel;

		public NodeViewModel Node1 => Control.MainDiagramControl.NetworkViewModel.Nodes.Single(x => x.Name == "Node1");
		public NodeViewModel Node2 => Control.MainDiagramControl.NetworkViewModel.Nodes.Single(x => x.Name == "Node2");
		public NodeViewModel Node3 => Control.MainDiagramControl.NetworkViewModel.Nodes.Single(x => x.Name == "Node3");
		public NodeViewModel Node3Child => Control.MainDiagramControl.NetworkViewModel.Nodes.Single(x => x.Name == "Node3Child");
		public NodeViewModel Node3GrandChild => Control.MainDiagramControl.NetworkViewModel.Nodes.Single(x => x.Name == "Node3GrandChild");

		#endregion

		#region Ribbon Actions

		public IEnumerable<RibbonButtonViewModel> AllRibbonButtons
		{
			get
			{
				foreach (var tab in Control.RibbonControlExposed_ForTesting.Model.Tabs)
				{
					foreach (var group in tab.Groups)
					{
						foreach (var button in group.Items)
						{
							yield return button;
						}
					}
				}
			}
		}

		public RibbonButtonViewModel FindOnlyRibbonButton<T>()
			where T : NetworkActionBase
		{
			return AllRibbonButtons.Single(x => x.Action is T);
		}

		public RibbonButtonViewModel FindOnlyRibbonButton(string actionName)
		{
			return AllRibbonButtons.Single(x => x.Action.GetName() == actionName);
		}

		public INetworkAction FindOnlyRibbonAction<T>()
			where T : NetworkActionBase
		{
			return FindOnlyRibbonButton<T>().Action;
		}

		public INetworkAction FindOnlyRibbonAction(string actionName)
		{
			return FindOnlyRibbonButton(actionName).Action;
		}

		#endregion

		public void Dispose()
		{
			ApplicationHelper.DoEvents();
			Window?.Close();
			Control.Dispose();
		}
	}
}
