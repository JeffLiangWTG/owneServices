using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Sailing.GUI
{
	public partial class SailingPluginUserControl : ZUserControl
	{
		public SailingPluginUserControl()
		{
			InitializeComponent();
			actualArrivalMessageUserControl.SetBindPrepend("Destinations.");
		}
	}
}
