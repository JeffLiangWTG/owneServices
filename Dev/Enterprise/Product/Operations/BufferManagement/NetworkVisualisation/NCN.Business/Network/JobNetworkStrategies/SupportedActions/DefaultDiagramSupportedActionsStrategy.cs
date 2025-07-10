using CargoWise.NetworkVisualisation.Integration;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	class DefaultDiagramSupportedActionsStrategy : JobNetworkSupportedActionsStrategy
	{
		internal override NetworkActions GetSupportedActions()
		{
			return NetworkActions.GenericActions;
		}
	}
}
