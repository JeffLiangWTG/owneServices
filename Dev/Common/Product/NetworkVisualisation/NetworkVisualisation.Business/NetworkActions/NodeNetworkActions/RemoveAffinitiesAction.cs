using System.Collections.Generic;
using System.Linq;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.NetworkVisualisation.Business
{
	public class RemoveAffinitiesAction : DynamicNetworkAction
	{
		public RemoveAffinitiesAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, new MultipleSelectedEntitiesExecutionStrategy(RefreshType.Affinities), group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		#region Main Properties

		protected override ResourceString GetDefaultNameCore() => ResString.GetMultilingualString("3aaba4b3-1372-430e-8180-66916925480c", "Remove Applied Affinities");

		protected override ResourceString GetDefaultDescriptionCore() => ResString.GetMultilingualString("7fd78040-9cef-49fc-b091-13b0afa09886", "Select Applied Affinities that no longer apply to this Diagram");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "resource name")]
		protected override string IconName => "Minus";

		protected override IEnumerable<INetworkAction> GetChildActionsCore(INetworkEntity activeEntity)
		{
			return GetRemoveAffinityActions(ShouldUpdateOnNetworkEvents);
		}

		#endregion

		#region Accessibility

		protected override INetworkActionAccessibility IsApplicableToEntityCore(INetworkEntity entity) => NetworkActionAccessibility.Allowed;

		protected override INetworkActionAccessibility IsEnabledForEntityCore(INetworkEntity entity)
		{
			return new NetworkActionAccessibility(GetRemoveAffinityActions(shouldUpdateOnNetworkEvents: false).Any(a => a.IsEnabledForEntity(entity).IsAllowed),
				entity,
				() => Res.GetString("DC9314E8-EC75-4CBB-B5CA-BAC9CD66585A", "There should be at least one affinity applied."));
		}

		#endregion

		#region Execution

		protected override INetworkActionResult ExecuteForEntityCore(INetworkEntity entity)
		{
			return null;
		}

		#endregion

		#region Implementation

		IEnumerable<INetworkAction> GetRemoveAffinityActions(bool shouldUpdateOnNetworkEvents)
		{
			var appliedAffinities = NetworkViewModel.Network.DiagramEntity.AvailableAffinities.Where(a => NetworkViewModel.SelectedEntities.Any(e => e.AppliedAffinities.Contains(a)));

			foreach (var affinity in appliedAffinities)
			{
				yield return new RemoveAffinityChildAction(NetworkViewModel, affinity, shouldUpdateOnNetworkEvents);
			}
		}

		#endregion

		#region RemoveAffinityChildAction

		public class RemoveAffinityChildAction : DynamicNetworkAction
		{
			public RemoveAffinityChildAction(INetworkViewModel networkViewModel, IAffinity affinity, bool shouldUpdateOnNetworkEvents)
				: base(networkViewModel, new MultipleSelectedEntitiesExecutionStrategy(RefreshType.Affinities), shouldUpdateOnNetworkEvents: shouldUpdateOnNetworkEvents)
			{
				this.affinity = affinity;
			}

			readonly IAffinity affinity;

			#region Main Properties

			protected override ResourceString GetDefaultNameCore() => ResString.GetMultilingualString("E6E3FA32-1BFA-4A68-A1FC-312B9514F203", "{0}", affinity.Name);

			protected override ResourceString GetDefaultDescriptionCore() => null;

			#endregion

			#region Accessibility

			protected override INetworkActionAccessibility IsApplicableToEntityCore(INetworkEntity entity) => NetworkActionAccessibility.Allowed;

			protected override INetworkActionAccessibility IsEnabledForEntityCore(INetworkEntity entity)
			{
				return new NetworkActionAccessibility(entity.AppliedAffinities.Contains(affinity),
					() => new NetworkActionDenialReason(entity,
						ResString.GetMultilingualString("7F9861AD-8310-45E6-92FF-3B738CC80760", "Cannot remove the affinity {0} as it is not applied.", affinity.Name),
						needsNotification: false));
			}

			#endregion

			#region Execution

			protected override INetworkActionResult ExecuteForEntityCore(INetworkEntity entity)
			{
				NetworkViewModel.RemoveAffinityLink(affinity, entity);
				return null;
			}

			#endregion
		}

		#endregion
	}
}
