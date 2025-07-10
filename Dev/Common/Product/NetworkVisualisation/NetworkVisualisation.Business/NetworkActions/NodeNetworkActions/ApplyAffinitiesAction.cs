using System.Collections.Generic;
using System.Linq;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.NetworkVisualisation.Business
{
	public class ApplyAffinitiesAction : DynamicNetworkAction
	{
		public ApplyAffinitiesAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, new MultipleSelectedEntitiesExecutionStrategy(RefreshType.Affinities), group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		#region Main Properties

		protected override ResourceString GetDefaultNameCore() => ResString.GetMultilingualString("2a25a844-dc7c-400d-a72e-f7164433dd5f", "Apply Available Affinities");

		protected override ResourceString GetDefaultDescriptionCore() => ResString.GetMultilingualString("8cac9d82-0869-436a-beae-7de95d20719d", "Select Available Affinities that apply to this Diagram");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "resource name")]
		protected override string IconName => "Plus";

		protected override IEnumerable<INetworkAction> GetChildActionsCore(INetworkEntity activeEntity)
		{
			return GetApplyAffinityActions(ShouldUpdateOnNetworkEvents);
		}

		#endregion

		#region Accessibility

		protected override INetworkActionAccessibility IsApplicableToEntityCore(INetworkEntity entity) => NetworkActionAccessibility.Allowed;

		protected override INetworkActionAccessibility IsEnabledForEntityCore(INetworkEntity entity)
		{
			return new NetworkActionAccessibility(GetApplyAffinityActions(shouldUpdateOnNetworkEvents: false).Any(a => a.IsEnabledForEntity(entity).IsAllowed),
				entity,
				() => Res.GetString("80C492AA-B8C5-4D8E-9011-4CBEEB2085E5", "There should be at least one affinity defined for the diagram which is not applied to the entity."));
		}

		#endregion

		#region Execution

		protected override INetworkActionResult ExecuteForEntityCore(INetworkEntity entity)
		{
			return null;
		}

		#endregion

		#region Implementation

		IEnumerable<INetworkAction> GetApplyAffinityActions(bool shouldUpdateOnNetworkEvents)
		{
			var availableAffinities = NetworkViewModel.Network.DiagramEntity.AvailableAffinities.Where(a => NetworkViewModel.SelectedEntities.Any(e => e.AvailableAffinities.Contains(a)));

			foreach (var affinity in availableAffinities)
			{
				yield return new ApplyAffinityChildAction(NetworkViewModel, affinity, shouldUpdateOnNetworkEvents);
			}
		}

		#endregion

		#region ApplyAffinityChildAction

		public class ApplyAffinityChildAction : DynamicNetworkAction
		{
			public ApplyAffinityChildAction(INetworkViewModel networkViewModel, IAffinity affinity, bool shouldUpdateOnNetworkEvents)
				: base(networkViewModel, new MultipleSelectedEntitiesExecutionStrategy(RefreshType.Affinities), shouldUpdateOnNetworkEvents: shouldUpdateOnNetworkEvents)
			{
				this.affinity = affinity;
			}

			readonly IAffinity affinity;

			#region Main Properties

			protected override ResourceString GetDefaultNameCore() => ResString.GetMultilingualString("CCA35EEC-7CC1-4825-84AD-6B37A2EC1C54", "{0}", affinity.Name);

			protected override ResourceString GetDefaultDescriptionCore() => null;

			#endregion

			#region Accessibility

			protected override INetworkActionAccessibility IsApplicableToEntityCore(INetworkEntity entity) => NetworkActionAccessibility.Allowed;

			protected override INetworkActionAccessibility IsEnabledForEntityCore(INetworkEntity entity)
			{
				return new NetworkActionAccessibility(entity.AvailableAffinities.Contains(affinity) && !entity.AppliedAffinities.Contains(affinity),
					() => new NetworkActionDenialReason(entity,
						ResString.GetMultilingualString("62FC6DCA-C683-4335-94F3-0285CD7E27B8", "The affinity {0} is already applied.", affinity.Name),
						needsNotification: false));
			}

			#endregion

			#region Execution

			protected override INetworkActionResult ExecuteForEntityCore(INetworkEntity entity)
			{
				NetworkViewModel.CreateAffinityLink(affinity, entity);
				return null;
			}

			#endregion
		}

		#endregion
	}
}
