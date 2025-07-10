using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.NetworkVisualisation.Business
{
	public class RemoveFromDiagramAction : DynamicNetworkAction
	{
		public RemoveFromDiagramAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, new MultipleSelectedEntitiesExecutionStrategy(), group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		#region Main Properties

		protected override ResourceString GetDefaultNameCore() => ResString.GetMultilingualString("d1378612-b408-4fe6-be97-02fe0eea1f7f", "Remove from Diagram");

		protected override ResourceString GetDefaultDescriptionCore() => ResString.GetMultilingualString("5b9330a2-77b6-4a67-afac-d4a8ae0f1523", "Removes this shape from the diagram");

		#endregion

		#region Accessibility

		protected override INetworkActionAccessibility IsApplicableToEntityCore(INetworkEntity entity)
		{
			return NetworkActionAccessibilityHelper.CheckEntityIsNotRoot(entity);
		}

		protected override INetworkActionAccessibility IsEnabledForEntityCore(INetworkEntity entity)
		{
			return new NetworkActionAccessibility(GetNode(entity).CanHide,
				entity,
				() => Res.GetString("A50A7CF4-8475-4C49-8C29-29025C79318E", "The entity should support removing from diagram."));
		}

		#endregion

		#region Execution

		protected override INetworkActionResult ExecuteForEntityCore(INetworkEntity entity)
		{
			GetNetworkViewModel().RemoveFromDiagram(GetNode(entity));
			return null;
		}

		#endregion
	}
}
