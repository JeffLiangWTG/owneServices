using System;
using System.Collections.Generic;

namespace CargoWise.NetworkVisualisation.Integration
{
	public interface INetwork
	{
		string Name { get; set; }
		bool IsReadOnly { get; }

		IDiagramEntity DiagramEntity { get; }
		IObservableReloadableCollection<INetworkEntity> Entities { get; }

		INetworkScaleDescriptor ScaleDescriptor { get; }
		IEntityPositionStrategy EntityPositionStrategy { get; }

		INetworkEntityController Controller { get; }

		#region Entity Operations

		bool DeleteEntity(IProposedNetworkEntity entity);
		IEnumerable<IProposedNetworkEntity> HideEntity(IProposedNetworkEntity entity);
		IEnumerable<INetworkEntity> ShowEntity(IProposedNetworkEntity entity, INetworkEntity parentEntity);

		#endregion

		#region Entity Relationship Operations

		bool DeleteRelationship(IEntityRelationship relationship);
		IEntityRelationship CreateRelationship(IProposedNetworkEntity sourceEntity, IProposedNetworkEntity destEntity);
		bool HideRelationship(IEntityRelationship relationship);
		IEntityRelationship ShowRelationship(IProposedNetworkEntity sourceEntity, IProposedNetworkEntity destEntity);
		IEntityRelationship GetRelationship(IProposedNetworkEntity sourceEntity, IProposedNetworkEntity destEntity);

		#endregion

		#region Refresh

		INetworkRefresher Refresher { get; }

		#endregion

		#region Special Interface Operations

		#region Paste

		bool TryHandlePaste(IEnumerable<INetworkEntity> selectedEntities);

		#endregion

		#region Open External Form

		void ModifyAffinities(IDiagramEntity diagramEntity);
		void EditEntity(IProposedNetworkEntity entity);
		void ViewEntity(IProposedNetworkEntity entity);
		IEnumerable<INetworkEntity> PickAndImportEntities(IProposedNetworkEntity parentEntity);

		#endregion

		IDisposable SuspendRefreshingOnEntityCountChanged();

		#endregion

		#region Custom Network Actions

		IEnumerable<INetworkAction> GetCustomNetworkActions(INetworkViewModel networkViewModel);
		IEnumerable<INetworkAction> GetCreateEntityActions(INetworkViewModel networkViewModel);

		#endregion

		#region CopyPasteShapes

		bool TryCopyShapeStateToClipBoard(IEnumerable<INetworkEntity> shapeStates);
		INetworkActionResult PasteShapeFromClipBoard(INetworkViewModel networkViewModel);

		#endregion
	}
}
