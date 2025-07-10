using System.Collections.Generic;
using System.Linq;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.NetworkVisualisation.Business
{
	public class CreateNewAction : DynamicNetworkAction
	{
		public CreateNewAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, null, group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		#region Main Properties

		protected override ResourceString GetDefaultNameCore() => ResString.GetMultilingualString("924e95a5-00d8-4772-ad9b-e10dd8ca2241", "Create New...");

		protected override ResourceString GetDefaultDescriptionCore() => ResString.GetMultilingualString("49d512f3-1481-459f-99f7-ebbf45b7ffc1", "Creates a new shape and adds it to the diagram");

		protected override IEnumerable<INetworkAction> GetDefaultChildActionsCore()
		{
			return GetNetworkViewModel().GetCreateEntityActions();
		}

		#endregion

		#region Accessibility

		protected override INetworkActionAccessibility IsApplicableToEntityCore(INetworkEntity entity)
		{
			return new NetworkActionAccessibility(GetNetworkViewModel().GetCreateEntityActions().Any(a => a.IsApplicableToEntity(entity).IsAllowed),
				entity,
				() => Res.GetString("B8659155-4138-4385-A9BB-FE0761A753F4", "The entity should support create entity actions."));
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
