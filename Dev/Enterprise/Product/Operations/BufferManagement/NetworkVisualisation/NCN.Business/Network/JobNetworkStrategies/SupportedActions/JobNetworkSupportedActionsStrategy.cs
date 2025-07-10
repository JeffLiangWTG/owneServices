using CargoWise.NetworkVisualisation.Integration;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	class JobNetworkSupportedActionsStrategy
	{
		internal virtual NetworkActions GetSupportedActions()
		{
			return NetworkActions.Hide
				| NetworkActions.Show
				| NetworkActions.StyleDiagram
				| NetworkActions.EditEntity
				| NetworkActions.AddChildEntities
				| NetworkActions.GenericActions;
		}
	}
}
