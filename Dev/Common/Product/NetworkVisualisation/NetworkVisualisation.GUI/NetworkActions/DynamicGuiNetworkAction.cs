using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.GUI
{
#pragma warning disable CS0618 // Disable the warning for obsolete usage
	public abstract class DynamicGuiNetworkAction : DynamicNetworkAction
	{
		protected DynamicGuiNetworkAction(INetworkViewModel networkViewModel, NetworkUserControl control, INetworkActionExecutionStrategy executionStrategy = null, int group = 0, int groupIndex = 0)
			: base(networkViewModel, executionStrategy, group, groupIndex)
		{
			Control = control;
		}

		protected NetworkUserControl Control;
	}
#pragma warning restore CS0618 // Restore the warning for obsolete usage
}
