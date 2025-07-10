using System.Linq;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business
{
	public abstract class NetworkActionExecutionStrategyBase : INetworkActionExecutionStrategy
	{
		public INetworkActionAccessibility IsActionApplicable(IDynamicNetworkAction action) => IsActionApplicableCore(action);

		public INetworkActionAccessibility IsActionEnabled(IDynamicNetworkAction action) => IsActionEnabledCore(action);

		public INetworkActionAccessibility CanActionStartExecution(IDynamicNetworkAction action) => CanActionStartExecutionCore(action);

		public INetworkActionResult ExecuteAction(IDynamicNetworkAction action) => ExecuteCore(action);

		#region Abstract and Virtual Methods

		protected abstract INetworkActionAccessibility IsActionApplicableCore(IDynamicNetworkAction action);

		protected abstract INetworkActionAccessibility IsActionEnabledCore(IDynamicNetworkAction action);

		protected abstract INetworkActionAccessibility CanActionStartExecutionCore(IDynamicNetworkAction action);

		protected abstract INetworkActionResult ExecuteCore(IDynamicNetworkAction action);

		#endregion

		#region Implementation

		protected bool CheckCanStartExecution(IDynamicNetworkAction action)
		{
			var overallAccessibility = action.CheckCanStartExecution();

			if (!overallAccessibility.IsAllowed && overallAccessibility.DenialReasons.Any(r => r.NeedsNotification))
			{
				action.NetworkViewModel.Controller?.NotifyActionCannotBeExecuted(overallAccessibility);
			}

			return overallAccessibility.IsAllowed;
		}

		protected INetworkEntityController GetController(IDynamicNetworkAction action) => action.NetworkViewModel.Controller;

		#endregion
	}
}
