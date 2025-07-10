using System.Collections.Generic;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.NetworkVisualisation.Business
{
	public class ImportAction : DynamicNetworkAction
	{
		public ImportAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, null, group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		#region Main Properties

		protected override ResourceString GetDefaultNameCore() => ResString.GetMultilingualString("05c6f327-55d0-4756-bb0b-9e8183450d40", "Import");

		protected override ResourceString GetDefaultDescriptionCore() => ResString.GetMultilingualString("c340d92f-f17d-4743-8336-a740093d3665", "Import an existing diagram into this diagram");

		protected override IEnumerable<INetworkAction> GetChildActionsCore(INetworkEntity activeEntity)
		{
			return GetImportActions(GetNetworkViewModel(), GetNode(activeEntity));
		}

		#endregion

		#region Accessibility

		protected override INetworkActionAccessibility IsApplicableToEntityCore(INetworkEntity entity)
		{
			return new NetworkActionAccessibility(GetNode(entity).SupportsChildEntities,
					entity,
					() => Res.GetString("2A92ED86-827D-49E1-815B-42C905898BDD", "The entity should support child entities."));
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

		static IEnumerable<INetworkAction> GetImportActions(NetworkViewModel networkViewModel, NodeViewModel node)
		{
			yield return new PositionalNetworkAction(point => networkViewModel.ImportEntity(point, node.Entity),
				name: ResString.GetMultilingualString("e18194cf-9d8f-4fa7-911b-9648e7d9fed3", "Copy An Existing Diagram"),
				description: ResString.GetMultilingualString("919f4675-0602-4665-a1d3-6428a9ab117a", "Clones an existing diagram and adds it into this diagram")
				);
		}

		#endregion
	}
}
