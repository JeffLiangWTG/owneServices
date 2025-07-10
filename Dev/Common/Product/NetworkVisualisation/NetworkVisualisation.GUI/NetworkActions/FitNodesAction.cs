using System.Linq;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.NetworkVisualisation.GUI
{
	public class FitNodesAction : DynamicGuiNetworkAction
	{
		#region Main Properties

#pragma warning disable CS0618 // Disable the warning for obsolete usage
		public FitNodesAction(INetworkViewModel networkViewModel, NetworkUserControl control, int group = 0, int groupIndex = 0)
			: base(networkViewModel, control, new CommonNetworkActionExecutionStrategy(), group, groupIndex)
		{
		}
#pragma warning restore CS0618 // Restore the warning for obsolete usage

		protected override ResourceString GetDefaultNameCore() => ResString.GetMultilingualString("A76D5A54-2BE7-4EC6-AEA4-2CAC88058B53", "Fit");

		protected override ResourceString GetDefaultDescriptionCore() => ResString.GetMultilingualString("141AB991-7D0D-4034-B08C-1B4E2D865C2B", "Fit nodes to the view-port");

		protected override ResourceString GetDescriptionCore(INetworkEntity activeEntity)
		{
			return NetworkViewModel.SelectedEntities.Any()
				? NetworkViewModel.SelectedEntities.Count() == 1
					? ResString.GetMultilingualString("2FC74822-30DF-456E-890E-3F5CE3F2CBF6", "Fit selected node to the view-port")
					: ResString.GetMultilingualString("2F224B94-173C-496A-B3E3-3560B2D45429", "Fit selected nodes to the view-port")
				: ResString.GetMultilingualString("0CE58709-DBB4-45C5-82EC-AE788A7720C1", "Fit all nodes to the view-port");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "resource name")]
		protected override string IconName => "Fit";

		#endregion

		#region Accessibility

		protected override INetworkActionAccessibility IsApplicableToEntityCore(INetworkEntity entity) => NetworkActionAccessibility.Allowed;

		protected override INetworkActionAccessibility IsEnabledForEntityCore(INetworkEntity entity) => NetworkActionAccessibility.Allowed;

		#endregion

		#region Execution

		protected override INetworkActionResult ExecuteForEntityCore(INetworkEntity entity)
		{
			Control?.FitNodes();
			return null;
		}

		#endregion
	}
}
