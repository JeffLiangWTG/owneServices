using System.Collections.Generic;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.NetworkVisualisation.Business
{
	public class AffinitiesAction : DynamicNetworkAction
	{
		public AffinitiesAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0)
			: base(networkViewModel, new MultipleSelectedEntitiesExecutionStrategy(), group, groupIndex, shouldUpdateOnNetworkEvents: false)
		{
		}

		#region Main Properties

		protected override ResourceString GetDefaultNameCore() => ResString.GetMultilingualString("b9a402fb-36cd-43a3-95aa-ec41b7c6e3ca", "Affinities");

		protected override ResourceString GetDefaultDescriptionCore() => ResString.GetMultilingualString("88f6faca-d738-4180-9419-2c5be179ec57", "Create and modify Affinities for this diagram");

		protected override IEnumerable<INetworkAction> GetDefaultChildActionsCore()
		{
			return GetAffinitiesActions(GetNetworkViewModel());
		}

		#endregion

		#region Accessibility

		protected override INetworkActionAccessibility IsApplicableToEntityCore(INetworkEntity entity)
		{
			return new NetworkActionAccessibility(GetNode(entity).SupportsAffinities,
					entity,
					() => Res.GetString("2125468E-493A-4584-AADC-FC8E04C0AA3B", "The entity should support affinities."));
		}

		protected override INetworkActionAccessibility IsEnabledForEntityCore(INetworkEntity entity) => NetworkActionAccessibility.Allowed;

		#endregion

		#region Execution

		protected override INetworkActionResult ExecuteForEntityCore(INetworkEntity entity)
		{
			return null;
		}

		#endregion

		#region Implementation

		static IEnumerable<INetworkAction> GetAffinitiesActions(NetworkViewModel networkViewModel)
		{
			yield return new EditAffinitiesAction(networkViewModel, shouldUpdateOnNetworkEvents: false);
			yield return new ApplyAffinitiesAction(networkViewModel, shouldUpdateOnNetworkEvents: false);
			yield return new RemoveAffinitiesAction(networkViewModel, shouldUpdateOnNetworkEvents: false);
		}

		#endregion
	}
}
