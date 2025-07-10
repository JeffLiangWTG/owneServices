using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI
{
	public class BMRibbonViewModelProvider : IRibbonDataProvider
	{
#pragma warning disable CS0618
		public RibbonViewModel GetRibbonViewModel(NetworkViewModel networkViewModel, NetworkUserControl control) => IsNetworkRibbonControlEnabled(networkViewModel) ? new BMRibbonViewModel(networkViewModel, control) : null;
#pragma warning restore CS0618

		// ribbon should be collapsed when the diagram is removed via data refresh bus (see JobNetworkRefresher)
		bool IsNetworkRibbonControlEnabled(NetworkViewModel networkViewModel) => networkViewModel.Network is JobNetwork && BMSRegistry.Instance.NCNRibbonEnabled.Value;
	}
}
