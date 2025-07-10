using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business
{
	public interface INetworkActionExecutionStrategy
	{
		INetworkActionAccessibility IsActionApplicable(IDynamicNetworkAction action);

		INetworkActionAccessibility IsActionEnabled(IDynamicNetworkAction action);

		INetworkActionAccessibility CanActionStartExecution(IDynamicNetworkAction action);

		INetworkActionResult ExecuteAction(IDynamicNetworkAction action);
	}
}
