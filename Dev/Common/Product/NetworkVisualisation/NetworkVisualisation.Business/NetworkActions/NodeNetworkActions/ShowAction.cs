using System.Collections.Generic;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.NetworkVisualisation.Business
{
	public class ShowAction : DynamicNetworkAction
	{
		public ShowAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0)
			: base(networkViewModel, null, group, groupIndex, shouldUpdateOnNetworkEvents: false)
		{
		}

		#region Main Properties

		protected override ResourceString GetDefaultNameCore() => ResString.GetMultilingualString("1a3b886c-2fed-4ca2-bff8-63d63b684ef8", "Show");

		protected override ResourceString GetDefaultDescriptionCore() => ResString.GetMultilingualString("5659de22-1b25-4f6b-af4c-4d9b7768eb06", "Show existing items on this Diagram");

		protected override IEnumerable<INetworkAction> GetDefaultChildActionsCore()
		{
			yield return new ShowHiddenEntityAction(NetworkViewModel, shouldUpdateOnNetworkEvents: false);
			yield return new ShowHiddenDependencyAction(NetworkViewModel, shouldUpdateOnNetworkEvents: false);
		}

		#endregion

		#region Accessibility

		protected override INetworkActionAccessibility IsApplicableToEntityCore(INetworkEntity entity)
		{
			return new NetworkActionAccessibility(GetNode(entity).SupportsShowItems,
					entity,
					() => Res.GetString("1147D283-2DE9-4EF2-B57C-76906689E684", "The entity should support showing hidden objects on the diagram."));
		}

		protected override INetworkActionAccessibility IsEnabledForEntityCore(INetworkEntity entity) => NetworkActionAccessibility.Allowed;

		#endregion

		#region Execution

		protected override INetworkActionResult ExecuteForEntityCore(INetworkEntity entity)
		{
			return null;
		}

		#endregion
	}
}
