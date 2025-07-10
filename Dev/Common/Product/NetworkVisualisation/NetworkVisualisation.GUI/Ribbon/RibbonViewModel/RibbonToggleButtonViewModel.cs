using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.NetworkVisualisation.GUI
{
	public class RibbonToggleButtonViewModel : RibbonButtonViewModel
	{
		// this class is introduced to let NetworkRibbonControl differentiate between a ribbon button and ribbon toggle button

		public RibbonToggleButtonViewModel(RibbonViewModel ribbonViewModel, INetworkAction action)
			: base(ribbonViewModel, imageLayout: RibbonImageLayout.BothLargeAndSmallImages, action)
		{
		}
		public RibbonToggleButtonViewModel(RibbonViewModel ribbonViewModel, RibbonImageLayout imageLayout, INetworkAction action)
			: base(ribbonViewModel, GetUniqueKey(), imageLayout, action)
		{
		}

		public RibbonToggleButtonViewModel(RibbonViewModel ribbonViewModel, string key, INetworkAction action)
			: base(ribbonViewModel, key, RibbonImageLayout.BothLargeAndSmallImages, action)
		{
		}

		public RibbonToggleButtonViewModel(RibbonViewModel ribbonViewModel, string key, RibbonImageLayout imageLayout, INetworkAction action)
			: base(ribbonViewModel, key, label: null, tooltip: null, action?.GetIconName(), imageLayout, action)
		{
		}

		public RibbonToggleButtonViewModel(RibbonViewModel ribbonViewModel, ResourceString label, INetworkAction action)
			: base(ribbonViewModel, key: label.ResourceKey, label, tooltip: null, action?.GetIconName(), imageLayout: RibbonImageLayout.BothLargeAndSmallImages, action)
		{
		}

		public RibbonToggleButtonViewModel(RibbonViewModel ribbonViewModel, ResourceString label, RibbonImageLayout imageLayout, INetworkAction action)
			: base(ribbonViewModel, key: label.ResourceKey, label, tooltip: null, action?.GetIconName(), imageLayout, action)
		{
		}

		public RibbonToggleButtonViewModel(RibbonViewModel ribbonViewModel, ResourceString label, ResourceString tooltip, INetworkAction action)
			: base(ribbonViewModel, key: label.ResourceKey, label, tooltip, action?.GetIconName(), imageLayout: RibbonImageLayout.BothLargeAndSmallImages, action)
		{
		}

		public RibbonToggleButtonViewModel(RibbonViewModel ribbonViewModel, ResourceString label, ResourceString tooltip, string iconName, RibbonImageLayout imageLayout, INetworkAction action)
			: base(ribbonViewModel, key: label.ResourceKey, label, tooltip, iconName, imageLayout, action)
		{
		}
	}
}
