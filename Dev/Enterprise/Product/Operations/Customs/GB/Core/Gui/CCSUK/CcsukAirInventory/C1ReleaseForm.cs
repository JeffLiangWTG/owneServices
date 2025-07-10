using Enterprise.Customs.GB.Ccsuk.AirCargoInventory;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public partial class C1ReleaseForm : ZChildForm
	{
		public C1ReleaseForm(ICcsukCusAwb awb)
		{
			var controller = new NonPersistentC1ReleaseOrchestrator(awb);
			var userControl = new C1ReleaseUserControl(controller, Close);
			userControl.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Top;
			Controls.Add(userControl);
			userControl.BringToFront();
			InitializeComponent();
		}
	}
}
