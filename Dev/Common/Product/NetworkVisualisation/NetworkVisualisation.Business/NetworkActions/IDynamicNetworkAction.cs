using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business
{
	public interface IDynamicNetworkAction : INetworkAction
	{
		INetworkViewModel NetworkViewModel { get; }

		#region Accessibility

		INetworkActionAccessibility IsApplicableToEntity(INetworkEntity entity);

		INetworkActionAccessibility IsEnabledForEntity(INetworkEntity entity);

		INetworkActionAccessibility PerformPreExecutionChecksForEntity(INetworkEntity entity);

		INetworkActionAccessibility CheckCanStartExecutionForNetwork();

		#endregion

		#region Execution

		INetworkActionResult ExecuteForEntityWithoutAccessCheck(INetworkEntity entityToExecute);

		#endregion
	}
}
