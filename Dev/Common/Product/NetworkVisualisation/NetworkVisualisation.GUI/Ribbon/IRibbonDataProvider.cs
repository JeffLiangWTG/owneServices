using CargoWise.NetworkVisualisation.Business;

namespace CargoWise.NetworkVisualisation.GUI
{
	public interface IRibbonDataProvider
	{
#pragma warning disable CS0618 // Disable the warning for obsolete usage
		RibbonViewModel GetRibbonViewModel(NetworkViewModel networkViewModel, NetworkUserControl control);
#pragma warning restore CS0618 // Restore the warning for obsolete usage
	}
}
