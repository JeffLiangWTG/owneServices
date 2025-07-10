using System.Windows.Forms;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentShipmentToHawbMatcher;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public class CcsukShipmentAndHawbLinkerForm : ZChildForm
	{
		public CcsukShipmentAndHawbLinkerForm(NonPersistentShipmentToHawbMatcherHeader header)
		{
			CaptionRenderingEnabled = true;
			CaptionResourceString = Res.GetData("a36ae285-6f7e-434f-a799-ea1371404d0c", "Match CCS-UK HAWBs to Shipments");
			userControl = new CcsukShipmentAndHawbLinkerControl(header, delegate(DialogResult r)
			{ this.DialogResult = r; Close(); });
			userControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MinimumSize = userControl.MinimumSize;
			Controls.Add(userControl);
			userControl.BringToFront();
			InitializeComponent();

			userControl.AllowOverlap(MainStatusBar);
		}

		readonly CcsukShipmentAndHawbLinkerControl userControl;

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (userControl != null)
			{
				userControl.Dispose();
			}
		}
	}
}
