using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.NetworkVisualisation.Business
{
	public class EditAffinitiesAction : DynamicNetworkAction
	{
		public EditAffinitiesAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, null, group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		#region Main Properties

		protected override ResourceString GetDefaultNameCore() => ResString.GetMultilingualString("327263a1-2bcd-4568-8c10-0658b73d77eb", "Edit Affinities");

		protected override ResourceString GetDefaultDescriptionCore() => ResString.GetMultilingualString("e94182af-58eb-4412-83ef-4137de084a74", "Edit the Affinities for this diagram");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "resource name")]
		protected override string IconName => "Edit";

		#endregion

		#region Accessibility

		protected override INetworkActionAccessibility IsApplicableToEntityCore(INetworkEntity entity)
		{
			return NetworkActionAccessibilityHelper.CheckEntityIsRoot(entity);
		}

		protected override INetworkActionAccessibility IsEnabledForEntityCore(INetworkEntity entity) => NetworkActionAccessibility.Allowed;

		#endregion

		#region Execution

		protected override INetworkActionResult ExecuteForEntityCore(INetworkEntity entity)
		{
			NetworkViewModel.Network.ModifyAffinities((IDiagramEntity)entity);
			NetworkViewModel.Network.Refresh(RefreshType.AffinitiesRefreshRequired);
			return null;
		}

		#endregion
	}
}
