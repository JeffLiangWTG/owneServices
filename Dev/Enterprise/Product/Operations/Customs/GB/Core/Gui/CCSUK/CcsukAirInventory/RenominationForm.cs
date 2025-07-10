using Enterprise.Customs.GB.Ccsuk.AirCargoInventory;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public partial class RenominationForm : ZChildForm
	{
		public RenominationForm(ICcsukCusAwb awb)
		{
			var controller = new NonPersistentRenominationOrchestrator(awb);
			var grid = new RenominationUserControl(controller, Close);
			Controls.Add(grid);
			grid.BringToFront();
			InitializeComponent();
		}
	}
}
