using CargoWise.Common;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public partial class CheckInAllChildPiecesForm : ZChildForm
	{
		public CheckInAllChildPiecesForm(ICcsukCusAwb awb)
		{
			Argument.NotNull(awb, "awb");
			var controller = new NonPersistentCheckInAllChildPiecesOrchestrator(awb);
			this.controller = controller;
			var checkInAllChildPiecesControl = new CheckInAllChildPiecesUserControl(controller, Close);
			checkInAllChildPiecesControl.Dock = System.Windows.Forms.DockStyle.Fill;
			Controls.Add(checkInAllChildPiecesControl);
			checkInAllChildPiecesControl.BringToFront();
			InitializeComponent();
		}

		internal NonPersistentCheckInAllChildPiecesOrchestrator controller;
	}
}
