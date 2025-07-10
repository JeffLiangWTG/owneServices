using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business
{
	public class DummyNetwork : INetwork
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Diagram entity name for debugging")]
		public DummyNetwork()
		{
			DiagramEntity = new Entity()
			{
				Name = "Dummy Diagram Entity"
			};
			entities = new ImpObservableSet<INetworkEntity>();
			hiddenEntities = new List<IProposedNetworkEntity>();
			hiddenRelationships = new List<IEntityRelationship>();

			SupportedActions = NetworkActions.StyleDiagram;
		}

		readonly IObservableReloadableCollection<INetworkEntity> entities;
		readonly List<IProposedNetworkEntity> hiddenEntities;
		readonly List<IEntityRelationship> hiddenRelationships;

		public IObservableReloadableCollection<INetworkEntity> Entities
		{
			get { return entities; }
		}

		public INetworkScaleDescriptor ScaleDescriptor
		{
			get { return null; }
		}

		public IEntityPositionStrategy EntityPositionStrategy
		{
			get { return new EntityPositionStrategy(); }
		}

		public string Name { get; set; }
		public bool IsReadOnly { get; set; }
		public IDiagramEntity DiagramEntity { get; set; }
		public IProposedNetworkEntity SelectedEntity { get; set; }
		public NetworkActions SupportedActions { get; set; }

		public INetworkEntityController Controller { get; set; }

		public INetworkEntity CreateNewEntity(string shapeType, bool isNonScheduled = false)
		{
			var entity = new Entity { ShapeType = shapeType, IsNonScheduled = isNonScheduled };
			entities.Add(entity);

			return entity;
		}

		public IDisposable SuspendRefreshingOnEntityCountChanged()
		{
			return null;
		}

		#region SuppressResourceStringsCheckRegion

		public static INetwork GetDummyNetwork(bool checkForDesignMode)
		{
			if (checkForDesignMode && IsInDesignMode())
			{
				return new DummyNetwork();
			}
			else
			{
				return new DummyNetwork { Name = Res.GetString("e30d35b5-7629-497e-b9a9-42178287aaf8", "Loading"), DiagramEntity = new Entity() };
			}
		}

		/// <summary>
		/// There is a bug in the WPF control way of checking that the designer is attached.
		/// http://stackoverflow.com/questions/425760/is-there-a-designmode-property-in-wpf
		/// </summary>
		static bool IsInDesignMode()
		{
			return System.Reflection.Assembly.GetExecutingAssembly().Location.Contains("VisualStudio");
		}

		#endregion

		#region Entity Operations

		public bool DeleteEntity(IProposedNetworkEntity entity)
		{
			return entities.Remove((INetworkEntity)entity);
		}

		public IEnumerable<IProposedNetworkEntity> HideEntity(IProposedNetworkEntity entity)
		{
			entities.Remove((INetworkEntity)entity);
			hiddenEntities.Add(entity);

			return new[] { entity };
		}

		public IEnumerable<INetworkEntity> ShowEntity(IProposedNetworkEntity entity, INetworkEntity parentEntity)
		{
			if (hiddenEntities.Contains(entity))
			{
				hiddenEntities.Remove(entity);
			}
			INetworkEntity proposedNetworkEntity = (INetworkEntity)entity;
			entities.Add(proposedNetworkEntity);

			return new[] { proposedNetworkEntity };
		}

		#endregion

		#region Entity Relationship Operations

		public bool DeleteRelationship(IEntityRelationship relationship)
		{
			foreach (var entity in Entities)
			{
				var existingRelationship = entity.Links.FirstOrDefault(r => r.From == relationship.From && r.To == relationship.To);
				if (existingRelationship != null)
				{
					((Entity)entity).Links.Remove(existingRelationship);
				}
			}

			return true;
		}

		public virtual IEntityRelationship CreateRelationship(IProposedNetworkEntity sourceEntity, IProposedNetworkEntity destEntity)
		{
			var relationship = new Relationship { From = sourceEntity, To = destEntity };

			((Entity)sourceEntity).Links.Add(relationship);
			((Entity)destEntity).Links.Add(relationship);

			return relationship;
		}

		public bool HideRelationship(IEntityRelationship relationship)
		{
			if (relationship.From != null && relationship.From.Links.Contains(relationship))
			{
				((Entity)relationship.From).Links.Remove(relationship);
			}
			if (relationship.To != null && relationship.To.Links.Contains(relationship))
			{
				((Entity)relationship.To).Links.Remove(relationship);
			}

			hiddenRelationships.Add(relationship);

			return true;
		}

		public IEntityRelationship ShowRelationship(IProposedNetworkEntity sourceEntity, IProposedNetworkEntity destEntity)
		{
			var relationship = hiddenRelationships.FirstOrDefault(r => r.From == sourceEntity && r.To == destEntity);
			if (relationship != null)
			{
				hiddenRelationships.Remove(relationship);
			}

			return relationship;
		}
		public IEntityRelationship GetRelationship(IProposedNetworkEntity sourceEntity, IProposedNetworkEntity destEntity)
		{
			return ShowRelationship(sourceEntity, destEntity);
		}

		#endregion

		#region Special Interface Operations

		#region Paste

		public bool TryHandlePaste(IEnumerable<INetworkEntity> selectedEntities)
		{
			return false;
		}

		#endregion

		#region Open External Form

		void INetwork.EditEntity(IProposedNetworkEntity entity)
		{
		}

		IEnumerable<INetworkEntity> INetwork.PickAndImportEntities(IProposedNetworkEntity parentEntity)
		{
			yield break;
		}

		void INetwork.ModifyAffinities(IDiagramEntity diagramEntity)
		{
			//nothing
		}

		public void ViewEntity(IProposedNetworkEntity entity)
		{
		}

		#endregion

		#endregion

		#region Refresh

		public INetworkRefresher Refresher
		{
			get { return refresher ?? (refresher = new NetworkRefresher()); }
			set { refresher = value; }
		}

		INetworkRefresher refresher;

		#endregion

		#region Custom Network Actions

		public IEnumerable<INetworkAction> GetCreateEntityActions(INetworkViewModel networkViewModel)
		{
			return сreateEntityActions;
		}
		readonly List<INetworkAction> сreateEntityActions = new List<INetworkAction>();

#if DEBUG
		public void AddCreateEntityAction_ForTest(INetworkAction action)
		{
			сreateEntityActions.Add(action);
		}
#endif

		public IEnumerable<INetworkAction> GetCustomNetworkActions(INetworkViewModel networkViewModel)
		{
			return customNetworkActions;
		}
		readonly List<INetworkAction> customNetworkActions = new List<INetworkAction>();

#if DEBUG
		public void AddCustomNetworkAction_ForTest(INetworkAction action)
		{
			customNetworkActions.Add(action);
		}

		public void ClearCustomNetworkActions_ForTest()
		{
			customNetworkActions.Clear();
		}

#endif
		public bool TryCopyShapeStateToClipBoard(IEnumerable<INetworkEntity> shapeStates)
		{
			throw new NotImplementedException();
		}

		public INetworkActionResult PasteShapeFromClipBoard(INetworkViewModel networkViewModel)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
