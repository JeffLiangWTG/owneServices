using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.NetworkVisualisation.Business
{
	public class BringToFrontAction : DynamicNetworkAction
	{
		public BringToFrontAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, new MultipleSelectedEntitiesExecutionStrategy(), group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		#region Main Properties

		protected override ResourceString GetDefaultNameCore() => ResString.GetMultilingualString("4a430690-6dc5-4eb1-83cb-bd3bd633d031", "Bring to Front");

		protected override ResourceString GetDefaultDescriptionCore() => ResString.GetMultilingualString("43353F61-7A8E-43E7-9C92-EE0FB2E5CB8D", "Brings this shape to front");

		protected override string IconName => "BringToFront";

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
			GetNode(entity).BringToFront();
			return null;
		}

		#endregion
	}
}
