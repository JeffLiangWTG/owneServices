using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.NetworkVisualisation.GUI
{
	public class SearchAction : DynamicGuiNetworkAction
	{
#pragma warning disable CS0618 // Disable the warning for obsolete usage
		public SearchAction(INetworkViewModel networkViewModel, NetworkUserControl control, int group = 0, int groupIndex = 0)
			: base(networkViewModel, control, null, group, groupIndex)
		{
		}
#pragma warning restore CS0618 // Restore the warning for obsolete usage

		#region Main Properties

		protected override ResourceString GetDefaultNameCore() => ResString.GetMultilingualString("6183c980-83cb-41ca-a9cf-7dc5c4f9bd9f", "Search");

		protected override ResourceString GetDefaultDescriptionCore() => ResString.GetMultilingualString("98f8db2d-6c8d-47de-ba4c-f0334777307f", "Search the shapes in this diagram (Ctrl-F)");

		#endregion

		#region Accessibility

		protected override INetworkActionAccessibility IsApplicableToEntityCore(INetworkEntity entity) => NetworkActionAccessibility.Allowed;

		protected override INetworkActionAccessibility IsEnabledForEntityCore(INetworkEntity entity) => NetworkActionAccessibility.Allowed;

		#endregion

		#region Execution

		protected override INetworkActionResult ExecuteForEntityCore(INetworkEntity entity)
		{
			if (entity.IsDiagramWithRibbon)
			{
				Control.SwitсhFocusToFinder();
			}
			else
			{
				Control.OpenFinderForm();
			}
			return null;
		}

		#endregion
	}
}
