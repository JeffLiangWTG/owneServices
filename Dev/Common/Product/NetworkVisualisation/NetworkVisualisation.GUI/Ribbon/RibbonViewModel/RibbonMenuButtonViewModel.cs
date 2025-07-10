using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.NetworkVisualisation.GUI
{
	public class RibbonMenuButtonViewModel : RibbonButtonViewModel
	{
		// this class is introduced to let NetworkRibbonControl differentiate between a ribbon button and ribbon menu button
		// unfortunately RibbonMenuButton does not turn into RibbonButton when the list of child actions is empty
		// all the functionality needed for ribbon menu button is implemented in the ancestor class

		public RibbonMenuButtonViewModel(RibbonViewModel ribbonViewModel, INetworkAction action)
			: base(ribbonViewModel, imageLayout: RibbonImageLayout.BothLargeAndSmallImages, action)
		{
		}
		public RibbonMenuButtonViewModel(RibbonViewModel ribbonViewModel, RibbonImageLayout imageLayout, INetworkAction action)
			: base(ribbonViewModel, GetUniqueKey(), imageLayout, action)
		{
		}

		public RibbonMenuButtonViewModel(RibbonViewModel ribbonViewModel, string key, INetworkAction action)
			: base(ribbonViewModel, key, imageLayout: RibbonImageLayout.BothLargeAndSmallImages, action)
		{
		}

		public RibbonMenuButtonViewModel(RibbonViewModel ribbonViewModel, string key, RibbonImageLayout imageLayout, INetworkAction action)
			: base(ribbonViewModel, key, label: null, tooltip: null, action?.GetIconName(), imageLayout, action)
		{
		}

		public RibbonMenuButtonViewModel(RibbonViewModel ribbonViewModel, ResourceString label, INetworkAction action)
			: base(ribbonViewModel, key: label.ResourceKey, label, tooltip: null, action?.GetIconName(), imageLayout: RibbonImageLayout.BothLargeAndSmallImages, action)
		{
		}

		public RibbonMenuButtonViewModel(RibbonViewModel ribbonViewModel, ResourceString label, RibbonImageLayout imageLayout, INetworkAction action)
			: base(ribbonViewModel, key: label.ResourceKey, label, tooltip: null, action?.GetIconName(), imageLayout, action)
		{
		}

		public RibbonMenuButtonViewModel(RibbonViewModel ribbonViewModel, ResourceString label, ResourceString tooltip, INetworkAction action)
			: base(ribbonViewModel, key: label.ResourceKey, label, tooltip, action?.GetIconName(), imageLayout: RibbonImageLayout.BothLargeAndSmallImages, action)
		{
		}

		public RibbonMenuButtonViewModel(RibbonViewModel ribbonViewModel, ResourceString label, ResourceString tooltip, string iconName, RibbonImageLayout imageLayout, INetworkAction action)
			: base(ribbonViewModel, key: label.ResourceKey, label, tooltip, iconName, imageLayout, action)
		{
		}
	}
}
