using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.Common;
using CargoWise.NetworkVisualisation.Integration;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.Business.Test
{
	public static class NetworkVisualisationTestHelper
	{
		#region Helper Methods

		#region Entity and Node Creation

		public static void AddEntityToNetwork(INetwork network, INetworkEntity entity)
		{
			network.Entities?.Add(entity); //having Entities null most likely means having a mocked network
		}

		public static NodeViewModel CreateEntityWithNodeAndAddToNetwork(NetworkViewModel networkViewModel, string entityName = null)
		{
			return AddEntityToNetworkAndCreateNode(networkViewModel, new Entity()
			{
				Name = entityName,
				Parent = networkViewModel.Network.DiagramEntity
			});
		}

		public static NodeViewModel AddEntityToNetworkAndCreateNode(NetworkViewModel networkViewModel, INetworkEntity entity)
		{
			AddEntityToNetwork(networkViewModel.Network, entity);
			return CreateNodeAndAddToNetworkViewModel(networkViewModel, entity);
		}

		public static NodeViewModel CreateNodeAndAddToNetworkViewModel(NetworkViewModel networkViewModel, INetworkEntity entity)
		{
			return networkViewModel.SetDefaultsForNewNode(entity, nodeLocation: null, centerNode: false); // we need a node to be created with all its connectors
		}

		public static Entity GetTestEntity(NodeViewModel nodeViewModel) => nodeViewModel.Entity as Entity;

		#endregion

		#region Entity Activation

		public static IDisposable TemporarilyActivateEntityForNetworkActions(INetworkViewModel networkViewModel, INetworkEntity entity)
		{
			var model = (NetworkViewModel)networkViewModel;
			Assertion.AssertNotNull(model.DiagramNodeViewModel);
			var activated = false;
			NodeViewModel previouslyRightClickedNode = null;
			List<INetworkEntity> previouslySelectedEntities = new List<INetworkEntity>(model.SelectedEntities);

			if (entity != null)
			{
				var node = model.GetNodeForEntity(entity);
				Assertion.AssertNotNull($"Ensure you've created a node view model for the given entity {entity.Name} (e.g. by creating all shapes before creating the network view model, or using methods like CreateNewShape() on NetworkViewModel, or calling Refresh() on NetworkViewModel) otherwise it won't be made active.", node);

				previouslyRightClickedNode = model.RightClickedNode;

				if (node == model.DiagramNodeViewModel)
				{
					model.RightClickedNode = model.DiagramNodeViewModel;
					model.SelectEntities(Array.Empty<INetworkEntity>());
					Assertion.AssertEquals($"Diagram entity {model.DiagramNodeViewModel.Entity.Name} should be active now, but alas {model.ActiveEntity.Name} is active instead", model.DiagramNodeViewModel.Entity.EntityPK, model.ActiveEntity.EntityPK);
				}
				else
				{
					model.RightClickedNode = node;

					if (!model.SelectedEntities.Any())
					{
						model.SelectSingleEntity(entity);
					}
					Assertion.AssertEquals($"Given entity {entity.Name} should be active now, but alas {model.ActiveEntity.Name} is active instead.", entity.EntityPK, model.ActiveEntity.EntityPK);
				}
				activated = true;

				return new DisposableAction(() =>
				{
					if (activated)
					{
						model.RightClickedNode = previouslyRightClickedNode;
						model.SelectEntities(previouslySelectedEntities);
					}
				});
			}

			return null;
		}

		#endregion

		#endregion

		#region Assertion Methods

		public static void AssertBackgroundColourBrush(NodeViewModel viewModel, params Color[] colors)
		{
			Assertion.AssertEquals(colors.Length, viewModel.StatusColors.colors.Count());
			var statusColors = viewModel.StatusColors.colors.ToList();
			for (var i = 0; i < colors.Length; i++)
			{
				var c = statusColors[i].color;
				Assertion.AssertEquals(colors[i].A, c.A);
				Assertion.AssertEquals(colors[i].R, c.R);
				Assertion.AssertEquals(colors[i].G, c.G);
				Assertion.AssertEquals(colors[i].B, c.B);
			}
		}
		#endregion
	}
}
