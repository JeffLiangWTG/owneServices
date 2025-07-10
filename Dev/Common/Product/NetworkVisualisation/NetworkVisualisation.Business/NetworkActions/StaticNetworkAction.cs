using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.NetworkVisualisation.Business
{
	public class StaticNetworkAction : NetworkActionBase
	{
		public StaticNetworkAction(ResourceString name = null, ResourceString description = null, INetworkActionAccessibility isEnabled = null, bool isActivated = false, IEnumerable<INetworkAction> childActions = null, int group = 0, int groupIndex = 0, string iconName = "")
			: this(null, name, description, isEnabled, isActivated, childActions, group, groupIndex, iconName)
		{
		}

		public StaticNetworkAction(Action action, ResourceString name = null, ResourceString description = null, INetworkActionAccessibility isEnabled = null, bool isActivated = false, IEnumerable<INetworkAction> childActions = null, int group = 0, int groupIndex = 0, string iconName = "")
			: this(() =>
			{
				action?.Invoke();
				return new EmptyActionResult();
			}, name, description, isEnabled, isActivated, childActions, group, groupIndex, iconName)
		{
		}

		public StaticNetworkAction(Func<INetworkActionResult> action, ResourceString name = null, ResourceString description = null, INetworkActionAccessibility isEnabled = null, bool isActivated = false, IEnumerable<INetworkAction> childActions = null, int group = 0, int groupIndex = 0, string iconName = "")
		{
			this.name = name;
			this.description = description;
			this.isEnabled = isEnabled ?? NetworkActionAccessibility.Allowed;
			this.isActivated = isActivated;
			this.iconName = iconName;
			ChildActions = childActions ?? Enumerable.Empty<INetworkAction>();

			this.action = action ?? (() => new EmptyActionResult());

			Group = group;
			GroupIndex = groupIndex;
		}

		readonly ResourceString name;

		readonly ResourceString description;

		readonly bool isActivated;

		readonly IEnumerable<INetworkAction> ChildActions;

		readonly INetworkActionAccessibility isEnabled;

		readonly string iconName;

		#region Implementation

		readonly Func<INetworkActionResult> action;

		#region NetworkActionBase Overrides

		protected sealed override ResourceString GetNameCore() => name;

		protected sealed override ResourceString GetDescriptionCore() => description;

		protected sealed override string GetIconNameCore() => iconName;

		protected sealed override bool IsActivatedCore() => isActivated;

		protected sealed override IEnumerable<INetworkAction> GetChildActionsCore() => ChildActions;

		protected sealed override INetworkActionAccessibility IsApplicableCore() => NetworkActionAccessibility.Allowed; // static network actions do not change their state so not applicable static action makes no sense

		protected sealed override INetworkActionAccessibility IsEnabledCore() => isEnabled;

		protected override INetworkActionAccessibility CheckCanStartExecutionCore() => NetworkActionAccessibility.Allowed;

		protected override INetworkActionResult ExecuteCore() => CheckCanStartExecution().IsAllowed ? action?.Invoke() : null;

		#endregion

		#endregion
	}
}
