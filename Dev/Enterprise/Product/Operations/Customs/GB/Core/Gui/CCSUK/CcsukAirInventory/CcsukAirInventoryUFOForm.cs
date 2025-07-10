using System.ComponentModel;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public partial class CcsukAirInventoryUFOForm : ZChildForm
	{
		public CcsukAirInventoryUFOForm(CusMAWB mawb)
		{
			SetDataBinding(mawb, "");
			var orchestrator = new CreateUFOOrchestrator(mawb);
			var userControl = new CcsukAirInventoryUFOUserControl(orchestrator, Close);
			Controls.Add(userControl);
			userControl.BringToFront();
			InitializeComponent();
		}

		protected override void ZForm_Closing(object sender, CancelEventArgs e)
		{ }
	}
}
