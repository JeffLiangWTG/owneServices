using System.Collections.Generic;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.NetworkVisualisation.Business
{
	public abstract class NetworkActionBase : INetworkAction
	{
		protected NetworkActionBase(int group = 0, int groupIndex = 0)
		{
			Group = group;
			GroupIndex = groupIndex;
		}

		#region Main Network Action Attributes

		public ResourceString GetName() => GetNameCore();

		public ResourceString GetDescription() => GetDescriptionCore();

		public string GetIconName() => GetIconNameCore();

		public bool IsActivated() => IsActivatedCore();

		public IEnumerable<INetworkAction> GetChildActions() => GetChildActionsCore();

		#endregion

		#region Accessibility

		// There are three levels of accessibility of network actions:
		// - IsApplicable - silently determines whether the network action should appear in the context menu (buttons in the ribbon are always visible).
		//		Also for "multiactions" (actions which perform for multiple selected entities) defines whether the action can be executed: it can't if it is not applicable for at least one of the selected entities.
		//		So this property is not purely an equivalent of visibility.
		// - IsEnabled - silently determines whether the network action should be enabled in the context menu and ribbon.
		//		Similarly for "multiactions" defines whether the action can be executed: it can if it is enabled for at least one of the selected entities.
		//		Not applicable actions are automatically not enabled.
		// - CheckCanStartExecution - determines whether the network action can start execution and shows reasons why the action cannot be executed if there are any.
		//		May include some user confirmations or heavy calculations otherwise simply defined by enabledness.
		//		Not enabled actions are automatically cannot start execution.

		public INetworkActionAccessibility IsApplicable() => IsApplicableCore();

		public INetworkActionAccessibility IsEnabled() => IsApplicable().UnionIfAllowed(() => IsEnabledCore());

		public INetworkActionAccessibility CheckCanStartExecution() => IsEnabled().UnionIfAllowed(() => CheckCanStartExecutionCore());

		#endregion

		#region Execution

		public INetworkActionResult Execute() => ExecuteCore();

		#endregion

		#region Grouping

		public int Group { get; set; }

		public int GroupIndex { get; set; }

		public void Refresh(RefreshArgs args)
		{
			RefreshCore(args);
		}

		protected virtual void RefreshCore(RefreshArgs args)
		{
			// Non-dynamic actions don't need to be refreshed.
		}

		#endregion

		public override string ToString() => GetType().Name + ": " + GetName();

		#region Abstract methods

		protected abstract ResourceString GetNameCore();

		protected abstract ResourceString GetDescriptionCore();

		protected abstract string GetIconNameCore();

		protected abstract bool IsActivatedCore();

		protected abstract IEnumerable<INetworkAction> GetChildActionsCore();

		protected abstract INetworkActionAccessibility IsApplicableCore();

		protected abstract INetworkActionAccessibility IsEnabledCore();

		protected abstract INetworkActionAccessibility CheckCanStartExecutionCore();

		protected abstract INetworkActionResult ExecuteCore();

		#endregion
	}
}
