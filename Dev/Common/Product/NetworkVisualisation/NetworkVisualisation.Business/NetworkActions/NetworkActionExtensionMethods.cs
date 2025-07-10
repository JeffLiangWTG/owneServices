using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business
{
	public static class NetworkActionsExtensionMethods
	{
		#region Convert Network Actions to Menu Items

		public static NetworkActionMenuItem ToMenuItem(this INetworkAction action)
		{
			if (action == null)
			{
				return null;
			}

			return new NetworkActionMenuItem(
				name: action.GetName(),
				tooltip: NetworkActionHelper.GetTooltipForAction(action.GetDescription(), action.IsEnabled()),
				enabled: action.IsEnabled().IsAllowed,
				ticked: action.IsActivated(),
				items: action.GetChildActions().ToMenuItemsGrouped().ToArray(),
				action)
			{
			};
		}

		public static IEnumerable<NetworkActionMenuItem> ToMenuItemsGrouped(this IEnumerable<INetworkAction> actions)
		{
			if (actions == null)
			{
				yield break;
			}

			var actionGroups = actions
				.Where(a => a != null && a.IsApplicable().IsAllowed)
				.GroupBy(a => a.Group)
				.OrderBy(g => g.Key)
				.ToArray();

			for (var i = 0; i < actionGroups.Length; i++)
			{
				foreach (var action in actionGroups[i].OrderBy(a => a.GroupIndex))
				{
					var menuItem = action.ToMenuItem();

					if (menuItem != null)
					{
						yield return menuItem;
					}
				}

				if (i < actionGroups.Length - 1)
				{
					yield return null; //menu item separator
				}
			}
		}

		#endregion

		#region Network Action Accessibility

		public static INetworkActionAccessibility IsApplicableToEntity(this INetworkAction action, INetworkEntity entity)
		{
			if (action is StaticNetworkAction staticAction)
			{
				return staticAction.IsApplicable();
			}

			if (action is IDynamicNetworkAction dynamicAction)
			{
				return dynamicAction.IsApplicableToEntity(entity);
			}

			ReportUnknownNetworkActionType(action);
			return null;
		}

		public static INetworkActionAccessibility IsEnabledForEntity(this INetworkAction action, INetworkEntity entity)
		{
			if (action is StaticNetworkAction staticAction)
			{
				return staticAction.IsApplicable();
			}

			if (action is IDynamicNetworkAction dynamicAction)
			{
				return dynamicAction.IsEnabledForEntity(entity);
			}

			ReportUnknownNetworkActionType(action);
			return null;
		}

		static void ReportUnknownNetworkActionType(INetworkAction action)
		{
			ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "Unknown network action type: {0}", action));
		}

		#endregion
	}
}
