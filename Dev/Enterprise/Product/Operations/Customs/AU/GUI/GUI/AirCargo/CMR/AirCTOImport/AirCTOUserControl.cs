using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public partial class AirCTOUserControl : ZUserControl
	{
		public AirCTOUserControl()
		{
			InitializeComponent();
			flightOutturnUserControl.SetBindPrepend("FakeFlightOuturnUnderbond+");
			messagesUserControl.SetBindPrepend("FakeFlightOuturnUnderbond+");
		}

		protected override void OnLoad(System.EventArgs e)
		{
			base.OnLoad(e);
			SetupPlugins();
		}

		private void SetupPlugins()
		{
			airCTOMainTabControl.PlugIns.Add(ControllerIDs.Customs.AU.AirCTOCusUnderbondPluginController);
		}
	}
}
