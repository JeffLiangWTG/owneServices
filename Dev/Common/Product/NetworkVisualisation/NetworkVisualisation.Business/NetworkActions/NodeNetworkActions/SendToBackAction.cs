using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.NetworkVisualisation.Business
{
	public class SendToBackAction : DynamicNetworkAction
	{
		public SendToBackAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, new MultipleSelectedEntitiesExecutionStrategy(), group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		#region Main Properties

		protected override ResourceString GetDefaultNameCore() => ResString.GetMultilingualString("05998de0-a816-4b58-931b-feab490ff3b9", "Send to Back");

		protected override ResourceString GetDefaultDescriptionCore() => ResString.GetMultilingualString("2219DD89-3ED9-46B6-A3C8-671B6A1F2214", "Sends this shape to back");

		protected override string IconName => "SendToBack";

		#endregion

		#region Accessibility

		protected override INetworkActionAccessibility IsApplicableToEntityCore(INetworkEntity entity)
		{
			return NetworkActionAccessibilityHelper.CheckEntityIsNotRoot(entity);
		}

		protected override INetworkActionAccessibility IsEnabledForEntityCore(INetworkEntity entity) => NetworkActionAccessibility.Allowed;

		#endregion

		#region Execution

		protected override INetworkActionResult ExecuteForEntityCore(INetworkEntity entity)
		{
			GetNode(entity).SendToBack();
			return null;
		}

		#endregion
	}
}
