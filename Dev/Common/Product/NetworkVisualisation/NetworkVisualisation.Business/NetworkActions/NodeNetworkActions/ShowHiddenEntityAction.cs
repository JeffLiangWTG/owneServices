using System.Collections.Generic;
using System.Linq;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.NetworkVisualisation.Business
{
	public class ShowHiddenEntityAction : DynamicNetworkAction
	{
		public ShowHiddenEntityAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, null, group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		#region Main Properties

		protected override ResourceString GetDefaultNameCore() => ResString.GetMultilingualString("e6ded8c6-b5da-4bb5-8d62-afc35a8e6179", "Entity");

		protected override ResourceString GetDefaultDescriptionCore() => ResString.GetMultilingualString("7d9bd700-42a6-4022-9f9c-70f7653e053c", "Show an existing entity on this diagram");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "resource name")]
		protected override string IconName => "Entity";

		protected override IEnumerable<INetworkAction> GetChildActionsCore(INetworkEntity activeEntity)
		{
			return GetHiddenEntityActions(GetNetworkViewModel(), GetNode(activeEntity)).ToArray();
		}

		#endregion

		#region Accessibility

		protected override INetworkActionAccessibility IsApplicableToEntityCore(INetworkEntity entity) => NetworkActionAccessibility.Allowed;

		protected override INetworkActionAccessibility IsEnabledForEntityCore(INetworkEntity entity)
		{
			return new NetworkActionAccessibility(GetNode(entity).HiddenEntities.Any(),
				entity,
				() => Res.GetString("8D0AA057-ADE4-4C09-BBC8-A687FB257DE0", "There should be at least one hidden entity."));
		}

		#endregion

		#region Execution

		protected override INetworkActionResult ExecuteForEntityCore(INetworkEntity entity)
		{
			return null;
		}

		#endregion

		#region Implementation
		static IEnumerable<INetworkAction> GetHiddenEntityActions(NetworkViewModel networkViewModel, NodeViewModel node)
		{
			node.HiddenEntities.Reload();

			if (node.HiddenEntities.Any())
			{
				yield return new StaticNetworkAction(() =>
				{
					foreach (var hiddenEntity in node.HiddenEntities.ToList())
					{
						var entity = networkViewModel.Network.ShowEntity(hiddenEntity, node.Entity);
						entity = networkViewModel.Network.EntityPositionStrategy.SetPositionsForNewEntities(entity, networkViewModel.Network.Entities, new Location(0, 0));
						node.NetworkViewModel.AddAllEntitiesIntoDiagram(entity, new Location() { X = entity.First().X, Y = entity.First().Y });
					}
				}, name: ResString.GetMultilingualString("9601815a-f7cd-4d28-993d-1917c262b79e", "All"), group: 0);
			}

			foreach (var hiddenEntity in node.HiddenEntities)
			{
				yield return new PositionalNetworkAction(point => node.NetworkViewModel.AddAllEntitiesIntoDiagram(networkViewModel.Network.ShowEntity(hiddenEntity, node.Entity), point),
					name: ResString.GetMultilingualString("37357AAD-2324-4F85-9837-C362F88FC898", "{0}", hiddenEntity.Name),
					group: 1);
			}
		}

		#endregion
	}
}
