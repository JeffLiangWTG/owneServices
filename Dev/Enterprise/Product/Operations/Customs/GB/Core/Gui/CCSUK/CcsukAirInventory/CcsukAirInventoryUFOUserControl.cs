using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public partial class CcsukAirInventoryUFOUserControl : ZUserControl
	{
		public CcsukAirInventoryUFOUserControl(CreateUFOOrchestrator orchestrator, VoidMethodToClose closeMethod)
		{
			this.orchestrator = orchestrator;
			closeFormMethod = closeMethod;
			base.SetDataBinding(this.orchestrator.Mawb, "");
			InitializeComponent();
		}

		public void ButtonGenerate_Click(object sender, System.EventArgs e)
		{
			var sentWithoutNotifications = orchestrator.SendFRIMessageForUFO(new Customs.GUI.SendsMessagesToCustomsGUI());
			if (sentWithoutNotifications && closeFormMethod != null)
			{
				closeFormMethod();
			}
		}

		public delegate void VoidMethodToClose();
		readonly CreateUFOOrchestrator orchestrator;
		readonly VoidMethodToClose closeFormMethod;
	}
}
