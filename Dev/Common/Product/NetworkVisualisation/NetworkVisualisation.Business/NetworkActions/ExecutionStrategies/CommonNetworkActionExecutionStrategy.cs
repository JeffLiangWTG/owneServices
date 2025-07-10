using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business
{
	public class CommonNetworkActionExecutionStrategy : NetworkActionExecutionStrategyBase
	{
		protected override INetworkActionAccessibility IsActionApplicableCore(IDynamicNetworkAction action)
		{
			return action.IsApplicableToEntity(GetEntityToExecute(action));
		}

		protected override INetworkActionAccessibility IsActionEnabledCore(IDynamicNetworkAction action)
		{
			return action.IsEnabledForEntity(GetEntityToExecute(action));
		}

		protected override INetworkActionAccessibility CanActionStartExecutionCore(IDynamicNetworkAction action)
		{
			return action.PerformPreExecutionChecksForEntity(GetEntityToExecute(action)).UnionIfAllowed(() => action.CheckCanStartExecutionForNetwork());
		}

		protected override INetworkActionResult ExecuteCore(IDynamicNetworkAction action)
		{
			if (!CheckCanStartExecution(action))
			{
				return null;
			}

			return action.ExecuteForEntityWithoutAccessCheck(GetEntityToExecute(action));
		}

		static INetworkEntity GetEntityToExecute(IDynamicNetworkAction action) => action.NetworkViewModel.ActiveEntity;
	}
}
