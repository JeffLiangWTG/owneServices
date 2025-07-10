using System;
using System.Collections.Generic;

namespace CargoWise.NetworkVisualisation.Integration
{
	public interface INetworkViewModel
	{
		INetwork Network { get; }

		INetworkEntityController Controller { get; }

		INetworkEntity ActiveEntity { get; }

		INetworkEntity FirstSelectedEntity { get; }

		IEnumerable<INetworkEntity> SelectedEntities { get; }

		void CreateAffinityLink(IAffinity affinity, IProposedNetworkEntity proposedEntity);

		void RemoveAffinityLink(IAffinity affinity, IProposedNetworkEntity proposedEntity);

		event EventHandler<RefreshArgs> SelectionChanged;

		void SelectEntities(IEnumerable<INetworkEntity> entitiesToSelect);
	}
}
